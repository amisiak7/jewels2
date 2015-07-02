using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Number indicator of the current level
[RequireComponent (typeof (Text))]
public class LevelCounter : MonoBehaviour {

	Text label;
	
	void  Awake (){
		label = GetComponent<Text> ();
	} 
	
	void OnEnable () {
		if (LevelProfile.main != null)
			label.text = "Level " + LevelProfile.main.level.ToString();
		else
			label.text = "";
	}

}