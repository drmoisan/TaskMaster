# Policy Audit — qfc-twin-processcmdkey-alt-chord-over-claim-663

- **Issue:** #663
- **Branch:** `bug/qfc-twin-processcmdkey-alt-chord-over-claim-663`
- **Head:** `20f1b201ec544b4ca464bcf7b50e2b5480e007ed`
- **Base:** `origin/main` @ `9ca9e99a86428717891a4b54fed70f573a0a2d65`
- **Merge base (recomputed):** `9ca9e99a86428717891a4b54fed70f573a0a2d65` — identical to the supplied base; the three-dot anchor `origin/main...HEAD` is correct.
- **Work Mode:** `full-bug` — `spec.md` is the sole acceptance-criteria source. The absence of `user-story.md` is correct for this mode and is not a finding.
- **Audit timestamp:** 2026-09-01T19-05
- **Reviewer scope:** the full branch diff against the resolved base branch.

All paths in this document are repository-relative. The worktree root is rendered as `<repo-root>`.

## Verdict Summary

| Area | Verdict |
|---|---|
| Scope invariant / base resolution | PASS |
| Evidence location compliance | PASS |
| General Code Change Policy | PASS |
| C# Code Change Policy | PASS |
| General Unit Test Policy | PASS |
| C# Unit Test Policy | PASS |
| C# coverage thresholds | PASS |
| C# coverage artifact at the canonical path | FAIL (non-blocking; see PA-1) |
| Toolchain loop (format, analyzers, type-check, tests) | PASS |
| Bugfix workflow (RED-first) | PASS |
| Tonality policy | PASS |
| **Blocking findings** | **0** |

## Rejected Scope Narrowing

None. The delegating prompt named the three changed `.cs` files and the surrounding documentation set, but it did not instruct this reviewer to limit the audit to a plan, task, or phase, and it did not mark any language out of scope. This audit was performed against the full `origin/main...HEAD` diff, which comprises 68 files.

The delegating prompt did direct this reviewer not to create `artifacts/csharp/coverage.xml` and to read the transcribed coverage evidence instead. That direction is recorded rather than rejected: it does not narrow the audit scope, and an explicit coverage verdict for C# is recorded below as required. The independent consequence of the raw document's absence is recorded honestly as finding PA-1.

## Changed-File Inventory (full branch diff)

| Category | Files | Notes |
|---|---|---|
| C# production | 2 | `QuickFiler/Controllers/QfcFormKeyHandler.cs` (+19/-0), `QuickFiler/Viewers/QfcFormViewer.cs` (+1/-4) |
| C# test | 1 | `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs` (+144/-0) |
| Feature documentation | 4 | `issue.md`, `spec.md`, `plan.2026-08-31T20-16.md`, `research/2026-09-01T01-05-...md` |
| Evidence artifacts | 49 | under `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/` |
| Agent memory | 11 | under `.claude/agent-memory/` |
| Promotion record | 1 | `docs/features/potential/promoted/2026-08-31-invoke-mstest-single-assembly-strictmode-count-throw.md` |

Languages with changed files in the branch diff: **C# only**. No `.ts`, `.py`, `.ps1`, `.psm1`, or `.sh` file appears in the diff. Coverage verdicts for TypeScript, Python, and PowerShell are therefore not required; those languages have zero changed files on this branch.

The promotion record for issue #713 is intentional and correct. Issue #713 was opened during this feature's preparation for a tooling defect that `spec.md` documents as excluded from this fix. The record is not a stray sweep from an unrelated queued promotion; `evidence/qa-gates/code-commit.md` documents that the source commit used an explicit three-path `git add` precisely to avoid such a sweep, and the `.cs` change set confirms none occurred.

## Evidence Location Compliance

**PASS.** `git diff --name-only origin/main...HEAD` returns zero paths matching `^artifacts/(baselines|baseline|qa|qa-gates|evidence|coverage|regression-testing|post-change)/`. In fact no path under `artifacts/` appears in the branch diff at all.

