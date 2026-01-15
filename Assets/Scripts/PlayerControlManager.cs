using System.Collections.Generic;
using UnityEngine;

public class PlayerControlManager : MonoBehaviour
{
    public static PlayerControlManager Instance { get; private set; }

    private HashSet<string> controlLocks = new HashSet<string>();
    private FirstPersonController playerController;
    private PlayerInteraction playerInteraction;

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

        playerController = GetComponent<FirstPersonController>();
        playerInteraction = GetComponent<PlayerInteraction>();
    }

    public void LockControl(string lockId)
    {
        if (controlLocks.Add(lockId))
        {
            UpdateControlState();
        }
    }

    public void UnlockControl(string lockId)
    {
        if (controlLocks.Remove(lockId))
        {
            UpdateControlState();
        }
    }

    public bool IsControlLocked()
    {
        return controlLocks.Count > 0;
    }

    public bool IsLockedBy(string lockId)
    {
        return controlLocks.Contains(lockId);
    }

    private void UpdateControlState()
    {
        bool shouldBeEnabled = controlLocks.Count == 0;

        if (playerController != null)
        {
            playerController.SetControlEnabled(shouldBeEnabled);
        }

        if (playerInteraction != null)
        {
            playerInteraction.SetInteractionEnabled(shouldBeEnabled);
        }
    }

    public void ClearAllLocks()
    {
        controlLocks.Clear();
        UpdateControlState();
    }
}
