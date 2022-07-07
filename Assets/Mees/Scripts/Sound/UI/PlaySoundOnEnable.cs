using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnEnable : MonoBehaviour {

    public string soundName;

    private Sound sound;

    private void Awake() {
        sound = SoundManager.Instance.GetSoundByName(soundName);
    }

    private void OnEnable() {
        sound.Play();
        Debug.Log("played " + soundName);
    }
}
