using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingBall : MonoBehaviour {

    public GameObject lightingBallOutput;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        lightingBallOutput.transform.localRotation = GetComponent<Transform>().localRotation;
        lightingBallOutput.transform.localPosition = (10f*(GetComponent<Transform>().localPosition + new Vector3(3.5f,3.5f,3.5f))/3f)+ new Vector3(-5,0,-5);
    }
}
