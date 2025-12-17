
# Assembler.Core

The fundamental building blocks for building a binary with inline comments.

Because this assembler exists within an existing language, C#/.NET, it can be
considered equivalent to an assembler with strong macro or scripting support.

## Notable concepts

### `Assembly`

An immutable builder to which you can add lines of known bytes, yet unknown
bytes, and comments. Unknown bytes are computed once `.Assemble` is called.

### `Reference`

Acts much like a label in traditional assembly, but it lexically scoped within
this program. Can be created with `new` or with `.DeclareReference` or inline
with some extension methods. You must call `.DefineReference`, or an extension
that makes that call, once for each reference you use.

### `Line`

You can call `.AddLines` on an `Assembly` but it's expected that extension
methods will be provided for specific use cases. Each `Line` can contain a
number of ordered `Fragment`s and an optional comment. Each `Fragment` has a set
number of bytes and can either be a:
* `ByteFragment` - Bytes within this have known values when it's created
* `ReferenceFragment` - Byte values within this are calculated later when the
                        address of one or more `Reference`s are known
### `OutputLine`

A stream of these are returned when you can `.Assemble` on an `Assembly`. Each
can contain bytes and a comment.

## Helpful extensions

### `DeclareReference` and `DefineReference`

You can call `.DeclareReference` to create a `Reference` instance without
leaving the fluent syntax often used in conjunction with an `Assembly` instance.

You can use an overload of `.DefineReference` to create a `Reference` and
define its address to be the current `Assembly` position in a single call.

### `Append`

Allows you to better structure the building of your binary while still using
fluent syntax. This simply takes another method or lambda which expresses the
lines which will be added at this point. `.Append` will also add some auto
generated comments which can make debugging easier.

An overload of `.Append` defines a `Reference` instance at the end of given
lambda. This allows you to model common branching constructs like an `if` block
by inserting the lines needed to go to the end of the `.Append` lambda if some
condition is met. It can also be used to inline routines rather than call them.

### `Repeat`

It's common to need to repeat lines or higher level extension method calls,
e.g. to unroll a loop. You can call `.Repeat` and specify the number of repeats
and the lambda to create each.

### `Variables`

It's common to group multiple values together and:
* Reserve positions in the binary for them, i.e. globals
* Place them together on the/a stack
* Place them together on a heap

To make this more elegant `Variables.ByteSizesToOffsets` is provided. It accepts
an object or array which can have other objects or arrays nested within. The
objects can be anonymous, records, tuples etc. Each object or array must then
contain only values, properties or fields of type `int`. The value of each `int`
you provide is the number of bytes required to  store that value.

`ByteSizesToOffsets` returns you an instance of the same type but with all the
`int` values instead set to be an individual offset, the number of bytes that
must be skipped to obtain that value. This instance also provides a
`.TotalBytes` property to make allocations a breeze.

As an example:
```csharp
record Point(int X = 2, int Y = 1);

var offsets = Variables.ByteSizesToOffsets
    (new { Head = new Point(),
           Tail = new Point() });

Console.WriteLine(offsets.Tail.Y); // Prints '5'

Console.WriteLine(offsets.TotalBytes); // Prints '6'
```

## `CollapseRepeats`

An extension over a stream of `OutputLine`s as returned by `Assembly.Assemble`.
This tries to spot repeated patterns of equivalent `OutputLine`s and express
them in a more terse way, preserving all the bytes but combining the comments.