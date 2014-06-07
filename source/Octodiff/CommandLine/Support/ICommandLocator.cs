namespace Octodiff.CommandLine.Support
{
    public interface ICommandLocator
    {
        ICommandMetadata[] List();
        ICommandMetadata Find(string name);
        ICommand Create(ICommandMetadata metadata);
    }
}