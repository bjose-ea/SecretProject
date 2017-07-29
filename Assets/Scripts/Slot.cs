using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slot : MonoBehaviour
{
	public bool isFilled = false;
	public GameObject hero;
	public Vector3 position;
	public int PlayerID = 1;
	public string herotype;
	public bool lockedon = false;
	public int MatchCount;
	public Animator Matchfeedanim;
	[HideInInspector]
	public BoardManager BM;
	public Color defaultPlayer1Color;
	public Color defaultPlayer2Color;
	public bool CanInteractible = true;

	[HideInInspector]
	public int c;
	[HideInInspector]
	public int r;


	GameObject h;
	Slot destination;
	[HideInInspector]
	public float driftseconds = 0.2f;

	[HideInInspector]
	public bool isDrifting;

	public void Update ()
	{
		if (!CanInteractible) {
			Col (Color.cyan);
		} else {
			if (PlayerID == 1)
				Col (defaultPlayer1Color);
			else
				Col (defaultPlayer2Color);
		}
				


	}

	public void Start ()
	{
		BM = GameObject.FindGameObjectWithTag ("BoardManager").GetComponent<BoardManager> ();
	}

	public void Fill (GameObject h, Vector3 pos, int playerID)
	{  
		//Debug.Log ("hhh");
		if (!isFilled)
			isFilled = true;

		hero = h;
		position = pos;
		SetPlayerID (playerID);
		CanInteractible = true;
		if (hero)
		if (hero.gameObject.GetComponent<Heroes> ()) {
			herotype = hero.gameObject.GetComponent<Heroes> ().type;
			hero.gameObject.GetComponent<Heroes> ().itslot = this;
		}
            
	}

	public void UnFill ()
	{
		isFilled = false;
		hero = null;
		herotype = null; 
	}

	public void MoveTo (Slot s, float seconds, bool wantDataTransfer, bool Return)
	{
		//Debug.Log("Swapp"+hero);

		h = hero;
		destination = s;
		driftseconds = seconds;
		if (Return)
			wantDataTransfer = false;
		StartDrift (wantDataTransfer, Return);


	}

	public void heroMoveTo (GameObject hero, float seconds, bool wantDataTransfer, bool Return)
	{
		//Debug.Log("Swapp"+hero);
		h = hero;
		destination = this;
		driftseconds = seconds;
		StartDrift (wantDataTransfer, Return);
	}

	int count = 0;

	private void StartDrift (bool wantDataTransfer, bool Return)
	{

		StartCoroutine (MoveOverSeconds (h.gameObject, destination.gameObject.transform.position, driftseconds, wantDataTransfer, Return));
		CanInteractible = false;
		//BM.canInput = false;
	}


	public IEnumerator MoveOverSpeed (GameObject objectToMove, Vector3 end, float speed, bool wantDataTransfer, bool WanaReturn)
	{
		// speed should be 1 unit per second
		while (objectToMove.transform.position != end) {
			objectToMove.transform.position = Vector3.MoveTowards (objectToMove.transform.position, end, speed * Time.deltaTime);
			yield return new WaitForEndOfFrame ();
		}

		EndAction (wantDataTransfer, WanaReturn);
	}

	public IEnumerator MoveOverSeconds (GameObject objectToMove, Vector3 end, float seconds, bool wantDataTransfer, bool WanaReturn)
	{
		float elapsedTime = 0;
		Vector3 startingPos = transform.position;
		while (elapsedTime < seconds) {
			//Debug.Log (objectToMove);
			objectToMove.transform.position = Vector3.Lerp (startingPos, end, (elapsedTime / seconds));
			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame ();
		}
		objectToMove.transform.position = end;

		EndAction (wantDataTransfer, WanaReturn);
	}

	public void Col (Color c)
	{
		this.gameObject.GetComponent<SpriteRenderer> ().color = c;
	}


	public void heromovingto (GameObject objectToMove, Slot startslot, float seconds, bool wantDataTransfer)
	{
		StartCoroutine (HeroMoveOverSeconds (objectToMove, startslot, position, seconds, wantDataTransfer));
	}

	public IEnumerator HeroMoveOverSeconds (GameObject objectToMove, Slot startslot, Vector3 end, float seconds, bool wantDataTransfer)
	{

		CanInteractible = false;
		startslot.CanInteractible = false;
		float elapsedTime = 0;
		Vector3 startingPos = objectToMove.transform.position;
		while (elapsedTime < seconds) {
			//Debug.Log (objectToMove);
			objectToMove.transform.position = Vector3.Lerp (startingPos, end, (elapsedTime / seconds));
			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame ();
		}
		objectToMove.transform.position = end;

		heroMoveEndAction (objectToMove, startslot, wantDataTransfer);
	}


	void heroMoveEndAction (GameObject hero, Slot startslot, bool wantDataTransfer)
	{
		CanInteractible = true;
		startslot.CanInteractible = true;
		if (wantDataTransfer)
			Fill (hero, position, PlayerID);
	}

	void EndAction (bool wantDataTransfer, bool WanaReturn)
	{
		if (wantDataTransfer)
			destination.Fill (h, destination.position, destination.PlayerID);
		if (count == 0) {
			if (WanaReturn) {
				count++;
				StartDrift (false, false);
			}

		} else {
			
			h = null;
			destination = null;
			count = 0;
		}
		//BM.canInput = true;
		CanInteractible = true;

	}

	public void SetPlayerID (int i)
	{
		if (i == 1) {
			if (this.gameObject.GetComponent<SpriteRenderer> ())
				this.gameObject.GetComponent<SpriteRenderer> ().color = defaultPlayer1Color;

			PlayerID = 1;
		} else if (i == 2) {
			if (this.gameObject.GetComponent<SpriteRenderer> ())
				this.gameObject.GetComponent<SpriteRenderer> ().color = defaultPlayer2Color;

			PlayerID = 2;
		} else {
			Debug.Log ("Slot: i got Wrong Player ID");
		}
	}



	public void LockedOnState (int counter, bool AlreadylockedOn)
	{
		lockedon = true;
		if (!AlreadylockedOn) {
			// if (this.gameObject.GetComponent<SpriteRenderer>())
			//this.gameObject.GetComponent<SpriteRenderer>().color = Color.red;//new Color(Random.Range(0f,1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1);
			if (!Matchfeedanim.gameObject.activeSelf) {
				Matchfeedanim.gameObject.SetActive (true);
				Matchfeedanim.Play ("MatchFeed_StartAnim");
			}
			MatchCount = counter;
		} else {
			MatchCount = counter;
			if (!Matchfeedanim.gameObject.activeSelf) {
				Matchfeedanim.gameObject.SetActive (true);
				Matchfeedanim.Play ("MatchFeed_StartAnim");
			}
  
		}
       


	}
}
