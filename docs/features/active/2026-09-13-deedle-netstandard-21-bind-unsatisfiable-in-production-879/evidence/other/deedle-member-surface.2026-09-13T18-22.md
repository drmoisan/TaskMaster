# Deedle Member Surface (metadata-only read)

Timestamp: 2026-09-14T09-40

Command:

```
pwsh -NoProfile -Command '
$p = "TaskMaster.Test/bin/Debug/Deedle.dll"
Write-Output ("DEEDLE_DLL_PRESENT=" + (Test-Path -LiteralPath $p))
$fs = [System.IO.File]::OpenRead((Resolve-Path -LiteralPath $p).Path)
$pe = New-Object System.Reflection.PortableExecutable.PEReader($fs)
$md = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
Write-Output ("TYPEDEF_COUNT=" + @($md.TypeDefinitions).Count)
$found = 0
foreach ($h in $md.TypeDefinitions) {
$td = $md.GetTypeDefinition($h)
if (($md.GetString($td.Namespace) -eq "Deedle") -and ($md.GetString($td.Name) -eq "Reflection")) {
Write-Output ("TYPE_FOUND=Deedle.Reflection")
Write-Output ("TYPE_ATTRS=" + $td.Attributes)
foreach ($mh in $td.GetMethods()) {
$m = $md.GetMethodDefinition($mh)
if ($md.GetString($m.Name) -eq "convertRecordSequence") {
$found = $found + 1
Write-Output ("MEMBER_ATTRS=" + $m.Attributes)
Write-Output ("MEMBER_GENERIC_PARAM_COUNT=" + @($m.GetGenericParameters()).Count) } } } }
Write-Output ("CONVERT_RECORD_SEQUENCE_DEFINITIONS=" + $found)
$pe.Dispose()
$fs.Dispose()
'
```

The bounded adaptation authorised by `[P1-T6]` — prepending
`[void][System.Reflection.Assembly]::Load("System.Reflection.Metadata")` — was NOT taken. Both
`[System.Reflection.PortableExecutable.PEReader]` and
`[System.Reflection.Metadata.PEReaderExtensions]` resolved in the pwsh host as written.

The payload additionally carries a leading
`Set-Location -LiteralPath "C:/Users/DanMoisan/repos/TaskMaster-wt/bugs-2026-09-11-item-879"`
statement, because the executor was launched without worktree isolation and pwsh would otherwise
start in the coordinator session worktree, resolving every repository-relative path in the wrong
tree. That statement changes no measurement; it fixes the tree the measurement is taken against.

EXIT_CODE: 0

Output Summary:

```
DEEDLE_DLL_PRESENT=True
TYPEDEF_COUNT=2527
TYPE_FOUND=Deedle.Reflection
TYPE_ATTRS=Abstract, Sealed
MEMBER_ATTRS=Assembly, Static
MEMBER_GENERIC_PARAM_COUNT=1
CONVERT_RECORD_SEQUENCE_DEFINITIONS=1
```

Acceptance Condition: MET. `DEEDLE_DLL_PRESENT=True`; `TYPEDEF_COUNT=2527`, greater than 0, so the
reader opened a real assembly and the name-comparison mechanism is live; exactly one
`TYPE_FOUND=Deedle.Reflection` line; `CONVERT_RECORD_SEQUENCE_DEFINITIONS=1`, so `[P2-T5]`'s
single-member lookup is unambiguous; `MEMBER_GENERIC_PARAM_COUNT=1`.

`MEMBER_ATTRS=Assembly, Static` is recorded and not gated. `Assembly` accessibility means
`convertRecordSequence` is internal to `Deedle.dll`, so `[P2-T5]`'s
`BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static` lookup is load-bearing: a
public-only lookup would return null and the helper would throw `InvalidOperationException`.
`TYPE_ATTRS=Abstract, Sealed` is the metadata encoding of an F# module, which is a static class.
