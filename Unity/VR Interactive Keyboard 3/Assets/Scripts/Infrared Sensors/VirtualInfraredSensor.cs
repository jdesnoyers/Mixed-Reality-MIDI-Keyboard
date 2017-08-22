using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Interaction;
using TMPro;

public class VirtualInfraredSensor : MonoBehaviour {


    [SerializeField] private InteractionBehaviour vIRVolume;    //volume for contact detection
    [SerializeField] private InteractionBehaviour vIRPad;       //surface for distance measurement
    [SerializeField] private GameObject textOutput;             //where to display value
    public float glowOutput = 0f;       //output value

    [ColorUsage(true,true,0f,8f,0.125f,3f)] public Color contactEmission = Color.white; //maximum value colour
    public Color noContactEmission = Color.black;       //minimum value colour
    
    private Material _material; //from Leap SimpleInteractionEmission
    private Color _targetColor; //from Leap SimpleInteractionEmission

    private bool volumeContact;

    // Use this for initialization
    void Start () {

        //subscribe to leap motion interaction contact contact
        vIRVolume.OnContactBegin += VolumeContactBegin;
        vIRVolume.OnContactEnd += VolumeContactEnd;

        var renderer = vIRVolume.GetComponent<Renderer>();
        if (renderer != null)
        {
            _material = renderer.material;
        }

        textOutput.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        
        if (volumeContact == true)  //if the hand is in the volume
        {
            if (vIRPad.isHovered)   //and hovering, set target colour to value based on the hand's distance from the pad
            {
                glowOutput = vIRPad.closestHoveringControllerDistance.Map(0f, 0.5f, 0f, 1f);
                _targetColor = Color.Lerp(contactEmission, noContactEmission, glowOutput);
            }
            if(textOutput.activeInHierarchy)
            {
                textOutput.GetComponent<TextMeshPro>().text = glowOutput.ToString("F3");
            }
        }
        else
        {
            _targetColor = noContactEmission;
        }
        
        _material.SetColor("_EmissionColor", Color.Lerp(_material.GetColor("_EmissionColor"), _targetColor, 20F * Time.deltaTime)); //from Leap SimpleInteractionEmission, set the colour


    }
    
    private void VolumeContactBegin()
    {
        volumeContact = true;
        textOutput.SetActive(true);
    }

    private void VolumeContactEnd()
    {
        volumeContact = false;
        textOutput.SetActive(false);
    }

    private void OnDrawGizmos() //debuging gizmos
    {
        if(vIRVolume.isPrimaryHovered)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(vIRVolume.closestHoveringController.hoverPoint, .02f);
        }

    }

}
