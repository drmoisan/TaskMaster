# Remediation Plan (Cycle 3) — hierarchical-lcppn-folder-prediction (#177)

- **Cycle:** 3
- **Plan timestamp:** 2026-06-16T01-04 (UTC)
- **Work Mode:** full-feature
- **Base:** `main`
- **Head:** `TaskMaster-wt-2026-06-08-12-06` (`eebcc910`)
- **Spec / requirements source (authoritative):**
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/2026-06-16T01-04-remediation/remediation-inputs.2026-06-16T01-04.md`
- **Acceptance criteria source:**
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/user-story.md` (AC21–AC24 verbatim)
- **Research anchors:**
  `artifacts/research/2026-06-15T00-00-issue-177-lcppn-integration-findings.md`
- **Feature root (`<FEATURE>`):**
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177`
- **Evidence root (canonical, non-overridable):** `<FEATURE>/evidence/<kind>/`

> EVIDENCE_LOCATION_OVERRIDE_REJECTED: none supplied. All evidence in this plan resolves to
> `<FEATURE>/evidence/<kind>/` per `evidence-and-timestamp-conventions`. The research artifact
> path under `artifacts/research/` is a non-evidence read-only input and is permitted.

## Scope (cycle 3 only — do not exceed)

Two in-scope findings, both from the remediation-inputs file:

- **F4 / AC21 + AC22** — production enablement, DEFAULT ON. Source `UseLcppnPredictor` from the
  application's persistent settings so it defaults to ON and is honored by the three production
  callers without hand-editing each call site; centralize at `OlFolderClassifierGroup` config
  resolution. Setting remains toggleable to OFF (AC13 flag-off flat parity preserved). Fallback
  (AC22): setting ON but `Globals.AF.FolderPredictor` null/unbuilt → `GetFolderPredictorAsync`
  returns the flat group without throwing.
- **F5 / AC23** — persistence / load-on-startup. Serialize `LcppnFolderPredictor` to its OWN file
  (distinct from `Folder.json`); rehydrate `Globals.AF.FolderPredictor` at startup via the
  `AppAutoFileObjects` load path; fail-soft (holder stays null → flat fallback) when the file is
  missing/unreadable.

## Invariants (must hold across every phase; verified in Phase 5)

- **INV-1 Containment (AC24):** ZERO diff in `SpamBayes.cs`, `Triage.cs`,
  `CategoryClassifierGroup.cs`, `MulticlassEngine.cs`, and `Manager["Actionable"]` usage.
- **INV-2 ManagerAsyncLazy typing:** `ManagerAsyncLazy` dictionary value typing
  (`AsyncLazy<BayesianClassifierGroup>`) is unchanged.
- **INV-3 Flat rebuild retained:** the always-on flat rebuild + serialize of `Manager["Folder"]` in
  `BuildClassifiersAsync` is NOT retired; the flat group remains the fallback.
- **INV-4 Default-ON via reachable config:** the default value of `UseLcppnPredictor` is sourced
  from persistent settings and is ON; no per-call site hand-sets the flag.
- **INV-5 File-size cap:** no file exceeds 500 lines. `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs`
  (608) and `SortEmail.cs` (1406) are already over cap — this plan must NOT increase their line counts
  (target: do not touch them at all; centralize at `OlFolderClassifierGroup`).
  `BayesianClassifierGroup.cs` (515) must not grow. `TaskMaster/AppGlobals/AppAutoFileObjects.cs` (847)
  is also already over cap and must NOT grow beyond the minimal wiring lines added by this cycle (the
  two load-list/await call sites plus the `partial` keyword); all new deserialize/load logic goes into
  the new file `TaskMaster/AppGlobals/AppAutoFileObjects.FolderPredictorLoad.cs` (must be <= 500 lines;
  see Phase 3). New/changed test files must be <= 500 lines (split if needed).
- **INV-6 Test discipline:** MSTest + Moq + FluentAssertions; no temporary files; in-memory/seam
  serialization pattern; new/changed code >= 90% strict coverage; repo >= 80%.
- **INV-7 AC13 preserved:** flag-off flat parity remains green; toggling the setting OFF restores
  byte-for-byte flat behavior.

---

### Phase 0 — Baseline capture and policy reading

- [x] [P0-T1] Read policy files in required order and record evidence. Read, in order: `CLAUDE.md`;
  `.claude/rules/general-code-change.md`; `.claude/rules/general-unit-test.md`;
  `.claude/rules/csharp.md`; `.claude/rules/ci-workflows.md`; `.claude/rules/tonality.md`. Write
  `<FEATURE>/evidence/baseline/phase0-instructions-read.2026-06-16T01-04.md` containing `Timestamp:`,
  `Policy Order:`, and the explicit list of files read. Acceptance: artifact exists with all three
  fields populated and the six files listed.
- [x] [P0-T2] Capture line counts of every file in scope. Record current line counts for:
  `UtilitiesCS/EmailIntelligence/Bayesian/LcppnFolderPredictorConfig.cs`;
  `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs`;
  `UtilitiesCS/EmailIntelligence/Bayesian/LcppnFolderPredictor.cs`;
  `TaskMaster/AppGlobals/AppAutoFileObjects.cs` (expect 847; already over cap);
  `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs`;
  `UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs`;
  `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs` (expect 608);
  `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` (expect 1406);
  `UtilitiesCS/EmailIntelligence/Bayesian/BayesianClassifierGroup.cs` (expect 515); and the three
  existing LCPPN test files plus `UtilitiesCS.Test/EmailIntelligence/FolderPredictorSeam_Tests.cs`.
  Write `<FEATURE>/evidence/baseline/file-line-counts.2026-06-16T01-04.md` with `Timestamp:`,
  `Command:`, `EXIT_CODE:`, `Output Summary:` (the per-file counts). Acceptance: artifact lists every
  file with its current line count.
- [x] [P0-T3] Capture baseline CSharpier formatting state. Run `dotnet tool run csharpier --check .`
  (or `csharpier --check .`). Write
  `<FEATURE>/evidence/baseline/csharpier-baseline.2026-06-16T01-04.md` with `Timestamp:`,
  `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact records the exit code and whether
  any files need formatting.
