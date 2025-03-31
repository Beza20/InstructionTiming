using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorBasedInstruction : MonoBehaviour
{
    [SerializeField] private FurnitureState furnitureState; // Reference to the FurnitureState script

    public AudioSource audioSource;       // General AudioSource for playing modular instructions
    public AudioClip connectClip;         // "Connect" clip
    public AudioClip fixOrientationClip;  // "Fix the orientation" clip
    public AudioClip andClip;             // "And" clip

    public List<AudioClip> pieceClips;    // List of audio clips for each piece

    public void HandleInstruction()
    {
        if (audioSource.isPlaying)
        {
            Debug.Log("Audio source is currently playing. Skipping new instruction.");
            return;
        }
        // Get errors directly from FurnitureState
        int currentSubtaskIndex = furnitureState.GetCurrentSubtaskIndex();
        if (currentSubtaskIndex < 0) return;

        float positionError = furnitureState.GetCurrentPositionError();
        float rotationError = furnitureState.GetCurrentRotationError();
        Debug.Log(positionError);
        Debug.Log(rotationError);

        // Determine which instruction to play
        if (positionError > 0.01)
        {
           
            PlayConnectInstruction(currentSubtaskIndex);
        }
        else 
        {
            PlayOrientationInstruction(currentSubtaskIndex);
        }
    }

    private void PlayConnectInstruction(int subtaskIndex)
    {
        AudioClip pieceAClip = GetPieceClip(subtaskIndex, "A");
        AudioClip pieceBClip = GetPieceClip(subtaskIndex, "B");

        StartCoroutine(PlayConnectSequence(pieceAClip, pieceBClip, subtaskIndex));
    }

    private IEnumerator PlayConnectSequence(AudioClip pieceAClip, AudioClip pieceBClip, int subtaskIndex)
    {
        // Play "Connect"
        audioSource.PlayOneShot(connectClip);
        yield return new WaitForSeconds(connectClip.length);

        // Play "Piece A"
        if (pieceAClip != null)
        {
            audioSource.PlayOneShot(pieceAClip);
            yield return new WaitForSeconds(pieceAClip.length);
        }

        // Play "And"
        audioSource.PlayOneShot(andClip);
        yield return new WaitForSeconds(andClip.length);

        // Play "Piece B"
        if (pieceBClip != null)
        {
            audioSource.PlayOneShot(pieceBClip);
            yield return new WaitForSeconds(pieceBClip.length);
        }

        Debug.Log($"Instruction: Connect 'PieceA' and 'PieceB' of subtask {subtaskIndex + 1}");
    }

    private void PlayOrientationInstruction(int subtaskIndex)
    {
        AudioClip pieceAClip = GetPieceClip(subtaskIndex, "A");
        AudioClip pieceBClip = GetPieceClip(subtaskIndex, "B");

        StartCoroutine(PlayOrientationSequence(pieceAClip, pieceBClip, subtaskIndex));
    }

    private IEnumerator PlayOrientationSequence(AudioClip pieceAClip, AudioClip pieceBClip, int subtaskIndex)
    {
        // Play "Fix the orientation"
        audioSource.PlayOneShot(fixOrientationClip);
        yield return new WaitForSeconds(fixOrientationClip.length);

        // Play "Piece A"
        if (pieceAClip != null)
        {
            audioSource.PlayOneShot(pieceAClip);
            yield return new WaitForSeconds(pieceAClip.length);
        }

        // Play "And"
        audioSource.PlayOneShot(andClip);
        yield return new WaitForSeconds(andClip.length);

        // Play "Piece B"
        if (pieceBClip != null)
        {
            audioSource.PlayOneShot(pieceBClip);
            yield return new WaitForSeconds(pieceBClip.length);
        }

        Debug.Log($"Instruction: Fix the orientation between 'PieceA' and 'PieceB' of subtask {subtaskIndex + 1}");
    }

    private AudioClip GetPieceClip(int subtaskIndex, string pieceType)
    {
        // Fetch the correct clip for PieceA or PieceB based on subtask index
        int clipIndex = pieceType == "A" ? subtaskIndex * 2 : subtaskIndex * 2 + 1;

        if (clipIndex >= 0 && clipIndex < pieceClips.Count)
        {
            return pieceClips[clipIndex];
        }

        Debug.LogWarning($"Audio clip for Piece{pieceType} of subtask {subtaskIndex + 1} not found!");
        return null;
    }
}
