using System;
using UnityEngine;

public class DiscardConfirmationMenu : MonoBehaviour
{
    [SerializeField] private SelectableButton confirmButton;
    [SerializeField] private SelectableButton cancelButton;

    private SelectableButton currentlySelectedButton;
    private int currentSelectedIndex = 0;
    private SelectableButton[] buttons;
    private Action onConfirm;
    private Action onCancel;

    private void Awake()
    {
        buttons = new SelectableButton[] { cancelButton, confirmButton };
    }

    public void Show(Action confirmCallback, Action cancelCallback)
    {
        onConfirm = confirmCallback;
        onCancel = cancelCallback;
        
        gameObject.SetActive(true);
        currentSelectedIndex = 0;
        SelectButton(currentSelectedIndex);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        
        if (currentlySelectedButton != null)
        {
            currentlySelectedButton.SetSelected(false);
            currentlySelectedButton = null;
        }
    }

    public void HandleNavigation()
    {
        bool moveDown = Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);
        bool moveUp = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);

        if (moveDown)
        {
            currentSelectedIndex = (currentSelectedIndex + 1) % buttons.Length;
            SelectButton(currentSelectedIndex);
        }
        else if (moveUp)
        {
            currentSelectedIndex--;
            if (currentSelectedIndex < 0)
            {
                currentSelectedIndex = buttons.Length - 1;
            }
            SelectButton(currentSelectedIndex);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ExecuteCurrentButton();
        }
    }

    private void SelectButton(int index)
    {
        if (currentlySelectedButton != null)
        {
            currentlySelectedButton.SetSelected(false);
        }

        currentlySelectedButton = buttons[index];
        currentlySelectedButton.SetSelected(true);
    }

    private void ExecuteCurrentButton()
    {
        if (currentlySelectedButton == confirmButton)
        {
            onConfirm?.Invoke();
        }
        else if (currentlySelectedButton == cancelButton)
        {
            onCancel?.Invoke();
        }
    }
}
