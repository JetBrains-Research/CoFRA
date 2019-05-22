set old_wd=%CD%
cd %~dp0
cd ../

mkdir tmp

copy "./Cofra.ReSharperPlugin.0.1.0.nupkg" "./tmp/Cofra.ReSharperPlugin.0.1.0.nupkg"
xcopy /e "./scripts/data/Rider" "./tmp"

powershell.exe -nologo -noprofile -command "Compress-Archive -Force -Path tmp/* -DestinationPath Cofra.RiderPlugin.0.1.0.zip"

rmdir /s /q tmp

cd %old_wd%