using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Generator of playing field
public class FieldAssistant : MonoBehaviour {

	public static FieldAssistant main;
	public Dictionary<string, Slot> slots;
	
	float slotoffset = 0.7f;

	[HideInInspector]
	public Field field;
	GameObject slotFolder;

	public static int width {
		get {
			if (main.field != null)
				return main.field.width;
			return 0;
		}
	}

	public static int height {
		get {
			if (main.field != null)
				return main.field.height;
			return 0;
		}
	}
	
	// Names of chip colors
	string[] chipTypes = {"Red", "Green", "Blue", "Yellow", "Purple", "Orange"};
	
	void  Awake (){
		main = this;
	}

	// Field generator
	public void  CreateField (){
		RemoveField (); // Removing old field
		
		field = new Field (LevelProfile.main.width, LevelProfile.main.height);
		field.chipCount = LevelProfile.main.chipCount;

		SessionAssistant.Reset();
		
		//Generating field;
		GenerateSlots();
		GenerateJelly();
		GenerateBlocks();
		GenerateWalls();
		GenerateChips();
		GeneratePowerups();

		SessionAssistant.main.enabled = true;
		SessionAssistant.main.eventCount ++;

	}

	// Removing old field
	public void  RemoveField (){
		if (slotFolder) Destroy(slotFolder);
	}

	// Generation slots for chips
	void  GenerateSlots (){
		slotFolder = new GameObject();
		slotFolder.name = "Slots";
        GameObject o;
        GameObject sd;
        SlotTeleport teleport;

		Slot s;
		Vector3 position;
		
		slots = new Dictionary<string, Slot>();
		
		for (int x = 0; x < field.width; x++)					
		for (int y = 0; y < field.height; y++) {
			field.slots[x,y] = LevelProfile.main.GetSlot(x, height-y-1);
			field.generator[x,y] = LevelProfile.main.GetGenerator(x, height-y-1);
            field.sugarDrop[x, y] = LevelProfile.main.GetSugarDrop(x, height - y - 1);
            field.teleport[x,y] = LevelProfile.main.GetTeleport(x, height-y-1);

			if (field.slots[x,y]) {
				position = new Vector3();
				position.x = -slotoffset * (0.5f * (field.width-1) - x);
				position.y = -slotoffset * (0.5f * (field.height-1) - y);
				o = ContentAssistant.main.GetItem("SlotEmpty", position);
				o.name = "Slot_" + x + "x" + y;
				o.transform.parent = slotFolder.transform;
				s = o.GetComponent<Slot>();
				s.x = x;
				s.y = y;

				if (field.generator[x,y])
					s.gameObject.AddComponent<SlotGenerator>();


				if (field.teleport[x,y] > 0) {
					teleport = s.gameObject.AddComponent<SlotTeleport>();
					teleport.targetID = field.teleport[x,y];
				}

                if (LevelProfile.main.target == FieldTarget.SugarDrop && field.sugarDrop[x, y]) {
                    s.sugarDropSlot = true;
                    sd = ContentAssistant.main.GetItem("SugarDrop", position);
                    sd.name = "SugarDrop";
                    sd.transform.parent = o.transform;
                    sd.transform.localPosition = Vector3.zero;
                }


                    slots.Add(x + "x" + y, s);
			}
		}
		
		foreach (Slot slot in GameObject.FindObjectsOfType<Slot>())
			slot.Initialize();
	}

	// Generation jelly in slots
	void  GenerateJelly (){
		GameObject o;
		Slot s;
		Jelly j;
		
		for (int x = 0; x < field.width; x++)				
		for (int y = 0; y < field.height; y++) {
			field.jellies[x,y] = LevelProfile.main.GetJelly(x,height-y-1);
			if (field.slots[x,y] && field.jellies[x,y] > 0) {
				s = GetSlot(x, y);
				o = ContentAssistant.main.GetItem("Jelly");
				o.transform.position = s.transform.position;
				o.name = "Jelly_" + x + "x" + y;
				o.transform.parent = s.transform;
				j = o.GetComponent<Jelly>();
				j.level = field.jellies[x,y];
				s.SetJelly(j);
			}
		}
	}

