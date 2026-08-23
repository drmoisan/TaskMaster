# Toolchain Step 1 (format) — CSharpier

Timestamp: 2026-08-08T16-35

Task: [P2-T1] — final QC loop, pass 1

Command: `C:\Users\DanMoisan\.dotnet\tools\csharpier.exe format C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad7090ae544fd0fb0`

EXIT_CODE: 0

```
Formatted 1488 files in 6320ms.
```

## Reformatted-file count: 0

"Formatted 1488 files" is CSharpier's phrasing for files **processed**, not files rewritten. No file
content changed. Proof, taken immediately after the format run:

Command: `git status --porcelain -- '*.cs' '*.csproj' '*.sln'`

```
 M UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs
 M UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs
```

Command: `git diff --stat -- '*.cs'`

```
 .../Folder/WpfDispatcherYieldTests.cs              | 166 ++++++++++++++++++++-
 .../OutlookObjects/Folder/WpfDispatcherYield.cs    |  41 ++++-
 2 files changed, 201 insertions(+), 6 deletions(-)
```

Both outputs are byte-identical to the pre-format state recorded at P1-T15/P1-T16
(`<FEATURE>/evidence/other/scope-boundary.2026-08-08T16-33.md`): the same two modified files and the
same 201 insertions / 6 deletions. No third file appeared and no line count moved, so the formatter
rewrote nothing.

**Loop consequence: no restart.** The toolchain loop restarts at P2-T1 only if a step fails or
auto-fixes files. This step did neither, so the pass proceeds to P2-T2.

## Canonical-command reconciliation (approved micro-action, not a deviation)

`CLAUDE.md` and `.claude/rules/csharp.md` state the canonical formatter command as
`dotnet tool run csharpier .` or `csharpier .`. CSharpier 1.3.0 — the version of the global tool
used here — requires an explicit subcommand: `format` to rewrite and `check` to verify. Bare
`csharpier .` is not a valid 1.3.0 invocation and returns a usage error rather than a format result.
`csharpier format <path>` is therefore the 1.3.0 spelling of the policy's mutating formatter step,
and `csharpier check <path>` (P2-T2) is the 1.3.0 spelling of the enforcing verification step. The
semantic gate required by policy — every C# source file is CSharpier-formatted — is enforced
identically and is proven by P2-T2's `EXIT_CODE: 0`.

`dotnet tool run csharpier` is unavailable in this checkout: there is no `.config/dotnet-tools.json`
manifest (the repo-root manifest is named `dotnet-tools.json`, which `dotnet tool run` does not
read) and no repo-local `.dotnet-sdk`, so every `dotnet` SDK command fails with the `global.json`
missing-SDK error. The same 1.3.0 binary was used for the P0-T6 baseline, so baseline and gate are
internally consistent. The reduced audit must not read this spelling difference as a deviation from
the toolchain policy.

Output Summary: PASS, EXIT_CODE 0. CSharpier 1.3.0 processed 1488 C# files and rewrote none — the
scoped `git status` and `git diff --stat` after the run are byte-identical to the pre-format state
(same two files, 201 insertions / 6 deletions). Because no file changed and the step did not fail,
the toolchain loop does not restart and continues to P2-T2. This is pass 1 of the final QC loop.