- [x] [P0-T4] Capture baseline analyzer build. Run
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  Write `<FEATURE>/evidence/baseline/analyzer-baseline.2026-06-16T01-04.md` with `Timestamp:`,
  `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail + warning/error counts). Acceptance: artifact
  records the build result.
- [x] [P0-T5] Capture baseline nullable / TreatWarningsAsErrors build. Run
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
  Write `<FEATURE>/evidence/baseline/nullable-baseline.2026-06-16T01-04.md` with `Timestamp:`,
  `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact records the build result.
- [x] [P0-T6] Capture baseline test run with coverage (numeric). Run
  `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` for the affected test assemblies
  (at minimum `UtilitiesCS.Test` and any `TaskMaster`-side test assembly that covers
  `AppAutoFileObjects`). Write
  `<FEATURE>/evidence/baseline/test-coverage-baseline.2026-06-16T01-04.md` with `Timestamp:`,
  `Command:`, `EXIT_CODE:`, and an `Output Summary:` that includes the numeric baseline repo-wide
  line-coverage percentage and the current coverage of the in-scope files
  (`OlFolderClassifierGroup.cs`, `LcppnFolderPredictorConfig.cs`, `LcppnFolderPredictor.cs`,
  `AppAutoFileObjects.cs`). Acceptance: artifact records the pass count and numeric coverage
  headline values (no placeholders).
- [x] [P0-T7] Record the AC13 regression baseline. Identify and run the existing AC13 flag-off
  parity test(s) in `UtilitiesCS.Test/EmailIntelligence/FolderPredictorSeam_Tests.cs` (e.g.
  `GetFolderPredictorAsync_FlagOff_*`) and confirm green at baseline. Write
  `<FEATURE>/evidence/baseline/ac13-baseline.2026-06-16T01-04.md` with `Timestamp:`, `Command:`,
  `EXIT_CODE:`, `Output Summary:` (named tests + pass/fail). Acceptance: artifact shows the AC13
  tests passing at baseline.

