# fsharp-core-hintpath-netstandard21-skew (Issue #895)

- Date captured: 2026-09-14
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/ (Issue #895)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #895
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/895
- Last Updated: 2026-09-14
- Work Mode: full-bug

## Summary

The six `FSharp.Core` `HintPath` entries in the solution are split between the `lib/netstandard2.0` and
`lib/netstandard2.1` flavours of the `FSharp.Core.11.0.100` package. The netstandard2.1 flavour references
`netstandard, Version=2.1.0.0`, an identity that does not exist for .NET Framework on any machine, so that
copy is unloadable wherever it is deployed. Which flavour lands in any given output directory is
last-writer-wins build-order nondeterminism, so a project can flip between working and broken across a
rebuild with no source change.

## Environment

- OS/version: Windows 11 Pro 10.0.26200, .NET Framework 4.8.1 (net481)
- Python version: not applicable
- Command/flags used: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"`
- Data source or fixture: `packages/FSharp.Core.11.0.100`, which ships both `lib/netstandard2.0` and `lib/netstandard2.1`

## Steps to Reproduce

1. Rebuild the solution.
2. Create a child `AppDomain` whose `ApplicationBase` is `QuickFiler.Test/bin/Debug` and whose
   `ConfigurationFile` is `QuickFiler.Test.dll.config`.
3. From inside that domain, load `Deedle.dll` from the same directory and invoke
   `Deedle.Frame.FromRecords<T>(IEnumerable<T>)` over a one-element sequence of any record type.
4. Repeat with `ApplicationBase` set to `TaskMaster.Test/bin/Debug`.

## Expected Behavior

Both directories deploy the same, loadable `FSharp.Core`, so the Deedle call behaves identically in each and
no output directory depends on build ordering for correctness.

## Actual Behavior

Rooted at `QuickFiler.Test/bin/Debug` the call raises the production chain; rooted at
`TaskMaster.Test/bin/Debug` it returns a `Frame` normally.

```
TypeInitializationException [Deedle.Reflection]
 ---> TypeInitializationException [<StartupCode$Deedle>.$FrameUtils]
 ---> FileNotFoundException [netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51]
```

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet:

Measured `HintPath` split, by file and line:

| Project file | Line | Flavour | netstandard reference |
|---|---|---|---|
| `QuickFiler/QuickFiler.csproj` | 52 | `lib/netstandard2.1` | 2.1.0.0 |
| `QuickFiler.Test/QuickFiler.Test.csproj` | 259 | `lib/netstandard2.1` | 2.1.0.0 |
| `ToDoModel/ToDoModel.csproj` | 42 | `lib/netstandard2.1` | 2.1.0.0 |
| `UtilitiesCS/UtilitiesCS.csproj` | 70 | `lib/netstandard2.0` | 2.0.0.0 |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | 598 | `lib/netstandard2.0` | 2.0.0.0 |
| `ToDoModel.Test/ToDoModel.Test.csproj` | 96 | `lib/netstandard2.0` | 2.0.0.0 |

`netstandard 2.0.0.0` resolves from the GAC; `netstandard 2.1.0.0` does not exist for .NET Framework.
`Deedle.dll` 3.0.0.0 references `netstandard 2.0.0.0` and `FSharp.Core 4.5.0.0`, so Deedle is never the
source of the 2.1.0.0 request. `FSharp.Core` is.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

`TaskMaster.csproj` lines 501 and 517 reference `QuickFiler` and `ToDoModel`, both netstandard2.1, alongside
four netstandard2.0 projects. The add-in output directory currently receives the netstandard2.0 copy, which
is why the reported production failure does not reproduce there today. That is build-order luck, not
correctness: the add-in can flip back into the failing state on any rebuild without a source change. The
originally reported failure in issue #879 is evidence that it has already been in that state once.

## Suspected Cause / Notes

A NuGet package shipping both `lib/netstandard2.0` and `lib/netstandard2.1` produces per-project `HintPath`
skew in a legacy `packages.config` solution. When projects with different flavours feed the same output
directory, the copy that wins is MSB3277-class last-writer-wins.

Found while investigating issue #879. Two successive regression-probe designs for that issue returned green
against an unfixed tree solely because the probe's child domain was rooted at `TaskMaster.Test/bin/Debug`,
where nothing requests `netstandard 2.1.0.0`. The probe designs were correct; the directory was wrong.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas
- [x] Integration scenario to retest
- [x] Manual verification notes

Align all six `HintPath` entries on `lib/netstandard2.0`, which is the only flavour loadable on .NET
Framework. Consider a build-time guard asserting that no deployed `FSharp.Core.dll` references
`netstandard 2.1.0.0`, so the skew cannot silently return. Retest by rebuilding and re-running the child
domain probe rooted at `QuickFiler.Test/bin/Debug`; it must stop raising the chain. Manual verification is a
fresh Outlook session exercising the QuickFiler ribbon path that originally failed.

This is complementary to issue #879, not a replacement for it. That issue's remedy makes the add-in robust
to whichever flavour is deployed; this one removes the unloadable flavour so the question stops arising.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
