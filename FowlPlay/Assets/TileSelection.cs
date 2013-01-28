using UnityEngine;
using System.Collections;

public class TileSelection : MonoBehaviour {
	
	private bool aMouseHoveringOnObject = false;
	private bool aObjectIsSelected = false;

	// This will eventually be an array that stores all units/characters.
	// For demo purposes, it is just one (the blue pill).
	public GameObject aCharacter;
	
	public ObjectSelection aSelfObjectSelection;
	
	// This one will also be an array that compiles all units/characters.
	public ObjectSelection aCharacterObjectSelection;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		if (Input.GetMouseButtonDown(0) && aMouseHoveringOnObject)
		{
			// select the object
			if (!aObjectIsSelected)
			{
				if (aCharacterObjectSelection.isObjectSelected())
				{
        			renderer.material.color = Color.red;
					aObjectIsSelected = true;
					//Debug.Log("Object selected.");
				}
			}
			
			// de-select the object
			else if (aObjectIsSelected)
			{
				renderer.material.color = Color.gray;
				aObjectIsSelected = false;
				//Debug.Log("Object de-selected.");
			}
		}
	}
	
	void OnMouseEnter() 
	{
		aMouseHoveringOnObject = true;
		//Debug.Log("Object entered.");	
    }
	
	void OnMouseExit()
	{
		aMouseHoveringOnObject = false;
		//Debug.Log("Object exited.");
	}
	
	public bool isMouseHoveringObject()
	{
		return aMouseHoveringOnObject;
	}
	
	public bool isObjectSelected()
	{
		return aObjectIsSelected;	
	}
}