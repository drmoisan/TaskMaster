---
name: project-839-createcancellationtoken-init-plan-seams
description: "#839 plan seams (QfcHomeController.Init token-source fix) - a lone dead-comment deletion between two blank lines is not CSharpier-stable (delete comment + adjacent blank, 500 -> 499); the planner-output hook forces the QA loop into the final phase with the check-offs; single-assembly coverage must bypass the runner's -SearchRoot discovery and its 80 percent document-level assert by dot-sourcing ConvertTo-DerivedCoverageSettingsXml; Glob misses on gitignored dirs are inconclusive; nested quotes inside $() are legal"
metadata:
  type: project
---

Seams re-derived while authoring the issue #839 plan in worktree `.claude/worktrees/agent-a63e372c42be95942`
at 2405a829d (2026-09-12, revision round 0).

**Why:** each one either made a caller-supplied instruction unsatisfiable as written, or would have drawn a
preflight finding.

**How to apply:** re-check before planning any QuickFiler controller fix at the 500-line ceiling, any
single-assembly coverage gate, or any plan that must pass `.claude/hooks/validate-planner-output.ps1`.

1. **"Delete exactly ONE line to stay at 500" is not CSharpier-stable when the dead line sits between two
   blank lines.** QfcHomeController.cs lines 464 and 466 are blank around the line-465 dead comment, so
   deleting 465 alone leaves a double blank that `csharpier format .` collapses; the diff then shows two
   deleted lines whatever the executor did, and a "single line removed" gate reads as violated. Plan the
   comment plus its adjacent blank line as one hand edit (file lands at 499), and reconcile the AC wording
   in a decision record ("the single NON-BLANK line removed"). Choose the later candidate (465) over the
   earlier one (41) so no line number below it moves.

2. **The planner-output hook forces QA vocabulary into the FINAL phase** (`validate-planner-output.ps1:339`
   matches `(qa|quality|toolchain|format|lint|type|test|coverage)` against the last phase's title plus task
   text). A plan whose last phase is "commit and check-off" fails unless that phase also carries the QA
   loop or QA-worded tasks. Put the toolchain loop, footprint gates, commit and per-AC check-offs in one
   final phase, loop first.

3. **Single-assembly coverage cannot go through the runner's entry point.** `-SearchRoot .` discovers every
   `*.Test.dll` (including the UtilitiesCS.Test shell-icon classes that stall vstest on this workstation),
   and `Assert-CoberturaLineCoverageThreshold` (`:344`) throws below 80 percent on whatever denominator it
   was given, so a QuickFiler.Test-only run can exit non-zero on a healthy tree. Dot-source the script
   (entry guard at `:349`), call `ConvertTo-DerivedCoverageSettingsXml` for the test-dll exclusion, and
   invoke `dotnet-coverage collect ... -- <vstest> <one dll> /Settings: /InIsolation "/logger:console;verbosity=normal" "/TestCaseFilter:TestCategory!=LiveOutlook"`
   yourself; that also lets you add the console verbosity the runner's fixed argument list omits, so one run
   yields both per-test Passed/Failed lines and the Cobertura XML. Gate per-file no-regression plus
   changed-line hits; record the repo-wide 80 percent floor as unmeasured.

4. **A Glob miss on a gitignored directory is NOT conclusive.** `.dotnet-sdk/`, `packages/` and `bin/`
   are all ignored (`.gitignore:350`, `:191`), and the Glob tool returned nothing for each even though the
   caller may have bootstrapped the worktree. Write guarded bootstrap tasks (`if (-not (Test-Path ...))`)
   whose acceptance is the post-task marker, never an unconditional install and never an assumption of
   presence.

5. **PowerShell one-liner quoting inside a bash single-quoted `pwsh -Command`:** nested double quotes
   inside `$( ... )` within a double-quoted string are legal (`"X=$($x.GetAttribute("name"))"`), so `\"`
   escapes are never needed; a literal double quote inside a plain double-quoted string uses doubling
   (`"Task ""Csc"""`). `$host` is a reserved automatic variable; name the machine-name local `$machine`.

