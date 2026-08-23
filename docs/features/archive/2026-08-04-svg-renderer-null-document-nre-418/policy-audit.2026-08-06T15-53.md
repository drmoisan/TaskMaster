# Policy Audit — svg-renderer-null-document-nre (Issue #418)

- Artifact timestamp: `2026-08-06T15-53`
- Review cycle: reaudit 4 (maintainer-decision verification)
- Auditor: feature-review agent
- Work mode: `minor-audit` (marker read from `issue.md:12`)

## Baseline Resolution

| Field | Value |
|---|---|
| Base branch (requested) | `main` |
| Base ref (resolved) | `origin/main` @ `ce0c91e686bf7e060aaab6f185ee6883269e4fd4` |
| Merge-base SHA | `ce0c91e686bf7e060aaab6f185ee6883269e4fd4` |
| Head | `bug/svg-renderer-null-document-nre-418` @ `215a6f7c8bbbc3157ecd4967bd44af632d786b8b` |
| Range | `ce0c91e6..215a6f7c` |
| PR-context summary | `artifacts/pr_context.summary.txt` (Head ref matches `215a6f7c`; fresh) |
| PR-context appendix | `artifacts/pr_context.appendix.txt` |

The merge-base was recomputed rather than accepted from the caller. `git merge-base HEAD origin/main`
returns `ce0c91e686bf7e060aaab6f185ee6883269e4fd4`, matching the supplied value. The PR-context summary
records `Head ref (resolved): bug/svg-renderer-null-document-nre-418 @ 215a6f7c...`, which equals
`git rev-parse HEAD`, so the artifacts are current for this head and were not regenerated.

### Delta since the previous review cycle

The previous cycle (`2026-08-05T00-04`) audited head `69e675d0` and returned PARTIAL with blocking
count 1. Two commits have landed since:

```
215a6f7c docs(418): AC-11 verified by the maintainer; G-9 exception authorized
db8b59fb docs(418): record the cycle-2 reaudit; blocking count 2 -> 1
```

`git diff --name-only 69e675d0 HEAD` returns nine paths, all Markdown: three feature-review artifacts
from the prior cycle, three `.claude/agent-memory/feature-review/` files, `issue.md`, and the new
evidence capture. **Zero `.cs`, `.csproj`, `packages.config`, or `app.config` paths appear.** The
caller's assertion to that effect was verified independently and holds. Every code finding and every
coverage figure from the `2026-08-05T00-04` cycle therefore carries forward unchanged, and this audit
re-measured rather than re-derived them.

## Executive Summary

**Verdict: PASS. Blocking count 0, changed from 1.**

Both items that the previous cycle routed to the maintainer have been discharged, and both were
assessed on their merits rather than accepted on assertion.

1. **G-2 / AC-11 is CLOSED.** The maintainer executed the designer-load runbook and the capture is
   attached at `evidence/regression-testing/designer-load-2026-08-06T19-47.md`. The criterion's stated
   requirement — the form opens in the Visual Studio WinForms designer without a
   `NullReferenceException` — is met and evidenced. All eleven acceptance criteria are now `- [x]`.

2. **G-9 is CLOSED by a ratified maintainer exception.** The `COVERAGE_MEMBER_UNREACHABLE` exception
   for `ResolveByNameAndKey` is extended to the file-level floor for `SVGControl/SvgAssemblyResolver.cs`.
   The waiver was already non-blocking; the authorization removes it as an open decision.

The AC-11 capture's three self-declared limitations were examined independently. **Two are correctly
characterized and one is stated more pessimistically than the runbook requires.** None warrants
downgrading any criterion. However, this audit found **one materially overstated claim in the capture**
that the capture itself did not flag, plus two mandatory runbook evidence fields that were omitted.
Both are recorded as new non-blocking findings (G-10, G-11) with concrete corrections.

The G-9 waiver is a legitimate disposition. It is a threshold exception rather than a measurement
exclusion, so it does not breach the no-exclusion rule; its technical basis is checkable and correct;
and its scope is narrow and explicit. It nevertheless leaves three gaps, the material one being that
the waiver is recorded **only in a gitignored file** and so will not appear in the pull request (G-12).

No item blocks the pull request. Four documentation corrections are recommended before merge.

## Rejected Scope Narrowing

None. The caller prompt supplied a base ref, a merge-base SHA, a head SHA, the active feature folder,
and the acceptance-criteria source, then directed: "Determine scope yourself from the branch diff per
the SKILL contract." That is a delegation of scope determination to this agent, not a narrowing of it.

The caller additionally asserted that no source or build-configuration file changed since `69e675d0`
and that prior code findings and coverage figures therefore stand. That is a factual claim about the
diff, not an instruction to skip a check. It was verified independently before being relied upon
(`git diff --name-only 69e675d0 HEAD`, nine paths, all Markdown) and found accurate. The full
feature-vs-base audit against `ce0c91e6` was performed regardless, and every language with changed
files in the branch diff carries an explicit verdict below.

