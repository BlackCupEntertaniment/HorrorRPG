using System.Collections;
using UnityEngine;

public class DialogueItemInteraction : MonoBehaviour, IInteractable
{
    [Header("Dialogue Settings")]
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private bool stuckDialogue = false;
    [SerializeField] private bool fastText = false;

    [Header("Item Configuration")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;

    [Header("Interaction Settings")]
    [SerializeField] private bool canBePickedUp = true;
    [SerializeField] private string customPrompt = "";
    [SerializeField] private bool oneTimeOnly = false;

    private bool hasInteracted = false;
    private bool isWaitingForDialogue = false;

    public void Interact()
    {
        if (!CanInteract())
            return;

        if (DialogueSystem.Instance != null && dialogueData != null)
        {
            DialogueSystem.Instance.StartDialogue(dialogueData, stuckDialogue, fastText);
            isWaitingForDialogue = true;
            StartCoroutine(WaitForDialogueEnd());

            if (oneTimeOnly)
            {
                hasInteracted = true;
            }
        }
    }

    private IEnumerator WaitForDialogueEnd()
    {
        yield return null;

        while (DialogueSystem.Instance != null && DialogueSystem.Instance.IsDialogueActive())
        {
            yield return null;
        }

        isWaitingForDialogue = false;

        if (canBePickedUp && itemData != null)
        {
            ShowItemConfirmation();
        }
    }

    private void ShowItemConfirmation()
    {
        string confirmationMessage = $"Você quer adicionar {quantity}x {itemData.itemName} ao seu inventário?";
        
        DialogueSystem.Instance.StartDialogueWithConfirmation(
            confirmationMessage,
            OnItemConfirmed,
            OnItemCancelled,
            stuckDialogue,
            fastText
        );
    }

    private void OnItemConfirmed()
    {
        CollectItems();
    }

    private void OnItemCancelled()
    {
        if (oneTimeOnly)
        {
            hasInteracted = true;
        }
    }

    private void CollectItems()
    {
        if (!canBePickedUp || itemData == null)
            return;

        if (InventoryManager.Instance != null)
        {
            int addedQuantity = InventoryManager.Instance.AddItem(itemData, quantity);

            if (addedQuantity > 0)
            {
                quantity -= addedQuantity;

                if (quantity <= 0)
                {
                    Destroy(gameObject);
                }
            }
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
            return "Pressione E para interagir";
        }

        return "Pressione E para interagir";
    }

    public bool CanInteract()
    {
        if (!canBePickedUp)
        {
            return false;
        }

        if (oneTimeOnly && hasInteracted)
        {
            return false;
        }

        if (isWaitingForDialogue)
        {
            return false;
        }

        if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsDialogueActive())
        {
            return false;
        }

        return dialogueData != null && itemData != null;
    }

    public void SetCanInteract(bool value)
    {
        canBePickedUp = value;
    }

    public void ResetInteraction()
    {
        hasInteracted = false;
    }
}
