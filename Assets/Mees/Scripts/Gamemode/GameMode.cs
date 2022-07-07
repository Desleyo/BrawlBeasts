using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Photon.Pun;

public abstract class GameMode : MonoBehaviourPunCallbacks
{
    public float gameDuration = 120.0F;
    private float maxGameDuration = -1f;

    protected GameObject[] players;
    //player, punten
    //punten = (playerAmount - place)
    protected Dictionary<GameObject, float> leaderBoard;
    public ScoreImageInfo highestScoreImage;
    public List<int> playerIds;
    public List<float> playerScore;
    public float startTime;

    [Space] public Slider timeBarSlider;

    public GameObject[] gameModeActivatables;

    public void Initiate()
    {
        if (maxGameDuration == -1)
            maxGameDuration = gameDuration;

        playerIds = new List<int>();
        playerScore = new List<float>();
        leaderBoard = new Dictionary<GameObject, float>();

        players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            leaderBoard.Add(player, 0);
        }

        gameDuration = maxGameDuration;
        startTime = Time.time;
        timeBarSlider.value = timeBarSlider.maxValue = gameDuration;

        NeedToReset();
    }

    public void NeedToReset()
    {
        foreach (GameObject player in players)
        {
            if (player.GetComponent<PhotonView>().IsMine)
            {
                player.GetComponent<Inventory>().SetCurrentItem(null);
                player.GetComponent<Movement>().ResetMovement();
            }

            player.GetComponent<PlayerInfo>().propTail.SetActive(false);
            player.GetComponent<PlayerInfo>().playerTail.SetActive(true);
        }

        GameModeManager.manager.currentMap.GetComponent<MapInfo>().moleSpawnsParent.SetActive(false);
        GameModeManager.manager.currentMap.GetComponent<MapInfo>().pickupTail.SetActive(false);

        gameModeActivatables = GameObject.FindGameObjectsWithTag("Activatables");
        foreach (GameObject activatable in gameModeActivatables)
            activatable.SetActive(false);

        foreach (ScoreImageInfo image in FindObjectsOfType<ScoreImageInfo>())
        {
            image.ribbon.SetActive(false);
        }
    }

    public void PrepairForLoadingScreen(float time) {
        foreach (GameObject player in players) {
            if (player.GetComponent<PhotonView>().IsMine) {
                player.GetComponent<Movement>().ResetMovement();
                player.GetComponent<Movement>().ApplySpeedBoost(0.0F, time);
            }
        }

    }

    private void Update() {
        if (!GameModeManager.manager.gameStarted)
            return;

        timeBarSlider.value = GetGameDurationLeft();

        this.OnGameUpdate();
        if (this.HasGameEnded() && playerIds.Count == 0 && PhotonNetwork.IsMasterClient)
        {
            players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
                if (!leaderBoard.ContainsKey(player))
                    leaderBoard.Add(player, 0);

            int roundwinnerId = 0;
            foreach (KeyValuePair<GameObject, float> val in leaderBoard.OrderBy(key => key.Value))
            {
                playerIds.Add(val.Key.GetComponent<PhotonView>().ViewID);
                playerScore.Add(val.Value);

                if (val.Key.GetComponent<PlayerInfo>().playerScoreImage.GetComponent<ScoreImageInfo>().ribbon.activeSelf)
                    roundwinnerId = val.Key.GetComponent<PhotonView>().ViewID;
            }

            GameModeManager.manager.photonView.RPC("UpdateRankings", RpcTarget.All, roundwinnerId, playerIds.ToArray(), playerScore.ToArray());
        }

    }

    public void UpdateScoreBoard(GameObject player)
    {
        if (HasGameEnded())
            return;

        foreach (GameObject scoreImage in ScoreBoardManager.manager.scoreBoardImages)
        {
            if (scoreImage.GetComponent<PhotonView>().OwnerActorNr == player.GetComponent<PhotonView>().OwnerActorNr)
            {
                scoreImage.GetComponent<ScoreImageInfo>().scoreText.text = leaderBoard[player].ToString("0");
            }
        }

        foreach (GameObject playerInstance in players)
            playerInstance.GetComponent<PlayerInfo>().playerScoreImage.GetComponent<ScoreImageInfo>().ribbon.SetActive(false);

        float highestScore = 0;
        foreach(GameObject playerObj in leaderBoard.Keys)
        {
            if(leaderBoard[playerObj] > highestScore && leaderBoard[playerObj] >= 1)
            {
                highestScore = leaderBoard[playerObj];
                highestScoreImage = playerObj.GetComponent<PlayerInfo>().playerScoreImage.GetComponent<ScoreImageInfo>();
            }
        }

        if (highestScore != 0)
            highestScoreImage.ribbon.SetActive(true);
    }

    public virtual void OnPlayerCollide(int fromId, int toId) {
        GameObject from = PhotonView.Find(fromId).gameObject;
        GameObject to = PhotonView.Find(toId).gameObject;

        Movement mFrom = from.GetComponent<Movement>();
        if (mFrom.dashing) {
            Movement mTo = to.GetComponent<Movement>();
            mTo.photonView.RPC("ApplyKnockback", RpcTarget.All, fromId, mTo.power);
        }
    }

    public abstract void OnGameUpdate();

    public float GetGameDurationLeft() {
        return gameDuration - Mathf.Min(Time.time - startTime, gameDuration);
    }

    public bool HasGameEnded() {
        return GetGameDurationLeft() == 0;
    }
}