No caller text attempted to limit the audit to a plan subset, to a subset of changed files, or to
exclude any language from measurement.

## Evidence Location Compliance

All evidence produced by this feature is written under
`docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/<kind>/`, per
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.

The branch diff was scanned for files written under `artifacts/baselines/`, `artifacts/qa/`,
`artifacts/evidence/`, or `artifacts/coverage/`:

```
git diff --name-only ce0c91e6...HEAD | grep -E '^artifacts/(baselines|qa|evidence|coverage)/'
```

Result: **zero matches. PASS.** The new capture this cycle,
`evidence/regression-testing/designer-load-2026-08-06T19-47.md`, is in the canonical
`<FEATURE>/evidence/<kind>/` location and uses the correct `<kind>` subdirectory for a manual
regression observation. The runbook itself instructs the operator to that path and warns against
`artifacts/`-rooted paths, which is why the operator landed it correctly.

`validate_evidence_locations.py --root .` is not present in this repository; the scan above was
performed directly against the diff.

## Change Inventory (feature-vs-base)

`git diff --numstat ce0c91e6...HEAD` reports 158 changed paths.

| Category | Files | Detail |
|---|---|---|
| C# production source | 3 | `SvgRenderer.cs` (+115/-107), `SvgAssemblyResolver.cs` (+157, new), `SvgAssemblyProbe.cs` (+93, new) |
| C# test source | 3 | `SvgRendererParseContractTests.cs` (+358), `SvgAssemblyProbeDirectoryTests.cs` (+347), `SvgRendererNullToleranceTests.cs` (+144), all new |
| Project / solution | 3 | `SVGControl.csproj` (+2), `SVGControl.Test.csproj` (+12), `TaskMaster.sln` (+14) |
| Application config | 2 | `SVGControl.Test/app.config` (+1/-1), `SVGControl.Test/packages.config` (+2) |
| Markdown | 147 | feature docs, evidence, agent memory |

Languages with changed files in the branch diff: **C# only.** No `.ts`, `.tsx`, `.py`, `.ps1`, or
`.psm1` path appears in the diff.

## PR-Context Artifact Corrections

The PR-context collector again misclassified this branch. `artifacts/pr_context.summary.txt:222`
recorded `Core logic changes: 0 files` and filed all eleven C#/project/config paths under
`Docs/templates/agents/tooling: 109 files`. This is the same collector defect recorded as G-7 in the
three prior cycles; it persists at this head.

The defect is not cosmetic. The `validate-feature-review-coverage.ps1` hook derives its changed-language
set from the summary's bullet list, not from the diff. With the top-ten churn list containing only
Markdown paths, the hook enumerated zero languages and would have skipped per-language enforcement
entirely.

**Correction applied in place** (disclosed as a reviewer side effect, G-6): a
`REVIEWER CORRECTION` block plus a `Core logic changes (corrected): 11 files` census in the collector's
own bullet format was appended to the overview section, enumerating all eleven paths with their
`(+N/-N)` churn. Verified after the edit:

```
Languages: CSharp
```

No source, test, or policy file was modified. Also carried forward from prior cycles: the summary's
close-candidate list contains spurious entries `#AC-1` .. `#AC-11` and `#DE06-4337`, the latter a
fragment of the `SVGControl.Test` project GUID parsed as an issue reference.

## 1. General Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Independence | PASS | No shared mutable state across the three new test classes; each test constructs its own subject. |
| Isolation | PASS | Each `[TestMethod]` targets a single member or a single decision branch. |
| Fast execution | PASS | 9-assembly run completes without a long-running test; see `evidence/qa-gates/test-coverage.2026-08-05T05-00.md`. |
| Determinism | PASS | Order-dependence defect G-8 was closed in cycle 2 by adding the explicit `ExCSS` reference; standalone and paired runs now agree (75/75/0 and 76/76/0). |
| Readability | PASS | Descriptive method names; Arrange-Act-Assert structure throughout. |
| MSTest framework | PASS | `[TestClass]` / `[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`. |
| Moq for mocking | PASS | Parse-delegate seam driven via `Setup(...).Returns((SvgDocument)null)`. |
| FluentAssertions | PASS | Used for assertions across the three new classes. |
| No external dependencies | PASS | No network, database, or external process. |
| No temporary files | PASS | The null-returning parse path is driven through an injected `Func<byte[], SvgDocument?>` delegate rather than by staging a file. |
| Test file location | FAIL (pre-existing, G-4) | Tests sit beside the project rather than in a mirrored `tests/` tree. Repository-wide convention predating this branch. |

### 1.2 Coverage Verification

