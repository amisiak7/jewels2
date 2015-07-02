using UnityEngine;
using System.Collections;

// The base class for chips
public class Chip : MonoBehaviour {

	public SlotForChip parentSlot; // Slot which include this chip
	public string chipType = "None"; // Chip type name
	public int id; // Chip color ID
	public int powerId; // Chip type ID
	public bool move = false; // is chip involved in the fall (SessionAssistant.main.gravity)
    public bool destroyable = true;
	public int movementID = 0;
	public Vector3 impulse = Vector3.zero;
	Vector3 impulsParent = new Vector3(0,0,-1);
	Vector3 startPosition; 
	Vector3 moveVector;
	public bool destroing = false; // in the process of destruction
	float velocity = 0; // current velocity
	float acceleration = 10f; // acceleration
	static float velocityLimit = 8f; 

	// Colors for each chip color ID
	public static Color[] colors = {
		new Color(0.75f, 0.3f, 0.3f),
		new Color(0.3f, 0.75f, 0.3f),
		new Color(0.3f, 0.5f, 0.75f),
		new Color(0.75f, 0.75f, 0.3f),
		new Color(0.75f, 0.3f, 0.75f),
		new Color(0.75f, 0.6f, 0.3f)
	};
	
	Vector3 lastPosition;
	Vector3 zVector;
	
	void  Awake (){
		velocity = 1;
		move = true;
		SessionAssistant.main.gravity ++;
	}

	// function of conditions of possibility of matching
	public bool IsMatcheble (){
		if (id < 0) return false;
		if (destroing) return false;
		if (SessionAssistant.main.gravity == 0) return true;
		if (move) return false;
		if (transform.position != parentSlot.transform.position) return false;
		if (velocity != 0) return false;

		foreach (Side side in Utils.straightSides)
			if (parentSlot[side]
			&& parentSlot[side].gravity
			&& !parentSlot[side].GetShadow()
			&& !parentSlot[side].GetChip())
				return false;

		return true;
	}

	// function describing the physics of chips
	void  Update () {
		if (destroing) return;
		if (!SessionAssistant.main.isPlaying) return;
		if (impulse != Vector3.zero && (parentSlot || impulsParent.z != -1)) {
			if (impulsParent.z == -1) {
				if (!parentSlot) {
					impulse = Vector3.zero;
					return;
				}
				if (!move) SessionAssistant.main.gravity ++;
				move = true;
				impulsParent = parentSlot.transform.position;
			} 
			transform.position += impulse * Time.deltaTime;
			transform.position += (impulsParent - transform.position) * Time.deltaTime;
			impulse -= impulse * Time.deltaTime;
			impulse -= transform.position - impulsParent;
			impulse *= 1 - 6 * Time.deltaTime;
			if ((transform.position - impulsParent).magnitude < 2 * Time.deltaTime && impulse.magnitude < 2) {
				impulse = Vector3.zero;
				transform.position = impulsParent;
				impulsParent.z = -1;
				if (move) {
					AudioAssistant.Shot("ChipHit");
					SessionAssistant.main.gravity --;
				}

				move = false;
			}
			return;
		}
		
		if (!SessionAssistant.main.CanIGravity()) return;
		if (destroing) return;
		
		if (SessionAssistant.main.matching > 0 && !move) return;
		moveVector.x = 0;
		moveVector.y = 0;
		
		if (parentSlot && transform.position != parentSlot.transform.position) {
			if (!move) {
				move = true;
				SessionAssistant.main.gravity ++;
				velocity = 2;
			}
			
			velocity += acceleration * Time.deltaTime;
			if (velocity > velocityLimit) velocity = velocityLimit;
			
			lastPosition = transform.position;
			
			if (Mathf.Abs(transform.position.x - parentSlot.transform.position.x) < velocity * Time.deltaTime) {
				zVector = transform.position;
				zVector.x = parentSlot.transform.position.x;
				transform.position = zVector;
			}
			if (Mathf.Abs(transform.position.y - parentSlot.transform.position.y) < velocity * Time.deltaTime) {
				zVector = transform.position;
				zVector.y = parentSlot.transform.position.y;
				transform.position = zVector;
			}
			
			if (transform.position == parentSlot.transform.position) {
				parentSlot.SendMessage("GravityReaction");
				if (transform.position != parentSlot.transform.position) 
					transform.position = lastPosition;
			}
			
			if (transform.position.x < parentSlot.transform.position.x)
				moveVector.x = 10;
			if (transform.position.x > parentSlot.transform.position.x)
				moveVector.x = -10;
			if (transform.position.y < parentSlot.transform.position.y)
				moveVector.y = 10;
			if (transform.position.y > parentSlot.transform.position.y)
				moveVector.y = -10;
			moveVector = moveVector.normalized * velocity;
			transform.position += moveVector * Time.deltaTime;
		} else {
			if (move) {
				move = false;
				velocity = 0;
				movementID = SessionAssistant.main.GetMovementID();
				AudioAssistant.Shot("ChipHit");
				SessionAssistant.main.gravity --;
			}
		}
	}

