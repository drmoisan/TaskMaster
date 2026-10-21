# Phase 1 — Acceptance-Criteria Inventory

Timestamp: 2026-09-13T23-19

AC source: `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/spec.md`,
section `## Acceptance Criteria`. Work mode is `full-bug`, so `spec.md` is the sole
acceptance-criteria source and `user-story.md` is correctly absent.

## Inventory

| ID | spec.md line |
|---|---|
| AC1 | 473 |
| AC2 | 477 |
| AC3 | 481 |
| AC4 | 484 |
| AC5 | 491 |
| AC6 | 495 |
| AC7 | 499 |
| AC8 | 504 |
| AC9 | 509 |
| AC10 | 515 |
| AC11 | 521 |
| AC12 | 528 |
| AC13 | 534 |
| AC14 | 536 |
| AC15 | 543 |
| AC16 | 545 |
| AC17 | 547 |
| AC18 | 550 |
| AC19 | 554 |

Nineteen entries, matching the count recorded in the `[P0-T1]` artifact's
`Requirements Sources:` section.

## Spot Check

Each of the nineteen named lines was read and confirmed to begin with the six characters
`- [ ] `:

```
AC1 line 473 UNCHECKED=True
AC2 line 477 UNCHECKED=True
AC3 line 481 UNCHECKED=True
AC4 line 484 UNCHECKED=True
AC5 line 491 UNCHECKED=True
AC6 line 495 UNCHECKED=True
AC7 line 499 UNCHECKED=True
AC8 line 504 UNCHECKED=True
AC9 line 509 UNCHECKED=True
AC10 line 515 UNCHECKED=True
AC11 line 521 UNCHECKED=True
AC12 line 528 UNCHECKED=True
AC13 line 534 UNCHECKED=True
AC14 line 536 UNCHECKED=True
AC15 line 543 UNCHECKED=True
AC16 line 545 UNCHECKED=True
AC17 line 547 UNCHECKED=True
AC18 line 550 UNCHECKED=True
AC19 line 554 UNCHECKED=True
TOTAL_UNCHECKED_IN_AC_SECTION=19
```

The final line is an independent count of unchecked checkbox items across the whole
`## Acceptance Criteria` section body. It equals 19, so the inventory is exhaustive: there is
no unchecked criterion in that section that the nineteen named line numbers omit.

No acceptance criterion is checked off in Phases 0 through 2. The plan assigns every
check-off to Phase 6 tasks `[P6-T6]` through `[P6-T24]`.

## Evidence-Directory Correction Check

Recorded per `[P1-T1]`.

```
COVERAGE_DIR_HITS=0
QA_GATES_DIR_HITS=2
```

`COVERAGE_DIR_HITS` is 0 and `QA_GATES_DIR_HITS` is 2, which is greater than 1. The
planner's substitution of the canonical `evidence/qa-gates/` kind for the non-canonical
`evidence/coverage/` is present, and no `evidence/coverage/` reference remains.

## Non-Canonical Evidence Path Sweep

Recorded per `[P1-T3]`. The sweep covers `spec.md` alone and deliberately never covers the
plan file, which quotes all eight non-canonical literals in its own search lists.

```
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/spec.md artifacts/baselines/ HITS=0
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/spec.md artifacts/baseline/ HITS=0
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/spec.md artifacts/qa/ HITS=0
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/spec.md artifacts/qa-gates/ HITS=0
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/spec.md artifacts/evidence/ HITS=0
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/spec.md artifacts/coverage/ HITS=0
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/spec.md evidence/coverage/ HITS=0
docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/spec.md evidence/post-change/ HITS=0
```

Exactly eight lines were emitted and every line ends with `HITS=0`.

## Scope Lock

Recorded per `[P1-T4]`.

Reproduced verbatim from this plan's `## Authorised Write Set`, items 1 to 13.

Production:

1. `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs` (new)
2. `UtilitiesCS/UtilitiesCS.csproj` (one new `Compile Include` item)
3. `TaskMaster/ThisAddIn.cs` (add `static ThisAddIn()`)
4. `TaskMaster/app.config` (one new `dependentAssembly` block)

Tests:

