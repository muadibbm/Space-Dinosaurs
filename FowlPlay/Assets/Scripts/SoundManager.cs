using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {
	
	private bool isCamera;
	
	// Use this for initialization
	void Start () {
		if(gameObject == GameObject.Find("Main Camera") || gameObject == GameObject.Find("MiniGameManager"))
		{
			isCamera = true;
		}
		else
		{
			isCamera = false;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(isCamera)
		{
			gameObject.audio.mute = !PauseMenuGUI.music;
		}
		else
		{
			gameObject.audio.mute = !PauseMenuGUI.sfx;
		}
	}
	
	void PauseMusic()
	{
		audio.Pause();	
	}
	
	void PlayMusic()
	{
		audio.Play();	
	}
}
