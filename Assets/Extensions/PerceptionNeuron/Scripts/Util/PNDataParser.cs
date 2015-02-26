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
using System;
using System.Collections;
using PNBVHDataReaderWraper;
using System.Runtime.InteropServices;

public class PNDataParser : MonoBehaviour {
	public bool IsConnected;
	public bool WithDisplacement;
	public bool WithReference;
	public string ServerIP = "127.0.0.1";					// IP of the machine running Perception Axis.
	public string ServerPort = "7001";						// Port to connect to.
	public Vector3[] PositionValues = new Vector3[59];		// The received BVH position values transformed into a list of Vector3's.
	public Vector3[] RotationValues = new Vector3[59];		// The received BVH rotation values trahsformed into a list of Vector3's.
	public bool DEBUG_Enabled = false;
	public float[] DEBUG_ReceivedFrameData = new float[360];

	private string _btnConnectString = "Connect"; 			// Text of the Connect/Disconnect Button.
	private FrameDataReceived _DataReceivedHandle;
	private BvhOutputBinaryHeader _bvhHeader;
	private float[] _valuesBuffer = new float[180];         // Default is 180, will be changed data receiving if needed.
	private int _receivedFramesCounter = 0;

	// callbacks
	private SocketConnectionStatus _OnSocketStatusChanged;
	private FrameDataReceived _OnFrameDataReceived;

	/// <summary>
	/// Runs each time we received a new binary package from the BVHDataReader DLL.
	/// </summary>
	/// <param name="customObject">tba.</param>
	/// <param name="header">tba.</param>
	/// <param name="data">tba.</param>
	private void OnFrameDataReceived(IntPtr customObject, IntPtr header, IntPtr data){
		// read header info
		_bvhHeader = (BvhOutputBinaryHeader)Marshal.PtrToStructure(header, typeof(BvhOutputBinaryHeader));
		
		// show in gui
		WithDisplacement = _bvhHeader.bWithDisp!=0;
		WithReference = _bvhHeader.bWithReference !=0;
		
		// frame counter
		_receivedFramesCounter++;
		
		// Change the buffer length if necessory
		if (_bvhHeader.DataCount != _valuesBuffer.Length)
		{
			//Debug.Log("<color=yellow>Received DataCount was not the same length as the buffer! Changing length to: " + _bvhHeader.DataCount + "</color>");
			_valuesBuffer = new float[_bvhHeader.DataCount];
		}
		
		// read and convert data to values array
		Marshal.Copy(data, _valuesBuffer, 0, (int)_bvhHeader.DataCount);
		
		// Parsing position and rotation data
		ParseBVHData (_valuesBuffer);

		if (DEBUG_Enabled) { 
			_receivedFramesCounter++;
		}
	}

	private void OnSocketStatusChanged(IntPtr customObject, SocketConnectionStatusTypes status, [MarshalAs(UnmanagedType.LPStr)]string msg)
	{
		// status changed
		IsConnected = (status == SocketConnectionStatusTypes.CS_Connected);
		_btnConnectString = IsConnected ? "Disconnect" : "Connect"; 
		Debug.Log(msg);
	}

	/// <summary>
	/// Start the data reader work thread.
	/// </summary>
	void Start () {
		// Socket status handle
		_OnSocketStatusChanged = new SocketConnectionStatus (OnSocketStatusChanged);
		PNDataReader.BRRegisterConnectionStatusCallback(IntPtr.Zero, _OnSocketStatusChanged);
		// Data receive handle
		_OnFrameDataReceived = new FrameDataReceived (OnFrameDataReceived);
		PNDataReader.BRRegisterFrameDataCallback(IntPtr.Zero, _OnFrameDataReceived);


		if (PNDataReader.BRGetConnectionStatus() == SocketConnectionStatusTypes.CS_Connected) {
			PNDataReader.BRDisconnect();
		} else {
			PNDataReader.BRConnectTo(ServerIP, int.Parse(ServerPort));
		}

		// Create parser
	//	_DataReceivedHandle = new FrameDataReceived (DataReceived);
		//PNDataReader.BRRegisterFrameDataCallback(IntPtr.Zero, _DataReceivedHandle);
		//PNDataReader.BRSetOutputDataType(DataBlockTypes.DT_BinaryBlock);
	}

	/// <summary>
	/// Update the connection status if we're connected.
	/// </summary>
	void Update() {
		// Update connection status
		if (PNDataReader.BRGetConnectionStatus () == SocketConnectionStatusTypes.CS_Connected) {
			IsConnected = true;
		} else {
			IsConnected = false;
		}
	}

	/// <summary>
	/// If we quit the application or exit playmode in the editor, then also disconnect the data reader.
	/// </summary>
	void OnApplicationQuit() {
		if (PNDataReader.BRGetConnectionStatus() != SocketConnectionStatusTypes.CS_Disconnected ) {
			// disconnect
			PNDataReader.BRDisconnect();
		}
	}

