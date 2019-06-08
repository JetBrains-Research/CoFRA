old_wd=$(pwd)
cd `dirname $0`
cd ../

mkdir unzipped
mkdir tmp
mkdir "tmp/Cofra.RiderPlugin.0.1.0"
mkdir "tmp/Cofra.RiderPlugin.0.1.0/dotnet"

cp -a "./scripts/data/Rider/." "./tmp/Cofra.RiderPlugin.0.1.0"

unzip Cofra.RiderPlugin.0.1.0.nupkg -d "./unzipped"
chmod -R 775 ./unzipped
cp -a "./unzipped/DotFiles/." "./tmp/Cofra.RiderPlugin.0.1.0/dotnet"

cd tmp
zip -r ../Cofra.RiderPlugin.0.1.0.zip ./Cofra.RiderPlugin.0.1.0
cd ../

rm -rf ./unzipped
rm -rf ./tmp

cd $old_wd
