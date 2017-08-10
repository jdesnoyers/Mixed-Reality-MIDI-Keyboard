using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCombiner : MonoBehaviour {

    
    // Use this for initialization
    void Start () {
        
        StaticBatchingUtility.Combine(gameObject);

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
