using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItem : MonoBehaviourPunCallbacks
{
    public Item item;
    public float despawnTime = 10.0F;

    public float pickupDelay = 0.25F;
    private float timeTillDespawn;

    private Sound pickupSound;

    void Start() {
        timeTillDespawn = despawnTime;
        pickupSound = SoundManager.Instance.GetSoundByName("Pick Up");
    }

    void Update() {
        pickupDelay = Mathf.Max(pickupDelay - Time.deltaTime, 0);
        timeTillDespawn = Mathf.Max(timeTillDespawn - Time.deltaTime, 0);
        if (timeTillDespawn == 0) {
            photonView.RPC("DestroyPickup", RpcTarget.All, photonView.ViewID);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && pickupDelay == 0) {
            PhotonView pv = other.GetComponent<PhotonView>();
            if (pv.IsMine) {
                Inventory inventory = other.GetComponent<Inventory>();
                if (inventory.currentItem == null) {
                    inventory.SetCurrentItem(item);
                    photonView.RPC("DestroyPickup", RpcTarget.All, photonView.ViewID);
                }
            }
        }
    }

    [PunRPC]
    void DestroyPickup(int id) {
        pickupSound.Play();
        PhotonView pv = PhotonView.Find(id);
        if (pv.IsMine) {
            PhotonNetwork.Destroy(pv.gameObject);
        }
    }

}
