using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
	public enum Swipe
	{
		Up,
		Down,
		Left,
		Right,
		None,
		UpLeft,
		UpRight,
		DownLeft,
		DownRight}

	;

	public int Rows = 5;
	public int Columns = 6;
	public GameObject SlotPrefab;
	public GameObject[] HeroesTypes;
	public GameObject HighLighterprefab;
	public int PlayerControlID;

	float tweakFactor = 0.5f;
	Swipe swipeDirection = Swipe.None;


	GameObject[] HighLighter;
	List<GameObject> heroPool = new List<GameObject> ();
	public Slot[,] Player1slots;
	public Slot[,] Player2slots;
	[HideInInspector]
	public bool start = false;
	public float cascadecheckSpeed = 0.5f;
	public float cascadeobjectspeed = 0.6f;


	bool anyMoved = false;
	bool renewBoard = false;
	//Input System requirements
	[HideInInspector]
	public bool canInput = true;
	[HideInInspector]
	public Slot slot1;
	[HideInInspector]
	public Slot slot2;
	Slot[,] tiles;
	public float DistanceBetweentwoBoards;
	//GameObject h1;
	//GameObject h2;
	Slot ts1;
	Slot ts2;
	public float driftseconds = 0.2f;

	//[HideInInspector]
	public bool isDrifting;

	#region Initialization stuffs

	void PoolShuffle ()
	{
		System.Random rand = new System.Random ();
		int r = heroPool.Count;
		while (r > 1) {
			r--;

			int n = rand.Next (r + 1);
			GameObject val = heroPool [n];
			heroPool [n] = heroPool [r];
			heroPool [r] = val;
		}
	}

	void IntializingHeroPool (int rows, int cols, List<GameObject> HeroList, bool Shuffle)
	{
		int numCopies = (rows * cols);
		GameObject HeroPoolParent = new GameObject ("HeroPoolParent");

		for (int i = 0; i < numCopies; i++) {
			for (int j = 0; j < HeroesTypes.Length; j++) {
				GameObject o = (GameObject)Instantiate (HeroesTypes [j], new Vector3 (-10, -10, 0), HeroesTypes [j].transform.rotation);

				o.SetActive (false);
				o.transform.parent = HeroPoolParent.transform;
				HeroList.Add (o);
			}
		}
		if (Shuffle)
			PoolShuffle ();
	}

	void InitializingPointers ()
	{
		HighLighter = new GameObject[3];
		HighLighter [1] = (GameObject)Instantiate (HighLighterprefab, new Vector3 (-6, -6, 0), transform.rotation);
		HighLighter [1].name = "Pointer_Player1";
		HighLighter [1].SetActive (false);
		HighLighter [2] = (GameObject)Instantiate (HighLighterprefab, new Vector3 (-6, -6, 0), transform.rotation);
		HighLighter [2].name = "Pointer_Player2";
		HighLighter [2].SetActive (false);
	}


	void InitiaizingSlots ()
	{
		//Initializing slots array limit
		Player1slots = new Slot[Columns, Rows];
		Player2slots = new Slot[Columns, Rows];
		GameObject p1slotsParent = new GameObject ("Player1SlotsParent");
		GameObject p2slotsParent = new GameObject ("Player2SlotsParent");

		//List<GameObject> tempheroes = new List<GameObject>();
		//Player 1 Spawning
		for (int r = 0; r < Rows; r++)
			for (int c = 0; c < Columns; c++) {
				Vector3 slotPos = new Vector3 (c, r, 0);

				//Initializing Slots and filing slots 2D array
				GameObject sl = (GameObject)Instantiate (SlotPrefab, slotPos, SlotPrefab.transform.rotation);
				sl.transform.parent = p1slotsParent.transform;
				sl.name = "p: " + 1 + " + slot (" + c + " , " + r + ")";
				Player1slots [c, r] = sl.GetComponent<Slot> ();

				//Initializing Heroes, setting other parameters for each slots (like player ID, Hero name, is filled)
				for (int n = 0; n < heroPool.Count; n++) {
					
					GameObject o = heroPool [n];
					if (!o.activeSelf) {
						o.transform.position = new Vector3 (slotPos.x, slotPos.y, slotPos.z);
						o.SetActive (true);

						Player1slots [c, r].Fill (o, slotPos, 1);
						Player1slots [c, r].c = c;
						Player1slots [c, r].r = r;
						o.name = o.name + "(" + c + ", " + r + ")";
						//tempheroes.Add(o);
						n = heroPool.Count + 1;
					}
				}

			}

		//Player 2 Spawning
		for (int r = 0; r < Rows; r++)
			for (int c = 0; c < Columns; c++) {
				Vector3 slotPos = new Vector3 (c, 5 + DistanceBetweentwoBoards + r, 0);

				//Initializing Slots and filing slots 2D array
				GameObject sl = (GameObject)Instantiate (SlotPrefab, slotPos, SlotPrefab.transform.rotation);
				sl.transform.parent = p2slotsParent.transform;
				sl.name = "p: " + 2 + " + slot (" + c + " , " + r + ")";
				Player2slots [c, r] = sl.GetComponent<Slot> ();

				//Initializing Heroes, setting other parameters for each slots (like player ID, Hero name, is filled)
				for (int n = 0; n < heroPool.Count; n++) {
					GameObject o = heroPool [n];
					if (!o.activeSelf) {
						o.transform.position = new Vector3 (slotPos.x, slotPos.y, slotPos.z);
						o.SetActive (true);

						Player2slots [c, r].Fill (o, slotPos, 2);
						Player2slots [c, r].c = c;
						Player2slots [c, r].r = r;

						o.name = o.name + "(" + c + ", " + r + ")";
						//tempheroes.Add(o);
						n = heroPool.Count + 1;
					}
				}

			}


	}

	#endregion


	void Start ()
	{
		//Pointers creation
		InitializingPointers ();

		// object Pooling
		//Hero pooling and saving them into a list "heroPool"
		IntializingHeroPool (Rows, Columns, heroPool, true);


		//Filling heroes on the board and saving them into "slots" for both Player 1 and player 2
		InitiaizingSlots ();
        
	}

	void Update ()
	{
		if (canInput) {
			//FixingInputManager (PlayerControlID);
			//SwipeInputManager (PlayerControlID, true);
			EfficientSwipeManager (PlayerControlID);
			//IntelligentInputManager (PlayerControlID);


		}

		if (PlayerControlID == 1)
			tiles = Player1slots;
		else
			tiles = Player2slots;
		
		PCDetectSwipe ();

		//GridCheck (PlayerControlID);

		if (Input.GetKeyDown (KeyCode.R)) {
			Application.LoadLevel (Application.loadedLevelName);
		}

		if (Input.GetKeyDown (KeyCode.A)) {////////BUG CRASHED

			NewGridCheck (PlayerControlID);
		}
			
	}


	#region Puzzles and dragon Input Sysytem (NOT WORKING PROPERLY)

	bool temp = true;
	[HideInInspector]
	public Slot prev;
	[HideInInspector]
	public Slot SelectedSlot;
	[HideInInspector]
	public GameObject SelectedHero;
	bool downfirst;
	bool release;

	void FixingInputManager (int PID)
	{

		if ((Input.GetMouseButtonDown (0)) && (!slot1)) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit2D hit = Physics2D.GetRayIntersection (ray, 1000, 1 << 8); // '8' is the "Slotslayer"
			release = false;
			if (hit) {
				if (hit.collider.gameObject.GetComponent<Slot> ().PlayerID == PID) {
					slot1 = hit.collider.gameObject.GetComponent<Slot> ();
					SelectedSlot = slot1;
					if (!downfirst) {
						//SelectedSlot = hit.collider.gameObject.GetComponent<Slot>();
						SelectedHero = SelectedSlot.hero;
						downfirst = true;
					}
	                  


				}
			}
		} else if ((Input.GetMouseButton (0)) && SelectedSlot) {

			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit2D hit = Physics2D.GetRayIntersection (ray, 1000, 1 << 8);
			if (SelectedHero) {
				if (!release) {
					Vector3 vec;
					vec = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0));

					SelectedHero.transform.position = new Vector3 (vec.x, vec.y, -1);

				}
	               
			}
	           

			if (hit) {
				if (hit.collider.gameObject.GetComponent<Slot> ().PlayerID == PID) {
					slot2 = hit.collider.gameObject.GetComponent<Slot> ();

					HighLighter [PID].SetActive (true);
					HighLighter [PID].transform.position = slot2.position;

					if (temp) {
						slot1 = slot2;
						temp = false;
					} 

					if ((slot1 != slot2) && (temp == false)) {

						//c++;
						//Debug.Log(c);
						prev = slot1;
						FixHeroTransformSwap (prev, slot2);
						//prev.Col (Color.red);
						temp = true;
					}
				}

			}
		}
		if ((Input.GetMouseButtonUp (0)) && slot1) {
			release = true;
			slot1.heroMoveTo (SelectedHero, driftseconds, false, false);
			slot1.Fill (SelectedHero, slot1.position, slot1.PlayerID);
			if (HighLighter [PID].activeSelf) {
				HighLighter [PID].SetActive (false);
			}
			ResetSlotValues ();
		}

	}

	void ResetSlotValues ()
	{

		slot1 = null;
		slot2 = null;
		prev = null;
		SelectedSlot = null;
		SelectedHero = null;
		downfirst = false;
	}

	void FixHeroTransformSwap (Slot prev, Slot s2)
	{
		s2.MoveTo (prev, driftseconds, false, false);
		FixSlotDataSwap (prev, s2);
	}

	public void FixSlotDataSwap (Slot prev, Slot s2)
	{
		//GameObject temphero = s2.hero;
		//int tempplayerID = s2.PlayerID;
		prev.Fill (s2.hero, prev.position, s2.PlayerID);
		//Debug.Log("gg");
		//s1.Fill(s2.hero, s1.position, s1.PlayerID);
		//s1.Fill(temphero, s1.position, tempplayerID);

	}


	#endregion

	#region Swipe Input System

	Slot[,] Slotlib;

	void EfficientSwipeManager (int PID)
	{
		
		if (PID == 1)
			Slotlib = Player1slots;
		else if (PID == 2)
			Slotlib = Player2slots;
			

		if ((Input.GetMouseButtonDown (0)) && (!slot1)) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit2D hit = Physics2D.GetRayIntersection (ray, 1000, 1 << 8); // '8' is the "Slotslayer"
			//swipeDirection = Swipe.None;
			if (hit) {
				if (hit.collider.gameObject.GetComponent<Slot> ()) {
					if (hit.collider.gameObject.GetComponent<Slot> ().CanInteractible == true)
					if (hit.collider.gameObject.GetComponent<Slot> ().PlayerID == PID) {
						slot1 = hit.collider.gameObject.GetComponent<Slot> ();
						if (!HighLighter [PID].activeSelf) {
							HighLighter [PID].transform.position = slot1.position;
							HighLighter [PID].SetActive (true);
						}
					} else {
						Debug.Log ("Other Player slot :" + hit.collider.gameObject);
					}

				} else
					Debug.Log ("Slot script missing on : " + hit.collider.gameObject);
			}
		} else if ((Input.GetMouseButtonUp (0)) && (slot1)) {
			slot1 = null;
			slot2 = null;
			HighLighter [PID].SetActive (false);
			swipeDirection = Swipe.None;

		} else if ((Input.GetMouseButton (0)) && slot1) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit2D hit = Physics2D.GetRayIntersection (ray, 1000, 1 << 8);

			if (hit) {
				if ((hit.collider.gameObject.GetComponent<Slot> ().PlayerID == PID) && (hit.collider.gameObject.GetComponent<Slot> ().CanInteractible == true))
					slot2 = hit.collider.gameObject.GetComponent<Slot> ();

				if ((slot1 && slot2) && (slot1.PlayerID == slot2.PlayerID)) {

					int x, y;
					x = (int)slot1.c;
					y = (int)slot1.r;

					if (x + 1 < Columns)
					if (swipeDirection == Swipe.Right) {
						
						slot2 = Slotlib [x + 1, y];
						if (slot1) {
							decideAction (slot1, slot2);
							slot1 = null;
						}

						//HeroTransformSwap (slot1, slot2);

						//Reset (PID);
					}

					if (x - 1 >= 0)
					if (swipeDirection == Swipe.Left) {
						slot2 = Slotlib [x - 1, y];


						if (slot1) {
							decideAction (slot1, slot2);

							slot1 = null;
						}
						//HeroTransformSwap (slot1, slot2);
						//Reset (PID);
					}

					if (y + 1 < Rows)
					if (swipeDirection == Swipe.Up) {
						slot2 = Slotlib [x, y + 1];

						if (slot1) {
							decideAction (slot1, slot2);

							slot1 = null;
						}

						//HeroTransformSwap (slot1, slot2);
						//Reset (PID);
					}

					if (y - 1 >= 0)
					if (swipeDirection == Swipe.Down) {
						slot2 = Slotlib [x, y - 1];


						if (slot1) {
							decideAction (slot1, slot2);

							slot1 = null;
						}
						//HeroTransformSwap (slot1, slot2);
						//Reset (PID);
					}

					if ((x + 1 < Columns) && (y + 1 < Rows))
					if (swipeDirection == Swipe.UpRight) {
						slot2 = Slotlib [x + 1, y + 1];
						if (slot1) {
							decideAction (slot1, slot2);
							slot1 = null;
						}
					}

					if ((x - 1 >= 0) && (y + 1 < Rows))
					if (swipeDirection == Swipe.UpLeft) {
						slot2 = Slotlib [x - 1, y + 1];
						if (slot1) {
							decideAction (slot1, slot2);

							slot1 = null;
						}
					}

					if ((x - 1 >= 0) && (y - 1 >= 0))
					if (swipeDirection == Swipe.DownLeft) {
						slot2 = Slotlib [x - 1, y - 1];
						if (slot1) {
							decideAction (slot1, slot2);

							slot1 = null;
						}
					}

					if ((x + 1 < Columns) && (y - 1 >= 0))
					if (swipeDirection == Swipe.DownRight) {
						slot2 = Slotlib [x + 1, y - 1];
						if (slot1) {
							decideAction (slot1, slot2);

							slot1 = null;
						}

					}

				}

				if ((hit.collider.gameObject.GetComponent<Slot> ().PlayerID != PID) || (hit.collider.gameObject.GetComponent<Slot> ().CanInteractible == false)) {
					Reset (PID);
				}


			} else {
				Reset (PID);
			}

		}  

		if (Input.GetMouseButtonUp (0)) {
			Reset (PID);	
		}

	}



	public bool CheckBeforeSwipe (Slot s1, Slot s2)
	{
		bool s1bool = false;
		bool s2bool = false;

		s1bool = eachslotcheck (s1);
		s2bool = eachslotcheck (s2);

		if (s1bool || s2bool)
			return true;
		else
			return false;

	}

	bool eachslotcheck (Slot s)
	{
		int c;
		int r;
		c = s.c;
		r = s.r;
		bool goahead = false;
		bool r1 = false;
		bool r2 = false;
		bool r3 = false;
		bool r4 = false;
		bool finalr = false;
		bool c1 = false;
		bool c2 = false;
		bool c3 = false;
		bool c4 = false;
		bool finalc = false;

		//Rows
		if (r + 1 < Rows)
		if (tiles [c, r + 1].isFilled)
		if (s.herotype == tiles [c, r + 1].herotype) {
			r1 = true;
		}	

		if (r + 2 < Rows)
		if (tiles [c, r + 2].isFilled)
		if (s.herotype == tiles [c, r + 2].herotype) {
			r2 = true;
		}

		if (r - 1 >= 0)
		if (tiles [c, r - 1].isFilled)
		if (s.herotype == tiles [c, r - 1].herotype) {
			r3 = true;
		}
		if (r - 2 >= 0)
		if (tiles [c, r - 2].isFilled)
		if (s.herotype == tiles [c, r - 2].herotype) {
			r4 = true;
		}

		if ((r1 && r2) || (r1 && r3) || (r3 && r4)) {
			finalr = true;
		}

		//Columns
		if (c + 1 < Columns)
		if (tiles [c + 1, r].isFilled)
		if (s.herotype == tiles [c + 1, r].herotype) {
			c1 = true;
		}	

		if (c + 2 < Columns)
		if (tiles [c + 2, r].isFilled)
		if (s.herotype == tiles [c + 2, r].herotype) {
			c2 = true;
		}

		if (c - 1 >= 0)
		if (tiles [c - 1, r].isFilled)
		if (s.herotype == tiles [c - 1, r].herotype) {
			c3 = true;
		}

		if (c - 2 >= 0)
		if (tiles [c - 2, r].isFilled)
		if (s.herotype == tiles [c - 2, r].herotype) {
			c4 = true;
		}
		if ((c1 && c2) || (c1 && c3) || (c3 && c4)) {
			finalc = true;
		}

		if (finalc || finalr) {
			goahead = true;
		} else
			goahead = false;

		//Debug.Log (s + "" + " c" + finalc + " r" + finalr + " :: r" + r1 + r2 + r3 + r4 + " :: c" + c1 + c2 + c3 + c4);
		return goahead;
	}

	void decideAction (Slot s1, Slot s2)
	{
		if (slot1.CanInteractible && slot2.CanInteractible)
		if ((s1.isFilled == true && s2.isFilled == true)) {
			dataSwap (s1, s2);
			if (CheckBeforeSwipe (s1, s2)) {
				
				dataSwap (s1, s2);
				HeroTransformSwap (slot1, slot2, true, false);

				Reset (PlayerControlID);

				//Invoke ("callcheck", driftseconds);
				Debug.Log ("Hogya");

			} else {
				HeroTransformSwap (slot1, slot2, false, true);
				dataSwap (s1, s2);
				Reset (PlayerControlID);
				print ("nhi Hua");
			}
		} else
			Reset (PlayerControlID);

	}


	void callcheck ()
	{
		//NewGridCheck (PlayerControlID);				
	}

	void Reset (int PID)
	{
		slot1 = null;
		slot2 = null;
		HighLighter [PID].SetActive (false);
		swipeDirection = Swipe.None;
	}

	void dataSwap (Slot s1, Slot s2)
	{
		Slot k1;
		Slot k2;
		GameObject h1, h2;

		k1 = s1;
		k2 = s2;
		h1 = k1.hero;
		h2 = k2.hero;
		//Debug.Log (h1 + " " + " " + h2);
		s1.Fill (h2, k1.position, k1.PlayerID);
		s2.Fill (h1, k2.position, k2.PlayerID);

	}

	void HeroTransformSwap (Slot s1, Slot s2, bool wantDataTransfer, bool WanaReturn)
	{
		//h1 = s1.hero;
		//h2 = s2.hero;
		ts1 = s1;
		ts2 = s2;

		s2.MoveTo (ts1, driftseconds, wantDataTransfer, WanaReturn);
		s1.MoveTo (ts2, driftseconds, wantDataTransfer, WanaReturn);

		//Debug.Log (Vector3.Distance (Player1slots [0, 0].position, Player1slots [0, 1].position));

	}

	#endregion

	#region Getting Input from Device

	public float minSwipeLength = 0.8f;
	Vector2 firstPressPos;
	Vector2 secondPressPos;
	Vector2 currentSwipe;

	public void MobileDetectSwipe ()
	{
		if (Input.touches.Length > 0) {
			Touch t = Input.GetTouch (0);

			if (t.phase == TouchPhase.Began) {
				firstPressPos = new Vector2 (t.position.x, t.position.y);
			}

			if (t.phase == TouchPhase.Ended) {
				secondPressPos = new Vector2 (t.position.x, t.position.y);
				currentSwipe = new Vector3 (secondPressPos.x - firstPressPos.x, secondPressPos.y - firstPressPos.y);

				// Make sure it was a legit swipe, not a tap
				if (currentSwipe.magnitude < minSwipeLength) {
					//Debug.Log ("Tapped");
					swipeDirection = Swipe.None;
					return;
				}

				currentSwipe.Normalize ();

				currentSwipe = new Vector3 (secondPressPos.x - firstPressPos.x, secondPressPos.y - firstPressPos.y);

				// Make sure it was a legit swipe, not a tap
				if (currentSwipe.magnitude > minSwipeLength) {

					swipeDirection = Checkswipe ();
					//Debug.Log (swipeDirection);

				}
			} else {
				swipeDirection = Swipe.None; 
				//debugInfo.text = "No swipe"; // if you display this, you will lose the debug text when you stop swiping
			}
		}
	}

	public void PCDetectSwipe ()
	{

		if (Input.GetMouseButtonDown (0)) {
			Vector3 vec;
			vec = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0));
			firstPressPos = new Vector2 (vec.x, vec.y);
			start = true;
		}

		if ((Input.GetMouseButton (0)) && (start)) {
			Vector3 vec;
			vec = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0));
			secondPressPos = new Vector2 (vec.x, vec.y);


			currentSwipe = new Vector3 (secondPressPos.x - firstPressPos.x, secondPressPos.y - firstPressPos.y);

			// Make sure it was a legit swipe, not a tap
			if (currentSwipe.magnitude > minSwipeLength) {

				swipeDirection = Checkswipe ();
				//Debug.Log (swipeDirection);
				start = false;
			}


		}

		//swipeDirection = Swipe.None; 
		//debugInfo.text = "No swipe"; // if you display this, you will lose the debug text when you stop swiping
	}

	Swipe Checkswipe ()
	{
		currentSwipe.Normalize ();

		//Debug.Log (currentSwipe.x.ToString () + " " + currentSwipe.y.ToString ());

		// Swipe up
		if (currentSwipe.y > 0 && currentSwipe.x > 0 - tweakFactor && currentSwipe.x < tweakFactor) {
			return Swipe.Up;
			//Debug.Log ("Up swipe");

			// Swipe down
		} else if (currentSwipe.y < 0 && currentSwipe.x > 0 - tweakFactor && currentSwipe.x < tweakFactor) {
			return Swipe.Down;
			//Debug.Log ("Down swipe");

			// Swipe left
		} else if (currentSwipe.x < 0 && currentSwipe.y > 0 - tweakFactor && currentSwipe.y < tweakFactor) {
			return Swipe.Left;
			//Debug.Log ("Left swipe");

			// Swipe right
		} else if (currentSwipe.x > 0 && currentSwipe.y > 0 - tweakFactor && currentSwipe.y < tweakFactor) {
			return Swipe.Right;
			//Debug.Log ("Right swipe");

			// Swipe up left
		} else if (currentSwipe.y > 0 && currentSwipe.x < 0) {
			return Swipe.UpLeft;
			//Debug.Log ("Up Left swipe");

			// Swipe up right
		} else if (currentSwipe.y > 0 && currentSwipe.x > 0) {
			return Swipe.UpRight;
			//Debug.Log ("Up Right swipe");

			// Swipe down left
		} else if (currentSwipe.y < 0 && currentSwipe.x < 0) {
			return Swipe.DownLeft;
			//Debug.Log ("Down Left swipe");

			// Swipe down right
		} else if (currentSwipe.y < 0 && currentSwipe.x > 0) {
			return Swipe.DownRight;
			//debugInfo.text = "Down Right swipe";
			//Debug.Log ("Down Right swipe");
		} else
			return Swipe.None;
	}

	#endregion

	#region OLD GridCheck and Generation

	/*void GridCheck (int PID)
	{
		Slot[,] slots;
		if (PID == 1)
			slots = Player1slots;
		else
			slots = Player2slots;


		int counter = 1;
		//ROW CHECK///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		for (int r = 0; r < Rows; r++) {
			counter = 1;
			for (int c = 1; c < Columns; c++) {
				//if (!slots[c, r])
				if (slots [c, r].PlayerID == PID)
				if (slots [c, r] != null && slots [c - 1, r] != null) {
					if (slots [c, r].herotype == slots [c - 1, r].herotype) {
						counter++;
						Debug.Log (counter);
					} else {
						counter = 1;
					}

					if (counter == 6) {

						CheckSlotAndDeativate (slots [c, r], counter);
						CheckSlotAndDeativate (slots [c - 1, r], counter);
						CheckSlotAndDeativate (slots [c - 2, r], counter);
						CheckSlotAndDeativate (slots [c - 3, r], counter);
						CheckSlotAndDeativate (slots [c - 4, r], counter);
						CheckSlotAndDeativate (slots [c - 5, r], counter);

					} else if (counter == 5) {
						CheckSlotAndDeativate (slots [c, r], counter);
						CheckSlotAndDeativate (slots [c - 1, r], counter);
						CheckSlotAndDeativate (slots [c - 2, r], counter);
						CheckSlotAndDeativate (slots [c - 3, r], counter);
						CheckSlotAndDeativate (slots [c - 4, r], counter);

					} else if (counter == 4) {
						CheckSlotAndDeativate (slots [c, r], counter);
						CheckSlotAndDeativate (slots [c - 1, r], counter);
						CheckSlotAndDeativate (slots [c - 2, r], counter);
						CheckSlotAndDeativate (slots [c - 3, r], counter);


					} else if (counter == 3) {

						CheckSlotAndDeativate (slots [c, r], counter);
						CheckSlotAndDeativate (slots [c - 1, r], counter);
						CheckSlotAndDeativate (slots [c - 2, r], counter);
					}


				}


			}
		}

		//COLUMN CHECK///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		for (int c = 0; c < Columns; c++) {
			counter = 1;
			for (int r = 1; r < Rows; r++) {
				if (slots [c, r].PlayerID == PID)
				if (slots [c, r] != null && slots [c, r - 1] != null) {
					if (slots [c, r].herotype == slots [c, r - 1].herotype) {
						counter++;  
						Debug.Log (counter);

					} else {
						counter = 1;
					}

					if (counter == 6) {

						CheckSlotAndDeativate (slots [c, r], counter);
						CheckSlotAndDeativate (slots [c, r - 1], counter);
						CheckSlotAndDeativate (slots [c, r - 2], counter);
						CheckSlotAndDeativate (slots [c, r - 3], counter);
						CheckSlotAndDeativate (slots [c, r - 4], counter);
						CheckSlotAndDeativate (slots [c, r - 5], counter);

					} else if (counter == 5) {
						CheckSlotAndDeativate (slots [c, r], counter);
						CheckSlotAndDeativate (slots [c, r - 1], counter);
						CheckSlotAndDeativate (slots [c, r - 2], counter);
						CheckSlotAndDeativate (slots [c, r - 3], counter);
						CheckSlotAndDeativate (slots [c, r - 4], counter);

					} else if (counter == 4) {
						CheckSlotAndDeativate (slots [c, r], counter);
						CheckSlotAndDeativate (slots [c, r - 1], counter);
						CheckSlotAndDeativate (slots [c, r - 2], counter);
						CheckSlotAndDeativate (slots [c, r - 3], counter);


					} else if (counter == 3) {

						CheckSlotAndDeativate (slots [c, r], counter);
						CheckSlotAndDeativate (slots [c, r - 1], counter);
						CheckSlotAndDeativate (slots [c, r - 2], counter);
					}

				}
			}

		}


	}

	void CheckSlotAndDeativate (Slot sl, int counter)
	{
		
		if (sl.hero != null) {
			sl.hero.SetActive (false);
			sl.hero = null;

			Invoke ("RenewGrid", 0.5f);


		}

	}

	void RenewGrid ()
	{
		bool anyMoved = false;
		PoolShuffle ();
		for (int r = 1; r < Rows; r++) {
			for (int c = 0; c < Columns; c++) {
				if (r == Rows - 1 && tiles [c, r].hero == null) {
					Vector3 tilePos = new Vector3 (c, r, 0);
					for (int n = 0; n < heroPool.Count; n++) {
						GameObject o = heroPool [n];

						if (!o.activeSelf) {
							o.transform.position = new Vector3 (tilePos.x, tilePos.y, tilePos.z);
							o.SetActive (true);
							tiles [c, r].hero = o;
							n = heroPool.Count + 1;
						}
					}
				}

				if (tiles [c, r].hero != null) {
					if (tiles [c, r - 1].hero == null) {
						tiles [c, r - 1].Fill (tiles [c, r].hero, tiles [c, r - 1].position, tiles [c, r - 1].PlayerID);
						tiles [c, r - 1].hero.transform.position = new Vector3 (c, r - 1, 0);
						tiles [c, r].hero = null;
						anyMoved = true;
					}
				}
			}
		}

		if (anyMoved) {
			Invoke ("RenewGrid", 0.5f);
			//Debug.Log ("Match");
		}	
	}*/

	#endregion

	#region NEW GRID CHECK & GENERATION LOGIC

	void NewGridCheck (int PID)
	{
		Slot[,] slots;
		if (PID == 1)
			slots = Player1slots;
		else
			slots = Player2slots;


		int counter = 1;
		//ROW CHECK///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		for (int r = 0; r < Rows; r++) {
			counter = 1;
			for (int c = 1; c < Columns; c++) {
				//if (!slots[c, r])
				if (slots [c, r].PlayerID == PID)
				if (slots [c, r].CanInteractible && slots [c - 1, r].CanInteractible) {
					if (slots [c, r].isFilled && slots [c - 1, r].isFilled) {
						if (slots [c, r].herotype == slots [c - 1, r].herotype) {
							counter++;

						} else {
							counter = 1;
						}

						if (counter == 6) {
							CheckSlotAndDeativate (slots [c, r], 6);
							CheckSlotAndDeativate (slots [c - 1, r], 6);
							CheckSlotAndDeativate (slots [c - 2, r], 6);
							CheckSlotAndDeativate (slots [c - 3, r], 6);
							CheckSlotAndDeativate (slots [c - 4, r], 6);
							CheckSlotAndDeativate (slots [c - 5, r], 6);

						} else if (counter == 5) {
							CheckSlotAndDeativate (slots [c, r], 5);
							CheckSlotAndDeativate (slots [c - 1, r], 5);
							CheckSlotAndDeativate (slots [c - 2, r], 5);
							CheckSlotAndDeativate (slots [c - 3, r], 5);
							CheckSlotAndDeativate (slots [c - 4, r], 5);

						} else if (counter == 4) {
							CheckSlotAndDeativate (slots [c, r], 4);
							CheckSlotAndDeativate (slots [c - 1, r], 4);
							CheckSlotAndDeativate (slots [c - 2, r], 4);
							CheckSlotAndDeativate (slots [c - 3, r], 4);


						} else if (counter == 3) {

							CheckSlotAndDeativate (slots [c, r], 3);
							CheckSlotAndDeativate (slots [c - 1, r], 3);
							CheckSlotAndDeativate (slots [c - 2, r], 3);
						}


					}
				}



			}
		}

		//COLUMN CHECK///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		for (int c = 0; c < Columns; c++) {
			counter = 1;
			for (int r = 1; r < Rows; r++) {
				if (slots [c, r].PlayerID == PID)
				if (slots [c, r].CanInteractible && slots [c, r - 1].CanInteractible) {
					//if (slots [c, r].isFilled && slots [c, r - 1].isFilled) {
					if (slots [c, r].herotype == slots [c, r - 1].herotype) {
						counter++;  


					} else {
						counter = 1;
					}

					if (counter == 6) {

						CheckSlotAndDeativate (slots [c, r], 6);
						CheckSlotAndDeativate (slots [c, r - 1], 6);
						CheckSlotAndDeativate (slots [c, r - 2], 6);
						CheckSlotAndDeativate (slots [c, r - 3], 6);
						CheckSlotAndDeativate (slots [c, r - 4], 6);
						CheckSlotAndDeativate (slots [c, r - 5], 6);

					} else if (counter == 5) {
						CheckSlotAndDeativate (slots [c, r], 5);
						CheckSlotAndDeativate (slots [c, r - 1], 5);
						CheckSlotAndDeativate (slots [c, r - 2], 5);
						CheckSlotAndDeativate (slots [c, r - 3], 5);
						CheckSlotAndDeativate (slots [c, r - 4], 5);

					} else if (counter == 4) {
						CheckSlotAndDeativate (slots [c, r], 4);
						CheckSlotAndDeativate (slots [c, r - 1], 4);
						CheckSlotAndDeativate (slots [c, r - 2], 4);
						CheckSlotAndDeativate (slots [c, r - 3], 4);


					} else if (counter == 3) {

						CheckSlotAndDeativate (slots [c, r], 3);
						CheckSlotAndDeativate (slots [c, r - 1], 3);
						CheckSlotAndDeativate (slots [c, r - 2], 3);
					}
					//}

				}
			}

		}

		if (renewBoard) {
			RenewGrid ();
			renewBoard = false;
		}

	}

	void CheckSlotAndDeativate (Slot sl, int counter)
	{
		Debug.Log (counter);
		if (sl.isFilled) {
			sl.hero.SetActive (false);
			sl.CanInteractible = false;
			sl.MatchCount = counter;
			sl.isFilled = false;
			renewBoard = true;
		}

	}


	void RenewGrid ()
	{
		anyMoved = false;
		PoolShuffle ();

		for (int r = 0; r < Rows; r++) {
			for (int c = 0; c < Columns; c++) {

				//Debug.Log (tiles [c, r].herotype + tiles [c, r].c + tiles [c, r].r);

				if (!tiles [c, r].isFilled) {
					if (r == 0) {
						for (int n = 0; n < heroPool.Count; n++) {
							GameObject o = heroPool [n];

							if (!o.activeSelf) {
								o.transform.position = tiles [c, r].position;
								//o.animation
								o.SetActive (true);
								tiles [c, r].hero = o;
								n = heroPool.Count + 1;
								tiles [c, r].Col (Color.red);
							}
							tiles [c, r].Fill (o, tiles [c, r].position, PlayerControlID);

						}
					}


					if (r - 1 >= 0) {
						
						SingleHeroMove (tiles [c, r - 1].hero, tiles [c, r - 1], tiles [c, r], false);
						tiles [c, r].Fill (tiles [c, r - 1].hero, tiles [c, r].position, tiles [c, r].PlayerID);
						if (tiles [c, r - 1].isFilled)
							tiles [c, r - 1].isFilled = false;

						anyMoved = true;
					} 

					//Invoke ("RenewGrid", cascadecheckSpeed);
				} 

			}

		}

		if (anyMoved)
			Invoke ("RenewGrid", cascadecheckSpeed);

	}

	void SingleHeroMove (GameObject objectToMove, Slot startslot, Slot endslot, bool wantDataTransfer)
	{
		
		//from.MoveTo (to, 1f, wantDataTransfer, WanaReturn);
		endslot.heromovingto (objectToMove, startslot, cascadeobjectspeed, wantDataTransfer);

	}


	void GridFill ()
	{
		
	}

	#endregion
}



