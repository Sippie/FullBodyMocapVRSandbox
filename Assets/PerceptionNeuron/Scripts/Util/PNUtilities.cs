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

/// <summary>
/// Various utility functions that help with removing and adding components in the right place
/// and also help with finding the right bone structure for the Perception Neuron motion pipeline.
/// </summary>
public class PNUtilities : MonoBehaviour {

	/// <summary>
	/// Loads the references for the Transforms inside the received list.
	/// </summary>
	/// <param name="target">The target Transform reference list.</param>
	/// <param name="root">The root object. Used to search in all children.</param>
	/// <param name="string">The first part of the names of the bones.</param>
	/// <param name="boneNameID">A index number used to find different bone names from a list.</param>
	public static void LoadSkeletonReference( Transform[] target, Transform root, string prefix, int boneNameID ) {
		if (root == null) {
			Debug.Log("<color=red>[ERROR]</color> LoadSkeletonReference was called with null root object!");
			return;
		}
		if (target == null) {
			Debug.Log("<color=red>[ERROR]</color> LoadSkeletonReference was called with null target reference list!");
			return;
		}

		Transform[] sList = root.GetComponentsInChildren<Transform> ();
		//Transform[] tsList = Skeleton_Target_Root.GetComponentsInChildren<Transform> ();
		
		for (int i=0; i < target.Length; i++) {
			target[i] = FindBone(sList, prefix + GetBoneName(boneNameID,i) );
		}

		Debug.Log ("Loaded Skeleton Reference for target object: " + root.name);
	}

	/// <summary>
	/// Searches for a Transform by the received name inside the received list. Returns the Transform if it found one. Returns null if there was none.
	/// </summary>
	/// <param name="list">The list of Transforms to search in.</param>
	/// <param name="name">The name of the Transform we want to find.</param>
	public static Transform FindBone(Transform[] list, string name) {
		for (int i=0; i < list.Length; i++) {
			if ( list[i].name == name ) {
				return list[i];
			}
		}
		
		//if we get this far it means we didn't find a object with the input name
		Debug.Log("Did not find the bone object [" + name +"]!");
		return null;
	}

	/// <summary>
	/// Adds a RigidBody component to each Transform in the received target list.
	/// </summary>
	/// <param name="target">The target list of Transforms objects to use.</param>
	public static void AttachRigidbodies( Transform[] target) {
		int counter = 0;
		
		foreach (Transform t in target) {
			if ( t != null ) {
				
				//clear existing rigibody
				Rigidbody r = t.GetComponent<Rigidbody>();
				if ( r != null ) DestroyImmediate(r);
				
				Rigidbody newRB;
				newRB = t.gameObject.AddComponent<Rigidbody>();
				//if ( counter > 0 ) { // first one needs to be non-kinematic in order to move the hips global position using physics
					newRB.isKinematic = true;
				//}
				//newRB.interpolation = RigidbodyInterpolation.Extrapolate;
				newRB.useGravity = false;
				counter++;
			}
		}
		
		Debug.Log (counter + " Rigidbody Components added to: " + target[0].root.name);
	}

