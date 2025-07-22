using UnityEngine;
using System;

public class SubscriptionManager : MonoBehaviour
{
    public static SubscriptionManager Instance;

    [Header("Assign this in the Inspector")]
    public GameObject expiredPanel;

    private DateTime startTime;
    private const double subscriptionPeriodDays = 30.0;

    private bool isGameStopped = false;
    public bool isSubscribed = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        isSubscribed = PlayerPrefs.GetInt("IsSubscribed", 0) == 1;

        if (PlayerPrefs.HasKey("SubscriptionStartTime"))
        {
            startTime = DateTime.Parse(PlayerPrefs.GetString("SubscriptionStartTime"));
        }

        if (expiredPanel != null)
            expiredPanel.SetActive(false);
    }

    private void Update()
    {
        if (isGameStopped || !isSubscribed) return;

        TimeSpan elapsed = DateTime.Now - startTime;

        if (elapsed.TotalDays >= subscriptionPeriodDays)
        {
            StopGame("30-day subscription expired.");
        }
    }

    public void ActivateSubscription()
    {
        isSubscribed = true;
        startTime = DateTime.Now;

        PlayerPrefs.SetInt("IsSubscribed", 1);
        PlayerPrefs.SetString("SubscriptionStartTime", startTime.ToString());
        PlayerPrefs.Save();

        Debug.Log("7-day subscription started.");
    }

    private void StopGame(string reason)
    {
        Debug.Log(reason);
        isGameStopped = true;
        Time.timeScale = 0f;

        if (expiredPanel != null)
            expiredPanel.SetActive(true);
    }
}