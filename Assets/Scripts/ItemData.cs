using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    public string description;
    public Sprite icon;

    [Header("Properties")]
    public bool isStackable = true;
    public int maxStackSize = 99;
}

[System.Serializable]
public class InventorySlot
{
    public ItemData itemData;
    public int quantity;

    public InventorySlot(ItemData data, int qty)
    {
        itemData = data;
        quantity = qty;
    }

    public bool CanStack(ItemData data)
    {
        return itemData == data && itemData.isStackable && quantity < itemData.maxStackSize;
    }

    public void AddQuantity(int amount)
    {
        quantity += amount;
        if (quantity > itemData.maxStackSize)
        {
            quantity = itemData.maxStackSize;
        }
    }
}
