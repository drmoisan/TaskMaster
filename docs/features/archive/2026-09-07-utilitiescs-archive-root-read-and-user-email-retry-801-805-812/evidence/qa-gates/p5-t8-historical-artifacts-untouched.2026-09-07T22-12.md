# Phase 5 — Historical #797 Artifacts Untouched (P5-T8)

Timestamp: 2026-09-08T08-20

Command: `git diff --name-only origin/main...HEAD`

EXIT_CODE: 0

Command: `git diff --name-only HEAD -- docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/`

EXIT_CODE: 0

Command: `git status --porcelain --untracked-files=all -- docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/`

EXIT_CODE: 0

Output Summary:

The five historical artifacts named in spec Non-Goals items 3 and 4, which AC5 requires to stay unmodified:

1. `docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/plan.2026-09-06T22-00.md`
2. `docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/research/research-folder-settings-persistence.md`
3. `docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/issue-updates/issue-797.2026-09-06T22-00.md`
4. `docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/code-review.2026-09-07T22-40.md`
5. `docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/feature-audit.2026-09-07T22-40.md`

Finding 1 — `git diff --name-only origin/main...HEAD` lists **none** of the five. The commit made by P5-T1 touched no path under the #797 feature folder at all, so the three-dot span scoped to that directory is empty.

Finding 2 — `git diff --name-only HEAD -- <the 797 folder>` lists exactly one path, and it is not one of the five:

- `docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/spec.md`

That is the file P5-T5, P5-T6 and P5-T7 correct. Its presence is what makes this gate non-vacuous: the span is demonstrably observing a non-empty change set for this directory, so the absence of the five from that same span is a real observation rather than a reading taken over nothing.

Finding 3 — the companion `git status --porcelain --untracked-files=all` span for the same directory reports exactly one entry:

- ` M docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/spec.md`

None of the five appears as modified, and none appears as untracked. The porcelain companion is required because a name-listing diff enumerates tracked changes only and cannot report a newly created file; nothing new was created here.

Why the `HEAD` operand is required for findings 2 and 3: the P5-T5 through P5-T7 edits are uncommitted at the time this task runs, because the commit that captures them is P5-T11. An `origin/main...HEAD` span alone would therefore observe an empty change set for this directory and could not fail whatever the executor had done to `spec.md`.

Conclusion: AC5's requirement that the five dated #797 records stay unmodified is satisfied. The only #797 file this branch changes is `spec.md`, which Non-Goals items 3 and 4 do not name and which P5-T5 through P5-T7 amend with dated corrections referencing #812.
