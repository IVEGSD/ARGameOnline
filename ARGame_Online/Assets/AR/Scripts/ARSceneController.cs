using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ARSceneController : MonoBehaviour
{

	[Header("Object Control")] 
	public GameObject Objects;
	public GameObject ControlCanvas;    
	
	// Update is called once per frame
	void Update ()
	{
	    // if marker is tracked, active all the objects to the world
		Objects.SetActive(ARObjectController.isTracked);		
		ControlCanvas.SetActive(ARObjectController.isTracked);
	}

}
