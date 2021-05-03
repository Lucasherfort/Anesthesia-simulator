using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TargetUI : MonoBehaviour {

    private GameObject Cvs;

    private GameObject MainRotor;
    private GameObject SecondRotor;

    public bool _debugUI;
    public bool _debugHideUI;

    private bool _UIShowed;

    private float _mainRotorSpeed = -0.1f;
    private float _secondRotorSpeed = 2f;
    // Use this for initialization
    void Awake()
    {
        Cvs = this.transform.GetChild(0).gameObject;
        MainRotor = Cvs.transform.Find("MainRotor").gameObject;
        SecondRotor = Cvs.transform.Find("SecondRotor").gameObject;

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
        if(_UIShowed)
        {
            RotateRotors();
        }

    }

    public void ShowUI()
    {
        this.gameObject.transform.up = Vector3.up;
        Cvs.SetActive(true);
        MainRotor.GetComponent<Image>().DOFade(1f, 0.2f).SetEase(Ease.InOutElastic).OnComplete(() => 
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
            MainRotor.GetComponent<Image>().DOFade(0f, 0.2f).OnComplete(() =>
            {
                Cvs.SetActive(false);
                _UIShowed = false;
            });
        });
    }

    void RotateRotors()
    {
        MainRotor.transform.Rotate(Vector3.forward * _mainRotorSpeed);
        SecondRotor.transform.Rotate(Vector3.forward * _secondRotorSpeed);
    }
}
