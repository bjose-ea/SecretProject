using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using System;
using System.Text;



public class NetworkManager : Photon.MonoBehaviour 
{
	public		int			    			roomCreatedTime,
											gamePlayStartTime,		 			// when the game play was started, time shoud be synced
											roundNum,
											roundStartTime	;				// when a particular round starts...

	private		bool						isConnectedToPhotonMasterServer;

	public		Text 						pingText;


	#region Events For Networking
	public static event System.Action 				OnRoomJoinedEvent; 					// When Player is waiting for the other players to join....He has JUST Joined the room.
	public static event System.Action<int, int>  	OnPlayersConnectedToRoomEvent;	 	// When connected to room, this event notifies the total no. of players in that room every second.
	public static event System.Action			OnRoomFullEvent;					// When room is full or closed due to time up...

	public static event System.Action				OnRoundStartedEvent;				// Whenever round is started....
	public static event System.Action				OnGameOverEvent;					// Whenever game is over...i.e. somebody reached the highest score....

	#endregion


	void OnGUI()
	{
		if (GUI.Button (new Rect (0, 0, 200, 50), ("MASTER CLIENT:" + PhotonNetwork.isMasterClient)))
		{
		}

	}



	public static NetworkManager instance;


	void Awake()
	{
		if(instance == null)
		{
			instance = this;
		}
	}


	void Start () 
	{
		PhotonNetwork.autoJoinLobby = false;    // We join randomly. Always. No need to join a lobby to get the list of rooms.
		PhotonNetwork.logLevel 		= PhotonLogLevel.Full;
		// Reseting static fields .....
		gamePlayStartTime			= 0;
		roundNum					= 0;
		// Check for network connection every second, it its lost, then try to connect again...
		StartCoroutine(CheckForNetworkConnection());
		StartCoroutine(PingInfoUpdate());
	}


	void OnEnable()
	{
		isGameStarted 	= false;
	}


	void OnDisable()
	{
		StopAllCoroutines();
	}


	IEnumerator CheckForNetworkConnection()
	{
		while(true)
		{
			// Having it in update to ensure if due to some issue Photon disconnects, then it while try to connect again.
			if (!PhotonNetwork.connected)
			{
				// Connect to Photon Server as configured in the editor. This parameter will prevent other players who are using different 
				// app version from connecting to the players of this app version.
				PhotonNetwork.ConnectUsingSettings("1.0");
			}
			else
			{
				// To showcase the the total no. of players connected to Photon cloud for this application..
//				totalNumOfPlayers = PhotonNetwork.countOfPlayers.ToString();
			}
			//TODO Enable below line to test in offline mode and comment out the above if-else condition.
			//	PhotonNetwork.offlineMode = true;

			yield return new WaitForSeconds(1f);
		}

	}


	IEnumerator PingInfoUpdate()
	{
		while (true) 
		{
			if (PhotonNetwork.connected)
			{
				pingText.text = "Latency - " + PhotonNetwork.GetPing () + " ms - " + PhotonNetwork.networkingPeer.State.ToString ();
			} 
			else 
			{
				pingText.text = PhotonNetwork.networkingPeer.State.ToString ();
			}
			yield return new WaitForSeconds (1f);
		}
	}


	public void OverrideBestCloudServer(CloudRegionCode region)	
	{
		Debug.Log ("Region : " + region);
	}


	public virtual void OnConnectedToMaster()
	{
		if (PhotonNetwork.networkingPeer.AvailableRegions != null) 
			Debug.LogWarning("List of available regions counts " + PhotonNetwork.networkingPeer.AvailableRegions.Count + ". First: " + PhotonNetwork.networkingPeer.AvailableRegions[0] + " \t Current Region: " + PhotonNetwork.networkingPeer.CloudRegion);

		Debug.Log("OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room. " + PhotonNetwork.networkingPeer.State.ToString());

		isConnectedToPhotonMasterServer = true;
	}


	public void JoinRandomMatchRoom()
	{
		if(!PhotonNetwork.connected)
		{
			Debug.LogError("No Connectivity");
			return;
		}

		if(PhotonNetwork.networkingPeer.State != ClientState.ConnectedToMaster)
		{
			// If not connected to the master server, then come back as it would only join the room if not connected 
			// to any other room.
			PhotonNetwork.LeaveRoom();
			Debug.Log ("Leaving Room");

		}
		StartCoroutine (ConnectToARoom ());

	}

