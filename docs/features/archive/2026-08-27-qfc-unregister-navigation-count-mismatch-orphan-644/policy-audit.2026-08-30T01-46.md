# Policy Audit — Issue #644 (cycle-exit reaudit)

- **Timestamp:** 2026-08-30T01-46
- **Branch:** `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- **Head:** `85a1939f92f64ebada4e71d19cc095dc2e8e8a26`
- **Base branch:** `main`; `origin/main` = `fa2ddefacf2c08abe18f3e3250d77da804534637`
- **Merge base:** `fa2ddefacf2c08abe18f3e3250d77da804534637` (verified by `git merge-base HEAD origin/main`)
- **Ahead / behind:** 6 ahead, 0 behind (`git rev-list --left-right --count origin/main...HEAD` -> `0	6`)
- **Work mode:** `full-bug`; AC source is `spec.md` only
- **Cycle:** exit reaudit of an **elective** remediation cycle. The cycle-entry audit at
  `2026-08-29T23-06` returned 0 blocking; the cycle was opened by orchestrator election over two
  non-blocking findings.

## Method statement

This audit re-derives its verdicts over the **entire branch diff against the resolved base branch**,
not over the remediation cycle's two items. Every verdict below was reproduced by a command run in
this session against the working tree at `85a1939f`. Where a figure is quoted from a committed
evidence artifact rather than re-measured, the artifact is named and the reason for not re-measuring
is stated.

The predecessor audit at `2026-08-29T23-06` was read in full and its findings are individually
re-tested in section 8. No verdict is inherited.

## Executive summary

| Item | Result |
|---|---|
| **Total blocking findings** | **0** |
| Non-blocking findings carried forward from cycle entry | 10 |
| Non-blocking findings closed by the remediation cycle | 2 (CR-1, PA-7) |
| Non-blocking findings newly raised by this reaudit | 3 (CR-6, PA-8, OB-1) |
| Acceptance criteria PASS | 17 of 18 |
| Acceptance criteria PARTIAL | 1 (AC-16) |
| Acceptance criteria FAIL | 0 |
| Merge recommendation | **GO** |

The two items the cycle set out to fix are closed and were verified independently, not accepted on
report. One further instance of the same defect class survives in the same file and was never
recorded by any prior artifact; it is raised here as CR-6 and is non-blocking.

## Rejected scope narrowing

The delegating prompt supplied no instruction that narrows the audit to a plan, task, phase, or file
subset. It directed the reaudit to "re-derive it over the whole delivered change", which is
congruent with the scope invariant. Nothing was rejected.

For completeness, the delegating prompt did name the two remediated items and did characterise ten
other findings as carried forward. That framing was **not** treated as a scope limit: all twelve
predecessor findings were re-tested against the tree, and an independent sweep was run for defects
belonging to none of them. That sweep produced CR-6 and PA-8.

## Evidence location compliance

Scanned the branch diff for artifacts written to non-canonical evidence locations:

```
$ git diff --name-only fa2ddefa...HEAD | grep -E "^artifacts/(baselines|qa|evidence|coverage)/"
(no output)
```

Zero violations. All 58 evidence artifacts are under
`docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/`, in
six kind-named subfolders (`baseline/`, `issue-updates/`, `other/`, `qa-gates/`,
`regression-testing/`, `remediation-baseline/`), each with an ISO-8601 timestamp in the filename.
This satisfies `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` and AC-17.

`validate_evidence_locations.py` is not present in this repository
(`find . -name "validate_evidence_locations.py"` returns nothing outside `packages/`). The direct
`git diff --name-only` scan above is the authoritative substitute and is dispositive for this
branch, because it enumerates every path the branch adds or modifies.

**Verdict: PASS.**

## 1. Change footprint

Six code paths plus one feature folder. Verified against the resolved base, not against the plan's
anchor:

```
$ git diff --numstat fa2ddefa...HEAD -- QuickFiler QuickFiler.Test
14	10	QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs
12	12	QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs
361	0	QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs
3	4	QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs
1	0	QuickFiler.Test/QuickFiler.Test.csproj
18	9	QuickFiler/Controllers/QfcCollectionController.cs
```

```
$ git diff --name-only fa2ddefa...HEAD | grep -v "^docs/features/active/2026-08-27-qfc" | grep -v "^QuickFiler"
(no output)
```

No path outside the six code paths and the feature folder appears in the branch diff. The numstat
against the review anchor `e968a1a8` is byte-identical to the three-dot numstat against `fa2ddefa`,
so the anchor choice does not change the audited footprint.

One production file is modified; none is added; no interface file is touched; `QuickFiler.csproj`
is unchanged.

## 2. Changed languages

| Language | Changed files | Coverage language |
|---|---|---|
| C# | 5 (`.cs`), one added | yes |
| MSBuild project XML | 1 (`.csproj`) | no coverage language of its own |
| TypeScript | 0 | not present on this branch |
| Python | 0 | not present on this branch |
| PowerShell | 0 | not present on this branch |

Verified: `git diff --name-only fa2ddefa...HEAD` returns no `.ts`, `.tsx`, `.py`, `.ps1`, or `.psm1`
path. C# is therefore the single language requiring an explicit coverage verdict, and it receives
one in section 5.

## 3. Toolchain gates

| Gate | Command | Result | How verified in this session |
|---|---|---|---|
| Format | `dotnet tool run csharpier check .` | exit 0, `Checked 1562 files in 4658ms`, no unformatted file | **re-run by this audit** |
| Lint / analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | exit 0, 0 errors | evidence `qa-gates/p2-t3-analyzer-build.2026-08-29T23-23.md`, plus the compile proof below |
| Type-check / nullable | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | exit 0, 0 errors, no CS0414 | evidence `qa-gates/p2-t4-nullable-build.2026-08-29T23-23.md`, plus the compile proof below |
| Test | `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"` | 1254 total, 1254 passed, 0 failed | **re-read from the TRX by this audit** |

