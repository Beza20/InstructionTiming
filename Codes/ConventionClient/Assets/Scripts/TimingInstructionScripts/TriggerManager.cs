using UnityEngine;
using UnityEngine.UI;  // Required for handling UI elements (Checkboxes)
using System.Collections.Generic;

public class TriggerManager : MonoBehaviour
{
    [Header("Trigger GameObjects")]
    [SerializeField] private GameObject A1;     // A1 GameObject
    [SerializeField] private GameObject A2;     // A2 GameObject
    [SerializeField] private GameObject B1;      // B1 GameObject
    [SerializeField] private GameObject B2;      // B2 GameObject
    [SerializeField] private GameObject P;      // p GameObject
    [SerializeField] private GameObject Random;      // random GameObject


    [Header("Trigger States")]
    public bool isA1Active = false;  // State for A1 GameObject (Checkbox)
    public bool isA2Active = false;  // State for A2 GameObject (Checkbox)
    public bool isB1Active = false;   // State for B GameObject (Checkbox)
    public bool isB2Active = false;   // State for B GameObject (Checkbox)
    public bool isPActive = false;   // State for B GameObject (Checkbox)

    public bool isRActive = false;   // State for B GameObject (Checkbox)
    



    // Start is called before the first frame update
    void Start()
    {
        // Initialize GameObject states based on the boolean values
        UpdateGameObjectStates();
    }

    // Update is called once per frame
    void Update()
    {
        // Optionally, you can update GameObjects if states change dynamically during gameplay
        UpdateGameObjectStates();
    }

    // Method to update the active state of GameObjects based on the bools
    private void UpdateGameObjectStates()
    {
        A1.SetActive(isA1Active);
        A2.SetActive(isA2Active);
        B1.SetActive(isB1Active);
        B2.SetActive(isB2Active);
        P.SetActive(isPActive);
        Random.SetActive(isRActive);
    }

    // You can use this method to update bool values when toggles are changed
    public void OnA1ToggleChanged(bool value)
    {
        isA1Active = value;
        UpdateGameObjectStates();
    }

    public void OnA2ToggleChanged(bool value)
    {
        isA2Active = value;
        UpdateGameObjectStates();
    }

    public void OnB1ToggleChanged(bool value)
    {
        isB1Active = value;
        UpdateGameObjectStates();
    }

    public void OnB2ToggleChanged(bool value)
    {
        isB2Active = value;
        UpdateGameObjectStates();
    }

    public void OnPToggleChanged(bool value)
    {
        isPActive = value;
        UpdateGameObjectStates();
    }

    public void OnRToggleChanged(bool value)
    {
        isRActive = value;
        UpdateGameObjectStates();
    }
    

    

    public List<MonoBehaviour> GetActiveTriggerScripts()
    {
        List<MonoBehaviour> activeTriggers = new List<MonoBehaviour>();

        if (isA1Active && A1 != null)
            activeTriggers.AddRange(A1.GetComponents<MonoBehaviour>());

        if (isA2Active && A2 != null)
            activeTriggers.AddRange(A2.GetComponents<MonoBehaviour>());

        if (isB1Active && B1 != null)
            activeTriggers.AddRange(B1.GetComponents<MonoBehaviour>());

        if (isB2Active && B2 != null)
            activeTriggers.AddRange(B2.GetComponents<MonoBehaviour>());

        if (isPActive && P != null)
            activeTriggers.AddRange(P.GetComponents<MonoBehaviour>());

        if (isRActive && Random != null)
            activeTriggers.AddRange(Random.GetComponents<MonoBehaviour>());

        return activeTriggers;
    }

}
