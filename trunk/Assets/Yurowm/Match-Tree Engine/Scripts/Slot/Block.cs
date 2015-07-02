using UnityEngine;
using System.Collections;

// Destroyable blocks on playing field
public class Block : BlockInterface {

	public int level = 1; // Level of block. From 1 to 3. Each "BlockCrush"-call fall level by one. If it becomes zero, this block will be destroyed.
	public Sprite[] sprites; // Images of blocks of different levels. The size of the array must be equal to 3
	SpriteRenderer sr;
	int eventCountBorn;

	public void Initialize (){
		slot.gravity = false;
		sr = GetComponent<SpriteRenderer>();
		eventCountBorn = SessionAssistant.main.eventCount;
		sr.sprite = sprites[level-1];
	}

	
	#region implemented abstract members of BlockInterface
	
	// Crush block funtion
	override public void  BlockCrush () {
		if (eventCountBorn == SessionAssistant.main.eventCount) return;
		eventCountBorn = SessionAssistant.main.eventCount;
		GameObject o = ContentAssistant.main.GetItem ("BlockCrush");
		o.transform.position = transform.position;
		level --;
		FieldAssistant.main.field.blocks [slot.x, slot.y] = level;
		if (level == 0) {
			slot.gravity = true;
			SlotGravity.Reshading();
			Destroy(gameObject);
			return;
		}
		if (level > 0) {
			GetComponent<Animation>().Play("BlockCrush");
			sr.sprite = sprites[level-1];
		}
	}

	public override bool CanBeCrushedByNearSlot () {
		return true;
	}

	#endregion
}