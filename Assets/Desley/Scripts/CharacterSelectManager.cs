using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class CharacterSelectManager : MonoBehaviourPunCallbacks
{
    public static CharacterSelectManager manager;

    [SerializeField] Transform newCharacterPos;
    [SerializeField] Transform[] otherCharacterPositions;
    [SerializeField] public GameObject[] allCharacters;
    GameObject currentCharacter;
    PlayerInfo currentPlayerInfo;
    int skinsLength;
    int itemsLength;

    [HideInInspector] public int currentCharacterIndex;
    [HideInInspector] public int currentSkinIndex;
    [HideInInspector] public int currentItemIndex;

    [Space, SerializeField] GameObject readyButton;
    [SerializeField] TMP_InputField usernameInputField;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] float maxWaitTime = 3;
    float currentWaitTime = Mathf.Infinity;

    [Space, SerializeField] GameObject characterSelectUI;
    [SerializeField] TextMeshProUGUI playerJoinedText;
    [SerializeField] TextMeshProUGUI amountReadyText;
    [SerializeField] TextMeshProUGUI waitTimeText;

    int amountOfPlayersReady;
    bool startedTimer;

    private Sound selectSound;

    void Awake()
    {
        manager = this;
        selectSound = SoundManager.Instance.GetSoundByName("Character Select");
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;

        SpawnNewPlayerCharacter(Random.Range(0, allCharacters.Length));

        UpdatePlayerCount();

        StartCoroutine(PositionCharactersOnStart());
    }

    IEnumerator PositionCharactersOnStart()
    {
        yield return new WaitForSeconds(.1f);

        foreach (GameObject character in GetAllPlayers())
        {
            if (!character.GetComponent<PhotonView>().IsMine)
            {
                foreach (Transform pos in otherCharacterPositions)
                {
                    float smallestDist = Mathf.Infinity;
                    foreach (GameObject player in GetAllPlayers())
                    {
                        float distance = Vector3.Distance(pos.position, player.transform.position);
                        if (distance < smallestDist)
                            smallestDist = distance;
                    }

                    if (smallestDist > 1)
                    {
                        character.transform.SetParent(pos);
                        character.transform.position = pos.position;
                        character.transform.rotation = pos.rotation;
                        break;
                    }
                }
            }
        }
    }

    [PunRPC]
    void PositionNewCharacter(int playerId)
    {
        GameObject character = PhotonView.Find(playerId).gameObject;

        foreach (Transform pos in otherCharacterPositions)
        {
            float smallestDist = Mathf.Infinity;
            foreach (GameObject player in GetAllPlayers())
            {
                float distance = Vector3.Distance(pos.position, player.transform.position);
                if (distance < smallestDist)
                    smallestDist = distance;
            }

            if (smallestDist > 1)
            {
                character.transform.SetParent(pos);
                character.transform.position = pos.position;
                character.transform.rotation = pos.rotation;
                return;
            }
        }
    }

    public void SpawnNewPlayerCharacter(int index)
    {
        if(currentCharacter)
            PhotonNetwork.Destroy(currentCharacter);

        GameObject newCharacter = PhotonNetwork.Instantiate(allCharacters[index].name, new Vector3(0, 0, 0), Quaternion.identity);
        newCharacter.transform.rotation = Quaternion.Euler(0, 180, 0);
        newCharacter.transform.SetParent(newCharacterPos);
        newCharacter.transform.position = newCharacterPos.position;

        currentCharacter = newCharacter;
        currentPlayerInfo = newCharacter.GetComponent<PlayerInfo>();
        skinsLength = currentPlayerInfo.bodySkins.Length;
        itemsLength = currentPlayerInfo.items.Length;
        currentCharacterIndex = index;
        currentSkinIndex = 0;
        currentItemIndex = -1;

        currentPlayerInfo.photonView.RPC("UpdateSkin", RpcTarget.All, currentSkinIndex);
        currentPlayerInfo.photonView.RPC("UpdateItem", RpcTarget.All, currentItemIndex);

        selectSound.source.Play();

        photonView.RPC("PositionNewCharacter", RpcTarget.Others, newCharacter.GetComponent<PhotonView>().ViewID);
    }

    public void ChangeSkin(bool nextSkin)
    {
        currentSkinIndex += nextSkin ? 1 : -1;
        currentSkinIndex = currentSkinIndex < 0 ? skinsLength - 1 : currentSkinIndex > skinsLength - 1 ? 0 : currentSkinIndex;

        currentPlayerInfo.photonView.RPC("UpdateSkin", RpcTarget.All, currentSkinIndex);
    }

    public void ChangeItem(bool nextItem)
    {
        currentItemIndex += nextItem ? 1 : -1;
        currentItemIndex = currentItemIndex < -1 ? itemsLength - 1 : currentItemIndex > itemsLength - 1 ? -1 : currentItemIndex;

        currentPlayerInfo.photonView.RPC("UpdateItem", RpcTarget.All, currentItemIndex);
    }

    public void UpdatePlayerNames()
    {
        foreach(GameObject player in GetAllPlayers())
        {
            if (player.GetComponent<PhotonView>().IsMine)
            {
                player.GetComponent<PlayerInfo>().UpdateNickname();
                return;
            }
        }
    }

    GameObject[] GetAllPlayers()
    {
        return GameObject.FindGameObjectsWithTag("Player");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerCount();
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("UpdatePlayersReady", RpcTarget.All, amountOfPlayersReady);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        UpdatePlayerCount();

        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("ResetReadyStatus", RpcTarget.All);
    }

    void UpdatePlayerCount()
    {
        playerJoinedText.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString() + " / " + PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
    }

    [PunRPC]
    void UpdatePlayersReady(int readyAmount)
    {
        amountOfPlayersReady = readyAmount;
        amountReadyText.text = amountOfPlayersReady.ToString();
    }

    public void RequestReadyUp()
    {
        if (!string.IsNullOrEmpty(ScoreBoardManager.manager.playerUsername.text))
        {
            readyButton.SetActive(false);
            photonView.RPC("ReadyUp", RpcTarget.All);
        }
    }

    [PunRPC]
    void ResetReadyStatus()
    {
        amountOfPlayersReady = 0;
        amountReadyText.text = "0";
        readyButton.SetActive(true);
        usernameInputField.enabled = true;

        startedTimer = false;
        currentWaitTime = Mathf.Infinity;

        if(PhotonNetwork.IsMasterClient)
            PhotonNetwork.CurrentRoom.IsOpen = PhotonNetwork.CurrentRoom.IsVisible = true;
    }

    [PunRPC]
    void ReadyUp()
    {
        amountOfPlayersReady++;
        amountReadyText.text = amountOfPlayersReady.ToString();

        if(amountOfPlayersReady > PhotonNetwork.PlayerList.Length / 2 && !startedTimer)
        {
            startedTimer = true;
            currentWaitTime = maxWaitTime;

            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.CurrentRoom.IsOpen = PhotonNetwork.CurrentRoom.IsVisible = false;
        }
    }

    void Update()
    {
        if (currentWaitTime != Mathf.Infinity)
        {
            currentWaitTime -= Time.deltaTime;
            DisplayTimer();

            if (currentWaitTime <= 0)
            {
                usernameInputField.enabled = false;

                MoveCamera moveCam = FindObjectOfType<MoveCamera>();
                moveCam.StartCoroutine(moveCam.SetCameraPos());

                GameModeManager.manager.StartCoroutine(GameModeManager.manager.SetModesAndMaps());

                currentWaitTime = Mathf.Infinity;
            }
        }
        else if (waitTimeText.gameObject.activeSelf)
            waitTimeText.gameObject.SetActive(false);
    }

    void DisplayTimer()
    {
        if (!waitTimeText.gameObject.activeSelf)
            waitTimeText.gameObject.SetActive(true);

        waitTimeText.text = currentWaitTime.ToString("0");
    }

    public void DisableSelectUI()
    {
        characterSelectUI.GetComponent<CanvasGroup>().alpha = 0;
        characterSelectUI.GetComponent<CanvasGroup>().interactable = false;
    }
}
