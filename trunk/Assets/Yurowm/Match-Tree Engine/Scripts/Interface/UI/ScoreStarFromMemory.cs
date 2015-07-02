using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Indicator of displaying stars (for a current level)
// The level number is calculated by searching in parent objects a LevelButton component 
[RequireComponent (typeof (Image))]
public class ScoreStarFromMemory : MonoBehaviour {
	
	Image image;
	
	public Sprite oneStar; // Image of one star
	public Sprite twoStar; // Image of two star
	public Sprite threeStar; // Image of three star
	LevelButton button;
	
	void Awake () {
		image = GetComponent<Image> ();
		button = GetComponentInParent<LevelButton> ();
	}
	
	void OnEnable () {
		image.enabled = true;
		switch (HowManyStars ()) {
			case 0: image.enabled = false; break;
			case 1: image.sprite = oneStar; break;
			case 2: image.sprite = twoStar; break;
			case 3: image.sprite = threeStar; break;
		}
	}

	public int HowManyStars () {
		int numberOfStars = 0;
		
		int bestScore = PlayerPrefs.GetInt ("Best_Score_" + button.GetNumber ());
		if (bestScore > button.profile.firstStarScore)
			numberOfStars ++;
		if (bestScore > button.profile.secondStarScore)
			numberOfStars ++;
		if (bestScore > button.profile.thirdStarScore)
			numberOfStars ++;
		
		return numberOfStars;
	}

	public static int HowManyStars (int level) {
		if (!LevelButton.all.ContainsKey(level)) return 0;

		int numberOfStars = 0;
		
		int bestScore = PlayerPrefs.GetInt ("Best_Score_" + level);
		if (bestScore > LevelButton.all[level].profile.firstStarScore)
			numberOfStars ++;
		if (bestScore > LevelButton.all[level].profile.secondStarScore)
			numberOfStars ++;
		if (bestScore > LevelButton.all[level].profile.thirdStarScore)
			numberOfStars ++;
		
		return numberOfStars;
	}

}
