using UnityEngine;
using System.Collections;

// scaling background, depending on the size of the camera
public class BackgroundScaler : MonoBehaviour {

	public Camera targetCamera;
	public float minSize = 5;
	public float multiplier = 0.2f;
	
	void LateUpdate () {
		transform.localScale = Vector3.one * Mathf.Max (minSize, targetCamera.orthographicSize) * multiplier;
	}
}
