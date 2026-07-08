---
name: project-flaky-ci-physicalfileinfoadapter-test
description: A specific FS-adapter test flakes on CI by opening the real TaskMaster.sln; recognize it, re-run the failed job first, and only fix if it recurs deterministically
metadata:
  type: project
---

`UtilitiesCS.Test.HelperClasses.PhysicalFileSystemAdapters_Tests.PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo` intermittently fails the required CI check `Format, build, analyze, and test` with `System.IO.IOException: The process cannot access the file 'TaskMaster.sln' because it is being used by another process`.

**Why:** the test opens the real repo solution file. At `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs:207` the read open `adapter.Open(FileMode.Open, FileAccess.Read)` uses `FileInfo.Open(mode, access)` (PhysicalFileInfoAdapter.cs:134), which defaults to `FileShare.None`. Under parallel CI execution + coverage instrumentation the solution file is held open by another process, so the read open throws. The test's own comments claim read opens "do not contend" — that assumption is wrong for the `FileShare.None` open at line 207. It is a determinism/isolation violation (UT policy) that predates and is unrelated to feature work that does not touch it.

**How to apply:** if this exact test is the ONLY required-check failure and the diff does not touch `UtilitiesCS` FS adapters, treat it as pre-existing flakiness: re-run the failed job (`gh run rerun <run-id> --failed`) and re-poll — it cleared on the first re-run for PR #272 (issue #270). This is a legitimate CI re-run, not a test-level retry hack. If it recurs deterministically, fix it per [[project_flaky_fs_adapter_test_pattern]] (injectable-delegate seam / consistent `FileShare.ReadWrite`, never a temp/scratch file) via the remediation loop — do not merge around a deterministic failure. A real follow-up fix (add `FileShare.ReadWrite` to the line-207 read open, or seam the FileInfo) would remove the flakiness for good.
