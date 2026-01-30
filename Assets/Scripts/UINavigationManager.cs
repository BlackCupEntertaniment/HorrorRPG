using System;
using System.Collections.Generic;
using UnityEngine;

public class UINavigationManager : MonoBehaviour
{
    public static UINavigationManager Instance { get; private set; }

    private Stack<UIState> navigationStack = new Stack<UIState>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBackNavigation();
        }
    }

    public void PushState(UIState state)
    {
        if (state == null)
        {
            Debug.LogWarning("Tentativa de adicionar UIState nulo à pilha de navegação");
            return;
        }

        navigationStack.Push(state);
    }

    public void PopState()
    {
        if (navigationStack.Count > 0)
        {
            navigationStack.Pop();
        }
    }

    public void ClearStack()
    {
        navigationStack.Clear();
    }

    private void HandleBackNavigation()
    {
        if (navigationStack.Count > 0)
        {
            UIState currentState = navigationStack.Peek();
            currentState?.onBackPressed?.Invoke();
        }
    }

    public int GetStackCount()
    {
        return navigationStack.Count;
    }
}

public class UIState
{
    public string stateName;
    public Action onBackPressed;

    public UIState(string name, Action backCallback)
    {
        stateName = name;
        onBackPressed = backCallback;
    }
}
