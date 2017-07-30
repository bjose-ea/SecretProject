using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
//	public int[] randomArrayOfPlayerOneHeroes;
//	public int[] randomArrayOfPlayerOneHeroes;
	public GameObject HighLighterprefab;
	public int PlayerControlID;
	public List<PlayerBoard> playerBoards;
	public GameObject playerBoardPrefab;
	float tweakFactor = 0.5f;
	Swipe swipeDirection = Swipe.None;
//	private bool isGameOver;

	GameObject[] HighLighter;
//	public List<Heroes> heroPool = new List<Heroes> ();
//	public Slot[,] Player1slots;
//	public Slot[,] Player2slots;
	[HideInInspector]
	public bool start = false;
	public float cascadecheckSpeed = 0.5f;
	public float cascadeobjectspeed = 0.6f;
	bool syncPending = false;
	PhotonView _photonView;

	public bool anyMoved = false;
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

//	void PoolShuffle ()
//	{
//		System.Random rand = new System.Random ();
//		int r = heroPool.Count;
//		while (r > 1) {
//			r--;
//
//			int n = rand.Next (r + 1);
//			Heroes val = heroPool [n];
//			heroPool [n] = heroPool [r];
//			heroPool [r] = val;
//		}
//	}

	int[] GetRandomArrayOfHeroes()
	{
		int[] temp = new int[Rows*Columns];
		for(int i = 0; i < temp.Length; i++)
		{
			temp[i] = UnityEngine.Random.Range(0, Enum.GetNames(typeof(HeroType)).Length);
		}
		return temp;
	}

	void IntializingHeroPool (int rows, int cols, List<Heroes> HeroList, bool Shuffle)
	{
		int numCopies = (rows * cols);
		GameObject HeroPoolParent = new GameObject ("HeroPoolParent");
		for (int i = 0; i < numCopies; i++) 
		{
			for (int j = 0; j < HeroesTypes.Length; j++) 
			{
				GameObject o = (GameObject)Instantiate (HeroesTypes [j]);

				o.SetActive (false);
				o.transform.parent = HeroPoolParent.transform;
				HeroList.Add (o.GetComponent<Heroes>());
				o.GetComponent<Heroes>().heroType = (HeroType)j;
			}
		}
//		if (Shuffle)
//			PoolShuffle ();

	}

	/// <summary>
	/// 	Callback raised by the Photon server when any of the custom property is changed for any player
	/// 	currently residing in the same room and is sent to all the clients...
	/// </summary>
	/// <param name="playerAndUpdatedProps">Player and updated properties as hashtable</param>
	void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps) 
	{
		PhotonPlayer remotePlayer 				 = playerAndUpdatedProps[0] as PhotonPlayer;
		ExitGames.Client.Photon.Hashtable  props = playerAndUpdatedProps[1] as ExitGames.Client.Photon.Hashtable;

		if(props != null && props.ContainsKey(PlayerProperty.PLAYER_BOARD_HEROES))
		{
			playerBoards[remotePlayer.GetPlayersIndex()].randomArrayOfHeroes = remotePlayer.GetPlayerBoardHeroes();
			//Filling heroes on the board and saving them into "slots" for both Player 1 and player 2
			InitiaizingSlots (remotePlayer.GetPlayersIndex());
		}
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


	void InitiaizingSlots (int i)
	{
		Debug.Log("playerIndex>>" + i);

		float Yoffset = 0;
		int index = 0;
		Vector3 slotPos = new Vector3 (Columns, Rows, 0);
		for (int r = 0; r < Rows; r++)
		{
			for (int c = 0; c < Columns; c++) 
			{
				slotPos = new Vector3 (c, r, 0);
				//Initializing Slots and filing slots 2D array
				GameObject sl = (GameObject)Instantiate (SlotPrefab, slotPos, SlotPrefab.transform.rotation);
				sl.transform.parent = playerBoards[i].transform;
				sl.name = "p: " + 1 + " + slot (" + c + " , " + r + ")";
				playerBoards[i].PlayerSlots [c, r] = sl.GetComponent<Slot> ();
				playerBoards[i].PlayerSlots [c, r].slotIndex = index;

				GameObject o = (GameObject)Instantiate (HeroesTypes[playerBoards[i].randomArrayOfHeroes [index]]);
				o.transform.SetParent(playerBoards[i].PlayerSlots [c, r].gameObject.transform, false);
				o.SetActive (true);
				playerBoards[i].PlayerSlots [c, r].Fill (o, slotPos, i+1);
				playerBoards[i].PlayerSlots [c, r].c = c;
				playerBoards[i].PlayerSlots [c, r].r = r;
				o.name = o.name + "(" + c + ", " + r + ")";
				index++;

			}
		}
		if(PhotonNetwork.player.GetPlayersIndex() == i)
			Yoffset = 0;
		else
			Yoffset = DistanceBetweentwoBoards;
		playerBoards[i].transform.position = new Vector3 (playerBoards[i].transform.position.x, playerBoards[i].transform.position.y + Yoffset, playerBoards[i].transform.position.z);
			
	}

	#endregion

	List<Slot> MatchSlot = new List<Slot> ();



	void Awake()
	{
		_photonView = GetComponent<PhotonView>();
//		isGameOver = true;
	}

	void OnEnable()
	{
		NetworkManager.OnRoomFullEvent += HandleStartGame;
	}

	void OnDisable()
	{
		NetworkManager.OnRoomFullEvent -= HandleStartGame;
	}

	void Start()
	{

		InitializeBoard();
	}

	void HandleStartGame()
	{

		if(PhotonNetwork.isMasterClient)
		{
			for(int i = 0; i < PhotonNetwork.playerList.Length; i++)
			{
				PhotonNetwork.playerList[i].SetPlayerBoardHeroes(GetRandomArrayOfHeroes());
			}
		}

		UIManager.GetInstance().goWaitingPanel.SetActive(false);
			
	}

	void InitializeBoard ()
	{ 

		//Pointers creation
		InitializingPointers ();


		playerBoards[0].PlayerSlots = new Slot[Columns, Rows];
		playerBoards[1].PlayerSlots = new Slot[Columns, Rows];

		PlayerControlID = PhotonNetwork.player.GetPlayersIndex() + 1;
		if (PlayerControlID == 1)
			tiles = playerBoards[0].PlayerSlots;
		else
			tiles = playerBoards[1].PlayerSlots;

	}



	void Update ()
	{
		/*/while (matched == true) {
			//StartCheck (PlayerControlID);
		}*/

//		if(isGameOver)
//			return;
		
		if (canInput) {
			//FixingInputManager (PlayerControlID);
			//SwipeInputManager (PlayerControlID, true);
			EfficientSwipeManager (PlayerControlID);
			//IntelligentInputManager (PlayerControlID);


		}

//		if (PlayerControlID == 1)
//			tiles = Player1slots;
//		else
//			tiles = Player2slots;
		
		PCDetectSwipe ();

		//GridCheck (PlayerControlID);

		if (Input.GetKeyDown (KeyCode.R)) {
			Application.LoadLevel (Application.loadedLevelName);
		}

		if (Input.GetKeyDown (KeyCode.A)) {////////BUG CRASHED

			//NewGridCheck (PlayerControlID);
		}

		if (Input.GetKeyDown (KeyCode.D)) {////////BUG CRASHED

			StartCoroutine (RenewGrid (0));
		}
			
	}


	#region Puzzles and dragon Input Sysytem (NOT WORKING PROPERLY)

	bool temp = true;
	[HideInInspector]
	public Slot prev;
	[HideInInspector]
	public Slot SelectedSlot;
	[HideInInspector]
	public Heroes SelectedHero;
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
			slot1.heroMoveTo (SelectedHero.gameObject, driftseconds, false, false);
			slot1.Fill (SelectedHero.gameObject, slot1.position, slot1.PlayerID);
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
		prev.Fill (s2.hero.gameObject, prev.position, s2.PlayerID);
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
			Slotlib = playerBoards[0].PlayerSlots;
		else if (PID == 2)
			Slotlib = playerBoards[1].PlayerSlots;
			

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
		if (s.hero.heroType == tiles [c, r + 1].hero.heroType) {
			r1 = true;
		}	

		if (r + 2 < Rows)
		if (tiles [c, r + 2].isFilled)
		if (s.hero.heroType == tiles [c, r + 2].hero.heroType) {
			r2 = true;
		}

		if (r - 1 >= 0)
		if (tiles [c, r - 1].isFilled)
		if (s.hero.heroType == tiles [c, r - 1].hero.heroType) {
			r3 = true;
		}
		if (r - 2 >= 0)
		if (tiles [c, r - 2].isFilled)
		if (s.hero.heroType == tiles [c, r - 2].hero.heroType) {
			r4 = true;
		}

		if ((r1 && r2) || (r1 && r3) || (r3 && r4)) {
			finalr = true;
		}

		//Columns
		if (c + 1 < Columns)
		if (tiles [c + 1, r].isFilled)
		if (s.hero.heroType == tiles [c + 1, r].hero.heroType) {
			c1 = true;
		}	

		if (c + 2 < Columns)
		if (tiles [c + 2, r].isFilled)
		if (s.hero.heroType == tiles [c + 2, r].hero.heroType) {
			c2 = true;
		}

		if (c - 1 >= 0)
		if (tiles [c - 1, r].isFilled)
		if (s.hero.heroType == tiles [c - 1, r].hero.heroType) {
			c3 = true;
		}

		if (c - 2 >= 0)
		if (tiles [c - 2, r].isFilled)
		if (s.hero.heroType == tiles [c - 2, r].hero.heroType) {
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
				StartCoroutine (NewGridCheck (PlayerControlID, driftseconds));
				//Invoke ("callcheck", driftseconds);


			} else {
				HeroTransformSwap (slot1, slot2, false, true);
				dataSwap (s1, s2);
				Reset (PlayerControlID);
			}
		} else
			Reset (PlayerControlID);

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
		h1 = k1.hero.gameObject;
		h2 = k2.hero.gameObject;
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

	#region NEW GRID CHECK & GENERATION LOGIC


	IEnumerator NewGridCheck (int PID, float t)
	{
		
		yield return new WaitForSeconds (t);
		Slot[,] slots;
		if (PID == 1)
			slots = playerBoards[0].PlayerSlots;
		else
			slots = playerBoards[1].PlayerSlots;


		int counter = 1;
		//ROW CHECK///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		for (int r = 0; r < Rows; r++) {
			counter = 1;

			for (int c = 1; c < Columns; c++) {
				//if (!slots[c, r])
				if (slots [c, r].PlayerID == PID)
				if (slots [c, r].CanInteractible && slots [c - 1, r].CanInteractible) {
					if (slots [c, r].isFilled && slots [c - 1, r].isFilled) {
						if (slots [c, r].hero.heroType == slots [c - 1, r].hero.heroType) {
							
							counter++;

						} else {
							//Debug.Log (counter);	
							counter = 1;
						}


						//RSlot [counter] = slots [c - 1, r];
						if (counter == 6) {
							MatchSlot.Clear ();
							MatchSlot.Add (slots [c, r]);
							MatchSlot.Add (slots [c - 1, r]);
							MatchSlot.Add (slots [c - 2, r]);
							MatchSlot.Add (slots [c - 3, r]);
							MatchSlot.Add (slots [c - 4, r]);
							MatchSlot.Add (slots [c - 5, r]);
						} else if (counter == 5) {
							MatchSlot.Clear ();
							MatchSlot.Add (slots [c, r]);
							MatchSlot.Add (slots [c - 1, r]);
							MatchSlot.Add (slots [c - 2, r]);
							MatchSlot.Add (slots [c - 3, r]);
							MatchSlot.Add (slots [c - 4, r]);
						} else if (counter == 4) {

							MatchSlot.Clear ();
							MatchSlot.Add (slots [c, r]);
							MatchSlot.Add (slots [c - 1, r]);
							MatchSlot.Add (slots [c - 2, r]);
							MatchSlot.Add (slots [c - 3, r]);

						} else if (counter == 3) {
							MatchSlot.Clear ();
							MatchSlot.Add (slots [c, r]);
							MatchSlot.Add (slots [c - 1, r]);
							MatchSlot.Add (slots [c - 2, r]);

						}

						//Debug.Log (tiles [c, r].herotype + tiles [c, r].c + tiles [c, r].r + "");

						/*if (c == Columns - 1) {
							
						}*/

					}

				}



			}
			Debug.Log("MatchSlot>" + MatchSlot.Count);
			CheckSlotAndDeativate ();
		}

		//COLUMN CHECK///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		for (int c = 0; c < Columns; c++) {
			counter = 1;
			for (int r = 1; r < Rows; r++) {
				if (slots [c, r].PlayerID == PID)
				if (slots [c, r].CanInteractible && slots [c, r - 1].CanInteractible) {
					//if (slots [c, r].isFilled && slots [c, r - 1].isFilled) {
					if (slots [c, r].hero.heroType == slots [c, r - 1].hero.heroType) {
						counter++;  


					} else {
						counter = 1;
					}


					//RSlot [counter] = slots [c - 1, r];
					if (counter == 6) {
						MatchSlot.Clear ();
						MatchSlot.Add (slots [c, r]);
						MatchSlot.Add (slots [c, r - 1]);
						MatchSlot.Add (slots [c, r - 2]);
						MatchSlot.Add (slots [c, r - 3]);
						MatchSlot.Add (slots [c, r - 4]);
						MatchSlot.Add (slots [c, r - 5]);
					} else if (counter == 5) {
						MatchSlot.Clear ();
						MatchSlot.Add (slots [c, r]);
						MatchSlot.Add (slots [c, r - 1]);
						MatchSlot.Add (slots [c, r - 2]);
						MatchSlot.Add (slots [c, r - 3]);
						MatchSlot.Add (slots [c, r - 4]);
					} else if (counter == 4) {

						MatchSlot.Clear ();
						MatchSlot.Add (slots [c, r]);
						MatchSlot.Add (slots [c, r - 1]);
						MatchSlot.Add (slots [c, r - 2]);
						MatchSlot.Add (slots [c, r - 3]);

					} else if (counter == 3) {
						MatchSlot.Clear ();
						MatchSlot.Add (slots [c, r]);
						MatchSlot.Add (slots [c, r - 1]);
						MatchSlot.Add (slots [c, r - 2]);

					}
						

				}
			}
			CheckSlotAndDeativate ();

		}


		//StartCoroutine (delay ());

		if (renewBoard) {
			//t = false;
			StartCoroutine (RenewGrid (0));
			renewBoard = false;
		}
	}

	/*IEnumerator delay(float f)
	{
		if (renewBoard) {
			//t = false;
			StartCoroutine (RenewGrid (0));
			renewBoard = false;
		}
	}*/

	void CheckSlotAndDeativate ()
	{
		int i = 0;
		for (i = 0; i < MatchSlot.Count; i++) {
			if (MatchSlot [i].isFilled) {
				MatchSlot [i].hero.gameObject.SetActive (false);
				MatchSlot [i].CanInteractible = false;
				MatchSlot [i].MatchCount = MatchSlot.Count;
				MatchSlot [i].isFilled = false;
				renewBoard = true;
				MatchSlot [i].hero.GotMatch (MatchSlot.Count);
			}
		}
		MatchSlot.Clear ();
	}



	//bool t = false;

	IEnumerator RenewGrid (float k)
	{
		anyMoved = false;
//		PoolShuffle (); //TODO

		yield return new WaitForSeconds (k);

		if (PlayerControlID == 1)
		{
			for (int r = 0; r < Rows; r++) 
			{
				for (int c = 0; c < Columns; c++) 
				{

					if (!tiles [c, r].isFilled) 
					{
						if (r == 0) 
						{

							GameObject o = Instantiate(HeroesTypes [UnityEngine.Random.Range(0, Enum.GetNames(typeof(HeroType)).Length)]);//heroPool [n].gameObject;
							o.transform.position = tiles [c, r].position;
							o.SetActive (true);
							tiles [c, r].hero = o.GetComponent<Heroes>();
							tiles [c, r].Col (Color.red);

							tiles [c, r].Fill (o, tiles [c, r].position, PlayerControlID);
						}


						if (r - 1 >= 0) 
						{
						
							SingleHeroMove (tiles [c, r - 1].hero.gameObject, tiles [c, r - 1], tiles [c, r], false);
							tiles [c, r].Fill (tiles [c, r - 1].hero.gameObject, tiles [c, r].position, tiles [c, r].PlayerID);
							if (tiles [c, r - 1].isFilled)
								tiles [c, r - 1].isFilled = false;

							anyMoved = true;
						} 
					} 

				}

			}
		}
		if (PlayerControlID == 2)
		{
			for (int r = 0; r < Rows; r++) 
			{
				for (int c = 0; c < Columns; c++) 
				{
					if (!tiles [c, r].isFilled) 
					{
						if (r == 0)
						{
							GameObject o = Instantiate(HeroesTypes [UnityEngine.Random.Range(0, Enum.GetNames(typeof(HeroType)).Length)]);
							o.transform.position = tiles [c, r].position;
							o.SetActive (true);
							tiles [c, r].hero = o.GetComponent<Heroes>();
							tiles [c, r].Col (Color.red);
							tiles [c, r].Fill (o, tiles [c, r].position, PlayerControlID);
						}


						if (r - 1 >= 0) 
						{

							SingleHeroMove (tiles [c, r - 1].hero.gameObject, tiles [c, r - 1], tiles [c, r], false);
							tiles [c, r].Fill (tiles [c, r - 1].hero.gameObject, tiles [c, r].position, tiles [c, r].PlayerID);
							if (tiles [c, r - 1].isFilled)
								tiles [c, r - 1].isFilled = false;

							anyMoved = true;
						} 
					} 

				}

			}
		}

		if (anyMoved) 
		{
			StartCoroutine (RenewGrid (cascadecheckSpeed));//restart ();

		} 
		else 
		{		
			StartCoroutine (NewGridCheck (PlayerControlID, 0));
		}

	}

	void restart ()
	{
		//yield return new WaitForSeconds (k);
		StartCoroutine (RenewGrid (cascadecheckSpeed));
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

	public bool matched = false;

	void StartCheck (int PID)
	{


		int counter = 1;
		//ROW CHECK///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		for (int r = 0; r < Rows; r++) {
			counter = 1;

			for (int c = 1; c < Columns; c++) {
				//if (!slots[c, r])
				if (tiles [c, r].PlayerID == PID)
				if (tiles [c, r].CanInteractible && tiles [c - 1, r].CanInteractible) {
					if (tiles [c, r].isFilled && tiles [c - 1, r].isFilled) {
						if (tiles [c, r].hero.heroType == tiles [c - 1, r].hero.heroType) {

							counter++;

						} else {
							//Debug.Log (counter);	
							counter = 1;
						}


						//RSlot [counter] = slots [c - 1, r];
						if (counter == 6) {
							
							fillother (tiles [c, r]);
							fillother (tiles [c - 1, r]);
							fillother (tiles [c - 2, r]);
							fillother (tiles [c - 3, r]);
							fillother (tiles [c - 4, r]);
							fillother (tiles [c - 5, r]);

						} else if (counter == 5) {
							
							fillother (tiles [c, r]);
							fillother (tiles [c - 1, r]);
							fillother (tiles [c - 2, r]);
							fillother (tiles [c - 3, r]);
							fillother (tiles [c - 4, r]);

						} else if (counter == 4) {


							fillother (tiles [c, r]);
							fillother (tiles [c - 1, r]);
							fillother (tiles [c - 2, r]);
							fillother (tiles [c - 3, r]);


						} else if (counter == 3) {
							
							fillother (tiles [c, r]);
							fillother (tiles [c - 1, r]);
							fillother (tiles [c - 2, r]);

						} else
							matched = false;
							

					}

				}



			}
		}

		//COLUMN CHECK///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		for (int c = 0; c < Columns; c++) {
			counter = 1;
			for (int r = 1; r < Rows; r++) {
				if (tiles [c, r].PlayerID == PID)
				if (tiles [c, r].CanInteractible && tiles [c, r - 1].CanInteractible) {
					//if (slots [c, r].isFilled && slots [c, r - 1].isFilled) {
					if (tiles [c, r].hero.heroType == tiles [c, r - 1].hero.heroType) {
						counter++;  


					} else {
						counter = 1;
					}


					//RSlot [counter] = slots [c - 1, r];
					if (counter == 6) {
						
						fillother (tiles [c, r]);
						fillother (tiles [c, r - 1]);
						fillother (tiles [c, r - 2]);
						fillother (tiles [c, r - 3]);
						fillother (tiles [c, r - 4]);
						fillother (tiles [c, r - 5]);

					} else if (counter == 5) {
						
						fillother (tiles [c, r]);
						fillother (tiles [c, r - 1]);
						fillother (tiles [c, r - 2]);
						fillother (tiles [c, r - 3]);
						fillother (tiles [c, r - 4]);

					} else if (counter == 4) {


						fillother (tiles [c, r]);
						fillother (tiles [c, r - 1]);
						fillother (tiles [c, r - 2]);
						fillother (tiles [c, r - 3]);


					} else if (counter == 3) {
						
						fillother (tiles [c, r]);
						fillother (tiles [c, r - 1]);
						fillother (tiles [c, r - 2]);


					} else
						matched = false;



				}
			}

		}



	}


	void fillother (Slot s)
	{
		matched = true;
		s.hero.gameObject.SetActive (false);
//		for (int n = 0; n < heroPool.Count; n++) 
//		{
		GameObject o = Instantiate(HeroesTypes [UnityEngine.Random.Range(0, Enum.GetNames(typeof(HeroType)).Length)]);//heroPool [n].gameObject;

//			if (!o.activeSelf) {
				o.transform.position = s.position;
				//o.animation
				o.SetActive (true);
				s.hero = o.GetComponent<Heroes>();
//				n = heroPool.Count + 1;
				//s [c, r].Col (Color.red);
//			}
			s.Fill (o, s.position, PlayerControlID);

//		}
	}
}





