using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSplash : MonoBehaviour
{

    public GameObject splashParticle;
    private Sound splash;

    private void Start() {
        splash = SoundManager.Instance.GetSoundByName("Water Splash");
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && GameModeManager.manager.gameStarted) {
            Instantiate(splashParticle, other.transform.position, splashParticle.transform.rotation);
            splash.Play();
        }
    }
}
