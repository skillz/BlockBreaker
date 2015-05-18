using UnityEngine;
using System.Collections;


public class GameInitScript : MonoBehaviour
{
	//Once all initialization stuff has had time to run, start the main menu
	void Update()
	{
		Application.LoadLevel("MainMenu");
		Destroy(this);
	}
}