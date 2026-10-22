# Policy Audit — narrow-fileio2-retryable-exception-set (Issue #707)

- Reviewed: 2026-09-03T08-32
- Branch: `bug/narrow-fileio2-retryable-exception-set-707`
- HEAD: `1fa9e1dd`
- Diff scope (authoritative): `git diff 67c2e3b0eca90a52e9aee82ccd100acce4722169 HEAD -- ":(exclude).claude"` — 50 files, 693 insertions / 60 deletions
- Work mode: `full-bug` (AC source: `spec.md` `## Acceptance Criteria`, 9 items)

## Rejected Scope Narrowing

None detected. The delegation prompt supplied the correct reconciliation-merge base (`67c2e3b0`) and explicitly warned against the stale `merge-base HEAD main` result (`687f15fb`), rather than attempting to narrow scope. No instruction in the delegation prompt attempted to narrow the audit to a plan/task/phase subset, mark any language "out of scope," or skip a toolchain/coverage check. This section is included to satisfy the Scope Invariant's disclosure requirement, not because a narrowing attempt occurred.

One related note: the plan's own P0-T7 task computed `BASE_SHA` via a bare `git merge-base HEAD main`, which — as anticipated by the delegation prompt — resolved to the stale ancestor `687f15fb` rather than the reconciliation-merge tip `67c2e3b0`. The executor self-detected this discrepancy in `evidence/qa-gates/p7-t2-commit-verification.md`, disclosed it transparently, and independently computed the reconciliation-relative diff (47 paths at that point in the sequence, later 50 after the final evidence commit), confirming the plan's own footprint was exactly the two source files plus the feature folder. This is a self-corrected internal deviation, not a caller-attempted narrowing, and does not require a Rejected Scope Narrowing entry. One downstream AC-verification task (`evidence/qa-gates/p6-t8-ac8-caller-scope.md`, AC8) used the same stale `BASE_SHA` without the discrepancy note; its 360-path diff is a superset of the correct 50-path scope, so its conclusion (neither excluded caller file appears) is verified as unaffected by the staleness (confirmed independently below).

## Evidence Location Compliance

No files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/` appear in the scoped diff (`git diff --name-only 67c2e3b0..HEAD -- ":(exclude).claude" | grep -E "^artifacts/"` returned no output). All evidence for this feature is written under the canonical `docs/features/active/2026-08-31-narrow-fileio2-retryable-exception-set-707/evidence/{baseline,regression-testing,qa-gates}/` tree, matching `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. `scripts/**/validate_evidence_locations.py` was not found in this repository (searched via Glob); this repo does not ship that validator (consistent with prior review findings that several named validators referenced in the shared reviewer scaffolding do not exist in TaskMaster). Manual scan is clean. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` entries required.

**Verdict: PASS.**

## 1. Bugfix Workflow (General Code Change Policy)

- **Failing regression test first**: `evidence/regression-testing/p2-t3-missingdirectory-fail-before.md` records the new test `WriteTextFileAsync_WhenDirectoryDoesNotExist_ShouldReturnFalseWithoutRetrying` failing pre-fix (vstest exit 1; assertion `missingDirectoryFactoryCalls.Should().Be(1)` observed 100). `evidence/regression-testing/p4-t2-fileio2-tests-postfix.md` records it passing post-fix (12/12, exit 0). Both artifacts were verified directly. **PASS.**
- **Minimal, targeted fix**: `git diff` for `FileIO2.cs` shows exactly one new `catch (DirectoryNotFoundException ex)` block (8 lines) inserted ahead of the existing `catch (IOException ex)`; no other line in the file changed. **PASS.**
- **Verify locally before review, full toolchain in order**: format (`p5-t2-format-check.md`, exit 0), analyzer build (`p5-t3-analyzer-build.md`, 0/0), nullable build (`p5-t4-nullable-build.md`, 0/0), test (`p5-t5-utilitiescs-coverage.md`, 4769/4786 passed, 17 pre-existing unrelated failures — see feature-audit AC9 for disposition). All four evidence artifacts were read directly and match the reported commands/exit codes. **PASS.**

## 2. C# Code Change Policy (CLAUDE.md / `.claude/rules/csharp.md` where applicable)

| Gate | Command (verified against evidence) | Result | Verdict |
|---|---|---|---|
| Formatting (CSharpier) | `dotnet tool run csharpier check .` (`p5-t2-format-check.md`) | Checked 1576 files, exit 0, no drift | PASS |
| Analyzer build | `msbuild ... /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (`p5-t3-analyzer-build.md`) | 0 Warnings, 0 Errors | PASS |
| Nullable build | `msbuild ... /t:Rebuild ... /p:TreatWarningsAsErrors=true` (`p5-t4-nullable-build.md`) | 0 Warnings, 0 Errors | PASS |
| Test (MSTest + Moq/FluentAssertions seam) | `dotnet-coverage collect <vstest> ...` substitution (see §3 below) | 4769/4786 passed; 12/12 FileIO2_Tests passed | PASS (with disclosed pre-existing-failure caveat, see feature-audit AC9) |

