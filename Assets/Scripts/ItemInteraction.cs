using UnityEngine;

public class ItemInteraction : MonoBehaviour, IInteractable
{
    [Header("Item Configuration")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;

    [Header("Interaction Settings")]
    [SerializeField] private bool canBePickedUp = true;
    [SerializeField] private string customPrompt = "";

    private const float MESSAGE_DISPLAY_DURATION = 2f;

    public void Interact()
    {
        if (!CanInteract())
            return;

        if (InventoryManager.Instance != null && itemData != null)
        {
            int addedQuantity = InventoryManager.Instance.AddItem(itemData, quantity);

            if (addedQuantity == 0)
            {
                if (InteractionPromptUI.Instance != null)
                {
                    InteractionPromptUI.Instance.ShowPrompt("Não foi possível pegar o item, Inventario cheio");
                    Invoke(nameof(HidePrompt), MESSAGE_DISPLAY_DURATION);
                }
            }
            else if (addedQuantity < quantity)
            {
                quantity -= addedQuantity;

                if (InteractionPromptUI.Instance != null)
                {
                    InteractionPromptUI.Instance.ShowPrompt("Não foi possível pegar todos os itens, o inventario está cheio");
                    Invoke(nameof(HidePrompt), MESSAGE_DISPLAY_DURATION);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private void HidePrompt()
    {
        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.HidePrompt();
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
