using UnityEngine;
using System.Collections;

/// <summary>
/// 	Extension methods for the room to fetch information which would need throughout the room for syncing purpose....
/// </summary>
public static class PhotonRoomExtensions
{

	public static void SetStartGame(this Room room)
	{
		if (room == null || room.CustomProperties == null)
		{
			return;
		}
		// Set the room property for the turn number and starting timer.....
		ExitGames.Client.Photon.Hashtable turnProps = new ExitGames.Client.Photon.Hashtable();

		turnProps [RoomProperty.GAMEPLAY_START_TIME] 		= PhotonNetwork.networkingPeer.ServerTimeInMilliSeconds; 
		turnProps[RoomProperty.TURN_START_TIME] 			= PhotonNetwork.networkingPeer.ServerTimeInMilliSeconds; 
		turnProps[RoomProperty.ROUND_NUMBER] 				= GetRoundNumber(room) + 1; 
		room.SetCustomProperties(turnProps);

		// Send all the buffered messages immediately to all the clients via server....
		PhotonNetwork.SendOutgoingCommands();

	}

	public static void SetChangeTurn(this Room room)
	{
		if (room == null || room.CustomProperties == null)
		{
			return;
		}
		// Set the room property for the turn number and starting timer.....
		ExitGames.Client.Photon.Hashtable turnProps = new ExitGames.Client.Photon.Hashtable();

		turnProps[RoomProperty.TURN_START_TIME] 			= PhotonNetwork.networkingPeer.ServerTimeInMilliSeconds; 
		turnProps[RoomProperty.ROUND_NUMBER] 				= GetRoundNumber(room)+1; 
		room.SetCustomProperties(turnProps);

		// Send all the buffered messages immediately to all the clients via server....
		PhotonNetwork.SendOutgoingCommands();

	}

	public static void SetGameOver(this Room room)
	{
		if (room == null || room.CustomProperties == null)
		{
			return;
		}
		// Set the room property for the turn number and starting timer.....
		ExitGames.Client.Photon.Hashtable turnProps = new ExitGames.Client.Photon.Hashtable();

		turnProps[RoomProperty.GAME_OVER_TIME] 				= PhotonNetwork.networkingPeer.ServerTimeInMilliSeconds; 
		room.SetCustomProperties(turnProps);

		// Send all the buffered messages immediately to all the clients via server....
		PhotonNetwork.SendOutgoingCommands();

	}
		
	public static int GetRoundNumber(this RoomInfo room)
	{
		if (room == null || room.CustomProperties == null || !room.CustomProperties.ContainsKey(RoomProperty.ROUND_NUMBER))
		{
			return 0;
		}

		return (int)room.CustomProperties[RoomProperty.ROUND_NUMBER];
	}


	public static int GetTurnStartTime(this RoomInfo room)
	{
		if (room == null || room.CustomProperties == null || !room.CustomProperties.ContainsKey(RoomProperty.TURN_START_TIME))
		{
			return 0;
		}

		return (int)room.CustomProperties[RoomProperty.TURN_START_TIME];
	}


	public static int GetGameStartedTime(this RoomInfo room)
	{
		if (room == null || room.CustomProperties == null || !room.CustomProperties.ContainsKey(RoomProperty.GAMEPLAY_START_TIME))
		{
			return 0;
		}

		return (int)room.CustomProperties[RoomProperty.GAMEPLAY_START_TIME];
	}
		
}