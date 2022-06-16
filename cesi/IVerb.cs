using System.CommandLine;

namespace cesi;

public interface IVerb
{
    public Command MakeCommand();
}