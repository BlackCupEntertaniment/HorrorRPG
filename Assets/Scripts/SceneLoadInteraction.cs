using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadInteraction : MonoBehaviour, IInteractable
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad;
    [SerializeField] private string customPrompt = "Press E to enter";
    
    [Header("Interaction Settings")]
    [SerializeField] private bool canInteract = true;

    public void Interact()
    {
        if (!CanInteract())
            return;

        LoadScene();
    }

    public string GetInteractionPrompt()
    {
        return customPrompt;
    }

    public bool CanInteract()
    {
        return canInteract && !string.IsNullOrEmpty(sceneToLoad);
    }

    private void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("SceneLoadInteraction: Nome da cena não definido!");
            return;
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}
