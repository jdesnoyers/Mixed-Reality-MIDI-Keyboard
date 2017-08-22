using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Interaction;

public class objectProximity : MonoBehaviour {

    public float proximityDistance = 0.01f;         // Trigger distance from finger to object
	public float currDistance;

    private InteractionBehaviour intObj;    // Interaction Behaviour required to access Leap Motion hand distance tracking
    private string state = null;            // Active or null (Is the button currently being held down?)

    void Start(){

        // Get the Interaction Behaviour component from your object
        intObj = GetComponent<InteractionBehaviour>();

    }

	void Update(){
		
        // If fingers are within hovering distance of the object
        if (intObj.isPrimaryHovered)
        {
			currDistance = intObj.primaryHoverDistance;
            // If your fingers are within the objects proximity and the button isn't already being held down
            if (intObj.primaryHoverDistance < proximityDistance && state == null)
            {

                state = "Active";

                // Call Button Pressed Function
                buttonPress bPScr = gameObject.GetComponent<buttonPress>();
                bPScr.ButtonPressed();

            }
            // If fingers have left the objects proximity
            else if(state == "Active" && intObj.primaryHoverDistance > proximityDistance)
            {

                state = null;

                // Call Button UnPressed Function
                buttonPress bPScr = gameObject.GetComponent<buttonPress>();
                bPScr.ButtonUnPressed();

            }

        }

    }

}
