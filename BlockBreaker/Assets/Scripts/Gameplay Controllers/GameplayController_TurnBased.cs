using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameplayController_TurnBased : GameplayController
{
	/// <summary>
	/// Game state loaded in from Skillz in the event of an online turn-based tournament.
	/// </summary>
	public static TurnBasedGameData Skillz_GameData = new TurnBasedGameData();
    /// <summary>
    /// The Skillz ID of this player.
    /// </summary>
    public static string PlayerUniqueID;
	/// <summary>
	/// Whether the player has taken a turn yet.
    /// Used in Skillz turn-based tournaments.
	/// </summary>
    public static bool Skillz_tookTurn;


	//Player scores are stored statically so they can be accessed across several scenes.
	public static int PlayerOneScore, PlayerTwoScore;


	/// <summary>
	/// There are two players, and it's either player 1's turn or player 2's turn.
	/// </summary>
	[NonSerialized]
	public bool IsPlayer1Turn = true;


	public UnityEngine.UI.Text CurrentPlayerUIText,
							   TurnsLeftUIText;
	public GameObject AbandonButton;

	/// <summary>
	/// The minimum amount of time between moves.
	/// </summary>
	public float TurnWaitTime = 2.0f;

	/// <summary>
	/// The number of turns remaining until the game ends.
	/// </summary>
	public int TurnsLeft = 20;

	/// <summary>
	/// The amount of time between a block being destroyed and a replacement being spawned.
	/// </summary>
	public float BlockSpawnWaitTime = 0.6f;
	
	/// <summary>
	/// The score gained from clearing a single block.
	/// Larger values are preferable because they mean more precision
	///     in the exponential score gain from clearing extra blocks.
	/// </summary>
	public int BaseBlockScoreValue = 100;
	/// <summary>
	/// The exponent determining how much better the player's score is
	/// when he clears a few large chunks compared to a lot of small chunks.
	/// Should always be greater than (or equal to) 1.0.
	/// </summary>
	public float ScoreBlockExponent = 2.0f;

	
	public void Abandon()
	{
		Application.LoadLevel("MainMenu");
		if (Skillz.tournamentIsInProgress())
			Skillz.notifyPlayerAbortWithCompletion();
	}

	protected override void Awake()
	{
		base.Awake();

		PlayerOneScore = 0;
		PlayerTwoScore = 0;

		//When the blocks are cleared, change turns.
		OnBlocksClearedChanged += (clearedBlocks, reasonForClearing) =>
		{
			//Pause the player's input.
			StartCoroutine(PauseInputCoroutine(TurnWaitTime));

			//Update score.
			float blockMultiplier = Mathf.Pow (clearedBlocks.Count, ScoreBlockExponent);
			if (IsPlayer1Turn)
			{
				PlayerOneScore += Mathf.RoundToInt(BaseBlockScoreValue * blockMultiplier);
			}
			else
			{
				PlayerTwoScore += Mathf.RoundToInt(BaseBlockScoreValue * blockMultiplier);
			}

			//Drop extra blocks in for every block that was cleared.
			StartCoroutine(WaitSpawnCoroutine(clearedBlocks));

			//Change turns.
			IsPlayer1Turn = !IsPlayer1Turn;
			TurnsLeft -= 1;
			Skillz_tookTurn = true;
		};
	}
	void Start()
	{
        Skillz_tookTurn = false;

        GameGridGenerator gen = FindObjectOfType<GameGridGenerator>();

        //If we're in a Skillz turn-based tournament, some special logic for block generation is needed.
        if (Skillz.tournamentIsInProgress())
        {
            CurrentPlayerUIText.text = "Your turn";

            //If starting a new tournament, randomize the block layout.
            if (Skillz_GameData.Blocks == null)
            {
                gen.Seed = Skillz.getRandomNumber();
            }
            //Otherwise, just set up the generator to create the right number of blocks.
            else
            {
                TurnsLeft = Skillz_GameData.TurnsLeft;

                gen.NBlocksX = Skillz_GameData.Blocks.GetLength(0);
                gen.NBlocksY = Skillz_GameData.Blocks.GetLength(1);

                IsPlayer1Turn = true;
            }
        }

        gen.GenerateBlocks(true);

        //Now that the blocks have been generated, fill them in with the correct colors
        //    if we're continuing a Skillz tournament.
        if (Skillz.tournamentIsInProgress() && Skillz_GameData.Blocks != null)
        {
            for (int x = 0; x < gen.NBlocksX; ++x)
            {
                for (int y = 0; y < gen.NBlocksY; ++y)
                {
                    Vector2i loc = new Vector2i(x, y);

                    if (GameGrid.Instance.GetBlock(new Vector2i(x, y)) != null)
                    {
                        if (Skillz_GameData.Blocks[x, y] >= 0)
                        {
                            GameGrid.Instance.GetBlock(loc).ColorID = Skillz_GameData.Blocks[x, y];
                        }
                        else
                        {
                            GameGrid.Instance.DestroyBlock(loc);
                        }
                    }
                }
            }

            //Set up player scores.
            PlayerOneScore = Skillz_GameData.GetScore(PlayerUniqueID);
            PlayerTwoScore = Skillz_GameData.GetOtherScore(PlayerUniqueID);
        }
	}
	protected override void Update()
	{
		base.Update();

		//Update UI text.
		TurnsLeftUIText.text = TurnsLeft.ToString() + " left";
        if (!Skillz.tournamentIsInProgress())
        {
            CurrentPlayerUIText.text = (IsPlayer1Turn ? "Player 1" : "Player 2");
        }

        //If in online tournament, check for end of turn.
        if (Skillz.tournamentIsInProgress())
        {
            if (Skillz_tookTurn && !LocalPlayer.IsInputDisabled)
            {
                //Serialize the game data for output.
                string knownID = Skillz_GameData.KnownPlayerID;
                Skillz_GameData = new TurnBasedGameData();
                Skillz_GameData.TurnsLeft = TurnsLeft;
                Skillz_GameData.KnownPlayerID = knownID;
                Skillz_GameData.SetScore(PlayerUniqueID, PlayerOneScore);
                Skillz_GameData.SetOtherScore(PlayerUniqueID, PlayerTwoScore);
                Skillz_GameData.Blocks = new int[GameGrid.Instance.GetGridSize().X,
                                                 GameGrid.Instance.GetGridSize().Y];
                for (int x = 0; x < GameGrid.Instance.GetGridSize().X; ++x)
                {
                    for (int y = 0; y < GameGrid.Instance.GetGridSize().Y; ++y)
                    {
                        Vector2i loc = new Vector2i(x, y);
                        GameGridBlock block = GameGrid.Instance.GetBlock(loc);

                        if (block == null)
                        {
                            Skillz_GameData.Blocks[x, y] = -1;
                        }
                        else
                        {
                            Skillz_GameData.Blocks[x, y] = block.ColorID;
                        }
                    }
                }

                //Is the game over?
                if (TurnsLeft == 0)
                {
                    Skillz_tookTurn = false;
                    Skillz.completeTurnWithGameData(Skillz_GameData.ToString(),
                                                    PlayerOneScore.ToString(),
                                                    PlayerOneScore, PlayerTwoScore,
                                                    Skillz.SkillzTurnBasedRoundOutcome.SkillzRoundNoOutcome,
                                                    Skillz.SkillzTurnBasedMatchOutcome.SkillzMatchWin);
                }
                //Otherwise, continue as normal.
                else
                {
                    Skillz_tookTurn = false;
                    Skillz.completeTurnWithGameData(Skillz_GameData.ToString(),
                                                    PlayerOneScore.ToString(),
                                                    PlayerOneScore, PlayerTwoScore,
                                                    Skillz.SkillzTurnBasedRoundOutcome.SkillzRoundNoOutcome,
                                                    Skillz.SkillzTurnBasedMatchOutcome.SkillzMatchNoOutcome);
                }
            }
        }
        //Otherwise, check for end of game.
        else
        {
			if (!LocalPlayer.IsInputDisabled && TurnsLeft <= 0)
			{
				Application.LoadLevel("PresentTurnBasedScore");
			}
        }
	}


	private System.Collections.IEnumerator WaitSpawnCoroutine(List<Vector2i> clearedBlocks)
	{
		yield return new WaitForSeconds(BlockSpawnWaitTime);
		
		//Get how many blocks cleared in each column.
		int[] clearsPerCol = new int[Grid.GetGridSize().X];
		for (int x = 0; x < clearsPerCol.Length; ++x)
		{
			clearsPerCol[x] = 0;
		}
		foreach (Vector2i loc in clearedBlocks)
		{
			clearsPerCol[loc.X] += 1;
		}
		
		//Add blocks to each column to compensate for the missing ones.
		for (int x = 0; x < clearsPerCol.Length; ++x)
		{
			if (clearsPerCol[x] > 0)
			{
				//First find the lowest spot with no block above it.
				int lowestY = 0;
				while (Grid.GetBlock(new Vector2i(x, lowestY)) != null)
				{
					lowestY += 1;
				}
				//Now start at that spot and stack up the new blocks so they fall into place.
				for (int y = lowestY; y < lowestY + clearsPerCol[x]; ++y)
				{
					int deltaStart = y - lowestY;
					CreateBlock(new Vector2((float)x, (float)(Grid.GetGridSize().Y + deltaStart)),
					            new Vector2i(x, y),
					            UnityEngine.Random.Range(0, Constants.NBlockTypes));
				}
			}
		}
	}
}