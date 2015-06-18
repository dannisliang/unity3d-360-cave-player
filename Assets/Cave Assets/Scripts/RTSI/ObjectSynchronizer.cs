using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class ObjectSynchronizer : MonoBehaviour {
	[DllImport( "RTSIPlugin" )]
	protected static extern IntPtr RegisterSubscriberFloat( [MarshalAs(UnmanagedType.LPStr)]string variable_name );
	
	[DllImport( "RTSIPlugin" )]
	protected static extern IntPtr RegisterPublisherFloat( [MarshalAs(UnmanagedType.LPStr)]string variable_name );
	
	[DllImport( "RTSIPlugin" )]
	protected static extern float GetValueFloat( IntPtr variable );
	
	[DllImport( "RTSIPlugin" )]
	protected static extern void SetValueFloat( IntPtr variable, float value );
	
	
	private IntPtr rX;
	private IntPtr rY;
	private IntPtr rZ;
	private IntPtr rW;
	private IntPtr tX;
	private IntPtr tY;
	private IntPtr tZ;
	
	
	// Use this for initialization
	public virtual void Start () {
		
		if( CameraSynchronizer.IsEnabled() )
		{
			if( !CameraSynchronizer.IsMaster() )
			{
				rX = RegisterSubscriberFloat( gameObject.name + ".rx" );
				rY = RegisterSubscriberFloat( gameObject.name + ".ry" );
				rZ = RegisterSubscriberFloat( gameObject.name + ".rz" );
				rW = RegisterSubscriberFloat( gameObject.name + ".rw" );
				
				tX = RegisterSubscriberFloat( gameObject.name + ".tx" );
				tY = RegisterSubscriberFloat( gameObject.name + ".ty" );
				tZ = RegisterSubscriberFloat( gameObject.name + ".tz" );
				
			}
			else
			{			
				rX = RegisterPublisherFloat( gameObject.name + ".rx" );
				rY = RegisterPublisherFloat( gameObject.name + ".ry" );
				rZ = RegisterPublisherFloat( gameObject.name + ".rz" );
				rW = RegisterPublisherFloat( gameObject.name + ".rw" );
				
				tX = RegisterPublisherFloat( gameObject.name + ".tx" );
				tY = RegisterPublisherFloat( gameObject.name + ".ty" );
				tZ = RegisterPublisherFloat( gameObject.name + ".tz" );
			}
			
			CameraSynchronizer.AddObject( this );
		}
	}
	
	public virtual void WriteVariables()
	{
		SetValueFloat( rX, transform.rotation.x );
		SetValueFloat( rY, transform.rotation.y );
		SetValueFloat( rZ, transform.rotation.z );
		SetValueFloat( rW, transform.rotation.w );
		
		SetValueFloat( tX, transform.position.x );
		SetValueFloat( tY, transform.position.y );
		SetValueFloat( tZ, transform.position.z );
	}
	
	public virtual void ReadVariables()
	{
		float rx = GetValueFloat( rX );
		float ry = GetValueFloat( rY );
		float rz = GetValueFloat( rZ );
		float rw = GetValueFloat( rW );
		
		float tx = GetValueFloat( tX );
		float ty = GetValueFloat( tY );
		float tz = GetValueFloat( tZ );
		
		Quaternion orientation = new Quaternion( rx, ry, rz, rw );
		
		transform.rotation = orientation;
		transform.position = new Vector3( tx, ty, tz );
	}
}
