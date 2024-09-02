#!/bin/sh

BASENAME=Bat

# Create a new solution
dotnet new sln -o ${BASENAME}

# Create projects
cd ${BASENAME}
dotnet new classlib -o ${BASENAME}.Shared
dotnet new classlib -o ${BASENAME}.Shared.EF
dotnet new webapi -o ${BASENAME}.Api
dotnet new blazor -o ${BASENAME}.Blazor --interactivity Auto --all-interactive
cd ${BASENAME}.Blazor && dotnet new razorclasslib -o ${BASENAME}.Blazor.App && cd ..

# Add projects to solution
## add the Blazor (server) project first to make it the default project
dotnet sln add ${BASENAME}.Blazor/${BASENAME}.Blazor/${BASENAME}.Blazor.csproj
dotnet sln add ${BASENAME}.Blazor/${BASENAME}.Blazor.App/${BASENAME}.Blazor.App.csproj
dotnet sln add ${BASENAME}.Blazor/${BASENAME}.Blazor.Client/${BASENAME}.Blazor.Client.csproj
dotnet sln add ${BASENAME}.Shared/${BASENAME}.Shared.csproj
dotnet sln add ${BASENAME}.Shared.EF/${BASENAME}.Shared.EF.csproj
dotnet sln add ${BASENAME}.Api/${BASENAME}.Api.csproj

# Add references
dotnet add ${BASENAME}.Shared.EF/${BASENAME}.Shared.EF.csproj reference ${BASENAME}.Shared/${BASENAME}.Shared.csproj
dotnet add ${BASENAME}.Api/${BASENAME}.Api.csproj reference ${BASENAME}.Shared/${BASENAME}.Shared.csproj
dotnet add ${BASENAME}.Api/${BASENAME}.Api.csproj reference ${BASENAME}.Shared.EF/${BASENAME}.Shared.EF.csproj
#dotnet add ${BASENAME}.Blazor/${BASENAME}.Blazor/${BASENAME}.Blazor.csproj reference ${BASENAME}.Shared/${BASENAME}.Shared.csproj
dotnet add ${BASENAME}.Blazor/${BASENAME}.Blazor.App/${BASENAME}.Blazor.App.csproj reference ${BASENAME}.Shared/${BASENAME}.Shared.csproj
#dotnet add ${BASENAME}.Blazor/${BASENAME}.Blazor.Client/${BASENAME}.Blazor.Client.csproj reference ${BASENAME}.Shared/${BASENAME}.Shared.csproj
dotnet add ${BASENAME}.Blazor/${BASENAME}.Blazor/${BASENAME}.Blazor.csproj reference ${BASENAME}.Blazor/${BASENAME}.Blazor.App/${BASENAME}.Blazor.App.csproj
dotnet add ${BASENAME}.Blazor/${BASENAME}.Blazor.Client/${BASENAME}.Blazor.Client.csproj reference ${BASENAME}.Blazor/${BASENAME}.Blazor.App/${BASENAME}.Blazor.App.csproj
dotnet add ${BASENAME}.Blazor/${BASENAME}.Blazor/${BASENAME}.Blazor.csproj reference ${BASENAME}.Api/${BASENAME}.Api.csproj
