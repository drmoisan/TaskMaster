# P0-T8 — Pinned formatter restore (baseline)

Timestamp: 2026-09-13T23-01

Command: `dotnet tool restore` (run with the working directory set to the worktree root, where
`dotnet-tools.json` pins CSharpier to 1.2.6)

EXIT_CODE: 0

Output Summary:

The restore printed:

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

### Success-case observations

The restore command's own exit code does not distinguish a restored manifest from an absent one,
and it prints a different line on a warm cache than on a cold one, so two further observations were
taken.

**1. The `csharpier` row of `dotnet tool list --local`** (exit code 0):

| Package Id | Version | Commands |
|---|---|---|
| csharpier | 1.2.6 | csharpier |

The Version column reads `1.2.6` and the Commands column reads `csharpier`, as required.

**2. `dotnet tool run csharpier check --help`** — exit code 0, first output line `Description:`.
This invocation exits zero only when the manifest tool is restored and runnable.

**3. Additional observation.** `dotnet tool run csharpier --version` was also run during execution
and printed `1.2.6` with exit code 0, so on this manifest that invocation does run. It is recorded
in addition to, and not in place of, the two observations above.

### Prerequisite recorded: repo-local .NET SDK install

The first invocation of `dotnet tool restore` in this worktree failed with exit code
`-2147450725` and the diagnostic:

```
The command could not be loaded, possibly because:
  * You intended to execute a .NET application:
      The application 'tool' does not exist or is not a managed .dll or .exe.
  * You intended to execute a .NET SDK command:
      The repo-local .NET SDK is missing. Run ./scripts/vscode/Install-RepoDotNetSdk.ps1 from the repository root, then retry dotnet format TaskMaster.sln.
```

The repository's own bootstrap script `scripts/vscode/Install-RepoDotNetSdk.ps1` was run as the
diagnostic directs. It downloaded and installed .NET SDK 8.0.205 into the worktree-local
`.dotnet-sdk` directory (the script's default install location, derived from its own script root
rather than from the working directory). `dotnet tool restore` then succeeded as recorded above.
This is a worktree bootstrap step, not a change to any tracked file; `.dotnet-sdk` is not in this
plan's pathspec set and is not committed.

Every formatting command in this plan is invoked through `dotnet tool run` so that the
manifest-pinned 1.2.6 is used rather than any global install.
