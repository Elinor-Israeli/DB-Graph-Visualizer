namespace DBGraphVisualizer
{
    partial class GraphForm
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Msagl.GraphViewerGdi.GViewer viewer;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            this.SuspendLayout();

            // viewer
            this.viewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewer.Name = "viewer";
            this.viewer.Size = new System.Drawing.Size(800, 450);
            this.viewer.TabIndex = 0;

            // Form1
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.viewer);
            this.Name = "GraphForm";
            this.Text = "Database Graph Viewer";
            this.ResumeLayout(false);
        }

        #endregion
    }
}
