# Policy Audit — quickfiler-keyboard-action-defects (Issue #444, closes #472, #482)

- Artifact: `policy-audit.2026-08-27T20-34.md`
- Reviewer: feature-review agent
- Review timestamp (UTC): 2026-08-27T20-34
- Branch under review: `bug/quickfiler-keyboard-action-defects-444` @ `833423ba`
- Base branch: `origin/epic/quickfiler-bug-family-integration`
- Resolved diff base (merge-base): `4f238289` — recomputed independently with `git merge-base HEAD origin/epic/quickfiler-bug-family-integration`
- Work mode: `full-bug` (persisted marker in `issue.md`); AC source is `spec.md` only; `user-story.md` is intentionally absent (NONE, by design)

## Base Resolution Note

The caller-supplied state ("0 behind the base") was measured against base tip `4f238289`. At review
time `origin/epic/quickfiler-bug-family-integration` has advanced 10 commits to `13a22ade`; all 10
belong to the sibling #493 fan-in (`Merge pull request #653 ...`), verified with
`git log --oneline 4f238289..origin/epic/quickfiler-bug-family-integration`. The audit scope is the
merge-base diff `4f238289..HEAD`, which equals the three-dot diff against the epic tip; diffing
two-dot against the moved tip would misattribute sibling #493 changes as deletions on this branch.
The orchestrator should perform (or delegate) a final merge-up before the integration PR merge.

## Rejected Scope Narrowing

None. No caller instruction attempted to narrow the audit below the full branch-vs-base diff. The
caller's file list matched the measured diff exactly.

## Branch Diff Scope (verified with `git diff --numstat 4f238289..HEAD`)

Production C# (3 files): `QuickFiler/Controllers/KbdActions.cs` (+36/-0),
`QuickFiler/Controllers/QfcCollectionController.cs` (+8/-8),
`QuickFiler/Controllers/QfcItemController.Navigation.cs` (+28/-4).
Test C# (4 files): `QuickFiler.Test/Controllers/KbdActionsTests.cs` (+37/-0),
`QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs` (+91/-0),
`QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` (+226/-0, new),
`QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs` (+107/-0).
Build config: `QuickFiler.Test/QuickFiler.Test.csproj` (+1/-0).
Documentation/evidence: feature folder docs and 85 evidence artifacts, one promoted potential entry,
two orchestrator agent-memory files. Changed languages with code files: C# only. TypeScript, Python,
and PowerShell each have zero changed files in the branch diff (verified via
`git diff --name-only 4f238289..HEAD`), so no coverage verdict is required for those three languages.

## 1. Toolchain Gates (CLAUDE.md C#1/CUT3, general-code-change.md)

| Gate | Verdict | Evidence |
| --- | --- | --- |
| Formatting (CSharpier, pinned via `dotnet tool run`) | PASS | Executor: `evidence/qa-gates/p4-t2-format-check.2026-08-27T19-48.md` EXIT_CODE 0, 1541 files. Independent reviewer re-check: `dotnet tool run csharpier check` over the 7 changed `.cs` files, exit 0, "Checked 7 files". |
| Analyzers (`msbuild /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`) | PASS | `evidence/qa-gates/p4-t4-analyzers.2026-08-27T19-50.md` EXIT_CODE 0, 0 errors; the 5 warnings are the pre-existing System.Reactive packages.config diagnostic (pre-existing at base, not introduced here). |
| Type check / nullable (`msbuild /t:Rebuild ... /p:TreatWarningsAsErrors=true`, no `/p:Nullable=enable`) | PASS | `evidence/qa-gates/p4-t5-typecheck.2026-08-27T19-51.md` EXIT_CODE 0. Command matches the CI form; `/p:Nullable=enable` confirmed absent. |
| Tests (vstest, `/EnableCodeCoverage /InIsolation`, `\.claude\` exclusion) | PASS | `evidence/qa-gates/p4-t6-final-tests.2026-08-27T19-53.md`: 6713/6713 passed, 0 failed. Independent reviewer verification: parsed `evidence/qa-gates/p4-t6/p4-t6-final.trx` with `xml.etree.ElementTree` — 6713 `UnitTestResult` elements, all `outcome=Passed`. |
| Single clean final pass | PASS | `evidence/qa-gates/p4-t12-clean-pass.2026-08-27T20-00.md` EXIT_CODE 0. |

## 2. Coverage (general-unit-test.md, quality-tiers.md; C# is the only changed-code language)

Verification model: evidence inspection plus independent re-parse of the raw Cobertura documents the
executor's wrapper produced (`coverage/coverage.cobertura.baseline.xml` and
`coverage/coverage.cobertura.final.xml`, gitignored but still present in the worktree). Every figure
below was re-derived by the reviewer from the raw XML and matches the committed evidence
(`evidence/qa-gates/p4-t8-coverage-final.2026-08-27T19-58.md`,
`evidence/qa-gates/p4-t11-coverage-delta.2026-08-27T20-00.md`) exactly. Arithmetic cross-check:
54402/63905 = 85.13%, 12935/16330 = 79.21%.

| Coverage row | Measured | Verdict |
| --- | --- | --- |
| C# repo-wide line coverage (floor 85%; unfiltered whole-run denominator) | 85.13% (baseline 85.04%, delta +0.09) | PASS |
| C# repo-wide branch coverage (floor 75%) | 79.21% (baseline 79.12%, delta +0.09) | PASS |
| C# changed file `KbdActions.cs` line coverage (no-regression + changed-line rule) | 98.98% line (baseline 93.98%), branch-rate 1 | PASS |
| C# changed file `QfcItemController.Navigation.cs` line coverage | 92.13% line (baseline 90.68%), branch 87.5% | PASS |
| C# new member `SyncExpandedRegistrations` line coverage (new-code floor 90%) | 100% line, 100% branch (single `<method>` node, independently located) | PASS |
| C# new guard branch in `KbdActions.cs` ctor — both paths covered | throw line 61 hits=1, normal-completion line 65 hits=1 (independently read from raw XML) | PASS |

Notes:
- `QuickFiler/Controllers/QfcCollectionController.cs` carries a class-level
  `[ExcludeFromCodeCoverage]` at its declaration, pre-existing at base `4f238289` (verified with
  `git show 4f238289:QuickFiler/Controllers/QfcCollectionController.cs`, attribute at the class
  declaration). No coverage figure is attributed to it in either document (XPath over `//class`
  returns no node), and the feature diff does not add or alter any coverage exclusion. The attribute
  predates this feature and falls under the ratified COM/VSTO exemption in CLAUDE.md §UT2; the
  tension with the stricter Coverage Exclusion Policy in `.claude/rules/general-unit-test.md` is a
  pre-existing repository documentation conflict (Observation OB-2 below), not a change made by this
  branch.
