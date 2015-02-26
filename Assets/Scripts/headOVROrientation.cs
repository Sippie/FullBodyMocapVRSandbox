using UnityEngine;
using System.Collections;
using Ovr;

public class headOVROrientation : MonoBehaviour {


	public Transform inputOVR;
	public Transform headBone;



	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {


		float[] neckOffset = new float[]{Hmd.OVR_DEFAULT_NECK_TO_EYE_HORIZONTAL, Hmd.OVR_DEFAULT_NECK_TO_EYE_VERTICAL};
		neckOffset = OVRManager.capiHmd.GetFloatArray(Hmd.OVR_KEY_NECK_TO_EYE_DISTANCE, neckOffset);
		Vector3 neck = new Vector3(0, neckOffset[1], neckOffset[0]);
		headBone.transform.localPosition = neck;
	
	}
}
