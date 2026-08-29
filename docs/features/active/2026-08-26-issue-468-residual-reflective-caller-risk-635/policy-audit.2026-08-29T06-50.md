# Policy Audit — Issue #635 Residual Reflective-Caller Risk

- **Issue:** #635
- **Branch:** `bug/issue-468-residual-reflective-caller-risk-635`
- **Base branch:** `main`
- **Merge base:** `b56400ab663a85b6039139d4548f408821e957ce` (equals `origin/main`)
- **Head reviewed:** `73bd8082e1776d7957ca0c9a3226b3587e4a658f`
- **Work mode:** `full-bug` (AC source: `spec.md` only)
- **Timestamp:** 2026-08-29T06-50
- **Verdict:** PASS — 0 blocking findings

## 1. Scope Resolution

The audit scope is the full branch diff against the resolved base branch, recomputed in this review
rather than accepted from the caller.

```
git rev-parse HEAD          -> 73bd8082e1776d7957ca0c9a3226b3587e4a658f
git rev-parse origin/main   -> b56400ab663a85b6039139d4548f408821e957ce
git merge-base HEAD origin/main -> b56400ab663a85b6039139d4548f408821e957ce
```

The merge base equals `origin/main`, so the branch is 4 commits ahead and 0 behind. The four commits
are `44f4a802`, `d6cfb21c`, `53bfe771`, `73bd8082`.

### Language composition of the branch diff

Measured independently of the executor's own claim:

```
pwsh -NoProfile -Command '$p = git diff --name-only origin/main...HEAD; "TOTAL: " + $p.Count;
  "NON_MD_COUNT: " + (@($p | Where-Object { $_ -notlike "*.md" }).Count)'
TOTAL: 32
NON_MD_COUNT: 0
```

Every changed path carries the `.md` extension. No `.cs`, `.csproj`, `.props`, `.targets`, `.resx`,
`.config`, `.settings`, `.xaml`, `.ps1`, `.py`, `.ts` or `.tsx` file is added, modified, or deleted
anywhere on the branch.

The caller's directive stated 34 paths; the measured diff is 32 paths. The discrepancy is in the
caller's count, not in the branch, and it does not affect any conclusion: the all-Markdown property
holds over the 32 measured paths.

### Rejected Scope Narrowing

None. The caller prompt directed a full-branch audit and supplied no instruction to narrow scope to a
plan, task, phase, or file subset. The caller's instruction not to emit coverage artifacts is a
prohibition on fabricating measurements for languages with zero changed files, not a scope narrowing,
and is consistent with the coverage rules applied in section 6.

## 2. Policy Reading Order Applied

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/quality-tiers.md`, `.claude/rules/tonality.md`

Language-specific rules (`.claude/rules/csharp.md`, `.claude/rules/powershell.md`) govern no file in
this diff, because the diff contains no file of either language.

## 3. Evidence Location Compliance

| Check | Result |
|---|---|
| Files written under `artifacts/baselines/` | 0 |
| Files written under `artifacts/qa/` | 0 |
| Files written under `artifacts/evidence/` | 0 |
| Files written under `artifacts/coverage/` | 0 |

All 18 evidence artifacts are written under
`docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/<kind>/`,
using the canonical kinds `baseline/`, `other/`, `qa-gates/` and `regression-testing/`. This is the
location required by `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.

`validate_evidence_locations.py` is not present in this repository, so the scan was performed with a
path-prefix filter over the branch diff instead. No non-canonical evidence path appears in the diff.

**Verdict: PASS.**

## 4. General Code Change Policy

| Rule | Verdict | Evidence |
|---|---|---|
| No production code, test code, or reusable script file exceeds 500 lines | PASS | No such file is added or modified. All 32 changed paths are Markdown, which `.claude/rules/general-code-change.md` exempts from the limit. |
| I/O boundary and design principles | PASS (vacuous) | No executable code changes. |
| No temporary files created in tests | PASS | No test is added or modified. |
| Dependencies unchanged | PASS | No `packages.config`, `.csproj`, or manifest is modified. |
| Bugfix workflow: fix scope not widened | PASS | The item records rather than repairs; the non-goals section fixes the disposition in advance and no repair was performed because no caller was found. |

