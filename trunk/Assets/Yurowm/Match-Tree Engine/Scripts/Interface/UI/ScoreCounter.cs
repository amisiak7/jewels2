using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// current score indicator 
[RequireComponent (typeof (Text))]
public class ScoreCounter : MonoBehaviour {
	
	
	Text label;
	public float updateSpeed = 1f;

	int target = 0;
	int current = 0;
	
	void  Awake (){
		label = GetComponent<Text> ();
	} 

	void OnEnable () {
		current = SessionAssistant.main.score;
	}

	void  Update (){
		target = SessionAssistant.main.score;
		current = Mathf.CeilToInt(Mathf.MoveTowards (current, target, Time.deltaTime * (target - current) * updateSpeed));
		label.text = current.ToString();
	}
}