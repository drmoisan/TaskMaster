# AC3 Mutating-Call Inventory (P2-T14)

Timestamp: 2026-09-30T11-31
Task: P2-T14
Command: Read tool over the two formatted Write Set files; no command executed
EXIT_CODE: 0 (scoped to the read-only derivation)
Output Summary: PFS carries 34 inventoried mutating call sites (owned output directory 4, its parent 2, existing owned entry 2, missing owned path 26, mock 0), equal to the plan's expected inventory; every DIW call site (16) lies in one of the three Moq-based tests and targets a wrapper built over `Mock<IDirectoryInfo>.Object`; no call names the repository root, a tracked repository path or the solution file. Mock `Setup` and `SetupSet` expressions configure the mock and are not calls on the object under test, so they are not inventoried. Every missing-owned-path site is preceded in its method by an `adapter.Exists.Should().BeFalse()` precondition; the file copy, move, replace and backup destinations by `File.Exists(...).Should().BeFalse()`; the directory move destination by `Directory.Exists(directoryMoveTarget).Should().BeFalse()`.

MUTATING-CALL-INVENTORY:

PFS (UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs):

- CALL: PhysicalDirectoryInfoAdapter_AccessorsAndNoOpCreation_MirrorOwnedDirectory | adapter.Create() | owned output directory
- CALL: PhysicalDirectoryInfoAdapter_AccessorsAndNoOpCreation_MirrorOwnedDirectory | adapter.Create(directory.GetAccessControl()) | owned output directory
- CALL: PhysicalDirectoryInfoAdapter_AccessorsAndNoOpCreation_MirrorOwnedDirectory | parentAdapter.CreateSubdirectory(directory.Name) | its parent
- CALL: PhysicalDirectoryInfoAdapter_AccessorsAndNoOpCreation_MirrorOwnedDirectory | parentAdapter.CreateSubdirectory(directory.Name, parent.GetAccessControl()) | its parent
- CALL: PhysicalDirectoryInfoAdapter_AccessorsAndNoOpCreation_MirrorOwnedDirectory | adapter.Refresh() | owned output directory
- CALL: PhysicalDirectoryInfoAdapter_AccessorsAndNoOpCreation_MirrorOwnedDirectory | adapter.SetAccessControl(security) | owned output directory
- CALL: PhysicalDirectoryInfoAdapter_SettersOnMissingDirectory_ThrowWithoutCreatingEntries | adapter.CreationTime = stamp | missing owned path
- CALL: PhysicalDirectoryInfoAdapter_SettersOnMissingDirectory_ThrowWithoutCreatingEntries | adapter.CreationTimeUtc = stamp | missing owned path
- CALL: PhysicalDirectoryInfoAdapter_SettersOnMissingDirectory_ThrowWithoutCreatingEntries | adapter.LastAccessTime = stamp | missing owned path
- CALL: PhysicalDirectoryInfoAdapter_SettersOnMissingDirectory_ThrowWithoutCreatingEntries | adapter.LastAccessTimeUtc = stamp | missing owned path
- CALL: PhysicalDirectoryInfoAdapter_SettersOnMissingDirectory_ThrowWithoutCreatingEntries | adapter.LastWriteTime = stamp | missing owned path
- CALL: PhysicalDirectoryInfoAdapter_SettersOnMissingDirectory_ThrowWithoutCreatingEntries | adapter.LastWriteTimeUtc = stamp | missing owned path
- CALL: PhysicalDirectoryInfoAdapter_SettersOnMissingDirectory_ThrowWithoutCreatingEntries | adapter.Attributes = FileAttributes.Directory | missing owned path
- CALL: PhysicalDirectoryInfoAdapter_SettersOnMissingDirectory_ThrowWithoutCreatingEntries | adapter.Refresh() | missing owned path
- CALL: PhysicalDirectoryInfoAdapter_MissingDirectoryAndUnsupportedInfo_BranchesBehaveAsExpected | adapter.Delete() | missing owned path
- CALL: PhysicalDirectoryInfoAdapter_MissingDirectoryAndUnsupportedInfo_BranchesBehaveAsExpected | adapter.Delete(recursive: true) | missing owned path
- CALL: PhysicalDirectoryInfoAdapter_MissingDirectoryAndUnsupportedInfo_BranchesBehaveAsExpected | adapter.MoveTo(directoryMoveTarget) | missing owned path
- CALL: PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo | adapter.Refresh() | existing owned entry
- CALL: PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo | adapter.SetAccessControl(security) | existing owned entry
- CALL: PhysicalFileInfoAdapter_SettersOnMissingFile_ThrowWithoutCreatingFiles | adapter.IsReadOnly = true | missing owned path
- CALL: PhysicalFileInfoAdapter_SettersOnMissingFile_ThrowWithoutCreatingFiles | adapter.Attributes = FileAttributes.Normal | missing owned path
- CALL: PhysicalFileInfoAdapter_SettersOnMissingFile_ThrowWithoutCreatingFiles | adapter.CreationTime = stamp | missing owned path
- CALL: PhysicalFileInfoAdapter_SettersOnMissingFile_ThrowWithoutCreatingFiles | adapter.CreationTimeUtc = stamp | missing owned path
- CALL: PhysicalFileInfoAdapter_SettersOnMissingFile_ThrowWithoutCreatingFiles | adapter.LastAccessTime = stamp | missing owned path
- CALL: PhysicalFileInfoAdapter_SettersOnMissingFile_ThrowWithoutCreatingFiles | adapter.LastAccessTimeUtc = stamp | missing owned path
- CALL: PhysicalFileInfoAdapter_SettersOnMissingFile_ThrowWithoutCreatingFiles | adapter.LastWriteTime = stamp | missing owned path
- CALL: PhysicalFileInfoAdapter_SettersOnMissingFile_ThrowWithoutCreatingFiles | adapter.LastWriteTimeUtc = stamp | missing owned path
- CALL: PhysicalFileInfoAdapter_SettersOnMissingFile_ThrowWithoutCreatingFiles | adapter.Refresh() | missing owned path
- CALL: PhysicalFileInfoAdapter_MissingFileBranches_ThrowOrNoOpWithoutCreatingFiles | adapter.Delete() | missing owned path
- CALL: PhysicalFileInfoAdapter_MissingFileBranches_ThrowOrNoOpWithoutCreatingFiles | adapter.CopyTo(copyTarget) | missing owned path
- CALL: PhysicalFileInfoAdapter_MissingFileBranches_ThrowOrNoOpWithoutCreatingFiles | adapter.CopyTo(copyTarget, overwrite: true) | missing owned path
- CALL: PhysicalFileInfoAdapter_MissingFileBranches_ThrowOrNoOpWithoutCreatingFiles | adapter.MoveTo(moveTarget) | missing owned path
- CALL: PhysicalFileInfoAdapter_MissingFileBranches_ThrowOrNoOpWithoutCreatingFiles | adapter.Replace(replaceTarget, backupTarget) | missing owned path
- CALL: PhysicalFileInfoAdapter_MissingFileBranches_ThrowOrNoOpWithoutCreatingFiles | adapter.Replace(replaceTarget, backupTarget, ignoreMetadataErrors: true) | missing owned path

