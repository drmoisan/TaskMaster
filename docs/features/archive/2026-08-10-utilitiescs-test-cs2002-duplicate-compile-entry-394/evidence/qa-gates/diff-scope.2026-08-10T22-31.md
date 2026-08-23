Timestamp: 2026-08-10T22-31

Command: `git diff --stat` (run from repository root)

EXIT_CODE: 0

Output Summary:
```
 UtilitiesCS.Test/UtilitiesCS.Test.csproj           |  1 -
 .../plan.2026-08-10T14-09.md                       | 34 +++++++++++-----------
 .../spec.md                                        | 10 +++----
 3 files changed, 22 insertions(+), 23 deletions(-)
```

Supplementary `git status --porcelain` output confirms the same three tracked-file changes plus one
new untracked directory, all within this feature's own folder:
```
 M UtilitiesCS.Test/UtilitiesCS.Test.csproj
 M docs/features/active/2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394/plan.2026-08-10T14-09.md
 M docs/features/active/2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394/spec.md
?? docs/features/active/2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394/evidence/
```

`UtilitiesCS.Test/UtilitiesCS.Test.csproj` shows exactly `1 -` (one deletion) and no `+` insertions,
matching the plan's single-line-deletion scope lock. The `plan.*.md` and `spec.md` diffs are this
feature's own checklist/AC checkbox check-offs (`- [ ]` -> `- [x]`), not code or governance changes.
The untracked `evidence/` directory is this feature's own evidence artifacts. `packages/` (created by
the P0-T6 `nuget restore`) and `*/bin/`, `*/obj/` (created by the P0-T9/P2-T1/P2-T3 rebuilds) do not
appear in either output because they are gitignored (`git check-ignore -v packages` confirms
`packages/` is matched by `.gitignore`; `bin/`/`obj/` are matched by `.gitignore:26`/`:27`). No file
under `CLAUDE.md`, `.claude/rules/**`, or `scripts/**` appears in the diff. No other source file
appears. This satisfies the plan's scope-lock acceptance criterion.
