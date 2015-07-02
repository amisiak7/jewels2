using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Soomla.Store;

// Game logic class
public class SessionAssistant : MonoBehaviour {

	public static SessionAssistant main;
	List<Solution> solutions = new List<Solution>();

	[HideInInspector]
	public int animate = 0; // Number of current animation
	[HideInInspector]
	public int matching = 0; // Number of current matching process
	[HideInInspector]
	public int gravity = 0; // Number of current falling chips process
	[HideInInspector]
	public int lastMovementId;
	[HideInInspector]
	public int movesCount; // Number of remaining moves
	[HideInInspector]
	public int swapEvent; // After each successed swap this parameter grows by 1 
	[HideInInspector]
	public int[] countOfEachTargetCount = {0,0,0,0,0,0};// Array of counts of each color matches. Color ID is index.
	[HideInInspector]
	public float timeLeft; // Number of remaining time
	[HideInInspector]
	public int eventCount; // Event counter
	[HideInInspector]
	public int score = 0; // Current score
	[HideInInspector]
	public int[] colorMask = new int[6]; // Mask of random colors: color number - colorID
    [HideInInspector]
    public int targetSugarDropsCount;
    [HideInInspector]
	public bool isPlaying = false;
	[HideInInspector]
	public bool outOfLimit = false;
	[HideInInspector]
	public bool reachedTheTarget = false;
    [HideInInspector]
    public int creatingSugarTask = 0;
    [HideInInspector]
    public bool firstChipGeneration = false;

    bool targetRoutineIsOver = false;
	bool limitationRoutineIsOver = false;
	
	bool wait = false;
	public static int scoreC = 10; // Score multiplier
	
	void  Awake (){
		main = this;
	}

	void Update() {
		DebugPanel.Log ("Animate", "Session", animate);
		DebugPanel.Log ("Matching", "Session", matching);
        DebugPanel.Log("Gravity", "Session", gravity);
        DebugPanel.Log("Sugar Task", "Session", creatingSugarTask);
        DebugPanel.Log("Last Movement ID", "Session", lastMovementId);
        DebugPanel.Log("Event count", "Session", eventCount);
    }

	void Start () {
		AudioAssistant.main.PlayMusic ("Menu");
	}

    // Reset variables
    public static void Reset() {
        main.animate = 0;
        main.gravity = 0;
        main.matching = 0;

        main.eventCount = 0;
        main.lastMovementId = 0;
        main.swapEvent = 0;
        main.score = 0;
        main.firstChipGeneration = true;

        main.isPlaying = true;
        main.movesCount = LevelProfile.main.moveCount;
        main.timeLeft = LevelProfile.main.duraction;
        main.countOfEachTargetCount = new int[] { 0, 0, 0, 0, 0, 0};
        main.creatingSugarTask = 0;


        main.reachedTheTarget = false;
		main.outOfLimit = false;

		main.targetRoutineIsOver = false;
		main.limitationRoutineIsOver = false;
	}

	// Add extra moves (booster effect)
	public void AddExtraMoves () {
		if (!isPlaying) return;
		if (StoreInventory.GetItemBalance("move") == 0) return;
		StoreInventory.TakeItem ("move", 1);
		movesCount += 5;
		UIServer.main.ShowPage ("Field");
		wait = false;
	}

	// Add extra time (booster effect)
	public void AddExtraTime () {
		if (!isPlaying) return;
		if (StoreInventory.GetItemBalance("time") == 0) return;
		StoreInventory.TakeItem ("time", 1);
		timeLeft += 15;
		UIServer.main.ShowPage ("Field");
		wait = false;
	}

	// Resumption of gameplay
	public void Continue () {
		UIServer.main.ShowPage ("Field");
		wait = false;
	}

	// Starting next level
	public void PlayNextLevel() {
		if (CPanel.uiAnimation > 0) return;
		if (!LevelButton.all.ContainsKey(LevelProfile.main.level + 1)) return;
		LevelButton.all[LevelProfile.main.level + 1].LoadLevel ();
	}

	// Restart the current level
	public void RestartLevel() {
		if (CPanel.uiAnimation > 0) return;
		LevelButton.all[LevelProfile.main.level].LoadLevel ();
	}

