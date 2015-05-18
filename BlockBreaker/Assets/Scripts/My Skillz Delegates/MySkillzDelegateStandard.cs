using UnityEngine;
using System.Collections.Generic;

public class MySkillzDelegateStandard : SkillzSDK.SkillzDelegateStandard
{
	public override void OnTournamentWillBegin(Dictionary<string, string> gameRules)
	{
		//Parse the Skillz difficulty and go to the timed match scene.
		Application.LoadLevel("TimedGameScene");
		
		if (gameRules.ContainsKey("skillz_difficulty"))
		{
			MySkillzDelegate.SkillzDifficulty = System.Int32.Parse(gameRules["skillz_difficulty"]);
		}
		else
		{
			MySkillzDelegate.SkillzDifficulty = 5;
		}
		
	}
	
	public override void OnTournamentCompleted() { Application.LoadLevel("MainMenu"); }
}