Catch-clause ordering compiles: `catch (DirectoryNotFoundException ex)` at line 126 precedes `catch (IOException ex)` at line 134 in the built method (`UtilitiesCS/To Depricate/FileIO2.cs`, read directly via `git show HEAD:...`). `DirectoryNotFoundException : IOException`, so C# requires the more-derived clause first (CS0160 otherwise); the analyzer/nullable rebuild both succeeded at exit 0, which would not be possible if the ordering were reversed. Independently confirmed by direct read of the compiled method body, not merely trusted from evidence prose.

No `.csproj`, `.editorconfig`, or `AssemblyInfo.cs` file appears in the scoped diff (`git diff --stat` filtered on those patterns returned no output). Both changed production/test files (301 and 373 lines respectively) are well under the 500-line file-size limit.

## 3. Coverage Verification

**Toolchain substitution (issue #752 workaround).** `scripts/vscode/Invoke-MSTestWithCoverage.ps1` excludes any assembly path containing a `.claude` segment; this worktree is rooted under `.claude/worktrees/`, so the literal wrapper-script command fails with "No test assemblies found." The executor substituted a direct `dotnet-coverage collect <vstest.console.exe> <dll> /InIsolation ... --output-format cobertura` invocation, documented identically in `evidence/baseline/p0-t17-utilitiescs-coverage.md` (baseline) and `evidence/qa-gates/p5-t5-utilitiescs-coverage.md` (post-change). Both artifacts state the substitution explicitly, cite issue #752, and record the resolved `vstest.console.exe` path (the `Extensions\TestPlatform` binary — the correct one per this repo's binding-redirect precedent, not the TestWindow copy that silently drops the redirect). The substitution carries the same acceptance conditions the literal wrapper-script invocation would have (total tests, pass/fail counts, coverage figures derived from the same Cobertura XML format) — it does not weaken the gate; it is a mechanical tool-resolution workaround for a known, out-of-scope environment defect (#752), not a scope reduction of what is measured.

**Coverage artifact.** No canonical `artifacts/csharp/coverage.xml` exists in this worktree; the derived Cobertura figures are captured directly in the feature-folder evidence tree (`p0-t18-coverage-figures.md`, `p0-t19-fileio2-coverage.md`, `p5-t6-coverage-figures.md`, `p5-t7-fileio2-coverage.md`, `p5-t8-coverage-delta.md`), which this repository's prior review practice accepts as the coverage artifact of record when the canonical path is not produced (committed feature-evidence Cobertura figures count as the artifact). This is not an artifact-absence FAIL.

**Changed-file coverage (blocking gate):**
- New-code floor (>=90% line coverage on the new catch block): `p5-t8-coverage-delta.md` computes `D_COVERED / D_VALID = 14 / 14 = 100%` (14 new valid lines, 14 new covered lines) on the `FileIO2.cs` delta, isolated by a baseline-vs-post-change Cobertura diff (`p0-t19` 241/276 -> `p5-t7` 255/290). **PASS**, well above the 90% floor. Note: the raw source diff added 8 textual lines, while the Cobertura delta reports 14 new valid lines; this reviewer attributes the difference to the async state-machine's multiple compiler-generated classes being merged onto one source-file entry (`Merge-CoberturaClassesByFilename`, noted in `p5-t7-fileio2-coverage.md`), which can produce more than one sequence point per source line. The delta is internally consistent (baseline and post-change both derived via the identical merge transform) and the acceptance conclusion (100% new-code coverage) is not affected by this observation. Not a blocking finding.
- No-regression on changed lines: `p5-t8-coverage-delta.md`'s acceptance table shows `POSTCHANGE_LINES_VALID (64661) >= BASELINE_LINES_VALID (64654)` and `POSTCHANGE_LINES_COVERED (38941) >= BASELINE_LINES_COVERED (38938)`, both TRUE. **PASS.**

**Repository-wide coverage (C#, `UtilitiesCS.Test`-scoped run):** `38941 / 64661 = 60.23%` post-change (`38938 / 64654 = 60.24%` baseline), both well below the uniform 85% line-coverage floor in `.claude/rules/quality-tiers.md`. Per the reviewing delegation's explicit instruction and this repository's established precedent (`.claude/agent-memory/atomic-planner/project_coverage_threshold_conflict_claude_md_vs_general_unit_test.md`; corroborated by this reviewer's own memory of repeated prior findings that a scoped `UtilitiesCS.Test`-only run is accepted non-blocking for small additive bugfixes while changed-line and new-code thresholds remain fully blocking), this sub-floor repo-wide figure is recorded as **FAIL, non-blocking**. It reflects a pre-existing repository-wide coverage gap unrelated to this change's two-file footprint (the same run also carries 38938/64654 baseline, i.e. the gap predates this branch), not a regression introduced here. The two blocking gates (new-code >=90%, no changed-line regression) both PASS as detailed above.

**Other languages:** No TypeScript, Python, or PowerShell production files appear in the scoped diff (`git diff --name-only ... | grep -E "\.(ts|tsx|py|ps1|psm1)$"` returned no output). Coverage verdicts for those languages are correctly omitted (zero changed files), not marked N/A/UNVERIFIED for changed files.

| Language | Changed files | Coverage artifact | Verdict |
|---|---|---|---|
| C# | 2 (`FileIO2.cs`, `FileIO2_Tests.cs`) | Feature-evidence Cobertura figures (see above) | New-code: PASS (100%); No-regression: PASS; Repo-wide (scoped run): FAIL, non-blocking per established precedent |
| TypeScript | 0 | n/a | Not applicable (no changed files) |
| Python | 0 | n/a | Not applicable (no changed files) |
| PowerShell | 0 | n/a | Not applicable (no changed files) |

## 4. General Unit Test Policy / C# Unit Test Policy

- **Framework**: MSTest `[TestMethod]`, FluentAssertions (`Should().Be(...)`, `Should().BeFalse()`) — confirmed by direct read of the new test in `FileIO2_Tests.cs`. No Moq needed for this test (the seam is plain delegates, matching the existing sibling tests' pattern). **PASS.**
- **Independence/Isolation/Determinism**: the new test uses local counters and injected delegates (`writerFactory`, `delay`), no shared static/mutable state, no real filesystem or wall-clock wait (`delay` returns `Task.CompletedTask` synchronously, never invoked). No temp files. **PASS.**
- **AAA structure and documented intent**: test carries a 4-line XML-doc-style summary comment explaining the scenario, and is structured Arrange/Act/Assert with a blank-line separator and inline `// Arrange` / `// Act` / `// Assert` comments. **PASS.**
- **Test file location**: `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` colocated with the existing sibling tests for the same class, consistent with this repo's existing `*.Test` project convention (not the `tests/` mirrored-tree convention that applies to other languages in this repo). **PASS** (matches existing repo style, per policy §"Where the repo already has a clear style, match that style").

## Summary

| Category | Verdict |
|---|---|
| Bugfix Workflow | PASS |
| C# toolchain (format/analyze/nullable/test) | PASS |
| Coverage — new-code / no-regression (blocking) | PASS |
| Coverage — repo-wide scoped run | FAIL, non-blocking (pre-existing, disclosed) |
| Evidence Location Compliance | PASS |
| Unit Test Policy (MSTest/FluentAssertions/AAA/determinism) | PASS |
| Scope Narrowing | None detected |

**No blocking policy findings.**
