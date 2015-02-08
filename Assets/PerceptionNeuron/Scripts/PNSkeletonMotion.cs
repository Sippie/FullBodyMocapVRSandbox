/************************************************************************************

Copyright   :   Copyright 2014 Beijing Noitom Technology Ltd. All Rights reserved.
Pending Patents:  PCT/CN2014/085659  PCT/CN2014/071006  
 
Licensed under the Perception Neuron SDK License Beta Version (the “License");
You may only use the Perception Neuron SDK when in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in the form of either an electronic or a hard copy.
 
Unless required by applicable law or agreed to in writing, the Perception Neuron SDK
distributed under the License is provided on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing conditions and
limitations under the License.

************************************************************************************/

using UnityEngine;
using System.Collections;

[RequireComponent (typeof (PNDataParser))]

public class PNSkeletonMotion : MonoBehaviour {
	private LeapOne leapcontroller;
	public int startIgnore = 30;
	public bool DirectMotion = false;
	public int UpdateRateSkeleton = 96;						// How many times per second we update position and rotation of each bone.
	public int UpdateRateTargetSkeleton = 96;				// How many times per second we update position and rotation of each bone.
	public bool AutoSettings = true;
	public bool UseUpdateByPhysics;							// Apply the position and rotation using Unity's Rigidbody Physics.
	public bool UseUpdateInGlobalSpace;						// Use the received values as global position and rotation instead of local.

	[HideInInspector]
	public string UsedSkeletonNamePrefix = "Robot_";
	[HideInInspector]
	public GameObject SavedSkeletonTargetRoot;

	public Transform[] Skeleton = new Transform[59];			// The reference list for all the Bones. These bones get moved to the position and rotation of the Ghost Bones.
	public Transform[] TargetSkeleton = new Transform[59];		// The reference list for all the Target Bones. These display the received motion.

	private Vector3[] _lastPositionUpdates = new Vector3[59];
	private Vector3[] _lastRotationUpdates = new Vector3[59];

	private PNDataParser _dataParser;
	private CustomFixedUpdate TargetSkeleton_UpdateInstance;
	private CustomFixedUpdate Skeleton_UpdateInstance;

	
	void Awake() {
		TargetSkeleton_UpdateInstance = new CustomFixedUpdate(1.0f/UpdateRateTargetSkeleton, ApplyMotionToTargetSkeleton);
		Skeleton_UpdateInstance = new CustomFixedUpdate(1.0f/UpdateRateSkeleton, ApplyMotionToSkeleton);
	}


	void Start () {
		leapcontroller = GetComponent<LeapOne>();
		if (AutoSettings) {
			//decide what to do depending on our setup
			// if we have rigidbodies then turn on physics movement
			if (Skeleton [1].rigidbody != null) {
				UseUpdateInGlobalSpace = true;
				UseUpdateByPhysics = true;
			}
		}

		_dataParser = GetComponent<PNDataParser> ();

		if(UseUpdateInGlobalSpace) {
			// If we don't detach all the bones from the hierarchy the rotational movement change using physics rotation method
			// will not work. Best thing is to parent it to the reference object above the hips, thus detaching all bones from the
			// chain and getting proper rotations as well as proper collisions.
			foreach (Transform t in Skeleton) {
				if (t != null)
					t.parent = Skeleton[0].transform.parent.transform;	
			}
		}
	}

	/// <summary>
	/// Invoke the callbacks to the functions that update bone positions and rotations.
	/// </summary>
	void Update () {
		TargetSkeleton_UpdateInstance.Update();
		Skeleton_UpdateInstance.Update();
	}

	/// <summary>
	/// If the data parses is connected and we're using the physic system to move each bone, then run the bone update
	/// functions inside FixedUpdate to stay inside Unitys physic engine loop.
	/// </summary>
	void FixedUpdate () {
		// physic functions by Unity need to happen at a frame-rate independent fixed interval, like inside FixedUpdate
		if (!DirectMotion) { // only do physics movement stuff if we're not applying data values directly
			if (_dataParser.IsConnected && UseUpdateByPhysics) {
				if (Skeleton[0].rigidbody == null ) {
					Debug.Log("<color=red>[WARNING]</color> No RigidBody Component found while trying to move using physics!");
					return;
				}
				for (int i=0; i<Skeleton.Length; i++) {
					if (Skeleton[i] != null) {
						UpdateTransformRigidbody(Skeleton[i].rigidbody, TargetSkeleton[i].position, TargetSkeleton[i].rotation);
					}
				}
			}
		}
	}


