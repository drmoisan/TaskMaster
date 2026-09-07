# P6-T1 — AC4 seam declaration

Timestamp: 2026-09-07T14-34
Task: [P6-T1]
Issue: #796
Channel used: A

## Branch taken

AC4-NEW-MEMBER: REQUIRED

Quoted from evidence/other/close-ordering-decision.md:

> AC4-NEW-MEMBER: REQUIRED

and:

> AC4-MECHANISM: SearchOwnedDismissalLatch

and from the derivation recorded beneath them:

> The existing `_searchLeaveHandoffPending` field cannot carry this meaning. It is consumed
> destructively on its first read, by design [...] so it is false again immediately after the
> handoff it guards, while the popup is still open. Provenance must persist for as long as the
> popup is open, which is a different lifetime. Overloading the one-shot field would break the #680
> contract that the plan requires be preserved.

The REQUIRED branch is taken. The member was declared in
`QuickFiler/Controllers/QfcItemController.EventHandlers.cs`, beside the existing one-shot latch, as
the private boolean field `_searchOwnedDismissal` together with the internal get-only accessor
`SearchOwnsDropDownDismissal` that reads it. No interface changed and no public surface changed:
the field is private and the accessor is internal on an internal partial class, matching the
precedent executed task P1-T4 set in this same file with `IsBreadcrumbSelectorOpen`.

## What this task deliberately did NOT do

The suppression behaviour is not present. `TextBoxSearch_Leave` is unchanged, so it still dismisses
regardless of provenance, and the Phase 6 fail-before test therefore fails at its assertion rather
than at compilation. The producers are also not yet written; both land at task P6-T5.

The exact qualifier the plan requires: this task left
`QuickFiler/Controllers/QfcItemController.EventHandlers.cs` unchanged EXCEPT for the declaration
described above. An unqualified unchanged-file claim would be false, because executed task P1-T4
already added the observational selector-open forward to this same file.

The existing structures the mechanism extends are all unchanged by this task: the one-shot latch
`_searchLeaveHandoffPending`, its single producer in the `Keys.Down` branch, and its single
read-and-clear consumer in `TextBoxSearch_Leave`.

## Compile check

Command:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=TestResults\796\p6-t1\analyzer-rebuild.log;Verbosity=detailed"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 0
Build summary: 1 Warning(s). 0 Error(s).
Raw log (gitignored): TestResults/796/p6-t1/analyzer-rebuild.log

## The one warning, recorded rather than hidden

```
QuickFiler\Controllers\QfcItemController.EventHandlers.cs(201,22): warning CS0649: Field 'QfcItemController._searchOwnedDismissal' is never assigned to, and will always have its default value false
```

This is the expected and intended consequence of a task whose whole purpose is to declare the seam
without writing to it. It is transient: task P6-T5 adds the two producers, after which the field is
assigned and the diagnostic has no subject. It is recorded here so that the intermediate state is
auditable and so a later reader does not mistake it for drift.

The warning does not fail this gate. This task's stated acceptance is that the solution compiles
under the P0-T8 command form, and it does, with EXIT_CODE 0. Whether the warning has in fact cleared
is re-measured at task P6-T5 and recorded in that task's evidence rather than predicted here.

Output Summary: REQUIRED branch taken; the SearchOwnedDismissalLatch member is declared with no
suppression behaviour; the solution compiles with one expected transient CS0649.
