using System.Collections.Generic;
using UnityEngine;

public class ConnectionDetector
{
    private const float positionTolerance = 0.05f;
    private const float orientationTolerance = 2f;

    public struct DetectedConnection
    {
        public int PieceAIndex;
        public int PieceBIndex;
        public int MatchedSubtaskIndex;
        public float Distance;
        public Quaternion RotationDiff;
        public float Error;
        public float IdealDistance;       
        public Quaternion IdealRotation;  
    }

    public List<DetectedConnection> DetectBestConnections(
        List<GameObject> piecesA,
        List<GameObject> piecesB,
        List<SubtaskData> subtasks,
        PieceAssignmentManager assignmentManager)
    {
        List<DetectedConnection> bestConnections = new List<DetectedConnection>();
        
        for (int i = 0; i < piecesA.Count; i++)
        {
            if (piecesA[i] == null) continue;

            DetectedConnection bestMatch = new DetectedConnection {
                PieceAIndex = i,
                Error = float.MaxValue
            };

            for (int j = 0; j < piecesB.Count; j++)
            {
                if (piecesB[j] == null) continue;
                if (!assignmentManager.IsValidPairing(i, j)) continue;

                float distance = Vector3.Distance(piecesA[i].transform.position, piecesB[j].transform.position);
                Quaternion rotDiff = Quaternion.Inverse(piecesA[i].transform.rotation) * piecesB[j].transform.rotation;
                
                float posError = Mathf.Clamp01(Mathf.Abs(distance - subtasks[i].RelativeDistance) / positionTolerance);
                float rotError = Mathf.Clamp01(Quaternion.Angle(rotDiff, subtasks[i].AngleDifference) / orientationTolerance);
                float totalError = 0.4f * posError + 0.6f * rotError;

                if (totalError < bestMatch.Error)
                {
                    bestMatch = new DetectedConnection {
                        PieceAIndex = i,
                        PieceBIndex = j,
                        MatchedSubtaskIndex = i,
                        Distance = distance,
                        RotationDiff = rotDiff,
                        Error = totalError,
                        IdealDistance = subtasks[i].RelativeDistance,         
                        IdealRotation = subtasks[i].AngleDifference          
            
                    };
                }
            }

            if (bestMatch.Error < float.MaxValue)
                bestConnections.Add(bestMatch);
        }
        
        return bestConnections;
    }
}