using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Management;

public enum Mode
{
    Training,
    Reality
}


[RequireComponent(typeof(AudioBox))]
public class GameManager : MonoBehaviour
{
    static public GameManager Instance{get; private set;}

    [SerializeField]
    private bool EnabledVR = false;

    public bool EnabledHaptic = false;

    [SerializeField]
    private AudioBox audioBox;

    public Mode Mode = Mode.Reality;

    private void Awake()
    {
        if(Instance)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        audioBox = GetComponent<AudioBox>();

        if (EnabledVR)
        {
            XRGeneralSettings.Instance.Manager.StartSubsystems();
        }
        else
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
        }
    }

    private void Start()
    {
        audioBox.PlayLoop(SoundLoop.RoomSoundEffect);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!Application.isEditor)
            {
                // AbortSimualtion
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }
    }
}
