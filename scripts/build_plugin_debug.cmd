set old_wd=%CD%
cd %~dp0
cd ../

dotnet publish src/Core/Core.csproj -c Debug -o bin/publish
dotnet pack src/ReSharperPlugin/ReSharperPlugin.csproj -c Debug -o ../../

cd %old_wd%