using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Boulder : MonoBehaviourPunCallbacks
{
    Rigidbody rb;

    [SerializeField] GameObject shadowPrefab;
    GameObject currentShadow;
    [SerializeField] float shadowIncreaseSize = .1f;
    Vector3 shadowSize;

    [Space, SerializeField] float maxFallSpeed = 10f;
    [SerializeField] float fallSpeedAcceleration = 2f;
    float fallSpeed = 1f;

    [Space, SerializeField] LayerMask groundLayer;

    private Sound sound;

    private void Start()
    {
        sound = SoundManager.Instance.CreateCopy(SoundManager.Instance.GetSoundByName("Falling Rock"));
        Destroy(sound.source.gameObject, sound.clip.length);
        sound.Play();
        rb = GetComponent<Rigidbody>();
        
        PlaceShadow();  
    }

    void PlaceShadow()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down, out hit, 100f, groundLayer))
        {
            GameObject newShadow = PhotonNetwork.Instantiate(shadowPrefab.name, hit.point, Quaternion.identity);

            photonView.RPC("SetShadowReference", RpcTarget.All, newShadow.GetComponent<PhotonView>().ViewID);
        }
    }

    [PunRPC]
    void SetShadowReference(int shadowId)
    {
        currentShadow = PhotonView.Find(shadowId).gameObject;

        shadowSize = currentShadow.transform.localScale;
    }

    void Update()
    {
        if (currentShadow)
        {
            shadowSize += new Vector3(shadowIncreaseSize, 0, shadowIncreaseSize) * Time.deltaTime;
            currentShadow.transform.localScale = shadowSize;
        }

        fallSpeed += Time.deltaTime * fallSpeedAcceleration;
        fallSpeed = Mathf.Clamp(fallSpeed, 1, maxFallSpeed);

        if (PhotonNetwork.IsMasterClient)
            rb.velocity = new Vector3(0, -fallSpeed, 0);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            PhotonView pv = collision.gameObject.GetComponent<PhotonView>();
            if(!pv.IsMine)
                return;

            GameModeManager.manager.currentGameMode.GetComponent<SeekShelter>().photonView.RPC("PlayerHit", RpcTarget.All, pv.ViewID);
        }
        else if (collision.gameObject.layer == 8 && photonView.IsMine)
        {  
            PhotonNetwork.Destroy(currentShadow);
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
