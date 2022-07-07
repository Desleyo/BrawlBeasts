using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundSettings : MonoBehaviour
{

    public Slider masterVolume, musicVolume, fxVolume;

    public void SetCurrentSliderValues() {
        SoundManager soundManager = SoundManager.Instance;
        masterVolume.value = soundManager.GetVolumeByType(Sound.Type.Master);
        musicVolume.value = soundManager.GetVolumeByType(Sound.Type.Music);
        fxVolume.value = soundManager.GetVolumeByType(Sound.Type.SoundFX);
    }

    public void ApplySettings() {
        SoundManager.Instance.ApplyVolume(masterVolume.value, musicVolume.value, fxVolume.value);
    }

}
