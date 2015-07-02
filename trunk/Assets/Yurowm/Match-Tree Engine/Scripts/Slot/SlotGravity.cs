using UnityEngine;
using System.Collections;

// The component responsible for the transfer of chips from one slot to another in accordance with physics
[RequireComponent (typeof (Slot))]
[RequireComponent (typeof (SlotForChip))]
public class SlotGravity : MonoBehaviour {
	
	
	public Slot slot;
	public SlotForChip slotForChip;
	public Chip chip;

	// No shadow - is a direct path from the slot up to the slot with a component SlotGenerator. Towards must have slots (without blocks and wall)
	// This concept is very important for the proper physics chips
	public bool shadow;
	
	void  Awake (){
		slot = GetComponent<Slot>();
		slotForChip = GetComponent<SlotForChip>(); 
	}
	
	void  Start (){
		Shading();
	}

	// Update shadows at all slots (for example, after the blocks destruction)
	public static void  Reshading () { 
		SlotGravity[] sgs = GameObject.FindObjectsOfType<SlotGravity>();
		foreach(SlotGravity sg in sgs) sg.Shading();
	}
	
	// shadow determination
	public void  Shading (){
		int yMax = FieldAssistant.height;
		Slot s;
		string key;
		
		for (int y = slot.y; y < yMax; y++) {
			key = slot.x + "x" + y;
            if (!FieldAssistant.main.slots.ContainsKey(key)) {
                shadow = true;
                return;
            }
			s = FieldAssistant.main.slots[key];
            if (!s.GetBlock() && (s.generator || s.teleportTarget))
                break;
            if (!s || !s.gravity || !s[Side.Top]) {
				shadow = true;
				return;
			}
		}
		shadow = false;
	}
	
	void  Update (){
		GravityReaction();
	}

	// Gravity iteration
	void  GravityReaction (){
		if (!SessionAssistant.main.CanIGravity()) return; // Work is possible only in "Gravity" mode
		
		chip = slotForChip.GetChip();
		if (!chip) return; // Work is possible only with the chips, otherwise nothing will move
		
		if (transform.position != chip.transform.position) return; // Work is possible only if the chip is physically clearly in the slot
		
		if (!slot [Side.Bottom] || !slot [Side.Bottom].gravity) return; // Work is possible only if there is another bottom slot

		// provided that bottom neighbor doesn't contains chip, give him our chip
		if (!slot[Side.Bottom].GetChip()) {
			slot[Side.Bottom].SetChip(chip);
			GravityReaction();
			return;
		} 

		// Otherwise, we try to give it to their neighbors from the bottom-left and bottom-right side
		if (Random.value > 0.5f) { // Direction priority is random
			SlideLeft();
			SlideRight();
		} else {
			SlideRight();
			SlideLeft();	
		}
	}


	
	void  SlideLeft (){
		if (slot[Side.BottomLeft] // target slot must exist
		    && slot[Side.BottomLeft].gravity // target slot must contain gravity
		    && ((slot[Side.Bottom] && slot[Side.Bottom][Side.Left]) || (slot[Side.Left] && slot[Side.Left][Side.Bottom])) // target slot should have a no-diagonal path that is either left->down or down->left
		    && !slot[Side.BottomLeft].GetChip() // target slot should not have a chip
		    && slot[Side.BottomLeft].GetShadow() // target slot must have shadow otherwise it will be easier to fill it with a generator on top
		    && !slot[Side.BottomLeft].GetChipShadow()){ // target slot should not be shaded by another chip, otherwise it will be easier to fill it with this chip
			slot[Side.BottomLeft].SetChip(chip); // transfer chip to target slot
		}
	}
	
	void  SlideRight (){
		if (slot[Side.BottomRight] // target slot must exist
		    && slot[Side.BottomRight].gravity // target slot must contain gravity
		    && ((slot[Side.Bottom] && slot[Side.Bottom][Side.Right]) || (slot[Side.Right] && slot[Side.Right][Side.Bottom])) // target slot should have a no-diagonal path that is either right->down or down->right
		    && !slot[Side.BottomRight].GetChip() // target slot should not have a chip
		    && slot[Side.BottomRight].GetShadow() // target slot must have shadow otherwise it will be easier to fill it with a generator on top
		    && !slot[Side.BottomRight].GetChipShadow()) {// target slot should not be shaded by another chip, otherwise it will be easier to fill it with this chip
			slot[Side.BottomRight].SetChip(chip); // transfer chip to target slot
		}
	}
}