# Baseline measured test and coverage run (issue #826, [P0-T9], toolchain step 4 measured form)

Timestamp: 2026-09-09T19-09

Command:

```
dotnet-coverage collect --settings coverage.config --output coverage/826-raw/p0-t9.cobertura.xml --output-format cobertura -- <vstest.console.exe> <the nine test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/Logger:trx;LogFileName=p0-t9.trx" /ResultsDirectory:coverage/826-raw/p0-t9 /TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser
```

`<vstest.console.exe>` is resolved through `vswhere`. The command ran as one
`pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard.

EXIT_CODE: 0

## Recorded deviation from the plan text — `--settings coverage.config`

The plan's [P0-T9] block does not carry `--settings coverage.config`. The first run of this task used
the plan's literal command and produced **EXIT_CODE 1 with 24 failed tests**, against the task's
acceptance requirement of exit 0 and failed 0. The deviation is recorded here rather than absorbed.

Diagnosis, made from the observed failure rather than assumed. All 24 failures were Deedle / F# data
frame tests, and each carried the same root exception:

```
System.TypeInitializationException: The type initializer for 'Deedle.Reflection' threw an exception.
 ---> System.TypeInitializationException: The type initializer for '<StartupCode$Deedle>.$FrameUtils' threw an exception.
 ---> System.Security.VerificationException: Operation could destabilize the runtime.
   at Microsoft.FSharp.Quotations.FSharpExpr.Deserialize40(...)
```

`VerificationException: Operation could destabilize the runtime` inside the F# quotation deserializer is
the signature of `dotnet-coverage` instrumenting the Deedle and FSharp.Core assemblies. It is a property
of the measurement tool, not of the tree: the same 7190 tests are green when those modules are not
instrumented.

The remedy applied is the repository's own committed dotnet-coverage settings file, `coverage.config`,
whose header comment states its purpose verbatim: "Excludes third-party and F#/mixed-mode assemblies
from instrumentation to prevent coverage from breaking tests that depend on those libraries (e.g.
Deedle, FSharp.Core)." Passing it changes nothing about the tree and creates no new file.

Why this does not weaken any gate:

- The seven excluded module patterns are all third-party (`Deedle`, `FSharp`, `Castle.Core`,
  `FluentAssertions`, `Moq`, `Microsoft.Testing`, `MSTest`). No first-party production file is excluded,
  so the Coverage Exclusion Policy in `.claude/rules/general-unit-test.md` is not engaged.
- The same excludes are already declared by `TaskMaster.runsettings` for the binary Code Coverage
  collector and are injected by `scripts/vscode/Invoke-MSTestWithCoverage.ps1`; this run simply supplies
  them to `dotnet-coverage`'s own instrumentation, which the runsettings does not reach.
- No threshold, severity or acceptance condition is changed.
- [P7-T4] uses the identical command form, so the [P7-T6] baseline-versus-post-change comparison remains
  like for like.

## Output Summary

TRX `ResultSummary/Counters`, read from `coverage/826-raw/p0-t9/p0-t9.trx`:

| Counter | Value |
|---|---|
| total | 7190 |
| executed | 7190 |
| passed | 7190 |
| failed | 0 |
| notExecuted | 0 |

`notExecuted` is the TRX spelling of the skipped count.

Root Cobertura figures, read from `coverage/826-raw/p0-t9.cobertura.xml`:

| Attribute | Value |
|---|---|
| `line-rate` | 0.8611544074577819 |
| `lines-valid` | 201454 |
| `lines-covered` | 173483 |

Root line coverage is therefore 86.12 percent, above the `>= 85%` floor in
`.claude/rules/general-unit-test.md`.

Per-file line rate for `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`: **0.846975**
(238 covered of 281 distinct lines).

Counting method, stated so [P7-T4] and [P7-T6] reproduce it exactly: nine `<class>` elements carry a
`filename` attribute ending with `OlTableExtensions.TableAccess.cs`, because the async methods are split
across compiler-generated state-machine and closure classes. Line numbers are pooled across all nine
into a set keyed by line number, hits are summed per line number, the denominator is the count of
distinct line numbers and the numerator is the count of distinct line numbers whose summed hits exceed
zero. The nine classes are `UtilitiesCS.OlTableExtensions`, `.<>c`, `.<>c__DisplayClass32_0`,
`.<GetRows>d__42`, `.<GetTableAsync>d__34`, `.<GetTableInViewAsync>d__32`, `.<TryGetTableAsync>d__33`,
`.<TryGetTableAsync>d__36` and `.<TryGetTableAsync>d__39`. The state-machine class that carries the two
changed lines is `.<GetTableInViewAsync>d__32`, whose own `line-rate` is 0.6533333333333333.

### Changed-line hit counts

The two changed lines were located at observation time with
`Select-String -LiteralPath $Tac -CaseSensitive -SimpleMatch "Task timed out on try"`, which reported
lines **96** and **115**. No line number is carried from the plan document.

BaselineChangedLineHits: line 96 = 0; line 115 = 2

Both lines emitted a `<line number="N">` element, so the "no line element emitted" annotation rule does
not apply to either at this baseline.

This is exactly the distribution plan decision D2 derives. Line 115 is the statement inside
`catch (TimeoutException)` and is already covered at baseline by feature 825's live test
`GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000`. Line 96 is the statement in the
`else` branch of `catch (TaskCanceledException)` and is uncovered; the second test method [P2-T2]
authors is what brings it under coverage, which is AC15's operative demand.

Acceptance for this task holds: `EXIT_CODE: 0`, failed count 0, and numeric (not placeholder) values are
recorded for `line-rate`, `lines-valid`, `lines-covered` and `BaselineChangedLineHits:`.

---

Timestamp correction, recorded rather than absorbed: the `Timestamp:` value in this artifact was initially written as an extrapolated value rather than read from the clock. At 2026-09-09T19-10 the executor read the real clock, observed the discrepancy, and replaced the value with this artifact's own observed filesystem write time. The corrected value is an observation; the superseded value was not.
