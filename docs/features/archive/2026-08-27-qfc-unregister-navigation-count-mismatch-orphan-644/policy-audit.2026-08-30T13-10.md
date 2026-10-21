# Policy Audit — Issue #644 (remediation cycle 2, exit reaudit)

- **Timestamp:** 2026-08-30T13-10
- **Branch:** `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- **Head:** `4572fef5efbc330abb7f34856b7d24dc7869da8b`
- **Base branch:** `main`; `origin/main` = `fa2ddefacf2c08abe18f3e3250d77da804534637`
- **Merge base:** `fa2ddefacf2c08abe18f3e3250d77da804534637`, resolved by `git merge-base HEAD origin/main`
- **Ahead / behind:** 8 ahead, 0 behind (`git rev-list --left-right --count origin/main...HEAD` -> `0	8`)
- **Diff anchor used for span citations:** `e968a1a8804b7641380d4489c496662824d45767`
- **Work mode:** `full-bug` (marker at `issue.md` line 14); AC source is `spec.md` only
- **Cycle:** exit reaudit of remediation cycle 2. Cycle-2 inputs:
  `remediation-inputs.2026-08-30T02-08.md`; cycle-2 plan: `remediation-plan.2026-08-30T02-08.md`.

## Method statement

Every verdict below is re-derived over the **entire branch diff against the resolved base branch**,
not over the remediation cycle's two items. Verdicts were reproduced by commands executed in this
session against the working tree at `4572fef5`. Where a figure comes from a committed or untracked
evidence artifact rather than a re-measurement, the artifact is named and the reason for not
re-measuring is stated.

The two predecessor audits at `2026-08-29T23-06` and `2026-08-30T01-46` were read in full. Their
findings are individually re-tested in section 9. No verdict is inherited.

## Diff anchor

All span citations use `e968a1a8`, the commit that merged the then-current `origin/main` tip into
this branch, rather than the plan's literal `ecdb1c84`. This is the recorded
`local_execution_overrides` entry `diff_anchor_substitution` (confirmed present in
`artifacts/orchestration/orchestrator-state.json`). The substitution is settled and is not reported
as a finding.

It was checked for equivalence rather than assumed. The two-dot numstat against `e968a1a8` and the
three-dot numstat against the merge base `fa2ddefa` agree file for file and count for count over
`QuickFiler` and `QuickFiler.Test`, so the anchor choice does not change the audited footprint.

## Rejected scope narrowing

The delegating prompt directed the audited diff to be taken with `':!.claude/agent-memory'`, which
excludes 15 changed paths from the branch diff. Verbatim caller text:

> `git diff e968a1a8804b7641380d4489c496662824d45767 HEAD -- . ':!.claude/agent-memory'`

Per the scope invariant this exclusion is **rejected**: the audit scope is the full branch diff
against the resolved base branch, and a caller-supplied file-subset limit does not narrow it. The 15
excluded paths were enumerated, read, and audited; they produce findings PA-9 and PA-10 in section
10. Justification: an excluded subtree that the branch actually modifies can carry policy violations,
and here it carried one (a host-identity string) that the exclusion would have hidden.

No other narrowing was attempted. The prompt's instruction not to re-litigate the diff anchor is not
a narrowing — it fixes a span-citation base, and equivalence to the resolved merge base was verified
independently above.

## Executive summary

| Item | Result |
|---|---|
| **Total blocking findings** | **0** |
| Non-blocking findings closed by this cycle | 2 (CR-2, CR-6) |
| Non-blocking findings carried forward | 9 (PA-1..PA-6, PA-8, CR-3, CR-4, CR-5) |
| Non-blocking findings newly raised here | 2 (PA-9, PA-10) + 1 code-review item (CR-7) |
| Observations, no action required | 2 (OB-2, OB-3) |
| Acceptance criteria PASS | 17 of 18 |
| Acceptance criteria PARTIAL | 1 (AC-16) |
| Acceptance criteria FAIL | 0 |
| Merge recommendation | **GO** |

## Evidence location compliance

Scan of the branch diff for artifacts written to non-canonical evidence locations:

```
$ git diff --name-only fa2ddefa...HEAD | grep -E "^artifacts/(baselines|qa|evidence|coverage)/"
(no output, grep exit 1)
```

Zero violations. All evidence artifacts sit under
`docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/`
in six kind-named subfolders (`baseline/`, `issue-updates/`, `other/`, `qa-gates/`,
`regression-testing/`, `remediation-baseline/`), each with an ISO-8601 timestamp in the filename.
This satisfies `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` and AC-17.

`validate_evidence_locations.py` is not present in this repository, so the `git diff --name-only`
scan above is the authoritative substitute. It is dispositive because it enumerates every path the
branch adds or modifies.

No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` condition arose: the delegating prompt specified the
canonical feature-folder destination.

