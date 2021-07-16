using UnityEngine;

public class AppManager : MonoBehaviour
{
    static public AppManager Instance { get; private set; }

    [SerializeField]
    private bool EnabledVR = false;

    [SerializeField]
    private GameObject HapticScript = null;

    public bool EnabledHaptic = false;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        UnityEngine.XR.XRSettings.enabled = EnabledVR;
        HapticScript.GetComponent<TwoHapticsProbeNeedle>().enabled = EnabledHaptic;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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
