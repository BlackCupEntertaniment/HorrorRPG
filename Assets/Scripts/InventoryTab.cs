using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryTab : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tabText;
    [SerializeField] private Image tabImage;

    private ItemCategory category;
    private Color selectedTextColor = Color.black;
    private Color deselectedTextColor = Color.white;

    public void Initialize(ItemCategory itemCategory, string displayText)
    {
        category = itemCategory;
        
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

    public ItemCategory GetCategory()
    {
        return category;
    }
}
