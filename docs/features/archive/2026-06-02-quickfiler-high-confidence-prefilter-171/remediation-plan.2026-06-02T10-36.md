# Remediation Plan — quickfiler-high-confidence-prefilter (Issue #171)

- **Issue:** #171
- **Date:** 2026-06-02T10-36
- **Work mode:** remediation (resolve findings from feature review)
- **Authoritative spec:** `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/remediation-inputs.2026-06-02T10-36.md`
- **Base:** `development` @ `5e944344041b10becb98c56d358176fc9e7b8ee9`
- **Head:** `ae7eb670ee7738640cab2b41bc7226255224f7ca`
- **Feature folder:** `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/`

## Scope (findings remediated — do not broaden)

This plan addresses exactly three findings and nothing else:

- **R1 (BLOCKING):** Produce the canonical machine-readable C# coverage artifact at `artifacts/csharp/coverage.xml` (Cobertura-style XML with per-line counters), generated from vstest `/EnableCodeCoverage` over the two in-scope test assemblies, so the coverage gate can confirm `QfcHighConfidencePreFilter.cs` line coverage >= 90% and no changed-file regression vs baseline.
- **R2 (SUPPORTING):** From `artifacts/csharp/coverage.xml`, verify and document that changed lines from Issue #171 are covered or are legitimate COM/WinForms boundaries, and record the repo-wide / per-module figure with an explicit pre-existing-condition justification, comparing the six touched files against the baseline.
- **R3 (LOW):** Restore `TaskMaster/TaskMaster.csproj` to its base-branch (`development`) form so the branch diff for that file is minimal and justified, restoring the trailing newline and original multi-line attribute formatting.

## Evidence and Output Location Invariant

- **Coverage artifact (R1):** the machine-readable coverage file is written to `artifacts/csharp/coverage.xml`. `artifacts/csharp/` is a permitted orchestration output path per `csharp-qa-gate`; the file is generated on disk (gitignored, need not be committed) but MUST exist for the re-audit.
- **All other evidence (baselines, verification, QA, regression, coverage analysis):** written ONLY under the feature folder evidence subtree `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/<kind>/` per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.
- Writing remediation evidence to `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, `artifacts/evidence/`, or any other non-canonical evidence path is a policy violation and is rejected. If any caller instruction supplies a non-canonical evidence path, record `EVIDENCE_LOCATION_OVERRIDE_REJECTED: <supplied path> replaced with <canonical path>` and use the canonical path.

## Language and Toolchain

C# / VSTO change verification uses the repo C# toolchain in this exact order, restarting from step 1 if any step fails or rewrites files:

1. **Format:** `dotnet tool run csharpier .` (check mode for verification: `dotnet tool run csharpier check .`)
2. **Analyzers:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. **Nullable type-check:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. **Test + coverage:** `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`

## Standing Constraints (apply to every task — from remediation-inputs "Do Not Do")

- Do NOT re-run or weaken the existing Issue #171 unit tests to inflate coverage numbers.
- Do NOT add live Outlook COM, network I/O, or temporary files to unit tests.
- Do NOT broaden scope beyond Issue #171; do NOT refactor the oversized controllers.
- Do NOT silently delete or relax the `[ExcludeFromCodeCoverage]` boundary on `FolderScoringService`.
- Do NOT modify acceptance-criteria text in `spec.md` / `user-story.md`.
- Do NOT edit policy documents.

## Touched Files (six in-scope files compared in R2)

`QfcHomeController.cs`, `QfcFormController.cs`, `QfcCollectionController.cs`, `QfcItemController.cs`, `QfcItemGroup.cs`, and the new file `QfcHighConfidencePreFilter.cs`. (`FolderScorer.cs` is unchanged by Issue #171 and is reported for completeness only.)

## Finding-to-Task Map

- **R1** → P1-T1, P1-T2, P1-T3, P3-T1
- **R2** → P2-T1, P2-T2, P2-T3, P3-T2
- **R3** → P0-T6, P2-T4 (verification reference), P3-T3, P3-T4

---

### Phase 0 — Context Capture and Baseline

- [x] [P0-T1] Re-read and confirm the repo policy sources before any change: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/tonality.md`, and the C# Code Change / C# Unit Test policy sections in `CLAUDE.md`. Acceptance: write `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/baseline/remediation-policy-read-171.2026-06-02T10-36.md` listing each file read and confirming: CSharpier (not `dotnet format`) for formatting, MSTest+Moq+FluentAssertions for tests, the 4-step toolchain order, the canonical coverage artifact path `artifacts/csharp/coverage.xml`, and the canonical evidence subtree `evidence/<kind>/`. (R1, R2, R3)

