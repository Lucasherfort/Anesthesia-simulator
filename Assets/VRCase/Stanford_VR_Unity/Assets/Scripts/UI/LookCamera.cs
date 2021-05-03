using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookCamera : MonoBehaviour {

    public float speed = 2f;
   
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(transform.position - Camera.main.transform.position), Time.deltaTime * speed);
    }
}
