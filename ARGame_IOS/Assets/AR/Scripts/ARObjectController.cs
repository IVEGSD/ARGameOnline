using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ARObjectController : DefaultTrackableEventHandler
{
	public static bool isTracked = false;
	
	public override void OnTrackableStateChanged(
		TrackableBehaviour.Status previousStatus,
		TrackableBehaviour.Status newStatus)
	{
		if (newStatus == TrackableBehaviour.Status.DETECTED ||
		    newStatus == TrackableBehaviour.Status.TRACKED ||
		    newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
		{
			//active object
			isTracked = true;
			Debug.Log("isTracked: " + isTracked);
		}
		else if (previousStatus == TrackableBehaviour.Status.TRACKED &&
		         newStatus == TrackableBehaviour.Status.NO_POSE)
		{
			//inactive object
			isTracked = false;
			Debug.Log("isTracked: " + isTracked);
			OnTrackingLost();
		}
		else
		{
			//inactive object
			isTracked = false;
			Debug.Log("isTracked: " + isTracked);
			OnTrackingLost();
		}
	}

	public TrackableBehaviour.Status GetCurrentStatus()
	{
		return mTrackableBehaviour.CurrentStatus;
	}

}
