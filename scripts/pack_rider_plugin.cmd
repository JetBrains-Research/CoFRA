set old_wd=%CD%
cd %~dp0
cd ../

mkdir tmp
mkdir "tmp/Cofra.RiderPlugin.0.1.0"
mkdir "tmp/Cofra.RiderPlugin.0.1.0/dotnet"

xcopy /e "./scripts/data/Rider" "./tmp/Cofra.RiderPlugin.0.1.0"

powershell.exe -nologo -noprofile -command "& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::ExtractToDirectory('Cofra.RiderPlugin.0.1.0.nupkg', 'unzipped'); }"
xcopy /e "./unzipped/DotFiles" "./tmp/Cofra.RiderPlugin.0.1.0/dotnet"

REM powershell.exe -nologo -noprofile -command "Compress-Archive -Force -Path tmp/* -DestinationPath Cofra.RiderPlugin.0.1.0.zip"
7z a "Cofra.RiderPlugin.0.1.0.zip" "./tmp/Cofra.RiderPlugin.0.1.0"

rmdir /s /q unzipped
rmdir /s /q tmp

cd %old_wd%

if %errorlevel% neq 0 exit /b %errorlevel%
