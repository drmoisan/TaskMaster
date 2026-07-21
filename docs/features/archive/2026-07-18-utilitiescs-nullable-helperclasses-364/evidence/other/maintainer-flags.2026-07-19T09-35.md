# Maintainer Flags — FileSystem Adapter Root Boundaries (Issue #364, Batch 4)

- Timestamp: 2026-07-19T09-35
- Task: [P4-T7]

## (a) Behavior-preserving `!` at adapter/wrapper root boundaries

The following members pass a BCL `DirectoryInfo?`/`FileInfo?` (null at the filesystem root) into a `*Wrapper` constructor that throws `ArgumentNullException` on null, or return a nullable BCL value as the non-null interface contract. The implemented FileSystem interfaces (`IDirectoryInfo`, `IFileInfo`) live under `UtilitiesCS/Interfaces/IHelperClasses/` and are OUT of scope (oblivious). A behavior-preserving `!` (with a `// why` comment) is therefore the correct annotation; it does not change the contract:

- `PhysicalDirectoryInfoAdapter.Parent` → `new DirectoryInfoWrapper(_directoryInfo.Parent!)`
- `PhysicalDirectoryInfoAdapter.Root` → unchanged (`DirectoryInfo.Root` is non-null; no `!` needed)
- `PhysicalFileInfoAdapter.Directory` → `new DirectoryInfoWrapper(_fileInfo.Directory!)`
- `PhysicalFileInfoAdapter.DirectoryName` → `_fileInfo.DirectoryName!`

`DirectoryInfoWrapper` and `FileInfoWrapper` delegate to the inner (oblivious) interface, so their corresponding members required no `!`.

## (b) FLAGGED (not fixed): latent root-throws behavior

Accessing `Parent`/`Directory`/`DirectoryName` on a filesystem-root path throws `ArgumentNullException` at runtime (the BCL value is null and the wrapper ctor rejects null). This is a pre-existing, latent design question surfaced — but NOT changed — by the nullable annotation. Making the members nullable would be a contract change and is blocked from rippling to the out-of-scope oblivious interface. This is FLAGGED for a possible future issue; it is not fixed in issue #364 (annotation-only scope).

## Seam preservation confirmation (PhysicalFileInfoAdapter)

The `PhysicalFileInfoAdapter` injectable-delegate seam (`_appendText`/`_openByMode`/`_openByModeAndAccess`/`_openWrite` fields, both constructors, and the `?? throw` guards) is byte-unchanged except that a file-level `#nullable enable` pragma and the two root-boundary annotations above were added. `git diff` confirms no `+`/`-` line touches the seam fields, constructors, or `?? throw` guards. The `PhysicalFileSystemAdapters_Tests` determinism seam is not perturbed.
