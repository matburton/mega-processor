
This solution includes the following projects:

# [Assembler.Core](Core/README.md)

The fundamental building blocks for building a binary with inline comments.

Because this assembler exists within an existing language, C#/.NET, it can be
considered equivalent to an assembler with strong macro or scripting support.

[Learn more](Core/README.md)

# [Assembler.MegaProcessor](MegaProcessor/README.md)

Provides extensions over the architecture agnostic `Assembler.Core` to model
the MegaProcessor's instruction set as a domain specific language as well as
providing other marginally higher level extension methods.

You're also encouraged to create your own extension methods over these
extension methods if the binary you're assembling has reoccurring patterns.

[Learn more](MegaProcessor/README.md)