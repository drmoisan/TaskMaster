# P8-T9 — Solution analyzer build after the issue #490 fixes

Timestamp: 2026-08-28T01-25
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:normal
EXIT_CODE: 0
ExpectedExitCode: 0

## Acceptance

`EXIT_CODE: 0`, `Build succeeded.`, `5 Warning(s)`, `0 Error(s)`. All five warnings are the
pre-existing `System.Reactive` `packages.config` advisory raised by
`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`, one each
for `QuickFiler`, `TaskMaster`, `ToDoModel`, `UtilitiesCS.Test` and `UtilitiesCS`. That is
character-for-character the P0-T11 analyzer baseline (`5 Warning(s)`, `0 Error(s)`), so this change
introduces **no new diagnostic of any kind**: a filter for `: (warning|error) [A-Z]+[0-9]+` over the
full log returns zero lines, meaning there is no `CS`, `CA`, `IDE`, `MA` or `RCS` diagnostic.

This is the solution-level command, so the spaced platform spelling `"/p:Platform=Any CPU"` is the
correct one and is used verbatim as the plan prints it. The single-project spelling defect recorded
at P7-T4 and P7-T8 does not apply to a `.sln` invocation; MSBuild normalises `Any CPU` to `AnyCPU`
when driving a solution.

## Non-vacuity

Occurrences of the literal `Skipping target "CoreCompile"` in the `/v:normal` log: **0**.
`CoreCompile:` target invocations in the same log: **65**. The build genuinely compiled; no project's
`CoreCompile` was skipped by the incremental up-to-date check, which is why `/t:Rebuild` is used
rather than `/t:Build`.

## What a clean build does and does not prove

A missed `SetFolderItems` to `AddFolderItems` rename site is `CS1061`, because the member no longer
exists on `IItemViewer` after P8-T4. A clean solution build is therefore positive proof that **every**
rename call site was updated — the three production sites in P8-T6 and the fourteen test sites in
P8-T7 — across all eighteen projects, not merely the ones this plan enumerated.

It is **not** proof for `FocusSubject`. A `bool`-returning invocation is a legal expression
statement, so the build would still have succeeded with the sole caller left as
`_itemViewer.FocusSubject();`. The proof that the caller was updated is P8-T3's
`git grep -F -n "_ = _itemViewer.FocusSubject();" -- QuickFiler/Controllers/QfcItemController.MailActions.cs`
returning **exactly one** match, recorded at
`QuickFiler/Controllers/QfcItemController.MailActions.cs:86`. This task's summary cites that P8-T3
result and does not claim the build discharges it.

## A dangling cref was deliberately left in place and is not diagnosed

`QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs:16` carries
`/// <see cref="IItemViewer.SetFolderItems"/>`, which now names a member that no longer exists.
P8-T7 forbids editing that comment. The build emits no `CS1574`, because XML documentation file
generation is not enabled on `QuickFiler.Test.csproj`, so `cref` resolution is never performed. The
comment's staleness is a documentation matter for Phase 9, not a build failure.

Output Summary: The full solution rebuilds at `EXIT_CODE: 0` with `Build succeeded.`,
`5 Warning(s)` and `0 Error(s)`, matching the P0-T11 analyzer baseline exactly; every warning is the
pre-existing `System.Reactive` `packages.config` advisory and there is no `CS`/`CA`/`IDE` diagnostic.
The gate is non-vacuous: **0** occurrences of `Skipping target "CoreCompile"` and 65 `CoreCompile`
invocations. The clean build proves every `SetFolderItems` rename call site was updated; the
`FocusSubject` caller proof is the P8-T3 artifact's single `_ = _itemViewer.FocusSubject();` match,
not this build.
