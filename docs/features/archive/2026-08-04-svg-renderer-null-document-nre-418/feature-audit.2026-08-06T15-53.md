# Feature Audit — svg-renderer-null-document-nre (Issue #418)

- Artifact timestamp: `2026-08-06T15-53`
- Review cycle: reaudit 4 (maintainer-decision verification)
- Work mode: `minor-audit` (marker read from `issue.md:12`)
- Acceptance-criteria source: the explicit `## Acceptance Criteria` section of
  `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md`

## Scope and Baseline

| Field | Value |
|---|---|
| Base branch | `main` |
| Base ref (resolved) | `origin/main` @ `ce0c91e686bf7e060aaab6f185ee6883269e4fd4` |
| Merge-base SHA | `ce0c91e686bf7e060aaab6f185ee6883269e4fd4` |
| Head | `bug/svg-renderer-null-document-nre-418` @ `215a6f7c8bbbc3157ecd4967bd44af632d786b8b` |
| Changed paths | 158 (11 code/config, 147 Markdown) |
| Languages with changed files | C# only |

The audit scope is the full branch diff against the resolved base, not any plan, task, or phase subset.
The merge-base was recomputed rather than accepted from the caller and matches the supplied value.

Since the previous cycle's head `69e675d0`, only Markdown changed. The AC-11 evidence capture was taken
at `db8b59fb`; `git diff --name-only db8b59fb HEAD` returns two Markdown paths, so that observation
remains valid for the current head without re-execution.

The work-mode marker resolves to `minor-audit`, and `issue.md` contains the required explicit
`## Acceptance Criteria` heading, so the fail-closed condition does not apply. `spec.md` and
`user-story.md` are not AC sources in this mode and were not consulted.

## Acceptance Criteria Inventory

Eleven criteria, all in checkbox format under the required heading. AC-1 through AC-6 address the
confirmed error-handling defect and are unconditional; AC-7 and AC-8 address the underlying
parse/binding failure; AC-9 through AC-11 were added during scoping.

| ID | Criterion (abbreviated) | Source line |
|---|---|---|
| AC-1 | Failing regression test exists first | `issue.md:74` |
| AC-2 | No silent exception swallow | `issue.md:75` |
| AC-3 | Parse failure degrades visibly instead of throwing an NRE | `issue.md:78` |
| AC-4 | Fail-fast API exists; null-tolerant call sites keep their contract | `issue.md:81` |
| AC-5 | Coverage on changed code | `issue.md:82` |
| AC-6 | Toolchain passes in a single clean pass | `issue.md:95` |
| AC-7 | Underlying failure identified in writing | `issue.md:100` |
| AC-8 | `AssemblyResolve` fallback resolves from the assembly's own directory | `issue.md:101` |
| AC-9 | `SVGControl.Test` builds and runs | `issue.md:104` |
| AC-10 | Incorrect ExCSS redirect in the test config is corrected | `issue.md:107` |
| AC-11 | Designer load verified by the documented human step | `issue.md:112` |

## Acceptance Criteria Evaluation

