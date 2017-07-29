using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heroes : MonoBehaviour
{
	public string type;
	public Slot itslot;
	public Slot destinationslot;
	//public bool
	public float Health = 100;
	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.W)) {
			GotMatch (3);

		}



	}

	public void GotMatch (int counter)
	{

		if (counter == 3) {
			match3Action ();
		} else if (counter == 4) {
			match4Action ();
		} else if (counter == 5) {
			//Action 3
		} else if (counter == 6) {
			//Action 4
		}
	}

	void match3Action ()
	{
		if (type == "yellow") {
			
		}
		Debug.Log ("Doaction");
	}

	void match4Action ()
	{
		if (type == "yellow") {

		}

	}

	void decreaseHealth (float damage)
	{
		
	}
}
