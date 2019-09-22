using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using Newtonsoft.Json;
using Zicore.Neat.Base;
using Zicore.Neat.IO;
using Zicore.Neat.IO.Model;
using Zicore.Neat.Visualization.VM;

namespace Zicore.Neat.Visualization.Drawing
{
    public class NetworkRenderer : Panel
    {
        private RendererVM vm;
        private readonly DrawingGroup backingStore = new DrawingGroup();
        public IGenome Genome { get; set; } = new ExportGenome();
        private readonly Pen nodePen = new Pen(Brushes.Black, 2);
        private readonly Pen conEnabledPen = new Pen(Brushes.LightGreen, 3);
        private readonly Pen conDisabledPen = new Pen(Brushes.DarkRed, 3);
        private readonly Brush nodeBrush = Brushes.DodgerBlue;

        public NetworkRenderer()
        {
            
        }

        protected override void OnInitialized(EventArgs e)
        {
            if (DataContext is RendererVM dataContextVM)
            {
                vm = dataContextVM;
                vm.PropertyChanged += VMOnPropertyChanged;
            }
            base.OnInitialized(e);
        }

        private void VMOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            MainViewModel.UIDispatcher.Invoke(Render, DispatcherPriority.Render);
        }

        public static IGenome LoadGenome()
        {
            var filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "GenomeExport.json");
            using (System.IO.StreamReader sr = new System.IO.StreamReader(filePath))
            {
                var jsonText = sr.ReadToEnd();
                var result = Exporter.Import<ExportGenome>(jsonText);
                return result;
            }
        }

        protected override void OnRender(DrawingContext g)
        {
            base.OnRender(g);
            Render();
            g.DrawDrawing(backingStore);
        }

        public void Render()
        {
            var drawingContext = backingStore.Open();
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                if (vm?.Genome != null)
                {
                    Render(vm.Genome, ActualWidth, ActualHeight, drawingContext, conEnabledPen, conDisabledPen, nodeBrush, nodePen);
                }
            }
            drawingContext.Close();
        }

        public static void Render(IGenome genome, double width, double height, DrawingContext g, Pen conEnabledPen, Pen conDisabledPen, Brush nodeBrush, Pen nodePen)
        {
            double radius = 24;
            double diameter = radius * 2;
            double margin = 40;
            double outputRight = width;

            double widthHidden = width - margin * 2 - diameter * 2;
            double heightHidden = height - margin * 2;

            double minSpaceHidden = margin;

            var sensors = genome.Nodes.Where(x => x.Type == NodeGeneType.Sensor).ToList();
            var outputs = genome.Nodes.Where(x => x.Type == NodeGeneType.Output).ToList();
            var hidden = genome.Nodes.Where(x => x.Type == NodeGeneType.Hidden).ToList();

            var connections = genome.Connections.ToList();

            int maxIoNodeCount = Math.Max(sensors.Count, outputs.Count);

            double heightBetweenIoNodes = height / maxIoNodeCount;

            Dictionary<INodeGene, Point> nodePositions = new Dictionary<INodeGene, Point>();
            Dictionary<int, INodeGene> nodes = genome.Nodes.ToDictionary(x => x.Id, x => x);

            List<Point> randomPositions = new List<Point>();
            Random rnd = new Random();
            for (int i = 0; i < hidden.Count; i++)
            {
                double x = rnd.Next((int)margin, (int)width - (int)margin);
                double y = rnd.Next((int)margin, (int)height - (int)margin);
                randomPositions.Add(new Point(x,y));
            }


            // calculate positions
            for (int i = 0; i < sensors.Count; i++)
            {
                var node = sensors[i];
                var p = CreateIoPoint(0, i * heightBetweenIoNodes, new Thickness(margin, margin, 0, 0));
                nodePositions[node] = p;
            }

            for (int i = 0; i < outputs.Count; i++)
            {
                var node = outputs[i];
                var p = CreateIoPoint(outputRight, i * heightBetweenIoNodes, new Thickness(0, margin, margin, 0));
                nodePositions[node] = p;
            }

            double step = Math.Sqrt(heightHidden / widthHidden) * hidden.Count;
            int countPerRow = (int)(widthHidden / (step + diameter + minSpaceHidden));

            if (countPerRow == 0)
                countPerRow = 2;

            double ox = minSpaceHidden;
            double oy = minSpaceHidden;

            int t = 1;
            int j = 1;
            for (int i = 0; i < hidden.Count; i++)
            {
                if (j % countPerRow == 0)
                {
                    j = 1;
                    t++;
                }

                j++;

                var node = hidden[i];
                double px = (step + diameter + minSpaceHidden) * j;
                double py = (step + diameter + minSpaceHidden) * t;

                //var rndP = randomPositions[i];
                var rndP = new Point(px, py);

                var p = CreateIoPoint(rndP.X, rndP.Y, new Thickness(0, 0, 0, 0));
                nodePositions[node] = p;
            }

            // draw

            for (int i = 0; i < connections.Count; i++)
            {
                var con = connections[i];

                if (nodes.TryGetValue(con.Input, out var input) && nodes.TryGetValue(con.Output, out var output))
                {
                    if (nodePositions.ContainsKey(input) && nodePositions.ContainsKey(output))
                    {
                        var p1 = nodePositions[input];
                        var p2 = nodePositions[output];

                        if (con.Enabled)
                        {
                            DrawLine(g, p1, p2, conEnabledPen, radius);
                        }
                        else
                        {
                            DrawLine(g, p1, p2, conDisabledPen, radius);
                        }
                    }
                }
            }

            foreach (var n in nodes.Values)
            {
                if (nodePositions.ContainsKey(n))
                {
                    var p = nodePositions[n];
                    DrawNode(g, p, nodeBrush, nodePen, radius, n.Id);
                }
            }
        }

        public static void DrawNode(DrawingContext g, Point p, Brush brush, Pen pen, double radius, object text)
        {
            double fontSize = radius * 1.0;

            g.DrawEllipse(brush, pen, p, radius, radius);
            var formattedText = new FormattedText(text.ToString(), CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, new Typeface(new FontFamily("Calibri"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal), fontSize, Brushes.Black, 20);

            double w = formattedText.Width;
            double h = formattedText.Height;

            Point textPoint = new Point(p.X - w * 0.5, p.Y - h * 0.5);

            g.DrawText(formattedText, textPoint);
        }

        public static void DrawLine(DrawingContext g, Point p1, Point p2, Pen pen, double radius)
        {
            radius += 2;
            //g.DrawLine(pen, p1, p2);
            Vector v1 = new Vector(p1.X, p1.Y);
            Vector v2 = new Vector(p2.X, p2.Y);

            Vector dir = v2 - v1;
            dir.Normalize();

            v1 = (v1 + dir * radius);
            v2 = (v2 - dir * radius);

            p1 = new Point(v1.X, v1.Y);
            p2 = new Point(v2.X, v2.Y);

            DrawArrow(g, pen, p1, p2, 10, EndpointStyle.None, EndpointStyle.ArrowHead);

            //Vector dir = (p1 - p2) * 1;
            //dir = new Vector(-dir.Y, dir.X);

            //Point p3 = new Point(p1.X + dir.X, p1.Y + dir.Y);
            //Point p4 = new Point(p1.X + dir.X, p1.Y + dir.Y);
            //dir.Normalize();


            //var path = MakeCurve(new Point[] {new Point(10, 10 ), new Point(40, 40), new Point(90, 90), new Point(100, 100) }, 1);
            //path.Stroke = Brushes.Lime;
            //path.StrokeThickness = 2;

            //g.DrawGeometry(Brushes.Black, pen, path.Data);
        }

        // Draw an arrowhead at the given point
        // in the normalizede direction <nx, ny>.
        private static void DrawArrowhead(DrawingContext gr, Pen pen, Point p, double nx, double ny, double length)
        {
            double ax = length * (-ny - nx);
            double ay = length * (nx - ny);
            Point[] points =
            {
                new Point(p.X + ax, p.Y + ay),
                p,
                new Point(p.X - ay, p.Y + ax)
            };
            gr.DrawLine(pen, points[1], points[0]);
            gr.DrawLine(pen, points[1], points[2]);
        }

        private enum EndpointStyle
        {
            None,
            ArrowHead,
            Fletching
        }

        // Draw arrow heads or tails for the
        // segment from p1 to p2.
        private static void DrawArrow(DrawingContext gr, Pen pen, Point p1, Point p2, double length, EndpointStyle style1, EndpointStyle style2)
        {
            // Draw the shaft.
            gr.DrawLine(pen, p1, p2);

            // Find the arrow shaft unit vector.
            double vx = p2.X - p1.X;
            double vy = p2.Y - p1.Y;
            float dist = (float)Math.Sqrt(vx * vx + vy * vy);
            vx /= dist;
            vy /= dist;

            // Draw the start.
            if (style1 == EndpointStyle.ArrowHead)
            {
                DrawArrowhead(gr, pen, p1, -vx, -vy, length);
            }
            else if (style1 == EndpointStyle.Fletching)
            {
                DrawArrowhead(gr, pen, p1, vx, vy, length);
            }

            // Draw the end.
            if (style2 == EndpointStyle.ArrowHead)
            {
                DrawArrowhead(gr, pen, p2, vx, vy, length);
            }
            else if (style2 == EndpointStyle.Fletching)
            {
                DrawArrowhead(gr, pen, p2, -vx, -vy, length);
            }
        }

        // Make a Bezier curve connecting these points.
        private static Path MakeCurve(Point[] points, double tension)
        {
            if (points.Length < 2) return null;
            Point[] result_points = MakeCurvePoints(points, tension);

            // Use the points to create the path.
            return MakeBezierPath(result_points.ToArray());
        }

        // Make an array containing Bezier curve points and control points.
        private static Point[] MakeCurvePoints(Point[] points, double tension)
        {
            if (points.Length < 2) return null;
            double control_scale = tension / 0.5 * 0.175;

            // Make a list containing the points and
            // appropriate control points.
            List<Point> result_points = new List<Point>();
            result_points.Add(points[0]);

            for (int i = 0; i < points.Length - 1; i++)
            {
                // Get the point and its neighbors.
                Point pt_before = points[Math.Max(i - 1, 0)];
                Point pt = points[i];
                Point pt_after = points[i + 1];
                Point pt_after2 = points[Math.Min(i + 2, points.Length - 1)];

                double dx1 = pt_after.X - pt_before.X;
                double dy1 = pt_after.Y - pt_before.Y;

                Point p1 = points[i];
                Point p4 = pt_after;

                double dx = pt_after.X - pt_before.X;
                double dy = pt_after.Y - pt_before.Y;
                Point p2 = new Point(
                    pt.X + control_scale * dx,
                    pt.Y + control_scale * dy);

                dx = pt_after2.X - pt.X;
                dy = pt_after2.Y - pt.Y;
                Point p3 = new Point(
                    pt_after.X - control_scale * dx,
                    pt_after.Y - control_scale * dy);

                // Save points p2, p3, and p4.
                result_points.Add(p2);
                result_points.Add(p3);
                result_points.Add(p4);
            }

            // Return the points.
            return result_points.ToArray();
        }

        // Make a Path holding a series of Bezier curves.
        // The points parameter includes the points to visit
        // and the control points.
        private static Path MakeBezierPath(Point[] points)
        {
            // Create a Path to hold the geometry.
            Path path = new Path();

            // Add a PathGeometry.
            PathGeometry path_geometry = new PathGeometry();
            path.Data = path_geometry;

            // Create a PathFigure.
            PathFigure path_figure = new PathFigure();
            path_geometry.Figures.Add(path_figure);

            // Start at the first point.
            path_figure.StartPoint = points[0];

            // Create a PathSegmentCollection.
            PathSegmentCollection path_segment_collection =
                new PathSegmentCollection();
            path_figure.Segments = path_segment_collection;

            // Add the rest of the points to a PointCollection.
            PointCollection point_collection =
                new PointCollection(points.Length - 1);
            for (int i = 1; i < points.Length; i++)
                point_collection.Add(points[i]);

            // Make a PolyBezierSegment from the points.
            PolyBezierSegment bezier_segment = new PolyBezierSegment();
            bezier_segment.Points = point_collection;

            // Add the PolyBezierSegment to othe segment collection.
            path_segment_collection.Add(bezier_segment);

            return path;
        }

        public static Point CreateIoPoint(double x, double y, Thickness margin)
        {
            return new Point(x + margin.Left - margin.Right, y + margin.Top - margin.Bottom);
        }
    }
}
