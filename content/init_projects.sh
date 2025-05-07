#!/bin/sh

BASENAME=Bat
PWD=$(pwd)

# Create a new solution
dotnet new sln -o ${BASENAME}

# Create base projects
cd ${BASENAME}
dotnet new classlib -o ${BASENAME}.Libs
dotnet new classlib -o ${BASENAME}.Shared
dotnet new webapi -o ${BASENAME}.Shared.Api
dotnet new classlib -o ${BASENAME}.Shared.EF
dotnet new webapi -o ${BASENAME}.Api
dotnet new blazor -o ${BASENAME}.Blazor --interactivity Auto --all-interactive
cd ${BASENAME}.Blazor && dotnet new razorclasslib -o ${BASENAME}.Blazor.App && cd ..

# Add projects to solution
## add the Blazor (server) project first to make it the default project
dotnet sln add ${BASENAME}.Blazor/${BASENAME}.Blazor/${BASENAME}.Blazor.csproj
dotnet sln add ${BASENAME}.Blazor/${BASENAME}.Blazor.App/${BASENAME}.Blazor.App.csproj
dotnet sln add ${BASENAME}.Blazor/${BASENAME}.Blazor.Client/${BASENAME}.Blazor.Client.csproj
dotnet sln add ${BASENAME}.Libs/${BASENAME}.Libs.csproj
dotnet sln add ${BASENAME}.Shared/${BASENAME}.Shared.csproj
dotnet sln add ${BASENAME}.Shared.Api/${BASENAME}.Shared.Api.csproj
dotnet sln add ${BASENAME}.Shared.EF/${BASENAME}.Shared.EF.csproj
dotnet sln add ${BASENAME}.Api/${BASENAME}.Api.csproj

# Add references
dotnet add ${BASENAME}.Shared/${BASENAME}.Shared.csproj reference ${BASENAME}.Libs/${BASENAME}.Libs.csproj
dotnet add ${BASENAME}.Shared.EF/${BASENAME}.Shared.EF.csproj reference ${BASENAME}.Shared/${BASENAME}.Shared.csproj
dotnet add ${BASENAME}.Shared.Api/${BASENAME}.Shared.Api.csproj reference ${BASENAME}.Shared/${BASENAME}.Shared.csproj
dotnet add ${BASENAME}.Shared.Api/${BASENAME}.Shared.Api.csproj reference ${BASENAME}.Shared.EF/${BASENAME}.Shared.EF.csproj
dotnet add ${BASENAME}.Api/${BASENAME}.Api.csproj reference ${BASENAME}.Shared.Api/${BASENAME}.Shared.Api.csproj
dotnet add ${BASENAME}.Blazor/${BASENAME}.Blazor.App/${BASENAME}.Blazor.App.csproj reference ${BASENAME}.Shared/${BASENAME}.Shared.csproj
dotnet add ${BASENAME}.Blazor/${BASENAME}.Blazor.Client/${BASENAME}.Blazor.Client.csproj reference ${BASENAME}.Blazor/${BASENAME}.Blazor.App/${BASENAME}.Blazor.App.csproj
dotnet add ${BASENAME}.Blazor/${BASENAME}.Blazor/${BASENAME}.Blazor.csproj reference ${BASENAME}.Api/${BASENAME}.Api.csproj
dotnet add ${BASENAME}.Blazor/${BASENAME}.Blazor/${BASENAME}.Blazor.csproj reference ${BASENAME}.Blazor/${BASENAME}.Blazor.App/${BASENAME}.Blazor.App.csproj
dotnet add ${BASENAME}.Blazor/${BASENAME}.Blazor/${BASENAME}.Blazor.csproj reference ${BASENAME}.Blazor/${BASENAME}.Blazor.Client/${BASENAME}.Blazor.Client.csproj
cd ${PWD}

## create demo projects
cd ${BASENAME}
dotnet new classlib -o ${BASENAME}.Demo.Shared
dotnet new webapi -o ${BASENAME}.Demo.Api
cd ${BASENAME}.Blazor && dotnet new razorclasslib -o ${BASENAME}.Blazor.Demo.App && cd ..
## add demo projects to solution
dotnet sln add ${BASENAME}.Demo.Shared/${BASENAME}.Demo.Shared.csproj
dotnet sln add ${BASENAME}.Demo.Api/${BASENAME}.Demo.Api.csproj
dotnet sln add ${BASENAME}.Blazor/${BASENAME}.Blazor.Demo.App/${BASENAME}.Blazor.Demo.App.csproj
## demo projects references
dotnet add ${BASENAME}.Demo.Shared/${BASENAME}.Demo.Shared.csproj reference ${BASENAME}.Shared/${BASENAME}.Shared.csproj
dotnet add ${BASENAME}.Demo.Shared/${BASENAME}.Demo.Shared.csproj reference ${BASENAME}.Shared.EF/${BASENAME}.Shared.EF.csproj
dotnet add ${BASENAME}.Demo.Api/${BASENAME}.Demo.Api.csproj reference ${BASENAME}.Demo.Shared/${BASENAME}.Demo.Shared.csproj
dotnet add ${BASENAME}.Demo.Api/${BASENAME}.Demo.Api.csproj reference ${BASENAME}.Shared.Api/${BASENAME}.Shared.Api.csproj
dotnet add ${BASENAME}.Blazor/${BASENAME}.Blazor.Demo.App/${BASENAME}.Blazor.Demo.App.csproj reference ${BASENAME}.Demo.Shared/${BASENAME}.Demo.Shared.csproj
dotnet add ${BASENAME}.Blazor/${BASENAME}.Blazor.Demo.App/${BASENAME}.Blazor.Demo.App.csproj reference ${BASENAME}.Blazor/${BASENAME}.Blazor.App/${BASENAME}.Blazor.App.csproj
## base projects --> demo projects
dotnet add ${BASENAME}.Api/${BASENAME}.Api.csproj reference ${BASENAME}.Demo.Api/${BASENAME}.Demo.Api.csproj
dotnet add ${BASENAME}.Blazor/${BASENAME}.Blazor/${BASENAME}.Blazor.csproj reference ${BASENAME}.Demo.Api/${BASENAME}.Demo.Api.csproj
dotnet add ${BASENAME}.Blazor/${BASENAME}.Blazor/${BASENAME}.Blazor.csproj reference ${BASENAME}.Blazor/${BASENAME}.Blazor.Demo.App/${BASENAME}.Blazor.Demo.App.csproj
dotnet add ${BASENAME}.Blazor/${BASENAME}.Blazor.Client/${BASENAME}.Blazor.Client.csproj reference ${BASENAME}.Blazor/${BASENAME}.Blazor.Demo.App/${BASENAME}.Blazor.Demo.App.csproj
