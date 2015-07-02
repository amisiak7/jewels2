// Use this for initialization
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Counter block on the playing field
[RequireComponent (typeof (Text))]
public class BlockCounter : MonoBehaviour {
	
	
	Text label;
	int count = 0;
	float lastUpdate = 0;
	
	void  Awake (){
		label = GetComponent<Text> ();
	} 
	
	void OnEnable () {
		lastUpdate = -9E9f;
		Update ();
	}
	
	void  Update (){
		if (lastUpdate + 0.3f > Time.unscaledTime) return; // Update frequency limiter
		lastUpdate = Time.unscaledTime;
		count = GameObject.FindObjectsOfType<Block> ().Length;
		
		label.text = count.ToString();
	}
}