# 2026-09-08-coverage-aggregation-double-counts-method-rows (Atomic Plan)

- **Issue:** #815
- **Parent:** epic `review-residuals-2026-09-08` (wave 0), child feature F815
- **Owner:** drmoisan
- **Last Updated:** 2026-09-09T02-05
- **Status:** Ready for preflight (revision round 3 applied)
- **Version:** 1.3
- **Work Mode:** full-bug (marker `- Work Mode: full-bug` in `issue.md`; `spec.md` is the sole authoritative acceptance-criteria source; `user-story.md` is absent and its absence is correct by design)

---

## Standing Rules For Every Task In This Plan

**Evidence location.** Every artifact this plan names resolves under
`docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/<kind>/`
with `<kind>` one of `baseline`, `regression-testing`, `qa-gates`, `other`. Writing to
`artifacts/baseline*/`, `artifacts/qa*/`, `artifacts/coverage/` or `artifacts/evidence/` is a policy
violation and is rejected by the `enforce-evidence-locations.ps1` PreToolUse hook. Artifact
filenames are task-ID-derived and carry no timestamp token, so every asserted path in this plan is a
concrete literal with no placeholder.

**Evidence schema.** Every command-step artifact carries, at minimum, `Timestamp:` (ISO-8601
`yyyy-MM-ddTHH-mm`), `Command:` (the exact command), `EXIT_CODE:`, and `Output Summary:`. An
artifact whose gate is expected to return non-zero additionally carries `ExpectedExitCode:` with the
exact spelling shown. Baseline test-step artifacts carry numeric coverage headline values in
`Output Summary:`.

**Fail-closed evidence rule.** If any baseline artifact, QA artifact, or coverage-comparison
artifact named by this plan is missing or is missing a required field, the verdict is BLOCKED or
INCOMPLETE, never PASS. A checklist box stays unchecked while its artifact is absent or incomplete.

**Bash discipline.** Only `git *`, `pwsh *`, `poetry run *` and the three allowlisted `.claude/lib`
scripts run through the Bash tool without prompting, and every `&&` or `|` segment is checked
independently. Do not run `cd`, `grep`, `sed`, `cat`, `ls`, `cp`, `mv` or `mkdir` through Bash. Use
`git -C` when a directory must be named, and use the Read, Grep and Glob tools for inspection.

**PowerShell payload quoting.** Every `pwsh -NoProfile -Command` payload in this plan uses outer
single quotes for the shell and inner double quotes for every PowerShell string literal. Do not
reverse them.

**PowerShell toolchain order.** format -> analyze -> test. **Type checking does not apply to
PowerShell** (`.claude/rules/powershell.md` step 3 states this explicitly); the plan records that
step as NOT APPLICABLE with an artifact rather than omitting it. Restart the loop from format if any
step fails or changes a file.

**Toolchain invocation.** PowerShell gates run through the drm-copilot PoshQC MCP tools
`mcp__drm-copilot__run_poshqc_format`, `mcp__drm-copilot__run_poshqc_analyze` and
`mcp__drm-copilot__run_poshqc_test`, each with an explicit `scan_folders` argument. Never omit
`scan_folders`: omitting it silently widens the run. Coverage for changed files is measured by a
direct `Invoke-Pester` run (see the AC10 note below), because the PoshQC test tool's bundled
coverage artifact measures no file under the scripts tree.

**Observed success-case output for each gate tool.** These were observed on this branch head before
this plan was authored, and every acceptance condition below asserts only values these tools
actually print on a successful run:

- `mcp__drm-copilot__run_poshqc_format` returns an ok flag and no per-file summary line. It is a
  write-mode tool, so its exit status alone cannot distinguish a clean run from a repairing one.
  Every format task in this plan therefore pairs it with a scoped `git status --porcelain` tree
  observation, which is the discriminating signal.
- `mcp__drm-copilot__run_poshqc_analyze` returns an ok flag plus a sentence of the form
  `PSScriptAnalyzer reported N issue(s)`, and exits non-zero when N is greater than zero. It is the
  count that is asserted; no per-rule or per-file breakdown is demanded of it.
- `mcp__drm-copilot__run_poshqc_test` returns an ok flag and writes `artifacts/pester/pester-junit.xml`.
  Test totals are read from that file, not from the tool's return value.
- `Invoke-Pester` **does not set a non-zero exit code when an `It` block fails** unless `Run.Exit` is
  enabled. Every direct-Pester task in this plan therefore asserts on `FailedCount` and `PassedCount`
  from the `-PassThru` result object and records `EXIT_CODE: 0`. Do not write `ExpectedExitCode: 1`
  for a red Pester run.
- The direct `Invoke-Pester` coverage run writes a JaCoCo document whose per-file figures are read as
  a `<counter>` child of a `<sourcefile>` element carrying `type="LINE"`, `covered` and `missed`
  attributes. P0-T8 is the first task to read that shape and its acceptance requires both named files
  to appear with numeric values, so a shape mismatch fails P0-T8 loudly rather than yielding a silent
  zero. If P0-T8's reader prints nothing, record the observed element and attribute names in
  `evidence/baseline/p0-t8-coverage-baseline.md` and report the discrepancy; do not substitute a
  different selector without recording the observation that motivated it.

**Merge-base anchor.** Every committed-state diff gate in this plan is anchored with the three-dot
form `epic/review-residuals-2026-09-08-integration...HEAD`, which git resolves through the merge base.
The ref `epic/review-residuals-2026-09-08-integration` exists as a local ref in the shared common git
directory and therefore resolves from this worktree. No task pins a commit SHA as an expectation. A
task that must observe an edit **before** that edit is committed uses the two-dot form
`git diff --numstat epic/review-residuals-2026-09-08-integration --` followed by the path, which
compares the named ref against the working tree, because the
three-dot form compares committed trees only and prints nothing for an uncommitted edit. P2-T2 and
P2-T3 are the two tasks in that position, and P4-T5 carries the committed three-dot re-verification
for both of them.

**Untracked-file visibility.** `git grep` and `git diff --name-only` see only files present in the
index, so both are blind to a file this plan creates until it is staged. Every gate that must see a
plan-created file runs after the staging or commit task that puts it in the index, and every
name-listing diff is paired with `git status --porcelain --untracked-files=all` in the same task.
`--untracked-files=all` is required because plain porcelain collapses a new directory to a single
entry.

**Tracked-noise carve-out.** `.claude/agent-memory/` is a tracked directory that agents write during
a run, so an unscoped repository-wide `git status` or `git diff` gate is not satisfiable. Every such
gate in this plan is either pathspec-scoped or carries an explicit `.claude/agent-memory/`
carve-out, and the carve-out is stated as a rule rather than as a list of file names.

---

## Decisions This Plan Makes, With Rationale

### D1 — File placement for the new function (this is the AC11 file-placement decision)

**Decision.** Create one new production file `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1`
holding three functions, and dot-source it from `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`
with one added line. Create one new test file
`tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1`. Do not add the function to
`Invoke-MSTestWithCoverage.Helpers.ps1` and do not add its tests to
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`.

**Rationale, from physical line counts re-derived against the current tree in this authoring pass.**
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` is 469 lines, leaving 31 lines of headroom
against the 500-line ceiling in `.claude/rules/general-code-change.md`. Three functions with
comment-based help do not fit in 31 lines. The repository has already solved this twice and recorded
the reasoning in code: `scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1` lines 20-23 and
`scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` lines 14-17 each state that the function
lives in its own file because Helpers.ps1 is at the ceiling, and that Helpers.ps1 dot-sources the file
so a caller that dot-sources Helpers.ps1 alone still resolves the function. That is exactly the AC1
resolution requirement, so following the precedent satisfies AC1 rather than working around it.

