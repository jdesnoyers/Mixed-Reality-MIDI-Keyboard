using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Interaction;

[RequireComponent(typeof(InteractionBehaviour))]


public class Virtual2DSlider : MonoBehaviour {

    public float proximityDistance;         // Trigger distance from finger to object
    public float sliderLength = 0.1f;       // Length of Slider
    public float sliderStartPos = 0f;       // start postion of slider (from 0 to 1)

    [SerializeField] private GameObject sliderBase;
    private InteractionBehaviour intObj;    // Interaction Behaviour required to access Leap Motion hand distance tracking
    private string state = null;            // Active or null (Is the button currently being held down?)


    private float _sliderOutput;   //outputs position as float between 0 and 1
    public float SliderOutput
    {
        get{return _sliderOutput; }
    }

    void Start () {

        intObj = GetComponent<InteractionBehaviour>();  //capture related interaction behaviour
        transform.localPosition = new Vector3(transform.localPosition.x, sliderLength * sliderStartPos, transform.localPosition.z); //move slider to specified start point
        _sliderOutput = sliderStartPos;

        //adjust slider base to match input size and range
        sliderBase.transform.localScale = new Vector3(sliderBase.transform.localScale.x, sliderLength, sliderBase.transform.localScale.z);
        sliderBase.transform.localPosition = new Vector3(sliderBase.transform.localPosition.x, sliderLength/2, sliderBase.transform.localPosition.z);


    }

    // Update is called once per frame
    void Update () {

        // If fingers are within hovering distance of the object
        if (intObj.isPrimaryHovered)
        {

            // If your fingers are within the objects proximity and the button isn't already being held down
            if (intObj.primaryHoverDistance <= proximityDistance)
            {

                if(state == null)
                    state = "Active";

                if (transform.localPosition.y <= sliderLength && transform.localPosition.y <= 0)
                {
                    transform.localPosition = new Vector3(transform.localPosition.x, transform.parent.transform.InverseTransformPoint(intObj.primaryHoveringControllerPoint).y, transform.localPosition.z); //follow finger position
                    _sliderOutput = transform.localPosition.y / sliderLength;    //capture output value
                }



            }
            // If fingers have left the object's proximity
            else if (state == "Active" && intObj.primaryHoverDistance > proximityDistance)
            {

                state = null;
                

            }

        }

    }
}
