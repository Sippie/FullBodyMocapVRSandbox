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

[ExecuteInEditMode]

/// <summary>
/// Updates the line renderer start and end points we're using to show the skeleton bone setup.
/// </summary>
public class BoneVisualizer : MonoBehaviour {
	public Transform StartPoint;
	public Transform EndPoint;
	private LineRenderer myLR;

	void Awake() {
		myLR = GetComponent<LineRenderer> ();
	}

	void Update () {
		// update as fast as our frame rate is!
		UpdatePoints ();
	
	}

	/// <summary>
	/// Update the line renderer start and end points.
	/// </summary>
	private void UpdatePoints() {
		if (myLR != null && myLR.enabled) {
			if(StartPoint) myLR.SetPosition(0, StartPoint.position);
			if(EndPoint) myLR.SetPosition(1, EndPoint.position);
			
		}
	}
}
