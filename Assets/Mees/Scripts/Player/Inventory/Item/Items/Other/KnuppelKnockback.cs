using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class KnuppelKnockback : MonoBehaviourPunCallbacks {

    public Inventory inv;
    public ItemReusable reusable;
    public float knockback = 10.0F;

    private void Start() {
        inv = gameObject.transform.root.GetComponent<Inventory>();
        reusable = inv.currentItem as ItemReusable;

        Physics.IgnoreCollision(GetComponent<Collider>(), inv.GetComponent<Collider>());
    }

    private void OnTriggerEnter(Collider other) {
        if (!photonView.IsMine)
            return;

        if (other.CompareTag("Player") && !reusable.CanUseItem()) {
            int playerId = inv.GetComponent<PhotonView>().ViewID;

            other.gameObject.GetComponent<Movement>().photonView.RPC("ApplyKnockback", RpcTarget.All, playerId, knockback);
        }
    }

}
