using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    public TMP_InputField openEndedInput;
    public Button submitButton;

    private string responseQ1 = "";
    private string openEndedResponse = "";
    private string currentTriggerSource = "";

    [SerializeField] private TriggerManager triggerManager;
    //[SerializeField] private TriggerInstructionPlayer instructionPlayer;

    private List<MonoBehaviour> triggerScripts = new List<MonoBehaviour>();
    private string sessionLogFilePath;

    void Start()
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filename = $"TriggerInterviewLog_{timestamp}.csv";
        sessionLogFilePath = Path.Combine(Application.persistentDataPath, filename);

        // Write CSV header if new
        if (!File.Exists(sessionLogFilePath))
        {
            File.WriteAllText(sessionLogFilePath, "Time,Trigger,Q1,OpenEnded\n");
        }

        yesButton.onClick.AddListener(() => OnAnswer("Yes"));
        noButton.onClick.AddListener(() => OnAnswer("No"));
        submitButton.onClick.AddListener(OnSubmitTextAnswer);

        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
        openEndedInput.gameObject.SetActive(false);
        submitButton.gameObject.SetActive(false);

        RefreshTriggerScripts();
    }


    public void TriggerInterview(string triggerSourceName)
    {
        currentTriggerSource = triggerSourceName;
        PauseTriggers();
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
        responseQ1 = answer;
        questionAudioSource.Stop();
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);

        if (answer == "Yes")
        {
            openEndedResponse = "";
            LogResponses();  // Log only Yes
            ResumeTriggers();
            //instructionPlayer?.OnTriggerEvent();
        }
        else if (answer == "No")
        {
            LogInitialNo();  // Log right away
            StartCoroutine(PlayFollowUpAndEnableInput());
        }
    }

    private void OnSubmitTextAnswer()
    {
        openEndedResponse = openEndedInput.text.Trim();
        openEndedInput.gameObject.SetActive(false);
        submitButton.gameObject.SetActive(false);

        LogResponses(); ; // Completes the row
        ResumeTriggers();
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
    private void LogInitialNo()
    {
        openEndedResponse = ""; // clear just in case
        string log = $"{GetUnixTimestamp()}, {currentTriggerSource}, No, ";
        Debug.Log(log);
        File.AppendAllText(sessionLogFilePath, log); // Don't add \n yet
    }

    private void LogResponses()
    {
        string log = $"{GetUnixTimestamp()}, {currentTriggerSource}, {responseQ1}, {openEndedResponse}";
        Debug.Log(log);
        File.AppendAllText(sessionLogFilePath, log + "\n");
    }


    public void RefreshTriggerScripts()
    {
        triggerScripts = triggerManager.GetActiveTriggerScripts();
    }
    private IEnumerator PlayFollowUpAndEnableInput()
    {
        if (question2Audio != null)
        {
            questionAudioSource.clip = question2Audio;
            questionAudioSource.Play();
            yield return new WaitForSecondsRealtime(question2Audio.length + 0.5f); // Wait for audio to finish
        }

        openEndedInput.text = "";
        openEndedInput.gameObject.SetActive(true);
        submitButton.gameObject.SetActive(true);
        openEndedInput.ActivateInputField();  // Autofocus input
    }
    private string GetUnixTimestamp()
    {
        double epoch = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        return epoch.ToString("F3");  // Includes milliseconds (e.g., 1708132456.789)
    }
}
