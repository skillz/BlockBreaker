using UnityEngine;
using System.Collections;


public class GameInitScript : MonoBehaviour
{
	void Start()
	{
		Skillz.skillzInitForGameIdAndEnvironment("1016", Skillz.SkillzEnvironment.SkillzSandbox);
		Application.LoadLevel("MainMenu");
	}
}