---

### Phase 1 — Default-ON config sourced from persistent settings (F4 / AC21)

Touches: `TaskMaster/Properties/Settings.settings` (+ generated `Settings.Designer.cs`),
`UtilitiesCS/EmailIntelligence/Bayesian/LcppnFolderPredictorConfig.cs`,
`UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs`.
Does NOT touch `EmailFiler.cs`, `SortEmail.cs`, or
`UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs` (centralization avoids the over-cap files per
INV-5).

- [x] [P1-T1] Add a persistent toggle to application settings, default ON. In
  `TaskMaster/Properties/Settings.settings` add a user/application setting `UseLcppnPredictor`
  (type `bool`) with default value `True`. Regenerate `Settings.Designer.cs` accordingly.
  Acceptance: `Properties.Settings.Default.UseLcppnPredictor` exists, is typed `bool`, and resolves
  to `true` by default. Verification: a test in Phase 4 reads the setting default as ON.
- [x] [P1-T2] Introduce a single config-resolution seam on `OlFolderClassifierGroup` that sources
  the default from settings. In
  `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs`, replace the
  hard-coded `FolderPredictorConfig` initializer at lines 40-41 (`new LcppnFolderPredictorConfig()`)
  with a resolution path that reads the persisted default through the existing globals/settings
  mechanism (do not call `Properties.Settings.Default` directly inside `UtilitiesCS`; route the
  bool through `IApplicationGlobals`/`AF` so it remains mockable). Provide an injectable seam (a
  `virtual` property or a settable `FolderPredictorConfig`) so tests can supply a config without a
  live settings store. Acceptance: a default-constructed production-style `OlFolderClassifierGroup`
  over globals whose persisted setting is ON yields `FolderPredictorConfig.UseLcppnPredictor == true`
  without any caller hand-setting the flag. Verification: Phase 4 default-ON selection test.
- [x] [P1-T3] Expose the persisted toggle on the application-globals boundary. Add a `bool
  UseLcppnPredictor` accessor on the `IAppAutoFileObjects` interface (reached from `UtilitiesCS` via
  `IApplicationGlobals.AF`), alongside the existing `FolderPredictor` holder declared at
  `IAppAutoFileObjects.cs:45`. Do NOT place the accessor on `IApplicationGlobals` directly; it belongs
  on `IAppAutoFileObjects`. Back it in the TaskMaster `AppAutoFileObjects` implementation by
  `Properties.Settings.Default.UseLcppnPredictor`. `UtilitiesCS` must NOT reference
  `TaskMaster.Properties.Settings` (the settings access stays in the TaskMaster implementation; only
  the `bool` flows across the interface boundary). Acceptance: the new accessor compiles, is declared
  on `IAppAutoFileObjects` so it is mockable with Moq, the TaskMaster implementation returns the
  persisted setting, and `UtilitiesCS` contains no reference to `TaskMaster.Properties.Settings`.
  Verification: Phase 4 tests mock this accessor to drive ON and OFF paths.
- [x] [P1-T4] Confirm `LcppnFolderPredictorConfig` default is no longer the source of truth for
  production enablement. In `UtilitiesCS/EmailIntelligence/Bayesian/LcppnFolderPredictorConfig.cs`,
  keep `UseLcppnPredictor` as a plain serializable property (do not flip the class-level default to
  true, because the production default now comes from settings via P1-T2/P1-T3; flipping the class
  default would mask OFF in tests that construct the config directly). Add an XML-doc note that the
  production default is resolved from persistent settings at `OlFolderClassifierGroup` construction.
  Acceptance: the file change is documentation/comment only on the default semantics; `Create(...)`
  retains `useLcppnPredictor = false` as its explicit-parameter default so existing AC13 tests
  remain valid. Verification: build + existing AC13 tests still pass.
