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
	PLAY_SCENECHANGE
}

public class AudioController : MonoBehaviour
{
	public AudioClip impactAudio;
	public AudioClip whackAudio;
	public AudioClip swingAudio;
	public AudioClip timeUpAudio;
	public AudioClip victoryAudio;
	public AudioClip sceneChangeAudio;

	[SerializeField]
	private AudioSource oneShotSource;
	[SerializeField]
	private AudioSource timeDangerSource;

	public void PlayEvent(AudioEvent audio)
	{
		switch(audio)
		{
			case AudioEvent.PLAY_IMPACT:
				oneShotSource.PlayOneShot(impactAudio);
				break;
			case AudioEvent.PLAY_WHACK:
				oneShotSource.PlayOneShot(whackAudio);
				break;
			case AudioEvent.PLAY_SWING:
				oneShotSource.PlayOneShot(swingAudio);
				break;
			case AudioEvent.PLAY_TIMEUP:
				oneShotSource.PlayOneShot(timeUpAudio);
				break;
			case AudioEvent.START_TIMEDANGER:
				timeDangerSource.Play();
				break;
			case AudioEvent.STOP_TIMEDANGER:
				timeDangerSource.Stop();
				break;
			case AudioEvent.PLAY_VICTORY:
				oneShotSource.PlayOneShot(victoryAudio);
				break;
			case AudioEvent.PLAY_SCENECHANGE:
				oneShotSource.PlayOneShot(sceneChangeAudio);
				break;
		}
	}
}
