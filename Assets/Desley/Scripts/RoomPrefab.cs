using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomPrefab : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI playersText;

    RoomManager roomManager;

    public RoomInfo RoomInfo { get; private set; }

    public void SetRoomInfo(RoomInfo roomInfo, RoomManager rManager)
    {
        roomManager = rManager;
        RoomInfo = roomInfo;

        nameText.text = roomInfo.Name;
        playersText.text = roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers;
    }

    public void RequestJoinRoom()
    {
        roomManager.JoinRoom(RoomInfo.Name);
    }
}
