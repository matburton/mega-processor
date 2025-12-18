
FROM mcr.microsoft.com/dotnet/sdk:10.0.101-noble AS build.container
WORKDIR /app
COPY . .
RUN --mount=type=cache,id=nuget,target=./packages \
    --mount=type=cache,id=nuget,target=/tmp/NuGetScratchroot \
    dotnet publish 'Assembler/Example/Source' \
        --configuration Release \
        --no-self-contained \
        --use-current-runtime \
        --output out

FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine3.23
WORKDIR /app
COPY --from=build.container /app/out .
USER guest
ENTRYPOINT ["dotnet", "Assembler.Example.dll"]