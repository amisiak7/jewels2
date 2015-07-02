using UnityEngine;
using System.Collections;

public class AnimationAssistant : MonoBehaviour {
	// This script is responsible for procedural animations in the game. Such as change of place 2 chips and the effect of the explosion.
	
	public static AnimationAssistant main; // Main instance. Need for quick access to functions.
	void  Awake (){
		main = this;
	}

	float swapDuration = 0.2f;

	// Temporary Variables
	bool swaping = false; // ИTRUE when the animation plays swapping 2 chips
	[HideInInspector]
	public bool forceSwap = false; // Forced change of place of chips, ignoring matching.


	void Start() {
		BombMixEffect.Initialize (); // Initialization recipes mixing bombs
	}

	// Function immediate swapping 2 chips
	public void SwapTwoItemNow (Chip a, Chip b) {
		if (!a || !b) return;
		if (a == b) return;
		if (a.parentSlot.slot.GetBlock() || b.parentSlot.slot.GetBlock()) return;

		Vector3 posA = a.parentSlot.transform.position;
		Vector3 posB = b.parentSlot.transform.position;
		
		a.transform.position = posB;
		b.transform.position = posA;
		
		a.movementID = SessionAssistant.main.GetMovementID();
		b.movementID = SessionAssistant.main.GetMovementID();
		
		SlotForChip slotA = a.parentSlot;
		SlotForChip slotB = b.parentSlot;
		
		slotB.SetChip(a);
		slotA.SetChip(b);
	}

	// The function of swapping 2 chips
	public void SwapTwoItem (Chip a, Chip b) {
		if (!SessionAssistant.main.isPlaying) return;
		StartCoroutine (SwapTwoItemRoutine(a, b)); // Starting corresponding coroutine
	}


	// Coroutine swapping 2 chips
	IEnumerator SwapTwoItemRoutine (Chip a, Chip b){
		// cancellation terms
		if (swaping) yield break; // If the process is already running
		if (!a || !b) yield break; // If one of the chips is missing
		if (a.parentSlot.slot.GetBlock() || b.parentSlot.slot.GetBlock()) yield break; // If one of the chips is blocked

		if (!SessionAssistant.main.CanIAnimate()) yield break; // If the core prohibits animation
		switch (LevelProfile.main.limitation) {
			case Limitation.Moves:
				if (SessionAssistant.main.movesCount <= 0) yield break; break; // If not enough moves
			case Limitation.Time:
				if (SessionAssistant.main.timeLeft <= 0) yield break; break; // If not enough time
		}

		bool mix = false; // The effect of mixing or not
		if (BombMixEffect.ContainsPair (a.chipType, b.chipType)) // Checking the possibility of mixing
						mix = true;

		int move = 0; // Number of points movement which will be expend
		
		SessionAssistant.main.animate++;
		swaping = true;
		
		Vector3 posA = a.parentSlot.transform.position;
		Vector3 posB = b.parentSlot.transform.position;
		
		float progress = 0;

        Vector3 normal = a.parentSlot.slot.x == b.parentSlot.slot.x ? Vector3.right : Vector3.up;

		// Animation swapping 2 chips
		while (progress < swapDuration) {
            a.transform.position = Vector3.Lerp(posA, posB, progress / swapDuration) + normal * Mathf.Sin(3.14f * progress / swapDuration) * 0.2f;
			if (!mix) b.transform.position = Vector3.Lerp(posB, posA, progress/swapDuration) - normal * Mathf.Sin(3.14f * progress / swapDuration) * 0.2f;
			
			progress += Time.deltaTime;
			
			yield return 0;
		}
		
		a.transform.position = posB;
		if (!mix) b.transform.position = posA;
		
		a.movementID = SessionAssistant.main.GetMovementID();
		b.movementID = SessionAssistant.main.GetMovementID();
		
		if (mix) { // Scenario mix effect
			swaping = false;
			BombPair pair = new BombPair(a.chipType, b.chipType);
			SlotForChip slot = b.parentSlot;
			a.HideChip();
			b.HideChip();
			BombMixEffect.Mix(pair, slot);
			SessionAssistant.main.movesCount--;
			SessionAssistant.main.animate--;
			yield break;
		}

		// Scenario the effect of swapping two chips
		SlotForChip slotA = a.parentSlot;
		SlotForChip slotB = b.parentSlot;
		
		slotB.SetChip(a);
		slotA.SetChip(b);
		
		
		move++;

		// searching for solutions of matching
		int count = 0; 
		SessionAssistant.Solution solution;
		
		solution = slotA.MatchAnaliz();
		if (solution != null) count += solution.count;
		
		solution = slotB.MatchAnaliz();
		if (solution != null) count += solution.count;
		
		// Scenario canceling of changing places of chips
		if (count == 0 && !forceSwap) {
			AudioAssistant.Shot("SwapFailed");
			while (progress > 0) {
                a.transform.position = Vector3.Lerp(posA, posB, progress / swapDuration) - normal * Mathf.Sin(3.14f * progress / swapDuration) * 0.2f;
                b.transform.position = Vector3.Lerp(posB, posA, progress / swapDuration) + normal * Mathf.Sin(3.14f * progress / swapDuration) * 0.2f;
				
				progress -= Time.deltaTime;
				
				yield return 0;
			}
			
			a.transform.position = posA;
			b.transform.position = posB;
			
			a.movementID = SessionAssistant.main.GetMovementID();
			b.movementID = SessionAssistant.main.GetMovementID();
			
			slotB.SetChip(b);
			slotA.SetChip(a);
			
			move--;
		} else {
			AudioAssistant.Shot("SwapSuccess");
			SessionAssistant.main.swapEvent ++;
		}

        SessionAssistant.main.firstChipGeneration = false;

		SessionAssistant.main.movesCount -= move;
		SessionAssistant.main.EventCounter ();
		
		SessionAssistant.main.animate--;
		swaping = false;
	}

