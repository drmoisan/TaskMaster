# Git seam and content adapter of the repository hygiene guard. Invoke-GitExe is the only place
# the git executable is reached; tests mock it, never git itself. Read-TrackedFileText takes an
# injectable content delegate so tests supply bytes or text in memory.

function Assert-GitExitCode {
    <#
    .SYNOPSIS
        Throws when a git invocation reported a non-zero exit code.
    .DESCRIPTION
        Pure check used by Invoke-GitExe. The message carries the joined argument list so a failed
        enumeration names the command that failed; a broken git call can never read as a clean
        tree.
    .PARAMETER ExitCode
        The exit code git reported.
    .PARAMETER GitArgs
        The argument list passed to git.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [int]$ExitCode,

        [Parameter(Mandatory = $true)]
        [string[]]$GitArgs
    )

    if ($ExitCode -ne 0) {
        throw ('git ' + ($GitArgs -join ' ') + ' failed with exit code ' + $ExitCode + '.')
    }
}

function Invoke-GitExe {
    <#
    .SYNOPSIS
        Runs git with the given argument list and returns its output.
    .DESCRIPTION
        Splats the argument list into git with the error stream merged, then throws through
        Assert-GitExitCode when git exits non-zero.
    .PARAMETER GitArgs
        The argument list passed to git.
    .OUTPUTS
        System.Object
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$GitArgs
    )

    $output = & git @GitArgs 2>&1
    Assert-GitExitCode -ExitCode $LASTEXITCODE -GitArgs $GitArgs
    return $output
}

function Get-TrackedFileRecord {
    <#
    .SYNOPSIS
        Enumerates the tracked files of the repository as path records.
    .DESCRIPTION
        Reads the NUL-separated eol listing from git ls-files and returns one record per tracked
        file with a Path property and an IsBinaryInIndex property, which is true when the index
        attribute segment reads i/-text.
    .OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param()

    $output = Invoke-GitExe -GitArgs @('ls-files', '--eol', '-z')
    $listing = -join @($output | Where-Object { $_ -is [string] })
    foreach ($entry in $listing.Split([char]0)) {
        if ($entry.Length -eq 0) {
            continue
        }

        $tab = $entry.IndexOf([char]9)
        if ($tab -lt 0) {
            throw ('Malformed git ls-files --eol record: ' + $entry)
        }

        [pscustomobject]@{
            Path            = $entry.Substring($tab + 1)
            IsBinaryInIndex = $entry.Substring(0, $tab).Contains('i/-text')
        }
    }
}

function Read-TrackedFileText {
    <#
    .SYNOPSIS
        Reads a tracked file through the content delegate and decodes it by byte-order mark.
    .DESCRIPTION
        The default delegate returns the file's bytes. A string result is returned unchanged; a
        null or empty result is empty text; any other result is converted to a byte array and
        decoded by byte-order mark (UTF-8 with mark, UTF-16 little-endian, UTF-16 big-endian),
        otherwise as UTF-8 without throwing on invalid bytes. When the record is binary in the
        index and the bytes carry no UTF-16 mark, the function returns null: the record is not
        scanned for the profile-path rule.
    .PARAMETER Path
        The repository-relative path of the tracked file.
    .PARAMETER ReadContent
        Optional delegate that takes the path and returns bytes or text.
    .PARAMETER IsBinaryInIndex
        Set when the eol listing marks the record as binary in the index.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter()]
        [scriptblock]$ReadContent = $null,

        [Parameter()]
        [switch]$IsBinaryInIndex
    )

    if ($null -eq $ReadContent) {
        # The unary comma hands the byte array back as one object instead of unrolling it.
        $ReadContent = { param([string]$FilePath) , (Get-Content -LiteralPath $FilePath -AsByteStream -Raw) }
    }

    $content = & $ReadContent $Path
    if ($null -eq $content) {
        return ''
    }

    if ($content -is [string]) {
        return $content
    }

    $bytes = [byte[]]$content
    if ($bytes.Length -eq 0) {
        return ''
    }

    $isUtf16LittleEndian = $bytes.Length -ge 2 -and $bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE
    $isUtf16BigEndian = $bytes.Length -ge 2 -and $bytes[0] -eq 0xFE -and $bytes[1] -eq 0xFF
    if ($IsBinaryInIndex -and -not ($isUtf16LittleEndian -or $isUtf16BigEndian)) {
        return $null
    }

    if ($isUtf16LittleEndian) {
        return [System.Text.Encoding]::Unicode.GetString($bytes, 2, $bytes.Length - 2)
    }

    if ($isUtf16BigEndian) {
        return [System.Text.Encoding]::BigEndianUnicode.GetString($bytes, 2, $bytes.Length - 2)
    }

    $utf8 = [System.Text.UTF8Encoding]::new($false, $false)
    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        return $utf8.GetString($bytes, 3, $bytes.Length - 3)
    }

    return $utf8.GetString($bytes)
}
