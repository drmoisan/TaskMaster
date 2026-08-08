# Toolchain Step 1 (format) — CSharpier — FINAL CLEAN PASS (pass 4)

Timestamp: 2026-08-08T16-48

Task: [P2-T1] — final QC loop, pass 4 (the clean pass attested by P2-T6)

Command: `C:\Users\DanMoisan\.dotnet\tools\csharpier.exe format C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad7090ae544fd0fb0`

EXIT_CODE: 0

```
Formatted 1488 files in 1325ms.
```

## Reformatted-file count: 0

"Formatted 1488 files" is CSharpier's phrasing for files **processed**, not rewritten. Verified
immediately after the run:

Command: `git diff --stat -- '*.cs'`

```
 .../Folder/WpfDispatcherYieldTests.cs              | 166 ++++++++++++++++++++-
 .../OutlookObjects/Folder/WpfDispatcherYield.cs    |  41 ++++-
 2 files changed, 201 insertions(+), 6 deletions(-)
```

Identical to the pre-format state (same two files, same 201 insertions / 6 deletions). No file was
rewritten, so the loop does not restart.

## Pass history (recorded for the P2-T6 ordinal)

| Pass | Outcome | Reason |
|---|---|---|
| 1 | restarted | P2-T5 failed with 2 pre-existing `QuickFiler.Test` failures |
| 2 | restarted | P2-T5 failed with the same 2 pre-existing failures |
| 3 | abandoned mid-pass | The attribution experiment's file restore used `Copy-Item`, which preserves source timestamps; the restored files were older than the build outputs, so MSBuild treated everything as up to date (1.06s, no `CoreCompile`) and the binaries still held baseline code. Detected and corrected rather than reported as a pass. |
| **4** | **CLEAN** | all five steps passed in order, no file rewritten |

Between pass 3 and pass 4 the two changed files' `LastWriteTime` values were set forward so MSBuild
would genuinely recompile. That is a filesystem-metadata change only — file **content** was
unchanged, proven by SHA-256 before and after the attribution experiment
(`<FEATURE>/evidence/regression-testing/preexisting-failure-attribution.2026-08-08T16-52.md`) — and
it occurred **before** pass 4's P2-T1, so it does not break the "no file rewritten during the pass"
condition.

## Canonical-command reconciliation (approved micro-action, not a deviation)

`CLAUDE.md` and `.claude/rules/csharp.md` give the canonical formatter command as
`dotnet tool run csharpier .` or `csharpier .`. CSharpier 1.3.0 requires an explicit subcommand:
`format` to rewrite, `check` to verify. Bare `csharpier .` is not a valid 1.3.0 invocation and
returns a usage error rather than a format result, so `csharpier format <path>` is the 1.3.0
spelling of the policy's formatter step and `csharpier check <path>` (P2-T2) is the enforcing
verification. The semantic gate — every C# file is CSharpier-formatted — is enforced identically.

`dotnet tool run csharpier` is unavailable here: no `.config/dotnet-tools.json` manifest (the
repo-root file is `dotnet-tools.json`, which `dotnet tool run` does not read) and no repo-local
`.dotnet-sdk`, so every `dotnet` SDK command fails with the `global.json` missing-SDK error. The
same 1.3.0 binary produced the P0-T6 baseline, so baseline and gate are consistent. The reduced
audit must not read this spelling difference as a toolchain deviation.

Output Summary: PASS, EXIT_CODE 0. CSharpier 1.3.0 processed 1488 files and rewrote none — the
scoped `git diff --stat` after the run is identical to before (2 files, 201 insertions / 6
deletions). This is pass 4, the clean pass; passes 1 and 2 restarted on pre-existing out-of-scope
test failures and pass 3 was abandoned after a stale-build condition was detected and corrected.
