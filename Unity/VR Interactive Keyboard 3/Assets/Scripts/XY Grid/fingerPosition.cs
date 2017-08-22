using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

public class fingerPosition : MonoBehaviour {

	[Header("Sphere Collider")]
	public float collideeSize = 0.01f;                                  // Size of all Trigger Objects Sphere Collider

	//If collidingObjects are fingers then make sure Elements 0-4 is the left hand and 5-9 is the right hand
	public GameObject leftHand = null;
	public GameObject rightHand = null;

	public GameObject[] fingers = new GameObject[10];

	public string[] touchingFingers = new string[10];
	public Vector3[] touchPosition3 = new Vector3[10];
	public Vector2[] touchPosition2 = new Vector2[10];
	public Vector3 objectPosition;

	// Use this for initialization
	void Start() {

		objectPosition = transform.position;

		for(int i = 0; i < 10; i++){

			if(i < 5){

				fingers[i] = leftHand.transform.GetChild(i).gameObject;

			}else{

				fingers[i] = rightHand.transform.GetChild(i - 5).gameObject;

			}

		}

		// Loop all trigger objects
		for (int i = 0; i < 10; i++) {

			// Initialize all trigger objects to have a sphere collider and set the specified size
			if (fingers[i] != null && !fingers[i].GetComponent<SphereCollider>()) {
				fingers[i].AddComponent<SphereCollider> ();
				fingers[i].GetComponent<SphereCollider> ().radius = collideeSize;

			}

		}

		for(int i = 0; i < touchPosition3.Length; i++)
		{

			touchPosition3[i] = Vector3.zero;
			touchPosition2[i] = Vector2.zero;

		}

	}

	// Update is called once per frame
	void Update () {

		objectPosition = transform.position;

			for (int i = 0; i < touchPosition3.Length; i++) {

				if (touchPosition3[i] != Vector3.zero) {

					float x = fingers[i].transform.InverseTransformPoint (objectPosition).x;
					float y = fingers[i].transform.InverseTransformPoint (objectPosition).y;
					float z = fingers[i].transform.InverseTransformPoint (objectPosition).z;

					if (i < 5) {

						touchingFingers [i] = "Left" + fingers[i].name;

						touchPosition3 [i] = new Vector3 (x, y, z);
						touchPosition2 [i] = new Vector2 (x, y);

					} else {

						touchingFingers [i] = "Right" + fingers[i].name;

						touchPosition3 [i] = new Vector3 (x, y, z);
						touchPosition2 [i] = new Vector2 (x, y);

					}

				}

			}

	}

	void OnTriggerEnter(Collider collider)
	{
		for (int i = 0; i < touchingFingers.Length; i++)
			{

				// If colliding object is the same trigger object
				if (collider.gameObject.GetInstanceID() == fingers[i].GetInstanceID())
				{

					float x = fingers[i].transform.InverseTransformPoint(objectPosition).x;
					float y = fingers[i].transform.InverseTransformPoint(objectPosition).y;
					float z = fingers[i].transform.InverseTransformPoint(objectPosition).z;
					x *= 100f;
					x = Mathf.Round (x)/100f;
					y *= 100f;
					y = Mathf.Round (y)/100f;
					z *= 100f;
					z = Mathf.Round (z)/100f;

					if (i < 5)
					{

						touchingFingers[i] = "Left" + fingers[i].name;

						touchPosition3[i] = new Vector3(x, y, z);
						touchPosition2[i] = new Vector2(x, y);

					}
					else
					{

						touchingFingers[i] = "Right" + fingers[i].name;

						touchPosition3[i] = new Vector3(x, y, z);
						touchPosition2[i] = new Vector2(x, y);

					}

				}

			}

	}

	void OnTriggerExit(Collider collider){

			for (int i = 0; i < touchingFingers.Length; i++)
			{

				// If colliding object is the same trigger object
				if (collider.gameObject.GetInstanceID() == fingers[i].GetInstanceID())
				{

					if (i < 5)
					{

						touchingFingers[i] = null;
						touchPosition3[i] = Vector3.zero;
						touchPosition2[i] = Vector2.zero;

					}
					else
					{

						touchingFingers[i] = null;
						touchPosition3[i] = Vector3.zero;
						touchPosition2[i] = Vector2.zero;

					}

				}

			}

		}


}

