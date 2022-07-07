using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BrawlHammerHit : MonoBehaviourPunCallbacks {

    public float stunTime = 2.0F;
    public Inventory inv;
    public ItemReusable reusable;
    public GameObject particle;

    private GameObject spawned;
    private Collider collider;

    private float lastTimeParticleSpawned;

    private Sound hitSound;

    private void Start()
    {
        inv = gameObject.transform.root.GetComponent<Inventory>();
        reusable = inv.currentItem as ItemReusable;
        hitSound = SoundManager.Instance.GetSoundByName("Ulti Hammer Swing");

        Physics.IgnoreCollision(collider = GetComponent<Collider>(), inv.GetComponent<Collider>());
    }


    private void OnTriggerEnter(Collider other)
    {
        if(photonView.IsMine)
        {
            if (!reusable.CanUseItem())
            {
                photonView.RPC("PlaySound", RpcTarget.All);
                inv.SetCurrentItem(null);
                Vector3 closest = inv.transform.position + inv.transform.forward;
                spawned = PhotonNetwork.Instantiate(particle.name, closest, Quaternion.identity);
                Invoke(nameof(DestroyEffect), 1.0f);


                foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
                {
                    if (player != inv.gameObject)
                    {
                        player.GetComponent<Movement>().photonView.RPC("ApplyStun", RpcTarget.All, stunTime);
                        player.GetComponent<Inventory>().photonView.RPC("DropItem", RpcTarget.All);

                        Brawl brawl = GameModeManager.manager.currentGameMode.GetComponentInChildren<Brawl>();
                        if (brawl != null && brawl.gameObject.activeSelf)
                        {
                            brawl.photonView.RPC("OnCombatTag", RpcTarget.All, player.GetComponent<PhotonView>().ViewID, transform.root.GetComponent<PhotonView>().ViewID);
                        }

                    }
                }
            }
        }

    }

    [PunRPC]
    void PlaySound() {
        hitSound.Play();
    }



    void DestroyEffect() {
        PhotonNetwork.Destroy(spawned);
    }
}
