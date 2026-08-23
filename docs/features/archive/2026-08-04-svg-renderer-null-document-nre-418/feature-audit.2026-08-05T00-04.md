# Feature Audit — svg-renderer-null-document-nre (Issue #418)

- Artifact timestamp: `2026-08-05T00-04`
- Review cycle: reaudit 3 (remediation cycle 2 verification)

## Scope and Baseline

| Item | Value |
|---|---|
| Base branch | `main` |
| Base ref (resolved) | `origin/main` @ `ce0c91e686bf7e060aaab6f185ee6883269e4fd4` |
| Merge-base SHA | `ce0c91e686bf7e060aaab6f185ee6883269e4fd4` (recomputed by reviewer; matched) |
| Head | `bug/svg-renderer-null-document-nre-418` @ `69e675d014d001b2e17ee15c3279ce6a5ba46609` |
| Work mode | `minor-audit` (marker read from `issue.md:12`) |
| AC source (per work mode) | `issue.md`, section `## Acceptance Criteria` — the only authoritative source under `minor-audit` |
| Changed files | 152 (6 `.cs`, 5 build-configuration, 141 `.md`) |
| Commits in range | 12, of which 4 are functional and 8 documentation |

Scope is the full branch-vs-base diff. It was derived by the reviewer from
`git diff --numstat ce0c91e6..69e675d0`, not from any plan, task, or caller-supplied subset, and not
from the two-file remediation delta. The caller explicitly directed full-scope derivation and asserted
that none of its factual notes constrained scope or findings; that is consistent with the SKILL contract
and no narrowing was attempted or applied.

Prior cycles produced the `2026-08-04T20-25` and `2026-08-04T22-28` artifact sets. This cycle evaluates
the same eleven criteria against the head produced by remediation cycle 2, whose functional commit is
`69e675d0`.

## Acceptance Criteria Inventory

Eleven criteria, AC-1 through AC-11, all in markdown checkbox form under the required
`## Acceptance Criteria` heading in `issue.md`. AC-1 through AC-6 address the confirmed error-handling
defect and are unconditional; AC-7 and AC-8 address the underlying parse/binding failure; AC-9 and AC-10
address test-project repair; AC-11 is the human designer-load verification.

| ID | Criterion (abbreviated) | Source line | Checkbox state at review |
|---|---|---|---|
| AC-1 | Failing regression test exists first | `issue.md:74` | `[x]` |
| AC-2 | No silent exception swallow | `issue.md:75` | `[x]` |
| AC-3 | Parse failure degrades visibly instead of throwing an NRE | `issue.md:78` | `[x]` |
| AC-4 | A fail-fast API exists; null-tolerant call sites keep their contract | `issue.md:81` | `[x]` |
| AC-5 | Coverage on changed code | `issue.md:82` | `[x]` |
| AC-6 | Toolchain passes in a single clean pass | `issue.md:95` | `[x]` |
| AC-7 | Underlying failure identified in writing | `issue.md:100` | `[x]` |
| AC-8 | `AssemblyResolve` fallback resolves from the assembly's own directory | `issue.md:101` | `[x]` |
| AC-9 | `SVGControl.Test` builds and runs | `issue.md:104` | `[x]` |
| AC-10 | Incorrect ExCSS redirect in the test config is corrected | `issue.md:107` | `[x]` |
| AC-11 | Designer load verified by the documented human step | `issue.md:112` | `[ ]` |

No criterion was added, removed, or reworded by any agent this cycle. The `[P2-T11]` evidence-note
amendment appended to AC-10 leaves the criterion text and its `[x]` state unchanged, which the reviewer
verified against the diff.

## Acceptance Criteria Evaluation

