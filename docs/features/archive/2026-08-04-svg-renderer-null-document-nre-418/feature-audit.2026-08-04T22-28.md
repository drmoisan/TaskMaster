# Feature Audit — svg-renderer-null-document-nre (Issue #418)

- Audit timestamp: 2026-08-04T22-28
- Cycle: 2 (re-audit after remediation cycle 1)
- Companion artifacts: `policy-audit.2026-08-04T22-28.md`, `code-review.2026-08-04T22-28.md`

## Scope and Baseline

| Item | Value |
|---|---|
| Base branch (resolved) | `main` → `origin/main` @ `ce0c91e686bf7e060aaab6f185ee6883269e4fd4` |
| Merge-base SHA | `ce0c91e686bf7e060aaab6f185ee6883269e4fd4` (independently recomputed) |
| Head | `bug/svg-renderer-null-document-nre-418` @ `a62391f719c6d5ecc3d80115916c95d1966ca514` |
| Prior cycle head | `ea106111a6daf7e05f8a804ac00b4a713598962a` |
| Diff range | `ce0c91e6...a62391f7` (three-dot, merge-base) |
| Work mode | `minor-audit` (marker at `issue.md:12`) |
| Acceptance-criteria source | `issue.md` section `## Acceptance Criteria` — the sole authoritative source under `minor-audit` |
| `spec.md` / `user-story.md` | Neither exists in the feature folder, which is correct for `minor-audit` |

Scope is the full feature-vs-base diff: 83 changed files comprising 6 C# source files, 5 C# project
and binding-configuration files, and 72 documentation and agent-memory files. Scope was determined
from the branch diff, not from the remediation delta. The caller explicitly directed the full scope
and attempted no narrowing.

One commit landed since cycle 1: `a62391f7`, executing the 40-task remediation plan
`remediation-plan.2026-08-05T01-50.md` for items R-2 through R-6. R-1 (AC-11) was deliberately not
attempted because no agent can execute a Visual Studio WinForms designer session.

## Acceptance Criteria Inventory

Eleven criteria, AC-1 through AC-11, all in markdown checkbox form under the required
`## Acceptance Criteria` heading. AC-1 through AC-6 address the confirmed error-handling defect and
are unconditional; AC-7 and AC-8 address the underlying parse/binding failure; AC-9 and AC-10 address
test-project repair; AC-11 is the human designer-load verification.

| ID | Subject | Source line | Checkbox state at audit |
|---|---|---|---|
| AC-1 | Failing regression test exists first | `issue.md:74` | `[x]` |
| AC-2 | No silent exception swallow | `issue.md:75` | `[x]` |
| AC-3 | Parse failure degrades visibly, never an NRE | `issue.md:78` | `[x]` |
| AC-4 | Fail-fast API exists; null-tolerant call sites keep their contract | `issue.md:81` | `[x]` |
| AC-5 | Coverage on changed code | `issue.md:82` | `[x]` |
| AC-6 | Toolchain passes in a single clean pass | `issue.md:95` | `[x]` |
| AC-7 | Underlying failure identified in writing | `issue.md:100` | `[x]` |
| AC-8 | `AssemblyResolve` fallback resolves from the assembly's own directory | `issue.md:101` | `[x]` |
| AC-9 | `SVGControl.Test` builds and runs | `issue.md:104` | `[x]` |
| AC-10 | Incorrect ExCSS redirect in the test config is corrected | `issue.md:107` | `[x]` |
| AC-11 | Designer load verified by the documented human step | `issue.md:110` | `[ ]` |

The `## Proposed Fix / Validation Ideas` and `## Next Step` sections also contain checkboxes. Under
`minor-audit` these are **not** acceptance criteria and are excluded from this evaluation, per
`acceptance-criteria-tracking`.

## Acceptance Criteria Evaluation

