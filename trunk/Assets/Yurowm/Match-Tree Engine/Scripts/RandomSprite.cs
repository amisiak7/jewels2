using UnityEngine;
using System.Collections;

// Set random sprite for SpriteRendere from array
// It is used in chips
[RequireComponent (typeof (SpriteRenderer))]
public class RandomSprite : MonoBehaviour {

	public Sprite[] sprites;
	
	void  Start (){
		int id = Random.Range(0, sprites.Length);
		GetComponent<SpriteRenderer>().sprite = sprites[id];
		Destroy(this);
	}
}