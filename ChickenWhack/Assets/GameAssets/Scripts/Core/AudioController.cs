using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AudioEvent {
	PLAY_IMPACT,
	PLAY_WHACK,
	PLAY_SWING,
	PLAY_TIMEUP,
	START_TIMEDANGER,
	STOP_TIMEDANGER,
	PLAY_VICTORY,
	PLAY_BUTTON,
	PLAY_MENUMUSIC,
	PLAY_GAMEMUSIC,
	STOP_MUSIC,
	PLAY_SCENECHANGE,
	PLAY_CHICKENFLY,
}

public class AudioController : MonoBehaviour
{
	[System.Serializable]
	public class AudioAsset
	{
		public AudioClip clip;
		public float volume = 1f;
	}

	public AudioAsset impactAudio;
	public AudioAsset whackAudio;
	public AudioAsset swingAudio;
	public AudioAsset[] chickenFlyAudio;
	public AudioAsset timeDangerAudio;
	public AudioAsset timeUpAudio;
	public AudioAsset loseAudio;
	public AudioAsset cheersAudio;
	public AudioAsset victoryAudio;
	public AudioAsset buttonAudio;
	public AudioAsset sceneChangeAudio;
	public AudioAsset menuMusic;
	public AudioAsset gameMusic;

	[SerializeField]
	private AudioSource musicSource;
	[SerializeField]
	private AudioSource oneShotSource;
	[SerializeField]
	private AudioSource timeDangerSource;

	private void PlayAudioAsset(AudioAsset asset)
	{
		oneShotSource.PlayOneShot(asset.clip, asset.volume);
	}

	public void PlayEvent(AudioEvent audio)
	{
		switch(audio)
		{
			case AudioEvent.PLAY_IMPACT:
				PlayAudioAsset(impactAudio);
				break;
			case AudioEvent.PLAY_WHACK:
				PlayAudioAsset(whackAudio);
				break;
			case AudioEvent.PLAY_SWING:
				PlayAudioAsset(swingAudio);
				break;
			case AudioEvent.PLAY_TIMEUP:
				PlayAudioAsset(loseAudio);
				PlayAudioAsset(timeUpAudio);
				break;
			case AudioEvent.START_TIMEDANGER:
				timeDangerSource.clip = timeDangerAudio.clip;
				timeDangerSource.volume = timeDangerAudio.volume;
				timeDangerSource.Play();
				break;
			case AudioEvent.STOP_TIMEDANGER:
				timeDangerSource.Stop();
				break;
			case AudioEvent.PLAY_VICTORY:
				PlayAudioAsset(cheersAudio);
				PlayAudioAsset(victoryAudio);
				break;
			case AudioEvent.PLAY_BUTTON:
				PlayAudioAsset(buttonAudio);
				break;
			case AudioEvent.PLAY_MENUMUSIC:
				musicSource.Stop();
				musicSource.clip = menuMusic.clip;
				musicSource.volume = menuMusic.volume;
				musicSource.Play();
				break;
			case AudioEvent.PLAY_GAMEMUSIC:
				musicSource.Stop();
				musicSource.clip = gameMusic.clip;
				musicSource.volume = gameMusic.volume;
				musicSource.Play();
				break;
			case AudioEvent.STOP_MUSIC:
				musicSource.Stop();
				break;
			case AudioEvent.PLAY_SCENECHANGE:
				PlayAudioAsset(sceneChangeAudio);
				break;
			case AudioEvent.PLAY_CHICKENFLY:
				PlayAudioAsset(chickenFlyAudio[Random.Range(0, chickenFlyAudio.Length)]);
				break;
		}
	}
}
