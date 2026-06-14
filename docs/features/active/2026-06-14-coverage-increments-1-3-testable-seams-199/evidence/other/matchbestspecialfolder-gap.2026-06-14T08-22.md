# Flag-and-Stop Gap — AppFileSystemFolderPaths.MatchBestSpecialFolder (P3-T2)

Timestamp: 2026-06-14T08-22

Task: [P3-T2] MatchBestSpecialFolder tests (TaskMaster.Test)

## Summary

`AppFileSystemFolderPaths.MatchBestSpecialFolder(string)` cannot be exercised as "pure LINQ/string
matching; no filesystem read" without either mutating the local filesystem (prohibited by the
no-temp-files / no-filesystem-mutation test constraints) or introducing a new production seam
(prohibited as a silent edit). Per the feature Flag-and-Stop rule, the method is intentionally NOT
covered and no production change is made.

## Why it is not reachable in isolation

- The method (AppFileSystemFolderPaths.cs line 56) reads the `SpecialFolders` ConcurrentDictionary
  and returns the Key of the longest Value contained in the input path (or null when SpecialFolders
  is null/empty).
- `SpecialFolders` has only a `protected` setter (line 289). The only accessible constructors are
  the public parameterless `AppFileSystemFolderPaths()` (line 14) and `public Reload()` (line 251);
  BOTH call `LoadFolders()`.
- `LoadFolders()` (line 127) reads many `Environment.GetFolderPath(...)` / environment variables and
  calls `CreateMissingPaths` -> `Directory.CreateDirectory` for each special folder, including
  derived paths ("Flow", "PreReads", "PythonStaging") that combine OneDrive roots and may not exist
  on the machine. That is a real filesystem WRITE, which the General + C# Unit Test Policy prohibits
  in unit tests (no temp files; isolate I/O; deterministic).
- The only constructor that skips LoadFolders is `private AppFileSystemFolderPaths(bool async)`
  (line 26), which is inaccessible to TaskMaster.Test.
- A test subclass could set `SpecialFolders` via the protected setter, but the base constructor's
  `LoadFolders()` has already executed (and mutated the filesystem) before the subclass body runs.
  There is no construction path that avoids LoadFolders.

## What a fix would require (out of scope)

An `internal` LoadFolders-free constructor or an `internal` settable `SpecialFolders` seam reachable
via `InternalsVisibleTo("TaskMaster.Test")`. Adding either is a production change and a scope change;
it is flagged here for maintainer direction rather than performed.

## Disposition

Coverage gap for MatchBestSpecialFolder is accepted under the Flag-and-Stop rule. The other two
Increment 3 targets (AppStagingFilenames and the remaining AppQuickFilerSettings properties) are
fully covered via the Settings.Default snapshot/restore pattern without any production change.

Note: the SpecialFolders==null/empty early-return branch of MatchBestSpecialFolder is likewise not
reachable without constructing the instance, and is included in this gap.