- [x] [P1-T5] Verify file-size cap for Phase 1 touched files. Confirm
  `OlFolderClassifierGroup.cs`, `LcppnFolderPredictorConfig.cs`, and the globals interface file each
  remain <= 500 lines after the edits; confirm `EmailFiler.cs`, `SortEmail.cs`,
  `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs` are byte-for-byte unchanged. Write
  `<FEATURE>/evidence/qa-gates/phase1-filesize.2026-06-16T01-04.md` with `Timestamp:`, `Command:`,
  `EXIT_CODE:`, `Output Summary:` (post-edit counts + unchanged-files confirmation). Acceptance: all
  edited files <= 500 lines and the three over-cap callers are untouched.

---

### Phase 2 — Safe fallback confirmation under default-ON (F4 / AC22)

Touches: `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs`
(confirm only; the fallback branch at lines 82-87 already exists).

- [x] [P2-T1] Confirm the fallback branch in `GetFolderPredictorAsync`. In
  `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs` lines 80-91,
  confirm that when `FolderPredictorConfig?.UseLcppnPredictor == true` but
  `Globals.AF.FolderPredictor is null`, the method returns `await Globals.AF.Manager["Folder"]`
  (the flat group) and does not throw. Make no behavior change unless the branch is missing; if a
  guard is required to avoid a null dereference under the new default-ON path, add the minimal guard.
  Acceptance: the accessor returns the flat group (never null, never throws) when the holder is null
  under default-ON config. Verification: Phase 4 fallback regression test.

---

### Phase 3 — Persistence to own file + load-on-startup rehydration (F5 / AC23)

Touches: `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs`
(build/serialize path), `TaskMaster/AppGlobals/AppAutoFileObjects.cs` (minimal load-list/await wiring
+ `partial` keyword only — this file is already 847 lines, over the 500 cap, and must NOT grow beyond
the wiring lines), and a NEW file `TaskMaster/AppGlobals/AppAutoFileObjects.FolderPredictorLoad.cs`
(holds the `LoadFolderPredictorAsync()` deserialize + fail-soft logic; must be <= 500 lines). May add
one new small production file for an LCPPN serialization-config/file-name helper if needed to keep
`OlFolderClassifierGroup.cs` under cap.

- [x] [P3-T1] Define the dedicated LCPPN serialization file name/path, distinct from `Folder.json`.
  Establish the file name (e.g. `LcppnFolder.json`) and folder (the same `AppData/Bayesian`
  location used by `BuildClassifiersAsync` at `OlFolderClassifierGroup.cs:227`). Encapsulate this in
  a single internal helper (a method on `OlFolderClassifierGroup` or a new small file
  `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/LcppnFolderPredictorStore.cs`) that
  builds a `NewSmartSerializableConfig`/file path for `LcppnFolderPredictor`. Acceptance: the file
  name is a single named constant distinct from `Folder.json`; the helper compiles and is unit-test
  reachable. Verification: Phase 4 round-trip test asserts the configured file name is the dedicated
  name, not `Folder.json`.
- [x] [P3-T2] Configure `SmartSerializable.Config` and serialize the LCPPN predictor on the build
  path. In `OlFolderClassifierGroup.cs` the current flag-on build block is the single in-line
  assignment at lines 279-282: `Globals.AF.FolderPredictor = await BuildLcppnPredictorAsync(collection);`.
  Rewrite that statement into the following exact sequence (replacing the single assignment): (1)
  introduce a local `LcppnFolderPredictor predictor = await BuildLcppnPredictorAsync(collection);`;
  (2) set `predictor.Config` to the dedicated-file `SmartSerializable<T>.Config` built in P3-T1 (file
  name + folder, distinct from `Folder.json`); (3) call `predictor.Serialize();`; (4) assign
  `Globals.AF.FolderPredictor = predictor;`. (`SmartSerializable<T>.Config` and
  `SmartSerializable<T>.Serialize()` are confirmed present on `LcppnFolderPredictor`.) Do NOT alter the
  flat `Manager["Folder"]` rebuild + serialize above it (INV-3). Acceptance: with the flag on,
  `BuildClassifiersAsync` builds the predictor into a local, sets its dedicated `Config`, calls
  `Serialize()`, then assigns the holder; the flat `Folder.json` write is unchanged. Verification:
  Phase 4 serialize-path test asserts `Config` is set with the dedicated file name and `Serialize()`
  is invoked.
