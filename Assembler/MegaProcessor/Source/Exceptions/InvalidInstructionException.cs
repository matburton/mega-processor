
namespace Assembler.MegaProcessor.Exceptions;

public sealed class InvalidInstructionException
    (string message, Exception? innerException = null)
    :
    Exception(message, innerException);