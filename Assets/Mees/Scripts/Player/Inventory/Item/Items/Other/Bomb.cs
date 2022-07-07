using Photon.Pun;
using UnityEngine;

public class Bomb : MonoBehaviourPunCallbacks {

    public float bombPower = 50.0F;
    public float rayDist = 15;

    public GameObject bombParticle;
    public Transform middle;

    public float timeLeft;

    private float start;

    private MeshRenderer meshRenderer;
    private Color startColor;

    private Sound bombFuse, bombExplode;

    private void Start() {


        this.bombFuse = SoundManager.Instance.CreateCopy(SoundManager.Instance.GetSoundByName("Bomb Fuse"));
        this.bombExplode = SoundManager.Instance.CreateCopy(SoundManager.Instance.GetSoundByName("Bomb Explode"));

        this.bombFuse.Play();

        this.start = timeLeft;
        this.meshRenderer = GetComponent<MeshRenderer>();
        this.startColor = meshRenderer.material.color;
    }

    private void Update() {
        timeLeft -= Time.deltaTime;
        this.meshRenderer.material.color = ColorUtils.combineColors(startColor, Color.red, 1 - (1 / start * timeLeft));
        if (timeLeft <= 0) {
            bombExplode.Play();
            bombFuse.Stop();
            Destroy(bombFuse);
            Destroy(bombExplode, bombExplode.clip.length);
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player")) {
                if (obj.GetComponent<PhotonView>().IsMine) {
                    middle.LookAt(obj.transform);
                    if (Physics.Raycast(middle.position, middle.forward, out RaycastHit hit, rayDist)) {
                        Debug.Log(hit.transform.gameObject);
                        if (hit.transform.root.gameObject == obj) {
                            obj.GetComponent<Movement>().ApplyKnockback(photonView.ViewID, bombPower);
                        }
                    }
                    Destroy(Instantiate(bombParticle, transform.position, Quaternion.identity), 3);
                    Destroy(gameObject);
                }
            }
        }
    }


}