- [x] [P3-T3] Make `AppAutoFileObjects` partial and place the load logic in a new file (file-size cap
  compliance). `TaskMaster/AppGlobals/AppAutoFileObjects.cs` is already 847 lines (over the 500 cap)
  and is NOT currently declared `partial`; adding the load method body in place would grow it further
  and is therefore prohibited. Do two things: (a) add the `partial` keyword to the `AppAutoFileObjects`
  class declaration in `AppAutoFileObjects.cs` (this is the only content change permitted in that file
  besides the wiring lines in P3-T4); and (b) create a NEW file
  `TaskMaster/AppGlobals/AppAutoFileObjects.FolderPredictorLoad.cs` containing `partial class
  AppAutoFileObjects` with the `LoadFolderPredictorAsync()` method that, when the persisted setting is
  ON, deserializes the dedicated LCPPN file (via `LcppnFolderPredictor.Static.DeserializeAsync` /
  `SmartSerializable` with the config from P3-T1) and assigns the result to `FolderPredictor` (the
  existing holder property at line 617). The fail-soft-on-missing-file logic (P3-T5) lives in this new
  file as well. Acceptance: `AppAutoFileObjects` compiles as a `partial class`; the new file
  `AppAutoFileObjects.FolderPredictorLoad.cs` contains `LoadFolderPredictorAsync()` and is <= 500
  lines; `AppAutoFileObjects.cs` is unchanged except for the added `partial` keyword. Verification:
  build succeeds and the new file's line count is recorded in P3-T6.
- [x] [P3-T4] Wire the rehydration step into the application load path (minimal in-place edit only).
  In `TaskMaster/AppGlobals/AppAutoFileObjects.cs`, add ONLY the minimal wiring lines that invoke
  `LoadFolderPredictorAsync()`: add it to the `tasks` list in `LoadParallelAsync` (lines 69-78) and add
  the corresponding `await` call in `LoadSequentialAsync` (lines 82-94), consistent with the
  surrounding load steps. No other logic may be added to `AppAutoFileObjects.cs` (the method body lives
  in `AppAutoFileObjects.FolderPredictorLoad.cs` per P3-T3). Acceptance: after `LoadAsync`, given a
  present dedicated LCPPN file and the setting ON, `Globals.AF.FolderPredictor` is non-null and is an
  `LcppnFolderPredictor`; the only edits to `AppAutoFileObjects.cs` are the two wiring call sites plus
  the `partial` keyword from P3-T3. Verification: Phase 4 load-path test.
- [x] [P3-T5] Make load fail-soft on missing/unreadable file. In the new `LoadFolderPredictorAsync`
  (in `TaskMaster/AppGlobals/AppAutoFileObjects.FolderPredictorLoad.cs` per P3-T3), wrap
  deserialization so that a missing file or a read failure leaves `FolderPredictor` null (no throw)
  and logs through the established log4net pattern (`logger.Error`/`logger.Warn`). Distinguish
  fail-soft (absent/unreadable → null + log) from fail-fast (genuine corruption surfaced through
  logging). Acceptance: when the dedicated file is absent or unreadable, `LoadAsync` completes without
  throwing and `FolderPredictor` remains null, so `GetFolderPredictorAsync` falls back to flat (AC22).
  Verification: Phase 4 missing-file fail-soft test.
