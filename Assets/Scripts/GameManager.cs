using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject homePanel;
    public GameObject questionPanel;
    public GameObject aboutPanel;
    public GameObject videoPanel;
    public GameObject endPanel; // 🔹 Add this via Inspector

    [Header("GameObjects")]
    public GameObject sun;
    public GameObject satellite;
    public GameObject earth;

    [Header("Video")]
    public VideoPlayer introVideo;
    public VideoClip startVideo;
    public VideoClip endVideo;

    private bool isPlayingEndVideo = false;

    void Start()
    {
        introVideo.clip = startVideo;
        sun.SetActive(false);
        satellite.SetActive(false);
        earth.SetActive(false);
        endPanel?.SetActive(false); // 🔹 hide endPanel

        homePanel.SetActive(true);
        questionPanel.SetActive(false);
        aboutPanel.SetActive(false);
        videoPanel.SetActive(false);
        introVideo.gameObject.SetActive(false);

        if (introVideo != null)
        {
            introVideo.loopPointReached += OnVideoEnd;
        }
    }

    public void OnStartButton()
    {
        homePanel.SetActive(false);
        aboutPanel.SetActive(false);
        introVideo.gameObject.SetActive(true);
        videoPanel.SetActive(true);

        if (introVideo != null)
        {
            introVideo.clip = startVideo;
            introVideo.isLooping = false;
            introVideo.Play();
        }
        else
        {
            Debug.LogWarning("Intro video not assigned.");
            ShowQuestions(); // fallback
        }
    }

    public void PlayEndVideo()
    {
        if (introVideo == null || endVideo == null) return;

        introVideo.gameObject.SetActive(true);
        videoPanel.SetActive(true);
        questionPanel.SetActive(false);
        sun.SetActive(false);
        satellite.SetActive(false);
        earth.SetActive(false);

        isPlayingEndVideo = true;
        introVideo.clip = endVideo;
        introVideo.Play();
    }

    public void OnAboutButton()
    {
        aboutPanel.SetActive(true);
        homePanel.SetActive(false);
    }

    public void OnBackButton()
    {
        aboutPanel.SetActive(false);
        homePanel.SetActive(true);
    }

    public void OnExitButton()
    {
        Application.Quit();
    }

    public void OnRestartButton()
    {
        SceneManager.LoadScene("SolArk");
    }

    public void OnSkipVideo()
    {
        if (introVideo != null && introVideo.isPlaying)
        {
            introVideo.Stop();
        }

        ShowQuestions();
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        if (isPlayingEndVideo)
        {
            ShowEndPanel();
        }
        else
        {
            ShowQuestions();
        }
    }

    private void ShowQuestions()
    {
        videoPanel.SetActive(false);
        questionPanel.SetActive(true);
        sun.SetActive(true);
        satellite.SetActive(true);
        earth.SetActive(true);

        FindObjectOfType<VRQuestionManager>()?.StartQuizTimer();
    }

    private void ShowEndPanel()
    {
        videoPanel.SetActive(false);
        endPanel?.SetActive(true);
        homePanel?.SetActive(false);
        questionPanel?.SetActive(false);
        aboutPanel?.SetActive(false);
    }
}
