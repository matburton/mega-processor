
namespace Assembler.MegaChess.Draw;

internal sealed class Piece
{
    public sealed class References
    {
        public Reference Draw { get; } = new ();
    };

    public References Refs { get; } = new ();

    public Assembly Build(Assembly a) => a
        // TODO
        .DefineReference(Refs.Draw)
        ;
}