Coverage was verified by inspecting pre-existing artifacts produced during execution. No coverage run
was re-executed by this agent.

| Language | Changed files | Artifact | Present |
|---|---|---|---|
| C# | 11 | `artifacts/csharp/coverage.xml` | Yes |
| TypeScript | 0 | `coverage/lcov.info` | Not required; zero changed files |
| Python | 0 | `artifacts/python/lcov.info` | Not required; zero changed files |
| PowerShell | 0 | `artifacts/pester/powershell-coverage.xml` | Not required; zero changed files |

#### 1.2.1 Per-language comparison

- **C# / dotnet line coverage: PASS.**
  - Baseline: 85.4097% (93539/109518)
  - Post-change: 85.4006% (93529/109518)
  - Change: −0.0091 pts, −10 covered lines, 0 change to the denominator
  - Disposition: PASS against the `>= 85%` floor, with a 0.4006-point margin.
  - New/changed-code coverage: 100.0000%
  - Evidence: `evidence/qa-gates/coverage-delta.2026-08-05T05-00.md` lines 40-43; independently
    recomputed by this agent from `artifacts/csharp/coverage.xml`, which reports
    `LINE missed="15989" covered="93529"` → 93529/109518 = 85.4006%.

- **C# / dotnet branch coverage: PASS.**
  - Baseline: 78.7220% (21584/27418)
  - Post-change: 78.6928% (21576/27418)
  - Change: −0.0292 pts, −8 covered branches, 0 change to the denominator
  - Disposition: PASS against the `>= 75%` floor, with a 3.6928-point margin.
  - New/changed-code coverage: 100.0000%
  - Evidence: same artifact; recomputed from `artifacts/csharp/coverage.xml`
    `BRANCH missed="5842" covered="21576"` → 21576/27418 = 78.6928%.

- **TypeScript coverage: not measured. Zero changed files of this language in the branch diff.**
- **Python coverage: not measured. Zero changed files of this language in the branch diff.**
- **PowerShell coverage: not measured. Zero changed files of this language in the branch diff.**

#### 1.2.2 File-level tiers

| File | Tier | Line | Branch | Floor | Verdict |
|---|---|---|---|---|---|
| `SVGControl/SvgAssemblyProbe.cs` | new | 100.0000% (102/102) | 100.0000% (92/92) | `>= 90%` new-file | PASS |
| `SVGControl/SvgAssemblyResolver.cs` | new | 61.6279% (106/172) | 53.8462% (28/52) | `>= 85%` / `>= 90%` | FAIL, dispositioned non-blocking via the ratified maintainer exception recorded under G-9 |
| `SVGControl/SvgRenderer.cs` | modified | 80.1932% (332/414) | 76.1905% (64/84) | `>= 85%` | FAIL, non-blocking; no regression on changed lines; owned by G-1 |

Newly added members all measure 100.000% line rate: `OpenFromBytes`, both `TryGetSvgDocument`
overloads, `GetSvgDocumentOrThrow`, `DescribeFailure`, `SvgAssemblyProbe.TryGetDirectoryFromCodeBase`,
`SvgAssemblyProbe.GetProbeDirectories`, and `SvgAssemblyResolver.Install()` at 6/6. No regression on
any changed line.

## 2. General Code Change Policy Compliance

| Requirement | Status | Notes |
|---|---|---|
| Simplicity first | PASS | The fix replaces a swallowing `catch` with a `Try`-pattern boundary; no new indirection. |
| Reusability | PASS | Ordered-candidate logic factored into `SvgAssemblyProbe` as pure helpers. |
| Extensibility | PASS | `TryGetSvgDocument` has an internal overload accepting an injected parse delegate. |
| Separation of concerns | PASS | Pure directory-candidate logic is separated from the host-bound `AssemblyResolve` handler. |
| Fail fast, no silent swallow | PASS | Zero bare `catch` blocks remain; all four catch sites declare `Exception ex` and log. |
| File size <= 500 lines | PASS | `SvgRenderer.cs` 449, `SvgAssemblyResolver.cs` 157, `SvgAssemblyProbe.cs` 93; largest new test file 358. |
| No new dependencies | PASS | `ExCSS` was already a repository package; the change adds a reference, not a package. |
| I/O isolation | PASS | Domain logic testable without filesystem or network. |

### 2.1 modified-workflow-needs-green-run

The rule fires when the branch diff modifies `.github/workflows/**`, `scripts/benchmarks/**`, or
`.github/actions/**`.

```
git diff --name-only ce0c91e6...HEAD | grep -E '^(\.github/workflows/|scripts/benchmarks/|\.github/actions/)'
```

Result: **zero matches. Rule does not fire.** No Blocking finding on this rule.

## 3. Language-Specific Code Change Policy Compliance (C#)

