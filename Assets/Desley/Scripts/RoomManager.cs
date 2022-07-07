using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager manager;

    [SerializeField] TMP_InputField createInput;
    [SerializeField] int charachterLimit;

    [Space, SerializeField] TextMeshProUGUI maxPlayerCount;
    [SerializeField] byte minPlayers;
    [SerializeField] byte maxPlayers;
    [SerializeField] TextMeshProUGUI maxRoundCount;

    public string roomName;
    public byte playerLimit = 4;
    public int maxRounds = 5;
    public bool publicRoom = true;
    public bool devTools = false;

    private void Start()
    {
        manager = this;

        if(createInput)
            createInput.characterLimit = charachterLimit;

        DontDestroyOnLoad(this);
    }

    public void SetMaxPlayers(bool addMaxPlayer)
    {    
        playerLimit = addMaxPlayer ? playerLimit += 1 : playerLimit -= 1;
        playerLimit = (byte)Mathf.Clamp(playerLimit, minPlayers, maxPlayers);

        maxPlayerCount.text = playerLimit.ToString();
    }

    public void SetMaxRounds(bool addMaxRound)
    {
        maxRounds = addMaxRound ? maxRounds += 1 : maxRounds -= 1;
        maxRounds = Mathf.Clamp(maxRounds, 1, 20);
        maxRoundCount.text = maxRounds.ToString();
    }

    public void SetPrivateRoom(bool status)
    {
        publicRoom = status;
    }

    public void SetDevTools(bool status)
    {
        devTools = status;
    }

    public void RequestPrivateJoin(TMP_InputField field)
    {
        JoinRoom(field.text);
    }

    public void CreateRoom()
    {
        if(!string.IsNullOrEmpty(createInput.text))
            StartCoroutine(WaitForTransition(1f));
    }

    IEnumerator WaitForTransition(float time)
    {
        VignetteTransition.manager.PlayTransition(true);

        yield return new WaitForSeconds(time);

        string createRoomName = publicRoom ? createInput.text : createInput.text + " (private)";
        PhotonNetwork.CreateRoom(createRoomName, new RoomOptions { MaxPlayers = playerLimit }, TypedLobby.Default);
    }

    public void JoinRoom(string roomToJoin)
    {
        List<RoomPrefab> rooms = new List<RoomPrefab>();
        rooms = GetComponent<RoomList>().rooms;

        RoomInfo roomInfo = null;
        foreach(RoomPrefab room in rooms)
        {
            if (room.RoomInfo.Name == roomToJoin || room.RoomInfo.Name == roomToJoin + " (private)")
                roomInfo = room.RoomInfo;
        }

        if (roomInfo != null)
        {
            if (roomToJoin != "" && roomInfo.IsOpen && roomInfo.PlayerCount != roomInfo.MaxPlayers)
            {
                roomName = string.Join(" ", roomInfo.Name).Contains("(private)") ? roomToJoin + " (private)" : roomToJoin;

                FindObjectOfType<VignetteTransition>().PlayTransition(true);

                Invoke(nameof(WaitForTransition), 1f);
            }
        }
    }

    void WaitForTransition()
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        PhotonNetwork.LoadLevel(2);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        FindObjectOfType<VignetteTransition>().PlayTransition(false);
    }
}
