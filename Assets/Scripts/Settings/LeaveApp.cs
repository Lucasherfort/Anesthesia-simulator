using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class LeaveApp : MonoBehaviour
{
    private void Start() 
    {
        InputManager.Input.Apps.Quit.performed += AbortSimualtion;
    }

    public void AbortSimualtion(InputAction.CallbackContext _context)
    {
        if (!Application.isEditor)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }

    private void OnDestroy() 
    {
        InputManager.Input.Apps.Quit.performed -= AbortSimualtion;
    }
}
