# utilitiescs-nullable-helperclasses — Atomic Implementation Plan

- **Issue:** #364
- **Parent:** Epic `utilitiescs-nullable-remediation` (child, Wave 0)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-18T21-21
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-feature

## Requirements Sources (read all in Phase 0)

- `docs/features/active/utilitiescs-nullable-helperclasses/spec.md` (Definition of Done — AC source)
- `docs/features/active/utilitiescs-nullable-helperclasses/user-story.md` (Acceptance Criteria — AC source)
- `docs/features/active/utilitiescs-nullable-helperclasses/issue.md`
- `docs/features/active/utilitiescs-nullable-helperclasses/research/research-findings.2026-07-18T21-45.md`
- `docs/features/epics/utilitiescs-nullable-remediation/epic.md`

Policy compliance is governed by `CLAUDE.md`, `.claude/rules/general-code-change.md`,
`.claude/rules/general-unit-test.md`, and `.claude/rules/csharp.md`. Do not duplicate their content
here; comply with them.

## Hard Constraints (encoded, non-negotiable)

- Per-file `#nullable enable` pragma on each remediated file under `UtilitiesCS/HelperClasses/`;
  bring each opted-in file to ZERO CS86xx under the pragma.
- NO project-level or solution-level `<Nullable>` element. `UtilitiesCS.csproj` keeps none.
- Annotation and null-safety ONLY: `?` annotations, null guards, `!` only where justified, and
  null-flow corrections. NO behavior change, NO refactor, NO API redesign, NO feature work.
- Annotations on public members become cross-module contracts for Wave-1 dependents; keep public
  signatures behavior-compatible and annotate to reflect actual runtime null behavior.

## CRITICAL Toolchain Deviation (applies to every nullable/type-check task in this plan)

The nullable / type-check verification step MUST use the pragma-only build and MUST NOT add
`/p:Nullable=enable`:

`msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`

Rationale: adding `/p:Nullable=enable` turns nullable ON project-wide and surfaces the entire
epic's ~2131 CS86xx diagnostics across ~234 files as false failures unrelated to issue #364.
Enforcement for this child is per-file pragma only. This is a deliberate, documented deviation from
the stock `CLAUDE.md` / `.claude/rules/csharp.md` type-check command, for THIS child only. It MUST
NOT be resolved by editing `.claude/rules/*`. The remaining toolchain stages are standard:

