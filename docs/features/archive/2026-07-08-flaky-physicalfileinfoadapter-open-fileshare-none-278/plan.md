# flaky-physicalfileinfoadapter-open-fileshare-none (Plan)

- **Issue:** #278
- **Feature folder:** `docs/features/active/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/`
- **Work Mode:** minor-audit (per `issue.md` metadata block)
- **Owner:** Dan Moisan
- **Status:** Draft

## Scope Statement

Sole requirements source: `docs/features/active/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/issue.md`, `## Acceptance Criteria` section (AC1–AC6, confirmed present at lines 68–75 of that file). No `spec.md`/`user-story.md` exists or is required for this minor-audit fix; their presence in this feature folder would be a fail-closed condition and none is expected.

In-scope files (AC6):
- `UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs`
- `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs`

No other file may be modified.

## Bugfix-Workflow Nuance (documented per CLAUDE.md Bugfix Workflow)

This is a de-flaking (non-determinism) fix. A classic deterministic "failing test first" cannot be produced because the defect is a `FileShare.None` handle-contention race that only manifests under concurrent CI access to `TaskMaster.sln`; it cannot be reliably forced to fail locally without introducing new non-determinism. Phase 0 therefore substitutes a **fail-before exception dossier** (citing the existing CI failure evidence already captured in `issue.md`) for a locally-reproduced red run, per the `evidence-and-timestamp-conventions` fail-before contract. The deliverable this plan verifies is **determinism**: after the fix, the test acquires no `FileShare.None` (or otherwise contended) handle on any shared/real file, verified by a repeated-run task in the final QA phase.

## Evidence Location

All evidence artifacts are written under:
`docs/features/active/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/evidence/<kind>/`

using ISO-8601 timestamps (`yyyy-MM-ddTHH-mm`) in each artifact file name, per `evidence-and-timestamp-conventions`. No evidence is written under any `artifacts/` path.

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read `CLAUDE.md` (repository root) in full and note the four-document policy compliance order it declares.
  - Acceptance: confirmed by inclusion in the file list recorded in `evidence/other/phase0-instructions-read.md` (P0-T5).
- [x] [P0-T2] Read `.claude/rules/general-code-change.md` in full.
  - Acceptance: confirmed by inclusion in the file list recorded in `evidence/other/phase0-instructions-read.md` (P0-T5).
- [x] [P0-T3] Read `.claude/rules/general-unit-test.md` in full.
  - Acceptance: confirmed by inclusion in the file list recorded in `evidence/other/phase0-instructions-read.md` (P0-T5).
- [x] [P0-T4] Read `.claude/rules/csharp.md` in full.
  - Acceptance: confirmed by inclusion in the file list recorded in `evidence/other/phase0-instructions-read.md` (P0-T5).
- [x] [P0-T5] Write `docs/features/active/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/evidence/other/phase0-instructions-read.md` containing `Timestamp:`, `Policy Order:` (CLAUDE.md → general-code-change.md → general-unit-test.md → csharp.md), the explicit list of files read (P0-T1–P0-T4), and one recorded observation: `CLAUDE.md`/`csharp.md` state a repository line-coverage floor of `>= 80%` with `>= 90%` for new/changed code, while `.claude/rules/general-unit-test.md` states `>= 85%` line / `>= 75%` branch uniformly; this plan's coverage tasks (P3-T7) verify against the combined stricter bar (`>= 85%` line, `>= 90%` on changed lines) so both documents are satisfied simultaneously, and this discrepancy is reported to the user rather than silently resolved.
  - Acceptance: file exists with all four required fields populated (no placeholder text).
- [x] [P0-T6] Confirm `docs/features/active/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/issue.md` contains an explicit `## Acceptance Criteria` heading with items AC1–AC6, and confirm no `spec.md` or `user-story.md` file exists in the same folder.
  - Acceptance: both conditions verified true; if either is false, halt and report a fail-closed blocker instead of proceeding to Phase 1.
- [x] [P0-T7] Re-read `UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs` and confirm line 134 currently reads `public FileStream Open(FileMode mode, FileAccess access) => _fileInfo.Open(mode, access);` (unseamed 2-arg overload).
  - Acceptance: line content matches exactly; record the actual line number found (if drifted from 134) in `evidence/baseline/line-anchors-baseline.<timestamp>.md`.
