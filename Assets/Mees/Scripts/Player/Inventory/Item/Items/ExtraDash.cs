using UnityEngine;

[CreateAssetMenu(fileName = ("Consumable"), menuName = "Item/New Dash")]
public class ExtraDash : ItemConsumable
{
    protected override void OnUseItem(GameObject user) {
        user.GetComponent<Movement>().DoDash(false, true);
    }

}
