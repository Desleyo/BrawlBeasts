using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TailGrabber : GameMode
{

    public GameObject currentTailHolder;
    private GameObject tailInMiddle;

    private float tailCooldown = 1.0F;
    private float tailCooldownLeft;

    void OnEnable()
    {
        StartGame();
    }

    private void OnDisable()
    {
        photonView.RPC("SetActivePickupTails", RpcTarget.All, false);
    }

    void StartGame()
    {
        Initiate();
        Invoke(nameof(WaitToSetTailholder), .3f);
    }

    void WaitToSetTailholder()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject randomTailHolder = players[(int)Mathf.Round(Random.Range(0, players.Length - 1))];
            photonView.RPC("SetCurrentTailHolder", RpcTarget.All, randomTailHolder.GetComponent<PhotonView>().ViewID);
        }
    }

    public override void OnGameUpdate()
    {
        if (currentTailHolder)
        {
            if (!leaderBoard.ContainsKey(currentTailHolder))
            {
                leaderBoard.Add(currentTailHolder, 0);
            }

            leaderBoard[currentTailHolder] += Time.deltaTime;
            UpdateScoreBoard(currentTailHolder);
        }
        tailCooldownLeft = Mathf.Max(tailCooldownLeft - Time.deltaTime, 0);
    }

    [PunRPC]
    public void SetCurrentTailHolder(int playerId)
    {
        GameObject tailHolder = playerId != -1 ? PhotonView.Find(playerId).gameObject : null;

        if (currentTailHolder)
        {
            currentTailHolder.GetComponent<PlayerInfo>().playerTail.SetActive(true);
            currentTailHolder.GetComponent<PlayerInfo>().propTail.SetActive(false);
        }

        currentTailHolder = tailHolder;
        if (currentTailHolder)
        {
            currentTailHolder.GetComponent<PlayerInfo>().playerTail.SetActive(false);
            currentTailHolder.GetComponent<PlayerInfo>().propTail.SetActive(true);
            tailCooldownLeft = tailCooldown;
        }
    }

    public void OnCurrentHolderDeath()
    {
        photonView.RPC("SetCurrentTailHolder", RpcTarget.All, -1);
        photonView.RPC("SetActivePickupTails", RpcTarget.All, true);
    }

    [PunRPC]
    private void SetActivePickupTails(bool active)
    {
        //TODO: fix bug idk wrm ie niet werkt
        GameModeManager.manager.currentMap.GetComponent<MapInfo>().pickupTail.SetActive(active);
    }

    public override void OnPlayerCollide(int fromId, int toId)
    {
        GameObject to = PhotonView.Find(toId).gameObject;

        if (to == currentTailHolder && tailCooldownLeft == 0)
            photonView.RPC("SetCurrentTailHolder", RpcTarget.All, fromId);

        base.OnPlayerCollide(fromId, toId);
    }

    [PunRPC]
    void AddTime(float addedTime)
    {
        if (gameDuration == Mathf.Infinity)
        {
            gameDuration = addedTime;
            startTime = Time.time;
        }
        else
            gameDuration += addedTime;

        timeBarSlider.maxValue = gameDuration;

        timeBarSlider.enabled = gameDuration == Mathf.Infinity ? false : true;
    }
}
