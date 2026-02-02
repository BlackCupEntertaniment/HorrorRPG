using UnityEngine;
using System.Collections;

public enum ProjectileState
{
    Looping,
    Traveling,
    Blocked,
    Hit
}

public enum DefensePosition
{
    Left = 0,
    Up = 1,
    Right = 2
}

public class BattleProjectile : MonoBehaviour
{
    private ProjectileConfig config;
    private ProjectileState currentState;
    private DefensePosition targetPosition;
    private Transform targetTransform;
    private Vector3 loopCenter;
    private float loopAngle;
    private float loopTimer;
    private float loopDuration;
    private int damageAmount;
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock propertyBlock;

    public ProjectileState CurrentState => currentState;
    public DefensePosition TargetPosition => targetPosition;
    public int DamageAmount => damageAmount;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
    }

    public void Initialize(ProjectileConfig projectileConfig, Vector3 center, int damage)
    {
        config = projectileConfig;
        loopCenter = center;
        damageAmount = damage;
        currentState = ProjectileState.Looping;
        loopAngle = Random.Range(0f, 360f);
        loopTimer = 0f;
        loopDuration = Random.Range(config.minLoopTime, config.maxLoopTime);
        
        if (meshRenderer != null && propertyBlock != null)
        {
            meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_BaseColor", config.projectileColor);
            meshRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    public void StartTravelToTarget(Transform target, DefensePosition position)
    {
        targetTransform = target;
        targetPosition = position;
        currentState = ProjectileState.Traveling;
    }

    public void BlockProjectile()
    {
        currentState = ProjectileState.Blocked;
        StartCoroutine(DestroyAfterDelay(0.5f));
    }

    public void HitTarget()
    {
        currentState = ProjectileState.Hit;
        StartCoroutine(DestroyAfterDelay(0.2f));
    }

    private void Update()
    {
        switch (currentState)
        {
            case ProjectileState.Looping:
                UpdateLooping();
                break;
            case ProjectileState.Traveling:
                UpdateTraveling();
                break;
        }
    }

    private void UpdateLooping()
    {
        loopTimer += Time.deltaTime;
        loopAngle += config.loopSpeed * Time.deltaTime;
        
        Vector3 offset = new Vector3(
            Mathf.Cos(loopAngle) * config.loopRadius,
            0f,
            Mathf.Sin(loopAngle) * config.loopRadius
        );
        
        transform.position = loopCenter + offset;
        
        transform.rotation = Quaternion.Euler(0f, 0f, loopAngle * Mathf.Rad2Deg);
        
        if (loopTimer >= loopDuration)
        {
            ProjectileManager.Instance.OnProjectileReadyToAttack(this);
        }
    }

    private void UpdateTraveling()
    {
        if (targetTransform == null)
        {
            ReturnToPool();
            return;
        }
        
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetTransform.position,
            config.travelSpeed * Time.deltaTime
        );
        
        float distance = Vector3.Distance(transform.position, targetTransform.position);
        
        if (distance < 0.1f)
        {
            ProjectileManager.Instance.OnProjectileReachedTarget(this);
        }
    }

    public float GetDistanceToTarget()
    {
        if (targetTransform == null)
            return float.MaxValue;
        
        return Vector3.Distance(transform.position, targetTransform.position);
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (ProjectileManager.Instance != null)
        {
            ProjectileManager.Instance.ReturnProjectileToPool(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