All 49 evidence artifacts are written under `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/<kind>/`, using only canonical sub-paths: `baseline/` (14), `qa-gates/` (22), `regression-testing/` (4), `issue-updates/` (1), `other/` (4). No non-canonical evidence sub-path is used.

`spec.md` records, under "Coverage impact and targets", the reason coverage evidence was placed in `evidence/qa-gates/` rather than an `evidence/coverage/` sibling: `coverage` is not a member of the canonical sub-path set enumerated by the `evidence-and-timestamp-conventions` skill. That reasoning is correct and matches the skill text.

No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` condition arose.

## Coverage Verification

### Changed-language determination

C# is the only language with changed files on this branch. The determination was made from `git diff --name-only origin/main...HEAD`, not from the PR context summary, which misclassifies this branch (see finding PA-2).

### C# coverage figures

The evidence of record is `evidence/qa-gates/coverage-final.md`, with the pre-change baseline at `evidence/baseline/coverage.md`. Both transcribe readings from post-processed Cobertura documents, so the two figures are commensurable — the baseline artifact states explicitly that `BASELINE_CLASS_LINE_RATE` was taken from the post-processed document and that the post-change reading must be taken from the same document kind, and the post-change artifact confirms it was.

| Scope | Measured | Floor | Verdict |
|---|---|---|---|
| C# repo-wide line coverage, post-change root `line-rate` 0.853726 (85.3726%) vs pre-change 0.853866 | 85.3726% | >= 85% | **PASS** |
| C# repo-wide branch coverage, post-change root `branch-rate` 0.794078 vs pre-change 0.794064 | 79.4078% | >= 75% | **PASS** |
| C# new-method line coverage on `ClaimsAltChord` (new code) | 100% (`line-rate` 1) | >= 90% | **PASS** |
| C# new-method branch coverage on `ClaimsAltChord` | 100% (`branch-rate` 1) | >= 75% | **PASS** |
| C# declaring-class line coverage, `QuickFiler.Controllers.QfcFormKeyHandler`, vs baseline 1 | 1 | not lower than baseline | **PASS** |
| C# changed-line coverage regression on the new method's seven instrumented lines | zero uncovered | no regression | **PASS** |

**C# coverage threshold verdict: PASS.** Every applicable line and branch floor is met or exceeded, and there is no regression on changed lines.

Corroborating detail read from the transcribed `<method>` element: all seven instrumented lines (29, 30, 31, 32, 35, 36, 37) carry `hits="1"`; line 30 reports `condition-coverage="100% (4/4)"` for the compound null-or-no-Alt guard and line 36 reports `condition-coverage="100% (2/2)"` for the `Keys.Menu`-or-`Keys.None` acceptance. Both branch arms of the new predicate are therefore exercised, not merely its lines.

The root `lines-covered` figure fell by 3 while `lines-valid` grew by 7. The +7 is exactly the new method's instrumented line set. The −3 lies outside the changed set; `evidence/qa-gates/coverage-final.md` attributes it to non-determinism in the concurrent instrumented suite's denominator and declines to make a stronger claim. That disposition is appropriate and consistent with the known non-determinism of this repository's C# coverage constants. It does not affect the changed-line no-regression reading, which is established by direct per-line measurement rather than by subtraction.

### Threshold-source note

`CLAUDE.md` states a repository-wide floor of 80% and a new-module target of 90%; `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state a uniform 85% line and 75% branch floor. The two documents are unreconciled in this repository. This branch clears both formulations on every row, so the conflict does not change any verdict here. It is recorded because it recurs.

### PA-1 — C# coverage artifact at the canonical path is absent (FAIL, non-blocking)

**Verdict: FAIL.** The canonical C# coverage artifact path `artifacts/csharp/coverage.xml` does not exist in this worktree, and no raw Cobertura document exists anywhere in the tree or in the branch's git history.

This row is recorded as a FAIL rather than argued into a pass, per the honest-reporting requirement. The mitigating context is stated so a maintainer can weigh it, not so it can be reclassified:

