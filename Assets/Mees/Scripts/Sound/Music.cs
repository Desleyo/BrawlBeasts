using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour {

    public string songName;
    public bool waitForLoading;
    private Sound song;

    private void Awake() {
        song = SoundManager.Instance.GetSoundByName(songName);

    }


    private void OnEnable() {
        float time = GameModeManager.manager.TimeLeftUntilDone();
        if (waitForLoading && time != GameModeManager.manager.transitionTime && time != 0) {
            Invoke(nameof(SetCurrentSong), time - SoundManager.Instance.musicTransitionTime);
        } else {
            SetCurrentSong();
        }
    }

    private void SetCurrentSong() {
        SoundManager.Instance.SetCurrentMusic(song);
    }

}
