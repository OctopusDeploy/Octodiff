
using System;
using System.Drawing;
using System.Windows.Forms;
using MetroFramework.Forms;
using System.Threading.Tasks;

namespace Octopatcher.GUI
{
	/// <summary>
	/// Description of OperationInProgressIndicator.
	/// </summary>
	public partial class OperationInProgressIndicator : MetroForm
	{
		Func<int> actionToRun;
		public OperationInProgressIndicator(Func<int> ax)
		{
			actionToRun = ax;
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		async void OperationInProgressIndicatorLoad(object sender, EventArgs e)
		{
            var aEcode = new Task<int>(actionToRun);
            aEcode.Start();
            int exitCode = await aEcode;
			if (exitCode == 0)
				MessageBox.Show("The operation completed successfully.");
			else
				MessageBox.Show(String.Format("The operation exited with code {0}",exitCode));
			this.Close();
		}
	}
}
