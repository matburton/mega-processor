
# Assembler.MegaProcessor

Provides extensions over the architecture agnostic `Assembler.Core` to model
the MegaProcessor's instruction set as a domain specific language as well as
providing other marginally higher level extension methods.

You're also encouraged to create your own extension methods over these
extension methods if the binary you're assembling has reoccurring patterns.

## Notable concepts

### `Calculation`

A calculation can implicitly be created from an `int`, a `Reference` or from
something more elaborate such as `someRef + 2 * 6`. This allows extension
methods over `.AddLines` to be created which accept a `Calculation`, avoiding
needing many similar overloads.

### `Register`

Instructions can often be used in tandem with one or more registers, these are
exposed by instances in `Register` and are typed to make it clearer which
registers can be used at any particular point. It's _expected_ you add
`using static Register;` to each file so that you can use `R1` etc directly.

### `Condition`

The `.GoToIf` extension method allows you to add conditionally branching
instructions. The conditions for this are available as instances in `Condition`.
It's _recommended_ you ass `using C = Condition;` to each file for terser use.

### Instruction Set

The instruction set is minimally abstracted to make each available operation
more obvious, including in terms of the number of cycles consumed and the number
of binary bytes it'll incur. Each extension method has XML comments which can
be quickly viewed in any competent IDE.

Here's a non-exhaustive list of single instruction extension methods:
* `.Set*` e.g. `.SetByteValue(R0, someReference * 2)`
* `.AddConst` e.g. `.AddConst(R2, -2)`
* `.Copy*` e.g.
    * `.CopyTo(R2, R1)`
    * `CopyByteToIndex(R2, R1, bumpIndex: true)`
    * `CopyWordFromStack(m_Variables.Tail.Head, R3)`
    * `CopyByteTo(someReference + 3, R0)`
* `.PushToStack` and `.PopFromStack`
* `.GoTo*` e.g.
    * `.GoTo(someReference)`
    * `.GoToIf(C.Equal, 0x4)`
    * `.GoToIf(C.User.Zero, out var someNewReference)`
* `.StackAdd` e.g. `.StackAdd(m_Variables.TotalBytes)`
* `.CallRoutine*` e.g. `.CallRoutine(someReference)`
* `.ReturnFrom*` e.g. `.ReturnFromRoutine()`

## Helpful extensions

### `Routine`

Defines the position of a routine in the binary by accepting or returning a
`Reference` instance. You supply a method or lambda which expresses the
instructions in this routine. A `.ReturnFromRoutine` instruction is
automatically inserted at the end of lambda.

In many some cases you might be able to choose whether to call or inline the
instructions in a routine by instead using an overload of `.Append`.

### `Loop`

Much like `.Append` but automatically defines a `Rereference` instance _before_
the instructions expressed by your nominated method or lambda. You can use this
with `.Append` create sections which can branch back to the start or end of the
section at any point.

### `DefineGlobals`

Defines a `Reference` instance at the current position and optionally reserves
sufficient space for an instance returned by `Variables.ByteSizesToOffsets`.

For instance:
```csharp
public static class SomeModule
{
    public static Assembly Build(Assembly) => a
        .DefineGlobals(out var globals, Offsets)
        .SetWordValue(R0, 0x1337)
        .CopyWordTo(globals + Offsets.ThingB, R0);

    private sealed record Globals(ThingA = 1, ThingB = 2);

    private static readonly Offsets =
        Variables.ByteSizesToOffsets(new Globals());
}
```

### `AddWords`

It's common to include blocks of static data in your binary, such a pre-computed
lookup tables or bitmaps to copy to the display RAM. This defines a `Reference`
instance at the current position.

### `ToIntelHex`

An extension over a stream of `OutputLine`s as returned by `Assembly.Assemble`.
This returns a stream of string which when separated by new lines and saved
to a file can be loaded in to the official MegaProcessor simulator or in to the
MegaProcessor itself.

### `ToListing`

An extension over a stream of `OutputLine`s as returned by `Assembly.Assemble`.
This returns a human-readable stream of strings which contains all the bytes
in the built binary along with all the comments. It's recommended to use this
in conjunction with `CollapseRepeats`.

### `Program.Main`

You're here to express your MegaProcessor program, not add the finishing touches
to an assembler. To that end `Program.Main` is available and provides the
expected ways of interacting with the assembler.

Your entry assembly can hence have a `Program.cs` which contains only:
```csharp
return Assembler.MegaProcessor.Program.Main(args, BuildMyStuff);
```
Where `BuildMyStuff` is something which takes an empty `Assembly` instance and
returns an `Assembly` instance populated, such that `.Assemble` will return your
binary.