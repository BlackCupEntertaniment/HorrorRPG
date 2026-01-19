using UnityEngine;

public class BattleTrigger3D : MonoBehaviour
{
    [Header("Enemy Renderer")]
    [SerializeField] private MeshRenderer enemyRenderer;

    [Header("Trigger Settings")]
    [SerializeField] private bool disableAfterTrigger = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        if (!IsPlayer(other))
            return;

        if (enemyRenderer == null)
        {
            Debug.LogWarning("BattleTrigger3D: enemyRenderer não está definido no Inspector.");
            return;
        }

        BattleManager.Instance.StartBattle(enemyRenderer);

        if (disableAfterTrigger)
        {
            hasTriggered = true;
            gameObject.SetActive(false);
        }
    }

    private bool IsPlayer(Collider other)
    {
        if (other.CompareTag("Player"))
            return true;

        if (other.GetComponent<CharacterController>() != null)
            return true;

        return false;
    }
}