	// Generation of destructible blocks
	void GenerateBlocks (){
		GameObject o;
		Slot s;
		Block b;
		Weed w;
		Branch brch;
		
		for (int x = 0; x < field.width; x++)					
		for (int y = 0; y < field.height; y++) {
			field.blocks[x,y] = LevelProfile.main.GetBlock(x,height-y-1);
			if (field.slots[x,y]) {
				if (field.blocks[x,y] > 0) {
					if (field.blocks[x,y] <= 3) {
						s = GetSlot(x, y);
						o = ContentAssistant.main.GetItem("Block");
						o.transform.position = s.transform.position;
						o.name = "Block_" + x + "x" + y;
						o.transform.parent = s.transform;
						b = o.GetComponent<Block>();
						s.SetBlock(b);
						b.slot = s;
						b.level = field.blocks[x,y];
						b.Initialize();
					}
					if (field.blocks[x,y] == 4) {
						s = GetSlot(x, y);
						o = ContentAssistant.main.GetItem("Weed");
						o.transform.position = s.transform.position;
						o.name = "Weed_" + x + "x" + y;
						o.transform.parent = s.transform;
						w = o.GetComponent<Weed>();
						s.SetBlock(w);
						w.slot = s;
						w.Initialize();
					}
					if (field.blocks[x,y] == 5) {
						s = GetSlot(x, y);
						o = ContentAssistant.main.GetItem("Branch");
						o.transform.position = s.transform.position;
						o.name = "Branch_" + x + "x" + y;
						o.transform.parent = s.transform;
						brch = o.GetComponent<Branch>();
						s.SetBlock(brch);
						brch.slot = s;
						brch.Initialize();
					}
				}
			}
		}
		
		SlotGravity.Reshading();
	}

	// Generation impassable walls
	void  GenerateWalls (){
		int x;
		int y;
		GameObject o;
		Wall w;
		Vector3 position;
			

		for (x = 0; x < field.width-1; x++)		
			for (y = 0; y < field.height; y++) {
			field.wallsV[x,y] = LevelProfile.main.GetWallV(x,height-y-1);
				if (field.wallsV[x,y]) {
					position = new Vector3();
					position.x = -slotoffset * (0.5f * (field.width-2) - x);
					position.y = -slotoffset * (0.5f * (field.height-1) - y);
					o = ContentAssistant.main.GetItem("Wall", position);
					o.transform.position = position;
					o.name = "WallV_" + x + "x" + y;
					o.transform.parent = GetSlot(x, y).transform;
					w = o.GetComponent<Wall>();
					w.x = x;
					w.y = y;
					w.h = false;
				}
			}

		for (x = 0; x < field.width; x++)	
			for (y = 0; y < field.height-1; y++) {
			field.wallsH[x,y] = LevelProfile.main.GetWallH(x, height-y-2);
				if (field.wallsH[x,y]) {
					position = new Vector3();
					position.x = -slotoffset * (0.5f * (field.width-1) - x);
					position.y = -slotoffset * (0.5f * (field.height-2) - y);
					o = ContentAssistant.main.GetItem("Wall", position, Quaternion.Euler(0, 0, -90));
					o.name = "WallH_" + x + "x" + y;
					o.transform.parent = GetSlot(x, y).transform;
					w = o.GetComponent<Wall>();
					w.x = x;
					w.y = y;
					w.h = true;
				}
			}
		
		foreach (Wall z in GameObject.FindObjectsOfType<Wall>())
			z.Initialize();
	}

	// Generation bombs
	void GeneratePowerups ()
	{
		int x;
		int y;
		
		for (x = 0; x < field.width; x++)				
			for (y = 0; y < field.height; y++)
				field.powerUps[x,y] = LevelProfile.main.GetPowerup(x,height-y-1);

		for (x = 0; x < field.width; x++)				
		for (y = 0; y < field.height; y++) {
			if (field.powerUps[x,y] > 0 && field.slots[x,y]) {
				switch (field.powerUps[x,y]) {
					case 1: AddPowerup(x, y, Powerup.CrossBomb); break;
					case 2: AddPowerup(x, y, Powerup.SimpleBomb); break;
					case 3: AddPowerup(x, y, Powerup.ColorBomb); break;
				}
			}
		}
	}

	// Generation chips
	void  GenerateChips (){
		int x;
		int y;

		for (x = 0; x < field.width; x++)				
			for (y = 0; y < field.height; y++)
				field.chips[x,y] = LevelProfile.main.GetChip(x,height-y-1);

		field.FirstChipGeneration();
		
		int id;
		
		for (x = 0; x < field.width; x++)				
			for (y = 0; y < field.height; y++) {
				id = field.GetChip(x, y);
				if (id >= 0 && id != 9 && (field.blocks[x,y] == 0 || field.blocks[x,y] == 5)) 
					GetNewSimpleChip(x, y, new Vector3(0, 2, 0), id);
				if (id == 9 && (field.blocks[x,y] == 0)) 
					GetNewStone(x, y, new Vector3(0, 2, 0));
			}
	}

	// Creating a simple random color chips
	public Chip GetNewSimpleChip (int x, int y, Vector3 position){
		return GetNewSimpleChip(x, y, position, SessionAssistant.main.colorMask[Random.Range(0, field.chipCount)]);
	}

