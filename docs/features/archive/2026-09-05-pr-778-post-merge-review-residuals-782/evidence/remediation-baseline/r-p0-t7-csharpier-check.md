# [P0-T7] Baseline — CSharpier format check

Timestamp: 2026-09-06T01-32

Command:

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
dotnet tool run csharpier check .
```

Run from the worktree root. The SDK preamble is required because `global.json` pins an SDK the host
cannot satisfy through a plain `dotnet` on `PATH`. CSharpier is invoked through `dotnet tool run` so
the version pinned by the root `dotnet-tools.json` manifest is used; CSharpier 1.2.6 requires an
explicit subcommand, so `check` is written out.

EXIT_CODE: 0

Output Summary: the read-only check passed with no reformatting required. The printed line, verbatim:

```text
Checked 1583 files in 3964ms.
```

BASELINE-CSHARPIER-CHECKED-FILES: 1583

## Consumer

[P4-T2] re-runs this same command after the two test-file edits and asserts that its recorded
`Checked` numeral equals **1583**. The two edits change existing files and add none, so the checked
file count is expected to be unchanged. A different numeral would mean the tracked file set moved and
must be explained in the [P4-T2] artifact before that task is checked.

The elapsed-milliseconds figure on the same printed line is not asserted against; it varies between
runs and carries no acceptance meaning.
