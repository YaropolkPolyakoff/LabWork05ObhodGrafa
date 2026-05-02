using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GraphTraversalApp
{
    public partial class MainForm : Form
    {
        private Graph myGraph;
        private Dictionary<int, GraphVertex> visualVertices;
        private Panel canvas;
        private TextBox startInput, outputBox;
        private GraphVertex drag, select, edge1;
        private bool delVertex, addEdge, delEdge;
        private Point offset;

        public MainForm()
        {
            CreateUI();
            myGraph = new Graph();
            visualVertices = new Dictionary<int, GraphVertex>();
            MakeDefaultGraph();
        }

        private void CreateUI()
        {
            Text = "Обход графа";
            Width = 1000;
            Height = 700;
            StartPosition = FormStartPosition.CenterScreen;

            canvas = new Panel { Left = 10, Top = 10, Width = 700, Height = 600, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            canvas.Paint += OnPaint;
            canvas.MouseDown += OnMouseDown;
            canvas.MouseMove += OnMouseMove;
            canvas.MouseUp += OnMouseUp;
            Controls.Add(canvas);

            int x = 720, y = 10;

            AddBtn("Добавить вершину", x, y, (s, e) => DoAddVertex());
            y += 40;
            AddBtn("Удалить вершину", x, y, (s, e) => DoRemoveVertexMode());
            y += 40;
            AddBtn("Добавить ребро", x, y, (s, e) => DoAddEdgeMode());
            y += 40;
            AddBtn("Удалить ребро", x, y, (s, e) => DoRemoveEdgeMode());
            y += 40;
            AddBtn("Очистить", x, y, (s, e) => DoClear());
            y += 60;

            Controls.Add(new Label { Left = x, Top = y, Width = 260, Text = "ID вершины для обхода:" });
            y += 25;
            startInput = new TextBox { Left = x, Top = y, Width = 260, Text = "0" };
            Controls.Add(startInput);
            y += 40;

            AddBtn("BFS Обход", x, y, (s, e) => DoBFS());
            y += 40;
            AddBtn("DFS Обход", x, y, (s, e) => DoDFS());
            y += 60;

            Controls.Add(new Label { Left = x, Top = y, Width = 260, Text = "Результат обхода:" });
            y += 25;
            outputBox = new TextBox { Left = x, Top = y, Width = 260, Height = 80, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical };
            Controls.Add(outputBox);
            y += 90;

            Controls.Add(new Label { Left = x, Top = y, Width = 260, Height = 100, Text = "Перетаскивайте вершины.\nКликните для ребра на 2 вершины.\nГраф направленный.\n5 вершин по умолчанию." });
        }

        private void AddBtn(string txt, int x, int y, EventHandler handler)
        {
            var b = new Button { Left = x, Top = y, Width = 260, Height = 30, Text = txt };
            b.Click += handler;
            Controls.Add(b);
        }

        private void MakeDefaultGraph()
        {
            int cx = 350, cy = 300, r = 200;
            var ids = new List<int>();
            for (int i = 0; i < 5; i++)
            {
                double ang = i * Math.PI * 2.0 / 5.0 - Math.PI * 0.5;
                int vx = cx + (int)(Math.Cos(ang) * r);
                int vy = cy + (int)(Math.Sin(ang) * r);
                int vid = myGraph.AddVertex();
                visualVertices[vid] = new GraphVertex(vid, new Point(vx, vy));
                ids.Add(vid);
            }
            myGraph.AddEdge(ids[0], ids[1]);
            myGraph.AddEdge(ids[1], ids[2]);
            myGraph.AddEdge(ids[2], ids[3]);
            myGraph.AddEdge(ids[3], ids[4]);
            myGraph.AddEdge(ids[4], ids[0]);
            myGraph.AddEdge(ids[0], ids[2]);
            canvas.Refresh();
        }

        private void OnPaint(object s, PaintEventArgs e)
        {
            var gfx = e.Graphics;
            gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var p = new Pen(Color.Black, 2);

            foreach (var v in visualVertices.Values)
            {
                foreach (var n in myGraph.GetNeighbors(v.Id))
                {
                    if (visualVertices.ContainsKey(n))
                    {
                        var nv = visualVertices[n];
                        PaintArrow(gfx, p, v.Position, nv.Position, v.Radius);
                    }
                }
            }

            var fnt = new Font("Arial", 12, FontStyle.Bold);
            foreach (var v in visualVertices.Values)
            {
                Brush br = Brushes.LightBlue;
                if (v == select) br = Brushes.Yellow;
                if (v == edge1) br = Brushes.LightGreen;

                int d = v.Radius * 2;
                gfx.FillEllipse(br, v.Position.X - v.Radius, v.Position.Y - v.Radius, d, d);
                gfx.DrawEllipse(Pens.Black, v.Position.X - v.Radius, v.Position.Y - v.Radius, d, d);

                var lbl = v.Id.ToString();
                var sz = gfx.MeasureString(lbl, fnt);
                gfx.DrawString(lbl, fnt, Brushes.Black, v.Position.X - sz.Width * 0.5f, v.Position.Y - sz.Height * 0.5f);
            }
        }

        private void PaintArrow(Graphics g, Pen p, Point from, Point to, int rad)
        {
            double dx = to.X - from.X;
            double dy = to.Y - from.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist < 0.1) return;

            dx = dx / dist;
            dy = dy / dist;

            var p1 = new Point(from.X + (int)(dx * rad), from.Y + (int)(dy * rad));
            var p2 = new Point(to.X - (int)(dx * rad), to.Y - (int)(dy * rad));

            g.DrawLine(p, p1, p2);

            double asize = 15;
            double aang = 0.5236;
            double lineang = Math.Atan2(dy, dx);

            var ap1 = new Point(p2.X - (int)(asize * Math.Cos(lineang - aang)), p2.Y - (int)(asize * Math.Sin(lineang - aang)));
            var ap2 = new Point(p2.X - (int)(asize * Math.Cos(lineang + aang)), p2.Y - (int)(asize * Math.Sin(lineang + aang)));

            g.DrawLine(p, p2, ap1);
            g.DrawLine(p, p2, ap2);
        }

        private GraphVertex GetVertexAt(Point pt)
        {
            foreach (var v in visualVertices.Values)
            {
                double dx = pt.X - v.Position.X;
                double dy = pt.Y - v.Position.Y;
                double d = Math.Sqrt(dx * dx + dy * dy);
                if (d <= v.Radius) return v;
            }
            return null;
        }

        private void OnMouseDown(object s, MouseEventArgs e)
        {
            var hit = GetVertexAt(e.Location);

            if (delVertex)
            {
                if (hit != null)
                {
                    myGraph.RemoveVertex(hit.Id);
                    visualVertices.Remove(hit.Id);
                    canvas.Refresh();
                }
                delVertex = false;
                return;
            }

            if (addEdge || delEdge)
            {
                if (hit != null)
                {
                    if (edge1 == null)
                    {
                        edge1 = hit;
                        canvas.Refresh();
                    }
                    else
                    {
                        if (addEdge) myGraph.AddEdge(edge1.Id, hit.Id);
                        if (delEdge) myGraph.RemoveEdge(edge1.Id, hit.Id);
                        edge1 = null;
                        addEdge = false;
                        delEdge = false;
                        canvas.Refresh();
                    }
                }
                return;
            }

            if (hit != null)
            {
                drag = hit;
                offset = new Point(e.X - hit.Position.X, e.Y - hit.Position.Y);
            }
        }

        private void OnMouseMove(object s, MouseEventArgs e)
        {
            if (drag != null && e.Button == MouseButtons.Left)
            {
                drag.Position = new Point(e.X - offset.X, e.Y - offset.Y);
                canvas.Refresh();
            }
        }

        private void OnMouseUp(object s, MouseEventArgs e)
        {
            drag = null;
        }

        private void DoAddVertex()
        {
            var r = new Random();
            int vx = r.Next(50, 650);
            int vy = r.Next(50, 550);
            int id = myGraph.AddVertex();
            visualVertices[id] = new GraphVertex(id, new Point(vx, vy));
            canvas.Refresh();
        }

        private void DoRemoveVertexMode()
        {
            delVertex = true;
            edge1 = null;
            addEdge = false;
            delEdge = false;
        }

        private void DoAddEdgeMode()
        {
            addEdge = true;
            delEdge = false;
            delVertex = false;
            edge1 = null;
            canvas.Refresh();
        }

        private void DoRemoveEdgeMode()
        {
            delEdge = true;
            addEdge = false;
            delVertex = false;
            edge1 = null;
            canvas.Refresh();
        }

        private void DoClear()
        {
            myGraph = new Graph();
            visualVertices.Clear();
            outputBox.Text = "";
            canvas.Refresh();
        }

        private void DoBFS()
        {
            if (int.TryParse(startInput.Text, out int vid))
            {
                if (visualVertices.ContainsKey(vid))
                {
                    var result = myGraph.BFS(vid);
                    outputBox.Text = "BFS: " + string.Join(" -> ", result);
                }
                else
                    MessageBox.Show("Вершина не найдена");
            }
            else
                MessageBox.Show("Неверный ID");
        }

        private void DoDFS()
        {
            if (int.TryParse(startInput.Text, out int vid))
            {
                if (visualVertices.ContainsKey(vid))
                {
                    var result = myGraph.DFS(vid);
                    outputBox.Text = "DFS: " + string.Join(" -> ", result);
                }
                else
                    MessageBox.Show("Вершина не найдена");
            }
            else
                MessageBox.Show("Неверный ID");
        }
    }
}
