using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class NetworkInput : MonoBehaviour {
	[DllImport( "RTSIPlugin" )]
	protected static extern IntPtr RegisterSubscriberInt( [MarshalAs(UnmanagedType.LPStr)]string variable_name );
	
	[DllImport( "RTSIPlugin" )]
	protected static extern IntPtr RegisterSubscriberFloat( [MarshalAs(UnmanagedType.LPStr)]string variable_name );
	
	[DllImport( "RTSIPlugin" )]
	protected static extern int GetValueInt( IntPtr variable );
	
	[DllImport( "RTSIPlugin" )]
	protected static extern float GetValueFloat( IntPtr variable );
	
	public enum ButtonState { PRESSED, RELEASED };
	public enum SensorType { BUTTON, AXIS };
	public enum TrackableType { ALL, ONLY_POSITION, ONLY_ORIENTATION };
	
	[System.Serializable]
	public class Sensor
	{
		public string name;
		public string positiveButton;
		public string negativeButton;
		public SensorType type = SensorType.BUTTON;
		
		[System.NonSerialized]
		public IntPtr positivePointer;
		[System.NonSerialized]
		public  IntPtr negativePointer;
		[System.NonSerialized]
		public ButtonState positiveCurrentState;
		[System.NonSerialized]
		public ButtonState negativeCurrentState;
		[System.NonSerialized]
		public ButtonState positiveLastState;
		[System.NonSerialized]
		public ButtonState negativeLastState;
		[System.NonSerialized]
		public int positiveValue;
		[System.NonSerialized]
		public int negativeValue;
	}
	
	[System.Serializable]
	public class Trackable
	{
		public string name;
		public GameObject trackableObject;
		public TrackableType transformation = NetworkInput.TrackableType.ALL;
		public Boolean localTransformation = false;
		
		// posição do tracker
		[System.NonSerialized]
		public IntPtr pX;
		[System.NonSerialized]
		public IntPtr pY;
		[System.NonSerialized]
		public IntPtr pZ;
		// orientação do tracker
		[System.NonSerialized]
		public IntPtr qX;
		[System.NonSerialized]
		public IntPtr qY;
		[System.NonSerialized]
		public IntPtr qZ;
		[System.NonSerialized]
		public IntPtr qW;
	}
	
	[System.Serializable]
	public class MouseProperties
	{
		public string XAxis;
		public string YAxis;
		
		[System.NonSerialized]
		public IntPtr XAxisPtr;
		[System.NonSerialized]
		public IntPtr YAxisPtr;
	}
	
	public MouseProperties mouse;
	public Trackable[] trackables;
	public Sensor[] sensors;
	
	static private Dictionary<string, Sensor> sensorsDict = new Dictionary<string,Sensor>();
	static private Dictionary<string, Trackable> trackablesDict = new Dictionary<string,Trackable>();
	static private bool updated;
	static public Vector3 mousePosition = Vector3.zero;

	// Use this for initialization
	void Start () {
		
		updated = true;
		foreach( Sensor sensor in sensors )
		{
			try
			{
				sensorsDict.Add(sensor.name, sensor);
				
				if( sensor.positiveButton.Length != 0 )
				{
					sensor.positivePointer = RegisterSubscriberInt( sensor.positiveButton );
				}
				
				if( sensor.negativeButton.Length != 0 )
				{
					sensor.negativePointer = RegisterSubscriberInt( sensor.negativeButton );
				}
				
				
				sensor.positiveLastState = ButtonState.RELEASED;
				sensor.positiveCurrentState = ButtonState.RELEASED;
				sensor.positiveValue = 0;
					
					
				sensor.negativeLastState = ButtonState.RELEASED;
				sensor.negativeCurrentState = ButtonState.RELEASED;
				sensor.negativeValue = 0;
			}
			catch( ArgumentException )
			{
				Debug.Log( "Sensor with name " + sensor.name + " added twice." );
			}
		}
		
		foreach( Trackable trackable in trackables )
		{
			try
			{
				trackablesDict.Add( trackable.name, trackable );
				
				if( trackable.transformation == TrackableType.ONLY_POSITION || trackable.transformation == TrackableType.ALL )
				{
					trackable.pX = RegisterSubscriberFloat (trackable.name + ".data.x");
					trackable.pY = RegisterSubscriberFloat (trackable.name + ".data.y");
					trackable.pZ = RegisterSubscriberFloat (trackable.name + ".data.z");
				}
				
				if( trackable.transformation == TrackableType.ONLY_ORIENTATION || trackable.transformation == TrackableType.ALL )
				{
					trackable.qX = RegisterSubscriberFloat (trackable.name + ".data.qx");
					trackable.qY = RegisterSubscriberFloat (trackable.name + ".data.qy");
					trackable.qZ = RegisterSubscriberFloat (trackable.name + ".data.qz");
					trackable.qW = RegisterSubscriberFloat (trackable.name + ".data.qw");
				}
			}
			catch( ArgumentException )
			{
				Debug.Log( "Trackable with name " + trackable.name + " added twice." );
			}
		}
	
		if( mouse.XAxis.Length != 0 && mouse.YAxis.Length != 0 )
		{
			mouse.XAxisPtr = RegisterSubscriberFloat( mouse.XAxis );
			mouse.YAxisPtr = RegisterSubscriberFloat( mouse.YAxis );
		}
			
	}
	
	void LateUpdate()
	{
		updated = false;
	}
	
	// Update is called once per frame
	void Update () {	
		
		if( updated ) return;
		
		foreach( Sensor sensor in sensorsDict.Values )
		{
			int positiveValue = 0;
			int negativeValue = 0;
			
			if( sensor.type == SensorType.BUTTON )
			{
			
				if( sensor.positiveButton.Length != 0 )
				{
					positiveValue = GetValueInt(sensor.positivePointer);
					if( positiveValue != 0 && positiveValue != 1 )
						positiveValue = 0;
				}
			
				if( sensor.negativeButton.Length != 0 )
				{
					negativeValue = GetValueInt(sensor.negativePointer);
					
					if( negativeValue != 0 && negativeValue != 1 )
						negativeValue = 0;
				}
			}
			
			sensor.positiveLastState = sensor.positiveCurrentState;
			sensor.negativeLastState = sensor.negativeCurrentState;
			
			sensor.positiveCurrentState = positiveValue == 0 ? ButtonState.RELEASED : ButtonState.PRESSED;
			sensor.negativeCurrentState = negativeValue == 0 ? ButtonState.RELEASED : ButtonState.PRESSED;
			
			sensor.positiveValue = positiveValue;
			sensor.negativeValue = negativeValue;
		}
		
		foreach( Trackable trackable in trackablesDict.Values )
		{
			if( trackable.transformation == TrackableType.ONLY_POSITION || trackable.transformation == TrackableType.ALL )
			{
				Vector3 position = new Vector3(GetValueFloat( trackable.pX ), GetValueFloat( trackable.pY ), -GetValueFloat( trackable.pZ ));
				if( trackable.localTransformation )
				{
					trackable.trackableObject.transform.localPosition = position;
				}
				else
				{
					trackable.trackableObject.transform.position = position;
				}
			}
			
			if( trackable.transformation == TrackableType.ONLY_ORIENTATION || trackable.transformation == TrackableType.ALL )
			{
				Quaternion rotation = new Quaternion( -GetValueFloat( trackable.qX ), -GetValueFloat( trackable.qY ), GetValueFloat( trackable.qZ ), GetValueFloat( trackable.qW ) );
				if( trackable.localTransformation )
				{
					trackable.trackableObject.transform.localRotation = rotation;
				}
				else
				{
					trackable.trackableObject.transform.rotation = rotation;
				}
			}
		}
		
		if( mouse.XAxis.Length != 0 && mouse.YAxis.Length != 0 )
		{
			float mouseX = GetValueFloat( mouse.XAxisPtr );
			float mouseY = GetValueFloat( mouse.YAxisPtr );
			
			if( mouseX < -1.0f || mouseX > 1.0f )
				mouseX = 0.0f;
			
			if( mouseY < -1.0f || mouseY > 1.0f )
				mouseX = 0.0f;
			
			mouseX = (1.0f + mouseX) / 2.0f * Screen.width;
			mouseY = (1.0f + mouseY) / 2.0f * Screen.height;
			
			mousePosition.x = mouseX;
			mousePosition.y = mouseY;
		}
		
		updated = true;
	}
	
	static public int GetAxis( string axisName )
	{
		try
		{
			return sensorsDict[axisName].positiveValue + (-sensorsDict[axisName].negativeValue);
		}
		catch(KeyNotFoundException)
		{
		}
			
		return 0;
	}
	
	static public bool GetButton( string buttonName )
	{
		try
		{
			return sensorsDict[buttonName].positiveCurrentState == ButtonState.PRESSED;
		}
		catch( KeyNotFoundException )
		{
		}
		
		return false;
	}
	
	static public bool GetButtonDown( string buttonName )
	{
		try
		{
			return sensorsDict[buttonName].positiveLastState == ButtonState.RELEASED && sensorsDict[buttonName].positiveCurrentState == ButtonState.PRESSED;
		}
		catch(KeyNotFoundException)
		{
		}
		
		return false;
	}
	
	static public bool GetButtonUp( string buttonName )
	{
		try
		{
			return sensorsDict[buttonName].positiveLastState == ButtonState.PRESSED && sensorsDict[buttonName].positiveCurrentState == ButtonState.RELEASED;
		}
		catch(KeyNotFoundException)
		{
		}
		
		return false;
	}
}
