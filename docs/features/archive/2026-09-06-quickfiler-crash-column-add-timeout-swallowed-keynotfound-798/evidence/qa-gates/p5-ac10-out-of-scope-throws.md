# P5-T3 — Out-of-scope `throw e;` occurrences unchanged (AC10)

Timestamp: 2026-09-07T03-03
Task: [P5-T3]
Base commit: c431dc32

This task records three observations. All three are required by the task's acceptance
condition. Repository paths below are repository-relative; no absolute host path is recorded.

---

## Observation 1 — Counted search of the QuickFiler project directory

Command: search of the QuickFiler project directory for the literal `throw e;`

EXIT_CODE: 0

Observed matches: exactly 2.

| Path | Line | Form |
|---|---|---|
| `QuickFiler/Controllers/QfcQueue.cs` | 71 | live statement, out of AC4 scope (different type) |
| `QuickFiler/Helper Classes/cInfoMail.cs` | 162 | commented out (`//                    throw e;`) |

Both are the occurrences the plan predicts. Neither is touched by this change. The two
`QfcDatamodel` partial-family files now contain zero occurrences of the literal, recorded
under P5-T1 and P5-T2.

Output Summary: 2 matches, matching the plan's predicted set exactly — one live occurrence in
the QuickFiler queue type and one commented-out occurrence in the QuickFiler mail helper class.

---

## Observation 2 — Anchored name-listing diff

Command: git diff --name-only c431dc32 -- QuickFiler/ ":(exclude)QuickFiler/Controllers/QfcDatamodel.cs" ":(exclude)QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs"

EXIT_CODE: 0

Output Summary: no output lines. No tracked file under the QuickFiler project directory,
other than the two files this phase edits, differs from the base commit.

---

## Observation 3 — Porcelain status companion (load-bearing at this position)

Command: git status --porcelain --untracked-files=all -- QuickFiler/ ":(exclude)QuickFiler/Controllers/QfcDatamodel.cs" ":(exclude)QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs"

EXIT_CODE: 0

Output Summary: no output lines. No untracked, added, modified or deleted working-tree entry
exists under that pathspec.

This companion is the load-bearing half of the pair at this point in the plan. P5-T3 runs
before the P7-T1 commit, so every change made by Phases 1 through 6 is still uncommitted
working-tree state. A newly created file under the QuickFiler project directory would be
untracked and therefore invisible to the name-listing diff in Observation 2, which enumerates
tracked changes only. Observation 3 is the observation that can see it, and it is empty.

---

## Verdict

The two out-of-scope `throw e;` occurrences are present, unchanged, and confined to the two
files the plan names. No file under the QuickFiler project directory outside the two
`QfcDatamodel` partials is created, modified or deleted by this change.
