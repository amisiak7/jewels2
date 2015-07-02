using UnityEngine;
using System.Collections;

// Slot which generates new simple chips.
[RequireComponent (typeof (Slot))]
[RequireComponent (typeof (SlotForChip))]
public class SlotGenerator : MonoBehaviour {

	public Slot slot;
	public SlotForChip slotForChip;
	public Chip chip;

	float lastTime = -10;
	float delay = 0.15f; // delay between the generations
	
	void  Awake (){
		slot = GetComponent<Slot>();
        slot.generator = true;
		slotForChip = GetComponent<SlotForChip>(); 
	}
	
	void  Update (){
		if (!SessionAssistant.main.CanIGravity ()) return; // Generation is possible only in case of mode "gravity"
		
		if (slotForChip.GetChip()) return; // Generation is impossible, if slot already contains chip
		
		if (slot.GetBlock()) return; // Generation is impossible, if the slot is blocked

		if (lastTime + delay > Time.time) return; // limit of frequency generation
		lastTime = Time.time;

        if (SessionAssistant.main.creatingSugarTask > 0 && !SessionAssistant.main.firstChipGeneration) {
            FieldAssistant.main.GetSugarChip(slot.x, slot.y, transform.position + new Vector3(0, 0.2f, 0)); // creating new sugar chip
            SessionAssistant.main.creatingSugarTask--;
            return;
        }

		if (Random.value > LevelProfile.main.stonePortion)
			FieldAssistant.main.GetNewSimpleChip(slot.x, slot.y, transform.position + new Vector3(0, 0.2f, 0)); // creating new chip
		else
			FieldAssistant.main.GetNewStone(slot.x, slot.y, transform.position + new Vector3(0, 0.2f, 0)); // creating new stone
	}
}