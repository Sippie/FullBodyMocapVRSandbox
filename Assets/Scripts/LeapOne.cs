using UnityEngine;
using System.Collections;
using Leap;
using System.Collections.Generic;

public class LeapOne : MonoBehaviour {

	//public GameObject testcube;

	//public Vector3 RightforeArmRotOffset;
	//public Vector3 RightHandRotOffset;
	//public Vector3 RightHandPosOffset;
	
	public int handAmount;
	
	public float confidenceThreshold = 0.1f;
	public float handconfidence;

	public Transform RightArm;
	public Transform RightForeArm;
	public Transform RightHand;
	public HandModel RightHandModel;
	public Transform LeftArm;
	public Transform LeftForeArm;
	public Transform LeftHand;
	public HandModel LeftHandModel;
	public Hand leapHand;
	public GameObject leapcarrier;
	private HandController handcontroller;
	private Frame frame;
	public Transform[] RightfullHand;
	public Transform[] LeftfullHand;
	private Hand hand1;
	private Hand hand2;

	public bool ikActive = false;
	/*
	private HandModel leapCurrentHand;
	//private HandModel[] leapHandModels;

	// LEAP
	public Vector3 leapRightHandPos;

	public Transform[] RightFingers;
	//private CustomFixedUpdate Leap_UpdateInstance;

	// LEAP FRAME
	public Vector3 leapRightForeArmPos;
	*/



	public float getConfidence(){
		return handconfidence;
	}


	void Start () {
		Leap.Utils.IgnoreCollisions(gameObject, gameObject);
		//animator = GetComponent<Animator>();
		RightfullHand = RightHand.GetComponentsInChildren<Transform>();
		LeftfullHand = LeftHand.GetComponentsInChildren<Transform>();
		handcontroller = leapcarrier.GetComponent<HandController>();

	}


	void LateUpdate () {
		
		
		if(handcontroller.IsConnected()){			
			
			frame = handcontroller.GetFrame();
			hand1 = frame.Hands.Rightmost;
			hand2 = frame.Hands.Leftmost;
			//hand2 = frame.Hands.Frontmost;
			
			
			// hand1 CONFIDENCE
			if(hand1.Confidence > confidenceThreshold){
				
				if (hand1.IsRight){
					//RightHand.parent = null;
					RightForeArm.parent =null;

					// UPDATE HAND & FINGERS

						RightHandModel.SetLeapHand(hand2);
						RightHandModel.SetController(handcontroller);
						RightHandModel.UpdateHand();

				}//isright

				else{
					//LeftHand.parent = null;
					LeftForeArm.parent =null;
					
					// UPDATE HAND & FINGERS
					
					LeftHandModel.SetLeapHand(hand1);
					LeftHandModel.SetController(handcontroller);
					LeftHandModel.UpdateHand();
					
				}//isleft
				
			}// hand1 CONFIDENCE

			// hand2 CONFIDENCE
			if(hand2.Confidence > confidenceThreshold){
				
				if (hand2.IsLeft){
					//LeftHand.parent = null;
					LeftForeArm.parent =null;
					
					// UPDATE HAND & FINGERS
					
					LeftHandModel.SetLeapHand(hand1);
					LeftHandModel.SetController(handcontroller);
					LeftHandModel.UpdateHand();

					
				}//isright
				
				else{
					//RightHand.parent = null;
					RightForeArm.parent =null;
					
					// UPDATE HAND & FINGERS
					
					RightHandModel.SetLeapHand(hand2);
					RightHandModel.SetController(handcontroller);
					RightHandModel.UpdateHand();
					
				}//isleft
				
			}// hand2 CONFIDENCE

						
				
			
			else{
				RightHand.parent = RightForeArm;
				LeftHand.parent = LeftForeArm;
				RightForeArm.parent = RightArm;
				LeftForeArm.parent = LeftArm;

				//reset local position
				RightHand.transform.localPosition = new Vector3(0.2819f, 0, 0);
				RightForeArm.transform.localPosition = new Vector3(0.25876f, 0, 0);
				LeftHand.transform.localPosition = new Vector3(-0.2819f, 0, 0);
				LeftForeArm.transform.localPosition = new Vector3(-0.25876f, 0, 0);
				foreach (Transform child in RightfullHand) {
					child.transform.localEulerAngles = Vector3.zero;
				}
				foreach (Transform child in LeftfullHand) {
					child.transform.localEulerAngles = Vector3.zero;
				}

			}
			
		}// handcontroller connected
		
	}//update
			


}



		





