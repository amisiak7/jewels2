using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Soomla.Store;

// Сценарий отображения элемента баланса в окружающей среде Soomla
[RequireComponent (typeof (Text))]
public class ItemCounter : MonoBehaviour {

	Text label;
	public string itemID; // Item ID

	void Awake () {
		label = GetComponent<Text> ();
	}
	
	void OnEnable () {
		Refresh (); // Updating when counter is activated
	}

	// Refreshing couter function
	public void Refresh() {
		label.text = StoreInventory.GetItemBalance(itemID).ToString();
	}

	// Refreshing all counters function
	public static void RefreshAll() {
		foreach (ItemCounter counter in GameObject.FindObjectsOfType<ItemCounter> ())
			counter.Refresh();
		foreach (ItemMask mask in GameObject.FindObjectsOfType<ItemMask> ())
			mask.Refresh();
	}
}