using UnityEngine;

[CreateAssetMenu(fileName = ("Item"), menuName = "Item/New Item")]
public abstract class Item : ScriptableObject
{
    //item name,desc & icon
    public string itemName, description;

    public GameObject obj;

    public Sprite icon;

    public PickupItem pickupItem;

    public abstract bool UseItem(GameObject user);
    
    protected abstract void OnUseItem(GameObject user);


}
