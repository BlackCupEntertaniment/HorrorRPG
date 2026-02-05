using UnityEngine;

[System.Serializable]
public class ProjectileSpawnData
{
    [Header("Projectile Settings")]
    [Tooltip("Configuração do projétil a ser spawnado")]
    public ProjectileConfig projectileConfig;
    
    [Header("Timing")]
    [Tooltip("Delay em segundos antes de spawnar este projétil")]
    public float spawnDelay = 0f;
    
    [Header("Damage")]
    [Tooltip("Multiplicador de dano para este projétil (1.0 = dano base do inimigo)")]
    [Range(0f, 5f)]
    public float damageMultiplier = 1f;
}