**A finding this plan adds that `spec.md` does not record.** `spec.md` lists file sizes for
production files only. The test tree is tighter than the production tree:
`tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` is 496 lines,
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` is 494 lines, and
`tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` is 486 lines. Adding this
feature's tests to Helpers.Tests.ps1 would reach the 500-line ceiling on the sixth added line and
breach it on the seventh, because the ceiling permits 500 lines and forbids 501. A new test
file is therefore the only placement that satisfies AC11.

**Change budget check.** `.claude/rules/powershell.md` caps a batch at 3 production files and 3 test
files. The batch-budget hook counts any `.ps1` inside the worktree root that is neither under
`tests/` nor named `*.Tests.ps1` as a production file, whatever `.gitignore` says, so the count below
must include every such file this plan writes. This plan modifies or creates exactly 3 production
files inside the worktree root (`Invoke-MSTestWithCoverage.FirstParty.ps1` new,
`Invoke-MSTestWithCoverage.Helpers.ps1` one added line, `Invoke-MSTestWithCoverage.ps1` one added
line) and exactly 1 test file. Within cap.
`scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1` is inside the spec's Write Set but this
plan does not modify it, because the new function calls `Get-CoberturaPackageLineSummary` unchanged.
The D7 throwaway helper is created in the session scratchpad outside the worktree root, so it is not
part of this inventory, which is why the production count remains 3.

### D2 — The three functions and their contracts

All three live in `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1`.

1. `Get-CoberturaFirstPartyCoverageSummary -XmlDocument [xml] [-ProjectNames [string[]]]` — the
   aggregation. `ProjectNames` defaults to `(Get-KoverageProjectAllowlist)`. It selects
   `/coverage/packages/package`, skips any package whose `name` is not in `ProjectNames`, and
   accumulates `Get-CoberturaPackageLineSummary` over each retained package. It returns a
   `pscustomobject` carrying `LineRate`, `BranchRate`, `LinesCovered`, `LinesValid`,
   `BranchesCovered`, `BranchesValid` as strings, using the identical rounding and the identical `'0'`
   zero-denominator fallback as `Get-CoberturaCoverageSummary` and `Get-CoberturaPackageLineSummary`,
   plus `LinePercent` and `BranchPercent` as two-decimal strings. It throws
   `Cobertura XML does not contain a <packages> node.` when no `<packages>` node is present, matching
   `Get-CoberturaCoverageSummary` at `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` line 116
   exactly. It is pure: no I/O, no mutation of the input document.
2. `Format-CoberturaFirstPartyCoverageSummary -Summary [pscustomobject]` — the pure formatter AC8
   requires. It takes the aggregation result and returns a single string. It performs no I/O.
3. `Get-CoberturaFirstPartyCoverageReport -CoberturaXml [string] [-ProjectNames [string[]]]` — casts
   the string to `[xml]`, calls the aggregation, calls the formatter, returns the string. This
   composition function exists so that the change to `Invoke-MSTestWithCoverageMain` is exactly one
   line; see D3.

**The rendered text.** `Format-CoberturaFirstPartyCoverageSummary` returns one line of the shape
`First-party coverage: lines <covered>/<valid> (<pct>%), branches <covered>/<valid> (<pct>%)`. For the
Phase 1 fixture the exact string is, verbatim:

`First-party coverage: lines 3/4 (75.00%), branches 4/8 (50.00%)`

That string carries all four counts and both derived percentages, which is what AC8 requires, and it
is asserted by a named Pester test rather than by a text search.

### D3 — Why the entry-point change is exactly one line

`Invoke-MSTestWithCoverageMain` begins at `scripts/vscode/Invoke-MSTestWithCoverage.ps1` line 248 and
returns early at line 324 when `-NoExecute` is supplied. The existing tests exercise it only through
that early return, so no unit test can reach the post-processing block at lines 339-345. Any line
added there is therefore an uncovered line by construction. Confining the wiring to one line, with
every computation and every string it emits living in unit-tested pure functions, is what keeps the
uncovered surface at exactly one line. The AC10 gate below pins that: the `missed` LINE counter for
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` may rise by at most 1 against the Phase 0 baseline.
The single uncovered wiring line is recorded as a measured finding in evidence, per Non-Goal 4 and
AC12, and no threshold is lowered to accommodate it.

The added line is placed immediately before the existing line 345, so the existing
`Done. Coverage artifact: $resolvedOutputPath` output line is retained and the file goes from 350 to
351 physical lines.

### D4 — Why AC6 is not an `[expect-fail]` task, and where the real fail-before evidence comes from

AC6's stated mechanism is a **differential assertion inside a passing test**: a private test-scoped
helper reproduces the descendant-axis selection, is run over the identical fixture, and the test
asserts that the helper's `LinesValid` and `BranchesValid` are strictly greater than the values the
new function returns. That test is expected to **pass** once the fix lands, and it would remain a
passing test forever. Tagging it `[expect-fail]` would be wrong: the atomic-plan contract reserves
that tag for a regression task expected to fail before the fix, and this task has no such state.

The plan nonetheless supplies genuine fail-before evidence, because `CLAUDE.md`'s Bugfix Workflow
requires a failing regression test first. Phase 1 authors the whole test file **before** any
production code exists, and P1-T2 runs it and records the red result. At that point
`Get-CoberturaFirstPartyCoverageSummary` does not resolve, so every `It` that calls it fails with a
`CommandNotFoundException`. P1-T2 carries the `[expect-fail]` tag and its own artifact. The test file
dot-sources `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` alone and never dot-sources the
new file directly, which is both what AC1 requires and what keeps the Phase 1 failure a set of
`It`-level failures rather than a container-level error.

Ordering consequence, checked deliberately: between P1-T2 and P3-T1 the folder
`tests/scripts/vscode` contains failing tests, so **no whole-folder test gate is scheduled in that
window**. The next `mcp__drm-copilot__run_poshqc_test` over `tests/scripts/vscode` is P5-T6, after
Phase 2 has landed the implementation.

### D5 — Why the AC3 search is authored as a controlled comparison

AC3 asks for a case-sensitive search for the literal `.//line` over `scripts/vscode/` returning zero
matches. Re-derived against the current tree in this authoring pass, that search **already returns
zero before any change is made**, so a bare zero-match assertion passes vacuously whatever the
executor does, and would also pass if the search itself were broken by wrong quoting, a wrong path, or
a regex interpretation of the leading dot.

The gate is therefore authored as a single command over both folders at once, with three
discriminating properties:

1. **Positive control.** After Phase 1 the literal `.//line` is present exactly once in the tree
   under these two folders, inside the AC6 differential helper in
   `tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1`. The gate requires that hit
   to appear. If the search mechanism is broken, the hit is missing and the gate fails.
2. **Negative assertion over the same run.** In the same output, no line may name a path under
   `scripts/vscode/`. Because the positive control proves the search works, the absence under
   `scripts/vscode/` is a real observation rather than an artifact of a broken query.
3. **Case-sensitivity control.** A companion run of the same command with the pattern `.//LINE`
   must produce no output, while the `-i` variant of that same run must produce the same single
   line as (1). That pair is what demonstrates the search is case-sensitive.

**Quoting.** The gate uses `git grep -F`, not `Select-String`. In a PowerShell regex the leading `.`
of `.//line` is a metacharacter matching any character, `\|` is a literal pipe rather than an
alternation, and a backslash or double quote inside a pattern that must survive shell quoting has to
be spelled `\x5C` and `\x22`. `git grep -F` takes a fixed string, is case-sensitive by default, needs
no PowerShell quoting at all, and is inside the Bash allowlist under `git *`.

**Literals this plan instructs the executor to create.** The following tokens do not yet exist in the
tracked tree and are quoted here verbatim so that a later search assertion against them is a real
instruction rather than an unfalsifiable claim: `Get-CoberturaFirstPartyCoverageSummary`,
`Format-CoberturaFirstPartyCoverageSummary`, `Get-CoberturaFirstPartyCoverageReport`,
`pair is counted exactly once`.

### D6 — The AC13 permitted-prefix set, and two stated deviations from its literal wording

AC13 states that the anchored name-only diff lists only paths under `scripts/vscode/`,
`tests/scripts/vscode/`, and this feature's folder. This plan implements AC13 with a permitted-prefix
set of five entries and records two deviations:

- **Deviation 1 — `.claude/agent-memory/`.** That directory is tracked and agents write to it during
  a run, including this feature's executor. A three-prefix gate is not satisfiable. The prefix is
  admitted as a rule, not as a file list, and it is not a code or governance surface.