- [x] [P0-T8] Re-read `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs` and confirm line 207 currently reads `using (var openModeRead = adapter.Open(FileMode.Open, FileAccess.Read))` inside `PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo`.
  - Acceptance: line content matches exactly; record the actual line number found (if drifted from 207) in `evidence/baseline/line-anchors-baseline.<timestamp>.md` (same artifact as P0-T7).
- [x] [P0-T9] Run `dotnet tool run csharpier . --check` (or `csharpier . --check` if installed globally) from the repository root and write `docs/features/active/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/evidence/baseline/csharpier-baseline.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (pass/fail and count of unformatted files, if any, in the two in-scope files).
  - Acceptance: artifact exists with all four fields populated with real values (no `UNVERIFIED`).
- [x] [P0-T10] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and write `docs/features/active/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/evidence/baseline/analyzer-baseline.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (warning/error count baseline, noting any pre-existing diagnostics in the two in-scope files).
  - Acceptance: artifact exists with all four fields populated with real values.
- [x] [P0-T11] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and write `docs/features/active/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/evidence/baseline/nullable-baseline.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (pass/fail; explicitly note the known pre-existing vendored SVGControl/UtilitiesSwordfish nullable-diagnostic baseline count so the final pass can be compared for zero new diagnostics on touched files).
  - Acceptance: artifact exists with all four fields populated with real values.
- [x] [P0-T12] Run `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo /EnableCodeCoverage` and write `docs/features/active/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/evidence/baseline/mstest-targeted-baseline.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including the numeric baseline line-coverage percentage for `PhysicalFileInfoAdapter.cs` and a note that this run's pass/fail is inherently non-deterministic (the defect under fix).
  - Acceptance: artifact exists with all four fields populated, including a numeric coverage value (not a placeholder).
