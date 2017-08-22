using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiJack;
using Leap.Unity.Interaction;

public class RoomControls : MonoBehaviour {

    [Header("Inputs")]
    public GameObject wallColourControl;
    public GameObject keyboardEndControlH;
    public GameObject keyboardEndControlS;
    public GameObject keyboardEndControlV;
    public GameObject spotlightLumControl;
    public GameObject pointlightLumControl;

    [Space(10)]

    [Header("Outputs")]

    public GameObject roomScaleOutput;
    public GameObject staticRoom;
    public Material wallColourOutput;
    public Material keyboardEndOutput;
    public GameObject spotlight;
    public GameObject pointlight;

    [HideInInspector] public float roomScale;
    [HideInInspector] public Color wallColour;
    [HideInInspector] public Color keyboardEndColour;
    [HideInInspector] public float spotlightLum;
    [HideInInspector] public float pointlightLum;

    private float roomScaleSave;

    private bool roomActive;

    // Use this for initialization
    void Start () {


        MidiMaster.knobDelegate += RoomKnobHandler; //subscribe to MIDI input

        //save the defaults
        roomScaleSave = roomScaleOutput.transform.localScale.x;
        wallColour = wallColourOutput.color;
        keyboardEndColour = keyboardEndOutput.color;
        spotlightLum = spotlight.GetComponent<Light>().intensity;
        pointlightLum = pointlight.GetComponent<Light>().intensity;


    }
	
	// Update is called once per frame
	void Update () {

        if (roomActive)
        {
            //light intensity adjustments using slider pots
            spotlight.GetComponent<Light>().intensity = spotlightLumControl.GetComponent<InteractionSlider>().HorizontalSliderValue * 2.0f;
            pointlight.GetComponent<Light>().intensity = pointlightLumControl.GetComponent<InteractionSlider>().HorizontalSliderValue * 2.5f;
            

            roomScaleOutput.transform.localScale = new Vector3(.2f + roomScale, .2f + roomScale, .2f + roomScale); //scale the Room to the Abstraction Slider

            //set the colour based on the location of the sphere
            wallColourOutput.SetColor("_Color", Color.HSVToRGB(-wallColourControl.transform.localPosition.x / 4, -wallColourControl.transform.localPosition.y / 4, -wallColourControl.transform.localPosition.z / 4));

            //set the colour based on 3 sliders
            keyboardEndOutput.SetColor("_Color", Color.HSVToRGB(keyboardEndControlH.GetComponent<InteractionSlider>().HorizontalSliderValue, keyboardEndControlS.GetComponent<InteractionSlider>().HorizontalSliderValue, 2*keyboardEndControlV.GetComponent<InteractionSlider>().HorizontalSliderValue));
        }

    }

    void RoomKnobHandler(MidiChannel channel, int knob, float value)
    {
        switch (knob)
        {
            case 20:    //if the knob is the abstraction knob
                {
                    roomScale = value * 2;
                }
                break;
            case 116:   //if it's the activation button
                {
                    if(value == 1.0)
                    {
                        roomActive = true;
                        staticRoom.SetActive(false);
                        roomScaleOutput.SetActive(true);
                    }
                    else
                    {
                        roomActive = false;
                        ResetRoom();
                    }
                }
                break;
        }
    }

    private void OnApplicationQuit()
    {
        ResetRoom();
    }

    private void ResetRoom()
    {
        //get the defaults
        roomScaleOutput.transform.localScale = new Vector3(roomScaleSave, roomScaleSave, roomScaleSave);
        wallColourOutput.color = wallColour;
        keyboardEndOutput.color = keyboardEndColour;
        spotlight.GetComponent<Light>().intensity = spotlightLum;
        pointlight.GetComponent<Light>().intensity = pointlightLum;
        staticRoom.SetActive(true);
        roomScaleOutput.SetActive(false);
    }
}
