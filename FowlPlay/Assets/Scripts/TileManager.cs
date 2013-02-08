using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TileManager : MonoBehaviour {
	
	public static GameObject aCurrentlySelectedTile;
	
	public CharacterManager aCharacterManager;
	
	private static GameObject[] allTiles;
	private static Hashtable allTilesHT;
	private Hashtable rangeHT;
	private GameObject[] range;
	
	public static bool aSingleTileIsSelected = false;
	
	public static bool tileOccupied;
	
	// Use this for initialization
	void Start () {		
		allTiles = GameObject.FindGameObjectsWithTag("Tile");
		
		Debug.Log("Position of first tile: " + allTiles[0].transform.position);
		Debug.Log("Position of second tile: " + allTiles[1].transform.position);
		
		allTilesHT = new Hashtable();
		
		foreach (GameObject tile in allTiles)
			allTilesHT.Add(tile.transform.position, tile);
		
		rangeHT = new Hashtable();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void highlightRange(GameObject pUnit)
	{	
		Vector3 unitsTile = pUnit.transform.position;
		unitsTile.y = 2;
		GameObject currentTile = getTileAt(unitsTile);
		
		GameObject[] surr = getSurroundingTiles(currentTile, 3);
		
		foreach (GameObject tile in surr)
			if (tile != null)
				tile.renderer.material.color = Color.green;
	}
	
	public void unhighlightRange()
	{		
		foreach (GameObject tile in range)
			tile.renderer.material.color = Color.gray;
	}
	
	/**
	 * Returns an array of tiles that contain all of the tiles that surround the
	 * chosen tile, excluding the chosen tile.
	 * 
	 * NOTE: It still does not account for the case when it should select tile that
	 * doesn't exist because, for example, it goes beyond the map.
	 * */
	public GameObject[] getSurroundingTiles(GameObject pCenterTile, int pRange)
	{
		rangeHT.Clear();

		// We collect them clockwise
		GameObject[] lTiles = new GameObject[totalNumberOfSurroundingTiles(pRange)];
		GameObject[] firstLayer = getSurroundingSix(pCenterTile);
		
		int layersToGo = pRange - 1;
		int counter = 0;
		
		for (int i = 0; i < firstLayer.Length; i++)
		{
			lTiles[counter] = firstLayer[i];
			if(firstLayer[i] != null)
			{
				rangeHT.Add(firstLayer[i].transform.position, firstLayer[i]);
				counter++;
			}
		}
			
		if (pRange >= 2) 
		{
			GameObject[] nextLayer = getNextLayer(firstLayer);
			while (layersToGo > 0)
			{
				for (int i = 0; i < nextLayer.Length; i++)
				{
					if (nextLayer[i] != null)
					{
						lTiles[counter] = nextLayer[i];
						rangeHT.Add(nextLayer[i].transform.position, nextLayer[i]);
						counter++;
					}
				}
				layersToGo -= 1;
				nextLayer = getNextLayer(nextLayer);
			}
		}
		
		Debug.Log("Counter size = " + counter);
		Debug.Log("Complete array size = " + lTiles.Length);
		
		// We collect them counter-clockwise, to catch any that we may have missed when we
		// went clockwise (for example at a map edge).

		if (counter < lTiles.Length)
		{			
			int layersToGocc = pRange - 1;
				
			if (pRange >= 2) 
			{
				GameObject[] nextLayer = getNextLayerCC(firstLayer);
				while (layersToGocc > 0)
				{
					for (int i = 0; i < nextLayer.Length; i++)
					{
						if (nextLayer[i] != null)
							if(!rangeHT.ContainsKey(nextLayer[i].transform.position))
							{
								lTiles[counter] = nextLayer[i];
								rangeHT.Add(nextLayer[i].transform.position, nextLayer[i]);
								counter++;
							}
					}
					layersToGocc -= 1;
					nextLayer = getNextLayerCC(nextLayer);
				}
			}
		}
		
		range = lTiles;
		return lTiles;
	}
	
	/**
	 * Calculates the total number of tiles surrounding the current tile
	 * for the given range. Does not count the current tile.
	 * */
	public int totalNumberOfSurroundingTiles(int pRange)
	{
		int total = 0;
		
		for (int i = 1; i <= pRange; i++)
			total += (6*i);
		
		return total;
	}
	
	/**
	 * Given a layer of hexagon tiles, it will return the next layer.
	 * */
	public GameObject[] getNextLayer (GameObject[] currentLayer)
	{
		GameObject[] nextLayer = new GameObject[currentLayer.Length + 6];
		int nextLayerLevel = (currentLayer.Length / 6) + 1;
		
		int counter = 0;
		int innerCounter = 0;
		
		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 1);
		counter++;
		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 2);
		counter++;
		innerCounter++;
		
		if (nextLayerLevel >= 3)
		{
			for (int i = 0; i < nextLayerLevel - 2; i++, counter++, innerCounter++)
				nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 2);				
		}

		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 2);
		counter++;
		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 3);
		counter++;
		innerCounter++;

		if (nextLayerLevel >= 3)
		{
			for (int i = 0; i < nextLayerLevel - 2; i++, counter++, innerCounter++)
				nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 3);				
		}

		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 3);
		counter++;
		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 4);
		counter++;
		innerCounter++;

		if (nextLayerLevel >= 3)
		{
			for (int i = 0; i < nextLayerLevel - 2; i++, counter++, innerCounter++)
				nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 4);				
		}

		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 4);
		counter++;
		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 5);
		counter++;
		innerCounter++;
		
		if (nextLayerLevel >= 3)
		{
			for (int i = 0; i < nextLayerLevel - 2; i++, counter++, innerCounter++)
				nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 5);				
		}
		
		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 5);
		counter++;
		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 6);
		counter++;
		innerCounter++;
		
		if (nextLayerLevel >= 3)
		{
			for (int i = 0; i < nextLayerLevel - 2; i++, counter++, innerCounter++)
				nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 6);				
		}
		
		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 6);
		counter++;
		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 1);
		counter++;
		innerCounter++;
		
		if (nextLayerLevel >= 3)
		{
			for (int i = 0; i < nextLayerLevel - 2; i++, counter++, innerCounter++)
				nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 1);				
		}
		
		return nextLayer;
		
	}
	
	/**
	 * Does the exact same thing as the one above, but counter-clockwise.
	 * */
	public GameObject[] getNextLayerCC (GameObject[] currentLayer)
	{
		GameObject[] nextLayer = new GameObject[currentLayer.Length + 6];
		int nextLayerLevel = (currentLayer.Length / 6) + 1;
		
		int counter = 0;
		int innerCounter = 0;
		
		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 6);
		counter++;
		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 5);
		counter++;
		innerCounter++;
		
		if (nextLayerLevel >= 3)
		{
			for (int i = 0; i < nextLayerLevel - 2; i++, counter++, innerCounter++)
				nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 5);				
		}

		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 5);
		counter++;
		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 4);
		counter++;
		innerCounter++;

		if (nextLayerLevel >= 3)
		{
			for (int i = 0; i < nextLayerLevel - 2; i++, counter++, innerCounter++)
				nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 4);				
		}

		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 4);
		counter++;
		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 3);
		counter++;
		innerCounter++;

		if (nextLayerLevel >= 3)
		{
			for (int i = 0; i < nextLayerLevel - 2; i++, counter++, innerCounter++)
				nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 3);				
		}

		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 3);
		counter++;
		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 2);
		counter++;
		innerCounter++;
		
		if (nextLayerLevel >= 3)
		{
			for (int i = 0; i < nextLayerLevel - 2; i++, counter++, innerCounter++)
				nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 2);				
		}
		
		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 2);
		counter++;
		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 1);
		counter++;
		innerCounter++;
		
		if (nextLayerLevel >= 3)
		{
			for (int i = 0; i < nextLayerLevel - 2; i++, counter++, innerCounter++)
				nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 1);				
		}
		
		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 1);
		counter++;
		nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 6);
		counter++;
		innerCounter++;
		
		if (nextLayerLevel >= 3)
		{
			for (int i = 0; i < nextLayerLevel - 2; i++, counter++, innerCounter++)
				nextLayer[counter] = getSingleNeighbor(currentLayer[innerCounter], 6);				
		}
		
		return nextLayer;
		
	}
	
	/**
	 * Returns the neighbor of a tile at a specified direction
	 * */
	public GameObject getSingleNeighbor (GameObject pTile, int pDirection)
	{
		if (pTile == null)
			return null;
		
		Vector3 position = pTile.transform.position;
		
		switch (pDirection)
		{
			// north - 1
			case 1:
				position.z += 8;
				break;
			
			// north-east - 2
			case 2:
				position.x += 7;
				position.z += 4;
				break;
			
			// south-east - 3
			case 3:
				position.x += 7;
				position.z -= 4;
				break;
			
			// south - 4
			case 4:
				position.z -= 8;
				break;
			
			// south-west - 5
			case 5:
				position.x -= 7;
				position.z -= 4;
				break;
			
			// north-west - 6
			case 6:
				position.x -= 7;
				position.z += 4;
				break;
			
		}
		
		return getTileAt(position);
	}
	
	/**
	 * Returns the six tiles that surround the chosen tile.
	 * */
	public GameObject[] getSurroundingSix (GameObject pTile)
	{
		GameObject[] lTiles = new GameObject[6];
		Vector3 position = pTile.transform.position;
		Vector3[] surroundingLayer = new Vector3[6];
		
		// north
		surroundingLayer[0].x = position.x;
		surroundingLayer[0].y = position.y;
		surroundingLayer[0].z = position.z + 8;
		
		// north-east
		surroundingLayer[1].x = position.x + 7;
		surroundingLayer[1].y = position.y;
		surroundingLayer[1].z = position.z + 4;

		// south-east
		surroundingLayer[2].x = position.x + 7;
		surroundingLayer[2].y = position.y;
		surroundingLayer[2].z = position.z - 4;

		// south
		surroundingLayer[3].x = position.x;
		surroundingLayer[3].y = position.y;
		surroundingLayer[3].z = position.z - 8;

		// south-west
		surroundingLayer[4].x = position.x - 7;
		surroundingLayer[4].y = position.y;
		surroundingLayer[4].z = position.z - 4;

		// north-west
		surroundingLayer[5].x = position.x - 7;
		surroundingLayer[5].y = position.y;
		surroundingLayer[5].z = position.z + 4;
		
		for (int i = 0; i < lTiles.Length; i++)
			lTiles[i] = getTileAt(surroundingLayer[i]);
		
		return lTiles;
	}
	
	public void highlightTile(GameObject pTile)
	{
		pTile.renderer.material.color = Color.cyan;
	}
	
	/**
	 * Returns the tile at the given position.
	 * */
	public GameObject getTileAt(Vector3 pPosition)
	{
		GameObject ltile = null;
		
		if(allTilesHT.Contains(pPosition))
			ltile = (GameObject)allTilesHT[pPosition];
		return ltile;
	}
	
	public void selectTile(GameObject pTile)
	{
		if (rangeHT.ContainsKey(pTile.transform.position) && !isTileOccupied(pTile))
		{
			aCurrentlySelectedTile = pTile;
			aSingleTileIsSelected = true;
			
			foreach (GameObject tile in range)
				if (tile != null)
					tile.renderer.material.color = Color.gray;
			
			pTile.renderer.material.color = Color.yellow;
			
			//Vector3 newPosition = 
			//Vector3 oldPosition = pTile.transform.position;
			//CharacterManager.unitsHT.Remove(
		}		
	}
	
	public static void deselect()
	{
		if (aSingleTileIsSelected) 
		{
			aCurrentlySelectedTile.renderer.material.color = Color.gray;
			aCurrentlySelectedTile = null;
			aSingleTileIsSelected = false;
		}
	}
	
	private bool isTileOccupied(GameObject pTile)
	{
		Debug.Log("Checking whether a tile is occupied.");
		Vector3 correctedPosition = pTile.transform.position;
		Vector3 correctedPosition1 = pTile.transform.position;
		Debug.Log("Tile position = " + correctedPosition);
		correctedPosition.y = 7;
		correctedPosition1.y = 2.5f;
		Debug.Log("Corrected position = " + correctedPosition);
		
		if (CharacterManager.unitsHT.ContainsKey(correctedPosition) || CharacterManager.unitsHT.ContainsKey(correctedPosition1))
		{
			Debug.Log("Yep, occupied");
			return true;
		}
		
		else
		{
			Debug.Log("No, free.");
			return false;
		}
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
