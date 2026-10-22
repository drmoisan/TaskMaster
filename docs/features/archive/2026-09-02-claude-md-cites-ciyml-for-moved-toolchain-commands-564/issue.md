# claude-md-cites-ciyml-for-moved-toolchain-commands (Issue #564)

- Date captured: 2026-08-15
- Author: drmoisan
- Status: Promoted -> docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/ (Issue #564)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #564
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/564
- Last Updated: 2026-08-15
- Work Mode: full-bug

## Summary

`CLAUDE.md` cites `.github/workflows/ci.yml` as the authority for three toolchain commands. PR #556 (#553, the CI parallel job split) moved those commands into reusable workflows, so the citations name a file that no longer contains them.

## Environment

- OS/version: not applicable (documentation-only defect)
- Runtime: not applicable
- UI paths: not applicable
- Data source or fixture: `CLAUDE.md`, `.github/workflows/ci.yml`, `.github/workflows/_build-analyzers.yml`, `.github/workflows/_format-check.yml`, `.github/workflows/_build-nullable.yml`

## Steps to Reproduce

1. Open `CLAUDE.md` and read the C# toolchain guidance around lines 194, 202, and 210-211.
2. Follow the citation to `.github/workflows/ci.yml` to verify the cited command.
3. Observe that post-split, `.github/workflows/ci.yml` contains no `msbuild` or `csharpier` invocation at all; it is a dispatcher of five `uses:` calls to reusable workflows.

## Expected Behavior

Each citation names the reusable workflow file that actually contains the cited command:
- The CSharpier pinned-version citation (line ~194) names `.github/workflows/_format-check.yml`.
- The analyzer `/t:Build /m` citation (line ~202) names `.github/workflows/_build-analyzers.yml`.
- The nullable command citation (line ~210-211) names `.github/workflows/_build-nullable.yml`, which contains the step named "Build with nullable warnings treated as errors".

## Actual Behavior

All three citations still name `.github/workflows/ci.yml`:
- line ~194 — attributes the analyzer step's `/t:Build /m` to `ci.yml`; it now lives in `.github/workflows/_build-analyzers.yml`.
- line ~202 — attributes the CSharpier pinned-version invocation to `ci.yml`; it now lives in `.github/workflows/_format-check.yml`.
- line ~210-211 — states the nullable command is "character-for-character the command in `.github/workflows/ci.yml` (step 'Build with nullable warnings treated as errors')". `ci.yml` contains no step of that name; it is a job name in `.github/workflows/_build-nullable.yml`.

The claims themselves remain true of the tree; only the file being cited is wrong.

**Correction (verified 2026-09-02):** the line/content pairing quoted above is transcribed verbatim from the original GitHub issue #564 body, but that pairing is itself swapped relative to the current tree. Four independent reads of `CLAUDE.md` (research artifact, `prd-feature` review, `atomic-planner` self-review, `atomic-executor` preflight) all confirm: line 194 is the **CSharpier** pinned-version bullet (not the analyzer bullet), and line 202 is the **analyzer** `/t:Build /m` bullet (not the CSharpier bullet). Line 210-211 (nullable) is correctly attributed. `spec.md`'s Root Cause Analysis and the approved plan (`plan.2026-09-02T08-58.md`) both use the corrected mapping (line 194 → `_format-check.yml`, line 202 → `_build-analyzers.yml`, line 210 → `_build-nullable.yml`) and are authoritative over this section.

## Logs / Screenshots

- [x] Not applicable (documentation-only defect; no runtime logs)
- Verified by reading `CLAUDE.md` lines 185-213 and the five files under `.github/workflows/` on 2026-09-02.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

`CLAUDE.md` is always loaded into every agent session and is the top of the policy precedence order. An agent that follows the citation to verify a command will not find it, and the third reference names a step that does not exist anywhere under that name. Severity is Low because the commands themselves remain correct; only the supporting citation is stale.

## Suspected Cause / Notes

The corresponding text in `.claude/rules/csharp.md` was corrected during the `main` merge at `fb8eff9b`; the same fix was not applied to `CLAUDE.md`. Verified on 2026-09-02: `.claude/rules/csharp.md` carries no citation to `ci.yml` or to any reusable workflow file name, so it requires no change for this fix.

Found during the `build-ci-coverage-gate-fidelity` epic fan-in review.

## Proposed Fix / Validation Ideas

Repoint the three references in `CLAUDE.md` to `_build-analyzers.yml`, `_format-check.yml`, and `_build-nullable.yml` respectively. Documentation-only; no logic change.

Validation:

- [ ] Confirm each of the three updated citations in `CLAUDE.md` names the reusable workflow file that actually contains the cited command, verified by reading the corresponding `.github/workflows/*.yml` file.
- [ ] Confirm no other citation to `.github/workflows/ci.yml` remains in `CLAUDE.md` for a command that moved to a reusable workflow.
- [ ] Confirm `.claude/rules/csharp.md` is unchanged (already correct).

## Next Step

- [x] Promote to GitHub issue (bug-report template) — already open as issue #564
- [ ] Move to active fix folder / branch
