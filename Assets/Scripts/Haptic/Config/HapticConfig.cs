using UnityEngine;

[CreateAssetMenu(fileName = "HapticConfig", menuName = "Haptic/HapticConfig", order = 1)]
public class HapticConfig : ScriptableObject
{
    public Vector3 Force_Left = Vector3.zero;
    public Vector3 Force_Right = Vector3.zero;

    public double firstPlaneStiffness = .25;
    public double secondPlaneStiffness = .33;

    public float positionFirstPlane = 0;
    public float positionSecondPlane = -5;

    //public SimulationState _hapticState = SimulationState.SIMULATION_OFF;
    //public SimulationState _state = SimulationState.SIMULATION_OFF;

    public Vector3 contactPosition = Vector3.zero;

    public float kStiffness1stLayerHaptic = 31.5f;

    public float forceStiffness1 = 0f;

    public float forceFriction1 = 0f;

    public float forceCutting1 = 0f;

    public float forceStiffness2 = 0f;

    public float forceFriction2 = 0f;

    public float forceCutting2 = 0f;

    public float forceDumping12 = 0f;

    public float forceTotalY = 0f;

    public float kDamping1stLayerHaptic = 1.67f;

    public float kCutting1stLayerHaptic = 1.22f;

    public float UnitLength = 0.01f;
}

/*
public enum SimulationState : int
{
    SIMULATION_START = -1,
    SIMULATION_END = -2,
    SIMULATION_OFF = 0,
    SIMULATION_ON = 1,
    SIMULATION_START_POINT = 2,
    SIMULATION_IN_PROGRESS = 6,
    TARGET_REACHED = 3,
    SAVING_DATA = 4,
    DATA_SAVED = 5,
    TRAINING_OFF = 0,
    TRAINING_BUTTON = 3,
    TRAINING_FAMILIARIZATION = 2,
    TRAINING_START = 1,
    TRAINING_ON = -1,
    FREE_MANIPULATION = 7,
    SEARCHING_TARGET = 6,
    WAIT_TO_START_BUTTON = 5,
    TECHNICAL_PROBLEM = 4,
    NEEDLE_FEEDBACK_ON = 2,
    NEEDLE_FEEDBACK_OFF = 8,
    TURN_OFF = -10,
    FREEZE_POSITION = 16,
    SWITCH_TO_VERIFICATION_MODE = 17,
    MODE_NEW = 18
}
*/
