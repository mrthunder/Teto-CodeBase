using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SettingsScreen : UIScreen
{

    public const string m_SFXVolumeKey = "sfxVolume";
    public const string m_MusicVolumeKey = "musicVolume";
    public const string m_BGVolumeKey = "bgVolume";
    public const string m_XInvertKey = "cameraInvertX";
    public const string m_YInvertKey = "cameraInvertY";


    public bool cameraInvertX = false;
    public bool cameraInvertY = false;
    float sfxVolume = 0;
    float musicVolume = 0;
    float bgVolume = 0;

    public Slider m_SFX;
    public Slider m_BG;
    public Slider m_Music;
    public Toggle m_xInvert;
    public Toggle m_YInvert;


    void Awake()
    {
        sfxVolume = PlayerPrefs.GetFloat(m_SFXVolumeKey,1);
      
        m_SFX.value = sfxVolume;

        musicVolume = PlayerPrefs.GetFloat(m_MusicVolumeKey,1);
       
        m_Music.value = musicVolume;
        bgVolume = PlayerPrefs.GetFloat(m_BGVolumeKey,1);
        
        m_BG.value = bgVolume;
        m_xInvert.isOn = (PlayerPrefs.GetInt(m_XInvertKey,1) == 1 ? true : false);
        m_YInvert.isOn = (PlayerPrefs.GetInt(m_YInvertKey,0) == 1 ? true : false);
        SoundManager.Instance.SetGameParameter(GameParameters.SFX, m_SFX.value);
        SoundManager.Instance.SetGameParameter(GameParameters.Music, m_Music.value);
        SoundManager.Instance.SetGameParameter(GameParameters.BG, m_BG.value);
    }

    public override void ShowScreen()
    {
       
    }

    public void SFXVolume(float val)
    {
        val = Mathf.Clamp01(val);
        sfxVolume = val;
        SoundManager.Instance.SetGameParameter(GameParameters.SFX, val);
    }

    public void MusicVolume(float val)
    {
        val = Mathf.Clamp01(val);
        musicVolume = val;
        SoundManager.Instance.SetGameParameter(GameParameters.Music, val);
    }

    public void BGVolume(float val)
    {
        val = Mathf.Clamp01(val);
        bgVolume = val;
        SoundManager.Instance.SetGameParameter(GameParameters.BG, val);
    }

    public void CameraInvertX(bool val)
    {
        cameraInvertX = val;
    }

    public void CameraInvertY(bool val)
    {
        cameraInvertY = val;
    }


    public void OnBackButton()
    {
        PlayerPrefs.SetFloat(m_SFXVolumeKey, sfxVolume);
        PlayerPrefs.SetFloat(m_MusicVolumeKey, musicVolume);
        PlayerPrefs.SetFloat(m_BGVolumeKey, bgVolume);
        PlayerPrefs.SetInt(m_XInvertKey, (cameraInvertX ? 1 : 0));
        PlayerPrefs.SetInt(m_YInvertKey, (cameraInvertY ? 1 : 0));
        ScreenManager.Instance.Show<MenuScreen>();

    }
}