- [x] [P0-T2] Re-read the authoritative remediation spec `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/remediation-inputs.2026-06-02T10-36.md` and the existing coverage baseline `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/coverage/coverage-baseline-171.2026-06-02T14-05.txt`. Acceptance: write `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/baseline/remediation-inputs-read-171.2026-06-02T10-36.md` recording the three findings (R1/R2/R3), the baseline per-file figures for the six touched files (QfcHomeController 50.51%, QfcFormController 39.64%, QfcCollectionController 3.81%, QfcItemController 7.02%, QfcItemGroup 53.85%, QfcHighConfidencePreFilter new-file), and the per-module baseline (QuickFiler.dll 24.11%, UtilitiesCS.dll 87.58%). (R1, R2)

- [x] [P0-T3] Capture the current state of the coverage artifact target. Acceptance: write `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/baseline/coverage-artifact-presence-171.2026-06-02T10-36.txt` recording the result of `Test-Path artifacts/csharp/coverage.xml` (expected: False at remediation start, confirming the blocking absence the re-audit flagged). (R1)

- [x] [P0-T4] Capture baseline format state for the branch: run `dotnet tool run csharpier check .` and record exit status and any files reported. Acceptance: write `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/baseline/remediation-csharpier-baseline-171.2026-06-02T10-36.txt` with the exact command and result. This establishes whether CSharpier currently reports `TaskMaster/TaskMaster.csproj` (CSharpier formats only `*.cs`, so the `.csproj` must not appear; record this fact for R3). (R3)

- [x] [P0-T5] Capture the verifier toolchain availability for coverage conversion: confirm presence of one conversion path (`dotnet-coverage` tool OR `Microsoft.CodeCoverage.Console.exe`) and `vstest.console.exe`. Acceptance: write `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/baseline/remediation-tooling-baseline-171.2026-06-02T10-36.txt` recording which coverage-to-Cobertura conversion tool is available and its resolved path/version. (R1)

- [x] [P0-T6] Capture the current branch diff of `TaskMaster/TaskMaster.csproj` versus base. Run `git diff development -- TaskMaster/TaskMaster.csproj`. Acceptance: write `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/baseline/csproj-diff-before-171.2026-06-02T10-36.txt` capturing the full diff (showing the collapsed multi-line attributes and removed trailing newline) as the before-state for the R3 restoration. (R3)

---

### Phase 1 — Produce Canonical C# Coverage Artifact (R1)

- [x] [P1-T1] Build the two in-scope test assemblies and run vstest with coverage. Run, in order: the analyzer build (toolchain step 2) and nullable build (toolchain step 3) to ensure `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` and `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` are current, then `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`. Do NOT modify, skip, or weaken any existing #171 test. Acceptance: vstest produces a `.coverage` results file; write its absolute path and the test pass/fail counts to `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/coverage/remediation-vstest-run-171.2026-06-02T10-36.txt`. (R1)

- [x] [P1-T2] Convert the `.coverage` result from P1-T1 to Cobertura-style XML at the canonical path. Use the conversion tool confirmed in P0-T5, for example `dotnet-coverage merge <path-to-.coverage> -f cobertura -o artifacts/csharp/coverage.xml` (or the equivalent `Microsoft.CodeCoverage.Console.exe` Cobertura export). Acceptance: the file `artifacts/csharp/coverage.xml` exists on disk (verify with `Test-Path artifacts/csharp/coverage.xml` returning True); record the exact conversion command and resulting file size in `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/coverage/remediation-coverage-convert-171.2026-06-02T10-36.txt`. (R1)

- [x] [P1-T3] Verify `artifacts/csharp/coverage.xml` parses as well-formed Cobertura XML with per-line counters. Load it as XML (e.g., `[xml](Get-Content artifacts/csharp/coverage.xml -Raw)`) and confirm the document root is a Cobertura `<coverage>` element containing `<packages>/<package>/<classes>/<class>/<lines>/<line>` elements with `number` and `hits` attributes. Acceptance: write `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/coverage/remediation-coverage-parse-171.2026-06-02T10-36.txt` recording that the XML parsed without error, the count of `<class>` elements, and a confirmation that per-line `hits` counters are present (with at least one example line element for `QfcHighConfidencePreFilter.cs`). (R1)

---

### Phase 2 — Verify Changed-Line and Repo-Wide Coverage From the Artifact (R2)

- [x] [P2-T1] From `artifacts/csharp/coverage.xml`, extract per-file line coverage for the new file `QfcHighConfidencePreFilter.cs` and confirm line coverage of its testable surface is >= 90%. Do NOT relax `[ExcludeFromCodeCoverage]` on `FolderScoringService`; the COM-bound adapter remains excluded. Acceptance: write `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/coverage/remediation-newfile-coverage-171.2026-06-02T10-36.txt` recording the covered/total line counts and computed percentage for `QfcHighConfidencePreFilter.cs` from the canonical artifact, confirming `>= 90%`. (R2)

