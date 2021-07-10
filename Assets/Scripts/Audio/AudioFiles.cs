using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioFiles", menuName = "Audio/AudioFiles", order = 2)]
public class AudioFiles : ScriptableObject
{
    [Header("Ambiance")]
    [SerializeField]
    private SoundLoopClip roomSoundEffect = null;
 
    public SoundOneShotClip SoundOneShotToClip (SoundOneShot sound) 
    {
        switch (sound) 
        {
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

            default : 
                Debug.LogError("SoundLoopClip : " + sound + " was not found!");
                return null;
        }
    }
}

public enum SoundOneShot 
{

}

public enum SoundLoop 
{
    RoomSoundEffect
}
