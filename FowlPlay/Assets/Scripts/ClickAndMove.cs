using UnityEngine;
using System.Collections;

public class ClickAndMove : MonoBehaviour
{	
	public float aSpeedOfMovement = 20.0f;
	public static bool aIsObjectMoving = false;
	public static bool aIsObjectRotating = false;
	public static bool aMovementHappened = false;
	
	public static Vector3[] aPath;
	
	private Vector3 destination;
	
	private GameObject manager;
	
	void Start () 
	{
		manager = GameObject.Find("Character");
	}

	// Update is called once per frame
  	void Update ()
	{
		
	}
	
	
	IEnumerator move()
	{
		// Start the movement.
		yield return StartCoroutine("moveHelper");
		
		// Proceed once the movement has ended.
		CharacterManager.aCurrentlySelectedUnit.transform.position = destination;
		manager.SendMessage("deselectTile");
		manager.SendMessage("paintAttackableTilesAfterMove");
		aIsObjectMoving = false;
		
		CharacterManager.aMidTurn = true;
		
		if (CharacterManager.aSingleUnitIsSelected)
			CharacterManager.aRotationAfterMove = CharacterManager.aCurrentlySelectedUnit.transform.rotation;

	}
	
	// Move takes the currently selected unit and moves it to the currently selected tile.
	IEnumerator moveHelper()
	{
		if (CharacterManager.aSingleUnitIsSelected)
		{
			if (TileManager.aSingleTileIsSelected)
			{	
				Vector3 startTile = TileManager.getTileUnitIsStandingOn(CharacterManager.aCurrentlySelectedUnitOriginalPosition);
				
				destination = TileManager.aCurrentlySelectedTile.transform.position;
				
				manager.SendMessage("shortestPath");
				aIsObjectMoving = true;
				
				// Slide the unit to the location following the path, or directly if the distance is just one.
				if (aPath.Length > 1)
					iTween.MoveTo(CharacterManager.aCurrentlySelectedUnit, iTween.Hash("path", aPath, "time", 2.0f, "orienttopath", true));
				else
					iTween.MoveTo(CharacterManager.aCurrentlySelectedUnit, iTween.Hash("position", aPath[0], "time", 1.0f, "orienttopath", true));
				
				// Update hashtable and tags
				
				// The starting tile is made unoccupied.
				TileManager.occupiedTilesHT.Remove(startTile);
				TileManager.getTileAt(startTile).tag = "Tile";
					
				// The destination tile is marked occupied.
				TileManager.occupiedTilesHT.Add(destination, CharacterManager.aCurrentlySelectedUnit);
				TileManager.getTileAt(destination).tag = "OccupiedTile";
				
				// Set the movement flag to true
				aMovementHappened = true;
				
				destination.y = CharacterManager.aCurrentlySelectedUnitOriginalPosition.y;
				
				// This next loop ensures the routine finishes once the iTween animation has finished.
				do
				{
					yield return new WaitForSeconds(0.5f);
				} while (iTween.tweens.Count > 0);
				
			}
		}
	}
	
}