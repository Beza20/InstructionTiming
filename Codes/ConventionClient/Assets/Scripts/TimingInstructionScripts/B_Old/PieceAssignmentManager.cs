using System.Collections.Generic;

public class PieceAssignmentManager
{
    private Dictionary<int, int> pieceAssignments = new Dictionary<int, int>();
    private Dictionary<int, List<int>> groupLookup = new Dictionary<int, List<int>>();
    private Dictionary<int, int> reciprocalAssignments = new Dictionary<int, int>();

    public void Initialize(List<int[]> interchangeableGroups, int pieceCount)
    {
        pieceAssignments.Clear();
        groupLookup.Clear();
        reciprocalAssignments.Clear();

        for (int i = 0; i < pieceCount; i++)
        {
            pieceAssignments[i] = i; // Default to exact matches
            reciprocalAssignments[i] = i;
        }

        if (interchangeableGroups != null)
        {
            foreach (var group in interchangeableGroups)
            {
                int groupId = groupLookup.Count;
                groupLookup[groupId] = new List<int>(group);
            }
        }
    }

    public bool IsValidPairing(int pieceAIndex, int pieceBIndex)
    {
        // Valid if either exact match or in same group
        return pieceBIndex == reciprocalAssignments[pieceAIndex] || 
               IsInSameGroup(pieceBIndex, reciprocalAssignments[pieceAIndex]);
    }

    public bool IsInSameGroup(int index1, int index2)
    {
        if (index1 == index2) return true;
        foreach (var group in groupLookup.Values)
        {
            if (group.Contains(index1) && group.Contains(index2))
                return true;
        }
        return false;
    }

    public void CommitConnection(ConnectionDetector.DetectedConnection conn)
    {
        pieceAssignments[conn.PieceAIndex] = conn.PieceBIndex;
        
        // Handle reciprocal assignments for interchangeable groups
        if (groupLookup.Count > 0)
        {
            foreach (var group in groupLookup.Values)
            {
                if (group.Contains(conn.PieceBIndex))
                {
                    foreach (int member in group)
                    {
                        if (member != conn.PieceBIndex)
                        {
                            reciprocalAssignments[member] = conn.PieceAIndex;
                        }
                    }
                    break;
                }
            }
        }
    }
}