- [x] [P0-T13] Create the fail-before exception dossier `docs/features/active/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/evidence/regression-testing/fail-before-exception.<timestamp>.md` containing `Timestamp:`, `WhyFailingRunImpossible:` (contention-timing-dependent race; cannot be forced to fail deterministically without introducing new non-determinism), and an alternative-proof section citing the CI failure already recorded in `issue.md` (job URL `https://github.com/drmoisan/TaskMaster/actions/runs/28914676821/job/85779070610`, the exact `IOException` stack trace at `PhysicalFileInfoAdapter.cs:134` / `PhysicalFileSystemAdapters_Tests.cs:207`, and the PR #272 / issue #270 observation of 4995 passed / 1 failed / 1 skipped with a passing re-run).
  - Acceptance: dossier exists with all required fields populated with real citations (no placeholder text).

---

### Phase 1 — Production Seam Extension for `Open(FileMode, FileAccess)` (AC1)

- [x] [P1-T1] In `UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs`, add a new private readonly field `private readonly Func<FileMode, FileAccess, FileStream> _openByModeAndAccess;` immediately after the existing `_openByMode` field (line 18), and extend the existing seam comment block (lines 12–16) to state that `Open(FileMode, FileAccess)` is now included in the injectable-delegate seam because its default `FileShare.None` behavior can contend with a shared/real file, not because it is a write-mode member.
  - Acceptance: field declared with the exact signature `Func<FileMode, FileAccess, FileStream>`; comment updated; file compiles (verified in Phase 3).
- [x] [P1-T2] In the public constructor `PhysicalFileInfoAdapter(FileInfo fileInfo)` (lines 21–27), add the binding line `_openByModeAndAccess = _fileInfo.Open;` immediately after the existing `_openByMode = _fileInfo.Open;` line, so production behavior is unchanged (defaults to the real `FileInfo.Open(FileMode, FileAccess)` overload).
  - Acceptance: binding line present in the public constructor only; no behavior change to the constructor's existing three assignments.
- [x] [P1-T3] In the internal test-only constructor (lines 29–40), add a new parameter `Func<FileMode, FileAccess, FileStream> openByModeAndAccess` after the existing `openByMode` parameter, and add the null-guarded assignment `_openByModeAndAccess = openByModeAndAccess ?? throw new ArgumentNullException(nameof(openByModeAndAccess));` following the existing `_openByMode` assignment pattern.
  - Acceptance: internal constructor signature has the new parameter in the stated position; null-guard assignment matches the existing style for `_openByMode`/`_openWrite`.
- [x] [P1-T4] Change the method body at (currently) line 134 from `public FileStream Open(FileMode mode, FileAccess access) => _fileInfo.Open(mode, access);` to `public FileStream Open(FileMode mode, FileAccess access) => _openByModeAndAccess(mode, access);`.
  - Acceptance: method delegates through `_openByModeAndAccess` only; no other line in the method changes.

---

### Phase 2 — Test De-Flaking via Seam-Based Verification (AC2, AC3, AC4)

- [x] [P2-T1] In `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs`, inside `PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo`, remove the real-file contended-open block (currently lines 206–210: the `bool openModeReadCanRead;` declaration and the `using (var openModeRead = adapter.Open(FileMode.Open, FileAccess.Read)) { openModeReadCanRead = openModeRead.CanRead; }` block).
  - Acceptance: no call to `adapter.Open(FileMode.Open, FileAccess.Read)` against the real `GetSolutionFile()`-backed `adapter` remains anywhere in the method.
- [x] [P2-T2] Remove the now-orphaned assertion `openModeReadCanRead.Should().BeTrue();` (currently line 287).
  - Acceptance: no reference to `openModeReadCanRead` remains in the file.
- [x] [P2-T3] Add a new sentinel stream declaration alongside the existing sentinels (currently lines 254–265), named `sentinelOpenModeAndAccessStream`, opened read-only against the test assembly DLL using the same pattern already used for `sentinelOpenModeStream`: `using var sentinelOpenModeAndAccessStream = new FileStream(typeof(PhysicalFileSystemAdapters_Tests).Assembly.Location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);`.
  - Acceptance: declaration present, uses `FileShare.ReadWrite` against the test assembly DLL (no temporary/scratch file, satisfying AC3), and is a `using var` local disposed at method scope end.
- [x] [P2-T4] Update the `seamAdapter` construction (currently lines 266–271) to pass a new delegate argument `(mode, access) => sentinelOpenModeAndAccessStream` in the parameter position matching the new internal-constructor parameter added in P1-T3.
  - Acceptance: `seamAdapter` construction compiles against the updated internal constructor signature and supplies exactly four delegate arguments in the correct order.
- [x] [P2-T5] Add a new assertion adjacent to the existing seam assertions (currently lines 291–293): `seamAdapter.Open(FileMode.Open, FileAccess.Read).Should().BeSameAs(sentinelOpenModeAndAccessStream);`.
  - Acceptance: assertion present and exercises the seamed `Open(FileMode, FileAccess)` overload added in Phase 1, satisfying AC2 and AC4 (production line 134's delegation is exercised without any real/shared-file `FileShare.None` handle).
- [x] [P2-T6] Update the existing block comment at (currently) lines 202–206 to correct and clarify the read-path contention assessment: state explicitly that `OpenRead()` and `OpenText()` internally request `FileShare.Read` (not `FileShare.ReadWrite`), that `FileShare.Read` is compatible with any other handle on the file that also permits read-sharing (the default sharing mode used by checkout/build/coverage tooling), and that only the exclusive `FileShare.None` request from the 2-arg `Open` overload conflicted with a concurrently open handle — which is why `OpenRead()`/`OpenText()` are left unseamed and continue to run against the real `TaskMaster.sln` (documenting the AC6 scope-extension assessment directly in the test file).
  - Acceptance: comment text no longer claims `OpenRead()`/`OpenText()` request `FileShare.ReadWrite`; comment explicitly states the FileShare.None-vs-FileShare.Read distinction and the resulting scope decision.
- [x] [P2-T7] Update the write-mode seam block comment (currently lines 236–246) to add one clause noting that the seam now also covers the read-mode `Open(FileMode, FileAccess)` overload, and why: its default `FileShare.None` behavior — not its read/write direction — is what requires the seam.
  - Acceptance: comment mentions `Open(FileMode, FileAccess)` alongside `AppendText`/`Open(mode)`/`OpenWrite` with the `FileShare.None` rationale distinct from the write-mode rationale.

---

### Phase 3 — Final QA Loop (AC5, AC6)

Run the four-step C# toolchain in this exact order. If any step fails or changes files, restart the loop from step 1 (P3-T1) until a single clean pass completes.

- [x] [P3-T1] Run `dotnet tool run csharpier .` from the repository root and write `docs/features/active/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/evidence/qa-gates/csharpier-final.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (must report zero files reformatted; if any file was reformatted, restart the loop from this task).
  - Acceptance: `EXIT_CODE: 0` and `Output Summary:` states zero files changed.
- [x] [P3-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and write `docs/features/active/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/evidence/qa-gates/analyzer-final.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (must report zero new diagnostics on the two in-scope files versus the P0-T10 baseline; if any new diagnostic appears, fix it and restart the loop from P3-T1).
  - Acceptance: `EXIT_CODE: 0` and `Output Summary:` reports zero new diagnostics on the two in-scope files.
- [x] [P3-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and write `docs/features/active/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/evidence/qa-gates/nullable-final.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (must report zero new diagnostics on the two in-scope files versus the P0-T11 baseline; the known pre-existing vendored SVGControl/UtilitiesSwordfish nullable-error baseline is unaffected by this change and is not a new diagnostic; if a new diagnostic appears on a touched file, fix it and restart the loop from P3-T1).
  - Acceptance: `EXIT_CODE: 0` and `Output Summary:` reports zero new diagnostics on the two in-scope files.
- [x] [P3-T4] Run `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo /EnableCodeCoverage` and write `docs/features/active/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/evidence/qa-gates/mstest-targeted-final.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including the numeric post-change line-coverage percentage for `PhysicalFileInfoAdapter.cs` (must show the `Open(FileMode, FileAccess)` delegation line, formerly line 134, as covered).
  - Acceptance: `EXIT_CODE: 0`, test passes, and `Output Summary:` includes a numeric coverage value showing the `Open(FileMode, FileAccess)` line covered.
- [x] [P3-T5] Re-run the same command from P3-T4 four additional times consecutively (five total executions) without any intervening file changes, and write `docs/features/active/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/evidence/qa-gates/determinism-repeat-final.<timestamp>.md` with `Timestamp:`, `Command:`, and `Output Summary:` recording the pass/fail outcome and exit code of each of the five runs.
  - Acceptance: all five runs report `EXIT_CODE: 0` with no `IOException`; this is the empirical evidence that the flakiness (AC2) is resolved.
- [x] [P3-T6] Run `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage` (full `UtilitiesCS.Test` assembly) and write `docs/features/active/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/evidence/qa-gates/mstest-full-final.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including the numeric assembly-wide line-coverage percentage.
  - Acceptance: `EXIT_CODE: 0`, all tests in the assembly pass, and `Output Summary:` includes a numeric assembly-wide coverage value.
- [x] [P3-T7] Compare the P0-T12 baseline coverage value against the P3-T4/P3-T6 post-change coverage values and write `docs/features/active/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/evidence/qa-gates/coverage-delta-final.<timestamp>.md` recording `Baseline coverage:`, `Post-change coverage:`, `Changed-line coverage:` (must be `>= 90%` per CLAUDE.md/csharp.md UT2 new-code threshold), and `Assembly-wide coverage:` (must be `>= 85%` per `.claude/rules/general-unit-test.md`, which is also `>= 80%` per CLAUDE.md/csharp.md), confirming no regression on the changed lines.
  - Acceptance: artifact records all four numeric values and states explicitly that neither threshold is violated; if either is violated, the outcome is remediation-required, not PASS.
- [x] [P3-T8] Run `git diff --stat` against the base branch and write `docs/features/active/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/evidence/qa-gates/scope-diff-final.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` listing every changed file path.
  - Acceptance: the listed changed files are exactly `UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs` and `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs` (AC6); any additional changed file is a blocking finding.
- [x] [P3-T9] Update AC1–AC6 checkboxes to checked in `docs/features/active/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/issue.md` and mirror the update at `docs/features/active/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/evidence/issue-updates/issue-278.<timestamp>.md` per the issue-update mirroring convention (`Timestamp:`, exact text posted, `PostedAs:`).
  - Acceptance: `issue.md` shows all six AC boxes checked; mirror artifact exists with all required fields.

---

## Acceptance Criteria Coverage Map

| AC | Covered by |
|----|------------|
| AC1 | P1-T1, P1-T2, P1-T3, P1-T4 |
| AC2 | P2-T1, P2-T2, P2-T4, P2-T5, P3-T5 |
| AC3 | P2-T3 (test-assembly-DLL sentinel, no temp/scratch file) |
| AC4 | P2-T5, P3-T4, P3-T7 |
| AC5 | P3-T1, P3-T2, P3-T3, P3-T4, P3-T6, P3-T7 |
| AC6 | P0-T6, P2-T6, P2-T7, P3-T8 |
