using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// The script is responsible for the logic of the progress bar of game score
[RequireComponent (typeof (Slider))]
public class ScoreBar : MonoBehaviour {

	Slider slider;

	float target = 0;
	float current = 0;
	int max = 0;

	void Awake () {
		slider = GetComponent<Slider> ();
	}

	void OnEnable () {
		if (SessionAssistant.main == null) return;
		current = SessionAssistant.main.score;
		if (LevelProfile.main != null)
			max = LevelProfile.main.thirdStarScore;
	}

	void Update () {
		target = SessionAssistant.main.score;
		current = Mathf.CeilToInt(Mathf.MoveTowards (current, target, Time.deltaTime * (target - current) / 3));
		slider.value = current / max;
	}
}
