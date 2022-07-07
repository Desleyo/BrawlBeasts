using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SeekShelter : GameMode
{
    MapInfo currentMapInfo;
    GameObject boulder;

    [SerializeField] Item gameModeItem;

    [Space, SerializeField] int spawnOnPlayerChance = 5;
    [SerializeField] float spawnHeight = 30f;
    [SerializeField] float minSpawnTime = 2f;
    [SerializeField] float maxSpawnTime = 5f;
    float spawnTimeLeft;

    float minClampX;
    float maxClampX;
    float minClampZ;
    float maxClampZ;

    public GameObject[] allPlayers;

    private void OnEnable()
    {
        StartGame();
    }

    public void StartGame()
    {
        Initiate();

        currentMapInfo = GameModeManager.manager.currentMap.GetComponent<MapInfo>();
        boulder = currentMapInfo.boulder;

        minClampX = currentMapInfo.minBoulderClampX;
        maxClampX = currentMapInfo.maxBoulderClampX;
        minClampZ = currentMapInfo.minBoulderClampZ;
        maxClampZ = currentMapInfo.maxBoulderClampZ;

        spawnTimeLeft = 3f;

        //foreach (GameObject player in players)
        //{
        //    if (player.GetComponent<PhotonView>().IsMine)
        //    {
        //        player.GetComponent<Inventory>().SetCurrentItem(gameModeItem);
        //        return;
        //    }
        //}
    }

    public override void OnGameUpdate()
    {
        spawnTimeLeft -= Time.deltaTime;

        if (spawnTimeLeft <= 0 && PhotonNetwork.IsMasterClient)
            SpawnNewBoulder();

        foreach(GameObject player in players)
        {
            if (player.activeSelf)
            {
                if (!leaderBoard.ContainsKey(player))
                    leaderBoard.Add(player, 0);

                leaderBoard[player] += Time.deltaTime;
                UpdateScoreBoard(player);
            }
        }
    }

    void SpawnNewBoulder()
    {
        float posX = 0;
        float posZ = 0;
        float randomYRotation = Random.Range(0f, 360f);

        if (Random.Range(0, spawnOnPlayerChance) == 0)
        {
            allPlayers = GameObject.FindGameObjectsWithTag("Player");
            if (allPlayers.Length == 0)
                return;

            int randomPlayer = Random.Range(0, allPlayers.Length);

            posX = allPlayers[randomPlayer].transform.position.x;
            posZ = allPlayers[randomPlayer].transform.position.z;
        }
        else
        {
            posX = Random.Range(minClampX, maxClampX);
            posZ = Random.Range(minClampZ, maxClampZ);
        }

        PhotonNetwork.Instantiate(boulder.name, new Vector3(posX, spawnHeight, posZ), Quaternion.Euler(-90, randomYRotation, 0));

        photonView.RPC("ResetTimer", RpcTarget.All, Random.Range(minSpawnTime, maxSpawnTime));
    }

    [PunRPC]
    void ResetTimer(float addedTime)
    {
        spawnTimeLeft = addedTime;
    }

    [PunRPC]
    void PlayerHit(int playerId)
    {
        PhotonView.Find(playerId).gameObject.SetActive(false);

        bool allPlayersInactive = true;
        foreach (GameObject player in players)
        {
            if (player.activeSelf)
                allPlayersInactive = false;
        }

        if (allPlayersInactive)
            gameDuration = 0;
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
