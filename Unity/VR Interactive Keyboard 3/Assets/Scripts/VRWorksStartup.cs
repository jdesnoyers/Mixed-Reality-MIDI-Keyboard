using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NVIDIA;

[RequireComponent(typeof(VRWorks))]

public class VRWorksStartup : MonoBehaviour {
    

	// Use this for initialization
	void Start () {


        VRWorks _vrWorks = GetComponent<VRWorks>();

        if (_vrWorks.IsFeatureAvailable(VRWorks.Feature.LensMatchedShading))
        {
            _vrWorks.SetActiveFeature(VRWorks.Feature.LensMatchedShading);
        }
        else if (_vrWorks.IsFeatureAvailable(VRWorks.Feature.MultiResolution))
        {
            _vrWorks.SetActiveFeature(VRWorks.Feature.MultiResolution);
        }

        Debug.Log(_vrWorks.GetActiveFeature());

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
