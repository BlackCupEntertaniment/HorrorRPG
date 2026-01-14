using UnityEngine;

public class DialogueTrigger : MonoBehaviour, IInteractable
{
    [Header("Dialogue Settings")]
    [SerializeField] private DialogueData dialogueData;

    [Header("Dialogue Options")]
    [SerializeField] private bool stuckDialogue = false;
    [SerializeField] private bool fastText = false;

    [Header("Trigger Settings")]
    [SerializeField] private bool canInteract = true;
    [SerializeField] private bool oneTimeOnly = false;

    private bool hasInteracted = false;

    public void Interact()
    {
        if (!canInteract || (oneTimeOnly && hasInteracted))
        {
            return;
        }

        if (DialogueSystem.Instance != null && dialogueData != null)
        {
            DialogueSystem.Instance.StartDialogue(dialogueData, stuckDialogue, fastText);
            
            if (oneTimeOnly)
            {
                hasInteracted = true;
            }
        }
    }

    public string GetInteractionPrompt()
    {
        return "Pressione E para interagir";
    }

    public bool CanInteract()
    {
        if (!canInteract)
        {
            return false;
        }

        if (oneTimeOnly && hasInteracted)
        {
            return false;
        }

        if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsDialogueActive())
        {
            return false;
        }

        return true;
    }

    public void SetCanInteract(bool value)
    {
        canInteract = value;
    }

    public void ResetInteraction()
    {
        hasInteracted = false;
    }
}