Formatter re-run, verbatim:

```
$ dotnet tool run csharpier check .
Checked 1562 files in 4658ms.
EXIT=0
```

Test counters read directly from the TRX rather than from console text:

```
$ grep -o '<Counters[^/]*/>' coverage/trx/remediation-p2-t5/*.trx
<Counters total="1254" executed="1254" passed="1254" failed="0" error="0" timeout="0"
 aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0"
 disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
```

**Compile proof for the two msbuild gates.** The two solution rebuilds were not re-run by this
audit; each is a multi-minute full-solution rebuild and the delta since the cycle-entry audit is
comment text and two string literals in one test file. That is not accepted on report alone. The
substantive property — that the edited file compiles and that the executed assembly contains the
edit — is proved directly by timestamps: the edited source is stamped `2026-08-30 01:25:39`, the
built `QuickFiler.Test.dll` is stamped `2026-08-30 01:34:38`, and the TRX run began at
`2026-08-30T01:36:52`. The assembly postdates the edit and the run postdates the assembly, so the
1254 passing results were produced by code containing the remediation. Neither Roslyn analyzers nor
nullable flow analysis inspects the contents of an XML documentation comment or of a string literal
passed as a `because:` argument, so neither gate can be moved by this remediation once the file
compiles.

**Verdict: PASS.**

## 4. Test policy compliance

`.claude/rules/general-unit-test.md` and the C# unit test policy:

| Requirement | Result | Evidence |
|---|---|---|
| MSTest framework | PASS | `using Microsoft.VisualStudio.TestTools.UnitTesting;` in the new file |
| Moq for mocking | PASS | `using Moq;` |
| FluentAssertions for assertions | PASS | 18 `Should()` occurrences in the new file |
| No temporary files | PASS | no `Path.GetTempFileName`, `Path.GetTempPath`, or `File.Create` match |
| No wall-clock waits | PASS | no `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow` match |
| No unseeded randomness | PASS | no `new Random()` match |
| Tests mirror production layout | PASS | `QuickFiler.Test/Controllers/` mirrors `QuickFiler/Controllers/` |
| Test file under 500 lines | PASS | new file 361 lines; largest changed test file 499 |
| Scenario completeness | PASS | positive, negative (empty ledger), edge (width crossing), error (null field), state transition (repeated cycles) |

