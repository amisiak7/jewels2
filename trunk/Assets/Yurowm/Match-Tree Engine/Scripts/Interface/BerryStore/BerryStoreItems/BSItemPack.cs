using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Element of shop BerryStore, responsible for item packs
public class BSItemPack : MonoBehaviour {

	public string packID;

	public Text title;
	public Text description;
	public Text priceTag1;
	public Text priceTag2;

	public BerryStorePurchaser buyButton;
	public ItemMask buyButtonMask;
	public Transform priseDollars;
	public Transform priseSeeds;
	public bool IAP = false;

	public ItemCounter counter;
	public ItemMask counterMask;
	
	string price = "-$";

	void Start () {
		BerryStoreAssistant.StoreItemPack pack =  null;

		foreach (BerryStoreAssistant.StoreItemPack p in BerryStoreAssistant.main.itemPacks)
			if (p.id == packID)
				pack = p;
		if (pack != null) {
			title.text = pack.name;
			description.text = pack.description;

			buyButton.id = pack.id;

			if (IAP) {
				priceTag1.text = pack.price.ToString() + "$";
				Destroy(priseSeeds.gameObject);
				Destroy(buyButtonMask);
				Destroy(priceTag2.gameObject);
			} else {
				priceTag2.text = pack.price.ToString();
				Destroy(priseDollars.gameObject);
				buyButtonMask.itemID = "seed";
				buyButtonMask.mustBe = ItemMask.ComparisonOperator.EqualGreater;
				buyButtonMask.value = pack.price;
				Destroy(priceTag1.gameObject);
			}

			counterMask.itemID = pack.itemId;
			counterMask.mustBe = ItemMask.ComparisonOperator.Greater;
			counterMask.value = 0;

			counter.itemID = pack.itemId;
		}
	}

	void OnEnable () {
		if (BerryStoreAssistant.main.marketItems.ContainsKey(packID))
			price = BerryStoreAssistant.main.marketItems[packID];
		priceTag1.text = price;
	}
}
