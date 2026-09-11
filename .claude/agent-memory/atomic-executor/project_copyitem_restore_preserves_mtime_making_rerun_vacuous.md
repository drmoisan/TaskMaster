---
name: copyitem-restore-preserves-mtime-making-rerun-vacuous
description: Copy-Item restores a file with the ORIGINAL LastWriteTime, so MSBuild skips CoreCompile and the restore-and-rerun step of a fail-before/pass-after experiment silently tests the old binary
metadata:
  type: project
---

`Copy-Item` preserves the SOURCE file's `LastWriteTime`. Restoring a backup over a source file
therefore gives it an OLD timestamp — older than the assembly just built from the modified version.
MSBuild's up-to-date check then skips `CoreCompile`, the build reports exit 0 without recompiling,
and the "restored" test run exercises the previous binary.

**Why:** Hit on 2026-09-07 re-deriving AC2 fail-before evidence for #798. Removed instrumentation,
built, ran (2 failed, correct). Restored via `Copy-Item`, rebuilt (exit 0, project listed in output),
re-ran — still 2 failed. That looked like a genuine finding that the fix did not work. It was not:
the restored `.cs` carried its pre-removal mtime `02:25:01` while the DLL was built at `02:47:07`,
so nothing recompiled.

**How to apply:** After ANY file restore that precedes a build, set the mtime forward before
rebuilding: `(Get-Item -LiteralPath $p).LastWriteTime = (Get-Date)`. This changes the timestamp only,
so re-hash to show content is untouched and byte-identity still holds. Diagnose the class with a
direct comparison — get the source and the output assembly `LastWriteTime` and assert source is
newer; if it is not, the run you are about to record is vacuous. MSBuild printing
`Project -> path\to.dll` does NOT prove a compile happened: that line comes from
`CopyFilesToOutputDirectory`, which runs even when `CoreCompile` is skipped.

Distinct from [[incremental-build-vacuous-baseline]] and the `/t:Build` vs `/t:Rebuild` property
hazard: those are about command-line `/p:` changes not invalidating the up-to-date check. This one
fires on a source-content change, because the tool that wrote the content backdated it.
