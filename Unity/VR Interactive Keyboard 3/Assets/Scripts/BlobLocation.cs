using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MidiJack;
using HTC.UnityPlugin.PoseTracker;


public class BlobLocation : MonoBehaviour {

    public GameObject leap_Hand_Controller;
    public GameObject offsetIRTracker;
    public GameObject vrInteractiveKeyboard;
    private bool blobState = false;
    private bool pauseState = false;

    public List<Vector3> blobCapture = new List<Vector3>();
    public List<ID_LineVector> lineCalculator = new List<ID_LineVector>();

    public int movingAverage = 10;
    private List<Vector3> localPositionAverager = new List<Vector3>();
    private List<Quaternion> localRotationAverager = new List<Quaternion>();
    private Vector3 localPositionAverage = new Vector3();
    private Quaternion localRotationAverage = new Quaternion();


    public UnityEvent trackerPauseOn;
    public UnityEvent trackerPauseOff;
    public UnityEvent blobToggleOn;
    public UnityEvent blobToggleOff;


    private Vector3 saveChildLocalPosition;
    private Vector3 saveChildLocalEulerAngles;
    private Vector3 saveLocalPosition;
    private Vector3 saveLocalEulerAngles;


    // Use this for initialization
    void Start () {
        MidiMaster.knobDelegate += TrackerKnobHandler;
    }

