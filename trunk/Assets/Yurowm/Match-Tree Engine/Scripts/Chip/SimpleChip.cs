using UnityEngine;
using System.Collections;

// The class is responsible for logic SimpleChip
[RequireComponent (typeof (Chip))]
public class SimpleChip : MonoBehaviour {

	public Chip chip;
	bool mMatching = false;
	public bool matching {
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
		chip.chipType = "SimpleChip";
	}

	// Coroutine destruction / activation
	IEnumerator  DestroyChipFunction (){
		
		matching = true;
		AudioAssistant.Shot("ChipCrush");
		
		yield return new WaitForSeconds(0.1f);
		matching = false;
		
		chip.ParentRemove();


		float velocity = 0;
		Vector3 impuls = new Vector3(Random.Range(-3f, 3f), Random.Range(1f, 5f), 0);
//		if (chip.impulse != Vector3.zero)
			impuls += chip.impulse;
		chip.impulse = Vector3.zero;
		SpriteRenderer sprite = GetComponent<SpriteRenderer>();
		sprite.sortingLayerName = "Foreground";

		SetSortingLayer sLayer = GetComponentInChildren<SetSortingLayer> ();
		sLayer.sortingLayerName = "Foreground";
		sLayer.Refresh ();
		
		float rotationSpeed = Random.Range (-720f, 720f);
		float growSpeed = Random.Range (0.2f, 0.8f);

//		animation.Play("Minimizing");
//		animation ["Minimizing"].speed *= animation ["Minimizing"].length / t;
		while (transform.position.y > -10) {
			velocity += Time.deltaTime * 20;
			velocity = Mathf.Min(velocity, 40);
			transform.position += impuls * Time.deltaTime * transform.localScale.x;
			transform.position -= Vector3.up * Time.deltaTime * velocity * transform.localScale.x;
			transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
			transform.localScale += Vector3.one * growSpeed * Time.deltaTime;
			yield return 0;
		}

		Destroy(gameObject);
	}
}