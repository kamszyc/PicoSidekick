$drive = (Get-WmiObject -Query "SELECT * FROM Win32_Volume WHERE Label='CIRCUITPY'" | Select-Object -ExpandProperty DriveLetter)

if ($drive) {
    Copy-Item "$PSScriptRoot\..\*.py" -Destination "$drive\" -Force -Recurse
    Write-Host "Files copied to CIRCUITPY drive: $drive"
} else {
    Write-Host "CIRCUITPY drive not found."
}