6. **Backticked toolchain switches and regexes are harvestable blast-radius tokens.** `/t:Rebuild`,
   `/p:Nullable=enable`, `/TestCaseFilter:...`, `.*\.Test\.dll$`, `[Tt]est[Rr]esult*/` are all whitespace-free
   backticked tokens containing a slash or backslash. Write them as prose. Two adjacent code spans joined by
   a bare `/` (`` `Passed:`/`Failed:` ``) also produce a spurious `` `/` `` token; join them with words.
   Sweep with `` `[^`\s]*[\\/][^`\s]*` `` and require every hit to be a Write Set or feature-evidence path.

7. **Verified tree facts (re-derive before reuse):** QfcHomeController.cs is exactly 500 lines; `Init()`
   86-106; loaders read `this.Token` at 88 and 94 and `this._tokenSource`/`this._token` at 102-103;
   `CreateCancellationToken()` 467-471; no `#nullable`. QfcHomeControllerTests.cs is 275 lines,
   `Init_InitializesCorrectly` 112-163, `Assert.AreEqual(` x10, zero `Cleanup()` calls. Family count 6
   (Efc 62/126/162/399, Qfc 467, MetricsTests 124). `RibbonController.LoadQuickFiler()` (line 97) has zero
   callers. QuickFiler.Test has no LiveOutlook-category test. TaskMaster.cli.runsettings configures no
   logger, so no TRX is ever produced by a run that omits `/Logger:trx`.

Preflight round 1 seams (2026-09-12, applied as 22 deltas):

8. **Never assert a git hunk-header position for an insertion that lands above a blank line.** Git's change
   compaction slides the insertion group past the blank line, so an edit after line 163 is reported as
   `@@ -164,0 +165,N @@`. Assert the shape (`,0 +` in the one `@@` line) plus a numstat deleted count of 0.
9. **One `EXIT_CODE:` row per artifact.** The evidence collector treats the field as per-file; a task that
   runs several commands, some deliberately exiting 1, names ONE invocation's exit code as the row and
   records the others as named `Output Summary:` lines (`NULLABLE_GREP_EXIT=1`).
10. **A single-axis Cobertura parse (`lines/line` only) can miss a covered line.** Union `./lines/line` and
    `./methods/method/lines/line` keyed by number with max hits, as `Get-CoberturaClassLineSummary`
    (Helpers.ps1:159, axes at 194-195) does. Write the XPath axes as prose in plan decisions: a backticked
    `./lines/line` is a slash-bearing token the blast-radius sweep harvests.
11. **Do not assert a FluentAssertions subject name.** `Expected capturedSource not to be` needs PDB caller
    identification; when that fails the message is `Expected object not to be <null>.` Assert `not to be`.
12. **Agent-memory paths committed above the BASE-SHA break an "only Write Set paths" AC10 gate.** The
    orchestrator removed the commit rather than weakening the AC; the plan keeps a two-arm residue rule
    (porcelain snapshot AND anchored-diff snapshot at P0) and records the subtraction explicitly in the
    footprint artifact (`INHERITED-AND-EXCLUDED:` / `THIS-ITEM-FOOTPRINT:`). D14's halt condition must
    tolerate the same prefix the P0 acceptance tolerates, or the plan contradicts itself.

Preflight round 2 seams (2026-09-12, applied as Deltas 23-30; all single-sentence replacements, radius held at 60):

13. **A porcelain gate that runs AFTER a commit task must admit the plan file itself.** The check-off protocol
    writes `[x]` into the plan as each task passes, so the commit task's own mark lands after its commit and
    the plan file is a porcelain line on every correct run. Admit it for the porcelain span only; never
    subtract it from the anchored diff (it is a Write Set path). Same mechanism at the terminal task: its own
    `[x]` is written after the final commit, so add a pathspec'd `git add`/`git commit` of the plan file and
    declare that commit outside the artifact-bearing count.
14. **Never record a porcelain residue COUNT in an Acceptance sentence.** Agents write memory throughout the
    run; the round-1 figure (eight) was already nine at round 2. State the composition (all under
    `.claude/agent-memory/`) and say the count is deliberately unrecorded.
15. **A Phase 0 artifact sentence about the diff must be prospective** ("The planned diff will add ... is not
    expected to lower ..."); the same sentence in the post-change comparison task stays observational.
16. **Attribute a helper's documented reason to the right doc-comment block.** Helpers.ps1 `.DESCRIPTION`
    (165-172) gives the DEDUPLICATION premise (issue 441); `.PARAMETER ClassNode` (177-178) gives the
    one-view-only case. Conflating them drew an S-finding even though the command was correct.
17. **Exempt commit form for the pre-implementation gate** (helpers.ps1:33, 230-292): single bare segment,
    `git commit -m "<msg>" -- <path under docs/features/active/>`; `-m` value is consumed as a non-pathspec,
    at least one operand required, every operand under an exempt tree; only `$`, backtick, `<`, `>` are
    forbidden on the line (parentheses and `#` in the message are fine). Never shorten to a pathspec-free
    commit.
18. **Traceability Evidence column must list every artifact the check-off task reads**, not only the
    spec-fixed ones (R-1 named three rows where the check-off read an omitted artifact).

Related: [[project_797_folder_settings_persistence_plan_seams]], [[validate-planner-output-hook-line-anchored-gotchas]],
[[reference_invoke_mstest_with_coverage_script]], [[pwsh-command-quoting-in-plan-tasks]],
[[agent-worktrees-need-sdk-and-nuget-bootstrap]], [[project_823_self_anchor_diff_base_seams]].
