# P4-T8 — Scope of the AC2 guard in ParkFocusAndCancelSelectors

Timestamp: 2026-09-07T14-15
Task: [P4-T8]
Issue: #796
Channel used: A

## Branch taken

CANCEL-LOOP ONLY. Focus parking stays unconditional.

The branch is fixed by the line task P4-T7 recorded in
evidence/qa-gates/p4-t7-park-focus-decision.md. Quoted from that artifact:

> - The line `PARK-FOCUS-SUPPRESSION: IN SCOPE FOR P4-T8` is deliberately ABSENT from this artifact,
>   so task P4-T8 scopes its guard to the cancel loop only and focus parking stays unconditional.

That line being absent is the condition the plan states for the cancel-loop-only branch, so the
guard was added immediately above the `foreach (QfcItemGroup group in groups)` cancel loop inside
the member `ParkFocusAndCancelSelectors`, and the `if (_formViewer?.IsWebView2Focused == true)`
focus-parking block above it is untouched.

## What the diff for this task contains

The whole diff for this task in `QuickFiler/Controllers/QfcFormController.Deactivate.cs` is one
added guard plus its reason comment. In particular:

- The per-item boundary `catch (Exception exception)` inside `ParkFocusAndCancelSelectors`, and the
  `logger.Error` call that forms its whole body, are unchanged: neither appears in the diff.
- The AC6 instrumentation added by executed task P1-T4 is unchanged and still emitted at Debug
  level: neither the entry `logger.Debug(FormatDeactivationDiagnostics(...))` call nor the per-item
  `logger.Debug(FormatItemCancelDiagnostics(...))` call appears in the diff.
- The focus-parking block is unchanged: it does not appear in the diff.

Verification command and result:

```
git diff -- QuickFiler/Controllers/QfcFormController.Deactivate.cs
```

EXIT_CODE: 0

The diff reports one hunk at the cancel-loop boundary, containing twelve added lines and no removed
or modified line.

## Compile check

Command:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=TestResults\796\p4-t8\analyzer-rebuild.log;Verbosity=detailed"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 0
Build summary: Build succeeded. 0 Error(s).
Raw log (gitignored): TestResults/796/p4-t8/analyzer-rebuild.log

The Failed-to-Passed transition of FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector
is measured by task P4-T9 and is that task's acceptance, not this one's.

Output Summary: cancel-loop-only branch taken; focus parking unconditional; catch and AC6 log
statements unchanged; solution rebuilds clean.

---

## P4-T11 DOC REPAIR:

Timestamp: 2026-09-07T14-19

Measurement command (the position-independent form the plan specifies, which reads the maximal run
of consecutive `///` lines immediately above each of the two declarations):

```
pwsh -NoProfile -Command '$lines = Get-Content -LiteralPath QuickFiler\Controllers\QfcFormController.Deactivate.cs; function Block([int]$d) { $i = $d - 1; $b = @(); while ($i -ge 0 -and $lines[$i].TrimStart().StartsWith("///")) { $b = ,$lines[$i] + $b; $i-- }; return ,$b }; $p = ($lines | Select-String -SimpleMatch "internal void ParkFocusAndCancelSelectors()").LineNumber - 1; $f = ($lines | Select-String -SimpleMatch "internal static string FormatDeactivationDiagnostics(").LineNumber - 1; $bp = Block $p; $bf = Block $f; "Park-Summary=" + (@($bp | Select-String -SimpleMatch "<summary>").Count); "Park-791=" + (@($bp | Select-String -SimpleMatch "#791").Count); "Fmt-Summary=" + (@($bf | Select-String -SimpleMatch "<summary>").Count)'
```

EXIT_CODE: 0

Printed output:

```
Park-Summary=1
Park-791=1
Fmt-Summary=1
```

Required: `Park-Summary=1`, `Fmt-Summary=1`, and `Park-791` at 1 or greater. All three met.

### Diff scope

Command:

```
git diff -U1 -- QuickFiler/Controllers/QfcFormController.Deactivate.cs
```

EXIT_CODE: 0

The diff for this task consists of three hunks, and every added, removed and modified line in all
three is a `///` comment line: the stranded pair removed from above
`FormatDeactivationDiagnostics`, the corrected `activeFormIsNull` parameter description, and the
same stranded pair reinserted immediately above `ParkFocusAndCancelSelectors`. The fourth hunk in
the file's diff against the branch HEAD is the AC2 guard landed by the earlier task P4-T8 and is not
part of this task's diff. No file outside the feature folder other than
`QuickFiler/Controllers/QfcFormController.Deactivate.cs` appears in this task's diff.

### Compile check

Command:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=TestResults\796\p4-t11\analyzer-rebuild.log;Verbosity=detailed"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 0
Build summary: 0 Error(s).

### DEVIATION RECORDED

Deviation: this task also corrected the `<param name="activeFormIsNull">` description on
`FormatDeactivationDiagnostics`, which is a change beyond the stranded-block move the task text
describes.

What was wrong: the description asserted that "a null active form is corroborating evidence of a
self-inflicted deactivation and a non-null one of a genuine deactivation to a foreign window". The
manual observation this item produced measures the opposite on all four observations —
`ActiveFormNull=False` on each of the three self-inflicted popup gestures and `ActiveFormNull=True`
on the one deactivation attributed to focus leaving the form — as recorded under
`AC2-ITEMVIEWER-WIRING: REQUIRED` in evidence/other/close-ordering-decision.md.

Why it was made here: the correction is comment-only and lands in the same file and the same task
whose whole subject is the `///` comments of that file, so it adds no path, no gate and no
executable change, and it stays inside this task's acceptance envelope that no line which is not a
`///` line may be added, removed or modified. Leaving a statement in shipped source that this
item's own evidence refutes was judged worse than the deviation. The deviation was authorised by
the executing directive for this phase run and is recorded here rather than left implicit.

Scope of the deviation: one `<param>` element in one file. It changes no behaviour, no signature
and no test.

### POST-P4-T11 line count

Appended to evidence/qa-gates/p4-t10-file-size.md under the heading
`POST-P4-T11 RE-MEASUREMENT:`, as the plan directs. No second file-size artifact was created.
