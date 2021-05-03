using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour {

	void Awake()
    {
        DontDestroyOnLoad(this);
    }
    
    public GameObject GetGO()
    {
        return gameObject;
    }
}
