# P0-T8 — Diff anchor, inertness proof and baseline worktree state

Timestamp: 2026-09-13T00-48

Command: `git -C . merge-base HEAD origin/main`, then `git -C . rev-parse HEAD`, then `git -C . diff --name-only 2405a829d6afd3b12eb7c228d57158a97cb4e2ca HEAD`, then `git -C . status --porcelain --untracked-files=all -- . ":(exclude).claude/agent-memory" ":(exclude)docs/features/potential"`

EXIT_CODE: 0

BASE-SHA: 2405a829d6afd3b12eb7c228d57158a97cb4e2ca

PHASE0-HEAD-SHA: 16368e9d977da216b5a40eeb59c92c5dcd9eb452

INHERITED-PATHS:

```
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/issue.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/plan.2026-09-12T16-09.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/research/2026-09-12T15-30-gettableinviewasync-null-contract-research.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/spec.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/user-story.md
```

Every one of the five inherited paths is a file entry of the Write Set section of `spec.md` (its lines 428, 432, 431, 429 and 430 respectively). No inherited path lies outside the Write Set, so the anchor is inert for this plan's footprint gates and no halt fired. The five paths are the feature documents the preparation commit contributed between the anchor and HEAD; they carry no production source, no test source and no project file.

BASE-UNTRACKED:

```
 M docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/plan.2026-09-12T16-09.md
?? docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/ac12-amendment-confirmed.2026-09-12T16-09.md
?? docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/phase0-instructions-read.2026-09-12T16-09.md
?? docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/scratch-root.2026-09-12T16-09.md
```

Every entry lies under `docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/`, and none lies elsewhere. No halt fired.

**Tracked-or-untracked determination required by P4-T17.** The five feature documents are TRACKED at the base commit: all five appear in the merge-base-to-HEAD name-listing diff above, and none of them appears as an untracked entry in the porcelain listing. The modified entry is this plan file, which the executor has been checking off as it goes. The three untracked entries are the Phase 0 evidence artifacts written by P0-T5, P0-T6 and P0-T7 respectively; the three evidence directories themselves were created by P0-T5 and are otherwise empty at capture time.

Consequently, when P4-T17 runs its post-commit scope assertion, the listing it reads is expected to hold this plan file plus whichever evidence artifacts are written after that commit, and is NOT expected to hold spec.md, issue.md, user-story.md or the research record as untracked entries.

Output Summary: merge base resolved and recorded as `BASE-SHA`; head recorded as `PHASE0-HEAD-SHA`; the anchored name-listing diff reports five inherited paths, all Write Set feature documents, so the anchor is inert; the porcelain span reports four entries, all under the feature folder; the feature documents began tracked. All four commands exited 0 and no halt condition fired.
