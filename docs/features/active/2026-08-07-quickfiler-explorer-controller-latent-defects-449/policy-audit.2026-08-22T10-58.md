# Policy Audit — Issue #449, QuickFiler Explorer Controller Latent Defects

- **Timestamp:** 2026-08-22T10-58
- **Reviewer:** feature-review agent
- **Branch:** `bug/quickfiler-explorer-controller-latent-defects-449-exec` at `af6531ed3a321a3e59245097cde0fa237546c83b`
- **Base / merge-base:** `c551eabab0aa0a6b1a284252811a2e1de819634e` (tip of `epic/quickfiler-suite-determinism-foundation-integration`). Independently confirmed: `git merge-base HEAD c551eaba...` returns the same SHA, so the supplied base is the true merge-base.
- **Work mode:** `full-bug` (from `issue.md` marker) — `spec.md` is the sole AC source.
- **Diff scope:** the full branch diff `c551eaba..HEAD` — 50 files, +4,813/−274. Two commits: `05156a3a` (fix), `af6531ed` (AC check-offs + evidence). `git status --porcelain` shows one untracked file (see Finding NB-2); no tracked modifications.

## Scope Statement

The audit scope is the full branch diff against the resolved merge-base. No caller-supplied narrowing was attempted; no `## Rejected Scope Narrowing` entries are required. `artifacts/pr_context.summary.txt` is absent in this worktree; scope was derived directly from `git diff c551eaba..HEAD` (the authoritative source), which shows changed files in exactly two languages of content: C# (`.cs`, `.csproj`) and Markdown documentation.

## Section 1 — Policy Compliance Verdicts