- **Deviation 2 — `docs/features/potential/`.** AC14 requires the `CLAUDE.md` CUT3 wording mismatch
  to be raised as a separate promotion, and this repository's promotion lifecycle is file-based, so
  the promotion route may create a record under that path. If the route creates no local file, the
  prefix simply does not appear and the gate is unaffected.

The gate remains falsifiable in both directions: any path outside the five prefixes fails it, and the
task additionally asserts, one by one, that `CLAUDE.md`, any path under `.claude/skills/`, any path
under `.claude/rules/`, `.editorconfig`, `BannedSymbols.txt`, any `.cs` file, and the two historical
documents carrying the descendant-axis snippet appear **zero** times in the listing. That second half
is a prohibition-shaped assertion and is immune to the agent-memory noise.

### D7 — One authorized throwaway helper, placed in the session scratchpad

P3-T3 (AC7) must run two aggregations over a 671136-line committed Cobertura document. Expressing
that as a single `pwsh -NoProfile -Command` payload is unreadable and quoting-fragile. This plan
authorizes exactly **one** throwaway helper script, named `aggregate-compare-815.ps1` and created in
the agent session scratchpad directory.

The scratchpad is the correct location on its own terms. The helper is a transient measurement aid
that exists only for the duration of one task, produces no deliverable, and is deleted before the
commit; the repository environment designates the session scratchpad for exactly this class of
temporary file, and `.claude/rules/general-code-change.md` exempts a throwaway script created and
deleted within an agent session from the 500-line rule. Keeping it out of the tracked tree also keeps
it out of the production file inventory that `.claude/hooks/enforce-powershell-batch-budget.ps1`
maintains, which is consistent with the file not being production code: that hook classifies any
`.ps1` inside the worktree root that is neither under `tests/` nor named `*.Tests.ps1` as a
production file, whatever its `.gitignore` status, and this plan's three production slots are already
committed to `Invoke-MSTestWithCoverage.FirstParty.ps1`, `Invoke-MSTestWithCoverage.Helpers.ps1` and
`Invoke-MSTestWithCoverage.ps1`.

The helper accepts the repository root and the Cobertura document path as explicit parameters and
resolves every repository file through them, so it does not depend on the working directory the
process starts in. **No artifact written by this plan may record the helper's absolute path.** The
artifact records the helper's file name and the repository-relative document path only. Keeping
absolute host paths out of committed artifacts is a convention this plan adopts rather than a rule
codified under `.claude/rules/`; the requirement stands on its own terms, because an absolute path
here would record the operator's account name and machine name. P4-T1 deletes the
helper. The helper is kept short. No second helper is authorized.

### D8 — The fixture, and the exact counts both computations produce over it

The fixture is a here-string XML literal held in the test file. It creates **no file on disk**, per
`.claude/rules/general-unit-test.md`, which prohibits temporary files in tests outright, and it
matches the established idiom of every existing assertion in
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` and
`tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1`.

The fixture's root element is `<coverage>` carrying a single `<packages>` child, matching
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` lines 213-229. That root is
load-bearing rather than cosmetic: both the new function and the pinned differential snippet select
on the absolute path `/coverage/packages/package`, so a fixture rooted anywhere else returns no
package on either side and the differential assertion compares zero against zero.

One package `Ns`, one class `Ns.Foo`, `filename="Ns\Foo.cs"`. Every `<line>` below carries
`branch="True"` and a `condition-coverage` attribute.

- Class-level `<lines>` rollup, three rows: line 20 `hits="1"` `condition-coverage="100% (2/2)"`;
  line 30 `hits="0"` `condition-coverage="0% (0/2)"`; line 40 `hits="1"`
  `condition-coverage="50% (1/2)"`.
- `<methods>`, five rows across four methods: `.ctor ()`, `.ctor (int)` and `.ctor (string)` each
  carry line 20 `hits="1"` `condition-coverage="100% (2/2)"`; `M ()` carries line 30 `hits="0"`
  `condition-coverage="0% (0/2)"` and line 50 `hits="1"` `condition-coverage="50% (1/2)"`.

Eight `<line>` elements in total. The shape is the field-initializer-across-constructors case
confirmed at issue #670. **Line 40 appears only in the class-level view and line 50 appears only in
the method-level view**, which is what AC5 requires and what the research document's proposed fixture
did not supply; **line 20 appears four times and line 30 twice**, which is the differing-multiplicity
requirement.

Hand-derived counts, stated here so the plan fixes the evidence rather than leaving the executor to
select it:

| Quantity | Descendant-axis `.//line` (the defect) | De-duplicated (the new function) |
|---|---|---|
| LinesValid | 8 | 4 |
| LinesCovered | 6 | 3 |
| BranchesValid | 16 | 8 |
| BranchesCovered | 10 | 4 |
| LineRate | `0.75` | `0.75` |
| BranchRate | `0.625` | `0.5` |

Derivation of the de-duplicated column, by hand evaluation of `Get-CoberturaClassLineSummary` at
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` lines 190-247: the union keyed by line number
is `{20, 30, 40, 50}`; line 20 has maximum hits 1 and is covered, line 30 has hits 0, lines 40 and 50
have hits 1; every retained entry is a branch, and the retained `condition-coverage` denominators are
2, 2, 2 and 2, giving BranchesValid 8 and BranchesCovered 2 + 0 + 1 + 1 = 4.

Derivation of the descendant-axis column, by hand evaluation of the pinned snippet at
`docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/plan.2026-09-07T20-14.md`
lines 101-117: all eight rows increment `$lv`; six rows carry `hits` greater than zero (class rows 20
and 40, the three constructor copies of line 20, and method row 50), giving `$lc` 6; all eight rows
carry `condition-coverage`, summing denominators 2+2+2+2+2+2+2+2 = 16 and numerators
2+0+1+2+2+2+0+1 = 10.

**Note the line rate is identical under both computations at `0.75`, while the counts differ by a
factor of two.** That is the property Repro & Evidence records and it is exactly why AC5 forbids a
rate assertion: a test written against `LineRate` would pass over this fixture whether or not the fix
is correct. The tests assert counts.

**These figures are hand-derived and are not yet confirmed by execution.** The research document
flags the same limitation for its own smaller fixture. P3-T2 records the executed figures, and if any
executed figure differs from the table above the executor stops, records the discrepancy as a finding
in `evidence/regression-testing/p3-t2-differential-counts.md`, and reports it rather than editing the
assertion to match the observed value.

### D9 — The named tests this plan requires

All in `tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1`, `Describe` block
`Get-CoberturaFirstPartyCoverageSummary` unless stated otherwise.

**Every test that invokes a function in this file supplies `-ProjectNames` explicitly.** T-A, T-B,
T-D, T-E, T-F and T-G all pass `-ProjectNames @('Ns')`, and the private differential helper is called
with the same one-element allowlist, so the two computations in T-A are compared over an identical
retained package set. T-C is the only test that reads the parameter's default, and it reads it from
the function AST rather than by invoking the function. The reason is mechanical: the default value
`(Get-KoverageProjectAllowlist)` is evaluated at parameter binding and derives its names from the
repository's project files, so it cannot contain the fixture package name `Ns`. A test that omitted
the parameter would retain no package, return zero counts, and additionally perform a recursive
repository scan, which `.claude/rules/general-unit-test.md` forbids in a unit test. The idiom matches
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` line 232.

- **T-A** `counts a line repeated across constructor rows once, in both the line and the branch totals`
  — AC5 and AC6. Asserts `LinesValid` is `'4'`, `LinesCovered` is `'3'`, `BranchesValid` is `'8'`,
  `BranchesCovered` is `'4'`, and additionally asserts the differential: the private helper's
  `LinesValid` is `8` and is strictly greater than `4`, and its `BranchesValid` is `16` and is
  strictly greater than `8`. Asserts no rate or percentage.
- **T-B** `excludes a package outside the supplied first-party allowlist from both totals` — AC4.
  Fixture adds `<package name="Ns.Test">` carrying three covered non-branch lines. Called with
  `-ProjectNames @('Ns')`. Asserts `LinesValid` is still `'4'` and `LinesCovered` is still `'3'`.
