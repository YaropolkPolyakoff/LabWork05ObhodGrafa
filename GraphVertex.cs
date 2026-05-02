using System.Drawing;

namespace GraphTraversalApp
{
    public class GraphVertex
    {
        public int Id { get; set; }
        public Point Position { get; set; }
        public int Radius { get; set; }

        public GraphVertex(int id, Point position, int radius = 25)
        {
            Id = id;
            Position = position;
            Radius = radius;
        }
    }
}
