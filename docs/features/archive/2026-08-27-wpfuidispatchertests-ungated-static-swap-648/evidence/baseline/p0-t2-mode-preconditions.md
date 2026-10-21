# P0-T2 — minor-audit Preconditions on Disk

Timestamp: 2026-09-01T13-19

Command:
```
grep -c "^## Acceptance Criteria$" issue.md
grep -n "^- \[ \] AC-" issue.md
ls -1
```
(run from
`docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/`)

EXIT_CODE: 0

Output Summary:

1. Heading check. `grep -c "^## Acceptance Criteria$" issue.md` printed `1`. The exact heading line
   `## Acceptance Criteria` was found, once, in
   `docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/issue.md`.

2. AC checkbox count. Seven `AC-` checkbox items were counted beneath that heading, `AC-1` through
   `AC-7`, all in the unchecked `- [ ]` form at the following lines of `issue.md`:

   - `- [ ] AC-1` at `:123`
   - `- [ ] AC-2` at `:127`
   - `- [ ] AC-3` at `:130`
   - `- [ ] AC-4` at `:134`
   - `- [ ] AC-5` at `:137`
   - `- [ ] AC-6` at `:141`
   - `- [ ] AC-7` at `:145`

   The baseline count of lines beginning `- [ ] AC-` in that file is therefore 7. P1-T10 measures its
   decrease against this figure.

3. Optional documents absent. `ls -1` on the feature folder listed exactly four entries: `evidence/`,
   `issue.md`, `plan.2026-08-31T20-07.md`, and `research/`. Neither `spec.md` nor `user-story.md`
   exists in the feature folder, which is the state the `minor-audit` work mode requires.

All three observations passed. The persisted work-mode marker `- Work Mode: minor-audit` is at
`issue.md:12`, so `issue.md` is the sole acceptance-criteria source for this plan.