    // Update is called once per frame
    void Update () {


        

        if (blobState && !pauseState)   //if blob tracking is on
        {
            blobCapture = new List<Vector3>(leap_Hand_Controller.GetComponent<CameraProcessing>().Blobs);

            for (int i = 0; i < blobCapture.Count; i++)
            {

                if (Vector3.Distance(blobCapture[i], leap_Hand_Controller.GetComponent<Transform>().position) >= 1f)
                {
                    blobCapture.RemoveAt(i);    //remove any blobs too far from the controller
                }

            }


            //glossy keys giving false readings - need to improve viewing angle of IR LEDs before proceeding with 4 point recognition
            /*
            //if 4 blobs are found, remove the left one by finding the blob that is at least 500 mm from another blob and between 200 and 300 mm from some other blobs (known specifically of the left blob)
            if(blobCapture.Count == 4)
            {
                int[] findLeftBlob = new int[4];

                for (int i = 0; i < blobCapture.Count - 1; i++)
                {
                    for (int j = i + 1; j < blobCapture.Count; j++)
                    {
                        float sqrMagnitudeCalc = Vector3.SqrMagnitude(blobCapture[i] - blobCapture[j]);
                        if ((sqrMagnitudeCalc > 0.25f) || ((sqrMagnitudeCalc > .04f) && (sqrMagnitudeCalc < .09f)))
                        {
                            findLeftBlob[i]++;
                            findLeftBlob[j]++;
                        }
                    }
                }

                int leftBlobIndex = 0;
                int leftBlobMax = 0;

                for (int i = 0; i < 4; i++)
                {
                    if(findLeftBlob[i] > leftBlobMax)
                    {
                        leftBlobMax = findLeftBlob[i];
                        leftBlobIndex = i;
                    }
                }

                blobCapture.RemoveAt(leftBlobIndex);
               
            }*/

            if(blobCapture.Count == 3)  //ensure there are 3 blobs to work with
            {
                for (int i = 0; i < blobCapture.Count - 1; i++)
                {
                    for (int j = i + 1; j < blobCapture.Count; j++)
                    {
                        lineCalculator.Add(new ID_LineVector(i, j, blobCapture[i], blobCapture[j], Vector3.Magnitude(blobCapture[i] - blobCapture[j])));    //scalable combination of all possible lines
                    }
                }

                //sort from smallest to largest by magnitude
                lineCalculator.Sort(delegate (ID_LineVector x, ID_LineVector y)
                {
                    return x.Magnitude.CompareTo(y.Magnitude);
                });
                
                string triangleOrientation = null; //create triangle orientation (left or right) tracking variable

                //glossy keys giving false readings - need to improve viewing angle of IR LEDs before proceeding with 4 point recognition
                //determine which of 4 possible triangles is being calculated based on the longest side
                /*if (lineCalculator[lineCalculator.Count - 1].Magnitude > 0.5f)
                {
                    if (lineCalculator[lineCalculator.Count - 2].Magnitude > 0.41f)
                    {
                        triangleOrientation = "top";
                    }
                    else if (lineCalculator[lineCalculator.Count - 1].Magnitude > 0.35f)
                    {
                        triangleOrientation = "bottom";
                    }
                }
                else*/
                if ((lineCalculator[2].Magnitude < 0.5f) && (lineCalculator[2].Magnitude > 0.41f) && (lineCalculator[1].Magnitude > 0.38f) && (lineCalculator[0].Magnitude > 0.1f ))
                {
                    triangleOrientation = "right";
                }
                /*else if (lineCalculator[lineCalculator.Count - 1].Magnitude > 0.2f)
                {
                    triangleOrientation = "left";
                }*/
                else
                {
                    triangleOrientation = null;
                }

                if (triangleOrientation != null)
                {
                    GameObject transformCalc = new GameObject();

                    Debug.DrawLine(blobCapture[0], blobCapture[1], Color.green);
                    Debug.DrawLine(blobCapture[0], blobCapture[2], Color.blue);
                    Debug.DrawLine(lineCalculator[2].VectStart, lineCalculator[2].VectEnd, Color.magenta);

                    //calculate cross product of any coplanar lines to find normal vector
                    Vector3 side1 = (blobCapture[1] - blobCapture[0]) / Vector3.Magnitude(blobCapture[1] - blobCapture[0]);
                    Vector3 side2 = (blobCapture[2] - blobCapture[0]) / Vector3.Magnitude(blobCapture[1] - blobCapture[0]);
                    Vector3 upVect = Vector3.Cross(side1, side2);

                    //if normal vector is pointing relatively upwards, then use it to orient the keyboard vertically, otherwise invert the vector
                    if (upVect.y <= 0)
                        upVect = new Vector3(-upVect.x, -upVect.y, -upVect.z);

                    upVect.Normalize(); //normalize the vertical vector

                    Vector3 rightVect = new Vector3();
                    Vector3 forwardVect = new Vector3();


                    if (triangleOrientation == "right") //if it's the right triangle the position will be at one end of the shortest line, with the forward vector along the shortest line
                    {
                        if ((lineCalculator[1].IndexS == lineCalculator[0].IndexS) || (lineCalculator[1].IndexE == lineCalculator[0].IndexS)) //if starting point of shortest line touches mid-sized line
                        {
                            transformCalc.transform.position = lineCalculator[0].VectStart;
                            forwardVect = (lineCalculator[0].VectEnd - lineCalculator[0].VectStart) / lineCalculator[0].Magnitude;
                        }
                        else  //if end point of shortest line touches mid-sized line
                        {
                            transformCalc.transform.position = lineCalculator[0].VectEnd;
                            forwardVect = (lineCalculator[0].VectStart - lineCalculator[0].VectEnd) / lineCalculator[0].Magnitude;
                        }
                    }
                    //glossy keys giving false readings - need to improve viewing angle of IR LEDs before proceeding with 4 point recognition
                    /*else if (triangleOrientation == "left") //if it's the left triangle the position will be at one end of the shortest line, with the forward vector along the shortest line
                    {
                        if ((lineCalculator[1].IndexS == lineCalculator[0].IndexS) || (lineCalculator[1].IndexE == lineCalculator[0].IndexS)) //if starting point of shortest line touches mid-sized line
                        {
                            transformCalc.transform.position = lineCalculator[0].VectEnd;
                            forwardVect = (lineCalculator[0].VectStart - lineCalculator[0].VectEnd) / lineCalculator[0].Magnitude;
                        }
                        else  //if end point of shortest line touches mid-sized line
                        {
                            transformCalc.transform.position = lineCalculator[0].VectStart;
                            forwardVect = (lineCalculator[0].VectEnd - lineCalculator[0].VectStart) / lineCalculator[0].Magnitude;
                        }

                    }
                    else if (triangleOrientation == "bottom") //if it's the bottom triangle the position will be at one end of the mid-sized line, with the right vector along the mid-sized line
                    {
                        if ((lineCalculator[0].IndexS == lineCalculator[1].IndexS) || (lineCalculator[0].IndexE == lineCalculator[1].IndexS)) //if starting point of mid-sized line touches shortest line
                        {
                            transformCalc.transform.position = lineCalculator[1].VectStart;
                            rightVect = (lineCalculator[1].VectStart - lineCalculator[1].VectEnd) / lineCalculator[1].Magnitude;
                        }
                        else  //if end point of shortest line touches mid-sized line
                        {
                            transformCalc.transform.position = lineCalculator[1].VectEnd;
                            forwardVect = (lineCalculator[1].VectEnd - lineCalculator[0].VectStart) / lineCalculator[1].Magnitude;
                        }

                        forwardVect = Vector3.Cross(upVect, rightVect); //find the forward vector
                    }
                    else if (triangleOrientation == "top") //if it's the top triangle, the position will be offset from one end of the shortest line, with the right vector along the shortest line
                    {
                        if (lineCalculator[1].IndexS == lineCalculator[0].IndexS) //if starting point of shortest line touches start of mid-sized line
                        {
                            rightVect = (lineCalculator[0].VectEnd - lineCalculator[0].VectStart) / lineCalculator[0].Magnitude;
                            forwardVect = Vector3.Cross(upVect, rightVect); //find the forward vector
                            transformCalc.transform.position = lineCalculator[0].VectStart - (forwardVect*Vector3.Dot(lineCalculator[1].VectEnd - lineCalculator[1].VectStart,forwardVect));

                        }
                        if (lineCalculator[1].IndexE == lineCalculator[0].IndexS) //if starting point of shortest line touches end of mid-sized line
                        {
                            rightVect = (lineCalculator[0].VectEnd - lineCalculator[0].VectStart) / lineCalculator[0].Magnitude;
                            forwardVect = Vector3.Cross(upVect, rightVect); //find the forward vector
                            transformCalc.transform.position = lineCalculator[0].VectStart - (forwardVect * Vector3.Dot(lineCalculator[1].VectStart - lineCalculator[1].VectEnd, forwardVect));

                        }
                        if (lineCalculator[1].IndexS == lineCalculator[0].IndexE) //if ending point of shortest line touches start of mid-sized line
                        {
                            rightVect = (lineCalculator[0].VectStart - lineCalculator[0].VectEnd) / lineCalculator[0].Magnitude;
                            forwardVect = Vector3.Cross(upVect, rightVect); //find the forward vector
                            transformCalc.transform.position = lineCalculator[0].VectStart - (forwardVect * Vector3.Dot(lineCalculator[1].VectEnd - lineCalculator[1].VectStart, forwardVect));

                        }
                        else  //if end point of shortest line touches start of mid-sized line
                        {
                            rightVect = (lineCalculator[0].VectStart - lineCalculator[0].VectEnd) / lineCalculator[0].Magnitude;
                            forwardVect = Vector3.Cross(upVect, rightVect); //find the forward vector
                            transformCalc.transform.position = lineCalculator[0].VectStart - (forwardVect * Vector3.Dot(lineCalculator[1].VectStart - lineCalculator[1].VectEnd, forwardVect));
                        }
                    }*/
                    
                    transformCalc.transform.localRotation = Quaternion.LookRotation(upVect, forwardVect);


                    if (localPositionAverager.Count >= movingAverage)    //if the number of datapoints is equal to the moving average, remove the oldest
                    {
                        //remove oldest from average
                        localPositionAverage = Vector3.LerpUnclamped(localPositionAverager[0], localPositionAverage, ((float)localPositionAverager.Count) / (localPositionAverager.Count - 1));
                        //Vec3_RemoveFromAverage(localPositionAverager[0], localPositionAverage, localPositionAverager.Count);
                        localRotationAverage = Quaternion.LerpUnclamped(localRotationAverager[0], localRotationAverage, ((float)localRotationAverager.Count) / (localRotationAverager.Count - 1));

                        //remove oldest from list
                        localPositionAverager.RemoveAt(0);
                        localRotationAverager.RemoveAt(0);
                    }

                    //add the newest data point
                    localPositionAverager.Add(transformCalc.transform.localPosition);
                    localRotationAverager.Add(transformCalc.transform.localRotation);

                    //include newest point in average
                    localPositionAverage = Vector3.LerpUnclamped(localPositionAverager[localPositionAverager.Count - 1], localPositionAverage, ((float)localPositionAverager.Count - 1) / localPositionAverager.Count);
                    localRotationAverage = Quaternion.Lerp(localRotationAverager[localRotationAverager.Count - 1], localRotationAverage, ((float)localRotationAverager.Count - 1) / localRotationAverager.Count);

                    // move the keyboard transform to the calculated average
                    transform.localPosition = localPositionAverage;
                    transform.localRotation = localRotationAverage;

                    Destroy(transformCalc); //delete the transform calculating gameobject
                }
                lineCalculator.Clear();


            }


        }
        

    }

