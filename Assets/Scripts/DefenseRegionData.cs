using UnityEngine;

[System.Serializable]
public class DefenseRegionData
{
    public DefensePosition position;
    public float defenseTimer;
    public bool isCharging;
    public bool isActive;

    private const float MAX_DEFENSE_TIME = 1.0f;

    public DefenseRegionData(DefensePosition pos)
    {
        position = pos;
        defenseTimer = 0f;
        isCharging = false;
        isActive = false;
    }

    public void StartCharging()
    {
        isCharging = true;
    }

    public void StopCharging()
    {
        if (isCharging && defenseTimer > 0f)
        {
            isActive = true;
        }
        isCharging = false;
    }

    public void UpdateTimer(float deltaTime)
    {
        if (isCharging)
        {
            defenseTimer = Mathf.Min(defenseTimer + deltaTime, MAX_DEFENSE_TIME);
        }
    }

    public void Reset()
    {
        defenseTimer = 0f;
        isCharging = false;
        isActive = false;
    }

    public float GetDamageMultiplier()
    {
        if (!isActive || defenseTimer <= 0f)
            return 1.0f;

        return 1.0f - (defenseTimer / MAX_DEFENSE_TIME);
    }

    public bool HasDefense()
    {
        return isActive && defenseTimer > 0f;
    }
}
