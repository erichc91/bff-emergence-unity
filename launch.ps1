$unityExe = "C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe"
$root     = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host ""
Write-Host "Launching BFF Emergence in Unity 6..." -ForegroundColor Cyan
Write-Host ""
Write-Host "  FIRST TIME:" -ForegroundColor Yellow
Write-Host "    1. Wait for import to finish (~30 sec)"
Write-Host "    2. Menu: Tools > BFF > Create Scene"
Write-Host "    3. Press PLAY"
Write-Host ""
Write-Host "  WHAT YOU'LL SEE:" -ForegroundColor Green
Write-Host "    - Random noise first (all colours)"
Write-Host "    - Gold/orange spreads = replicators taking over"
Write-Host "    - Red appears later = loop structure (parasite)"
Write-Host "    - Oscillations = Red Queen arms race"
Write-Host ""

Start-Process -FilePath $unityExe -ArgumentList @('-projectPath', $root)
