using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemAmountText;
    [SerializeField] private Image backgroundImage;

    private InventorySlot currentSlot;
    private InventoryManager inventoryManager;
    private Color defaultNameColor = Color.white;
    private Color defaultAmountColor = Color.white;
    private Color selectedNameColor = Color.black;
    private Color selectedAmountColor = Color.black;

    private void Awake()
    {
        if (itemNameText == null)
            itemNameText = transform.Find("ItemNameText").GetComponent<TextMeshProUGUI>();
        
        if (itemAmountText == null)
            itemAmountText = transform.Find("ItemAmountText").GetComponent<TextMeshProUGUI>();
        
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (itemNameText != null)
            defaultNameColor = itemNameText.color;
        
        if (itemAmountText != null)
            defaultAmountColor = itemAmountText.color;
    }

    public void Setup(InventorySlot slot, InventoryManager manager)
    {
        currentSlot = slot;
        inventoryManager = manager;

        if (slot != null && slot.itemData != null)
        {
            itemNameText.text = slot.itemData.itemName;
            itemAmountText.text = "x" + slot.quantity.ToString();
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void OnSlotClicked()
    {
        if (currentSlot != null && currentSlot.itemData != null && inventoryManager != null)
        {
            inventoryManager.SelectSlot(this);
        }
    }

    public void SetSelected(bool selected)
    {
        if (backgroundImage != null)
        {
            backgroundImage.enabled = selected;
        }

        if (itemNameText != null)
        {
            itemNameText.color = selected ? selectedNameColor : defaultNameColor;
        }

        if (itemAmountText != null)
        {
            itemAmountText.color = selected ? selectedAmountColor : defaultAmountColor;
        }
    }

    public InventorySlot GetSlot()
    {
        return currentSlot;
    }

    public int GetSlotIndex()
    {
        return inventoryManager != null ? inventoryManager.GetSlotIndex(this) : -1;
    }
}
