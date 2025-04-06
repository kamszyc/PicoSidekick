$drive = (Get-WmiObject -Query "SELECT * FROM Win32_Volume WHERE Label='CIRCUITPY'" | Select-Object -ExpandProperty DriveLetter)

if ($drive) {
    Copy-Item "$PSScriptRoot\..\*.py" -Destination "$drive\" -Force -Recurse
    Remove-Item -Recurse -Force -Path "$drive\pngs\"
    New-Item -ItemType Directory -Path "$drive\pngs\"
    Copy-Item "$PSScriptRoot\..\pngs\*.png" -Destination "$drive\pngs\" -Force -Recurse
    Write-Host "Files copied to CIRCUITPY drive: $drive"
} else {
    Write-Host "CIRCUITPY drive not found."
}