- Format: `dotnet tool run csharpier .` (or `csharpier .`)
- Analyzers / codestyle: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- Type-check (nullable, pragma-only): the `/t:Rebuild ... /p:TreatWarningsAsErrors=true` command above (NO `/p:Nullable=enable`)
- Test + coverage: `vstest.console.exe <UtilitiesCS test assemblies> /EnableCodeCoverage` (repo-canonical full-suite driver: `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, which wraps `vstest.console.exe` with coverage and emits Cobertura XML)

## Evidence Path Scheme (non-overridable)

All evidence artifacts resolve under
`docs/features/active/utilitiescs-nullable-helperclasses/evidence/<kind>/` with kinds `baseline`,
`regression-testing`, `qa-gates`, `other`. Timestamps use `yyyy-MM-ddTHH-mm`. No `artifacts/...`
evidence path is used. The delegation prompt supplied only canonical `evidence/` kinds, so no
`EVIDENCE_LOCATION_OVERRIDE_REJECTED` substitution is required.

---

### Phase 0 — Policy Reads and Baseline Capture

- [x] [P0-T1] Read the policy and requirements files in order and emit a policy-read evidence artifact to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/baseline/phase0-instructions-read.<yyyy-MM-ddTHH-mm>.md`
  - Read order: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, then `docs/features/active/utilitiescs-nullable-helperclasses/spec.md`, `user-story.md`, `issue.md`, and `research/research-findings.2026-07-18T21-45.md`.
  - Acceptance: artifact contains `Timestamp:`, `Policy Order:`, and an explicit list of every file read.
- [x] [P0-T2] Run the CSharpier format check baseline and record it to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/baseline/csharpier-baseline.<yyyy-MM-ddTHH-mm>.md`
  - Command: `dotnet tool run csharpier --check .`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and any unformatted-file count).
- [x] [P0-T3] Run the analyzer/codestyle build baseline and record it to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/baseline/analyzer-build-baseline.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build result and analyzer warning/error counts).
- [x] [P0-T4] Run the pragma-only nullable build baseline (expected clean because no `HelperClasses/` file yet carries a pragma and the project has no `<Nullable>` element) and record it to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/baseline/nullable-build-baseline.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording the pre-opt-in CS86xx count (expected 0) and confirming NO `/p:Nullable=enable` was passed.
- [x] [P0-T5] Run the coverage baseline over the UtilitiesCS test assemblies and record numeric coverage to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/baseline/coverage-baseline.<yyyy-MM-ddTHH-mm>.md` with the Cobertura XML at `docs/features/active/utilitiescs-nullable-helperclasses/evidence/baseline/coverage-baseline.<yyyy-MM-ddTHH-mm>.cobertura.xml`
  - Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-helperclasses/evidence/baseline/coverage-baseline.<yyyy-MM-ddTHH-mm>.cobertura.xml` (full-suite driver wrapping `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`).
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with NUMERIC baseline overall `line-rate`/`branch-rate` from the Cobertura root `<coverage>` element AND the targeted `UtilitiesCS/HelperClasses/` line percentage if obtainable from per-package figures; passed/failed test counts recorded.

### Phase 1 — Batch 1 Root Pure and Simple Helpers

- [x] [P1-T1] Add `#nullable enable` to `UtilitiesCS/HelperClasses/BinaryFlags/GenericBitwise.cs` and apply annotation-only null-safety edits (prefer removing redundant `= null` field initializers reassigned in the ctor) so the file emits zero CS86xx under the pragma
  - Acceptance: file carries the pragma; no behavior change; zero CS86xx for this file under the pragma-only build (verified in P1-T9).
- [x] [P1-T2] Add `#nullable enable` to `UtilitiesCS/HelperClasses/MergeSortImplementations.cs` and apply annotation-only null-safety edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P1-T9).
- [x] [P1-T3] Add `#nullable enable` to `UtilitiesCS/HelperClasses/ObjectSize.cs` and annotate reflection `GetValue` results as nullable (`object?`) with real guards to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P1-T9).
- [x] [P1-T4] Add `#nullable enable` to `UtilitiesCS/HelperClasses/ParamArray.cs` and annotate the genuinely-optional `_args` field nullable (fixing CS8618 and the `AnyNull()` dereference) to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P1-T9).
- [x] [P1-T5] Add `#nullable enable` to `UtilitiesCS/HelperClasses/SimpleRegex.cs` and apply annotation-only null-safety edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P1-T9).
- [x] [P1-T6] Add `#nullable enable` to `UtilitiesCS/HelperClasses/Tokenizer.cs` and apply annotation-only null-safety edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P1-T9).
- [x] [P1-T7] Add `#nullable enable` to `UtilitiesCS/HelperClasses/SegmentStopWatch.cs` and apply annotation-only null-safety edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P1-T9).
- [x] [P1-T8] Run CSharpier over the Batch 1 files (`UtilitiesCS/HelperClasses/BinaryFlags/GenericBitwise.cs`, `MergeSortImplementations.cs`, `ObjectSize.cs`, `ParamArray.cs`, `SimpleRegex.cs`, `Tokenizer.cs`, `SegmentStopWatch.cs`) with `dotnet tool run csharpier .` and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [x] [P1-T9] Run the pragma-only nullable build and record Batch 1 verification to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/qa-gates/batch1-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 7 opted-in Batch 1 files and NO new diagnostics elsewhere (build result matches the P0-T4 baseline).
- [x] [P1-T10] Run the Batch 1 UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/regression-testing/batch1-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~GenericBitwise|FullyQualifiedName~MergeSort|FullyQualifiedName~ObjectSize|FullyQualifiedName~ParamArray|FullyQualifiedName~SimpleRegex|FullyQualifiedName~Tokenizer|FullyQualifiedName~SegmentStopWatch"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with passed/failed counts; all Batch 1 tests green and behavior-identical (no assertions added, removed, or weakened).

