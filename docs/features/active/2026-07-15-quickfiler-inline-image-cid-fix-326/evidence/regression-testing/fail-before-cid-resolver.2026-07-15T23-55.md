# Fail-Before Evidence — CidImageResolver / IAttachment.ContentId — P1-T3 [expect-fail]

- **Timestamp:** 2026-07-15T23-55
- **Command:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`
  (invoked in git-bash as `MSYS_NO_PATHCONV=1 msbuild.exe TaskMaster.sln -t:Build
  -p:Configuration=Debug -p:Platform="Any CPU" -nologo -v:minimal` to avoid git-bash POSIX
  path-mangling of `/`-prefixed MSBuild switches; semantically identical command.)
- **EXIT_CODE:** 1 (non-zero, expected failure)
- **Output Summary:** Build failed with 7 compiler errors, all originating from the newly-added
  `UtilitiesCS.Test/OutlookObjects/MailItem/CidImageResolverTests.cs`, referencing the not-yet-existing
  `CidImageResolver` type and the not-yet-existing `IAttachment.ContentId` / `AttachmentSerializable.ContentId`
  member:

```
CidImageResolverTests.cs(22,17): error CS0117: 'AttachmentSerializable' does not contain a definition for 'ContentId'
CidImageResolverTests.cs(27,26): error CS0103: The name 'CidImageResolver' does not exist in the current context
CidImageResolverTests.cs(45,17): error CS0117: 'AttachmentSerializable' does not contain a definition for 'ContentId'
CidImageResolverTests.cs(50,26): error CS0103: The name 'CidImageResolver' does not exist in the current context
CidImageResolverTests.cs(64,58): error CS0117: 'AttachmentSerializable' does not contain a definition for 'ContentId'
CidImageResolverTests.cs(65,54): error CS0117: 'AttachmentSerializable' does not contain a definition for 'ContentId'
CidImageResolverTests.cs(66,62): error CS0117: 'AttachmentSerializable' does not contain a definition for 'ContentId'
CidImageResolverTests.cs(69,23): error CS0103: The name 'CidImageResolver' does not exist in the current context
```

This is the auditable fail-before evidence for AC-1 through AC-3 (the three `CidImageResolver` tests
in `CidImageResolverTests.cs`), confirming the regression test was written and observed to fail before
the fix, per the repo's mandatory Bugfix Workflow.
