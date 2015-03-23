using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SkillzHelper;

/// <summary>
/// The methods defined below in SkillzDelegate.cs must be implemented by the game developer.
///
/// These methods will be called by the Skillz API at certain points, as indicated in the comments below.
/// This script must be attached to a copy of the SkillzDelegate prefab, because these methods are called
/// by passing messages to the SkillzDelegate game object. Put a copy of that prefab in all the scenes
/// in your game to ensure these methods are always accesible when they may be needed.
/// </summary>
public class SkillzDelegate : MonoBehaviour
{
	public static int SkillzDifficulty { get; private set; }

    private static string PlayerUniqueID
    {
        get { return GameplayController_TurnBased.PlayerUniqueID; }
        set { GameplayController_TurnBased.PlayerUniqueID = value; }
    }
    private static TurnBasedGameData GameData
    {
        get { return GameplayController_TurnBased.Skillz_GameData; }
        set { GameplayController_TurnBased.Skillz_GameData = value; }
    }

    /// <summary>
    /// Modifies the game constants to reflect the given Skillz matchmaking difficulty.
    /// </summary>
    public static void SetUpSkillzDifficulty()
    {
        //Harder games should have more types of blocks.
		float difficultyLerp = (SkillzDifficulty - 1) / 10.0f;
        float nBlocksF = Mathf.Lerp(2.0f, GameConstants.Instance.BlockMaterials.Count, difficultyLerp);
        int nBlocks = Mathf.RoundToInt(nBlocksF);
        GameConstants.Instance.NBlockTypes = Mathf.Clamp(nBlocks, 2,
                                                         GameConstants.Instance.BlockMaterials.Count);
    }



    /// <summary>
    /// This block of code will be run immediately after launching Skillz from the multiplayer button.
    /// </summary>
    /// <param name="param">
    /// This parameter will always be an empty string and is here to meet the Unity method signature requirements.
    /// </param>
    public void skillzLaunchHasCompleted(string param)
    {
    }

    /// <summary>
    /// This block of code will be run when a player is entering a game from Skillz. It should load the level and take any
    /// other necessary initialization actions in order to start the game.
    /// </summary>
    /// <param name="gameRules">
    /// A string that contains all of the game rules that have been defined for the tournament the player
    /// is about to enter. These rules can be used to intialize different game types.
    /// </param>
    public void skillzTournamentWillBegin(string gameRules)
    {
        Application.LoadLevel("TimedGameScene");

        Dictionary<string, string> gameRulesDictionary = Helper.ParseGameRulesStringInToDictionary(gameRules);

        if (gameRulesDictionary.ContainsKey("skillz_difficulty"))
        {
			SkillzDifficulty = System.Int32.Parse(gameRulesDictionary["skillz_difficulty"]);
        }
        else
        {
			SkillzDifficulty = 5;
        }

    }

    /// <summary>
    /// This block of code is run when exiting Skillz back to the main application.
    /// Code here can be used to make sure the player lands back in the correct place in the main game application.
    /// </summary>
    /// <param name="param">
    /// This parameter will always be an empty string and is here to meet the Unity method signature requirements.
    /// </param>
    public void skillzWillExit(string param)
    {
    }

    /// <summary>
    /// This block of code will be run immediately before launching Skillz.
    /// </summary>
    /// <param name="param">
    /// This parameter will always be an empty string and is here to meet the Unity method signature requirements.
    /// </param>
    public void skillzWillLaunch(string param)
    {
    }

    /// <summary>
    /// This block of code is run when a player finishes a Skillz game and is reporting their score and exiting back to the Skillz portal.
    /// Code here can be used to reset/clean up the game that was just played.
    /// </summary>
    /// <param name="param">
    /// This parameter will always be an empty string and is here to meet the Unity method signature requirements.
    /// </param>
    public void skillzWithTournamentCompletion(string param)
    {
        Application.LoadLevel("MainMenu");
    }

    /// <summary>
    /// This code is called when a player quits a game early via Skillz.notifyPlayerAbortWithCompletion().
    /// Code here can be used to reset/clean up the game that was just played.
    /// </summary>
    /// <param name="param">
    /// This parameter will always be an empty string and is here to meet the Unity method signature requirements.
    /// </param>
    public void skillzWithPlayerAbort(string param)
    {
        Application.LoadLevel("MainMenu");
    }


