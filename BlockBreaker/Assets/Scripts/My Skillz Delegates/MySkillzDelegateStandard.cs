using UnityEngine;
using System.Collections.Generic;

public class MySkillzDelegateStandard : SkillzSDK.SkillzDelegateStandard
{
	public override void OnTournamentWillBegin(SkillzSDK.Match matchInfo)
	{
		Debug.Log(matchInfo);

		//Parse the Skillz difficulty and go to the timed match scene.
		Application.LoadLevel("TimedGameScene");
		
		Dictionary<string, string> gameRules = matchInfo.GameParams;
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
