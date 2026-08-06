# Remediation Inputs — svg-renderer-null-document-nre (Issue #418)

- Artifact timestamp: `2026-08-06T15-53`
- Review cycle: reaudit 4 (maintainer-decision verification)
- Base: `origin/main` @ `ce0c91e686bf7e060aaab6f185ee6883269e4fd4`
- Head: `bug/svg-renderer-null-document-nre-418` @ `215a6f7c8bbbc3157ecd4967bd44af632d786b8b`

## Source Artifacts

| Artifact | Path |
|---|---|
| Policy audit | `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/policy-audit.2026-08-06T15-53.md` |
| Code review | `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/code-review.2026-08-06T15-53.md` |
| Feature audit | `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/feature-audit.2026-08-06T15-53.md` |

## Headline

**Blocking count: 0. Changed from 1.** The remediation loop is complete. Both maintainer-assigned items
from the previous cycle are discharged, and both were assessed on their merits rather than accepted on
assertion.

- **RM-1 / G-2 / AC-11 — DISCHARGED.** The maintainer executed the designer-load runbook and attached
  the capture. AC-11 evaluates PASS; all eleven criteria are now PASS.
- **RM-2 / G-9 — DISCHARGED.** The maintainer authorized extending the ratified
  `COVERAGE_MEMBER_UNREACHABLE` exception to the file-level floor for `SvgAssemblyResolver.cs`. The
  waiver is a legitimate disposition.

**No remediation plan is authored for this cycle, and none should be.** There are zero blocking
findings. The four items below are documentation corrections that improve the accuracy and durability
of the audit trail; none affects the correctness of the delivered fix, none gates the pull request, and
dispatching an executor for them would be disproportionate. They are listed here so a maintainer can
action them directly, in a few minutes, without a remediation cycle.

## Recommended Before Merge

### RC-1 — Correct a false evidentiary claim in the AC-11 capture (Medium)

- **Source finding:** policy audit G-10; code review CR-Med-1.
- **File:** `evidence/regression-testing/designer-load-2026-08-06T19-47.md`, line 39.
- **Condition:** the capture states "The dual-channel behavior is proven by unit tests in
  `SVGControl.Test`". No such test exists.

  ```
  grep -rn "Trace\|log4net\|Listener\|Appender" SVGControl.Test/*.cs   -> no matches
  grep -rn "DescribeFailure" SVGControl.Test/*.cs                      -> no matches
  ```

  The emission lines are executed by the parse-failure constructor tests — hence `DescribeFailure` at
  100% line coverage — but nothing asserts what is emitted or on which channel.
- **Why it matters:** the clause is load-bearing. It is the fallback the capture offers when
  disclaiming the unexercised designer-host observation, so the reader is told the behavior is proven
  by tests when the real basis is one notch weaker. The conclusion still holds, because an
  implementation-shape requirement is legitimately verifiable by inspection, but an audit trail should
  state the basis it actually has.
- **Required action:** reword to "The dual-channel behavior is verified by code inspection of the four
  paired `logger.Error` / `Trace.TraceError` emission sites in `SvgRenderer.cs`, and is executed though
  not asserted by the parse-failure tests; no test captures `Trace` or `log4net` output."
- **Assignment:** orchestrator or maintainer. Single-sentence documentation edit.
- **Optional follow-up:** a `TraceListener`-capturing test would convert the inspection into an
  assertion. That is coverage-uplift work, not a defect in the fix.

### RC-2 — Record the two omitted mandatory runbook fields (Medium)

- **Source finding:** policy audit G-11; code review CR-Med-2.
- **File:** `evidence/regression-testing/designer-load-2026-08-06T19-47.md`, header block.
- **Condition:** the runbook's "must contain, at minimum" list requires the Visual Studio product name
  and version with the build configuration, and a record of whether Visual Studio was restarted or the
  solution reopened after the build. Both are absent.
- **Why it matters:** runbook step 2 exists to guarantee the designer loads the freshly built
  `SVGControl.dll`, and states that reason explicitly. Without the record, the capture does not
  formally exclude a cached or shadow-copied pre-fix assembly, since a pre-fix binary also renders
  successfully whenever the `ExCSS` bind happens to succeed. AC-11 still holds — the same environment
  demonstrably produced the `NullReferenceException` pre-fix, which is the bug report itself, and now
  produces a clean render — but the inference spans two sessions instead of one recorded prerequisite,
  and it compounds the capture's own limitation 2 on unestablished attribution.
- **Required action:** append an addendum recording the Visual Studio product name and version, the
  build configuration used, and whether Visual Studio was restarted after the build. **If the restart
  cannot now be recalled, record it as unknown rather than assuming it.**
- **Assignment:** maintainer. Only the operator holds these facts.
- **Also recommended in the same edit:** restate limitation 3 to reflect that runbook step 10 is
  explicitly conditional ("only if the designer error page reported a failure to load `ExCSS`"), so its
  precondition did not hold and it was correctly not performed, rather than "was not reported", which
  reads as an omission.

### RC-3 — Transcribe the G-9 waiver into the committed record (Medium)

- **Source finding:** policy audit G-12.
- **Condition:** the waiver exists only in `artifacts/orchestration/orchestrator-state.json`, which is
  untracked and ignored:

  ```
  git check-ignore -v artifacts/orchestration/orchestrator-state.json
    -> .gitignore:57:artifacts/    artifacts/orchestration/orchestrator-state.json
  ```