**Verdict: PASS.**

## 1. Change footprint

```
$ git diff --numstat fa2ddefa...HEAD -- QuickFiler QuickFiler.Test
14	10	QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs
14	14	QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs
361	0	QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs
3	4	QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs
1	0	QuickFiler.Test/QuickFiler.Test.csproj
18	9	QuickFiler/Controllers/QfcCollectionController.cs
```

```
$ git diff --name-only fa2ddefa...HEAD | grep -v "^docs/features/active/2026-08-27-qfc" | grep -v "^QuickFiler"
.claude/agent-memory/atomic-executor/MEMORY.md
.claude/agent-memory/atomic-executor/project_analyzer_hintpath_skew_breaks_all_four_gates.md
.claude/agent-memory/atomic-executor/project_caller_stated_preflight_count_drifts_before_execution.md
.claude/agent-memory/atomic-executor/project_doubled_backslash_dedoubles_bash_to_native_exe.md
.claude/agent-memory/atomic-executor/project_orchestrator_override_does_not_satisfy_an_ac.md
.claude/agent-memory/atomic-executor/project_recursive_delete_idioms_blocked_use_dotnet_api.md
.claude/agent-memory/atomic-executor/project_selftest_probe_literal_trips_the_next_sweep_pass.md
.claude/agent-memory/atomic-planner/MEMORY.md
.claude/agent-memory/atomic-planner/observation-scope-must-match-blast-radius.md
.claude/agent-memory/atomic-planner/project_644_ac16_referral_revision_seams.md
.claude/agent-memory/atomic-planner/project_644_cycle2_sweep_gate_evasion_seams.md
.claude/agent-memory/atomic-planner/project_644_pa7_redaction_plan_seams.md
.claude/agent-memory/atomic-planner/runtime-derived-account-token-pattern.md
.claude/agent-memory/feature-review/MEMORY.md
.claude/agent-memory/feature-review/project_644-review-residuals.md
```

Those 15 paths arrive with the branch tip `4572fef5`, an agent-memory checkpoint commit made after
the cycle-1 exit audit. They are recorded at PA-9 and PA-10; they are not code.

Code footprint: one production file modified, none added, no interface file touched,
`QuickFiler/QuickFiler.csproj` unchanged. That is the property AC-14 exists to guard, and it holds.

## 2. Changed languages

Derived from `git diff --name-only fa2ddefa...HEAD`, not from the PR-context summary (see OB-2).

| Language | Changed files on the branch | Coverage language |
|---|---|---|
| C# | 5 `.cs` files — 1 added, 4 modified | yes |
| MSBuild project XML | 1 `.csproj` | not a coverage language of its own |
| Markdown | 100 `.md` files (feature folder and agent memory) | not a coverage language |
| TypeScript | 0 | none on this branch |
| Python | 0 | none on this branch |
| PowerShell | 0 | none on this branch |

The branch diff contains no `.ts`, `.tsx`, `.py`, `.ps1`, or `.psm1` path. C# is therefore the only
language requiring an explicit coverage verdict, and section 5 records one.

