using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class ComplexRegion
    {
        private List<List<Edge>> regions;

        public ComplexRegion()
        {
            regions = new();
        }

        public ComplexRegion UnionRegion(Rectangle r)
        {
            Edge[] rectEdges = new Edge[] {
                new(r.Left, r.Right, r.Top, true),
                new(r.Top, r.Bottom, r.Right, false),
                new(r.Right, r.Left, r.Bottom, true),
                new(r.Bottom, r.Top, r.Left, false)
            };

            List<Point>[] intersections = new List<Point>[4];
            int i;
            for (i = 0; i < 4; i++) intersections[i] = new();

            Dictionary<Point, Junction> junctions = new();
            
            List<List<Edge>> splitRegions = new(regions.Count);

            i = 0;
            foreach (Edge rectEdge in rectEdges)
            {
                foreach (var region in regions)
                {
                    List<Edge> splitRegion = new();
                    foreach (Edge regEdge in region)
                    {
                        if (Intersects(regEdge, rectEdge))
                        {
                            Point intersection = Intersection(regEdge, rectEdge);
                            intersections[i].Add(intersection);
                            splitRegion.AddRange(Split(regEdge, intersection));
                        }
                        else
                        {
                            splitRegion.Add(regEdge);
                        }
                    }
                }
                ++i;
            }

            return new();
        }

        private static bool Intersects(Edge e1, Edge e2)
        {
            if (e1.isHoriz == e2.isHoriz) return false;

            int e1Min, e2Min;
            return (e1Min = Math.Min(e1.start, e1.end)) <= e2.constant && e2.constant < e1.start + e1.end - e1Min
                && (e2Min = Math.Min(e2.start, e2.end)) < e1.constant && e1.constant <= e2.start + e2.end - e2Min;
        }

        private static Point Intersection(Edge e1, Edge e2)
        {
            return e1.isHoriz ? new(e2.constant, e1.constant) : new(e1.constant, e2.constant);
        }

        private static Edge[] Split(Edge e, Point p)
        {
            int mid = e.isHoriz ? p.X : p.Y;
            return new Edge[] { new(e.start, mid, e.constant, e.isHoriz), new(mid, e.end, e.constant, e.isHoriz) };
        }
        
        public record Edge(int start, int end, int constant, bool isHoriz)
        {
            public Point Start => isHoriz ? new Point(start, constant) : new Point(constant, start);
            public Point End => isHoriz ? new Point(end, constant) : new Point(constant, end);

            public bool IsDegenerate => start == end;

            public static Edge FromPoints(Point start, Point end)
            {
                if (start.Y == end.Y)
                {
                    return new Edge(start.X, end.X, start.Y, true);
                }
                else
                {
                    return new Edge(start.Y, end.Y, start.X, false);
                }
            }

            public Point Dir
            {
                get {
                    int clamp(int c)
                    {
                        return Math.Max(-1, Math.Min(1, c));
                    }
                    Point diff = End.Subtract(Start);
                    return new Point(clamp(diff.X), clamp(diff.Y));
                }
            }
        }

        private record Junction(EdgeList.Node N1, EdgeList.Node N2)
        {
            public EdgeList.Node OutputNode
            {
                get
                {
                    //Point d1 = N1.NextE.
                    return N1;
                }
            }
        }

        private class EdgeList
        {
            public Node Head;

            public EdgeList(IEnumerable<Point> points)
            {
                var it = points.GetEnumerator();
                if (!it.MoveNext()) throw new Exception();

                Node prev = new(it.Current);
                Head = prev;

                while (it.MoveNext())
                {
                    Node curr = new(it.Current);
                    prev.LinkTo(curr);
                    prev = curr;
                }

                prev.LinkTo(Head);
            }

            public static void Trace(Junction j)
            {
                
            }

            public Dictionary<Point, Junction> Split(EdgeList other)
            {
                Dictionary<Point, Junction> result = new();

                foreach (Node n in Nodes())
                {
                    Node? iPrev = other.FindClosestIntersectionPrev(n.PrevE);

                    if (iPrev == null) continue;

                    Point iPoint = Intersection(iPrev.NextE, n.PrevE);
                    Node n1 = SplitEdge(iPrev, iPrev.Next, iPoint);
                    Node n2 = SplitEdge(n.Prev, n, iPoint);
                    if (n1.P != n2.P) throw new Exception("fun");
                    if (n1.NextE.isHoriz == n2.NextE.isHoriz) throw new Exception("fun");
                    result.Add(iPoint, new Junction(n1, n2));
                }

                return result;
            }

            private Node? FindClosestIntersectionPrev(Edge e)
            {
                Point start = e.Start;
                Node? closestNode = null;
                int closestDist = 0;
                foreach (Node node in this.Nodes())
                {
                    if (Intersects(node.NextE, e))
                    {
                        int dist = Intersection(node.NextE, e).ManhattanDist(start);
                        if (closestNode == null || dist < closestDist)
                        {
                            closestDist = dist;
                            closestNode = node;
                        }

                    }
                }

                return closestNode;
            }

            public IEnumerable<Node> Nodes()
            {
                Node curr = Head;
                do
                {
                    yield return curr;
                    curr = curr.Next;
                } while (curr != Head);
            }

            public static Node SplitEdge(Node start, Node end, Point midP)
            {
                Node mid = new(midP);
                start.LinkTo(mid);
                mid.LinkTo(end);
                return mid;
            }

            public class Node
            {
                public Point P;
                public Edge PrevE, NextE;
                public Node Prev, Next;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
                public Node(Point p)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
                {
                    P = p;
                }

                public void LinkTo(Node next)
                {
                    next.Prev = this;
                    Next = next;
                    Edge e = Edge.FromPoints(P, next.P);
                    NextE = e;
                    next.PrevE = e;
                }
            }
        }
    }
}
