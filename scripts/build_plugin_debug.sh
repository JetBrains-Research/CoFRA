old_wd=$(pwd)
cd `dirname $0`
cd ../

dotnet publish src/Core/Core.csproj -c Debug -o bin/publish
dotnet pack src/ReSharperPlugin/RiderPlugin.csproj -c Debug -o ../../

cd $old_wd