- **Why it matters:** the waiver will not appear in the pull request, will not survive a fresh clone,
  and the next coverage audit of this file will re-derive G-9 with no record that it was adjudicated.
  This inverts the property the repository's own exemption mechanism is designed for: `CLAUDE.md` UT2
  specifies exemptions be applied via `[ExcludeFromCodeCoverage]` attributes "in source code
  (**reviewable in PRs**)" or `coverage.config` excludes — both deliberately visible to a reviewer.
- **Required action:** add a short subsection to `issue.md` under AC-5, beside the existing
  `COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgAssemblyResolver.ResolveByNameAndKey` note, recording the
  authorizing maintainer, the date, the scope sentence ("this file only; does not exempt `Install()`"),
  and the basis. Documentation edit; no source change and no coverage-tooling change.
- **Note:** do **not** implement this as an `[ExcludeFromCodeCoverage]` attribute or a
  `coverage.config` exclude. The waiver's legitimacy rests on it being a threshold exception with the
  file still fully measured; converting it to an exclusion would remove those 66 lines from the
  repository-wide denominator and would breach `.claude/rules/general-unit-test.md`.
- **Assignment:** orchestrator or maintainer. This is the one worth doing first.

### RC-4 — Give the waived residual a follow-up owner (Low)

- **Source finding:** policy audit G-13; code review CR-Info-2.
- **Condition:** `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md` owns the G-1
  residual on `SvgRenderer.cs` but names neither `SvgAssemblyResolver` nor `ResolveByNameAndKey`.
- **Required action:** add `SVGControl/SvgAssemblyResolver.cs` to that file with a note that its
  shortfall is waived rather than accepted indefinitely, so it returns to review if a host-level seam
  later makes the `AssemblyResolve` wiring testable.
- **Assignment:** maintainer.

## Non-Blocking Findings Carried Forward (no action required before merge)

| ID | File | Summary |
|---|---|---|
| G-1 | `SVGControl/SvgRenderer.cs` | Modified-file line coverage 80.1932% against the 85% floor. Entire shortfall in six untouched pre-existing members; no regression on changed lines. Owned by `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md`. |
| G-3 | repository-level | The mandated solution-wide nullable gate returns exit 0 while compiling 0 `CoreCompile` targets, so its exit status is non-probative. Mitigated here by forced per-project rebuilds. Recommend a per-changed-project gate. |
| G-4 | `SVGControl.Test/` | Test files sit beside the project rather than in a mirrored `tests/` tree. Pre-existing repository-wide convention across all nine test projects. |
| G-7 | `artifacts/pr_context.summary.txt` | Collector defects persist at this head: C# files classified as documentation (`Core logic changes: 0 files`), and spurious close candidates including `#AC-1`..`#AC-11` and `#DE06-4337`, a fragment of the `SVGControl.Test` project GUID. Corrected in place for this review; the collector remains uncorrected. |
| G-14 | `artifacts/orchestration/orchestrator-state.json` | `maintainer_waivers` is an undocumented extension to the checkpoint shape, and its `runbook_path` is a disclosed placeholder. No validator in this repository inspects it. If the block is to persist, document it in `.claude/rules/orchestrator-state.md`. |
| CR-Low-1 | `SVGControl/SvgAssemblyResolver.cs` | Diagnostic prefixes still read `"SvgRenderer load ..."`, naming a type the code no longer lives in. |
| CR-Low-2 | `SVGControl/SvgAssemblyResolver.cs` | Resolver reaches back into `SvgRenderer` for `DescribeFailure` and `typeof`; the R-6 separation is incomplete. |
| CR-Low-3 | `SVGControl/SvgRenderer.cs` | The two byte-array constructors carry near-identical 17-line bodies. |
| CR-Low-4 | `SVGControl/SvgAssemblyResolver.cs` | The pre-guard region sits outside the containment `try`. Disclosed and accepted in Design Decision 11. |
| CR-Info-1 | `SVGControl/app.config`, `SVGControl.Test/app.config` | Stale `Fizzler` redirect to an absent `1.3.0.0`. Correctly deferred to `docs/features/potential/2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md`; recommend promoting it. |

## Why No Remediation Plan Is Authored

The SKILL contract directs creating a remediation plan when remediation is triggered. It is not
triggered here in any sense that requires one:

- **Blocking count is 0.** No acceptance criterion is FAIL or PARTIAL; all eleven are PASS.
- **No toolchain check fails.** The single clean pass is recorded with `EXIT_CODE: 0` at every stage.
- **No code review finding is a blocker.** The two Medium findings are corrections to an evidence
  artifact; the four Low findings are optional polish carried forward.
- **No coverage trigger fires unresolved.** Repository-wide C# coverage is 85.4006% line and 78.6928%
  branch, both above their floors. The one file below the new-file threshold is covered by a ratified
  maintainer exception. The coverage artifact is present.

The four items above are documentation edits totalling a few paragraphs, three of which only the
maintainer can supply or authorize. Authoring a plan and dispatching an executor for them would
manufacture a fifth review cycle for work that does not touch code and does not affect merge
readiness.

**Recommendation: close the remediation loop and proceed to a pull request**, actioning RC-1 through
RC-4 as a single documentation commit before or alongside it. RC-3 is the one with lasting
consequence, because it is the difference between a coverage waiver that a future reviewer can find
and one that vanishes with the working directory.

## Go / No-Go

**GO.** The feature is ready for a pull request.

All eleven acceptance criteria pass, the toolchain passes in one clean pass, repository-wide coverage
clears both floors, no evidence-location or workflow-green-run rule is breached, and the blocking count
is zero. The two file-level coverage shortfalls are each dispositioned: one owned by a tracked
follow-up, one waived by the maintainer on a checkable basis with an explicitly narrow scope.