## 3. Toolchain gates

| Gate | Command | Result | How verified in this session |
|---|---|---|---|
| Format | `dotnet tool run csharpier check .` | exit 0, `Checked 1562 files in 5232ms`, no unformatted file | **re-run by this audit** |
| Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | exit 0 | orchestrator artifact `evidence/other/resume-toolchain-verification.2026-08-30T13-20.md`, gate 2, executed at this exact head in this worktree |
| Type-check / nullable | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | exit 0, no CS0414 | same artifact, gate 3 |
| Test | `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"` | 1254 total, 1254 passed, 0 failed | **re-run by this audit** |

Formatter re-run, verbatim:

```
$ dotnet tool run csharpier check .
Checked 1562 files in 5232ms.
```

Test re-run, verbatim tail:

```
Test Run Successful.
Total tests: 1254
     Passed: 1254
 Total time: 11.0809 Seconds
```

**Why the two msbuild gates were not re-run by this audit, and why the citation is nonetheless
sound.** Each is a multi-minute full-solution rebuild that mutates build output; the review contract
prefers check-only commands. More importantly, both were already executed **at this exact head, in
this worktree**, by the resuming orchestrator, and that run is recorded at
`evidence/other/resume-toolchain-verification.2026-08-30T13-20.md`. That artifact is untracked at
review time and is named here so its status is not misread as committed evidence. It is a stronger
basis than the cycle-1 mtime argument because it bootstrapped a worktree that had never built this
branch and used `/t:Rebuild`, so `CoreCompile` ran on every project and the gate is not vacuous.

**Independent corroboration that the executed binary contains the remediation.** The cycle-2 test
evidence recorded the SHA-256 of the edited source on both sides of its run. That digest was
recomputed here against the file at head:

```
$ certutil -hashfile QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs SHA256
972bcd8f142e50099783c4f92bda624639e13dfe5a2767ed4ac189e2679d3dab
```

It matches the recorded value character for character, so the source that was tested is byte-identical
to the source under review. The built `QuickFiler.Test.dll` timestamp also postdates the source
timestamp, and this audit's own re-run of the suite against that assembly returned 1254 of 1254.

**Verdict: PASS.**

## 4. Test policy compliance

`.claude/rules/general-unit-test.md` plus the C# unit test policy:

| Requirement | Result | Evidence |
|---|---|---|
| MSTest framework | PASS | `using Microsoft.VisualStudio.TestTools.UnitTesting;` in the added file |
| Moq for mocking | PASS | `using Moq;` |
| FluentAssertions for assertions | PASS | `Should()` throughout the added file |
| No temporary files | PASS | banned-pattern sweep below returns nothing |
| No wall-clock waits | PASS | same sweep |
| No unseeded randomness | PASS | same sweep |
| Tests mirror production layout | PASS | `QuickFiler.Test/Controllers/` mirrors `QuickFiler/Controllers/` |
| Test file under 500 lines | PASS | largest changed test file 499 |
| Scenario completeness | PASS | positive, negative, edge, error, and state-transition cases all present |

```
$ grep -nE "Thread\.Sleep|Task\.Delay|DateTime\.(Now|UtcNow)|Path\.GetTempFileName|Path\.GetTempPath|File\.Create|new Random\(\)" QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs
(no output, grep exit 1)
```

File sizes and `[TestMethod]` counts, re-derived this session with `awk 'END{print NR}'` and
`grep -c` rather than `Measure-Object -Line`:

| File | Base | Head | `[TestMethod]` base -> head | Limit | Result |
|---|---|---|---|---|---|
| `QfcCollectionControllerTests.cs` | 500 | 499 | 13 -> 13 | 500 / frozen at 13 | PASS |
| `QfcCollectionControllerNavigationDigitsTests.cs` | 226 | 226 | 3 -> 3 | 500 | PASS |
| `QfcCollectionControllerNavigationLedgerTests.cs` | absent | 361 | 0 -> 6 | 500 | PASS |
| `QfcCollectionControllerDefects468Tests.cs` | 494 | 498 | 8 -> 8 | 500 | PASS |
| `QfcCollectionController.cs` (production) | 2437 | 2446 | n/a | 500 | FAIL, pre-existing — PA-2 |

