using Photon.Pun;
using UnityEngine;

[CreateAssetMenu(fileName = ("Consumable"), menuName = "Item/New Bomb")]
public class ThrowBomb : ItemConsumable {

    public float height = 5.0F;

    private float power = 10.0F;

    protected override void OnUseItem(GameObject user) {
        GameObject obj = PhotonNetwork.Instantiate("Bomb", user.transform.position + (user.transform.forward * 1.5F), user.transform.rotation);

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        Vector3 direction = user.transform.forward;
        direction *= power;
        direction.y = height;
        rb.velocity = direction;
    }

}
