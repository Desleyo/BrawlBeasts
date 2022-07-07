using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class GameModeManager : MonoBehaviourPunCallbacks
{
    public static GameModeManager manager;
    [SerializeField] LoadingScreen loadScreen;
    public GameObject devToolsScreen;
    CharacterSelectManager selectManager;
    GameObject player;
    GameObject cam;

    [Space, SerializeField] GameObject[] allCharacters;

    [Space, SerializeField] Animator animator;
    [SerializeField] Slider transitionTimeSlider;
    public float transitionTime;

    [Space]
    public GameObject[] gameModes;
    public GameObject[] maps;

    //Current variables on play time
    [HideInInspector] public GameObject currentGameMode;
    [HideInInspector] public GameObject currentMap;

    public List<int> modePerRound = new List<int>();
    public List<int> mapPerRound = new List<int>();

    [Space, SerializeField] Image[] modeButtonImages;
    [SerializeField] Image[] mapButtonImages;
    [SerializeField] Image infiniteTimeButtonImage;

    [HideInInspector] public bool gameStarted;
    [HideInInspector] public int maxRounds;
    [HideInInspector] public int currentRound = -1;
    [HideInInspector] public bool devTools;
    bool playerSpawned;

    Dictionary<GameObject, int> ranking = new Dictionary<GameObject, int>();

    [Space, SerializeField] TextMeshProUGUI roundWinnerText;
    [SerializeField] float winnerOnScreenTime = 5f;
    GameObject roundWinner;

    [Space, SerializeField] GameObject winScreen;
    [SerializeField] TextMeshProUGUI winnerText;
    [SerializeField] GameObject[] characterIcons;

    [Space, SerializeField] GameObject rankingPanel;
    [SerializeField] GameObject playerResultPrefab;

    public GameObject[] players;

    private void Awake()
    {
        manager = this;
    }

    private void Start()
    {
        selectManager = CharacterSelectManager.manager;
        cam = GameObject.FindGameObjectWithTag("MainCamera");

        if (PhotonNetwork.IsMasterClient)
        {
            maxRounds = RoomManager.manager.maxRounds;
            devTools = RoomManager.manager.devTools;
        }
    }

    private void Update()
    {
        if (devTools && Input.GetButtonDown("DevTools") && PhotonNetwork.IsMasterClient)
        {
            devToolsScreen.SetActive(!devToolsScreen.activeSelf);

            if (devToolsScreen.activeSelf)
                Cursor.lockState = CursorLockMode.None;
            else if (currentRound != -1)
                Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        if (PhotonNetwork.IsMasterClient && modePerRound.Count != 0)
        {
            photonView.RPC("SetSelectedModeAndMap", RpcTarget.Others, modePerRound[0], mapPerRound[0]);
        }
    }

    [PunRPC]
    void SetSelectedModeAndMap(int modeIndex, int mapIndex)
    {
        if(modePerRound.Count == 0)
        {
            modePerRound.Add(modeIndex);
            mapPerRound.Add(mapIndex);
        }
        else
        {
            modePerRound[0] = modeIndex;
            mapPerRound[0] = mapIndex;
        }
    }

    public void AddDevMode(int modeIndex)
    {
        if (currentRound == maxRounds && currentGameMode.GetComponent<GameMode>().HasGameEnded())
            return;

        photonView.RPC("ChangeModeAndMapList", RpcTarget.All, modeIndex, Random.Range(0, maps.Length), true);

        foreach (Image image in modeButtonImages)
            image.color = Color.white;

        modeButtonImages[modeIndex].color = Color.green;
    }

    public void AddDevMap(int mapIndex)
    {
        if (currentRound == maxRounds && currentGameMode.GetComponent<GameMode>().HasGameEnded())
            return;

        photonView.RPC("ChangeModeAndMapList", RpcTarget.All, Random.Range(0, gameModes.Length), mapIndex, false);

        foreach (Image image in mapButtonImages)
            image.color = Color.white;

        mapButtonImages[mapIndex].color = Color.green;
    }

    [PunRPC]
    void ChangeModeAndMapList(int modeIndex, int mapIndex, bool changingMode)
    {
        if (currentRound == -1 && modePerRound.Count == 1 || currentRound + 1 < maxRounds && modePerRound.Count != currentRound + 1)
        {
            if(changingMode)
                modePerRound[currentRound + 1] = modeIndex;
            else
                mapPerRound[currentRound + 1] = mapIndex;
        }
        else
        {
            modePerRound.Add(modeIndex);
            mapPerRound.Add(mapIndex);
            maxRounds += 1;
        }
    }

    public void ChangeGameDuration(float addedTime)
    {
        if (!gameStarted || currentGameMode.GetComponent<GameMode>().gameDuration <= 0)
            return;

        GameMode mode = currentGameMode.GetComponent<GameMode>();

        if (addedTime == 0)
        {
            addedTime = infiniteTimeButtonImage.color == Color.white ? Mathf.Infinity : 120f;
            infiniteTimeButtonImage.color = addedTime == Mathf.Infinity ? Color.green : Color.white;
        }
        else if (mode.gameDuration == Mathf.Infinity)
            return;
        else if (addedTime < 0 && currentGameMode.GetComponent<GameMode>().gameDuration - addedTime < 0)
            addedTime = mode.gameDuration;

        mode.photonView.RPC("AddTime", RpcTarget.All, addedTime);
    }

    [PunRPC]
    void GetRoomInfo(int mRounds, bool dTools)
    {
        maxRounds = mRounds;
        devTools = dTools;
    }

    public IEnumerator SetModesAndMaps()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("GetRoomInfo", RpcTarget.All, RoomManager.manager.maxRounds, RoomManager.manager.devTools);

            if(modePerRound.Count == maxRounds)
            {
                photonView.RPC("AddModeAndMap", RpcTarget.All, -1, -1);

                yield break;
            }

            while(modePerRound.Count != maxRounds)
             {
                int randomMode = Random.Range(0, gameModes.Length);
                int randomMap = Random.Range(0, maps.Length);

                if(modePerRound.Count != 0)
                {
                    while (modePerRound[modePerRound.Count - 1] == randomMode)
                    {
                        randomMode = Random.Range(0, gameModes.Length);

                        yield return null;
                    }
                    while(mapPerRound[mapPerRound.Count - 1] == randomMap)
                    {
                        randomMap = Random.Range(0, maps.Length);

                        yield return null;
                    }
                }

                photonView.RPC("AddModeAndMap", RpcTarget.All, randomMode, randomMap);
             }
        }
    }

    [PunRPC]
    void AddModeAndMap(int randomMode, int randomMap)
    {
        if (modePerRound.Count == maxRounds)
        {
            StartCoroutine(TransitionBetweenRounds());
            return;
        }

        modePerRound.Add(randomMode);
        mapPerRound.Add(randomMap);

        if (modePerRound.Count == maxRounds)
            StartCoroutine(TransitionBetweenRounds());
    }

    public void SpawnPlayer()
    {
        if (playerSpawned)
            return;

        GameObject character = allCharacters[selectManager.currentCharacterIndex];
        player = PhotonNetwork.Instantiate(character.name, new Vector3(0, 0, 0), Quaternion.identity);

        PlayerInfo info = player.GetComponent<PlayerInfo>();
        info.SetupPlayer(player);
        info.photonView.RPC("UpdateSkin", RpcTarget.All, selectManager.currentSkinIndex);
        info.photonView.RPC("UpdateItem", RpcTarget.All, selectManager.currentItemIndex);

        playerSpawned = true;
    }

    IEnumerator SetPlayerPos()
    {
        //Wait for every play to spawn/instantiate
        if(currentRound == 0)
        {
            yield return new WaitForSeconds(1);
            StartCoroutine(GetAllPlayers());
        }

        if (!PhotonNetwork.IsMasterClient)
            yield break;

        MapInfo info = currentMap.GetComponent<MapInfo>();
        foreach(GameObject player in players)
        {
            yield return new WaitForSeconds(.25f);

            bool canPos = false;
            while (!canPos)
            {
                canPos = true;

                int index = Random.Range(0, info.spawnPositions.Length);
                Transform pos = info.spawnPositions[index];
                foreach (GameObject otherPlayer in players)
                {
                    if (otherPlayer)
                    {
                        if (Vector3.Distance(otherPlayer.transform.position, pos.position) < 3)
                            canPos = false;
                    }
                }

                if (canPos)
                    photonView.RPC("SetLocalSpawnPos", RpcTarget.All, player.GetComponent<PhotonView>().ViewID, index);

                yield return null;
            }
        }
    }

    [PunRPC]
    void SetLocalSpawnPos(int playerId, int index)
    {
        PhotonView pv = PhotonView.Find(playerId);
        if (pv.IsMine)
        {
            pv.transform.position = currentMap.GetComponent<MapInfo>().spawnPositions[index].position;
        }
    }

    public IEnumerator TransitionBetweenRounds()
    {
        if (currentGameMode) {
            Invoke("StopMovement", winnerOnScreenTime);

        }

        if (currentRound == -1)
        {
            currentRound = 0;

            if (!devToolsScreen.activeSelf)
                Cursor.lockState = CursorLockMode.Locked;
        }

        if (roundWinner)
        {
            roundWinnerText.gameObject.SetActive(true);
            roundWinnerText.text = roundWinner.GetComponent<PhotonView>().Owner.NickName;

            yield return new WaitForSeconds(winnerOnScreenTime);

            if (currentRound == maxRounds)
            {
                winScreen.SetActive(true);
                winScreen.transform.localScale = new Vector3(.1f, .1f, .1f);

                float scale = .1f;
                while(winScreen.transform.localScale.x < 1)
                 {
                    scale += Time.deltaTime;
                    winScreen.transform.localScale = new Vector3(scale, scale, scale);
                    yield return null;
                 } 

                bool showingWinner = false;
                foreach (KeyValuePair<GameObject, int> val in ranking.OrderBy(key => key.Value).Reverse())
                {
                    if (!showingWinner)
                    {
                        winnerText.text = val.Key.GetComponent<PhotonView>().Owner.NickName;
                        characterIcons[val.Key.GetComponent<PlayerInfo>().characterIndex].SetActive(true);
                        showingWinner = true;
                    }

                    GameObject playerResult = Instantiate(playerResultPrefab, rankingPanel.transform);
                    playerResult.GetComponent<TextMeshProUGUI>().text = val.Key.GetComponent<PhotonView>().Owner.NickName;
                    playerResult.GetComponentsInChildren<TextMeshProUGUI>()[1].text = ranking[val.Key].ToString();
                }

                yield return new WaitForSeconds(10);
            }

            gameStarted = false;
        }
        else if(currentRound != 0)
        {
            roundWinnerText.gameObject.SetActive(true);
            roundWinnerText.text = "";
            yield return new WaitForSeconds(winnerOnScreenTime);
        }

        if (currentRound < maxRounds)
        {
            loadScreen.SetupLoadingScreen(modePerRound[currentRound], mapPerRound[currentRound], currentRound, maxRounds);

            animator.SetTrigger("FadeIn");

            StartCoroutine(TransitionTimer());

            yield return new WaitForSeconds(transitionTime / 2);

            if(currentRound != 0)
            {
                foreach (GameObject player in players)
                {
                    if (player)
                        player.SetActive(true);
                }
            }

            StartGameMode();

            yield return new WaitForSeconds(transitionTime / 2);

            animator.SetTrigger("FadeOut");

            if (currentRound == 0)
                StartCoroutine(GetAllPlayers());

            currentGameMode.SetActive(true);
            gameStarted = true;
        }
        else
            StartCoroutine(GameEnded());
    }

    IEnumerator TransitionTimer()
    {
        float time = 0;
        transitionTimeSlider.maxValue = transitionTime;

        while(time < transitionTime)
        {
            time += Time.deltaTime;
            transitionTimeSlider.value = time;
            yield return null;
        }
    }

    IEnumerator GetAllPlayers()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        while(players.Length != PhotonNetwork.PlayerList.Length)
        {
            players = GameObject.FindGameObjectsWithTag("Player");
            yield return null;
        }
    }

    void StopMovement() {
        currentGameMode.GetComponent<GameMode>().PrepairForLoadingScreen(transitionTime);
    }

    void ResetBeforeGameMode()
    {
        foreach (Image image in modeButtonImages)
            image.color = Color.white;
        foreach (Image image in mapButtonImages)
            image.color = Color.white;

        ScoreBoardManager.manager.ResetScoreBoard();
        roundWinnerText.gameObject.SetActive(false);
    }

    void StartGameMode()
    {
        ResetBeforeGameMode();

        if (currentRound == 0)
            gameModes[0].GetComponent<GameMode>().timeBarSlider.gameObject.SetActive(true);

        if (currentGameMode && currentMap)
        {
            currentGameMode.SetActive(false);
            currentMap.SetActive(false);
        }

        if (currentRound == 0)
            SpawnPlayer();

        currentGameMode = gameModes[modePerRound[currentRound]];
        currentMap = maps[mapPerRound[currentRound]];

        currentMap.SetActive(true);

        cam.GetComponent<MoveCamera>().SetCameraBoundaries(currentMap.GetComponent<MapInfo>());

        StartCoroutine(SetPlayerPos());
    }

    [PunRPC]
    public void UpdateRankings(int roundWinnerId, int[] playerIds, float[] scores)
    {
        if(roundWinnerId != 0)
            roundWinner = PhotonView.Find(roundWinnerId).gameObject;

        int points = 1;
        int index = 0;

        foreach (int id in playerIds)
        {
            GameObject player = PhotonView.Find(id).gameObject;

            if (!ranking.ContainsKey(player))
                ranking.Add(player, 0);

            ranking[player] += points;

            if(index + 1 < playerIds.Length)
                if (scores[index] != scores[index + 1])
                    points++;

            index++;
        }

        currentRound++;

        StartCoroutine(TransitionBetweenRounds());
    }

    IEnumerator GameEnded()
    {
        VignetteTransition.manager.PlayTransition(true);

        yield return new WaitForSeconds(1);

        PhotonNetwork.LoadLevel(2);
    }

    public float TimeLeftUntilDone() {
        return transitionTime - transitionTimeSlider.value;
    }

}
