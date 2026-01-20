using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BattlePlayerEffects : MonoBehaviour
{
    public static BattlePlayerEffects Instance { get; private set; }

    [Header("Damage Flash Settings")]
    [SerializeField] private Image damageFlashImage;
    [SerializeField] private float flashDuration = 0.3f;
    [SerializeField] private float flashAlpha = 0.5f;

    [Header("Camera Shake Settings")]
    [SerializeField] private bool enableCameraShake = false;

    private Coroutine damageFlashCoroutine;

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

        if (damageFlashImage != null)
        {
            Color color = damageFlashImage.color;
            color.a = 0f;
            damageFlashImage.color = color;
        }
    }

    public void PlayDamageEffects()
    {
        PlayDamageFlash();
        
        if (enableCameraShake)
        {
            PlayCameraShake();
        }
    }

    public void PlayDamageFlash()
    {
        if (damageFlashImage == null)
            return;

        if (damageFlashCoroutine != null)
        {
            StopCoroutine(damageFlashCoroutine);
        }

        damageFlashCoroutine = StartCoroutine(DamageFlashCoroutine());
    }

    public void PlayCameraShake()
    {
    }

    private IEnumerator DamageFlashCoroutine()
    {
        float elapsedTime = 0f;
        Color color = damageFlashImage.color;

        while (elapsedTime < flashDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / flashDuration;
            float alpha = Mathf.Lerp(flashAlpha, 0f, normalizedTime);
            
            color.a = alpha;
            damageFlashImage.color = color;
            
            yield return null;
        }

        color.a = 0f;
        damageFlashImage.color = color;
        damageFlashCoroutine = null;
    }
}
