using UnityEngine;

public enum EnemyType
{
    None,
    Demon,
    Ghost,
    Zombie
}

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
public class WeaponData : ItemData
{
    [Header("Weapon Stats")]
    public int baseDamage = 10;
    public EnemyType effectiveAgainst = EnemyType.None;
    public float effectivenessMultiplier = 2f;
    
    [Header("Ammo System")]
    public ItemData ammoType;
    
    public bool requiresAmmo => ammoType != null;
    public bool IsDefaultWeapon => !requiresAmmo;
    
    public int GetEffectiveDamage(EnemyType targetType)
    {
        if (targetType == effectiveAgainst)
        {
            return Mathf.RoundToInt(baseDamage * effectivenessMultiplier);
        }
        return baseDamage;
    }
    
    public bool CanUse(InventoryManager inventory)
    {
        if (!requiresAmmo) 
            return true;
        
        return inventory.HasItem(ammoType, 1);
    }
}
