dotnet build test/src/PDASimulator.Tests/PDASimulator.Tests.csproj -c Release
dotnet build test/src/ReSharperPlugin.Tests/ReSharperPlugin.Tests.csproj -c Release

dotnet msbuild test/src/PDASimulator.Tests/PDASimulator.Tests.csproj -t:RunTestsAppVeyor -p:Configuration=Release
dotnet msbuild test/src/ReSharperPlugin.Tests/ReSharperPlugin.Tests.csproj -t:RunTestsAppVeyor -p:Configuration=Release