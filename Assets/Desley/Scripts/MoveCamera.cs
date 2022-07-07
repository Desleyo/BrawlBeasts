using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MoveCamera : MonoBehaviourPunCallbacks
{
    public static MoveCamera manager;
    [SerializeField] GameObject characterSelectPlane;
    [SerializeField] Transform cameraGamePos;
    [SerializeField] Camera childCam;
    Camera cam;

    [Space, SerializeField] float positionSmoothness = 10f;
    [SerializeField] float zoomSmoothness = 5f;
    [SerializeField] float offsetZ = 25;
    [SerializeField] float minFov = 50f;
    [SerializeField] float maxFov = 70f;

    float minClampX;
    float maxClampX;
    float minClampZ;
    float maxClampZ;
    
    [Header("Debugging")]
    public GameObject[] players;
    public Vector3 midPoint;

    private void Awake()
    {
        manager = this;
        cam = GetComponent<Camera>();
    }

    public IEnumerator SetCameraPos()
    {
        yield return new WaitForSeconds(3);

        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player.GetComponent<PhotonView>().IsMine)
                PhotonNetwork.Destroy(player);
        }

        characterSelectPlane.SetActive(false);

        transform.position = cameraGamePos.position;
        transform.rotation = cameraGamePos.rotation;
        cam.fieldOfView = 60f;
    }

    private void FixedUpdate()
    {
        if (GameModeManager.manager.gameStarted && PhotonNetwork.IsMasterClient)
        {
            players = GameObject.FindGameObjectsWithTag("Player");

            if (players.Length == 0)
                return; 

            midPoint = new Vector3(0, 0, 0);
            foreach(GameObject player in players)
            {
                if (player && player.activeSelf)
                    midPoint += player.transform.position;
            }

            //Move camera towards the middle of all players
            Vector3 midPos = new Vector3(midPoint.x / players.Length, transform.position.y, midPoint.z / players.Length - offsetZ);
            Vector3 smoothPos = Vector3.Lerp(transform.position, midPos, positionSmoothness * Time.deltaTime);
            transform.position = smoothPos;

            //Clamp the camera according to map boundairies
            float clampedX = Mathf.Clamp(transform.position.x, minClampX, maxClampX);
            float clampedZ = Mathf.Clamp(transform.position.z, minClampZ, maxClampZ);
            transform.position = new Vector3(clampedX, transform.position.y, clampedZ);

            //Zoom In/Out according to the distance to each player
            float cameraFov = 60f;
            if (players.Length > 1)
            {
                float distance = Vector3.Distance(midPoint / players.Length, players[0].transform.position);
                float clampedDistance = Mathf.Clamp(distance, 0, 10);
                float smoothFov = Mathf.Lerp(cam.fieldOfView, 50f + distance, zoomSmoothness * Time.deltaTime);
                cameraFov = smoothFov;
            }
            else if (cam.fieldOfView != 60f)
                cameraFov = 60f;

            photonView.RPC("CameraFovNetworking", RpcTarget.All, cameraFov);
        }
        else if(!characterSelectPlane)
        {
            transform.position = Vector3.Lerp(transform.position, cameraGamePos.position, positionSmoothness * Time.deltaTime);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, maxFov, zoomSmoothness * Time.deltaTime);
        }
    }

    [PunRPC]
    void CameraFovNetworking(float fov)
    {
        cam.fieldOfView = fov;
        cam.fieldOfView = childCam.fieldOfView = Mathf.Clamp(cam.fieldOfView, 50f, maxFov);
    }

    public void SetCameraBoundaries(MapInfo currentMapInfo)
    {
        minClampX = currentMapInfo.minMapClampX;
        maxClampX = currentMapInfo.maxMapClampX;
        minClampZ = currentMapInfo.minMapClampZ;
        maxClampZ = currentMapInfo.maxMapClampZ;
    }
}
