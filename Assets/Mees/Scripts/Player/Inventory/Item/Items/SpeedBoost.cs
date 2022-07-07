using UnityEngine;

[CreateAssetMenu(fileName = ("Consumable"), menuName = "Item/New Speedboost")]
public class SpeedBoost : ItemConsumable {

    public float boost = 20.0F;
    public float duration = 2.0F;

    protected override void OnUseItem(GameObject user) {
        SoundManager.Instance.GetSoundByName("Pepper Eat").Play();
        SoundManager.Instance.GetSoundByName("Pepper Active").Play();
        user.GetComponent<Movement>().photonView.RPC("ApplySpeedBoost", Photon.Pun.RpcTarget.All, boost, duration);
    }

}
