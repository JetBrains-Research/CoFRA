set old_wd=%CD%
cd %~dp0
cd ../

dotnet publish src/Core/Core.csproj -c Release -o bin/publish -f netcoreapp2.1

dotnet pack src/ReSharperPlugin/ReSharperPlugin.csproj -c Release -o ../../
dotnet pack src/ReSharperPlugin/RiderPlugin.csproj -c Release -o ../../

cd %old_wd%