- The canonical `artifacts/csharp/coverage.xml` was deliberately not created by the executor or by
  this review; verification was performed from the feature's committed evidence and the raw local
  Cobertura documents, which is the required evidence-inspection model.
- Test files are excluded from the production coverage discussion by policy; the four changed test
  files are measured by the suite, not gated by the floor.

## 3. Deletion Invariant (epic NFR)

Requirement: no file loses content the base gained. Independent verification:
`git diff --numstat 4f238289..HEAD | awk '$1==0 && $2>0'` returned **zero rows** (command re-run by
the reviewer). Files with deletions (`plan` 167/167, `spec` 54/54, `QfcCollectionController.cs` 8/8,
orchestrator memory 46/41, `MEMORY.md` 1/1) all pair deletions with additions — in-place edits, not
content loss. Verdict: PASS.

## 4. Shared Project Files (epic NFR — sibling ownership)

| Check | Result | Verdict |
| --- | --- | --- |
| `QuickFiler/QuickFiler.csproj` untouched | `git diff 4f238289..HEAD -- QuickFiler/QuickFiler.csproj` produced 0 lines | PASS |
| `QuickFiler.Test/QuickFiler.Test.csproj` exactly one added line | +1/-0; the single line is `<Compile Include="Controllers\QfcCollectionControllerNavigationDigitsTests.cs" />`, inserted between the `QfcCollectionControllerTests.cs` and `QfcCollectionControllerDarkModeTests.cs` entries as the spec requires | PASS |
| No edits outside the owned `Controllers\Qfc*`/`KbdActions` region; no `Viewers\Breadcrumb*` (feature 501) or `Viewers\WebView2*` (feature 476) paths | diff file list contains no path under `QuickFiler/Viewers/`; production edits confined to the three owned `Controllers` files | PASS |
| Forbidden files (`KeyboardHandler.cs`, `IQfcCollectionController.cs`, the nine other `QfcItemController` partials, `QfcCollectionControllerTests.cs`) absent from diff | grep of the diff name list returned no match | PASS |

## 5. File Size (general-code-change.md, 500-line cap)

