using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CinematicUI : Singleton<CinematicUI> {

    private RawImage m_img;
    private AudioSource m_sound;

    public bool m_bIsPlaying = false;

    public bool m_bIsComplete = false;

	// Use this for initialization
	void Start () {
        m_img = GetComponentInChildren<RawImage>();
        m_sound = GetComponentInChildren<AudioSource>();
        m_img.gameObject.SetActive(false);
	}
	
	public void PlayMovie(MovieTexture movie)
    {
        m_img.gameObject.SetActive(true);
        m_img.texture = (MovieTexture)movie;
        m_sound.clip = movie.audioClip;
        movie.Play();
        m_bIsPlaying = movie.isPlaying;
        
    }

    public IEnumerator OnPlaying(MovieTexture movie, bool closeWindow = true)
    {
        m_img.gameObject.SetActive(true);
        m_img.texture = (MovieTexture)movie;
        m_sound.clip = movie.audioClip;
        m_sound.Play();
        movie.Play();
        m_bIsPlaying = movie.isPlaying;

        yield return new WaitUntil(() => !movie.isPlaying);
        m_img.gameObject.SetActive(!closeWindow);
    }


}
