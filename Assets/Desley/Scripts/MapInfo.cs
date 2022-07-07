using UnityEngine;

public class MapInfo : MonoBehaviour
{
    public GameObject moleSpawnsParent;
    public GameObject itemSpawnsParent;

    public Transform[] spawnPositions;
    public Transform[] moleSpawnPositions;
    public Transform[] circlePositions;

    public GameObject pickupTail;
    public GameObject boulder;

    [Space]
    public float minMapClampX;
    public float maxMapClampX;
    public float minMapClampZ;
    public float maxMapClampZ;

    [Space]
    public float minBoulderClampX;
    public float maxBoulderClampX;
    public float minBoulderClampZ;
    public float maxBoulderClampZ;
}
