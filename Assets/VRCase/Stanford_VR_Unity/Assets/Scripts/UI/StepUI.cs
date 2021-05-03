using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StepUI : MonoBehaviour
{

    private GameObject Cvs;
    //show up Scale
    private GameObject ScaleApparition; //UI grey flash

    //1st_step 
    private GameObject Border; //Show border, scale by inside
    public float BorderFinalScale = 1;

    //2nd step  -> Show white background
    private GameObject Background;

    //3rd step ->
    private GameObject Step;


    //4th step -> Display text
    private Text mainText;
    private string maintextvalue;
    private Text stepText;
    public string steptextvalue;
    private Text stepNumber;
    private string stepnumbervalue;


    //Final step -> Display Step

    public float ScaleInitValue;
    public float ScaleFinalValue = 0.9f;

    private Color BackgroundColor = Color.white;

    private bool _UIShowed = false;
    public bool _debugUI = false;
    public bool _debugHideUI = false;
    // Use this for initialization

    private float timer;
    private float maxUIDuration = 20f;
    void Awake()
    {
        Cvs = this.transform.GetChild(0).gameObject;
        Background = Cvs.transform.Find("Background").gameObject;
        Step = Cvs.transform.Find("Step").gameObject;
        Border = Cvs.transform.Find("Border").gameObject;
        stepText = Cvs.transform.Find("Steptext").gameObject.GetComponent<Text>();
        mainText = Cvs.transform.Find("Maintext").gameObject.GetComponent<Text>();
        stepNumber = Cvs.transform.Find("Numbertext").gameObject.GetComponent<Text>();

        //HideUI();
    }

    void Start()
    {
        ShowUI();
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
            timer += Time.deltaTime;
            if(timer>=maxUIDuration)
            {
                timer = 0;
                HideUI();
            }
        }
    }


    public void ShowUI()
    {
        if (Cvs != null)
        {
            timer = 0;
            Cvs.SetActive(true);
            Border.GetComponent<Image>().enabled = true;
            Border.transform.localScale = Vector3.zero;
            Border.GetComponent<Image>().DOFade(1f, 0.8f);
            Border.transform.DOScale(BorderFinalScale, 0.4f).SetEase(Ease.InExpo).OnComplete(() =>
            {
                Background.GetComponent<Image>().enabled = true;
                Background.GetComponent<Image>().DOColor(BackgroundColor, 0.2f).SetEase(Ease.InOutElastic).OnComplete(() =>
                {
                    Background.GetComponent<Image>().DOFade(0f, 0.05f).OnComplete(() =>
                    {
                        Background.GetComponent<Image>().DOColor(BackgroundColor, 0.05f).SetEase(Ease.InOutElastic);
                    });
                    mainText.DOText(maintextvalue, 0.5f, true, ScrambleMode.None).OnComplete(() =>
                    {
                        Step.transform.localScale = Vector3.zero;
                        Step.GetComponent<Image>().enabled = true;
                        Step.transform.DOScale(1, 0.5f).SetEase(Ease.OutCubic).OnComplete(()=>
                        {
                            stepNumber.DOText(stepnumbervalue, 0.5f, true, ScrambleMode.None);
                            stepText.DOText(steptextvalue, 0.5f, true, ScrambleMode.None);
                            _UIShowed = true;
                        });
                    });
                });
            });
        }
    }


    public void HideUI()
    {
        stepNumber.DOText(string.Empty, 0.5f, true, ScrambleMode.None);
        mainText.DOText(string.Empty, 0.5f, true, ScrambleMode.None);
        stepText.DOText(string.Empty, 0.5f, true, ScrambleMode.None);

        //Border.transform.DOScale(Vector3.zero, 0.6f).SetEase(Ease.InExpo);
        Border.GetComponent<Image>().DOFade(0f, 0.8f).OnComplete(() => 
        {
            Border.GetComponent<Image>().enabled = false;
        });

        Background.GetComponent<Image>().DOFade(0f, 0.8f).OnComplete(() =>
        {
            Step.GetComponent<Image>().enabled = false;
            Background.GetComponent<Image>().enabled = false;
            Cvs.SetActive(false);
        });

        _UIShowed = false;
    }


    public void UpdateNumberValue(int value)
    {
        stepnumbervalue = value.ToString();
    }

    public void UpdateTextValue(string value)
    {
        maintextvalue = value;
    }
}