    public void PauseTracking(bool pause)   //pause all connected trackers
    {
        pauseState = pause;
        if(blobState)
        {
            leap_Hand_Controller.GetComponent<CameraProcessing>().enabled = !pause;
        }
    }

    void TrackerKnobHandler(MidiChannel channel, int knob, float value)
    {
        //IR position control mode
        switch (knob)
        {
            case 117:   //switch tracker case
                {
                    if ((value > 0) && !blobState) //when IR positioning is on
                    {

                        blobState = true;
                        blobToggleOn.Invoke();  //toggle external scripts

                        //save the current transform for when turned off
                        saveLocalPosition = transform.localPosition;
                        saveLocalEulerAngles = transform.localEulerAngles;
                        saveChildLocalPosition = vrInteractiveKeyboard.transform.localPosition;
                        saveChildLocalEulerAngles = vrInteractiveKeyboard.transform.localEulerAngles;

                        vrInteractiveKeyboard.transform.localPosition = offsetIRTracker.transform.localPosition;  //move Keyboard offset to align with IR tracking
                        vrInteractiveKeyboard.transform.localEulerAngles = offsetIRTracker.transform.localEulerAngles;
                        
                        //add startup alignment so keyboard doesn't jump

                        BlobTrackingAutoUnpause();

                    }
                    else if ((value == 0) && blobState)
                    {
                        blobState = false;

                        //return offset to Tracker settings
                        transform.localPosition = saveLocalPosition;
                        transform.localEulerAngles = saveLocalEulerAngles;
                        vrInteractiveKeyboard.transform.localPosition = saveChildLocalPosition;
                        vrInteractiveKeyboard.transform.localEulerAngles = saveChildLocalEulerAngles;

                        leap_Hand_Controller.GetComponent<CameraProcessing>().enabled = false;  //turn off camera processing (blob tracking)

                        blobToggleOff.Invoke();

                    }
                }
                break;

            case 115:   //pause tracker case
                {
                    if ((value == 0) && pauseState)
                    {

                        BlobTrackingAutoUnpause();


                    }
                    else if ((value > 0) && !pauseState)
                    {

                        pauseState = true;

                        trackerPauseOn.Invoke();

                        leap_Hand_Controller.GetComponent<CameraProcessing>().enabled = false;  //turn off camera processing (blob tracking)


                    }
                }
                break;

        }
        
    }

    void BlobTrackingAutoUnpause()
    {
        pauseState = false;
        trackerPauseOff.Invoke();
        leap_Hand_Controller.GetComponent<CameraProcessing>().enabled = blobState;  //activate camera processing (blob tracking) only if the keyboard is in IR location mode
    }

}

public class ID_LineVector  //class for sorting and analyzing lines
{
    public int IndexS {get; set;}           //start index
    public int IndexE { get; set; }         //end index
    public Vector3 VectStart {get; set;}    //start vector
    public Vector3 VectEnd { get; set; }    //end vector
    public float Magnitude { get; set; }    //magnitude


    public ID_LineVector(int indexS,int indexE, Vector3 vectStart, Vector3 vectEnd, float magnitude)    //constructor for LineVector object
    {
        IndexS = indexS;
        IndexE = indexE;
        VectStart = vectStart;
        VectEnd = vectEnd;
        Magnitude = magnitude;
    }
}
 