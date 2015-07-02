using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// The class is responsible for logic CrossBomb
[RequireComponent (typeof (Chip))]
public class CrossBomb : MonoBehaviour {
	
	Chip chip;
	int birth; // Event count at the time of birth SessionAssistant.main.eventCount
	bool mMatching = false;
	bool matching {
		set {
			if (value == mMatching) return;
			mMatching = value;
			if (mMatching)
				SessionAssistant.main.matching ++;
			else
				SessionAssistant.main.matching --;
		}

		get {
			return mMatching;
		}
	}
	void OnDestroy () {
		matching = false;
	}
	
	void  Awake (){
		chip = GetComponent<Chip>();
		chip.chipType = "CrossBomb";
		birth = SessionAssistant.main.eventCount;
		AudioAssistant.Shot ("CreateCrossBomb");
	}

	// Coroutine destruction / activation
	IEnumerator  DestroyChipFunction (){
		if (birth == SessionAssistant.main.eventCount) {
			chip.destroing = false;
			yield break;
		}
		
		matching = true;

		GetComponent<Animation>().Play("CrossBump");
		AudioAssistant.Shot("CrossBombCrush");
		
		int sx = chip.parentSlot.slot.x;
		int sy = chip.parentSlot.slot.y;
		
		chip.ParentRemove();
		
		
		yield return new WaitForSeconds(0.3f);
		
		int width = FieldAssistant.main.field.width;
		int height = FieldAssistant.main.field.height;
		
		Slot s;
		
		FieldAssistant.main.JellyCrush(sx, sy);

		Dictionary<Side, bool> sides = new Dictionary<Side, bool> ();
		foreach(Side side in Utils.straightSides)
			sides.Add(side, true);
		
		for (int x = 0; x < width; x++) {
			if (x == sx) continue;
			s = FieldAssistant.main.GetSlot(x, sy);
			FieldAssistant.main.BlockCrush(x, sy, false);
			FieldAssistant.main.JellyCrush(x, sy);
			if (s && s.GetChip()) {
				s.GetChip().SetScore(0.5f);
				s.GetChip().DestroyChip();
			}
		}
		
		for (int y= 0; y < height; y++) {
			if (y == sy) continue;
			s = FieldAssistant.main.GetSlot(sx, y);
			FieldAssistant.main.BlockCrush(sx, y, false);
			FieldAssistant.main.JellyCrush(sx, y);
			if (s && s.GetChip()) {
				s.GetChip().SetScore(0.5f);
				s.GetChip().DestroyChip();
			}
		}
		
		AnimationAssistant.main.Explode(transform.position, 5, 10);

		yield return new WaitForSeconds(0.1f);

		matching = false;
		
		while (GetComponent<Animation>().isPlaying) yield return 0;
		Destroy(gameObject);
		FieldAssistant.main.BlockCrush(sx, sy, false);
		FieldAssistant.main.JellyCrush(sx, sy);
	}

	
}