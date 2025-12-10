
namespace Assembler.Core.References;

public interface IReferences
{
    /// <exception cref="Exceptions.InvalidReferenceException" />
    ///
    [Pure]
    int GetAddress(Reference reference);

    int CurrentLineAddress { get; }
}