    // Creating a sugar chips
    public Chip GetSugarChip(int x, int y, Vector3 position) {
        GameObject o = ContentAssistant.main.GetItem("Sugar");
        o.transform.position = position;
        o.name = "Sugar";
        Chip chip = o.GetComponent<Chip>();
        GetSlot(x, y).SetChip(chip);
        return chip;
    }

    public Chip GetNewStone (int x, int y, Vector3 position) {
		GameObject o = ContentAssistant.main.GetItem ("Stone");
		o.transform.position = position;
		o.name = "Stone";
		Chip chip = o.GetComponent<Chip> ();
		GetSlot(x, y).SetChip(chip);
		return chip;
	}

	// Creating a simple chip specified color
	public Chip GetNewSimpleChip (int x, int y, Vector3 position, int id) {
		GameObject o = ContentAssistant.main.GetItem ("SimpleChip" + chipTypes [id]);
		o.transform.position = position;
		o.name = "Chip_" + chipTypes[id];
		Chip chip = o.GetComponent<Chip> ();
		GetSlot(x, y).SetChip(chip);
		return chip;
	}

	// Creating a cross-bombs specified color
	public Chip GetNewCrossBomb (int x, int y,Vector3 position, int id){
		GameObject o = ContentAssistant.main.GetItem ("CrossBomb" + chipTypes [id]);
		o.transform.position = position;
		o.name = "CrossBomb_" + chipTypes[id];
		Chip chip = o.GetComponent<Chip> ();
		GetSlot(x, y).SetChip(chip);
		return chip;
	}

	// Creating a simple bomb specified color
	public Chip GetNewBomb (int x, int y, Vector3 position, int id){
		GameObject o = ContentAssistant.main.GetItem ("SimpleBomb" + chipTypes [id]);
		o.transform.position = position;
		o.name = "Bomb_" + chipTypes[id];
		Chip chip = o.GetComponent<Chip> ();
		GetSlot(x, y).SetChip(chip);
		return chip;
	}

	// Make a color bomb
	public Chip GetNewColorBomb (int x, int y, Vector3 position, int id){
		GameObject o = ContentAssistant.main.GetItem ("ColorBomb" + chipTypes [id]);
		o.transform.position = position;
		o.name = "ColorBomb" + chipTypes[id];
		Chip chip = o.GetComponent<Chip> ();
		GetSlot(x, y).SetChip(chip);
		return chip;
	}

	// Make a bomb in the specified location with the ability to transform simple chips in a bomb
	public Chip AddPowerup(int x, int y, Powerup p) {
		SlotForChip slot = GetSlot (x, y).GetComponent<SlotForChip> ();
		Chip chip = slot.chip;
		int id;
		if (chip)
			id = chip.id;
		else 
			id = Random.Range(0, LevelProfile.main.chipCount);
		if (chip)
			Destroy (chip.gameObject);
		switch (p) {
			case Powerup.SimpleBomb: chip = FieldAssistant.main.GetNewBomb(slot.slot.x, slot.slot.y, slot.transform.position, id); break;
			case Powerup.CrossBomb: chip = FieldAssistant.main.GetNewCrossBomb(slot.slot.x, slot.slot.y, slot.transform.position, id); break;
			case Powerup.ColorBomb: chip = FieldAssistant.main.GetNewColorBomb(slot.slot.x, slot.slot.y, slot.transform.position, id); break;
		}
		return chip;
	}

	// Create a bomb with the possibility of transformation of simple chips in bomb
	public void AddPowerup(Powerup p) {
		SimpleChip[] chips = GameObject.FindObjectsOfType<SimpleChip>();
		if (chips.Length == 0) return;
		SimpleChip chip = null;
		while (chip == null || chip.matching)
			chip = chips[Random.Range(0, chips.Length - 1)];
		SlotForChip slot = chip.chip.parentSlot;
		if (slot)
			AddPowerup (slot.slot.x, slot.slot.y, p);
	}

	// Request of slot object
	public Slot GetSlot (int x, int y){
		if (field.GetSlot(x, y)) 
			return slots[x + "x" + y];
		return null;
	}

	// Request of neighboring slot object
	public Slot GetNearSlot (int x, int y, Side side){
		int offsetX = Utils.SideOffsetX (side);
		int offsetY = Utils.SideOffsetY (side);
		if (field.GetSlot(x + offsetX, y + offsetY)) 
			return slots[(x + offsetX).ToString() + "x" + (y + offsetY).ToString()];
		return null;
	}
	
	// Request of jelly object
	public Jelly GetJelly ( int x ,   int y  ){
		if (field.GetSlot(x, y))
			return slots[x + "x" + y].GetJelly();		
		return null;
	}

	// Request of block object
	public BlockInterface GetBlock ( int x ,   int y  ){
		if (field.GetSlot(x, y))
			return slots[x + "x" + y].GetBlock();		
		return null;
	}

