using UnityEngine;

// Pause object
public class GamePauseObject : MonoBehaviour {

	static int activeObjectsCount = 0; // Count of active objects of this type. If set to null, the game is paused.

	void OnEnable () {
		activeObjectsCount ++; 
		PauseUpdate ();
	}

	void OnDisable() {
		activeObjectsCount --;
		PauseUpdate ();
	}

	void PauseUpdate() {
		if (UIServer.main != null)
			UIServer.main.SetPause (activeObjectsCount == 0);
	}
}
