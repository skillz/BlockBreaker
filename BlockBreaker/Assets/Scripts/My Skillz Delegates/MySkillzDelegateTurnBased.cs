using UnityEngine;
using System.Collections.Generic;


/// <summary>
/// Implements Skillz callbacks for turn-based tournaments.
/// </summary>
public class MySkillzDelegateTurnBased : SkillzSDK.SkillzDelegateTurnBased
{
	//Store some Skillz-related data statically so it persists between scenes.

	public static SkillzSDK.TurnBasedMatch TBMInfo { get; set; }
	
	public static string PlayerUniqueID
	{
		get { return GameplayController_TurnBased.PlayerUniqueID; }
		set { GameplayController_TurnBased.PlayerUniqueID = value; }
	}
	public static TurnBasedGameData GameData
	{
		get { return GameplayController_TurnBased.Skillz_GameData; }
		set { GameplayController_TurnBased.Skillz_GameData = value; }
	}


	public override void OnTurnBasedTournamentWillBegin(SkillzSDK.TurnBasedMatch matchInfo)
	{
		GameplayController_TurnBased.Skillz_tookTurn = false;
		Application.LoadLevel("TurnBasedGameScene");
		
		TBMInfo = matchInfo;
		
		PlayerUniqueID = matchInfo.PlayerID.ToString();
		if (TBMInfo.SkillzDifficulty.HasValue)
		{
			MySkillzDelegate.SkillzDifficulty = (int)TBMInfo.SkillzDifficulty.Value;
		}
		else
		{
			MySkillzDelegate.SkillzDifficulty = 5;
		}
		
		//If the tournament is a continuation of an ongoing match, load the tournament data into the "GameData" instance.
		GameData = new TurnBasedGameData();
		if (TBMInfo.ContinueMatchData.HasValue)
		{
			if (!GameData.ParseFrom(TBMInfo.ContinueMatchData.Value.GameData))
			{
				Debug.LogError("Couldn't parse game data: '" + TBMInfo.ContinueMatchData.Value.GameData);
				return;
			}
		}
		//Otherwise, set the GameData instance to an empty value, with the current player name.
		else
		{
			GameData.Blocks = null;
			GameData.KnownPlayerIDScore = 0;
			GameData.UnknownPlayerIDScore = 0;
			
			GameData.KnownPlayerID = PlayerUniqueID;
		}
		
	}
	public override void OnTurnEnd() { Application.LoadLevel("MainMenu"); }
	
	public override void OnTurnBasedReviewWillBegin(SkillzSDK.TurnBasedMatch matchInfo)
	{
		TBMInfo = matchInfo;
		
		//Read in the player ID and the game data.
		
		PlayerUniqueID = TBMInfo.PlayerID.ToString();
		
		GameplayController_TurnBased.Skillz_GameData = new TurnBasedGameData();
		if (!GameData.ParseFrom(matchInfo.ContinueMatchData.Value.GameData))
		{
			Debug.LogError("Couldn't parse game data: '" + matchInfo.ContinueMatchData.Value.GameData);
			return;
		}
		
		Application.LoadLevel("ReviewGameScene");
	}
	public override void OnReviewEnd() { Application.LoadLevel("MainMenu"); }
}