| ID | Verdict | Basis |
|---|---|---|
| AC-1 | **PASS** | Fail-before / pass-after both recorded. `evidence/regression-testing/ac1-fail-before.2026-08-04T14-36.md` shows 4 failures, each an `NullReferenceException` at `SvgRenderer.cs:133`; `ac1-pass-after.2026-08-04T14-36.md` shows the same four tests passing with unchanged assertions. Tests are deterministic MSTest in `SVGControl.Test` and use no temporary files or external services. |
| AC-2 | **PASS** | Reviewer-verified by inspection of all changed files: zero bare `catch` blocks remain. Four catch sites, all declaring `Exception ex` and all logging. `SvgRenderer.cs:302` logs via `logger.Error` plus `Trace.TraceError` and returns `false` with the exception in `out error`, a result the caller must inspect. The three resolver sites (`SvgAssemblyResolver.cs:100,132,143`) use `Trace.TraceWarning` with a documented re-entrancy rationale for not using `log4net` inside an `AssemblyResolve` handler. The relocation and the added third catch are both correctly disclosed in the criterion's 2026-08-05 amendment. |
| AC-3 | **PASS** | Verified in code and **directly observed at runtime**. Both byte-array constructors call `TryGetSvgDocument`, assign `_doc` from the out parameter, and on failure set `_original = Size.Empty` after logging; neither contains an unguarded `_doc.Draw()` nor a `throw` on the failure path (`SvgRenderer.cs:30-70`). The dual-channel requirement is empirically confirmed: the reviewer's isolated test run emitted `SvgRenderer could not parse the SVG payload: System.IO.FileNotFoundException: Could not load file or assembly 'ExCSS, Version=4.3.2.0 ...'` on the `Trace` channel, captured in the vstest `Debug Trace` section. A real bind failure therefore produced a named, diagnosable message and no `NullReferenceException` — exactly the behavior the criterion requires. |
| AC-4 | **PASS** | `SvgRenderer.cs` declares `public static bool TryGetSvgDocument(byte[], out SvgDocument?, out Exception?)` (line 319), `public static SvgDocument GetSvgDocumentOrThrow(byte[])` (line 332, whose `InvalidOperationException` carries the original parser exception as `InnerException`), and retains the tolerant `public static SvgDocument? GetSvgDocument(byte[])` (line 345) with no `try`/`catch` of its own. `SVGControl/SvgImageSelector.cs` is absent from the branch diff, so all six named tolerant consumers keep their contracts unchanged. Argument boundaries guarded by `ArgumentNullException` at lines 284-287. The `internal class SvgRenderer` surface-scope note in the criterion is accurate and is not a defect. |
| AC-5 | **PASS** | The criterion's requirements are member-scoped and all are met. Every member this feature added or modified measures 100% line coverage: both byte-array constructors 17/17 and 18/18, `OpenFromBytes` 5/5, both `TryGetSvgDocument` overloads 23/23 and 3/3, `GetSvgDocumentOrThrow` 6/6, `GetSvgDocument` 4/4, `DescribeFailure` 5/5, `.cctor` 6/6, `SvgAssemblyResolver.Install` 6/6, and all of `SvgAssemblyProbe` at 102/102 line and 92/92 branch. All clear the >= 90% new-member threshold. No changed line regressed: the entire 82-line residual in `SvgRenderer.cs` lies in six pre-existing members this fix did not touch. Success, parse-failure, and argument-boundary paths are all covered; the null-returning branch is driven through the Moq parse seam and the throwing branch asserts `XmlException` as `InnerException`. The separate file-level policy floors are not met and are recorded as policy gaps G-1 and G-9, which are outside this criterion's text. |
| AC-6 | **PASS** | Independently reproduced rather than accepted on the record. Format: `dotnet tool run csharpier check .` → exit 0, 1467 files, 0 needing formatting. Analyzer: mandated solution build → exit 0, 0 errors, 6 warnings, all pre-existing and none in changed files. Type check: the mandated solution-wide form returns exit 0 but compiles nothing, so the reviewer forced a genuine recompile of the changed projects; `SVGControl` and `SVGControl.Test` both compiled under `/nullable:enable /langversion:latest` with **zero** diagnostics and **zero** `CS8630`, which supplies the "no new diagnostics" verification the vacuous gate cannot. Tests: 6150/6150 in the mandated 9-assembly wrapper. The gate's structural vacuity is recorded as policy gap G-3(b) and is a repository-level concern, not a defect in this delivery. |
| AC-7 | **PASS** | The criterion requires a written identification, and `research/2026-08-04T15-05-svg-renderer-null-document-research.md` (607 lines) delivers all three required elements: the exception, the reproducing hosts, and whether the fallback is reached. Its central claim is **independently corroborated at runtime** by the reviewer: the isolated run's exception chain reads `FileNotFoundException` for `ExCSS, Version=4.3.2.0` with an inner `FileNotFoundException` for `ExCSS, Version=4.2.3.0`, confirming that `Svg 3.4.8` binds `ExCSS 4.2.3.0` exactly as the artifact concluded. The designer-host observation remains tracked as human requirement H-2. One caveat, recorded in policy gap G-8: the criterion's cited vstest corroboration is conditional on assembly ordering. |
| AC-8 | **PASS** | `SvgAssemblyResolver.ResolveByNameAndKey` runs strategy 3 after the already-loaded scan and the `Assembly.Load` attempt, iterating `SvgAssemblyProbe.GetProbeDirectories(self.Location, self.CodeBase, AppDomain.CurrentDomain.BaseDirectory)` and gating every `Assembly.LoadFrom` result through `PublicKeyTokensEqual` (lines 109-138). The `_resolving.Add`/`Remove` re-entrance guard still encloses strategies 2 and 3 and the method still ends `return null;`. Empty-`Location` tolerance is implemented at `SvgAssemblyProbe.cs:43-50` and covered by test. The public-key-token requirement is now verified by **measurement** rather than inspection: `PublicKeyTokensEqual` moved to `SvgAssemblyProbe` and measures 15/15 line and 18/18 branch. Eighteen `SvgAssemblyProbeDirectoryTests` pass, including the empty-`Location` skip, the unparsable code base, the invalid-path-character `baseDirectory`, case-insensitive de-duplication, and the all-null case. The relocation and the corrected test count are both properly disclosed in the criterion's amendment. |
| AC-9 | **PASS** | `SVGControl.Test` is a solution member: `TaskMaster.sln:42-43` declares the project and lines 264-276 add its twelve configuration mappings. The project compiles — the reviewer observed it emit to `SVGControl.Test\bin\Debug\SVGControl.Test.dll` during a forced recompile — and its tests execute under `vstest.console.exe`, which the reviewer ran three times. The `EnsureNuGetPackageBuildImports` `<Error>` does not fire. The amendment correctly discloses that the five package pins named in the original text were superseded by the rebase onto `ce0c91e6` (PR #419) and records the delivered versions. |
| AC-10 | **PARTIAL** | The redirect **value** is corrected as required: `SVGControl.Test/app.config:23` now reads `oldVersion="0.0.0.0-4.3.2.0" newVersion="4.3.2.0"`, replacing the `4.2.4.0` target that existed nowhere in the repository, and matching both `SVGControl/app.config` and the deployed `packages/ExCSS.4.3.2`. The amendment correctly discloses the `4.3.1.0` → `4.3.2.0` change of target. However the criterion's stated objective — "so the test host can resolve ExCSS through the binding redirect rather than depending on the `AssemblyResolve` fallback to mask it" — is **not achieved**. `ExCSS.dll` is absent from `SVGControl.Test/bin/Debug` because the project references `Svg` but not `ExCSS`, and legacy `packages.config` projects do not flow transitive copy-local. A binding redirect cannot resolve an assembly that is not on the probing path, and the fallback cannot either, since it probes that same directory. Six tests consequently fail unless a sibling assembly supplies ExCSS first. See policy gap G-8 and code-review finding CR-8. This PARTIAL does not add to the blocking count: G-8 already carries the remedy. |
| AC-11 | **FAIL** | Undelivered. The runbook `runbooks/verify-winforms-designer-load.runbook.md` (283 lines) exists and is complete, but has not been executed, so no `evidence/regression-testing/designer-load-<timestamp>.md` capture exists and `issue.md:110` remains `[ ]`. Correctly tracked as ratified human-interaction requirements H-1 (satisfies AC-11) and H-2 (satisfies AC-7) in `artifacts/orchestration/orchestrator-state.json`, both with `response: "exception"` and both citing the runbook path, satisfying the `.claude/rules/orchestrator-state.md` invariant that an `exception` response carry a non-empty `runbook_path`. Not remediable by any agent: opening a form in the legacy in-process Visual Studio WinForms designer has no unattended automation surface. Requires a human operator session or an explicit maintainer waiver. |

## Summary

Ten of eleven acceptance criteria are satisfied. AC-10 is downgraded to PARTIAL this cycle on the
strength of a new measurement: its corrective value is delivered, but its stated objective is not,
for the reason recorded as policy gap G-8. AC-11 remains FAIL and is a human-only item.

Change relative to cycle 1:

| Criterion | Cycle 1 | Cycle 2 | Note |
|---|---|---|---|
| AC-1 .. AC-4 | PASS | PASS | unchanged; AC-2 and AC-3 now additionally corroborated at runtime |
| AC-5 | PASS | PASS | member coverage improved; two previously-cited gaps closed to 100% |
| AC-6 | PASS | PASS | `CS8630` eliminated; "no new diagnostics" now positively verified by a forced recompile |
| AC-7 | PASS | PASS | central research claim now empirically corroborated by the reviewer |
| AC-8 | PASS | PASS | containment strengthened; key-token check now measured, not inspected |
| AC-9 | PASS | PASS | unchanged |
| AC-10 | PASS | **PARTIAL** | downgraded on new evidence (G-8), not on a change in the code |
| AC-11 | FAIL | FAIL | unchanged; human-only, ratified exception |

Feature verdict: **PARTIAL**. Blocking count **2**, changed from 1 at cycle 1. The cycle-1 blocker
(AC-11) is unchanged. The added blocker is policy gap G-8 / code-review finding CR-8, a test-isolation
defect that was present at cycle 1's head `ea106111` and that the reviewer failed to detect then. It
is newly surfaced, not caused by the remediation. Every item the remediation plan set out to deliver
(R-2 through R-6) is verified delivered, and all seven actionable cycle-1 code-review findings are
verified resolved.

Recommendation: **no-go for merge as-is.** Two actions clear it. First, add the `ExCSS` reference to
`SVGControl.Test` (one `<Reference>` item plus one `packages.config` line, mirroring the `Svg`
reference this branch already added) and confirm the assembly returns 75/75 standalone; this closes
G-8 and restores AC-10 to PASS. Second, either execute the AC-11 runbook in a human session or obtain
an explicit maintainer waiver for it. The production code requires no changes to merge.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md` § `## Acceptance Criteria`
- Total AC items: 11
- Checked off (delivered): 10
- Remaining (unchecked): 1
- Items remaining: AC-11 — Designer load verified by the documented human step

## Acceptance Criteria Check-off

No checkbox state was modified by this audit. Rationale for each class:

- **AC-1 through AC-9 (PASS, already `[x]`).** Already checked off by the executor in prior cycles.
  Each is re-verified PASS in this audit, so no change is required.
- **AC-10 (PARTIAL, currently `[x]`).** Evaluated PARTIAL this cycle. `acceptance-criteria-tracking`
  directs reviewers to leave PARTIAL items unchecked, but it authorizes reviewers only to *check off*
  passing criteria; it does not authorize clearing a checkbox an executor set in a prior cycle.
  Modifying the criterion in either direction would also risk being read as altering delivered scope.
  The discrepancy is therefore recorded here explicitly rather than resolved by mutating `issue.md`:
  **AC-10 is marked `[x]` in the source file but is evaluated PARTIAL in this audit.** The remediation
  planner should treat AC-10 as open until the `ExCSS` reference lands, at which point the existing
  `[x]` becomes accurate without any edit.
- **AC-11 (FAIL, currently `[ ]`).** Correctly unchecked. Must remain unchecked until the human
  designer-load capture exists under `evidence/regression-testing/`.

No phantom criteria were added and no criterion text was altered, per rules 3 and 5 of
`acceptance-criteria-tracking`.
