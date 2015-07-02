using UnityEngine;
using System.Collections;

// Jelly element on playing field
public class Jelly : MonoBehaviour {

	public int level = 1; // Level of jelly. From 1 to 3. Each "JellyCrush"-call fall level by one. If it becomes zero, this jelly will be destroyed.
	public Sprite[] sprites; // Images of jellies of different levels. The size of the array must be equal to 3
	SpriteRenderer sr;
	
	void  Start (){
		sr = GetComponent<SpriteRenderer>();
		sr.sprite = sprites[level-1];
	}

	// Crush block funtion
	void  JellyCrush (){
		GameObject o = ContentAssistant.main.GetItem ("JellyCrush");
		o.transform.position = transform.position;
		if (level == 1) {
			Destroy(gameObject);
			return;
		}
		level --;
		sr.sprite = sprites[level-1];
	}
}