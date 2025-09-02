[System.Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int score;
    public int rank;

    public LeaderboardEntry(string name, int playerScore, int playerRank)
    {
        playerName = name;
        score = playerScore;
        rank = playerRank;
    }
}