	/// <summary>
	/// Adds LineRenderer components to all Transforms inside the received target list. Also adjust some settings along the way.
	/// </summary>
	/// <param name="target">The target list of Transforms objects to use.</param>
	/// <param name="prefix">The first part of the bone names. Used to find end points for the fingers, feets and head.</param>
	/// <param name="startSize">The start size of the LineRenderer.</param>
	/// <param name="endSize">The end size of the LineRenderer.</param>
	/// <param name="color">The color of the LineRenderer.</param>
	/// <param name="rQPlus">The value by how much we increase the RenderQueue of the material. This is used to render some lines on top of others.</param>
	public static void CreateBoneLines(Transform[] target, string prefix, float startSize, float endSize, Color color, int rQPlus ) {
		if (target[0] == null) {
			Debug.Log("<color=red>[ERROR]</color> CreateBoneLines was called with null target reference list!");
			return;
		}
		int counter = 0;

		// make a search list:
		Transform[] SearchList = target [0].GetComponentsInChildren<Transform> ();

		for (int i=0; i < target.Length; i++) {			
			if (target[i] != null) { 
				// use line renderer instead of gizmo for bone display
				//if we have a linerender already then remove the old one first
				LineRenderer oldGizmo = target[i].GetComponent<LineRenderer>();
				if ( oldGizmo != null ) DestroyImmediate(oldGizmo);
				BoneVisualizer oldBV = target[i].GetComponent<BoneVisualizer>();
				if ( oldBV != null ) DestroyImmediate(oldBV);
				
				
				LineRenderer newLine = target[i].gameObject.AddComponent<LineRenderer>() as LineRenderer;
				BoneVisualizer newBV = target[i].gameObject.AddComponent<BoneVisualizer>() as BoneVisualizer;

				// changed this to loading a material from the asset library because the generated material would not save to a prefab.
				Material newMat;
				if (rQPlus == 0 ) {
					newMat = Resources.LoadAssetAtPath("Assets/PerceptionNeuron/Resources/BoneLinesMaterial_0.mat", typeof(Material)) as Material;
				} else {
					newMat = Resources.LoadAssetAtPath("Assets/PerceptionNeuron/Resources/BoneLinesMaterial_1.mat", typeof(Material)) as Material;
				}
				//Material newMat = new Material(Shader.Find("Debug/BoneDebugShader"));;
				//change render order to render target bones on top of real bones
				newMat.renderQueue += rQPlus;
				
				newMat.color = color;
				newBV.renderer.sharedMaterial = newMat;
				newBV.StartPoint = target[i];
				newLine.SetWidth(startSize,endSize);
				
				// set end point depending on chain
				// chains:
				// 0 hip to spine
				if (i == 0 ) {
					newBV.EndPoint = target[7];
					
					// 0 - 3 hips to right foot
				} else if (i <= 3 ) {
					// if its the last limb, find the end point in the next child
					if (i == 3) 	newBV.EndPoint = FindBone (SearchList, prefix + "RightToeBase");
					else 			newBV.EndPoint = target[i+1];
					
					
					// 0 + 4-6 hips to left foot
				} else if (i <= 6) {
					if (i == 6) 	newBV.EndPoint = FindBone (SearchList, prefix + "LeftToeBase");
					else 			newBV.EndPoint = target[i+1];
					
					// 0 + 7-12 hips to head
				} else if (i <= 12 ) {
					if (i == 12) {
						if (FindBone (SearchList, "shou_4:head") != null) 
									newBV.EndPoint = FindBone (SearchList, "shou_4:head");
						else  
									newBV.EndPoint = target[i]; // if we dont find another end point on the head set the end point to the start point
					} else { 		
						if (target[i+1] == null ) // Is there no valid spine end point?
									newBV.EndPoint = target[i];
						else
									newBV.EndPoint = target[i+1]; 
					}
				}
				// 13 - 16 right shoulder to right hand
				else if (i <= 16 ) {
									newBV.EndPoint = target[i+1];
				}
				// right hand fingers:
				// 17 - 19 thumb
				else if (i <= 19 ) {
					// if its the last limb, find the end point in the next child
					if (i == 19) 	newBV.EndPoint = FindBone (SearchList, prefix + "RightHandThumb4");
					else 			newBV.EndPoint = target[i+1];
				}
				// 20 - 23 index
				else if (i <= 23 ) {
					// if its the last limb, find the end point in the next child
					if (i == 23) 	newBV.EndPoint = FindBone (SearchList, prefix + "RightHandIndex4");
					else 			newBV.EndPoint = target[i+1];
				}
				// 24 - 27 middle
				else if (i <= 27 ) {
					// if its the last limb, find the end point in the next child
					if (i == 27) 	newBV.EndPoint = FindBone (SearchList, prefix + "RightHandMiddle4");
					else 			newBV.EndPoint = target[i+1];
				}
				// 28 - 31 ring
				else if (i <= 31 ) {
					// if its the last limb, find the end point in the next child
					if (i == 31) 	newBV.EndPoint = FindBone (SearchList, prefix + "RightHandRing4");
					else 			newBV.EndPoint = target[i+1];
				}
				// 32 - 35 pinky
				else if (i <= 35 ) {
					// if its the last limb, find the end point in the next child
					if (i == 35) 	newBV.EndPoint = FindBone (SearchList, prefix + "RightHandPinky4");
					else 			newBV.EndPoint = target[i+1];
				}
				// 36 - 39 left shoulder to left hand
				else if (i <= 39 ) {
					newBV.EndPoint = target[i+1];
				}
				// left hand fingers:
				// 40 - 42 thumb
				else if (i <= 42 ) {
					// if its the last limb, find the end point in the next child
					if (i == 42) 	newBV.EndPoint = FindBone (SearchList, prefix + "LeftHandThumb4");
					else 			newBV.EndPoint = target[i+1];
				}
				// 43 - 46 index
				else if (i <= 46 ) {
					// if its the last limb, find the end point in the next child
					if (i == 46) 	newBV.EndPoint = FindBone (SearchList, prefix + "LeftHandIndex4");
					else 			newBV.EndPoint = target[i+1];
				}
				// 47 - 50 middle
				else if (i <= 50 ) {
					// if its the last limb, find the end point in the next child
					if (i == 50) 	newBV.EndPoint = FindBone (SearchList, prefix + "LeftHandMiddle4");
					else 			newBV.EndPoint = target[i+1];
				}
				// 51 - 54 ring
				else if (i <= 54 ) {
					// if its the last limb, find the end point in the next child
					if (i == 54) 	newBV.EndPoint = FindBone (SearchList, prefix + "LeftHandRing4");
					else 			newBV.EndPoint = target[i+1];
				}
				// 55 - 58 pinky
				else if (i <= 58 ) {
					// if its the last limb, find the end point in the next child
					if (i == 58) 	newBV.EndPoint = FindBone (SearchList, prefix + "LeftHandPinky4");
					else 			newBV.EndPoint = target[i+1];
				}
				
				//change size of line if we're finger bones
				if (i > 16 && i < 36 || i > 39 ) {
					newLine.SetWidth(startSize/3,endSize/3);
				}
				counter++;
			}
		}
		Resources.UnloadUnusedAssets();
		Debug.Log (counter + " LineRenderer Components added to: " + target[0].root.name);
	}

