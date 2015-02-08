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
using UnityEditor;
using System.Collections;

/// <summary>
/// This class provides an interface to setup a character so it can be used with Perception Neuron.
/// </summary>
public class PNEditorSetupTool : EditorWindow {
	public PNSkeletonMotion TargetScript;
	public string prefix = "Robot_";
	public Transform Skeleton_Root;
	public Transform Skeleton_Target_Root;
	public bool AddBoneLines = false;
	public bool AddColliders = true;
	public bool AddRigidbodies = true;
	public Color BoneColor = Color.blue;
	public Color BoneTargetColor = Color.red;
	public float BoneLineStartSize = 0.03f;
	public float BoneLineEndSize = 0.008f;
	private Vector2 scrollPosition = new Vector2(0,0);

	[MenuItem ("Perception Neuron/1. Skeleton Setup Tool")]

	static void Init () {
		// Get existing open window or if none, make a new one:
		PNEditorSetupTool window = (PNEditorSetupTool)EditorWindow.GetWindow (typeof (PNEditorSetupTool));
		window.Show ();
	}
	
	void OnGUI () {
		float wFix = 0.5f;
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height-20));

		GUILayout.Label ("Skeleton Setup Settings", EditorStyles.boldLabel);
		GUILayout.Label("This editor tool will help you to setup a default skeleton to be used with your Perception Neuron system. " +
			"You can use it with your own character model or with the example characters provided in this SDK. \n" +
			"How to use: Simply select the object that contains the " +
			"PNSkeletonMotion.cs script, adjust what settings you want to use, and then hit the button. \n" +
			"Please check the documentaiton if you run into any problems. \n", EditorStyles.wordWrappedMiniLabel);

		GUILayout.BeginHorizontal ();		
		GUILayout.Label("Target Body Script:");
		TargetScript = EditorGUILayout.ObjectField (TargetScript, typeof(PNSkeletonMotion), true, GUILayout.Width(Screen.width*wFix) ) as PNSkeletonMotion;
		GUILayout.EndHorizontal();

		if (TargetScript) {
			GUILayout.BeginHorizontal ();		
			GUILayout.Label("Skeleton Root Object above the hips:");
			Skeleton_Root = EditorGUILayout.ObjectField (Skeleton_Root, typeof(Transform), true, GUILayout.Width(Screen.width*wFix) ) as Transform;
			GUILayout.EndHorizontal();

			if (Skeleton_Root) {
				GUILayout.BeginHorizontal();
				GUILayout.Label("Bone Names prefix: ");
				prefix = EditorGUILayout.TextField(prefix, GUILayout.Width(Screen.width*wFix) );
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
				GUILayout.Label("Add colliders?");
				AddColliders = EditorGUILayout.Toggle(AddColliders, GUILayout.Width(Screen.width*wFix) );
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("Add RigidBody Components?");
				AddRigidbodies = EditorGUILayout.Toggle(AddRigidbodies, GUILayout.Width(Screen.width*wFix) );
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
				GUILayout.Label("Use direct motion?");
				TargetScript.DirectMotion = EditorGUILayout.Toggle(TargetScript.DirectMotion, GUILayout.Width(Screen.width*wFix) );
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
				GUILayout.Label("Add Bone Lines?");
				AddBoneLines = EditorGUILayout.Toggle(AddBoneLines, GUILayout.Width(Screen.width*wFix) );
				GUILayout.EndHorizontal();

				if (AddBoneLines) {
					GUILayout.BeginHorizontal();
					GUILayout.Label("Bone Color: ");
					BoneColor = EditorGUILayout.ColorField(BoneColor, GUILayout.Width(Screen.width*wFix) );
					GUILayout.EndHorizontal();
					
					GUILayout.BeginHorizontal();
					GUILayout.Label("Target Bones Color: ");
					BoneTargetColor = EditorGUILayout.ColorField(BoneTargetColor, GUILayout.Width(Screen.width*wFix) );
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("Bone Line Start Size: ");
					BoneLineStartSize = EditorGUILayout.Slider(BoneLineStartSize,0.001f, 0.1f, GUILayout.Width(Screen.width*wFix) );
					GUILayout.EndHorizontal();
					
					GUILayout.BeginHorizontal();
					GUILayout.Label("Bone Line End Size: ");
					BoneLineEndSize = EditorGUILayout.Slider(BoneLineEndSize,0.001f, 0.1f, GUILayout.Width(Screen.width*wFix) );
					GUILayout.EndHorizontal();

				}

				if (GUILayout.Button ("Prepare Bones") ) {
					if ( Skeleton_Root == null ) {
						Debug.Log("The Root Object can not be empty! Please set it first to the GameObject above the Hips Object.");
						return;
					}

					Debug.Log("<color=green>[PERCEPTION NEURON]</color> Load target skeleton...");
					// if we created another target bone setup before then delete it
					if (TargetScript.SavedSkeletonTargetRoot != null) {
						DestroyImmediate(TargetScript.SavedSkeletonTargetRoot);
					}

					GameObject clone = null;

					// instantiate the target bones skeleton
					if (!TargetScript.DirectMotion) {
						GameObject targetSkeletonPrefab = Resources.LoadAssetAtPath("Assets/PerceptionNeuron/Resources/TargetMotionSkeleton.prefab", typeof(GameObject)) as GameObject;
						if (targetSkeletonPrefab == null) {
							Debug.Log("<color=red>[ERROR] Did not find the file 'TargetMotionSkeleton.prefab' in (Assets/PerceptionNeuron/Resources/)! Did you move or rename the file/folder?</color>");
							return;
						}
						clone = Instantiate(targetSkeletonPrefab, Skeleton_Root.position, Skeleton_Root.rotation) as GameObject;
						TargetScript.SavedSkeletonTargetRoot = clone;
					}

					Debug.Log("<color=green>[PERCEPTION NEURON]</color> Load skeleton reference for current Skeleton...");
					PNUtilities.LoadSkeletonReference(TargetScript.Skeleton, Skeleton_Root, prefix, 0);
					if (!TargetScript.DirectMotion) {
						Debug.Log("<color=green>[PERCEPTION NEURON]</color> Load skeleton reference for Target Skeleton...");
						PNUtilities.LoadSkeletonReference(TargetScript.TargetSkeleton, clone.transform, "Robot_", 0);
					}

					TargetScript.UsedSkeletonNamePrefix = prefix; // Save the prefix we used for this character

					if (AddBoneLines) {
						Debug.Log("<color=green>[PERCEPTION NEURON]</color> Create bone lines for the current skeleton...");
						PNUtilities.CreateBoneLines(TargetScript.Skeleton, prefix, BoneLineStartSize, BoneLineEndSize, BoneColor, 1);
						if (!TargetScript.DirectMotion) {
							Debug.Log("<color=green>[PERCEPTION NEURON]</color> Create bone lines for the target skeleton...");
							PNUtilities.CreateBoneLines(TargetScript.TargetSkeleton, "Robot_", BoneLineStartSize, BoneLineEndSize, BoneTargetColor, 0);
						}
					}

					if (AddRigidbodies) {
						Debug.Log("<color=green>[PERCEPTION NEURON]</color> Add RigidBody components to the skeleton...");
						PNUtilities.AttachRigidbodies (TargetScript.Skeleton);
						//if (!TargetScript.DirectMotion) {
						//	PNUtilities.AttachRigidbodies (TargetScript.TargetSkeleton);
						//}
					}

					if (AddColliders) {
						Debug.Log("<color=green>[PERCEPTION NEURON]</color> Load the default collider setup on the current skeleton...");
						//prefab = Resources.LoadAssetAtPath<GameObject>("Assets/Artwork/mymodel.fbx");
						GameObject colliders = Resources.LoadAssetAtPath("Assets/PerceptionNeuron/Resources/DefaultColliderSetup.prefab", typeof(GameObject)) as GameObject;
						PNUtilities.CopyColliders(colliders, TargetScript.Skeleton);
					}

					
					if (!TargetScript.DirectMotion) clone.transform.parent = Skeleton_Root.root;
					Resources.UnloadUnusedAssets();
				}
			}
		}
		GUILayout.EndScrollView();
	}

	void OnInspectorUpdate() {
		this.Repaint();
	}
}
