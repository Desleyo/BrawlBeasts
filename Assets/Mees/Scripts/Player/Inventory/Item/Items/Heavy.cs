using UnityEngine;

[CreateAssetMenu(fileName = ("Consumable"), menuName = "Item/New Heavy")]
public class Heavy : ItemConsumable {

    public float duration = 5.0F;
    public float kbMultiplier = 0.5F;

    protected override void OnUseItem(GameObject user) {
        user.GetComponent<LessKnockback>().ApplyEffect(duration, kbMultiplier);
    }

}
