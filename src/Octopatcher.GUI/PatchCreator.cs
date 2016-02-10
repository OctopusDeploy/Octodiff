
using System;
using System.Drawing;
using System.Windows.Forms;
using MetroFramework.Forms;
using Octodiff.CommandLine;

namespace Octopatcher.GUI
{
	/// <summary>
	/// Description of PatchCreator.
	/// </summary>
	public partial class PatchCreator : MetroForm
	{
		string sigFile;
		string changedFile;
		string delFile;
		public PatchCreator()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
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
			delFile = browseForSaveFile("Patch Files (*.1337)|*.1337|All Files (*.*)|*.*","Select Output File");
			if (sigFile == null || changedFile == null || delFile == null)
			{ MessageBox.Show("Not enough arguments"); return; }
			new OperationInProgressIndicator(RunCommand).Show();
		}
		int RunCommand()
		{
			DeltaCommand cmd = new DeltaCommand();
			return cmd.Execute(new string[]{sigFile, changedFile, delFile, "false"});
		}
		void PatchCreatorLoad(object sender, EventArgs e)
		{
	
		}
		void Button2Click(object sender, EventArgs e)
		{
			sigFile = browseForOpenFile("Signature (*.sig)|*.sig|All Files (*.*)|*.*","Select Original File Signature");
			if (sigFile != null)
				label1.Text = System.IO.Path.GetFileName(sigFile);
		}
		void Button3Click(object sender, EventArgs e)
		{
			changedFile = browseForOpenFile("All Files (*.*)|*.*","Select Modified File");
			if (changedFile != null)
				label2.Text = System.IO.Path.GetFileName(changedFile);
		}
	}
}
