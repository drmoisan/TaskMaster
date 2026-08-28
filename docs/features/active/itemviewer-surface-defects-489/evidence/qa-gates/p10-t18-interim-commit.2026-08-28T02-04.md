# P10-T18 — Interim commit of every source, project, test and evidence change

Timestamp: 2026-08-28T02-04
Command: git add docs/features/active/itemviewer-surface-defects-489/ ; git commit -F <message file> ; git status --porcelain -- QuickFiler/ QuickFiler.Test/
EXIT_CODE: 0

## Resulting commit SHA

**`5935fc937e0223e8115c2f0b3510960959a9daa6`**

Message subject: `docs(489): record the Phase 10 scope-discipline and reconciliation evidence`

The message names issue **#489** — "the itemviewer-surface-defects child of the quickfiler-bug-family
epic" — and the four defect issues this feature addresses: **#486**, **#487**, **#489** and **#490**.
No closing keyword (`fixes`, `closes`, `resolves`) appears anywhere in the message, so no issue is
auto-closed by it.

## Acceptance

| P10-T18 condition | Result |
|---|---|
| `EXIT_CODE: 0` | Met — `git commit` returned `0` |
| `git status --porcelain -- QuickFiler/ QuickFiler.Test/` produces zero output lines | Met — **0** lines |

## What was committed, and where the source changes landed

Because this run follows the "commit after each phase" discipline — two earlier runs in this epic lost
unsaved work to a spend limit — the source, project and test changes were already committed by the
phase that produced them. `5935fc93` therefore carries the remaining documentation and evidence, and
the two `QuickFiler` project directories were already clean when it ran. The full set of commits
holding this feature's work up to this point:

| Commit | Contents |
|---|---|
| `e651cf0ace7c9ceda474e9a77fad7dd1c358795a` | Phase 9 — `ItemViewer.FolderSearch.cs` bare-forward `FocusSearch()`, the four XML documentation blocks on `IItemViewer.cs`, and the Phase 9 evidence including both fail-before exception dossiers |
| `a4758e86454cfd9df6d3f042e1e86fd1bf1f0896` | Phase 8 verification evidence for issue #490 |
| `dca9110f` | #490 fixes — `SetFolderItems` renamed to `AddFolderItems`, `FocusSubject` returns `bool`, dialog read-back dropped |
| `1c4b9f15`, `cef4a74e` | #490 RED tests |
| `6281d21f`, `2613d48c` | #489 fixes and RED tests |
| `e7e89412`, `aa55e9a1` | #487 fixes and RED tests |
| **`5935fc937e0223e8115c2f0b3510960959a9daa6`** | **This commit** — Phase 10 scope-discipline and reconciliation evidence, plus the four execution-discovered findings appended to `spec.md` § Out-of-Scope Findings as rows E1 through E4 |

Files in `5935fc93`: 18 new evidence artifacts (17 under `evidence/qa-gates/`, 1 under
`evidence/other/`), the modified `spec.md`, and the modified plan file carrying the Phase 9 and
Phase 10 check-offs through `[P10-T17]`.

## Format gate before committing

`dotnet tool run csharpier check .` was run immediately before the commit and returned
`EXIT_CODE: 0`, `Checked 1547 files`. No C# file is unformatted.

## Check-off ordering

`[P10-T18]`'s own checkbox is flipped **after** this commit, because a task cannot honestly be marked
complete before the work it describes has been performed. The check-off is carried by a small
follow-up commit whose only content is that one checkbox and this artifact. The acceptance condition
is unaffected: it is scoped to `QuickFiler/` and `QuickFiler.Test/`, and the plan file lives under
`docs/`, which is deliberately outside every scope-lock pathspec in this plan.

`.claude/agent-memory/` is tracked rather than gitignored and is outside every pathspec in this plan
without exception. It was clean throughout this batch and contributed nothing to either commit.

Output Summary: The interim commit succeeded with `EXIT_CODE: 0`. The resulting SHA is
**`5935fc937e0223e8115c2f0b3510960959a9daa6`**, subject
`docs(489): record the Phase 10 scope-discipline and reconciliation evidence`, whose message names
issue #489 and the four closed issues #486, #487, #489 and #490 and contains no closing keyword.
`git status --porcelain -- QuickFiler/ QuickFiler.Test/` produces **zero** output lines. The source,
project and test changes were already committed by the phases that produced them under this run's
commit-as-you-go discipline — Phase 9's is `e651cf0ace7c9ceda474e9a77fad7dd1c358795a` — so this commit
carries the 18 remaining evidence artifacts, the amended `spec.md`, and the plan check-offs.
`csharpier check .` returned `EXIT_CODE: 0` over 1547 files immediately beforehand.
