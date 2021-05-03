using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public AudioSource TooltipSound;

    public AudioSource Woman1Voice1;
    public AudioSource Woman2Voice1;

    public AudioSource Woman1Voice2;
    public AudioSource Woman2Voice2;

    public AudioSource Woman1Voice3;
    public AudioSource Woman2Voice3;

    private int ManVoiceId;
    private int WomanVoiceId;

    private float MinRandomTime = 30f;
    private float MaxRandomTime = 40f;

    private bool isplaying = true;

	// Use this for initialization
	void Start ()
    {
        StartCoroutine(Woman1Speaking());
        StartCoroutine(Woman2Speaking());
    }
	
	// Update is called once per frame
	void Update () {
		
	}


    private IEnumerator Woman1Speaking()
    {
        while (isplaying)
        {
            float TimeforWoman = Random.Range(MinRandomTime, MaxRandomTime);
            yield return new WaitForSeconds(TimeforWoman);
            if (WomanVoiceId == 0)
            {
                Woman1Voice1.Play();
                WomanVoiceId++;
            }

            else if(WomanVoiceId == 1)
            {
                Woman1Voice2.Play();
                WomanVoiceId++;
            }

            else
            {
                Woman1Voice3.Play();
                WomanVoiceId = 0;
            }
        }
    }

    private IEnumerator Woman2Speaking()
    {
        while (isplaying)
        {
            float TimeforMan = Random.Range(MinRandomTime, MaxRandomTime);
            yield return new WaitForSeconds(TimeforMan);
            if (ManVoiceId == 0)
            {
                Woman2Voice1.Play();
                ManVoiceId++;
            }

            else if (ManVoiceId == 1)
            {
                Woman2Voice2.Play();
                ManVoiceId++;
            }

            else
            {
                Woman2Voice3.Play();
                ManVoiceId = 0;
            }
        }
    }
}
