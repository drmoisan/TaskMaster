# QA Gate — Format Verify, Read-Only (Issue #656)

Timestamp: 2026-09-01T14-49
Task: [P4-T2] (toolchain loop pass 1, step 1 verification)
Satisfies: AC-14

Command:
```
dotnet tool run csharpier check .
```

EXIT_CODE: 0

Output Summary: `Checked 1566 files in 4732ms.` — the final summary line of the command output,
transcribed verbatim. CSharpier reported no file requiring formatting and exited 0, which is what
AC-14 requires.

The command is invoked through `dotnet tool run` so the manifest-pinned CSharpier 1.2.6 is the
version that runs, matching the version CI uses after `dotnet tool restore`. A globally installed
CSharpier of a different version would produce diffs that disagree with the CI format step.

## Loop-restart determination

`check` is read-only and rewrote nothing, so this step did not trigger a restart of the toolchain
loop. The preceding write-mode `format` step also required no restart: the literals this change
introduced survived it byte-for-byte, verified immediately after the format pass by

```
@(Select-String -Path QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs -SimpleMatch 'bool hostOpen = _host.IsOpen;').Count            = 1
@(Select-String -Path QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs -SimpleMatch 'if (_closeCompleted && !hostOpen)').Count        = 1
@(Select-String -Path QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs -SimpleMatch 'if (_closeCompleted)').Count                     = 0
@(Select-String -Path QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs -SimpleMatch 'Issue #656').Count                               = 2
```

Each of the four values equals the value asserted by the Phase 2 tasks, so the formatter did not
reflow any asserted literal onto a second line and the Phase 2 acceptance conditions still hold
against the formatted files. Post-format line counts are 395 for the coordinator and 213 for
`Part3.cs`, both under the 500-line limit; those are the counts recorded by P4-T9.

Output Summary: Format gate passes read-only with exit 0 across 1566 files. AC-14 is satisfied.
