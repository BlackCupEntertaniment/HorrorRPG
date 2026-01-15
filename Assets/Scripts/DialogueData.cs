using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [TextArea(3, 10)]
    public string[] sentences;

    [Header("Confirmation Settings")]
    public bool requiresConfirmation = false;
}
