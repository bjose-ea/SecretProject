using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCreationScript : MonoBehaviour
{
	public int Rows = 10;
	public int Columns = 6;
	public GameObject SlotPrefab;
	public GameObject[] HeroesTypes;
	public GameObject HighLighterprefab;
	GameObject[] HighLighter;
	public int PlayerControlID;
	[HideInInspector]
	public bool canInput = true;
	int horzDist;
	int vertDist;
	GameObject h1;
	GameObject h2;
	Slot ts1;
	Slot ts2;
	bool renewBoard;
	List<GameObject> heroPool = new List<GameObject> ();
	Slot[,] slots;


	[HideInInspector]
	public Slot slot1;
	[HideInInspector]
	public Slot slot2;


	public float driftseconds = 1;
	float driftTimer = 0;
    
	[HideInInspector]
	public bool isDrifting;

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
		int numCopies = (rows * cols) / 2;
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

	void Start ()
	{
       

		HighLighter = new GameObject[3];
		HighLighter [1] = (GameObject)Instantiate (HighLighterprefab, new Vector3 (-6, -6, 0), transform.rotation);
		HighLighter [1].name = "Pointer_Player1";
		HighLighter [1].SetActive (false);
		HighLighter [2] = (GameObject)Instantiate (HighLighterprefab, new Vector3 (-6, -6, 0), transform.rotation);
		HighLighter [2].name = "Pointer_Player2";
		HighLighter [2].SetActive (false);

		slots = new Slot[Columns, Rows];
		GameObject SlotsParent = new GameObject ("SlotsParent");
        

		// object Pooling
		//Hero pooling and saving them into a list "heroPool"
		IntializingHeroPool (Rows, Columns, heroPool, true);

		for (int r = 0; r < Rows; r++) {
			for (int c = 0; c < Columns; c++) {
				Vector3 slotPos = new Vector3 (c, r, 0);

				//Initializing Slots and filing slots 2D array
				GameObject sl = (GameObject)Instantiate (SlotPrefab, slotPos, SlotPrefab.transform.rotation);
				sl.transform.parent = SlotsParent.transform;
				sl.name = "slot (" + c + " , " + r + ")";
				slots [c, r] = sl.GetComponent<Slot> ();

				//Initializing Heroes, setting other parameters for each slots (like player ID, Hero name, is filled)
				for (int n = 0; n < heroPool.Count; n++) {
					GameObject o = heroPool [n];
					if (!o.activeSelf) {
						o.transform.position = new Vector3 (slotPos.x, slotPos.y, slotPos.z);
						o.SetActive (true);

						if (r < Rows / 2)
							slots [c, r].Fill (o, slotPos, 1);
						else
							slots [c, r].Fill (o, slotPos, 2);


						o.name = o.name + "(" + c + ", " + r + ")";

						n = heroPool.Count + 1;
					}
				}

			}
		}

	}

	void Update ()
	{
		if (canInput) {
			InputManager (PlayerControlID);
			// OtherInputManager(PlayerControlID);
		}
            

		if (isDrifting) {
			driftTimer += Time.deltaTime;
			if (driftTimer > driftseconds) {
				StopDrift ();
			} else {
				float ratio = driftTimer / driftseconds;
				h1.transform.position = Vector3.Lerp (h1.transform.position, ts2.position, ratio);
				h2.transform.position = Vector3.Lerp (h2.transform.position, ts1.position, ratio);
			}
		}

        
	}

	void InputManager (int PID)
	{
		if ((Input.GetMouseButtonDown (0)) && (!slot1)) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit2D hit = Physics2D.GetRayIntersection (ray, 1000, 1 << 8); // '8' is the "Slotslayer"

			if (hit) {
				if (hit.collider.gameObject.GetComponent<Slot> ()) {
					if (hit.collider.gameObject.GetComponent<Slot> ().lockedon == false)
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

		} else if ((Input.GetMouseButton (0)) && slot1) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit2D hit = Physics2D.GetRayIntersection (ray, 1000, 1 << 8);

			if (hit) {
				if (hit.collider.gameObject.GetComponent<Slot> ().PlayerID == PID)
					slot2 = hit.collider.gameObject.GetComponent<Slot> ();

				if ((slot1 && slot2) && (slot1.PlayerID == slot2.PlayerID)) {
					horzDist = (int)Mathf.Abs (slot1.position.x - slot2.position.x);
					vertDist = (int)Mathf.Abs (slot1.position.y - slot2.position.y);

					if ((horzDist > 1) || (vertDist > 1)) {

						slot1 = null;
						slot2 = null;
						HighLighter [PID].SetActive (false);

					} else if ((horzDist == 1 ^ vertDist == 1)) {


						//Transform the real position
						if (!slot1.lockedon && !slot2.lockedon)
							HeroTransformSwap (slot1, slot2, false);

						slot1 = null;
						slot2 = null;
						HighLighter [PID].SetActive (false);
					}

				}
                

				if (hit.collider.gameObject.GetComponent<Slot> ().PlayerID != PID) {
					slot1 = null;
					slot2 = null;
					HighLighter [PID].SetActive (false);
				}


			} else {
				Ray ray1 = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit2D hit1 = Physics2D.GetRayIntersection (ray1, 1000, 1 << 11);
				if (hit1) {
					slot1 = null;
					slot2 = null;
					HighLighter [PID].SetActive (false);
				}

			}
            

		}      

	}

	void SlotDataSwap (Slot s1, Slot s2)
	{
		GameObject temphero = s2.hero.gameObject;
		int tempplayerID = s2.PlayerID;

		s2.Fill (s1.hero.gameObject, s2.position, s1.PlayerID);
		s1.Fill (temphero, s1.position, tempplayerID);

	}

	void HeroTransformSwap (Slot s1, Slot s2, bool teleport)
	{

		if (teleport) {
			Vector3 pos = s1.hero.transform.position;
			s1.hero.transform.position = s2.hero.transform.position;
			s2.hero.transform.position = pos;

			SlotDataSwap (ts1, ts2);
		} else {
			h1 = s1.hero.gameObject;
			h2 = s2.hero.gameObject;
			ts1 = s1;
			ts2 = s2;

			StartDrift ();

            
		}

		CheckGrid (PlayerControlID);////////////////////////////////////////////////////////////////////////////////////////////////////////

	}

	private void StartDrift ()
	{
		driftTimer = 0;
		isDrifting = true;
		canInput = false;
		SlotDataSwap (ts1, ts2);

	}

	private void StopDrift ()
	{
        
		isDrifting = false;
		//SlotDataSwap(ts1, ts2);
		h1 = null;
		h2 = null;
		ts1 = null;
		ts2 = null;
		canInput = true;

	}

	bool temp = true;
	Slot prev;

	void OtherInputManager (int PID)
	{
        
		if (Input.GetMouseButton (0)) {

			if (Input.GetMouseButtonDown (0)) {
				Ray ray1 = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit2D hit1 = Physics2D.GetRayIntersection (ray1, 1000, 1 << 8); // '8' is the "Slotslayer"

				if (hit1) {
					slot1 = hit1.collider.gameObject.GetComponent<Slot> ();   
				}
  
			}
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit2D hit = Physics2D.GetRayIntersection (ray, 1000, 1 << 8); // '8' is the "Slotslayer"
           
			if (hit) {
                
				slot2 = hit.collider.gameObject.GetComponent<Slot> ();
				if (temp) {
					slot1 = slot2;
					temp = false;
				}

				if (slot1 != slot2) {
					//Debug.Log("ss");
					prev = slot1;
					HeroTransformSwap (prev, slot2, false);
					temp = true;
				}
                
                
                
                
			}
            


		}
	}


	void CheckGrid (int PID)
	{
		int counter = 1;
		//ROW CHECK///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		for (int r = 0; r < Rows; r++) {
			counter = 1;
			for (int c = 1; c < Columns; c++) {
				//if (!slots[c, r])
				if (slots [c, r].PlayerID == PID)
				if (slots [c, r] != null && slots [c - 1, r] != null) {
					if (slots [c, r].hero.heroType == slots [c - 1, r].hero.heroType) {
						counter++;
                        
					} else {
						counter = 1;
					}

					if (counter == 6) {

						SetlockedOn (slots [c, r], 6);
						SetlockedOn (slots [c - 1, r], 6);
						SetlockedOn (slots [c - 2, r], 6);
						SetlockedOn (slots [c - 3, r], 6);
						SetlockedOn (slots [c - 4, r], 6);
						SetlockedOn (slots [c - 5, r], 6);

					} else if (counter == 5) {
						SetlockedOn (slots [c, r], 5);
						SetlockedOn (slots [c - 1, r], 5);
						SetlockedOn (slots [c - 2, r], 5);
						SetlockedOn (slots [c - 3, r], 5);
						SetlockedOn (slots [c - 4, r], 5);
					} else if (counter == 4) {
						SetlockedOn (slots [c, r], 4);
						SetlockedOn (slots [c - 1, r], 4);
						SetlockedOn (slots [c - 2, r], 4);
						SetlockedOn (slots [c - 3, r], 4);

						//slots[c, r] = null;
						// slots[c - 1, r] = null;
						//slots[c - 2, r] = null;
						//slots[c - 3, r] = null;  
					} else if (counter == 3) {
						SetlockedOn (slots [c, r], 3);
						SetlockedOn (slots [c - 1, r], 3);
						SetlockedOn (slots [c - 2, r], 3);


						// slots[c, r] = null;
						//slots[c - 1, r] = null;
						//slots[c - 2, r] = null;
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
					if (slots [c, r].hero.heroType == slots [c, r - 1].hero.heroType) {
						counter++;  
                            
					} else {
						counter = 1;
					}

					if (counter == 6) {

						SetlockedOn (slots [c, r], 6);
						SetlockedOn (slots [c, r - 1], 6);
						SetlockedOn (slots [c, r - 2], 6);
						SetlockedOn (slots [c, r - 3], 6);
						SetlockedOn (slots [c, r - 4], 6);
						SetlockedOn (slots [c, r - 5], 6);

					} else if (counter == 5) {
						SetlockedOn (slots [c, r], 5);
						SetlockedOn (slots [c, r - 1], 5);
						SetlockedOn (slots [c, r - 2], 5);
						SetlockedOn (slots [c, r - 3], 5);
						SetlockedOn (slots [c, r - 4], 5);
					} else if (counter == 4) {
						SetlockedOn (slots [c, r], 4);
						SetlockedOn (slots [c, r - 1], 4);
						SetlockedOn (slots [c, r - 2], 4);
						SetlockedOn (slots [c, r - 3], 4);


						//slots[c, r] = null;
						//slots[c, r - 1] = null;
						//slots[c, r - 2] = null;
						//slots[c, r - 3] = null;  
					} else if (counter == 3) {
						SetlockedOn (slots [c, r], 3);
						SetlockedOn (slots [c, r - 1], 3);
						SetlockedOn (slots [c, r - 2], 3);

						// slots[c, r] = null;
						// slots[c, r - 1] = null;
						// slots[c, r - 2] = null;
					}
                        

				}
			}

		}


	}

	void ConstantCheckGrid (int PID)
	{
		int counter = 1;
		//ROW CHECK///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		for (int r = 0; r < Rows; r++) {
			counter = 1;
			for (int c = 1; c < Columns; c++) {
				//if (!slots[c, r])
				if (slots [c, r].PlayerID == PID)
				if (slots [c, r] != null && slots [c - 1, r] != null) {
					if (slots [c, r].hero.heroType == slots [c - 1, r].hero.heroType) {
						counter++;

					} else {
						counter = 1;
					}

                        
					if (counter >= 3) {
						SetHeroesNull (slots [c, r]);
						SetHeroesNull (slots [c - 1, r]);
						SetHeroesNull (slots [c - 2, r]);

						RenewGrid ();
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
					if (slots [c, r].hero.heroType == slots [c, r - 1].hero.heroType) {
						counter++;

					} else {
						counter = 1;
					}

                        
					if (counter == 3) {
						SetHeroesNull (slots [c, r]);
						SetHeroesNull (slots [c, r - 1]);
						SetHeroesNull (slots [c, r - 2]);

						RenewGrid ();
						//renewBoard = true;
					}


				}
			}

		}

		if (renewBoard) {
			RenewGrid ();
			renewBoard = false;
		}

	}
	// NOT WORKINGGG

	void RenewGrid ()
	{
		Debug.Log ("check;");

		for (int r = 0; r < Rows; r++) {
			for (int c = 0; c < Columns; c++) {
				Vector3 slotPos = new Vector3 (c, r, 0);
				if (slots [c, r].hero == null) {
					for (int n = 0; n < heroPool.Count; n++) {
						GameObject o = heroPool [n];
						if (!o.activeSelf) {
							o.transform.position = new Vector3 (slotPos.x, slotPos.y, slotPos.z);
							o.SetActive (true);

							if (r < Rows / 2)
								slots [c, r].Fill (o, slotPos, 1);
							else
								slots [c, r].Fill (o, slotPos, 2);


							o.name = o.name + "(" + c + ", " + r + ")";

							n = heroPool.Count + 1;
						}
					}
				}
			}

		}
	}
	// NOT WORKINGGG


	void SetlockedOn (Slot s, int counter)
	{
		if (s.hero != null) {
			if (!s.lockedon) {
				s.LockedOnState (counter, false);
			} else {
				s.LockedOnState (counter, true);
			}
            
		} else {
			Debug.Log ("BoardCreationScript");
		}
	}

	void SetHeroesNull (Slot s)
	{
		if (s.hero != null) {
			if (s.hero != null) {
				s.hero.gameObject.SetActive (false);
			}

			s.hero = null; 

		}
        
	}


}
        