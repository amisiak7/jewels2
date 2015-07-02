using UnityEngine;
using System.Collections;

// The class is responsible for logic SimpleBomb
[RequireComponent (typeof (Chip))]
public class SimpleBomb : MonoBehaviour {

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
		chip.chipType = "SimpleBomb";
		birth = SessionAssistant.main.eventCount;
		AudioAssistant.Shot ("CreateBomb");
	}

	// Coroutine destruction / activation
	IEnumerator  DestroyChipFunction (){
		if (birth == SessionAssistant.main.eventCount) {
			chip.destroing = false;
			yield break;
		}
		
		matching = true;
		GetComponent<Animation>().Play("SimpleBump");
		AudioAssistant.Shot("BombCrush");

		int sx = chip.parentSlot.slot.x;
		int sy = chip.parentSlot.slot.y;
		
		chip.ParentRemove();
		
		yield return new WaitForSeconds(0.1f);

		FieldAssistant.main.JellyCrush(sx, sy);

		NeighborMustDie(sx, sy+1);
		NeighborMustDie(sx+1, sy+1);
		NeighborMustDie(sx+1, sy);
		NeighborMustDie(sx+1, sy-1);
		NeighborMustDie(sx, sy-1);
		NeighborMustDie(sx-1, sy-1);
		NeighborMustDie(sx-1, sy);
		NeighborMustDie(sx-1, sy+1);

		AnimationAssistant.main.Explode(transform.position, 3, 25);
		
		yield return new WaitForSeconds(0.1f);
		matching = false;
		
		while (GetComponent<Animation>().isPlaying) yield return 0;
		Destroy(gameObject);
	}
	
	void  NeighborMustDie ( int x ,   int y  ){
		Slot s = FieldAssistant.main.GetSlot(x, y);
		if (s) {
			if (s.GetChip()) {
				s.GetChip().SetScore(0.5f);
				s.GetChip().DestroyChip();
			}
			FieldAssistant.main.BlockCrush(x, y, false);
			FieldAssistant.main.JellyCrush(x, y);
		}
		
	}
}