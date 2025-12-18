
The repository contains an assembler for the
[MegaProcessor](https://www.megaprocessor.com) as a C#/.NET library. It can be
considered equivalent to an assembler with strong macro or scripting support.

[Learn more](Assembler/README.md)

[See an example](Assembler/Example/Source/Snail.cs)

#### Run the example:

If you have `dotnet` installed, run this in the root of the repository:
```shell
dotnet run --project 'Assembler/Example/Source' -- --listing
```

If you have `docker` available:
```shell
docker buildx build . -t 'assembler.example'

docker run --rm -it --network none 'assembler.example' --listing
```