	// returns the value of the potential of the current chips. needs for estimation of solution potential.
	public int GetPotencial (){
		return GetPotencial(powerId);
	}

	// potential depending on powerID
	public static int GetPotencial (int i){
		if (i == 0) return 1; // Simple Chip
		if (i == 1) return 7; // Simple Bomb
		if (i == 2) return 12; // Cross Bomb
		if (i == 3) return 30; // Color Bomb
		return 0;
	}

	// try to move the chips in the desired direction
	public void  Swap (Side side){
        if (!parentSlot) return;
		if (parentSlot[side]) AnimationAssistant.main.SwapTwoItem(this, parentSlot[side].GetChip());
	}

	// separation of the chips from the parent slot
	public void  ParentRemove (){
		if (!parentSlot) return;
		parentSlot.chip = null;
		parentSlot = null;
	}
	
	void  OnDestroy (){
		if (move) SessionAssistant.main.gravity --;
	}

	// Starting the process of destruction of the chips
	public void  DestroyChip (){
        if (!destroyable) return;
		if (destroing) return;
		if (parentSlot.slot.GetBlock ()) {
			parentSlot.slot.GetBlock ().BlockCrush();
			return;
		}
		destroing = true;
		if (id >= 0) SessionAssistant.main.countOfEachTargetCount [id] --;
		SendMessage("DestroyChipFunction", SendMessageOptions.DontRequireReceiver); // It sends a message to another component. It assumes that there is another component to the logic of a specific type of chips
	}

	// Physically destroy the chip without activation and adding score points
	public void  HideChip (){
		if (destroing) return;
		destroing = true;
		if (id == Mathf.Clamp(id, 0, 5))
			SessionAssistant.main.countOfEachTargetCount [id] --;
		ParentRemove();
		Destroy(gameObject);
	}

	// Adding score points
	public void  SetScore (float s){
		if (id < 0 || id > 5 ) return;
		SessionAssistant.main.score += Mathf.RoundToInt(s * SessionAssistant.scoreC);
		ScoreBubble.Bubbling(Mathf.RoundToInt(s * SessionAssistant.scoreC), transform, id);
	}

	// To begin the process of flashing (for hints - SessionAssistant.main.ShowHint)
	public void Flashing (int eventCount){
		StartCoroutine (FlashingUntil (eventCount));
	}

	// Coroutinr of flashing chip until a specified count of events
	IEnumerator  FlashingUntil (int eventCount){
		GetComponent<Animation>().Play("Flashing");
		while (eventCount == SessionAssistant.main.eventCount) yield return 0;
		if (!this) yield break;
		while (GetComponent<Animation>()["Flashing"].time % GetComponent<Animation>()["Flashing"].length > 0.1f) yield return 0;
		GetComponent<Animation>()["Flashing"].time = 0;
		yield return 0;
		GetComponent<Animation>().Stop("Flashing");
	}
}