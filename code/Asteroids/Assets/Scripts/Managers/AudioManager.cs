using UnityEngine;

/// <summary>
/// Class to manage the audio in the game
/// </summary>
public class AudioManager : Singleton<AudioManager>
{
    [SerializeField]
    private AudioClip _inGameClip;

	private AudioSource _audioSource;
    
	private void Start()
	{
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = _inGameClip;
        _audioSource.Play();
	}

    /// <summary>
    /// Plays one time a audioclip
    /// </summary>
    /// <param name="audioClip">The audio clip to be played</param>
	public void Play(AudioClip audioClip)
	{
        _audioSource.PlayOneShot(audioClip);
	}
}
