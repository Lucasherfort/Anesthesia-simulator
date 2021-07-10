using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioBox))]
public class GameManager : MonoBehaviour
{
    static public GameManager Instance{get; private set;}

    [SerializeField]
    private AudioBox audioBox;

    private void Awake()
    {
        if(Instance)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        audioBox = GetComponent<AudioBox>();
    }

    private void Start()
    {
        audioBox.PlayLoop(SoundLoop.RoomSoundEffect);
    }
}
