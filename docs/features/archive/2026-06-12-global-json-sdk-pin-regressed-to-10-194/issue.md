# global-json-sdk-pin-regressed-to-10 (Issue #194)

- Date captured: 2026-06-12
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/global-json-sdk-pin-regressed-to-10/ (Issue #194)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #194
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/194
- Last Updated: 2026-06-13
- Work Mode: minor-audit

## Summary

The committed `global.json` SDK pin was changed from `8.0.205` to `10.0.200`, regressing the repo-local .NET 8 SDK workaround and breaking the `Install-RepoDotNetSdk` Pester test. Investigation concluded the pin should be reverted to `8.0.205`.

## Environment

- OS/version: Windows, PowerShell 7+
- Python version: N/A
- Command/flags used: `Invoke-Pester tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1`
- Data source or fixture: repo-root `global.json`

## Steps to Reproduce

1. Run the Pester suite under `tests/scripts/vscode`.
2. Observe `Install-RepoDotNetSdk.Tests.ps1` line 22 fails: expected `8.0.205`, actual `10.0.200`.

## Expected Behavior

`global.json` `sdk.version` is `8.0.205`, consistent with `Install-RepoDotNetSdk.ps1` (which defaults to `8.0.205` and installs to `.dotnet-sdk/sdk/8.0.205`) and with the test that documents the repo-local .NET 8 SDK pin.

## Actual Behavior

`global.json` pins `sdk.version` `10.0.200`. This was introduced by commit `32bd99e2` ("(bug): fixed virtual lines as well as dotnet reference") with no feature doc, issue, or acceptance criterion indicating a deliberate SDK upgrade. The `codex-web-setup-test.yml` workflow reads this field to build the `.dotnet-sdk/sdk/<version>` marker directory, so the committed value is inconsistent with the install script (`8.0.205`).

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: see `artifacts/research/2026-06-12-global-json-sdk-version-research.md` (full evidence chain and determination).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Breaks a repo Pester test and desynchronizes `global.json` from the install script and the codex-web-setup workflow marker directory. CI is not affected (CI installs .NET 10 independently via `actions/setup-dotnet` and uses `csharpier`, not `dotnet format`).

## Suspected Cause / Notes

Commit `32bd99e2` overwrote the deliberate `8.0.205` pin (added in `b3c3c8f1`) with the author's host SDK version `10.0.200`, incidentally rather than as an intended upgrade. The `dotnet format` wording in the test/script comments is stale (CLAUDE.md now mandates `csharpier` and prohibits `dotnet format`); `csharpier` is unaffected by SDK version, but the `global.json` value remains load-bearing for the codex-web-setup workflow marker.

## Proposed Fix / Validation Ideas

- [x] Revert `global.json` `sdk.version` from `10.0.200` to `8.0.205`. Do not change the test.
- [ ] Optionally update the stale "retry dotnet format" message to reference `csharpier .` (cosmetic).
- [x] Validation: re-run `tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1`; the SDK-pin assertions pass. Confirm no CI step regresses.

## Acceptance Criteria

- [x] AC1: `global.json` `sdk.version` is `8.0.205` (reverted from `10.0.200`); `rollForward`, `allowPrerelease`, and `paths` are unchanged.
- [x] AC2: `tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1` passes, including the `global.json SDK selection` assertions (version `8.0.205`, `rollForward` `latestFeature`, `allowPrerelease` false, `paths` contains `.dotnet-sdk` and `$host$`).
- [x] AC3: No other `global.json` keys or unrelated files are modified (scope limited to the one-field revert).
- [x] AC4: The PowerShell toolchain (PoshQC format, PSScriptAnalyzer, Pester) passes with no new findings on changed/related files.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
- [ ] Implement revert; verify; minor-audit review; PR