	IEnumerator ConnectToARoom()
	{
		while (PhotonNetwork.networkingPeer.State != ClientState.ConnectedToMaster) // PEERSTATE not working
		{
			yield return null;
		}
		PhotonNetwork.JoinRandomRoom(null, (byte)ConfigData.Instance.multiplayerMaxPlayers, MatchmakingMode.RandomMatching, null, null);
		PhotonNetwork.autoCleanUpPlayerObjects = false;
	}

	/// <summary>
	/// 	Callback raised when joining random room failed due to unavailability of any rooms according to the filter provided.
	/// </summary>
	void OnPhotonRandomJoinFailed()
	{
		Debug.Log("OnPhotonRandomJoinFailed");

		RoomOptions roomOptions = new RoomOptions();
		roomOptions.MaxPlayers  = (byte)ConfigData.Instance.multiplayerMaxPlayers;

//		roomOptions.CustomRoomProperties = null;//new ExitGames.Client.Photon.Hashtable() { { "C2", currentGameTargetScore } };
//		roomOptions.CustomRoomPropertiesForLobby = new string[] { "C2" }; 
		PhotonNetwork.CreateRoom("" , roomOptions, null);
	}


	/// <summary>
	/// 	Callback raised when creating room failed due to same existing room on the Photon server.
	/// </summary>
	void OnPhotonCreateGameFailed()
	{
		
	}



	void  OnCreatedRoom()
	{

	}


	public virtual void OnJoinedRoom()
	{
		if(PhotonNetwork.player.ID == 1)
		{
			// If first player to create the room, then set the room created custom property...
			StartCoroutine(SetRoomCreatedProperty());
		}

		// Reseting variables for last room values if any... 
		isConnectedToPhotonMasterServer = false;	// Reset it to false, as we should not try to join a room if already connected to other room.

		try
		{
			ExitGames.Client.Photon.Hashtable customPlayerProperties = new ExitGames.Client.Photon.Hashtable();

			PhotonNetwork.player.SetCustomProperties(customPlayerProperties);
		}
		catch(Exception e)
		{}

		PhotonNetwork.SetMasterClient(PhotonNetwork.player);
		PhotonNetwork.player.SetPlayersIndex ();
		// Wait for another 1 min since the Room was created...
		StartCoroutine("LookForOtherPlayersInRoom");  
	}


	/// <summary>
	/// 	Sets the room creation time property.  Sets the Custom Property of the room by the very first person to create the room.
	/// </summary>
	IEnumerator SetRoomCreatedProperty()
	{
		// Wait till ServerTime is O, It generally happens when users have just joined a room, it takes a bit of time to return the correct value.
		while(PhotonNetwork.networkingPeer.ServerTimeInMilliSeconds == 0)
			yield return null;

		// If he is the first player to join the room then, Set the Room property which will be synced to all the players.
		roomCreatedTime = PhotonNetwork.networkingPeer.ServerTimeInMilliSeconds;

		ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable(); 
		customRoomProperties[RoomProperty.ROOM_CREATED_TIME] =  roomCreatedTime;
		PhotonNetwork.room.SetCustomProperties(customRoomProperties);

		PhotonNetwork.SendOutgoingCommands();
	}



