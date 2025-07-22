using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

    [Header("Earth Texture Swap")]
    public Renderer earthRenderer;
    public Texture newEarthTexture;
    public GameObject exitPanel;
    public GameManager gameManager;

    [Header("Game Settings")]
    public float gameDuration = 300f;
    public GameObject questionPanel;
    public Animator satelliteAnimator;

    [Header("Laser Settings")]
    public GameObject laserBeam;
    public Transform satelliteOrigin;
    public Transform earthTarget;
    public float laserDelay = 7f;
    public float laserDuration = 3f;
    public float laserWidth = 0.1f;

    private List<QuestionData> questions;
    private int currentQuestionIndex = 0;
    private float timeRemaining;
    private bool isTimerRunning = false;
    private int correctAnswers = 0;
    private bool animationPlayed = false;

    void Start()
    {
        LoadQuestionsFromJSON();
        ShuffleQuestions();
        ShowQuestion(currentQuestionIndex);
        timeRemaining = gameDuration;

        if (watchObject != null)
            watchObject.SetActive(false);
    }

    void Update()
    {
        if (watchObject != null && questionPanel != null)
        {
            watchObject.SetActive(questionPanel.activeSelf);
        }

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
            watchTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
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
        // Fisher-Yates Shuffle
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
        if (questions == null || index >= questions.Count)
            return;

        var question = questions[index];
        questionText.text = question.question;

        for (int i = 0; i < optionButtons.Count; i++)
        {
            TMP_Text btnText = optionButtons[i].GetComponentInChildren<TMP_Text>();
            btnText.text = question.options[i];

            int choiceIndex = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => CheckAnswer(choiceIndex));
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
            Debug.Log("🚀 Playing satellite animation...");
            satelliteAnimator.Play("OpenAnimation");
            StartCoroutine(WaitForAnimationThenFireLaser());
        }
        else
        {
            Debug.LogWarning("Satellite Animator not assigned!");
        }
    }

    IEnumerator WaitForAnimationThenFireLaser()
    {
        float animationLength = 3.5f;
        yield return new WaitForSeconds(animationLength + laserDelay);

        FireLaser();

        yield return new WaitForSeconds(laserDuration); // wait for laser to finish

        if (earthRenderer != null && newEarthTexture != null)
        {
            Debug.Log("🌍 Earth texture changing...");
            earthRenderer.material.mainTexture = newEarthTexture;
        }

        yield return new WaitForSeconds(1f); // short delay before video

        // 🎥 Play final video
        if (gameManager != null)
        {
            gameManager.PlayEndVideo();
        }
    }

    void FireLaser()
    {
        if (laserBeam != null && satelliteOrigin != null && earthTarget != null)
        {
            LineRenderer line = laserBeam.GetComponent<LineRenderer>();
            laserBeam.SetActive(true);
            line.startWidth = laserWidth;
            line.endWidth = laserWidth;
            line.SetPosition(0, satelliteOrigin.position);
            line.SetPosition(1, earthTarget.position);

            Debug.Log("🔫 Laser fired!");

            // Delay texture change to simulate "impact"
            StartCoroutine(ChangeEarthTextureAfterImpact());
        }
        else
        {
            Debug.LogWarning("❌ Laser or origin/target not assigned.");
        }
    }

    IEnumerator ChangeEarthTextureAfterImpact()
    {
        yield return new WaitForSeconds(laserDuration); // Wait till laser ends

        if (earthRenderer != null && newEarthTexture != null)
        {
            Debug.Log("🌍 Earth texture changing...");
            earthRenderer.material.mainTexture = newEarthTexture;
        }
        else
        {
            Debug.LogWarning("Missing earthRenderer or newEarthTexture!");
        }
    }

    void EndQuizDueToTimeout()
    {
        Debug.Log("⏱️ Time's up! Quiz ended.");
    }
}
