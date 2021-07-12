using System;
using UnityEngine;

public class LeaveApp : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            AbortSimualtion();
        }
    }

    private void AbortSimualtion()
    {
        if (!Application.isEditor)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