The #468 freeze on the characterisation file holds: 500 lines to 499, and exactly 13 `[TestMethod]`
attributes on both sides.

**Verdict: PASS**, with the pre-existing production file-size violation recorded at PA-2.

## 5. Coverage verification

C# is the only language with changed files on this branch, so it receives an explicit verdict. There
is no `.ts`, `.tsx`, `.py`, `.ps1`, or `.psm1` file anywhere in the branch diff, so no other language
requires one.

| Language | Changed files | Artifact | Repo-wide figure | Verdict |
|---|---|---|---|---|
| C# — line coverage | 5 | `artifacts/csharp/coverage.xml` absent at review time | 85.3194% / 85.3475% from the recorded runs | **FAIL** (artifact absent), dispositioned non-blocking below |
| C# — branch coverage | 5 | same | recorded root branch-rate 0.792927 = 79.29% | **PASS**, +4.29 points over the 75% floor |

Artifact presence was re-checked this session: `ls artifacts` returns `orchestration/`,
`pr_context.appendix.txt`, `pr_context.summary.txt`; `artifacts/csharp/` does not exist; the
repository `coverage/` directory is empty in this worktree. Under the mandatory-artifact rule an
absent artifact for a language with changed files is recorded FAIL.

**Why that C# line-coverage FAIL is dispositioned non-blocking.** It is procedural rather than a code
defect, and the substantive figures it stands in for were produced, recorded, and remain
arithmetically checkable:

| Run | Tree state | `lines-covered` | `lines-valid` | Rate | Percent |
|---|---|---|---|---|---|
| A — `[P0-T12]` baseline | pre-change | 54800 | 64221 | 0.853303 | 85.3303 |
| E — `[P4-T6]` designated | final | 54793 | 64221 | 0.853194 | 85.3194 |
| F — noise measurement | final, byte-identical tree to E | 54811 | 64221 | 0.853475 | 85.3475 |

`54793 / 64221 = 0.85319` and `54811 / 64221 = 0.85347` reproduce the recorded rates, so the figures
are internally consistent rather than asserted. Both final-state figures clear the 85% floor. The
denominator is invariant at 64221 across all three runs, which is the mechanical proof that no
production file changed instrumented size. This cycle changed no production file at all, so no figure
above can have moved since they were taken.

Recommendation, unchanged from both predecessors and still not acted on: copy the post-processed
Cobertura document into `evidence/coverage/` on future runs so a later reviewer can re-parse it
directly instead of re-deriving from recorded root attributes.

## 6. Verification of the cycle-2 remediation

Cycle 2 is commit `d7faef54`, text-only in one file.

```
$ git diff --numstat d7faef54^ d7faef54 -- QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs
2	2	QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs
```

**Item 1 — CR-6, line 179. Remediated.** The `because:` argument of the first assertion in
`UnregisterNavigation_AfterRegisteringAtTwoDigitsAndShrinkingToNine_RemovesTheTwoDigitKeys` now
reads `"the ledger replays each recorded key verbatim, so no '0'-prefixed key survives"`. It names
the delivered mechanism. The prior text named `_registeredDigits`, a field this branch deleted;
`git grep "_registeredDigits"` over `*.cs` returns zero occurrences, confirming there is no recorded
width for any message to refer to.

**Item 2 — CR-2, line 145. Remediated.** The sentence now reads
`After the / #472 fix, it replayed the recorded width and removed "01".."09".` It is attributed to
#472 and in the past tense, so the paragraph that follows reads as the update it is. The second
paragraph of that `<summary>` block, which the inputs placed out of bounds, is unchanged.

