using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Battle/Enemy")]
public class EnemyData : ScriptableObject
{
    [Header("Enemy Info")]
    public string enemyName = "Enemy";
    
    [Header("Enemy Stats")]
    public int maxHealth = 50;
    public int baseDamage = 10;
    public EnemyType category = EnemyType.None;
}