	/// <summary>
	/// Updates the transform position and rotation in global space.
	/// </summary>
	/// <param name="t">The transform we're changing.</param>
	/// <param name="pos">The Vector3 position in global space.</param>
	/// <param name="rot">The Quaternion rotation in global space.</param>
	private void UpdateTransformGlobally(Transform t, Vector3 pos, Quaternion rot) {
		// position
		if ( pos != Vector3.zero ) {
			if ( pos != t.position ) {
				t.position = pos;
			}
		}
		// rotation
		if ( rot != Quaternion.identity ) {
			if (rot != t.rotation) {
				t.rotation = rot;
			} 
		}
	}
	
	/// <summary>
	/// Updates the transform position and rotation in local space.
	/// </summary>
	/// <param name="t">The transform we're changing.</param>
	/// <param name="pos">The Vector3 position in local space.</param>
	/// <param name="rot">The Quaternion rotation in local space.</param>
	private void UpdateTransformLocally(Transform t, Vector3 pos, Quaternion rot) {
		// position
		if ( pos != Vector3.zero ) {
			if ( pos != t.localPosition ) {
				t.localPosition = pos;
			}
		}
		
		// rotation
		if ( rot != Quaternion.identity ) {
			if (rot != t.localRotation) {
				t.localRotation = rot;
			}
		}
	}

	/// <summary>
	/// Updates the rigidbody position and rotation using Unitys physic engine functions.
	/// </summary>
	/// <param name="r">The rigidbody we're changing.</param>
	/// <param name="pos">The Vector3 position in global space.</param>
	/// <param name="rot">The Quaternion rotation in global space.</param>
	private void UpdateTransformRigidbody(Rigidbody r, Vector3 pos, Quaternion rot) {
		// position
		if ( pos != Vector3.zero ) {
			if ( pos != r.position ) {
				r.MovePosition(pos);
			} 
		}
		
		// rotation
		if ( rot != Quaternion.identity ) {
			if (rot != r.rotation) {
				r.MoveRotation(rot);
			} 
		}
	}

	/// <summary>
	/// Uses the original BVH values received from the dataParser and applies them to the target skeleton setup.
	/// </summary>
	/// <param name="dt">How many times per second we run this function.</param>
	private void ApplyMotionToTargetSkeleton( float dt ){
		if (Skeleton[0] == null ) {
			Debug.Log("<color=red>[WARNING]</color> You didn't assign a bone setup in the Skeleton reference list!");
			return;
		}
		if (!DirectMotion && TargetSkeleton[0] == null ) {
			Debug.Log("<color=red>[WARNING]</color> You didn't assign a bone setup in the Target Motion Skeleton reference list!");
			return;
		}
		if (!_dataParser.IsConnected) {
			return;		
		}

		if (DirectMotion) {
			// dont move the target skeleton if we apply the motion directly
			return;	
		}

		for (int i=0; i<TargetSkeleton.Length; i++) {
			Vector3 pV = _dataParser.PositionValues[i];
			Vector3 rV = _dataParser.RotationValues[i];

			if (TargetSkeleton[i] != null ) {
				// Use the physics engine functions to move the rigidbody to a new position instead of doing it stop motion like
				// unfortunately doesn't really work properly with local rotation changes
				// also see unity documentation for this function
				
				// apply the motion to the ghost skeleton then use that information to move ourself	
				if(leapcontroller != null && leapcontroller.getConfidence() < leapcontroller.confidenceThreshold){
					if(i < startIgnore || i > 35){ //from arm
						// position
						if ( pV != Vector3.zero ) {
							if ( pV != _lastPositionUpdates[i] ) {
							TargetSkeleton[i].localPosition = pV;
								// save updated values
								_lastPositionUpdates[i] = pV;
							} 
						}

						// rotation
						if ( rV != Vector3.zero ) {
							if (rV != _lastPositionUpdates[i]) {
								TargetSkeleton[i].localEulerAngles = rV;
								// save updated values
								_lastRotationUpdates[i] = rV;
							} 
						}
					}
				}else{
					if(i < startIgnore || i > 35){
						// position
						if ( pV != Vector3.zero ) {
							if ( pV != _lastPositionUpdates[i] ) {
								TargetSkeleton[i].localPosition = pV;
								// save updated values
								_lastPositionUpdates[i] = pV;
							} 
						}
						
						// rotation
						if ( rV != Vector3.zero ) {
							if (rV != _lastRotationUpdates[i]) {
								TargetSkeleton[i].localEulerAngles = rV;
								// save updated values
								_lastRotationUpdates[i] = rV;
							} 
						}
					}
				} 
			}
		}
	}
	