## 5. Toolchain Gate Applicability

The seven-stage toolchain loop applies to the languages present in the diff. The diff contains
Markdown only, so no stage has an input file.

| Gate | Applicability | Basis |
|---|---|---|
| CSharpier format | No input on this branch | Zero `.cs`/`.xml`/`packages.config` paths in the diff |
| .NET analyzers (msbuild) | No input on this branch | Zero C# compilation inputs in the diff |
| Nullable / type-check (msbuild) | No input on this branch | Zero C# compilation inputs in the diff |
| MSTest via vstest | No input on this branch | Zero test-project inputs in the diff |
| PSScriptAnalyzer / Pester | No input on this branch | Zero `.ps1`/`.psm1` paths in the diff |

The executor recorded this as `TOOLCHAIN_BRANCH: 2` in
`evidence/qa-gates/p4-t3-toolchain-gate.2026-08-29T04-55.md`. **The branch selection was assessed
against the actual diff rather than by re-running the gates**, as directed. The branch-one condition
is "any path outside the feature folder carrying a C# or PowerShell extension". The measured diff
contains zero such paths across all 32 entries, so branch two is the correct selection. The six
non-feature-folder paths are all `.claude/agent-memory/**/*.md`, which are in neither extension set.

The repository carries a pre-existing analyzer version skew — `packages.config` pins
Meziantou.Analyzer 3.0.174 and Roslynator.Analyzers 4.16.1 while the hand-written Analyzer items name
3.0.156 and 4.16.0 — which makes msbuild fail CS0006 in a fresh worktree. This is not introduced by
this branch and is not a finding against it. It is also not load-bearing here, because the gates have
no input regardless.

**Verdict: PASS.**

## 6. Coverage Verification

No language with changed files on this branch is a coverage language. All 32 changed paths are
Markdown. The per-language rows below are therefore recorded for languages that have **zero** changed
files in the branch diff, which is the only circumstance in which a non-PASS/FAIL verdict is
permitted by the scope invariant.

| Language | Changed files in branch diff | Coverage verdict |
|---|---|---|
| C# | 0 | not applicable — zero changed files |
| PowerShell | 0 | not applicable — zero changed files |
| Python | 0 | not applicable — zero changed files |
| TypeScript | 0 | not applicable — zero changed files |

No coverage artifact was generated, read, or emitted by this review. Generating one would fabricate a
measurement of a quantity this branch does not change and would expose an unrelated repository-wide
threshold to a gate this branch cannot influence. The following canonical paths were confirmed absent
in the worktree and were deliberately left absent: `coverage/lcov.info`, `artifacts/python/lcov.info`,
`artifacts/pester/powershell-coverage.xml`, `artifacts/csharp/coverage.xml`.

The no-regression requirement is satisfied trivially: the item changes no executable line, so no
changed line can regress.

**Verdict: PASS.**

## 7. Host-Identity Hygiene

Every one of the 32 changed files was scanned for absolute host paths, account names, and machine
names:

```
pwsh -NoProfile -Command '$files = git diff --name-only origin/main...HEAD;
  $pat = "<drive>:\\\\Users|/c/Users|<user>|<USER-SHORT>|worktrees\\\\agent-|DESKTOP-|LAPTOP-";
  ... Select-String -LiteralPath $f -Pattern $pat -AllMatches ...'
FILES_WITH_HOST_TOKENS: 2
```

The pattern operands are shown above with the account name and short name replaced by the
placeholders `<user>` and `<USER-SHORT>`; the executed command used the literal values.

Both matches are in `.claude/agent-memory/**/MEMORY.md` and are the anti-leak rule's own placeholder
text — the literal string `` `C:\Users\<account>\...` `` used to state the prohibition. Neither
contains a real account name, machine name, or resolved host path.

