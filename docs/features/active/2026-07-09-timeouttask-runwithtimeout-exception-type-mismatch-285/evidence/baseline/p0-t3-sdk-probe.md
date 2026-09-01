# P0-T3 — .NET SDK Probe and Bootstrap

Timestamp: 2026-09-01T08-04

## Invocation 1 — initial probe

Command: `dotnet --version`

EXIT_CODE: -2147450725

Output Summary: The probe failed with the repo-local SDK guard message:

```text
The command could not be loaded, possibly because:
  * You intended to execute a .NET application:
      The application '--version' does not exist or is not a managed .dll or .exe.
  * You intended to execute a .NET SDK command:
      The repo-local .NET SDK is missing. Run ./scripts/vscode/Install-RepoDotNetSdk.ps1 from the repository root, then retry dotnet format TaskMaster.sln.
```

The message names `.dotnet-sdk` bootstrap explicitly, so the plan's conditional bootstrap branch was
taken.

## Invocation 2 — bootstrap

Command: `pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1` (run from the repository root)

EXIT_CODE: 0

Output Summary:

```text
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to <worktree-root>\.dotnet-sdk.
```

The installed version `8.0.205` matches the `global.json` pin.

## Invocation 3 — final probe

Command: `dotnet --version`

EXIT_CODE: 0

Output Summary: stdout is `8.0.205`, a version string of the form `8.` or higher.

## Bootstrap Determination

**Was the bootstrap needed? Yes.** This is a fresh agent worktree; `.dotnet-sdk` was absent before
this task and `dotnet --version` could not run. `scripts/vscode/Install-RepoDotNetSdk.ps1` was
executed once and succeeded.

## Footprint Note (recorded, no exclusion-set entry follows)

The SDK install directory the bootstrap script creates is `.dotnet-sdk` at the worktree root. It is
matched by the directory-only glob at `.gitignore` line 350, which is `.dotnet*/`. Verified after the
install by running `git status --porcelain`, whose complete output was:

```text
 M docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/plan.2026-09-01T00-30.md
?? docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/evidence/
```

`.dotnet-sdk` does not appear. It therefore never enters `git status --porcelain` and sits outside
the footprint sets asserted by P3-T1, P3-T11, and P4-T14. **No exclusion-set entry follows from this
observation.** It is recorded here only so a reader of the footprint artifacts does not have to
re-derive it. The exclusion sets used by P3-T1, P3-T11, and P4-T14 remain exactly
`.claude/agent-memory/` plus the P0-T6 unformatted-file list, and nothing else.

Acceptance: met. A final `dotnet --version` invocation is recorded with `EXIT_CODE: 0` and stdout
`8.0.205`; the artifact records that the bootstrap script was needed and did run; and the
`.gitignore` line 350 observation is recorded with its verification command and output.
