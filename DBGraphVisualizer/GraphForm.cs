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
    public class GraphForm : Form
    {
        private Graph _graph;
        private Dictionary<Edge, string> _edgeDescriptions = new();
        private System.ComponentModel.IContainer _components = null;
        private Microsoft.Msagl.GraphViewerGdi.GViewer _viewer;
        private StyledTooltip _styledTooltip = new StyledTooltip();

        private static readonly int ViewerWidth = Screen.PrimaryScreen.Bounds.Width;
        private static readonly int ViewerHeight = Screen.PrimaryScreen.Bounds.Height;

        private static readonly Font SugiFont = new Font("Segoe UI", 9, System.Drawing.FontStyle.Bold);
        private static readonly System.Drawing.Color LightCream = ColorTranslator.FromHtml("#fffff0");
        private static readonly System.Drawing.Color DarkBlue = ColorTranslator.FromHtml("#2b579a");

        public GraphForm()
        {
            InitializeComponent();
            this.Size = new Size(ViewerWidth, ViewerHeight);
            Load += GraphForm_Load;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (_components != null))
            {
                _components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Initializes the components and layout of the <see cref="GraphForm"/>, 
        /// including setting up the MSAGL graph viewer control and form properties.
        /// </summary>
        private void InitializeComponent()
        {
            this._viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            this.SuspendLayout();

            // viewer
            this._viewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._viewer.Name = "viewer";
            this._viewer.Size = new System.Drawing.Size(ViewerWidth, ViewerHeight);
            this._viewer.TabIndex = 0;

            // GraphForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(ViewerWidth, ViewerHeight);
            this.Controls.Add(this._viewer);
            this.Name = "GraphForm";
            this.Text = "Database Graph Viewer";
            this.ResumeLayout(false);
        }

        /// <summary>
        /// Handles the form load event by initializing and rendering the database schema graph.
        /// Loads tables and relationships from the schema, adds them to the MSAGL graph, 
        /// sets layout settings, and assigns the graph to the viewer.
        /// </summary>
        /// <param name="sender">The source of the event, typically the <see cref="GraphForm"/> itself.</param>
        /// <param name="e">Event arguments associated with the form load event.</param>
        private void GraphForm_Load(object sender, EventArgs e)
        {
            Dictionary<string, Table> tables = SchemaLoader.LoadTables();
            List<Relationship> relationships = SchemaLoader.LoadRelationships(tables);

            _graph = new Graph("Database Schema");
            AddTableNodes(_graph, tables);
            AddEdgesWithDescriptions(_graph, relationships);

            _graph.LayoutAlgorithmSettings = new Microsoft.Msagl.Layout.Layered.SugiyamaLayoutSettings
            {
                LayerSeparation = 100,
                NodeSeparation = 100
            };

            _viewer.Graph = _graph;
            _viewer.ObjectUnderMouseCursorChanged += Viewer_ObjectUnderMouseCursorChanged;
        }

        /// <summary>
        /// Adds table nodes to the MSAGL graph, assigning custom shapes, labels,
        /// boundary rendering, and user data based on the provided table metadata.
        /// </summary>
        /// <param name="graph">The MSAGL graph to which the table nodes will be added.</param>
        /// <param name="tables">A dictionary of table names and their metadata used to create nodes.</param>
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

        // <summary>
        /// Adds edges to the MSAGL graph representing relationships between tables,
        /// applies custom rendering, and stores a textual description of each relationship.
        /// </summary>
        /// <param name="graph">The MSAGL graph to which the edges will be added.</param>
        /// <param name="relationships">A list of table relationships defining the graph's edges and their cardinality.</param>
        private void AddEdgesWithDescriptions(Graph graph, List<Relationship> relationships)
        {
            foreach (var rel in relationships)
            {
                var edge = graph.AddEdge(rel.FromTable, rel.ToTable);
                edge.Attr.ArrowheadAtTarget = ArrowStyle.None;
                edge.DrawEdgeDelegate = new DelegateToOverrideEdgeRendering(DrawCustomEdge);
                
                if (rel.IsUnique)
                {
                     _edgeDescriptions[edge] = $"each **{rel.ToTable}** can only have one **{rel.FromTable}**";
                }
                else
                {
                    _edgeDescriptions[edge] = $"each **{rel.ToTable}** may have multiple **{rel.FromTable}s**";
                }
            }
        }

        // <summary>
        /// Calculates a custom rectangular boundary curve for a given MSAGL node,
        /// based on the content of its associated table (header and keys).
        /// </summary>
        /// <param name="node">The MSAGL node whose boundary is to be calculated. 
        /// The node is expected to have a <c>UserData</c> of type <c>Table</c> with a list of keys.</param>
        /// <returns>
        /// A rectangle-shaped <see cref="Microsoft.Msagl.Core.Geometry.Curves.ICurve"/> 
        /// representing the node's visual boundary, sized dynamically based on its content.
        /// </returns>
        private ICurve GetNodeBoundary(Microsoft.Msagl.Drawing.Node node)
        {
            float width = 120;
            float height = 0;

            if (node.UserData is Table table)
            {
                using var g = CreateGraphics();

                var headerSize = g.MeasureString(node.LabelText, SugiFont);
                height += headerSize.Height + 6;

                foreach (var key in table.AllKeys)
                {
                    var keySize = g.MeasureString(key, SugiFont);
                    height += keySize.Height + 6;
                    width = Math.Max(width, keySize.Width + 20);
                }

                width = Math.Max(width, headerSize.Width + 20);
            }

            return Microsoft.Msagl.Core.Geometry.Curves.CurveFactory.CreateRectangle(width, height, new Microsoft.Msagl.Core.Geometry.Point());
        }

        /// <summary>
        /// Handles changes to the object under the mouse cursor in the MSAGL viewer.
        /// If the cursor is over an edge, displays a styled tooltip with a description of the relationship.
        /// Otherwise, hides the tooltip.
        /// </summary>
        /// <param name="sender">The source of the event, typically the viewer control.</param>
        /// <param name="e">The event data containing the object currently under the mouse cursor.</param>

        private void Viewer_ObjectUnderMouseCursorChanged(object sender, ObjectUnderMouseCursorChangedEventArgs e)
        {
            if (_viewer.ObjectUnderMouseCursor?.DrawingObject is Edge edge)
            {
                if (_edgeDescriptions.TryGetValue(edge, out var desc))
                {
                    Point mouseScreen = Cursor.Position;
                    _styledTooltip.ShowTooltip(desc, new Point(mouseScreen.X + 10, mouseScreen.Y + 10));
                }
            }
            else
            {
                _styledTooltip.HideTooltip();
            }
        }

        /// <summary>
        /// Custom rendering logic for drawing a table node in the graph, including a rounded rectangle shape,
        /// a colored header, and a list of keys.
        /// </summary>
        /// <param name="node">The MSAGL node to be drawn, expected to contain a <see cref="Table"/> in its UserData.</param>
        /// <param name="graphics">The graphics object used for custom rendering.</param>
        /// <returns>Returns <c>true</c> if the node was successfully drawn.</returns>
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

                g.FillPath(new SolidBrush(LightCream), path);
                g.DrawPath(Pens.LightGray, path);
            }

            // Header background
            g.FillRectangle(new SolidBrush(DarkBlue), x, y, width, headerHeight);

            // Header text
            using var headerBrush = new SolidBrush(LightCream);
            string headerText = node.LabelText ?? node.Id;
            var headerSize = g.MeasureString(headerText, SugiFont);
            g.DrawString(headerText, SugiFont, headerBrush, x + (width - headerSize.Width) / 2, y + 4);

            // Keys
            if (node.UserData is Table table)
            {
                using var keyBrush = new SolidBrush(DarkBlue);

                float yOffset = y + headerHeight + 6;

                foreach (var pk in table.AllKeys)
                {
                    var keySize = g.MeasureString(pk, SugiFont);
                    g.DrawString(pk, SugiFont, keyBrush, x + (width - keySize.Width) / 2, yOffset);
                    yOffset += SugiFont.Height + 4;
                }
            }

            g.Restore(state);
            return true;
        }

        /// <summary>
        /// Custom rendering logic for drawing an edge (relationship) between two table nodes,
        /// including a styled line and a small circle at the start of the edge.
        /// </summary>
        /// <param name="edge">The MSAGL edge to be drawn, representing a relationship between tables.</param>
        /// <param name="graphics">The graphics object used for drawing the edge.</param>
        /// <returns>Returns <c>true</c> if the edge was successfully drawn.</returns>
        private bool DrawCustomEdge(Edge edge, object graphics)
        {
            var g = (Graphics)graphics;
            var curve = edge.GeometryEdge?.Curve;
            if (curve == null) return false;

            var pen = new Pen(System.Drawing.Color.LightSkyBlue, 0.01f);
            g.DrawLine(pen, (float)curve.Start.X, (float)curve.Start.Y, (float)curve.End.X, (float)curve.End.Y);

            double dx = curve.End.X - curve.Start.X;
            double dy = curve.End.Y - curve.Start.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);
            if (length == 0) return true;

            _viewer.Paint += (sender, e) =>
            {
                var g = e.Graphics;

                float circleX = (float)(curve.Start.X - 5);
                float circleY = (float)(curve.Start.Y - 5);

                float radius = 10f;

                var brush = new SolidBrush(System.Drawing.Color.LightSkyBlue);
                g.FillEllipse(brush, circleX, circleY, radius, radius);
                g.DrawEllipse(Pens.LightSkyBlue, circleX, circleY, radius, radius);
            };

            return true;
        }
    }
}