DIW (UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs):

- CALL: PropertyDelegates_ShouldMirrorMockedIDirectoryInfo | wrapper.Attributes = FileAttributes.ReadOnly | mock
- CALL: PropertyDelegates_ShouldMirrorMockedIDirectoryInfo | wrapper.CreationTime = nextLocal | mock
- CALL: PropertyDelegates_ShouldMirrorMockedIDirectoryInfo | wrapper.CreationTimeUtc = nextUtc | mock
- CALL: PropertyDelegates_ShouldMirrorMockedIDirectoryInfo | wrapper.LastAccessTime = nextAccessLocal | mock
- CALL: PropertyDelegates_ShouldMirrorMockedIDirectoryInfo | wrapper.LastAccessTimeUtc = nextAccessUtc | mock
- CALL: PropertyDelegates_ShouldMirrorMockedIDirectoryInfo | wrapper.LastWriteTime = nextWriteLocal | mock
- CALL: PropertyDelegates_ShouldMirrorMockedIDirectoryInfo | wrapper.LastWriteTimeUtc = nextWriteUtc | mock
- CALL: EnumerationAndArrayMethods_ShouldDelegateToWrappedIDirectoryInfo | wrapper.CreateSubdirectory("child") | mock
- CALL: EnumerationAndArrayMethods_ShouldDelegateToWrappedIDirectoryInfo | wrapper.CreateSubdirectory("child", security) | mock
- CALL: LifecycleAndAccessControlMethods_ShouldDelegateToWrappedIDirectoryInfo | wrapper.Create() | mock
- CALL: LifecycleAndAccessControlMethods_ShouldDelegateToWrappedIDirectoryInfo | wrapper.Create(directorySecurity) | mock
- CALL: LifecycleAndAccessControlMethods_ShouldDelegateToWrappedIDirectoryInfo | wrapper.Delete() | mock
- CALL: LifecycleAndAccessControlMethods_ShouldDelegateToWrappedIDirectoryInfo | wrapper.Delete(recursive: true) | mock
- CALL: LifecycleAndAccessControlMethods_ShouldDelegateToWrappedIDirectoryInfo | wrapper.MoveTo("moved") | mock
- CALL: LifecycleAndAccessControlMethods_ShouldDelegateToWrappedIDirectoryInfo | wrapper.Refresh() | mock
- CALL: LifecycleAndAccessControlMethods_ShouldDelegateToWrappedIDirectoryInfo | wrapper.SetAccessControl(directorySecurity) | mock

