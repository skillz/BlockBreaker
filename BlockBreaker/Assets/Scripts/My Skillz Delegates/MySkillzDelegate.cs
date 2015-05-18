using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Implements the basic Skillz callbacks.
/// </summary>
public class MySkillzDelegate : SkillzSDK.SkillzDelegateBase
{
	//Track Skillz data statically so it persists between scenes.
	public static int SkillzDifficulty { get; set; }
	
	
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

	//Bring the player back to the main menu when returning to the Skillz UI.
	public override void OnTournamentAborted() { Application.LoadLevel("MainMenu"); }
}