	/// <summary>
	/// Searches for all Rigidbody components in the children of the received target object and removes them.
	/// </summary>
	/// <param name="target">The root object to search in.</param>
	public static void RemoveRigidbodies(GameObject target) {
		if (target == null) {
			Debug.Log("<color=red>[ERROR]</color> RemoveRigidbodies was called with no target object!");
			return;
		}
		int counter = 0;
		Rigidbody[] RBs = target.GetComponentsInChildren<Rigidbody> ();
		
		foreach (Rigidbody t in RBs) {
			if ( t != null ) {
				DestroyImmediate(t);
				counter++;
			}
		}		
		Debug.Log (counter + " Rigidbody Components removed in " + target.name);
	}

	/// <summary>
	/// Searches for all Box and Capsule Collider components in the children of the received target object and then removes the GameObject they belong to.
	/// </summary>
	/// <param name="target">The root object to search in.</param>
	public static void RemoveColliders(GameObject target) {
		if (target == null) {
			Debug.Log("<color=red>[ERROR]</color> RemoveColliders was called with no target object!");
			return;
		}
		int counter = 0;
		CapsuleCollider[] currentCC = target.GetComponentsInChildren<CapsuleCollider>();
		BoxCollider[] currentBC = target.GetComponentsInChildren<BoxCollider>();
		
		foreach ( CapsuleCollider c in currentCC ) {
			if (c != null) {
				DestroyImmediate(c.gameObject);
				counter++;
			}
		}
		foreach ( BoxCollider c in currentBC ) {
			if (c != null) {
				DestroyImmediate(c.gameObject);
				counter++;
			}
		}

		Debug.Log (counter + " GameObjects with Box and Capsule Collider Components removed in " + target.name);
	}

	/// <summary>
	/// Searches for all LineRenderer components and BoneVisualizer scripts in the children of the received target object and removes them.
	/// </summary>
	/// <param name="target">The root object to search in.</param>
	public static void RemoveBoneLines(GameObject target) {
		if (target == null) {
			Debug.Log("<color=red>[ERROR]</color> RemoveBoneLines was called with null target reference list!");
			return;
		}
		int counter = 0;
		int counter2 = 0;
		LineRenderer[] LRs = target.GetComponentsInChildren<LineRenderer> ();
		BoneVisualizer[] BVs = target.GetComponentsInChildren<BoneVisualizer> ();
		
		foreach (LineRenderer t in LRs) {
			if ( t != null ) {
				DestroyImmediate(t);
				counter++;
			}
		}	
		foreach (BoneVisualizer t in BVs) {
			if ( t != null ) {
				DestroyImmediate(t);
				counter2++;
			}
		}
		Debug.Log (counter + " LineRenderer Components and " + counter2 + " BoneVisualizer scripts removed in " + target.name);
	}


