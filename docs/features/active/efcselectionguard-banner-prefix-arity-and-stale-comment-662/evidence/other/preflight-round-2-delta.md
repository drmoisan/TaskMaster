# Preflight Round 2 — Delta (issue 662)

- Timestamp: 2026-09-01T06-20
- Directive: `DIRECTIVE: PREFLIGHT VALIDATION ONLY`
- Reviewer: atomic-executor (validation-only pass; nothing was executed, edited, or written)
- Plan under review: `docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662/plan.2026-08-31T20-11.md`
- Tree reviewed: HEAD `fb02abef`, base `2b85134b42872e405602e6064e02dc9cda6c319b`
- Signal: `PREFLIGHT: REVISIONS REQUIRED`
- Convergence: `CONVERGENCE: FURTHER ROUNDS LIKELY`
- Coverage: all 52 tasks reached, plus every prose region.

Convergence rationale recorded by the reviewer: three items require new procedure text
(evidence sanitisation, artifact-staleness checks, a multi-node coverage comparison rule).
Round 2 showed the planner substitutes its own wording for some supplied items, and
substituted text is unreviewed until the next round. Verbatim replacement text is supplied
below for every item to reduce that exposure.

---

## Round-1 defects: all 15 confirmed closed

| ID | Status | Closing text |
|---|---|---|
| B1 | Closed | The `vswhere` resolution appears 11 times: once in the Toolchain prose and once in each of the ten affected spans (P0-T8, P0-T9, P0-T11, P0-T12, P2-T3, P2-T4, P2-T5, P2-T6, P2-T7, P2-T8). No span missed. The idiom matches the repository's own working resolution in `scripts/vscode/Invoke-MSTest.ps1:102` and `scripts/vscode/Invoke-Restore.ps1:27`. |
| B2 | Closed | P1-T1: "This task is a brief, not a delegation: the executor has no agent-invocation tool." |
| B3 | Closed | `/Settings:TaskMaster.runsettings` in exactly the four `/EnableCodeCoverage` spans; the CLI variant retained only in P2-T5 and P2-T6. Verified against both files: the repo-root file carries the Code Coverage `DataCollector` with the seven `ModulePath` exclusions, the CLI variant carries `MSTest/Parallelize` only. |
| B4 | Closed | P0-T6 and P0-T7 each carry an explicit "stop and report BLOCKED before Phase 1 begins" branch. |
| B5 | Closed (residual D3, D4) | P0-T13's `PostProcessed:` discriminator and P2-T10's differing-value BLOCKED stop. Script citations re-derived and correct: `:236` throw, `:341` threshold assertion, `:343` post-processed write. |
| B6 | Closed | P2-T10 records changed-code coverage as a number. |
| B7 | Closed | P2-T20 names six artifacts including the two post-change vstest artifacts. |
| B8 | Closed | P2-T23 scope gate over the six file-extension pathspecs with the anchored diff plus the porcelain companion. |
| B9 | Closed | P0-T18, P1-T11 and P2-T23 each place the check-off before the `git add`; the "write no further artifact" sentence is gone. |
| N1 | Closed | P0-T3 asserts the on-disk SDK path and the feature band, and explicitly rejects `dotnet --list-sdks`. |
| N2 | Closed | P2-T12 states the two independent decrements. |
| N3 | Closed | P1-T4 bounds the identifier to exactly one occurrence. |
| N4 | Closed | P1-T5 bounds the replacement to at most three comment lines. |
| N5 | Closed | No self-derived allowance figure remains; the aggregate is recorded but ungated. |
| N6 | Closed | P0-T3 and P0-T13 each carry a network-failure BLOCKED branch. |

## The five planner corrections from round 2: four accepted, one defective

1. **P2-T10 enclosing-member rule** — member identification is unambiguous and the shift
   argument is correct, but the resolution rule it substitutes is defective. See D4.
2. **P2-T23 `:(exclude)` pathspec** — correct. Residual D7 only.
3. **The three two-sentence replacements** (P0-T18, P1-T11, P1-T1) — all correct, no dangling
   fragment.
