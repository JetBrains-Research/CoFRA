Set RelativeOutputPath="bin/Release/net472"

dotnet build test/src/PDASimulator.Tests/PDASimulator.Tests.csproj -c Release
dotnet build test/src/ReSharperPlugin.Tests/ReSharperPlugin.Tests.csproj -c Release

nunit3-console test/src/PDASimulator.Tests/%RelativeOutputPath%/PDASimulator.Tests.dll
nunit3-console test/src/ReSharperPlugin.Tests/%RelativeOutputPath%/ReSharperPlugin.Tests.dll