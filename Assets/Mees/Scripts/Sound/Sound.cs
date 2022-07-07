using UnityEngine;

[CreateAssetMenu(menuName = "Sound")]
public class Sound : ScriptableObject {

    public string name;
    public AudioClip clip;
    public AudioSource source;
    public Type audioType;

    public void Play() {
        if (!source.isPlaying) {
            source.Play();
        }
    }

    public void Stop() {
        //nullcheck for sceneload
        if (source != null) {
            source.Stop();
        } else {
            Debug.LogError("Source is null? - " + name);
        }
    }

    public enum Type {
        Master, SoundFX, Music
    }

}