### Phase 2 — Batch 2 Logging

- [x] [P2-T1] Add `#nullable enable` to `UtilitiesCS/HelperClasses/Logging/DebugTextLogger.cs` and apply annotation-only null-safety edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P2-T6).
- [x] [P2-T2] Add `#nullable enable` to `UtilitiesCS/HelperClasses/Logging/DebugTextWriter.cs` and apply annotation-only null-safety edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P2-T6).
- [x] [P2-T3] Add `#nullable enable` to `UtilitiesCS/HelperClasses/Logging/VerboseLogger.cs` and apply annotation-only null-safety edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P2-T6).
- [x] [P2-T4] Add `#nullable enable` to `UtilitiesCS/HelperClasses/Logging/TraceUtility.cs` and settle its cross-module extension-method return-nullability contracts (`GetMyMethodNames`, `GetMyTraceString`, `GetCallerMethod`, `GetAssembly`) with real guards/nullable annotations (`MethodBase?`, `DeclaringType`, `StackFrame.GetMethod()`, lazy `_projectNames`) to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; public extension-method signatures remain behavior-compatible; zero CS86xx (verified in P2-T6).
- [x] [P2-T5] Run CSharpier over the Batch 2 files (`Logging/DebugTextLogger.cs`, `Logging/DebugTextWriter.cs`, `Logging/VerboseLogger.cs`, `Logging/TraceUtility.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [x] [P2-T6] Run the pragma-only nullable build and record Batch 2 verification to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/qa-gates/batch2-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 4 opted-in Batch 2 files and NO new diagnostics elsewhere.
- [x] [P2-T7] Run the Batch 2 UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/regression-testing/batch2-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~DebugTextLogger|FullyQualifiedName~VerboseLogger|FullyQualifiedName~TraceUtility"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; all Batch 2 tests green and behavior-identical.

### Phase 3 — Batch 3 Cloning and Reflection

- [x] [P3-T1] Add `#nullable enable` to `UtilitiesCS/HelperClasses/CloningFunctions/DeepCompare.cs` and annotate `PropertyInfo.GetValue` results and the `List<(string, object?, object?)>` element contract with guards to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P3-T6).
- [x] [P3-T2] Add `#nullable enable` to `UtilitiesCS/HelperClasses/CloningFunctions/ObjectCopier.cs` and make the deliberate `Clone<T>` nullable-return contract decision (`T?` for the `return default` null-source path and `(T?)formatter.Deserialize(...)`) to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; the `Clone<T>` return-nullability decision is recorded as a deliberate downstream contract; zero CS86xx (verified in P3-T6).
- [x] [P3-T3] Add `#nullable enable` to `UtilitiesCS/HelperClasses/CloningFunctions/DispatchUtility.cs` and annotate COM-interop null surfaces (`GetType` returning `Type?` when `!throwIfNotFound`, `Invoke` returning `object?`, `out`/`ref` params, `!` on documented post-call COM reads with `// why`) to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; public `GetType`/`Invoke` nullability reflects actual behavior; zero CS86xx (verified in P3-T6).
- [x] [P3-T4] Add `#nullable enable` to `UtilitiesCS/HelperClasses/ReflectionHelper.cs` and annotate nullable reflection locals (`Type? = type.BaseType`, `ex.Types` as `Type?[]`) with guards, consuming the TraceUtility contracts settled in Batch 2, to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P3-T6).
- [x] [P3-T5] Run CSharpier over the Batch 3 files (`CloningFunctions/DeepCompare.cs`, `CloningFunctions/ObjectCopier.cs`, `CloningFunctions/DispatchUtility.cs`, `ReflectionHelper.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [x] [P3-T6] Run the pragma-only nullable build and record Batch 3 verification to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/qa-gates/batch3-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 4 opted-in Batch 3 files and NO new diagnostics elsewhere.
- [x] [P3-T7] Run the Batch 3 UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/regression-testing/batch3-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~DeepCompare|FullyQualifiedName~ObjectCopier|FullyQualifiedName~DispatchUtility|FullyQualifiedName~ReflectionHelper"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; all Batch 3 tests green and behavior-identical.

### Phase 4 — Batch 4 FileSystem Wrappers and Adapters

- [ ] [P4-T1] Add `#nullable enable` to `UtilitiesCS/HelperClasses/FileSystem/FileSystemInfoWrapper.cs` (clean delegating wrapper; ctor already `?? throw`) and apply annotation-only edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P4-T9).
- [ ] [P4-T2] Add `#nullable enable` to `UtilitiesCS/HelperClasses/FileSystem/DirectoryInfoWrapper.cs` and annotate the `Parent`/`Root` BCL-null-into-throwing-ctor boundary with a behavior-preserving `!` plus a short `// why` comment (the wrapped `IDirectoryInfo` is oblivious/out-of-scope) to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; root boundary uses `!` (not a nullable contract change); zero CS86xx (verified in P4-T9).
- [ ] [P4-T3] Add `#nullable enable` to `UtilitiesCS/HelperClasses/FileSystem/FileInfoWrapper.cs` and annotate the `Directory`/`DirectoryName` null boundary with a behavior-preserving `!` plus `// why` comment to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P4-T9).
- [ ] [P4-T4] Add `#nullable enable` to `UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs` and annotate the `Parent`/`Root` boundary (BCL `DirectoryInfo?` passed into the throwing `DirectoryInfoWrapper` ctor) with a behavior-preserving `!` plus `// why` comment to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; latent root-throws behavior is preserved (not fixed); zero CS86xx (verified in P4-T9).
- [ ] [P4-T5] Add `#nullable enable` to `UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs` and annotate the `Directory`/`DirectoryName` boundary while preserving the injectable-delegate seam EXACTLY (the `_appendText`/`_openByMode`/`_openByModeAndAccess`/`_openWrite` fields, both constructors, and the `?? throw` guards must remain unchanged) to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; seam fields, both ctors, and `?? throw` guards byte-unchanged except for annotations; the `PhysicalFileSystemAdapters_Tests` seam is not perturbed; zero CS86xx (verified in P4-T9).
- [ ] [P4-T6] Add `#nullable enable` to `UtilitiesCS/HelperClasses/FileSystem/MyFileSystemInfo.cs` and annotate the `AsDirectory`/`AsFile` (`as`) locals, the `Length` dereference, `Equals(object? obj)`, and `==`/`!=` operand nullability to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P4-T9).
- [ ] [P4-T7] Record the FileSystem adapter root-boundary flags to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/other/maintainer-flags.<yyyy-MM-ddTHH-mm>.md`
  - Acceptance: artifact records (a) the behavior-preserving `!` decision at `Parent`/`Root`/`Directory`/`DirectoryName` boundaries, and (b) the latent "root throws `ArgumentNullException`" behavior FLAGGED for a possible future issue (not fixed here), with `Timestamp:`.
- [ ] [P4-T8] Run CSharpier over the Batch 4 files (`FileSystem/FileSystemInfoWrapper.cs`, `FileSystem/DirectoryInfoWrapper.cs`, `FileSystem/FileInfoWrapper.cs`, `FileSystem/PhysicalDirectoryInfoAdapter.cs`, `FileSystem/PhysicalFileInfoAdapter.cs`, `FileSystem/MyFileSystemInfo.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P4-T9] Run the pragma-only nullable build and record Batch 4 verification to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/qa-gates/batch4-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 6 opted-in Batch 4 files and NO new diagnostics elsewhere.
- [ ] [P4-T10] Run the Batch 4 UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/regression-testing/batch4-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~FileSystemInfoWrapper|FullyQualifiedName~DirectoryInfoWrapper|FullyQualifiedName~FileInfoWrapper|FullyQualifiedName~PhysicalFileSystemAdapters|FullyQualifiedName~MyFileSystemInfo"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; all Batch 4 tests green and behavior-identical, with the PhysicalFileSystemAdapters tests deterministic (no reintroduced shared-file flakiness).

