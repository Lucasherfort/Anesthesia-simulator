using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BloodAppearance : MonoBehaviour {

    private float FadeDuration = 1f;

	// Use this for initialization
	void Start ()
    {
        ShowBlood();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void ShowBlood()
    {
        this.GetComponent<Renderer>().material.DOFade(1f, FadeDuration);
    }
}
