
namespace Octopatcher.GUI
{
	partial class OperationInProgressIndicator
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ProgressBar progressBar1;
		
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OperationInProgressIndicator));
			this.label1 = new System.Windows.Forms.Label();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 54);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(210, 23);
			this.label1.TabIndex = 0;
			this.label1.Text = "Operation in Progress...";
			// 
			// progressBar1
			// 
			this.progressBar1.BackColor = System.Drawing.Color.White;
			this.progressBar1.ForeColor = System.Drawing.Color.DeepPink;
			this.progressBar1.Location = new System.Drawing.Point(24, 99);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(210, 23);
			this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			this.progressBar1.TabIndex = 1;
			// 
			// OperationInProgressIndicator
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(257, 145);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.label1);
			this.DisplayHeader = false;
			this.DisplayTitle = true;
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "OperationInProgressIndicator";
			this.Padding = new System.Windows.Forms.Padding(20, 30, 20, 20);
			this.ShadowType = MetroFramework.Forms.MetroFormShadowType.None;
			this.Text = "OctoPatcher";
			this.Load += new System.EventHandler(this.OperationInProgressIndicatorLoad);
			this.ResumeLayout(false);

		}
	}
}