Banned-pattern sweep, verbatim:

```
$ grep -nE "Thread\.Sleep|Task\.Delay|DateTime\.(Now|UtcNow)|Path\.GetTempFileName|Path\.GetTempPath|File\.Create|new Random\(\)" QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs
(no output)
```

File-size measurements, re-derived with `awk 'END{print NR}'` (not `Measure-Object -Line`):

| File | Lines at head | `[TestMethod]` | Limit | Result |
|---|---|---|---|---|
| `QfcCollectionControllerTests.cs` | 499 (base 500) | 13 (base 13) | 500 / frozen at 13 | PASS |
| `QfcCollectionControllerNavigationDigitsTests.cs` | 226 | 3 | 500 | PASS |
| `QfcCollectionControllerNavigationLedgerTests.cs` | 361 | 6 | 500 | PASS |
| `QfcCollectionControllerDefects468Tests.cs` | 498 | 8 | 500 | PASS |
| `QfcCollectionController.cs` (production) | 2446 (base 2437) | n/a | 500 | FAIL, pre-existing — see PA-2 |

The frozen characterisation file went from 500 lines to 499 and held at exactly 13 `[TestMethod]`
attributes, so issue #468's freeze is respected.

**Verdict: PASS**, with the pre-existing production file-size violation recorded at PA-2.

## 5. Coverage verification (C#)

C# is the only language with changed files on this branch, so an explicit PASS or FAIL verdict is
mandatory for it. Both verdicts are recorded here.

- **C# line coverage: FAIL.** The canonical artifact `artifacts/csharp/coverage.xml` does not exist
  at review time. `ls artifacts/` returns `orchestration/`, `pr_context.appendix.txt`,
  `pr_context.summary.txt`; `ls coverage/` returns `.gitkeep` and `trx/`; no Cobertura or JaCoCo
  document is committed anywhere under the feature folder. Under the mandatory-artifact rule, an
  absent artifact for a language with changed files is recorded FAIL. **Dispositioned non-blocking**
  — the rationale is in the paragraph below this list.
- **C# branch coverage: PASS.** The recorded root `branch-rate` is `0.792927`, that is 79.29%,
  against the 75% floor. Measured margin +4.29 points.

**Why the line-coverage FAIL is non-blocking.** The FAIL is procedural, not a code defect, and the
substantive figures it stands in for were produced, recorded, and are arithmetically checkable:

| Figure | `lines-covered` | `lines-valid` | Rate | Percent |
|---|---|---|---|---|
| AC-0 baseline (`[P0-T12]`, run A, pre-change) | 54800 | 64221 | 0.853303 | 85.3303% |
| Post-change, run E | 54793 | 64221 | 0.853194 | 85.3194% |
| Post-change, run F (byte-identical tree to E) | 54811 | 64221 | 0.853475 | 85.3475% |

Both post-change figures clear the 85% floor. `54793 / 64221 = 0.85319` and
`54811 / 64221 = 0.85347`, which reproduces the recorded rates, so the figures are internally
consistent and were not simply asserted. The denominator is invariant at 64221 across all three
runs. The single changed production file carries `[ExcludeFromCodeCoverage]` and was verified absent
from all 558 `<class>` entries of the post-processed document, so no line this change edits sits in
either the numerator or the denominator. This remediation cycle changed no production file at all
(`git diff a2c69aea..85a1939f --name-only -- QuickFiler/Controllers/QfcCollectionController.cs`
returns empty), so no figure above can have moved since the cycle-entry audit measured them.

The `[ExcludeFromCodeCoverage]` attribute is on line 21 of the production file and predates this
branch; the tension between it and the Coverage Exclusion Policy is recorded at PA-1.

Recommendation, unchanged from cycle entry and still not acted on: copy the post-processed Cobertura
document into `evidence/coverage/` on future runs so a later reviewer can re-parse it directly
instead of re-deriving from recorded root attributes.

