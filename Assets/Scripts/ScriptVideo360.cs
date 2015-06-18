using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent (typeof (AudioSource))]
public class ScriptVideo360 : MonoBehaviour
{
	public Camera cam;
	public bool debugEnabled;
	public Text debugText;
	public GameObject canvas;

//	string url = "http://www.unity3d.com/webplayers/Movie/sample.ogg";
//	string url = "http://10.0.200.241:8080/panoramic.ogg";
	string url = "file:///D:/Videos360/video360.ogg";
	MovieTexture sphereTexture;

	void Awake ()
	{
		cam.aspect = 1f;
		cam.fieldOfView = 90f;

		canvas.SetActive (debugEnabled);
	}

	void Start ()
	{
		StartCoroutine (LoadMovie (@url));
//		LoadFromResource ();
	}

	IEnumerator LoadMovie (string url)
	{
		debugText.text += "\nLoading...";

		WWW videoStream = new WWW (url);
		sphereTexture = videoStream.movie;
		GetComponent <AudioSource>().clip = videoStream.audioClip;

		float startLoadTime = Time.timeSinceLevelLoad;

		while (!sphereTexture.isReadyToPlay)
		{
			if (!string.IsNullOrEmpty(videoStream.error))
			{
				debugText.text += videoStream.error;
				return false;
			}
			yield return 0;
		}
		debugText.text += "\nTime " + (Time.timeSinceLevelLoad - startLoadTime).ToString ();
		debugText.text += "\nReady!";

		GetComponent<Renderer> ().material.mainTexture = sphereTexture;
		GetComponent<AudioSource> ().Play ();
		sphereTexture.Play ();
		sphereTexture.loop = true;
	}

//	void LoadFromResource ()
//	{
//		MovieTexture texture = (MovieTexture)Resources.Load ("sample", typeof(MovieTexture));
//		GetComponent <Renderer>().material.mainTexture =  texture;
//		texture.Play ();
//	}
}
