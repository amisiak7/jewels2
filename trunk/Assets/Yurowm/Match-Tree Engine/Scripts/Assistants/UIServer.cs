using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Class of displaying actual UI elements
public class UIServer : MonoBehaviour {

	public static UIServer main;
	
	public string defaultPage; // name of starting page
	public Page[] pages; // Page list

	private Dictionary<string, CPanel> dPanels = new Dictionary<string, CPanel>(); // Dictionary panels. It is formed automatically from the child objects
	private Dictionary<string, Dictionary<string, bool>> dPages = new Dictionary<string, Dictionary<string, bool>>(); // Dictionary pages. It is based on an array of "pages"

	private string currentPage; // Current page name
	private string previousPage; // Previous page name

	void Start () {
		ArraysConvertation(); // filling dictionaries
		ShowPage (defaultPage); // Showing of starting page
	}

	void Awake () {
		main = this;
	}

	// filling dictionaries
	void ArraysConvertation () {
		// filling panels dictionary of the child objects of type "CPanel"
		foreach (CPanel gm in GetComponentsInChildren<CPanel>(true))
			dPanels.Add(gm.name, gm);
		// filling pages dictionary of "pages" arrays element
		foreach (Page pg in pages) {
			Dictionary<string, bool> p = new Dictionary<string, bool>();
			for (int i = 0; i < pg.panels.Length; i++)
				p.Add(pg.panels[i], true);
			dPages.Add(pg.name, p);
		}
	}

	// displaying a page with a given name
	public void ShowPage (string p) {
		if (CPanel.uiAnimation > 0) return;
		if (currentPage == p) return;
		previousPage = currentPage;
		currentPage = p;
		foreach (string key in dPanels.Keys) {
			if (dPages[p].ContainsKey("?" + key)) continue;
			dPanels[key].SetActive(dPages[p].ContainsKey(key));
		}
	}

	// hide all panels
	public void HideAll () {
		foreach (string key in dPanels.Keys) {
			dPanels[key].SetActive(false);
		}
	}

	// show previous page
	public void ShowPreviousPage () {
		ShowPage (previousPage);
	}

	// enable / disable pause
	public void SetPause (bool p) {
		Time.timeScale = p ? 0 : 1;
	}

	// Class information about the page
	[System.Serializable]
	public struct Page {
		public string name; // page name
		public string[] panels; // a list of names of panels in this page
	}
}