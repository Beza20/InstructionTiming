using UnityEngine;
using UnityEngine.UI;  // Required for handling UI elements (Checkboxes)

public class TriggerManager : MonoBehaviour
{
    [Header("Trigger GameObjects")]
    [SerializeField] private GameObject A1;     // A1 GameObject
    [SerializeField] private GameObject A2;     // A2 GameObject
    [SerializeField] private GameObject B;      // B GameObject

    [Header("Trigger States")]
    public bool isA1Active = false;  // State for A1 GameObject (Checkbox)
    public bool isA2Active = false;  // State for A2 GameObject (Checkbox)
    public bool isBActive = false;   // State for B GameObject (Checkbox)

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
        B.SetActive(isBActive);
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

    public void OnBToggleChanged(bool value)
    {
        isBActive = value;
        UpdateGameObjectStates();
    }
}
