using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactableLayer = ~0;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    
    [Header("Raycast Settings")]
    [SerializeField] private Transform raycastOrigin;
    
    private IInteractable currentInteractable;
    private Camera playerCamera;
    private bool interactionEnabled = true;
    private bool isShowingPrompt = false;

    private void Start()
    {
        playerCamera = Camera.main;
        
        if (raycastOrigin == null)
        {
            raycastOrigin = playerCamera.transform;
        }
    }

    private void Update()
    {
        if (interactionEnabled)
        {
            CheckForInteractable();
            HandleInteractionInput();
        }
        else
        {
            SetCurrentInteractable(null);
        }
    }

    private void CheckForInteractable()
    {
        Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            
            if (interactable != null && interactable.CanInteract())
            {
                SetCurrentInteractable(interactable);
                return;
            }
        }
        
        SetCurrentInteractable(null);
    }

    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(interactionKey) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void SetCurrentInteractable(IInteractable interactable)
    {
        if (currentInteractable != interactable)
        {
            currentInteractable = interactable;

            if (currentInteractable != null)
            {
                ShowInteractionPrompt();
            }
            else
            {
                HideInteractionPrompt();
            }
        }
    }

    private void ShowInteractionPrompt()
    {
        if (InteractionPromptUI.Instance != null && !isShowingPrompt)
        {
            string promptMessage = currentInteractable?.GetInteractionPrompt();
            InteractionPromptUI.Instance.ShowPrompt(promptMessage);
            isShowingPrompt = true;
        }
    }

    private void HideInteractionPrompt()
    {
        if (InteractionPromptUI.Instance != null && isShowingPrompt)
        {
            InteractionPromptUI.Instance.HidePrompt();
            isShowingPrompt = false;
        }
    }

    public IInteractable GetCurrentInteractable()
    {
        return currentInteractable;
    }

    public string GetInteractionPrompt()
    {
        return currentInteractable?.GetInteractionPrompt();
    }

    public void SetInteractionEnabled(bool enabled)
    {
        interactionEnabled = enabled;
    }

    private void OnDrawGizmos()
    {
        if (raycastOrigin != null)
        {
            Gizmos.color = currentInteractable != null ? Color.green : Color.red;
            Gizmos.DrawRay(raycastOrigin.position, raycastOrigin.forward * interactionRange);
        }
    }
}
