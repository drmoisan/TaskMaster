---
name: untracked-file-and-linecount-gate-seams
description: "#501 R1 preflight (B1-B4): git grep misses untracked new files (need git add -N), Measure-Object -Line undercounts blank lines (436 vs 487), and two-dot BASELINE..HEAD diffs are vacuous before the plan's first commit (use single-commit form)"
metadata:
  type: project
---

Three gate-instrument defects the #501 revision-1 preflight caught; all three are plan-authoring rules, not repo facts.

1. **`git grep`/tracked-file gates on files a task creates need `git add -N <path>` first.** `git grep` and `git ls-files` see only indexed paths; a newly created `.cs` file is invisible until `git add -N` (intent-to-add, no content staged) runs. Every plan task whose acceptance greps a file the plan itself creates must instruct the `git add -N` step, and downstream tasks that grep the same file should note the dependency. A multi-path scan gate should additionally assert `git ls-files -- <path>` returns each scanned path, proving none was silently skipped.
   **Why:** #501 B1/B2 — three acceptance checks were unsatisfiable-by-construction right after file creation.
   **How to apply:** any acceptance using `git grep`, `git ls-files`, or tracked-tree scans against plan-created files.

2. **`Get-Content | Measure-Object -Line` undercounts physical lines.** `Measure-Object -Line` counts lines *within each input string*; a blank line is an empty string contributing zero. Measured: 436 vs 487 actual on a real file (51 blank lines dropped). Use `(Get-Content -LiteralPath <path>).Count` for every 500-line-cap gate, state the instrument in the artifact, and bind all line-count acceptances via one conventions bullet instead of editing each task.
   **Why:** #501 B4 — an undercounting instrument makes a near-cap file (487/500) read as comfortably under.
   **How to apply:** every "line count is at or below N" acceptance in C# plans; see also [[literal-call-clauses-block-file-size-tightening]].

3. **Two-dot `git diff BASELINE_SHA..HEAD` is vacuous before the plan's first commit.** When the only commit task is in the final phase, `HEAD == BASELINE_SHA` at every earlier gate, and the two-dot form compares two identical commits — it prints nothing whatever the working tree holds (verified: real 2-line edit, two-dot printed nothing, single-commit form printed `2 0`). Use the single-commit form `git diff --numstat BASELINE_SHA -- <path>` (commit vs working tree) for all pre-commit diff gates.
   **Why:** #501 B3 — the "must-pass test unedited" gate could never fail. Complements [[diff-gates-need-a-commit-task]] (that memory covers post-commit ranges; this one covers pre-commit gates).
   **How to apply:** any diff acceptance placed before the plan's commit task.

Also confirmed in the same cycle (B5): a plan must not offer a global-`csharpier` fallback as the recorded invocation — CLAUDE.md § C#1.1 forbids it; the remedy for a failing manifest invocation is relocating `dotnet-tools.json` to `.config/`, and P7 format/check tasks must state `dotnet tool run csharpier format .` / `check .` unconditionally (no "or the recorded equivalent" escape).
