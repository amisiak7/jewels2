using UnityEngine;
using System.Collections;

// Effect of mixing 2 CrossBomb
// Destroys all the chips of the current horizontal, vertical and two diagonals
public class CrossCrossMixEffect : BombMixEffect {

	int sx = 0;
	int sy = 0;

	void Start() {
		StartCoroutine (MixEffect ());
	}

	// Effect logic
	IEnumerator MixEffect (){
		yield return 0;
		
		Chip chip = GetChip ();
		while (chip.parentSlot == null) yield return 0;
		transform.position = chip.parentSlot.transform.position;
		
		while (!SessionAssistant.main.CanIMatch()) yield return 0;
		
		SessionAssistant.main.matching ++;
		SessionAssistant.main.EventCounter ();
		
		GetComponent<Animation>().Play ();
		AudioAssistant.Shot("CrossBombCrush");
		
		sx = chip.parentSlot.slot.x;
		sy = chip.parentSlot.slot.y;

		yield return new WaitForSeconds(0.3f);

		FieldAssistant.main.JellyCrush(sx, sy);
	
		for (int i = 1; i < 12; i++) {
			Crush(sx+i, sy+i);
			Crush(sx+i, sy);
			Crush(sx+i, sy-i);
			Crush(sx, sy+i);
			Crush(sx, sy-i);
			Crush(sx-i, sy+i);
			Crush(sx-i, sy);
			Crush(sx-i, sy-i);
			yield return new WaitForSeconds(0.05f);
		}
		
		yield return new WaitForSeconds(0.2f);
		SessionAssistant.main.matching --;
		
		while (GetComponent<Animation>().isPlaying) yield return 0;
		FieldAssistant.main.BlockCrush(sx, sy, false);
		
		destroingLock = false;
		DestroyChipFunction ();
	}

	void Crush(int x, int y) {
		if (!CheckTarget(x, y)) return;
		Slot s;
		s = FieldAssistant.main.GetSlot(x, y);
		FieldAssistant.main.BlockCrush(x, y, false);
		FieldAssistant.main.JellyCrush(x, y);
		if (s && s.GetChip()) {
			s.GetChip().SetScore(0.5f);
			s.GetChip().DestroyChip();
		}
	}

	bool CheckTarget(int x, int y) {
		if (x == sx) return true;
		if (y == sy) return true;
		if (x - y == sx - sy) return true;
		if (x + y == sx + sy) return true;
		return false;
	}
}
