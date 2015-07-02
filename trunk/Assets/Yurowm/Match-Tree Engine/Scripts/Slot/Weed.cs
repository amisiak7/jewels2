using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Weed : BlockInterface {

	public static List<Weed> all = new List<Weed>();

	int eventCountBorn;

	bool destroying = false;

	public static int lastSwapWithCrush = 0;

	void Start () {
		transform.rotation = Quaternion.Euler (0, 0, Random.Range (0f, 360f));	
	}

	public void Initialize (){
		slot.gravity = false;
		eventCountBorn = SessionAssistant.main.eventCount;
		all.Add (this);
	}	

	#region implemented abstract members of BlockInterface
	
	// Crush block funtion
	override public void  BlockCrush () {
		if (eventCountBorn == SessionAssistant.main.eventCount) return;
		if (destroying) return;

		lastSwapWithCrush = SessionAssistant.main.swapEvent;

		destroying = true;
		eventCountBorn = SessionAssistant.main.eventCount;
		GameObject o = ContentAssistant.main.GetItem ("WeedCrush");
		o.transform.position = transform.position;

		slot.gravity = true;
		SlotGravity.Reshading();
		Destroy(gameObject);
		return;
	}

	public override bool CanBeCrushedByNearSlot () {
		return true;
	}

	#endregion

	void OnDestroy () {
		all.Remove (this);
	}

	public static void Grow () {
		List<Slot> slots = new List<Slot> ();

		foreach (Weed weed in all)
			foreach (Side side in Utils.straightSides)
                if (weed.slot[side] && !weed.slot[side].GetBlock() && !(weed.slot[side].GetChip() && weed.slot[side].GetChip().chipType == "SugarChip"))
					slots.Add(weed.slot[side]);

		if (slots.Count == 0) return;

		Slot target = slots[Random.Range(0, slots.Count)];

		if (target.GetChip())
			target.GetChip().HideChip();

		Weed newWeed = ContentAssistant.main.GetItem<Weed>("Weed");
		newWeed.transform.position = target.transform.position;
		newWeed.name = "New_Weed";
		newWeed.transform.parent = target.transform;
		target.SetBlock(newWeed);
		newWeed.slot = target;
		newWeed.Initialize();

		ContentAssistant.main.GetItem ("WeedCrush", newWeed.transform.position);
	}
}