| File | Base lines | Head lines | Verdict |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/KbdActions.cs` | 146 | 182 | PASS |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2437 | 2437 | Non-blocking (NB-1): pre-existing excess, unchanged size, remediation explicitly out of this feature's permitted scope (spec AC, plan decision D-P6, #468 freeze) |
| `QuickFiler/Controllers/QfcItemController.Navigation.cs` | 228 | 252 | PASS |
| `QuickFiler.Test/Controllers/KbdActionsTests.cs` | 88 | 125 | PASS |
| `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs` | 181 | 272 | PASS |
| `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` | new | 226 | PASS |
| `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs` | 391 | 498 | PASS — Observation OB-3: 2 lines below the cap; the next feature to touch this file must split it |

Line counts measured with `wc -l` against `git show 4f238289:<path>` and the working tree.

## 6. Evidence Location Compliance

- All 85 feature evidence files live under
  `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/<kind>/` (baseline,
  qa-gates, regression-testing, issue-updates, other) — verified from the diff file list.
- Zero diff paths under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or
  `artifacts/coverage/`: `git diff --name-only 4f238289..HEAD | grep -E '^artifacts/'` returned no
  rows.
- `validate_evidence_locations.py` does not exist in this repository (searched; TaskMaster does not
  ship it), so the scan above was performed manually. No violation found.
- EVIDENCE_LOCATION_OVERRIDE_REJECTED: none required; no instruction supplied a non-canonical path.
- Zero `.ps1` files under the evidence tree (matches
  `evidence/qa-gates/p5-t29-evidence-locations.2026-08-27T20-20.md`).

Verdict: PASS.

## 7. Artifact Hygiene (no absolute host paths)

- `evidence/qa-gates/p4-t6/p4-t6-final.trx`: independent scans found 0 case-insensitive
  user-profile path prefixes, 6713 of 6713 `computerName="host"`, and the document parses as
  well-formed XML.
- Recursive case-insensitive scan of the feature folder and the promoted potential entry for the
  account-name and machine-name tokens: 0 hits.

Verdict: PASS.

## 8. Bugfix Workflow (general-code-change.md — defects only)

Fail-before/pass-after evidence exists for all three defects:
`evidence/qa-gates/fail-before-444.2026-08-27T09-45.md`, `fail-before-472.2026-08-27T09-45.md`,
`fail-before-482.2026-08-27T09-45.md` (red state) and `p1-t6-444-green`, `p2-t7-472-green`,
`p3-t8-482-green` (green state). The #444 decision-pin test
(`RegisterAsyncKeyActions_RegistersExactlyOneDownBoundToSelectNextItemAsync`) is correctly recorded
as pass-after-only with no red state expected, because upstream #468 removed the duplicate
registration. Fixes are minimal and targeted; the deeper count-mismatch defect was opened as issue
#644 instead of widening scope, which is the required behavior. Verdict: PASS.

## 9. Promotion Lifecycle

The count-mismatch follow-up was promoted to
`docs/features/potential/promoted/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan.md` and
GitHub issue #644 (commit `12256da4`). Reviewer verification: `gh issue view 644` returned
`{"number":644,"state":"OPEN","title":"Bug: qfc-unregister-navigation-count-mismatch-orphan"}`.
Verdict: PASS (the PR-body recording clause remains deferred; see the feature audit).

## 10. Tonality (tonality.md)

Spec, plan, evidence artifacts, and code comments reviewed: factual, neutral, evidence-first; no
humor, hyperbole, or decorative metaphor found. Verdict: PASS.

## Findings Summary

Blocking: **0**.

Non-blocking:
- NB-1 — `QuickFiler/Controllers/QfcCollectionController.cs` at 2437 lines exceeds the 500-line cap
  (`.claude/rules/general-code-change.md` §File Size Limit). Pre-existing at base, size unchanged by
  this feature (+8/-8 in-place), and remediation is explicitly forbidden to this feature by the epic
  ownership rules. Tracked debt; no action for this branch.

Observations:
- OB-1 — Branch is 10 commits behind the current epic tip (sibling #493 fan-in merged after this
  branch's last merge-up). A final merge-up is required before the integration PR merge; no textual
  overlap is expected (the #493 diff touches UiThread dispatcher test fixtures, not this feature's
  files).
- OB-2 — Pre-existing repository documentation conflict on coverage floors: CLAUDE.md §UT2 states
  >= 80% line / >= 90% new code, while `.claude/rules/general-unit-test.md` and
  `.claude/rules/quality-tiers.md` state >= 85% line / >= 75% branch. The measured figures clear
  every reading. The executor recorded the conflict rather than silently resolving it (spec AC),
  which is the required handling. Not introduced by this feature.
- OB-3 — `QfcItemController.NavigationTests.cs` is at 498 of 500 lines.
- OB-4 — The AC-444-01 verification command ("repository-wide `*.cs` search for the identifier
  `WireUpKeyboardHandler` returns zero hits") returned zero hits at Phase 0 capture
  (`evidence/baseline/p0-t12-upstream-468-verification.2026-08-27T09-45.md`) but returns one hit at
  head: a prose mention inside an XML doc comment in the new test file
  `QfcCollectionControllerNavigationDigitsTests.cs:60`. The identifier exists nowhere as a code
  member; the substantive condition (the method and its duplicate registration are gone) holds.
- OB-5 — `evidence/issue-updates/p5-t25-ac472-10-deferred.2026-08-27T20-16.md` contains an
  internally inconsistent deferral line: `DEFERRED-TO-ORCHESTRATOR: AC-472-10 promotion and issue
  creation are outside this feature's scope per decision D-472-B`, while the same artifact (and the
  branch history, commit `12256da4`) shows promotion and issue creation were completed on this
  branch. Only the PR-body clause is actually outstanding. Wording defect in one evidence artifact;
  no substantive effect.

Remediation required: none. No `remediation-inputs` artifact is produced.
