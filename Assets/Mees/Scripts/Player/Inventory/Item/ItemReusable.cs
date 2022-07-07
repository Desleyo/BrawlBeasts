using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemReusable : Item {

    public float cooldown;

    private float lastTimeUsed = 0;

    private void OnEnable() {
        lastTimeUsed = Time.time;
    }

    public override bool UseItem(GameObject user) {
        //Debug.Log(Time.time + " - " + lastTimeUsed);
        if (this.CanUseItem()) {
            this.OnUseItem(user);
            lastTimeUsed = Time.time;
        }
        return false;
    }

    public bool CanUseItem() {
        return Time.time - lastTimeUsed >= cooldown;
    }

}
