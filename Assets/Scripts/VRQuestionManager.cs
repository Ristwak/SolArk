using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;

[System.Serializable]
public class QuestionData
{
    public string question;
    public List<string> options;
    public string correct;
}

[System.Serializable]
public class QuestionList
{
    public List<QuestionData> questions;
}

public class VRQuestionManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text questionText;
    public List<Button> optionButtons;
    public TMP_Text watchTimerText;
    public GameObject watchObject;

    [Header("Game Settings")]
    public float gameDuration = 300f;
    public GameObject questionPanel;
    public Animator satelliteAnimator;

    [Header("Earth Appearance")]
    public GameObject earthObject;
    public Texture newEarthTexture;
    public float earthTextureChangeDelay = 3f;

    private List<QuestionData> questions;
    private int currentQuestionIndex = 0;
    private float timeRemaining;
    private bool isTimerRunning = false;
    private int correctAnswers = 0;
    private bool animationPlayed = false;

    [Header("Ray Settings")]
    public GameObject rayEffect;
    public Transform rayOrigin;
    public Transform rayTarget;
    public float delayAfterAnimation = 2f;
    [SerializeField] private float rayThickness = 0.1f;

    [Header("End Sequence Panels")]
    public GameObject videoPanel;
    public GameObject exitPanel;
    public float postTextureChangeDelay = 2f;

    [Header("Video")]
    public VideoClip startvideo;
    public VideoClip enVideo;

    private VideoPlayer videoPlayer;

    void Start()
    {
        LoadQuestionsFromJSON();
        ShuffleQuestions();

        if (videoPanel != null)
        {
            videoPlayer = videoPanel.GetComponentInChildren<VideoPlayer>();
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached += OnVideoFinished;
                PlayStartVideo(); // 🔁 Start video plays here
            }
        }

        timeRemaining = gameDuration;

        if (watchObject != null)
            watchObject.SetActive(false);
    }

    void Update()
    {
        if (watchObject != null && questionPanel != null)
            watchObject.SetActive(questionPanel.activeSelf);

        if (isTimerRunning)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerUI(timeRemaining);

            if (timeRemaining <= 0)
            {
                isTimerRunning = false;
                EndQuizDueToTimeout();
            }
        }
    }

    public void StartQuizTimer()
    {
        isTimerRunning = true;
    }

    void UpdateTimerUI(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        if (watchTimerText != null)
        {
            watchTimerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    void LoadQuestionsFromJSON()
    {
        TextAsset jsonText = Resources.Load<TextAsset>("SolArk");
        if (jsonText != null)
        {
            questions = JsonUtility.FromJson<QuestionList>(jsonText.text).questions;
        }
        else
        {
            Debug.LogError("SolArk.json not found in Resources!");
            questions = new List<QuestionData>();
        }
    }

    void ShuffleQuestions()
    {
        if (questions == null || questions.Count <= 1) return;

        for (int i = 0; i < questions.Count; i++)
        {
            int rand = Random.Range(i, questions.Count);
            var temp = questions[i];
            questions[i] = questions[rand];
            questions[rand] = temp;
        }
    }

    void ShowQuestion(int index)
    {
        if (questions == null || index >= questions.Count) return;

        var question = questions[index];
        questionText.text = question.question;

        for (int i = 0; i < optionButtons.Count; i++)
        {
            TMP_Text btnText = optionButtons[i].GetComponentInChildren<TMP_Text>();

            if (i < question.options.Count)
            {
                btnText.text = question.options[i];
                optionButtons[i].gameObject.SetActive(true);

                int choiceIndex = i;
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => CheckAnswer(choiceIndex));
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void CheckAnswer(int selectedIndex)
    {
        string selected = questions[currentQuestionIndex].options[selectedIndex];
        string correct = questions[currentQuestionIndex].correct;

        if (selected == correct)
        {
            Debug.Log("✅ Correct!");
            correctAnswers++;

            if (correctAnswers == 5 && !animationPlayed)
            {
                questionPanel.SetActive(false);
                PlaySatelliteAnimation();
            }
        }
        else
        {
            Debug.Log("❌ Incorrect!");
        }

        currentQuestionIndex++;

        if (currentQuestionIndex < questions.Count)
        {
            ShowQuestion(currentQuestionIndex);
        }
        else
        {
            Debug.Log("🎉 Quiz Complete!");
            isTimerRunning = false;
        }
    }

    void PlaySatelliteAnimation()
    {
        animationPlayed = true;

        if (satelliteAnimator != null)
        {
            questionPanel.SetActive(false);
            satelliteAnimator.Play("OpenAnimation");
            Invoke(nameof(FireRayToEarth), delayAfterAnimation);
        }
        else
        {
            Debug.LogWarning("Satellite Animator not assigned!");
        }
    }

    void EndQuizDueToTimeout()
    {
        Debug.Log("⏱️ Time's up! Quiz ended.");
        questionPanel.SetActive(false);
    }

    void FireRayToEarth()
    {
        Debug.Log("🔭 Firing ray from satellite to Earth!");

        if (rayEffect != null && rayOrigin != null && rayTarget != null)
        {
            rayEffect.SetActive(true);
            rayEffect.transform.position = rayOrigin.position;

            LineRenderer line = rayEffect.GetComponent<LineRenderer>();
            if (line != null)
            {
                line.startWidth = rayThickness;
                line.endWidth = rayThickness;

                line.SetPosition(0, rayOrigin.position);
                line.SetPosition(1, rayTarget.position);
            }

            Invoke(nameof(ChangeEarthTexture), earthTextureChangeDelay);
        }
        else
        {
            Debug.LogWarning("Ray Effect or its positions not assigned.");
        }
    }

    void ChangeEarthTexture()
    {
        if (earthObject != null && newEarthTexture != null)
        {
            Renderer renderer = earthObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = newEarthTexture;
                Debug.Log("🌍 Earth texture changed!");

                Invoke(nameof(ShowEndVideo), postTextureChangeDelay);
            }
        }
    }

    void ShowEndVideo()
    {
        Debug.Log("🎞 Showing end video...");

        if (watchObject) watchObject.SetActive(false);
        if (rayEffect) rayEffect.SetActive(false);
        if (questionPanel) questionPanel.SetActive(false);

        if (videoPanel && videoPlayer != null && enVideo != null)
        {
            videoPanel.SetActive(true);
            videoPlayer.clip = enVideo;
            videoPlayer.Play();
        }
    }

    void PlayStartVideo()
    {
        if (videoPanel && videoPlayer != null && startvideo != null)
        {
            videoPanel.SetActive(true);
            videoPlayer.clip = startvideo;
            videoPlayer.Play();
        }
    }

    public void SkipVideo()
    {
        Debug.Log("⏩ Video skipped.");

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            OnVideoFinished(videoPlayer); // Manually trigger the finish handler
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("📽️ Video finished. Showing exit panel.");

        if (exitPanel != null)
            exitPanel.SetActive(true);

        if (videoPanel != null)
            videoPanel.SetActive(false);
    }
}
