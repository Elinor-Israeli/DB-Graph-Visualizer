using DBGraphVisualizer.Models;
using DBGraphVisualizer.Services;
using Microsoft.Msagl.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Msagl.Core.Geometry.Curves;


namespace DBGraphVisualizer
{
    public partial class GraphForm : Form
    {
        private Graph _graph;
        private Dictionary<Edge, string> _edgeDescriptions = new();

        private StyledTooltip styledTooltip = new StyledTooltip();

        public GraphForm()
        {
            InitializeComponent();
            Load += GraphForm_Load;
        }

        private void GraphForm_Load(object sender, EventArgs e)
        {
            Dictionary<string, Table> tables = SchemaLoader.LoadTables();
            List<Relationship> relationships = SchemaLoader.LoadRelationships();

            _graph = new Graph("Database Schema");
            AddTableNodes(_graph, tables);
            AddEdgesWithDescriptions(_graph, relationships);

            _graph.LayoutAlgorithmSettings = new Microsoft.Msagl.Layout.Layered.SugiyamaLayoutSettings
            {
                LayerSeparation = 100,
                NodeSeparation = 100
            };

            viewer.Graph = _graph;
            viewer.ObjectUnderMouseCursorChanged += Viewer_ObjectUnderMouseCursorChanged;
        }

        private void AddTableNodes(Graph graph, Dictionary<string, Table> tables)
        {
            foreach (var table in tables.Values)
            {
                var node = graph.AddNode(table.Name);
                node.LabelText = table.Name;
                node.Attr.Shape = Shape.DrawFromGeometry;
                node.DrawNodeDelegate = new DelegateToOverrideNodeRendering(DrawCustomNode);
                node.NodeBoundaryDelegate = new DelegateToSetNodeBoundary(GetNodeBoundary);
                node.UserData = table;
            }
        }

        private void AddEdgesWithDescriptions(Graph graph, List<Relationship> relationships)
        {
            foreach (var rel in relationships)
            {
                var edge = graph.AddEdge(rel.FromTable, rel.ToTable);
                edge.Attr.ArrowheadAtTarget = ArrowStyle.None;
                edge.DrawEdgeDelegate = new DelegateToOverrideEdgeRendering(DrawCustomEdge);
                _edgeDescriptions[edge] = $"{rel.FromTable}.{rel.FromColumn} ➝ {rel.ToTable}.{rel.ToColumn}";
            }
        }

        private ICurve GetNodeBoundary(Microsoft.Msagl.Drawing.Node node)
        {
            float width = 120;
            float height = 30;

            if (node.UserData is Table table)
            {
                using var g = CreateGraphics();
                using var font = new Font("Segoe UI", 9, System.Drawing.FontStyle.Bold);

                var headerSize = g.MeasureString(node.LabelText, font);
                height += headerSize.Height;

                foreach (var key in table.PrimaryKeys)
                {
                    var keySize = g.MeasureString(key, font);
                    height += keySize.Height + 6;
                    width = Math.Max(width, keySize.Width + 20);
                }

                width = Math.Max(width, headerSize.Width + 20);
            }

            return Microsoft.Msagl.Core.Geometry.Curves.CurveFactory.CreateRectangle(width, height, new Microsoft.Msagl.Core.Geometry.Point());
        }

        private void Viewer_ObjectUnderMouseCursorChanged(object sender, ObjectUnderMouseCursorChangedEventArgs e)
        {
            if (viewer.ObjectUnderMouseCursor?.DrawingObject is Edge edge)
            {
                if (_edgeDescriptions.TryGetValue(edge, out var desc))
                {
                    Point mouseScreen = Cursor.Position;
                    styledTooltip.ShowTooltip(desc, new Point(mouseScreen.X + 10, mouseScreen.Y + 10));
                }
            }
            else
            {
                styledTooltip.HideTooltip();
            }
        }


        private bool DrawCustomNode(Microsoft.Msagl.Drawing.Node node, object graphics)
        {
            var g = (Graphics)graphics;
            var geom = node.GeometryNode;
            var center = geom.Center;

            float width = (float)geom.Width;
            float height = (float)geom.Height;
            float x = (float)(center.X - width / 2);
            float y = (float)(center.Y - height / 2);

            float headerHeight = 25f;
            float cornerRadius = 10f;

            var state = g.Save();
            g.ScaleTransform(1, -1);
            g.TranslateTransform(0, -(float)(2 * center.Y));

            // Background with rounded corners
            using (var path = new GraphicsPath())
            {
                path.AddArc(x, y, cornerRadius, cornerRadius, 180, 90);
                path.AddArc(x + width - cornerRadius, y, cornerRadius, cornerRadius, 270, 90);
                path.AddArc(x + width - cornerRadius, y + height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
                path.AddArc(x, y + height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
                path.CloseFigure();

                g.FillPath(new SolidBrush(ColorTranslator.FromHtml("#fffff0")), path); // light cream
                g.DrawPath(Pens.LightGray, path);
            }

            // Header background
            g.FillRectangle(new SolidBrush(ColorTranslator.FromHtml("#2b579a")), x, y, width, headerHeight);

            // Header text
            using var headerFont = new Font("Segoe UI", 9, System.Drawing.FontStyle.Bold);
            using var headerBrush = new SolidBrush(ColorTranslator.FromHtml("#fffff0"));
            string headerText = node.LabelText ?? node.Id;
            var headerSize = g.MeasureString(headerText, headerFont);
            g.DrawString(headerText, headerFont, headerBrush, x + (width - headerSize.Width) / 2, y + 4);

            // Keys
            if (node.UserData is Table table)
            {
                using var keyFont = new Font("Segoe UI", 9, System.Drawing.FontStyle.Bold);
                using var keyBrush = new SolidBrush(ColorTranslator.FromHtml("#2b579a"));

                float yOffset = y + headerHeight + 6;

                foreach (var pk in table.PrimaryKeys)
                {
                    g.DrawString(pk, keyFont, keyBrush, x + 10, yOffset);
                    yOffset += keyFont.Height + 4;
                }
            }

            g.Restore(state);
            return true;
        }

        private bool DrawCustomEdge(Edge edge, object graphics)
        {
            var g = (Graphics)graphics;
            var curve = edge.GeometryEdge?.Curve;
            if (curve == null) return false;

            using var pen = new Pen(System.Drawing.Color.LightSlateGray, 1.5f);
            g.DrawLine(pen, (float)curve.Start.X, (float)curve.Start.Y, (float)curve.End.X, (float)curve.End.Y);

            double dx = curve.End.X - curve.Start.X;
            double dy = curve.End.Y - curve.Start.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);
            if (length == 0) return true;

            double backOffset = 5.0;
            double offsetX = backOffset * dx / length;
            double offsetY = backOffset * dy / length;

            float circleX = (float)(curve.End.X - offsetX - 3);
            float circleY = (float)(curve.End.Y - offsetY - 3);
            float radius = 6f;

            using var brush = new SolidBrush(System.Drawing.Color.LightSkyBlue);
            g.FillEllipse(brush, circleX, circleY, radius, radius);
            g.DrawEllipse(Pens.LightGray, circleX, circleY, radius, radius);

            return true;
        }
    }
}

