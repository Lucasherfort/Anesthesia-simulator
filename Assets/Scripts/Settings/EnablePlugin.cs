using UnityEngine;

public class EnablePlugin : MonoBehaviour
{
    static public EnablePlugin Instance { get; private set; }

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
}
