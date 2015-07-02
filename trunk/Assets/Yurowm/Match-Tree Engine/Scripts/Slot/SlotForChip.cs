using UnityEngine;
using System.Collections;

// slots which can contain the chips
[RequireComponent (typeof (Slot))]
public class SlotForChip : MonoBehaviour {

	public Chip chip;
	public Slot slot;

	public Slot this[Side index] { // access to neighby slots on the index
		get {
			return slot.nearSlot[index];
		}
	}

	void  Awake (){
		slot = GetComponent<Slot>();
	}
	
	public Chip GetChip (){
		return chip;
	}

	// function of assigning chip to the slot
	public void  SetChip (Chip c){
		if (chip) chip.parentSlot = null;
		chip = c;
		chip.transform.parent = transform;
		if (chip.parentSlot) {
			chip.parentSlot.chip = null;
			FieldAssistant.main.field.chips[chip.parentSlot.slot.x, chip.parentSlot.slot.y] = -1;
		}
		chip.parentSlot = this;
		FieldAssistant.main.field.chips[slot.x, slot.y] = chip.id;
	}

	public void  CrushChip (){
		chip.DestroyChip();
		chip = null;
	}

	// Analysis of chip for combination
	public SessionAssistant.Solution MatchAnaliz (){
		
		if (!GetChip()) return null;
		if (!GetChip().IsMatcheble()) return null;

		//    T
		//    T
		//    T
		// LLLXRRR	X - current chip
		//    B
		//    B
		//    B

		int t = 0; // number of chips with same color on top
		int r = 0; // number of chips with same color on right
		int b = 0; // number of chips with same color on bottom
		int l = 0; // number of chips with same color on left

		int potencialV = 0; // vertical solution potential
		int potencialH = 0; // horizontal solution potential

		// coordinats of current chip/slot
		int x = slot.x;
		int y = slot.y;

		// playing field size
		int width = FieldAssistant.main.field.width;
		int height = FieldAssistant.main.field.height;
		
		Slot s;
		GameObject o;

		// searching right chips
		for (x = slot.x + 1; x < width; x++) {
			o = GameObject.Find("Slot_" + x + "x" + slot.y);
			if (!o) break;
			s = o.GetComponent<Slot>();
			if (!s) break;
			if (!s.GetChip()) break;
			if (s.GetChip().id != chip.id) break;
			if (!s.GetChip().IsMatcheble()) break;
			potencialH += s.GetChip().GetPotencial();
			r++;
		}

		// searching left chips
		for (x = slot.x - 1; x >= 0; x--) {
			o = GameObject.Find("Slot_" + x + "x" + slot.y);
			if (!o) break;
			s = o.GetComponent<Slot>();
			if (!s) break;
			if (!s.GetChip()) break;
			if (s.GetChip().id != chip.id) break;
			if (!s.GetChip().IsMatcheble()) break;
			potencialH += s.GetChip().GetPotencial();
			l++;
		}

		// searching top chips
		for (y = slot.y + 1; y < height; y++) {
			o = GameObject.Find("Slot_" + slot.x + "x" + y);
			if (!o) break;
			s = o.GetComponent<Slot>();
			if (!s) break;
			if (!s.GetChip()) break;
			if (s.GetChip().id != chip.id) break;
			if (!s.GetChip().IsMatcheble()) break;
			potencialV += s.GetChip().GetPotencial();
			t++;
		}

		// searching bottom chips
		for (y = slot.y - 1; y >= 0; y--) {
			o = GameObject.Find("Slot_" + slot.x + "x" + y);
			if (!o) break;
			s = o.GetComponent<Slot>();
			if (!s) break;
			if (!s.GetChip()) break;
			if (s.GetChip().id != chip.id) break;
			if (!s.GetChip().IsMatcheble()) break;
			potencialV += s.GetChip().GetPotencial();
			b++;
		}

		// Formation of solution if it is there 
		if (r + l >= 2 || t + b >= 2) {
			SessionAssistant.Solution solution = new SessionAssistant.Solution();
			solution.h = r + l >= 2;
			solution.v = t + b >= 2;
			solution.x = slot.x;
			solution.y = slot.y;
			solution.count = 1;
			solution.count += solution.v ? t + b : 0;
			solution.count += solution.h ? r + l : 0;
			solution.id = chip.id;
			solution.posH = r;
			solution.negH = l;
			solution.posV = t;
			solution.negV = b;
			if (solution.v) solution.potencial += potencialV;
			if (solution.h) solution.potencial += potencialH;
			solution.potencial += chip.GetPotencial();
			return solution;
		}
		return null;
	}
}