# csharpier-documented-command-incompatible-with-pinned-version (Issue #509)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/csharpier-documented-command-incompatible-with-pinned-version/ (Issue #509)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #509
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/509
- Last Updated: 2026-08-08
## Summary

The C# format command documented in `CLAUDE.md` and `.claude/rules/csharp.md` is `dotnet tool run csharpier .`, which is CSharpier v0 syntax. The repository pins CSharpier **1.2.6** in `dotnet-tools.json`, and 1.2.6 requires a subcommand (`format <directoryOrFile>` or `check <directoryOrFile>`). The documented command therefore cannot format the repository as written.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: repo-local .NET SDK 8.0.205 installed to `.dotnet-sdk` by `scripts/vscode/Install-RepoDotNetSdk.ps1`
- Command/flags used: `./.dotnet-sdk/dotnet.exe tool run csharpier .` (documented) versus `./.dotnet-sdk/dotnet.exe tool run csharpier format .` (working)
- Data source or fixture: `dotnet-tools.json` at repository root, which pins `csharpier` to `1.2.6`

## Steps to Reproduce

1. From the repository root, run `./.dotnet-sdk/dotnet.exe tool restore` so the pinned CSharpier 1.2.6 is available.
2. Run the command documented in `CLAUDE.md` § "C# Toolchain (run in this exact order)" step 1: `dotnet tool run csharpier .`
3. Compare with `./.dotnet-sdk/dotnet.exe tool run csharpier -- --help`, which lists the available commands.

## Expected Behavior

The format command documented in the policy files is the command that actually formats the repository with the pinned formatter version, so an agent or developer following the documented toolchain order can complete step 1 without substitution.

## Actual Behavior

CSharpier 1.2.6 exposes only the subcommands `format`, `check`, `pipe-files`, and `server`. Invoking it with a bare path argument does not run the documented format step. Verified on 2026-08-08:

```
Commands:
  format <directoryOrFile>  Format files.
  check <directoryOrFile>   Check that files are formatted. Will not write any changes.
  pipe-files                Keep csharpier running so that multiples files can be piped to it via stdin.
  server                    Run CSharpier as a server so that multiple files may be formatted.
```

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `./.dotnet-sdk/dotnet.exe tool run csharpier --version` returns `1.2.6`; the subcommand list above is the output of `... tool run csharpier -- --help`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

The documented toolchain is the authority every agent session and contributor follows, and formatting is step 1 of a mandatory four-stage loop that must restart from step 1 on any change. A stale invocation string forces each session to independently discover the correct syntax and deviate from a policy document, which is precisely the kind of silent divergence the policy exists to prevent. There is also a correctness trap: a globally installed CSharpier (1.3.0 was present on the affected machine) can satisfy the bare-path form, so a session may format with an unpinned version and produce diffs that disagree with CI.

## Suspected Cause / Notes

Observed during orchestration of issue #438 on 2026-08-08.

- `CLAUDE.md` § "C# Toolchain (run in this exact order)" step 1 and § "C#1. Tooling & Baseline for C#" item 1 both give `dotnet tool run csharpier .` / `csharpier .`.
- `.claude/rules/csharp.md` § Toolchain item 1 repeats the same form.
- `dotnet-tools.json` (repository root, not `.config/`) pins `csharpier` `1.2.6` with `rollForward: false`.
- CSharpier moved to subcommand syntax after v0; the documented form predates the version pin.
- Note the related hazard: `dotnet-tools.json` sits at the repository root rather than the conventional `.config/dotnet-tools.json`.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: not applicable — this is a documentation correction. If a toolchain-command lint exists or is added, assert that documented commands are executable against the pinned tool versions.
- [ ] Integration scenario to retest: run each documented toolchain command verbatim from a clean checkout after `dotnet tool restore` and confirm each exits 0.
- [ ] Manual verification notes: update `CLAUDE.md` (both locations) and `.claude/rules/csharp.md` to `dotnet tool run csharpier format .`, and document `check .` as the non-mutating gate form. Consider stating explicitly that the repo-pinned tool must be used and that a globally installed CSharpier must not be substituted, since version drift produces format diffs that disagree with CI.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
