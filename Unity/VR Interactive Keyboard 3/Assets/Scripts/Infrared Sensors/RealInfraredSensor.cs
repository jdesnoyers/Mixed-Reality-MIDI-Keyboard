using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiJack;
using TMPro;

public class RealInfraredSensor : MonoBehaviour {

    [ColorUsage(true, true, 0f, 8f, 0.125f, 3f)] public Color pitchUpEmission = Color.blue;
    [ColorUsage(true, true, 0f, 8f, 0.125f, 3f)] public Color pitchNeutralEmission = Color.magenta;
    [ColorUsage(true, true, 0f, 8f, 0.125f, 3f)] public Color pitchDownEmission = Color.red;
    [ColorUsage(true, true, 0f, 8f, 0.125f, 3f)] public Color offEmission = Color.magenta;

    [SerializeField] private GameObject textOutput;

    private Material _material; //from Leap SimpleInteractionEmission
    private Color _targetColor; //from Leap SimpleInteractionEmission

    // Use this for initialization
    void Start () {

        _material = GetComponent<Renderer>().material;
	}

	
	// Update is called once per frame
	void Update () {

        if (MidiMaster.GetPitchBend() == 0) //if there is no pitch bend
        {
            _targetColor = offEmission;
            textOutput.GetComponent<TextMeshPro>().text = "";
        }
        else if (MidiMaster.GetPitchBend() > 0) //if there is positive pitch bend
        {
            _targetColor = Color.Lerp(pitchNeutralEmission, pitchUpEmission, MidiMaster.GetPitchBend());    //set colour based on pitch bend value
            textOutput.GetComponent<TextMeshPro>().text = MidiMaster.GetPitchBend().ToString("F3");     //display value
        }
        else  //if negative pitch bend
        {
            _targetColor = Color.Lerp(pitchNeutralEmission, pitchDownEmission, -MidiMaster.GetPitchBend());    //set colour based on pitch bend value
            textOutput.GetComponent<TextMeshPro>().text = MidiMaster.GetPitchBend().ToString("F3");     //display value
        }
        

        _material.SetColor("_EmissionColor", Color.Lerp(_material.GetColor("_EmissionColor"), _targetColor, 20F * Time.deltaTime)); //from Leap SimpleInteractionEmission

    }
}
