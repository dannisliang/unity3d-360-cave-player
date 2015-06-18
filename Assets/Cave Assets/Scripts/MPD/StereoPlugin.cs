using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class StereoPlugin : MonoBehaviour {
	
	[DllImport("MPDUnityPlugin")]
	private static extern void SetBackLeftBuffer();
	
	[DllImport("MPDUnityPlugin")]
	private static extern void SetBackRightBuffer();
	
	[DllImport("MPDUnityPlugin")]
	private static extern bool EnableStereo();
	
	private float depth;
	static bool left = false;
	
	private static bool enableStereo;
	private Matrix4x4 rightFrustum;
	private Matrix4x4 leftFrustum;
	private Matrix4x4 originalMatrix;
	
	public float screenProjectionPlane;
	public float intraocularDistance;
	
	void Start()
	{
		enableStereo = EnableStereo();
		UpdateStereoFrustum();
	}
	
	void UpdateStereoFrustum()
	{
		if( enableStereo )
		{
			float fLeft;
			float fRight;
			float fTop;
			float fBottom;
			
			originalMatrix = new Matrix4x4();
			originalMatrix = camera.projectionMatrix;
			
			float x = originalMatrix[0,0];
			float y = originalMatrix[1,1];
			float a = originalMatrix[0,2];
			float b = originalMatrix[1,2];
			
			float A = (2 * camera.nearClipPlane) / x;
			float B = (2 * camera.nearClipPlane) / y;
			
			fTop = ( (1 + b) * B ) / 2;
			fRight = ( (1 + a) * A ) / 2;
			fBottom = ( (b - 1) * B ) / 2;
			fLeft = ( (a - 1) * A ) / 2;
			
			float frustumShit = ( intraocularDistance / 2 ) * camera.nearClipPlane / screenProjectionPlane;
			depth = intraocularDistance / 2.0F;
			
			rightFrustum = PerspectiveOffCenter( fLeft - frustumShit, fRight - frustumShit, fBottom, fTop, camera.nearClipPlane, camera.farClipPlane );
			leftFrustum = PerspectiveOffCenter( fLeft + frustumShit, fRight + frustumShit, fBottom, fTop, camera.nearClipPlane, camera.farClipPlane );
		}
	}
	
	void OnPreRender()
	{
		if( enableStereo )
		{
			if( left == false  )
			{
				transform.Translate( -depth, 0.0F, 0.0F );
				camera.projectionMatrix = leftFrustum;
				SetBackLeftBuffer();
			}
			else
			{
				transform.Translate( depth, 0.0F, 0.0F );
				camera.projectionMatrix = rightFrustum; 
				SetBackRightBuffer();
			}
		}
	}
	
	void OnPostRender() 
	{
		if( enableStereo )
		{
			if( left == false )
			{
				transform.Translate( depth, 0, 0 );
			}
			else
			{
				transform.Translate(-depth,0,0 );
			}
			camera.projectionMatrix = originalMatrix;
			left = !left;
		}
	}
	
	static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far) 
	{
		float x = ( 2.0F * near ) / ( right - left );
		float y = ( 2.0F * near ) / ( top - bottom );
		float a = ( right + left ) / ( right - left );
		float b = ( top + bottom ) / ( top - bottom );
		float c = -( (far + near ) / ( far - near ) );
		float d = -( ( 2.0F * far * near ) / ( far - near ) );
		float e = -1.0F;
		Matrix4x4 m = new Matrix4x4();
		m[0, 0] = x;
		m[0, 1] = 0;
		m[0, 2] = a;
		m[0, 3] = 0;
		m[1, 0] = 0;
		m[1, 1] = y;
		m[1, 2] = b;
		m[1, 3] = 0;
		m[2, 0] = 0;
		m[2, 1] = 0;
		m[2, 2] = c;
		m[2, 3] = d;
		m[3, 0] = 0;
		m[3, 1] = 0;
		m[3, 2] = e;
		m[3, 3] = 0;
		return m;
}

}

