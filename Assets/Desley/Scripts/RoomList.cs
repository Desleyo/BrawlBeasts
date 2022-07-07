using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class RoomList : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField searchBarInput;
    [SerializeField] Transform content;
    [SerializeField] RoomPrefab roomPrefab;

    [Space, SerializeField] RoomManager roomManager;
    public List<RoomPrefab> rooms;

    RoomPrefab preExistingRoom;

    void Awake()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);

        foreach(RoomInfo info in roomList)
        {
            FilterRoomsOnInput();

            if (info.RemovedFromList) //Remove room from List if it's removed
            {
                int index = rooms.FindIndex(x => x.RoomInfo.Name == info.Name);
                Destroy(rooms[index].gameObject);
                rooms.RemoveAt(index);
            }
            else if (RoomAlreadyExists(info)) //Update the info of the room if called
            {
                preExistingRoom.SetRoomInfo(info, roomManager);
            }
            else //Make a new UI element for a newly created room
            {
                RoomPrefab newRoom = Instantiate(roomPrefab, content);
                newRoom.SetRoomInfo(info, roomManager);

                rooms.Add(newRoom);

                if (string.Join(" ", info.Name).Contains("(private)"))
                    newRoom.gameObject.SetActive(false);
            }
        }
    }

    bool RoomAlreadyExists(RoomInfo info)
    {
        preExistingRoom = null;
        bool exists = false;

        foreach(RoomPrefab room in rooms)
        {
            if (room.RoomInfo.Name == info.Name)
            {
                preExistingRoom = room;
                exists = true;
            }
        }

        return exists;
    }

    public void FilterRoomsOnInput()
    {
        if (string.IsNullOrEmpty(searchBarInput.text))
        {
            foreach (RoomPrefab room in rooms)
            {
                bool active = !string.Join(" ", room.RoomInfo.Name).Contains("(private)");
                room.gameObject.SetActive(active);
            }

            return;
        }

        foreach(RoomPrefab room in rooms)
        {
            if (room.nameText.text.Contains("(private)"))
                return;

            int amountOfOverlap = 0;
            foreach(char nameChar in room.nameText.text.ToCharArray())
            {
                foreach(char inputChar in searchBarInput.text.ToCharArray())
                {
                    if (nameChar == inputChar)
                        amountOfOverlap++;
                }
            }

            bool activate = amountOfOverlap >= 2;
            room.gameObject.SetActive(activate);
        }
    }
}