## 6. Code change policy compliance

| Rule | Source | Result |
|---|---|---|
| Simplicity first | `general-code-change.md` | PASS — a ledger field plus a lazy accessor replaces a derived-format loop |
| Separation of concerns | `general-code-change.md` | PASS — no I/O introduced |
| Fail fast, no silent error swallow | `general-code-change.md` | PARTIAL — the discarded `bool` from `Remove` is pre-existing and is recorded at CR-3 |
| File size <= 500 lines | `general-code-change.md` | FAIL on the production file, pre-existing — PA-2 |
| Comments synchronized with behavior | `CLAUDE.md` C#6.3 | PARTIAL — CR-2 and CR-6, both non-blocking |
| Bugfix workflow: failing test first | `CLAUDE.md` | PASS — T1 recorded red at `[P1-T4]`, green at `[P2-T5]` |
| Bugfix workflow: minimal targeted fix | `CLAUDE.md` | PASS — net +9 production lines, confined to three members and the field block |
| Deeper problems open a new issue | `CLAUDE.md` | PASS — CR-3 and CR-5 routed as promotion candidates, not widened into this fix |
| No policy documents modified | scope invariant | PASS — no path under `.claude/rules/` in the branch diff |

## 7. Host identity sweep

Performed independently of the cycle's own sweeps, with the account and machine tokens **derived at
runtime** rather than spelled in the command. The three tokens were written to a scratchpad file by
`powershell -NoProfile -Command "$env:USERNAME; $env:COMPUTERNAME; $env:USERPROFILE"` and consumed
with `grep -f`, so no token appears literally in this artifact or in the command:

```
$ grep -rniIf <runtime-token-file> docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644
(no output)
```

Zero hits for the account name, the machine name, and the user-profile path across all 66 files of
the feature folder, including the 18 evidence artifacts the remediation added.

Supplementary sweeps, all clean:

- Mail local-part: `grep -rniI "dmoisan" <feature folder>` — no output.
- Mail domain: `grep -rniI "realgoodfoods" <feature folder>` — no output.

Absolute paths that remain, and why each is not a host-identity leak:

```
$ grep -rniIE "[A-Za-z]:[\\/](Users|Program Files|ProgramData)" <feature folder>
```

returns 12 lines, every one of them a `C:\Program Files` or `C:\Program Files (x86)` path naming
`vswhere.exe`, `vstest.console.exe`, or a NuGet fallback config. These are fixed system locations
identical on every Windows host: none carries the account name, the machine name, or the user
profile, and none matches the account or machine token in the runtime sweep above. The remediation
plan reasoned about this explicitly at `[P2-T5]` and chose to write them verbatim; that judgment is
correct.

One residual string is worth naming so a later reader does not mistake it for a miss: line 7 of
`research/research.2026-08-29T07-55.md` still contains `.git/worktrees/agent-a9e13727f905b003a/HEAD`.
That is a repository-relative path and an agent worktree identifier. It carries no account name, no
machine name, and no absolute path, and it is what preserves the specific-worktree information that
line 5's redaction generalised away. It is not a leak and requires no action.

The TRX produced by this cycle's test gate is named with the account and machine name by
`vstest.console.exe` default behavior. It sits at `coverage/trx/remediation-p2-t5/` and is matched by
`.gitignore` line 144 (`coverage/*`), so it is untracked and does not enter the commit. Every
evidence artifact citing it writes it in `<account>_<HOST>` placeholder form. Verified: the TRX path
does not appear in `git diff --name-only fa2ddefa...HEAD`.

**Verdict: PASS.** PA-7 is closed.

## 8. Disposition of every cycle-entry finding

The cycle-entry audit at `2026-08-29T23-06` recorded 12 non-blocking findings. Each is re-tested
below against the tree at `85a1939f`. None has become blocking.

