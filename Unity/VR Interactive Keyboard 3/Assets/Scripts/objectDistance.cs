using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

//IF MODIFYING CODE LOOK FOR COMMENTS ABOUT PLACING AN ELSE IF CONDITION. WILL BE IN ALL CAPS

public class objectDistance : MonoBehaviour {

    [Header("Sphere Collider")]
	public float collideeSize = 0.01f;                                  // Size of all Trigger Objects Sphere Collider
	[Header("Box Collider")]
	public Vector3 colliderSize = new Vector3(10f, 2.220446e-16f, 10f); // Size of Box Collider on main object
	public bool isHands = false;
	public bool isOther = false;
    [Space(10)]
    //If collidingObjects are fingers then make sure Elements 0-4 is the left hand and 5-9 is the right hand
    public GameObject[] collidingObjects;                               // Trigger Objects can be any object with almost any amount
    [Space(10)]
    public bool isTouching = false;

    private string state = null;                                        // Active or null (Is the button currently being held down?)

    // Use this for initialization
    void Start() {


		//Don't let several conditions be true
		if (isHands == true) {

			isOther = false;

		} else if (isOther == true) {

			isHands = false;

		}

		//Init Hand Colliders
		if (isHands == true) {

			// Loop all trigger objects
			for (int i = 0; i < collidingObjects.Length; i++) {

				// Initialize all trigger objects to have a sphere collider and set the specified size
				if (collidingObjects [i] != null && !collidingObjects [i].GetComponent<SphereCollider> ()) {
					collidingObjects [i].AddComponent<SphereCollider> ();
					collidingObjects [i].GetComponent<SphereCollider> ().radius = collideeSize;

				}

			}

		}

	}
	
	// Update is called once per frame
	void Update () {

	}

    void OnTriggerEnter(Collider collider)
    {
        
		//Detect fingers touching object
		if (isHands == true) {

			// If object not active currently
			if (state == null) {

				for (int i = 0; i < collidingObjects.Length; i++) {
				
					// If colliding object is the same trigger object
					if (collider.gameObject.GetInstanceID () == collidingObjects [i].GetInstanceID ()) {

						state = "Active";
                        isTouching = true;

						if (gameObject.name == "2DButton") {

							// Call Button Pressed Function
							buttonPress bPScr = gameObject.GetComponent<buttonPress> ();
							bPScr.ButtonPressed ();

						}//PLACE ELSE IF CONDITION(S) HERE TO ADD MORE FEATURES FOR DIFFERENT TYPES OF BUTTONS

					}

				}

			}

		}

    }

	void OnTriggerExit(Collider collider){

		//Detect fingers not touching the object anymore
		if (isHands == true) {

			//If object was active
			if (state == "Active") {

				for (int i = 0; i < collidingObjects.Length; i++) {

					// If colliding object is the sane trigger object
					if (collider.gameObject.GetInstanceID () == collidingObjects [i].GetInstanceID ()) {

						state = null;
                        isTouching = false;

						if (gameObject.name == "2DButton") {
							
							// Call Button UnPressed Function
							buttonPress bPScr = gameObject.GetComponent<buttonPress> ();
							bPScr.ButtonUnPressed ();

						}//PLACE ELSE IF CONDITION(S) HERE TO ADD MORE FEATURES FOR DIFFERENT TYPES OF BUTTONS

					}

				}

			}

		}

    }


}
