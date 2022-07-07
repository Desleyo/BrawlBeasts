using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class ArrowLookAt : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject arrow;
    [SerializeField] TextMeshProUGUI username;
    GameObject cam;

    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera");
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            if (GameModeManager.manager.gameStarted && !arrow.activeSelf)
                arrow.SetActive(true);

            if (arrow.activeSelf)
                arrow.transform.LookAt(cam.transform);
        }
        else
        {
            if (GameModeManager.manager.gameStarted && !username.gameObject.activeSelf)
            {
                username.gameObject.SetActive(true);
                username.text = photonView.Owner.NickName;
            }
            else
            {
                if (!username.gameObject.activeSelf)
                    username.gameObject.SetActive(true);

                username.text = photonView.Owner.NickName;
            }

            username.transform.LookAt(cam.transform);
        }
    }
}
