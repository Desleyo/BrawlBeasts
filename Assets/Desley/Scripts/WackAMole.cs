using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WackAMole : GameMode
{
    [SerializeField] Item hammer;

    Transform[] moleSpawnPositions;
    [SerializeField] GameObject molePrefab;
    public List<Transform> spawnedMoles = new List<Transform>();
    List<GameObject> moleAlreadyHit = new List<GameObject>();

    [SerializeField] float spawnTimeMin;
    [SerializeField] float spawnTimeMax;
    [SerializeField] float disappearTime = 5f;
    float spawnTimeLeft = 3f;

    private Sound moleAppear, moleDissapear, moleWack;

    void OnEnable()
    {
        StartGame();
        this.moleAppear = SoundManager.Instance.GetSoundByName("Mole Appear");
        this.moleDissapear = SoundManager.Instance.GetSoundByName("Mole Dissapear");
        this.moleWack = SoundManager.Instance.GetSoundByName("Mole Wack");

    }

    public void StartGame()
    {
        Initiate();

        GameModeManager.manager.currentMap.GetComponent<MapInfo>().moleSpawnsParent.SetActive(true);
        moleSpawnPositions = GameModeManager.manager.currentMap.GetComponent<MapInfo>().moleSpawnPositions;
       
        foreach(GameObject player in players)
        {
            if (player.GetComponent<PhotonView>().IsMine)
            {
                player.GetComponent<Inventory>().SetCurrentItem(hammer);
                return;
            }
        }
    }

    public override void OnGameUpdate()
    {
        spawnTimeLeft -= Time.deltaTime;

        if (spawnTimeLeft <= 0 && PhotonNetwork.IsMasterClient)
            StartCoroutine(SpawnNewMole());
    }

    IEnumerator SpawnNewMole()
    {
        bool canSpawn = false;
        while (!canSpawn)
        {
            canSpawn = true;

            Transform pos = moleSpawnPositions[Random.Range(0, moleSpawnPositions.Length)];
            foreach(Transform mole in spawnedMoles)
            {
                if(Vector3.Distance(mole.position, pos.position) < 3)
                {
                    canSpawn = false;
                }
            }

            if(canSpawn)
            {
                Transform mole = PhotonNetwork.Instantiate(molePrefab.name, pos.position, Quaternion.Euler(0, 180, 0)).transform;
                moleAppear.Play();
                photonView.RPC("ListMoles", RpcTarget.All, mole.GetComponent<PhotonView>().ViewID, Random.Range(spawnTimeMin, spawnTimeMax));
            }

            yield return null;
        }
    }

    IEnumerator MoleDisappear(GameObject mole, float waitToDisappear, bool isHit)
    {
        yield return new WaitForSeconds(waitToDisappear);

        if (mole)
            mole.GetComponent<Collider>().enabled = false;

        if (isHit)
        {
            mole.GetComponent<Animator>().SetTrigger("Hit");
            //mole hit sound
            //moleDissapear.Play();
            moleWack.Play();
            yield return new WaitForSeconds(1);
        }

        if (mole)
        {
            moleDissapear.Play();
            mole.GetComponent<Animator>().SetTrigger("Down");

            yield return new WaitForSeconds(1);

            if (mole)
            {
                spawnedMoles.Remove(mole.transform);
                moleAlreadyHit.Remove(mole);

                if (PhotonNetwork.IsMasterClient)
                    PhotonNetwork.Destroy(mole);
            }
        }

    }

    [PunRPC]
    void ListMoles(int moleId, float timeToBeAdded)
    {
        Transform mole = PhotonView.Find(moleId).transform;
        spawnedMoles.Add(mole);

        spawnTimeLeft = timeToBeAdded;
   
        StartCoroutine(MoleDisappear(mole.gameObject, disappearTime, false));
    }

    [PunRPC]
    public void HitMole(int playerId, int moleId)
    {
        GameObject player = PhotonView.Find(playerId).gameObject;
        GameObject mole = PhotonView.Find(moleId).gameObject;

        if (!moleAlreadyHit.Contains(mole))
            moleAlreadyHit.Add(mole);
        else
            return;

        if (!leaderBoard.ContainsKey(player))
        {
            leaderBoard.Add(player, 0);
        }

        leaderBoard[player]++;
        UpdateScoreBoard(player);

        StartCoroutine(MoleDisappear(mole, 0, true));
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