**Stated constraints, each checked against the tree rather than accepted on report:**

| Constraint | Measured |
|---|---|
| Comment and string-literal text only | Line 145 is an XML documentation line. Line 179 is the contents of a `because:` string argument. No assertion operator, subject, or expected value changed. |
| No test name changed | 3 method names at head are identical to the 3 at `d7faef54^` |
| No attribute changed | 3 `[TestMethod]`, unchanged |
| No `.Should()` chain changed | The chains at 175-186 and 219-223 are structurally identical; only the literal at 179 differs |
| File remains 226 lines | measured 226 |
| File retains 3 `[TestMethod]` | measured 3 |
| Cycle-1 correction at line 222 undisturbed | reads `"the ledger replays each key verbatim, so every key is removed regardless of group count"` — unchanged |
| Cycle-1 correction at lines 189-196 undisturbed | reads `After the fix the ledger replays / the nine recorded keys "1".."9" verbatim ...` — unchanged |

**Formatter reflow did not occur.** The replacement at line 179 is nine characters longer than the
text it replaced, and the enclosing `.BeEmpty(` call kept its single-argument single-line form. The
re-run of `csharpier check` in section 3 confirms the file is at formatter fixed point.

**Class-scoped sweep, re-run independently:**

```
$ grep -rn --include=*.cs -iE "recorded (registration )?width|loop bound" .
QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs:305: ... before the ledger the loop bound dereferenced that null field and
QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs:145: /// #472 fix, it replayed the recorded width and removed "01".."09".
QuickFiler/Controllers/QfcCollectionController.cs:2372: // was allocated as Count + 1 while the loop bound stayed Count, so the trailing
```

Three hits, each read in context and each legitimate past-tense history:

- `NavigationLedgerTests.cs:305` — "before the ledger the loop bound dereferenced that null field",
  explicitly past tense and accurate.
- `NavigationDigitsTests.cs:145` — the remediated sentence itself, now past tense and attributed to
  #472.
- `QfcCollectionController.cs:2372` — an unrelated pre-existing comment about issue #469's
  diagnostics array in a different method, past tense.

The defect class is closed: no surviving hit asserts a deleted mechanism as a present-tense reason.
The count is three rather than the two the cycle-2 inputs anticipated; that difference is explained
at OB-3 and is not a shortfall.

## 7. Code change policy compliance

| Rule | Source | Result |
|---|---|---|
| Simplicity first | `general-code-change.md` | PASS — a ledger field plus a lazy accessor replaces a derived-format loop |
| Separation of concerns | `general-code-change.md` | PASS — no I/O introduced |
| Fail fast, no silent error swallow | `general-code-change.md` | PARTIAL — the discarded `bool` from `Remove` is pre-existing, recorded at CR-3, non-blocking |
| File size <= 500 lines | `general-code-change.md` | FAIL on the production file, pre-existing — PA-2 |
| Comments synchronized with behavior | `CLAUDE.md` C#6.3 | PASS — CR-1, CR-2 and CR-6 all closed and re-verified; class-scoped sweep clean |
| Bugfix workflow: failing test first | `CLAUDE.md` | PASS — T1 recorded red at `[P1-T4]`, green at `[P2-T5]` and in this audit's own run |
| Bugfix workflow: minimal targeted fix | `CLAUDE.md` | PASS — net +9 production lines across three members and the field block |
| Deeper problems open a new issue | `CLAUDE.md` | PASS — CR-3, CR-5 and PA-8 routed as promotion candidates rather than widened into this fix |
| No policy documents modified | scope invariant | PASS — no path under `.claude/rules/` or `.github/instructions/` in the branch diff |
| Tonality | `.claude/rules/tonality.md` | PASS — feature-folder artifacts read factually; no hyperbole, humour, or decorative metaphor found |

## 8. Host identity sweep

Run with the account, machine, and user-profile tokens **derived at runtime** and consumed through
`grep -f`, so no token is spelled in this artifact or in the command:

