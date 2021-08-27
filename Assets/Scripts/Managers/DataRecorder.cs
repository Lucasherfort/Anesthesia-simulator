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

    public void SaveData(TimeSpan DurationAnesthesia, TimeSpan DurationBeforeFirstInsertion, TimeSpan DurationToCompleteMiddle, int NbInsertion, int NbTouchNerve, int NbTouchVein, int NbTouchArtery)
    {
        if(!Application.isEditor) 
        {
            string path = Application.dataPath;
            
            path = path.Substring(0, path.LastIndexOf('/'));
            path = path + "/RecordedData/";
            System.IO.Directory.CreateDirectory(path);
            path = path + "RecordedData_" + DateTime.Now.ToString("dd-MM-yy") + "_" + DateTime.Now.ToString("HH-mm-ss") + ".txt";


            string durationAnesthesia = "Date : " + DateTime.Now.ToString() + "\n\nDurée de l'anesthésie : " + string.Format("{0:D2}", DurationAnesthesia.Minutes) + ":" + string.Format("{0:D2}", DurationAnesthesia.Seconds);
            string durationBeforeFirstInsertion = "\n\nDurée avant la première insertion : " + string.Format("{0:D2}", DurationBeforeFirstInsertion.Minutes) + ":" + string.Format("{0:D2}", DurationBeforeFirstInsertion.Seconds);
            string durationToCompleteMiddle = "\n\nDurée pour compléter 50 %  de l'anesthésie: " + string.Format("{0:D2}", DurationToCompleteMiddle.Minutes) + ":" + string.Format("{0:D2}", DurationToCompleteMiddle.Seconds);
            string NbInsertionAiguille = "\n\nNb insertions aiguille : " + NbInsertion;
            string NbNerve = "\n\nNb nerfs touchés : " + NbTouchNerve;
            string NbVeine = "\n\nNb veines touchées : " + NbTouchVein;
            string NbArtère = "\n\nNb artères touchées : " + NbTouchArtery;


            string report = durationAnesthesia + durationBeforeFirstInsertion + durationToCompleteMiddle+ NbInsertionAiguille+ NbNerve+ NbVeine+ NbArtère;
          
            System.IO.File.WriteAllText(path, report);            
            
        }
    }
}