- **T-C** `defaults the ProjectNames parameter to Get-KoverageProjectAllowlist` — AC4. Reads the
  parameter's default-value extent text from the function AST and asserts it is
  `(Get-KoverageProjectAllowlist)`. This assertion performs no filesystem scan, so the test stays
  fast and deterministic.
- **T-D** `throws when the document carries no packages node` — negative scenario. Asserts the thrown
  message is `Cobertura XML does not contain a <packages> node.`
- **T-E** `returns zero counts and a zero rate when a retained package carries no classes` —
  boundary. Asserts `LinesValid` is `'0'`, `LineRate` is `'0'` and `BranchRate` is `'0'`, mirroring
  `tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1` lines 48-69.
- **T-F**, in `Describe` block `Format-CoberturaFirstPartyCoverageSummary`,
  `renders the four counts and both percentages on one line` — AC8. Asserts the returned string is
  exactly `First-party coverage: lines 3/4 (75.00%), branches 4/8 (50.00%)` for the T-A fixture's
  summary, without invoking a coverage run.
- **T-G**, in `Describe` block `Get-CoberturaFirstPartyCoverageReport`,
  `renders the report line from a Cobertura string without touching the filesystem` — AC1 and AC8.
  Passes the T-A fixture here-string and `-ProjectNames @('Ns')`, and asserts the returned string is
  exactly `First-party coverage: lines 3/4 (75.00%), branches 4/8 (50.00%)`. This test exists because
  the composition function is the only production path the entry-point wiring calls, and P5-T8 gates
  the whole new file at the 90 percent floor; leaving the composition function untested would put
  that floor at risk for a function that is pure and trivially callable.

The private differential helper reproduces the pinned snippet's descendant-axis selection and its
`condition-coverage` accumulation, with **one deliberate change**: the hard-coded nine-name
`$firstParty` literal is replaced by a parameter. The reason is stated in-code and here: the pinned
literal does not contain `Ns`, so retaining it would make the helper return all zeros over this
fixture and the differential assertion would compare against zeros, which would demonstrate nothing.
Replacing the literal with a parameter also keeps the nine production assembly names out of the new
test file. That is a design property of the test file rather than a precondition of the P4-T3 gate:
after the correction recorded in P4-T3, that gate searches `scripts/vscode/` and the pinned
2026-09-07 plan document only, matching AC4's stated scope, and it does not search
`tests/scripts/vscode/`.

---

### Phase 0 — Policy Reads and Baseline Capture

- [x] [P0-T1] Read, in this exact order, `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/powershell.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/plan-acceptance-gates.md`, then `docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/issue.md`, then that folder's `spec.md` in full, then `research/research.2026-09-08T23-50.md`. Write `evidence/baseline/phase0-instructions-read.md` carrying `Timestamp:`, `Policy Order:` and the explicit list of the nine files read. Acceptance: the artifact exists and lists all nine paths in that order.
- [x] [P0-T2] Record the branch baseline. Run `git rev-parse --abbrev-ref HEAD` and `git merge-base HEAD epic/review-residuals-2026-09-08-integration` and `git status --porcelain --untracked-files=all -- scripts/vscode tests/scripts/vscode`. Write `evidence/baseline/p0-t2-branch-baseline.md`. Acceptance: the merge-base command exits 0 and prints a 40-character object name, and the scoped porcelain command prints nothing.
- [x] [P0-T3] Capture pre-change physical line counts for every `.ps1` file under `scripts/vscode` and `tests/scripts/vscode` with `pwsh -NoProfile -Command 'foreach ($f in @(Get-ChildItem -Recurse -File -Filter "*.ps1" -Path "scripts/vscode","tests/scripts/vscode")) { Write-Output ($f.FullName + "=" + (Get-Content -LiteralPath $f.FullName).Count) }'`. Write `evidence/baseline/p0-t3-file-line-counts.md`. Acceptance: the artifact records 22 entries, among them `Invoke-MSTestWithCoverage.Helpers.ps1` at 469, `Invoke-MSTestWithCoverage.ps1` at 350, `Invoke-MSTestWithCoverage.Helpers.Tests.ps1` at 494, `Invoke-MSTest.RunSettings.Tests.ps1` at 496 and `Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` at 486, and no entry exceeds 500.
- [x] [P0-T4] Run `mcp__drm-copilot__run_poshqc_format` with `scan_folders` set to the two-element list `scripts/vscode` and `tests/scripts/vscode`, then observe the tree with `git status --porcelain --untracked-files=all -- scripts/vscode tests/scripts/vscode`. Write `evidence/baseline/p0-t4-format-baseline.md`. Acceptance: the tool returns ok AND the scoped porcelain command prints nothing, establishing that the two folders carried no pre-existing formatter drift. The tree observation is the discriminating signal; the ok flag alone is not, because the tool rewrites files and still succeeds.
- [x] [P0-T5] Run `mcp__drm-copilot__run_poshqc_analyze` with `scan_folders` set to the single element `scripts/vscode`. Write `evidence/baseline/p0-t5-analyze-scripts-baseline.md` carrying `ExpectedExitCode: 1`. Acceptance: the artifact records the exact integer N from the tool's `PSScriptAnalyzer reported N issue(s)` sentence. The expected value is 16, measured on this branch head by the orchestrator and independently recorded on 2026-08-04 by the issue 400 delivery; if the observed value differs, record the observed value as the baseline and note the discrepancy, because P5-T4 compares against the recorded baseline rather than against the literal 16. These findings are pre-existing and out of scope; no later task in this plan demands zero findings for this folder.
- [x] [P0-T6] Run `mcp__drm-copilot__run_poshqc_analyze` with `scan_folders` set to the single element `tests/scripts/vscode`. Write `evidence/baseline/p0-t6-analyze-tests-baseline.md`. Acceptance: the tool returns ok and exits 0. The ok flag is the asserted signal, because the tool was observed to exit non-zero whenever its finding count is greater than zero (see P0-T5), so an ok return over this folder is itself the zero-finding observation. If the tool additionally prints a `PSScriptAnalyzer reported N issue(s)` sentence on this run, record N; do not treat the absence of that sentence as a failure, because a zero-finding run is not required to print it. A zero demand is satisfiable for this folder and is asserted again after the change.
- [x] [P0-T7] Run `mcp__drm-copilot__run_poshqc_test` with `scan_folders` set to the single element `tests/scripts/vscode`, then read the totals with `pwsh -NoProfile -Command '[xml]$j = Get-Content -Raw -LiteralPath "artifacts/pester/pester-junit.xml"; $r = $j.DocumentElement; if (-not $r.HasAttribute("tests")) { throw "junit root carries no tests attribute" }; Write-Output ("TESTS=" + $r.GetAttribute("tests") + " ERRORS=" + $r.GetAttribute("errors") + " FAILURES=" + $r.GetAttribute("failures"))'`. Write `evidence/baseline/p0-t7-test-baseline.md`. Acceptance: the tool returns ok, the recorded line carries `ERRORS=0` and `FAILURES=0`, and the exact `TESTS` integer is recorded. The expected value is 96, measured on this branch head by the orchestrator, who also established that omitting `scan_folders` produced the identical 96 and therefore that `tests/scripts/vscode` is the whole tracked Pester suite; if the observed value differs, record the observed value as the baseline, because P5-T6 compares against the recorded baseline. This baseline must be captured before Phase 1 adds any test, because AC10 compares the post-change total against it.
- [x] [P0-T8] Capture the pre-change changed-file coverage baseline with a direct Pester run: `pwsh -NoProfile -Command '$c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode"; $c.Run.PassThru = $true; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = "scripts/vscode"; $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t8-coverage-baseline.jacoco.xml"; $r = Invoke-Pester -Configuration $c; Write-Output ("PASSED=" + $r.PassedCount + " FAILED=" + $r.FailedCount)'`. Then read the per-file LINE counters with `pwsh -NoProfile -Command '[xml]$j = Get-Content -Raw -LiteralPath "docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t8-coverage-baseline.jacoco.xml"; foreach ($n in @($j.SelectNodes("//sourcefile"))) { foreach ($c in @($n.SelectNodes("./counter"))) { if ($c.type -eq "LINE") { Write-Output ($n.name + " covered=" + $c.covered + " missed=" + $c.missed) } } }'`. Write `evidence/baseline/p0-t8-coverage-baseline.md` recording, in `Output Summary:`, the numeric `covered` and `missed` LINE values for `Invoke-MSTestWithCoverage.Helpers.ps1` and for `Invoke-MSTestWithCoverage.ps1`, and the overall folder line percentage. Acceptance: `FAILED=0`, the JaCoCo file exists, and both named files appear in the per-file listing with numeric values.
- [x] [P0-T9] Record why the bundled PowerShell coverage artifact is non-probative for this feature. Run `pwsh -NoProfile -Command 'Test-Path -LiteralPath "artifacts/pester/powershell-coverage.xml"'`. This plan authorizes exactly two branches and no third. **Branch A, the file exists:** record its report-level LINE `missed` and `covered` totals and the `name` attribute of every `<package>` element; acceptance is that no recorded package name names any path under the scripts tree, so the artifact measures no file this feature changes. **Branch B, the file does not exist:** record the negative claim auditably with `SearchScope: artifacts/pester/`, `SearchPatterns: powershell-coverage.xml`, `SearchResult: none`; acceptance is that the three fields are present. Write `evidence/baseline/p0-t9-bundled-coverage-nonprobative.md` recording which branch applied and the branch's values. Either branch establishes the same conclusion, which is what AC10 requires recorded: the bundled artifact cannot supply changed-file coverage for this feature, so AC10 is measured by the direct Pester run of P0-T8 and P5-T7 instead. Note for the executor: this file was verified absent from this worktree while the plan was authored, and the orchestrator's report of LINE missed 6403 and covered 0 across nine `.claude` and `.codex` packages was measured in a different worktree, so Branch B is the likely outcome and is not a failure.
- [x] [P0-T10] Capture the pre-change descendant-axis baseline and its controls. Run `git grep -c -F -e './/line' -- scripts/vscode tests/scripts/vscode`, then `git grep -c -i -F -e './/line' -- scripts/vscode tests/scripts/vscode`, then `git grep -c -F -e 'VBFunctions' -- scripts/vscode docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/plan.2026-09-07T20-14.md`. Write `evidence/baseline/p0-t10-descendant-axis-baseline.md` carrying `ExpectedExitCode: 1` for the first two commands. Acceptance: both of the first two commands print nothing and exit 1, establishing the pre-change baseline of zero under both folders; the third prints exactly one line whose path is the 2026-09-07 plan document, which is the positive control proving the search mechanism reports a hit when the literal is present. Record the exact integer count that line carries. The expected value is 3, measured on this branch head; if the observed value differs, record the observed value as the baseline and note the discrepancy, because P4-T3 compares against the recorded baseline rather than against a literal.
- [x] [P0-T11] Capture the threshold and AC7 fixture baseline. Run `pwsh -NoProfile -Command '(Get-FileHash -Algorithm SHA256 -LiteralPath "scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1").Hash; (Get-FileHash -Algorithm SHA256 -LiteralPath "docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/coverage-final.cobertura.xml").Hash'` and `git grep -c -F -e 'is below the required 80' -- scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`. Write `evidence/baseline/p0-t11-threshold-and-fixture-baseline.md`. Acceptance: both SHA-256 values are recorded as 64-character hex strings, and the `git grep` output is exactly one line with count 1, pinning the existing failure message before any change.
- [x] [P0-T12] Confirm the full-bug mode markers. Verify that `issue.md` carries the line `- Work Mode: full-bug`, that `spec.md` carries an `## Acceptance Criteria` heading, and that no `user-story.md` exists in the feature folder. Write `evidence/baseline/p0-t12-mode-markers.md`. Acceptance: all three conditions hold and the artifact records that the absence of `user-story.md` is correct by design for full-bug mode.