- [x] [P2-T2] From `artifacts/csharp/coverage.xml`, extract per-file covered/total line counts for the six touched files and compare each against `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/coverage/coverage-baseline-171.2026-06-02T14-05.txt`. For any touched file whose changed lines remain uncovered, classify each uncovered changed line as either covered, or a legitimate COM/WinForms boundary (with the specific reason). Acceptance: write `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/coverage/remediation-changed-line-verification-171.2026-06-02T10-36.md` with a per-file table (file, baseline %, artifact %, delta, changed-line gate result, COM/WinForms justification where applicable) confirming no changed-line coverage regression. (R2)

- [x] [P2-T3] From `artifacts/csharp/coverage.xml`, extract the per-module figures (QuickFiler.dll, UtilitiesCS.dll) and record the repo-wide application-coverage figure with an explicit pre-existing-condition justification. The whole-repo number counting modules not exercised by the two in-scope test assemblies (TaskMaster.dll, ToDoModel.dll, Tags.dll, etc.) is a documented pre-existing condition not introduced by Issue #171. Acceptance: write `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/coverage/remediation-module-coverage-171.2026-06-02T10-36.md` recording per-module covered/total/percent from the artifact, the baseline comparison (QuickFiler.dll vs 24.11%, UtilitiesCS.dll vs 87.58%), and the explicit pre-existing-condition justification for the sub-80% repo-wide figure. (R2)

- [x] [P2-T4] Cross-reference the artifact-derived figures against the existing human-readable coverage evidence `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/coverage/coverage-comparison-171.2026-06-02T10-26.md` to confirm consistency between the previously reported numbers and the now machine-readable artifact. Acceptance: write `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/coverage/remediation-coverage-consistency-171.2026-06-02T10-36.md` confirming the artifact-derived per-file and per-module figures match (or noting and explaining any reconciliation) the prior human-readable comparison. (R2)

---

### Phase 3 — Restore TaskMaster.csproj and Final Verification

- [x] [P3-T1] Restore `TaskMaster/TaskMaster.csproj` to its base-branch (`development`) form, restoring the original multi-line attribute formatting that was incidentally collapsed and the trailing newline. Use `git show development:TaskMaster/TaskMaster.csproj` as the authoritative source for the base-branch content; retain only any change Issue #171 actually requires (none is expected). Acceptance: the working-copy `TaskMaster/TaskMaster.csproj` matches the base-branch content except for any explicitly justified, #171-required change. (R3)

- [x] [P3-T2] Verify the restored `.csproj` diff is minimal. Run `git diff development -- TaskMaster/TaskMaster.csproj`. Acceptance: write `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/qa/csproj-diff-after-171.2026-06-02T10-36.txt` capturing the diff; the result must show no diff (or only an explicitly justified, #171-required change documented in the same file), and confirm the trailing newline is present. (R3)

- [x] [P3-T3] Confirm CSharpier introduces no new error from the `.csproj` restoration. Run `dotnet tool run csharpier check .` and compare against the P0-T4 baseline. CSharpier formats only `*.cs` files and does not touch `.csproj`, so the restoration must not introduce any new CSharpier finding. Acceptance: write `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/qa/remediation-csharpier-after-171.2026-06-02T10-36.txt` recording the command and result, confirming no new CSharpier error versus the P0-T4 baseline. (R3)

- [x] [P3-T4] Run the full C# toolchain in order, restarting from step 1 on any failure or file rewrite: (1) `dotnet tool run csharpier .`; (2) analyzer msbuild; (3) nullable msbuild; (4) `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`. Do NOT weaken any existing #171 test. Acceptance: write `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/qa/remediation-qa-final-171.2026-06-02T10-36.md` recording the four exact commands, format-clean status, analyzer count, nullable count, and test pass/fail counts, confirming zero new analyzer/nullable findings and zero newly failing tests versus the Issue #171 baseline. (R1, R2, R3)

- [x] [P3-T5] Final re-audit confirmation gate. Confirm: (a) `artifacts/csharp/coverage.xml` exists (`Test-Path` True) and parses as Cobertura XML (re-confirm P1-T3); (b) the coverage gate confirms `QfcHighConfidencePreFilter.cs` line coverage >= 90% from the artifact (P2-T1); (c) no changed-file coverage regression versus baseline across the six touched files (P2-T2); (d) the `TaskMaster/TaskMaster.csproj` diff versus `development` is minimal/justified with the trailing newline restored (P3-T2). Acceptance: write `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/qa/remediation-reaudit-confirmation-171.2026-06-02T10-36.md` recording each of the four checks (a)-(d) with its pass result and the evidence path that demonstrates it, mapped back to R1/R2/R3. (R1, R2, R3)

---

## Preflight Note

This plan is provided for validation-only preflight through `atomic-executor` at the same target path (`docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/remediation-plan.2026-06-02T10-36.md`). Revisions update this file in place; no sibling plan files are created. No implementation is executed by the planner.