	// Crush jelly function
	public void JellyCrush (int x, int y) {
		GameObject j = GameObject.Find("Jelly_" + x + "x" + y);
		if (j) j.SendMessage("JellyCrush", SendMessageOptions.DontRequireReceiver);	
	}

	// Crush block function
	public void  BlockCrush (int x, int y, bool radius) {
		BlockInterface b;
		Slot s;
		Chip c;
		StoneChip sc;


		if (radius) {
			foreach (Side side in Utils.straightSides) {
				b = null;
				s = null;
				c = null;
				sc = null;

				b = GetBlock(x + Utils.SideOffsetX(side), y + Utils.SideOffsetY(side));
				if (b && b.CanBeCrushedByNearSlot()) b.BlockCrush();
				
				s = GetSlot(x + Utils.SideOffsetX(side), y + Utils.SideOffsetY(side));
				if (s) c = s.GetChip();
				if (c) sc = c.GetComponent<StoneChip>();
				if (sc) c.DestroyChip();
			}
		} 

		b = GetBlock(x, y);
		if (b) b.BlockCrush();
	}
}

// The class information about the playing field and the target level
public class Field {
	public int width;
	public int height;
	public int chipCount;
	public bool[,] slots;
	public int[,] teleport;
    public bool[,] generator;
    public bool[,] sugarDrop;
    public int[,] chips;
	public int[,] powerUps;
	public int[,] blocks;
	public int[,] jellies;
	public bool[,] wallsH;
	public bool[,] wallsV;

	public FieldTarget target = FieldTarget.Score;
	public int targetValue = 0;
	
	
	public Field (int w, int h){
		width = w;
		height = h;
		slots = new bool [w,h];
        generator = new bool[w, h];
        sugarDrop = new bool[w, h];
        teleport = new int[w,h];
		chips = new int [w,h];
		powerUps = new int [w,h];
		blocks = new int [w,h];
		jellies = new int [w,h];
		wallsV = new bool [w,h];
		wallsH = new bool [w,h];
	}
	
	public bool GetSlot (int x, int y){
		if (x >= 0 && x < width && y >= 0 && y < height) return slots[x,y];
		return false;
	}
	
	public int GetChip (int x, int y){
		if (x >= 0 && x < width && y >= 0 && y < height) return chips[x,y];
		return 0;
	}
	
	public void  NewRandomChip (int x, int y, bool unmatching){
		if (chips[x,y] == -1) return;
		
		chips[x,y] = Random.Range(0, chipCount + 1);
		
		if (unmatching) {
			while (GetChip(x, y) == GetChip(x, y+1)
			       || GetChip(x, y) == GetChip(x+1, y)
			       || GetChip(x, y) == GetChip(x, y-1)
			       || GetChip(x, y) == GetChip(x-1, y))
				chips[x,y] = Random.Range(0, chipCount + 1);
		}
	}
	
	public void  FirstChipGeneration (){
		int x;
		int y;
		
		// impose a mask slots
		for (x = 0; x < width; x++) {
			for (y = 0; y < height; y++) {
				if (!slots[x,y] || (blocks[x,y] > 0 && blocks[x,y] != 5))
					chips[x,y] = -1;
				if (blocks[x,y] == 5 && chips[x,y] == -1)
					chips[x,y] = 0;
			}
		}
		
		// replace random chips on nonrandom
		for (x = 0; x < width; x++)					
			for (y = 0; y < height; y++) 
				if (chips[x,y] == 0 && chips[x,y] != 9)
					NewRandomChip(x, y, true);	
		
		// nonrandom give chips to the normal (0 to 5)
		for (x = 0; x < width; x++)					
			for (y = 0; y < height; y++) 
				if (chips[x,y] > 0 && chips[x,y] != 9)
					chips[x,y] --;

		
		// shuffling color
		// create a deck of colors and shuffling its
		int[] a = {0, 1, 2, 3, 4, 5};
		int j;
		for (int i = 5; i > 0; i--) {
			j = Random.Range(0, i);
			a[j] = a[j] + a[i];
			a[i] = a[j] - a[i];
			a[j] = a[j] - a[i];
		}

		SessionAssistant.main.colorMask = a;
		
		// apply the results to the matrix shuffling chips	
		for (x = 0; x < width; x++)					
			for (y = 0; y < height; y++) 
				if (chips[x,y] >= 0 && chips[x,y] != 9)
					chips[x,y] = a[chips[x,y]];
	}
	
}

public enum FieldTarget {
	Score = 0,
	Jelly = 1,
	Block = 2,
    Color = 3,
    SugarDrop = 4
}

public enum Limitation {
	Moves,
	Time
}

public enum Powerup {
	SimpleBomb,
	CrossBomb,
	ColorBomb
}