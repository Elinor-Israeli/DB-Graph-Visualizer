using AdventureWorksGraph.Models;
using AdventureWorksGraph.Services;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AdventureWorksGraph
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
        }

        private void AddTableNodes(Graph graph, Dictionary<string, Table> tables)
        {
            foreach (var table in tables.Values)
            {
                string label = $"{table.Name}";

                if (table.PrimaryKeys.Count > 0)
                    label += $"\nPK:\n  {string.Join("\n  ", table.PrimaryKeys.Select(pk => pk))}";

                if (table.ForeignKeys.Count > 0)
                    label += $"\nFK:\n  {string.Join("\n  ", table.ForeignKeys.Select(fk => fk))}";

                Node node = graph.AddNode(table.Name);
                node.LabelText = label;
                node.Attr.Shape = Shape.Box;
                node.Label.FontSize = 8;
                node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.LightSkyBlue;
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            Dictionary<string, Table> tables = SchemaLoader.LoadTables();
            List<Relationship> relationships = SchemaLoader.LoadRelationships();

            Graph graph = new Graph("Database Schema");

            AddTableNodes(graph, tables);

            foreach (var rel in relationships)
            {
                var edge = graph.AddEdge(rel.FromTable, rel.ToTable);
                edge.LabelText = $"{rel.FromColumn} ➝ {rel.ToColumn}";
                edge.Attr.ArrowheadAtTarget = ArrowStyle.Normal;
            }

            Bitmap image = RenderGraphToBitmap(graph);
            pictureBox1.Image = image;

            MessageBox.Show($"Image size: {image.Width} x {image.Height}");
        }


        private Bitmap RenderGraphToBitmap(Graph graph, int padding = 20)
        {
            var renderer = new GraphRenderer(graph);
            renderer.CalculateLayout();

            var boundingBox = graph.BoundingBox;

            int width = (int)Math.Ceiling(boundingBox.Width + 2 * padding);
            int height = (int)Math.Ceiling(boundingBox.Height + 2 * padding);

            var bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(System.Drawing.Color.White);
                renderer.Render(g, padding, padding, width - 2 * padding, height - 2 * padding);
            }

            return bitmap;
        }
    }
}
