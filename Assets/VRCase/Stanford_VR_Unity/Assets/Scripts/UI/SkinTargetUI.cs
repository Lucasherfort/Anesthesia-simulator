using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SkinTargetUI : MonoBehaviour
{

    private GameObject Cvs;

    private GameObject Background;
    private GameObject SecondRotor;
    private Transform CurrentPos;

    public bool _debugUI;
    public bool _debugHideUI;

    private bool _UIShowed;

    private float timeMultiplier = 4f;
    private float _secondRotorSpeed = 1f;
    private float Delta = 0.08f;
    // Use this for initialization
    void Awake()
    {
        Cvs = this.transform.GetChild(0).gameObject;
        Background = Cvs.transform.Find("Background").gameObject;
        SecondRotor = Cvs.transform.Find("SecondRotor").gameObject;

        CurrentPos = SecondRotor.transform;
        
        //HideUI();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug
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

        //Rotation
        if (_UIShowed)
        {
            FloatingRotatingUI();
        }

    }

    public void ShowUI()
    {
        Cvs.SetActive(true);
        Background.GetComponent<Image>().DOFade(0.8f, 0.2f).SetEase(Ease.InOutElastic).OnComplete(() =>
        {
            SecondRotor.GetComponent<Image>().DOFade(1f, 0.2f).OnComplete(() =>
            {
                _UIShowed = true;
            });
        });
    }

    public void HideUI()
    {
        SecondRotor.GetComponent<Image>().DOFade(0f, 0.2f).OnComplete(() =>
        {
            Background.GetComponent<Image>().DOFade(0f, 0.2f).OnComplete(() =>
            {
                Cvs.SetActive(false);
                _UIShowed = false;
            });
        });
    }

    void FloatingRotatingUI()
    {
        SecondRotor.transform.Rotate(Vector3.forward * _secondRotorSpeed);
        SecondRotor.transform.localPosition = new Vector3(CurrentPos.localPosition.x, CurrentPos.localPosition.y,
                                                          CurrentPos.localPosition.z + Delta*Mathf.Sin(Time.time * timeMultiplier));
    }
}