4. **P2-T9's extended span** — correct; inherits D4's wording change.
5. **Restart-loop triggerability** — correct and falsifiable; the equal-and-non-zero branch
   routes to `REMEDIATION-REQUIRED` rather than looping. Residual D9 only.

## Counts re-derived against HEAD `fb02abef` (measured, not inferred)

All 16 occurrence assertions carry either `-- '*.cs'` or a single-file pathspec; no unscoped
figure exists, so the growth of this feature's own document set moves nothing. P0-T14 gives 1/1,
P0-T15 gives 2/2, P0-T16 gives 3 with a 9-line superset cross-check, P0-T17 gives no output at
exit 1. P1-T8's post-change figure of 2 holds because the four-equals call form currently returns
exit 1 in that file. All three created literals are absent pre-change. The substring trap was
re-confirmed empirically in both directions. Every line and environment citation was re-derived
and is correct.

---

## Plan delta (apply all)

### D1 (blocking) — Committed evidence will carry the operator account, machine name and worktree root

The six vstest runs each write a TRX. `.gitignore` covers `*.coverage` and `*.coveragexml` but
not `*.trx`, so the `git add` spans stage the TRX files as produced. A TRX carries account and
machine identifiers in `runUser`, `computerName` and `runDeploymentRoot`, and the worktree root
in the `storage` attribute of every unit-test element. The two Cobertura copies carry an absolute
`filename` on every class node whenever `PostProcessed:` is `no`, which the plan treats as an
ordinary state. This is the recurrence class recorded for issues #511 and #468, and no task
budgets for it.

**Insert as a new paragraph in "Fail-Closed Evidence Rules", after the "Evidence location:" paragraph:**

**Artifact hygiene rule:** No file this plan commits may contain an absolute host path, an
account name, or a machine name. Before each of the three `git add` spans (P0-T18, P1-T11,
P2-T23), sweep every file under this feature's `evidence/` tree and substitute, case-insensitively
and in binary mode: the worktree root with a repo-root placeholder, the user-profile directory
with a user-profile placeholder, the account name with a user placeholder, and the machine name
with a host placeholder. The substitution must be case-insensitive because `vstest.console.exe`
writes the `storage` attribute of every unit-test element in lower case while the worktree root is
mixed case, so a case-sensitive pass clears the TRX header and leaves one path per test intact.
Verify with a recursive, case-insensitive, fixed-string search for the account name and for the
machine name over the feature folder; both must return no matching file. Record in the commit
task's artifact only the count of files rewritten and the token classes substituted — worktree-root
prefix, user-profile path, `computerName`, `runUser`, `storage`, Cobertura `filename`. Do not
record the verification command with the real values substituted in, and do not record any
pre-substitution value: an artifact that documents its own substitution with a before-column
reintroduces the identifiers it removes.

**Append to P0-T18, P1-T11 and P2-T23, immediately before the sentence beginning "Mark this task's own checkbox":**

Run the artifact-hygiene sweep defined in the Fail-Closed Evidence Rules over this feature's
`evidence/` tree first, and record its result in this task's artifact. The sweep runs before the
check-off and before the `git add`, because the `git add` span stages the artifacts as they stand
on disk.

### D2 (blocking) — P2-T9, P2-T17, P2-T18 and P2-T19 can pass on an artifact produced by an earlier pass

On a Phase 2 loop restart the plan states the four results directories are reused and their TRX
files overwritten, but nothing verifies that. If a run aborts before the logger writes, the
previous pass's TRX remains and the reading tasks report its counters as the final pass's. The
same exposure applies to the Cobertura document: the coverage script throws at `:236` when the
inner run exits non-zero, and in that path `dotnet-coverage` may not have written a new document,
so the post-change task can copy the document left by the baseline task and the delta task would
compare a capture against itself and report a zero delta.

**Append to each of P2-T5, P2-T6, P2-T7 and P2-T8:**

Before the command runs, delete the results directory named in this task's span if it exists,
using `[System.IO.Directory]::Delete` with the recursive flag. After the run, record the produced
TRX's `LastWriteTime` in this task's artifact and confirm it is later than the `Timestamp:`
recorded by P2-T1 in the current loop pass. Without both, a loop restart can leave the previous
pass's TRX in place, and the tasks that read it — P2-T17, P2-T18 and P2-T19 — would report the
previous pass's counters as the final pass's.

