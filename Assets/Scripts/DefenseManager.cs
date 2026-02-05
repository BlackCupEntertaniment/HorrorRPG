using UnityEngine;
using System.Collections.Generic;

public class DefenseManager : MonoBehaviour
{
    public static DefenseManager Instance { get; private set; }

    [Header("Defense Settings")]
    [SerializeField] private float globalCooldown = 1.5f;

    [Header("Input Settings")]
    [SerializeField] private KeyCode leftDefenseKey = KeyCode.LeftArrow;
    [SerializeField] private KeyCode upDefenseKey = KeyCode.UpArrow;
    [SerializeField] private KeyCode rightDefenseKey = KeyCode.RightArrow;

    private Dictionary<DefensePosition, DefenseRegionData> defenseRegions;
    private float globalCooldownTimer;
    private bool defenseEnabled;

    public bool IsDefenseEnabled => defenseEnabled;

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

        InitializeDefenseRegions();
    }

    private void InitializeDefenseRegions()
    {
        defenseRegions = new Dictionary<DefensePosition, DefenseRegionData>
        {
            { DefensePosition.Left, new DefenseRegionData(DefensePosition.Left) },
            { DefensePosition.Up, new DefenseRegionData(DefensePosition.Up) },
            { DefensePosition.Right, new DefenseRegionData(DefensePosition.Right) }
        };
    }

    private void Update()
    {
        if (!defenseEnabled)
            return;

        UpdateGlobalCooldown();
        UpdateDefenseTimers();
        ProcessDefenseInput();
    }

    private void UpdateGlobalCooldown()
    {
        if (globalCooldownTimer > 0f)
        {
            globalCooldownTimer -= Time.deltaTime;
        }
    }

    private void UpdateDefenseTimers()
    {
        foreach (var region in defenseRegions.Values)
        {
            region.UpdateTimer(Time.deltaTime);
        }
    }

    private void ProcessDefenseInput()
    {
        if (globalCooldownTimer > 0f)
            return;

        ProcessRegionInput(DefensePosition.Left, leftDefenseKey);
        ProcessRegionInput(DefensePosition.Up, upDefenseKey);
        ProcessRegionInput(DefensePosition.Right, rightDefenseKey);
    }

    private void ProcessRegionInput(DefensePosition position, KeyCode key)
    {
        DefenseRegionData region = defenseRegions[position];

        if (Input.GetKeyDown(key))
        {
            if (!region.isActive && globalCooldownTimer <= 0f)
            {
                region.Activate();
                globalCooldownTimer = globalCooldown;
                
                PlayDefenseAnimation(position);
                
                Debug.Log($"Defesa {position} ativada! Timer iniciado.");
            }
        }
    }

    private void PlayDefenseAnimation(DefensePosition position)
    {
        if (HandAnimationManager.Instance != null)
        {
            switch (position)
            {
                case DefensePosition.Left:
                    HandAnimationManager.Instance.PlayReachAnimationLeftHand();
                    break;
                case DefensePosition.Up:
                    HandAnimationManager.Instance.PlayReachAnimation();
                    break;
                case DefensePosition.Right:
                    HandAnimationManager.Instance.PlayReachAnimationRightHand();
                    break;
            }
        }
    }

    public void EnableDefense(bool enable)
    {
        defenseEnabled = enable;
        
        if (!enable)
        {
            ResetAllDefenses();
        }
        
        Debug.Log($"Sistema de defesa {(enable ? "ativado" : "desativado")}");
    }

    public int OnProjectileHit(DefensePosition position, int baseDamage)
    {
        if (!defenseRegions.ContainsKey(position))
        {
            Debug.LogError($"Posição de defesa inválida: {position}");
            return baseDamage;
        }

        DefenseRegionData region = defenseRegions[position];
        
        if (!region.HasDefense())
        {
            Debug.Log($"Projétil atingiu {position} - Sem defesa! Dano total: {baseDamage}");
            return baseDamage;
        }

        float damageMultiplier = region.GetDamageMultiplier();
        int finalDamage = Mathf.CeilToInt(baseDamage * damageMultiplier);
        
        float normalizedTime = region.defenseTimer / 1.0f;
        float defensePercent = (1.0f - damageMultiplier) * 100f;
        
        string timingQuality = GetTimingQuality(normalizedTime);
        Debug.Log($"Projétil atingiu {position} - {timingQuality} (Timer: {normalizedTime:P0}) - Bloqueio {defensePercent:F0}% - Dano: {finalDamage}/{baseDamage}");
        
        region.Reset();
        
        return finalDamage;
    }

    private string GetTimingQuality(float normalizedTime)
    {
        if (normalizedTime < 0.1f) return "MUITO CEDO";
        if (normalizedTime < 0.4f) return "CEDO";
        if (normalizedTime < 0.6f) return "PERFEITO";
        if (normalizedTime < 0.9f) return "TARDE";
        return "MUITO TARDE";
    }

    public void ResetAllDefenses()
    {
        foreach (var region in defenseRegions.Values)
        {
            region.Reset();
        }
        globalCooldownTimer = 0f;
    }

    public DefenseRegionData GetRegionData(DefensePosition position)
    {
        if (defenseRegions.ContainsKey(position))
        {
            return defenseRegions[position];
        }
        return null;
    }

    public float GetGlobalCooldownRemaining()
    {
        return Mathf.Max(0f, globalCooldownTimer);
    }
}
