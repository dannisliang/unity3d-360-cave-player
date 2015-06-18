using UnityEngine;
using System.Collections;

[RequireComponent (typeof (AudioSource))]
public class ScriptVideo360 : MonoBehaviour
{
	string url = "http://www.unity3d.com/webplayers/Movie/sample.ogg";
	MovieTexture sphereTexture;

	void Start ()
	{
		StartCoroutine (LoadMovie (@url));
	}

	IEnumerator LoadMovie (string url)
	{
		Debug.Log ("Load Movie");
		WWW videoStream = new WWW (url);
		sphereTexture = videoStream.movie;
		GetComponent <AudioSource>().clip = videoStream.audioClip;

		float startLoadTime = Time.timeSinceLevelLoad;

		while (!sphereTexture.isReadyToPlay)
		{
			yield return 0;
		}
		
		Debug.Log ("Time to be ready: " + (Time.timeSinceLevelLoad - startLoadTime).ToString () );
		Debug.Log ("Ready!");
		GetComponent<Renderer> ().material.mainTexture = sphereTexture;
		GetComponent<AudioSource> ().Play ();
		sphereTexture.Play ();
	}
}