| Requirement | Status | Evidence |
|---|---|---|
| CSharpier formatting | PASS | `evidence/qa-gates/csharpier-format.2026-08-05T05-00.md`, 0 files reformatted. |
| .NET analyzer build | PASS | `evidence/qa-gates/analyzer-build.2026-08-05T01-50.md`, `EXIT_CODE: 0`, 0 errors, 0 new diagnostics against the `2026-08-04T21-04` baseline. |
| Nullable / TreatWarningsAsErrors | PARTIAL (G-3) | `evidence/qa-gates/nullable-build.2026-08-05T05-00.md`, `EXIT_CODE: 0`. The solution-wide invocation is incrementally satisfied and compiles 0 `CoreCompile` targets; forced per-project rebuilds were used to obtain a probative result. |
| Test execution | PASS | `evidence/qa-gates/test-coverage.2026-08-05T05-00.md`, 9 assemblies, 6150/6150 passed, 0 failed. |
| Naming conventions | PASS | `PascalCase` types and public members, `camelCase` locals throughout the three new files. |
| Nullable annotations | PASS | `out SvgDocument?`, `out Exception?`, `byte[]?` parameters correctly annotated. |
| Minimal public surface | PASS | `SvgAssemblyResolver` and `SvgAssemblyProbe` are `internal static`; `SvgRenderer` remains `internal`. |
| XML docs on non-obvious contracts | PASS | `TryGetSvgDocument` and `GetSvgDocumentOrThrow` document their contracts. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Requirement | Status |
|---|---|
| MSTest framework | PASS |
| Moq for mocking | PASS |
| FluentAssertions preferred | PASS |
| Arrange-Act-Assert | PASS |
| Seam-based mocking for boundaries | PASS — injectable `Func<byte[], SvgDocument?>` parse delegate |
| No banned time APIs | PASS — no `DateTime.Now`, `Thread.Sleep`, or `Task.Delay` in the new tests |
| Deterministic in IDE and CLI | PASS — G-8 closed in cycle 2 |

## 5. Test Coverage Detail

Coverage figures are byte-identical to the `2026-08-05T00-04` cycle, as expected: no `.cs` file changed
between `69e675d0` and `215a6f7c`.

| Class | Line | Branch |
|---|---|---|
| `SVGControl.SvgRenderer` | 332/414 = 80.1932% | 64/84 = 76.1905% |
| `SVGControl.SvgAssemblyProbe` | 102/102 = 100.0000% | 92/92 = 100.0000% |
| `SVGControl.SvgAssemblyResolver` | 106/172 = 61.6279% | 28/52 = 53.8462% |

The entire `SvgAssemblyResolver` shortfall is one member, `ResolveByNameAndKey` at 47/80 = 58.75%.
`Install()` measures 6/6 = 100%.

## 6. Test Execution Metrics

| Metric | Value |
|---|---|
| Test assemblies discovered | 9 |
| Total tests | 6150 |
| Passed | 6150 |
| Failed | 0 |
| Standalone `SVGControl.Test` run | 75/75/0 |
| `SVGControl.Test`-first paired run | 76/76/0 |

## 7. Code Quality Checks

| Check | Result |
|---|---|
| Formatting (CSharpier) | PASS — 0 files need formatting |
| Linting (.NET analyzers) | PASS — 0 errors, 0 new diagnostics |
| Type checking (nullable) | PARTIAL — see G-3 |
| Unit tests | PASS — 6150/6150 |
| File size limit | PASS — all changed files under 500 lines |
| Evidence locations | PASS — no non-canonical evidence paths in the diff |
| Workflow green-run rule | PASS — rule does not fire |

## 8. Gaps and Exceptions

### G-1 — Modified-file line coverage below the 85% floor (FAIL, non-blocking, carried forward)

`SVGControl/SvgRenderer.cs` measures 80.1932% against the `>= 85%` modified-file floor. Unchanged from
the previous three cycles. The entire shortfall sits in six pre-existing members untouched by this
branch; every line this branch changed is covered, so there is no regression on changed lines. Owned
by `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md`. Does not block.

### G-2 — AC-11 designer-load verification (CLOSED this cycle)

Previously the sole blocker. The maintainer executed
`runbooks/verify-winforms-designer-load.runbook.md` at head `db8b59fb` and captured the result at
`evidence/regression-testing/designer-load-2026-08-06T19-47.md`. Reported outcome: the form renders,
no `NullReferenceException`, and the default SVG artwork is visible in the control.

**Verification of currency.** The capture was taken at `db8b59fb`, not at the current head `215a6f7c`.
`git diff --name-only db8b59fb HEAD` returns exactly two paths — the capture itself and `issue.md` —
both Markdown. The observation therefore remains valid for the current head without re-execution.

**Independent assessment of the capture's three declared limitations.** The capture deliberately
records limitations rather than glossing them. Each was examined:

1. *"AC-3's designer-host observability was not exercised."* **Correctly characterized, and it should
   not downgrade AC-3.** AC-3's operative requirement is on the implementation: "the implementation
   must therefore also emit the failure through a channel the designer surfaces ... in addition to the
   `log4net` call. Both channels must carry the exception type and message." That is a statically
   checkable property of the code, not a claim requiring observation inside `devenv.exe`. It is
   satisfied: `SvgRenderer.cs` pairs `logger.Error(detail, error)` with `Trace.TraceError(detail)` at
   all four emission sites (lines 42-43, 63-64, 298-299, 307-308), and `DescribeFailure` at lines 74-79
   composes `error.GetType().FullName + ": " + error.Message`, so both channels carry type and message.
   The capture is right that observation in the designer host would require inducing a parse failure
   there, and right that this sits outside the criterion.

   However, the capture's supporting sentence overstates the evidentiary basis. See **G-10**.

2. *"Attribution of the successful bind is not established."* **Correctly characterized, and
   appropriately hedged.** The three candidate mechanisms named — the binding redirect, the AC-8
   directory-probing fallback, and pre-existing shadow-copy presence — are genuinely
   indistinguishable from a pass/fail render. This does not weaken AC-8, whose criterion is about the
   *implementation* of the fallback (strategy-3 probing exists, tolerates an empty `Location`,
   preserves the re-entrance guard and the public-key-token gate), all of which is verified by the
   eighteen `SvgAssemblyProbeDirectoryTests`. AC-8 does not require proving that the fallback is what
   resolved this particular bind. The capture's chosen verb for AC-8 is "corroborated" rather than
   "proven", which is the accurate strength.

