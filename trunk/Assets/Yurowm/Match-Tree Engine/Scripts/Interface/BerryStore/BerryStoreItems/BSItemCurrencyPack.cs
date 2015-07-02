using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Element of shop BerryStore, responsible for currency packs
public class BSItemCurrencyPack : MonoBehaviour {

	public string packID;

	public Text title;
	public Text description;
	public Text priceTag;

	public BerryStorePurchaser buyButton;

	string price = "-$";

	void Start () {
		BerryStoreAssistant.StoreCurrencyPack pack =  null;

		foreach (BerryStoreAssistant.StoreCurrencyPack p in BerryStoreAssistant.main.currencyPacks)
			if (p.id == packID)
				pack = p;
		if (pack != null) {
			priceTag.text = price;
			title.text = pack.name;
			description.text = pack.description;

			buyButton.id = packID;
		} else {
			Debug.LogError("Item " + packID + " is not founded!");
		}
	}

	void OnEnable () {
		if (BerryStoreAssistant.main.marketItems.ContainsKey(packID))
			price = BerryStoreAssistant.main.marketItems[packID];
		priceTag.text = price;
	}
}