	/// <summary>
	/// Copys the colliders from a prefab collider setup ontu the target skeleton structure.
	/// </summary>
	/// <param name="colliderSetup">The prefab with all the colliders in a BVH skeleton structure.</param>
	/// <param name="target">The target skeleton list we will copy the colliders to.</param>
	public static void CopyColliders(GameObject colliderSetup, Transform[] target) {
		if (colliderSetup == null) {
			Debug.Log("<color=red>[ERROR]</color> CopyColliders was called with no collider setup prefab!");
			return;
		}
		if (target == null) {
			Debug.Log("<color=red>[ERROR]</color> CopyColliders was called with null target reference list!");
			return;
		}
		// find and remove the old colliders first
		RemoveColliders (target [0].gameObject);

		// Instantiate the colliders
		GameObject csObj = Instantiate (colliderSetup, target [0].root.position, target [0].root.rotation) as GameObject;

		// generate source list from sourceObj
		int counter = 0;
		Transform[] sourceL = new Transform[59];
		LoadSkeletonReference (sourceL, csObj.transform, "Robot_", 0);

		for (int i=target.Length-1; i >= 0; i--) {
			// find all the objects with colliders and copy them over
			// Do this from the bottom of the hierarchy so we only copy the childrens of the current bone

			// if we dont have a valid target bone then skip over it
			if (target[i] != null) {
				CapsuleCollider[] currentCC = sourceL[i].GetComponentsInChildren<CapsuleCollider>();
				BoxCollider[] currentBC = sourceL[i].GetComponentsInChildren<BoxCollider>();

				foreach ( CapsuleCollider c in currentCC ) {
					if (c != null) {
						//GameObject clone = Instantiate(c.gameObject, c.transform.position, c.transform.rotation) as GameObject;
						//clone.transform.parent = target[i];
						Vector3 savedPos = c.transform.localPosition;
						Quaternion savedRot = c.transform.localRotation;
						c.transform.parent = target[i];
						c.transform.localPosition = savedPos;
						c.transform.localRotation = savedRot;
						c.renderer.enabled = false;

						c.gameObject.name = "COL_" + target[i].name;
						counter++;
					}
				}
				foreach ( BoxCollider c in currentBC ) {
					if (c != null) {
						//GameObject clone = Instantiate(c.gameObject, c.transform.position, c.transform.rotation) as GameObject;
						//clone.transform.parent = target[i];
						Vector3 savedPos = c.transform.localPosition;
						Quaternion savedRot = c.transform.localRotation;
						c.transform.parent = target[i];
						c.transform.localPosition = savedPos;
						c.transform.localRotation = savedRot;
						c.renderer.enabled = false;

						c.gameObject.name = "COL_" + target[i].name;
						counter++;
					}
				}
			}
		}
		DestroyImmediate (csObj); // destroy the now empty collider skeleton prefab we instantiated before
		Resources.UnloadUnusedAssets ();
		Debug.Log (counter + " Box and Sphere Collider Objects copied to " + target[0].root.name);
	}


	/// <summary>
	/// Turns all the LineRenderer components and BoneVisualizer scripts in the childrens in the received target object on or off.
	/// </summary>
	/// <param name="target">The target object we search in.</param>
	public static void ToggleBoneLines(GameObject target) {
		if (target == null) {
			Debug.Log("<color=red>[ERROR]</color> ToggleBoneLines was called with no target object!");
			return;
		}
		LineRenderer[] BoneLines = target.GetComponentsInChildren<LineRenderer> ();
		BoneVisualizer[] BoneLineScript = target.GetComponentsInChildren<BoneVisualizer> ();
		
		foreach (LineRenderer lr in BoneLines) {
			lr.enabled = !lr.enabled;
		}
		foreach (BoneVisualizer s in BoneLineScript) {
			s.enabled = !s.enabled;
		}
	}
	
