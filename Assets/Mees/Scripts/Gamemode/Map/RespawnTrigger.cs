using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RespawnTrigger : MonoBehaviourPunCallbacks {

    public float stunTime = 2.0F;
    public GameObject particles, deathParticle;
    public string mapSoundName;

    private Sound respawnSound;
    private Sound soundBeforeRespawn;

    GameObject spawned;

    private void Start() {
        respawnSound = SoundManager.Instance.GetSoundByName("Character Select");
        soundBeforeRespawn = SoundManager.Instance.GetSoundByName(mapSoundName);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && GameModeManager.manager.gameStarted) {
            PhotonView pv = other.GetComponent<PhotonView>();
            if (pv.IsMine) {

                bool soundNull = soundBeforeRespawn == null;
                if (!soundNull) {
                    photonView.RPC("PlayRespawnSound", RpcTarget.All);
                }

                PhotonNetwork.Instantiate(deathParticle.name, other.gameObject.transform.position, Quaternion.identity);

                photonView.RPC("SetPlayerEnabled", RpcTarget.All, pv.ViewID, false);
                StartCoroutine(Respawn(pv, soundNull ? 2 : soundBeforeRespawn.clip.length));

                TailGrabber tailGrabber = GameModeManager.manager.currentGameMode.GetComponentInChildren<TailGrabber>();

                if (tailGrabber != null && tailGrabber.gameObject.activeSelf && other.gameObject == tailGrabber.currentTailHolder) {
                    tailGrabber.OnCurrentHolderDeath();
                }

                Brawl brawl = GameModeManager.manager.currentGameMode.GetComponentInChildren<Brawl>();
                if (brawl != null && brawl.gameObject.activeSelf) {
                    brawl.OnDeath(pv.ViewID);
                }
            }
        }
    }

    [PunRPC]
    private void PlaySound() {
        respawnSound.Play();
    }

    [PunRPC]
    private void PlayRespawnSound() {
        soundBeforeRespawn.Play();
    }

    [PunRPC]
    private void SetPlayerEnabled(int id, bool active) {
        PhotonView.Find(id).gameObject.SetActive(active);
    }

    IEnumerator Respawn(PhotonView pv, float delay) {
        yield return new WaitForSeconds(delay);

        MapInfo info = GameModeManager.manager.currentMap.GetComponent<MapInfo>();
        RespawnBlink blink = pv.GetComponent<RespawnBlink>();

        bool canPos = false;
        while (!canPos)
        {
            canPos = true;

            Transform pos = info.spawnPositions[Random.Range(0, info.spawnPositions.Length)];
            foreach (GameObject otherPlayer in GameModeManager.manager.players)
            {
                if (otherPlayer)
                {
                    if (Vector3.Distance(otherPlayer.transform.position, pos.position) < 3)
                        canPos = false;
                }
            }

            if (canPos)
            {
                photonView.RPC("SetPlayerEnabled", RpcTarget.All, pv.ViewID, true);
                pv.transform.position = pos.position;
                blink.photonView.RPC("StartAnimation", RpcTarget.All, true, stunTime, 1.0f, true);
                pv.GetComponent<LessKnockback>().enabled = false;
                pv.GetComponent<Movement>().ApplySpeedBoost(0, stunTime);
                pv.GetComponent<Rigidbody>().velocity = Vector3.zero;

                spawned = PhotonNetwork.Instantiate(particles.name, pos.position, Quaternion.identity);
                Invoke(nameof(DestroyEffect), 1.0f);

                if (GameModeManager.manager.gameStarted) {
                    photonView.RPC("PlaySound", RpcTarget.All);
                }
            }

            yield return null;
        }
    }

    void DestroyEffect() {
        PhotonNetwork.Destroy(spawned);
    }
}
