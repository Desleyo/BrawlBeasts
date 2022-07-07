using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HammerHit : MonoBehaviourPunCallbacks {
    public Inventory inv;
    public ItemReusable reusable;
    public GameObject particle;

    private readonly Vector3 color = new Vector3(1, 0, 0);

    private GameObject spawned;
    private Collider collider;

    private float lastTimeParticleSpawned;

    private Sound groundHit;

    private void Start()
    {
        inv = gameObject.transform.root.GetComponent<Inventory>();
        reusable = inv.currentItem as ItemReusable;

        Physics.IgnoreCollision(collider = GetComponent<Collider>(), inv.GetComponent<Collider>());
        groundHit = SoundManager.Instance.CreateCopy(SoundManager.Instance.GetSoundByName("Hammer Hit Other"));
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
            return;

        if (other.CompareTag("Activatables"))
        {
            if (!reusable.CanUseItem()) {

                int playerId = inv.GetComponent<PhotonView>().ViewID;
                int moleId = other.GetComponent<PhotonView>().ViewID;

                GameModeManager.manager.currentGameMode.GetComponent<WackAMole>().photonView.RPC("HitMole", RpcTarget.All, playerId, moleId);
            }
        } else if (other.CompareTag("Player")) {
            if (!reusable.CanUseItem()) {
                RespawnBlink blink = other.gameObject.GetComponent<RespawnBlink>();
                blink.photonView.RPC("StartAnimation", RpcTarget.All, false, 1.0f, 1.0f, true);
            }


        } else {
            if (!reusable.CanUseItem() && Time.time - lastTimeParticleSpawned > 0.7F) {
                Vector3 closest = collider.ClosestPoint(other.transform.position);
                photonView.RPC("OnHitOther", RpcTarget.All, closest);
                lastTimeParticleSpawned = Time.time;
                
            }
        }

    }

    [PunRPC]
    void OnHitOther(Vector3 particlePos) {
        Destroy(Instantiate(particle, particlePos, Quaternion.identity), 1.0F);
        groundHit.Play();
    }

}