	/// <summary>
	/// This function uses the position and rotation values from each target skeleton bone to move the skinned skeleton setup.
	/// Depending on the settings uses the global or local values.
	/// </summary>
	/// <param name="dt">How many times per second we run this function.</param>
	private void ApplyMotionToSkeleton(float dt){
		if (Skeleton[0] == null ) {
			Debug.Log("[WARNING] You didn't assign a bone setup in the Skeleton reference list!");
			return;
		}
		if (!_dataParser.IsConnected || UseUpdateByPhysics) {
			return;		
		}

		//check if we're using displacement data and haven't updated our settings
		if(AutoSettings) {
			UseUpdateInGlobalSpace = _dataParser.WithDisplacement;
		}

		// apply the motion directly from the data reader as local values as we receive them
		if (DirectMotion) {

 			Skeleton[0].localPosition = _dataParser.PositionValues[0];

			if(leapcontroller != null && leapcontroller.getConfidence() < leapcontroller.confidenceThreshold){

			for (int i=0; i<TargetSkeleton.Length; i++) {

				if ( Skeleton[i] != null) {
						Skeleton[i].localEulerAngles = _dataParser.RotationValues[i];
				}

			}
			}else{
				for (int i=0; i<TargetSkeleton.Length; i++) {
					if(i < startIgnore || i > 35){
					//if(i == 12){ //head only
						if ( Skeleton[i] != null) {
							Skeleton[i].localEulerAngles = _dataParser.RotationValues[i];
						}
					
					}
				}

			}
			

		} else {
			Debug.Log("no direct motion");
			for (int i=0; i<Skeleton.Length; i++) {

				if (Skeleton[i] != null) {
					// only update position and rotation of real body if we're not using Unitys physic system
					if(UseUpdateInGlobalSpace) {
						UpdateTransformGlobally(Skeleton[i], TargetSkeleton[i].position, TargetSkeleton[i].rotation);
					} else {
						UpdateTransformLocally(Skeleton[i], TargetSkeleton[i].localPosition, TargetSkeleton[i].localRotation);
					}
				}

			}
		}
		//leapcontroller.ApplyLeap();
	}

	/// <summary>
	/// Display some info and help to turn some things on and off for testing.
	/// </summary>
	void OnGUI() {
		int boxW = 160;
		int boxH = 200;
		int boxX = 3;
		int boxY = Screen.height - boxH - 10;

		// border
		GUI.Box(new Rect(boxX+3,boxY+3,boxW,boxH), "Motion Settings");

		if (GUI.Button (new Rect (boxX+7, boxY+30, boxW -10, 22), "Toggle Bone Lines"))	{
			PNUtilities.ToggleBoneLines(Skeleton[0].root.gameObject);
		}		
		if (GUI.Button (new Rect (boxX+7, boxY+60, boxW -10, 22), "Toggle Bone Colliders"))	{
			PNUtilities.ToggleColliders(Skeleton[0].root.gameObject);
		}

		AutoSettings = GUI.Toggle(new Rect(boxX+10, boxY+100, boxW-10, 20), AutoSettings, "Auto Motion Settings");

		if ( !AutoSettings) {
			UseUpdateInGlobalSpace = GUI.Toggle(new Rect(boxX+10, boxY+120, boxW-10, 20), UseUpdateInGlobalSpace, "Global Values");
			UseUpdateByPhysics = GUI.Toggle(new Rect(boxX+10, boxY+140, boxW-10, 20), UseUpdateByPhysics, "Move using physics");
		}

		_dataParser.DEBUG_Enabled = GUI.Toggle(new Rect(boxX+10, boxY+160, boxW-10, 20), _dataParser.DEBUG_Enabled, "Show Debug Data");
	}
}
