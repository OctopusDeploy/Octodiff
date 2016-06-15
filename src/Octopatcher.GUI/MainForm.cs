﻿
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MetroFramework.Forms;
using Octodiff.CommandLine;

namespace Octopatcher.GUI
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : MetroForm
	{
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		void Button1Click(object sender, EventArgs e)
		{
			new SigMaker().ShowDialog();
		}
		void Button2Click(object sender, EventArgs e)
		{
			new PatchCreator().ShowDialog();
		}
		void Button3Click(object sender, EventArgs e)
		{
			new PatchApplier().ShowDialog();
		}
	}
}
