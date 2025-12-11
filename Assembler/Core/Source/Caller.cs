
namespace Assembler.Core;

public static class Caller
{
    public static string? ToComment(string? referenceProse = null,
                                    string? appendProse = null,
                                    string? filePath = null,
                                    int lineNumber = -1,
                                    string? type = null)
    {
        if (appendProse?.Contains('\n') is true) appendProse = null;

        if (referenceProse?.StartsWith("var ") is true)
        {
            referenceProse = referenceProse[4 ..];
        }

        var stringBuilder = new StringBuilder();

        Append(referenceProse);
        Append(type, prefix: " ");
        AddIfAny(":");
        Append(appendProse, prefix: " ");

        if (filePath is not null && lineNumber >= 0)
        {
            var fileName = filePath.Split('/', '\\').Last();

            Append($"<{fileName}:{lineNumber}>", prefix: " ");
        }

        return stringBuilder.Length is 0 ? null : $"{stringBuilder}";

        void Append(string? text, string? prefix = null)
        {
            if (text is null) return;

            AddIfAny(prefix);

            stringBuilder.Append(text);
        }

        void AddIfAny(string? spacer)
        {
            if (stringBuilder.Length is not 0) stringBuilder.Append(spacer);
        }
    }
}