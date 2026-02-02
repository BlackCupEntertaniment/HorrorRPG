using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum DefenseResult
{
    Perfect,
    Partial,
    Failed,
    TooLate
}

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;

    [Header("Defense Targets")]
    [SerializeField] private Transform leftTarget;
    [SerializeField] private Transform upTarget;
    [SerializeField] private Transform rightTarget;

    [Header("Defense Distances")]
    [SerializeField] private float perfectDefenseDistance = 0.5f;
    [SerializeField] private float partialDefenseDistance = 1.5f;

    [Header("Pool Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int poolSize = 10;

    private List<BattleProjectile> projectilePool = new List<BattleProjectile>();
    private BattleProjectile activeProjectile;
    private bool canDefend;
    private bool defenseBlocked;
    private System.Action<int> onProjectileResolved;

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

        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(projectilePrefab, transform);
            obj.SetActive(false);
            BattleProjectile projectile = obj.GetComponent<BattleProjectile>();
            if (projectile != null)
            {
                projectilePool.Add(projectile);
            }
        }
    }

    private void Update()
    {
        if (!canDefend || activeProjectile == null || defenseBlocked)
            return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            AttemptDefense(DefensePosition.Left);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            AttemptDefense(DefensePosition.Up);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            AttemptDefense(DefensePosition.Right);
        }
    }

    public IEnumerator ExecuteProjectileAttack(ProjectileConfig config, int damage)
    {
        defenseBlocked = false;
        canDefend = false;

        BattleProjectile projectile = GetProjectileFromPool();
        if (projectile == null)
        {
            Debug.LogError("Nenhum projétil disponível no pool!");
            onProjectileResolved?.Invoke(damage);
            yield break;
        }

        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;

        projectile.gameObject.SetActive(true);
        projectile.transform.position = spawnPosition;
        projectile.Initialize(config, spawnPosition, damage);
        activeProjectile = projectile;

        while (activeProjectile != null && activeProjectile.CurrentState == ProjectileState.Looping)
        {
            yield return null;
        }

        if (activeProjectile != null && activeProjectile.CurrentState == ProjectileState.Traveling)
        {
            canDefend = true;
        }

        while (activeProjectile != null && 
               (activeProjectile.CurrentState == ProjectileState.Traveling ||
                activeProjectile.CurrentState == ProjectileState.Blocked))
        {
            yield return null;
        }

        canDefend = false;
        activeProjectile = null;
    }

    public void OnProjectileReadyToAttack(BattleProjectile projectile)
    {
        if (projectile != activeProjectile)
            return;

        DefensePosition randomPosition = (DefensePosition)Random.Range(0, 3);
        Transform target = GetTargetTransform(randomPosition);

        if (target != null)
        {
            projectile.StartTravelToTarget(target, randomPosition);
        }
        else
        {
            Debug.LogError("Target transform não encontrado!");
            ReturnProjectileToPool(projectile);
        }
    }

    public void OnProjectileReachedTarget(BattleProjectile projectile)
    {
        if (projectile != activeProjectile)
            return;

        projectile.HitTarget();
        onProjectileResolved?.Invoke(projectile.DamageAmount);
    }

    private void AttemptDefense(DefensePosition inputPosition)
    {
        if (activeProjectile == null)
            return;

        if (inputPosition != activeProjectile.TargetPosition)
        {
            return;
        }

        float distance = activeProjectile.GetDistanceToTarget();
        DefenseResult result = EvaluateDefense(distance);

        switch (result)
        {
            case DefenseResult.Perfect:
                activeProjectile.BlockProjectile();
                onProjectileResolved?.Invoke(0);
                Debug.Log("Defesa Perfeita! Dano negado.");
                break;

            case DefenseResult.Partial:
                activeProjectile.BlockProjectile();
                int halfDamage = Mathf.CeilToInt(activeProjectile.DamageAmount * 0.5f);
                onProjectileResolved?.Invoke(halfDamage);
                Debug.Log($"Defesa Parcial! Dano reduzido para {halfDamage}.");
                break;

            case DefenseResult.Failed:
                defenseBlocked = true;
                Debug.Log("Defesa Falhou! Player bloqueado de defender novamente.");
                break;

            case DefenseResult.TooLate:
                Debug.Log("Muito tarde para defender!");
                break;
        }
    }

    private DefenseResult EvaluateDefense(float distance)
    {
        if (distance <= perfectDefenseDistance)
        {
            return DefenseResult.Perfect;
        }
        else if (distance <= partialDefenseDistance)
        {
            return DefenseResult.Partial;
        }
        else
        {
            return DefenseResult.Failed;
        }
    }

    private Transform GetTargetTransform(DefensePosition position)
    {
        switch (position)
        {
            case DefensePosition.Left:
                return leftTarget;
            case DefensePosition.Up:
                return upTarget;
            case DefensePosition.Right:
                return rightTarget;
            default:
                return null;
        }
    }

    private BattleProjectile GetProjectileFromPool()
    {
        foreach (BattleProjectile projectile in projectilePool)
        {
            if (!projectile.gameObject.activeInHierarchy)
            {
                return projectile;
            }
        }

        GameObject newObj = Instantiate(projectilePrefab, transform);
        BattleProjectile newProjectile = newObj.GetComponent<BattleProjectile>();
        if (newProjectile != null)
        {
            projectilePool.Add(newProjectile);
            return newProjectile;
        }

        return null;
    }

    public void ReturnProjectileToPool(BattleProjectile projectile)
    {
        if (projectile != null)
        {
            projectile.gameObject.SetActive(false);
        }
    }

    public void SetDamageCallback(System.Action<int> callback)
    {
        onProjectileResolved = callback;
    }
}