	// Starting a new game session
	public void StartSession(FieldTarget sessionType, Limitation limitationType) {
		StopAllCoroutines (); // Ending of all current coroutines
		AudioAssistant.main.PlayMusic ("Field"); // Running music

		switch (limitationType) { // Start corresponding coroutine depending on the limiation mode
			case Limitation.Moves: StartCoroutine(MovesLimitation()); break;
			case Limitation.Time: StartCoroutine(TimeLimitation());break;
		}

		switch (sessionType) { // Start corresponding coroutine depending on the target level
			case FieldTarget.Score: StartCoroutine(ScoreSession()); break;
			case FieldTarget.Jelly: StartCoroutine(JellySession()); break;
			case FieldTarget.Block: StartCoroutine(BlockSession()); break;
			case FieldTarget.Color: StartCoroutine(ColorSession()); break;
            case FieldTarget.SugarDrop: StartCoroutine(SugarDropSession()); break;
        }

		StartCoroutine (BaseSession()); // Base routine of game session
		StartCoroutine (ShowingHintRoutine()); // Coroutine display hints
		StartCoroutine (ShuffleRoutine()); // Coroutine of mixing chips at the lack moves
		StartCoroutine (FindingSolutionsRoutine()); // Coroutine of finding a solution and destruction of existing combinations of chips
		StartCoroutine (IllnessRoutine()); // Coroutine of Weeds logic
	}