### Phase 5 — Batch 5 COM P-Invoke and Form Special Cases

- [ ] [P5-T1] Add `#nullable enable` to `UtilitiesCS/HelperClasses/FileSystem/ShellUtilities.cs` and annotate `GetFileIcon` to `Icon?` plus P/Invoke marshaled-struct string fields to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P5-T8).
- [ ] [P5-T2] Add `#nullable enable` to `UtilitiesCS/HelperClasses/FileSystem/ShellUtilitiesStatic.cs` and annotate `GetFileIcon` return `Icon?` (matching the existing XML doc) and SHGetFileInfo/ShellExecute marshaled string fields to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P5-T8).
- [ ] [P5-T3] Add `#nullable enable` to `UtilitiesCS/HelperClasses/FileSystem/SysImageListHelper.cs` and annotate the two mutually-exclusive `listView`/`treeView` fields nullable, the collection-getter properties, and the `GetImageIndex` dereferences to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P5-T8).
- [ ] [P5-T4] Add `#nullable enable` to `UtilitiesCS/HelperClasses/WipUnfinished/ComStreamWrapper.cs` and apply annotation-only edits (`out STATSTG stat`; fields non-null in ctor) to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P5-T8).
- [ ] [P5-T5] Add `#nullable enable` to the hand-written partial `UtilitiesCS/HelperClasses/DvgForm.cs` and annotate the event handler `object? sender` to reach zero CS86xx
  - Acceptance: file carries the pragma; only the `object sender` -> `object? sender` annotation change; zero CS86xx (verified in P5-T8).