    #region Turn-Based
    /// <summary>
    /// This code is called when a player is beginning a turn-based tournament. It should load the level and do any necessary init
    /// </summary>
    /// <param name="turnBasedMatchInfo">
    /// The current turn based game state as a JSON string.
    /// </param>
    public void skillzTurnBasedTournamentWillBegin(string turnBasedMatchInfo)
    {
        GameplayController_TurnBased.Skillz_tookTurn = false;
        Application.LoadLevel("TurnBasedGameScene");

        Dictionary<string, object> turnBasedMatchInfoDictionary = Helper.DeserializeJSONToDictionary(turnBasedMatchInfo);

        PlayerUniqueID = turnBasedMatchInfoDictionary["playerUniqueId"].ToString();

        if (turnBasedMatchInfoDictionary.ContainsKey("skillz_difficulty"))
        {
            SkillzDifficulty = System.Int32.Parse(turnBasedMatchInfoDictionary["skillz_difficulty"].ToString());
        }
        else
        {
            SkillzDifficulty = 5;
        }

        //If the tournament is a continuation of an ongoing match, load the tournament data into the "GameData" instance.
        GameData = new TurnBasedGameData();
        if (turnBasedMatchInfoDictionary.ContainsKey("gameData"))
        {
            if (!GameData.ParseFrom(turnBasedMatchInfoDictionary["gameData"].ToString()))
            {
                Debug.LogError("Couldn't parse game data: '" + turnBasedMatchInfoDictionary["gameData"].ToString());
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

    /// <summary>
    /// This code is called when a player is completing their turn within a turn-based tournament
    /// It will take them back into the Skillz UI
    /// </summary>
    /// <param name="param">
    /// This parameter will always be an empty string and is here to meet the Unity method signature requirements.
    /// </param>
    public void skillzEndTurnCompletion(string param)
    {
        Application.LoadLevel("MainMenu");
    }

    /// <summary>
    /// This code is called in order to load an uninteractive game state, 
    /// allowing the player to review the game state, but not allowing them to
    /// complete a turn or manipulate a game state.
    /// </summary>
    /// <param name="turnBasedMatchInfo">
    /// The current turn based game state as a JSON string.
    /// </param>
    public void skillzReviewCurrentGameState(string turnBasedMatchInfo)
    {
        Dictionary<string, object> turnBasedMatchInfoDictionary = Helper.DeserializeJSONToDictionary(turnBasedMatchInfo);
        
        //Read in the player ID and the game data.

        PlayerUniqueID = turnBasedMatchInfoDictionary["playerUniqueId"].ToString();

        GameplayController_TurnBased.Skillz_GameData = new TurnBasedGameData();
        if (!GameData.ParseFrom(turnBasedMatchInfoDictionary["gameData"].ToString()))
        {
            Debug.LogError("Couldn't parse game data: '" + turnBasedMatchInfoDictionary["gameData"].ToString());
            return;
        }

        Application.LoadLevel("ReviewGameScene");
    }

    /// <summary>
    /// This code is called when your user has finished reviewing the current game state, use this method to return to the Skillz UI.
    /// </summary>
    /// <param name="param">
    /// This parameter will always be an empty string and is here to meet the Unity method signature requirements.
    /// </param>
    public void skillzFinishReviewingCurrentGameState(string param)
    {
        Application.LoadLevel("MainMenu");
    }
    #endregion

    [System.Obsolete("use SkillzHelper.Helper.DeserializeJSONToDictionary")]
    private Dictionary<string, object> deserializeJSONToDictionary(string jsonString)
    {
        return Helper.DeserializeJSONToDictionary(jsonString);
    }

    [System.Obsolete("use SkillzHelper.Helper.parseGameRulesStringInToDictionary")]
    private Dictionary<string, string> parseGameRulesStringInToDictionary(string gameRules)
    {
        return Helper.ParseGameRulesStringInToDictionary(gameRules);
    }
}
