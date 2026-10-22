# [P0-T10] Git baseline

Timestamp: 2026-08-26T08-25

Command: `git rev-parse HEAD`
Command: `git merge-base HEAD origin/epic/quickfiler-bug-family-integration`
Command: `git merge-base HEAD origin/main`
Command: `git status --porcelain`

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

Branch: `bug/qfc-collection-controller-defects-468`
HEAD commit subject: `(docs): epic-plan final documents` (2026-08-25 17:27:28 -0400)

### 1. `git rev-parse HEAD`

```
61edc19befcf6c4e95b5acd32542f2dcdab41b78
```

### 2. `git merge-base HEAD origin/epic/quickfiler-bug-family-integration`

```
61edc19befcf6c4e95b5acd32542f2dcdab41b78
```

**This is `<MERGE_BASE>` for every later diff gate in this plan.** This feature's PR base is the
epic integration branch, not `main`.

### 3. `git merge-base HEAD origin/main`

```
5be9c75903675621d654c53a8856f636d0de2869
```

Recorded for reference only. **The two merge-base values do NOT agree.** `origin/main` is behind the
epic integration branch, so a diff gate expressed against `origin/main` would additionally include
every change the epic integration branch has accumulated and would not isolate this feature's work.
Every diff gate in this plan therefore uses value 2 (`61edc19b`), not value 3.

### 4. `git status --porcelain` (verbatim)

```
 M .claude/agent-memory/orchestrator/completion-gate-receipt-shapes.md
 M docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md
?? docs/features/active/qfc-collection-controller-defects-468/evidence/
```

### Acceptance verification

- All four outputs are recorded above.
- The two merge-base values are stated as **not** in agreement, with the consequence recorded.
- **No `.cs`, `.csproj`, `.xml`, or `.sln` path appears in the porcelain output.** The three entries
  are two Markdown files and one untracked directory that contains only Markdown evidence
  artifacts.
- Per D16 the tree is non-empty by construction and this is a *type* assertion, not an
  empty-porcelain assertion. The HEAD SHA is recorded but is deliberately **not** pinned as a later
  expectation.

### Pre-existing working-tree dirt not created by this feature

`.claude/agent-memory/orchestrator/completion-gate-receipt-shapes.md` was already modified at the
moment Phase 0 began (17 insertions, 2 deletions). It is a **tracked** file under
`.claude/agent-memory/**` and it is **not** owned by this feature. Consequences carried forward:

1. It must not be included in any commit this plan makes. Every `git commit` in this plan uses an
   explicit pathspec rather than `git add -A` or `git commit -a`.
2. Any later gate expressed as an unscoped `git status --porcelain` or `git diff` would see this
   file and could fail for a reason unrelated to this feature. Later gates in this plan are already
   scoped: P1-T4 scopes to `<CTRL>`, and P1-T9 / P2-T12 / P3-T7 assert on
   `git show --name-only HEAD`, which reports only what was committed.

The second porcelain entry
(`docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md`) is this
plan's own checklist, modified by P0-T1 through P0-T5 checking their boxes. The third
(`.../evidence/`) is the Phase 0 evidence this task set is producing. Both are committed by P0-T16.
