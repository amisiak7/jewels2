using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (AudioListener))]
[RequireComponent (typeof (AudioSource))]
public class AudioAssistant : MonoBehaviour {

	public static AudioAssistant main;

	AudioSource music;
	AudioSource sfx;

	public float musicVolume = 1f;

	public AudioClip menuMusic;
	public AudioClip fieldMusic;

	// Arrays with sounds
	public AudioClip[] chipHit;
	public AudioClip[] chipCrush;
	public AudioClip[] swapSuccess;
	public AudioClip[] swapFailed;
	public AudioClip[] bombCrush;
	public AudioClip[] crossBombCrush;
	public AudioClip[] colorBombCrush;
	public AudioClip[] createBomb;
	public AudioClip[] createCrossBomb;
	public AudioClip[] createColorBomb;
	public AudioClip[] timeWarrning;
	public AudioClip[] youWin;
	public AudioClip[] youLose;
	public AudioClip[] buy;
	
	static Dictionary<string, AudioClip[]> data = new Dictionary<string, AudioClip[]>();
	static List<string> mixBuffer = new List<string>();
	static float mixBufferClearDelay = 0.05f;

    public bool mute = false;
    string currentTrack;

	void Awake () {
		main = this;


		AudioSource[] sources = GetComponents<AudioSource> ();
		music = sources[0];
		sfx = sources[1];

		// Filling the dictionary of sounds
		data.Clear ();
		data.Add ("ChipHit", chipHit);
		data.Add ("ChipCrush", chipCrush);
		data.Add ("BombCrush", bombCrush);
		data.Add ("CrossBombCrush", crossBombCrush);
		data.Add ("ColorBombCrush", colorBombCrush);
		data.Add ("SwapSuccess", swapSuccess);
		data.Add ("SwapFailed", swapFailed);
		data.Add ("CreateBomb", createBomb);
		data.Add ("CreateCrossBomb", createCrossBomb);
		data.Add ("CreateColorBomb", createColorBomb);
		data.Add ("TimeWarrning", timeWarrning);
		data.Add ("YouWin", youWin);
		data.Add ("YouLose", youLose);
		data.Add ("Buy", buy);

		StartCoroutine (MixBufferRoutine ());
        
        mute = PlayerPrefs.GetInt("Mute") == 1;
        FindObjectOfType<MuteIcon>().UpdateIcon();
	}

	// Coroutine responsible for limiting the frequency of playing sounds
	IEnumerator MixBufferRoutine() {
        float time = 0;

		while (true) {
            time += Time.unscaledDeltaTime;
            yield return 0;
            if (time >= mixBufferClearDelay) {
			    mixBuffer.Clear();
                time = 0;
            }
		}
	}

	// Launching a music track
	public void PlayMusic(string track) {
        if (track != "")
            currentTrack = track;
		AudioClip to = null;
		switch (track) {
		case "Menu": to = main.menuMusic; break;
		case "Field": to = main.fieldMusic; break;
		}
        StartCoroutine(main.CrossFade(to));
	}

	// A smooth transition from one to another music
	IEnumerator CrossFade(AudioClip to) {
		float delay = 1f;
		if (music.clip != null) {
			while (delay > 0) {
				music.volume = delay * musicVolume;
				delay -= Time.unscaledDeltaTime;
				yield return 0;
			}
		}
		music.clip = to;
        if (to == null || mute) {
            music.Stop();
            yield break;
        }
        delay = 0;
		if (!music.isPlaying) music.Play();
		while (delay < 1) {
			music.volume = delay * musicVolume;
			delay += Time.unscaledDeltaTime;
			yield return 0;
		}
		music.volume = musicVolume;
	}

	// A single sound effect
	public static void Shot(string clip) {
		if (data.ContainsKey (clip) && !mixBuffer.Contains(clip)) {
			if (data[clip].Length == 0) return;
			mixBuffer.Add(clip);
			main.sfx.PlayOneShot(data[clip][Random.Range(0, data[clip].Length)]);
		}
	}

    // Turn on/off music
    public void MuteButton() {
        mute = !mute;
        PlayerPrefs.SetInt("Mute", mute ? 1 : 0);
        PlayerPrefs.Save();
        PlayMusic(mute ? "" : currentTrack);
    }
}
