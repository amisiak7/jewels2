using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Target score indicator (from the first star score)
[RequireComponent (typeof (Text))]
public class TargetScoreCounter : MonoBehaviour {

	Text label;
	
	void  Awake (){
		label = GetComponent<Text> ();
	} 
	
	void  OnEnable (){
		if (LevelProfile.main != null)
			label.text = LevelProfile.main.firstStarScore.ToString();
	}
}