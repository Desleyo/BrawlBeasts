using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Inventory : MonoBehaviourPunCallbacks
{

    public Item currentItem;
    public Transform toParent;

    public List<Item> noHoldingItems;

    private Animator animator;

    public GameObject heldItemObj;

    private Movement movement;

    private Sound dropSound;

    private void Start() {
        animator = GetComponentInChildren<Animator>();
        movement = GetComponent<Movement>();
        dropSound = SoundManager.Instance.CreateCopy(SoundManager.Instance.GetSoundByName("Drop Item"));
    }

    private void Update() {
        //TODO: animation
        float armAnim = heldItemObj ? 1 : 0;

        if (heldItemObj != null) {
            heldItemObj.transform.localPosition = Vector3.zero;
            heldItemObj.transform.localRotation = Quaternion.identity;

            if (heldItemObj.CompareTag("Arms")) {
                animator.SetLayerWeight(animator.GetLayerIndex("Arms"), armAnim);
                animator.SetLayerWeight(animator.GetLayerIndex("Holding"), 0);
            } else {
                animator.SetLayerWeight(animator.GetLayerIndex("Holding"), armAnim);
                animator.SetLayerWeight(animator.GetLayerIndex("Arms"), 0);
            }
        } else {
            animator.SetLayerWeight(animator.GetLayerIndex("Holding"), 0);
            animator.SetLayerWeight(animator.GetLayerIndex("Arms"), 0);
        }

        if (!PauseManager.manager.pauseMenuOpen && GameModeManager.manager.gameStarted) {
            if (photonView.IsMine && movement.blockInputDurationLeft == 0.0F && currentItem != null && (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.E))) {
                UseCurrentItem();
            }


            if (photonView.IsMine && currentItem != null && Input.GetKeyDown(KeyCode.G)) {
                Brawl brawl = GameModeManager.manager.currentGameMode.GetComponentInChildren<Brawl>();
                if (brawl != null && brawl.gameObject.activeSelf) {
                    this.DropItem();
                }
            }
        }
    }

    public void UseCurrentItem() {
        if (currentItem.UseItem(gameObject)) {
            SetCurrentItem(null);
        }
    }

    public void SetCurrentItem(Item item) {
        if (item != null) {
            if(heldItemObj != null)
                PhotonNetwork.Destroy(heldItemObj); 

            heldItemObj = PhotonNetwork.Instantiate(item.obj.name, toParent.position, toParent.rotation);
            photonView.RPC("InitiateItem", RpcTarget.All, heldItemObj.GetComponent<PhotonView>().ViewID, photonView.ViewID);
            this.currentItem = item;
        } else if (heldItemObj != null) {
            PhotonNetwork.Destroy(heldItemObj);
            this.currentItem = null;
        }
    }


    [PunRPC]
    void InitiateItem(int itemId, int playerId) {
        GameObject item = PhotonView.Find(itemId).gameObject;
        Inventory playerInv = PhotonView.Find(playerId).GetComponent<Inventory>();

        item.transform.SetParent(playerInv.toParent);
        playerInv.heldItemObj = item;
    }


    [PunRPC]
    public void DropItem() {
        if (heldItemObj != null) {
            dropSound.Play();
        }

        if (photonView.IsMine && currentItem != null) {
            Vector3 pos = transform.position;
            pos.y += 0.6F;

            PhotonNetwork.Instantiate(currentItem.pickupItem.name, pos, Quaternion.identity);
            SetCurrentItem(null);
            SetCurrentItem(currentItem = null);
        }
    }

}