- [ ] [P5-T6] Confirm `UtilitiesCS/HelperClasses/DvgForm.Designer.cs` is left NON-opted-in (no `#nullable enable` pragma, no hand-edit) and record the epic-scope conflict to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/other/maintainer-flags.<yyyy-MM-ddTHH-mm>.md`
  - Acceptance: `DvgForm.Designer.cs` is byte-unchanged (no pragma, `InitializeComponent`/generated members untouched); artifact records the default exception (Designer file stays oblivious per the "do not touch Designer files" convention) and the maintainer-decision fallback (annotate only `private IContainer? components = null;` if all 43 files must be opted-in), with `Timestamp:`.
- [ ] [P5-T7] Run CSharpier over the Batch 5 hand-written files (`FileSystem/ShellUtilities.cs`, `FileSystem/ShellUtilitiesStatic.cs`, `FileSystem/SysImageListHelper.cs`, `WipUnfinished/ComStreamWrapper.cs`, `DvgForm.cs`) and confirm no residual formatting diff; do NOT run CSharpier against `DvgForm.Designer.cs`
  - Acceptance: `csharpier --check .` exits 0 for the touched files; `DvgForm.Designer.cs` unchanged.
- [ ] [P5-T8] Run the pragma-only nullable build and record Batch 5 verification to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/qa-gates/batch5-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 5 opted-in Batch 5 files, that `DvgForm.Designer.cs` produces no CS86xx (oblivious, does not cross-block `DvgForm.cs`), and NO new diagnostics elsewhere.
- [ ] [P5-T9] Run the Batch 5 UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/regression-testing/batch5-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~ShellUtilities|FullyQualifiedName~SysImageListHelper|FullyQualifiedName~ComStreamWrapper|FullyQualifiedName~DvgForm"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; all Batch 5 tests green and behavior-identical.

### Phase 6 — Batch 6 Windows Forms

- [ ] [P6-T1] Add `#nullable enable` to `UtilitiesCS/HelperClasses/Windows Forms/ControlPosition.cs` and apply annotation-only null-safety edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P6-T9).
- [ ] [P6-T2] Add `#nullable enable` to `UtilitiesCS/HelperClasses/Windows Forms/ControlResizer.cs` and annotate the `ControlInfo` non-null `string` fields (CS8618) and `ctl.Parent` re-access without altering the existing catch behavior to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P6-T9).
- [ ] [P6-T3] Add `#nullable enable` to `UtilitiesCS/HelperClasses/Windows Forms/ImageHelper.cs` and apply annotation-only null-safety edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P6-T9).
- [ ] [P6-T4] Add `#nullable enable` to `UtilitiesCS/HelperClasses/Windows Forms/MouseDownFilter.cs` and annotate `event EventHandler? FormClicked` and remove the redundant `form = null` init reassigned in the ctor to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P6-T9).
- [ ] [P6-T5] Add `#nullable enable` to `UtilitiesCS/HelperClasses/Windows Forms/OlvExtension.cs` and apply annotation-only edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P6-T9).
- [ ] [P6-T6] Add `#nullable enable` to `UtilitiesCS/HelperClasses/Windows Forms/ScreenHelper.cs` and apply annotation-only null-safety edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P6-T9).
- [ ] [P6-T7] Add `#nullable enable` to `UtilitiesCS/HelperClasses/Windows Forms/TableLayoutHelper.cs` and apply annotation-only null-safety edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P6-T9).
- [ ] [P6-T8] Run CSharpier over the Batch 6 files under `UtilitiesCS/HelperClasses/Windows Forms/` (`ControlPosition.cs`, `ControlResizer.cs`, `ImageHelper.cs`, `MouseDownFilter.cs`, `OlvExtension.cs`, `ScreenHelper.cs`, `TableLayoutHelper.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P6-T9] Run the pragma-only nullable build and record Batch 6 verification to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/qa-gates/batch6-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 7 opted-in Batch 6 files and NO new diagnostics elsewhere.
- [ ] [P6-T10] Run the Batch 6 UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/regression-testing/batch6-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~ControlResizer|FullyQualifiedName~ControlPosition|FullyQualifiedName~ImageHelper|FullyQualifiedName~MouseDownFilter|FullyQualifiedName~OlvExtension|FullyQualifiedName~ScreenHelper|FullyQualifiedName~TableLayoutHelper"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; all Batch 6 tests green and behavior-identical.

### Phase 7 — Batch 7 ThemeHelpers and ToolTips

- [ ] [P7-T1] Add `#nullable enable` to `UtilitiesCS/HelperClasses/ThemeHelpers/SystemThemeDetector.cs` and annotate `OpenSubKey` (`RegistryKey?`) and `GetValue` (`object?`) results with the existing guards to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P7-T8).
- [ ] [P7-T2] Add `#nullable enable` to `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs` and annotate the optional-parameter defaults (`IUiDispatcher? uiDispatcher = null`, `Action<string>? ... = null`) and reference-type fields to reach zero CS86xx, opting in together with `Theme.Rendering.cs`
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx across the partial type once both parts are opted in (verified in P7-T8).
- [ ] [P7-T3] Add `#nullable enable` to `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs` (same partial `Theme` type as `Theme.cs`; opt in together to keep consistent field-null-state analysis) and apply annotation-only edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx across the partial type (verified in P7-T8).
- [ ] [P7-T4] Add `#nullable enable` to `UtilitiesCS/HelperClasses/ThemeHelpers/ThemeControlGroup.cs` and apply annotation-only null-safety edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P7-T8).
- [ ] [P7-T5] Add `#nullable enable` to `UtilitiesCS/HelperClasses/ToolTips/QfcTipsDetails.cs` and apply annotation-only null-safety edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P7-T8).
- [ ] [P7-T6] Add `#nullable enable` to `UtilitiesCS/HelperClasses/ToolTips/TipsController.cs` and annotate the `_labelControl.Parent` casts (`as` -> nullable) and the uninitialized `_labelControl`/`_tlp`/`_panel` fields to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P7-T8).
- [ ] [P7-T7] Run CSharpier over the Batch 7 files (`ThemeHelpers/SystemThemeDetector.cs`, `ThemeHelpers/Theme.cs`, `ThemeHelpers/Theme.Rendering.cs`, `ThemeHelpers/ThemeControlGroup.cs`, `ToolTips/QfcTipsDetails.cs`, `ToolTips/TipsController.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P7-T8] Run the pragma-only nullable build and record Batch 7 verification to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/qa-gates/batch7-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 6 opted-in Batch 7 files (Theme partial type consistent) and NO new diagnostics elsewhere.
- [ ] [P7-T9] Run the Batch 7 UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/regression-testing/batch7-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~Theme|FullyQualifiedName~QfcTipsDetails|FullyQualifiedName~TipsController"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; all Batch 7 tests green and behavior-identical.

### Phase 8 — Batch 8 High-Contract Finish

- [ ] [P8-T1] Add `#nullable enable` to `UtilitiesCS/HelperClasses/Initializer.cs` and make the deliberate unconstrained-generic return-contract decisions for `SetAndSave`/`GetOrLoad`/`Load` (`ref T`, `default(T)` returns via `[return: MaybeNull]` / `T?`) to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; the generic return-nullability decisions are recorded as deliberate downstream contracts; public signatures behavior-compatible; zero CS86xx (verified in P8-T6).
- [ ] [P8-T2] Add `#nullable enable` to `UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs` and apply the two-group string-property contract split — `FilePath`/`FolderPath`/`FileName` non-null (default `""`) and `FileStemSeed`/`FileStemSuffix`/`FileStem`/`FileExtension` nullable sentinels — plus `object? sender` and `Path.GetDirectoryName` (`string?`) annotations to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; the string-property nullability split reflects actual runtime behavior; the Newtonsoft converter contract remains behavior-compatible; zero CS86xx (verified in P8-T6).
- [ ] [P8-T3] Add `#nullable enable` to `UtilitiesCS/HelperClasses/PrettyPrint.cs` and apply annotation-only null-safety edits (do NOT split the file) to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; no refactor/split; zero CS86xx (verified in P8-T6).
- [ ] [P8-T4] Record the file-size pre-existing-violation flags to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/other/maintainer-flags.<yyyy-MM-ddTHH-mm>.md`
  - Acceptance: artifact records (a) `PrettyPrint.cs` (677 lines) exceeds the 500-line limit as a PRE-EXISTING condition, flagged not fixed (splitting is out of annotation-only scope), and (b) `FilePathHelper.cs` (494 lines) is near the limit and any pragma+annotation breach of 500 is flagged rather than triggering a refactor, with `Timestamp:`.
- [ ] [P8-T5] Run CSharpier over the Batch 8 files (`Initializer.cs`, `FileSystem/FilePathHelper.cs`, `PrettyPrint.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P8-T6] Run the pragma-only nullable build and record Batch 8 verification to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/qa-gates/batch8-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 3 opted-in Batch 8 files and NO new diagnostics elsewhere.
- [ ] [P8-T7] Run the Batch 8 UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/regression-testing/batch8-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~Initializer|FullyQualifiedName~PropertyInitializer|FullyQualifiedName~FilePathHelper|FullyQualifiedName~PrettyPrint"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; all Batch 8 tests green and behavior-identical, including the FilePathHelper Newtonsoft converter tests.

