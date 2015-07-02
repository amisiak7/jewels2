using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// Start button level. It keeps the level information.
[RequireComponent (typeof (Button))]
public class LevelButton : MonoBehaviour {

	public static Dictionary<int, LevelButton> all = new Dictionary<int, LevelButton> (); // Dictionary of all levels. int - the number of levels, LevelButton - button with a level
    LevelButtonLocker locker;

	private Text label;
	[SerializeField]
	public LevelProfile profile; // Container with level information

	//Play level by level number
	public static void PlayLevel(int i) { 
		if (!all.ContainsKey(i)) return;
		all [i].OnClick ();
	}

	void Awake() {
		Button btn = GetComponent<Button> ();
        locker = GetComponent<LevelButtonLocker>();
		profile.level = GetNumber ();
		all.Add (profile.level, this);
		if (btn != null)
			btn.onClick.AddListener(() => OnClick());
		label = GetComponentInChildren<Text> ();
		gameObject.name = GetNumber ().ToString ();
		label.text = gameObject.name;
	}

	public void OnClick () {
		if (CPanel.uiAnimation > 0) return; 
		LoadLevel ();
	}

    public bool IsLocked() {
        if (locker)
            return locker.IsLocked();
        return false;
    }

    public void ShowItIfItCurrentLevel() {
        if (IsLocked()) return;
        if (!all.ContainsKey(profile.level + 1) || !all[profile.level + 1].IsLocked()) return;
        ScrollRect scrollRect = GetComponentInParent<ScrollRect>();
        RectTransform content = scrollRect.content;
        Vector3 position = content.transform.position;
        position.y += Screen.height / 2 - transform.position.y;
        content.transform.position = position;
        scrollRect.velocity = Vector2.up * 10;
    }

	// Play this level
	public void LoadLevel () {
		LevelProfile.main = profile;
		UIServer.main.ShowPage ("LevelSelectedPopup");
	}

	// determination the level number
	public int GetNumber () {
		return transform.GetSiblingIndex () + 1;
	}
}

// Класс информации об уровне
[System.Serializable]
public class LevelProfile {
	
	public static LevelProfile main; // current level
	const int maxSize = 12; // maximal playing field size

	public int levelID = 0; // Level ID
	public int level = 0; // LEvel number
	// field size
	public int width = 9;
	public int height = 9;
	public int chipCount = 6; // count of chip colors
	public int targetColorCount = 30; // Count of target color in Color mode
    public int targetSugarDropsCount = 0; // Count of sugar chips in SugaDrop mode
    public int firstStarScore = 100; // number of score points needed to get a first stars
	public int secondStarScore = 200; // number of score points needed to get a second stars
	public int thirdStarScore = 300; // number of score points needed to get a third stars
	public float stonePortion = 0f;
	
	public FieldTarget target = FieldTarget.Score; // Playing rules
	// Target score in Score mode = firstStarScore;
	// Count of jellies in Jelly mode colculate automaticly via jellyData array;
	// Count of blocks in Blocks mode colculate automaticly via blockData array;
	// Count of remaining chips in Color mode takes from "countOfEachTargetCount" array, where value is count, index is color ID ;

	public Limitation limitation = Limitation.Moves;
	// Session duration in time limitation mode = duration value (sec);
	// Count of moves in moves limimtation mode = moveCount value (sec);
	public int moveCount = 30; // Count of moves in TargetScore and JellyCrush
	public int duraction = 100;

	public int[] countOfEachTargetCount = new int[6]; // Array of counts of each color matches. Color ID is index.
	public void SetTargetCount (int index, int target) {
		countOfEachTargetCount [index] = target;
	}
	public int GetTargetCount (int index) {
		return countOfEachTargetCount [index];
	}