	/// <summary>
	/// Turns all the renderer components in the childrens in the received target object that have a Box or Capsule Collider on or off.
	/// </summary>
	/// <param name="target">The target object we search for childrens with colliders in.</param>
	public static void ToggleColliders(GameObject target) {
		if (target == null) {
			Debug.Log("<color=red>[ERROR]</color> ToggleColliders was called with no target object!");
			return;
		}
		
		CapsuleCollider[] BoneCapsuleColliders = target.GetComponentsInChildren<CapsuleCollider> ();
		BoxCollider[] BoneBoxColliders = target.GetComponentsInChildren<BoxCollider> ();
		
		foreach (CapsuleCollider cc in BoneCapsuleColliders) {
			cc.renderer.enabled = !cc.renderer.enabled;
		}
		foreach (BoxCollider bc in BoneBoxColliders) {
			bc.renderer.enabled = !bc.renderer.enabled;
		}
	}
	
	/// <summary>
	/// Set the renderer components of all childrens in the received target object that have a Box or Capsule Collider to the input bool value.
	/// </summary>
	/// <param name="target">The target object we search for childrens with colliders in.</param>
	public static void SetColliderRenderer(GameObject target, bool state) {
		if (target == null) {
			Debug.Log("<color=red>[ERROR]</color> SetColliderRenderer was called with no target object!");
			return;
		}
		
		CapsuleCollider[] BoneCapsuleColliders = target.GetComponentsInChildren<CapsuleCollider> ();
		BoxCollider[] BoneBoxColliders = target.GetComponentsInChildren<BoxCollider> ();
		
		foreach (CapsuleCollider cc in BoneCapsuleColliders) {
			cc.renderer.enabled = state;
		}
		foreach (BoxCollider bc in BoneBoxColliders) {
			bc.renderer.enabled = state;
		}
	}


