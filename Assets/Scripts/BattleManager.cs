using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    private const string CONTROL_LOCK_ID = "BattleSystem";

    [Header("Battle Arena")]
    [SerializeField] private GameObject battleArena;
    [SerializeField] private MeshRenderer battleEnemyRenderer;

    private bool isInBattle = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void StartBattle(MeshRenderer sourceEnemyRenderer)
    {
        if (isInBattle)
            return;

        if (sourceEnemyRenderer == null)
        {
            Debug.LogWarning("BattleManager: sourceEnemyRenderer é null.");
            return;
        }

        if (battleEnemyRenderer == null)
        {
            Debug.LogWarning("BattleManager: battleEnemyRenderer não está definido no Inspector.");
            return;
        }

        if (battleArena == null)
        {
            Debug.LogWarning("BattleManager: battleArena não está definido no Inspector.");
            return;
        }

        isInBattle = true;

        battleEnemyRenderer.sharedMaterials = sourceEnemyRenderer.sharedMaterials;

        battleArena.SetActive(true);

        PlayerControlManager.Instance.LockControl(CONTROL_LOCK_ID);

        Debug.Log("BattleManager: Batalha iniciada.");
    }

    public void EndBattle()
    {
        if (!isInBattle)
            return;

        isInBattle = false;

        battleArena.SetActive(false);

        PlayerControlManager.Instance.UnlockControl(CONTROL_LOCK_ID);

        Debug.Log("BattleManager: Batalha encerrada.");
    }

    public bool IsInBattle()
    {
        return isInBattle;
    }
}