- [x] [P3-T6] Verify INV-3 (flat rebuild retained) and file-size cap for Phase 3. Confirm the
  always-on flat `Manager["Folder"]` rebuild+serialize in `BuildClassifiersAsync` is intact and
  unconditional; confirm `OlFolderClassifierGroup.cs`, the new
  `TaskMaster/AppGlobals/AppAutoFileObjects.FolderPredictorLoad.cs`, and any new helper file are each
  <= 500 lines; confirm `TaskMaster/AppGlobals/AppAutoFileObjects.cs` grew only by the minimal wiring
  lines (the two load-list/await call sites) plus the `partial` keyword relative to its P0-T2 baseline
  (847) and was not otherwise expanded. Write
  `<FEATURE>/evidence/qa-gates/phase3-filesize-and-flat-retained.2026-06-16T01-04.md` with
  `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: flat rebuild unchanged; the
  new `AppAutoFileObjects.FolderPredictorLoad.cs` is <= 500 lines; `AppAutoFileObjects.cs` grows only
  by the wiring lines + `partial` keyword relative to the P0-T2 baseline; all other touched/new files
  <= 500 lines.

---

### Phase 4 — Tests for AC21 / AC22 / AC23 and AC13 regression

Touches: test files under `UtilitiesCS.Test/EmailIntelligence/` and any TaskMaster-side test
assembly covering `AppAutoFileObjects`. All tests MSTest + Moq + FluentAssertions, Arrange-Act-Assert,
no temporary files, in-memory/seam serialization. Split any test file that would exceed 500 lines.

- [x] [P4-T1] AC21 default-ON selection test (no explicit flag). Add a test asserting that a
  production-style `OlFolderClassifierGroup` constructed over mocked globals whose persisted
  `UseLcppnPredictor` accessor returns `true` (the default) yields `FolderPredictorConfig.UseLcppnPredictor == true`
  and, with a held LCPPN predictor, `GetFolderPredictorAsync()` returns the `LcppnFolderPredictor` —
  without any caller hand-setting the flag. Place in
  `UtilitiesCS.Test/EmailIntelligence/FolderPredictorSeam_Tests.cs` (or a new sibling file if the
  500-line cap would be exceeded). Acceptance: test fails against pre-Phase-1 code and passes after
  Phase 1; named clearly for AC21.
- [x] [P4-T2] AC21 toggle-OFF parity test. Add a test asserting that when the mocked persisted
  setting returns `false`, the production-style construction yields
  `FolderPredictorConfig.UseLcppnPredictor == false` and `GetFolderPredictorAsync()` returns the
  flat `BayesianClassifierGroup` (same instance), preserving AC13 flag-off behavior. Acceptance:
  test passes and demonstrates OFF restores flat-only selection.
- [x] [P4-T3] AC22 fallback regression test under default-ON. Add a test asserting that with the
  setting ON (default) but `Globals.AF.FolderPredictor` null, `GetFolderPredictorAsync()` returns
  the flat group and does not throw. Acceptance: test passes; clearly named for AC22. (This may
  extend the existing `GetFolderPredictorAsync_FlagOnButNoHeldPredictor_FallsBackToFlat` to the
  default-ON config path.)
- [x] [P4-T4] AC23 serialize-to-own-file round-trip test. Add a test that configures an
  `LcppnFolderPredictor` with the dedicated file name/path (P3-T1), serializes it via the
  `SmartSerializable` in-memory/seam pattern (`SerializeToString()` / `DeserializeObject()`), and
  round-trips losslessly; assert the configured file name is the dedicated LCPPN name and is NOT
  `Folder.json`. No temporary files. Place under
  `UtilitiesCS.Test/EmailIntelligence/Bayesian/` (extend or split
  `LcppnFolderPredictor_Serialization_Tests.cs`, keeping each file <= 500 lines). Acceptance: test
  passes and asserts the distinct file name.
- [x] [P4-T5] AC23 load-path rehydration test. Add a test (TaskMaster-side test assembly covering
  `AppAutoFileObjects`, or a seam-based test of `LoadFolderPredictorAsync`) asserting that given a
  persisted dedicated LCPPN file and the setting ON, the load path populates
  `Globals.AF.FolderPredictor` with an `LcppnFolderPredictor`. Use mocked filesystem/serialization
  seams; no temporary files. Acceptance: test passes; clearly named for AC23.
- [x] [P4-T6] AC23 missing-file fail-soft test. Add a negative test asserting that when the
  dedicated LCPPN file is absent or unreadable, the load path completes without throwing and leaves
  `Globals.AF.FolderPredictor` null, and that a subsequent `GetFolderPredictorAsync()` falls back to
  flat. Acceptance: test passes; clearly named for AC23/AC22 fail-soft.
- [x] [P4-T7] AC13 regression re-verification test presence. Confirm the existing AC13 flag-off
  parity tests in `FolderPredictorSeam_Tests.cs` remain present and unmodified in intent; if Phase 1
  changed construction, update only the arrangement (mocked OFF setting) without weakening
  assertions. Acceptance: AC13 tests still assert byte-for-byte flat behavior and pass.
- [x] [P4-T8] Verify test-file size cap. Confirm every new or modified test file is <= 500 lines;
  split as needed. Write
  `<FEATURE>/evidence/qa-gates/phase4-test-filesize.2026-06-16T01-04.md` with `Timestamp:`,
  `Command:`, `EXIT_CODE:`, `Output Summary:` (per-file counts). Acceptance: all test files
  <= 500 lines.

---

### Phase 5 — Final QA loop, coverage delta, and containment verification

Run the full C# toolchain in order until it passes in a single pass. Restart from the first step on
any failure or auto-fix.

- [ ] [P5-T1] Containment diff verification (INV-1, INV-2). Produce a diff of the branch against the
  cycle-3 entry point and confirm ZERO diff in `SpamBayes.cs`, `Triage.cs`,
  `CategoryClassifierGroup.cs`, `MulticlassEngine.cs`, and `Manager["Actionable"]` usage, and that
  `ManagerAsyncLazy` dictionary value typing (`AsyncLazy<BayesianClassifierGroup>`) is unchanged.
  Write `<FEATURE>/evidence/qa-gates/containment-diff.2026-06-16T01-04.md` with `Timestamp:`,
  `Command:`, `EXIT_CODE:`, `Output Summary:` (list of files with any diff vs. the contained set).
  Acceptance: artifact shows zero diff for the contained files and unchanged `ManagerAsyncLazy`
  typing.
- [ ] [P5-T2] Final-QC formatting. Run `dotnet tool run csharpier .` (or `csharpier .`). Write
  `<FEATURE>/evidence/qa-gates/final-csharpier.2026-06-16T01-04.md` with `Timestamp:`, `Command:`,
  `EXIT_CODE:`, `Output Summary:`. Acceptance: formatting clean (no files changed on the final pass);
  if files changed, restart the loop from this task.
- [ ] [P5-T3] Final-QC analyzers. Run
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  Write `<FEATURE>/evidence/qa-gates/final-analyzers.2026-06-16T01-04.md` with `Timestamp:`,
  `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: build passes with no analyzer errors.