```
$ powershell -NoProfile -ExecutionPolicy Bypass -Command "$env:USERNAME; $env:COMPUTERNAME; $env:USERPROFILE" > <scratch-token-file>
$ grep -rniIf <scratch-token-file> docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644
(no output, grep exit 1)
```

Zero hits across every file of the feature folder. PA-7 remains closed.

The same sweep was then extended to the 15 `.claude/agent-memory/**` paths the branch adds, which no
predecessor audit covered because commit `4572fef5` did not yet exist. It returned four hits. Three
are in files that exist byte-identically at the merge base (`git cat-file -e` confirms each), so the
branch neither introduces nor can cure them; they belong to the pre-existing class already tracked as
issue #685. One is introduced by this branch and is recorded at PA-10.

**Verdict: PASS**, with PA-10 recorded and cured in-session.

## 9. Disposition of every predecessor finding

| ID | Title | Status at this exit | Blocking |
|---|---|---|---|
| PA-1 | `[ExcludeFromCodeCoverage]` sits against the Coverage Exclusion Policy | STANDS, pre-existing | No |
| PA-2 | Production file exceeds the 500-line ceiling | STANDS, pre-existing | No |
| PA-3 | Canonical C# coverage document absent | STANDS, procedural | No |
| PA-4 | One sentence in the coverage evidence overstates the exclusion argument | STANDS, deliberate | No |
| PA-5 | Superseded "AC-16 is checked off" sentence in a recorded run artifact | STANDS, deliberate | No |
| PA-6 | `[P4-T8]` checked although its literal sixth clause did not hold | STANDS, deliberate | No |
| PA-7 | Absolute host path in committed feature-folder documents | CLOSED at cycle 1, re-verified here | No |
| PA-8 | Analyzer HintPath version skew in two projects | STANDS, pre-existing, promote separately | No |
| OB-1 | PR context artifacts stale at cycle-1 reaudit entry | CLOSED — see below | No |
| CR-1 | Stale mechanism description in a retained XML doc | CLOSED at cycle 1, re-verified here | No |
| CR-2 | Historical sentence names a removal set that is now wrong | **CLOSED by cycle 2** | No |
| CR-3 | The discarded `bool` from `Remove` is now a meaningful signal | STANDS | No |
| CR-4 | Lazy accessor read twice; allocates in order to clear | STANDS | No |
| CR-5 | Ledger is a non-thread-safe `List<T>` behind a non-atomic `??=` | STANDS | No |
| CR-6 | Fourth instance of the stale-mechanism class at line 179 | **CLOSED by cycle 2** | No |

Re-test detail for the items that could have moved:

**PA-1 — STANDS, pre-existing, non-blocking.** `[ExcludeFromCodeCoverage]` is at line 21 of the
production file. Provenance was re-checked rather than assumed:

```
$ git show e968a1a8:QuickFiler/Controllers/QfcCollectionController.cs | grep -n "ExcludeFromCodeCoverage"
21:    [ExcludeFromCodeCoverage]
```

The attribute is present at the anchor, and the production diff's hunks begin at lines 117, 1175 and
1183 — none of them near line 21. The attribute is neither added nor widened by this change, so the
argument that rests on it is not circular. The tension between `.claude/rules/general-unit-test.md`
and the maintainer-ratified COM/VSTO exemption in `CLAUDE.md` is a repository-level question, not a
defect authored here.

**PA-2 — STANDS, pre-existing.** Re-measured: base 2437 lines, head 2446, net +9, inside AC-14's
bound of 10. The violation is pre-existing at 4.9x the ceiling.

**PA-3 — STANDS, procedural.** Re-verified in section 5. The `coverage/` directory is now empty in
this worktree, so the cycle-2 TRX is no longer on disk here; the evidence artifact is the basis
instead, corroborated by this audit's own test re-run.

