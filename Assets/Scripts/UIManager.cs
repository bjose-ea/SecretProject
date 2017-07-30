using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour {

	private static UIManager instance = null;

	private NetworkManager networkManager;
	public GameObject goMainMenu;
	public GameObject goMatchMakingPanel;
	public GameObject goWaitingPanel;

	void Awake()
	{
		instance = this;
	}

	// Method to send the only instance of this class
	public static UIManager GetInstance()
	{
		return instance;
	}

	void Start()
	{
		networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
	}

	public void Enter()
	{
		if(!PhotonNetwork.connected)
		{
			Debug.Log ("Error  NoInternetConnection");
			return;
		}

		if(PhotonNetwork.networkingPeer.State != ClientState.ConnectedToMaster)
		{
			// If not connected to the master server, then come back as it would only join the room if not connected 
			// to any other room.
			PhotonNetwork.LeaveRoom();
			Debug.Log ("Error  ClientState not connected leaving room");
		}
		if (PhotonNetwork.connected && PhotonNetwork.networkingPeer.State == ClientState.ConnectedToMaster) 
		{
			networkManager.JoinRandomMatchRoom ();
		}

		goMainMenu.SetActive(false);
		goMatchMakingPanel.SetActive(true);

	}

}
