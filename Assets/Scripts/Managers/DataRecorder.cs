using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataRecorder : MonoBehaviour
{
    private void Start()
    {
        if(!Application.isEditor) 
        {
            string path = Application.dataPath;
            
            path = path.Substring(0, path.LastIndexOf('/'));
            path = path + "/RecordedData/";
            System.IO.Directory.CreateDirectory(path);
            path = path + "RecordedData_" + System.DateTime.Now.ToString("dd-MM-yy") + "_" + System.DateTime.Now.ToString("HH-mm-ss") + ".txt";
            
            string report = "Date : "+System.DateTime.Now.ToString()+"\n\nDurée de l'anesthésie : 00:00\n\nNb nerfs touchés : 0\n\nNb veines touchéees : 0\n\nNb artères touchéees : 0";
          
            System.IO.File.WriteAllText(path, report);            
            
        }
    }
}