### Phase 1 — Regression Test Authored Ahead Of The Fix

- [x] [P1-T1] Create `tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1`. It opens with `Set-StrictMode -Version Latest`, a `BeforeAll` block that dot-sources `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and nothing else, the D8 here-string fixture, the private descendant-axis differential helper described in D9, and the seven tests T-A through T-G named in D9 with their stated assertions. The file creates no file on disk and uses only in-memory here-string XML literals. Acceptance: the file exists, contains the seven `It` names verbatim as spelled in D9, contains exactly one occurrence of the literal `.//line` inside the differential helper, and contains no occurrence of the token `UtilitiesCS`.
- [x] [P1-T2] [expect-fail] Run the new test file alone, before any production code exists, with `pwsh -NoProfile -Command '$r = Invoke-Pester -Path "tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1" -PassThru; Write-Output ("PASSED=" + $r.PassedCount + " FAILED=" + $r.FailedCount)'`. Write `evidence/regression-testing/p1-t2-fail-before.md`. Acceptance: `FAILED` is 6 or greater and the recorded failure text names `Get-CoberturaFirstPartyCoverageSummary` as an unresolved command. Record `EXIT_CODE: 0` and do NOT write an `ExpectedExitCode` field: `Invoke-Pester` returns exit code 0 on a failing `It` block unless `Run.Exit` is enabled, so a non-zero expectation here would be unsatisfiable. The red result, not the exit code, is the fail-before evidence.
- [x] [P1-T3] Record the new test file's physical line count with `pwsh -NoProfile -Command '(Get-Content -LiteralPath "tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1").Count'` and append it to `evidence/regression-testing/p1-t3-test-file-size.md`. Acceptance: the recorded count is at or below 500. If it exceeds 500, split the file per the `.claude/rules/general-code-change.md` ceiling before proceeding; do not raise the ceiling.

### Phase 2 — Implementation

- [x] [P2-T1] Create `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` opening with `Set-StrictMode -Version Latest` and defining the three functions specified in D2 with comment-based help on each. The help for `Get-CoberturaFirstPartyCoverageSummary` states the counting invariant on a **single physical line** containing the literal `pair is counted exactly once`, in a sentence of the form: each (class, source line number) pair is counted exactly once, so LinesValid, LinesCovered, BranchesValid and BranchesCovered do not depend on whether a source line also appears under a method element. The function obtains per-class figures by calling `Get-CoberturaPackageLineSummary` and re-derives no part of the de-duplication rule. Acceptance: the file exists, its physical line count is at or below 500, it contains the three function names verbatim, it contains exactly one occurrence of the literal `pair is counted exactly once`, and it contains zero occurrences of the literals `.//line` and `condition-coverage`.
- [x] [P2-T2] Add exactly one line to `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, immediately after the existing line 4, dot-sourcing the new file in the same form as the three lines above it. Acceptance: `pwsh -NoProfile -Command '(Get-Content -LiteralPath "scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1").Count'` prints exactly `470`, and `git diff --numstat epic/review-residuals-2026-09-08-integration -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` prints one row for that path whose added count is 1 and whose removed count is 0, together with `git status --porcelain --untracked-files=all -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` showing the file modified. The two-dot form is used here rather than the three-dot form because the three-dot form compares committed trees only and this task's edit is not yet committed; the committed three-dot re-verification is carried by P4-T5.
- [x] [P2-T3] Add exactly one line to `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, immediately before the existing `Write-Output "Done. Coverage artifact: $resolvedOutputPath"` line, calling `Get-CoberturaFirstPartyCoverageReport` on `$processedXmlContent` and emitting the returned string with `Write-Output`. Acceptance: `pwsh -NoProfile -Command '(Get-Content -LiteralPath "scripts/vscode/Invoke-MSTestWithCoverage.ps1").Count'` prints exactly `351`, and `git grep -c -F -e 'Done. Coverage artifact:' -- scripts/vscode/Invoke-MSTestWithCoverage.ps1` prints one line with count 1, confirming the existing output line is retained rather than replaced, and `git diff --numstat epic/review-residuals-2026-09-08-integration -- scripts/vscode/Invoke-MSTestWithCoverage.ps1` prints one row for that path whose added count is 1 and whose removed count is 0, together with `git status --porcelain --untracked-files=all -- scripts/vscode/Invoke-MSTestWithCoverage.ps1` showing the file modified. The two-dot form is used here for the same reason it is used in P2-T2: the three-dot form compares committed trees only and this task's edit is not yet committed, so the committed three-dot re-verification is carried by P4-T5. The `git grep` command needs no such treatment, because `git grep` with no commit operand reads the working-tree content of a tracked file and this file is already tracked.

