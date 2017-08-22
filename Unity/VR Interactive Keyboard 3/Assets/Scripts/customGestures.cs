using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Leap.Unity;
using Leap;

public class customGestures : MonoBehaviour {

	// Swipe (Left or Right) Gesture
	public bool swipeGesture;
	[ConditionalHide("swipeGesture", true)]
	public float handSwipeDistance = 0.5f;
	[ConditionalHide("swipeGesture", true)]
	public float handSwipeVelocity = 1.0f;
    [ConditionalHide("swipeGesture", true)]
    public string xOrZAxis = "x";
    
    public UnityEvent swipeRight;
    public UnityEvent swipeLeft;

    // Gun Gesture (Both Hands or One)
    public bool gunGesture;
	[ConditionalHide("gunGesture", true)]
	public GameObject fingerGlowObject;
	[ConditionalHide("gunGesture", true)]
	public Vector3 leftIndexDirection;
	[ConditionalHide("gunGesture", true)]
	public Vector3 rightIndexDirection;
	[ConditionalHide("gunGesture", true)]
	public Vector3 leftIndexPosition;
	[ConditionalHide("gunGesture", true)]
	public Vector3 rightIndexPosition;

	[Space(10)]

	// General Settings
	public float radiusSize = 0.01f;			// Colliders on the fingers and palm
	public float forwardFingerRange = -40f;		// Range your finger can be bent forwards
	public float backwardFingerRange = 20f;		// Range your finger can be bent backwards

	[Space(10)]

	// Hands
	public HandModel leftHand;
	public HandModel rightHand;
    public GameObject leftAttachmentHand;
    public GameObject rightAttachmentHand;



    [Space(10)]

	// Fingers and palms array
	private GameObject[] myFingers = new GameObject[12];

	// Fingers and palms
	public bool showFingers;

	[SerializeField]
	[ConditionalHide("showFingers", true)]
	private FingerModel leftThumb;
	[SerializeField]
	[ConditionalHide("showFingers", true)]
	private FingerModel leftIndex;
	[SerializeField]
	[ConditionalHide("showFingers", true)]
	private FingerModel leftMiddle;
	[SerializeField]
	[ConditionalHide("showFingers", true)]
	private FingerModel leftRing;
	[SerializeField]
	[ConditionalHide("showFingers", true)]
	private FingerModel leftPinkie;
	[SerializeField]
	[ConditionalHide("showFingers", true)]
	private FingerModel rightThumb;
	[SerializeField]
	[ConditionalHide("showFingers", true)]
	private FingerModel rightIndex;
	[SerializeField]
	[ConditionalHide("showFingers", true)]
	private FingerModel rightMiddle;
	[SerializeField]
	[ConditionalHide("showFingers", true)]
	private FingerModel rightRing;
	[SerializeField]
	[ConditionalHide("showFingers", true)]
	private FingerModel rightPinkie;
	[SerializeField]
	[ConditionalHide("showFingers", true)]
	private GameObject leftPalm;
	[SerializeField]
	[ConditionalHide("showFingers", true)]
	private GameObject rightPalm;

	// Palm Position tracking variables
	public bool showPalmPositions = false;

	[SerializeField]
	[ConditionalHide("showPalmPositions", true)]
	private float palmStartL = 0f;
	[SerializeField]
	[ConditionalHide("showPalmPositions", true)]
	private float palmCurrL = 0f;
	[SerializeField]
	[ConditionalHide("showPalmPositions", true)]
	private float palmPrevL = 0f;
	[SerializeField]
	[ConditionalHide("showPalmPositions", true)]
	private float palmStartR = 0f;
	[SerializeField]
	[ConditionalHide("showPalmPositions", true)]
	private float palmCurrR = 0f;
	[SerializeField]
	[ConditionalHide("showPalmPositions", true)]
	private float palmPrevR = 0f;


    // X and Z Velocity Averaging variables
    private float avgVelTotal = 0f;
	private int countVel = 0;

    // States of left and right hands (null or Active)
    private string stateL = null;
	private string stateR = null;

	// Gun Finger glow left and right hand
	private bool glowingL = false;
	private bool glowingR = false;
    private GameObject glowObjectL = null;
    private GameObject glowObjectR = null;

