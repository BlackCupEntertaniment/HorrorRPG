using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [TextArea(3, 10)]
    public string[] sentences;

    [Header("Dialogue Behavior")]
    public bool stuckDialogue = false;
    public bool fastText = false;

    [Header("Confirmation Settings")]
    public bool requiresConfirmation = false;
}
