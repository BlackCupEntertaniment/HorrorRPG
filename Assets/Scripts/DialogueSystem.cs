using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Typing Settings")]
    [SerializeField] private bool fastText = false;
    [SerializeField] private float typingSpeed = 0.05f;

    [Header("Dialogue Settings")]
    [SerializeField] private bool stuckDialogue = false;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;

    private const string CONTROL_LOCK_ID = "DialogueSystem";

    private string[] currentSentences;
    private int currentSentenceIndex;
    private bool isTyping;
    private bool isDialogueActive;
    private Coroutine typingCoroutine;
    
    private bool currentFastText;
    private bool currentStuckDialogue;

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
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }
    }

    private void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(skipKey))
        {
            if (isTyping)
            {
                StopTyping();
                dialogueText.text = currentSentences[currentSentenceIndex];
                isTyping = false;
            }
            else
            {
                DisplayNextSentence();
            }
        }
    }

    public void StartDialogue(DialogueData dialogueData, bool? stuckOverride = null, bool? fastTextOverride = null)
    {
        if (dialogueData == null || dialogueData.sentences.Length == 0)
        {
            return;
        }

        currentFastText = fastTextOverride.HasValue ? fastTextOverride.Value : fastText;
        currentStuckDialogue = stuckOverride.HasValue ? stuckOverride.Value : stuckDialogue;

        currentSentences = dialogueData.sentences;
        currentSentenceIndex = 0;
        isDialogueActive = true;

        if (currentStuckDialogue && PlayerControlManager.Instance != null)
        {
            PlayerControlManager.Instance.LockControl(CONTROL_LOCK_ID);
        }

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        DisplaySentence(currentSentences[currentSentenceIndex]);
    }

    private void DisplaySentence(string sentence)
    {
        StopTyping();

        if (currentFastText)
        {
            dialogueText.text = sentence;
            isTyping = false;
        }
        else
        {
            typingCoroutine = StartCoroutine(TypeSentence(sentence));
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void DisplayNextSentence()
    {
        currentSentenceIndex++;

        if (currentSentenceIndex < currentSentences.Length)
        {
            DisplaySentence(currentSentences[currentSentenceIndex]);
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        StopTyping();
        isDialogueActive = false;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        if (currentStuckDialogue && PlayerControlManager.Instance != null)
        {
            PlayerControlManager.Instance.UnlockControl(CONTROL_LOCK_ID);
        }

        currentSentences = null;
        currentSentenceIndex = 0;
    }

    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    public void SetFastText(bool value)
    {
        fastText = value;
    }

    public void SetStuckDialogue(bool value)
    {
        stuckDialogue = value;
    }

    public void SetTypingSpeed(float speed)
    {
        typingSpeed = speed;
    }
}