**Append to P0-T13 and to P2-T9:**

Before running the script, delete `coverage\coverage.cobertura.xml` if it exists. After the run,
record that file's `LastWriteTime` in this task's artifact and confirm it is later than the time
the script was started. The script throws at `Invoke-MSTestWithCoverage.ps1:236` when the inner
run exits non-zero, and in that path `dotnet-coverage` may not have written a new document at all;
without the deletion and the timestamp check, the copy taken here can be a document produced by an
earlier task.

### D3 (blocking) — P2-T10's class comparison is undefined in the state the plan expects

P0-T13 and P2-T9 both record a count of class nodes per filename and state there may be more than
one when `PostProcessed:` is `no`. P2-T10 then gates on "the post-change `line-rate` for each of
the two named classes", in the singular, with no rule for selecting or combining nodes, leaving
the executor free to choose the evidence it is judged against. Separately, the `NOT APPLICABLE`
clause does not say whether the gate passes, so the class half can be treated as satisfied by an
absent measurement.

**In P2-T10, replace the clause "the baseline and post-change `line-rate` for `EfcSelectionGuard.cs` and for `FolderSuggestionTree.cs`;" with:**

for each of `EfcSelectionGuard.cs` and `FolderSuggestionTree.cs`, every class node whose
`filename` ends with that name, ordered by the node's `name` attribute, with that node's
`line-rate`, listed for the baseline capture and for the post-change capture; and the changed-code
coverage as a number.

**In P2-T10, replace the final sentence "If a class node is recorded as `NOT APPLICABLE` in the baseline, record it identically here and state that a 0/0 denominator yields no comparable figure." with:**

The class half of the gate passes when the two captures carry the same set of `name` values for
each filename and, for every `name` present in both, the post-change `line-rate` is not lower than
the baseline `line-rate`. A `name` present in one capture and absent in the other is BLOCKED, not
a pass. A filename recorded as `NOT APPLICABLE` in either capture is BLOCKED, not a pass: a 0/0
denominator yields no comparable figure and the gate cannot be judged on it.

### D4 (blocking) — P2-T10's "first line of that statement" rule measures a line the change does not touch

`IsValidCreationSelection`'s `return` statement begins on the line carrying the minimum-length
comparison; the renamed call site is its second operand. Under the stated rule the changed-code
figure for that statement is read from the minimum-length comparison rather than from the call
site the task names as changed. The rule also assumes a line element exists at the resolved
number, with no instruction if it does not. P0-T13's field group (5) uses a third convention —
pre-change lines 49 and 75, where 75 is mid-statement — so baseline and post-change record
different things about the same statements.

**In P2-T10, replace from "Take for each statement the line element whose number is the first line of that statement," through "expressed as `covered/3` and as a percentage." with:**

Record each statement's full post-format line span: the line carrying the `return` keyword through
the line carrying that statement's terminating semicolon. Record all three spans. For each
statement, enumerate every line element whose `number` falls inside that span, with its `hits`
value. The statement counts as covered when at least one of those elements carries `hits` greater
than zero. The figure is the count of the three covered statements, expressed as `covered/3` and
as a percentage. If a statement's span contains no line element at all, record BLOCKED and stop
rather than counting that statement either way. The span form is required rather than a single
line number, because `IsValidCreationSelection`'s `return` statement begins on the line carrying
the minimum-length comparison and the renamed call site is its second operand, so that statement's
first line is not the line this change touches.

**In P0-T13, replace field group (5) in full with:**

(5) for each of the three executable statements this change touches — the `return` statement in
`EfcSelectionGuard.IsValidFilingSelection` that reads `StartsWith(BannerPrefix`, the `return`
statement in `EfcSelectionGuard.IsValidCreationSelection` that reads `StartsWith(BannerPrefix`,
and the `return` statement in `FolderSuggestionTree.IsBanner` that reads `StartsWith(BannerPrefix`
— that statement's pre-change line span, from the line carrying the `return` keyword through the
line carrying its terminating semicolon, and every line element whose `number` falls inside that
span with that element's `hits` value. These are the pre-change spans and are valid here because
this baseline is captured before any edit; P2-T9 and P2-T10 resolve the same three statements to
their post-format spans by the same enclosing-member identification.

