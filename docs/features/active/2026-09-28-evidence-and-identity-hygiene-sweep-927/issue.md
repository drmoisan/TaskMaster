# evidence-and-identity-hygiene-sweep (Issue #927)

- Date captured: 2026-09-28
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/ (Issue #927)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #927
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/927
- Last Updated: 2026-09-28
- Work Mode: full-bug

## Summary
Consolidates the unresolved remainder of #602, #671 and #884, and the plan-file exclusion gap from #727 sub-finding 5. The common root cause: identity-bearing test artifacts and absolute host paths were committed before the committed-evidence convention existed, and nothing rejects them at commit or CI time. PR #881 fixed prevention in the `scripts/vscode` test tooling (explicit `/ResultsDirectory:` and `LogFileName=`, projections instead of raw documents). It did not remove existing content and did not add an enforcement guard, so the counts keep growing.

## Environment
- OS/version: Windows 11 Pro 10.0.26200
- Python version: not applicable (repository hygiene; tracked Markdown, TRX and Cobertura files)
- Command/flags used: `git ls-files`, `git grep -l -i <account>`, measured on `main` at `177b6d78e` on 2026-09-28
- Data source or fixture: tracked files under `docs/features/**`, `docs/research/**`, `tests/**`, `*.Test/**`, and the root-level `test-output.txt`

## Steps to Reproduce
1. `git ls-files -- "docs/features/**/*.trx"` returns 332 paths.
2. `git ls-files -- "docs/features/**/*cobertura*.xml"` returns 248 paths.
3. Search tracked files for the developer account name, host name, and absolute user-profile path.

## Expected Behavior
- No tracked raw test-platform document (`*.trx`) and no raw coverage collector document (`*cobertura*.xml`, `*.coverage`). This is the rule in the CLAUDE.md "Committed Test Evidence Format" section.
- No tracked file outside `.claude/**` contains an absolute user-profile path, the bare account name, or the bare host name. Use the placeholders `<repo-root>`, `<user-profile>`, `<user>` and `<host>`.
- A CI check rejects new violations, so the cleanup does not regress.

## Actual Behavior
Measured on `main` at `177b6d78e` (2026-09-28):

| Condition | Tracked files | At filing |
|---|---|---|
| Raw `.trx` under `docs/features` | 332 | 333 (#884) |
| Raw `*cobertura*.xml` under `docs/features` | 248 | 248 (#671) |
| Contains the account name | 1,222 | 991 (#602) |
| Contains an absolute user-profile path | 1,217 | 45 (#602) |
| Contains the host name | 183 | 146 (#602) |

- Nearly all hits are under `docs/features/**`.
- Other locations:
  - `test-output.txt` at the repository root (a stray run log)
  - one file under `tests/`
  - one file under `docs/research/`
  - about 13 files in `*.Test` projects. These may be test fixtures and must be triaged, not blindly rewritten.
- `.mcp.json` and `.codex/config.toml` match only through the npm package scope `@<account>/drm-copilot-mcp`. That is a package identifier, not a host identifier, and is out of scope.
- About 8 `.claude/**` files are affected, including `.claude/settings.json:75`. They are push-down owned from drm-copilot and are tracked upstream in the companion upstream issue. Do not edit them here.

## Logs / Screenshots
- [x] Attached minimal logs or snippet
- Snippet: counts above. The identifiers themselves are deliberately not reproduced.

## Impact / Severity
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

The repository is out of compliance with its own committed-evidence policy across hundreds of files. The leak surface grew 25-fold for profile paths after the prevention tooling landed. That shows prevention by convention alone does not hold.

## Suspected Cause / Notes
- Evidence Markdown written by agents quotes absolute paths and tool banners verbatim.
- Raw vstest and AltCover output embeds `runUser`, `computerName` and a lowercased `storage` path.
- No gate checks for either.
- The redaction sweeps agents ran in the past also excluded the plan file from their own residual scan (#727 sub-finding 5, from item #662).

## Proposed Fix / Validation Ideas
- [ ] `git rm` every tracked `*.trx`, `*cobertura*.xml` and `*.coverage` file.
  - Where a feature's figures are still needed, keep or produce the permitted projection. Otherwise rely on the committed summary.
  - Removing the files does not rewrite history. History rewriting is explicitly out of scope.
- [ ] Add `.gitignore` rules for the raw evidence document types.
- [ ] Replace identifiers with placeholders across all tracked non-`.claude` text files:
  - Apply the replacements longest-first so the substitution is idempotent.
  - Scan every file type, plan files included.
  - Delete `test-output.txt`.
  - Triage the `*.Test` hits individually.
- [ ] Add a CI guard to `ci.yml`, following the `_<name>.yml` convention, that fails on:
  - a tracked raw evidence document
  - a Windows absolute user-profile path pattern in any tracked file outside `.claude/**`
  - The guard must not embed the literal identifiers it searches for.
- [ ] Coordination: the other items in the same parallel run write evidence under `docs/features/active/**`. They must pass the new guard, so confirm their evidence follows the convention before the guard becomes required.
- [ ] Validation: all three `git ls-files`/`git grep` counts above return 0 outside `.claude/**`, and the guard fails on a deliberately introduced violation.

## Next Step
- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

Consolidates: #602 (non-`.claude` portion), #671 (remaining existing-file portion), #884, #727 sub-finding 5 (plan-file exclusion).