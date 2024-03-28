param(
    [string] $commandPath,
    [string[]] $commandArguments
)



function BuildRunCmdResult() {
    $result = New-Object psobject
    $result | Add-Member -MemberType NoteProperty -Name "Out" -Value ""
    $result | Add-Member -MemberType NoteProperty -Name "Error" -Value ""
    $result | Add-Member -MemberType NoteProperty -Name "Exception" -Value $null
    $result | Add-Member -MemberType NoteProperty -Name "PSException" -Value $null
    $result | Add-Member -MemberType NoteProperty -Name "ExitCode" -Value 0
    $result | Add-Member -MemberType NoteProperty -Name "StartTime" -Value $null
    $result | Add-Member -MemberType NoteProperty -Name "EndTime" -Value $null
    $result | Add-Member -MemberType NoteProperty -Name "Elapsed" -Value $null

    $result
}


function RunCmd([string] $commandName, [string[]] $cmdArguments) {

    $result = BuildRunCmdResult
    $result.StartTime = [DateTime]::Now

    Write-Verbose "Command '$commandName' Started: $($result.StartTime)"

    # NOTE: the use of stringbuilders and events is needed because if either of the StdOut/StdErr buffers fill, the process just hangs. 
    #    This reads the data as it comes in rather than filling the buffer

    # Creating string builders to store stdout and stderr.
    $stdOutBuilder = New-Object -TypeName System.Text.StringBuilder
    $stdErrBuilder = New-Object -TypeName System.Text.StringBuilder
    $builderScriptBlock = {
        if (-not [String]::IsNullOrEmpty($EventArgs.Data)){
            $Event.MessageData.AppendLine($EventArgs.Data)
        }
    }
    $stdOutEvent = $null
    $stdErrEvent = $null

    try {
        $ps = new-object System.Diagnostics.Process
        $ps.StartInfo.Filename = $commandName
        if ($cmdArguments -ne $Null -and $cmdArguments.Length -gt 0) {
            $ps.StartInfo.Arguments = [string]::Join(" ", $cmdArguments)
        }
        $ps.StartInfo.WorkingDirectory = (Get-Location).Path
        $ps.StartInfo.RedirectStandardOutput = $True
        $ps.StartInfo.RedirectStandardError = $True
        $ps.StartInfo.UseShellExecute = $false

        $stdOutEvent = Register-ObjectEvent -InputObject $ps -Action $builderScriptBlock -EventName 'OutputDataReceived' -MessageData $stdOutBuilder
        $stdErrEvent = Register-ObjectEvent -InputObject $ps -Action $builderScriptBlock -EventName 'ErrorDataReceived' -MessageData $stdErrBuilder

        Write-Verbose "Executing : $($ps.StartInfo.Filename)"
        Write-Verbose " Arguments: $($ps.StartInfo.Arguments)"
        [Void]$ps.Start()
        $ps.BeginOutputReadLine()
        $ps.BeginErrorReadLine()
        $ps.WaitForExit()

        # Unregistering events to retrieve process output.
        Unregister-Event -SourceIdentifier $stdOutEvent.Name
        Unregister-Event -SourceIdentifier $stdErrEvent.Name

        $result.Out = $stdOutBuilder.ToString() # $ps.StandardOutput.ReadToEnd()
        $result.Error = $stdErrBuilder.ToString() # $ps.StandardError.ReadToEnd()
        $result.ExitCode = $ps.ExitCode

    } catch {
        $result.Error = "$($result.Error)" + $_.Exception.Message
        $result.PSException = $_
        $result.Exception = $_.Exception
        $result.ExitCode = 9999

    } finally {
        $result.EndTime = [DateTime]::Now
        $ps.Dispose()
        $ps = $null
    }

    if ($result.Out -eq $null) { $result.Out = "" }
    if ($result.Error -eq $null) { $result.Error = "" }

    $result.Out = $result.Out.Trim($trimLineFeeds)
    $result.Error = $result.Error.Trim($trimLineFeeds)
    $result.EndTime = [DateTime]::Now

    $result.Elapsed = $result.EndTime - $result.StartTime

    Write-Verbose "Command '$commandName' Ended  : $($result.EndTime)"
    Write-Verbose "Command '$commandName' Elapsed: $($result.Elapsed)"

    $result
}

RunCmd -commandName $commandPath -cmdArguments $commandArguments 