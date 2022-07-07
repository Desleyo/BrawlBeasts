using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour {

    private static SoundManager _instance;

    public List<Sound> sounds;

    public Dictionary<Sound.Type, float> volumes = new Dictionary<Sound.Type, float>();

    //theme song transition
    private Sound currentMusic, previousMusic;
    public bool musicTransition = false;
    private float musicStepSize;

    public float musicTransitionTime = 2.0F;


    public static SoundManager Instance {
        get {
            return _instance;
        }
    }

    private void Awake() {
        if (_instance == null) {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            foreach (Sound.Type type in Enum.GetValues(typeof(Sound.Type))) {
                volumes.Add(type, 1.0F);
            }

            foreach (Sound sound in sounds) {
                GameObject soundObj = new GameObject(sound.name);
                AudioSource audioSource = soundObj.AddComponent<AudioSource>();
                audioSource.clip = sound.clip;
                audioSource.loop = sound.audioType == Sound.Type.Music;

                soundObj.transform.parent = gameObject.transform;
                sound.source = soundObj.GetComponent<AudioSource>();
            }

            ApplyVolume(0.1F, 1.0F, 1.0F);
        }
    }

    private void Update() {
        if (musicTransition) {
            if (previousMusic != null && previousMusic.source.isPlaying) {
                previousMusic.source.volume = Mathf.Max(previousMusic.source.volume - (musicStepSize * Time.deltaTime), 0.0F);
                if (previousMusic.source.volume == 0.0F) {
                    previousMusic.Stop();
                }
            } else {
                float volume = volumes[Sound.Type.Music] * volumes[Sound.Type.Master];
                currentMusic.source.volume = Mathf.Min(currentMusic.source.volume + (musicStepSize * Time.deltaTime), volume);

                if (currentMusic.source.volume == volume) {
                    musicTransition = false;
                }
            }

        }

    }

    public void SetCurrentMusic(Sound music) {
        Debug.Log("SetCurrentMusic - " + music.name);
        if (music != currentMusic) {
            previousMusic = currentMusic;
            currentMusic = music;

            float musicVolume = volumes[Sound.Type.Music] * (float)volumes[Sound.Type.Master];
            musicStepSize = musicVolume / musicTransitionTime;
            currentMusic.source.volume = 0.0F;

            currentMusic.Play();

            musicTransition = true;
        }
    }

    public Sound GetSoundByName(string name) {
        Sound sound = sounds.Find(s => s.name == name);

        if (sound == null) {
            Debug.Log("Couldn't find a sound named " + name + ".");
        }

        return sound;
    }

    public void PlaySoundByName(string soundName) {
        Sound sound = GetSoundByName(soundName);
        if (sound != null) {
            sound.source.Play();
        }
    }

    public void PlayNewSoundByName(string soundName) {
        Sound sound = GetSoundByName(soundName);
        if (sound != null) {
            GameObject obj = Instantiate(sound.source.gameObject);
            obj.transform.parent = gameObject.transform;
            Destroy(obj, sound.clip.length);
        }
    }

    public void ApplyVolume(float master, float music, float fx) {
        volumes[Sound.Type.Master] = master;
        volumes[Sound.Type.Music] = music;
        volumes[Sound.Type.SoundFX] = fx;

        foreach (Sound sound in sounds) {
            switch (sound.audioType) {
                case Sound.Type.Master:
                    sound.source.volume = master;
                    break;
                case Sound.Type.Music:
                    sound.source.volume = music * master;
                    break;
                case Sound.Type.SoundFX:
                    sound.source.volume = fx * master;
                    break;
            }
        }
    }

    public float GetVolumeByType(Sound.Type type) {
        return volumes[type];
    }

    public Sound CreateCopy(Sound toCopy) {
        Sound sound = (Sound)ScriptableObject.CreateInstance("Sound");

        GameObject soundObj = new GameObject(toCopy.name);
        AudioSource audioSource = soundObj.AddComponent<AudioSource>();
        audioSource.clip = toCopy.clip;
        audioSource.loop = toCopy.audioType == Sound.Type.Music;
        audioSource.volume = toCopy.source.volume;

        soundObj.transform.parent = gameObject.transform;

        sound.name = toCopy.name + "_" + Time.time;
        sound.audioType = toCopy.audioType;
        sound.clip = toCopy.clip;
        sound.source = soundObj.GetComponent<AudioSource>();

        sounds.Add(sound);
        return sound;
    }

}