### Phase 3 — Regression Green And Measured Evidence

- [x] [P3-T1] Re-run the new test file alone with `pwsh -NoProfile -Command '$r = Invoke-Pester -Path "tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1" -PassThru; Write-Output ("PASSED=" + $r.PassedCount + " FAILED=" + $r.FailedCount)'`. Write `evidence/regression-testing/p3-t1-pass-after.md`. Acceptance: the output is `PASSED=7 FAILED=0`, and the artifact cross-references `evidence/regression-testing/p1-t2-fail-before.md` so the fail-before and pass-after pair is auditable in one place.
- [x] [P3-T2] Record both computations' four counts for the fixture, which is the AC6 evidence requirement. Write `evidence/regression-testing/p3-t2-differential-counts.md` carrying, as executed values read from the test run rather than as restated plan text, the descendant-axis `LinesValid`, `LinesCovered`, `BranchesValid`, `BranchesCovered` and the de-duplicated four. Acceptance: the recorded pairs are 8 against 4, 6 against 3, 16 against 8, and 10 against 4; the artifact states that `LineRate` is `0.75` under both computations and records that this is why AC5 forbids a rate assertion. If any executed figure differs from these, record the discrepancy as a finding and stop; do not edit the test assertion to match an observed value.
- [x] [P3-T3] Run the corroborating measurement against the committed real Cobertura document. Create the single authorized throwaway helper `aggregate-compare-815.ps1` in the agent session scratchpad directory per D7. It takes a repository root and a document path as parameters, dot-sources `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, resolves one allowlist by calling `Get-KoverageProjectAllowlist` exactly once, then computes (a) the descendant-axis four counts using the pinned snippet's selection and accumulation over that resolved allowlist and (b) `Get-CoberturaFirstPartyCoverageSummary` over the same document with `-ProjectNames` bound to that same resolved allowlist, and prints both sets plus the document's SHA-256. Run it against `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/coverage-final.cobertura.xml`. Write `evidence/qa-gates/p3-t3-real-document-corroboration.md`. Acceptance: the artifact records both sets of four counts, both derived percentages, and the document's SHA-256 matching the value recorded in P0-T11; and the descendant-axis `LinesValid` is strictly greater than the de-duplicated `LinesValid`. The artifact records the resolved allowlist member names, so a third party re-running the measurement retains the identical package set on both sides. The artifact records the helper's file name and the repository-relative document path only, and records no absolute path. The artifact states explicitly that this criterion does not require re-deriving the 79.38 or 77.03 figures from issue 809, because that report is not committed.
- [x] [P3-T4] Record the entry-point reported text. Write `evidence/qa-gates/p3-t4-entry-point-report.md` recording the exact string T-F asserts, the name of the pure formatting function that produced it, and the fact that the assertion runs without invoking a coverage run. Acceptance: the artifact quotes the string `First-party coverage: lines 3/4 (75.00%), branches 4/8 (50.00%)` and names `Format-CoberturaFirstPartyCoverageSummary` as its producer and `Get-CoberturaFirstPartyCoverageReport` as the single-line entry-point call site.

### Phase 4 — Scope, Size, Threshold And Handoff Gates

- [ ] [P4-T1] Delete the scratchpad helper `aggregate-compare-815.ps1` created by P3-T3, then stage and commit the code, tests and Phase 0 through Phase 3 evidence with `git add -A -- scripts/vscode tests/scripts/vscode docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815` followed by a commit. Acceptance: `git status --porcelain --untracked-files=all -- scripts/vscode tests/scripts/vscode` prints nothing; `git status --porcelain --untracked-files=all --ignored -- coverage` prints no line naming `aggregate-compare-815.ps1`; and `pwsh -NoProfile -Command '@(Get-ChildItem -Recurse -File -Filter "aggregate-compare-815.ps1" -Path "scripts","tests","docs","coverage" -ErrorAction SilentlyContinue).Count'` prints `0`. Together these establish that no copy of the helper was left anywhere in the repository. The `--ignored` switch is required on the porcelain command because `.gitignore` line 144 ignores `coverage/*`, so a plain porcelain run over that folder prints nothing whether or not a file is present there and would gate nothing. A tracked-content search such as `git grep -F -e 'aggregate-compare-815' -- scripts tests docs` is deliberately NOT used here: this plan document is itself a file under `docs/` that names the helper in D7, in P3-T3 and in this task, and this task commits that document, so a content search for that token matches the plan's own prose and can never return no hits. The file-name search above is unaffected by that prose and is the discriminating observation. Staging is required before the following gates because `git grep` and `git diff --name-only` are blind to files that are not in the index.
- [ ] [P4-T2] Run the AC3 descendant-axis gate as the controlled comparison described in D5. Run `git grep -c -F -e './/line' -- scripts/vscode tests/scripts/vscode`, then `git grep -c -F -e './/LINE' -- scripts/vscode tests/scripts/vscode`, then `git grep -c -i -F -e './/LINE' -- scripts/vscode tests/scripts/vscode`. Write `evidence/qa-gates/p4-t2-descendant-axis-gate.md` carrying `ExpectedExitCode: 1` for the second command. Acceptance: the first command prints exactly one line, that line's path is `tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1` with count 1 and no line names any path under `scripts/vscode/`; the second command prints nothing and exits 1; the third prints the same single line as the first. Together these establish that the search finds the literal when present, that no occurrence survives under `scripts/vscode/`, and that the search is case-sensitive.
- [ ] [P4-T3] Run the AC4 hard-coded-allowlist gate. Run `git grep -c -F -e 'VBFunctions' -- scripts/vscode docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/plan.2026-09-07T20-14.md` and `git grep -c -F -e 'Get-KoverageProjectAllowlist' -- scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1`. Write `evidence/qa-gates/p4-t3-allowlist-derivation-gate.md`. Acceptance: the first command prints exactly one line, whose path is the 2026-09-07 plan document and whose count equals the count recorded in `evidence/baseline/p0-t10-descendant-axis-baseline.md`, and no line of that output names any path under `scripts/vscode/`; the second prints one line with count 1 or greater, confirming the delivered function names the derived allowlist helper. The search path deliberately excludes `tests/scripts/vscode`, because `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` carries a pre-existing occurrence of this literal in a test-data path at line 190 that predates this issue and is outside AC4's stated scope, which names `scripts/vscode/` only. That pre-existing occurrence is recorded in the artifact as an inherited baseline finding and is not modified by this feature.
- [ ] [P4-T4] Run the AC2 invariant and trace-unreachability gate. Run `git grep -c -F -e 'pair is counted exactly once' -- scripts/vscode` and `git grep -c -F -e 'condition-coverage' -- scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1 scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and `git grep -c -F -e 'Get-CoberturaPackageLineSummary' -- scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1`. Write `evidence/qa-gates/p4-t4-invariant-and-trace-gate.md`. Acceptance: the first prints exactly one line, naming `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` with count 1; the second prints exactly one line, naming `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` with count 6, which is the positive control, and no line naming the new file, proving the new code re-derives no branch parsing and so cannot reach step 3 of the Root Cause Analysis trace; the third prints one line with count 1 or greater, proving the new code reaches the de-duplicating helper by delegation rather than re-implementation.
- [ ] [P4-T5] Capture post-change physical line counts by re-running the P0-T3 command. Write `evidence/qa-gates/p4-t5-file-line-counts.md` recording, side by side with the P0-T3 values, the before and after count for every file in the Write Set. Acceptance: `Invoke-MSTestWithCoverage.Helpers.ps1` is 470 (was 469), `Invoke-MSTestWithCoverage.ps1` is 351 (was 350), `Invoke-MSTestWithCoverage.Helpers.Tests.ps1` is unchanged at 494, and every `.ps1` file under `scripts/vscode` and `tests/scripts/vscode`, including the two new files, is at or below 500. The task additionally runs `git diff --numstat epic/review-residuals-2026-09-08-integration...HEAD -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 scripts/vscode/Invoke-MSTestWithCoverage.ps1` together with `git status --porcelain --untracked-files=all -- scripts/vscode`, and records that each of the two paths shows one added line and zero removed lines against the committed merge base, which is the three-dot re-verification P2-T2 and P2-T3 defer to this task.
- [ ] [P4-T6] Run the AC12 no-threshold-lowered gate. Run `git diff --stat epic/review-residuals-2026-09-08-integration...HEAD -- scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` together with `git status --porcelain --untracked-files=all -- scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`, and `git grep -c -F -e 'is below the required 80' -- scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`, and `pwsh -NoProfile -Command '(Get-FileHash -Algorithm SHA256 -LiteralPath "scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1").Hash'`. Write `evidence/qa-gates/p4-t6-threshold-unchanged-gate.md`. Acceptance: the diff and the porcelain command both print nothing, the grep prints one line with count 1, and the SHA-256 equals the value recorded in P0-T11. The artifact also records the standing finding that this repository carries an 80 percent line floor in `CLAUDE.md` and an 85 percent line floor in `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` while the only automated script gate enforces 80; that divergence predates this issue, is recorded as a finding, and no threshold is changed by this feature.
- [ ] [P4-T7] Run the AC13 scope-boundary gate per D6. Run `git diff --name-only epic/review-residuals-2026-09-08-integration...HEAD` together with `git status --porcelain --untracked-files=all` in the same task, and record both listings in `evidence/qa-gates/p4-t7-scope-boundary.md`. Acceptance, part one: every path in the combined listing begins with one of the five permitted prefixes `scripts/vscode/`, `tests/scripts/vscode/`, `docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/`, `.claude/agent-memory/`, `docs/features/potential/`. Acceptance, part two, asserted individually and each expected to be absent: no listed path is `CLAUDE.md`; no listed path begins with `.claude/skills/`; no listed path begins with `.claude/rules/`; no listed path is `.editorconfig`; no listed path is `BannedSymbols.txt`; no listed path ends with `.cs`; no listed path is `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/plan.2026-09-07T20-14.md`; no listed path is `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t5-retained-cobertura-reaggregation.md`. The artifact records the two prefix deviations of D6 and their rationale.
- [ ] [P4-T8] Hand off the `CLAUDE.md` CUT3 step 4 wording mismatch as a separate promotion, and do not fix it here. Attempt the MCP promotion route for a new bug entry, supplying an explicit `promotion_type` and a `work_mode` value; if the MCP tool is unavailable in the executor's tool surface, do not halt: record the intended promotion text in full and mark the artifact `POSTING BLOCKED` with the reason. Write `evidence/other/p4-t8-claude-md-cut3-handoff.md`. Acceptance: the artifact states the mismatch precisely, namely that `CLAUDE.md` section CUT3 step 4 names `vstest.console.exe` with `/EnableCodeCoverage` while the route actually in use is dotnet-coverage collect wrapping vstest, cites the corroborating in-code comment at `scripts/vscode/Invoke-MSTestWithCoverage.ps1` lines 19-26, carries either the created issue number and URL or the `POSTING BLOCKED` marker with its reason, and states that `CLAUDE.md` is not modified by this branch. The companion assertion that `CLAUDE.md` is absent from the diff is carried by P4-T7.

### Phase 5 — Final QA Loop

The loop order for PowerShell is format, then analyze, then test. If any step fails or changes a
file, restart the loop from P5-T1. Type checking is NOT APPLICABLE and is recorded rather than
omitted.

- [ ] [P5-T1] Run `mcp__drm-copilot__run_poshqc_format` with `scan_folders` set to the two-element list `scripts/vscode` and `tests/scripts/vscode`. Write `evidence/qa-gates/p5-t1-format.md`. Acceptance: the tool returns ok. The exit status alone is not the acceptance signal for this write-mode tool; P5-T2 supplies the discriminating observation.
- [ ] [P5-T2] Observe the tree after the format run with `git status --porcelain --untracked-files=all -- scripts/vscode tests/scripts/vscode`. Write `evidence/qa-gates/p5-t2-format-tree-observation.md`. Acceptance: the command prints nothing, which is satisfiable because P4-T1 committed both folders, and which proves the formatter rewrote no file. If it prints any path, the formatter repaired drift: commit the repaired files and restart the loop at P5-T1.
- [ ] [P5-T3] Record that type checking does not apply. Write `evidence/qa-gates/p5-t3-typecheck-not-applicable.md` with `Command: NOT APPLICABLE` and `EXIT_CODE: NOT APPLICABLE`. Acceptance: the artifact cites `.claude/rules/powershell.md` toolchain step 3, which states that type checking is not applicable for PowerShell and that the loop proceeds to testing. This step is recorded explicitly rather than omitted so the loop's four stages are all auditable.
- [ ] [P5-T4] Run `mcp__drm-copilot__run_poshqc_analyze` with `scan_folders` set to the single element `scripts/vscode`. Write `evidence/qa-gates/p5-t4-analyze-scripts.md` carrying `ExpectedExitCode: 1`. Acceptance: the recorded integer N from the tool's `PSScriptAnalyzer reported N issue(s)` sentence is less than or equal to the baseline integer recorded in `evidence/baseline/p0-t5-analyze-scripts-baseline.md`, whose expected value is 16. Because this feature touches only three files in this folder and removes no existing code, any increase above 16 can only originate in a file this feature adds or modifies, so the equality-or-lower comparison is what establishes zero findings attributed to this feature's files. A demand for zero findings across this folder is explicitly NOT made, because the folder carries a pre-existing inherited baseline of 16.
- [ ] [P5-T5] Run `mcp__drm-copilot__run_poshqc_analyze` with `scan_folders` set to the single element `tests/scripts/vscode`. Write `evidence/qa-gates/p5-t5-analyze-tests.md`. Acceptance: the tool returns ok and exits 0, matching the P0-T6 baseline, and if it prints a `PSScriptAnalyzer reported N issue(s)` sentence then N is 0. This zero demand is satisfiable for this folder and the new test file lands in it, so it is the gate that proves the added test file introduces no analyzer finding.
- [ ] [P5-T6] Run `mcp__drm-copilot__run_poshqc_test` with `scan_folders` set to the single element `tests/scripts/vscode`, then re-run the P0-T7 JUnit reader command. Write `evidence/qa-gates/p5-t6-test.md`. Acceptance: the tool returns ok, and the recorded line shows `ERRORS=0`, `FAILURES=0`, and a `TESTS` value strictly greater than the baseline `TESTS` integer recorded in `evidence/baseline/p0-t7-test-baseline.md`, whose expected value is 96, reflecting the seven tests added by P1-T1.
- [ ] [P5-T7] Measure post-change changed-file coverage with a direct Pester run, re-running the P0-T8 command with `CodeCoverage.OutputPath` set to `docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p5-t7-coverage-final.jacoco.xml`, then re-running the P0-T8 per-file LINE counter reader against that file. Write `evidence/qa-gates/p5-t7-coverage-final.md`. Acceptance: `FAILED=0`; the JaCoCo file exists at that path; and the per-file listing carries numeric `covered` and `missed` LINE values for `Invoke-MSTestWithCoverage.FirstParty.ps1`, `Invoke-MSTestWithCoverage.Helpers.ps1` and `Invoke-MSTestWithCoverage.ps1`. The artifact records why the bundled `artifacts/pester/powershell-coverage.xml` is not used, cross-referencing `evidence/baseline/p0-t9-bundled-coverage-nonprobative.md`.
- [ ] [P5-T8] Verify the coverage thresholds and the no-regression comparison from the P5-T7 artifact with `pwsh -NoProfile -Command '[xml]$j = Get-Content -Raw -LiteralPath "docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p5-t7-coverage-final.jacoco.xml"; $sf = @(); foreach ($n in @($j.SelectNodes("//sourcefile"))) { if ($n.name -like "*Invoke-MSTestWithCoverage.FirstParty.ps1") { $sf += $n } }; if ($sf.Count -ne 1) { throw "FirstParty sourcefile node not found in JaCoCo output" }; $ln = $null; foreach ($c in @($sf[0].SelectNodes("./counter"))) { if ($c.type -eq "LINE") { $ln = $c } }; if ($null -eq $ln) { throw "FirstParty LINE counter not found" }; $cov = [int]$ln.covered; $mis = [int]$ln.missed; if (($cov + $mis) -eq 0) { throw "FirstParty LINE denominator is zero" }; Write-Output ("FIRSTPARTY_LINE_PCT=" + ((100 * $cov / ($cov + $mis)).ToString("0.00")) + " COVERED=" + $cov + " MISSED=" + $mis)'`. Write `evidence/qa-gates/p5-t8-coverage-comparison.md` recording baseline, post-change and new-code values. Acceptance, three parts: `FIRSTPARTY_LINE_PCT` is 90.00 or greater, meeting the floor `CLAUDE.md` section UT2 sets for newly added modules and functions; the `covered` LINE value for `Invoke-MSTestWithCoverage.Helpers.ps1` is greater than or equal to its P0-T8 baseline value; and the `missed` LINE value for `Invoke-MSTestWithCoverage.ps1` is no more than its P0-T8 baseline value plus 1. The artifact records the single uncovered wiring line of D3 as a measured finding with its reason, per Non-Goal 4 and AC12, and no threshold anywhere in the repository is lowered, weakened or deleted to make this gate pass.
- [ ] [P5-T9] Record the loop-completion declaration. Write `evidence/qa-gates/p5-t9-toolchain-loop.md` naming each stage of the final pass in order with its artifact path, stating whether any stage changed a file, and stating that the loop completed a full pass with no stage failing and no stage changing a file. Acceptance: the artifact names P5-T1, P5-T2, P5-T3, P5-T4, P5-T5, P5-T6, P5-T7 and P5-T8 with their paths and records a clean single pass. If any stage changed a file, this task records the restart and the loop repeats from P5-T1 before this artifact can be written.
- [ ] [P5-T10] Stage and commit the Phase 4 and Phase 5 evidence with `git add -A -- docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815` followed by a commit, then re-run `git status --porcelain --untracked-files=all` and record it in `evidence/qa-gates/p5-t10-final-tree.md`. Acceptance: the recorded listing contains no path outside the five permitted prefixes of D6, and contains no path under `scripts/vscode/`, `tests/scripts/vscode/`, or this feature's folder. Paths under `.claude/agent-memory/` may appear and are carved out by the D6 rule. The listing is captured after the commit and before this task's own artifact is written, because the artifact file and this task's own check-off both land inside this feature's folder and would otherwise appear in the listing the task asserts is empty of that folder.

### Phase 6 — Acceptance Criteria Check-Off

Each task below checks off exactly one acceptance criterion in
`docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/spec.md` by
changing that criterion's `- [ ]` to `- [x]`, and cites the evidence artifact that discharges it. Do
not batch two criteria into one task.

- [ ] [P6-T1] Check off AC1 in `spec.md`, citing `evidence/qa-gates/p4-t4-invariant-and-trace-gate.md` and `evidence/regression-testing/p3-t1-pass-after.md`. Acceptance: AC1 reads `- [x]` and the cited artifacts show the function resolves after dot-sourcing `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` alone and delegates to `Get-CoberturaPackageLineSummary`.
- [ ] [P6-T2] Check off AC2 in `spec.md`, citing `evidence/qa-gates/p4-t4-invariant-and-trace-gate.md`. Acceptance: AC2 reads `- [x]` and the cited artifact shows the invariant literal present exactly once in the new file and the branch-parsing literal absent from it.
- [ ] [P6-T3] Check off AC3 in `spec.md`, citing `evidence/qa-gates/p4-t2-descendant-axis-gate.md` and `evidence/baseline/p0-t10-descendant-axis-baseline.md`. Acceptance: AC3 reads `- [x]` and the cited artifacts carry the positive control, the negative assertion and the case-sensitivity control.
- [ ] [P6-T4] Check off AC4 in `spec.md`, citing `evidence/qa-gates/p4-t3-allowlist-derivation-gate.md` and the T-B and T-C results in `evidence/regression-testing/p3-t1-pass-after.md`. Acceptance: AC4 reads `- [x]`.
- [ ] [P6-T5] Check off AC5 in `spec.md`, citing `evidence/regression-testing/p3-t1-pass-after.md` and `evidence/regression-testing/p3-t2-differential-counts.md`. Acceptance: AC5 reads `- [x]` and the cited artifacts show the four count assertions and no rate assertion in test T-A.
- [ ] [P6-T6] Check off AC6 in `spec.md`, citing `evidence/regression-testing/p3-t2-differential-counts.md` and `evidence/regression-testing/p1-t2-fail-before.md`. Acceptance: AC6 reads `- [x]` and the cited artifacts record both computations' four counts for the fixture.
- [ ] [P6-T7] Check off AC7 in `spec.md`, citing `evidence/qa-gates/p3-t3-real-document-corroboration.md`. Acceptance: AC7 reads `- [x]` and the cited artifact carries both sets of four counts, both percentages, the document SHA-256 and the strict inequality on `LinesValid`.
- [ ] [P6-T8] Check off AC8 in `spec.md`, citing `evidence/qa-gates/p3-t4-entry-point-report.md`. Acceptance: AC8 reads `- [x]` and the cited artifact records the asserted string, its pure producer function, and the retained `Done. Coverage artifact:` line.
- [ ] [P6-T9] Check off AC9 in `spec.md`, citing `evidence/qa-gates/p5-t1-format.md`, `evidence/qa-gates/p5-t2-format-tree-observation.md`, `evidence/qa-gates/p5-t4-analyze-scripts.md` and `evidence/qa-gates/p5-t5-analyze-tests.md`. Acceptance: AC9 reads `- [x]`.
- [ ] [P6-T10] Check off AC10 in `spec.md`, citing `evidence/qa-gates/p5-t6-test.md`, `evidence/qa-gates/p5-t7-coverage-final.md`, `evidence/qa-gates/p5-t8-coverage-comparison.md` and `evidence/baseline/p0-t9-bundled-coverage-nonprobative.md`. Acceptance: AC10 reads `- [x]`.
- [ ] [P6-T11] Check off AC11 in `spec.md`, citing `evidence/baseline/p0-t3-file-line-counts.md` and `evidence/qa-gates/p4-t5-file-line-counts.md`. Acceptance: AC11 reads `- [x]` and the cited artifacts carry before-and-after counts for every Write Set file.
- [ ] [P6-T12] Check off AC12 in `spec.md`, citing `evidence/qa-gates/p4-t6-threshold-unchanged-gate.md` and `evidence/qa-gates/p5-t8-coverage-comparison.md`. Acceptance: AC12 reads `- [x]`.
- [ ] [P6-T13] Check off AC13 in `spec.md`, citing `evidence/qa-gates/p4-t7-scope-boundary.md` and `evidence/qa-gates/p5-t10-final-tree.md`. Acceptance: AC13 reads `- [x]` and the citation records the two permitted-prefix deviations of D6 with their rationale.
- [ ] [P6-T14] Check off AC14 in `spec.md`, citing `evidence/other/p4-t8-claude-md-cut3-handoff.md`. Acceptance: AC14 reads `- [x]` and the cited artifact carries either the raised issue number and URL or the `POSTING BLOCKED` marker with its reason.
- [ ] [P6-T15] Write the acceptance-criteria status summary to `evidence/issue-updates/p6-t15-ac-status.md`, listing AC1 through AC14 with PASS, PARTIAL or FAIL and the discharging artifact path for each. Acceptance: all fourteen rows are present, each names an artifact that exists on disk, and any PARTIAL or FAIL row states the specific unmet clause.
- [ ] [P6-T16] Stage and commit the Phase 6 spec check-offs and evidence with `git add -A -- docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815` followed by a commit, then re-run `git status --porcelain --untracked-files=all` and record it in `evidence/qa-gates/p6-t16-clean-tree.md`. Acceptance: the recorded listing contains no path under `scripts/vscode/`, `tests/scripts/vscode/`, or this feature's folder. Paths under `.claude/agent-memory/` may appear and are carved out by the D6 rule. The listing is captured after the commit and before this task's own artifact is written, because the artifact file and this task's own check-off both land inside this feature's folder and would otherwise appear in the listing the task asserts is empty of that folder.