	/// <summary>
	/// 	Waits for other players to join the room for the Waiting Timer...
	/// </summary>
	IEnumerator LookForOtherPlayersInRoom()
	{
		yield return new WaitForSeconds(1);

		object roomCreatedTime = null;

		// Wait till room created time is O, Sometimes, it takes a bit of time to update the correct value just after joining a room.
		while (roomCreatedTime == null || (int)roomCreatedTime == 0) 
		{
			PhotonNetwork.room.CustomProperties.TryGetValue(RoomProperty.ROOM_CREATED_TIME, out roomCreatedTime);
			yield return null;
		}

		// Notify other classes that room has been joined....
		if(OnRoomJoinedEvent != null)
			OnRoomJoinedEvent();

		while(PhotonNetwork.networkingPeer.ServerTimeInMilliSeconds - (int)roomCreatedTime < ConfigData.Instance.matchMakingWaitTime * 1000
			&&  PhotonNetwork.playerList.Length != ConfigData.Instance.multiplayerMaxPlayers)
		{

			if(PhotonNetwork.isMasterClient && PhotonNetwork.networkingPeer.ServerTimeInMilliSeconds - (int)roomCreatedTime >= 8*1000)
			{
				PhotonNetwork.room.IsVisible = false;
				PhotonNetwork.room.IsOpen    = false;
				Debug.Log("PhotonNetwork.room.IsVisible>>" + PhotonNetwork.room.IsVisible);
			}

			// Notify about the no. of players connected to the room currently...It updates every 1 second...
			if(OnPlayersConnectedToRoomEvent != null)
			{
				OnPlayersConnectedToRoomEvent(PhotonNetwork.playerList.Length,  ConfigData.Instance.matchMakingWaitTime - (int)Math.Round((double)(PhotonNetwork.networkingPeer.ServerTimeInMilliSeconds - (int)roomCreatedTime )/1000));
			}
			// Will raise the event every second.
			yield return new WaitForSeconds(1f);
		}

		if(PhotonNetwork.isMasterClient)
		{
			// Set the room visibility as false so that others can't join this room and neither they can see this room...
			PhotonNetwork.room.IsVisible = false;
			PhotonNetwork.room.IsOpen    = false;
			GameObject goGameController = GameObject.FindGameObjectWithTag ("BoardManager");

			if (goGameController == null) 
			{
				goGameController = PhotonNetwork.Instantiate ("BoardManager", Vector3.zero, Quaternion.identity, 0, null) as GameObject;
			}

		}
		UIManager.GetInstance().goMatchMakingPanel.SetActive(false);
		UIManager.GetInstance().goWaitingPanel.SetActive(true);

		StartCoroutine (WaitForBoardManagerToAppear ());

	}
		

	IEnumerator WaitForBoardManagerToAppear()
	{
		GameObject goGameController = null;

		while (goGameController == null) 
		{
			goGameController = GameObject.FindGameObjectWithTag ("BoardManager");
			yield return new WaitForSeconds(0.1f);			
		}

		if(OnRoomFullEvent != null)
			OnRoomFullEvent();
	}


	public void DisconnectRoom()
	{
		PhotonNetwork.LeaveRoom();
		StopCoroutine("LookForOtherPlayersInRoom");
	}



	/// <summary>
	/// 	Callback raised when disconnected from the Photon server........
	/// 	In some cases, other callbacks are called before OnDisconnectedFromPhoton() is called
	/// </summary>
	/// <param name="cause">Cause.</param>
	public virtual void OnFailedToConnectToPhoton(DisconnectCause cause)
	{
		Debug.Log("Cause: " + cause);
	}


	/// <summary>
	/// 	Callback by Photon when a player leaves the room....
	/// </summary>
	public virtual void OnLeftRoom()
	{
		// Reset the previous values.
		roomCreatedTime 			= 0;
		gamePlayStartTime			= 0;
		roundNum 					= 0;

		// Stops the running coroutine....
		StopCoroutine("LookForOtherPlayersInRoom");

//		foreach(GameObject player in GameObject.FindGameObjectsWithTag(Tag.CHARACTER)) 
//		{
//			// Destroy all the players locally instantiated due to the remote clients.....
//			Destroy(player);
//		}
		//TODO
		// Destroy the player's object from all the clients.....
		PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.player);	//not destroying the network instantiated players so that it can be converted to ai players
	}

	public static bool isGameStarted;


	/// <summary>
	/// 	Called by PUN when new properties for the room were set (by any client in the room).
	/// 	Mostly set by the master client to prevent multiple callbacks being sent to all clients.
	/// </summary>
	public void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
	{
		if(propertiesThatChanged.ContainsKey(RoomProperty.ROOM_CREATED_TIME))
		{
			// Custom Room property changed when room was first created...
			roomCreatedTime = (int) propertiesThatChanged[RoomProperty.ROOM_CREATED_TIME];
		}
		else if(propertiesThatChanged.ContainsKey(RoomProperty.GAMEPLAY_START_TIME))
		{
			// Gameplay start property reached to the client which should enable the main game play UI.....
			gamePlayStartTime = (int) propertiesThatChanged[RoomProperty.GAMEPLAY_START_TIME];

		}
		else if(propertiesThatChanged.ContainsKey(RoomProperty.ROUND_NUMBER))
		{
			// Sets the round number locally...
			roundNum = (int) propertiesThatChanged[RoomProperty.ROUND_NUMBER];
//
			if(OnRoundStartedEvent != null)
				OnRoundStartedEvent();
//

		}

		else if(propertiesThatChanged.ContainsKey(RoomProperty.GAME_OVER_TIME) && PhotonNetwork.room != null)
		{
			if (OnGameOverEvent != null)
				OnGameOverEvent ();
		}
			
	}
		
}