	IEnumerator BaseSession () {
		while (!limitationRoutineIsOver && !targetRoutineIsOver) {
			DebugPanel.Log("IsPlaying", true);
			yield return 0;
		}
		DebugPanel.Log("IsPlaying", false);

		// Checking the condition of losing
		if (!reachedTheTarget) {
			yield return StartCoroutine(FieldCamera.main.HideFieldRoutine());
			FieldAssistant.main.RemoveField();
			ShowLosePopup();
			yield break;
		}
		
		// Conversion of the remaining moves into bombs and activating them
		yield return StartCoroutine(BurnLastMovesToPowerups());
		
		yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));
		
		// Ending the session, showing win popup
		yield return StartCoroutine(FieldCamera.main.HideFieldRoutine());
		FieldAssistant.main.RemoveField();
		ShowWinPopup ();
	}

	// Game session to set the target score
	IEnumerator ScoreSession() {
		// Waiting until the rules of the game are carried out
		reachedTheTarget = false;
		while (!outOfLimit && score < LevelProfile.main.thirdStarScore) {
			yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));
			if (score >= LevelProfile.main.firstStarScore)
				reachedTheTarget = true;
		}

		targetRoutineIsOver = true;
	}
	
	// Gaming sessions with destroying jelly
	IEnumerator JellySession() {
		// Waiting until the rules of the game are carried out
		reachedTheTarget = false;
		while (!outOfLimit && GameObject.FindObjectsOfType<Jelly>().Length > 0) {
			yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));
		}


		if (GameObject.FindObjectsOfType<Jelly>().Length == 0)
			reachedTheTarget = true;
		
		targetRoutineIsOver = true;
	}

    // Gaming sessions with dropping sugar chips
    IEnumerator SugarDropSession() {
        // Set target
        targetSugarDropsCount = LevelProfile.main.targetSugarDropsCount;
        int sugarTaskCount = targetSugarDropsCount;
        float resource = 0;

        reachedTheTarget = false;
        // Waiting until all sugar chips will be created
        while (sugarTaskCount > 0) {
            switch (LevelProfile.main.limitation) {
                case Limitation.Moves:
                    resource = 1f * movesCount / LevelProfile.main.moveCount;
                    break;
                case Limitation.Time:
                    resource = 1f * timeLeft / LevelProfile.main.duraction;
                    break;
            }

            if (resource <= 0.4f + 0.6f * sugarTaskCount / LevelProfile.main.targetSugarDropsCount) {
                sugarTaskCount--;
                creatingSugarTask++;
            }

            yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));
        }

        // Waiting until the rules of the game are carried out
        while (targetSugarDropsCount > 0) {
            yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));
        }

        reachedTheTarget = true;

        targetRoutineIsOver = true;
    }

    // Gaming sessions with destroying blocks
    IEnumerator BlockSession() {
		// Waiting until the rules of the game are carried out
		reachedTheTarget = false;
		while (!outOfLimit && GameObject.FindObjectsOfType<Block>().Length > 0) {
			yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));
		}
		
		if (GameObject.FindObjectsOfType<Block>().Length == 0)
			reachedTheTarget = true;
		
		targetRoutineIsOver = true;
	}

	// Gaming sessions with destroying special colors chips
	IEnumerator ColorSession() {
		// Set targets
		for (int i = 0; i < LevelProfile.main.countOfEachTargetCount.Length; i++)
			countOfEachTargetCount[colorMask[i]] = LevelProfile.main.countOfEachTargetCount[i];

		// Waiting until the rules of the game are carried out
		reachedTheTarget = false;
		bool noMoreColorTargets = false;
		while (!outOfLimit && !noMoreColorTargets) {
			noMoreColorTargets = true;
			foreach (int t in countOfEachTargetCount)  {
				if (t > 0) {
					noMoreColorTargets = false;
					break;
				}
			}
			if (noMoreColorTargets)
				reachedTheTarget = true;
			yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));
		}
		
		targetRoutineIsOver = true;
	}

	#region Limitation Modes Logic	

	// Game session with limited time
	IEnumerator TimeLimitation() {
		outOfLimit = false;

		// Waiting until the rules of the game are carried out
		while (timeLeft > 0 && !targetRoutineIsOver) {
			timeLeft -= 0.1f;
			yield return new WaitForSeconds(0.1f);
			if (timeLeft <= 0) {
				yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));
				if (!reachedTheTarget) {
					UIServer.main.ShowPage("NoMoreTime");
					wait = true;
					// Pending the decision of the player - lose or purchase additional time
					while (wait) yield return new WaitForSeconds(0.5f);
				}
			}
		}

		yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));

		if (timeLeft <= 0) outOfLimit = true;

		limitationRoutineIsOver = true;
	}

	// Game session with limited count of moves
	IEnumerator MovesLimitation() {
		outOfLimit = false;
		
		// Waiting until the rules of the game are carried out
		while (movesCount > 0) {
			yield return new WaitForSeconds(0.1f);
			if (movesCount <= 0) {
				yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));
				if (!reachedTheTarget) {
					UIServer.main.ShowPage("NoMoreMoves");
					wait = true;
					// Pending the decision of the player - lose or purchase additional time
					while (wait) yield return new WaitForSeconds(0.5f);
				}
			}
		}

		yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));
		
		outOfLimit = true;
		limitationRoutineIsOver = true;
	}

	#endregion


	// Coroutine of searching solutions and the destruction of existing combinations
	IEnumerator FindingSolutionsRoutine () {
		List<Solution> solutions;
		Solution bestSolution;

		while (true) {
			if (isPlaying) {
				solutions = FindSolutions();
				if (solutions.Count > 0) {
					bestSolution = solutions[0];
					foreach(Solution solution in solutions)
						if (solution.potencial > bestSolution.potencial)
							bestSolution = solution;
					MatchSolution(bestSolution);
				} else 
					yield return StartCoroutine(Utils.WaitFor(CanIMatch, 0.1f));
			} else
				yield return 0;
		}
	}

	// Coroutine of conversion of the remaining moves into bombs and activating them
	IEnumerator BurnLastMovesToPowerups ()
	{
		yield return StartCoroutine(CollapseAllPowerups ());

		int newBombs = 0;
		switch (LevelProfile.main.limitation) {
			case Limitation.Moves: newBombs = movesCount; break;
			case Limitation.Time: newBombs = Mathf.CeilToInt(timeLeft); break;
		}

		int count;
		while (newBombs > 0) {
			count = Mathf.Min(newBombs, 6);
			while (count > 0) {
				count --;
				newBombs --;
				movesCount --;
				timeLeft --;
				switch (Random.Range(0, 2)) {
				case 0: FieldAssistant.main.AddPowerup(Powerup.SimpleBomb); break;
				case 1: FieldAssistant.main.AddPowerup(Powerup.CrossBomb); break;
				}
				yield return new WaitForSeconds(0.1f);
			}
			yield return new WaitForSeconds(0.5f);
			yield return StartCoroutine(CollapseAllPowerups ());
		}
	}

	// Weeds logic
	IEnumerator IllnessRoutine () {
		int c = swapEvent;
		Weed.lastSwapWithCrush = swapEvent;

		yield return new WaitForSeconds(1f);

		while (Weed.all.Count > 0) {
			yield return StartCoroutine(Utils.WaitFor(() => {return swapEvent > c && swapEvent > Weed.lastSwapWithCrush;}, 1f));
			yield return StartCoroutine(Utils.WaitFor(CanIWait, 0.3f));
			Weed.Grow();
			c = swapEvent;
		}
	}

	// Ending the session at user
	public void Quit() {
		if (CPanel.uiAnimation > 0) return;
		StopAllCoroutines ();
		StartCoroutine(QuitCoroutine());
		}

	// Coroutine of ending the session at user
	IEnumerator QuitCoroutine() {
		isPlaying = false;
		UIServer.main.HideAll ();
		yield return StartCoroutine(FieldCamera.main.HideFieldRoutine());
		FieldAssistant.main.RemoveField();
		UIServer.main.ShowPage ("LevelList");
		AudioAssistant.main.PlayMusic ("Menu");
	}

	// Coroutine of activation all bombs in playing field
	IEnumerator CollapseAllPowerups () {
		yield return StartCoroutine(Utils.WaitFor(CanIWait, 0.1f));
		List<Chip> powerUp = FindPowerups ();
		while (powerUp.Count > 0) {
			SessionAssistant.main.EventCounter();
			powerUp[Random.Range(0, powerUp.Count - 1)].DestroyChip();
			yield return StartCoroutine(Utils.WaitFor(CanIWait, 0.1f));
			powerUp = FindPowerups ();
		}
		yield return StartCoroutine(Utils.WaitFor(CanIWait, 0.1f));
	}

	// Finding bomb function
	List<Chip> FindPowerups ()
	{
		List<Chip> pu = new List<Chip>();

		foreach (ColorBomb bomb in GameObject.FindObjectsOfType<ColorBomb> ())
			pu.Add(bomb.gameObject.GetComponent<Chip>());
		foreach (CrossBomb bomb in GameObject.FindObjectsOfType<CrossBomb> ())
			pu.Add(bomb.gameObject.GetComponent<Chip>());
		foreach (SimpleBomb bomb in GameObject.FindObjectsOfType<SimpleBomb> ())
			pu.Add(bomb.gameObject.GetComponent<Chip>());

		return pu;
	}

	// Showing lose popup
	void ShowLosePopup ()
	{
		AudioAssistant.Shot ("YouLose");
		AudioAssistant.main.PlayMusic ("Menu");
		isPlaying = false;
		FindObjectOfType<FieldCamera>().HideField();
		UIServer.main.ShowPage ("YouLose");
	}

	// Showing win popup
	void ShowWinPopup ()
	{
		AudioAssistant.Shot ("YouWin");
		AudioAssistant.main.PlayMusic ("Menu");
		isPlaying = false;
		string key = "Best_Score_" + LevelProfile.main.level.ToString ();
		if (PlayerPrefs.GetInt(key) < score)
			PlayerPrefs.SetInt (key, score);
		PlayerPrefs.SetInt ("Complete_" + LevelProfile.main.level.ToString (), 1);
		FindObjectOfType<FieldCamera>().HideField();
        if (LevelButton.all.ContainsKey(LevelProfile.main.level + 1))
		    UIServer.main.ShowPage ("YouWin");
        else
            UIServer.main.ShowPage("LastYouWin");
    }

	// Conditions to start animation
	public bool CanIAnimate (){
		return gravity == 0 && matching == 0;
	}

	// Conditions to start matching
	public bool CanIMatch (){
		return animate == 0 && gravity == 0;
	}

	// Conditions to start falling chips
	public bool CanIGravity (){
		return (animate == 0 && matching == 0) || gravity > 0;
	}

	// Conditions for waiting player's actions
	public bool CanIWait (){
		return animate == 0 && matching == 0 && gravity == 0;
	}

	void  AddSolution ( Solution s  ){
		solutions.Add(s);
	}

	// Event counter
	public void  EventCounter (){
		eventCount ++;
	}

	// Search function possible moves
	List<Move> FindMoves (){
		List<Move> moves = new List<Move>();
		if (!FieldAssistant.main.gameObject.activeSelf) return moves;
		if (LevelProfile.main == null) return moves;

		int x;
		int y;
		int width = LevelProfile.main.width;
		int height = LevelProfile.main.height;
		Move move;
		Solution solution;
		SlotForChip slot;
		string chipTypeA = "";
		string chipTypeB = "";
		
		// horizontal
		for (x = 0; x < width - 1; x++)
		for (y = 0; y < height; y++) {
			if (!FieldAssistant.main.field.slots[x,y]) continue;
			if (!FieldAssistant.main.field.slots[x+1,y]) continue;
			if (FieldAssistant.main.field.blocks[x,y] > 0) continue;
			if (FieldAssistant.main.field.blocks[x+1,y] > 0) continue;
			if (FieldAssistant.main.field.chips[x,y] == FieldAssistant.main.field.chips[x+1,y]) continue;
			if (FieldAssistant.main.field.chips[x,y] == -1 || FieldAssistant.main.field.chips[x+1,y] == -1) continue;
			if (FieldAssistant.main.field.wallsV[x,y]) continue;
			move = new Move();
			move.fromX = x;
			move.fromY = y;
			move.toX = x + 1;
			move.toY = y;
			AnalizSwap(move);
			slot = FieldAssistant.main.GetSlot(move.fromX, move.fromY).GetComponent<SlotForChip>();
			chipTypeA = slot.chip == null ? "SimpleChip" : slot.chip.chipType;
			solution = slot.MatchAnaliz();
			if (solution != null) {
				move.potencial += solution.potencial;
				move.solution = solution;
			}
			slot = FieldAssistant.main.GetSlot(move.toX, move.toY).GetComponent<SlotForChip>();
			solution = slot.MatchAnaliz();
			chipTypeB = slot.chip == null ? "SimpleChip" : slot.chip.chipType;
			if (solution != null && (move.potencial < solution.potencial || move.solution == null)) move.solution = solution;
			if (solution != null)
				move.potencial += solution.potencial;
			AnalizSwap(move);
			if (move.potencial != 0 || (chipTypeA != "SimpleChip" &&  chipTypeB != "SimpleChip")) 
				moves.Add(move);		
		}
		
		// vertical
		for (x = 0; x < width; x++)
		for (y = 0; y < height - 1; y++) {
			if (!FieldAssistant.main.field.slots[x,y]) continue;
			if (!FieldAssistant.main.field.slots[x,y+1]) continue;
			if (FieldAssistant.main.field.blocks[x,y] > 0) continue;
			if (FieldAssistant.main.field.blocks[x,y+1] > 0) continue;
			if (FieldAssistant.main.field.chips[x,y] == FieldAssistant.main.field.chips[x,y+1]) continue;
			if (FieldAssistant.main.field.chips[x,y] == -1 || FieldAssistant.main.field.chips[x,y+1] == -1) continue;
			if (FieldAssistant.main.field.wallsH[x,y]) continue;
			move = new Move();
			move.fromX = x;
			move.fromY = y;
			move.toX = x;
			move.toY = y + 1;

			AnalizSwap(move);
			slot = FieldAssistant.main.GetSlot(move.fromX, move.fromY).GetComponent<SlotForChip>();
			chipTypeA = slot.chip == null ? "SimpleChip" : slot.chip.chipType;
			solution = slot.MatchAnaliz();
			if (solution != null) {
				move.potencial += solution.potencial;
				move.solution = solution;
			}
			slot = FieldAssistant.main.GetSlot(move.toX, move.toY).GetComponent<SlotForChip>();
			solution = slot.MatchAnaliz();
			chipTypeB = slot.chip == null ? "SimpleChip" : slot.chip.chipType;
			if (solution != null && (move.potencial < solution.potencial || move.solution == null)) move.solution = solution;
			if (solution != null)
				move.potencial += solution.potencial;
			AnalizSwap(move);
			if (move.potencial != 0 || (chipTypeA != "SimpleChip" &&  chipTypeB != "SimpleChip")) 
				moves.Add(move);		
		}

		return moves;
	}

	// change places 2 chips in accordance with the move for the analysis of the potential of this move
	void  AnalizSwap (Move move){
		SlotForChip slot;
		Chip fChip = GameObject.Find("Slot_" + move.fromX + "x" + move.fromY).GetComponent<Slot>().GetChip();
		Chip tChip = GameObject.Find("Slot_" + move.toX + "x" + move.toY).GetComponent<Slot>().GetChip();
		if (!fChip || !tChip) return;
		slot = tChip.parentSlot;
		fChip.parentSlot.SetChip(tChip);
		slot.SetChip(fChip);
	}

	// Implementation of solution. Destruction of the existing combination, addition score points and creation bombs.
	void  MatchSolution (Solution solution){

		EventCounter ();
		
		int sx = solution.x;
		int sy = solution.y;
		
		int x;
		int y;
		
		Chip chip = ((GameObject)GameObject.Find("Slot_" + sx + "x" + sy)).GetComponent<SlotForChip>().GetChip();
		
		int width = FieldAssistant.main.field.width;
		int height = FieldAssistant.main.field.height;
		
		Slot s;
		GameObject j;
		GameObject o;
		Chip c;
		
		int puX = -1;
		int puY = -1;
		int puID = -1;
		
		if (!chip.IsMatcheble()) return;

		//   T
		//   T
		// LLXRR
		//   B
		//   B

		// Destruction of right chips (R)
		if (solution.h)
		for (x = sx + 1; x < width; x++) {
			o = GameObject.Find("Slot_" + x + "x" + sy);
			if (!o) break;
			s = o.GetComponent<Slot>();
			if (!s) continue;
			if (!s.GetChip()) break;
			c = s.GetChip();
			if (c.id == chip.id) {
				if (!c.IsMatcheble()) break;
				if (c.movementID > puID) {
					puID = c.movementID;
					puX = x;
					puY = sy;
				}
				s.GetChip().SetScore(Mathf.Pow(2, solution.count-3)/solution.count);
				if (!s.GetBlock()) FieldAssistant.main.BlockCrush(x, sy, true);
				s.GetChip().DestroyChip();
				j = GameObject.Find("Jelly_" + x + "x" + sy);
				if (j) j.SendMessage("JellyCrush", SendMessageOptions.DontRequireReceiver);				
			}
			else break;
		}

		// Destruction of left chips (L)
		if (solution.h)
		for (x = sx - 1; x >= 0; x--) {
			o = GameObject.Find("Slot_" + x + "x" + sy);
			if (!o) break;
			s = o.GetComponent<Slot>();
			if (!s) continue;
			if (!s.GetChip()) break;
			c = s.GetChip();
			if (c.id == chip.id) {
				if (!c.IsMatcheble()) break;
				if (c.movementID > puID) {
					puID = c.movementID;
					puX = x;
					puY = sy;
				}
				s.GetChip().SetScore(Mathf.Pow(2, solution.count-3)/solution.count);
				if (!s.GetBlock()) FieldAssistant.main.BlockCrush(x, sy, true);
				s.GetChip().DestroyChip();
				j = GameObject.Find("Jelly_" + x + "x" + sy);
				if (j) j.SendMessage("JellyCrush", SendMessageOptions.DontRequireReceiver);	
			}
			else break;
		}

		// Destruction of top chips (T)
		if (solution.v)
		for (y = sy + 1; y < height; y++) {
			o = GameObject.Find("Slot_" + sx + "x" + y);
			if (!o) break;
			s = o.GetComponent<Slot>();
			if (!s) continue;
			if (!s.GetChip()) break;
			c = s.GetChip();
			if (c.id == chip.id) {
				if (!c.IsMatcheble()) break;
				if (c.movementID > puID) {
					puID = c.movementID;
					puX = sx;
					puY = y;
				}
				s.GetChip().SetScore(Mathf.Pow(2, solution.count-3)/solution.count);
				if (!s.GetBlock()) FieldAssistant.main.BlockCrush(sx, y, true);
				s.GetChip().DestroyChip();
				j = GameObject.Find("Jelly_" + sx + "x" + y);
				if (j) j.SendMessage("JellyCrush", SendMessageOptions.DontRequireReceiver);	
			}
			else break;
		}

		// Destruction of bottom chips (B)
		if (solution.v)
		for (y = sy - 1; y >= 0; y--) {
			o = GameObject.Find("Slot_" + sx + "x" + y);
			if (!o) break;
			s = o.GetComponent<Slot>();
			if (!s) continue;
			if (!s.GetChip()) break;
			c = s.GetChip();
			if (c.id == chip.id) {
				if (!c.IsMatcheble()) break;
				if (c.movementID > puID) {
					puID = c.movementID;
					puX = sx;
					puY = y;
				}
				s.GetChip().SetScore(Mathf.Pow(2, solution.count-3)/solution.count);
				if (!s.GetBlock()) FieldAssistant.main.BlockCrush(sx, y, true);
				s.GetChip().DestroyChip();
				j = GameObject.Find("Jelly_" + sx + "x" + y);
				if (j) j.SendMessage("JellyCrush", SendMessageOptions.DontRequireReceiver);	
			}
			else break;
		}
		
		if (chip.movementID > puID) {
			puID = chip.movementID;
			puX = sx;
			puY = sy;
		}

		// Destruction of central chip (X)
		chip.SetScore(Mathf.Pow(2, solution.count-3)/solution.count);
		if (!chip.parentSlot.slot.GetBlock()) FieldAssistant.main.BlockCrush(sx, sy, true);
		chip.DestroyChip();
		j = GameObject.Find("Jelly_" + solution.x + "x" + solution.y);
		if (j) j.SendMessage("JellyCrush", SendMessageOptions.DontRequireReceiver);	

		// Check for I5 model to create a color bomb
		//
		// I5 model
		// 
		// X
		// X
		// X   XXXXX
		// X
		// X
		//
		if (solution.count >= 5) {
			if ((solution.v && !solution.h) || (!solution.v && solution.h)) {
				FieldAssistant.main.GetNewColorBomb(puX, puY, FieldAssistant.main.GetSlot(puX, puY).transform.position, solution.id);
				return;
			}
		}
		
		// Check for I4 model to create a simple bomb
		//
		// I4 model
		// 
		// X
		// X
		// X   XXXX
		// X
		//
		if (solution.count >= 4) {
			if ((solution.v && !solution.h) || (!solution.v && solution.h)) {
				FieldAssistant.main.GetNewBomb(puX, puY, FieldAssistant.main.GetSlot(puX, puY).transform.position, solution.id);
				return;
			}
		}
		
		// Check for T4 model to create a cross bomb
		//
		// T4 model
		//
		// XXX	X	 X	  X
		//  X	XXX	 X	XXX
		//  X	X	XXX	  X
		//
		if (solution.count >= 4) {
			if (solution.v && solution.h) {
				FieldAssistant.main.GetNewCrossBomb(puX, puY, FieldAssistant.main.GetSlot(puX, puY).transform.position, solution.id);
				return;
			}
		}

		// I3 model has the lowest priority. It does not create any bombs
		//
		// I3 model
		// 
		// X
		// X   XXX
		// X
		//

	}
	
	public int GetMovementID (){
		lastMovementId ++;
		return lastMovementId;
	}
	
	public int GetMovesCount (){
		return movesCount;
	}

	// Coroutine of call mixing chips in the absence of moves
	IEnumerator ShuffleRoutine () {
		int shuffleOrder = 0;
		float delay = 1;
		while (true) {
			yield return StartCoroutine(Utils.WaitFor(CanIWait, delay));
			if (eventCount > shuffleOrder && !targetRoutineIsOver) {
				shuffleOrder = eventCount;
				yield return StartCoroutine(Shuffle(false));
			}
		}
	}

	// Coroutine of mixing chips
	public IEnumerator Shuffle (bool f) {
		bool force = f;

		List<Move> moves = FindMoves();
		if (moves.Count > 0 && !force)
			yield break;
		if (!isPlaying)
			yield break;

		isPlaying = false;

		Slot[] slots = GameObject.FindObjectsOfType<Slot> ();
		Dictionary<Slot, Vector3> positions = new Dictionary<Slot, Vector3> ();
		foreach (Slot slot in slots)
			positions.Add (slot, slot.transform.position);

        float t = 0;
		while (t < 1) {
			t += Time.unscaledDeltaTime * 3;
			for (int i = 0; i < slots.Length; i++) {
				slots[i].transform.position = Vector3.Lerp(positions[slots[i]], Vector3.zero, t);
			}
			yield return 0;
		}


		List<Solution> solutions = FindSolutions ();

        int itrn = 0;
        int targetID;
		while (f || moves.Count == 0 || solutions.Count > 0) {
            if (itrn > 150) {
                ShowLosePopup();
                yield break;
            }
            
            f = false;
			EventCounter();
			SlotForChip[] sc = GameObject.FindObjectsOfType<SlotForChip> ();
			for (int j = 0; j < sc.Length; j++) {
                targetID = Random.Range(0, j - 1);
                if (!sc[j].chip || !sc[targetID].chip) continue;
                if (sc[j].chip.chipType == "SugarChip" || sc[targetID].chip.chipType == "SugarChip") continue;
                AnimationAssistant.main.SwapTwoItemNow(sc[j].chip, sc[targetID].chip);
            }
			yield return 0;
			moves = FindMoves();
			solutions = FindSolutions ();
            itrn++;
			yield return 0;
		}


		t = 0;
		while (t < 1) {
			t += Time.unscaledDeltaTime * 3;
			for (int i = 0; i < slots.Length; i++) {
				slots[i].transform.position = Vector3.Lerp(Vector3.zero, positions[slots[i]], t);
			}
			yield return 0;
		}
		
		isPlaying = true;

	}

	// Function of searching possible solutions
	List<Solution> FindSolutions() {
		List<Solution> solutions = new List<Solution> ();
		Solution zsolution;
		foreach(SlotForChip slot in GameObject.FindObjectsOfType<SlotForChip>()) {
			zsolution = slot.MatchAnaliz();
			if (zsolution != null) solutions.Add(zsolution);
		}
		return solutions;
	}

	// Coroutine of showing hints
	IEnumerator ShowingHintRoutine () {
		int hintOrder = 0;
		float delay = 5;
		while (true) {
			yield return StartCoroutine(Utils.WaitFor(CanIWait, delay));
			if (eventCount > hintOrder) {
				hintOrder = eventCount;
				ShowHint();
			}
		}
	}

	// Showing random hint
	void  ShowHint (){
		if (!isPlaying) return;
		List<Move> moves = FindMoves();
        DebugPanel.Log("Moves count", "Session", moves.Count);

        foreach (Move move in moves) {
            Debug.DrawLine(FieldAssistant.main.GetSlot(move.fromX, move.fromY).transform.position, FieldAssistant.main.GetSlot(move.toX, move.toY).transform.position, Color.red, 10);
        
        }


		if (moves.Count == 0) return;

		Move bestMove = moves[ Random.Range(0, moves.Count) ];


		int x;
		int y;

		if (bestMove.solution == null) return;

		if (bestMove.solution.h)
			for (x = bestMove.solution.x - bestMove.solution.negH; x <= bestMove.solution.x + bestMove.solution.posH; x++)
				if (x != bestMove.solution.x)
					GameObject.Find("Slot_" + x + "x" + bestMove.solution.y).GetComponent<Slot>().GetChip().Flashing(eventCount);
		
		if (bestMove.solution.v)	
			for (y = bestMove.solution.y - bestMove.solution.negV; y <= bestMove.solution.y + bestMove.solution.posV; y++)
				if (y != bestMove.solution.y)
					GameObject.Find("Slot_" + bestMove.solution.x + "x" + y).GetComponent<Slot>().GetChip().Flashing(eventCount);
		
		if (bestMove.fromX != bestMove.solution.x || bestMove.fromY != bestMove.solution.y)
			GameObject.Find("Slot_" + bestMove.fromX + "x" + bestMove.fromY).GetComponent<Slot>().GetChip().Flashing(eventCount);
		
		if (bestMove.toX != bestMove.solution.x || bestMove.toY != bestMove.solution.y)
			GameObject.Find("Slot_" + bestMove.toX + "x" + bestMove.toY).GetComponent<Slot>().GetChip().Flashing(eventCount);
	}

	// Class with information of solution
	public class Solution {
		//   T
		//   T
		// LLXRR  X - center of solution
		//   B
		//   B

		public int count; // count of chip combination (count = T + L + R + B + X)
		public int potencial; // potential of solution
		public int id; // ID of chip color

		// center of solution
		public int x;
		public int y;

		public bool v; // is this solution is vertical?  (v = L + R + X >= 3)
		public bool h; // is this solution is horizontal? (h = T + B + X >= 3)

		public int posV; // number on right chips (posV = R)
		public int negV; // number on left chips (negV = L)
		public int posH; // number on top chips (posH = T)
		public int negH; // number on bottom chips (negH = B)
	}

	// Class with information of move
	public class Move {
		//
		// A -> B
		//

		// position of start chip (A)
		public int fromX;
		public int fromY;
		// position of target chip (B)
		public int toX;
		public int toY;

		public Solution solution; // solution of this move
		public int potencial; // potential of this move
	}
}