5. `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs` (new)
6. `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (one new `Compile Include` item)
7. `TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs` (new)
8. `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs` (new)
9. `TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs` (new)
10. `TaskMaster.Test/TaskMaster.Test.csproj` (three new `Compile Include` items)

Documents and evidence:

11. `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/spec.md`
12. `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/plan.2026-09-13T18-22.md`
13. Any path under
    `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/`

No file outside this list, and outside the inherited-path rule, may be created or modified by this plan.

Host Substitution: HOST=TaskMaster.Test

The `Decision:` field of `evidence/baseline/write-set-decision.2026-09-13T18-22.md` reads
`HOST=TaskMaster.Test`, so write-set item 14 is not taken and is not reproduced here. Items 7
through 10 stand exactly as listed above.

## AC10 Revision R2 Rewrite:

Timestamp: 2026-09-14T09-42

Command:

```
pwsh -NoProfile -Command '
$p = "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/spec.md"
$lines = @(Get-Content -LiteralPath $p)
Write-Output ("AC10_LINE_515_PREFIX=[" + $lines[514].Substring(0,6) + "]")
Write-Output ("AC11_LINE_521_PREFIX=[" + $lines[520].Substring(0,6) + "]")
Write-Output ("AC_HEADING_HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "## Acceptance Criteria").Count)
Write-Output ("DEEPEST_HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "deepest").Count)
Write-Output ("BIND_FAILURE_HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "bind failure").Count)
Write-Output ("RUNCLASSCONSTRUCTOR_HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "RunClassConstructor").Count)
'
```

The payload additionally carries a leading `Set-Location` to the item worktree, for the reason
recorded in `deedle-member-surface.2026-09-13T18-22.md`: the executor was launched without worktree
isolation, so pwsh would otherwise resolve every repository-relative path in the coordinator session
worktree. That statement changes no measurement.

EXIT_CODE: 0

Output Summary:

```
AC10_LINE_515_PREFIX=[- [ ] ]
AC11_LINE_521_PREFIX=[- [ ] ]
AC_HEADING_HITS=1
DEEPEST_HITS=1
BIND_FAILURE_HITS=1
RUNCLASSCONSTRUCTOR_HITS=0
```

Acceptance Condition: MET. Both prefix readings are the six characters of an unchecked markdown
checkbox, so the Revision R2 rewrite of AC10 occupied exactly six lines and displaced no sibling
criterion; every acceptance-criterion line number recorded above remains valid.
`AC_HEADING_HITS=1` is the positive control proving the file path and the search mechanism are
live. `DEEPEST_HITS=1` and `BIND_FAILURE_HITS=1` confirm the two single-line tokens the rewrite
introduced are each present exactly once.

`RUNCLASSCONSTRUCTOR_HITS=0` is recorded and deliberately not gated, per the task text: the literal
never appeared in `spec.md`, so a zero-hit assertion on it could not fail whatever the executor
does.

## AC10 Revision R5 Rewrite:

Timestamp: 2026-09-14T11-19

This block is appended alongside the Revision R2 block above rather than replacing it, as `[P1-T7]`
directs. Revision R5 rewrote AC10 a second time, so the two content tokens the R2 block measured
(`deepest`, `bind failure`) no longer describe the criterion; the R5 tokens are `netstandard2.1` and
`Deedle.Frame.FromRecords`. Revision R5 additionally rewrote two sibling regions of `spec.md` that the
re-rooting invalidated, each line for line: the build-output assumption at lines 392-394 and the Test
Strategy domain-configuration paragraph at lines 431-434. Neither is an acceptance criterion and
neither changed the file's line count; `QuickFiler.Test.dll.config` is the token that rewrite
introduced.

Command:

```
pwsh -NoProfile -Command '
$p = "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/spec.md"
$lines = @(Get-Content -LiteralPath $p)
Write-Output ("AC10_LINE_515_PREFIX=[" + $lines[514].Substring(0,6) + "]")
Write-Output ("AC11_LINE_521_PREFIX=[" + $lines[520].Substring(0,6) + "]")
Write-Output ("AC_HEADING_HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "## Acceptance Criteria").Count)
Write-Output ("NETSTANDARD21_FLAVOUR_HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "netstandard2.1").Count)
Write-Output ("FROMRECORDS_HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "Deedle.Frame.FromRecords").Count)
Write-Output ("QUICKFILER_TEST_CONFIG_HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "QuickFiler.Test.dll.config").Count)
Write-Output ("RUNCLASSCONSTRUCTOR_HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "RunClassConstructor").Count)
'
```

The payload additionally carries a leading `Set-Location` to the item worktree, for the reason
recorded in the Revision R2 block above. That statement changes no measurement.

EXIT_CODE: 0

Output Summary:

```
AC10_LINE_515_PREFIX=[- [ ] ]
AC11_LINE_521_PREFIX=[- [ ] ]
AC_HEADING_HITS=1
NETSTANDARD21_FLAVOUR_HITS=1
FROMRECORDS_HITS=1
QUICKFILER_TEST_CONFIG_HITS=1
RUNCLASSCONSTRUCTOR_HITS=0
```

Acceptance Condition: MET. Both prefix readings are the six characters of an unchecked markdown
checkbox, so the Revision R5 rewrite of AC10 again occupied exactly six lines and displaced no sibling
criterion; every acceptance-criterion line number recorded in the `## Inventory` table above remains
valid, and the line numbers `[P6-T6]` through `[P6-T24]` consume are unchanged.
`AC_HEADING_HITS=1` is the positive control proving the file path and the search mechanism are live.
`NETSTANDARD21_FLAVOUR_HITS=1`, `FROMRECORDS_HITS=1` and `QUICKFILER_TEST_CONFIG_HITS=1` confirm the
three single-line tokens the R5 rewrites introduced are each present exactly once. All three had zero
hits in `spec.md` before the rewrite, which is what makes those counts discriminating rather than
already satisfied.

`RUNCLASSCONSTRUCTOR_HITS=0` is recorded and deliberately not gated, per the task text, for the same
reason as in the Revision R2 block.

No acceptance criterion is checked off by this task. The plan assigns every check-off to Phase 6.

## AC4 Reading:

Recorded by `[P6-T9]` at the point AC4 is checked off at `spec.md` line 484.

AC4 READING: ten delegate-driven tests, eleventh test excluded

AC4's closing sentence at `spec.md` lines 489-490 — that all rungs are exercised through
injected delegates, touching neither the GAC nor the filesystem — is read as a property of the
ten delegate-driven tests that exercise the ladder rungs, and not of the eleventh test `[P2-T3]`
adds. That eleventh test,
`Install_ThenLoadOfUnresolvableName_LeavesTheLoadFailingWithoutHandlerThrowing`, drives the
subscribed handler through the real CLR binder in order to cover
`AssemblyBindingFallback.OnAssemblyResolve` and `AssemblyBindingFallback.CreateProductionLadder`,
neither of which any delegate-driven test can reach, because both run only when a genuine failed
bind raises the domain's assembly-resolution event.

Under that reading all four rungs retain injected-delegate coverage and AC4 is satisfied.

`spec.md` lines 447-448 note that only pure ladder logic belongs in this assembly because it is
masked by the PR #880 handler. That ground does not reach the eleventh test, whose asserted
outcome is that a display name matching nothing fails to load, which no masking handler can
change.

`spec.md` is not amended for this reading. AC4 spans lines 484-490 and a re-wrap would move the
fifteen criteria that follow it, every one of which this plan cites by line number.

### Revision R7 note — the reading and its pinned literal are unchanged

AC4 names the class `UtilitiesCS.Test.Bootstrap.AssemblyBindingFallbackTests` specifically. The
ten tests `[P4-T13]` adds live in a different class,
`UtilitiesCS.Test.Bootstrap.AssemblyBindingFallbackEdgeCaseTests`, so they are outside AC4's
subject and neither extend nor contradict its enumerated scenario list. The reading above
therefore still describes exactly eleven tests in exactly the class AC4 names, and the pinned
literal above is unchanged.

For completeness: the ten sibling tests are themselves delegate-driven or guard-clause tests, and
none of them drives a real bind. Each reaches its target either through the ladder's five
injected delegates or through a guard clause that returns before any rung runs, and the class
contains zero occurrences of a load-by-display-name call, a load-from-path call and a
file-existence call, as `[P4-T13]`'s own acceptance gates at zero. The eleventh test therefore
remains the single exception this reading carves out across the whole work.
