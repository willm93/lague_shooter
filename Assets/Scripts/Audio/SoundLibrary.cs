using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundLibrary : MonoBehaviour
{
    public SoundGroup[] soundGroups;
    Dictionary<string, AudioClip[]> library = new Dictionary<string, AudioClip[]>();

    void Awake()
    {
        foreach(SoundGroup soundGroup in soundGroups){
            library.Add(soundGroup.groupName, soundGroup.sounds);
        }
    }

    public AudioClip GetClipByName(string name)
    {
        if(library.ContainsKey(name)){
            AudioClip[] sounds = library[name];
            return sounds[Random.Range(0, sounds.Length)];
        }
        return null;
    }

    [System.Serializable]
    public class SoundGroup {
        public string groupName;
        public AudioClip[] sounds;
    }
}
