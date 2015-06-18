using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class CameraSynchronizer : ObjectSynchronizer {
	[DllImport( "RTSIPlugin" )]
	private static extern void LoadConfig( [MarshalAs(UnmanagedType.LPStr)]string config );
	
	[DllImport( "RTSIPlugin" )]
	private static extern bool CheckMaster();
	
	[DllImport( "RTSIPlugin" )]
	private static extern float GetXAngle();
	
	[DllImport( "RTSIPlugin" )]
	private static extern float GetYAngle();
	
	[DllImport( "RTSIPlugin" )]
	private static extern float GetZAngle();
	 
	[DllImport( "RTSIPlugin" )]
	private static extern bool Sync();
	
	public string config_file = "rtsi.xml";
	
	private float xAngle;
	private float yAngle;
	private float zAngle;
	
	public bool EnableRTSI = true;
	
	public static bool staticEnable;
	private static bool isMaster;
	private static List<ObjectSynchronizer> objs;
	
	virtual public void Awake()
	{
		staticEnable = EnableRTSI && enabled;
		if( staticEnable )
		{
			LoadConfig( config_file );
			isMaster = CheckMaster();
		}
		else
		{
			isMaster = true;
		}
		
		objs = new List<ObjectSynchronizer>();
	}
	
	public override void Start()
	{
		if( staticEnable )
		{
			base.Start();
			
			xAngle = GetXAngle();
			yAngle = GetYAngle();
			zAngle = GetZAngle();
			
			if( isMaster )
				transform.Rotate( xAngle, yAngle, zAngle );
		}
	}
	
	public virtual  void LateUpdate()
	{		
		if( staticEnable )
		{
			//GameObject player = GameObject.Find( "Player" );
			//RtsiIO rtsiIO = (RtsiIO) player.GetComponent( typeof(RtsiIO) );
			
			if( isMaster )
			{
				//rtsiIO.UpdateVariables();
				
				transform.Rotate( -xAngle, -yAngle, -zAngle );
				foreach( ObjectSynchronizer obj in objs )
				{
					obj.WriteVariables();
				}
			}
			
			if( !Sync() )
			{
				Screen.SetResolution( 800, 800, false );
				Application.Quit();
			}
			
			if( !isMaster )
			{
				foreach( ObjectSynchronizer obj in objs )
				{
					obj.ReadVariables();
				}
			}
			
			transform.Rotate( xAngle, yAngle, zAngle );
			
		}
	}
	
	public static bool IsEnabled()
	{
		return staticEnable;
	}
	
	public static bool IsMaster()
	{
		return isMaster;
	}
	
	public static void AddObject( ObjectSynchronizer obj )
	{
		objs.Add( obj );
	}
}