    //Gun Trigger State (null, Primed, Fired)
    private string triggerStateL = null;
	private string triggerStateR = null;
    private float timeStartL = 0f;
    private float timeCurrentL = 0f;
    private float timeStartR = 0f;
    private float timeCurrentR = 0f;

    // Use this for initialization
    void Start () {

		// Init left fingers
		leftThumb = leftHand.fingers[0];
		leftIndex = leftHand.fingers [1];
		leftMiddle = leftHand.fingers [2];
		leftRing = leftHand.fingers [3];
		leftPinkie = leftHand.fingers [4];

		// Init right fingers
		rightThumb = rightHand.fingers[0];
		rightIndex = rightHand.fingers [1];
		rightMiddle = rightHand.fingers [2];
		rightRing = rightHand.fingers [3];
		rightPinkie = rightHand.fingers [4];

		// Init palms
		leftPalm = leftHand.palm.gameObject;
		rightPalm = rightHand.palm.gameObject;

        myFingers[0] = leftAttachmentHand.transform.Find("Palm").GetChild(0).gameObject;
        myFingers[1] = leftAttachmentHand.transform.Find("ThumbTip").GetChild(0).gameObject;
        myFingers[2] = leftAttachmentHand.transform.Find("IndexTip").GetChild(0).gameObject;
        myFingers[3] = leftAttachmentHand.transform.Find("MiddleTip").GetChild(0).gameObject;
        myFingers[4] = leftAttachmentHand.transform.Find("RingTip").GetChild(0).gameObject;
        myFingers[5] = leftAttachmentHand.transform.Find("PinkyTip").GetChild(0).gameObject;
        myFingers[6] = rightAttachmentHand.transform.Find("Palm").GetChild(0).gameObject;
        myFingers[7] = rightAttachmentHand.transform.Find("ThumbTip").GetChild(0).gameObject;
        myFingers[8] = rightAttachmentHand.transform.Find("IndexTip").GetChild(0).gameObject;
        myFingers[9] = rightAttachmentHand.transform.Find("MiddleTip").GetChild(0).gameObject;
        myFingers[10] = rightAttachmentHand.transform.Find("RingTip").GetChild(0).gameObject;
        myFingers[11] = rightAttachmentHand.transform.Find("PinkyTip").GetChild(0).gameObject;

        /*
        // Init array of fingers and palms for ease of access
        myFingers [0] = leftMiddle.bones[1].gameObject;
        myFingers [1] = leftThumb.bones[2].gameObject;
		myFingers [2] = leftIndex.bones[3].gameObject;
        myFingers [3] = leftMiddle.bones[3].gameObject;
        myFingers [4] = leftRing.bones[3].gameObject;
        myFingers [5] = leftPinkie.bones[3].gameObject;
        myFingers [6] = rightMiddle.bones[1].gameObject;
        myFingers [7] = rightThumb.bones[2].gameObject;
        myFingers [8] = rightIndex.bones[3].gameObject;
        myFingers [9] = rightMiddle.bones[3].gameObject;
        myFingers [10] = rightRing.bones[3].gameObject;
		myFingers [11] = rightPinkie.bones[3].gameObject;

        // Place colliders on the array of fingers and palms
        for (int i = 0; i < myFingers.Length; i++) {

			// Init to have a sphere collider and set the specified radius size
			if (myFingers[i] != null && !myFingers[i].GetComponent<SphereCollider> ()) {

				myFingers[i].AddComponent<SphereCollider> ();
				myFingers[i].GetComponent<SphereCollider> ().radius = radiusSize;

			}

		}*/

        // Store Left and right hand original color
        if (!leftIndex.gameObject.GetComponent<MeshRenderer>()) {
			leftIndex.gameObject.AddComponent<MeshRenderer>();
		}

		if (!rightIndex.gameObject.GetComponent<MeshRenderer>()) {
			rightIndex.gameObject.AddComponent<MeshRenderer>();
		}
        

	} // END OF START

	// Update is called once per frame
	void Update () {

		if (swipeGesture == true) {

			SwipeGesture ();

		}else if (gunGesture == true) {

			GunGesture ();

		}

	} // END OF UPDATE

