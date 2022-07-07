using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    private Button button;
    private Sound sound;

    void Start() {
        button = GetComponent<Button>();
        if (sound == null) {
            sound = SoundManager.Instance.GetSoundByName("Button Press");
        }
        button.onClick.AddListener(() => sound.source.Play());
    }


}
