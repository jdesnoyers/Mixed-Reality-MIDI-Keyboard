using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeactivateHandText : MonoBehaviour {

    public float deactivateTime = 5f;
    private float countdownTimer;
    private string previousText;

	// Use this for initialization
	void Start () {
        gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        
        //if the text hasn't changed, start counting down
        if (!string.Equals(GetComponent<TextMeshPro>().text,previousText))
        {
            countdownTimer = Time.time;
        }

        //if countdown threshold reached, deactivate object
        if ((Time.time - countdownTimer >= deactivateTime))
        {
            gameObject.SetActive(false);

        }

        previousText = GetComponent<TextMeshPro>().text;


    }


    //start counting down when enabled
    private void OnEnable()
    {
        countdownTimer = Time.time;
    }
}