**PA-4, PA-5, PA-6 — STAND, deliberate.** All three concern recorded run artifacts that the
remediation declined to rewrite, correcting forward instead. That disposition is sound and is
endorsed again. PA-5's back-pointer gap — the `p4-t6` artifact's closing sentence still says AC-16 is
checked off, while `spec.md` shows it unchecked at line 707 — remains open and is still best cured by
an append-only superseded footer rather than an edit.

**PA-8 — STANDS, pre-existing, promote separately.** Re-verified: neither `UtilitiesCS.csproj` nor
`VBFunctions.csproj` appears in `git diff --name-only fa2ddefa...HEAD`. This cycle produced one new
data point that strengthens the cycle-1 analysis rather than changing its verdict: the resuming
orchestrator's bootstrap artifact records that supplying a `packages/` tree containing
`Meziantou.Analyzer.3.0.156` and `Roslynator.Analyzers.4.16.0` was a precondition for the analyzer
build to run at all. That confirms the skew is real and that the unconditional `Analyzer Include`
items require the older pair, while `packages.config` declares the newer pair. It remains
byte-identical at the merge base, untouched by this branch, and green on CI at `main`. It does not
block this pull request and is owed a separate issue.

**OB-1 — CLOSED.** `artifacts/pr_context.summary.txt` now records
`Head ref (resolved): bug/qfc-unregister-navigation-count-mismatch-orphan-644 @ 4572fef5...` and
`Merge base: fa2ddefa...`, both matching `git rev-parse HEAD` and `git merge-base HEAD origin/main`
measured this session. The artifacts are fresh at the branch tip; no regeneration was required.

**CR-3, CR-4, CR-5 — STAND.** This cycle touched no production file
(`git diff --name-only d7faef54^..HEAD -- QuickFiler/` is empty), so all three are necessarily
unchanged. Each was nonetheless re-read against the production diff in the code review at this
timestamp.

## 10. Findings raised by this reaudit

### PA-9 — the branch tip adds 15 paths outside the spec's enumerated Blast Radius (Minor, Non-blocking)

- **Location:** commit `4572fef5`, the 15 `.claude/agent-memory/**` paths listed in section 1.
- **Clause at issue:** `spec.md` Blast Radius states "Seven paths total: one created, six modified.
  Any path outside this list appearing in `git diff --name-only` is a scope violation and is a
  blocking finding at review." AC-14 restates the enumeration clause.
- **Measured:** the branch diff contains the seven enumerated paths, roughly 85 further feature-folder
  process artifacts, and these 15 agent-memory paths.
- **Why it is non-blocking, stated plainly rather than waved through.** Three reasons, and the third
  is the decisive one.
  1. The property AC-14 exists to guard — code-footprint containment — holds exactly: one production
     file modified, none added, `QuickFiler.csproj` unchanged, no interface file touched, production
     net +9 lines inside the stated bound of 10.
  2. The same literal clause was already read in a carve-out sense by both predecessor audits, which
     passed AC-14 while roughly 85 feature-folder artifacts sat outside the seven. `spec.md` itself
     carves out feature-folder process artifacts as "process outputs of the orchestration workflow,
     not part of the fix's code diff". Agent-memory files are the same category of process output;
     the spec simply did not anticipate them because it predates the checkpoint commit.
  3. Committing agent memory on a feature branch is established repository practice, not a deviation:
     `main` already carries such commits, including `a90d8a43` and `4cf822e8`. Merging these files
     carries no product risk and no build effect.
- **Recommendation:** when the next spec in this family writes a Blast Radius, add `.claude/agent-memory/**`
  to the same carve-out sentence that already covers feature-folder evidence, so the literal clause
  matches the practice the repository actually follows.

### PA-10 — a host-identity string introduced into agent memory by the branch tip (Minor, Non-blocking, cured in-session)

- **Location:** `.claude/agent-memory/feature-review/project_644-review-residuals.md`, line 55, a file
  **added** by commit `4572fef5` (`git cat-file -e fa2ddefa:<path>` fails, so it is new on this
  branch).
