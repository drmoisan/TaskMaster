# Policy Audit — ribbon-engine-toggle-defects (#735)

- **Timestamp:** 2026-09-03T09-05
- **Branch:** `bug/ribbon-engine-toggle-defects-735`
- **HEAD:** `30e66833e73267327a18e58228f493e8c8e3a4dd`
- **Work mode:** `full-bug` (from `issue.md`); AC source is `spec.md` only; `user-story.md` correctly absent
- **Reviewer scope:** full branch diff against the resolved base branch

---

## Scope Resolution

The caller supplied the anchor `a679cd08...HEAD`. That anchor is **not** correct at the time of this
review, and the reason matters: `a679cd08` is an ancestor of `HEAD`, so the three-dot form collapses
to the two-dot form, and `HEAD` (`30e66833`) is a merge of the item tip `3e45428e` with
`origin/main` at `b13d5b7b`. `b13d5b7b` carries the sibling items #729, #730 and #733. The supplied
anchor therefore yields **184 files**, most of them sibling-owned.

Two anchors were computed and reconciled:

| Anchor | Files | Content |
|---|---|---|
| `a679cd08...HEAD` (as supplied) | 184 | This item plus siblings #729/#730/#733 |
| `a679cd08...3e45428e` (item tip) | **78** | This item only |
| `b13d5b7b..HEAD` (resolved base branch -> HEAD) | **78** | This item only |

The last two are byte-identical in file set and in `458405`-vs-`398670` insertion counts
(`398670` insertions, `332`-vs-`34` deletions -> `34` deletions). The audited scope is therefore the
**78-file footprint**, which is simultaneously (a) this item's own work and (b) the full branch diff
against the resolved base branch `origin/main @ b13d5b7b`. The Scope Invariant is satisfied on its
own terms, not by accepting a caller narrowing.

### Rejected Scope Narrowing

None. The caller's instruction to exclude the sibling footprint is not a narrowing: it coincides
exactly with the full-branch-diff-against-base requirement, because the branch has already merged
the base. The literal anchor string supplied was superseded by the equivalent-and-correct
`b13d5b7b..HEAD`, and the substitution **widens** nothing and **narrows** nothing.

---

## Verdict Summary

| Area | Verdict |
|---|---|
| Policy compliance order applied (CLAUDE.md -> general-code-change -> general-unit-test -> csharp -> tonality) | PASS |
| Prohibited-path avoidance | PASS |
| File size ceiling (500 lines) | PASS |
| Coverage exclusion policy (no new exemption) | PASS |
| C# coverage thresholds | PASS |
| Toolchain order and closure | PASS |
| Evidence location compliance | PASS |
| Evidence sanitisation | PASS |
| Evidence accuracy | PARTIAL (one false reconciliation claim; see NB-3) |

**Blocking findings: 0. Non-blocking findings: 8.**

---

## Coverage Verification

Coverage was verified by inspecting pre-existing artifacts committed by the executor. No coverage
generation was rerun.