	// Function of creating of explosion effect
	public void  Explode (Vector3 center, float radius, float force){
		Chip[] chips = GameObject.FindObjectsOfType<Chip>();
		Vector3 impuls;
		foreach(Chip chip in chips) {
			if ((chip.transform.position - center).magnitude > radius) continue;
			impuls = (chip.transform.position - center) * force;
			impuls *= Mathf.Pow((radius - (chip.transform.position - center).magnitude) / radius, 2);
			chip.impulse += impuls;
		}
	}

	public void TeleportChip(Chip chip, Slot target) {
		StartCoroutine (TeleportChipRoutine (chip, target));
	}

	IEnumerator TeleportChipRoutine (Chip chip, Slot target) {
		if (!chip.parentSlot) yield break;

		TrailRenderer trail = chip.gameObject.GetComponentInChildren<TrailRenderer> ();
		float trailTime = 0;
		
		if (trail) {
			trailTime = trail.time;
			trail.time = 0;
		}

		float defScale = chip.transform.localScale.x;
		float scale = defScale;

//		chip.animation.Play("Minimizing");
//
//		while (chip.animation.isPlaying) {
//			yield return 0;
//		}

		while (scale > 0f) {
			scale -= Time.deltaTime * 10f;
			chip.transform.localScale = Vector3.one * scale;
			yield return 0;
		}


		if (!target.GetChip () && chip && chip.parentSlot) {
			Transform a = chip.parentSlot.transform;
			Transform b = target.transform;

            Color color;
            if (chip.id == Mathf.Clamp(chip.id, 0, 5))
                color = Chip.colors[chip.id];
            else
                color = Color.white;

            Lightning l = Lightning.CreateLightning(5, a, b, color);

			target.SetChip(chip);

			chip.transform.position = chip.parentSlot.transform.position;
			yield return 0;
			l.end = null;
		}

		yield return 0;


		if (trail) {
			trail.time = trailTime;
		}

		scale = 0.2f;
		while (scale < defScale) {
			scale += Time.deltaTime * 10f;
			chip.transform.localScale = Vector3.one * scale;
			yield return 0;
		}
		
		chip.transform.localScale = Vector3.one * defScale;
	}
}