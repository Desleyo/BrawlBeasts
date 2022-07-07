using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class ScoreBoardManager : MonoBehaviourPunCallbacks
{
    public static ScoreBoardManager manager;

    public List<GameObject> scoreBoardImages = new List<GameObject>();
    [SerializeField] Transform scoreBoardPanel;

    [Space, SerializeField] public TMP_InputField playerUsername;
    [SerializeField] int usernameCharacterLimit = 10;

    void Awake()
    {
        manager = this;
    }

    void Start()
    {
        playerUsername.characterLimit = usernameCharacterLimit;
    }

    [PunRPC]
    public void AddToScoreBoard(int scoreImageId)
    {
        scoreBoardPanel.gameObject.SetActive(true);

        GameObject newImage = PhotonView.Find(scoreImageId).gameObject;       
        newImage.GetComponent<ScoreImageInfo>().nameText.text = newImage.GetComponent<PhotonView>().Owner.NickName;

        scoreBoardImages.Add(newImage);

        if(scoreBoardImages.Count == PhotonNetwork.PlayerList.Length)
        {
            foreach(GameObject image in scoreBoardImages)
            {
                image.transform.SetParent(scoreBoardPanel);
                image.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            }
        }
    }

    public void ResetScoreBoard()
    {
        if(scoreBoardImages.Count != 0)
        {
            foreach (GameObject image in scoreBoardImages)
            {
                image.GetComponent<ScoreImageInfo>().scoreText.text = "0";
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        foreach(GameObject image in scoreBoardImages)
        {
            if (image.GetComponent<PhotonView>().OwnerActorNr == otherPlayer.ActorNumber)
            {
                scoreBoardImages.Remove(image);
                PhotonNetwork.DestroyPlayerObjects(otherPlayer);
            }
        }
    }
}
