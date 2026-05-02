using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphTraversalApp
{
    public class Graph
    {
        private Dictionary<int, List<int>> adjacencyList;
        private int nextVertexId;

        public Graph()
        {
            adjacencyList = new Dictionary<int, List<int>>();
            nextVertexId = 0;
        }

        public int AddVertex()
        {
            int id = nextVertexId++;
            adjacencyList[id] = new List<int>();
            return id;
        }

        public void RemoveVertex(int vertexId)
        {
            if (!adjacencyList.ContainsKey(vertexId))
                return;

            // Удалить вершину из всех списков смежности
            foreach (var vertex in adjacencyList.Keys.ToList())
            {
                adjacencyList[vertex].Remove(vertexId);
            }

            // Удалить саму вершину
            adjacencyList.Remove(vertexId);
        }

        public void AddEdge(int from, int to)
        {
            if (!adjacencyList.ContainsKey(from) || !adjacencyList.ContainsKey(to))
                return;

            if (!adjacencyList[from].Contains(to))
            {
                adjacencyList[from].Add(to);
            }
        }

        public void RemoveEdge(int from, int to)
        {
            if (adjacencyList.ContainsKey(from))
            {
                adjacencyList[from].Remove(to);
            }
        }

        public List<int> GetVertices()
        {
            return adjacencyList.Keys.ToList();
        }

        public List<int> GetNeighbors(int vertexId)
        {
            return adjacencyList.ContainsKey(vertexId) ? adjacencyList[vertexId] : new List<int>();
        }

        public List<int> BFS(int startVertex)
        {
            if (!adjacencyList.ContainsKey(startVertex))
                return new List<int>();

            List<int> result = new List<int>();
            HashSet<int> visited = new HashSet<int>();
            Queue<int> queue = new Queue<int>();

            queue.Enqueue(startVertex);
            visited.Add(startVertex);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                result.Add(current);

                foreach (int neighbor in adjacencyList[current])
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return result;
        }

        public List<int> DFS(int startVertex)
        {
            List<int> result = new List<int>();
            HashSet<int> visited = new HashSet<int>();
            DFSHelper(startVertex, visited, result);
            return result;
        }

        private void DFSHelper(int vertex, HashSet<int> visited, List<int> result)
        {
            if (!adjacencyList.ContainsKey(vertex) || visited.Contains(vertex))
                return;

            visited.Add(vertex);
            result.Add(vertex);

            foreach (int neighbor in adjacencyList[vertex])
            {
                DFSHelper(neighbor, visited, result);
            }
        }

        public bool HasEdge(int from, int to)
        {
            return adjacencyList.ContainsKey(from) && adjacencyList[from].Contains(to);
        }
    }
}
