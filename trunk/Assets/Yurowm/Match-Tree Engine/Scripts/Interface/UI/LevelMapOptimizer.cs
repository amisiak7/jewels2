using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class LevelMapOptimizer : MonoBehaviour {

    public ScrollRect scrollRect;
    public int offset = 50; 
    List<LevelButton> levels = new List<LevelButton>();

	void Awake () {
        levels = new List<LevelButton>(GetComponentsInChildren<LevelButton>());
        levels.Sort(delegate (LevelButton a, LevelButton b) {
            return (int) Mathf.Sign(a.transform.position.y - b.transform.position.y);
        });
        scrollRect.onValueChanged.AddListener(OnScroll);
	}

    void OnEnable() {
        for (int i = 0; i < levels.Count; i++) {
            levels[i].ShowItIfItCurrentLevel();
        }
    }

    private void OnScroll(Vector2 arg0) {
        int state = 0;
        for (int i = 0; i < levels.Count; i++) {
            switch (state) {
                case 0:
                    if (levels[i].transform.position.y < -offset)
                        levels[i].gameObject.SetActive(false);
                    else {
                        state++;
                        i--;
                    }
                    break;
                case 1:
                    if (levels[i].transform.position.y < Screen.height + offset)
                        levels[i].gameObject.SetActive(true);
                    else {
                        state++;
                        i--;
                    }
                    break;
                case 2:
                    levels[i].gameObject.SetActive(false);
                    break;
            }
        }
    }
}
