using UnityEngine;
using System;

public class DataRecorder : MonoBehaviour
{
    static public DataRecorder Instance{get; private set;}

    private void Awake()
    {
        if(Instance)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void SaveData(TimeSpan DurationAnesthesia, int NbTouchNerve, int NbTouchVein, int NbTouchArtery)
    {
        if(!Application.isEditor) 
        {
            string path = Application.dataPath;
            
            path = path.Substring(0, path.LastIndexOf('/'));
            path = path + "/RecordedData/";
            System.IO.Directory.CreateDirectory(path);
            path = path + "RecordedData_" + System.DateTime.Now.ToString("dd-MM-yy") + "_" + System.DateTime.Now.ToString("HH-mm-ss") + ".txt";

            string minute = string.Format("{0:D2}", DurationAnesthesia.Minutes);
            string seconds = string.Format("{0:D2}", DurationAnesthesia.Seconds);

            string report = "Date : "+System.DateTime.Now.ToString()+"\n\nDurée de l'anesthésie : "+minute+":"+seconds+"\n\nNb nerfs touchés : "+NbTouchNerve+"\n\nNb veines touchées : "+NbTouchVein+"\n\nNb artères touchées : "+NbTouchArtery;
          
            System.IO.File.WriteAllText(path, report);            
            
        }
    }
}