- The deletion was mandated by the plan and by AC-11 itself, which requires the document to be written, post-processed, read, transcribed, and then deleted, on the stated ground that raw Cobertura is machine-generated measurement data of order ten megabytes and is not committed in this repository. The measured size was 10,792,221 bytes.
- The transcription is verbatim and complete for the elements AC-11 names: the `<method>` element for `ClaimsAltChord` and the containing `<class>` element are both reproduced in full XML form, and the baseline artifact independently records the same class element in its pre-change state with one method and no `ClaimsAltChord`.
- The raw document was never committed. `git log --name-only origin/main..HEAD -- '*.cobertura.xml' 'artifacts/csharp/*'` returns nothing, so no multi-megabyte blob is reachable in branch history and no squash-merge remedy is required.
- The disposition mirrors the one accepted for the same class of artifact under feature #464.

**Residual risk from PA-1.** The transcription cannot be independently re-derived, because the source document no longer exists. The four figures above are therefore executor-attested rather than reviewer-reproducible. This is a real evidentiary limitation and is the reason the row reads FAIL. It is assessed as **non-blocking** because every other reading in this audit that could be independently reproduced was reproduced and matched, which raises confidence in the executor's transcription discipline generally, and because the residual risk is one of verification depth rather than of a known defect.

**Reachability of PA-1: latent, and process-only.** There is no runtime defect behind this finding. It is not merge-method-dependent.

## Policy-by-Policy Findings

### CLAUDE.md and General Code Change Policy

| Requirement | Verdict | Evidence |
|---|---|---|
| Bugfix workflow: failing regression test first | PASS | `evidence/regression-testing/red-run.md` records exit code 1 against an expected 1, with exactly three failures — `ClaimsAltChord_WithAltM_ReturnsFalse`, `ClaimsAltChord_WithAltF4_ReturnsFalse`, `ClaimsAltChord_WithAltLeft_ReturnsFalse` — each with verbatim FluentAssertions text reading `but found True` and each attributed to declaring type `QuickFiler.Controllers.Tests.QfcFormKeyHandlerTests` from its stack trace. |
| Bugfix workflow: minimal targeted fix | PASS | Net production change is +19/-0 in one file and +1/-4 in another. No opportunistic refactor. |
| Bugfix workflow: no scope widening | PASS | The tooling defect discovered during preparation was promoted to issue #713 rather than fixed here. |
| Simplicity first | PASS | One `internal static` pure predicate; a single bitwise mask and two comparisons. No indirection added. |
| Separation of concerns | PASS | The claim decision is pure logic in a controller-side helper; the WinForms message plumbing stays in the viewer. This is the stated purpose of `QfcFormKeyHandler`. |
| File size <= 500 lines | PASS | `QfcFormKeyHandler.cs` 39, `QfcFormViewer.cs` 293, `QfcFormKeyHandlerTests.cs` 211. All three measured at head with `awk 'END{print NR}'`. |
| Error handling / fail fast | PASS | The predicate is total over its input domain, throws nothing, and logs nothing, as `spec.md` states. |
| Naming | PASS | `PascalCase` member, `camelCase` local `keyCode`. `ClaimsAltChord` matches the delivered #467 precedent name. |
| Public API stability | PASS | Both the type and the new member are `internal`. No public surface changes. |
| Documentation comments | PASS | The new member carries a full XML doc comment with `<summary>`, both `<param>` elements, and `<returns>`. |
| No policy documents modified | PASS | No path under `.claude/rules/` or `.github/instructions/` appears in the diff. |
| No secrets or `.env` files | PASS | None in the diff. |

### C# Code Change Policy and toolchain

