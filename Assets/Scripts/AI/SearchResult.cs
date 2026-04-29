using System.Collections.Generic;
using UnityEngine;

namespace MazeChase.AI
{
    public class SearchResult
    {
        public List<Vector3> path = new List<Vector3>();
        public List<Vector3> visitedPositions = new List<Vector3>();
        public float totalCost;
        public int expandedNodeCount;
        public bool pathFound;
    }
}