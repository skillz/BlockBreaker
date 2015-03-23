using System;
using System.Collections;
using UnityEngine;


public class GameplayController_ReviewTurn : GameplayController
{
    private static TurnBasedGameData GameData
    {
        get { return GameplayController_TurnBased.Skillz_GameData; }
        set { GameplayController_TurnBased.Skillz_GameData = value; }
    }
    private static string PlayerUniqueID { get { return GameplayController_TurnBased.PlayerUniqueID; } }


    public UnityEngine.UI.Text Player1Score, Player2Score,
                               TurnsLeftText;


    public void ReturnToSkillz()
    {
        Skillz.finishReviewingCurrentGameState();
    }

    void Start()
    {
        //Make sure the tournament data is valid.
        if (!Skillz.tournamentIsInProgress())
        {
            Debug.LogError("Not in a Skillz tournament!");
        }
        else if (GameData.Blocks == null)
        {
            Debug.LogError("No 'blocks' game data!");
            return;
        }

        //Set up the UI.
        Player1Score.text = GameData.GetScore(PlayerUniqueID).ToString();
        Player2Score.text = GameData.GetOtherScore(PlayerUniqueID).ToString();

        //Create the game blocks.
        GameGrid.Instance.ResetGrid(new Vector2i(GameData.Blocks.GetLength(0),
                                                 GameData.Blocks.GetLength(1)));
        for (int x = 0; x < GameData.Blocks.GetLength(0); ++x)
        {
            for (int y = 0; y < GameData.Blocks.GetLength(1); ++y)
            {
                if (GameData.Blocks[x, y] >= 0)
                {
                    CreateBlock(new Vector2(x, y), new Vector2i(x, y), GameData.Blocks[x, y]);
                }
            }
        }
    }

    protected override void Update()
    {
        LocalPlayer.IsInputDisabled = true;
        TurnsLeftText.text = GameData.TurnsLeft.ToString() + " left";
    }
}