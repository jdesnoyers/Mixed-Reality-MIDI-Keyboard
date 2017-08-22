using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AngularVelocityOutput : MonoBehaviour {

    //the outputs for each axis
    public GameObject xAxisOutput;
    public GameObject yAxisOutput;
    public GameObject zAxisOutput;

    [HideInInspector] public Vector3 angularVelOut; //the value to display

    [SerializeField] private GameObject textOutput; //the text object to display it on

    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {

        //change scale of outputs based on angular velocity of input object
        xAxisOutput.transform.localScale = new Vector3(1,(Mathf.Abs(GetComponent<Rigidbody>().angularVelocity.x)/10)+0.1f,1);
        yAxisOutput.transform.localScale = new Vector3(1, (Mathf.Abs(GetComponent<Rigidbody>().angularVelocity.y) / 10) + 0.1f, 1);
        zAxisOutput.transform.localScale = new Vector3(1, (Mathf.Abs(GetComponent<Rigidbody>().angularVelocity.z) / 10) + 0.1f, 1);

        angularVelOut = GetComponent<Rigidbody>().angularVelocity;
        textOutput.GetComponent<TextMeshPro>().text = string.Concat("x: ",angularVelOut.x.ToString("F3"), "\n", "y: ", angularVelOut.y.ToString("F3"), "\n", "z: ", angularVelOut.z.ToString("F3"));
            


    }
}