	/// <summary>
	/// Parse the float values from the data reader into Vector3 lists.
	/// </summary>
	/// <param name="frameData">The foat values received in this binary data stream update.</param>
	private void ParseBVHData(float[] frameData) {
		// load received Data into unity inspector list
		if (DEBUG_Enabled) {
			for (int i=0; i<frameData.Length; i++) {
				DEBUG_ReceivedFrameData[i] = frameData[i];
			}
		}

		// position data is in centimeters, so devide by 100 for Unity's 1 meter scale system
		// rotation data starts with Y rotation for now. Will change later to be selectable inside 
		// first entry is hip position. everything after is euler rotation values
		
		int index = 0; // The index we increase for each parsed update
		int startIndex = 3; // Index position for full data parsing. Set to 3 if the data is without displacement to skip over the hips.
		int BVHStructureLength = 3; // Length for each update. 3 for rotation values only. 6 if we use displacement (positions).
		int BVHReferenceLength = 6;


		if (_bvhHeader.bWithDisp == 1) { // If we use displacement, then also parse position values
			BVHStructureLength = 6;
			startIndex = 0;
			WithDisplacement = true;
		} else {
			WithDisplacement = false;

			// HIPS
			// if we dont use displacement the first entry will be position data and all the rest rotation
			PositionValues [index] = new Vector3 (-frameData [0]/100, frameData [1]/100, frameData [2]/100);
		}

		if (WithReference) { // Increase the start index if we're sending bvh reference prefix
			startIndex += BVHReferenceLength;
		}

		// Put all the values into the correct lists. 
		for (int d=startIndex; d < frameData.Length; d += BVHStructureLength) {
			if (_bvhHeader.bWithDisp == 1) {
				//position
				PositionValues[index] = new Vector3 (-frameData [d]/100, frameData [d+1]/100, frameData [d+2]/100);
				//rotation
				RotationValues[index] = new Vector3(frameData [d+4], -frameData [d+3], -frameData [d+5]);
			} else {
				RotationValues[index] = new Vector3(frameData [d+1], -frameData [d], -frameData [d+2]);
			}
			
			index++;
		}
	}

	/// <summary>
	/// Show some status info and a button to start the data reader.
	/// </summary>
	void OnGUI() {
		int boxW = 200;
		int boxH = 130;
		
		// border
		GUI.Box(new Rect(3,3,boxW,boxH), "BVH Data Reader Settings");
		
		// Server IP
		GUI.Label(new Rect(20,40, 120,20), "Server IP:");
		ServerIP = GUI.TextField (new Rect (90, 40, 90, 20), ServerIP);
		
		// Server Port
		GUI.Label(new Rect(10, 70, 120, 20), "Server Port:");
		ServerPort = GUI.TextField (new Rect (90, 70, 90, 20), ServerPort);
		
		// dinamic set connect button string
		if (PNDataReader.BRGetConnectionStatus() == SocketConnectionStatusTypes.CS_Connected) {
			_btnConnectString = "Disconnect";
		} else {
			_btnConnectString = "Connect";
		}
		
		// connect button
		if (GUI.Button (new Rect (90, 100, 80, 20), _btnConnectString)) {
			if (PNDataReader.BRGetConnectionStatus() == SocketConnectionStatusTypes.CS_Connected) {
				PNDataReader.BRDisconnect();
			} else {
				PNDataReader.BRConnectTo(ServerIP, int.Parse(ServerPort));
			}
		}
		
		// Show debug inf:
		string ds1 = "Connection Status: \n";
		if (PNDataReader.BRGetConnectionStatus() == SocketConnectionStatusTypes.CS_Connected) {
			ds1 += "Connected to: " + ServerIP.ToString() + ":" +ServerPort.ToString();
		} else if (PNDataReader.BRGetConnectionStatus() == SocketConnectionStatusTypes.CS_Disconnected) {
			ds1 += "Disconnected";
		} else {
			ds1 += "Connecting to: " + ServerIP + ":" +ServerPort;
		}
		GUI.color = Color.red;
		GUI.Label(new Rect(boxW + 10,5, Screen.width - 40,50), ds1);
		
		if (DEBUG_Enabled && PNDataReader.BRGetConnectionStatus() != SocketConnectionStatusTypes.CS_Disconnected ) {
			string ds2 = "BVH Data info: \n";
			ds2 += 	"With Displacement: " + WithDisplacement + "\n" +
				"With Reference: " + WithReference + "\n" +
					"Data Length: " + _bvhHeader.DataCount + "\n" +
					"Received Frames: " + _receivedFramesCounter;
			GUI.color = Color.yellow;
			GUI.Label(new Rect(boxW + 10,55, Screen.width - 40, Screen.height - boxH), ds2);
		}
	}

}
