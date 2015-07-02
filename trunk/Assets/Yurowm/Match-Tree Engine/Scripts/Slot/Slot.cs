using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Base class for slots
public class Slot : MonoBehaviour {
	
	public bool gravity;
    public bool generator = false;
    public bool teleportTarget = false;
	Jelly jelly; // Jelly for this slot
    BlockInterface block; // Block for this slot

    bool initialized = false;

	// Position of this slot
	public int x;
	public int y;

	public Dictionary<Side, Slot> nearSlot = new Dictionary<Side, Slot> (); // Nearby slots dictionary
	public Slot this[Side index] { // access to neighby slots on the index
		get {
			return nearSlot[index];
		}
	}

	public Dictionary<Side, bool> wallMask = new Dictionary<Side, bool> (); // Dictionary walls - blocks the movement of chips in certain directions
	
	SlotForChip slotForChip;
	SlotGravity slotGravity;
    public bool sugarDropSlot = false;
	
	void  Awake (){
		slotForChip = GetComponent<SlotForChip>();
		slotGravity = GetComponent<SlotGravity>();
	}

	public void  Initialize (){
		if (initialized) return;
		foreach (Side side in Utils.allSides) // Filling of the nearby slots dictionary
			nearSlot.Add(side, FieldAssistant.main.GetNearSlot(x, y, side));
		foreach (Side side in Utils.straightSides) // Filling of the walls dictionary
			wallMask.Add(side, false);
		initialized = true;
	}

	// function of assigning chip to the slot
	public void  SetChip (Chip c){
		if (slotForChip) {
			FieldAssistant.main.field.chips[x, y] = c.id;
			FieldAssistant.main.field.powerUps[x, y] = c.powerId;
			slotForChip.SetChip(c);
		}
	}

    public Chip GetChip (){
		if (slotForChip) return slotForChip.GetChip();
		return null;
	}
	
	public BlockInterface GetBlock (){
		return block;
	}
	
	public Jelly GetJelly (){
		return jelly;
	}
	
	public void  SetBlock (BlockInterface b){
		block = b;
	}

	public void  SetJelly (Jelly j){
		jelly = j;
	}
	
	void  CrushChip (){
		if (slotForChip) slotForChip.CrushChip();
	}

	// Check for the presence of the "shadow" in the slot. No shadow - is a direct path from the slot up to the slot with a component SlotGenerator. Towards must have slots (without blocks and wall)
	// This concept is very important for the proper physics chips
	public bool GetShadow (){
		if (slotGravity) return slotGravity.shadow;
		else return false;
	}

	// Shadow can also discard the other chips - it's a different kind of shadow.
	public bool GetChipShadow (){
		Slot s = nearSlot[Side.Top];
		while (true) {
			if (!s) return false;
			if (!s.gravity)  return false;
			if (!s.GetChip()) s = s.nearSlot[Side.Top];
			else return true;
		}
	}
	
	// creating a wall between it and neighboring slot
	public void  SetWall (Side side){

		wallMask[side] = true;

		foreach (Side s in Utils.straightSides)
			if (wallMask[s]) {
				if (nearSlot[s]) nearSlot[s].nearSlot[Utils.MirrorSide(s)] = null;
				nearSlot[s] = null;
			}
	
		foreach (Side s in Utils.slantedSides)
			if (wallMask[Utils.SideHorizontal(s)] && wallMask[Utils.SideVertical(s)]) {
				if (nearSlot[s]) nearSlot[s].nearSlot[Utils.MirrorSide(s)] = null;
				nearSlot[s] = null;
			}

	}
}