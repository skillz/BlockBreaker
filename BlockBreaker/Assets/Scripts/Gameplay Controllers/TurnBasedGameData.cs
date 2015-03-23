using System;
using System.Text;
using System.Collections.Generic;


/// <summary>
/// All the data necessary to describe a Skillz turn-based tournament.
/// This class is meant to be serialized to/from a string; Skillz gives turn-based tournaments
///     the ability to submit a "gameData" string when ending a turn, and read that string back when
///     starting a new turn.
/// </summary>
public class TurnBasedGameData
{
    /// <summary>
    /// The blocks on the game grid, represented by their color ID.
    /// </summary>
    public int[,] Blocks = null;

    /// <summary>
    /// The number of turns left in the game.
    /// </summary>
    public int TurnsLeft = -1;

    /// <summary>
    /// The unque ID of the player who started the game.
    /// </summary>
    public string KnownPlayerID;
    /// <summary>
    /// The score of the player who started the game.
    /// </summary>
    public int KnownPlayerIDScore;

    /// <summary>
    /// The score of the player who took his turn second.
    /// (his identity is undetermined until he actually takes his turn).
    /// </summary>
    public int UnknownPlayerIDScore;


    /// <summary>
    /// Gets the score of the player with the given ID.
    /// </summary>
    public int GetScore(string playerID)
    {
        return (playerID == KnownPlayerID) ? KnownPlayerIDScore : UnknownPlayerIDScore;
    }
    /// <summary>
    /// Gets the score of the opponent of the player with the given ID.
    /// </summary>
    public int GetOtherScore(string playerID)
    {
        return (playerID == KnownPlayerID) ? UnknownPlayerIDScore : KnownPlayerIDScore;
    }
    /// <summary>
    /// Sets the score of the player with the given ID.
    /// </summary>
    public void SetScore(string playerID, int newScore)
    {
        if (playerID == KnownPlayerID)
        {
            KnownPlayerIDScore = newScore;
        }
        else
        {
            UnknownPlayerIDScore = newScore;
        }
    }
    /// <summary>
    /// Sets the score of the opponent of the player with the given ID.
    /// </summary>
    public void SetOtherScore(string playerID, int newScore)
    {
        if (playerID == KnownPlayerID)
        {
            UnknownPlayerIDScore = newScore;
        }
        else
        {
            KnownPlayerIDScore = newScore;
        }
    }


    /// <summary>
    /// Serializes the game data to a string.
    /// </summary>
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder("");
        sb.Append(TurnsLeft.ToString());
        sb.Append('x');
        sb.Append(KnownPlayerID + ";" + KnownPlayerIDScore);
        sb.Append('x');
        sb.Append(UnknownPlayerIDScore);
        sb.Append('x');
        sb.Append(Blocks.GetLength(0).ToString());
        sb.Append('x');
        sb.Append(Blocks.GetLength(1).ToString());
        sb.Append('x');
        for (int y = 0; y < Blocks.GetLength(1); ++y)
        {
            for (int x = 0; x < Blocks.GetLength(0); ++x)
            {
                sb.Append(Blocks[x, y].ToString());
                sb.Append(';');
            }
        }
        sb.Remove(sb.Length - 1, 1);

        return sb.ToString();
    }
    /// <summary>
    /// Parses the given string (presumably the return value from TurnBasedGameData.ToString())
    ///     and stores the data in this instance.
    /// Returns whether it was successful.
    /// </summary>
    public bool ParseFrom(string inStr)
    {
        //Get the individual pieces of data.
        string[] args = inStr.Split('x');

        //Make sure the right number of data pieces is there.
        if (args.Length != 6)
        {
            return false;
        }

        //Get the number of turns left.
        if (!int.TryParse(args[0], out TurnsLeft))
        {
            return false;
        }

        //Get the player scores, starting with the player who began the match.
        string[] p1Score = args[1].Split(';');
        if (p1Score.Length != 2)
        {
            return false;
        }
        KnownPlayerID = p1Score[0];
        if (!Int32.TryParse(p1Score[1], out KnownPlayerIDScore))
        {
            return false;
        }

        if (!Int32.TryParse(args[2], out UnknownPlayerIDScore))
        {
            return false;
        }


        //Get the width/height of the grid.
        int x, y;
        if (!int.TryParse(args[3], out x) || !int.TryParse(args[4], out y))
        {
            return false;
        }
        Blocks = new int[x, y];


        //Get the grid elements.

        string[] blocksStr = args[5].Split(';');
        if (blocksStr.Length != (x * y))
        {
            return false;
        }

        int index = 0;
        for (int _y = 0; _y < y; ++_y)
        {
            for (int _x = 0; _x < x; ++_x)
            {
                if (!int.TryParse(blocksStr[index++], out Blocks[_x, _y]))
                {
                    return false;
                }
            }
        }

        return true;
    }
}