# Final QC — Analyzer/Code-Style Build

Timestamp: 2026-07-19T06-45

## 1. Literal plan command (as written, `/t:Build`)

Command: `msbuild TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true`

EXIT_CODE: 0

Output Summary: Build succeeded. 0 Warning(s), 0 Error(s). Zero `csc.exe` invocations were
present in the log (`grep -c "csc.exe"` = 0), confirming this was an incremental no-op (nothing
recompiled) rather than a genuine analyzer pass, since the prior plain `/t:Build` runs (used
ahead of each batch's coverage step) had already brought the solution up to date under
MSBuild's incremental cache. This is not a reliable analyzer-pass proof by itself.

## 2. Genuine rebuild (definitive analyzer proof)

Command: `msbuild TaskMaster.sln -t:Rebuild -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true`

EXIT_CODE: 0

Output Summary: Build succeeded. 80 Warning(s) (up from the 76-warning baseline captured at
P0-T5 — see finding below), 0 Error(s). Warning categories: `CS8632` (pre-existing, unrelated
files without `#nullable`), `CS0618` (pre-existing obsolete-API usage), `CS0108`/`CS0169`/
`CS0067`/`CS4014`/`CS2002`/`CS0168`/`MSTEST0032` (all pre-existing, unchanged), plus 2 new
categories not present in the P0-T5 baseline summary:
- `CS0649` x2 (vendored `SVGControl/SvgImageSelector.cs` fields never assigned) — confirmed
  pre-existing at every nullable-pragma-gate run in this session (baseline P0-T6 through every
  batch gate); simply not captured by the P0-T5 baseline analyzer run because that earlier
  incremental build state did not force a genuine SVGControl recompile at that point. Out of
  scope: `SVGControl` is a vendored project, not one of the 24 cluster files.
- `CS8625` x3 unique occurrences (`EmailTokenizer_Tests.cs:62`, `SubjectMapEntry_Tests.cs:244`,
  `AsyncSerialization_Tests.cs:166`) — see finding below.

## Finding: pre-existing test files with their own `#nullable enable` now surface 2 new,
## non-blocking CS8625 warnings as a side effect of production remediation (not a regression)

Three `UtilitiesCS.Test` files already carried their own `#nullable enable` pragma **before**
this feature (confirmed: none of them are part of the 24-file cluster, and none were touched by
this feature). Two of the three intentionally call a production method with a literal `null` to
verify a guard clause, and now trigger `CS8625` because the called production file is newly
`#nullable enable`-enabled by this feature (the callee's parameter type is no longer oblivious):
- `EmailTokenizer_Tests.cs(62,41)`: `tokenizer.Tokenize(obj: null, ...)` — calls
  `EmailTokenizer.Tokenize(object obj, ...)` (Batch E); `obj` was intentionally left
  non-nullable since the method's own `if (obj is null) throw ArgumentNullException` guard
  documents the contract, per this feature's design (test exercises that exact guard).
- `SubjectMapEntry_Tests.cs(244,86)`: `entry.Encode(encoder.Object, tokens: null)` — calls
  `SubjectMapEntry.Encode(ISubjectMapEncoder encoder, string[] tokens)` (Batch C); `tokens`
  stays non-nullable per its `ReadyToEncode`-gated usage.
- `AsyncSerialization_Tests.cs(166,31)`: calls `AsyncSerialization.CopyToAsync(..., progress:
  null, ...)` — `AsyncSerialization.cs` is a **Wave-0** file (`utilitiescs-nullable-extensions`,
  issue #363, already merged before this feature branched) with its own pre-existing
  `#nullable enable`; this warning is unrelated to this feature's 24-file cluster entirely.

**Disposition**: these are pre-existing test files intentionally exercising null-guard paths;
they are not part of the 24-file cluster (AC1 does not apply), were not opted into `#nullable`
by this feature (AC6 is not violated — the cross-file interaction is exposed, not caused, by
this feature's per-file pragma architecture), and the tests still **pass at runtime** (confirmed
5702/5702 in every batch's coverage run). Under the plan's required verification gates (the
scoped `UtilitiesCS.csproj` per-file pragma gate and plain `vstest`/coverage runs — see
`final-nullable-pragma-gate.md` and `final-tests-coverage.md`), these test files are never
compiled with `TreatWarningsAsErrors`, so they do not fail any gate this plan requires. This is
recorded as a non-blocking observation for the maintainer; a future small fix (adding `!` to the
two feature-adjacent test call sites) is a reasonable follow-up but is out of scope for this
annotation-only, 24-file-cluster plan.

Neither genuine-rebuild run (`/t:Build` or `/t:Rebuild`) reported any error; both are "0
Error(s)". No restart of the Final QC phase is required (no files were changed by either
command).
