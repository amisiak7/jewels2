using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Soomla.Store;

// Implementation of Soomla Store plugin
public class BerryStoreAssistant : MonoBehaviour {
	 
	[ContextMenu ("Clear Data")]
	public void ClearData() {
		List<string> ids = new List<string> ();
		ids.Add ("seed");

		foreach (StoreItem i in items) ids.Add(i.id);

		foreach (string id in ids)
			StoreInventory.TakeItem(id, StoreInventory.GetItemBalance(id));

		PlayerPrefs.DeleteAll ();
	}

	// Add random number seed
	public void AddSomeSeeds() {
		StoreInventory.GiveItem("seed", Random.Range(10, 100));
		AudioAssistant.Shot ("Buy");
	}

	public static BerryStoreAssistant main;
	public Dictionary<string, string> marketItems = new Dictionary<string, string>();

	// Descriptions of goods
	public StoreItem[] items;
	public StoreItemPack[] itemPacks;
	public StoreCurrencyPack[] currencyPacks;

	void Awake () {
		main = this;
	}

	void Start () {
		SoomlaStore.Initialize(new BerryStoreAssets());
		StoreEvents.OnCurrencyBalanceChanged += onCurrencyBalanceChanged;
		StoreEvents.OnItemPurchased += onItemPurchased;
		StoreEvents.OnGoodBalanceChanged += onGoodBalanceChanged;
		StoreEvents.OnMarketPurchase += onMarketPurchase;
		StoreEvents.OnNotEnoughTargetItem += onNotEnoughTargetItem;
		StoreEvents.OnMarketItemsRefreshFinished += onMarketItemsRefreshFinished;
	}

	void onMarketItemsRefreshFinished (List<MarketItem> items) {
		foreach (MarketItem item in items)
			marketItems.Add(item.ProductId, item.MarketPriceAndCurrency);
	}

	// Function item purchase
	string developerPayload = "BerryMatchTreePayload";
	public void Purchase (string id)
	{
		StoreInventory.BuyItem (id, developerPayload);
	}

	// updating counters, when balance of currency changed
	void onCurrencyBalanceChanged(VirtualCurrency virtualCurrency, int balance, int amountAdded) {
		ItemCounter.RefreshAll ();
	}

	// When insufficient funds, page opens shop
	void onNotEnoughTargetItem (VirtualItem item)
	{
		if (item.ItemId == "seed")
			UIServer.main.ShowPage ("Store");
		StoreInventory.GetItemBalance (item.ID);
	}

	// updating counters, when balance of goods changed
	void onGoodBalanceChanged (VirtualGood good, int balance, int amountAdded)
	{		
		ItemCounter.RefreshAll ();
	}

	// Play of successfully purchased sound
	void onItemPurchased (PurchasableVirtualItem item, string payload)
	{
		if (payload != developerPayload) return;
		AudioAssistant.Shot ("Buy");
	}

	// Play of successfully purchased sound
	void onMarketPurchase (PurchasableVirtualItem item, string payload, Dictionary<string, string> extra)
	{
		if (payload != developerPayload) return;
		AudioAssistant.Shot ("Buy");
	}


	//Classes with description of goods
	[System.Serializable]
	public class StoreItem {

		public string name;
		public string id;
		public string description;
		public float price;
		public Currency currency;
		public string sku;
		public bool consumable = true;

		public VirtualGood Complete() {
			VirtualGood good;
			PurchaseType purchase = null;

			if (currency == Currency.Dollars)
				purchase = new PurchaseWithMarket(sku, price);
			if (currency == Currency.Seeds)
				purchase = new PurchaseWithVirtualItem("seed", Mathf.CeilToInt(price));

			if (consumable)
				good = new SingleUseVG(name, description, id, purchase);
			else
				good = new LifetimeVG(name, description, id, purchase);
			return good;
		}
		public enum Currency {Dollars, Seeds};
	}

	[System.Serializable]
	public class StoreItemPack {
		
		public string name;
		public string id;
		public string description;
		public string itemId;
		public int itemCount;
		public int price;
		public StoreItem.Currency currency;
		public string sku;

		public VirtualGood Complete ()
		{
			PurchaseType purchase;
			
			if (currency == StoreItem.Currency.Dollars)
				purchase = new PurchaseWithMarket(sku, price);
			else
				purchase = new PurchaseWithVirtualItem("seed", Mathf.CeilToInt(price));

			return new SingleUsePackVG (itemId, itemCount, name, description, id, purchase);
		}
	}

	[System.Serializable]
	public class StoreCurrencyPack {
		
		public string name;
		public string id;
		public string description;
		public int count;
		public int price;
		public string sku;

		public VirtualCurrencyPack Complete () {
			return new VirtualCurrencyPack (name, description, id, count, "seed", new PurchaseWithMarket(sku, price));
		}
	}
}

public class BerryStoreAssets : IStoreAssets {

	#region IStoreAssets implementation
	public int GetVersion () {
		return 3;
	}
	
	// Descriptions of in-game currency
	public VirtualCurrency[] GetCurrencies () {
		VirtualCurrency seeds = new VirtualCurrency (
			"Seed", //Name
			"Seed currency", //Description
			"seed" //Currency ID
			);
		return new VirtualCurrency[] {seeds};
	}
	
	// Descriptions of goods
	public VirtualGood[] GetGoods () {
		List<VirtualGood> goods = new List<VirtualGood> ();
		foreach (BerryStoreAssistant.StoreItem item in BerryStoreAssistant.main.items)
			goods.Add(item.Complete());
		foreach (BerryStoreAssistant.StoreItemPack pack in BerryStoreAssistant.main.itemPacks)
			goods.Add(pack.Complete());
		return goods.ToArray ();
	}
	
	// Descriptions of currency packs
	public VirtualCurrencyPack[] GetCurrencyPacks ()
	{
		List<VirtualCurrencyPack> packs = new List<VirtualCurrencyPack> ();
		foreach (BerryStoreAssistant.StoreCurrencyPack pack in BerryStoreAssistant.main.currencyPacks)
			packs.Add(pack.Complete());
		return packs.ToArray ();
	}
	
	// Descriptions of categories
	public VirtualCategory[] GetCategories ()
	{
		return new VirtualCategory[0];
	}
	#endregion


}
