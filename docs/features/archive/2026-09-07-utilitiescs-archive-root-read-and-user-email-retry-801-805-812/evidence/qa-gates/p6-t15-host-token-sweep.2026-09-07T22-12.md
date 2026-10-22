# Phase 6 — Host-Token Sweep of the Feature Folder (P6-T15)

Timestamp: 2026-09-08T08-42

Command: a single `pwsh -NoProfile -File` run deriving the two host values at run time as `Split-Path -Leaf $env:USERPROFILE` and `$env:USERPROFILE`, then searching entry NAMES across the whole feature folder and file CONTENTS under the evidence subtree with `String.Contains`, excluding this artifact from the contents search.

EXIT_CODE: 0

Neither derived value is written into this artifact. Both are computed at run time from the environment and used only as search inputs.

Output Summary:

## Name search — whole feature folder

Scope: `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/`, recursive, including hidden entries, files and directories.

- Entries examined: 37
- **Match count for the account token in names: 0**

## Contents search — evidence subtree only

Scope: `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/evidence/`, recursive, files only.

- Files examined: 28
- **Match count for the account token: 0**
- **Match count for the full profile path: 0**
- **Match count for the user attribute name that appears in a test-results header: 0**

The contents search is scoped to the evidence subtree because the plan file and the spec discuss these token classes in prose by design, so a whole-folder contents search would report deliberate prose mentions and could never return zero.

This artifact is excluded from the contents search. That exclusion is required rather than cosmetic: D2 makes `Command:` a mandatory field of every evidence artifact, and a command searching for a literal necessarily spells that literal, so a sweep that read its own record would report a match for it on every pass after the first and the repair-and-re-run loop could never reach zero. The exclusion is by exact full path, so it holds whether or not this file exists at the time the sweep runs.

## Repair performed, and the re-run that confirms it

The first pass returned a non-zero count. One evidence file contained the test-results user attribute name, in a closing sentence explaining why the `.trx` is not committed:

- `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/evidence/qa-gates/p6-t5-vstest.2026-09-07T22-12.md`

That was a deliberate prose mention rather than a leaked host value, but the acceptance condition is a count of zero over the evidence subtree and a prose mention is indistinguishable from a leak to a literal search. The sentence was rewritten to describe the attributes rather than to name one of them, and the sweep was re-run.

| Pass | Account in names | Account in contents | Profile path in contents | User attribute name in contents |
| --- | --- | --- | --- | --- |
| First | 0 | 0 | 0 | 1 |
| Second, after repair | 0 | 0 | 0 | **0** |

All four required counts are 0 on the accepted pass. The first pass is recorded rather than discarded, so the repair is auditable and the zero is demonstrably the result of a fix rather than of a search that could not match.

The sweep runs before P6-T16, the commit that publishes these files, because a sweep placed after that commit could not stop a token from entering the branch.
