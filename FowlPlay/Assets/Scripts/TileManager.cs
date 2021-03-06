using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class TileManager : MonoBehaviour {
	
	public static GameObject aCurrentlySelectedTile;
	public static GameObject aLastSelectedTile;
	
	// Used to access methods on CharacterManager
	public CharacterManager aCharacterManager;
	public static GameObject aCurrentlySelectedUnitCurrentTile;
	
	// Lists to aggregate tiles
	public static GameObject[] allTiles;
	private static GameObject[] allNonTiles; // NonTiles are the border tiles
	
	// Tracks all tiles (Vector3 : tile position, tile)
	public static Hashtable allTilesHT;
	
	// Tracks all occupied tiles (Vector3 : tile position, unit on tile)
	public static Hashtable occupiedTilesHT;
	
	// Used for finding the range of movement
	public static Hashtable costs;
	private List<GameObject> tilesInRange;
	private List<GameObject> tilesInAttackRange;
	public static List<GameObject> tilesInMidTurnAttackRange;
	
	// The materials used for highlighting the range and the tile colors.
	public Material aTileGrass, aTileBlue, aTileRed;
	
	public static bool aSingleTileIsSelected = false;
	
	// Use this for initialization
	void Start () {
		
		// Aggregate tiles
		allTiles = GameObject.FindGameObjectsWithTag("Tile");
		allNonTiles = GameObject.FindGameObjectsWithTag("NonTile");
		allTilesHT = new Hashtable();
		occupiedTilesHT = new Hashtable();
		
		// Used for calculating ranges.
		costs = new Hashtable();
		tilesInRange = new List<GameObject>();
		tilesInAttackRange = new List<GameObject>();
		tilesInMidTurnAttackRange = new List<GameObject>();
		
		foreach (GameObject tile in allTiles)
		{
			allTilesHT.Add(tile.transform.position, tile);
			costs.Add (tile.transform.position, -1);
		}
		
		foreach (GameObject tile in allNonTiles)
		{
			allTilesHT.Add(tile.transform.position, tile);
			costs.Add (tile.transform.position, -1);
		}
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void getRange(GameObject pUnit)
	{	
		Vector3 unitsTile = pUnit.transform.position;
		unitsTile.y = 2;
		
		int range;
		int attackRange;
		
		if(CharacterManager.aCurrentlySelectedUnit.GetComponent<DinosaurUnitFunctionalityAndStats>())
		{
			range = CharacterManager.aCurrentlySelectedUnit.GetComponent<DinosaurUnitFunctionalityAndStats>().moveRange;
			attackRange = CharacterManager.aCurrentlySelectedUnit.GetComponent<DinosaurUnitFunctionalityAndStats>().attackRange;
		}
		else
		{
			range = CharacterManager.aCurrentlySelectedUnit.GetComponent<BirdUnitFunctionalityAndStats>().moveRange;
			attackRange = CharacterManager.aCurrentlySelectedUnit.GetComponent<BirdUnitFunctionalityAndStats>().attackRange;
		}
		
		// The function runs and updates the attributes tilesInRange and tilesInAttackRange
		getTilesInRange(getTileAt(unitsTile), range, attackRange);
	}
	
	public void unhighlightRange()
	{
		if (!CharacterManager.aMidTurn)
		{
			foreach (GameObject tile in tilesInRange)
				if (tile != null && !tile.tag.Equals("NonTile"))
					tile.transform.Find("Object002").renderer.material = aTileGrass;
				
			foreach (GameObject tile in tilesInAttackRange)
				if (tile != null && !tile.tag.Equals("NonTile"))
					tile.transform.Find("Object002").renderer.material = aTileGrass;
		}
		
		else
		{
			foreach (GameObject tile in tilesInMidTurnAttackRange)
				if (tile != null && !tile.tag.Equals("NonTile"))
					tile.transform.Find("Object002").renderer.material = aTileGrass;
		}
	}
	
	/**
	 * Returns how many tiles you have to cross to reach the destination tile, including the destination tile.
	 * */
	public static int movementCost(GameObject pFrom, GameObject pTo)
	{
		if (pFrom == null || pTo == null)
			return 0;
		
		Vector3 lFrom = pFrom.transform.position;
		Vector3 lTo = pTo.transform.position;

		int distance = (int)(Math.Abs((lTo.x - lFrom.x)) + Math.Abs((lTo.z - lFrom.z)));
		
		if (distance == 0)
			return 0;
		
		int cost = 1;
		
		// 11 because that's the max distance within one layer of hexagons.
		while (distance > 11)
		{
			distance -= 11;
			cost++;
		}
		
		return cost;
	}
	
	// My implementation of Dijkstra's for finding the range of movement.
	// The only thing to be aware of is the pRunNumber parameter. For most cases you want to use 1, if you'll only be running it once.
	// If you'll be running it a second time to get rid of problematic tiles, then use 2 (after you used 1).
	// NB, it does NOT reset the costs hashtable after a pass. That is for the function that called it to do.
	// Returns all the tiles within range.
	private List<Vector3> dijkstra(GameObject pUnit, int pRange, int pAttackRange, int pRunNumber)
	{
		int range = pRange + pAttackRange;
		
		Vector3 position = getTileUnitIsStandingOn(pUnit);
		
		List<Vector3> results = new List<Vector3>();
		
		if (range == 1)
		{
			foreach(GameObject x in getSurroundingSix(getTileAt(position)))
				results.Add(x.transform.position);
			
			return results;
		}
		
		Queue<Vector3> open = new Queue<Vector3>();
		
		List<Vector3> closed = new List<Vector3>();
		
		open.Enqueue(position);
		
		do
		{
			Vector3 x = open.Dequeue();
			closed.Add(x);
			
			if ((int)costs[x] < range)
			{
				List<GameObject> surrounding = new List<GameObject>();
				if (pRunNumber == 1)
					surrounding = getSurroundingSix(getTileAt(x));
				else
					surrounding = getSurroundingSixX(getTileAt(x));
				
				foreach (GameObject neighbor in surrounding)
				{
					int newCost = (int)costs[x] + movementCost((GameObject)allTilesHT[x],neighbor);
					
					try {
						int costOfNeighbor = (int)costs[neighbor.transform.position];
						if ( costOfNeighbor == -1 || newCost < costOfNeighbor )
						{
							costs.Remove(neighbor.transform.position);
							costs.Add(neighbor.transform.position, newCost);
							
							if (!open.Contains(neighbor.transform.position))
								open.Enqueue(neighbor.transform.position);
						}
					} catch (NullReferenceException e) {
						Debug.Log("Caught exception - Null tile" + e);
					}
				}
			}
		
		} while (open.Count > 0);
		
		foreach (Vector3 x in closed)
			if ((int)costs[x] < range )
				results.Add(x);
		
		return results;
	}
	
	/**
	 * Returns all tiles that are within a specified range of the selected unit.
	 * It does not return anything. Instead, it updates the tilesInRange list.
	 * Similar to Dijkstra's.
	 * */
	public void getTilesInRange(GameObject pUnit, int pRange, int pAttackRange)
	{	
		int range = pRange + pAttackRange;
		tilesInRange.Clear();
		tilesInAttackRange.Clear();
		
		List<Vector3> firstPass = new List<Vector3>();
		
		// The first pass. We check to see what tiles are within range, without considering obstacles that would reduce range.
		List<Vector3> closed = dijkstra(pUnit,pRange,pAttackRange,1);
		List<Vector3> occupied = new List<Vector3>();
		
		bool isPtero = true;
		
		if(CharacterManager.aCurrentlySelectedUnit == CharacterManager.bird1 || CharacterManager.aCurrentlySelectedUnit == CharacterManager.bird2)
			isPtero = false;
		
		else if (!CharacterManager.aCurrentlySelectedUnit.GetComponent<DinosaurUnitFunctionalityAndStats>().species.Equals("Pterodactyl"))
			isPtero = false;
		
		foreach (Vector3 x in closed)
		{
			if ((int)costs[x] < range )
			{
				if(!isPtero)
				{
					if (getTileAt(x).tag.Equals("Tile"))
						getTileAt(x).transform.Find("Object002").renderer.material.color = Color.green;
					else
						occupied.Add(x);
					
					firstPass.Add(x);
				}
				
				else
				{
					GameObject currentTile = getTileAt(x);
						
					// Unoccupied tile
					if (currentTile.tag.Equals("Tile"))
					{
						// Within walk range
						if ((int)costs[x] < pRange)
						{
							currentTile.transform.Find("Object002").renderer.material = aTileBlue;
							tilesInRange.Add(currentTile);
						}
						
						// Tiles that can be attacked at edge
						else
						{
							if(!currentTile.tag.Equals("NonTile"))
							{
								currentTile.transform.Find("Object002").renderer.material = aTileRed;
								tilesInAttackRange.Add(currentTile);
							}
						}
					}
				}
			}

			costs.Remove(x);
			costs.Add(x, -1);
		}
		
		
		if (!isPtero)
		{
			// Second pass--repeat to remove the tiles that the algorithm detected the first time, but that are not actually reachable.
			List<Vector3> closedBis = dijkstra(pUnit,pRange,pAttackRange,2);
			
			foreach (Vector3 x in closedBis)
			{
				if(CharacterManager.aCurrentlySelectedUnit == CharacterManager.bird1 || CharacterManager.aCurrentlySelectedUnit == CharacterManager.bird2)
				{
					// If it is within range
					if ((int)costs[x] < range && (!getTileAt(x).tag.Equals("NonTile")))
					{
						GameObject currentTile = getTileAt(x);
						
						// Unoccupied tile
						if (currentTile.tag.Equals("Tile"))
						{
							// Within walk range
							if ((int)costs[x] < pRange)
							{
								currentTile.transform.Find("Object002").renderer.material = aTileBlue;
								tilesInRange.Add(currentTile);
							}
							
							// Tiles that can be attacked at edge
							else
							{
								if(!currentTile.tag.Equals("NonTile"))
								{
									currentTile.transform.Find("Object002").renderer.material = aTileRed;
									tilesInAttackRange.Add(currentTile);
								}
							}
						}
		
					}
				}
				
				else if(!CharacterManager.aCurrentlySelectedUnit.GetComponent<DinosaurUnitFunctionalityAndStats>().species.Equals("Pterodactyl"))
				{
					// If it is within range
					if ((int)costs[x] < range && (!getTileAt(x).tag.Equals("NonTile")))
					{
						GameObject currentTile = getTileAt(x);
						
						// Unoccupied tile
						if (currentTile.tag.Equals("Tile"))
						{
							// Within walk range
							if ((int)costs[x] < pRange)
							{
								currentTile.transform.Find("Object002").renderer.material = aTileBlue;
								tilesInRange.Add(currentTile);
							}
							
							// Tiles that can be attacked at edge
							else
							{
								if(!currentTile.tag.Equals("NonTile"))
								{
									currentTile.transform.Find("Object002").renderer.material = aTileRed;
									tilesInAttackRange.Add(currentTile);
								}
							}
						}
		
					}
				}
				
				// Reset the hashtable.
				costs.Remove(x);
				costs.Add(x, -1);
					
			}
			
			// Handle the occupied tiles
			foreach (Vector3 x in occupied)
			{
				if ((int)costs[x] < range)
				{
					GameObject occupyingUnit = (GameObject)occupiedTilesHT[x];
						
					if ((occupyingUnit.tag.Equals("Player1") && CharacterManager.aCurrentlySelectedUnit.tag.Equals("Player2")) || (occupyingUnit.tag.Equals("Player2") && CharacterManager.aCurrentlySelectedUnit.tag.Equals("Player1")) || occupyingUnit.tag.Equals("Enemy"))
					{
						if(!getTileAt(x).tag.Equals("NonTile"))
						{
							getTileAt(x).transform.Find("Object002").renderer.material = aTileRed;
							tilesInAttackRange.Add(getTileAt(x));
						}
					}
				}
				
				// Reset the hashtable.
				costs.Remove(x);
				costs.Add(x, -1);
			}
			
			// Get rid of the tiles that we marked as valid in the first pass, but were discovered to be invalid in the second pass.
			foreach (Vector3 y in firstPass)
				if (!closedBis.Contains(y) && getTileAt(y).transform.Find("Object002").renderer.sharedMaterial != aTileRed && !getTileAt(y).tag.Equals("NonTile"))
					getTileAt(y).transform.Find("Object002").renderer.material = aTileGrass;
			
		}
	}
	
	
	
	/**
	 * Calculates the maximum number of tiles reachable from the current tile
	 * for the given range. Does not count the current tile.
	 * */
	public int totalNumberOfReachableTiles(int pRange)
	{
		int total = 0;
		
		for (int i = 1; i <= pRange; i++)
			total += (6*i);
		
		return total;
	}
	
	
	
	/**
	 * Returns the neighbor of a tile at a specified direction
	 * */
	public static GameObject getSingleNeighbor (GameObject pTile, int pDirection)
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
	 * Returns the six tiles that getTilesInRange the chosen tile.
	 * */
	public static List<GameObject> getSurroundingSix (GameObject pTile)
	{
		List<GameObject> lTiles = new List<GameObject>();
		
		bool isPtero = true;
		
		if(CharacterManager.aCurrentlySelectedUnit == CharacterManager.bird1 || CharacterManager.aCurrentlySelectedUnit == CharacterManager.bird2)
			isPtero = false;
		
		else if (!CharacterManager.aCurrentlySelectedUnit.GetComponent<DinosaurUnitFunctionalityAndStats>().species.Equals("Pterodactyl"))
			isPtero = false;
		
		for (int i = 1; i < 7; i++)
		{
			GameObject x = getSingleNeighbor(pTile, i);
			if(!isPtero)
			{
				if (x != null && !x.tag.Equals("NonTile"))
					lTiles.Add(x);
			}
			else
			{
				if (x != null)
					lTiles.Add(x);
			}
		}
		
		return lTiles;
	}
	
	// This version is for range-finding purposes.
	public List<GameObject> getSurroundingSixX (GameObject pTile)
	{
		List<GameObject> lTiles = new List<GameObject>();
		
		bool isPtero = true;
		
		if(CharacterManager.aCurrentlySelectedUnit == CharacterManager.bird1 || CharacterManager.aCurrentlySelectedUnit == CharacterManager.bird2)
			isPtero = false;
		
		else if (!CharacterManager.aCurrentlySelectedUnit.GetComponent<DinosaurUnitFunctionalityAndStats>().species.Equals("Pterodactyl"))
			isPtero = false;
		
		for (int i = 1; i < 7; i++)
		{
			GameObject x = getSingleNeighbor(pTile, i);
			if(!isPtero)
			{
				if (x != null && x.transform.Find("Object002").renderer.material.color == Color.green && !x.tag.Equals("NonTile"))
					lTiles.Add(x);
			}
			else
			{
				if (x != null && x.transform.Find("Object002").renderer.material.color == Color.green)
					lTiles.Add(x);
			}
		}

		return lTiles;
	}
	
	// Finds the shortest path to the destination.
	public void shortestPath()
	{
		GameObject pStartTile = getTileAt(getTileUnitIsStandingOn(CharacterManager.aCurrentlySelectedUnit));
		GameObject pEndTile = aCurrentlySelectedTile;
		
		int distanceFromDestination = movementCost(pStartTile, pEndTile);
		
		// If the distance is just 1, then return the end tile.
		if (distanceFromDestination == 1)
		{
			Vector3 node = pEndTile.transform.position;
			node.y = CharacterManager.aCurrentlySelectedUnit.transform.position.y;
			Vector3[] result = new Vector3[] {node};
			
			if (CharacterManager.aTurn == 1 || CharacterManager.aTurn == 3)
				ClickAndMove.aPath = result;
			else
				UntamedManager.aPath = result;
		}
		
		// The list that will aggregate the tiles in the path.
		List<Vector3> lPath = new List<Vector3>();
		
		Vector3 currentPosition = pEndTile.transform.position;
		
		// Run Dijkstra's and get costs
		int range;
		
		if(CharacterManager.aCurrentlySelectedUnit.GetComponent<DinosaurUnitFunctionalityAndStats>())
			range = CharacterManager.aCurrentlySelectedUnit.GetComponent<DinosaurUnitFunctionalityAndStats>().moveRange;

		else
			range = CharacterManager.aCurrentlySelectedUnit.GetComponent<BirdUnitFunctionalityAndStats>().moveRange;
		
		dijkstra(CharacterManager.aCurrentlySelectedUnit, range+1, 0, 1);
		
		while (distanceFromDestination > 1)
		{
			// Get all surrounding tiles and check to see which one is closest to the destination.
			// Add it to the list and repeat until we get to the destination tile, working backwards.
			List<GameObject> surroundingTiles = getSurroundingSix(getTileAt(currentPosition));
			
			GameObject lowestCost = surroundingTiles[0];
			
			
			// Start off with any tile as the minimum cost, as long as it is not occupied.
			if (!lowestCost.tag.Equals("Tile"))
			{
				foreach (GameObject x in surroundingTiles)
				{
					if (x.tag.Equals("Tile"))
					{
						lowestCost = x;
						break;
					}
				}
			}
			
			foreach (GameObject tile in surroundingTiles)
			{
				if((int)costs[tile.transform.position] < (int)costs[lowestCost.transform.position] && tile.tag.Equals("Tile"))
					lowestCost = tile;
			}
			
			// Convert it into a node usable by a unit.
			Vector3 lowestCostPosition = lowestCost.transform.position;
			lowestCostPosition.y = CharacterManager.aCurrentlySelectedUnit.transform.position.y;
			
			lPath.Add(lowestCostPosition);
			currentPosition = lowestCost.transform.position;
			
			distanceFromDestination--;
		}
		
		lPath.Reverse();
		
		Vector3 final = pEndTile.transform.position;
		final.y = CharacterManager.aCurrentlySelectedUnit.transform.position.y;
		lPath.Add(final);

		
		Vector3[] results = lPath.ToArray();
		
		if (CharacterManager.aTurn == 1 || CharacterManager.aTurn == 3)
			ClickAndMove.aPath = results;
		else
			UntamedManager.aPath = results;
		
		CharacterManager.resetCosts();
	}
	
	public void highlightTile(GameObject pTile)
	{
		pTile.transform.Find("Object002").renderer.material.color = Color.cyan;
	}
	
	/**
	 * Returns the tile at the given position.
	 * */
	public static GameObject getTileAt(Vector3 pPosition)
	{
		GameObject ltile = null;

		if(allTilesHT.Contains(pPosition))
			ltile = (GameObject)allTilesHT[pPosition];
		
		return ltile;
	}
	
	public void selectTile(GameObject pTile)
	{
		// If the tile is not occupied and is within range
		if (pTile.tag.Equals("Tile") && tilesInRange.Contains(pTile))
		{
			aCurrentlySelectedTile = pTile;
			aSingleTileIsSelected = true;
			
			// un-paint the range
			unhighlightRange();
			
			pTile.transform.Find("Object002").renderer.material.color = Color.yellow;
		}		
	}
	
	public void deselectTile()
	{
		// Cannot deselct if no tile is selected!
		if (aSingleTileIsSelected && !aCurrentlySelectedTile.tag.Equals("NonTile")) 
		{		
			aCurrentlySelectedTile.transform.Find("Object002").renderer.material = aTileGrass;
			
			aLastSelectedTile = aCurrentlySelectedTile;
			
			aCurrentlySelectedTile = null;
			aSingleTileIsSelected = false;
		}
	}
	
	public void deselectSingleTile(GameObject pTile)
	{	
		if(!pTile.tag.Equals("NonTile"))
			pTile.transform.Find("Object002").renderer.material = aTileGrass;
	}
	
	private static bool isTileOccupied(GameObject pTile)
	{
		if (pTile.tag.Equals("OccupiedTile"))
			return true;
		
		else
			return false;
	}
	
	public static GameObject pickRandomTile()
	{
		GameObject randomTile;
		Vector3 tile = getTileUnitIsStandingOn(CharacterManager.aCurrentlySelectedUnit);
		
		do
		{
			randomTile = getSingleNeighbor(getTileAt(tile) ,UnityEngine.Random.Range(0, 6));
		}
		while (randomTile == null || isTileOccupied(randomTile) || !randomTile.tag.Equals("Tile"));
		
		return randomTile;
	}
	
	
	public static GameObject TruePickRandomTile()
	{
		GameObject randomTile;
		
		do 
			randomTile = allTiles[UnityEngine.Random.Range(0, allTiles.Length-1)]; 

		while (isTileOccupied(randomTile) || randomTile == null || !randomTile.tag.Equals("Tile"));
				
		return randomTile;	
		
	}
	
	// After the move is complete, we check to see if there are any enemies within attack range.
	// If there are, paint the attack range red and allow the player to choose to attack.
	public void paintAttackableTilesAfterMove()
	{	
			
		// If it's the robot, ignore all of this and end its turn.
		//if (CharacterManager.aCurrentlySelectedUnit.tag.Equals("Enemy"))
		//	SendMessage("endTurn");
		
		//else
		//{
			tilesInMidTurnAttackRange.Clear();
			
			bool canAttack = false;
			
			Vector3 unitsTile = getTileUnitIsStandingOn(CharacterManager.aCurrentlySelectedUnit);
			
			int attackRange;
			if(CharacterManager.aCurrentlySelectedUnit.GetComponent<DinosaurUnitFunctionalityAndStats>())
			{
				attackRange = CharacterManager.aCurrentlySelectedUnit.GetComponent<DinosaurUnitFunctionalityAndStats>().attackRange;
			}
			else
			{
				attackRange = CharacterManager.aCurrentlySelectedUnit.GetComponent<BirdUnitFunctionalityAndStats>().attackRange;
			}
			
			// Check to see whether there are any attackable units in range.
			foreach (Vector3 x in dijkstra(CharacterManager.aCurrentlySelectedUnit, 0, attackRange, 1))
			{
				if (getTileAt(x).tag.Equals("OccupiedTile") && x!= unitsTile)
				{
					GameObject occupyingUnit = (GameObject)occupiedTilesHT[x];
					if ((occupyingUnit.tag.Equals("Player1") && CharacterManager.aCurrentlySelectedUnit.tag.Equals("Player2")) || (occupyingUnit.tag.Equals("Player2") && CharacterManager.aCurrentlySelectedUnit.tag.Equals("Player1")) || occupyingUnit.tag.Equals("Enemy"))	
						canAttack = true;
				}
				
				tilesInMidTurnAttackRange.Add(getTileAt(x));
				costs.Remove(x);
				costs.Add(x, -1);
			}
			
			//if (canAttack)
			//{
				foreach (GameObject x in tilesInMidTurnAttackRange)
				{
					if (x.Equals(getTileAt(unitsTile)) && !x.tag.Equals("NonTile"))
						x.transform.Find("Object002").renderer.material = aTileGrass;
					else
					{
						if (x.tag.Equals("OccupiedTile"))
						{
							GameObject occupyingUnit = (GameObject)occupiedTilesHT[x.transform.position];
							if (((occupyingUnit.tag.Equals("Player1") && CharacterManager.aCurrentlySelectedUnit.tag.Equals("Player2")) || (occupyingUnit.tag.Equals("Player2") && CharacterManager.aCurrentlySelectedUnit.tag.Equals("Player1")) || occupyingUnit.tag.Equals("Enemy")) || CharacterManager.aCurrentlySelectedUnit.tag.Equals("Enemy"))	
								x.transform.Find("Object002").renderer.material = aTileRed;
						}
						
						else
						{
							if(!x.tag.Equals("NonTile"))
								x.transform.Find("Object002").renderer.material = aTileRed;
						}
					}
				}
				
			//}
		//}
			
	}
	
	public static Vector3 getTileUnitIsStandingOn(GameObject pUnit)
	{	
		Vector3 result = pUnit.transform.position;
		result.y = 2.0f;
		
		return result;
	}
	
	public static Vector3 getTileUnitIsStandingOn(Vector3 pUnit)
	{	
		Vector3 result = pUnit;
		result.y = 2.0f;
		
		return result;
	}
	
	public void CleanMidTurnTiles()
	{
		foreach(GameObject x in tilesInMidTurnAttackRange)
		{
			if (!x.tag.Equals("NonTile"))
				x.transform.Find("Object002").renderer.material = aTileGrass;
		}
		
	}
}