	/// <summary>
	/// Returns the bone name for the received index position according to the BVH structure.
	/// </summary>
	/// <param name="type">The indentification type for different bone naming conventions.</param>
	/// <param name="index">The index position of the bone name in the BVH structure.</param>
	public static string GetBoneName(int type, int index) {
		// Default names for Perception Neuron BVH structure
		if ( type == 0 ) {
			if (index==0) 	return "Hips";
			if (index==1) 	return "RightUpLeg";
			if (index==2) 	return "RightLeg";
			if (index==3) 	return "RightFoot";
			if (index==4) 	return "LeftUpLeg";
			if (index==5) 	return "LeftLeg";
			if (index==6) 	return "LeftFoot";
			if (index==7) 	return "Spine";
			if (index==8) 	return "Spine1";
			if (index==9) 	return "Spine2";
			if (index==10) 	return "Spine3";
			if (index==11) 	return "Neck"; 
			if (index==12)	return "Head";
			if (index==13)	return "RightShoulder";
			if (index==14)	return "RightArm";
			if (index==15)	return "RightForeArm";
			if (index==16)	return "RightHand";
			if (index==17)	return "RightHandThumb1";
			if (index==18)	return "RightHandThumb2";
			if (index==19)	return "RightHandThumb3";
			if (index==20)	return "RightInHandIndex";
			if (index==21)	return "RightHandIndex1";
			if (index==22)	return "RightHandIndex2";
			if (index==23)	return "RightHandIndex3";
			if (index==24)	return "RightInHandMiddle";
			if (index==25)	return "RightHandMiddle1";
			if (index==26)	return "RightHandMiddle2";
			if (index==27)	return "RightHandMiddle3";
			if (index==28)	return "RightInHandRing";
			if (index==29)	return "RightHandRing1";
			if (index==29)	return "RightHandRing1";
			if (index==30)	return "RightHandRing2";
			if (index==31)	return "RightHandRing3";
			if (index==32)	return "RightInHandPinky";
			if (index==33)	return "RightHandPinky1";
			if (index==34)	return "RightHandPinky2";
			if (index==35)	return "RightHandPinky3";
			if (index==36)	return "LeftShoulder";
			if (index==36)	return "LeftShoulder";
			if (index==36)	return "LeftShoulder";
			if (index==36)	return "LeftShoulder";
			if (index==37)	return "LeftArm";
			if (index==38)	return "LeftForeArm";
			if (index==39)	return "LeftHand";
			if (index==40)	return "LeftHandThumb1";
			if (index==41)	return "LeftHandThumb2";
			if (index==42)	return "LeftHandThumb3";
			if (index==43)	return "LeftInHandIndex";
			if (index==44)	return "LeftHandIndex1";
			if (index==45)	return "LeftHandIndex2";
			if (index==46)	return "LeftHandIndex3";
			if (index==47)	return "LeftInHandMiddle";
			if (index==48)	return "LeftHandMiddle1";
			if (index==49)	return "LeftHandMiddle2";
			if (index==50)	return "LeftHandMiddle3";
			if (index==51)	return "LeftInHandRing";
			if (index==52)	return "LeftHandRing1";
			if (index==53)	return "LeftHandRing2";
			if (index==54)	return "LeftHandRing3";
			if (index==55)	return "LeftInHandPinky";
			if (index==56)	return "LeftHandPinky1";
			if (index==57)	return "LeftHandPinky2";
			if (index==58)	return "LeftHandPinky3";
		} else if ( type == 1 ) { // 3D Studio Max biped setup Test
			/// Test to automatically load reference by the default names used with the biped bones tool in 3DS Max.
			/// May not work properly with displacement data because there is no InHand bone for each finger.
			if (index==0)	return "Pelvis";
			if (index==1) 	return "R Thigh";
			if (index==2) 	return "R Calf";
			if (index==3) 	return "R Foot";
			if (index==4) 	return "L Thigh";
			if (index==5) 	return "L Calf";
			if (index==6) 	return "L Foot";
			if (index==7)	return "Spine";
			if (index==8) 	return "Spine1";
			if (index==0) 	return "Spine2";
			if (index==10) 	return "Spine3";
			if (index==11) 	return "Neck";
			if (index==12) 	return "Head";
			if (index==13) 	return "R Clavicle";
			if (index==14) 	return "R UpperArm";
			if (index==15) 	return "R Forearm";
			if (index==16) 	return "R Hand";
			if (index==17) 	return "R Finger0";
			if (index==18) 	return "R Finger01";
			if (index==19) 	return "R Finger02";
			// no InHand bone for id 20 with 3ds max setup
			if (index==21) 	return "R Finger1";
			if (index==22) 	return "R Finger11";
			if (index==23) 	return "R Finger12";
			// no InHand bone for id 24 with 3ds max setup
			if (index==25) 	return "R Finger2";
			if (index==26) 	return "R Finger21";
			if (index==27) 	return "R Finger22";
			// no InHand bone for id 28 with 3ds max setup
			if (index==29) 	return "R Finger3";
			if (index==30) 	return "R Finger31";
			if (index==31) 	return "R Finger32";
			// no InHand bone for id 32 with 3ds max setup
			if (index==33) 	return "R Finger4";
			if (index==34) 	return "R Finger41";
			if (index==35) 	return "R Finger42";
			if (index==36) 	return "L Clavicle";
			if (index==37) 	return "L UpperArm";
			if (index==38) 	return "L Forearm";
			if (index==39) 	return "L Hand";
			if (index==40) 	return "L Finger0";
			if (index==41) 	return "L Finger01";
			if (index==42) 	return "L Finger02";
			// no InHand bone for id 43 with 3ds max setup
			if (index==44) 	return "L Finger1";
			if (index==45) 	return "L Finger11";
			if (index==46) 	return "L Finger12";
			// no InHand bone for id 47 with 3ds max setup
			if (index==48) 	return "L Finger2";
			if (index==49) 	return "L Finger21";
			if (index==50) 	return "L Finger22";
			// no InHand bone for id 51 with 3ds max setup
			if (index==52) 	return "L Finger3";
			if (index==53) 	return "L Finger31";
			if (index==54) 	return "L Finger32";
			// no InHand bone for id 55 with 3ds max setup
			if (index==56) 	return "L Finger4";
			if (index==57) 	return "L Finger41";
			if (index==58) 	return "L Finger42";
		} else {
			Debug.Log("<color=red>[ERROR] No naming setup found for the received input type id: </color>" + type);
		}



		// if we reached this low it means our input was wrong. Return with that info.

		return "INVALID_INPUT";
	}
}