| # | Policy area | Verdict | Evidence |
| --- | --- | --- | --- |
| 1 | Bugfix Workflow (failing regression test first, minimal fix, local verify) | **PASS** | Defect 2 carries a genuine fail-before/pass-after pair: `evidence/regression-testing/expect-fail-defect2.2026-08-22T09-16.md` (EXIT 1, Moq.MockException verbatim, against unfixed code) and `pass-after-defect2.2026-08-22T09-16.md` (EXIT 0 after a `/t:Rebuild`). Defects 1 and 3 carry exception dossiers adjudicated in Section 5. The fix is minimal: one changed assignment target, one member removal, one dead-region deletion, labelled hygiene. |
| 2 | Design principles (simplicity, separation of concerns) | **PASS** | The `NotInViewDialogInvoker` seam isolates the modal-dialog I/O behind an injectable delegate, matching the existing repo idiom (`QfcHomeController.QfcExplorerControllerLoader`). No new indirection beyond the seam. |
| 3 | 500-line file cap | **PASS** | Post-change counts measured by this reviewer (`awk 'END{print NR}'`): `QfcExplorerController.cs` 182, `IQfcExplorerController.cs` 14, `QuickFiler.Test.csproj` 486, `QfcExplorerControllerTests.cs` 387, `QfcExplorerController.ConversationViewTests.cs` 205. All under 500. The epic kickoff's predicted 1,065-line violation conflated the uncompiled `QuickFiler/Legacy/QuickFileController.cs`, which is not in the diff; no cap finding attaches to a file outside the diff. The 569-line intermediate state was split per plan task [P6-T14] (`evidence/other/test-file-size.2026-08-22T09-16.md`). |
| 4 | Error handling / logging | **PASS** | No `try`/`catch` added, changed, or removed; no logging added. The legacy broad-catch shape was deliberately not imported (spec D1), which complies with the broad-catch prohibition. |
| 5 | C# formatting (CSharpier, pinned 1.2.6 via `dotnet tool run`) | **PASS** | `evidence/qa-gates/step2b-csharpier-check.2026-08-22T09-16.md`: `csharpier check .` EXIT 0, 1,519 files (both new test files in scope), zero needing formatting. |
| 6 | C# analyzer build (`/t:Rebuild`, `EnableNETAnalyzers`, `EnforceCodeStyleInBuild`) | **PASS** | `evidence/qa-gates/step3-analyzer-build.2026-08-22T09-16.md`: EXIT 0, 5 pre-existing warnings, 0 errors, delta 0; non-vacuity proven by `Skipping target "CoreCompile"` count of 0 against 27 total `Skipping target` messages. |
| 7 | C# nullable/type-check build (`/t:Rebuild`, `TreatWarningsAsErrors`, no `/p:Nullable=enable`) | **PASS** | `evidence/qa-gates/step4-nullable-build.2026-08-22T09-16.md`: EXIT 0, 0 errors, same 5 pre-existing `packages.config` warnings not promoted. Command matches CI form. |
| 8 | Test framework mandate (MSTest + Moq + FluentAssertions) | **PASS** | Both new test files use `[TestClass]`/`[TestMethod]`/`[DataTestMethod]`, `Moq.MockRepository`, and FluentAssertions `.Should()`. No xUnit/NUnit introduced. |
| 9 | Toolchain order and single clean final pass | **PASS** | Final QA sequence recorded as steps 1–5 under `evidence/qa-gates/` (tool restore, format+check, analyzer rebuild, nullable rebuild, vstest with coverage, `/InIsolation`, `TestCategory!=LiveOutlook`, `\.claude\` excluded from discovery), all EXIT 0 in the final pass, plus a second consecutive identical full-suite run. |
| 10 | Unit test policy — determinism, no temp files, banned APIs | **PASS** | Reviewer-run scan on both new test files for `Thread.Sleep|Task.Delay|MessageBox.Show|Path.GetTempPath|new Form|Application.Run|DateTime.Now` returned zero matches (exit 1). Two consecutive full-suite runs report identical pass sets (`step5-second-consecutive-run.2026-08-22T09-16.md`). Mocked COM only; no live form, no message pump, no filesystem access. |
| 11 | Unit test policy — independence/isolation/AAA/documented intent | **PASS** | `[TestInitialize]` rebuilds the entire mock graph per test; no shared mutable state; every test carries an XML doc summary and Arrange/Act/Assert comments. See code review for detail. |
| 12 | Coverage exclusion policy direction | **PASS** | The change removes a pre-existing class-level `[ExcludeFromCodeCoverage]` (added 2026-06-13, commit `a564add0d`) and adds no exclusion anywhere. Reviewer-run `git grep ExcludeFromCodeCoverage -- QuickFiler/Controllers/QfcExplorerController.cs` returns zero matches. This is the direction both `CLAUDE.md` UT2 and `.claude/rules/general-unit-test.md` prescribe. |
| 13 | Public-API compatibility clause | **PASS** | One deliberate breaking change (`IQfcExplorerController.ExplConvView_Cleanup` removed): zero compiled callers, zero mock setups, one implementer edited in the same change; break called out in `spec.md` and the dossier. Reviewer-run `git grep ExplConvView_Cleanup -- "*.cs"` returns hits only in uncompiled `Legacy/` and `Notes/` files. |
| 14 | Tonality policy | **PASS** | Evidence artifacts and spec are factual and neutral; no humor, hyperbole, or decorative metaphor observed. |
| 15 | Epic hard constraints (no `.claude/**` edits, `/InIsolation`, evidence paths) | **PASS** | The diff touches nothing under `.claude/**`. `/InIsolation` present in every recorded vstest command. Evidence paths audited in Section 3. |

## Section 2 — Coverage Verification (per language, full branch diff)

Changed-file language census taken from `git diff c551eaba..HEAD --name-only`: C# source/test/project files (5) and Markdown (45). No `.ts`/`.tsx`, no `.py`, no `.ps1`/`.psm1` files are changed anywhere in the branch diff.

| Language | Coverage verdict |
| --- | --- |
| **C# / .NET** | **PASS** — repo-wide line coverage 85.3571% (156,317/183,133) post-change vs 85.3290% baseline (+0.0281 pp, no regression); `QuickFiler` package 80.9898% vs 80.9163% baseline (+0.0735 pp, epic NFR met); `QfcExplorerController` 87.8261% (101/115), previously absent from the report because the removed class-level attribute suppressed all four `<class>` elements; changed-line coverage 100% (3/3: lines 63, 139, 167 all hit). All figures independently recomputed by this reviewer from the raw Cobertura reports `coverage/baseline-p0t12.cobertura.xml` and `coverage/postchange-p7t6.cobertura.xml`, matching `evidence/qa-gates/coverage-delta.2026-08-22T09-16.md` to the digit. The measured repo-wide figure exceeds both the 85% rule floor and the 80% testable-denominator floor ratified in `CLAUDE.md`; the only machine-enforced numeric gate (`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487-489`, 80%) is cleared. |
| **TypeScript** | **PASS** — zero TypeScript files changed on this branch (verified via `git diff --name-only`); no coverage evidence is owed for this change. |
| **Python** | **PASS** — zero Python files changed on this branch (verified via `git diff --name-only`); no coverage evidence is owed for this change. |
| **PowerShell** | **PASS** — zero PowerShell files changed on this branch (verified via `git diff --name-only`); no coverage evidence is owed for this change. |

### Canonical C# coverage artifact disposition

`artifacts/csharp/coverage.xml` is absent, deliberately, and this reviewer did not create it. The substantive verification-from-existing-artifacts obligation is satisfied: the raw Cobertura reports exist on disk in `coverage/` and the committed evidence under `<FEATURE>/evidence/baseline/` and `<FEATURE>/evidence/qa-gates/` records the same figures, which this reviewer independently re-derived (root `line-rate` 0.8535709020220277; `QuickFiler` package 0.8098982423681776; per-file aggregation across all four `<class>` elements 87.8261%; per-line hits > 0 on lines 63, 139, 167). The evidence-verification model — inspect pre-existing coverage artifacts, do not rerun coverage generation — is met in full. The 85/75 constants hard-coded in `.claude/hooks/validate-feature-review-coverage.ps1` conflict with the 80% testable-denominator floor ratified in `CLAUDE.md` UT2; that contradiction is pre-existing and repository-wide (see Section 6). On the measured figures the outcome is identical under either floor, so no dispositional judgment between the two floors was required for this change.

### New-code coverage note

No new production class or module was added; the single new production member (the `NotInViewDialogInvoker` seam) has its executable initialiser line covered (hits > 0), and its production lambda body is deliberately unexercised by design so no dialog displays under test. Changed-line coverage is 100%, satisfying the no-regression-on-changed-lines requirement.

## Section 3 — Evidence Location Compliance

- All 43 evidence artifacts live under `docs/features/active/2026-08-07-quickfiler-explorer-controller-latent-defects-449/evidence/{baseline, qa-gates, regression-testing, other}/` — all four are canonical kinds per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. All 43 are Markdown; zero scripts or binaries leaked into `evidence/`.
- Reviewer-run scan of the branch diff for files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`: **zero matches** (grep exit 1).
- `validate_evidence_locations.py` does not exist in this repository (no `scripts/dev_tools/` tree); the scan above was performed manually against the full diff file list and is the operative check.
- No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` events: no instruction supplied a non-canonical evidence path.

**Verdict: PASS.**

## Section 4 — Shared-Surface Constraint (`QuickFiler.Test.csproj`, sibling #491)

Independently verified from the diff: the entire project-file diff is a **single hunk at lines 117–123** adding exactly two `<Compile Include>` lines (`QfcExplorerController.ConversationViewTests.cs`, `QfcExplorerControllerTests.cs`) immediately after the `QfcDatamodelLivenessTests` entry in the `Controllers` item group. The `Form1` compile region and the `Form1.resx` `EmbeddedResource` owned by sibling child #491 are untouched — no hunk reaches them and the appended lines sit approximately 40 lines clear of that region. **PASS.**

The move from the spec's "exactly one appended line / 485 lines" to two lines / 486 lines is a properly evidenced reconciliation, not a drift: the spec's own Constraints section pre-authorizes the split ("If the test set exceeds 500 lines, split into a second file ... and append a second compile entry in the same partitioned region"), plan task [P6-T14] is the executing provision, and `evidence/other/test-file-size.2026-08-22T09-16.md` records the trigger (569 lines), the split, both post-split counts, and the explicit supersession of the AC-12/AC-16 figures. See the feature audit for the AC-level judgment.

## Section 5 — Disclosed Items Adjudication

### 5.1 Flaky test in an untouched file — disclosure was correct; does not block

`UtilitiesCS.Test.Threading.ProgressTrackerAsync_Tests.InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker` failed once under full-suite load (NRE at `UtilitiesCS/Threading/ProgressTrackerAsync.cs:35`, 793 ms) and passed in isolation (191 ms) and in both subsequent full-suite runs with identical pass sets. Adjudication:

- The same-commit differing-outcome pattern (fail once, pass three times on an unchanged tree) is positive evidence of an environment/timing flake, not a regression introduced by this change. The diff touches nothing under `UtilitiesCS/Threading/` and `QfcExplorerController` has no relationship to `ProgressTrackerAsync`.
- The response was policy-correct: no retry added, no test modified, no timing tolerance applied, full disclosure in `evidence/qa-gates/step5-vstest-coverage.2026-08-22T09-16.md`, and the defect was promoted to a real GitHub issue (#584) with a structural root-cause analysis (`UiThread.Dispatcher` backed by a `null!`-initialised static with no lazy initialisation).
- **Verdict: does not block this PR.** The residual bookkeeping item is Finding NB-2 below.

### 5.2 Fail-before-exception dossiers for defects 1 and 3 — exceptions genuinely justified

- **Defect 1** (`fail-before-exception.defect1.2026-08-22T09-16.md`): a test *was* technically constructible — the dossier itself identifies the reflection contract test and a `NotThrow` shape — and rejects both on sound grounds. The reflection test asserts the absence of an API, encodes no behavior, and would permanently fail on any future restoration; the `NotThrow` shape applies only if the decision had been to implement rather than remove, and after removal it does not compile. No single assertion is meaningful both before and after the change, which is the defining requirement of a fail-before/pass-after pair. The adopted gate — the compiler (CS0535 forces the paired removal) plus clean analyzer/nullable builds plus the empty REMOVED set in the full-suite comparison — is the strongest available verification. The absence proof is complete (all six pre-change hits enumerated with compilation status; `Legacy/` and `Notes/` proven uncompiled via zero `Compile Include` entries; `--untracked` mock-setup search). **Exception justified.**
- **Defect 3** (`fail-before-exception.defect3.2026-08-22T09-16.md`): no test was constructible — the six deleted members are private/internal statics with a provably empty inbound call graph, so no input can transfer control into any deleted line. The dossier's non-vacuity check (12 matches at merge-base, 0 now, identical pattern and scope) is the discriminating detail that elevates this above a bare zero-match claim. The issue's own criterion ("a test run confirming no behavior change") is satisfied by the before/after full-suite comparison (6,437 → 6,452, +15 added, 0 removed). The `InternalsVisibleTo` angle for `StripTabsCrLf` was explicitly checked. **Exception justified.**

Both dossiers carry every field required by spec D7 (`Timestamp:`, `Command:`, `EXIT_CODE:`, `WhyFailingRunImpossible:`, `SearchScope:`, `SearchPatterns:`, `SearchResult:`), and this reviewer reproduced the AC-1 and AC-6 search results against the working tree.

## Section 6 — Pre-existing Policy Tension (noted, not a finding of this change)

`.claude/rules/general-unit-test.md` states a repo-wide 85% line floor and a no-production-file-exclusion policy; `CLAUDE.md` ratifies an 80% testable-denominator floor with a COM/VSTO/WinForms exemption; the only machine-enforced numeric gate in `scripts/` is the 80% repo-wide check. This tension is pre-existing and repository-wide. This change moves in the direction both policies agree on — it *removes* a production-file exclusion and raises measured coverage — and the measured figures clear every floor in play, so the tension has no bearing on this change's verdict.

## Section 7 — Findings

| ID | Classification | Finding |
| --- | --- | --- |
| NB-1 | **Non-blocking** (Minor) | `QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs:1-2` carries two unused `using` directives (`System.Collections`, `System.Collections.Generic`) — leftovers from the pre-split single file; their only consumers moved to `QfcExplorerController.ConversationViewTests.cs` in the [P6-T14] split. Verification: `grep -n "Collections\|IEnumerable\|List<" QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs` matches only the two directive lines. No gate fires (CS8019 is a hidden diagnostic; IDE0005's analyzer is not wired into these non-SDK projects), so this is hygiene of exactly the class D4 removed from the production file. Fix opportunistically in a later touch of the file; do not spin a remediation cycle for it. |
| NB-2 | **Non-blocking** (Residual bookkeeping) | The flaky-test promotion document `docs/features/potential/promoted/2026-08-22-uithread-dispatcher-null-race-progresstrackerasync.md` (Issue #584) exists only as an **untracked** file in this worktree (`git status --porcelain` shows `??`). The epic's hard constraints forbid a child branch writing under `docs/features/potential/**`, so it cannot ride this PR. The durable record currently is GitHub issue #584 itself, which satisfies the promotion requirement; the document must reach the repository through a non-child route (epic close or a direct commit to `main`) or be accepted as superseded by the issue body. Flagged for the orchestrator; no action owed by this change. |
| NB-3 | **Non-blocking** (Note) | AC-12's "exactly one appended line" and AC-16's "485" figure are superseded by two lines / 486, evidenced in `evidence/other/test-file-size.2026-08-22T09-16.md` and pre-authorized by the spec's own split provision. Recorded here so the numeric divergence between the checked-off AC text and the delivered state is traceable; judged properly evidenced in the feature audit. |

## Section 8 — Verdict

- **Blocking findings: 0**
- **Non-blocking findings: 3** (NB-1 minor hygiene, NB-2 residual bookkeeping for the orchestrator, NB-3 traceability note)
- No remediation-inputs artifact is produced: nothing requires a remediation cycle.
