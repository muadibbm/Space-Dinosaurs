using UnityEngine;
using System.Collections.Generic;

public class TileManager : MonoBehaviour {
	
	public static GameObject aCurrentlySelectedTile;
	
	public CharacterManager aCharacterManager;
	
	private GameObject[] allTiles;
	
	public static bool aSingleTileIsSelected = false;
	
	// Use this for initialization
	void Start () {		
		allTiles = GameObject.FindGameObjectsWithTag("Tile");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public static void selectTile(GameObject pTile)
	{
		aCurrentlySelectedTile = pTile;
		aSingleTileIsSelected = true;
		pTile.renderer.material.color = Color.yellow;
		//Debug.Log("CurrentlySelTile set at: " + aCurrentlySelectedTile.transform.position);
	}
	
	public static void deselect()
	{
		aCurrentlySelectedTile.renderer.material.color = Color.gray;
		aCurrentlySelectedTile = null;
		aSingleTileIsSelected = false;
	}
	
	private bool isTileOccupied(GameObject pTile)
	{
		/**
		if ((pTile.transform.position.x == aCharacterManager.unitPosition(0).x && pTile.transform.position.z == aCharacterManager.unitPosition(0).z) || (pTile.transform.position.x == aCharacterManager.unitPosition(1).x && pTile.transform.position.z == aCharacterManager.unitPosition(1).z))
			return true;
		else
			return false;
		*/
		return false;
	}
	
	public void pickRandomTile()
	{
		GameObject randomTile;
		
		do
		{
			randomTile = allTiles[Random.Range(0, allTiles.Length - 1)];
		}
		while (isTileOccupied(randomTile));
		
		Debug.Log("Random tile picked: " + randomTile.transform.position);
		
		AutoMove.destTile = randomTile;
		selectTile(randomTile);
		
		Debug.Log("destTile is " + AutoMove.destTile);
	}
}
