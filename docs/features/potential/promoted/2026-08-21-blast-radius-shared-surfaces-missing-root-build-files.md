# blast-radius-shared-surfaces-missing-root-build-files (Issue #576)

- Date captured: 2026-08-21
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/blast-radius-shared-surfaces-missing-root-build-files/ (Issue #576)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #576
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/576
- Last Updated: 2026-08-21
## Summary

`config/blast-radius.json` omits TaskMaster's root-level build files from `shared_surfaces`, so two work items that both edit `coverage.config`, `Directory.Build.targets`, or `TaskMaster.sln` are reported as non-conflicting and can be scheduled into the same parallel cohort. This is the second of the two defects raised in issue #545; the first (the unfit module map) was fixed by PR #575, which removed the degenerate `docs` edge that had been masking this one.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a — the blast-radius implementation in TaskMaster is PowerShell (`.claude/lib/blast-radius/*.psm1`); PowerShell 7.6.5
- Command/flags used: `Get-BlastRadius`, `Test-BlastRadiusConflict` from `.claude/lib/blast-radius/BlastRadius.psm1`
- Data source or fixture: `config/blast-radius.json` at commit `a01bdbb0`

## Steps to Reproduce

1. Check out `main` at `a01bdbb0` (or later).
2. Import `.claude/lib/blast-radius/BlastRadius.psm1` and load `config/blast-radius.json`.
3. Build two radii from structured plan text whose task lines cite the root build files in inline code, for example a `- [ ] [P1-T1]` line containing `` `coverage.config` ``, `` `Directory.Build.targets` ``, and `` `TaskMaster.sln` ``.
4. Inspect `paths`, `modules`, and `shared_surfaces` on each radius.
5. Call `Test-BlastRadiusConflict` on the pair.

## Expected Behavior

Two items that both edit the same root build file should carry that file in their `shared_surfaces` sets and report `conflict=True` with a `shared_surface_overlap` reason, so cohort coloring places them in different cohorts.

## Actual Behavior

Both radii drop the tokens entirely. Neither the `paths` set nor the `shared_surfaces` set records them, `modules` is empty, and the pair reports `conflict=False` with no reasons. The two items are therefore eligible for the same cohort and can run concurrently against the same build files.

Root cause: `shared_surfaces` contains only `.claude/settings.json`, `config/orchestration-routing.json`, and `config/blast-radius.json`, and `shared_surface_globs` is empty. Under the extractor's shape rules a separator-free root token is admitted only as an exact member of `shared_surfaces`, and none of TaskMaster's root build files is a member. `$script:KnownTopLevelSegment` in `BlastRadiusExtraction.psm1` likewise does not admit them (it lists `scripts/`, `tests/`, `docs/`, `config/`, `schemas/`, `packages/`, `extensions/`, `.claude/`, `.codex/`, `.github/`, `.agents/`).

Before PR #575 this false negative was masked: every item carried module `docs`, so every pair conflicted anyway. Issue #545 predicted this exactly — "Correcting the module map alone would turn this into a reported false negative."

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet — observed on `a01bdbb0`:

  ```text
  C paths   : docs/features/active/2026-08-21-c-512/**
  C modules : []
  C shared  : []
  D shared  : []
  STEP5 both edit coverage.config + Directory.Build.targets + TaskMaster.sln
        -> conflict=False   (expected True with a shared-surface reason)

  ---  control: a token that IS a declared shared surface  ---
  E shared  : [config/orchestration-routing.json]
        -> conflict=True   (expected True)
  ```

  The control shows the shared-surface mechanism itself works; only the membership list is wrong.

  For contrast, the module-map half of #545 is confirmed fixed on the same commit:

  ```text
  A modules : [QuickFiler]     B modules : [ToDoModel]     E modules : [QuickFiler]
  480(QuickFiler) vs 287(ToDoModel)  -> conflict=False   (expected False)
  480(QuickFiler) vs 468(QuickFiler) -> conflict=True    (expected True)
  ```

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High rather than Blocker: it does not break a build or a gate, but it silently permits concurrent edits to the files that every other item's QA evidence depends on. A wrong `conflict=False` is more dangerous than a wrong `conflict=True`, because contention is the only mechanism the parallel surface has for serializing work — there is no `depends_on` key to fall back on.

## Suspected Cause / Notes

- `config/blast-radius.json` — `shared_surfaces` (3 entries) and `shared_surface_globs` (empty).
- `.claude/lib/blast-radius/BlastRadiusExtraction.psm1` — `$script:KnownTopLevelSegment`, and the F1a rule admitting a separator-free root token only as an exact `shared_surfaces` member.
- Candidate additions: `coverage.config`, `Directory.Build.targets`, `TaskMaster.sln`, `.editorconfig`, `.globalconfig`, `dotnet-tools.json`, `.csharpierignore`, and a `shared_surface_globs` entry for `.github/workflows/**`.
- Verify the candidate list against the repository root rather than assuming it; confirm each file exists before adding it.
- Related: `.claude/rules/parallel-orchestration.md` still narrates the reference repo's seven subsystem modules and does not describe the shipped TaskMaster config. That prose/config divergence is a separate documentation defect worth its own entry.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas — a Pester case per added surface asserting the token is harvested into `shared_surfaces`, plus a pair test asserting `conflict=True` with reason `shared_surface_overlap`. Note there is currently no Pester suite covering `.claude/lib/**` in this repository and no CI job that runs Pester, so this may require standing that up first.
- [x] Integration scenario to retest — rerun both #545 reproduction steps and require step 4 to stay `conflict=False` (no regression of the PR #575 fix) while step 5 flips to `conflict=True`.
- [x] Manual verification notes — keep the negative control (two items editing unrelated C# projects) in the same run, so a fix that makes everything conflict again is caught immediately. That failure mode is precisely what #545 reported.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