Zero genuine host-identity leaks. The `[P1-T5]` artifact additionally documents that its command
prints only repository-relative paths from `git ls-files`, never a resolved PowerShell provider path,
which is the usual source of such a leak.

**Verdict: PASS.**

## 8. Non-Vacuity of Zero Results (primary risk area)

The central claims of this item are negative results. Each was re-executed in this review against the
current head, not accepted from the artifact.

### Partition A — the zero that matters

```
git grep -n -I -F -e WireUpKeyboardHandler ... -e _templateTlp \
  -- ":(exclude)*.cs" ":(exclude)docs/*" ":(exclude).claude/*"
(no output)
EXIT: 1
```

Reproduced exactly: zero selected lines, exit code 1. The artifact declares `ExpectedExitCode: 1`,
which is the correct success code for a bare `git grep` that selects nothing. The exit-code discipline
required by the specification is observed and the two styles (bare search versus counting wrapper) are
not mixed within a single artifact file.

### Measured scope

```
SCOPE_FILES_A: 683
AC16_SIX_EXTENSION_SCOPE: 153
TRACKED_CS: 1599
```

All three reproduce the recorded values exactly. The twelve-row extension census reproduces exactly
(`.md 190`, `.toml 96`, `.svg 77`, `.resx 62`, `.ps1 51`, `.config 38`, `.png 28`, `.json 28`,
`.csproj 18`, `.bak 11`, `.txt 9`, `.sh 9`). The scope is non-empty and contains the file types the
search claims to cover, including eight extensions outside the six the AC-16 search reached.

### The P1-T2 control does the job it exists to do

The control was re-run and returns 13 hits across 4 files for `QfcCollectionController` under the
**identical pathspec and flags**, differing from P1-T1 in exactly one respect: the search patterns.
Two of the four files are decisive:

- `QuickFiler/Notes/notes_interface_hierarchy` — an extensionless tracked file, unreachable by any
  extension-based search;
- `QuickFiler/QuickFiler.csproj.bak` — a `.bak` file, outside the six build-input extensions.

This establishes that the widened pathspec reaches real content that the narrower AC-16 scope could
not, so the P1-T1 zero is a measurement of absence rather than an artefact of an unreachable corpus.
**The control discharges its purpose.**

**Verdict: PASS.**

## 9. Total Classification (Partitions B and C)

### Partition B

Re-run at review head:

```
TOTAL=2474  CAT_D_DOCS=2456  CAT_E_CLAUDE=18  CAT_G_OTHER=0
```

The recorded values were 2337 / 2319 / 18 / 0. The total has drifted upward again since execution,
and **both acceptance identities still hold at this third, later commit**: `2456 + 18 = 2474 = TOTAL`
and `CAT_G_OTHER = 0`. This independently confirms the artifact's central argument that the acceptance
condition is invariant under prose accretion while a fixed hit count would not have been.

The category tests are path-derived (`-like "docs/*"`, `-like ".claude/*"`, residue) and require no
reading of hit text, so the assignment is mechanical rather than a judgment call. The tests are
exhaustive and mutually exclusive over the hit set, which is what makes the summation identity
equivalent to "every hit received exactly one category".

### Partition C

Re-run at review head:

```
PARTITION_C_HITS: 31   DISTINCT_FILES: 12
```

Both reproduce exactly, and the residual-category probe (every hit not under `TaskMaster/`,
`TaskMaster.Test/`, `UtilitiesCS/`, `QuickFiler/Controllers/QfcCollectionController.cs`, or
`QuickFiler.Test/`) returned empty, independently corroborating `CAT_G: 0`.

**Enumeration completeness: the per-hit table was counted row by row and contains exactly 31 rows**,
numbered 1 through 31, one per printed line. `CAT_A 2 + CAT_B 28 + CAT_C 1 + CAT_G 0 = 31`.

### Ordering dependence, verified

The caller flagged one row whose class turns on test ordering. Verified directly:

```
LINE20_RAW: [        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
              System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);]
FIRST_TOKEN_IS_SLASHSLASH: True
CONTAINS_GETCURRENTMETHOD: True
```

`QuickFiler/Legacy/QuickFileController.cs:20` contains `MethodBase.GetCurrentMethod()` and so would
satisfy the L1 text test, but its first non-whitespace token is `//`, which the L1 test explicitly
excludes. Under the stated order it lands in L3, exactly as the artifact records. **The ordering claim
is correct and the tests are applied in the stated order.**

The equivalent ordering statement in Partition C — that ten category B rows would satisfy the
category C test in isolation but take B because B precedes C — is consistent with the enumerated
table and with the printed output.

**Verdict: PASS.**

## 10. Closure Argument and Its Stated Limit

The P2-T3 argument bounds the values a member-name variable can take by the string literals present
in the calling assemblies' source text. Assessment:

- **The premise was verified independently and by a stronger test than the artifact used.** A search
  for the *quoted* forms of the identifiers across the QuickFiler test tree returns nothing (exit 1),
  so no string literal in that tree equals any of the thirteen. The artifact derives this from
  P1-T4's single `///` comment hit; the direct quoted-form search corroborates it.
- **The eight sites were verified in source.** All six `QfcCollectionController.TestSupport.cs` sites
  (38, 51, 65, 80, 95, 118) pass the bare identifier `name`; `QfcCollectionControllerTests.cs` 381/382
  and `QfcCollectionControllerNavigationDigitsTests.cs` 34/35 are two-line forms as described. In
  every case `name` is the `string name` parameter of a private static helper.
- **The named constant was verified.** `private const string ReentrancyCounterField =
  "removespecificcontrolgroupcounter";` at line 30. The value is not one of the thirteen, and the
  literal-equivalence reasoning for a `const string` is sound.
- **The derivation is total over the printed set.** `8 + 3 + 6 + 1 + 8 = 26`, which equals the 26
  lines command 1 printed (independently re-measured: `TYPEOF_LINES: 26`).
- **The limit is recorded, not argued away.** A member name assembled at run time by concatenation or
  interpolation would not appear as a literal and escapes the bound. This is stated in the closure
  artifact, restated in the decision record as "the class not proved absent", and carried in the
  specification's risk table. The decision record's two mitigations (test code rather than shipped
  code; helpers assert non-null so failure is loud) are explicitly labelled mitigations rather than
  closure.

The argument is sound within its stated limit, and the limit is disclosed at every level of the
evidence chain rather than buried. **Verdict: PASS.**

## 11. Reflection Inventory and Surface Checks

Re-measured independently:

```
QF_PROD_SCOPE_FILES: 228     QF_TEST_SCOPE_FILES: 151
GetField(  test=172 prod=0
GetMethod( test=69  prod=0
GetProperty( test=24 prod=0
GetMembers(  test=0  prod=0
InvokeMember( test=0 prod=0
```

Every figure reproduces exactly, including the `GetField(` family that the AC-16 inventory omitted.
The production tree returns zero for the combined name-resolving sweep (exit 1).

The `[assembly: ComVisible(false)]` claim was verified at `QuickFiler/Properties/AssemblyInfo.cs:22`.

**Verdict: PASS.**

## 12. Corrections to the AC-16 Record

Both corrections were verified against the original artifact rather than accepted from the summary.

- **Correction 1 (omitted thirteenth identifier).** Verified: `git show --stat 63eebd47` shows one
  source file changed, a pure deletion of 241 lines, and the P0-T4 filter output quotes
  `-        private TableLayoutPanel _templateTlp;` as a removed field declaration. The commit subject
  names the field explicitly.
- **Correction 2 (superseded test-tree claim).** Verified in the original AC-16 artifact at
  `docs/features/active/qfc-collection-controller-defects-468/evidence/other/p1-t1-reflective-caller-search.2026-08-26T08-25.md:205`,
  which reads "**Zero hits anywhere in `QuickFiler.Test`.** No test file contains any of the twelve
  identifiers". That claim no longer holds, and the superseding occurrence is correctly identified by
  file, line, and category.

