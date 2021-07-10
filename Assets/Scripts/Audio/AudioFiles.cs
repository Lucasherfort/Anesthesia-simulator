using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioFiles", menuName = "Audio/AudioFiles", order = 2)]
public class AudioFiles : ScriptableObject
{
    [SerializeField]
    private SoundLoopClip roomSoundEffect = null;

    [SerializeField]
    private SoundLoopClip fireAlarm = null;

    [SerializeField]
    private SingleSoundOneShotClip phoneRinging = null;
 
    public SoundOneShotClip SoundOneShotToClip (SoundOneShot sound) 
    {
        switch (sound) 
        {
            case SoundOneShot.PhoneRinging : return phoneRinging;

            default : 
                Debug.LogError("SoundOneShotClip : " + sound + " was not found!");
                return null;
        }
    }

    public SoundLoopClip SoundLoopToClip (SoundLoop sound) 
    {
        switch (sound) 
        {
            case SoundLoop.RoomSoundEffect : return roomSoundEffect;
            case SoundLoop.FireAlarm : return fireAlarm;

            default : 
                Debug.LogError("SoundLoopClip : " + sound + " was not found!");
                return null;
        }
    }
}

public enum SoundOneShot 
{
    PhoneRinging
}

public enum SoundLoop 
{
    RoomSoundEffect,
    FireAlarm
}