### Phase 9 — Final QC Loop, Coverage Delta, and Acceptance Verification

- [ ] [P9-T1] Run the CSharpier format gate over the repository and record it to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/qa-gates/final-csharpier.<yyyy-MM-ddTHH-mm>.md`
  - Command: `dotnet tool run csharpier --check .`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with a clean (exit 0) result. If CSharpier changes files, restart the toolchain loop from this task.
- [ ] [P9-T2] Run the analyzer/codestyle build gate and record it to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/qa-gates/final-analyzer-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with a clean build and no new analyzer diagnostics vs the P0-T3 baseline. If this step changes files, restart from P9-T1.
- [ ] [P9-T3] Run the pragma-only nullable/TreatWarningsAsErrors type-check gate and record it to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/qa-gates/final-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (NO `/p:Nullable=enable`)
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across all opted-in `HelperClasses/` files and NO new diagnostics elsewhere; records that `/p:Nullable=enable` was NOT passed. If this step changes files, restart from P9-T1.
- [ ] [P9-T4] Run the coverage-enabled test gate over the UtilitiesCS test assemblies and record numeric post-change coverage to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/qa-gates/final-coverage.<yyyy-MM-ddTHH-mm>.md` with the Cobertura XML at `docs/features/active/utilitiescs-nullable-helperclasses/evidence/qa-gates/final-coverage.<yyyy-MM-ddTHH-mm>.cobertura.xml`
  - Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-helperclasses/evidence/qa-gates/final-coverage.<yyyy-MM-ddTHH-mm>.cobertura.xml`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with NUMERIC post-change overall `line-rate`/`branch-rate` and the `UtilitiesCS/HelperClasses/` targeted percentage if obtainable, plus passed/failed test counts (all UtilitiesCS tests green). If this step changes files, restart from P9-T1.
- [ ] [P9-T5] Verify `UtilitiesCS.csproj` introduces no project-level or solution-level `<Nullable>` element and record the check to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/qa-gates/csproj-no-nullable.<yyyy-MM-ddTHH-mm>.md`
  - Command: grep `UtilitiesCS/UtilitiesCS.csproj` (and `TaskMaster.sln`) for `<Nullable>`.
  - Acceptance: artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming no `<Nullable>` element exists (DoD item satisfied).
