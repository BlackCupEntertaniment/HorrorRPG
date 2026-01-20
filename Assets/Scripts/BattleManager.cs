using UnityEngine;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    private const string CONTROL_LOCK_ID = "BattleSystem";
    private const float TURN_DELAY = 1f;

    [Header("Battle Arena")]
    [SerializeField] private GameObject battleArena;
    [SerializeField] private MeshRenderer battleEnemyRenderer;
    [SerializeField] private BattleEnemyEffects battleEnemyEffects;

    [Header("Enemy Data")]
    [SerializeField] private EnemyData currentEnemyData;

    private bool isInBattle = false;
    private bool isProcessingTurn = false;
    private int currentEnemyHealth;

    public EnemyData CurrentEnemyData => currentEnemyData;
    public int CurrentEnemyHealth => currentEnemyHealth;
    public bool IsProcessingTurn => isProcessingTurn;

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

    public void StartBattle(MeshRenderer sourceEnemyRenderer, EnemyData enemyData)
    {
        if (isInBattle)
            return;

        if (sourceEnemyRenderer == null)
        {
            Debug.LogWarning("BattleManager: sourceEnemyRenderer é null.");
            return;
        }

        if (enemyData == null)
        {
            Debug.LogWarning("BattleManager: enemyData é null.");
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

        currentEnemyData = enemyData;
        isInBattle = true;
        currentEnemyHealth = currentEnemyData.maxHealth;

        battleEnemyRenderer.sharedMaterials = sourceEnemyRenderer.sharedMaterials;

        battleArena.SetActive(true);

        PlayerControlManager.Instance.LockControl(CONTROL_LOCK_ID);

        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.InitializeBattle();
            BattleUIManager.Instance.OpenMainMenu();
        }

        Debug.Log("BattleManager: Batalha iniciada.");
    }

    public void PlayerAttack(WeaponData weapon)
    {
        if (isProcessingTurn || !isInBattle)
            return;

        StartCoroutine(ProcessPlayerTurn(weapon));
    }

    private IEnumerator ProcessPlayerTurn(WeaponData weapon)
    {
        isProcessingTurn = true;

        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.HideAllMenus();
        }

        yield return new WaitForSeconds(TURN_DELAY);

        int damage = weapon.GetEffectiveDamage(currentEnemyData.category);
        currentEnemyHealth = Mathf.Max(0, currentEnemyHealth - damage);

        if (battleEnemyEffects != null)
        {
            battleEnemyEffects.PlayHitEffects();
        }

        Debug.Log($"Player atacou com {weapon.itemName} causando {damage} de dano!");

        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.UpdateHealthBars();
        }

        if (currentEnemyHealth <= 0)
        {
            Debug.Log("Inimigo derrotado!");
            yield return new WaitForSeconds(TURN_DELAY);
            EndBattle();
            yield break;
        }

        yield return new WaitForSeconds(TURN_DELAY);

        int enemyDamage = currentEnemyData.baseDamage;
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.TakeDamage(enemyDamage);
            
            if (BattlePlayerEffects.Instance != null)
            {
                BattlePlayerEffects.Instance.PlayDamageEffects();
            }
            
            Debug.Log($"Inimigo atacou causando {enemyDamage} de dano!");
        }

        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.UpdateHealthBars();
        }

        if (PlayerStats.Instance != null && !PlayerStats.Instance.IsAlive())
        {
            Debug.Log("Player derrotado!");
            yield return new WaitForSeconds(TURN_DELAY);
            EndBattle();
            yield break;
        }

        yield return new WaitForSeconds(TURN_DELAY);

        isProcessingTurn = false;

        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.OpenMainMenu();
        }
    }

    public void EndBattle()
    {
        if (!isInBattle)
            return;

        isInBattle = false;
        isProcessingTurn = false;

        battleArena.SetActive(false);

        PlayerControlManager.Instance.UnlockControl(CONTROL_LOCK_ID);

        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.CloseBattleUI();
        }

        Debug.Log("BattleManager: Batalha encerrada.");
    }

    public bool IsInBattle()
    {
        return isInBattle;
    }
}
