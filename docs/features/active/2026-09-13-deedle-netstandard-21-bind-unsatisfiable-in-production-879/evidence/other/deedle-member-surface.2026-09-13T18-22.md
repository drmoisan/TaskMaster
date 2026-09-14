# Deedle Member Surface (metadata-only read)

Timestamp: 2026-09-14T11-19

This artifact was overwritten by the Revision R5 re-run of `[P1-T6]`, as that task directs. Revision R5
repoints the task twice: the file read moves from `TaskMaster.Test/bin/Debug/Deedle.dll` to
`QuickFiler.Test/bin/Debug/Deedle.dll`, which is the file the re-rooted child domain loads, and the
member measured moves from `Deedle.Reflection.convertRecordSequence` to the `Deedle.Frame.FromRecords`
overload set, which is production's entry point at `UtilitiesCS/Extensions/DfDeedle.cs` lines 123 and
237. The superseded Revision R2 measurement recorded
`TYPE_FOUND=Deedle.Reflection` and `CONVERT_RECORD_SEQUENCE_DEFINITIONS=1` against
`TaskMaster.Test/bin/Debug/Deedle.dll`; it no longer describes the member `[P2-T5]` is authored against.

Command:

```
pwsh -NoProfile -Command '
$p = "QuickFiler.Test/bin/Debug/Deedle.dll"
Write-Output ("DEEDLE_DLL_PRESENT=" + (Test-Path -LiteralPath $p))
$fs = [System.IO.File]::OpenRead((Resolve-Path -LiteralPath $p).Path)
$pe = New-Object System.Reflection.PortableExecutable.PEReader($fs)
$md = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
Write-Output ("TYPEDEF_COUNT=" + @($md.TypeDefinitions).Count)
$defs = 0
$arity1 = 0
foreach ($h in $md.TypeDefinitions) {
$td = $md.GetTypeDefinition($h)
$ns = $md.GetString($td.Namespace)
$n = $md.GetString($td.Name)
if (($ns -eq "Deedle") -and $n.StartsWith("Frame")) { Write-Output ("DEEDLE_FRAME_LIKE_TYPE=" + $ns + "." + $n) }
if (($ns -eq "Deedle") -and ($n -eq "Frame")) {
Write-Output ("TYPE_FOUND=Deedle.Frame")
Write-Output ("TYPE_ATTRS=" + $td.Attributes)
foreach ($mh in $td.GetMethods()) {
$m = $md.GetMethodDefinition($mh)
if ($md.GetString($m.Name) -eq "FromRecords") {
$g = @($m.GetGenericParameters()).Count
$defs = $defs + 1
if ($g -eq 1) { $arity1 = $arity1 + 1 }
Write-Output ("MEMBER_ATTRS=" + $m.Attributes)
Write-Output ("FROMRECORDS_MEMBER_GENERIC_PARAM_COUNT=" + $g) } } } }
Write-Output ("FROMRECORDS_DEFINITIONS=" + $defs)
Write-Output ("FROMRECORDS_GENERIC_ARITY_1_COUNT=" + $arity1)
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
DEEDLE_FRAME_LIKE_TYPE=Deedle.FrameData
DEEDLE_FRAME_LIKE_TYPE=Deedle.Frame`2
DEEDLE_FRAME_LIKE_TYPE=Deedle.FrameUtils
DEEDLE_FRAME_LIKE_TYPE=Deedle.Frame
TYPE_FOUND=Deedle.Frame
TYPE_ATTRS=Public, Serializable
MEMBER_ATTRS=Public, Static
FROMRECORDS_MEMBER_GENERIC_PARAM_COUNT=2
MEMBER_ATTRS=Public, Static
FROMRECORDS_MEMBER_GENERIC_PARAM_COUNT=1
DEEDLE_FRAME_LIKE_TYPE=Deedle.FrameExtensions
DEEDLE_FRAME_LIKE_TYPE=Deedle.FrameStatsExtensions
DEEDLE_FRAME_LIKE_TYPE=Deedle.FrameBuilder
DEEDLE_FRAME_LIKE_TYPE=Deedle.FrameModule
DEEDLE_FRAME_LIKE_TYPE=Deedle.FrameUtilsModule
FROMRECORDS_DEFINITIONS=2
FROMRECORDS_GENERIC_ARITY_1_COUNT=1
```

Acceptance Condition: MET. `DEEDLE_DLL_PRESENT=True`; `TYPEDEF_COUNT=2527`, greater than 0, so the
reader opened a real assembly and the name-comparison mechanism is live; exactly one
`TYPE_FOUND=Deedle.Frame` line; `FROMRECORDS_GENERIC_ARITY_1_COUNT=1`, so `[P2-T5]`'s shape-filtered
lookup selects exactly one candidate and the plan does not have to name the overload by parameter type.

`FROMRECORDS_DEFINITIONS=2` is recorded and deliberately not gated, as the task directs. `Deedle.Frame`
carries two `FromRecords` definitions, of generic arity 2 and 1 respectively. `[P2-T5]`'s selector
discriminates on generic arity and parameter count rather than on the total, and only one definition has
generic arity 1.

`MEMBER_ATTRS=Public, Static` is recorded and not gated. Both definitions are public and static, so
`[P2-T5]`'s `BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static` lookup covers the
accessibility this line reports. This differs from the superseded Revision R2 member
`Deedle.Reflection.convertRecordSequence`, which reported `Assembly, Static`; the `NonPublic` flag is
therefore no longer load-bearing for the lookup, but it remains harmless and the plan does not authorise
changing the flag set.

The `DEEDLE_FRAME_LIKE_TYPE=` lines are the recorded, deliberately ungated diagnostic. They confirm that
the static surface this repository calls as `Frame.FromRecords` is in fact spelled `Deedle.Frame` in
metadata: `TYPE_FOUND=Deedle.Frame` was emitted, so the F# module-name-collision case the task
anticipated did not arise. The sibling names `Deedle.Frame` + backtick + `2`, `Deedle.FrameModule`,
`Deedle.FrameUtils`, `Deedle.FrameUtilsModule`, `Deedle.FrameData`, `Deedle.FrameBuilder`,
`Deedle.FrameExtensions` and `Deedle.FrameStatsExtensions` are distinct types and none carries the
measured member. `TYPE_ATTRS=Public, Serializable` is the metadata encoding of the `Deedle.Frame` static
type and is recorded, not gated.
