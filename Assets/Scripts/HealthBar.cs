using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image healthBarImage;

    private const string DEFAULT_PLAYER_NAME = "player";

    public void SetName(string characterName)
    {
        if (nameText != null)
        {
            nameText.text = characterName;
        }
    }

    public void SetHealth(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }

        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        }
    }

    public void Initialize(string characterName, int currentHealth, int maxHealth)
    {
        SetName(characterName);
        SetHealth(currentHealth, maxHealth);
    }

    public void InitializeAsPlayerHealthBar(int currentHealth, int maxHealth)
    {
        Initialize(DEFAULT_PLAYER_NAME, currentHealth, maxHealth);
    }
}
