set old_wd=%CD%
cd %~dp0
cd ../

dotnet build test/src/PDASimulator.Tests/PDASimulator.Tests.csproj -c Release
dotnet build test/src/ReSharperPlugin.Tests/ReSharperPlugin.Tests.csproj -c Release

dotnet msbuild test/src/PDASimulator.Tests/PDASimulator.Tests.csproj -t:RunTestsAppVeyor -p:Configuration=Release
dotnet msbuild test/src/ReSharperPlugin.Tests/ReSharperPlugin.Tests.csproj -t:RunTestsAppVeyor -p:Configuration=Release

cd %old_wd%

if %errorlevel% neq 0 exit /b %errorlevel%