| Stage | Command evidence | Verdict |
|---|---|---|
| Format | `evidence/qa-gates/csharpier-check.md`: `dotnet tool run csharpier check .`, exit 0, `Checked 1566 files in 4478ms.`, no unformatted path reported. The 1566 file count equals the Phase 0 baseline, consistent with no file added or removed. Invoked through `dotnet tool run` as the policy requires. | PASS |
| Analyzers | `evidence/qa-gates/msbuild-analyzers.md`: exit 0; one console line matching `^\s*0 Error\(s\)$`; zero lines matching `: error [A-Z]+[0-9]+:`; 36 occurrences of the literal `Task "Csc"` in the detailed log, proving `CoreCompile` ran and the gate is not vacuous; `/t:Rebuild` used, not `/t:Build`. | PASS |
| Type-check / nullable | `evidence/qa-gates/msbuild-nullable.md`: `/t:Rebuild` with `/p:TreatWarningsAsErrors=true`, exit 0. `/p:Nullable=enable` is absent, matching the repository's per-file opt-in model and CI's actual invocation. | PASS |
| Tests | `evidence/qa-gates/tests-final.md`: `Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug`, exit 0, 6934 of 6934 passed across 9 discovered assemblies, empty failing list, standard error stream 0 bytes. | PASS |

The vacuity guard on the analyzer gate is a genuine strength of this evidence set. Counting `Task "Csc"` occurrences directly falsifies the failure mode in which a warm incremental build returns exit 0 having skipped compilation entirely; the count is 36, so the analyzers actually ran.

The warning comparison is baseline-relative rather than an absolute zero, and the artifact states why: the analyzer CI workflow runs without `/p:TreatWarningsAsErrors=true`, so a pre-existing warning naming a changed file could exist on `origin/main`. In this instance both the run's pair set and `BASELINE_WARNINGS` are empty, so the two formulations coincide and no leniency was actually consumed. The five reported warnings are codeless `System.Reactive.PackagesConfigCheck.targets` notices that carry no diagnostic identifier and name none of the three changed files.

### General Unit Test Policy

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence | PASS | Each of the seven new tests constructs its own `Mock<IQfcKeyboardHandler>` in its Arrange block. No shared fixture state, no static mutable state. |
| Isolation | PASS | Each test calls only `QfcFormKeyHandler.ClaimsAltChord` and asserts its return value. |
| Fast execution | PASS | The red run reports per-test durations of 73 ms, 1 ms, and `< 1 ms`; the whole 6934-test suite completes in 26.9 seconds. |
| Determinism | PASS | No clock read, no RNG, no `Thread.Sleep`, no `Task.Delay`, no wall-clock wait. Inputs are compile-time `Keys` constants. |
| Readability | PASS | Explicit `// Arrange`, `// Act`, `// Assert` comments in every one of the seven methods. |
| Scenario completeness | PASS | Positive (two key-data shapes), negative mnemonic, negative system chord, negative vestigial chord, no-modifier, modifier-only non-Alt, and null-argument cases are all covered. |
| Clear failure messages | PASS | Every assertion carries a FluentAssertions because-string. The red-run transcript demonstrates their quality in practice: `Expected result to be False because Alt+M is the Move Options mnemonic on the hosted item viewers and must reach the base implementation, but found True.` |
| No external dependencies | PASS | No database, network, file system, or external process. |
| No temporary files | PASS | Pattern VC-1 returns zero matches over the test file. |
| Coverage exclusion policy: no production path excluded | PASS | `evidence/qa-gates/no-new-exemption.md`, and independently `git diff -U0 origin/main...HEAD -- '*.cs' \| grep -c "^+.*ExcludeFromCodeCoverage"` returns 0. No `coverage.config` change appears in the diff. |
| Test file location | PASS with a repository-convention note | `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs` mirrors `QuickFiler/Controllers/QfcFormKeyHandler.cs`. The rule text specifies a `tests/` tree; this repository's established convention is a sibling `<Project>.Test/` tree. The new tests were added to a pre-existing file in the established location, which is the correct choice. The rule-versus-repository divergence is repository-wide and is not attributable to this branch. |