**In P2-T9, replace "resolved to their post-format line numbers by the enclosing-member rule stated in P2-T10" with:**

resolved to their post-format line spans by the enclosing-member span rule stated in P2-T10,
recorded as that rule requires

### D5 (non-blocking) — The stated `pwsh` wrapping is not executable from a POSIX shell

**In the Toolchain section, replace "Each affected task runs its whole span as a single PowerShell invocation; when the executor's shell is not PowerShell, the span is passed to `pwsh -NoProfile -Command` as one argument." with:**

Each affected task runs its whole span as a single PowerShell invocation. When the executor's
shell is not PowerShell, the span must reach `pwsh` without any expansion by the outer shell. From
a POSIX shell the working form is a quoted-delimiter heredoc inside a command substitution, with
the task's whole span placed verbatim in the heredoc body. Do not place the span directly inside a
double-quoted argument: the outer shell would expand the three resolution variables to empty
strings and would fail on the `ProgramFiles(x86)` environment reference, whose colon and
parentheses are not valid POSIX parameter-expansion syntax. Single-quoting the whole argument is
also unavailable, because every span contains single-quoted path literals. The quoted heredoc
delimiter suppresses expansion inside the body, and the result of a command substitution is not
expanded again.

### D6 (non-blocking) — The prelude has no null guard on the vswhere result

The repository's own two resolutions (`Invoke-MSTest.ps1:103-104`, `Invoke-Restore.ps1:28-29`)
each throw a named error when resolution returns nothing.

**Append to the Toolchain prelude paragraph, after the code block:**

Each span adds a guard after the resolution lines it uses, throwing a named error when `$msbuild`
or `$vstest` is empty. Without it an empty resolution fails with a null command-name error that
names neither tool.

### D7 (non-blocking) — P2-T23's scope check can miss an untracked out-of-scope file

**In P2-T23, change both `git status --porcelain` invocations to `git status --porcelain -uall`, and append:**

`-uall` is required on both status spans because the default untracked-file mode collapses a new
directory to one entry, which does not match the file-extension pathspecs and would hide an
out-of-scope file created inside it.

### D8 (non-blocking) — P1-T10's widening trace cites a different test from the one it describes

**In P1-T10, replace the trace clause with:**

and two traces showing that the prohibited widening edit is test-detected: for the new test, that
widening the guard to the producers' four-character value makes `IsValidFilingSelection("===")`
and `IsValidCreationSelection("===")` return true and fails two of its four assertions; and for
the pre-existing test, citing `QuickFiler.Test/Controllers/EfcFormControllerTests.cs:462-463` and
naming `:463` as the assertion that fails under that edit while `:462` still passes.

### D9 (non-blocking) — The Phase 2 loop has no bounded escape

**Append to the Phase 2 preamble:**

The loop is bounded at three passes. If a third pass ends with the same failing task and the same
failure signature as the second, do not start a fourth: record the signature and the three passes'
artifact timestamps in
`docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662/evidence/qa-gates/loop-termination.md`,
leave AC9 unchecked, and report `REMEDIATION-REQUIRED` naming the failing task.

### D10 (non-blocking) — Tracked agent-memory changes are declared "committed separately" with no task committing them

**In P2-T23, replace the agent-memory sentence with:**

Files written under `.claude/agent-memory/` are tracked but lie outside every pathspec in this
plan. After the commit above, run `git status --porcelain -uall -- .claude/agent-memory`, record
its output in this task's artifact, and if it is non-empty run `git add .claude/agent-memory`
followed by a separate commit whose message names agent memory and issue 662. Keeping it a
separate commit is what prevents the `git add` spans in this plan from sweeping an unrelated
queued change onto this branch, and the follow-up commit is what leaves the tree clean at the end
of the run.
