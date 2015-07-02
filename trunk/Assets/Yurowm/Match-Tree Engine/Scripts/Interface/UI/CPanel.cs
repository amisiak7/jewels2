using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// An important element of UI. It combines elements of the interface closest to the destination.
public class CPanel : MonoBehaviour {

	public static int uiAnimation = 0;

	public string hide; // Name of showing animation
	public string show; // Name of hiding animation

	private string currentClip = "";

	// Function of showing or hiding this panel. With running the appropriate animation.
	public void SetActive (bool a) {
		if (gameObject.activeSelf == a) return;
		currentClip = "";
		if (!a) {
			if (hide != "")
				currentClip = hide;
			else {
				gameObject.SetActive(false);
				return;
			}
		}
		if (a) {
			gameObject.SetActive(true);
			if (show != "")
				currentClip = show;
			else return;
			Update();
		}
		if (currentClip == "") return;
		GetComponent<Animation>().Play(currentClip);
		GetComponent<Animation>()[currentClip].time = 0;
		uiAnimation ++;
	}

	// animating the panel regardless of the time settings
	void Update () {
		if (currentClip == "") return;

		GetComponent<Animation>()[currentClip].time += Mathf.Min (Time.unscaledDeltaTime, Time.maximumDeltaTime);
		GetComponent<Animation>()[currentClip].enabled = true;
		GetComponent<Animation>().Sample();
		GetComponent<Animation>()[currentClip].enabled = false;

		if (GetComponent<Animation>() [currentClip].time >= GetComponent<Animation>() [currentClip].length) {
			if (currentClip == hide)
				gameObject.SetActive(false);
			currentClip = "";
			uiAnimation --;
		}
	}
}
	
