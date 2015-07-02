using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Soomla.Store;

// Class button activation booster
[RequireComponent (typeof (Button))]
public class BoosterButton : MonoBehaviour {

	// booster type
	// Page - showing a special page "value" booster
	// Message - send a message "value" itself. It assumes the presence of a script with the logic of the booster
	public enum BoosterButtonType {Page, Message};
	// Booster ID from Soomla Storage system 
	public string boosterItemId;
	public BoosterButtonType type;
	public string value;
	// Mask of displaying booster depending on the limitation mode
	public Limitation[] limitationMask;

	Button button;
	
	void Awake () {
		button = GetComponent<Button> ();
		button.onClick.AddListener(() => {
			OnClick();
		});
	}

	
	void OnClick () {
		if (StoreInventory.GetItemBalance (boosterItemId) == 0) {
			UIServer.main.ShowPage("Store");
			return;
		}
		if (type == BoosterButtonType.Message)
			SendMessage(value, SendMessageOptions.DontRequireReceiver);
		if (type == BoosterButtonType.Page)
			UIServer.main.ShowPage(value);
	}
}
