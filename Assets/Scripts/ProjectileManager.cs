using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;

    [Header("Defense Targets")]
    [SerializeField] private Transform leftTarget;
    [SerializeField] private Transform upTarget;
    [SerializeField] private Transform rightTarget;

    [Header("Pool Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int poolSize = 10;

    private List<BattleProjectile> projectilePool = new List<BattleProjectile>();
    private BattleProjectile activeProjectile;
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
    }

    public IEnumerator ExecuteProjectileAttack(ProjectileConfig config, int damage)
    {
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

        while (activeProjectile != null && 
               activeProjectile.CurrentState == ProjectileState.Traveling)
        {
            yield return null;
        }

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

        DefensePosition position = projectile.TargetPosition;
        int baseDamage = projectile.DamageAmount;
        
        int finalDamage = baseDamage;
        if (DefenseManager.Instance != null)
        {
            finalDamage = DefenseManager.Instance.OnProjectileHit(position, baseDamage);
        }

        projectile.HitTarget();
        onProjectileResolved?.Invoke(finalDamage);
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