    void OnTriggerEnter(Collider collider){

		//Loop through fingers and palms
		for (int i = 0; i < myFingers.Length; i++) {

			// If colliding object is a finger or palm
			if (collider.gameObject.GetInstanceID () == myFingers [i].GetInstanceID ()) {

				//If Swipe gesture enabled
				if (swipeGesture == true) {

					// Left Hand
					if (i < 6) {

						//All fingers except thumb extended
						if (IsExtended (leftIndex) && IsExtended (leftMiddle) && IsExtended (leftRing) && IsExtended (leftPinkie)) {

							stateL = "Active";

                            if (xOrZAxis == "x")
                            {

                                palmStartL = transform.InverseTransformPoint(myFingers[0].transform.position).x;

                            }
                            else
                            {

                                palmStartL = transform.InverseTransformPoint(myFingers[0].transform.position).z;

                            }

						}

					} 
					// Right hand
					else if (i >= 6 && i < 12) {

						//All fingers except thumb extended
						if (IsExtended (rightIndex) && IsExtended (rightMiddle) && IsExtended (rightRing) && IsExtended (rightPinkie)) {

							stateR = "Active";

                            if (xOrZAxis == "x")
                            {

                                palmStartR = transform.InverseTransformPoint(myFingers[6].transform.position).x;

                            }
                            else
                            {

                                palmStartR = transform.InverseTransformPoint(myFingers[6].transform.position).z;

                            }
						}

					}

				} 

				//If Gun Gesture enabled
				if (gunGesture == true) {

					// Left hand
					if (i < 6) {

						stateL = "Active";

					}

					// Right hand
					if (i >= 6 && i < 12) {

						stateR = "Active";

					}

				}



			}//ADD MORE GESTURES WITH ELSE IF HERE

		}

	} // END OF ON TRIGGER ENTER

	void OnTriggerExit(Collider collider){

		// Any finger leaving the object area will reset the state to null
		for (int i = 0; i < myFingers.Length; i++) {

			// If colliding object is a finger or palm
			if (collider.gameObject.GetInstanceID () == myFingers[i].GetInstanceID ()) {

				// Reset Left hand if active
				if(stateL == "Active" && i < 6){

					palmStartL = 0;
					palmCurrL = 0;
					palmPrevL = 0;
					stateL = null;

				}

				// Reset Right hand if active
				if (stateR == "Active" && i >= 6){

					palmStartR = 0;
					palmCurrR = 0;
					palmPrevR = 0;
					stateR = null;

				}

				// Reset average velocity regardless
				avgVelTotal = 0;
				countVel = 0;

			}

		}

	} // END OF ON TRIGGER EXIT


	void SwipeGesture(){

        string tempSwipe = null;

        if (xOrZAxis == "x")
        {

            tempSwipe = HasSwipedX();

        }
        else
        {

            tempSwipe = HasSwipedZ();

        }

		// LEFT SWIPED
		if (tempSwipe == "left") {

			if (stateL == "Active") {

                swipeLeft.Invoke();

                Debug.Log ("Swiped Left with Left Hand");		// OPTIONAL DEBUG

				// Reset values
				stateL = null;
				avgVelTotal = 0;
				countVel = 0;

			} else if(stateR == "Active") {

                swipeLeft.Invoke();

				Debug.Log ("Swiped Left with Right Hand");		// OPTIONAL DEBUG

				// Reset values
				stateR = null;
				avgVelTotal = 0;
				countVel = 0;

			}

		}

		// RIGHT SWIPED
		else if (tempSwipe == "right") {


			if (stateR == "Active") {

                swipeRight.Invoke();

                Debug.Log ("Swiped Right with Right Hand");	// OPTIONAL DEBUG

				// Reset values
				stateR = null;
				avgVelTotal = 0;
				countVel = 0;

			} else {

                swipeRight.Invoke();

                Debug.Log ("Swiped Right with Left Hand");	// OPTIONAL DEBUG

				// Reset values
				stateL = null;
				avgVelTotal = 0;
				countVel = 0;

			}

		}

	}