| ID | Verdict | Basis |
|---|---|---|
| AC-1 | **PASS** | Four `SvgRendererParseContractTests` recorded failing pre-fix with `NullReferenceException` at `SvgRenderer.cs:133` (`evidence/regression-testing/ac1-fail-before.2026-08-04T14-36.md`) and passing post-fix with unchanged assertions (`ac1-pass-after.2026-08-04T14-36.md`). The bugfix workflow's test-first ordering is evidenced, not asserted. |
| AC-2 | **PASS** | Zero bare `catch` blocks remain in `SvgRenderer.cs`. All four catch sites across `SvgRenderer.cs` and `SvgAssemblyResolver.cs` declare `Exception ex` and log rather than discard. The parse-path boundary logs on both channels and returns `false` with the exception in `out error`. The two resolver catches deliberately use `Trace.TraceWarning` only, avoiding `log4net` re-entrancy inside an `AssemblyResolve` handler — documented in-code with the reason. |
| AC-3 | **PASS** | Verified directly in source this cycle. Both byte-array constructors call `TryGetSvgDocument`, set `_original = Size.Empty` on failure, and contain no unguarded `_doc.Draw()` and no `throw` on the parse-failure path (`SvgRenderer.cs:32-44`, `53-65`). The dual-channel requirement is met: `logger.Error(detail, error)` is paired with `Trace.TraceError(detail)` at all four emission sites, and `DescribeFailure` (lines 74-79) composes `error.GetType().FullName + ": " + error.Message`, so both channels carry the exception type and message. See the note below on the basis of this verification. |
| AC-4 | **PASS** | `TryGetSvgDocument(byte[], out SvgDocument?, out Exception?)` and `GetSvgDocumentOrThrow(byte[])` exist; the latter's `InvalidOperationException.InnerException` is the original parser exception. `GetSvgDocument(byte[])` keeps its tolerant null-returning contract with no `try`/`catch` of its own, and `SvgImageSelector.cs` is unchanged, so all six null-tolerant consumers keep their behavior. The criterion's own note that these `public static` members sit on an `internal` type, and so describe an assembly-internal surface, is accurate. |
| AC-5 | **PASS** | All seven newly added members measure 100.000% line rate, above the `>= 90%` threshold; `SvgAssemblyResolver.Install()` measures 6/6 = 100%. No regression on changed lines. Repository-wide 85.4006% line and 78.6928% branch, both above their floors, independently recomputed by this agent from `artifacts/csharp/coverage.xml`. The file-level shortfall on `SvgAssemblyResolver.cs` is covered by the maintainer exception ratified this cycle; the shortfall on `SvgRenderer.cs` is pre-existing, sits entirely in untouched members, and is owned by a potential-feature follow-up. |
| AC-6 | **PASS** | `evidence/qa-gates/toolchain-clean-pass.2026-08-05T05-00.md` records one consecutive pass with no loop restart: CSharpier format, CSharpier check, restore, analyzer build, nullable build, and the coverage-enabled test run all at `EXIT_CODE: 0`. Both build gates match the `2026-08-04T21-04` baseline exactly in count, code, text, and emitting project, so "no new diagnostics" is a measured comparison rather than an assertion. The `2026-08-04` amendment to the relative form was correctly reverted once the VSTO runtime assemblies were confirmed present. |
| AC-7 | **PASS** | `research/2026-08-04T15-05-svg-renderer-null-document-research.md` names the exception, identifies the host, and states that the pre-existing fallback is reached but returns null because `Assembly.Load` probes the Visual Studio directory. That written identification is what the criterion requires. The conditional second sentence — that the AC-11 capture would supply an observed exception identity "if the bind still fails" — did not trigger, because the bind succeeded. The condition being false does not weaken the criterion, which was already satisfied on its own terms. |
| AC-8 | **PASS** | `ResolveByNameAndKey` runs strategy 3 after the already-loaded scan and the `Assembly.Load` attempt, iterating `SvgAssemblyProbe.GetProbeDirectories(...)` and gating every `Assembly.LoadFrom` result through `PublicKeyTokensEqual`. The re-entrance guard still encloses strategies 2 and 3 and the method still ends `return null;`. The ordered-candidate logic is covered by eighteen `SvgAssemblyProbeDirectoryTests` at 100% line and 100% branch, including the empty-`Location` skip, the unparsable code base, the invalid-path-character case, case-insensitive de-duplication, and the all-null empty-list case. Note that the criterion is satisfied by the implementation and its tests; it does not require proving the fallback is what resolved the designer bind, which the AC-11 capture correctly reports as unestablished. |
| AC-9 | **PASS** | `SVGControl.Test` is a solution member, its pinned packages restore under `packages/`, the hard MSBuild `<Error>` no longer fires, and the project compiles and its tests execute (9 assemblies discovered, 6150/6150 passed). The version amendment following the rebase onto `ce0c91e6` is accurate and correctly framed as a substitution of pin versions, not a change to the criterion's requirement. |
| AC-10 | **PASS** | `SVGControl.Test/app.config:23` reads `oldVersion="0.0.0.0-4.3.2.0" newVersion="4.3.2.0"`, matching both the deployed assembly and `SVGControl/app.config`. A repository-wide search for `newVersion="4.2.4.0"` in an ExCSS block returns zero matches. The cycle-2 addendum closed the one respect in which this was previously PARTIAL: the redirect's stated objective needed `ExCSS.dll` to be findable, and the explicit reference added in cycle 2 puts it on the probing path. Verified by the standalone run at 75/75/0 against 6 failures in both run shapes before the fix. |
| AC-11 | **PASS** | The runbook was executed by the maintainer and the capture is attached at `evidence/regression-testing/designer-load-2026-08-06T19-47.md`. The criterion's stated requirement — the form opens in the Visual Studio WinForms designer after the fix without a `NullReferenceException` — is met, with the default SVG artwork additionally visible. The conditional AC-7 clause did not trigger. See the extended assessment below. |

### Note on the basis of the AC-3 verification

AC-3 evaluates PASS, but the basis is narrower than the AC-11 capture claims. The capture states the
dual-channel behavior is "proven by unit tests in `SVGControl.Test`". No such test exists: there are
zero occurrences of `Trace`, `log4net`, `Listener`, `Appender`, or `DescribeFailure` anywhere in
`SVGControl.Test/*.cs`. The emission lines are *executed* by the parse-failure constructor tests, which
is why `DescribeFailure` measures 100% line coverage, but nothing asserts what is emitted or on which
channel.

This does not change the verdict. AC-3's operative requirement is a property of the implementation —
that it emit on both channels, and that both carry the exception type and message — which is
statically checkable and was checked directly in source this cycle. But the criterion's basis is code
inspection, not test assertion, and the evidence artifact should say so. Recorded as G-10 in the policy
audit and CR-Med-1 in the code review.

