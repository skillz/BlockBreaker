using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour
{
	public void OnTimedButton()
	{
		Application.LoadLevel("TimedGameScene");
	}

	public void OnTurnBasedButton()
	{
		Application.LoadLevel("TurnBasedGameScene");
	}

	public void OnMultiplayerButton()
	{
		SkillzSDK.Api.LaunchSkillz(SkillzSDK.Orientation.Landscape);
	}
}
