using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider effectsSlider;
    

    public void OnChange_MusicSlider()
    {
        SoundManager.Instance.musicSource.volume = musicSlider.value;
    }
    public void OnChange_EffectsSlider()
    {
        SoundManager.Instance.effectsSource.volume = effectsSlider.value;
    }

    public void Quit(){
        Application.Quit();
    }
}
