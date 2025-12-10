
namespace Assembler.Core.Exceptions;

public sealed class InvalidReferenceException
    (string message, Exception? innerException = null)
    :
    Exception(message, innerException);