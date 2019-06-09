set old_wd=%CD%
cd %~dp0
cd ../

dotnet publish src/Core/Core.csproj -c Debug -o bin/publish -f netcoreapp2.1

dotnet pack src/ReSharperPlugin/ReSharperPlugin.csproj -c Debug -o ../../
dotnet pack src/ReSharperPlugin/RiderPlugin.csproj -c Debug -o ../../

cd %old_wd%