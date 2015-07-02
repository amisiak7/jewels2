using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent (typeof (Text))]
public class TargetDescriptionDisplay : MonoBehaviour {
	
	Text text;
	
	void Awake () {
		text = GetComponent<Text> ();	
	}
	
	void OnEnable () {
		if (LevelProfile.main == null) {
			text.text = "";
			return;
		}
		string descrition = "";

		switch (LevelProfile.main.target) {
			case FieldTarget.Score: descrition += "You should to get " + LevelProfile.main.firstStarScore.ToString() + " score points."; break;
			case FieldTarget.Jelly: descrition += "You should to destroy all jellies in this level."; break;
			case FieldTarget.Block: descrition += "You should to destroy all blocks in this level."; break;
			case FieldTarget.Color: descrition += "You should to destroy the certain count of chips of certain color"; break;
            case FieldTarget.SugarDrop: descrition += "You should to drop all sugar chips"; break;
        }

		descrition += " ";

		switch (LevelProfile.main.limitation) {
		case Limitation.Moves: descrition += "You have only " + LevelProfile.main.moveCount.ToString() + " moves for doing this."; break;
		case Limitation.Time: descrition += "You have only " + Timer.ToTimerFormat(LevelProfile.main.duraction) + " of time for doing this."; break;
		}

		text.text = descrition;
	}
}
