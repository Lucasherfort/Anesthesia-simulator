using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TooltipUI : MonoBehaviour {

    private GameObject Cvs;
    //show up Scale
    private GameObject ScaleApparition; //UI White flash

    //1st_step 
    private GameObject Border; //Scale by inside
    public float BorderFinalScale = 1;

    //2nd step  -> Show Shadow and purple background
    private GameObject Background;

    //3rd step -> Display text
    private Text tooltipText;
    public string tooltipvalue;

    //Final step -> Display Link
    private GameObject Link;
    private GameObject Anchor;
    private GameObject Rotor;

    public float ScaleInitValue;
    public float ScaleFinalValue = 0.9f;

    private Color BackgroundColor = Color.white;

    private bool _UIShowed = false;
    public bool _debugUI = false;
    public bool _debugHideUI = false;

    private float _mainRotorSpeed = -1f;
    private float _secondRotorSpeed = 2f;
    // Use this for initialization

    void Awake()
    {
        Cvs = this.transform.GetChild(0).gameObject;
        ScaleApparition = Cvs.transform.Find("Flash").gameObject;
        Background = Cvs.transform.Find("Background").gameObject;
        Border = Cvs.transform.Find("Border").gameObject;
        Link = Cvs.transform.Find("Link").gameObject;
        Anchor = Cvs.transform.Find("Anchor").gameObject;
        Rotor = Cvs.transform.Find("Rotor").gameObject;
        tooltipText = Cvs.transform.Find("Text").gameObject.GetComponent<Text>();

        //BackgroundColor = Background.GetComponent<Image>().color;

        //HideUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (_debugUI)
        {
            _debugUI = false;
            ShowUI();
        }

        if (_debugHideUI)
        {
            _debugHideUI = false;
            HideUI();
        }

        if(_UIShowed)
        {
            RotateUI();
        }
    }


    public void ShowUI()
    {
        if (Cvs != null)
        {
            Cvs.SetActive(true);
            ScaleApparition.GetComponent<Image>().enabled = true;
            ScaleApparition.transform.DOScale(ScaleFinalValue, 0.7f).SetEase(Ease.OutElastic).OnComplete(() =>
            {
                ScaleApparition.GetComponent<Image>().DOFade(0f, 0.1f).OnComplete(() =>
                {
                    ScaleApparition.transform.localScale = new Vector3(ScaleInitValue, ScaleInitValue, ScaleInitValue);
                    ScaleApparition.GetComponent<Image>().enabled = false;


                    Border.GetComponent<Image>().enabled = true;
                    Border.transform.localScale = Vector3.zero;
                    Border.transform.DOScale(BorderFinalScale, 0.4f).SetEase(Ease.InExpo).OnComplete(() =>
                    {
                        Background.GetComponent<Image>().enabled = true;
                        Background.GetComponent<Image>().DOColor(BackgroundColor, 0.2f).SetEase(Ease.InOutElastic).OnComplete(() =>
                        {
                            Background.GetComponent<Image>().DOFade(0f, 0.05f).OnComplete(() =>
                            {
                                Background.GetComponent<Image>().DOColor(BackgroundColor, 0.05f).SetEase(Ease.InOutElastic);
                            });
                            tooltipText.GetComponent<Text>().DOText(tooltipvalue, 0.5f, true, ScrambleMode.None).OnComplete(() =>
                            {
                                Link.transform.localScale = Vector3.zero;
                                Link.GetComponent<Image>().enabled = true;

                                Anchor.GetComponent<Image>().enabled = true;
                                Anchor.GetComponent<Image>().DOFade(1f, 0.3f);
                                Rotor.GetComponent<Image>().enabled = true;
                                Rotor.GetComponent<Image>().DOFade(1f, 0.3f);
                                Link.transform.DOScale(1, 0.5f).SetEase(Ease.OutCubic);
                            });
                        });
                    });
                });

                _UIShowed = true;

            });
        }

    }


    public void HideUI()
    {
        Border.transform.DOScale(Vector3.zero, 0.6f).SetEase(Ease.InExpo);
        Border.GetComponent<Image>().DOFade(0f, 0.8f).OnComplete(() => { Border.GetComponent<Image>().enabled = false; });
        Background.GetComponent<Image>().DOFade(0f, 0.8f).OnComplete(() => {
            Background.GetComponent<Image>().enabled = false;
            Cvs.SetActive(false);
        });

        _UIShowed = false;

    }

    public void RotateUI()
    {
        Anchor.transform.Rotate(Vector3.forward * _mainRotorSpeed);
        Rotor.transform.Rotate(Vector3.forward * _secondRotorSpeed);
    }
}
