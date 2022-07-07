using UnityEngine;

public abstract class ItemConsumable : Item {

    public override bool UseItem(GameObject user) {
        this.OnUseItem(user);
        return true;

    }

}
