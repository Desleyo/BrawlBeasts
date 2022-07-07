using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LionCageButton : MonoBehaviourPunCallbacks
{
    MeshRenderer mRenderer;

    [SerializeField] LionCage lionCage;
    [SerializeField] float lionCageCooldown = 5f;
    float currentCooldown;

    void Start()
    {
        mRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        if (currentCooldown > 0)
            currentCooldown -= Time.deltaTime;
        else if (mRenderer.material.color != Color.red && PhotonNetwork.IsMasterClient)
            photonView.RPC("ChangeButtonColor", RpcTarget.All, true);
    }

    public void OnTriggerStay(Collider other)
    {
        if (currentCooldown > 0)
            return;
        else
            currentCooldown = lionCageCooldown;

        GameObject collidedWith = other.gameObject;
        if (collidedWith.CompareTag("Player") && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ChangeButtonColor", RpcTarget.All, false);
            lionCage.photonView.RPC("StartSequence", RpcTarget.All, collidedWith.GetComponent<PhotonView>().ViewID);
        }
    }

    [PunRPC]
    void ChangeButtonColor(bool active)
    {
        Color color = active ? Color.red : Color.black;
        mRenderer.material.color = color;
    }
}
