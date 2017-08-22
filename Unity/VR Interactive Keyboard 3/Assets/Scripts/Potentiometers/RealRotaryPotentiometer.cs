using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Leap.Unity;
using MidiJack;
using TMPro;

public class RealRotaryPotentiometer : MonoBehaviour {

    [Tooltip("Convert floats to midi code out of 127")]
    public bool displayAsMidiCode = true;
    [Tooltip("set as encoder (true) or potentiometer (false)")]
    public bool encoderMode = false;
    [ConditionalHide("encoderMode", true, true)] [Tooltip("MIDI CC Number")]
    public int midiKnobNumber = 74;
    [ConditionalHide("encoderMode", true, true)] [Tooltip("Maximum Rotation in Degrees")]
    public float maxRotation = 300f;
    [ConditionalHide("encoderMode", true, true)] [Tooltip("MIDI CC Name")]
    public string midiKnobName = "Cutoff";

    [Tooltip("how long to wait until UI contracts")]
    public float uiContractDelay = 1f;
    [Tooltip("how long for UI to contract")]
    public float uiContractTime = 0.5f;
    [Tooltip("how long for UI to expand")]
    public float uiExpandTime = 0.1f;


    [SerializeField] private GameObject uiCanvas;
    [SerializeField] private GameObject rightTextDisplay;
    [SerializeField] private GameObject leftTextDisplay;
    [SerializeField] private GameObject localTextDisplay;
    [SerializeField] private float scaleMaxSet = 2f;
    private Vector3 scaleMin;
    private Vector3 scaleMax;
    private Image uiImage;

    private int midiEncoderSetting = 0; //which value the encoder is currently setting
    private int midiEncoderOffset = 72; //the first encoder MIDI CC number
    
    private float[] midiEncoderValue = new float[8];    //stores the midi encoder variables
    private string[] midiEncoderName = new string[8] {"Release","Attack","","Decay","V rate","V depth","V delay","Misc"};    //stores the midi encoder names

    // time trackers for UI expand/contract
    private float activateTime;
    private float deactivateTime;
    private float delayTime;

    //booleans for UI expand/contract
    private bool activate = false;
    private bool delay = false;
    private bool deactivate =false;

    // Use this for initialization
    void Start () {

        uiImage = uiCanvas.transform.GetChild(0).GetComponent<Image>(); //find image in canvas

        MidiMaster.knobDelegate += PotKnobHandler;  //subscribe to knob delegate

        //set minimum scale based on actual scale, maximum based on multiplier
        scaleMin = uiCanvas.transform.localScale;
        scaleMax = scaleMin * scaleMaxSet;

        //display name
        if (encoderMode)
        {
            displayValue(midiEncoderName[0], 0, false);

        }
        else
        {
            displayValue(midiKnobName, 0, false);
        }
    }
	
	// Update is called once per frame
	void Update () {

        if (activate == true)   //if the knob has recently been activated, enlarge the display
        {
            uiCanvas.transform.localScale = Vector3.Lerp(scaleMin, scaleMax, (Time.time - activateTime)/uiExpandTime);

            if (Time.time - activateTime >= uiExpandTime)   //once expanded, move to delay phase
            {
                activate = false;
                delay = true;
                delayTime = Time.time;
                uiCanvas.transform.localScale = scaleMax;
            }
        }
        else if((delay == true) && (Time.time - delayTime >= uiContractDelay))   //if the knob has stopped turning, and the delay has passed, move to the deactivate phase
        {
            deactivate = true;
            delay = false;
            deactivateTime = Time.time;
        }
        else if(deactivate == true)     //contract the display back to it's original size
        {
            uiCanvas.transform.localScale = Vector3.Lerp(scaleMax, scaleMin, (Time.time - deactivateTime) / uiContractTime);

            if (Time.time - deactivateTime >= uiContractTime)   //once contracted, finish
            {
                deactivate = false;
                uiCanvas.transform.localScale = scaleMin;
            }
        }
	}
    
    void PotKnobHandler(MidiChannel channel, int knob, float value)
    {
        if(encoderMode == false)    //if it's a potentiometer
        {

            if (knob == midiKnobNumber) //if the incoming knob value matches this knob
            {
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 360 - value * maxRotation, transform.localEulerAngles.z);    //rotate the knob
                uiImage.fillAmount = value*maxRotation/360; //fill the UI
                displayValue(midiKnobName, value);  //display the value

                if ((activate == false) && (delay == false))    //if it was just activated, and the previous delay has elapsed, expand the image
                {
                    activateTime = Time.time;
                    uiCanvas.transform.localScale = scaleMin;
                    activate = true;
                    deactivate = false;
                }
                else if (delay == true)     //if in delay mode, keep delaying every time the knob is adjusted
                {
                    delayTime = Time.time;
                }

            }
        }
        else  //must be an encoder
        {
            if(knob==114)   //if it's the encoder button, switch the anticipated encoder input knob
            {
                midiEncoderSetting = (int)(value * 128);    //set the encoder knob between 0-7
                uiImage.fillAmount = midiEncoderValue[midiEncoderSetting];  //update the ui to the current knob
                displayValue(midiEncoderName[midiEncoderSetting], midiEncoderValue[midiEncoderSetting]);    //update the text to the current knob
            }
            else if(knob == midiEncoderOffset+midiEncoderSetting)   //if the encoder is sending MIDI data
            {
                //rotate the knob 15 degrees for every change in the input value (we want to match the encoder's actual position, which is different from the value captured)
                if (value > midiEncoderValue[midiEncoderSetting])
                {
                    transform.Rotate(0, -15, 0);
                }
                else if (value < midiEncoderValue[midiEncoderSetting])
                {
                    transform.Rotate(0, 15, 0);
                }

                midiEncoderValue[midiEncoderSetting] = value;   //save the value for later
                uiImage.fillAmount = value; //fill the UI
                displayValue(midiEncoderName[midiEncoderSetting],value);  //display the value

                if ((activate == false) && (delay == false))    //if it was just activated, and the previous delay has elapsed, expand the image
                {
                    activateTime = Time.time;
                    uiCanvas.transform.localScale = scaleMin;
                    activate = true;
                    deactivate = false;
                }
                else if (delay == true)     //if in delay mode, keep delaying every time the knob is adjusted
                {
                    delayTime = Time.time;
                }
            }
            
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
                if (rightTextDisplay != null)
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
