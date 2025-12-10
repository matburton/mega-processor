
using Assembler.Core.References;

namespace Assembler.MegaProcessor;

public static class ReferenceExtensions
{
    extension (Reference)
    {
        public static Calculation operator + (Reference r, int offset) =>
            new (references => references.GetAddress(r) + offset);

        public static Calculation operator + (int offset, Reference r) =>
            new (references => references.GetAddress(r) + offset);

        public static Calculation operator - (Reference r, int offset) =>
            new (references => references.GetAddress(r) - offset);
    }
}