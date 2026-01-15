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

    [Header("Prefab")]
    [SerializeField] private GameObject itemSlotPrefab;

    [Header("Settings")]
    [SerializeField] private int maxSlots = 9;

    private const string CONTROL_LOCK_ID = "InventorySystem";

    private List<InventorySlot> inventorySlots = new List<InventorySlot>();
    private List<ItemSlotUI> slotUIList = new List<ItemSlotUI>();
    private ItemSlotUI currentlySelectedSlot;
    private bool isInventoryOpen = false;
    private int currentSelectedIndex = -1;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }

        if (isInventoryOpen)
        {
            HandleInventoryNavigation();
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        
        if (inventoryCanvas != null)
        {
            inventoryCanvas.SetActive(isInventoryOpen);
        }

        if (isInventoryOpen)
        {
            RefreshInventoryUI();
            
            if (PlayerControlManager.Instance != null)
            {
                PlayerControlManager.Instance.LockControl(CONTROL_LOCK_ID);
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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

    public void AddItem(ItemData itemData, int quantity = 1)
    {
        InventorySlot existingSlot = inventorySlots.Find(slot => slot.CanStack(itemData));

        if (existingSlot != null)
        {
            existingSlot.AddQuantity(quantity);
        }
        else
        {
            inventorySlots.Add(new InventorySlot(itemData, quantity));
        }

        if (isInventoryOpen)
        {
            RefreshInventoryUI();
        }
    }

    private void RefreshInventoryUI()
    {
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i < inventorySlots.Count)
            {
                slotUIList[i].Setup(inventorySlots[i], this);
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

        if (isInventoryOpen && inventorySlots.Count > 0)
        {
            SelectSlotByIndex(0);
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

        currentSelectedIndex = slotUIList.IndexOf(selectedSlot);

        DisplayItemDetails(selectedSlot.GetSlot());
    }

    private void SelectSlotByIndex(int index)
    {
        if (index < 0 || index >= inventorySlots.Count)
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
        if (inventorySlots.Count == 0)
        {
            return;
        }

        bool moveDown = Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);
        bool moveUp = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);

        if (moveDown)
        {
            int nextIndex = currentSelectedIndex + 1;
            if (nextIndex >= inventorySlots.Count)
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
                previousIndex = inventorySlots.Count - 1;
            }
            SelectSlotByIndex(previousIndex);
        }
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
            itemDescriptionText.text = slot.itemData.description;
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
}
