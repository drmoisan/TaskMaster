---
name: project_602_host_identifier_sweep_plan_seams
description: "#602 repo-wide host-identifier sweep plan seams: literal-free counts must be single pwsh segments (bare assignments are non-allowlisted); never git add -A (untracked concurrent-run dir with a refused-word name); csproj URL lines defeat a generic drive-letter gate; the refused word must not appear even in prose"
metadata:
  type: project
---

Seams found authoring the #602 plan (2026-09-12), repository-wide identifier sweep, full-bug.

- **Literal-free acceptance counts must be ONE shell segment.** The Bash allowlist checks every
  chained segment, and a bare `ACCT=...;` assignment is itself non-allowlisted. Wrap the whole
  derivation and the `git grep` in `pwsh -NoProfile -Command '...'` (outer single, inner double),
  derive the account leaf with `Split-Path -Leaf $env:USERPROFILE`, and count with `@(...).Count`
  instead of `| wc -l`. Never use `$host` as a variable name (PowerShell automatic variable).
- **Never plan `git add -A`.** The executing checkout can carry an untracked concurrent-run
  documentation directory whose name contains the word the shell filter refuses, so it can neither be
  staged safely nor excluded by pathspec. Use `git add -u -- . ":(exclude)docs/features/potential"`
  plus explicit `git add` of the created paths, and `git status --porcelain -uno` for clean-tree
  clauses. Add a Phase 0 clean-tracked-tree halt so the self-anchor tag is not polluted.
- **A generic drive-letter gate (`[A-Za-z]:[\\/]`) is polluted by URLs** in `TaskMaster.csproj`
  (`http://` matches `p:/`); scope such transitions to `.vscode/settings.json` and the batch-budget
  JSON, where the count is exactly the leak.
- **Do not write the refused word anywhere in the plan**, including prose; say "concurrent run".
  One `docs/research` filename carries it; reach it only via the script's own enumeration.
- **8.3 short-name** of the account = first six chars upper-cased + `~` + digit; it appears both as
  a profile-path account segment and inside a flattened temp-dir segment, so profile-path rules need
  an alternation and rule 5 needs a sibling rule 6. Derive it as a parameter default, never a literal.
- **`.claude/state/powershell-batch-budget.default.json` is tracked-but-ignored**; the hook's
  session-id fallback is worktree-derived (`enforce-powershell-batch-budget.ps1:173,366`), so the
  hand edit is durable; plan a late re-check after every `.ps1` write.
- The 614 redaction-sweep file is real but a 100-cap Glob sorted by mtime hid it — glob the
  directory directly before declaring a named file missing.

Round-2 preflight seams (ten defects, all prose/acceptance, no task added):
- **A backticked glob in prose is a harvested Write Set path.** "No `*.cs` file is created" widened
  the footprint to every C# file; write "no C# source file" in plain prose. Globs are safe only inside
  a multi-word command span.
- **A dot-source failure message quotes the absolute script path**; an expect-fail artifact that
  records it verbatim re-creates the leak and fails the plan's own feature-folder residual gate.
  Instruct redaction up to the checkout root.
- **New-module coverage bar is 90 (CLAUDE.md UT2 line 310), not the 85/80 floor.** CLAUDE.md is the
  first authority; a new-module gate at the floor is a defect.
- **"Lists the same files" is a set criterion**: capture and compare the path list, not `.Count`.
- **Deliberate exit-1 baselines need `ExpectedExitCode: 1` stated in the task**, and baseline-relative
  MSBuild baselines need a conditional ExpectedExitCode clause, or the evidence collector renders them
  as failing rows.
- **`[expect-fail]` only on a task whose acceptance fails**; an authoring task that passes on a
  correct pass must not carry it (needs an evidence artifact it cannot have).
- **A class rooted at the whole features tree reaches out-of-scope subtrees** (promotion dir, the
  concurrent-run dir); add a first-component allowlist refusal + a unit test, and cascade every
  It-block/PASSED/enumerated-name count it changes (P1-T1, P1-T4, P6-T14).
- **Clean-tree gate vs executor memory**: the executor's tracked memory index would trip P0-T3; tell it
  to write agent-memory only at run end, staged by the closure task.
- **Name the evidence instance mechanically** ("first path in the second group, listing order"), never
  "one member of the list".

Round-3 preflight seams (two defects + one clarification, all text-local):
- **Never cite plan line numbers inside the plan's own SELF-REVIEW enumeration.** Any later insertion
  shifts them and the record then declares CITATION-TO-TREE: PASS against a state the file is not in.
  Anchor plan-internal references on task IDs (`P1-T4 acceptance clause`), and keep numeric lines only
  for citations into OTHER files.
- **The conditional-ExpectedExitCode rule reaches every task with an explicit `exit 1` branch whose
  result is read baseline-relatively downstream**, not only the MSBuild baselines. Sweep every Pester
  baseline (`if ($r.FailedCount -gt 0) { exit 1 }`) when applying a D5-class fix; round 2 missed P0-T26
  and P0-T27.
- **A "follows the convention at lines X, Y, Z" clause must name ONE prefix.** The fixture uses two
  different neutral roots (`C:\repo` at 31/60, `C:\fake` at 411/412/420); citing both sets as one
  convention invites a non-identical four-position substitution that P0-T31's INV4/INV5 then rejects.

**Why:** the prior attempt's plan used bash assignments and `git add -A`; both are unsatisfiable in
this environment.

**How to apply:** any plan whose gates derive tokens from the environment, or that stages a tree in a
checkout shared with an orchestrator session. See [[pwsh-command-quoting-in-plan-tasks]],
[[porcelain-collapses-untracked-directories]], [[powershell-batch-budget-caps-plan-authored-helpers]].
