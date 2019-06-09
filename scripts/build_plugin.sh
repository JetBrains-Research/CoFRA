set -e

old_wd=$(pwd)
cd `dirname $0`
cd ../

dotnet publish src/Core/Core.csproj -c Release -o bin/publish
dotnet pack src/ReSharperPlugin/RiderPlugin.csproj -c Release -o ../../

cd $old_wd