- [ ] [P5-T4] Final-QC nullable / TreatWarningsAsErrors. Run
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
  Write `<FEATURE>/evidence/qa-gates/final-nullable.2026-06-16T01-04.md` with `Timestamp:`,
  `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: build passes with warnings-as-errors.
- [ ] [P5-T5] Final-QC tests with coverage (numeric). Run
  `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` for the affected test assemblies.
  Write `<FEATURE>/evidence/qa-gates/final-test-coverage.2026-06-16T01-04.md` with `Timestamp:`,
  `Command:`, `EXIT_CODE:`, and an `Output Summary:` recording the numeric post-change repo-wide line
  coverage and the post-change coverage of each new/changed file
  (`OlFolderClassifierGroup.cs`, `LcppnFolderPredictorConfig.cs`, the new store helper if added,
  `TaskMaster/AppGlobals/AppAutoFileObjects.FolderPredictorLoad.cs` for the new
  `LoadFolderPredictorAsync`). Acceptance: all tests pass; numeric coverage recorded (no
  placeholders).
- [ ] [P5-T6] Coverage delta / threshold verification. Compare baseline (P0-T6) to final (P5-T5) and
  report: baseline repo coverage, post-change repo coverage, and new/changed-code coverage. Write
  `<FEATURE>/evidence/qa-gates/coverage-delta.2026-06-16T01-04.md` with `Timestamp:`,
  `Output Summary:`. Acceptance: repo-wide >= 80%; new/changed lines >= 90% strict; changed-line
  coverage does not regress. If any threshold is unmet, the cycle outcome is remediation-required
  (NOT PASS).
- [ ] [P5-T7] AC13 final regression re-verification. Re-run the AC13 flag-off parity tests and
  confirm green. Write `<FEATURE>/evidence/regression-testing/ac13-final.2026-06-16T01-04.md` with
  `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (named tests + pass). Acceptance: AC13
  tests pass.