	void GunGesture(){

		// LEFT ACTIVE
		if (stateL == "Active") {


			// Index extended and all other fingers not extended
			if (IsExtended(leftIndex) && !IsExtended(leftMiddle) && !IsExtended(leftRing) && !IsExtended(leftPinkie))
			{

				// PLACE WANTED LEFT SWIPE FUNCTIONALITY HERE //

				Debug.DrawRay(leftIndex.GetRay().origin,leftIndex.GetRay().direction, Color.green);
				leftIndexDirection = leftIndex.GetRay().direction;
				leftIndexPosition = leftIndex.GetRay().origin;

				// Index Begin Finger Glow
				if (glowingL == false) {
					
				    glowObjectL = Instantiate (fingerGlowObject, leftIndex.GetTipPosition (), fingerGlowObject.transform.rotation);
					glowingL = true;

					Debug.Log("Gun Left");

                } else if(glowingL == true && triggerStateL != "fired"){

					// Glow on so update position to finger tip
                    glowObjectL.transform.position = leftIndex.GetTipPosition();

                }

				// Check if Thumb Trigger Primed
				if ((((leftThumb.bones[2].localEulerAngles.z - 360f) > forwardFingerRange + 20f) || (leftThumb.bones[2].localEulerAngles.z < backwardFingerRange)) && triggerStateL == null) {

					triggerStateL = "primed";

                }

                if (!(((leftThumb.bones[2].localEulerAngles.z - 360f) > forwardFingerRange + 20f) || (leftThumb.bones[2].localEulerAngles.z < backwardFingerRange)) && triggerStateL == "primed" && timeStartL == 0)
                {

                    triggerStateL = "fired";

                    // ADD THUMB TRIGGER FUNCTIONALITY HERE //

                    timeStartL = Time.time;
                    glowObjectL.GetComponent<Rigidbody>().AddForce(leftIndex.GetRay().direction * 2, ForceMode.Impulse);

                    // END OF TRIGGER FUNCTIONALITY //

                    triggerStateL = null;
                    glowingL = false;

                }

                if (Time.time - timeStartL > 2f)
                {
                    
                    //Destroy(glowObjectL);

                }

                // END OF FUNCTIONALITY //

            } 
			// If Finger not in Gun Gesture any more
			else {


                glowingL = false;
                timeStartL = 0f;
                triggerStateL = null;
                Destroy(glowObjectL);

            }


        } 
		// If Hand not in object any more
		else {

            glowingL = false;
            timeStartL = 0f;
            triggerStateL = null;
            Destroy(glowObjectL);

        }

		// RIGHT ACTIVE
		if(stateR == "Active"){
	
			// Index extended and all other fingers not extended
			if (IsExtended(rightIndex) && !IsExtended(rightMiddle) && !IsExtended(rightRing) && !IsExtended(rightPinkie))
			{

				// PLACE WANTED RIGHT SWIPE FUNCTIONALITY HERE //

				Debug.DrawRay(rightIndex.GetRay().origin,rightIndex.GetRay().direction, Color.green);
				rightIndexDirection = rightIndex.GetRay().direction;
				rightIndexPosition = rightIndex.GetRay().origin;

				//Index Begin Finger Glow
                if (glowingR == false) {
					
					glowObjectR = Instantiate (fingerGlowObject, rightIndex.GetTipPosition (), fingerGlowObject.transform.rotation);
                    glowingR = true;

					Debug.Log ("Gun Right");

                } else if(glowingR == true && triggerStateR != "fired"){ 

					//Glow on so update position to finger tip
                    glowObjectR.transform.position = rightIndex.GetTipPosition();

                }

				// Check if Thumb Trigger Primed
				if ((((rightThumb.bones[2].localEulerAngles.z - 360f) > forwardFingerRange + 20f) || (rightThumb.bones[2].localEulerAngles.z < backwardFingerRange)) && triggerStateR == null) {

					triggerStateR = "primed";

                }

                if (!(((rightThumb.bones[2].localEulerAngles.z - 360f) > forwardFingerRange + 20f) || (rightThumb.bones[2].localEulerAngles.z < backwardFingerRange)) && triggerStateR == "primed" && timeStartR == 0f) {

					triggerStateR = "fired";

                    // ADD THUMB TRIGGER FUNCTIONALITY HERE //

                    //timeStartR = Time.time;
                    Destroy(glowObjectR);

                    GameObject shooter = Instantiate(fingerGlowObject, rightIndex.GetTipPosition(), fingerGlowObject.transform.rotation);
                    shooter.GetComponent<Rigidbody>().AddForce(rightIndex.GetRay().direction * 1,ForceMode.Impulse);

                    glowingR = false;
                    triggerStateR = null;

                    // END OF TRIGGER FUNCTIONALITY //

                }

                if (Time.time - timeStartR > 2f)
                {
                    //Destroy(glowObjectR);

                }

				// END OF FUNCTIONALITY //

            }
			// If Finger not in Gun Gesture any more
            else
            {

                glowingR = false;
                timeStartR = 0f;
                triggerStateR = null;
                Destroy(glowObjectR);

            }

        }
		// If Hand not in object any more
        else
        {

            glowingR = false;
            timeStartR = 0f;
            triggerStateR = null;
            Destroy(glowObjectR);

        }

	} // END OF GUN GESTURE