| ID | Title | Status at exit | Blocking |
|---|---|---|---|
| PA-1 | `[ExcludeFromCodeCoverage]` conflicts with the Coverage Exclusion Policy | **STANDS**, pre-existing | No |
| PA-2 | Production file exceeds the 500-line ceiling | **STANDS**, pre-existing | No |
| PA-3 | Canonical C# coverage artifact absent | **STANDS**, procedural | No |
| PA-4 | One sentence in the coverage evidence overstates the exclusion argument | **STANDS**, deliberate | No |
| PA-5 | Superseded "AC-16 is checked off" sentence in a recorded run artifact | **STANDS**, deliberate | No |
| PA-6 | `[P4-T8]` checked off although its literal sixth clause did not hold | **STANDS**, deliberate | No |
| PA-7 | Absolute host path with the account name in committed documents | **CLOSED** | No |
| CR-1 | Stale mechanism description in a retained XML doc | **CLOSED** | No |
| CR-2 | Historical sentence names a removal set that is now wrong | **STANDS** | No |
| CR-3 | The discarded `bool` from `Remove` is now a meaningful signal | **STANDS** | No |
| CR-4 | Lazy accessor read twice; allocates to clear | **STANDS** | No |
| CR-5 | Ledger is a non-thread-safe `List<T>` behind a non-atomic `??=` | **STANDS** | No |

Re-test detail for each:

**PA-1 — STANDS, non-blocking, pre-existing.** `[ExcludeFromCodeCoverage]` is on line 21 of
`QuickFiler/Controllers/QfcCollectionController.cs`. The production diff contains no hunk near line
21, so the attribute is untouched by this branch. The tension between the Coverage Exclusion Policy
in `.claude/rules/general-unit-test.md` and the maintainer-ratified COM/VSTO exemption in `CLAUDE.md`
is a repository-level question, not a defect authored here.

**PA-2 — STANDS, non-blocking, pre-existing.** Re-measured this session:

```
$ awk 'END{print NR}' QuickFiler/Controllers/QfcCollectionController.cs   -> 2446
$ git show fa2ddefa:QuickFiler/Controllers/QfcCollectionController.cs | awk 'END{print NR}' -> 2437
```

Net +9, inside AC-14's bound of 10. The violation is pre-existing at 4.9x the ceiling; adding 9 lines
does not create it.

**PA-3 — STANDS, non-blocking, procedural.** Re-verified this session; see section 5. The
`coverage/` directory now additionally holds `trx/` from this cycle's test run, but still no
Cobertura or JaCoCo document, so the finding is unchanged.

**PA-4 — STANDS, non-blocking.** The overstated sentence "There is no path by which this change adds
or removes coverage from any production file at all" is still present in
`evidence/qa-gates/p4-t6-coverage-final.2026-08-29T08-15.md`. The remediation deliberately excluded
it on the ground that a recorded run artifact must not be rewritten. That reasoning is sound and this
audit endorses it. The correct weaker claim — that the change cannot move the figure *through the
excluded file* — is what the invariant `lines-valid` of 64221 actually demonstrates, and section 5
states it in that form.

**PA-5 — STANDS, non-blocking.** The superseded sentence "AC-16 is checked off under this
adjudication" is still the final sentence of that same artifact. It contradicts `spec.md`, which is
the authoritative AC source and shows AC-16 as `- [ ]` at line 707. The cycle-entry recommendation
was to *append* a superseded-by footer rather than edit; that append was not made. Correcting forward
remains the right disposition, but the back-pointer gap the predecessor identified is still open.

**PA-6 — STANDS, non-blocking.** `plan.2026-08-29T07-42.md` still shows 58 checked and 0 unchecked
`[P#-T#]` tasks, including `[P4-T8]`. The remediation declined to uncheck it, reasoning that an
unchecked task with no remaining work is a worse record than a disclosed deviation. That is
defensible. The guarded property was re-verified independently by this audit in section 1: the full
branch diff is the six code paths plus this feature folder and nothing else.

**PA-7 — CLOSED.** Verified in section 7 by a runtime-derived token sweep returning zero hits across
the entire feature folder. The three redactions are:

1. `research/research.2026-08-29T07-55.md` line 5: the `- Worktree:` value replaced with
   `<repo-root>/.claude/worktrees/<agent-worktree>`.
