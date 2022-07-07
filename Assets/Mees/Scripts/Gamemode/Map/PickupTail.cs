using Photon.Pun;
using UnityEngine;

public class PickupTail : MonoBehaviourPunCallbacks
{

    public GameObject particle;

    private GameObject spawned;
    private Sound pickupTail;

    private void OnEnable() {
        pickupTail = SoundManager.Instance.GetSoundByName("Tail Pick Up");
        //spawned = PhotonNetwork.Instantiate(particle.name, transform.position, Quaternion.identity);
        Invoke(nameof(DestroyEffect), 1.0f);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            PhotonView pv = other.GetComponent<PhotonView>();
            if (pv.IsMine) {
                TailGrabber tail = GameModeManager.manager.currentGameMode.GetComponent<TailGrabber>();
                tail.photonView.RPC("SetCurrentTailHolder",RpcTarget.All ,pv.ViewID);
                photonView.RPC("SetTailActive", RpcTarget.All, false);
            }
        }
    }

    [PunRPC]
    void SetTailActive(bool active) {
        pickupTail.Play();
        gameObject.SetActive(active);
    }

    void DestroyEffect() {
        PhotonNetwork.Destroy(spawned);
    }

}
