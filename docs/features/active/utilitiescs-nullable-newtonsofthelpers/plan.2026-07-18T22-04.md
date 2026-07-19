# utilitiescs-nullable-newtonsofthelpers — Atomic Implementation Plan

- **Issue:** #367
- **Parent:** Epic `utilitiescs-nullable-remediation` (child, Wave 0)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-18T22-04
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-feature

## Requirements Sources (read all in Phase 0)

- `docs/features/active/utilitiescs-nullable-newtonsofthelpers/spec.md` (Definition of Done — AC source)
- `docs/features/active/utilitiescs-nullable-newtonsofthelpers/user-story.md` (Acceptance Criteria — AC source)
- `docs/features/active/utilitiescs-nullable-newtonsofthelpers/issue.md`
- `docs/features/active/utilitiescs-nullable-newtonsofthelpers/research/research-findings.2026-07-18T22-05.md`
- `docs/features/epics/utilitiescs-nullable-remediation/epic.md`

Policy compliance is governed by `CLAUDE.md`, `.claude/rules/general-code-change.md`,
`.claude/rules/general-unit-test.md`, and `.claude/rules/csharp.md`. Do not duplicate their content
here; comply with them.

## Hard Constraints (encoded, non-negotiable)

- Per-file `#nullable enable` pragma on each remediated file under `UtilitiesCS/NewtonsoftHelpers/`
  (recursive, including `MonoExtension/` and `SDIL Reader/`); bring each opted-in file to ZERO
  CS86xx under the pragma.
- NO project-level or solution-level `<Nullable>` element. `UtilitiesCS.csproj` keeps none.
- Annotation and null-safety ONLY: `?` annotations, null guards, `!` only where justified, and
  null-flow corrections. NO behavior change, NO refactor, NO API redesign, NO feature work.
- Newtonsoft.Json 13.0.4 override signatures (`ReadJson`/`WriteJson`/`CanConvert`/`Create`/
  `BindToType`/`BindToName`/`Trace`) carry framework-defined nullability that must be MATCHED, not
  changed: nullable positions (`existingValue`, `value`, converter `ReadJson` returns, `BindToType`
  `assemblyName`, `BindToName` `out string?` params, `Trace` `Exception? ex`) are annotated
  nullable; non-null positions (`serializer`, `reader`, `writer`, `objectType`, `serializedType`,
  `typeName`, `message`) stay non-null.
- Annotations on public members become cross-module contracts for serialization/persistence
  callers (ReusableTypes, OutlookObjects/Store, EmailIntelligence, and the app-level
  `AppGlobalsConverter` consumers); keep public signatures behavior-compatible and annotate to
  reflect actual runtime null behavior.

## CRITICAL Toolchain Deviation (applies to every nullable/type-check task in this plan)

The nullable / type-check verification step MUST use the pragma-only build and MUST NOT add
`/p:Nullable=enable`:

`msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`

Rationale: adding `/p:Nullable=enable` turns nullable ON project-wide and surfaces the entire
epic's ~2131 CS86xx diagnostics across ~234 files as false failures unrelated to issue #367.
Enforcement for this child is per-file pragma only. This is a deliberate, documented deviation from
the stock `CLAUDE.md` / `.claude/rules/csharp.md` type-check command, for THIS child only. It MUST
NOT be resolved by editing `.claude/rules/*`. The remaining toolchain stages are standard:

- Format: `dotnet tool run csharpier .` (or `csharpier .`)
- Analyzers / codestyle: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- Type-check (nullable, pragma-only): the `/t:Rebuild ... /p:TreatWarningsAsErrors=true` command above (NO `/p:Nullable=enable`)
- Test + coverage: repo-canonical full-suite driver `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (wraps `dotnet-coverage` + `vstest.console.exe` and emits Cobertura XML); fallback `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`

## Evidence Path Scheme (non-overridable)

All evidence artifacts resolve under
`docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/<kind>/` with kinds
`baseline`, `regression-testing`, `qa-gates`, `other`. Timestamps use `yyyy-MM-ddTHH-mm`. No
`artifacts/...` evidence path is used. The delegation prompt supplied only canonical `evidence/`
kinds, so no `EVIDENCE_LOCATION_OVERRIDE_REJECTED` substitution is required.

---

### Phase 0 — Policy Reads and Baseline Capture

- [ ] [P0-T1] Read the policy and requirements files in order and emit a policy-read evidence artifact to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/baseline/phase0-instructions-read.<yyyy-MM-ddTHH-mm>.md`
  - Read order (all as paths under this worktree root): `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, then `docs/features/active/utilitiescs-nullable-newtonsofthelpers/spec.md`, `user-story.md`, `issue.md`, `research/research-findings.2026-07-18T22-05.md`, and `docs/features/epics/utilitiescs-nullable-remediation/epic.md`.
  - Acceptance: artifact contains `Timestamp:`, `Policy Order:`, and an explicit list of every file read.
- [ ] [P0-T2] Run the CSharpier format check baseline and record it to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/baseline/csharpier-baseline.<yyyy-MM-ddTHH-mm>.md`
  - Command: `dotnet tool run csharpier --check .`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and any unformatted-file count).
