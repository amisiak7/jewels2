using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Button buy goods from BerryStore
[RequireComponent (typeof (Button))]
public class BerryStorePurchaser : MonoBehaviour {

	public string id; // Item ID

	Button button;

	void Awake () {
		button = GetComponent<Button> ();
		// Adds a new listener on the button
		button.onClick.AddListener (() => {
			OnClick();
		});
	}
	
	void OnClick () {
		BerryStoreAssistant.main.Purchase (id);
	}
}
