using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Soomla.Store;

// "FreeBombs" booster
// This object should be connected to a button component BoosterButton, which will send the message "BoosterActivate"
[RequireComponent (typeof (BoosterButton))]
public class BoosterFreeBombs : MonoBehaviour {

	public static bool busy = false;

	// Booster activation
	public void BoosterActivate () {
		if (busy) return;
		if (GameObject.FindObjectsOfType<SimpleChip>().Length == 0) return;
		StartCoroutine (CreatingBombs ());
	}

	// Coroutine of booster working
	IEnumerator CreatingBombs () {
		busy = true;
		StoreInventory.TakeItem ("freebombs", 1);
		yield return StartCoroutine (Utils.WaitFor (SessionAssistant.main.CanIWait, 0.1f));
		SessionAssistant.main.EventCounter ();
		FieldAssistant.main.AddPowerup(Powerup.SimpleBomb);
		yield return new WaitForSeconds (0.1f);
		FieldAssistant.main.AddPowerup(Powerup.SimpleBomb);
		yield return new WaitForSeconds (0.1f);
		FieldAssistant.main.AddPowerup(Powerup.SimpleBomb);
		yield return new WaitForSeconds (0.1f);
		FieldAssistant.main.AddPowerup(Powerup.CrossBomb);
		yield return new WaitForSeconds (0.1f);
		FieldAssistant.main.AddPowerup(Powerup.CrossBomb);
		yield return new WaitForSeconds (0.1f);
		FieldAssistant.main.AddPowerup(Powerup.ColorBomb);
		busy = false;
	}
}
