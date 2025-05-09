using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class TriggerManagerCoordinator : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource questionAudioSource;
    public AudioClip question1Audio;
    public AudioClip question2Audio;

    [Header("UI Buttons")]
    public Button yesButton;
    public Button noButton;

    [Header("Trigger Scripts to Pause")]
    //public MonoBehaviour[] triggerScripts; // MovementTrigger, A2TriggerSimplified, etc.

    private string responseQ1 = "";
    private string responseQ2 = "";
    private int questionIndex = 0;
    private string currentTriggerSource = "";

    [SerializeField] private TriggerManager triggerManager;

    private List<MonoBehaviour> triggerScripts = new List<MonoBehaviour>();

    void Start()
    {
        yesButton.onClick.AddListener(() => OnAnswer("Yes"));
        noButton.onClick.AddListener(() => OnAnswer("No"));
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
        RefreshTriggerScripts();
    }

    public void TriggerInterview(string triggerSourceName)
    {
        currentTriggerSource = triggerSourceName;
        PauseTriggers();
        questionIndex = 0;
        StartCoroutine(AskQuestionSequence());
    }

    private IEnumerator AskQuestionSequence()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        AskQuestion(question1Audio);
    }

    private void AskQuestion(AudioClip clip)
    {
        questionAudioSource.clip = clip;
        questionAudioSource.Play();
        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);
    }

    private void OnAnswer(string answer)
    {
        if (questionIndex == 0)
        {
            responseQ1 = answer;
            questionAudioSource.Stop();
            questionIndex = 1;
            yesButton.gameObject.SetActive(false);
            noButton.gameObject.SetActive(false);
            StartCoroutine(WaitAndAskNextQuestion());
        }
        else if (questionIndex == 1)
        {
            responseQ2 = answer;
            questionAudioSource.Stop();
            yesButton.gameObject.SetActive(false);
            noButton.gameObject.SetActive(false);
            ResumeTriggers();
            LogResponses();
        }
    }

    private IEnumerator WaitAndAskNextQuestion()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        AskQuestion(question2Audio);
    }

    private void PauseTriggers()
    {
        foreach (var script in triggerScripts)
            script.enabled = false;
    }

    private void ResumeTriggers()
    {
        foreach (var script in triggerScripts)
            script.enabled = true;
    }

    private void LogResponses()
    {
        string log = $"{DateTime.Now}, Trigger: {currentTriggerSource}, Q1: {responseQ1}, Q2: {responseQ2}";
        Debug.Log(log);
        File.AppendAllText(Application.dataPath + "/TriggerInterviewLog.csv", log + "\n");
    }
    public void RefreshTriggerScripts()
    {
        triggerScripts = triggerManager.GetActiveTriggerScripts();
    }
}
