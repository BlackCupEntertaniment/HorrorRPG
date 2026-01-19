using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject inventoryCanvas;
    [SerializeField] private Transform itemsGridParent;
    [SerializeField] private Image itemIconDisplay;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private ConfirmationMenu discardConfirmationMenu;
    [SerializeField] private GameObject emptyMessageObject;
    [SerializeField] private GameObject descriptionBackground;
    [SerializeField] private GameObject iconBackground;

    [Header("Tabs")]
    [SerializeField] private Transform tabsParent;
    [SerializeField] private GameObject tabPrefab;

    [Header("Prefab")]
    [SerializeField] private GameObject itemSlotPrefab;

    [Header("Settings")]
    [SerializeField] private int maxSlots = 9;

    [Header("Prompt Messages")]
    private const string inventoryFullMessage = "Inventario cheio";
    private const string partialPickupMessage = "Alguns itens ficaram para trás";
    [SerializeField] private float promptDisplayDuration = 2f;

    private const string CONTROL_LOCK_ID = "InventorySystem";
    private const string DISCARD_CONFIRMATION_MESSAGE = "Tem certeza que deseja descartar este item?";

    private List<InventorySlot> inventorySlots = new List<InventorySlot>();
    private List<ItemSlotUI> slotUIList = new List<ItemSlotUI>();
    private ItemSlotUI currentlySelectedSlot;
    private bool isInventoryOpen = false;
    private int currentSelectedIndex = -1;
    private bool isDiscardMenuOpen = false;
    private string originalDescriptionText = "";

    private List<InventoryTab> tabs = new List<InventoryTab>();
    private int currentTabIndex = 0;
    private ItemCategory currentCategory = ItemCategory.Consumable;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (inventoryCanvas != null)
        {
            inventoryCanvas.SetActive(false);
        }

        if (discardConfirmationMenu != null)
        {
            discardConfirmationMenu.gameObject.SetActive(false);
        }

        InitializeTabs();
        FindOrCreateSlots();
    }

    private void FindOrCreateSlots()
    {
        if (itemsGridParent != null)
        {
            ItemSlotUI[] existingSlots = itemsGridParent.GetComponentsInChildren<ItemSlotUI>(true);
            
            foreach (ItemSlotUI slot in existingSlots)
            {
                slotUIList.Add(slot);
                slot.gameObject.SetActive(false);
            }

            int slotsToCreate = maxSlots - slotUIList.Count;
            for (int i = 0; i < slotsToCreate; i++)
            {
                if (itemSlotPrefab != null)
                {
                    GameObject newSlot = Instantiate(itemSlotPrefab, itemsGridParent);
                    ItemSlotUI slotUI = newSlot.GetComponent<ItemSlotUI>();
                    if (slotUI != null)
                    {
                        slotUIList.Add(slotUI);
                        slotUI.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    private void InitializeTabs()
    {
        if (tabsParent == null || tabPrefab == null)
        {
            return;
        }

        ItemCategory[] categories = { ItemCategory.Consumable, ItemCategory.Equipable, ItemCategory.Key };
        string[] tabNames = { "CONS", "EQUIP", "KEY" };

        for (int i = 0; i < categories.Length; i++)
        {
            GameObject tabObject = Instantiate(tabPrefab, tabsParent);
            InventoryTab tab = tabObject.GetComponent<InventoryTab>();
            
            if (tab == null)
            {
                tab = tabObject.AddComponent<InventoryTab>();
            }

            tab.Initialize(categories[i], tabNames[i]);
            tabs.Add(tab);
        }

        currentTabIndex = 0;
        currentCategory = ItemCategory.Consumable;
        UpdateTabsVisuals();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }

        if (isInventoryOpen)
        {
            if (isDiscardMenuOpen)
            {
                HandleDiscardMenuNavigation();
            }
            else
            {
                HandleTabNavigation();
                HandleInventoryNavigation();
            }
        }
    }

    public void ToggleInventory()
    {
        if (BattleManager.Instance != null && BattleManager.Instance.IsInBattle())
        {
            return;
        }

        isInventoryOpen = !isInventoryOpen;
        
        if (inventoryCanvas != null)
        {
            inventoryCanvas.SetActive(isInventoryOpen);
        }

        if (isInventoryOpen)
        {
            currentTabIndex = 0;
            currentCategory = ItemCategory.Consumable;
            UpdateTabsVisuals();
            RefreshInventoryUI();
            
            if (PlayerControlManager.Instance != null)
            {
                PlayerControlManager.Instance.LockControl(CONTROL_LOCK_ID);
            }

        }
        else
        {
            if (PlayerControlManager.Instance != null)
            {
                PlayerControlManager.Instance.UnlockControl(CONTROL_LOCK_ID);
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public int AddItem(ItemData itemData, int quantity = 1)
    {
        int remainingQuantity = quantity;

        while (remainingQuantity > 0)
        {
            InventorySlot existingSlot = inventorySlots.Find(slot => slot.CanStack(itemData));

            if (existingSlot != null)
            {
                remainingQuantity = existingSlot.AddQuantity(remainingQuantity);
            }
            else
            {
                if (inventorySlots.Count >= maxSlots)
                {
                    break;
                }

                int quantityToAdd = Mathf.Min(remainingQuantity, itemData.maxStackSize);
                inventorySlots.Add(new InventorySlot(itemData, quantityToAdd));
                remainingQuantity -= quantityToAdd;
            }
        }

        if (isInventoryOpen)
        {
            RefreshInventoryUI();
        }

        int addedQuantity = quantity - remainingQuantity;

        if (addedQuantity == 0)
        {
            ShowInventoryMessage(inventoryFullMessage);
        }
        else if (addedQuantity < quantity)
        {
            string message = $"Você pegou {addedQuantity} de {itemData.itemName}\n{partialPickupMessage}";
            ShowInventoryMessage(message);
        }
        else
        {
            string message = $"Você pegou {addedQuantity} de {itemData.itemName}";
            ShowInventoryMessage(message);
        }

        return addedQuantity;
    }

    private void ShowInventoryMessage(string message)
    {
        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.ShowPromptWithDuration(message, promptDisplayDuration);
        }
    }

    private void RefreshInventoryUI()
    {
        List<InventorySlot> filteredSlots = GetFilteredSlots();

        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i < filteredSlots.Count)
            {
                slotUIList[i].Setup(filteredSlots[i], this);
            }
            else
            {
                slotUIList[i].Setup(null, this);
            }
        }

        if (currentlySelectedSlot != null)
        {
            currentlySelectedSlot.SetSelected(false);
            currentlySelectedSlot = null;
        }

        ClearItemDisplay();

        if (filteredSlots.Count == 0)
        {
            if (emptyMessageObject != null)
            {
                emptyMessageObject.SetActive(true);
            }

            if (descriptionBackground != null)
            {
                descriptionBackground.SetActive(false);
            }

            if (iconBackground != null)
            {
                iconBackground.SetActive(false);
            }
        }
        else
        {
            if (emptyMessageObject != null)
            {
                emptyMessageObject.SetActive(false);
            }

            if (descriptionBackground != null)
            {
                descriptionBackground.SetActive(true);
            }

            if (iconBackground != null)
            {
                iconBackground.SetActive(true);
            }

            if (isInventoryOpen)
            {
                SelectSlotByIndex(0);
            }
        }
    }

    private List<InventorySlot> GetFilteredSlots()
    {
        List<InventorySlot> filtered = new List<InventorySlot>();
        
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.itemData != null && slot.itemData.category == currentCategory)
            {
                filtered.Add(slot);
            }
        }
        
        return filtered;
    }

    private void UpdateTabsVisuals()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            tabs[i].SetSelected(i == currentTabIndex);
        }
    }

    private void HandleTabNavigation()
    {
        bool moveRight = Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
        bool moveLeft = Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);

        if (moveRight)
        {
            currentTabIndex++;
            if (currentTabIndex >= tabs.Count)
            {
                currentTabIndex = 0;
            }
            
            currentCategory = tabs[currentTabIndex].GetCategory();
            UpdateTabsVisuals();
            RefreshInventoryUI();
        }
        else if (moveLeft)
        {
            currentTabIndex--;
            if (currentTabIndex < 0)
            {
                currentTabIndex = tabs.Count - 1;
            }
            
            currentCategory = tabs[currentTabIndex].GetCategory();
            UpdateTabsVisuals();
            RefreshInventoryUI();
        }
    }

    public void SelectSlot(ItemSlotUI selectedSlot)
    {
        if (currentlySelectedSlot != null)
        {
            currentlySelectedSlot.SetSelected(false);
        }

        currentlySelectedSlot = selectedSlot;
        currentlySelectedSlot.SetSelected(true);

        List<InventorySlot> filteredSlots = GetFilteredSlots();
        currentSelectedIndex = slotUIList.IndexOf(selectedSlot);

        DisplayItemDetails(selectedSlot.GetSlot());
    }

    private void SelectSlotByIndex(int index)
    {
        List<InventorySlot> filteredSlots = GetFilteredSlots();
        
        if (index < 0 || index >= filteredSlots.Count)
        {
            return;
        }

        if (index < slotUIList.Count && slotUIList[index].GetSlot() != null)
        {
            SelectSlot(slotUIList[index]);
        }
    }

    private void HandleInventoryNavigation()
    {
        List<InventorySlot> filteredSlots = GetFilteredSlots();
        
        if (filteredSlots.Count == 0)
        {
            return;
        }

        bool moveDown = Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);
        bool moveUp = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);

        if (moveDown)
        {
            int nextIndex = currentSelectedIndex + 1;
            if (nextIndex >= filteredSlots.Count)
            {
                nextIndex = 0;
            }
            SelectSlotByIndex(nextIndex);
        }
        else if (moveUp)
        {
            int previousIndex = currentSelectedIndex - 1;
            if (previousIndex < 0)
            {
                previousIndex = filteredSlots.Count - 1;
            }
            SelectSlotByIndex(previousIndex);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.E))
        {
            OpenDiscardMenu();
        }
    }

    private void HandleDiscardMenuNavigation()
    {
        if (discardConfirmationMenu != null)
        {
            discardConfirmationMenu.HandleNavigation();
        }
    }

    private void OpenDiscardMenu()
    {
        if (currentlySelectedSlot == null || currentlySelectedSlot.GetSlot() == null)
        {
            return;
        }

        if (!currentlySelectedSlot.GetSlot().itemData.disposable)
        {
            return;
        }

        isDiscardMenuOpen = true;
        originalDescriptionText = itemDescriptionText != null ? itemDescriptionText.text : "";

        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = DISCARD_CONFIRMATION_MESSAGE;
        }

        if (discardConfirmationMenu != null)
        {
            discardConfirmationMenu.gameObject.SetActive(true);
            discardConfirmationMenu.Show(OnDiscardConfirmed, OnDiscardCancelled);
        }
    }

    private void OnDiscardConfirmed()
    {
        if (currentlySelectedSlot != null)
        {
            int slotIndex = GetSlotIndex(currentlySelectedSlot);
            if (slotIndex >= 0 && slotIndex < inventorySlots.Count)
            {
                inventorySlots.RemoveAt(slotIndex);
            }
        }

        CloseDiscardMenu();
        RefreshInventoryUI();
    }

    private void OnDiscardCancelled()
    {
        CloseDiscardMenu();
        
        if (currentlySelectedSlot != null)
        {
            DisplayItemDetails(currentlySelectedSlot.GetSlot());
        }
    }

    private void CloseDiscardMenu()
    {
        isDiscardMenuOpen = false;

        if (discardConfirmationMenu != null)
        {
            discardConfirmationMenu.Hide();
        }
    }

    public int GetSlotIndex(ItemSlotUI slotUI)
    {
        return slotUIList.IndexOf(slotUI);
    }

    private void DisplayItemDetails(InventorySlot slot)
    {
        if (slot == null || slot.itemData == null)
        {
            ClearItemDisplay();
            return;
        }

        if (itemIconDisplay != null)
        {
            itemIconDisplay.sprite = slot.itemData.icon;
            itemIconDisplay.enabled = slot.itemData.icon != null;
        }

        if (itemDescriptionText != null)
        {
            string description = slot.itemData.description;
            
            if (!slot.itemData.disposable)
            {
                description += ". Cannot be discarted";
            }
            
            itemDescriptionText.text = description;
        }
    }

    private void ClearItemDisplay()
    {
        if (itemIconDisplay != null)
        {
            itemIconDisplay.sprite = null;
            itemIconDisplay.enabled = false;
        }

        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = "";
        }
    }

    public bool HasItem(ItemData itemData, int quantity)
    {
        foreach (var slot in inventorySlots)
        {
            if (slot != null && slot.itemData == itemData)
            {
                if (slot.quantity >= quantity)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool ConsumeItem(ItemData itemData, int quantity)
    {
        foreach (var slot in inventorySlots)
        {
            if (slot != null && slot.itemData == itemData)
            {
                if (slot.quantity >= quantity)
                {
                    slot.quantity -= quantity;
                    if (slot.quantity <= 0)
                    {
                        inventorySlots.Remove(slot);
                    }
                    RefreshInventoryUI();
                    return true;
                }
            }
        }
        return false;
    }

    public List<ItemData> GetAllItemsOfCategory(ItemCategory category)
    {
        List<ItemData> items = new List<ItemData>();
        
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot != null && slot.itemData != null && slot.itemData.category == category)
            {
                items.Add(slot.itemData);
            }
        }

        return items;
    }

    public List<ItemData> GetAllItems()
    {
        List<ItemData> items = new List<ItemData>();
        
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot != null && slot.itemData != null)
            {
                items.Add(slot.itemData);
            }
        }

        return items;
    }

    public int GetItemQuantity(ItemData itemData)
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot != null && slot.itemData == itemData)
            {
                return slot.quantity;
            }
        }

        return 0;
    }
}
