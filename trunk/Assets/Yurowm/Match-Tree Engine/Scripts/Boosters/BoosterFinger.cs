using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Soomla.Store;

// "Finger" booster
// This object must be in the UI-panel of the booster. During activation (OnEnable) it turn a special mode of interaction with chips (ControlAssistant ignored)
public class BoosterFinger : MonoBehaviour {
	
	void OnEnable () {
		TurnController (false);
		StartCoroutine (Finger());
	}

	void OnDisable () {
		TurnController (true);
	}

	// Enable/Disable ControlAssistant
	void TurnController(bool b) {
		if (ControlAssistant.main == null) return;
		ControlAssistant.main.enabled = b;
	}

	// Coroutine of special control mode
	IEnumerator Finger () {
		yield return StartCoroutine (Utils.WaitFor (SessionAssistant.main.CanIWait, 0.1f));

		Chip targetChip = null;
		while (true) {
			if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
				targetChip = ControlAssistant.main.GetChipFromTouch();
			if (targetChip != null) {
				if (targetChip.gameObject.GetComponent<SimpleChip>() != null) {
					Slot slot = targetChip.parentSlot.slot;
					FieldAssistant.main.AddPowerup(slot.x, slot.y, Powerup.SimpleBomb);
					SessionAssistant.main.EventCounter();
					break;
				}
			}
			yield return 0;
		}

		StoreInventory.TakeItem ("finger", 1);
		UIServer.main.ShowPage ("Field");
	}
}
