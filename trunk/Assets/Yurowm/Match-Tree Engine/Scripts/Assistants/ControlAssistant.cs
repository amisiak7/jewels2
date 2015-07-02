using UnityEngine;
using System.Collections;

public class ControlAssistant : MonoBehaviour {
	
	
	public static ControlAssistant main;
	RaycastHit2D hit;
	public Camera controlCamera;
	
	Chip pressedChip;
	Vector2 pressPoint;
	
	bool isMobilePlatform;
	
	void  Awake (){
		main = this;
		isMobilePlatform = Application.isMobilePlatform;
	}
	
	void  Update (){
		if (Time.timeScale == 0) return;
		if (isMobilePlatform)
			MobileUpdate();
		else 
			DecktopUpdate();
	}
	
	// control function for mobile devices
	void  MobileUpdate (){
		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
			Vector2 point = controlCamera.ScreenPointToRay(Input.GetTouch(0).position).origin;
			hit = Physics2D.Raycast(point, Vector2.zero);
			if (!hit.transform) return;
			pressedChip = hit.transform.GetComponent<Chip>();
			pressPoint = Input.GetTouch(0).position;
		}
		if (Input.touchCount > 0 && pressedChip) {
			Vector2 move = Input.GetTouch(0).position - pressPoint;
			if (move.magnitude > Screen.height * 0.05f) {
				foreach (Side side in Utils.straightSides)
					if (Vector2.Angle(move, Utils.SideOffsetX(side) * Vector2.right + Utils.SideOffsetY(side) * Vector2.up) <= 45)
						pressedChip.Swap(side);
				pressedChip = null;
			}
		}
	}
	
	// Control function for stationary platforms
	void  DecktopUpdate (){
		if (Input.GetMouseButtonDown(0)) {
			Vector2 point = controlCamera.ScreenPointToRay(Input.mousePosition).origin;
			hit = Physics2D.Raycast(point, Vector2.zero);
			if (!hit.transform) return;
			pressedChip = hit.transform.GetComponent<Chip>();
			pressPoint = Input.mousePosition; 
		}
		if (Input.GetMouseButton(0) && pressedChip != null) {
			Vector2 move = Input.mousePosition;
			move -= pressPoint;
			if (move.magnitude > Screen.height * 0.05f) {
				foreach (Side side in Utils.straightSides)
					if (Vector2.Angle(move, Utils.SideOffsetX(side) * Vector2.right + Utils.SideOffsetY(side) * Vector2.up) <= 45)
						pressedChip.Swap(side);
				pressedChip = null;
			}
		}
	}
	
	public Chip GetChipFromTouch() {
		Vector2 point;
		if (isMobilePlatform) {
			if (Input.touchCount == 0) return null;
			point = controlCamera.ScreenPointToRay(Input.GetTouch(0).position).origin;
		} else 
			point = controlCamera.ScreenPointToRay(Input.mousePosition).origin;
		
		hit = Physics2D.Raycast(point, Vector2.zero);
		if (!hit.transform) return null;
		return hit.transform.GetComponent<Chip>();
	}
}