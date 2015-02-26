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
/// This class provides an interface to alter an existing PNSkeletonMotion setup.
/// </summary>
public class PNEditorTools : EditorWindow {
	public PNSkeletonMotion TargetScript;
	public bool AddBoneLines = false;
	public bool AddColliders = true;
	public Color BoneColor = Color.blue;
	public Color BoneTargetColor = Color.red;
	public float BoneLineStartSize = 0.03f;
	public float BoneLineEndSize = 0.008f;

	private Vector2 scrollPosition = new Vector2(0,0);
	
	[MenuItem ("Perception Neuron/2. Editor Tools")]

	static void Init () {
		// Get existing open window or if none, make a new one:
		PNEditorTools window = (PNEditorTools)EditorWindow.GetWindow (typeof (PNEditorTools));
		window.Show ();
	}
	
	void OnGUI () {
		float wFix = 0.5f;
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height-20));

		GUILayout.Label ("Skeleton Tools", EditorStyles.boldLabel);
		GUILayout.Label("Various helper functions to add or remove components from every bone. \n", EditorStyles.wordWrappedMiniLabel);

		// copy colliders
		GUILayout.BeginHorizontal ();		
		GUILayout.Label("Target Object:");
		TargetScript = EditorGUILayout.ObjectField (TargetScript, typeof(PNSkeletonMotion), true, GUILayout.Width(Screen.width*wFix) ) as PNSkeletonMotion;
		GUILayout.EndHorizontal();

		if (TargetScript) {
			// COLLIDERS :
			GUILayout.Label("\nCOLLIDER Settings:", EditorStyles.boldLabel);
			if (GUILayout.Button ("Show/Hide all Colliders") ) {
				PNUtilities.ToggleColliders(TargetScript.Skeleton[0].gameObject);
			}
			if (GUILayout.Button ("Add Default Colliders") ) {
				GameObject colliders = Resources.LoadAssetAtPath("Assets/PerceptionNeuron/Resources/DefaultColliderSetup.prefab", typeof(GameObject)) as GameObject;
				if (colliders == null) {
					Debug.Log("<color=red>[ERROR] Did not find the file 'DefaultColliderSetup.prefab' in (Assets/PerceptionNeuron/Resources/)! Did you move or rename the file/folder?</color>");
					return;
				}
				PNUtilities.CopyColliders(colliders, TargetScript.Skeleton);
			}
			if (GUILayout.Button ("Remove all Box and Capsule Colliders") ) {
				PNUtilities.RemoveColliders(TargetScript.Skeleton[0].gameObject);
			}
			
			// RIGIDBOY:
			GUILayout.Label("\nRIGIDBODY Settings:", EditorStyles.boldLabel);
			if (GUILayout.Button ("Add RigidBody Components") ) {
				PNUtilities.AttachRigidbodies(TargetScript.Skeleton);
				//if (!TargetScript.DirectMotion) PNUtilities.AttachRigidbodies(TargetScript.TargetSkeleton);
			}
			if (GUILayout.Button ("Remove all RigidBody Components") ) {
				PNUtilities.RemoveRigidbodies(TargetScript.Skeleton[0].gameObject);
				if (!TargetScript.DirectMotion) PNUtilities.RemoveRigidbodies(TargetScript.TargetSkeleton[0].gameObject);
			}

			//BONE LINES
			GUILayout.Label("\nBONE LINES Settings:", EditorStyles.boldLabel);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Create Bone Lines?");
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

				if (GUILayout.Button ("Create Bone Lines") ) {
					PNUtilities.CreateBoneLines(TargetScript.Skeleton, TargetScript.UsedSkeletonNamePrefix, BoneLineStartSize, BoneLineEndSize, BoneColor, 10);
					if (!TargetScript.DirectMotion) PNUtilities.CreateBoneLines(TargetScript.TargetSkeleton, "Robot_", BoneLineStartSize, BoneLineEndSize, BoneTargetColor, 0);
				}
			}
			if (GUILayout.Button ("Show/Hide all Bone Lines") ) {
				PNUtilities.ToggleBoneLines(TargetScript.Skeleton[0].root.gameObject);
			}
			if (GUILayout.Button ("Remove all Bone Lines in the target GameObject") ) {
				PNUtilities.RemoveBoneLines(TargetScript.Skeleton[0].root.gameObject);
			}
		}
		GUILayout.EndScrollView();
	}


	void OnInspectorUpdate() {
		this.Repaint();
	}
}
