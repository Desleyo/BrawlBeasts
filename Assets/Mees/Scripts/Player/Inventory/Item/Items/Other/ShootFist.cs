using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootFist : MonoBehaviourPunCallbacks {

    public Inventory inv;
    public ItemReusable reusable;
    public float knockback = 20.0F;

    public LayerMask groundLayer;

    private Animator animator;
    private Sound shoot;

    [HideInInspector] public float timeWhenUsed;
    private float timeWhenCanHit = 0.5F;

    private bool hasHitThroughObject, hasHitPlayer;

    private void Start() {
        inv = gameObject.transform.root.GetComponent<Inventory>();
        reusable = inv.currentItem as ItemReusable;

        Physics.IgnoreCollision(GetComponent<Collider>(), inv.GetComponent<Collider>());
    }

    private void OnTriggerStay(Collider other) {
        if (!photonView.IsMine)
            return;

        if (other.transform.root.CompareTag("Player")) {
            if (!hasHitThroughObject && !hasHitPlayer) {
                if (Time.time - timeWhenUsed <= timeWhenCanHit) {
                    int playerId = inv.GetComponent<PhotonView>().ViewID;

                    other.transform.root.gameObject.GetComponent<Movement>().photonView.RPC("ApplyKnockback", RpcTarget.All, playerId, knockback);
                    photonView.RPC("Hit", RpcTarget.All);
                    hasHitPlayer = true;
                }
            }
        } else if (other.gameObject.layer != groundLayer && !hasHitPlayer) {
            if (Time.time - timeWhenUsed <= timeWhenCanHit) {
                Debug.Log("Hitte iets anders dan de player (" + other.gameObject + ")");
            }
            hasHitThroughObject = true;
        }
    }

    [PunRPC]
    private void Use() {
        if (animator == null) {
            animator = transform.parent.parent.GetComponent<Animator>();
            shoot = SoundManager.Instance.CreateCopy(SoundManager.Instance.GetSoundByName("Shooting Fist"));
        }

        shoot.Play();
        animator.SetTrigger("Use");
        hasHitThroughObject = hasHitPlayer = false;
    }

    [PunRPC]
    private void Hit() {
        SoundManager.Instance.GetSoundByName("Fist Hit").Play();
    }

}