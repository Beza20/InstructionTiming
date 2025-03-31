using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ProceduralAudioManager : MonoBehaviour
{
    private AudioSource audioSource;
    private float sampleRate = 48000f; // Standard Unity sample rate

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayTone(float frequency, float duration, float amplitude = 0.5f)
    {
        AudioClip tone = GenerateTone(frequency, duration, amplitude);
        audioSource.clip = tone;
        audioSource.Play();
    }

    private AudioClip GenerateTone(float frequency, float duration, float amplitude)
    {
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            samples[i] = amplitude * Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate);
        }

        AudioClip clip = AudioClip.Create("GeneratedTone", sampleCount, 1, (int)sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