Property-based test density: `.claude/rules/quality-tiers.md` requires at least one property test per pure function for T1 and T2 modules and none for T3 and T4. `ClaimsAltChord` is a pure function, so the obligation would bind if QuickFiler were classified T1 or T2. **This item could not be resolved**, for a concrete reason: the file `quality-tiers.yml`, which that rule names as the source of truth mapping every project to a tier, does not exist at this repository's root. No tier can be assigned to QuickFiler, so no property-test obligation can be established or ruled out. This is a repository-wide governance gap, not a defect introduced by this branch, and it is recorded as finding PA-3 rather than charged against this change.

### C# Unit Test Policy

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `[TestMethod]` on all seven methods, within the existing `[TestClass]`; `using Microsoft.VisualStudio.TestTools.UnitTesting;` already present. |
| Moq for mocking | PASS | `new Mock<IQfcKeyboardHandler>()` in six of seven methods; `using Moq;` added. |
| FluentAssertions for assertions | PASS | Every assertion uses `.Should().BeTrue(...)` or `.Should().BeFalse(...)`. No MSTest `Assert` call is introduced. |
| No `Form`-derived type in the test assembly | PASS | `ExecutingAssembly_ContainsNoFormDerivedType` is reported `Passed [1 ms]` in the final run transcript, and pattern VC-1 returns zero matches. |

### Tonality Policy

**PASS.** The specification, plan, and all 49 evidence artifacts use neutral, factual, evidence-proportioned language. Claims are consistently qualified to the strength of the evidence. Two representative examples: `evidence/qa-gates/coverage-final.md` states of the three-line root movement that "No stronger claim is made here" rather than asserting a cause; `evidence/other/manual-validation.md` separates what the automated tests establish from what they do not, in explicit lists. No hyperbole, humor, or decorative metaphor was found.

## Non-Blocking Findings

### PA-1 — C# coverage artifact absent at the canonical path

FAIL, recorded above under Coverage Verification. Reachability: latent, process-only. Not merge-method-dependent.

### PA-2 — PR context summary misclassifies all three C# files as documentation

`artifacts/pr_context.summary.txt` is fresh: it stamps `2026-09-01 23:04:35 UTC` and pins `Head SHA: 20f1b201...`, which equals current `HEAD`. It is nonetheless unreliable in three respects.

1. **Language misclassification.** Its "Changed files overview" reports `Core logic changes: 0 files` and `Docs/templates/agents/tooling: 50 files`. None of the three changed `.cs` paths appears in that overview with a `(+N/-N)` entry; the eleven occurrences of `.cs` in the file are all prose quoted from `spec.md`. The branch's only production change is therefore invisible to any consumer that reads changed languages from this file. The practical consequence is that the coverage-validation hook, which derives its changed-language set from this summary, would compute an empty set and skip C# enforcement entirely. This audit records explicit C# coverage verdicts regardless, from the `git diff` determination.
2. **False GitHub CLI unavailability.** The summary reports `GitHub CLI unavailable: GitHub CLI (gh) is not installed.` while `gh` is in fact functional.
3. **Contaminated auto-close list.** The "author asserted" close-candidate list contains tokens scraped from acceptance-criterion identifiers and unrelated precedent citations. Only `#663` is this branch's issue. Issue `#713` is open and is explicitly excluded by `spec.md`; it must not be auto-closed by this merge.

This is a recurring defect in the context-collection tooling, not a defect in this branch. Reachability: **live in the review and PR tooling now**; it has no product runtime reachability. Not merge-method-dependent. The correction has been applied in place throughout this audit.

### PA-3 — `quality-tiers.yml` absent, so the tier-dependent gate matrix is unresolvable

`.claude/rules/quality-tiers.md` designates `quality-tiers.yml` at the repository root as the source of truth mapping every project to a tier, and states that adding a project without a tier classification fails CI. No such file exists at this repository's root. Consequently the tier-dependent obligations — property-test density, mutation score, untyped-escape-hatch budget, golden tests — cannot be evaluated for QuickFiler or for any other project here. Reachability: latent, governance-only; no runtime defect. Not merge-method-dependent. Repository-wide and pre-existing; not attributable to this branch.