- [ ] [P9-T6] Compute the coverage delta and changed-line no-regression check and record it to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/qa-gates/coverage-delta.<yyyy-MM-ddTHH-mm>.md`
  - Inputs: baseline Cobertura from P0-T5 and post-change Cobertura from P9-T4.
  - Acceptance: artifact records baseline coverage (numeric), post-change coverage (numeric), and changed-line coverage, and confirms NO coverage regression on changed lines; if regression is detected the outcome is remediation-required, not PASS.
- [ ] [P9-T7] Map each acceptance-criteria checkbox in BOTH `spec.md` `## Definition of Done` AND `user-story.md` `## Acceptance Criteria` (7 checkboxes) to its satisfying phase/task per the `acceptance-criteria-tracking` skill (full-feature mode: track each source file independently) and record it to `docs/features/active/utilitiescs-nullable-helperclasses/evidence/qa-gates/ac-checkoff.<yyyy-MM-ddTHH-mm>.md`
  - Acceptance: artifact contains `Timestamp:` and, for `spec.md` `## Definition of Done`, a row per DoD item — per-file pragma/zero-CS86xx (Phases 1-8 + P9-T3), no `<Nullable>` element (P9-T5), annotation/null-safety only (Phases 1-8 + P9-T1/T2), tests pass / no changed-line coverage regression (P9-T4/T6), full toolchain final pass with pragma-only type-check (P9-T1..T4), PhysicalFileInfoAdapter seam preserved (P4-T5), adapter root-boundary `!` with flag (P4-T2..T4, P4-T7), DvgForm.Designer.cs handling documented (P5-T6), PrettyPrint 500-line flag (P8-T4) — each mapped to a satisfying task with its evidence path; AND, for `user-story.md` `## Acceptance Criteria`, a separate independent section with one row per each of the 7 Acceptance-Criteria checkboxes mapped to its satisfying task with its evidence path. Both source files must have their checkboxes updated to `[x]` as each item is verified.
