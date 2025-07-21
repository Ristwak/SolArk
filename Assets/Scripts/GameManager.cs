using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject homePanel;
    public GameObject questionPanel;
    public GameObject aboutPanel;
    public GameObject videoPanel;

    [Header("GameObjects")]
    public GameObject sun;
    public GameObject satellite;
    public GameObject earth;

    [Header("Video")]
    public VideoPlayer introVideo;

    void Start()
    {
        sun.SetActive(false);
        satellite.SetActive(false);
        earth.SetActive(false);

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
            introVideo.isLooping = false;
            introVideo.Play();
        }
        else
        {
            Debug.LogWarning("Intro video not assigned.");
            ShowQuestions(); // fallback
        }
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
        ShowQuestions();
    }

    private void ShowQuestions()
    {
        videoPanel.SetActive(false);
        questionPanel.SetActive(true);
        sun.SetActive(true);
        satellite.SetActive(true);
        earth.SetActive(true);

        // Start the quiz timer now
        FindObjectOfType<VRQuestionManager>()?.StartQuizTimer();
    }
}