### PA-4 — Declared evidence timestamps are not clock-derived

The 45 evidence artifacts carry a `Timestamp:` field forming a strictly monotonic sequence from `2026-09-01T21-44` to `2026-09-01T23-45`, spanning two hours and one minute. Those declared values cannot be reconciled with the git metadata of the commits that contain them, and the contradiction does not depend on any timezone interpretation.

The tightest demonstration uses two commits read from a single clock:

- `evidence/qa-gates/code-commit.md` declares `Timestamp: 2026-09-01T23-24` and records that it created commit `ae2885e7`. Git reports `ae2885e7` with committer date `2026-09-01T18:55:14-04:00`.
- `evidence/qa-gates/end-state.md` declares `Timestamp: 2026-09-01T23-45` and records that it folded the last three paths into the preceding commit by `git commit --amend --no-edit`. Git reports the resulting `20f1b201` with committer date `2026-09-01T19:02:44-04:00`.

The declared interval between those two tasks is 21 minutes. The actual interval between the two commits, on one clock, is 7 minutes 30 seconds. Twenty-one declared minutes of work cannot occupy a seven-and-a-half-minute real interval.

An independent clock corroborates this. `artifacts/pr_context.summary.txt` was generated by separate tooling and stamps `2026-09-01 23:04:35 UTC`, which is `19:04:35 -04:00` — one minute fifty-one seconds after the final amend. Artifacts contained inside that amend declare production times as late as `23-45`, roughly forty minutes ahead of an independently observed wall clock.

**Assessment.** The `Timestamp:` fields are declarative bookkeeping rather than clock reads. They cannot be used to establish ordering, freshness, or elapsed duration. This finding is explicitly **not** a challenge to the substantive gate content, for two reasons that were checked directly:

- The RED-first ordering claim does not depend on the timestamps. It is established by content that could not have been produced from the final code: `evidence/regression-testing/red-run.md` reproduces three FluentAssertions failures reading `but found True` for the three tests that the delivered predicate makes return `False`.
- The test-count arithmetic is self-checking and matches across four independent runs: baseline 6927, red run 6934 with 3 failed, green and final runs 6934 with 0 failed. 6927 + 7 = 6934 confirms that exactly seven test methods were added and that no existing test was removed or renamed.

Reachability: latent, documentation and process only. There is no runtime defect. Not merge-method-dependent.

### PA-5 — Stale line-number citations in prose

Three documents cite `QuickFiler.Test/QuickFiler.Test.csproj:151` for the `QfcFormKeyHandlerTests.cs` compile entry: `spec.md` (Test Strategy), `plan.2026-08-31T20-16.md` (reading guide), and the research artifact. The entry is at line 152 at head, `origin/main` having added a line above it. Separately, AC-14's criterion text cites the retained unused locals at `QuickFiler/Viewers/QfcFormViewer.cs:64-67`; post-change they sit at lines 61, 62, and 64, the guard having collapsed from four lines to one.

Neither citation is load-bearing. No acceptance condition asserts a line number: AC-9 and AC-14 are verified by path-level diff listings and by the content-matching pattern VC-2, all of which were reproduced successfully. Reachability: latent, documentation only. Not merge-method-dependent.

## Blocking Findings

**None. Blocking count: 0.**

## Assumptions Recorded

1. The evidence artifacts' verbatim command transcripts are accepted as faithful where the underlying artifact was deleted by plan mandate — specifically the msbuild logs, the console captures, and the Cobertura document. Every reading that survived as a re-runnable check was independently reproduced by this reviewer and matched; no discrepancy was found in any reproducible check.
2. `spec.md`'s claim that `QuickFiler/Viewers/QfcFormViewerDark.cs`, `QuickFiler/Viewers/QfcFormViewerExpanded.cs`, and `QuickFiler/Legacy/` are absent from the compile item list was verified directly against `QuickFiler/QuickFiler.csproj` rather than assumed. It holds: none of those paths appears in the project file, while `Viewers\QfcFormViewer.cs` appears at line 452.
