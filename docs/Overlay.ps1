# Script to overlay modifications for SandCastle Help File Builder output
$root = $PSScriptRoot

$source = "$root\mods"
if(!(Test-Path "$source")) { Write-Host "Source $source does not exist." -ForegroundColor Red; return; }

$target = "$root\bin\score" # Check the target folder exist
if(!(Test-Path "$target")) { Write-Host "Target $target does not exist." -ForegroundColor Red; return; }

# Replace the files
Copy-Item -Path "$source\*" -Destination "$target" -Recurse -Force -PassThru | ForEach-Object { Write-Host $_.FullName }
