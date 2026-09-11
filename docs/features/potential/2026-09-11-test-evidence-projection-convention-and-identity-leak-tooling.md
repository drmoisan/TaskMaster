# test-evidence-projection-convention-and-identity-leak-tooling (Potential Bug)

- Date captured: 2026-09-11
- Author: Dan Moisan
- Status: Draft

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

## Summary

Implements the committed test-evidence convention the maintainer recorded on #671 on 2026-09-11 and removes the identity leaks that live in configuration rather than historical evidence (#728, the tooling half of #602). Raw `.trx` and raw `.cobertura.xml` output is no longer committed: the coverage entry point writes a package-level JaCoCo projection and the one-line first-party summary, the test runner writes a pass/fail summary, and raw output is discarded. Test-invocation scripts set an explicit results directory and log file name so the `<account>_<HOST>_<timestamp>.trx` default is never produced. The `PublishUrl` leak in `TaskMaster.csproj`, the five agent-memory files, and `.vscode/settings.json` are corrected in the same delivery.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: not applicable (PowerShell 7 toolchain scripts under `scripts/vscode/`, Pester; one `.csproj` line; Markdown)
- Command/flags used: `scripts/vscode/Invoke-MSTestWithCoverage.ps1`; `vstest.console.exe ... /Logger:trx`
- Data source or fixture: `git ls-files` on `main` at 3cb974422

## Steps to Reproduce

1. Run any repository test gate that writes evidence under a feature's `evidence/` tree. Observe a raw `.trx` whose `runUser`, `computerName`, and lowercased `storage` attributes carry the account, host, and absolute checkout path, and a raw Cobertura document of 10 to 43 MB.
2. Count tracked evidence on `main`: 332 `.trx` files (281 MB) and 248 `.cobertura.xml` files (3,227 MB in the working tree).
3. Read `TaskMaster/TaskMaster.csproj:37`: `<PublishUrl>` carries the developer's account name and employer organization name.
4. Search `.claude/agent-memory/` for the account or machine name: five files match (`epic-orchestrator/feedback_measure_whole_volume_before_blaming_worktrees.md`, `feature-review/project_464-review-residuals.md`, `feature-review/project_488-review-residuals.md`, `orchestrator/angle-bracket-redaction-breaks-trx-xml.md`, `orchestrator/collect-pr-context-lands-in-main-checkout.md`). These are not push-down owned.
5. Read the artifact-hygiene redaction sweep used during feature reviews: it excludes the plan file from its own residual scan (#671 comment of 2026-09-02, from item #662).

## Expected Behavior

- The coverage entry point emits a JaCoCo package-level projection (the #646 / PR #718 shape, about 2 KB) plus the `Get-CoberturaFirstPartyCoverageReport` line, and deletes the raw Cobertura document after the threshold assertion and projection are complete.
- The test runner path emits a summary (passed, failed, skipped, total, names of failed tests) and deletes the raw `.trx`.
- Every vstest invocation in `scripts/vscode/` passes `/ResultsDirectory:` and `/Logger:trx;LogFileName=<task>.trx`.
- `TaskMaster.csproj` carries no `<PublishUrl>` with a personal path (empty or a placeholder-relative value).
- The five agent-memory files use `<user>`, `<host>`, `<user-profile>` placeholders.
- `.vscode/settings.json` carries no absolute user-profile path.
- The redaction sweep scans the plan file.
- The evidence convention is written down once, in the evidence-and-timestamp-conventions skill or its nearest TaskMaster-owned equivalent, and cited by the atomic-plan contract's evidence tasks.

## Actual Behavior

Raw TRX and Cobertura are committed on every feature; the TRX carries host identity; the ad-hoc angle-bracket redaction makes the XML unparseable; `PublishUrl`, five memory files, and `.vscode/settings.json` leak identifiers; the sweep misses plan files.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `storage="c:\users\<account>\repos\taskmaster\...\quickfiler.test.dll"`, `runUser="<machine>\<account>"` (from #671).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

- No convention existed for what committed test evidence should look like; each item improvised.
- `.claude/settings.json` is push-down owned and is excluded from this item (routed upstream under #602).
- The historical sweep over the 900+ already-tracked files is a separate item (#602) and must run after this one so a fresh test run does not reintroduce the prefix.
- Do not delete the already-tracked raw files here; that is the sweep's job and its blast radius spans every feature folder.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: projection writer, summary writer, raw-file deletion, argument construction for `ResultsDirectory` and `LogFileName`, sweep includes plan file. Pester over `tests/scripts/vscode/`, no temporary files (use the in-memory fixture pattern from `Invoke-MSTestWithCoverage.FirstParty.Tests.ps1`).
- [x] Integration scenario to retest: run the VS Code coverage task once and confirm the feature evidence folder receives only the projection and summary.
- [x] Manual verification notes: `git grep` for the account and host name over the changed files returns zero.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

Closes #671 and #728 on merge. Partially addresses #602 (acceptance criteria 3 and 4).
