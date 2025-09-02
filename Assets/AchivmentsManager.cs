using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class AchivmentsManager : MonoBehaviour
{
    [SerializeField] private Image Reach100kPoints;
    [SerializeField] private Image GetTop1Ladder;
    [SerializeField] private Image CollectEggs;
    [SerializeField] private Image GrowUp200Times;
    
    [SerializeField] private Sprite Reach100kPointsCompleted;
    [SerializeField] private Sprite CollectEggsCompleted;
    [SerializeField] private Sprite GrowUp200TimesCompleted;
    [SerializeField] private Sprite GetTop1LadderCompleted;
    
    [SerializeField] LeaderboardManager leaderboard;

    public void UpdateUI()
    {
        if(PlayerPrefs.GetInt("CollectedPoints") >= 100000)
        {
            Reach100kPoints.sprite = Reach100kPointsCompleted;
        }

        if (PlayerPrefs.GetInt("CollectedEggs") >= 100)
        {
            CollectEggs.sprite = CollectEggsCompleted;
        }

        if (PlayerPrefs.GetInt("GrowUp200Times") >= 200)
        {
            GrowUp200Times.sprite = GrowUp200TimesCompleted;
        }

        if (leaderboard.GetPlayerRank("Player") == 1)
        {
            GetTop1Ladder.sprite = GetTop1LadderCompleted;
        }
    }
}
