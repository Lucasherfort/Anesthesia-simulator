using SmartLocalization;
using SmartLocalization.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicExample : MonoBehaviour {

    [System.Serializable]
    public class SampleButton
    {
        public string ButtonNameKey;
        public string actions;
    }

    
    public Transform ButtonsParent;
    public Button FRButton;
    public Button ENButton;
    public List<SampleButton> Buttons;
    private LanguageManager languageManager;
    // Use this for initialization
	void Start () {
        CreateButtons();
        languageManager = LanguageManager.Instance;
      
        FRButton.onClick.AddListener(SetLanguageToFrench);
        ENButton.onClick.AddListener(SetLanguageToEnglish);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void CreateButtons()
    {
        if(Buttons!=null && Buttons.Count != 0)
        {
            foreach(SampleButton newSampleButton in Buttons)
            {
                GameObject ButtonGO = Instantiate(Resources.Load("Button")) as GameObject;
                ButtonGO.transform.SetParent(ButtonsParent);
                ButtonGO.GetComponentInChildren<LocalizedText>().localizedKey = newSampleButton.ButtonNameKey;
                //ButtonGO.GetComponent<ButtonManager>().actions = newSampleButton.actions //Dynamic Logic Example
            }
        }
    }

    public void SetLanguageToFrench()
    {
        languageManager.ChangeLanguage("fr");
    }


    public void SetLanguageToEnglish()
    {
        languageManager.ChangeLanguage("en");
    }

    void OnDestroy()
    {
        FRButton.onClick.RemoveListener(SetLanguageToFrench);
        ENButton.onClick.RemoveListener(SetLanguageToEnglish);
    }
}
