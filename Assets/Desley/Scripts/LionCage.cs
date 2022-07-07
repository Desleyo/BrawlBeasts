using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LionCage : MonoBehaviourPunCallbacks
{

    [HideInInspector] public int activatorID;
    [SerializeField] Animator lionCageDoorAnimator;
    [SerializeField] Animator lionCageArmAnimator;
    [SerializeField] float cageOpenTime = .5f;
    [SerializeField] float waitForAnimation = .3f;
    [SerializeField] float cageCloseTime = .5f;
    [SerializeField] float hitPower = 25f;
    bool cageIsOpen;

    List<Collider> alreadyHit;

    private Sound growl;

    [PunRPC]
    public void StartSequence(int activatorID)
    {
        this.activatorID = activatorID;
        StartCoroutine(OpenLionCage());
    }

    private void Start() {
        growl = SoundManager.Instance.GetSoundByName("Growl");
    }

    public IEnumerator OpenLionCage()
    {
        alreadyHit = new List<Collider>();
        lionCageDoorAnimator.SetTrigger("DoorUp");

        yield return new WaitForSeconds(cageOpenTime);

        lionCageArmAnimator.SetTrigger("Attack");
        growl.Play();

        yield return new WaitForSeconds(waitForAnimation);

        cageIsOpen = true;

        yield return new WaitForSeconds(cageCloseTime);

        lionCageDoorAnimator.SetTrigger("DoorDown");
        cageIsOpen = false;
    }

    public void OnTriggerStay(Collider other)
    {
        if (!cageIsOpen || !other.gameObject.CompareTag("Player"))
            return;

        if (other.gameObject.GetComponent<PhotonView>().IsMine && !alreadyHit.Contains(other)) {
            other.gameObject.GetComponent<Movement>().photonView.RPC("ApplyKnockback", RpcTarget.All, photonView.ViewID, hitPower);
            Brawl brawl = GameModeManager.manager.currentGameMode.GetComponentInChildren<Brawl>();
            if (brawl != null && brawl.gameObject.activeSelf) {
                brawl.photonView.RPC("OnCombatTag", RpcTarget.All, other.GetComponent<PhotonView>().ViewID, activatorID);

            }

            alreadyHit.Add(other);
        }
    }
}
