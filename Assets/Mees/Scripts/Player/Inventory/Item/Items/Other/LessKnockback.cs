using Photon.Pun;
using UnityEngine;

public class LessKnockback : MonoBehaviourPunCallbacks {

    public PlayerInfo playerInfo;
    public Material ironMaterial;

    public float multiplier;
    public float timeLeft;

    private Material previousBody, previousTail, previousWing;

    private Sound previous, metalic;
    void OnEnable()
    {
        gameObject.GetComponent<Movement>().knockbackMultiplier = multiplier;

        photonView.RPC("UpdateMaterials", RpcTarget.All, true);
        metalic = SoundManager.Instance.CreateCopy(SoundManager.Instance.GetSoundByName("Walking Metalic"));
        if (previous == null) {
            previous = GetComponent<Movement>().walkingSound;
        }

        SoundManager.Instance.GetSoundByName("Metal Buff").Play();
        GetComponent<Movement>().walkingSound = metalic;
    }


    private void OnDisable() {
        GetComponent<Movement>().walkingSound = previous;
        gameObject.GetComponent<Movement>().knockbackMultiplier = 1.0F;

        photonView.RPC("UpdateMaterials", RpcTarget.All, false);
    }

    [PunRPC]
    void UpdateMaterials(bool enabled)
    {
        if (enabled)
        {
            playerInfo = gameObject.GetComponent<PlayerInfo>();

            previousBody = playerInfo.bodyRenderer.material;
            previousTail = playerInfo.tailRenderer.material;

            if (playerInfo.wingRenderer)
            {
                previousWing = playerInfo.wingRenderer.material;
                playerInfo.wingRenderer.material = ironMaterial;
            }

            playerInfo.bodyRenderer.material = playerInfo.tailRenderer.material = ironMaterial;
        }
        else
        {
            playerInfo.bodyRenderer.material = previousBody;
            playerInfo.tailRenderer.material = previousTail;

            if (playerInfo.wingRenderer)
                playerInfo.wingRenderer.material = previousWing;
        }
    }

    void Update()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0) {
            this.enabled = false;
        }
    }

    public void ApplyEffect(float time, float multiplier) {
        this.timeLeft = time;
        this.multiplier = multiplier;
        this.enabled = true;
    }

}
