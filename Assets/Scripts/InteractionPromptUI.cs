using UnityEngine;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    public static InteractionPromptUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject promptBox;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Prompt Settings")]
    [SerializeField] private string defaultPromptMessage = "Pressione E para interagir";

    private bool isPromptActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (promptBox != null)
        {
            promptBox.SetActive(false);
        }
    }

    public void ShowPrompt(string message = null)
    {
        if (promptBox != null && promptText != null)
        {
            promptText.text = string.IsNullOrEmpty(message) ? defaultPromptMessage : message;
            promptBox.SetActive(true);
            isPromptActive = true;
        }
    }

    public void HidePrompt()
    {
        if (promptBox != null)
        {
            promptBox.SetActive(false);
            isPromptActive = false;
        }
    }

    public bool IsPromptActive()
    {
        return isPromptActive;
    }
}
