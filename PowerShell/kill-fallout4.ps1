param(
    [switch] $Restart,
    [switch] $Silent,
    [switch] $London
)

$runCommandPath = join-path $PSScriptRoot "Run-Command.ps1"

$pathToFalloutRestart = "steam://rungameid/377160 -nolauncher"
$pathToFalloutLondon = "E:\Games\gog\Fallout London\installer.exe"
$secondsBetweenKillRetries = 5
$maxTimeToTryKill = 90

$sounds = @{
    NotFound           = "$($env:SystemRoot)\Media\Windows Message Nudge.wav";
    InstanceKilled     = "$($env:SystemRoot)\Media\Windows Proximity Notification.wav";
    AllInstancesKilled = "$($env:SystemRoot)\Media\Windows Unlock.wav"
}

if (-not [string]::IsNullOrWhiteSpace($pathToFalloutLondon) -and $London) {
    $pathToFalloutRestart = $pathToFalloutLondon
}

function PlayTone([string] $WAVFilePath) {
    if ($Silent.IsPresent) { return }
    #$tone =  "$($env:SystemRoot)\Media\Windows Notify.wav"
    $player = New-Object System.Media.SoundPlayer($WAVFilePath)
    $player.Play()
    $player.Dispose()
}

function GetFalloutRunningProcesses() {
    $result = get-process | where { $_.ProcessName -like "Fallout*" } 
    if ($null -ne $result) {
        $result | % { $_ }
    }
}


function KillRunningProcess([Int] $processId) {

    $passArgs = @("/pid", $processId, "/F")
    $result = . $runCommandPath -commandPath "Taskkill.exe" -commandArguments $passArgs 
    $result
}


function KillFalloutProcs([Object[]] $procs) {
    $remainingProcs = -1
    if ($null -eq $procs) { return $remainingProcs }

    $remainingProcs = $procs.Length

    $procs | % {
        write-host "Killing: $($_.ProcessName) ($($_.ID))" -foregroundcolor Yellow
        $killed = KillRunningProcess $($_.ID)
        if ($killed.ExitCode -eq 0) {
            PlayTone $sounds.InstanceKilled
            $remainingProcs--
        }
    }

    return $remainingProcs
}


function KillRunningFalloutProcesses() {
    $timer = new-timespan -seconds $maxTimeToTryKill
    $clock = [diagnostics.stopwatch]::StartNew()
    while ($clock.elapsed -lt $timer) {
        $f4 = GetFalloutRunningProcesses

        $remaining = KillFalloutProcs $f4

        if ($remaining -eq -1) {
            PlayTone $sounds.NotFound
            write-host "Fallout was not running" -foregroundcolor Green
            return $true
        }

        if ($remaining -eq 0) {
            $f4 = GetFalloutRunningProcesses
            if ($f4 -eq $null) {
                PlayTone $sounds.AllInstancesKilled
                write-host "Fallout killed successfully" -foregroundcolor Green
                return $true
            }
        }
        
        start-sleep -seconds $secondsBetweenKillRetries
    }
    return $false
}

$wasKilled = KillRunningFalloutProcesses


if ($wasKilled -eq $true -and $Restart) {
    write-host "Restarting Fallout..." -foregroundcolor White
    start-process $pathToFalloutRestart -ErrorAction SilentlyContinue
}

