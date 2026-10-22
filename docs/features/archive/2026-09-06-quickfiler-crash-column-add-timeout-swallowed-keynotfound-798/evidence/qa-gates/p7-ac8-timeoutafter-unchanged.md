# Phase 7 — AC8 overload-constraint audit

Timestamp: 2026-09-07T03-21
Task: [P7-T6]
Issue: #798

Host-specific absolute paths are redacted to a `<worktree>` token. All commands were executed with
the working directory set to `<worktree>`, anchored to base commit c431dc32, against HEAD
4a29d7e79112fcaf359110c16c6933d9819165fa.

## Clause 1 — the threading directory is untouched in both the committed and the working state

1. Command: git diff --name-only c431dc32 -- UtilitiesCS/Threading/
   EXIT_CODE: 0
   Output: none (zero output lines).

2. Command: git status --porcelain --untracked-files=all -- UtilitiesCS/Threading/
   EXIT_CODE: 0
   Output: none (zero output lines).

Verdict: PASS. No file under the threading directory is modified relative to the base commit, and
none carries an unstaged or untracked edit. This change adds no `TimeoutAfter` overload and alters
none of the existing ones.

## Clause 2 — exactly four `TimeoutAfter` declarations, all in the threading helper file

A repository-wide, declaration-anchored search over `*.cs` for lines matching the regular expression
`public static .*TimeoutAfter`.

The anchor is the declaration prefix rather than a trailing parenthesis. Two of the four declarations
are generic and carry a type-parameter list between the method name and the parameter list, so a
trailing-parenthesis anchor such as `TimeoutAfter(` misses them and would report two matches instead
of four.

Match count: 4

| # | Location (repository-relative) | Line | Declaration |
|---|---|---|---|
| 1 | `UtilitiesCS/Threading/TimeOutTask.cs` | 824 | `public static Task<TResult> TimeoutAfter<TResult>(` |
| 2 | `UtilitiesCS/Threading/TimeOutTask.cs` | 862 | `public static Task<TResult> TimeoutAfter<TResult>(` |
| 3 | `UtilitiesCS/Threading/TimeOutTask.cs` | 924 | `public static Task TimeoutAfter(this Task task, int millisecondsTimeout, int repeatAttempts)` |
| 4 | `UtilitiesCS/Threading/TimeOutTask.cs` | 949 | `public static Task TimeoutAfter(` |

All four matches sit in the single threading helper file `UtilitiesCS/Threading/TimeOutTask.cs`.
Matches 1 and 2 are the generic `Task<TResult> TimeoutAfter<TResult>(` declarations the acceptance
condition names. No `TimeoutAfter` declaration exists anywhere else in the repository, so this
change introduced none.

Verdict: PASS.

## Clause 3 — no banned wall-clock API in the changed production surface

The four modified production files and the new production partial were searched for the literals
`Task.Delay` and `Thread.Sleep`. `git grep` exits 1 when a search produces no match, so an exit code
of 1 with no output is the zero-match result this clause requires.

Files searched: `UtilitiesCS/Extensions/DfDeedle.cs`, `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs`,
`QuickFiler/Controllers/QfcDatamodel.cs`, `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs`,
`TaskMaster/Ribbon/RibbonViewer.cs`.

1. Command: git grep -n -F "Task.Delay" -- <the five paths above>
   EXIT_CODE: 1
   Output: none. Match count: 0.

2. Command: git grep -n -F "Thread.Sleep" -- <the five paths above>
   EXIT_CODE: 1
   Output: none. Match count: 0.

Verdict: PASS. Neither banned literal appears in the changed production surface. The timeout
behaviour is expressed through the existing `TimeoutAfter` helper rather than through a wall-clock
wait.

## Conclusion

All three AC8 clauses hold. The `TimeoutAfter` overload set is unchanged and remains at exactly four
public static declarations, all owned by `UtilitiesCS/Threading/TimeOutTask.cs`, and the changed
production surface introduces no wall-clock wait.

Output Summary: AC8 PASS on all three clauses. `git diff --name-only c431dc32` and
`git status --porcelain --untracked-files=all` both produce zero output lines for
`UtilitiesCS/Threading/`, so the threading helpers are untouched in the committed and the working
state. The declaration-anchored repository-wide search for `public static .*TimeoutAfter` over `*.cs`
returns exactly 4 matches, all in `UtilitiesCS/Threading/TimeOutTask.cs` at lines 824, 862, 924 and
949; lines 824 and 862 are the generic `Task<TResult> TimeoutAfter<TResult>(` declarations that a
trailing-parenthesis anchor would miss. Searches for `Task.Delay` and `Thread.Sleep` across the four
modified production files and the new production partial each return 0 matches.
