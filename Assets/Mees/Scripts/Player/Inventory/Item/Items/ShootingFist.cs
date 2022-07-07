using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = ("Reusable"), menuName = "Item/New Fist")]
public class ShootingFist : ItemReusable {

    private ShootFist fist;

    protected override void OnUseItem(GameObject user) {
        if (fist == null) {
            fist = user.transform.root.GetComponentInChildren<ShootFist>();
        }

        fist.photonView.RPC("Use", Photon.Pun.RpcTarget.All);
        fist.timeWhenUsed = Time.time;
    }
}