- PFS-MUTATING-CALLS: 34
- DIW-MUTATING-CALLS: 16
- CLASS-COUNT owned output directory = 4
- CLASS-COUNT its parent = 2
- CLASS-COUNT existing owned entry = 2
- CLASS-COUNT missing owned path = 26
- CLASS-COUNT mock = 0
- NON-INVENTORIED: seamAdapter.AppendText(), seamAdapter.Open(FileMode.Open), seamAdapter.Open(FileMode.Open, FileAccess.Read), seamAdapter.OpenWrite() (PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo; routed through the adapter's internal delegate seam to test-owned sentinel streams, so no file-system call is issued)

Acceptance for the inventory:

1. PFS-MUTATING-CALLS: 34 with CLASS-COUNT owned output directory 4, its parent 2, existing owned entry 2, missing owned path 26, mock 0 - met.
2. every DIW CALL line has class mock and a method among the three Moq-based names, with DIW-MUTATING-CALLS 16 (at least 7) - met.
3. every class is one of the five tokens - met.
4. no CALL line names the repository root, a tracked repository path or the solution file - met.

AC3 evidence also held: P2-T8 census (PFS `new DirectoryInfo(` 2 equal to `new DirectoryInfo(MissingOwnedPath(` 2; `new FileInfo(` 3 equal to `new FileInfo(MissingOwnedPath(` 2 plus `new FileInfo(typeof(` 1; `Exists.Should().BeFalse()` 6; `MissingOwnedPath(` 10; `__940_missing_` 1; `GetRepositoryRoot` 0; the solution-file literal 0; DIW `new DirectoryInfo(` 2 equal to `new DirectoryInfo(RootedFixturePath)` 2; `new FileInfo(` 1 equal to `new FileInfo(typeof(` 1), P2-T10 (`ADDED-ROOTWALK: 0`, `ADDED-TEMP: 0`) and P2-T5 (the four missing-path tests Passed). AC3: MET.
