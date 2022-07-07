using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    private Sound sound, sound2;

    private void Start() {
        sound = SoundManager.Instance.GetSoundByName("Walking");
        sound2 = SoundManager.Instance.CreateCopy(sound);
    }

    private void Update() {

    }


}
