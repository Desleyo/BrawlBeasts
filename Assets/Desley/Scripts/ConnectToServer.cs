using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        if (FindObjectOfType<RoomManager>())
            Destroy(FindObjectOfType<RoomManager>().gameObject);

        PhotonNetwork.ConnectUsingSettings();
        //PhotonNetwork.ConnectToRegion("asia");
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        SceneManager.LoadScene(1);
    }
}