| ID | Verdict | Evidence and reasoning |
|---|---|---|
| AC-1 | **PASS** | Unchanged. Fail-before and pass-after are both recorded: `evidence/regression-testing/ac1-fail-before.2026-08-04T14-36.md` shows 4 failures, each a `NullReferenceException` at `SvgRenderer.cs:133`; `ac1-pass-after.2026-08-04T14-36.md` shows the same four tests passing with unchanged assertions. The reviewer observed all four passing by name in its own standalone run at this head: `Constructor_WithMalformedBytesAndNoMargin_...`, `Constructor_WithMalformedBytesAndMargin_...`, `Constructor_WithEmptyBytesAndNoMargin_...`, `Constructor_WithEmptyBytesAndMargin_...`. Tests are deterministic MSTest in `SVGControl.Test` with no temporary files or external services. |
| AC-2 | **PASS** | Unchanged. Reviewer re-verified by inspection of `SVGControl/SvgAssemblyResolver.cs` at this head: zero bare `catch` blocks remain across the changed files. Four catch sites, all declaring `Exception ex` and all logging. The parse boundary in `SvgRenderer.TryGetSvgDocument` logs via `logger.Error` plus `Trace.TraceError` and returns `false` with the exception in `out error`, a result the caller must inspect. The three resolver sites (`SvgAssemblyResolver.cs:100,132,143`) use `Trace.TraceWarning` with the re-entrancy rationale stated in-code at lines 98-99 and 140-142. The relocation and the added containment catch are correctly disclosed in the criterion's 2026-08-05 amendment. The disclosed pre-guard residual at lines 50-54 is recorded as a Low code-review finding, not an AC failure. |
| AC-3 | **PASS** | Unchanged. Both byte-array constructors call `TryGetSvgDocument`, assign `_doc` from the out parameter, and on failure set `_original = Size.Empty` after logging through both channels; neither contains an unguarded `_doc.Draw()` nor a `throw` on the failure path (`SvgRenderer.cs:30-70`, re-read at this head). The dual-channel requirement was empirically confirmed in cycle 2, when a real `FileNotFoundException` for ExCSS produced a named, diagnosable `Trace` message and no `NullReferenceException`. That corroboration stands; the underlying bind failure it exploited is now itself fixed. |
| AC-4 | **PASS** | Unchanged. `SvgRenderer.cs` declares `public static bool TryGetSvgDocument(byte[], out SvgDocument?, out Exception?)`, `public static SvgDocument GetSvgDocumentOrThrow(byte[])` whose `InvalidOperationException.InnerException` is the original parser exception, and retains the tolerant `public static SvgDocument? GetSvgDocument(byte[])` with no `try`/`catch` of its own. `SVGControl/SvgImageSelector.cs` is absent from the branch diff, so all six named tolerant consumers keep their contracts. The reviewer observed the corresponding tests passing by name: `GetSvgDocumentOrThrow_WithMalformedBytes_ThrowsWithTheParserExceptionInner`, `TryGetSvgDocument_WithNullPayload_ThrowsArgumentNullException`, `Render_WithNullDocument_ReturnsNull`, `DocumentSetter_AssignedNull_SucceedsAndLeavesDocumentNull`. |
| AC-5 | **PASS** | The criterion's requirements are member-scoped and all remain met. Every member this feature added or modified measures 100% line coverage: both byte-array constructors 17/17 and 18/18, `OpenFromBytes` 5/5, both `TryGetSvgDocument` overloads 23/23 and 3/3, `GetSvgDocumentOrThrow` 6/6, `GetSvgDocument` 4/4, `DescribeFailure` 5/5, `.cctor` 6/6, `SvgAssemblyResolver.Install` 6/6, and all of `SvgAssemblyProbe` at 102/102 line and 92/92 branch. All clear the >= 90% new-member threshold. No changed line regressed — this cycle modified no `.cs` file at all, and across the branch the entire 82-line residual in `SvgRenderer.cs` lies in six pre-existing members the fix did not touch. Success, parse-failure, and argument-boundary paths are all covered. The separate **file-level** policy floors are not met and are recorded as policy gaps G-1 and G-9, which lie outside this criterion's text; G-9 is surfaced for a maintainer decision. |
| AC-6 | **PASS** | Format independently reproduced by the reviewer: `dotnet tool run csharpier check .` → exit 0, 1467 files checked, 0 needing formatting. Analyzer build exit 0, 0 errors, 5 warnings, **0 added diagnostics**; the single removal (`CS2002` in `UtilitiesCS.Test`) is `CoreCompile`-gated in a project that did not recompile and is correctly dispositioned non-regressive. Type check: the mandated solution-wide command returns exit 0 vacuously with 0 of 18 `CoreCompile` targets, which the executor disclosed rather than presenting as a pass; the two forced per-project rebuilds of `SVGControl` and `SVGControl.Test` both returned exit 0 with **0 diagnostics**. This cycle's forced-rebuild evidence is cleaner than cycle 2's, because no `.cs` file changed, so `UtilitiesCS` was not dragged in through its `ProjectReference` and the result is uncontaminated. Tests: 6150/6150 across nine assemblies, plus the reviewer's own standalone 75/75. Single pass, no loop restart. The gate's structural vacuity is recorded as policy gap G-3, a repository-level concern rather than a defect in this delivery. |
| AC-7 | **PASS** | The criterion requires a written identification, and `research/2026-08-04T15-05-svg-renderer-null-document-research.md` (607 lines) delivers all three required elements: the exception, the reproducing hosts, and whether the fallback is reached. Its central claim was empirically corroborated in cycle 2, when the reviewer observed the exception chain `FileNotFoundException` for `ExCSS, Version=4.3.2.0` with an inner `FileNotFoundException` for `ExCSS, Version=4.2.3.0`, confirming that `Svg 3.4.8` binds `ExCSS 4.2.3.0` exactly as the artifact concluded. The cycle-2 caveat — that the vstest corroboration was conditional on assembly ordering — is **now removed**: the ordering dependency is fixed, so the corroboration no longer rests on which assembly ran first. The designer-host observation remains tracked as human requirement H-2. |
| AC-8 | **PASS** | Unchanged. Re-read at this head: `SvgAssemblyResolver.ResolveByNameAndKey` runs strategy 3 after the already-loaded scan and the `Assembly.Load` attempt, iterating `SvgAssemblyProbe.GetProbeDirectories(self.Location, self.CodeBase, AppDomain.CurrentDomain.BaseDirectory)` at lines 109-114 and gating every `Assembly.LoadFrom` result through `PublicKeyTokensEqual` at line 127. The `_resolving.Add`/`Remove` re-entrance guard still encloses strategies 2 and 3 (lines 78 and 151) and the method still ends `return null;` at line 154. Empty-`Location` tolerance is implemented in `SvgAssemblyProbe` and covered by test. The public-key-token requirement is verified by measurement: `PublicKeyTokensEqual` measures 15/15 line and 18/18 branch. All eighteen `SvgAssemblyProbeDirectoryTests` pass in the reviewer's standalone run. |
| AC-9 | **PASS** | Unchanged and strengthened. `SVGControl.Test` is a solution member: `TaskMaster.sln:42-43` declares the project and lines 264-276 add its twelve configuration mappings. The `EnsureNuGetPackageBuildImports` `<Error>` does not fire. The project compiles and its tests execute under `vstest.console.exe` — the reviewer ran the assembly directly at this head and observed exit 0 with 75 tests discovered and executed. The amendment correctly discloses that the five package pins named in the original text were superseded by the rebase onto `ce0c91e6` (PR #419) and records the delivered versions. |
| AC-10 | **PASS** (upgraded from PARTIAL) | The redirect **value** was already correct: `SVGControl.Test/app.config:23` reads `oldVersion="0.0.0.0-4.3.2.0" newVersion="4.3.2.0"`, replacing the `4.2.4.0` target that existed nowhere in the repository and matching both `SVGControl/app.config` and the deployed `packages/ExCSS.4.3.2`. Cycle 2 downgraded this to PARTIAL because the criterion's **stated objective** — "so the test host can resolve ExCSS through the binding redirect rather than depending on the `AssemblyResolve` fallback to mask it" — was unachievable: `ExCSS.dll` was absent from `SVGControl.Test/bin/Debug`, and a redirect cannot resolve an assembly that is not on the probing path. Commit `69e675d0` supplies the missing assembly via an explicit `<Reference>` plus `packages.config` entry. Reviewer verification, independent of executor evidence: `ExCSS.dll` is present in `SVGControl.Test/bin/Debug` at 368,128 bytes; the reference identity is byte-identical to the three sibling production references; and the reviewer's own standalone run returns **75/75/0** against 75/69/**6** before the fix. The objective is achieved and the criterion is restored to PASS. |
| AC-11 | **FAIL** | Undelivered. The runbook `runbooks/verify-winforms-designer-load.runbook.md` (283 lines) exists and is complete, but has not been executed, so no designer-load evidence capture exists and `issue.md:112` remains `[ ]`. The reviewer verified the tracking directly by reading `artifacts/orchestration/orchestrator-state.json`: human-interaction requirements H-1 (`satisfies: AC-11`) and H-2 (`satisfies: AC-7`) are both present with `response: "exception"` and a `runbook_path` that resolves to the existing runbook, satisfying the `.claude/rules/orchestrator-state.md` invariant that an `exception` response carry a non-empty `runbook_path`. **Not remediable by any agent:** opening a form in the legacy in-process Visual Studio WinForms designer has no unattended automation surface. Requires a human operator session or an explicit maintainer waiver. |

### Cycle-over-cycle movement

| ID | Cycle 1 | Cycle 2 | Cycle 3 | Note |
|---|---|---|---|---|
| AC-1 .. AC-4 | PASS | PASS | PASS | unchanged |
| AC-5 | PASS | PASS | PASS | member coverage byte-identical; no `.cs` file changed this cycle |
| AC-6 | PASS | PASS | PASS | forced-rebuild evidence is cleaner this cycle — 0 diagnostics, uncontaminated by `UtilitiesCS` |
| AC-7 | PASS | PASS | PASS | the cycle-2 ordering caveat on its vstest corroboration is now removed |
| AC-8 | PASS | PASS | PASS | unchanged |
| AC-9 | PASS | PASS | PASS | unchanged; now additionally proven by a standalone execution |
| AC-10 | PASS | PARTIAL | **PASS** | **upgraded.** The stated objective is now achievable; verified by reviewer-executed 75/75 |
| AC-11 | FAIL | FAIL | FAIL | unchanged; human-only, ratified exception |

## Summary

**Ten of eleven acceptance criteria are satisfied. One is unmet.**

AC-10 is **restored to PASS** this cycle. It was downgraded to PARTIAL in cycle 2 not because the
delivered redirect value was wrong, but because the assembly the redirect names was never deployed, so
the criterion's stated objective could not be reached. Commit `69e675d0` deploys it. The reviewer
verified the outcome by executing the discriminating test shape directly rather than by reading the
executor's evidence, and observed 75 passed of 75 with exit 0.

AC-11 remains **FAIL** and is the sole unmet criterion. It requires a human to open
`UtilitiesCS/Dialogs/MyBoxViewer.cs` in the Visual Studio WinForms designer and confirm the form loads
without a `NullReferenceException`. It is correctly registered as a ratified human-interaction
exception with a complete runbook. No agent can execute it, and no further remediation cycle can close
it.

**Feature verdict: PARTIAL. Blocking count 1, changed from 2 at cycle 2.**

The cycle-2 blocker relating to test order-dependence is closed and verified. The remaining blocker is
AC-11. One additional item is surfaced for the maintainer without being routed to remediation: policy
gap G-9, the file-level coverage floor on `SVGControl/SvgAssemblyResolver.cs` at 61.6279%, whose entire
shortfall is a single CLR-invoked `AssemblyResolve` handler carrying a ratified
`COVERAGE_MEMBER_UNREACHABLE` exception. That file exists only because the resolver was extracted first
to relieve `SvgRenderer.cs` at 497 of its 500-line limit; absent the extraction, the same lines would
have counted against an already-existing file and no new-file threshold would have applied. Further
agent-side remediation would not move the figure without a new host-level seam or a ratified exemption.

**Recommendation: no further remediation cycle.** No agent-actionable blocking finding remains. The two
open items are both maintainer decisions — execute the AC-11 runbook (or waive it), and adjudicate G-9.

## Acceptance Criteria Check-off

Per `acceptance-criteria-tracking`, criteria evaluated PASS are checked off in the authoritative source
file; criteria evaluated PARTIAL, FAIL, or UNVERIFIED are left unchecked.

| ID | Verdict | Required state | State in `issue.md` | Action taken |
|---|---|---|---|---|
| AC-1 | PASS | `[x]` | `[x]` | none needed |
| AC-2 | PASS | `[x]` | `[x]` | none needed |
| AC-3 | PASS | `[x]` | `[x]` | none needed |
| AC-4 | PASS | `[x]` | `[x]` | none needed |
| AC-5 | PASS | `[x]` | `[x]` | none needed |
| AC-6 | PASS | `[x]` | `[x]` | none needed |
| AC-7 | PASS | `[x]` | `[x]` | none needed |
| AC-8 | PASS | `[x]` | `[x]` | none needed |
| AC-9 | PASS | `[x]` | `[x]` | none needed |
| AC-10 | PASS | `[x]` | `[x]` | none needed — already `[x]`; this cycle upgrades the reviewer verdict from PARTIAL to PASS, and the existing checkbox is now correct |
| AC-11 | FAIL | `[ ]` | `[ ]` | none needed — correctly left unchecked |

**No checkbox required modification.** Every PASS criterion was already `[x]` and the single FAIL
criterion was already `[ ]`. The source file's checkbox state is fully consistent with this cycle's
evaluation, so the reviewer wrote nothing to `issue.md`.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md`, section `## Acceptance Criteria`
- Total AC items: 11
- Checked off (delivered): 10
- Remaining (unchecked): 1
- Items remaining: **AC-11 — Designer load verified by the documented human step.** The runbook at `runbooks/verify-winforms-designer-load.runbook.md` must be executed in a human Visual Studio session, or the criterion waived by the maintainer.
