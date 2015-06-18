using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class MPDPlugin : MonoBehaviour {
	
	[DllImport("MPDUnityPlugin")]
	private static extern int GetMaxResolutionWidth();
	
	[DllImport("MPDUnityPlugin")]
	private static extern int GetMaxResolutionHeight();
	
	[DllImport("MPDUnityPlugin")]
	private static extern bool EnableMPD( [MarshalAs(UnmanagedType.LPStr)]string config );
	
	[DllImport("MPDUnityPlugin")]
	private static extern float GetDisplayLimitBottom();
	
	[DllImport("MPDUnityPlugin")]
	private static extern float GetDisplayLimitLeft();
	
	[DllImport("MPDUnityPlugin")]
	private static extern float GetDisplayLimitTop();
	
	[DllImport("MPDUnityPlugin")]
	private static extern float GetDisplayLimitRight();
	//private static extern void GetDisplayLimits( [MarshalAs(UnmanagedType.R4)]float bottom, [MarshalAs(UnmanagedType.R4)]float left, [MarshalAs(UnmanagedType.R4)]float top, [MarshalAs(UnmanagedType.R4)]float right );
	
	[DllImport("MPDUnityPlugin")]
	private static extern bool IsCalibrating();
	
	[DllImport("MPDUnityPlugin")]
	private static extern void SetRenderLimits( float bottom, float left, float top, float right );
	
	private static bool mpdStarted = false;
	private static short startCount = 0;
	private static short calibCount = 0;
	
	private bool needUpdateFrustum = false;
	private Matrix4x4 originalFrustum;
	
	public string config = "config.txt";
	
	private float timeElapsed=0;
	
	float gBottom = 0;
	float gLeft = 0;
	float gTop = 1;
	float gRight = 1;
	
	void Start () 
	{
		originalFrustum = new Matrix4x4();
		for( int i=0; i<4; i++ )
		{
			for( int j=0; j<4; j++ )
			{
				originalFrustum[i,j] = camera.projectionMatrix[i,j];
			}
		}
	
		int resolution_width;
		int resolution_height;
		
		resolution_width = GetMaxResolutionWidth();
		resolution_height = GetMaxResolutionHeight();
		
		Screen.SetResolution( resolution_width, resolution_height, true );
		Screen.showCursor = false;
	}
	
	// Update is called once per frame
	void OnPostRender () {

		if( !mpdStarted )
		{
			startCount++;
			
			// init step 1: wait for full-screen
			if( startCount == 2 )
			{
				EnableMPD( config );
			}
			// init step 2: wait for MPD to load completly
			else if( startCount == 4 )
			{
				UpdateFrustum();
				mpdStarted = true;
			}
			
		}
		else
		{
			timeElapsed += Time.deltaTime;
			if( timeElapsed >= 2 )
			{
				UpdateFrustum();
				timeElapsed = 0;
			}
		}
	}
	
	void UpdateFrustum()
	{
		
		float bottom = GetDisplayLimitBottom();
		float left   = GetDisplayLimitLeft();
		float top    = GetDisplayLimitTop();
		float right  = GetDisplayLimitRight();
		
		if( bottom == gBottom && left == gLeft && top == gTop && right == gRight )
			return;
		
		gBottom = bottom;
		gLeft   = left;
		gTop    = top;
		gRight  = right;
		
		float width;
		float height;
		
		float fLeft;
		float fRight;
		float fTop;
		float fBottom;
			
		float x = originalFrustum[0,0];
		float y = originalFrustum[1,1];
		float a = originalFrustum[0,2];
		float b = originalFrustum[1,2];
		
		float A = (2 * camera.nearClipPlane) / x;
		float B = (2 * camera.nearClipPlane) / y;
		
		fTop = ( (1 + b) * B ) / 2;
		fRight = ( (1 + a) * A ) / 2;
		fBottom = ( (b - 1) * B ) / 2;
		fLeft = ( (a - 1) * A ) / 2;
		
		width = fRight - fLeft;
		height = fTop - fBottom;
		
		float nBottom = fBottom + bottom * height;
		float nLeft = fLeft + left * width;
		float nTop = fBottom + top * height;
		float nRight = fLeft + right * width;
		
		SetRenderLimits( bottom, left, top, right );
		
		camera.projectionMatrix = PerspectiveOffCenter(nLeft, nRight, nBottom, nTop, camera.nearClipPlane, camera.farClipPlane );
		
		SendMessage( "UpdateStereoFrustum" );
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
