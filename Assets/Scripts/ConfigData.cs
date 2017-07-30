using UnityEngine;
using System.Collections;

[System.Serializable]
public class ConfigData
{
	private static volatile ConfigData _instance;
	private static object lockingObject = new UnityEngine.Object();  
	public ConfigData() {}
	// Property to send the only instance of this class
	public static ConfigData Instance
	{
		get 
		{
			if (_instance == null) 
			{
				lock (lockingObject) 
				{
					// Only a single thread should enter this critical section area.
					if (_instance == null) 
						_instance = new ConfigData();
				}
			}
			return _instance;
		}
		
		set 
		{
			_instance = value;
		}
	}

	// Common variables

	public		    int 				multiplayerMaxPlayers 		= 2;
	public		    int 				matchMakingWaitTime 		= 5;


}