- [ ] [P5-T8] Final file-size sweep. Confirm no production, test, or reusable script file added or
  modified in this cycle exceeds 500 lines, and that `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs`
  and `SortEmail.cs` line counts are unchanged from P0-T2. Confirm the new
  `TaskMaster/AppGlobals/AppAutoFileObjects.FolderPredictorLoad.cs` is <= 500 lines, and that
  `TaskMaster/AppGlobals/AppAutoFileObjects.cs` has grown only by the minimal wiring lines (the two
  load-list/await call sites) plus the `partial` keyword relative to its P0-T2 baseline (847). All
  other touched/new files <= 500. Write
  `<FEATURE>/evidence/qa-gates/final-filesize.2026-06-16T01-04.md` with `Timestamp:`, `Command:`,
  `EXIT_CODE:`, `Output Summary:`. Acceptance: the new `AppAutoFileObjects.FolderPredictorLoad.cs` is
  <= 500 lines; `AppAutoFileObjects.cs` grows only by the wiring lines + `partial` keyword relative to
  the P0-T2 baseline; all other touched/new files <= 500; over-cap callers (`FolderScorer.cs`,
  `SortEmail.cs`) unchanged.
- [ ] [P5-T9] Acceptance-criteria check-off summary. Record the status of AC21, AC22, AC23, AC24 and
  the AC13 re-verification, each with a pointer to its proving evidence artifact. Write
  `<FEATURE>/evidence/issue-updates/ac-status.2026-06-16T01-04.md` with `Timestamp:` and the AC →
  evidence mapping. Acceptance: every cycle-3 AC maps to a passing evidence artifact, or the cycle is
  marked remediation-required with the specific gap.

---

## Invariant-to-task traceability

- INV-1 / INV-2 (containment): P5-T1.
- INV-3 (flat rebuild retained): P3-T2, P3-T6, P5-T1.
- INV-4 (default-ON via reachable config): P1-T1, P1-T2, P1-T3, P4-T1.
- INV-5 (file-size cap; over-cap callers untouched): P1-T5, P3-T3, P3-T6, P4-T8, P5-T8.
- INV-6 (test discipline + coverage): P4-T1..T8, P5-T5, P5-T6.
- INV-7 (AC13 preserved): P1-T4, P4-T2, P4-T7, P5-T7.

## AC-to-phase mapping

- AC21 → Phase 1 (P1-T1..T4), Phase 4 (P4-T1, P4-T2).
- AC22 → Phase 2 (P2-T1), Phase 4 (P4-T3, P4-T6).
- AC23 → Phase 3 (P3-T1..T5), Phase 4 (P4-T4, P4-T5, P4-T6).
- AC24 → Phase 5 (P5-T1, P5-T5, P5-T6, P5-T8) + INV traceability above.
- AC13 (re-verify) → P1-T4, P4-T2, P4-T7, P5-T7.
