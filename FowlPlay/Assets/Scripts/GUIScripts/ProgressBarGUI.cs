using UnityEngine;
using System.Collections;

public class ProgressBarGUI : MonoBehaviour {
	
	public GUISkin hudSkin;
	public int guiDepth = 0;
	public static float healthBar = 0.5f;
	public static float tamenessBar = 0.5f;
	public static int healthPoints = 10;
	public static int tamePoints = 10;
	Rect barAreaNormalized;
	public Rect barArea;
	public Vector2 healthPos;
	public Vector2 healthSize;
	public Vector2 tamenessPos;
	public Vector2 tamenessSize;
	public Rect healthPointsArea;
	public Rect tamePointsArea;
	public Texture2D barEmpty;
	public Texture2D barFull;
	public static bool show = false;
	public static bool isBird = false;
	
	// Use this for initialization
	void Start () {
		barAreaNormalized = new Rect(barArea.x, barArea.y, barArea.width, barArea.height);
	}
	
	// Update is called once per frame
	void Update () {

	}
	
	void OnGUI() {
		GUI.skin = hudSkin;
		GUI.depth = guiDepth;
		if(show && !PauseMenuGUI.isPaused)
		{
			GUI.BeginGroup(barAreaNormalized);
			
				GUI.BeginGroup(new Rect(healthPos.x, healthPos.y, healthSize.x, healthSize.y));
				GUI.Box(new Rect(0,0, healthSize.x, healthSize.y), barEmpty);
					GUI.BeginGroup(new Rect(0,0, healthSize.x * healthBar, healthSize.y));
					GUI.Box(new Rect(0,0, healthSize.x, healthSize.y), barFull);
					GUI.EndGroup();
				GUI.EndGroup();
			
			GUI.Label(new Rect(healthPointsArea), healthPoints.ToString());
			if (!isBird)
			{
				GUI.BeginGroup(new Rect(tamenessPos.x, tamenessPos.y, tamenessSize.x, tamenessSize.y));
				GUI.Box(new Rect(0,0, tamenessSize.x, tamenessSize.y), barEmpty);
					GUI.BeginGroup(new Rect(0,0, tamenessSize.x * tamenessBar, tamenessSize.y));
					GUI.Box(new Rect(0,0, tamenessSize.x, tamenessSize.y), barFull);
					GUI.EndGroup();
				GUI.EndGroup();
			
			GUI.Label(new Rect(tamePointsArea), tamePoints.ToString());
			}
			GUI.EndGroup();
		}
    }
}
