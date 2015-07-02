using UnityEngine;
using System.Collections;

// Management of the main camera
public class FieldCamera : MonoBehaviour {

	public static FieldCamera main;
	float defaultFOV = 2;

	void Awake() {
		main = this;
		transform.position = new Vector3(0, 7, -10);
		GetComponent<Camera>().orthographicSize = defaultFOV;
	}

	// Switching to the display of the playing field
	public void ShowField (){
		StartCoroutine (ShowFieldRoutine ());
	}

	// Switching to display the game menu
	public void HideField (){
		StartCoroutine (HideFieldRoutine ());
	}
		
	// Coroutine of displaying of field
	public IEnumerator ShowFieldRoutine ()
	{
		float t = 0;
		float width = FieldAssistant.main.field.width * 0.37f * Screen.height / Screen.width;
		float height = FieldAssistant.main.field.height * 0.45f;
		float targetSize = width > height ? width : height;
		float size = GetComponent<Camera>().orthographicSize;

		Vector3 position = transform.position;
		while (t < 1) {
			t += (-Mathf.Abs(0.5f - t) + 0.5f + 0.05f) * Time.unscaledDeltaTime * 6;
			transform.position = Vector3.Lerp(position, new Vector3(0, 0, -10), t);
			GetComponent<Camera>().orthographicSize = Mathf.Lerp(size, targetSize, t);
			yield return 0;
		}
	}

	// Coroutine of displaying of game menu
	public IEnumerator HideFieldRoutine ()
	{
		float t = 0;

		float targetSize = defaultFOV;
		float size = GetComponent<Camera>().orthographicSize;
		Vector3 position = transform.position;

		while (t < 1) {
			t += (-Mathf.Abs(0.5f - t) + 0.5f + 0.05f) * Time.unscaledDeltaTime * 6;
			transform.position = Vector3.Lerp(position, new Vector3(0, 7, -10), t);
			GetComponent<Camera>().orthographicSize = Mathf.Lerp(size, targetSize, t);
			yield return 0;
		}
	}
}