using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : Menu
{
    public override string menuName {get; protected set;} = "options";

    public Slider[] volumeSliders;
    public Toggle fullscreen;
    public TMP_Dropdown resolution;
    public int[] screenWidths = new int[3];
    int activeScreenResIndex;
    bool isFullscreen;

    void Start()
    {
        activeScreenResIndex = PlayerPrefs.GetInt("screen_res_index");
        isFullscreen = PlayerPrefs.GetInt("fullscreen") == 1;

        volumeSliders[0].value = AudioManager.instance.masterVolume;
        volumeSliders[1].value = AudioManager.instance.sfxVolume;
        volumeSliders[2].value = AudioManager.instance.musicVolume;

        resolution.value = activeScreenResIndex;
        fullscreen.isOn = isFullscreen;
    }

    public void Back()
    {
        if (MenuManager.instance.menuLookup.ContainsKey(callingMenuName)){
            MenuManager.instance.OpenMenu(callingMenuName, menuName);
        } else {
            throw new ArgumentNullException("No calling menu to return to");
        }
    }

    public void SfxTest()
    {
        AudioManager.instance.PlaySound("Test");
    }

    public void SetMasterVolume()
    {
        AudioManager.instance.SetVolume(volumeSliders[0].value, AudioManager.AudioChannel.Master);
    }

    public void SetSfxVolume()
    {
        AudioManager.instance.SetVolume(volumeSliders[1].value, AudioManager.AudioChannel.Sfx);
    }
    public void SetMusicVolume()
    {
        AudioManager.instance.SetVolume(volumeSliders[2].value, AudioManager.AudioChannel.Music);
    }

    public void SetScreenResolution()
    {
        float aspectRatio = 16/9f;
        activeScreenResIndex = resolution.value;
        Screen.SetResolution(screenWidths[activeScreenResIndex], (int) (screenWidths[activeScreenResIndex] / aspectRatio), false);
        PlayerPrefs.SetInt("screen_res_index", activeScreenResIndex);
        PlayerPrefs.Save();
    }

    public void SetFullscreen()
    {
        if (fullscreen.isOn){
            resolution.interactable = false;
            Resolution[] resolutions = Screen.resolutions;
            Resolution maxResolution = resolutions[resolutions.Length - 1];
            Screen.SetResolution(maxResolution.width, maxResolution.height, true);
        } else {
            resolution.interactable = true;
            SetScreenResolution();
        }
        PlayerPrefs.SetInt("fullscreen", fullscreen.isOn? 1 : 0);
        PlayerPrefs.Save();
    }
}
