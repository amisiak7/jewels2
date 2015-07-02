using UnityEngine;
using System.Collections;

// Effect of mixing 2 SimpleBomb
// Уничтожает все фишки в радиусе 3 слотов
public class SimpleSimpleMixEffect : BombMixEffect {
	
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
		AudioAssistant.Shot("BombCrush");
		
		sx = chip.parentSlot.slot.x;
		sy = chip.parentSlot.slot.y;
		
		int width = FieldAssistant.main.field.width;
		int height = FieldAssistant.main.field.height;

		for (int x = 0; x < width; x++)
			for (int y = 0; y < height; y++)
				Crush(x, y);

		AnimationAssistant.main.Explode(transform.position, 5, 30);
		
		yield return new WaitForSeconds(0.6f);
		
		SessionAssistant.main.matching --;
		
		while (GetComponent<Animation>().isPlaying) yield return 0;

		FieldAssistant.main.BlockCrush(sx, sy, false);

		destroingLock = false;
		DestroyChipFunction ();
	}
	
	void Crush(int x, int y) {
		if (!CheckTarget(x, y)) return;
		Slot s;
		GameObject j;
		s = FieldAssistant.main.GetSlot(x, y);
		FieldAssistant.main.BlockCrush(x, y, false);
		j = GameObject.Find("Jelly_" + x + "x" + y);
		if (j) j.SendMessage("JellyCrush", SendMessageOptions.DontRequireReceiver);	
		if (s && s.GetChip()) {
			s.GetChip().SetScore(0.5f);
			s.GetChip().DestroyChip();
		}
	}
	
	bool CheckTarget(int x, int y) {
		return Vector2.Distance(new Vector2(x,y), new Vector2(sx, sy)) <= 3f;
	}
}
