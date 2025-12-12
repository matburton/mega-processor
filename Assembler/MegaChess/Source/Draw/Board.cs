
namespace Assembler.MegaChess.Draw;

internal sealed class Board
{
    public sealed class References
    {
        public Reference Draw { get; } = new ();
    };

    public References Refs { get; } = new ();

    public Assembly Build(Assembly a) => a
        .DefineReference(Refs.Draw)
        // TODO
        ;
}