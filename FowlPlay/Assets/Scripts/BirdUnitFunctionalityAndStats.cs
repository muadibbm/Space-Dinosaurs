using UnityEngine;
using System.Collections;

public class BirdUnitFunctionalityAndStats : MonoBehaviour {

	public int tamePower = 50;
	public int healthPoints = 100;
	public int maxHealthPoints = 100;
	public int defensePoints = 10;
	public int attackPoints = 15;
	public int tameCost = 1;
	public int attackCost = 1;
	public int moveCost = 1;
	public int moveRange = 3;
	public int attackRange = 2;
	public GameObject deathParticle;
	//Colors
	public Color player1Color = Color.green;
	public Color player2Color = Color.blue;
	public Color enemyColor = Color.red;
		
	void Start()
	{
		UpdateColor();
	}
	/*void OnMouseEnter()
	{
		UpdateColor();
	}*/
	
	/*void OnMouseEnter()
	{
		
		TameUnit(CharacterManager.aInteractUnit);
		Debug.Log("Taming " + CharacterManager.aInteractUnit  + "\n");
	}*/
	
	/**
	 * Has this unit take damage, usually called by another unit's "AttackUnit" function
	 * 
	 * @param dmg the ammount of damage to be dealt
	 * @return returns the remaining health of this unit after the attack.
	 */
	public int TakeAttackDamage(int dmg)
	{
		//Need to make a better formula.
		int actualDamageTaken = dmg - defensePoints;
		if (dmg - defensePoints > 0)
		{
			healthPoints -= actualDamageTaken;
		}
		else
		{
			actualDamageTaken = 0;
		}
		
		if (healthPoints <= 0)
		{
			StartCoroutine("Die");
		}
		gameObject.BroadcastMessage("showDamageText", "-" + actualDamageTaken.ToString());
		Debug.Log ("Current HP of " + gameObject + " is: " + healthPoints + "\n");
		UpdateGuiHealthBar();
		return healthPoints;
	}
	
	// Adds pAmount to this unit's HP.
	// If the recovery amount would result in having more HP than the max, just make it the max.
	public void RecoverHP(int pAmount)
	{
		healthPoints += pAmount;
		
		if (healthPoints > maxHealthPoints)
			healthPoints = maxHealthPoints;
		
		UpdateGuiHealthBar();
	}
	
	// Stores whether the unit has full HP in a temporary register in ClickAndMove & UntamedManager for the purpose of items.
	public void CheckHP()
	{
		ClickAndMove.fullHP = healthPoints == maxHealthPoints;
		UntamedManager.fullHP = healthPoints == maxHealthPoints;	
	}
	
	public void MovementCost (GameObject player)
	{
		player.SendMessage("RemoveAp", moveCost);
	}
	
	public void AttackUnit (GameObject unit)
	{
		unit.SendMessage("TakeAttackDamage", attackPoints);
		if (gameObject.tag == "Player1")
		{
			CharacterManager.bird1.SendMessage("RemoveMana", attackCost);
		}
		else if (gameObject.tag == "Player2")
		{
			CharacterManager.bird2.SendMessage("RemoveMana", attackCost);
		}
	}
	
	public void RemoveMoveMana()
	{
		if (gameObject.tag == "Player1")
		{
			CharacterManager.bird1.SendMessage("RemoveMana", moveCost);
		}
		else if (gameObject.tag == "Player2")
		{
			CharacterManager.bird2.SendMessage("RemoveMana", moveCost);
		}
	}
	
	//Want to make sure that this unit is the currently selected one.
	public void TameUnit (GameObject unit)
	{
		if (gameObject == CharacterManager.aCurrentlySelectedUnit)
		{
			unit.SendMessage("AddTamePointsByRate", tamePower);
			//Maybe remove AP from the player here as well based on the tame cost?
		}
		if (gameObject.tag == "Player1")
		{
			CharacterManager.bird1.SendMessage("RemoveMana", tameCost);
		}
		else if (gameObject.tag == "Player2")
		{
			CharacterManager.bird2.SendMessage("RemoveMana", tameCost);
		}
	}
	
	public void OnSelectedSetHud()
	{
		//Set the attributes of the GUI via send message and sending this units current stats and max stats.
	}
	
	public IEnumerator Die()
	{
		yield return new WaitForSeconds(1.0f);
		Instantiate(deathParticle, transform.position, deathParticle.transform.rotation);
		Destroy(gameObject);
	}
	
	public void UpdateGuiHealthBar()
	{
		ProgressBarGUI.healthBar = (float)healthPoints / (float)maxHealthPoints;
		ProgressBarGUI.healthPoints = healthPoints;
	}
	
	public void UpdateGuiTameBar()
	{
		PlayMenuGUI.untamed = false;
	}
	
	
	public void UpdateGuiTameButton()
	{
		PlayMenuGUI.untamed = false;
	}	
	
	public void UpdateColor()
	{
		if (tag == "Player1")
		{
			gameObject.transform.FindChild("model").transform.FindChild("body").renderer.material.color = player1Color;
		}
		else if (tag == "Player2")
		{
			gameObject.transform.FindChild("model").transform.FindChild("body").renderer.material.color = player2Color;
		}
		else if (tag == "Enemy")
		{
			gameObject.transform.FindChild("model").transform.FindChild("body").renderer.material.color = enemyColor;
		}
	}
	
	public float EndTurnTickUntame (Vector3 commanderPosition)
	{
		return 0.0f;
	}
	
}
