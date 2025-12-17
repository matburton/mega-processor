
namespace Assembler.Core.References;

public sealed class Reference([CallerFilePath] string? filePath = null,
                              [CallerLineNumber] int lineNumber = -1)
{
    internal string? Label { get; } =
        Caller.ToComment(filePath: filePath, lineNumber: lineNumber);
}