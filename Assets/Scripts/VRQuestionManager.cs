using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public GameObject watchObject; // 👈 Reference to the whole watch GameObject

    [Header("Game Settings")]
    public float gameDuration = 300f;

    private List<QuestionData> questions;
    private int currentQuestionIndex = 0;
    private float timeRemaining;
    private bool isTimerRunning = false;

    void Start()
    {
        LoadQuestionsFromJSON();
        ShowQuestion(currentQuestionIndex);
        timeRemaining = gameDuration;

        // Make sure watch is hidden at start
        if (watchObject != null)
            watchObject.SetActive(false);
    }

    void Update()
    {
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

        // 👇 Enable the watch when quiz starts
        if (watchObject != null)
            watchObject.SetActive(true);
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
            Debug.Log("Correct!");
        }
        else
        {
            Debug.Log("Incorrect!");
        }

        currentQuestionIndex++;
        if (currentQuestionIndex < questions.Count)
        {
            ShowQuestion(currentQuestionIndex);
        }
        else
        {
            Debug.Log("Quiz Complete!");
            isTimerRunning = false;
        }
    }

    void EndQuizDueToTimeout()
    {
        Debug.Log("⏱️ Time's up! Quiz ended.");
        // Add end-panel or feedback here if needed
    }
}