- **What it was:** the PA-7 residual bullet quoted the offending path in full, drive-rooted and
  including the account name, rather than quoting it in redacted form. The irony is exact: it is the
  memory entry that records PA-7.
- **Violated rule:** the repository's no-absolute-host-path convention, the same one PA-7 was raised
  and closed under.
- **Why it escaped every prior sweep:** commit `4572fef5` did not exist when the cycle-1 exit audit
  ran, and the delegating prompt for this cycle excluded `.claude/agent-memory` from the audited diff.
  It was found only because that exclusion was rejected under the scope invariant.
- **Cure applied in this session:** the bullet now names the shape of the path rather than reproducing
  it. No finding, verdict, figure, or location citation in that memory file was altered; the edit is
  one line replaced by one line, so every subsequent line keeps its number.
- **Verification after the edit:** the runtime-token sweep over the 15 branch-added agent-memory paths
  returns no hit attributable to this branch. Three residual hits remain in files that exist
  byte-identically at the merge base — `feature-review/project_464-review-residuals.md`,
  `feature-review/project_488-review-residuals.md`, and
  `atomic-executor/project_bash_heredoc_collapses_doubled_backslashes.md` — which the branch neither
  introduces nor can cure. They belong to issue #685.
- **Why non-blocking:** the leak is in an agent-memory index bullet, not in product code or in a
  reviewable audit artifact; it is one line; it is already cured; and the surviving instances of the
  same class are pre-existing and separately tracked.
- **Note for the orchestrator:** this edit leaves
  `.claude/agent-memory/feature-review/project_644-review-residuals.md` modified and unstaged. It
  should be committed with the review artifacts.

### OB-2 — the PR-context summary's changed-file list cannot be used to derive changed languages (Observation, no action)

`artifacts/pr_context.summary.txt` contains exactly ten `- <path> (+N/-N)` bullets, and all ten are
Markdown files: the generator lists only the largest files by churn, and this branch's documentation
churn dwarfs its code churn. An automated reader of that list would conclude the branch changes no
code language at all, which is wrong. This audit derived the changed-language set from
`git diff --name-only` instead, and section 2 records the result. Recorded so the discrepancy is
attributable rather than surprising; no artifact under `artifacts/` was modified, per the review
contract.

### OB-3 — the cycle-2 exit condition anticipated two sweep hits; three are correct (Observation, no action)

`remediation-inputs.2026-08-30T02-08.md` states the exit condition as "the class-scoped sweep
returning only the two legitimate past-tense hits". The sweep returns three. The difference is a
consequence of how item 2 was correctly remediated: CR-2 was cured by moving the sentence into the
past tense and attributing it to #472, which preserves the phrase "recorded width" as history rather
than deleting it. The inputs' arithmetic assumed deletion. The substantive exit condition — that no
surviving hit asserts a deleted mechanism as a present-tense reason — is met, and all three survivors
were read in context in section 6.

## 11. Compliance verdict

| Policy | Verdict |
|---|---|
| `CLAUDE.md` standing instructions | PASS |
| `.claude/rules/general-code-change.md` | PASS, with PA-2 recorded pre-existing |
| `.claude/rules/general-unit-test.md` | PASS, with PA-1 recorded pre-existing |
| `.claude/rules/quality-tiers.md` | PASS |
| C# code change policy | PASS |
| C# unit test policy | PASS |
| Evidence location conventions | PASS |
| Tonality policy | PASS |
| Scope invariant | PASS, one caller-supplied exclusion rejected and audited |

**Total blocking findings: 0.**

**Merge recommendation: GO.** The exit condition in `remediation-inputs.2026-08-30T02-08.md` is met:
both cycle items are independently verified closed, the class-scoped sweep returns only legitimate
past-tense history, and the total blocking count is zero. No remediation-inputs artifact is produced
at this timestamp, because no finding is remediation-required and the third permitted cycle should
not be spent.