2. `policy-audit.2026-08-29T23-06.md` line 482: the `- **Content:**` bullet, same replacement.
3. `policy-audit.2026-08-29T23-06.md` line 483: the `- **Verification:**` bullet, which previously
   quoted a command whose alternation named the account and the mail local-part; now phrased as "a
   case-insensitive search for the account name and the account's mail local-part".

**Meaning preservation, checked clause by clause.** No finding, verdict, measured figure, or location
citation was altered:

- The PA-7 finding itself survives intact under its own heading, still classified Non-blocking.
- Its location citation "line 5" is unchanged.
- Its Standing paragraph still names commit `28ee4720`, still records that the leak is part of the
  branch-versus-base diff, still cites `p4-t5` and `p4-t6` as correctly-redacting siblings, and still
  names issue #685.
- No figure appears on either edited line, so no figure could have moved.
- Both edits were one line replaced by one line, so every subsequent line keeps its number and every
  cross-reference elsewhere in the artifact remains correct. Confirmed by reading lines 475-495: line
  483 is still the `- **Verification:**` bullet.
- The `research.md` line still conveys what it did: that the research ran inside an agent worktree
  under the repository's `.claude/worktrees/` tree. The specific worktree identifier is not lost,
  because line 7 of the same file still carries it in relative form.

One residual readability effect is worth recording, though it is an observation rather than a
finding: the redacted `- **Content:**` bullet at line 482 now displays the same generic string that
the `- **Recommendation:**` bullet proposes as the fix, so a reader cannot see the offending text
itself. Meaning is nonetheless preserved, because the adjacent Verification bullet states that a
search for the account name and the mail local-part matched that line — which tells the reader
exactly what the original contained. No action required.

**CR-1 — CLOSED.** Verified by reading the file, not by accepting the report. The corrected block is
at lines 189-196 of `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`:

```
/// "01".."10" and left all nine single-digit keys orphaned. After the fix the ledger replays
/// the nine recorded keys "1".."9" verbatim, so the added tenth group is irrelevant to
/// unregistration.
```

This describes the delivered mechanism correctly: a ledger of recorded key strings replayed
verbatim, with no recorded width and no loop bound. Cross-checked against the production diff, which
deletes `_registeredDigits`, its assignment, and the `format` expression, and replaces the
`for (int i = 0; i < _itemGroups.Count; i++)` loop with
`foreach (var (sourceId, key) in RegisteredNavigationKeys)`. The third instance corrected during
planning, the `.BeEmpty(...)` because-message at line 222, is likewise correct.

**Neither correction touched executable code.** The complete remediation diff for this file is eight
lines, four removed and four added:

```
$ git diff a2c69aea..85a1939f -- QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs
```

Three of the four added lines are XML documentation comment lines; the fourth is the contents of one
string literal. No assertion, no test name, no attribute, and no executable statement is among them.
The file held at 226 lines and 3 `[TestMethod]` attributes, both re-measured this session.

**CR-2 — STANDS, non-blocking.** Lines 144-145 still read "After the / fix it replays the recorded
width and removes `"01".."09"`." Its cycle-entry recommendation was explicitly conditional — "if CR-1
is addressed, tighten this ... so the tense marks it as history". CR-1 *was* addressed, which made
that recommendation live, and it was not acted on. See CR-6 for the fuller treatment; CR-2 and CR-6
are the same defect class and should be corrected together.

**CR-3 — STANDS, non-blocking.** The production code still discards the `bool`:

```csharp
foreach (var (sourceId, key) in RegisteredNavigationKeys)
{
    _kbdHandler.StringActionsAsync.Remove(sourceId, key);
}
```

This cycle changed no production file, so the finding is necessarily unchanged. It remains a
promotion candidate rather than in-scope work, correctly, under the `CLAUDE.md` Bugfix Workflow rule
that a deeper design problem opens a new issue.

**CR-4 — STANDS, non-blocking, trivial.** `RegisteredNavigationKeys` is read twice in
`UnregisterNavigation` — once by the `foreach` and once by the `.Clear()`. On the no-prior-
registration path the lazy `??=` therefore allocates a `List<T>` solely to enumerate zero items and
clear it. Verified against the production diff.

