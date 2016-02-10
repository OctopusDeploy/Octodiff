
namespace Octopatcher.GUI
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Label label1;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(6, 242);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(294, 23);
			this.label1.TabIndex = 0;
			this.label1.Text = "(c) 2016, 0xFireball. Core code is from the OctoDiff project.";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(376, 266);
			this.Controls.Add(this.label1);
			this.DisplayHeader = false;
			this.DisplayTitle = true;
			this.Name = "MainForm";
			this.Padding = new System.Windows.Forms.Padding(20, 30, 20, 20);
			this.Text = "OctoPatcher";
			this.ResumeLayout(false);

		}
	}
}
