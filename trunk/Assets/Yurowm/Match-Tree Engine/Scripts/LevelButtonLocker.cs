using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent (typeof (LevelButton))]
[RequireComponent (typeof (Button))]
public class LevelButtonLocker : MonoBehaviour {

	LevelButton level;
	Button button;
	int num = 0;
	public bool alwaysUnlocked = false; // if true, will be unlocked always
	public GameObject[] lockedElements; // Elements that appear only when the level is locked
	public GameObject[] unlockedElements; // Elements that appear only when the level is unlocked


	void Awake () {
		level = GetComponent<LevelButton> ();
		button = GetComponent<Button> ();
	}

	void OnEnable () {
		num = level.GetNumber ();
        bool l = IsLocked();

        foreach (GameObject go in lockedElements)
			go.SetActive (l);
		foreach(GameObject go in unlockedElements)
			go.SetActive (!l);
		button.enabled = !l;
	}

    public bool IsLocked() {
        return !alwaysUnlocked && num > 1 && PlayerPrefs.GetInt("Complete_" + (num - 1).ToString()) == 0; // terms of locking
    }

	// Unlock all levels
	public static void UnlockAllLevels() {
		for (int i = 1; i <= LevelButton.all.Count; i++)
			PlayerPrefs.SetInt ("Complete_" + i.ToString (), 1);
	}
}