**CR-5 — STANDS, non-blocking, trivial.** The ledger is a plain `List<(string, string)>` behind
`_registeredNavigationKeys ??= new List<(string SourceId, string Key)>()`, which is not an atomic
operation. Verified against the production diff.

## 9. Findings raised by this reaudit

### CR-6 — a fourth instance of the CR-1 defect class survives, never previously recorded (Minor, Non-blocking)

- **Location:** `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`
  line 179, the `because:` argument of the first assertion in
  `UnregisterNavigation_AfterRegisteringAtTwoDigitsAndShrinkingToNine_RemovesTheTwoDigitKeys`.
- **Text at fault:** `"the recorded registration width is replayed, so the '0'-prefixed keys go"`
- **Violated rule:** `CLAUDE.md` C#6.3 and `.claude/rules/general-code-change.md` — comments must
  stay synchronized with behavior.
- **Why it is wrong:** "the recorded registration width" names `_registeredDigits`, which this
  branch deleted. Verified: `grep -rn "_registeredDigits"` over the repository returns zero
  occurrences. There is no recorded width to replay. The keys are removed because the ledger replays
  each recorded key string verbatim.
- **Why it was missed:** this instance is structurally identical to the line-222 message the cycle
  *did* correct — same file, same defect, same kind of string literal — differing only in which test
  it sits on. The cycle's acceptance clauses were anchored on specific literal fragments
  (`grown loop bound reaches`, `regardless of group count`), and its two sweep tasks
  (`p1-t5-pa7-sweep`, `p3-t3-pa7-final-sweep`) were scoped to host identity, not to this class. No
  task swept the file for the class as a class. Confirmed: `grep -rn` across all prior artifacts for
  `prefixed keys go`, `recorded registration width`, and `line 179` returns no match, so this
  instance appears in no predecessor record.
- **Verification command and output:**

```
$ grep -rn --include=*.cs -iE "recorded (registration )?width|width is replayed" .
./QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs:145: fix it replays the recorded width and removes "01".."09".
./QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs:179: "the recorded registration width is replayed, so the '0'-prefixed keys go"
```

- **Why non-blocking:** the text is a diagnostic message on an assertion that is itself correct and
  passing. It changes no behavior, and the identical class was classified Non-blocking at cycle
  entry as CR-1 and CR-2. Classifying it differently now would be inconsistent.
- **Recommendation:** correct lines 145 and 179 together in a single follow-up. Suggested wording for
  line 179: `"the ledger replays each recorded key verbatim, so no '0'-prefixed key survives"`.

**Completeness of this audit's own sweep.** The two lines above are the complete result. The sweep
was run across every `.cs` file in the repository, not only the changed ones, for both the
recorded-width phrasing and the loop-bound phrasing. The two other `loop bound` matches were checked
and are correct: `QfcCollectionController.cs:2372` is an unrelated pre-existing comment in a
different method, and `QfcCollectionControllerNavigationLedgerTests.cs:305` uses the past tense
("before the ledger the loop bound dereferenced that null field"), which is accurate history.

### PA-8 — pre-existing analyzer HintPath version skew in two projects (Non-blocking, pre-existing, promote separately)

- **Location:** `UtilitiesCS/UtilitiesCS.csproj` lines 3, 1295, 1303-1307;
  `VBFunctions/VBFunctions.csproj` lines 3, 58-62, 73.
- **The skew.** The `Import` and `Error Condition` items name `Meziantou.Analyzer.3.0.174`, matching
  the version declared in `packages.config`. The `Analyzer Include` HintPaths in the same files name
  `Meziantou.Analyzer.3.0.156` and `Roslynator.Analyzers.4.16.0`, both one version behind the
  declared `3.0.174` and `4.16.1`. Confirmed: no `packages.config` anywhere in the repository
  declares `3.0.156` or `4.16.0` — `grep -rl "3.0.156" --include=packages.config .` and the
  equivalent for `4.16.0` both return nothing, and all 16 `Meziantou.Analyzer` declarations name
  `3.0.174`.
