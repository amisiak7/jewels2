using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Indicator of displaying the remaining time (for Timer game mode)
[RequireComponent (typeof (Text))]
public class Timer : MonoBehaviour {

	Text label;
	
	void  Awake (){
		label = GetComponent<Text> ();
	} 

	void OnEnable() {
		StartCoroutine (TimerRoutine ());
	}

	void OnDisable() {
		StopAllCoroutines ();
	}
	
	void  Update (){
		label.text = ToTimerFormat(Mathf.Max(0, SessionAssistant.main.timeLeft));
	}

	// Refreshing timer coroutine
	IEnumerator TimerRoutine () {
		yield return 0;
		while(SessionAssistant.main.timeLeft > 10)
			yield return new WaitForSeconds(0.1f);
		while (SessionAssistant.main.timeLeft > 0) {
			AudioAssistant.Shot("TimeWarrning");
			yield return new WaitForSeconds(1f);
			}
	}

	// Conversion function of number of seconds into string in MM:SS format
	public static string ToTimerFormat(float t) {
		string f = "";
		float min = Mathf.FloorToInt (t / 60);
		float sec = Mathf.FloorToInt(t - 60f * min);
		f += min.ToString ();
		if (f.Length < 2)
			f = "0" + f;
		f += ":";
		if (sec.ToString().Length < 2)
			f += "0";
		f += sec.ToString ();
		return f;
	}
}