using System.Collections.Generic;
using UnityEngine;

public class ProgressEvaluator
{
    public struct EvaluationResult
    {
        public float Progress;
        public int IssueIndex;
        public float PositionError;
        public float RotationError;
    }

    public EvaluationResult Evaluate(List<ConnectionDetector.DetectedConnection> connections, int totalSubtasks)
    {
        float totalError = 0f;
        int currentIssueIndex = -1;
        float maxPositionError = 0f;
        float maxRotationError = 0f;

        foreach (var conn in connections)
        {
            totalError += conn.Error;

            if (conn.Error > 0.05f && 
                (currentIssueIndex == -1 || conn.Error > maxPositionError + maxRotationError))
            {
                currentIssueIndex = conn.PieceAIndex;
                maxPositionError = Mathf.Abs(conn.Distance - conn.IdealDistance);       
                maxRotationError = Quaternion.Angle(conn.RotationDiff, conn.IdealRotation); 
            }
        }

        return new EvaluationResult {
            Progress = 1f - (totalError / totalSubtasks),
            IssueIndex = currentIssueIndex,
            PositionError = maxPositionError,
            RotationError = maxRotationError
        };
    }
}