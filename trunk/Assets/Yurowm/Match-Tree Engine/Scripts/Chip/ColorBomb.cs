using UnityEngine;
using System.Collections;

// The class is responsible for logic ColorBomb
[RequireComponent (typeof (Chip))]
public class ColorBomb : MonoBehaviour {

	Chip chip;
	int birth; // Event count at the time of birth SessionAssistant.main.eventCount
	public Color color;
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
		chip.chipType = "ColorBomb";
		birth = SessionAssistant.main.eventCount;
		AudioAssistant.Shot ("CreateColorBomb");
	}

	// Coroutine destruction / activation
	IEnumerator  DestroyChipFunction (){
		if (birth == SessionAssistant.main.eventCount) {
			chip.destroing = false;
			yield break;
		}
		
		matching = true;

		GetComponent<Animation>().Play("ColorBump");
		AudioAssistant.Shot("ColorBombCrush");
		
		
		int width = FieldAssistant.main.field.width;
		int height = FieldAssistant.main.field.height;
		
		int sx = chip.parentSlot.slot.x;
		int sy = chip.parentSlot.slot.y;
		
		Slot s;

		FieldAssistant.main.JellyCrush(sx, sy);

		chip.ParentRemove();

		for (int x= 0; x < width; x++) {
			for (int y= 0; y < height; y++) {
				if (y == sy && x == sx) continue;
				s = FieldAssistant.main.GetSlot(x, y);
				if (s && s.GetChip() && s.GetChip().id == chip.id) {
					Lightning.CreateLightning(3, transform, s.GetChip().transform, color);
				}
			}
		}
		
		yield return new WaitForSeconds(0.3f);
		
		for (int x1= 0; x1 < width; x1++) {
			for (int y1= 0; y1 < height; y1++) {
				if (y1 == sy && x1 == sx) continue;
				s = FieldAssistant.main.GetSlot(x1, y1);
				if (s && s.GetChip() && s.GetChip().id == chip.id) {
					s.GetChip().SetScore(1);
					FieldAssistant.main.BlockCrush(x1, y1, true);
					FieldAssistant.main.JellyCrush(x1, y1);
					s.GetChip().DestroyChip();
				}
			}
		}
		
		yield return new WaitForSeconds(0.1f);
		matching = false;
		
		while (GetComponent<Animation>().isPlaying) yield return 0;
		Destroy(gameObject);
	}
}