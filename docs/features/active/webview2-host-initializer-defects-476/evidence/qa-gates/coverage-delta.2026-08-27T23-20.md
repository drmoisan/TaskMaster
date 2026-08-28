# Coverage Delta and Threshold Comparison ([P4-T5])

Timestamp: 2026-08-27T23-20

Command:

```
python <scratchpad>/cov.py  <feature>/evidence/qa-gates/coverage-postchange.cobertura.xml WebView2BreadcrumbHost WebView2CoreInitializer IWebViewCoreInitializer
python <scratchpad>/cov3.py <feature>/evidence/qa-gates/coverage-postchange.cobertura.xml WebView2BreadcrumbHost
python <scratchpad>/cov3.py <feature>/evidence/qa-gates/coverage-postchange.cobertura.xml WebView2CoreInitializer
head -c 300 <feature>/evidence/baseline/coverage-baseline.cobertura.xml
```

The two helper scripts were written to the session scratchpad outside the repository and are not
retained under `evidence/`. They parse the Cobertura document with `xml.etree.ElementTree`,
deduplicate `<line>` elements by line number within a `filename` (so a line repeated under both a
`<method>` and the class `<lines>` is counted once), and aggregate every `<class>` sharing a
`filename` so compiler-generated async and closure classes are not counted as separate denominators.

EXIT_CODE: 0

## Output Summary

Repository-wide line coverage rose from 85.1302% to 85.1435% (+0.0133 pp) and branch coverage from
79.1973% to 79.2018% (+0.0045 pp). Both blocking repository floors that the Phase 0 baseline met are
still met. The change-scoped no-regression gate is met provably: all three in-scope production files
were `ABSENT` from the pre-change document, so no measured line lost coverage. The change-scoped
**90% line coverage floor on newly measured members is NOT met for four members**:
`NavigateToString` 62.50%, `DetachCore` 66.67%, `CreateEnvironmentAsync` 83.33%, and
`EnsureCoreWebView2Async` 66.67%. Details and the specific line numbers are in sections (c) and (d).

---

## (a) Repository-wide rates

Read from the root `<coverage>` element of each document.

| Document | line-rate | branch-rate | lines-covered | lines-valid | branches-covered | branches-valid |
| --- | --- | --- | --- | --- | --- | --- |
| `evidence/baseline/coverage-baseline.cobertura.xml` | 0.851302 | 0.791973 | 54382 | 63881 | 12925 | 16320 |
| `evidence/qa-gates/coverage-postchange.cobertura.xml` | 0.851435 | 0.792018 | 54514 | 64026 | 12959 | 16362 |
| **Signed delta** | **+0.000133** | **+0.000045** | +132 | +145 | +34 | +42 |

As percentages: line 85.1302% to 85.1435%, **delta +0.0133 percentage points**; branch 79.1973% to
79.2018%, **delta +0.0045 percentage points**. Both deltas are positive.

**Which denominator this is.** These are the unfiltered repository-wide figures from the root
`<coverage>` element of the Cobertura document the wrapper emits and Koverage post-processes: every
`<package>` the collector produced, first-party and vendored alike, with no production-only filter
applied. The Phase 0 baseline recorded its figures from the same element of a document produced by
the same command, so the two are directly comparable and the signed delta above is a like-for-like
measurement. No first-party-filtered denominator is used anywhere in this artifact, because a
filtered figure would not be comparable against the recorded baseline.

Note on measurement stability: per-file `lines-covered` in this repository drifts by a small number
of lines between runs on an identical tree while `lines-valid` does not, so no gate in this artifact
is built on an exact covered-line count. The gates are built on rates and on the identity of the
specific uncovered lines.

---

## (b) Per-file rates for the two measured production files

Aggregated by the Cobertura `filename` attribute. `IWebViewCoreInitializer.cs` is omitted from the
table because it declares an interface only and has no executable body to instrument; it was `ABSENT`
before the change and is `ABSENT` after it, which is correct rather than a regression.

