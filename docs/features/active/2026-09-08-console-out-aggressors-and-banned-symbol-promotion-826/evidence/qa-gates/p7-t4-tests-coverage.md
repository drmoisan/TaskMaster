# Final QA loop, toolchain step 4 — measured test and coverage run (issue #826, [P7-T4])

Timestamp: 2026-09-09T19-40

Command:

```
dotnet-coverage collect --settings coverage.config --output coverage/826-raw/p7-t4.cobertura.xml --output-format cobertura -- <vstest.console.exe> <the nine test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/Logger:trx;LogFileName=p7-t4.trx" /ResultsDirectory:coverage/826-raw/p7-t4 /TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser
```

`<vstest.console.exe>` is resolved through `vswhere`. Run as one `pwsh -NoProfile -Command` block
carrying the plan's C2 preamble branch guard.

This is character-for-character the same command form as [P0-T9], including `--settings coverage.config`,
so the [P7-T6] baseline-versus-post-change comparison is like for like. The reason that flag is present,
and the fact that it excludes only third-party modules, are recorded in
`<FEATURE>/evidence/baseline/baseline-tests-coverage.md`.

EXIT_CODE: 0

## Output Summary

TRX `ResultSummary/Counters`, read from `coverage/826-raw/p7-t4/p7-t4.trx`:

| Counter | Value | Baseline ([P0-T9]) |
|---|---|---|
| total | 7192 | 7190 |
| executed | 7192 | 7190 |
| passed | 7192 | 7190 |
| failed | **0** | 0 |
| notExecuted | 0 | 0 |

The total rises by exactly 2, which is the two test methods [P2-T2] added. Nothing else changed count, so
the 33-file item-1 sweep broke no existing test.

Both new tests appear in the TRX with outcome `Passed`, matched on `UnitTestResult/@testName`:

- `GetTableInViewAsync_TimeoutSourceThrowsTimeout_EntersTimeoutCatchAndRetriesOnce` => Passed
- `GetTableInViewAsync_TimeoutSourceThrowsTaskCanceled_EntersCancelCatchElseAndRetriesOnce` => Passed

Root Cobertura figures, read from `coverage/826-raw/p7-t4.cobertura.xml`:

| Attribute | Value | Baseline |
|---|---|---|
| `line-rate` | 0.8613286095646392 | 0.8611544074577819 |
| `lines-valid` | 201534 | 201454 |
| `lines-covered` | 173587 | 173483 |

Post-change root line coverage is 86.1329 percent, above the `>= 85%` floor in
`.claude/rules/general-unit-test.md` and above the baseline 86.1154 percent.

Per-file line rate for `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`: **0.907473**
(255 covered of 281 distinct lines), against a baseline of 0.846975 (238 of 281). The denominator is
unchanged at 281 in both runs, so the rise is a genuine coverage gain rather than a denominator artefact.

The counting method is the one [P0-T9] states and is reproduced here unchanged: nine `<class>` elements
carry a `filename` ending with `OlTableExtensions.TableAccess.cs`, line numbers are pooled across all
nine into a map keyed by line number, hits are summed per line number, the denominator is the count of
distinct line numbers and the numerator is the count whose summed hits exceed zero. The state-machine
class carrying the two changed lines, `UtilitiesCS.OlTableExtensions.<GetTableInViewAsync>d__32`, has its
own `line-rate` rise from 0.6533333333333333 to **0.88**.

### Changed-line hit counts

The two changed lines were located at observation time with
`Select-String -LiteralPath $Tac -CaseSensitive -SimpleMatch "timed out on try"` against the post-change
file, which reported lines **96** and **115**. No line number is carried from the plan document.

PostChangeChangedLineHits: line 96 = 2; line 115 = 2

Both lines emitted a `<line number="N">` element, so the "no line element emitted" annotation rule does
not apply to either. The value 2 rather than 1 is the pooled figure: each line appears once under
`class/lines` and once under `method/lines`, and the counting method sums hits per line number. The
baseline applied the identical rule, which is why its figures were 0 and 2 for the same two lines.

## Recorded defect in the extraction, and its correction

The first extraction pass reported both changed lines as `no line element emitted`, which would have made
AC15 unsatisfiable. That reading was wrong and the cause was in the extraction script, not in the
coverage document.

`Select-String`'s `MatchInfo.LineNumber` property is typed `System.UInt64` on this PowerShell version,
while the map keys were built as `System.Int32` from `[int]$ln.GetAttribute("number")`.
`Hashtable.ContainsKey` compares boxed objects, and a boxed `UInt64` 96 is not equal to a boxed `Int32`
96, so the lookup missed every key. Verified directly: `$n.GetType().FullName` reported
`System.UInt64` for both changed line numbers, while `ContainsKey(96)` and `ContainsKey(115)` with
integer literals both returned `True` with values 2 and 2.

The [P0-T9] baseline extraction was not affected, because it compared with
`[int]$ln.GetAttribute("number") -eq $n`, and PowerShell's `-eq` operator performs numeric coercion where
`Hashtable.ContainsKey` does not. Its recorded figures of 0 and 2 are correct and needed no revision.

Recorded rather than absorbed, because a false `no line element emitted` reading is indistinguishable
from a genuine loss of coverage unless the mechanism is named.

Output Summary: 7192 of 7192 tests passed with 0 failed, both new test methods passed, root line coverage
rose from 86.1154 to 86.1329 percent, the per-file rate for the table-access file rose from 0.846975 to
0.907473, and both changed production lines carry a non-zero post-change hit count of 2. Toolchain step 4
passes.
