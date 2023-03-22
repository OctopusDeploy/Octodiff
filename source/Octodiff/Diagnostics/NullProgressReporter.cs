namespace Octodiff.Diagnostics
{
    public class NullProgressReporter : IProgressReporter
    {
        public static NullProgressReporter Instance { get; } = new NullProgressReporter();
        
        public void ReportProgress(string operation, long currentPosition, long total)
        {
        }
    }
}