    bool IsExtended(FingerModel finger){

		// Is Finger not past forward and backward range
		if (((finger.bones[1].localEulerAngles.z - 360f) > forwardFingerRange) || (finger.bones[1].localEulerAngles.z < backwardFingerRange) ||
			((finger.bones[2].localEulerAngles.z - 360f) > forwardFingerRange) || (finger.bones[2].localEulerAngles.z < backwardFingerRange)) {

			return true;

		} else {

			return false;

		}

	} // END OF IS EXTENDED

	float AverageVelocity(float currVel, float prevVel, float avgVelTotal, float countVel){

		// Average += (Curr - Prev) / Time
		avgVelTotal += (currVel - prevVel) / Time.deltaTime;
		// Divide Average by the count
		countVel++;

		return (avgVelTotal / countVel);

	} // END OF AVERAGE VELOCITY

    string HasSwipedX()
    {

        // Left Hand
        if (stateL == "Active")
        {

            // Current Palm Position
            palmCurrL = transform.InverseTransformPoint(leftPalm.transform.position).x;

            float distL = palmCurrL - palmStartL;

            // Palm Distance Travelled
            if (distL >= handSwipeDistance)
            {

                float avgVelL = AverageVelocity(palmCurrL, palmPrevL, avgVelTotal, countVel);

                // Palm Average Velocity
                if (avgVelL >= handSwipeVelocity)
                {

                    //Debug.Log(Vector3.Dot(leftHand.GetPalmNormal(), transform.forward));
                    if (Vector3.Dot(leftHand.GetPalmNormal(), transform.right) > 0.9 || Vector3.Dot(leftHand.GetPalmNormal(), transform.right) < -0.9)
                    {

                        // Previous Palm Position
                        palmPrevL = transform.InverseTransformPoint(leftPalm.transform.position).x;

                        return "right";

                    }

                }

                // Previous Palm Position
                palmPrevL = transform.InverseTransformPoint(leftPalm.transform.position).x;

            }
            else if (distL <= -handSwipeDistance)
            {

                float avgVelL = AverageVelocity(palmCurrL, palmPrevL, avgVelTotal, countVel);

                // Palm Average Velocity
                if (avgVelL <= -handSwipeVelocity)
                {

                    if (Vector3.Dot(leftHand.GetPalmNormal(), transform.right) < -0.9 || Vector3.Dot(leftHand.GetPalmNormal(), transform.right) > 0.9)
                    {

                        // Previous Palm Position
                        palmPrevL = transform.InverseTransformPoint(leftPalm.transform.position).x;

                        return "left";
                    }

                }

                // Previous Palm Position
                palmPrevL = transform.InverseTransformPoint(leftPalm.transform.position).x;

            }

        }
        // Right Hand
        else if (stateR == "Active")
        {

            // Current Palm Position
            palmCurrR = transform.InverseTransformPoint(rightPalm.transform.position).x;

            float distR = palmCurrR - palmStartR;

            // Palm Distance Travelled
            if (distR <= -handSwipeDistance)
            {

                float avgVelR = AverageVelocity(palmCurrR, palmPrevR, avgVelTotal, countVel);

                // Palm Average Velocity
                if (avgVelR <= -handSwipeVelocity)
                {

                    if (Vector3.Dot(rightHand.GetPalmNormal(), transform.right) > 0.9 || Vector3.Dot(rightHand.GetPalmNormal(), transform.right) < -0.9)
                    {

                        // Previous Palm Position
                        palmPrevR = transform.InverseTransformPoint(rightPalm.transform.position).x;

                        return "left";
                    }

                }

                // Previous Palm Position
                palmPrevR = transform.InverseTransformPoint(rightPalm.transform.position).x;

            }
            else if (distR >= handSwipeDistance)
            {

                float avgVelR = AverageVelocity(palmCurrR, palmPrevR, avgVelTotal, countVel);

                // Palm Average Velocity
                if (avgVelR >= handSwipeVelocity)
                {

                    if (Vector3.Dot(rightHand.GetPalmNormal(), transform.right) > 0.9 || Vector3.Dot(rightHand.GetPalmNormal(), transform.right) < -0.9)
                    {

                        // Previous Palm Position
                        palmPrevR = transform.InverseTransformPoint(rightPalm.transform.position).x;

                        return "right";
                    }

                    // Previous Palm Position
                    palmPrevR = transform.InverseTransformPoint(rightPalm.transform.position).x;


                }

            }

        }

        return null;

	} // END OF HAND SWIPED X

