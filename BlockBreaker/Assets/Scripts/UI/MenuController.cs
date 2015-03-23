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
		Skillz.launchTurnBasedSkillz(Skillz.SkillzOrientation.SkillzLandscape, true);
	}
}
