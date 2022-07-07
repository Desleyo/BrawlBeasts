using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Brawl : GameMode {

    public Item[] items;

    public float itemSpawnTime = 5.0F;
    private float timeLeft;


    private Dictionary<GameObject, GameObject> combatTag;
    private GameObject itemSpawnsParent;

    private void OnEnable() {
        Initiate();

        itemSpawnsParent = GameModeManager.manager.currentMap.GetComponent<MapInfo>().itemSpawnsParent;
        itemSpawnsParent.SetActive(true);
        combatTag = new Dictionary<GameObject, GameObject>();
        foreach (GameObject player in players) {
            combatTag.Add(player, null);
        }

    }

    private void OnDisable() {
        itemSpawnsParent.SetActive(false);
        foreach (GameObject destroy in GameObject.FindGameObjectsWithTag("Destroyable")) {
            Destroy(destroy);
        }
    }


    public override void OnGameUpdate() {
        if (PhotonNetwork.IsMasterClient) {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0) {
                SpawnRandomItem();
                timeLeft = itemSpawnTime;
            }
        }
    }

    [PunRPC]
    void OnCombatTag(int playerId, int cause) {
        PhotonView causePV = PhotonView.Find(cause);
        GameObject scorePlayer = PhotonView.Find(playerId).gameObject;
        combatTag[scorePlayer] = causePV.gameObject;

    }

    public void SpawnRandomItem() {
        while (true) {
            Transform toParent = itemSpawnsParent.transform.GetChild(Random.Range(0, itemSpawnsParent.transform.childCount));
            if (toParent.childCount == 0) {
                int rnd = Random.Range(0, items.Length);
                PickupItem random = items[Random.Range(0, items.Length)].pickupItem;
                GameObject obj = PhotonNetwork.Instantiate(random.name, toParent.position, Quaternion.identity);
                photonView.RPC("SetParent", RpcTarget.All, obj.GetComponent<PhotonView>().ViewID, rnd);
                break;
            }
        }
    }


    public void OnDeath(int playerId) {
        PhotonView pv = PhotonView.Find(playerId);
        GameObject deadPlayer = pv.gameObject;
        GameObject cause = combatTag[deadPlayer];

        if (pv.IsMine) {
            deadPlayer.GetComponent<Inventory>().SetCurrentItem(null);
        }

        if (cause != null && cause != deadPlayer) {
            combatTag[deadPlayer] = null;
            photonView.RPC("UpdateScore", RpcTarget.All, cause.GetComponent<PhotonView>().ViewID);
        }
    }

    [PunRPC]
    public void SetParent(int objId, int rnd) {
        GameObject obj = PhotonView.Find(objId).gameObject;
        Transform itemSpawnsParent = GameModeManager.manager.currentMap.GetComponent<MapInfo>().itemSpawnsParent.transform;
        obj.transform.SetParent(itemSpawnsParent.GetChild(rnd), true);
        obj.transform.localPosition = Vector3.zero;

    }

    [PunRPC]
    public void UpdateScore(int causeId) {
        GameObject cause = PhotonView.Find(causeId).gameObject;
        if (!leaderBoard.ContainsKey(cause)) {
            leaderBoard.Add(cause, 0);
        }
        leaderBoard[cause] += 1.0F;
        UpdateScoreBoard(cause);
    }

    [PunRPC]
    void AddTime(float addedTime) {
        if (gameDuration == Mathf.Infinity) {
            gameDuration = addedTime;
            startTime = Time.time;
        } else
            gameDuration += addedTime;

        timeBarSlider.maxValue = gameDuration;

        timeBarSlider.enabled = gameDuration == Mathf.Infinity ? false : true;
    }

}