The decision not to edit the historical AC-16 artifact is correct and is consistent with the
specification's non-goals.

**Verdict: PASS.**

## 13. Disposition of the Two Recorded Evidence Notes

The caller asked whether recording each was the right disposition or whether either should have
blocked. Both dispositions are judged correct.

### Note (a) — AC-9 names six sites, the derivation yields eight

**Recording was correct; blocking would have been wrong.** Three reasons:

1. The derivation yields a **superset**. Enumerating eight sites individually, each with its closure
   statement and the shared stated limit, over-satisfies AC-9's substantive requirement. No site that
   AC-9 could have meant is left unenumerated.
2. No six-element subset is identifiable with the specification's six, because the measured set is
   seven `GetField(` sites plus one `GetMethod(` site while the specification describes six
   `GetField(` sites. The executor stated this plainly instead of silently selecting a subset to make
   the figure agree — which is the failure mode that would have deserved a blocking finding.
3. Editing the approved specification to change the figure is prohibited by the
   acceptance-criteria-tracking protocol ("preserve text", "no phantom criteria"). Recording in
   evidence was the only compliant route.

The residual cost is that the specification's Verified Baseline Measurements section retains a figure
now known to be wrong. That is carried as non-blocking finding NB-4.

### Note (b) — reference drift from `b56400ab` to `d6cfb21c`

**Recording was correct; blocking would have been wrong.** The two moved figures, `TRACKED_TOTAL`
(11866 to 11873) and Partition B `TOTAL` (2229 to 2337), are both explicitly non-asserted reference
values. No asserted value moved: `SCOPE_FILES` 683, `AC16_SIX_EXTENSION_SCOPE` 153, `TRACKED_CS` 1599,
Partition C 31, and every test-column inventory value reproduced exactly.

This review supplies a third data point that settles the question. At review head the drift has
continued — `TRACKED_TOTAL` is now 11895 and Partition B `TOTAL` is now 2474 — while **every asserted
value still reproduces exactly and both Partition B identities still hold**. The figures that move are
precisely the ones the artifacts declined to assert, and the figures that are asserted are structurally
immune to the movement, because the Partition A pathspec excludes both trees into which this branch
writes. Blocking on drift in a deliberately non-asserted reference value would have been incorrect.

**Verdict: PASS.**

## 14. Findings Summary

| ID | Severity | Finding |
|---|---|---|
| — | Blocking | None |
| NB-1 | Non-blocking | `[P4-T2]` union of 28 paths predates the final commit; the final diff is 32 paths |
| NB-2 | Non-blocking | `[P2-T1]` substituted narrower reflection patterns than the specification's baseline list |
| NB-3 | Non-blocking | `dynamic` late binding is not enumerated as a name-resolution mechanism |
| NB-4 | Non-blocking | `spec.md` retains the superseded "six variable-argument sites" figure |
| NB-5 | Non-blocking | `[P2-T4]` COM limb does not measure per-type `ComVisible(true)` overrides |
| NB-6 | Non-blocking | `[P2-T3]` site 8 row cites the call line while sites 1-7 cite the printed grep line |
| NB-7 | Non-blocking | The AC-16 "398 build-input files" figure is never reconciled with the 153-file comparable scope |
| NB-8 | Non-blocking | PR context artifacts absent; could not be regenerated within the review's write scope |

Details are in `code-review.2026-08-29T06-50.md`.

## 15. Overall Policy Verdict

**PASS — 0 blocking findings.**

Every negative result in this item was re-executed at review head and reproduced. Every acceptance
condition is a total classification or a measured non-empty scope rather than a bare count, so none is
of the "could not have failed" shape that this review was directed to look for. The one acceptance
condition that could have failed on the branch itself — AC-12's Markdown-only diff — was verified
independently over all 32 paths.