3. *"Open question U-2 remains open."* **Stated more pessimistically than the runbook requires.**
   Runbook step 10 reads: "Optionally, and only if the designer error page reported a failure to load
   `ExCSS`, open `%LOCALAPPDATA%\Microsoft\VisualStudio\<version>\ProjectAssemblies\` ...". No designer
   error page appeared, so the step's stated precondition did not hold and the step was correctly not
   performed. The runbook's own evidence-field list confirms this by qualifying the field as "The step
   10 `ProjectAssemblies` observation, *if performed*". The capture describes it as "was not reported",
   which reads as an operator omission when it was in fact a conditional step whose condition was
   false. The runbook was fully executed with respect to every step whose precondition held. This is a
   disclosure erring toward under-claiming, which is the safe direction; it warrants a wording
   correction, not a downgrade.

**Disposition: G-2 CLOSED. AC-11 evaluates PASS.** Two new non-blocking findings arise from the
capture's contents (G-10, G-11).

### G-9 — New-file coverage floor on `SvgAssemblyResolver.cs` (CLOSED this cycle by ratified exception)

The maintainer authorized extending the ratified `COVERAGE_MEMBER_UNREACHABLE` exception for
`ResolveByNameAndKey` to the file-level floor for `SVGControl/SvgAssemblyResolver.cs`. Recorded in
`artifacts/orchestration/orchestrator-state.json` under `human_interaction.maintainer_waivers[0]`, with
`authorized_by`, `authorization_text`, `scope`, `basis`, `orchestrator_disclosure_at_time_of_request`,
and `effect` fields.

**Independent assessment: the waiver is a legitimate disposition.** Four points were checked:

1. **It is a threshold exception, not a measurement exclusion.** This is the distinction that decides
   whether it breaches `.claude/rules/general-unit-test.md`, which states "No production file may be
   excluded from coverage measurement" and prohibits `exclude` entries matching production paths.
   Verified against the diff: no `[ExcludeFromCodeCoverage]` attribute and no `coverage.config` change
   appears anywhere in the branch. The file remains fully in the denominator — its 66 uncovered lines
   are still counted in the repository-wide 85.4006% figure. **No breach.**

2. **The technical basis is accurate and checkable.** The shortfall is confined to one member; the
   extracted decision logic in `SvgAssemblyProbe` measures 100% line and 100% branch; `Install()`
   measures 6/6. The residual is `Assembly.Load` / `Assembly.LoadFrom` failure wiring on a member the
   CLR invokes only on a failed bind. Driving its strategy-3 branch would require staging a real
   mismatched-key assembly on disk, which `.claude/rules/general-unit-test.md` UT4 prohibits with zero
   approved exceptions. The stated basis holds.

3. **The scope is narrow and explicit.** The waiver names one file, excludes every other file, and
   explicitly does not exempt `Install()`. It does not create a general precedent for host-bound code.

4. **The decision was properly informed.** The orchestrator disclosed before asking that the new-file
   threshold attaches only because it sequenced the resolver extraction first, to relieve
   `SvgRenderer.cs` at 497 of its 500-line limit before a `catch` block was added — the point this
   reviewer sharpened in the previous cycle. Two alternatives were presented and rejected on the
   record: relocating a testable member into the file to lift the ratio (games the metric), and
   reverting the extraction (reinstates the 500-line breach). The maintainer decided with the
   self-inflicted framing visible. That is the correct way to route this decision.

**Disposition: G-9 CLOSED.** It was already non-blocking and did not gate the PR; the authorization
removes it as an open maintainer decision. Three residual gaps are recorded as G-12 and G-13.

### G-10 — AC-11 capture overstates the evidentiary basis for the dual-channel diagnostic (NEW, Medium, non-blocking)

The capture states, in its AC-3 limitation paragraph:

> "The dual-channel behavior is proven by unit tests in `SVGControl.Test`, and the degrade-without-throwing
> behavior is proven by the AC-1 regression tests ..."

The first clause is **false**. Verified:

```
grep -rn "Trace\|log4net\|Listener\|Appender" SVGControl.Test/*.cs   -> no matches
grep -rn "DescribeFailure" SVGControl.Test/*.cs                      -> no matches
```

There is no test in `SVGControl.Test` that installs a `TraceListener`, captures `log4net` output, or
asserts anything about `DescribeFailure`. **No test asserts either diagnostic channel.**

The `Trace.TraceError` and `logger.Error` calls are *executed* by the parse-failure constructor tests —
which is precisely why `DescribeFailure` measures 100% line coverage — but execution is not assertion.
Coverage of a logging call records that the line ran, not that it emitted the right content on the
right channel.

The second clause is accurate: the degrade-without-throwing behavior genuinely is proven by the four
`SvgRendererParseContractTests` constructor tests, which failed with `NullReferenceException` pre-fix
and pass post-fix.

**Why this matters.** The false clause is load-bearing in the capture's argument. It is the fallback
the capture offers when disclaiming the unexercised designer-host observation: the reader is told the
behavior is nonetheless proven by unit tests, when the actual basis is one notch weaker — code
inspection. The conclusion is still sound, because an implementation-shape requirement is legitimately
verifiable by inspection, but the audit trail should state the basis it actually has.

**Correction.** Amend the capture to read: "The dual-channel behavior is verified by code inspection of
the four paired `logger.Error` / `Trace.TraceError` emission sites in `SvgRenderer.cs` and is executed,
though not asserted, by the parse-failure tests; no test captures `Trace` or `log4net` output."

Does not block. Optionally, a `TraceListener`-capturing test would convert the inspection into an
assertion; that is a coverage-uplift item, not a defect in the fix.

### G-11 — AC-11 capture omits two mandatory runbook evidence fields (NEW, Medium, non-blocking)

The runbook's "The artifact must contain, at minimum" list specifies required fields. Checked against
the capture:

| Required field | Present |
|---|---|
| `Timestamp:` matching the filename | Yes |
| `Command:` naming the opened file | Substantively — the wording differs from the prescribed literal, the file is named in the summary |
| `EXIT_CODE: 0` for Pass | No — records `EXIT_CODE: n/a (human procedure, not a command)` |
| `Outcome: Pass \| Partial pass \| Fail` | No literal `Outcome:` field; "**PASS.**" appears in the summary and the classification is unambiguous |
| `Branch:` and the commit SHA built | SHA yes (`db8b59fb`); no `Branch:` line |
| **Visual Studio product name and version, and build configuration (`Debug\|Any CPU`)** | **Absent** |
| **Whether Visual Studio was restarted or the solution reopened after the build** | **Absent** |
| Designer error page text | Not applicable — none appeared |
| Logged exception type and message | Not applicable — required only for a Partial pass |
| Step 9 / step 10 observations | Correctly omitted; both are conditional |

The first four rows are formatting deviations that do not obscure any fact. The last two absent rows
are substantive, and the second of them is the one that matters.

Runbook step 2 directs the operator to close and reopen Visual Studio, and states the reason
explicitly: "This guarantees the designer loads the freshly built `SVGControl.dll`." Because the
capture does not record whether that was done, it does not exclude the designer having loaded a cached
or shadow-copied pre-fix `SVGControl.dll`. A pass under that scenario would still be a pass, because a
pre-fix binary also renders successfully whenever the `ExCSS` bind happens to succeed.

**This does not overturn AC-11**, for a reason that is independently documented: the same environment
demonstrably produced the `NullReferenceException` pre-fix — that observation is the bug report in
`issue.md` — and now produces a clean render. The change in outcome across the fix is evidence even
without a controlled in-session comparison. But the inference spans two sessions rather than one
recorded prerequisite, and the missing field is exactly the field the runbook added to close that gap.
It also compounds limitation 2 of the capture, the unestablished attribution.

**Correction.** Append a short addendum to the capture recording the Visual Studio product name and
version, the build configuration used, and whether Visual Studio was restarted after the build. If the
maintainer cannot now recall the restart, record that as unknown rather than assuming it.

Does not block.

### G-12 — The G-9 waiver is recorded only in a gitignored file (NEW, Medium, non-blocking, recommended before merge)

`artifacts/orchestration/orchestrator-state.json` is untracked and ignored:

```
git check-ignore -v artifacts/orchestration/orchestrator-state.json
  -> .gitignore:57:artifacts/    artifacts/orchestration/orchestrator-state.json
git ls-files --error-unmatch artifacts/orchestration/orchestrator-state.json
  -> error: pathspec ... did not match any file(s) known to git
```

The waiver therefore exists nowhere in the committed record. It will not appear in the pull request, it
will not survive a fresh clone, and the next coverage audit of this file will re-derive G-9 from
scratch with no record that it was ever adjudicated.

This runs against the repository's own stated intent for coverage exemptions. `CLAUDE.md` UT2 specifies
the exemption mechanism as `[ExcludeFromCodeCoverage]` attributes "in source code (**reviewable in
PRs**)" or `coverage.config` assembly-level excludes — both deliberately visible to a reviewer. A
waiver held only in an untracked file inverts that property.

**Correction.** Transcribe the waiver into the committed record before merge: the most natural home is
a short subsection under AC-5 in `issue.md`, alongside the existing
`COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgAssemblyResolver.ResolveByNameAndKey` note, recording the
authorizing maintainer, the date, the scope sentence, and the basis. This is a documentation edit, not
a code change.

### G-13 — The G-9 residual has no follow-up owner (NEW, Low, non-blocking)

G-1's residual is properly owned by `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md`.
G-9's is not:

```
grep -n "SvgAssemblyResolver\|ResolveByNameAndKey" docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md
  -> no matches
```

The waived file is named in no follow-up item, so the 66 uncovered lines have no path back to review
should a future change make them testable — for example, if a host-level seam is introduced for the
`AssemblyResolve` wiring. Recommend adding `SVGControl/SvgAssemblyResolver.cs` to the existing
potential-feature file with a note that its current shortfall is waived rather than accepted
indefinitely.

### G-14 — The waiver's `runbook_path` is a placeholder in an undefined schema key (NEW, Low, informational)

The `maintainer_waivers[0].runbook_path` field cites the designer-load runbook, which is unrelated to a
coverage adjudication. The entry discloses this candidly in an adjacent `runbook_path_note`, so it is
not misleading. Two observations:

1. `.claude/rules/orchestrator-state.md` defines the "exception requires `runbook_path`" invariant over
   `human_interaction.requirements[]`. This entry sits in `human_interaction.maintainer_waivers[]`, a
   key the rule does not define, so the invariant very likely does not reach it and the placeholder was
   probably unnecessary.
2. `scripts/dev_tools/validate_orchestrator_state.py` does not exist in this repository
   (`ls` returns "No such file or directory"), so no tooling here validates the block either way.

No action required beyond noting that `maintainer_waivers` is an undocumented extension to the
checkpoint shape. If the block is intended to persist, `.claude/rules/orchestrator-state.md` should
describe it.

### G-3 — Mandated nullable gate is non-probative (PARTIAL, carried forward)

The `CLAUDE.md`-mandated solution-wide nullable invocation returns exit 0 while compiling 0
`CoreCompile` targets, so its exit status carries no information about the changed projects. Mitigated
this feature by forced per-project rebuilds. Repository-level concern; recommend a per-changed-project
gate. Does not block.

### G-4 — Test-file location diverges from the mirrored-`tests/` rule (accepted, pre-existing)

`.claude/rules/general-unit-test.md` requires tests in a mirrored `tests/` tree. `SVGControl.Test`
sits beside the project, matching all eight sibling test projects. Repository-wide convention
predating this branch; correcting it here would be an out-of-mode refactor. Does not block.

### G-5 — MCP template and validator assets unavailable (documented assumption)

`resolve_policy_audit_template_asset` is not present in this session's tool surface, and no
`validate_orchestration_artifacts` tool is callable. These artifacts follow the heading structure of
the three prior cycles in this feature folder, which is the best available proxy for the canonical
templates. Documented as an assumption per the no-questions constraint.

### G-6 — Reviewer side effect, disclosed

This agent modified `artifacts/pr_context.summary.txt` to correct the collector's file classification,
as described under PR-Context Artifact Corrections. That file is a generated review input, not source
or policy. No source, test, or policy file was modified by this review.

### G-7 — PR-context collector defects (carried forward, corrected in place)

Two defects persist at this head: C# files classified as documentation (`Core logic changes: 0 files`),
and spurious close candidates including `#AC-1` .. `#AC-11` and `#DE06-4337`. Corrected in place for
this review. The collector itself remains uncorrected and will reproduce both on the next run.

### G-8 — Test order-dependence (CLOSED in cycle 2)

Closed by the explicit `ExCSS` reference added to `SVGControl.Test.csproj`. Verified by the reviewer's
own standalone run at 75/75/0.

## 9. Summary of Changes

The branch fixes a `NullReferenceException` raised from the byte-array `SvgRenderer` constructors when
`SvgDocument.Open` cannot produce a document. The swallowing `catch (Exception) { return null; }` is
replaced by a `TryGetSvgDocument` boundary that logs the cause on two channels and returns a result the
caller must inspect; the constructors degrade to `Size.Empty` instead of dereferencing null. A
directory-probing `AssemblyResolve` fallback was extracted into `SvgAssemblyResolver`, with its pure
decision logic in `SvgAssemblyProbe`. `SVGControl.Test` was wired into the solution, its packages
pinned and restored, and an explicit `ExCSS` reference added to make the suite order-independent.

## 10. Compliance Verdict

| Area | Verdict |
|---|---|
| General Unit Test Policy | PASS, with G-4 pre-existing |
| General Code Change Policy | PASS |
| C# Code Change Policy | PASS, with G-3 partial |
| C# Unit Test Policy | PASS |
| Coverage — C# repository-wide | PASS |
| Coverage — file-level | Two FAIL rows, both dispositioned non-blocking (G-1 owned, G-9 waived) |
| Evidence locations | PASS |
| Workflow green-run rule | PASS, rule does not fire |
| Acceptance criteria | 11 of 11 PASS |
| **Blocking findings** | **0** |

**Overall: PASS. The feature is ready for a pull request.** Four documentation corrections (G-10, G-11,
G-12, G-13) are recommended before merge; none gates it, and G-12 is the one worth doing first.

## Appendix A: Coverage Verification Detail

Coverage was verified from pre-existing artifacts. No coverage generation was re-run by this agent.

**Canonical artifact.** `artifacts/csharp/coverage.xml`, a JaCoCo-shaped conversion of the feature's
Cobertura output, containing:

```xml
<counter type="LINE" missed="15989" covered="93529" />
<counter type="BRANCH" missed="5842" covered="21576" />
```

- Line: 93529 / 109518 = **85.4006%** against `>= 85%` → **PASS**
- Branch: 21576 / 27418 = **78.6928%** against `>= 75%` → **PASS**

**Feature evidence artifact.** `evidence/qa-gates/coverage-delta.2026-08-05T05-00.md` reports the same
figures with a baseline comparison, computed by the same per-`<line>`-descendant method so the
comparison is like-for-like.

**Reconciliation.** The evidence artifact's baseline column reads 93539 covered lines; the canonical
artifact reads 93529. The ten-line difference is a measured delta between the two runs on an identical
denominator of 109518, documented in the delta artifact as `−10 covered, 0 total, −0.0091 pts`. Both
values clear the floor with margin, and no `.cs` file changed between the two runs.

**New-file and modified-file tiers.** Reported in section 1.2.2. `SvgAssemblyProbe.cs` PASS at 100%/100%;
`SvgAssemblyResolver.cs` FAIL at 61.6279%/53.8462%, dispositioned non-blocking under the ratified
exception; `SvgRenderer.cs` FAIL at 80.1932% against the modified-file floor with no regression on
changed lines, owned by the potential-feature follow-up.

**Languages with zero changed files.** TypeScript, Python, and PowerShell have no changed files in the
branch diff, so no artifact is required and none was inspected for them.

## Appendix B: Toolchain Commands Reference

Commands whose recorded results this audit inspected. This agent ran only the read-only verification
commands in the second block.

Executor-run toolchain (results inspected, not re-executed):

```
dotnet tool run csharpier .
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
vstest.console.exe <9 test assemblies> /EnableCodeCoverage
```

Reviewer-run verification (read-only):

```
git merge-base HEAD origin/main
git rev-parse HEAD
git diff --name-only 69e675d0 HEAD
git diff --name-only db8b59fb HEAD
git diff --numstat ce0c91e6...HEAD
git diff --name-only ce0c91e6...HEAD | grep -E '^\.github/workflows/|^scripts/benchmarks/|^\.github/actions/'
git diff --name-only ce0c91e6...HEAD | grep -E '^artifacts/(baselines|qa|evidence|coverage)/'
git diff ce0c91e6...HEAD -- '*.cs' '*.config' '*.csproj' | grep "ExcludeFromCodeCoverage\|coverage.config"
git check-ignore -v artifacts/orchestration/orchestrator-state.json
grep -rn "Trace|log4net|Listener|Appender|DescribeFailure" SVGControl.Test/*.cs
grep -n "SvgAssemblyResolver|ResolveByNameAndKey" docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md
```
