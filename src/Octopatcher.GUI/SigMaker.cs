
using System;
using System.Drawing;
using System.Windows.Forms;
using MetroFramework.Forms;
using Octodiff.CommandLine;

namespace Octopatcher.GUI
{
	/// <summary>
	/// Description of SigMaker.
	/// </summary>
	public partial class SigMaker : MetroForm
	{
		string basisFile;
		string sigFile;
		public SigMaker()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		void SigMakerLoad(object sender, EventArgs e)
		{
			
		}
		void Button2Click(object sender, EventArgs e)
		{
			basisFile = browseForOpenFile("All Files (*.*)|*.*","Select Basis File");
			if (basisFile != null)
				label1.Text = System.IO.Path.GetFileName(basisFile);
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
			sigFile = browseForSaveFile("Signature Files (*.sig)|*.sig|All Files (*.*)|*.*","Select Output File");
			if (basisFile == null || sigFile == null)
			{ MessageBox.Show("Not enough arguments"); return; }
			new OperationInProgressIndicator(RunSignatureCommand).Show();
		}
		int RunSignatureCommand()
		{
			SignatureCommand sigComm = new SignatureCommand();
			return sigComm.Execute(new string[]{basisFile, sigFile, "2048", "false"});
		}
	}
}
