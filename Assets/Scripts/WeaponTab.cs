using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum WeaponCategory
{
    Used,
    Basic,
    Limited
}

public class WeaponTab : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tabText;
    [SerializeField] private Image tabImage;

    private WeaponCategory category;
    private Color selectedTextColor = Color.black;
    private Color deselectedTextColor = Color.white;

    public void Initialize(WeaponCategory weaponCategory, string displayText)
    {
        category = weaponCategory;
        
        if (tabText != null)
        {
            tabText.text = displayText;
        }
        
        SetSelected(false);
    }

    public void SetSelected(bool isSelected)
    {
        if (tabText != null)
        {
            tabText.color = isSelected ? selectedTextColor : deselectedTextColor;
        }

        if (tabImage != null)
        {
            tabImage.enabled = !isSelected;
        }
    }

    public WeaponCategory GetCategory()
    {
        return category;
    }
}
