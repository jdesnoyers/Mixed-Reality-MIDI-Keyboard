using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Interaction;

public class VirtualRotaryPotentiometer : MonoBehaviour {

    [SerializeField] private float potStart = 0;    //potentiometer starting point
    [SerializeField] private GameObject handPointer;

    private float rotaryScale;
    private float potAngleMin;
    private float potAngleRange;
    private InteractionBehaviour rotaryPotBehaviour;

    private float _rotaryOutput = 0;    //outputs rotation as float between 0 and 1
    public float RotaryOutput
    {
        get { return _rotaryOutput; }
    }
    
    // Use this for initialization
    void Start () {

        potAngleMin = GetComponent<HingeJoint>().limits.min;  //capture minimum angle
        float potAngleMax = GetComponent<HingeJoint>().limits.max;  //capture minimum angle
        potAngleRange = potAngleMax - potAngleMin;                  //get the range from the attached hinge joint

        //set potentiometer to default value
        float potAngleStart = potStart * (potAngleRange) + potAngleMin;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, potAngleStart, transform.localEulerAngles.z);

        rotaryPotBehaviour = GetComponent<InteractionBehaviour>();

        //subscribe to leap motion interactions to show and hide pointer
        rotaryPotBehaviour.OnGraspBegin += ShowHandPointer;
        rotaryPotBehaviour.OnGraspEnd += HideHandPointer;
        
        _rotaryOutput = (potAngleStart - potAngleMin )/ (potAngleRange);
        
    }
	
	// Update is called once per frame
	void Update () {

        if (rotaryPotBehaviour.isGrasped)   //if the object is being grasped, show the indicator pointing to it
        {
            handPointer.transform.position = Vector3.Lerp(transform.position, rotaryPotBehaviour.graspingController.primaryHoveringPoint, 0.5f);
            handPointer.transform.LookAt(rotaryPotBehaviour.graspingController.primaryHoveringPoint);
            handPointer.transform.localScale = new Vector3(handPointer.transform.localScale.x, handPointer.transform.localScale.y, Vector3.Magnitude(rotaryPotBehaviour.graspingController.primaryHoveringPoint - transform.position)/2);

        }

        _rotaryOutput = (transform.localRotation.x - potAngleMin) / potAngleRange;    //need to correct to capture true rotation
    }

    void ShowHandPointer()
    {
        handPointer.SetActive(true);
        handPointer.transform.localScale = new Vector3(handPointer.transform.localScale.x, handPointer.transform.localScale.y, 0.005f);
        handPointer.transform.position = new Vector3(0, 0, 0);
    }

    void HideHandPointer()
    {
        handPointer.SetActive(false);
        handPointer.transform.localScale = new Vector3(handPointer.transform.localScale.x, handPointer.transform.localScale.y, 0.005f);
        handPointer.transform.position = new Vector3(0, 0, 0);
    }
}
