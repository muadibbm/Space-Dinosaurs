using UnityEngine;
using System.Collections;

[RequireComponent (typeof (AudioSource))]

public class PauseMenuGUI : MonoBehaviour {
	
	public AudioClip click;
	public GUISkin menuSkin;
	public Rect menuArea;
	public Rect resumeButton;
	public Rect optionsButton;
	public Rect backMainButton;
	public Rect quitButton;
	Rect menuAreaNormalized;
	string menuPage = "main";
	public Rect options;
	bool isPaused = false;
	
	// Use this for initialization
	void Start () {
		menuAreaNormalized = new Rect(menuArea.x * Screen.width - (menuArea.width * 0.5f), menuArea.y * Screen.height - (menuArea.height * 0.5f), menuArea.width, menuArea.height);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			if(!isPaused)
			{
				Time.timeScale = 0.0f;
				isPaused = true;
			}
			else
			{
				Time.timeScale = 1.0f;
				isPaused = false;
			}
		}
	}
	
	void OnGUI() {
		GUI.skin = menuSkin;
		if(isPaused)
		{
			GUI.BeginGroup(menuAreaNormalized);
			if(menuPage == "main")
			{
				if(GUI.Button(new Rect(resumeButton), "Resume"))
				{
					audio.PlayOneShot(click);
					Time.timeScale = 1.0f;
					isPaused = false;
				}
				if(GUI.Button(new Rect(optionsButton), "Options"))
				{
					audio.PlayOneShot(click);
					menuPage = "options";
				}
				if(GUI.Button(new Rect(backMainButton), "Back to Main Menu"))
				{
					StartCoroutine("ButtonAction", "Menu");
				}
				if(Application.platform != RuntimePlatform.OSXWebPlayer && Application.platform != RuntimePlatform.WindowsWebPlayer)
				{
					if(GUI.Button(new Rect(quitButton), "Quit"))
					{
						StartCoroutine("ButtonAction", "quit");
					}
				}
			}
			else if(menuPage == "options")
			{
				GUI.Label(new Rect(options), "Options Here");
				if(GUI.Button(new Rect(quitButton), "Back"))
				{
					audio.PlayOneShot(click);
					menuPage = "main";
				}
			}
			GUI.EndGroup();
		}
	}
	
	IEnumerator ButtonAction(string levelName) {
		audio.PlayOneShot(click);
		Time.timeScale = 1.0f;
		yield return new WaitForSeconds(0.35f);
		if(levelName != "quit")
		{
			Application.LoadLevel(levelName);
		}
		else
		{
			Application.Quit();
			Debug.Log("Quit!");
		}
	}
}