	public LevelProfile() {
		for (int x = 0; x < maxSize; x++)
			for (int y = 0; y < maxSize; y++)
				SetSlot(x, y, true);
		for (int x = 0; x < maxSize; x++)
			SetGenerator(x, 0, true);
        for (int x = 0; x < maxSize; x++)
            SetSugarDrop(x, maxSize - 1, true);
    }

	// Slot
	public bool[] slot = new bool[maxSize * maxSize];
	public bool GetSlot(int x, int y) {
		return slot [y * maxSize + x];
	}
	public void SetSlot(int x, int y, bool v) {
		slot[y * maxSize + x] = v;
	}
	
	// Generators
	public bool[] generator = new bool[maxSize * maxSize];
	public bool GetGenerator(int x, int y) {
		return generator [y * maxSize + x];
	}
	public void SetGenerator(int x, int y, bool v) {
		generator[y * maxSize + x] = v;
	}

	// Teleports
	public int[] teleport = new int[maxSize * maxSize];
	public int GetTeleport(int x, int y) {
		return teleport [y * maxSize + x];
	}
	public void SetTeleport(int x, int y, int v) {
		teleport[y * maxSize + x] = v;
	}

    // Sugar Drop slots
    public bool[] sugarDrop = new bool[maxSize * maxSize];
    public bool GetSugarDrop(int x, int y) {
        return sugarDrop[y * maxSize + x];
    }
    public void SetSugarDrop(int x, int y, bool v) {
        sugarDrop[y * maxSize + x] = v;
    }

    // Chip
    public int[] chip = new int[maxSize * maxSize];
	public int GetChip(int x, int y) {
		return chip [y * maxSize + x];
	}
	public void SetChip(int x, int y, int v) {
		chip[y * maxSize + x] = v;
	}
	
	// Jelly
	public int[] jelly = new int[maxSize * maxSize];
	public int GetJelly(int x, int y) {
		return jelly [y * maxSize + x];
	}
	public void SetJelly(int x, int y, int v) {
		jelly[y * maxSize + x] = v;
	}
	
	// Block
	public int[] block = new int[maxSize * maxSize];
	public int GetBlock(int x, int y) {
		return block [y * maxSize + x];
	}
	public void SetBlock(int x, int y, int v) {
		block[y * maxSize + x] = v;
	}
	
	// Powerup
	public int[] powerup = new int[maxSize * maxSize];
	public int GetPowerup(int x, int y) {
		return powerup [y * maxSize + x];
	}
	public void SetPowerup(int x, int y, int v) {
		powerup[y * maxSize + x] = v;
	}
	
	// Wall
	public bool[] wallV = new bool[maxSize * maxSize];
	public bool[] wallH = new bool[maxSize * maxSize];
	public bool GetWallV(int x, int y) {
		return wallV [y * maxSize + x];
	}
	public bool GetWallH(int x, int y) {
		return wallH [y * maxSize + x];
	}
	public void SetWallV(int x, int y, bool v) {
		wallV [y * maxSize + x] = v;
	}
	public void SetWallH(int x, int y, bool v) {
		wallH [y * maxSize + x] = v;
	}

	public LevelProfile GetClone() {
		LevelProfile clone = new LevelProfile ();
		clone.level = level;

		clone.width = width;
		clone.height = height;
		clone.chipCount = chipCount;
		clone.countOfEachTargetCount = countOfEachTargetCount;
        clone.targetSugarDropsCount = targetSugarDropsCount;
        clone.targetColorCount = targetColorCount;

		clone.firstStarScore = firstStarScore;
		clone.secondStarScore = secondStarScore;
		clone.thirdStarScore = thirdStarScore;

		clone.target = target;

		clone.duraction = duraction;
		clone.moveCount = moveCount;

		clone.slot = slot;
		clone.generator = generator;
		clone.teleport = teleport;
        clone.sugarDrop = sugarDrop;
		clone.chip = chip;
		clone.jelly = jelly;
		clone.block = block;
		clone.powerup = powerup;
		clone.wallV = wallV;
		clone.wallH = wallH;

		return clone;
	}
}