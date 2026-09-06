---
name: csharpier-blank-line-before-comment-breaks-numstat-bounds
description: CSharpier forces a blank line between a member declaration and a following comment, so a "one comment line" insertion costs 2 lines and breaks any numstat bound written as 1 insertion
metadata:
  type: project
---

CSharpier (1.2.6, this repo) **requires a blank line between a member declaration and a comment that follows it**. Inserting one standalone comment line above a field declaration therefore costs **two** inserted lines after `csharpier format .`, not one. This holds for both `//` and `///` comment styles — verified by removing the blank line and running `dotnet tool run csharpier check <file>`, which exits 1 and prints an `Expected: Around Line N` block containing the blank line, for each style.

**Why:** it broke issue #731's `[P5-T10]` scope gate. `[P1-T1]`/`[P1-T3]` inserted exactly one comment line above an `IEmailMoveMonitor` field in `QfcCollectionController.cs` and `QfcQueue.cs`, and both files measured baseline+1 immediately afterwards, satisfying those tasks. `[P5-T1]`'s mandated repository-wide format then added the blank lines, and the anchored `git diff --numstat` came out `3 1` against a bound of "at most 2 insertions, at most 1 deletion", and `2 0` against "at most 1 insertion, 0 deletions". Both breaches were exactly one formatter-inserted blank line. The *substance* of the AC ("the diff is limited to one statement and one comment line") was met; the numeric proxy encoding it was not.

**How to apply:** when a plan bounds a file with `git diff --numstat` and the change adds a standalone comment line, budget **2 insertions per comment**, not 1. Flag this at preflight: a bound of "1 insertion" for a comment-only change is unsatisfiable in any repo that runs CSharpier over the file. The two alternative placements do not help — a trailing comment on the declaration line, and deleting a nearby blank to compensate, are each 1 insertion **plus 1 deletion**, which breaks a `0 deletions` bound instead. There is no placement costing one net insertion. Also note the ordering trap: a task that verifies `line count == baseline + 1` *before* the format step passes, and the same file then fails a post-format diff bound, so the two checks disagree without either being wrong.

Related: [[csharpier-formats-xml-print-width]], [[new-cs-files-guarantee-a-format-loop-restart]].