    string HasSwipedZ()
    {

        // Left Hand
        if (stateL == "Active")
        {

            // Current Palm Position
            palmCurrL = transform.InverseTransformPoint(leftPalm.transform.position).z;

            float distL = palmCurrL - palmStartL;

            // Palm Distance Travelled
            if (distL >= handSwipeDistance)
            {

                float avgVelLZ= AverageVelocity(palmCurrL, palmPrevL, avgVelTotal, countVel);

                // Palm Average Velocity
                if (avgVelLZ >= handSwipeVelocity)
                {

                    if (Vector3.Dot(leftHand.GetPalmNormal(), transform.forward) > 0.9 || Vector3.Dot(leftHand.GetPalmNormal(), transform.forward) < -0.9)
                    {

                        // Previous Palm Position
                        palmPrevL = transform.InverseTransformPoint(leftPalm.transform.position).z;

                        return "right";

                    }

                }

                // Previous Palm Position
                palmPrevL = transform.InverseTransformPoint(leftPalm.transform.position).z;

            }
            else if (distL <= -handSwipeDistance)
            {

                float avgVelL = AverageVelocity(palmCurrL, palmPrevL, avgVelTotal, countVel);

                // Palm Average Velocity
                if (avgVelL <= -handSwipeVelocity)
                {

                    if (Vector3.Dot(leftHand.GetPalmNormal(), transform.forward) < -0.9 || Vector3.Dot(leftHand.GetPalmNormal(), transform.forward) > 0.9)
                    {

                        // Previous Palm Position
                        palmPrevL = transform.InverseTransformPoint(leftPalm.transform.position).z;

                        return "left";
                    }

                }

                // Previous Palm Position
                palmPrevL = transform.InverseTransformPoint(leftPalm.transform.position).z;

            }

        }
        // Right Hand
        else if (stateR == "Active")
        {

            // Current Palm Position
            palmCurrR = transform.InverseTransformPoint(rightPalm.transform.position).z;

            float distR = palmCurrR - palmStartR;

            // Palm Distance Travelled
            if (distR <= -handSwipeDistance)
            {

                float avgVelR = AverageVelocity(palmCurrR, palmPrevR, avgVelTotal, countVel);

                // Palm Average Velocity
                if (avgVelR <= -handSwipeVelocity)
                {

                    if (Vector3.Dot(rightHand.GetPalmNormal(), transform.forward) > 0.9 || Vector3.Dot(rightHand.GetPalmNormal(), transform.forward) < -0.9)
                    {

                        // Previous Palm Position
                        palmPrevR = transform.InverseTransformPoint(rightPalm.transform.position).z;

                        return "left";

                    }

                }

                // Previous Palm Position
                palmPrevR = transform.InverseTransformPoint(rightPalm.transform.position).z;

            }
            else if (distR >= handSwipeDistance)
            {

                float avgVelR = AverageVelocity(palmCurrR, palmPrevR, avgVelTotal, countVel);

                // Palm Average Velocity
                if (avgVelR >= handSwipeVelocity)
                {

                    if (Vector3.Dot(rightHand.GetPalmNormal(), transform.forward) > 0.9 || Vector3.Dot(rightHand.GetPalmNormal(), transform.forward) < -0.9)
                    {

                        // Previous Palm Position
                        palmPrevR = transform.InverseTransformPoint(rightPalm.transform.position).z;

                        return "right";

                    }

                    // Previous Palm Position
                    palmPrevR = transform.InverseTransformPoint(rightPalm.transform.position).z;


                }

            }

        }

        return null;

	} // END OF HAND SWIPED Z

}