| File | Pre-change line rate | Pre-change branch rate | Post-change line rate | Post-change branch rate | Post-change lines | Post-change branches |
| --- | --- | --- | --- | --- | --- | --- |
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` | ABSENT | ABSENT | 0.883495 (88.3495%) | 0.692308 (69.2308%) | 91 / 103 | 18 / 26 |
| `QuickFiler/Viewers/WebView2CoreInitializer.cs` | ABSENT | ABSENT | 0.777778 (77.7778%) | 0.666667 (66.6667%) | 14 / 18 | 4 / 6 |

The pre-change `ABSENT` readings are the values recorded in
`evidence/baseline/baseline-perfile-coverage.2026-08-27T20-06.md`. Both types carried a class-level
`[ExcludeFromCodeCoverage]` before the change, so the collector emitted no entry for either. Phase 3
removed those class-level attributes, which is why both files now appear.

---

## (c) Line coverage of the members newly entering measurement

Every member the task enumerates, with the exact uncovered line numbers. `PostMessageJson` is
reported including its `PostCore` local function, which the compiler emits as a separate method and
which is part of that member's source text.

| Member | File | Covered / valid | Line rate | Uncovered lines |
| --- | --- | --- | --- | --- |
| `.ctor(WebView2, IWebViewCoreInitializer)` | host | 1 / 1 | 100.00% | none |
| `.ctor(WebView2, IWebViewCoreInitializer, BreadcrumbUiDispatcher)` | host | 22 / 22 | 100.00% | none |
| `IsAttached` (getter) | host | 1 / 1 | 100.00% | none |
| `HasUiDispatcher` (getter) | host | 1 / 1 | 100.00% | none |
| `IsCoreInitialized` (getter) | host | 1 / 1 | 100.00% | none |
| `NavigateToString` | host | 5 / 8 | **62.50%** | 161, 162, 163 |
| `PostMessageJson` (incl. `PostCore`) | host | 17 / 18 | 94.44% | 207 |
| `InitializeAsync` | host | 18 / 20 | 90.00% | 242, 243 |
| `DetachCore` | host | 6 / 9 | **66.67%** | 316, 317, 318 |
| `CreateEnvironmentAsync` | initializer | 10 / 12 | **83.33%** | 55, 56 |
| `EnsureCoreWebView2Async` | initializer | 4 / 6 | **66.67%** | 85, 86 |
| **Aggregate over the eleven rows above** | | **86 / 99** | **86.87%** | |

Three further members entered measurement when the class-level exemption was removed although the
task does not enumerate them. They are recorded for completeness and are not gated by this task.

| Member | File | Covered / valid | Line rate | Uncovered lines |
| --- | --- | --- | --- | --- |
| `OnControlDisposed` | host | 13 / 13 | 100.00% | none |
| `.cctor` (static field initializers) | host | 6 / 6 | 100.00% | none |
| `LogDispatchFailure` | host | 0 / 3 | 0.00% | 278, 279, 280 |

### What each uncovered line is

- **161-163** is the inline fallback inside `NavigateToString` taken when no dispatcher has been
  installed. Line 162 is `ForwardNavigateToString(html);`, which reaches
  `WebView2.NavigateToString` and therefore the Evergreen runtime.
- **207** is `ForwardWebMessage(core, json);` inside `PostCore`, reachable only when
  `_control.CoreWebView2` is non-null, which requires a live runtime.
- **242-243** is the `ArgumentNullException` throw for a null `uiSyncContext` in `InitializeAsync`.
  No test in this feature passes null there. This is the one uncovered pair that is not host-bound.
- **316-318** is `core.WebMessageReceived -= OnWebMessageReceived;` inside `DetachCore`, reachable
  only when `_control.CoreWebView2` is non-null.
- **278-280** is the body of `LogDispatchFailure`, reachable only when a dispatched callback throws,
  which in this host means an SDK forward throwing on the UI thread.
- **55-56** is `return ForwardCreateEnvironmentAsync(cacheFolder, options);` and the closing brace of
  `CreateEnvironmentAsync`, reached only after both guards pass, at which point the call creates a
  user-data folder on disk and starts the Evergreen runtime.
- **85-86** is `return ForwardEnsureCoreWebView2Async(control, environment);` and the closing brace of
  `EnsureCoreWebView2Async`, likewise reached only on the SDK-bound happy path.

Eleven of the thirteen uncovered lines are statements that reach the WebView2 SDK, or the brace that
closes such a statement's method. The remaining two, 242-243, are the `InitializeAsync` null guard.

---

## (d) Thresholds applied, and the branch of Decisions Record item 8 taken for each

### Repository-wide floors (Decisions Record item 8)

`baseline-4-tests-coverage.2026-08-27T20-05.md` recorded the Phase 0 position against every floor.

| Floor | Source | Phase 0 baseline | Baseline met it? | Branch applied | Post-change figure | Result |
| --- | --- | --- | --- | --- | --- | --- |
| Line >= 80% | `CLAUDE.md` UT2 | 85.1302% | Yes | **Blocking** | 85.1435% | **MET** |
| Line >= 85% | `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md` | 85.1302% | Yes | **Blocking** | 85.1435% | **MET**, margin 0.1435 pp |
| Branch >= 75% | `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md` | 79.1973% | Yes | **Blocking** | 79.2018% | **MET** |

Because the Phase 0 baseline met every floor, the blocking branch of Decisions Record item 8 applies
to all three and the non-blocking branch applies to none. All three are met, and each moved upward.

**Threshold conflict, recorded as the task requires.** `CLAUDE.md` UT2 sets a repository-wide floor
of 80% line coverage with 90% for new code and states no branch floor.
`.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` set a uniform 85% line and
75% branch floor across tiers T1 through T4. The two documents disagree on the repository-wide line
floor, 80 versus 85, and on whether a branch floor exists at all. This artifact applies the stricter
of each pair, so the conflict does not change any verdict here: 85.1435% clears both line floors and
79.2018% clears the branch floor. The conflict is reported again in the Phase 5 status summary.

### Change-scoped blocking gates

| Gate | Result |
| --- | --- |
| No reduction in coverage on any line this change modified | **MET.** All three in-scope production files were `ABSENT` from the pre-change Cobertura document, so no line of theirs carried a pre-change measurement that could fall. No file outside those three is in this change's production classification, so no previously measured line was modified at all. |
| Newly measured members at or above 90% line coverage | **NOT MET for four members.** Named with their figures: `NavigateToString` **62.50%** (5/8), `DetachCore` **66.67%** (6/9), `CreateEnvironmentAsync` **83.33%** (10/12), `EnsureCoreWebView2Async` **66.67%** (4/6). Seven of the eleven enumerated members are at or above 90%: both constructors, `IsAttached`, `HasUiDispatcher` and `IsCoreInitialized` at 100% each, `PostMessageJson` at 94.44%, and `InitializeAsync` at exactly 90.00%. The aggregate across all eleven is 86.87%, also below 90%. Read either per member or in aggregate, this gate fails. |

### Why the four shortfalls were recorded rather than remediated

Every uncovered line in the four members named above is a statement that reaches the WebView2 SDK:
line 162 forwards to `WebView2.NavigateToString`, line 317 unsubscribes from a `CoreWebView2` that
only a live runtime can produce, line 55 calls `ForwardCreateEnvironmentAsync`, and line 85 calls
`ForwardEnsureCoreWebView2Async`. Covering any of them requires the external Evergreen WebView2
runtime, which `.claude/rules/general-unit-test.md` forbids a unit test from depending on and which
this plan forbids in the standing rules for Phase 1. The design this plan mandates extracts the SDK
body into a small `[ExcludeFromCodeCoverage]` forward and deliberately leaves the call statement
inside the measured member; that call statement is the residual uncovered line. The shortfall is
therefore structural to the mandated design rather than a gap a further test could close within
policy, and no task in this plan authorizes a redesign. It is recorded here with member and figure as
this task's acceptance directs, is carried into the Phase 5 status summary, and is reported to the
orchestrator as a blocking finding.
