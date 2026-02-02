using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile Config", menuName = "Battle/Projectile Config")]
public class ProjectileConfig : ScriptableObject
{
    [Header("Loop Settings")]
    public float loopRadius = 2f;
    public float loopSpeed = 5f;
    
    [Header("Attack Settings")]
    public float minLoopTime = 2f;
    public float maxLoopTime = 5f;
    public float travelSpeed = 8f;
    
    [Header("Visual")]
    public GameObject projectilePrefab;
    public Color projectileColor = Color.red;
}
