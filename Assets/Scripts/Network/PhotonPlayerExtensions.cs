using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;

/// <summary>
/// 	Extension used for PhotonPlayer class. Wraps access to the player's custom property.
/// </summary>
public static class PhotonPlayerExtensions
{

	public static int GetPlayersIndex(this PhotonPlayer player)
	{
		if (player.CustomProperties.ContainsKey(PlayerProperty.PLAYER_INDEX))
		{
			return (int)player.CustomProperties[PlayerProperty.PLAYER_INDEX];
		}

		return -1;
	}


	public static void SetPlayersIndex(this PhotonPlayer player)
	{
		if (!PhotonNetwork.connectedAndReady)
		{
			Debug.LogWarning("Not connected to the room....First connect to some room and then call this extension method");
			return;
		}

		List<int> tempcharacterIDList = new List<int>();

		// Get the Id of all the players in the room and then decide about the team ID after sorting the list of items.
		foreach(PhotonPlayer tempPlayer in PhotonNetwork.playerList)
		{
			tempcharacterIDList.Add(tempPlayer.ID);
		}

		tempcharacterIDList.Sort();

		PhotonNetwork.player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() {{PlayerProperty.PLAYER_INDEX, (int) tempcharacterIDList.IndexOf(player.ID)}});
		PhotonNetwork.SendOutgoingCommands ();
	}
		

	public static int[] GetPlayerBoardHeroes(this PhotonPlayer player)
	{
		if (player == null || player.customProperties == null || !player.customProperties.ContainsKey(PlayerProperty.PLAYER_BOARD_HEROES))
		{
			return null;
		}

		int[] newArray = ((string)player.customProperties[PlayerProperty.PLAYER_BOARD_HEROES]).Split(',').Select(Int32.Parse).ToArray();
		return	newArray;
	}


	public static void SetPlayerBoardHeroes(this PhotonPlayer player, int[] newArray)
	{
		if (player == null || player.customProperties == null)
		{
			return;
		}

		StringBuilder 				stringBuilder = new StringBuilder();

		stringBuilder.Remove(0, stringBuilder.Length);

		string newPlayerBoardHeroes = "";

		for(int i=0; i< newArray.Length; i++)
		{
			if(i > 0)
			{
				stringBuilder.Append(",");
				stringBuilder.Append(newArray[i]);
			}
			else
			{
				stringBuilder.Append(newArray[i]);
			}
		}

		newPlayerBoardHeroes = stringBuilder.ToString();

		ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable();
		playerProps[PlayerProperty.PLAYER_BOARD_HEROES] = newPlayerBoardHeroes;
		player.SetCustomProperties(playerProps);


		// Send all the buffered messages immediately to all the clients via server....
		PhotonNetwork.SendOutgoingCommands();
	}
}

