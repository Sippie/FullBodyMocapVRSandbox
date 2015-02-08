/******************************************************************************\
* Copyright (C) Leap Motion, Inc. 2011-2014.                                   *
* Leap Motion proprietary. Licensed under Apache 2.0                           *
* Available at http://www.apache.org/licenses/LICENSE-2.0.html                 *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using Leap;

// Class to setup a rigged hand based on a model.
public class RiggedHand : HandModel {

  public Transform palm;
  public Transform foreArm;
  public Transform armBone;
  //public bool isLeft; 
  public Vector3 modelFingerPointing = Vector3.forward;
  public Vector3 modelPalmFacing = -Vector3.up;
  
	//measured foreArmlength 56,804; 
	//measured Armlength 51,752; 

  public static float targetForeArmlength= 0.28402f*2;
  public static float targetArmlength= 0.25876f*2;
  public Vector3 targetElbowPosition;
	Vector3 correctedElbowPosition;

  public override void InitHand() {
	
		UpdateHand();
  }

  public float getBoneLength(Transform StartJoint, Transform EndtJoint){
		Vector3 vec = EndtJoint.position - StartJoint.position;
		float TargetLength = vec.magnitude;
		return  TargetLength;
	}


  public Quaternion Reorientation() {
    return Quaternion.Inverse(Quaternion.LookRotation(modelFingerPointing, -modelPalmFacing));
  }

  public override void UpdateHand() {

		//Debug.Log("foreArmlength: "+ targetForeArmlength);
		//Debug.Log("Armlength: "+ targetArmlength); 

    if (palm != null) {
	    //palm.position = GetPalmPosition();
		palm.position = GetWristPosition(); 
	 	palm.rotation = GetPalmRotation() * Reorientation();
    }

    if (foreArm != null) {
		//Vector3 moveRel = foreArm.transform.InverseTransformDirection (Vector3.forward);
			//Debug.Log(moveRel);
		//foreArm.position = GetArmCenter() + moveRel*4;
		//transform offset

		//foreArm.TransformDirection = GetArmDirection();
		//foreArm.position = GetArmCenter();
		
		
		targetElbowPosition = GetWristPosition()-(GetArmDirection() * targetForeArmlength);
		
		foreArm.position = targetElbowPosition;	
		foreArm.rotation = GetArmRotation () * Reorientation ();
	}

	if (armBone != null) {

		Vector3 ArmVector = targetElbowPosition - armBone.transform.position;
		armBone.transform.rotation = Quaternion.LookRotation(ArmVector.normalized) * Reorientation ();
	
		
		
		
		/*//measured Armlength 51,752; 
		float correntArmlength = getBoneLength(armBone,foreArm);
		
		float offset = targetArmlength - correntArmlength;

		//Debug.Log("offset " + offset);
		
		if (offset>0) {

			//Debug.Log("Armlength Overscaled " + correntArmlength);
			correctedElbowPosition = targetElbowPosition + (ArmVector * offset);
			//foreArm.position = correctedElbowPosition;
				//Debug.Log("Overscaled Armlength corrected " + offset);
		}
		if (offset<0) {
				
			//Debug.Log("Armlength Downscaled " + correntArmlength);
			correctedElbowPosition = targetElbowPosition + (ArmVector * offset);
			//foreArm.position = correctedElbowPosition;
				//Debug.Log("Downscaled Armlength corrected" + offset);
			}

	*/

  	}
		// FINGERS
		for (int i = 0; i < fingers.Length; ++i) {
			if (fingers[i] != null)
				fingers[i].UpdateFinger();
		}
  }
}