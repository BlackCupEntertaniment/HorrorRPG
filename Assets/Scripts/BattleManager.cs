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
    [SerializeField] private EnemyAnimationController enemyAnimationController;
    
    [Header("Timing System")]
    [SerializeField] private AttackTimingBar attackTimingBar;
    [SerializeField] private AttackTimingUI attackTimingUI;

    [Header("Transition Effects")]
    [SerializeField] private BattleTransitionEffects transitionEffects;

    [Header("Enemy Data")]
    [SerializeField] private EnemyData currentEnemyData;

    [Header("Battle Delays")]
    [SerializeField] private float battleStartDelay = 1f;
    [SerializeField] private float enemyDeathDelay = 1f;

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

        StartCoroutine(StartBattleSequence(sourceEnemyRenderer, enemyData));
    }

    private IEnumerator StartBattleSequence(MeshRenderer sourceEnemyRenderer, EnemyData enemyData)
    {
        currentEnemyData = enemyData;
        isInBattle = true;
        currentEnemyHealth = currentEnemyData.maxHealth;

        battleEnemyRenderer.sharedMaterials = sourceEnemyRenderer.sharedMaterials;

        bool transitionComplete = false;
        if (transitionEffects != null)
        {
            transitionEffects.PlayBattleStartEffects(() => transitionComplete = true);
            yield return new WaitUntil(() => transitionComplete);
        }

        battleArena.SetActive(true);

        PlayerControlManager.Instance.LockControl(CONTROL_LOCK_ID);

        if (enemyAnimationController != null)
        {
            enemyAnimationController.PlayStartAnimation();
        }

        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.InitializeBattle();
        }

        if (BattleUIManager.Instance != null)
        {
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

        AttackResult result = AttackResult.Miss;
        bool timingComplete = false;

        if (AttackTimingBar.Instance != null)
        {
            AttackTimingBar.Instance.StartTiming(weapon, (res) =>
            {
                result = res;
                timingComplete = true;
            });
        }
        else
        {
            result = AttackResult.Hit;
            timingComplete = true;
        }

        yield return new WaitUntil(() => timingComplete);

        int damage = weapon.GetDamageByResult(result, currentEnemyData.category);
        currentEnemyHealth = Mathf.Max(0, currentEnemyHealth - damage);

        if (battleEnemyEffects != null)
        {
            battleEnemyEffects.PlayHitEffects();
        }

        string resultText = result == AttackResult.Critical ? "CRÍTICO!" : 
                           result == AttackResult.Hit ? "acertou" : "ERROU!";
        Debug.Log($"Player {resultText} com {weapon.itemName} causando {damage} de dano!");

        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.UpdateHealthBars();
        }

        if (currentEnemyHealth <= 0)
        {
            if (enemyAnimationController != null)
            {
                enemyAnimationController.PlayDeadAnimation();
            }

            Debug.Log("Inimigo derrotado!");
            yield return new WaitForSeconds(enemyDeathDelay);
            
            bool transitionComplete = false;
            if (transitionEffects != null)
            {
                transitionEffects.PlayBattleEndEffects(() => transitionComplete = true);
                yield return new WaitUntil(() => transitionComplete);
            }
            
            EndBattle();
            yield break;
        }

        yield return new WaitForSeconds(TURN_DELAY);

        yield return StartCoroutine(ProcessEnemyAttack());

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

    private IEnumerator ProcessEnemyAttack()
    {
        int enemyDamage = currentEnemyData.baseDamage;

        if (DefenseManager.Instance != null)
        {
            DefenseManager.Instance.EnableDefense(true);
        }

        AttackData selectedAttack = currentEnemyData.GetRandomAttack();

        if (selectedAttack != null && ProjectileManager.Instance != null)
        {
            int projectilesHit = 0;
            int totalProjectiles = selectedAttack.GetProjectileCount();

            ProjectileManager.Instance.SetDamageCallback((damage, defense) =>
            {
                projectilesHit++;

                if (PlayerStats.Instance != null && damage > 0)
                {
                    PlayerStats.Instance.TakeDamage(damage);

                    if (BattlePlayerEffects.Instance != null)
                    {
                        BattlePlayerEffects.Instance.PlayDamageEffects(defense);
                    }

                    Debug.Log($"Projétil causou {damage} de dano!");
                }
                else if (damage == 0)
                {
                    if (BattlePlayerEffects.Instance != null)
                    {
                        BattlePlayerEffects.Instance.PlayDamageEffects(DefenseType.Perfect);
                    }

                    Debug.Log("Projétil bloqueado! Nenhum dano recebido.");
                }

                if (BattleUIManager.Instance != null)
                {
                    BattleUIManager.Instance.UpdateHealthBars();
                }
            });

            yield return StartCoroutine(
                ProjectileManager.Instance.ExecuteAttack(selectedAttack, enemyDamage)
            );

            Debug.Log($"Ataque '{selectedAttack.attackName}' concluído! {projectilesHit}/{totalProjectiles} projéteis atingiram o alvo.");
        }
        else
        {
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
        }

        if (DefenseManager.Instance != null)
        {
            DefenseManager.Instance.EnableDefense(false);
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
