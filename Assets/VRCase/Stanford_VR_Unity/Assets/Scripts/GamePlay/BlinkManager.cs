using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BlinkManager : MonoBehaviour {

    public Image Blink;
    public Image LogoSFH;
    public Image LogoStanford;
    public Text BlinkText;

    private float FadeDuration = 1f;
	// Use this for initialization
	void Start () {
		
	}
	
    public void HideBlink()
    {
        LogoSFH.DOFade(0f, FadeDuration);
        LogoStanford.DOFade(0f, FadeDuration);
        BlinkText.DOFade(0f, FadeDuration).OnComplete(()=>
        {
            Blink.DOFade(0f, FadeDuration * 2).OnComplete(() =>
            {
                StanfordVREventDispatcher.dispatchOnCaseStarted();
            });
        });
    }

    public void ShowFinalBlink()
    {
        Blink.DOFade(1f, FadeDuration*2).OnComplete(() =>
        {
            LogoSFH.DOFade(1f, FadeDuration).OnComplete(() =>
            {
                LogoStanford.DOFade(1f, FadeDuration).OnComplete(() =>
                {
                    BlinkText.DOFade(1f, FadeDuration);
                });
            });
        });

    }

	// Update is called once per frame
	void Update () {
		
	}
}
