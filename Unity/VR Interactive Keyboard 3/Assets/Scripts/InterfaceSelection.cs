using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceSelection : MonoBehaviour {

    public GameObject[] controlSurfaces;
    private int interfaceNumber = 0;


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void NextInterface()
    {

        InterfaceActive(false);//deactivate the old interface

        //increment interface
        if (interfaceNumber == controlSurfaces.Length -1)
        {
            interfaceNumber = 0;
        }
        else
        {       
            interfaceNumber++;
        }
        InterfaceActive(true);//activate the new interface
    }
    public void previousInterface()
    {
        InterfaceActive(false);//deactivate the old interface

        //decrement interface
        if (interfaceNumber == 0)
        {
            interfaceNumber = controlSurfaces.Length - 1;
        }
        else
        {
            interfaceNumber--;
        }

        InterfaceActive(true);//activate the new interface

    }
    private void InterfaceActive(bool active)
    {
        
        //deactivate the old interface
        if (controlSurfaces[interfaceNumber] != null)
        {
            controlSurfaces[interfaceNumber].SetActive(active);
        }
    }
}