- **Not authored by this change.**
  `git diff --name-only fa2ddefa...HEAD | grep -iE "UtilitiesCS.csproj|VBFunctions.csproj|packages.config"`
  returns empty. The branch touches neither project.
- **Identical at the merge base.** `git show fa2ddefa:UtilitiesCS/UtilitiesCS.csproj` reproduces the
  same 3.0.174 / 3.0.156 / 4.16.0 combination line for line.
- **It does not gate CI.** `gh run list --branch main --limit 6` returns `conclusion: success` for
  six consecutive runs, the most recent of which is the merge base `fa2ddefa` itself. The executor's
  escalation stated that "a cold worktree fails the analyzer build with 10 CS0006 errors". The
  coldest environment available — a CI runner checkout — does not reproduce that failure, at the
  merge base or in the five runs before it. The version skew is real; the stated consequence is not
  reproduced by CI, so the local failure had an additional local cause, most plausibly a
  partially-populated `packages/` tree in which the guarded 3.0.174 `Import` resolved while the
  unconditional 3.0.156 `Analyzer Include` did not.
- **The escalation changed zero tracked files.** The executor provisioned the named versions into
  `packages/`, which `.gitignore` line 349 excludes. All four directories are present locally
  (`Meziantou.Analyzer.3.0.156`, `Meziantou.Analyzer.3.0.174`, `Roslynator.Analyzers.4.16.0`,
  `Roslynator.Analyzers.4.16.1`) and none appears in `git status --porcelain`.
- **Judgment: this does not block this pull request.** It is not authored by the change, it is
  byte-identical at the merge base, merging this branch neither introduces nor worsens it, and it
  does not gate CI on `main`. Under the `CLAUDE.md` Bugfix Workflow rule, a pre-existing condition
  uncovered mid-fix opens a new issue rather than widening this one.
- **Recommendation:** promote to a separate issue — realign the `Analyzer Include` HintPaths in both
  projects to the `packages.config`-declared `Meziantou.Analyzer.3.0.174` and
  `Roslynator.Analyzers.4.16.1`, which removes the skew and the local cold-build failure together.

### OB-1 — PR context artifacts were stale at reaudit entry (Non-blocking, procedural, cured)

- `artifacts/pr_context.summary.txt` named `Head ref: a2c69aead286...`, the cycle-entry head, while
  the branch head is `85a1939f92f6...`. The appendix was likewise generated before the remediation
  commit.
- Both were regenerated by this audit at `2026-08-30T01-46` against the resolved base. The summary
  now names the correct head and merge base and records the digits-test numstat as `+12/-12`, up from
  `+8/-8` at cycle entry.
- Non-blocking and now cured. Recorded so the regeneration is attributable.

## 10. Compliance verdict

| Policy | Verdict |
|---|---|
| `CLAUDE.md` standing instructions | PASS |
| `.claude/rules/general-code-change.md` | PASS with PA-2 recorded pre-existing |
| `.claude/rules/general-unit-test.md` | PASS with PA-1 recorded pre-existing |
| `.claude/rules/quality-tiers.md` | PASS |
| C# code change policy | PASS |
| C# unit test policy | PASS |
| Evidence location conventions | PASS |
| Tonality policy | PASS |
| Scope invariant | PASS |

**Total blocking findings: 0.**

**Merge recommendation: GO.** The exit condition stated in `remediation-inputs.2026-08-29T23-23.md`
— a reaudit at a new timestamp with a total blocking count of zero and both cycle items confirmed
remediated — is met. CR-1 and PA-7 are independently verified closed. CR-6 is raised as a new
non-blocking finding of the same class as CR-2 and should be corrected with it in a follow-up rather
than by reopening this cycle: reopening would trade a documentation-comment correction against
another full toolchain pass and another audit cycle, and this class has already been shown to be
findable only by a class-scoped sweep, which this audit has now performed and recorded.

No remediation-inputs artifact is produced, because the blocking count is zero and no finding is
remediation-required.
