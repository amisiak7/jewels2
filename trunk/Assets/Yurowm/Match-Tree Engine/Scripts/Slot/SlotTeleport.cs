using UnityEngine;
using System.Collections;

public class SlotTeleport : MonoBehaviour {

	public Slot target;
	public Slot slot;

	public int targetID = 0;

	float lastTime = -10;
	float delay = 0.15f; // delay between the generations
	
	void  Start (){
		slot = GetComponent<Slot>();
		Initialize ();
	}

	void Initialize () {
		int2 position = ConvertIDtoPosition (targetID);
		target = FieldAssistant.main.GetSlot (position.x, position.y);
        target.teleportTarget = true;
	}

	void  Update (){
		if (!target) return; // Teleport is possible only if target is exist

		if (!SessionAssistant.main.CanIGravity ()) return; // Teleport is possible only in case of mode "gravity"
		
		if (!slot.GetChip()) return; // Teleport is possible only if slot contains chip

		if (target.GetChip()) return; // Teleport is impossible if target slot already contains chip
				
		if (slot.GetBlock()) return; // Teleport is impossible, if the slot is blocked

		if (slot.GetChip().transform.position != slot.transform.position) return;
		
		if (lastTime + delay > Time.time) return; // limit of frequency generation
		lastTime = Time.time;

		AnimationAssistant.main.TeleportChip (slot.GetChip (), target);
	}

	public static int2 ConvertIDtoPosition(int teleportID) {
		int2 position;
		position.y = Mathf.FloorToInt (1f * (teleportID - 1) / 12);
		position.x = teleportID - 1 - position.y * 12;
		position.y = LevelProfile.main.height - position.y - 1;
		return position;
	}
}