- [ ] [P0-T3] Run the analyzer/codestyle build baseline and record it to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/baseline/analyzer-build-baseline.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build result and analyzer warning/error counts).
- [ ] [P0-T4] Run the pragma-only nullable build baseline (expected clean because no `NewtonsoftHelpers/` file yet carries a top-of-file pragma — `NonRecursiveConverter.cs` has only a mid-file pragma — and the project has no `<Nullable>` element) and record it to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/baseline/nullable-build-baseline.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording the pre-opt-in CS86xx count (expected 0) and confirming NO `/p:Nullable=enable` was passed.
- [ ] [P0-T5] Run the coverage baseline over the UtilitiesCS test assemblies and record numeric coverage to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/baseline/coverage-baseline.<yyyy-MM-ddTHH-mm>.md` with the Cobertura XML at `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/baseline/coverage-baseline.<yyyy-MM-ddTHH-mm>.cobertura.xml`
  - Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/baseline/coverage-baseline.<yyyy-MM-ddTHH-mm>.cobertura.xml` (full-suite driver wrapping `dotnet-coverage` + `vstest.console.exe ... /EnableCodeCoverage`; fallback `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`).
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with NUMERIC baseline overall `line-rate`/`branch-rate` from the Cobertura root `<coverage>` element AND the targeted `UtilitiesCS/NewtonsoftHelpers/` line percentage if obtainable from per-package figures; passed/failed test counts recorded.
- [ ] [P0-T6] Record the cross-cutting maintainer flags to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/other/maintainer-flags.<yyyy-MM-ddTHH-mm>.md`
  - Acceptance: artifact records, with `Timestamp:`, (a) the pragma-only verification-command deviation (do NOT use `/p:Nullable=enable`) as a deliberate documented per-child deviation from `.claude/rules/csharp.md` that is NOT resolved by editing `.claude/rules/*`, and (b) the rules-vs-convention conflict (`.claude/rules/csharp.md` documents forcing `/p:Nullable=enable` globally, conflicting with the per-file opt-in convention) FLAGGED for the epic capstone child and NOT resolved here. This artifact is appended to by later per-batch flag tasks (P3-T3, P6-T4, P7-T1).

### Phase 1 — Batch 1 Leaf and Isolated Helpers

- [ ] [P1-T1] Add `#nullable enable` to `UtilitiesCS/NewtonsoftHelpers/AllInclusiveBinder.cs` and make the deliberate `GetAssemblies()` return-nullability contract decision (`Assembly[]?` for the `return null;` stub path; plain class, no interface constraint) with annotation-only edits so the file emits zero CS86xx under the pragma
  - Acceptance: file carries the pragma; annotation-only; the `Assembly[]?` return decision is recorded as a deliberate contract; zero CS86xx for this file under the pragma-only build (verified in P1-T4).
- [ ] [P1-T2] Add `#nullable enable` to `UtilitiesCS/NewtonsoftHelpers/MonoExtension/MonoExtension.cs` and apply annotation-only null-safety edits (Mono.Reflection `Instruction.Operand` casts; existing `is`-pattern branches already narrow) to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; namespace unchanged; zero CS86xx (verified in P1-T4).
- [ ] [P1-T3] Run CSharpier over the Batch 1 files (`NewtonsoftHelpers/AllInclusiveBinder.cs`, `NewtonsoftHelpers/MonoExtension/MonoExtension.cs`) with `dotnet tool run csharpier .` and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P1-T4] Run the pragma-only nullable build and record Batch 1 verification to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/qa-gates/batch1-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 2 opted-in Batch 1 files, NO new diagnostics elsewhere (result matches the P0-T4 baseline), and that `/p:Nullable=enable` was NOT passed.
- [ ] [P1-T5] Run the Batch 1 UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/regression-testing/batch1-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~AllInclusiveBinder|FullyQualifiedName~MonoExtension"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with passed/failed counts; all Batch 1 tests green and behavior-identical (no assertions added, removed, or weakened).

### Phase 2 — Batch 2 SDIL Reader

- [ ] [P2-T1] Add `#nullable enable` to `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` and apply annotation-only edits (`Module[]? modules`, the LoadOpCodes-only static fields, `info1.GetValue(null)` unbox guarded by `FieldType == typeof(OpCode)` behavior-preserving `!`) to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P2-T5).
- [ ] [P2-T2] Add `#nullable enable` to `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILInstruction.cs` and annotate the settable `object? operand` / `byte[]? operandData` fields (public `Operand`/`OperandData` become `object?`/`byte[]?`) with the existing `!= null` guards and behavior-preserving `!` on the `ReflectedType` reads to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P2-T5).
- [ ] [P2-T3] Add `#nullable enable` to `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs` and annotate the explicitly null-initialized fields (`List<ILInstruction>? instructions`, `byte[]? il`, `MethodInfo? mi`), preserving the `il`-non-null-after-construct invariant with a documented `= null!`/localized `!` and a `// why` comment, to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; the `il` invariant decision is documented in-code; zero CS86xx (verified in P2-T5).
- [ ] [P2-T4] Run CSharpier over the Batch 2 files (`SDIL Reader/ILGlobals.cs`, `SDIL Reader/ILInstruction.cs`, `SDIL Reader/MethodBodyReader.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P2-T5] Run the pragma-only nullable build and record Batch 2 verification to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/qa-gates/batch2-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 3 opted-in Batch 2 files and NO new diagnostics elsewhere.
- [ ] [P2-T6] Run the Batch 2 UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/regression-testing/batch2-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~ILGlobals|FullyQualifiedName~ILInstruction|FullyQualifiedName~MethodBodyReader"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; all Batch 2 tests green and behavior-identical.

### Phase 3 — Batch 3 Trace Writers

- [ ] [P3-T1] Add `#nullable enable` to `UtilitiesCS/NewtonsoftHelpers/NConsoleTraceWriter.cs` and match the `ITraceWriter.Trace` framework signature (`Exception? ex`) plus make the deliberate public `Log` property contract decision (`Action<string, Exception?>? Log`, resolving the CS8618 uninitialized field and the nullable-`ex` invoke) to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; `Trace(TraceLevel, string, Exception?)` matches the framework; `message` stays non-null; the `Log` property nullability is recorded as a deliberate public contract; zero CS86xx (verified in P3-T5).
- [ ] [P3-T2] Add `#nullable enable` to `UtilitiesCS/NewtonsoftHelpers/NLogTraceWriter.cs` (declared in the GLOBAL namespace — leave the namespace unchanged; annotate in place) and apply annotation-only edits (`Trace` `Exception? ex`; `GetLogFunction` return `Action<string, Exception>?`; behavior-preserving `!` on `GetCurrentMethod().DeclaringType`) to reach zero CS86xx
  - Acceptance: file carries the pragma; NO `namespace` block added (global namespace preserved); annotation-only; `Trace` matches the framework; zero CS86xx (verified in P3-T5).
- [ ] [P3-T3] Append the `NLogTraceWriter.cs` global-namespace flag to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/other/maintainer-flags.<yyyy-MM-ddTHH-mm>.md`
  - Acceptance: artifact records, with `Timestamp:`, that `NLogTraceWriter.cs` declares its class in the GLOBAL namespace (pre-existing structural oddity, not a nullable issue) and was annotated in place with the namespace unchanged (moving it would be an out-of-scope behavior/reference change).
- [ ] [P3-T4] Run CSharpier over the Batch 3 files (`NewtonsoftHelpers/NConsoleTraceWriter.cs`, `NewtonsoftHelpers/NLogTraceWriter.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P3-T5] Run the pragma-only nullable build and record Batch 3 verification to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/qa-gates/batch3-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 2 opted-in Batch 3 files and NO new diagnostics elsewhere.
- [ ] [P3-T6] Run the Batch 3 UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/regression-testing/batch3-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~NConsoleTraceWriter|FullyQualifiedName~NLogTraceWriter"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; all Batch 3 tests green and behavior-identical.

### Phase 4 — Batch 4 Binder and Simple Converters

- [ ] [P4-T1] Add `#nullable enable` to `UtilitiesCS/NewtonsoftHelpers/KnownTypesBinder.cs` and make the deliberate `ISerializationBinder` contract decisions — keep `BindToType` return non-null `Type` with behavior-preserving `!` and a `// why` comment (the body returns `SingleOrDefault(...)` which is null on no match; a `Type?` return would be CS8766), annotate `BindToName` `out string? assemblyName`, and annotate the caller-populated `KnownTypes` property (`IList<Type>?`) — to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; `BindToType` return stays non-null `Type` (not a nullable contract change); `assemblyName` out matches the framework `out string?`; `serializedType`/`typeName` stay non-null; zero CS86xx (verified in P4-T6).
- [ ] [P4-T2] Add `#nullable enable` to `UtilitiesCS/NewtonsoftHelpers/AppGlobalsConverter.cs` and match the `JsonConverter<IApplicationGlobals>` framework signature (`existingValue` -> `IApplicationGlobals?`, `WriteJson` `value` -> `IApplicationGlobals?`) while keeping the `ReadJson` return non-null (`return _globals;`) and the `AppGlobalsConverter(IApplicationGlobals)` constructor parameter non-null, to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; override parameters match the framework; the non-null `ReadJson` return and non-null ctor parameter are recorded as deliberate; zero CS86xx (verified in P4-T6).
- [ ] [P4-T3] Add `#nullable enable` to `UtilitiesCS/NewtonsoftHelpers/PeopleScoRemainingObjectConverter.cs` and match the non-generic `JsonConverter` framework signature (`existingValue`/`value` -> `object?`; `ReadJson` return `object?` for the `ToObject<PeopleScoRemainingObject>(serializer)` nullable body) to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; overrides match the framework; zero CS86xx (verified in P4-T6).
- [ ] [P4-T4] Normalize the pragma in `UtilitiesCS/NewtonsoftHelpers/NonRecursiveConverter.cs` by moving the existing mid-file `#nullable enable` (line 27) to the top of the file so the whole file is opted in (the `object?` overrides already conform), then confirm zero CS86xx with no new annotations expected
  - Acceptance: the `#nullable enable` pragma sits at the top of the file; the `ReadJson`/`WriteJson`/`OnReadJson`/`OnWriteJson` `object?` overrides are unchanged; annotation-only (pragma move only); zero CS86xx (verified in P4-T6).
- [ ] [P4-T5] Run CSharpier over the Batch 4 files (`NewtonsoftHelpers/KnownTypesBinder.cs`, `NewtonsoftHelpers/AppGlobalsConverter.cs`, `NewtonsoftHelpers/PeopleScoRemainingObjectConverter.cs`, `NewtonsoftHelpers/NonRecursiveConverter.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P4-T6] Run the pragma-only nullable build and record Batch 4 verification to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/qa-gates/batch4-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 4 opted-in Batch 4 files (including the NonRecursiveConverter pragma normalization) and NO new diagnostics elsewhere.
- [ ] [P4-T7] Run the Batch 4 UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/regression-testing/batch4-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~KnownTypesBinder|FullyQualifiedName~AppGlobalsConverter|FullyQualifiedName~PeopleScoRemainingObjectConverter|FullyQualifiedName~NonRecursiveConverter"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; all Batch 4 tests green and behavior-identical.

### Phase 5 — Batch 5 Reflection Composition Helper

- [ ] [P5-T1] Add `#nullable enable` to `UtilitiesCS/NewtonsoftHelpers/DerivedCompositionConverter_ConcurrentDictionary.cs` and apply annotation-only edits — resolve the CS8618 non-null auto-props not set on every ctor path, widen the reflection-populated `Dictionary<string, object>` to `Dictionary<string, object?>` (honest) or use behavior-preserving `!`, and annotate the `Activator.CreateInstance(typeof(TDerived), true)` `object?`->`TDerived` cast — to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; no behavior change; zero CS86xx (verified in P5-T3).
- [ ] [P5-T2] Run CSharpier over the Batch 5 file (`NewtonsoftHelpers/DerivedCompositionConverter_ConcurrentDictionary.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched file.
- [ ] [P5-T3] Run the pragma-only nullable build and record Batch 5 verification to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/qa-gates/batch5-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 1 opted-in Batch 5 file and NO new diagnostics elsewhere.
- [ ] [P5-T4] Run the Batch 5 UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/regression-testing/batch5-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~DerivedCompositionConverter"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; all Batch 5 tests green and behavior-identical.

### Phase 6 — Batch 6 Wrappers

- [ ] [P6-T1] Add `#nullable enable` to `UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs` and apply annotation-only edits — behavior-preserving `!` on the reflection null surfaces (`Activator.CreateInstance` cast, `DeclaringType`, `GetGetMethod()`/`GetSetMethod()`, `GetMethodBody().GetILAsByteArray()`, `ResolveField`), match this file's existing `ModifyGetMethod`/`ModifySetMethod` return-nullability behavior WITHOUT unifying across wrappers, make the `RemainingObject` `[JsonProperty]` contract decision (`object?` vs `= null!`, verified against `ScoDictionaryNew.cs` usage), and handle the `RemainingObject is JObject` / `JToken?` path with its existing guards — to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; the `RemainingObject` and per-file `ModifyGet/SetMethod` return decisions match existing behavior; NO split (>500-line pre-existing condition preserved); zero CS86xx (verified in P6-T6).
- [ ] [P6-T2] Add `#nullable enable` to `UtilitiesCS/NewtonsoftHelpers/WrapperScDictionary.cs` and apply annotation-only edits — behavior-preserving `!` on the same reflection null surfaces, match THIS file's `ModifySetMethod` non-null (throws-on-null) return behavior and the `ReplicateProperty` non-null-checked call site WITHOUT unifying, and make the `RemainingObject` contract decision — to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; per-file `ModifyGet/SetMethod` and `RemainingObject` decisions preserved (not unified with the other wrappers); NO split; zero CS86xx (verified in P6-T6).
- [ ] [P6-T3] Add `#nullable enable` to `UtilitiesCS/NewtonsoftHelpers/WrapperPeopleScoDictionaryNew.cs` and apply annotation-only edits — behavior-preserving `!` on `GetCurrentMethod().DeclaringType` into `LogManager.GetLogger(Type)` and the reflection null surfaces, the already-`is not null`-guarded `configField?.GetValue(...) as NewSmartSerializableConfig` path, match THIS file's `ModifySetMethod` nullable (`return null;`) return behavior, and make the `RemainingObject` contract decision — to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; per-file `ModifyGet/SetMethod` and `RemainingObject` decisions preserved (not unified); NO split; zero CS86xx (verified in P6-T6).
- [ ] [P6-T4] Append the three wrapper 500-line pre-existing-violation flags to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/other/maintainer-flags.<yyyy-MM-ddTHH-mm>.md`
  - Acceptance: artifact records, with `Timestamp:`, that `WrapperScoDictionary.cs` (~645 lines), `WrapperPeopleScoDictionaryNew.cs` (~607 lines), and `WrapperScDictionary.cs` (~520 lines) exceed the 500-line limit as a PRE-EXISTING condition (same handling as `PrettyPrint.cs` in sibling #364), flagged NOT fixed — annotation-only cannot bring them under 500 and splitting is out of scope; the files are not split.
- [ ] [P6-T5] Run CSharpier over the Batch 6 files (`NewtonsoftHelpers/WrapperScoDictionary.cs`, `NewtonsoftHelpers/WrapperScDictionary.cs`, `NewtonsoftHelpers/WrapperPeopleScoDictionaryNew.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P6-T6] Run the pragma-only nullable build and record Batch 6 verification to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/qa-gates/batch6-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 3 opted-in Batch 6 files and NO new diagnostics elsewhere.
- [ ] [P6-T7] Run the Batch 6 UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/regression-testing/batch6-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~WrapperScoDictionary|FullyQualifiedName~WrapperScDictionary|FullyQualifiedName~WrapperPeopleScoDictionaryNew"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; all Batch 6 tests green and behavior-identical.

### Phase 7 — Batch 7 Dictionary Converters

- [ ] [P7-T1] Confirm which `PeopleScoConverter` is the live/registered type (in-scope `UtilitiesCS/NewtonsoftHelpers/PeopleScoConverter.cs` vs out-of-scope `ToDoModel/Data Model/People/PeopleScoConverter.cs`, both under namespace `ToDoModel.Data_Model.People`) before annotating, and append the finding to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/other/maintainer-flags.<yyyy-MM-ddTHH-mm>.md`
  - Acceptance: artifact records, with `Timestamp:`, the registration/liveness evidence (which copy is referenced by the serializer/`[JsonConverter]` path) and confirms that ONLY the in-scope `NewtonsoftHelpers/` copy is annotated by this feature; the out-of-scope `ToDoModel/` copy is left unchanged.
- [ ] [P7-T2] Add `#nullable enable` to `UtilitiesCS/NewtonsoftHelpers/ScDictionaryConverter.cs` and match the generic `JsonConverter<TDerived>` framework signature (`existingValue` -> `TDerived?`, `WriteJson` `value` -> `TDerived?`, `ReadJson` return `TDerived?` for the `wrapper?.ToDerived()` body; `where TDerived : ScDictionary<...>` makes `TDerived?` valid) to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; the `ReadJson` return `TDerived?` is recorded as a registered cross-module contract; non-null positions preserved; zero CS86xx (verified in P7-T6).
- [ ] [P7-T3] Add `#nullable enable` to `UtilitiesCS/NewtonsoftHelpers/ScoDictionaryConverter.cs` and match BOTH the generic `JsonConverter<TDerived>` (`existingValue`/`value`/`ReadJson` return -> `TDerived?`) and the inner non-generic `JsonConverter` (`existingValue`/`value`/`ReadJson` return -> `object?`; behavior-preserving `value!`/guard on the `WriteJson` `value.GetType()` deref) framework signatures to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; both the generic and inner non-generic override surfaces match the framework and are recorded as registered cross-module contracts; zero CS86xx (verified in P7-T6).
- [ ] [P7-T4] Add `#nullable enable` to the in-scope `UtilitiesCS/NewtonsoftHelpers/PeopleScoConverter.cs` (only, per P7-T1) and match the `JsonConverter<PeopleScoDictionaryNew>` framework signature (`existingValue`/`value` -> `PeopleScoDictionaryNew?`, `ReadJson` return `PeopleScoDictionaryNew?` for the `wrapper?.ToDerived()` body) to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; the `ReadJson` return `PeopleScoDictionaryNew?` finalized against the P7-T1 confirmation; the out-of-scope `ToDoModel/` copy untouched; zero CS86xx (verified in P7-T6).
- [ ] [P7-T5] Run CSharpier over the Batch 7 files (`NewtonsoftHelpers/ScDictionaryConverter.cs`, `NewtonsoftHelpers/ScoDictionaryConverter.cs`, `NewtonsoftHelpers/PeopleScoConverter.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P7-T6] Run the pragma-only nullable build and record Batch 7 verification to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/qa-gates/batch7-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 3 opted-in Batch 7 files and NO new diagnostics elsewhere.
- [ ] [P7-T7] Run the Batch 7 UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/regression-testing/batch7-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~ScDictionaryConverter|FullyQualifiedName~ScoDictionaryConverter|FullyQualifiedName~PeopleScoConverter"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; all Batch 7 tests green and behavior-identical.

### Phase 8 — Batch 8 High-Contract Finish

- [ ] [P8-T1] Add `#nullable enable` to `UtilitiesCS/NewtonsoftHelpers/FilePathHelperConverter.cs` and match the `JsonConverter<FilePathHelper>` framework signature while settling its cross-module contracts — `WriteJson` `value` -> `FilePathHelper?` with behavior-preserving `value!`/guard on the `FolderPath`/`FileName` derefs, `TryGetValue(..., out string?)`, `reader.Value as string` (`string?`), `ExtractFolderPath(...)` return `string?`, the `is JsonTextReader textReader` pattern tightening on `GetErrorMessage`, and keep the `FilePathHelperConverter(IFileSystemFolderPaths)` constructor parameter non-null — to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; the `WriteJson`/`ReadJson` and `ExtractFolderPath` nullability reflect actual runtime behavior; the FilePathHelper serialization contract remains behavior-compatible; the non-null ctor parameter preserved; zero CS86xx (verified in P8-T3).
- [ ] [P8-T2] Run CSharpier over the Batch 8 file (`NewtonsoftHelpers/FilePathHelperConverter.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched file.
- [ ] [P8-T3] Run the pragma-only nullable build and record Batch 8 verification to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/qa-gates/batch8-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the opted-in Batch 8 file and NO new diagnostics elsewhere.
- [ ] [P8-T4] Run the Batch 8 UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/regression-testing/batch8-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~FilePathHelperConverter"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; all Batch 8 tests green and behavior-identical, including the FilePathHelper Newtonsoft converter round-trip tests.

### Phase 9 — Final QC Loop, Coverage Delta, and Acceptance Verification

- [ ] [P9-T1] Run the CSharpier format gate over the repository and record it to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/qa-gates/final-csharpier.<yyyy-MM-ddTHH-mm>.md`
  - Command: `dotnet tool run csharpier --check .`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with a clean (exit 0) result. If CSharpier changes files, restart the toolchain loop from this task.
- [ ] [P9-T2] Run the analyzer/codestyle build gate and record it to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/qa-gates/final-analyzer-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with a clean build and no new analyzer diagnostics vs the P0-T3 baseline. If this step changes files, restart from P9-T1.
- [ ] [P9-T3] Run the pragma-only nullable/TreatWarningsAsErrors type-check gate and record it to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/qa-gates/final-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (NO `/p:Nullable=enable`)
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across all opted-in `NewtonsoftHelpers/` files and NO new diagnostics elsewhere; records that `/p:Nullable=enable` was NOT passed. If this step changes files, restart from P9-T1.
- [ ] [P9-T4] Run the coverage-enabled test gate over the UtilitiesCS test assemblies and record numeric post-change coverage to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/qa-gates/final-coverage.<yyyy-MM-ddTHH-mm>.md` with the Cobertura XML at `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/qa-gates/final-coverage.<yyyy-MM-ddTHH-mm>.cobertura.xml`
  - Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/qa-gates/final-coverage.<yyyy-MM-ddTHH-mm>.cobertura.xml`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with NUMERIC post-change overall `line-rate`/`branch-rate` and the `UtilitiesCS/NewtonsoftHelpers/` targeted percentage if obtainable, plus passed/failed test counts (all UtilitiesCS tests green, including the `Threading/AppGlobalsConverterTests.cs`, `HelperClasses/NLogTraceWriter_Test.cs`, and ToDoModel People tests). If this step changes files, restart from P9-T1.
- [ ] [P9-T5] Verify `UtilitiesCS.csproj` introduces no project-level or solution-level `<Nullable>` element and record the check to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/qa-gates/csproj-no-nullable.<yyyy-MM-ddTHH-mm>.md`
  - Command: grep `UtilitiesCS/UtilitiesCS.csproj` (and `TaskMaster.sln`) for `<Nullable>`.
  - Acceptance: artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming no `<Nullable>` element exists (DoD item satisfied).
- [ ] [P9-T6] Compute the coverage delta and changed-line no-regression check and record it to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/qa-gates/coverage-delta.<yyyy-MM-ddTHH-mm>.md`
  - Inputs: baseline Cobertura from P0-T5 and post-change Cobertura from P9-T4.
  - Acceptance: artifact records baseline coverage (numeric), post-change coverage (numeric), and changed-line coverage, and confirms NO coverage regression on changed lines; if regression is detected the outcome is remediation-required, not PASS.
- [ ] [P9-T7] Map each acceptance-criteria checkbox in BOTH `spec.md` `## Definition of Done` AND `user-story.md` `## Acceptance Criteria` to its satisfying phase/task per the `acceptance-criteria-tracking` skill (full-feature mode) and record it to `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/qa-gates/ac-checkoff.<yyyy-MM-ddTHH-mm>.md`
  - Acceptance: artifact contains `Timestamp:` and, for `spec.md` `## Definition of Done`, a row per DoD item — per-file pragma/zero-CS86xx (Phases 1-8 + P9-T3), no `<Nullable>` element (P9-T5), annotation/null-safety only (Phases 1-8 + P9-T1/T2), framework-override signatures MATCHED (P3-T1/T2, P4-T1/T2/T3, P7-T2/T3/T4, P8-T1), tests pass / no changed-line coverage regression (P9-T4/T6), full toolchain final pass with pragma-only type-check (P9-T1..T4), three wrapper 500-line flags (P6-T4), duplicate PeopleScoConverter confirmed (P7-T1), NLogTraceWriter global namespace unchanged (P3-T2/T3), NonRecursiveConverter pragma normalized (P4-T4) — each mapped to a satisfying task with its evidence path; AND, for `user-story.md` `## Acceptance Criteria`, a separate independent section with one row per each of the 3 Acceptance-Criteria checkboxes mapped to its satisfying task with its evidence path. Both source files must have their checkboxes updated to `[x]` as each item is verified.
