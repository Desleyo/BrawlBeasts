using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PlayerInfo : MonoBehaviourPunCallbacks
{
    public int characterIndex;

    [Space] public List<GameObject> scoreBoardImages;

    [HideInInspector]public TMP_InputField playerUsername;
    [SerializeField] GameObject scoreImagePrefab;

    [HideInInspector] public GameObject playerScoreImage;

    public GameObject playerTail, propTail, fartParticle;

    [Space, SerializeField] public SkinnedMeshRenderer bodyRenderer;
    [SerializeField] public SkinnedMeshRenderer tailRenderer;
    [SerializeField] public SkinnedMeshRenderer wingRenderer;
    public Material[] bodySkins;
    public Material[] tailSkins;
    public Material[] wingSkins;
    public GameObject[] items;
    int currentSkinIndex;
    int currentItemIndex = -1;

    void Awake()
    {
        playerUsername = GameObject.FindGameObjectWithTag("UsernameInput").GetComponent<TMP_InputField>();

        if(photonView.IsMine)
            playerUsername.text = photonView.Owner.NickName;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        photonView.RPC("UpdateSkin", RpcTarget.All, currentSkinIndex);
        photonView.RPC("UpdateItem", RpcTarget.All, currentItemIndex);
    }

    public void UpdateNickname()
    {
        GetComponent<PhotonView>().Owner.NickName = !string.IsNullOrEmpty(playerUsername.text) ? playerUsername.text : "player";
    }

    [PunRPC]
    void UpdateSkin(int index)
    {
        bodyRenderer.material = bodySkins[index];
        tailRenderer.material = tailSkins[index];

        if(wingRenderer)
            wingRenderer.material = wingSkins[index];

        currentSkinIndex = index;
    }

    [PunRPC]
    void UpdateItem(int index)
    {
        foreach (GameObject item in items)
            item.SetActive(false);

        if (index != -1)
            items[index].SetActive(true);

        currentItemIndex = index;
    }

    public void SetupPlayer(GameObject player)
    {
        UpdateNickname();

        CharacterSelectManager.manager.DisableSelectUI();

        GameObject image = PhotonNetwork.Instantiate(scoreImagePrefab.name, new Vector3(0, 0, 0), Quaternion.identity);
        photonView.RPC("SetScoreImage", RpcTarget.All, photonView.ViewID, image.GetComponent<PhotonView>().ViewID);

        ScoreBoardManager.manager.photonView.RPC("AddToScoreBoard", RpcTarget.All, playerScoreImage.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    void SetScoreImage(int playerId, int imageId)
    {
        GameObject player = PhotonView.Find(playerId).gameObject;
        player.GetComponent<PlayerInfo>().playerScoreImage = PhotonView.Find(imageId).gameObject;
    }
}
