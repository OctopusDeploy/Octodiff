
using System;
using System.Drawing;
using System.Windows.Forms;
using MetroFramework.Forms;
using Octodiff.CommandLine;

namespace Octopatcher.GUI
{
	/// <summary>
	/// Description of PatchApplier.
	/// </summary>
	public partial class PatchApplier : MetroForm
	{
		string delFile;
		string basisFile;
		string newFile;
		public PatchApplier()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		void PatchApplierLoad(object sender, EventArgs e)
		{
	
		}
		
		string browseForSaveFile(string filter, string title)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Filter = filter;
			sfd.Title = title;
			if (sfd.ShowDialog() == DialogResult.OK)
			{
				return sfd.FileName;
			}
			return null;
		}
		string browseForOpenFile(string filter, string title)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = filter;
			ofd.Title = title;
			ofd.Multiselect = false;
			if (ofd.ShowDialog() == DialogResult.OK)
			{
				return ofd.FileName;
			}
			return null;
		}
		void Button1Click(object sender, EventArgs e)
		{
			newFile = browseForSaveFile("All Files (*.*)|*.*","Select Output File");
			if (basisFile == null || newFile == null || delFile == null)
			{ MessageBox.Show("Not enough arguments"); return; }
			new OperationInProgressIndicator(RunCommand).Show();
		}
		int RunCommand()
		{
			PatchCommand cmd = new PatchCommand();
			return cmd.Execute(new string[]{basisFile, delFile, newFile, "false"});
		}
		void PatchCreatorLoad(object sender, EventArgs e)
		{
	
		}
		void Button2Click(object sender, EventArgs e)
		{
			basisFile = browseForOpenFile("All Files (*.*)|*.*","Select Original File");
			if (basisFile != null)
				label1.Text = System.IO.Path.GetFileName(basisFile);
		}
		void Button3Click(object sender, EventArgs e)
		{
			delFile = browseForOpenFile("Patch Files (*.1337)|*.1337|All Files (*.*)|*.*","Select Patch File");
			if (delFile != null)
				label2.Text = System.IO.Path.GetFileName(delFile);
		}
	}
}