Canonical paths `artifacts/csharp/coverage.xml` and `artifacts/pester/powershell-coverage.xml` are
**absent** from the worktree (verified: `ls` on both directories returns "No such file or
directory"). The item instead committed same-session Cobertura documents inside the canonical
feature evidence tree, which is the accepted substitution:

- Baseline: `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/baseline/coverage-baseline.2026-09-02T12-04.cobertura.xml`
- Final: `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/coverage-final.2026-09-02T12-04.cobertura.xml`

Both are Cobertura (`<coverage line-rate=... branch-rate=...>` root), 9 packages, single-run
conversion rather than a multi-document merge.

### Changed languages in the audited footprint

Derived from `git diff --name-only b13d5b7b..HEAD`, **not** from `artifacts/pr_context.summary.txt`
(which is stale — see NB-2):

- **C#** — 12 changed files (6 production/test source, 2 project files, 1 XML, plus test additions). Coverage required.
- **PowerShell** — 0 changed `.ps1` files. Verified by `git diff --name-only b13d5b7b..HEAD` filtered to `*.ps1`, which returns empty.
- **Python** — 0 changed files. **TypeScript** — 0 changed files.

### Repo-wide figures (same-session baseline and final)

| Metric | Baseline | Final | Delta | Floor | Verdict |
|---|---|---|---|---|---|
| Line coverage | 85.3867% | **85.4109%** | +0.0242 pt | >= 85% | PASS |
| Branch coverage | 79.4649% | **79.4984%** | +0.0335 pt | >= 75% | PASS |

Internally consistent: `55225 / 64658 = 0.854109` and `13219 / 16628 = 0.794984`. The commit message
of `3e45428e` quotes both figures; both were re-derived here from the artifact rather than accepted
from prose.

### Per-file figures

| File | Tier | Line | Branch | Floor | Verdict |
|---|---|---|---|---|---|
| `TaskMaster/Ribbon/SpamManagerResetGate.cs` | new | 100% | 100% | >= 90% new | PASS |
| `TaskMaster/Ribbon/EngineTogglePressedStateCache.cs` | new | 94.8718% | 80% | >= 90% new | PASS |
| `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | modified | 100% (was 98.5185%) | 97.3684% (was 93.3333%) | >= 85 / >= 75, no regression | PASS |
| `TaskMaster/Ribbon/RibbonController.Intelligence.cs` | modified | not measured | not measured | n/a | See NB-6 |

`RibbonController` is absent from both Cobertura documents entirely (`grep -n "RibbonController"`
returns no match in the final document). This **confirms** the spec's claim that the residual
`ClearSpamManagerAsync` lines sit inside a pre-existing type-level exemption and that no coverage
credit is asserted for them.

### Coverage verdicts by language

- **C# coverage: PASS** — 85.4109% line / 79.4984% branch repo-wide; both new files above the 90% new-code floor; the one modified measurable file improved on both axes.
- **PowerShell (Pester) coverage: PASS** — vacuously satisfied. Zero `.ps1` files exist in the audited footprint; verified by name-only diff filtered to `*.ps1` returning empty. This is a scope fact, not a measurement.
- **Python coverage: PASS** — zero changed Python files in the audited footprint.
- **TypeScript coverage: PASS** — zero changed TypeScript files in the audited footprint.

---

## Prohibited Paths

The plan and spec forbid four paths. Verified directly with
`git diff --name-only b13d5b7b..HEAD -- <the four paths>`, which returned **empty**:

| Path | Present in diff |
|---|---|
| `TaskMaster/AppGlobals/AppOlObjects.cs` | No |
| `TaskMaster/AppGlobals/NonBlockingDelay.cs` | No |
| `TaskMaster/Ribbon/RibbonViewer.cs` | No |
| `TaskMaster.Test/Ribbon/RibbonViewerEngineCallbackShapeTests.cs` | No |

**PASS.** No cross-item collision with the concurrent parallel work.

---

## Coverage Exclusion Policy

`.claude/rules/general-unit-test.md` makes any production-path coverage exclusion a Blocking
finding. Two checks were run.

1. **No new exemption attribute.** `git diff b13d5b7b..HEAD -- "*.cs" "*.config" "*.runsettings" "*.csproj" | grep "^+.*\[ExcludeFromCodeCoverage"` returns exactly **two** hits, both of which are XML documentation prose, not attribute applications:
   - `TaskMaster/Ribbon/EngineTogglePressedStateCache.cs:29` — `/// This type is deliberately NOT marked <c>[ExcludeFromCodeCoverage]</c>: ...`
   - `TaskMaster/Ribbon/SpamManagerResetGate.cs:40` — same sentence.

   Both files were read in full; neither carries the attribute. **The `SpamManagerResetGate` was
   deliberately given no exemption, as the spec required. Confirmed.**

2. **Pre-existing exemption not widened.** `TaskMaster/Ribbon/RibbonController.cs:36-37` carries
   `[ExcludeFromCodeCoverage]` on `public partial class RibbonController`. Verified pre-existing:
   `git show a679cd08:TaskMaster/Ribbon/RibbonController.cs | grep -n ExcludeFromCodeCoverage`
   returns the same line 36. The attribute was neither added nor moved by this change, and no
   `coverage.config` / `.runsettings` exclusion entry was touched (no such file appears in the
   78-file footprint).

**PASS.** See NB-6 for the standing policy-text conflict this exposes, which is not attributable to
this change.

---

## Evidence Location Compliance

All evidence is under the canonical `<FEATURE>/evidence/<kind>/` tree: `evidence/baseline/`,
`evidence/qa-gates/`, `evidence/regression-testing/`, `evidence/issue-updates/`, `evidence/other/`.

`git diff --name-only b13d5b7b..HEAD -- "artifacts/baselines/*" "artifacts/qa/*" "artifacts/coverage/*" "artifacts/evidence/*"`
returns **empty**. No non-canonical evidence path was written.

`validate_evidence_locations.py` was **not run**: it does not exist in this repository.
`find <worktree> -name "validate_evidence_locations*"` returns no result. The check above was
performed manually with the equivalent path predicates; this substitution is recorded rather than
claimed as a script pass.

**PASS.**

---

## Evidence Sanitisation

The executor reported sanitising 466 account-token and 232 machine-token occurrences pre-commit.
Spot-checked independently:

| Check | Scope | Result |
|---|---|---|
| Account / host path tokens | `grep -rlI "DanMoisan\|danmoisan\|C:\\Users\\\|/c/Users/"` across the entire feature folder | **No match** |
| TRX machine name | `evidence/qa-gates/p4-t3/p4-t3.trx` | `computerName="REDACTED-MACHINE"` |
| TRX run user | same file | `runUser="REDACTED-MACHINE\REDACTED-ACCOUNT"` |
| Cobertura source root | `coverage-final...cobertura.xml` lines 3-5 | `<source>.</source>` — relative, no host path |

**PASS.** No host path or account name leaked into the branch. Note that sanitisation applied here
was performed **before** commit, so unlike some prior items there is no residual blob in history for
these artifacts.

---

## File Size Ceiling (500 lines)

Measured with `git grep -c ""` at both the merge base and the item tip.

| File | Base | Head | Verdict |
|---|---|---|---|
| `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | 389 | 415 | PASS |
| `TaskMaster/Ribbon/RibbonController.Intelligence.cs` | 412 | 444 | PASS |
| `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` | 323 | **496** | PASS (see NB-4) |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` | 459 | 459 | PASS |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` | new | 277 | PASS |
| `TaskMaster.Test/Ribbon/SpamManagerResetGateTests.cs` | new | 326 | PASS |
| `TaskMaster.Test/Ribbon/EngineTogglePressedStateCacheTests.cs` | new | 213 | PASS |
| `TaskMaster/Ribbon/SpamManagerResetGate.cs` | new | 141 | PASS |
| `TaskMaster/Ribbon/EngineTogglePressedStateCache.cs` | new | 157 | PASS |

### Size contingency (P4-T3 branch B) — independently verified

The contingency was challenged directly rather than accepted from the commit message. Measuring the
coordinator at the commit **immediately before** the extraction:

```
git grep -c "" a68c8598 -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs
-> a68c8598:TaskMaster/Ribbon/EngineToggleStateCoordinator.cs:515
```

**515 lines — genuinely over the 500 ceiling.** The extraction was therefore a policy-mandated
remedy, not an opportunistic refactor. The plan authorised it in advance at two places
(`plan.2026-09-02T12-04.md:67` and `:302`), and the plan's own scope gate explicitly admits "paths
created by the P4-T3 branch B if that branch was taken" (`:326`). Documentation was **not** trimmed
to fit: the coordinator's XML doc comments were relocated into the new file's `<remarks>` block
rather than deleted, and the new file carries 33 lines of type-level documentation.

**PASS.** The two added paths are plan-authorised and evidence-justified.

---

## Toolchain Order and Closure

`evidence/qa-gates/toolchain-loop-closure.2026-09-02T12-04.md` and the supporting gate files record
one clean pass in the mandated order: CSharpier format -> CSharpier check -> analyzer rebuild ->
nullable rebuild -> vstest with coverage.

- Final coverage-enabled run: `EXIT_CODE: 0`, `Total tests: 6982`, `Passed: 6982`, zero failures.
- Ribbon subset at P4-T3: `<Counters total="134" executed="134" passed="134" failed="0" .../>`.
- Analyzer and nullable rebuilds recorded at the baseline 5 warnings / 0 errors.

Per the caller, `origin/main` subsequently gained `Directory.Build.props`
(`RxUseUnsupportedPackagesConfig=true`), and the caller re-ran both builds against the merged tree
with exit 0 and 0 warnings. This reviewer has no build tools and did not re-run them; that
statement is accepted as caller-supplied and is marked as such rather than independently verified.

**PASS**, with the merged-tree build result recorded as caller-attested.

---

## Findings

### Blocking

**None.**

### Non-blocking

**NB-1 — Vestigial `string.Format` with a constant format string and zero arguments.**
`TaskMaster/Ribbon/SpamManagerResetGate.cs:132-139`. `BuildNotReadyMessage` calls
`string.Format(CultureInfo.CurrentCulture, "<constant literal>")` with no format arguments. The call
performs no substitution. It appears to be a shape copied from the precedent
`EngineGatedCommandRunner.BuildNotReadyMessage(string controlId)` (`:117`), which genuinely formats
a control id; this gate deliberately takes no argument, so the format call is inert. It also
introduces a latent `FormatException` if a literal `{` is ever added to the message text.
*Rule:* `.claude/rules/general-code-change.md`, Design Principles §1 (Simplicity first).
*Remedy:* return the string literal directly, or make it a `private const string`.

**NB-2 — Committed PR context artifacts describe a different issue.**
`artifacts/pr_context.summary.txt` at HEAD reports `Head ref (resolved): bug/ci-build-infra-debt-730
@ ff2106fe9da48a4afec7d7cee023b47d6655b938`, `Base ref (resolved): origin/main @ 8be5a6aa`, and an
author-asserted close list of `#395, #561, #562, #563, #569, #570, #730`. It is issue **#730**'s
context, not #735's. The appendix is correspondingly stale. This item did **not** author the file —
it is tracked in the repository and was inherited through the `origin/main` merge — so this is not
an execution defect, but the PR raised from this branch would carry the wrong context.
*Impact:* the coverage-enforcement hook derives changed languages from this summary rather than from
`git diff`, so it will see #730's PowerShell footprint rather than this item's C#-only footprint.
This audit derived changed languages from `git diff` instead and records the deviation here.
*Remedy:* regenerate both artifacts against `bug/ribbon-engine-toggle-defects-735` before opening the PR.

**NB-3 — A false reconciliation claim in the coverage-run evidence, and the same error in the commit message.**
`evidence/qa-gates/vstest-coverage-run.2026-09-02T12-04.md:25-34`. The file counts the new tests as
"2 + 9 + 6 + 9 = 26", describing the cache fixture as **9 cache tests**. The fixture actually
contains **10**:
`grep -c "\[TestMethod\]" TaskMaster.Test/Ribbon/EngineTogglePressedStateCacheTests.cs` returns `10`
(NextSequence x2, TryGetActive x3, TryApplyState x5). The correct total is `2 + 9 + 6 + 10 = 27`,
which matches the observed population delta `6955 -> 6982` exactly and the ribbon-subset delta
`107 -> 134` exactly.

Rather than recount, the evidence file rationalises the leftover test with a specific factual claim:
that `GetPressed_WhenEnginesAccessorReturnsNull_ReturnsFalseAndStartsNothing` is a pre-existing test
"whose fully qualified name did not match the P0-T8 baseline filter's class-name selection". That
claim is **false**. That test is present in the P0-T8 baseline TRX
(`grep -c` on `evidence/baseline/p0-t8/p0-t8.trx` returns `3` occurrences — result, definition and
entry), and the baseline TRX contains `107` tests, which is precisely the figure the narrative
claims was short by one. The commit message of `3e45428e` repeats the underlying error ("Nine new
unit tests cover the extracted class").

*Impact on delivery:* none. The substantive facts are all correct and independently confirmed — 27
new tests were added, all 6982 pass, no test was removed, and none was skipped. The defect is
confined to the evidence narrative and one commit-message sentence.
*Rule:* `.claude/rules/tonality.md`, Evidence-First Wording ("If something was verified, say it was
verified and state how"). An unverified explanation was asserted as fact to close an arithmetic gap.
*Remedy:* correct the count to 10 and delete the filter-mismatch paragraph.

**NB-4 — `RibbonExplorerXmlTests.cs` has 4 lines of headroom.**
`TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` is 496 lines against the 500-line ceiling in
`.claude/rules/general-code-change.md`. Compliant now; the next addition to this fixture will
breach. Worth noting because the same ceiling already forced the P4-T3 extraction on the production
side in this very item.

**NB-5 — Freshness ordering is by read-initiation, not by engine-sampling instant.**
`TaskMaster/Ribbon/EngineToggleStateCoordinator.cs:228` and `:318`. The monotonic ticket is taken
immediately before `EngineActiveAsync` is *invoked*. It therefore orders the *call*, not the moment
the engine internally samples its activation state. If `EngineActiveAsync` queues work internally, a
later-ticketed call could in principle observe an earlier state. This is a design limitation, not a
deviation: `spec.md:83` defines freshness explicitly as "when its underlying activation read
*began*", and the implementation matches that definition exactly. Recorded so the limitation is
visible rather than implied.

**NB-6 — Standing conflict between the coverage-exclusion rule text and the ratified COM/VSTO exemption.**
`.claude/rules/general-unit-test.md` states "No production file may be excluded from coverage
measurement" and directs feature-review agents to treat a production-path exclusion as Blocking.
`TaskMaster/Ribbon/RibbonController.cs:36` excludes a production type via
`[ExcludeFromCodeCoverage]`, which removes `RibbonController.Intelligence.cs` — a file modified by
this change — from measurement entirely. CLAUDE.md, which is first in the policy authority order,
expressly ratifies this attribute-based COM/VSTO/WinForms exemption. Applying CLAUDE.md's authority,
this is **not** Blocking, and it is in any case pre-existing and not widened here. Recorded as an
unreconciled policy-text conflict for maintainer attention, consistent with the previously observed
80/90-versus-85/75 divergence between CLAUDE.md and `.claude/rules`.

**NB-7 — Coverage figures are not reproducible against the current coverage script.**
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` and its helper modules changed on `origin/main` after
this item's coverage gates ran (sibling item #733, "correct Cobertura merge, MSTest discovery, and
closure-filter docs"). The figures above were produced by the pre-#733 script.

*Reviewer judgment, since the caller asked for one: the figures are trustworthy for the conclusions
drawn from them.* Three reasons. First, baseline and final were produced by the **same** script
version roughly 37 minutes apart in one session (Cobertura timestamps `1788412691` and
`1788414908`), so the delta and the no-regression conclusion are internally valid regardless of any
systematic error in the absolute figure. Second, the per-class figures that carry the new-code and
modified-file verdicts are read from single `<class>` elements and do not depend on cross-document
merge arithmetic at all. Third, the final document is a single-run conversion over 9 packages, not a
multi-document merge, so the specific merge path #733 repaired is not on the code path that produced
it. The residual uncertainty is confined to the third and fourth decimal place of the repo-wide
aggregate, which clears its floor by 0.41 points. No re-measurement is required.

**NB-8 — Four `.claude/agent-memory/**` files sit in the branch footprint outside the plan's write set.**
`.claude/agent-memory/atomic-planner/MEMORY.md`,
`.claude/agent-memory/atomic-planner/project_735_evidence_content_sanitization_seams.md`,
`.claude/agent-memory/task-researcher/MEMORY.md`,
`.claude/agent-memory/task-researcher/project_ribbon_engine_toggle_defects_735.md`.
Attribution checked before reporting, per the caller's caution: `git log --oneline` on those paths
shows they entered at `044551f0` ("prep(bug-735): promote, research, spec, and plan"), dated
2026-09-02 13:01, which precedes the earliest implementation commit `a3bfb865` (2026-09-03 01:31) by
roughly twelve hours. **Not attributable to this execution run.** Recorded only because they are
part of the branch footprint a PR would carry.

---

## Policy Compliance Order Applied

1. `CLAUDE.md` — C# toolchain order, MSTest/Moq/FluentAssertions mandate, COM/VSTO coverage exemption authority. Applied.
2. `.claude/rules/general-code-change.md` — design principles, 500-line ceiling, error handling, I/O boundaries. Applied.
3. `.claude/rules/general-unit-test.md` — determinism, no temp files, coverage floors, coverage exclusion policy, test file location. Applied.
4. `.claude/rules/quality-tiers.md` — uniform 85% line / 75% branch. Applied.
5. `.claude/rules/csharp.md` — read; C#-specific gates folded into the toolchain assessment above.
6. `.claude/rules/tonality.md` — applied to this artifact and used as the basis for NB-3.

No policy document was modified. No source, test, plan or spec file was modified by this review.
