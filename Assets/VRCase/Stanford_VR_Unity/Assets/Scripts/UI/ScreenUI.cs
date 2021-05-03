using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScreenUI : MonoBehaviour
{

    private GameObject Cvs;

    private GameObject Background;
    private GameObject BackgroundShadow;
    private GameObject Anchor;
    private GameObject Rotor;
    private GameObject PolyFull;
    private GameObject PolyFade;
    private GameObject PolyMid;
    public GameObject SwipeArrows { get; private set; }
    private GameObject PressCircles;

    private Text ScreenTitleText;
    private Text ScreenMainText;

    private float FadeDuration = 0.25f;

    private bool _isPlaying = false;
    // Use this for initialization

    void Awake()
    {
        Cvs = this.transform.GetChild(0).gameObject;
        BackgroundShadow = Cvs.transform.Find("BackgroundShadow").gameObject;
        Background = BackgroundShadow.transform.Find("Background").gameObject;
        Anchor = Cvs.transform.Find("Anchor").gameObject;
        Rotor = Cvs.transform.Find("Rotor").gameObject;
        PolyFade = Cvs.transform.Find("PolyFade").gameObject;
        PolyMid = Cvs.transform.Find("PolyMid").gameObject;
        PolyFull = Cvs.transform.Find("PolyFull").gameObject;
        SwipeArrows = Cvs.transform.Find("Arrows").gameObject;
        PressCircles = Cvs.transform.Find("Press").gameObject;
        ScreenTitleText = Cvs.transform.Find("Title").gameObject.GetComponent<Text>();
        ScreenMainText = Cvs.transform.Find("Text").gameObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowUI(bool ispress, string Titlevalue, string Mainvalue)
    {
        Cvs.SetActive(true);


        BackgroundShadow.GetComponent<Image>().DOFade(0.6f, FadeDuration);
        Background.GetComponent<Image>().DOFade(0.9f, FadeDuration).OnComplete(() =>
         {
             Rotor.GetComponent<Image>().DOFade(1f, FadeDuration);
             Anchor.GetComponent<Image>().DOFade(1f, FadeDuration).OnComplete(() => 
             {
                 PolyFade.GetComponent<Image>().DOFade(0.3f, FadeDuration).OnComplete(() =>
                 {
                     PolyMid.GetComponent<Image>().DOFade(0.6f, FadeDuration).OnComplete(() =>
                     {
                         PolyFull.GetComponent<Image>().DOFade(1f, FadeDuration).OnComplete(() =>
                         {
                             _isPlaying = true;
                             ScreenMainText.gameObject.SetActive(true);
                             ScreenTitleText.gameObject.SetActive(true);
                             ScreenTitleText.DOText(Titlevalue, FadeDuration, true, ScrambleMode.None);
                             ScreenMainText.DOText(Mainvalue, FadeDuration, true, ScrambleMode.None);
                             if (ispress) //We want to see press animation
                             {
                                 SwipeArrows.SetActive(false);
                                 PressCircles.transform.localScale = Vector3.zero;
                                 PressCircles.SetActive(true);
                                 StartCoroutine(PressAnimation());
                             }
                             else //Swipe animation
                             {
                                 PressCircles.SetActive(false);
                                 SwipeArrows.SetActive(true);
                                 SwipeArrows.GetComponent<Image>().DOFade(1f, FadeDuration);
                                 StartCoroutine(SwipeAnimation());
                             }
                         });
                     });
                 });
             });
         });   
    }
    public void HidePartUI()
    {
        ScreenMainText.gameObject.SetActive(false);
        ScreenTitleText.gameObject.SetActive(false);
        PolyFull.GetComponent<Image>().DOFade(0f, FadeDuration*2);
        StopAllCoroutines();
    }

    public void HideUI()
    {
        ScreenTitleText.gameObject.SetActive(false);
        ScreenMainText.gameObject.SetActive(false);
        StopAllCoroutines();
        PolyFull.GetComponent<Image>().DOFade(0f, FadeDuration).OnComplete(() =>
        {
            PolyMid.GetComponent<Image>().DOFade(0f, FadeDuration).OnComplete(() =>
            {
                PolyFade.GetComponent<Image>().DOFade(0f, FadeDuration).OnComplete(() =>
                {
                    Rotor.GetComponent<Image>().DOFade(0f, FadeDuration);
                    Anchor.GetComponent<Image>().DOFade(0f, FadeDuration).OnComplete(() =>
                    {
                        BackgroundShadow.GetComponent<Image>().DOFade(0f, FadeDuration);
                        Background.GetComponent<Image>().DOFade(0f, FadeDuration).OnComplete(() =>
                        {
                            Cvs.SetActive(false);
                            _isPlaying = false;
                        });
                    });
                });
            });
        });

    }

    private IEnumerator SwipeAnimation()
    {
        while (_isPlaying)
        {
            SwipeArrows.GetComponent<Image>().fillAmount = 0;
            SwipeArrows.GetComponent<Image>().DOFillAmount(1f, FadeDuration*4);
            yield return new WaitForSeconds(FadeDuration*4);
        }
    }

    public void UpdateRotor(float rate)
    {
        Rotor.GetComponent<Image>().fillAmount = rate;
    }

    private IEnumerator PressAnimation()
    {
        while (_isPlaying)
        {
            PressCircles.transform.localScale = Vector3.zero;
            PressCircles.transform.DOScale(1f, FadeDuration*4);
            yield return new WaitForSeconds(FadeDuration*4);
        }
    }
}
