using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using MidiJack;
using TMPro;

public class AbstractionLayer : MonoBehaviour {

    public bool displayAsMidiCode;
    [SerializeField] private GameObject abstractionKnob;
    [SerializeField] private GameObject uiCanvas;
    [SerializeField] private GameObject leftTextDisplay;
    [SerializeField] private GameObject rightTextDisplay;
    [SerializeField] private GameObject localTextDisplay;
    [SerializeField] private float abstractionMax = 100;
    public float maxBrightness = 2.0f;
    private Vector3 abstractionStart;
    private Color knobMinColour;
    private Color knobMaxColour;
    private Image uiImage;


    // Use this for initialization
    void Start () {

        abstractionStart = abstractionKnob.transform.localPosition;

        uiImage = uiCanvas.transform.GetChild(0).GetComponent<Image>(); //find image in canvas

        float hue; float sat; float val;
        knobMinColour = abstractionKnob.GetComponent<Renderer>().material.GetColor("_EmissionColor");
        Color.RGBToHSV(knobMinColour,out hue,out sat,out val);
        knobMaxColour = Color.HSVToRGB(hue, sat, maxBrightness);

        MidiMaster.knobDelegate += SliderKnobHandler;
        displayValue("Room Scale", 0, false);    //set value output

    }

    // Update is called once per frame
    void Update () {

    }

    void SliderKnobHandler(MidiChannel channel, int knob, float value)
    {
        switch (knob)
        {
            case 20:    //if the knob is the abstraction knob
                {
                    abstractionKnob.transform.localPosition = new Vector3(abstractionStart.x + (value * abstractionMax), abstractionStart.y, abstractionStart.z);   //set position

                    abstractionKnob.GetComponent<Renderer>().material.SetColor("_EmissionColor",Color.Lerp(knobMinColour,knobMaxColour,value)); //set colour
                    uiImage.fillAmount = value; //fill the UI
                    displayValue("Room Scale", value);    //set value output
                }
                break;
        }
    }

    void displayValue(string input, float value, bool global = true)
    {
        //scale float to midi value out of 127
        if (displayAsMidiCode == true)
        {
            value = (int)(value * 127);
        }

        localTextDisplay.GetComponent<TextMeshPro>().text = string.Concat(input, ": ", value);  //display text locally

        //if global text is requested, activate and display on the closest hand
        if (global)
        {
            if (Vector3.SqrMagnitude(leftTextDisplay.transform.position - transform.position) > Vector3.SqrMagnitude(rightTextDisplay.transform.position - transform.position))
            {
                if(rightTextDisplay != null)
                {
                    rightTextDisplay.SetActive(true);
                    rightTextDisplay.GetComponent<TextMeshPro>().text = string.Concat(input, ": ", value);
                }

            }
            else
            {
                if (leftTextDisplay != null)
                {
                    leftTextDisplay.SetActive(true);
                    leftTextDisplay.GetComponent<TextMeshPro>().text = string.Concat(input, ": ", value);
                }
            }
        }
    }

}



