using UnityEngine;

public class ItemInteraction : MonoBehaviour, IInteractable
{
    [Header("Item Configuration")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;

    [Header("Interaction Settings")]
    [SerializeField] private bool canBePickedUp = true;
    [SerializeField] private string customPrompt = "";

    public void Interact()
    {
        if (!CanInteract())
            return;

        if (InventoryManager.Instance != null && itemData != null)
        {
            InventoryManager.Instance.AddItem(itemData, quantity);
            Destroy(gameObject);
        }
    }

    public string GetInteractionPrompt()
    {
        if (!string.IsNullOrEmpty(customPrompt))
        {
            return customPrompt;
        }

        if (itemData != null)
        {
            if (quantity > 1)
            {
                return $"Press E to pick up {itemData.itemName} x{quantity}";
            }
            return $"Press E to pick up {itemData.itemName}";
        }

        return "Press E to pick up item";
    }

    public bool CanInteract()
    {
        return canBePickedUp && itemData != null && InventoryManager.Instance != null;
    }
}