### Extended assessment of AC-11 and its declared limitations

The capture records three limitations rather than glossing them. Each was assessed independently.

**Limitation 1 — AC-3's designer-host observability was not exercised. Correctly characterized; does
not downgrade any criterion.** AC-3 requires the implementation to emit on a channel the designer
surfaces. It does not require observing that emission inside `devenv.exe`. Since nothing failed, the
channel was never driven there, and the capture is right that confirming it would require inducing a
parse failure in the designer host — work outside this issue. The capture's honesty here is
appropriate; only its supporting citation is overstated, as noted above.

**Limitation 2 — attribution of the successful bind is not established. Correctly characterized and
appropriately hedged.** The three candidate mechanisms are genuinely indistinguishable from a pass/fail
render, and the capture says so plainly instead of claiming the fix as the cause. It uses "corroborated"
for AC-8 rather than "proven", which is the accurate strength. AC-8 is not weakened, because its
criterion concerns the fallback's implementation and tested decision logic, not which mechanism won a
particular race.

**Limitation 3 — open question U-2 remains open. Stated more pessimistically than the runbook
requires.** Runbook step 10 is explicitly conditional: "Optionally, and only if the designer error page
reported a failure to load `ExCSS` ...". No error page appeared, so the precondition was false and the
step was correctly not performed. The runbook's own field list qualifies it as "The step 10
`ProjectAssemblies` observation, **if performed**". The capture describes it as "was not reported",
which reads as an operator omission when it was a conditional step whose condition did not hold. The
runbook was fully executed with respect to every step whose precondition held. This errs toward
under-claiming, which is the safe direction, and warrants a wording correction rather than a downgrade.

**One gap the capture did not declare.** The runbook's mandatory-field list requires the Visual Studio
product name and version with the build configuration, and a record of whether Visual Studio was
restarted after the build. Both are absent. The second matters: runbook step 2 exists precisely to
guarantee the designer loads the freshly built `SVGControl.dll`, and without that record a cached
pre-fix assembly is not formally excluded. AC-11 nonetheless holds, because the same environment
demonstrably produced the `NullReferenceException` pre-fix — that observation is the bug report in
`issue.md` — and now produces a clean render with the artwork visible. The inference spans two sessions
rather than one recorded prerequisite. Recorded as G-11 / CR-Med-2 with a concrete correction.

**Overall on honesty.** The capture's limitations are honestly stated. Two are accurate and one is more
conservative than necessary; none is a downgrade in disguise, and none conceals a failure. The single
accuracy defect found is the "proven by unit tests" clause, which overstates the evidentiary basis in
the direction of confidence. That is worth correcting, and it is the only place in this feature's
documentation where the stated basis exceeds the measured one.

## Summary

**11 of 11 acceptance criteria PASS. Blocking count 0, changed from 1.**

The single blocker carried by the previous three cycles, AC-11, is delivered. The maintainer executed
the documented runbook and attached the capture; the criterion's stated requirement is met and its
conditional AC-7 clause did not trigger. The G-9 coverage adjudication, which was already non-blocking,
is closed by a ratified maintainer exception whose scope is narrow, whose technical basis is checkable,
and which is a threshold exception rather than a measurement exclusion — so it does not breach the
repository's prohibition on excluding production files from coverage.

The feature delivers what the issue opened on. A parse failure can no longer surface as an opaque
`NullReferenceException`; the underlying exception is captured, logged on two channels, and made
available through an explicit `Try` API without breaking the six existing null-tolerant consumers. The
binding failure that triggered the original report is addressed by a directory-probing fallback whose
decision logic is fully tested, and by an explicit `ExCSS` reference that also removed a test
order-dependence.

Four documentation corrections are recommended before merge — two to the AC-11 capture (G-10, G-11),
one to transcribe the G-9 waiver into the committed record (G-12), and one to give the waived residual
a follow-up owner (G-13). None gates the pull request.

## Acceptance Criteria Check-off

All eleven criteria were already `- [x]` in `issue.md` when this audit began; AC-11 was checked off by
the maintainer in commit `215a6f7c` alongside the evidence capture. This audit verified each check-off
against its cited evidence and confirms every one is correctly marked. **No checkbox was changed by
this review**, because none required changing.

### Acceptance Criteria Status

```
Source: docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md
Total AC items: 11
Checked off (delivered): 11
Remaining (unchecked): 0
Items remaining: none
```

Note on the unrelated checkboxes in `issue.md`: the `## Logs / Screenshots` and
`## Proposed Fix / Validation Ideas` sections contain unchecked boxes. Under `minor-audit`, only the
explicit `## Acceptance Criteria` section is the AC source, so those are not acceptance criteria and
were not evaluated or checked off.
