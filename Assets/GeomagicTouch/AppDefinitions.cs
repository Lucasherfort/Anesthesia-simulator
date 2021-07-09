// HEADER

// INCLUDES

/// <summary>
/// Namespace for Needle Simulator
/// </summary>
namespace ViRTSA
{
    /// <summary>
    /// Navigation technique enum
    /// </summary>
    public enum NavigationTechnique : int
    {
        TOUCH_BASED = 0,    // CHANGE: TOUCH_BASED
        HEAD_TRACKING = 1
    }

    /// <summary>
    /// Navigation technique enum
    /// </summary>
    public enum SimulationMode : int
    {
        MANIPULATION = 0,
        VERIFICATION = 1
    }

    /// <summary>
    /// Enum to identify the event of the button pressed
    /// </summary>
    public enum ButtonState : uint
    {
        PRESSED = 0,
        PRESSED_UP = 1,
        PRESSED_DOWN = 2
    }

    /// <summary>
    /// Enum to identify the haptic button pressed
    /// </summary>
    public enum HapticButtons : uint
    {
        NONE = 0,
        ONE = 1,
        TWO = 2
    }

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

    public enum UITexts : int
    {
        FINISH_TRAINING_MSG = 2,
        FINISH_EXPERIMENT_MSG = 3,
        GOODBYE_MSG = 4,
        PARTICIPANT = 5,
        TECHNIQUE = 6,
        MODE = 7,
        PARTICIPANT_HELP = 8,
        DO_TRAINING = 10,
        CONFIG = 11,
        LOG = 12,
        HAND = 13,
        LEFT = 14,
        RIGHT = 15,
        MALE = 16,
        FEMALE = 17,
        LANGUAGE = 18,
        START = 19,
        ENGLISH = 20,
        FRENCH = 21,
        SPANISH = 22,
        TOUCH_BASED = 23,
        HEAD_TRACKING = 24,
        MANIPULATION = 25,
        VERIFICATION = 26,
        BACK_BUTTON = 27,
        REPLAY_BUTTON = 28,
        NEXT_BUTTON = 29,
        REDO_BUTTON = 30,
        RESTART_BUTTON = 31,
        PARTICIPANT_VIEW = 32,
        ENVIRONMENT_VIEW = 33,
        TRAINING_TITLE_STEP_1 = 34,
        TRAINING_INSTRUCTIONS_STEP_1_HT = 35,
        TRAINING_INSTRUCTIONS_STEP_1_TB = 36,
        TRAINING_TITLE_STEP_11 = 37,
        TRAINING_INSTRUCTIONS_STEP_11 = 38,
        TRAINING_TITLE_STEP_12 = 39,
        TRAINING_INSTRUCTIONS_STEP_12 = 40,
        TRAINING_TITLE_STEP_14 = 41,
        TRAINING_INSTRUCTIONS_STEP_14 = 42,
        TRIAL = 43,
        TRIAL_WARNING = 44,
        TRIAL_START = 45,
        TRIAL_REDO = 46,
        TRIAL_FINISH = 47
    }
}