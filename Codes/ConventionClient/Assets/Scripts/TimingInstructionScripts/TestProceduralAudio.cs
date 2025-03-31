using UnityEngine;

public class TestProceduralAudio : MonoBehaviour
{
    public ProceduralAudioManager audioManager;

    void Update()
    {
        // Press the spacebar to play a tone
        if (Input.GetKeyDown(KeyCode.Space))
        {
            audioManager.PlayTone(440f, 1f, 0.5f); // Play a 440 Hz tone (A4) for 1 second at 50% volume
        }
    }
}
