using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// received stars indicator (for current level and current score)
[RequireComponent (typeof (Image))]
public class ScoreStar : MonoBehaviour {

	Image image;
	float lastUpdate = 0;
	bool filled = false;

	public Sprite fullStar; // Image of received star
	public Sprite emptyStar; // Image of unreceived star
	public StarType starType; // displaying star type - first, second, third
	public bool fromCurrentScore = true;

	void Awake () {
		image = GetComponent<Image> ();
	}

	void OnEnable () {
		lastUpdate = 0;
		filled = false;
		image.sprite = emptyStar;
	}

	void Update () {
		if (filled) return;
		if (lastUpdate + 0.5f > Time.unscaledTime) return;
		lastUpdate = Time.unscaledTime;
		float target = 0;
		switch (starType) {
			case StarType.First: target = LevelProfile.main.firstStarScore; break;
			case StarType.Second: target = LevelProfile.main.secondStarScore; break;
			case StarType.Third: target = LevelProfile.main.thirdStarScore; break;
		}

		if (fromCurrentScore)
			filled = target <= SessionAssistant.main.score;
		else
			filled = target <= PlayerPrefs.GetInt ("Best_Score_" + LevelProfile.main.level.ToString());

		if (filled)
			image.sprite = fullStar;
	}
}
