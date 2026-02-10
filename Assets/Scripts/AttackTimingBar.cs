using UnityEngine;
using System;

public class AttackTimingBar : MonoBehaviour
{
    public static AttackTimingBar Instance { get; private set; }
    
    private WeaponData currentWeapon;
    private float currentPosition = 0f;
    private float direction = 1f;
    private bool isActive = false;
    private Action<AttackResult> onComplete;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void StartTiming(WeaponData weapon, Action<AttackResult> callback)
    {
        currentWeapon = weapon;
        onComplete = callback;
        currentPosition = 0f;
        direction = 1f;
        isActive = true;
        
        if (AttackTimingUI.Instance != null)
        {
            AttackTimingUI.Instance.Show();
            AttackTimingUI.Instance.SetupZones(weapon);
        }
    }
    
    private void Update()
    {
        if (!isActive || currentWeapon == null)
            return;
        
        currentPosition += direction * currentWeapon.markerSpeed * Time.deltaTime;
        
        if (currentPosition >= 1f)
        {
            currentPosition = 1f;
            direction = -1f;
        }
        else if (currentPosition <= 0f)
        {
            currentPosition = 0f;
            direction = 1f;
        }
        
        if (AttackTimingUI.Instance != null)
        {
            AttackTimingUI.Instance.UpdateMarkerPosition(currentPosition);
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            EvaluateAndComplete();
        }
    }
    
    private void EvaluateAndComplete()
    {
        isActive = false;
        
        AttackResult result = currentWeapon.EvaluateTimingPosition(currentPosition);
        
        if (AttackTimingUI.Instance != null)
        {
            AttackTimingUI.Instance.Hide();
        }
        
        onComplete?.Invoke(result);
    }
    
    public float GetCurrentPosition()
    {
        return currentPosition;
    }
    
    public bool IsActive()
    {
        return isActive;
    }
}
