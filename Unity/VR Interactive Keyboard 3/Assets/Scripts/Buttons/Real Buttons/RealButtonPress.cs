using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MidiJack;

public class RealButtonPress : MonoBehaviour {

    [SerializeField] private bool button3D = true;
    [SerializeField] private string buttonOnText = "On";
    [SerializeField] private string buttonOffText = "Off";
    [SerializeField] [ColorUsage(true, true, 0f, 8f, 0.125f, 3f)]  private Color buttonOnColor = Color.green;
    [SerializeField] [ColorUsage(true, true, 0f, 8f, 0.125f, 3f)] private Color buttonOffColor = Color.red;
    [SerializeField] private GameObject textOutput;
    [SerializeField] private GameObject buttonBezzle;
    [SerializeField] private bool defaultOn = true;
    public bool directMidiInput = false;

    [ConditionalHide("directMidiInput", true)]
    public int midiKnobNumber;

    private bool animateButton =false;
    private float animateButtonTime;
    private float animationDuration = 0.2f;
    private Vector3 upPosition;
    private Vector3 downPosition;


    // Use this for initialization
    void Start () {

        ButtonOnOff(defaultOn); //set button state to default
        upPosition = transform.localPosition;
        downPosition = new Vector3(upPosition.x, upPosition.y + 0.002f, upPosition.z);

        //if it is accepting midi input directly, attach to the button knob handler
        if (directMidiInput)
            MidiMaster.knobDelegate += ButtonKnobHandler;

	}
	
	// Update is called once per frame
	void Update () {
		if(animateButton)   //animate the button down, then up, then reset
        {
            if (Time.time - animateButtonTime <= animationDuration)
            {
                transform.localPosition = Vector3.Lerp(upPosition, downPosition, (Time.time - animateButtonTime) / animationDuration);
            }
            else if (Time.time - animateButtonTime <= 2 * animationDuration)
            {
                transform.localPosition = Vector3.Lerp(downPosition, upPosition, (Time.time - animateButtonTime + animationDuration) / animationDuration);
            }
            else
            {
                transform.localPosition = upPosition;
                animateButton = false;
            }

        }
	}

    public void ButtonKnobHandler(MidiChannel channel, int knob, float value)
    {

        if(knob == midiKnobNumber)  //if the incoming knob value is this one
        {
            if(value == 1.0)
            {
                ButtonOnOff(true);    //activate button
                Debug.Log("button press");

            }
            else
            {
                ButtonOnOff(false);    //deactivate button
            }

        }
    }

    public void ButtonOnOff (bool buttonOn)
    {
        if (buttonBezzle != null)   //if a bezzle has been selected, change its colour, otherwise change the button's colour - Either way update corresponding text
        {
            if (buttonOn)
            {
                if(textOutput != null)
                    textOutput.GetComponent<TextMeshPro>().text = buttonOnText;

                buttonBezzle.GetComponent<Renderer>().material.SetColor("_EmissionColor", buttonOnColor);
            }
            else
            {
                if (textOutput != null)
                    textOutput.GetComponent<TextMeshPro>().text = buttonOffText;

                buttonBezzle.GetComponent<Renderer>().material.SetColor("_EmissionColor", buttonOffColor);
            }
        }
        else
        {
            if (buttonOn)
            {
                if (textOutput != null)
                    textOutput.GetComponent<TextMeshPro>().text = buttonOnText;

                GetComponent<Renderer>().material.SetColor("_EmissionColor", buttonOnColor);
            }
            else
            {
                if (textOutput != null)
                    textOutput.GetComponent<TextMeshPro>().text = buttonOffText;

                GetComponent<Renderer>().material.SetColor("_EmissionColor", buttonOffColor);
            }
        }

        //if it's a 3D button and animation hasn't started, start the animation
        if(button3D && !animateButton)
        {
            animateButton = true;
            animateButtonTime = Time.time;
        }
    }

    
}
