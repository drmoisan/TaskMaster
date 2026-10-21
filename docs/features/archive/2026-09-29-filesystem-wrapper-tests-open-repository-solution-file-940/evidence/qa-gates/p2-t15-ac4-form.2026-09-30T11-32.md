# AC4 Member Forms (P2-T15)

Timestamp: 2026-09-30T11-32
Task: P2-T15
Command: Read tool over the two formatted Write Set files and test-run-final.md; no command executed
EXIT_CODE: 0
Output Summary: fifteen FORM rows, each using one of the four form tokens; every named test reads Passed in FEATURE/evidence/regression-testing/test-run-final.md; the no-op members Create, Refresh and SetAccessControl each appear in a no-op-no-throw row; every member group keeps at least one P2-T8 census token at or above its bound. The per-file coverage evidence of D-8 (P2-T7 MEASUREMENT 3: the three final3 FILE lines NOT-LOWER=True on covered lines and covered branches) also holds.

- FORM: PDA | getters Exists, Attributes, CreationTime, CreationTimeUtc, LastAccessTime, LastAccessTimeUtc, LastWriteTime, LastWriteTimeUtc, Extension, FullName, Name, Parent, Root; GetAccessControl(), GetAccessControl(sections); GetObjectData; ToString | PhysicalDirectoryInfoAdapter_AccessorsAndNoOpCreation_MirrorOwnedDirectory | owned read-only fixture
  - EVIDENCE: RESULT PhysicalDirectoryInfoAdapter_AccessorsAndNoOpCreation_MirrorOwnedDirectory = Passed; P2-T8 PFS TOKEN .GetAccessControl( = 6, .GetObjectData( = 2, .MemberCount = 2, .ToString() = 4, .Root = 2
- FORM: PDA | Create(), Create(security), CreateSubdirectory(name), CreateSubdirectory(name, security), Refresh(), SetAccessControl(security) | PhysicalDirectoryInfoAdapter_AccessorsAndNoOpCreation_MirrorOwnedDirectory | no-op-no-throw
  - EVIDENCE: RESULT PhysicalDirectoryInfoAdapter_AccessorsAndNoOpCreation_MirrorOwnedDirectory = Passed; P2-T8 PFS TOKEN .Create( = 2, .CreateSubdirectory( = 2, .Refresh() = 4, .SetAccessControl( = 2
- FORM: PDA | the 18 enumeration overloads (EnumerateDirectories, EnumerateFiles, EnumerateFileSystemInfos, GetDirectories, GetFiles, GetFileSystemInfos, three overloads each) | PhysicalDirectoryInfoAdapter_Enumeration_WrapsOwnedDirectoryEntries | owned read-only fixture
  - EVIDENCE: RESULT PhysicalDirectoryInfoAdapter_Enumeration_WrapsOwnedDirectoryEntries = Passed; P2-T8 PFS TOKEN .EnumerateDirectories( = 3, .EnumerateFiles( = 3, .EnumerateFileSystemInfos( = 3, .GetDirectories( = 3, .GetFiles( = 3, .GetFileSystemInfos( = 3
- FORM: PDA | setters Attributes, CreationTime, CreationTimeUtc, LastAccessTime, LastAccessTimeUtc, LastWriteTime, LastWriteTimeUtc | PhysicalDirectoryInfoAdapter_SettersOnMissingDirectory_ThrowWithoutCreatingEntries | non-existent-path outcome
  - EVIDENCE: RESULT PhysicalDirectoryInfoAdapter_SettersOnMissingDirectory_ThrowWithoutCreatingEntries = Passed; P2-T8 PFS TOKEN Throw<FileNotFoundException>() = 20, Exists.Should().BeFalse() = 6, .CreationTimeUtc = 6
- FORM: PDA | Delete(), Delete(true), MoveTo | PhysicalDirectoryInfoAdapter_MissingDirectoryAndUnsupportedInfo_BranchesBehaveAsExpected | non-existent-path outcome
  - EVIDENCE: RESULT PhysicalDirectoryInfoAdapter_MissingDirectoryAndUnsupportedInfo_BranchesBehaveAsExpected = Passed; P2-T8 PFS TOKEN .Delete() = 2, .Delete(recursive: true) = 1, .MoveTo( = 2, Throw<DirectoryNotFoundException>() = 3
- FORM: PDA | WrapFileSystemInfo unsupported branch (the test-owned UnsupportedInfo) | PhysicalDirectoryInfoAdapter_MissingDirectoryAndUnsupportedInfo_BranchesBehaveAsExpected | owned read-only fixture
  - EVIDENCE: RESULT PhysicalDirectoryInfoAdapter_MissingDirectoryAndUnsupportedInfo_BranchesBehaveAsExpected = Passed; P2-T8 PFS TOKEN [TestMethod] = 7 (the method is present); MissingOwnedPath( = 10
- FORM: PFA | getters Exists, Extension, FullName, Name, Directory, DirectoryName, Length, IsReadOnly, Attributes, the six timestamps; GetAccessControl(), GetAccessControl(sections); GetObjectData; ToString; Open(mode, access, share), OpenRead(), OpenText() | PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo | owned read-only fixture
  - EVIDENCE: RESULT PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo = Passed; P2-T8 PFS TOKEN .Length = 1, .IsReadOnly = 3, .OpenRead() = 1, .OpenText() = 1, .Open( = 3, .MemberCount = 2
- FORM: PFA | AppendText(), Open(mode), Open(mode, access), OpenWrite() through the internal delegate seam with test-owned sentinel streams | PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo | owned read-only fixture
  - EVIDENCE: RESULT PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo = Passed; P2-T8 PFS TOKEN .AppendText() = 1, .OpenWrite() = 1, .Open( = 3
- FORM: PFA | Refresh(), SetAccessControl(security) | PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo | no-op-no-throw
  - EVIDENCE: RESULT PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo = Passed; P2-T8 PFS TOKEN .Refresh() = 4, .SetAccessControl( = 2
- FORM: PFA | setters IsReadOnly, Attributes, the six timestamps | PhysicalFileInfoAdapter_SettersOnMissingFile_ThrowWithoutCreatingFiles | non-existent-path outcome
  - EVIDENCE: RESULT PhysicalFileInfoAdapter_SettersOnMissingFile_ThrowWithoutCreatingFiles = Passed; P2-T8 PFS TOKEN Throw<FileNotFoundException>() = 20, .IsReadOnly = 3, .Attributes = 6
- FORM: PFA | Delete() (documented no-throw on a missing file), CopyTo(target), CopyTo(target, overwrite), MoveTo, Replace(target, backup), Replace(target, backup, ignoreMetadataErrors) | PhysicalFileInfoAdapter_MissingFileBranches_ThrowOrNoOpWithoutCreatingFiles | non-existent-path outcome
  - EVIDENCE: RESULT PhysicalFileInfoAdapter_MissingFileBranches_ThrowOrNoOpWithoutCreatingFiles = Passed; P2-T8 PFS TOKEN .CopyTo( = 2, .Replace( = 2, .MoveTo( = 2, .Delete() = 2
- FORM: DIWP | Exists, FullName, Name, Extension, Parent, Root (the rooted test-owned literal) | Properties_ShouldMirrorWrappedDirectoryInfo | owned read-only fixture
  - EVIDENCE: RESULT Properties_ShouldMirrorWrappedDirectoryInfo = Passed; P2-T8 DIW TOKEN RootedFixturePath = 3, new DirectoryInfo(RootedFixturePath) = 2, .Parent = 6, .Root = 4
- FORM: DIWP | ToString (the same rooted literal) | ToString_ShouldDelegateToWrappedDirectoryInfo | owned read-only fixture
  - EVIDENCE: RESULT ToString_ShouldDelegateToWrappedDirectoryInfo = Passed; P2-T8 DIW TOKEN .ToString() = 4
- FORM: DIWP | GetFiles(), GetDirectories(), EnumerateFileSystemInfos() (the owned directory and its parent) | GetDirectoriesAndGetFiles_ShouldReturnWrappedEntries, EnumerateFileSystemInfos_ShouldWrapDirectoriesAndFiles | owned read-only fixture
  - EVIDENCE: RESULT GetDirectoriesAndGetFiles_ShouldReturnWrappedEntries = Passed; RESULT EnumerateFileSystemInfos_ShouldWrapDirectoriesAndFiles = Passed; P2-T8 DIW TOKEN .GetFiles( = 7, .GetDirectories( = 7, .EnumerateFileSystemInfos( = 8
- FORM: DIWP | every member the three Moq-based tests exercise (unchanged by this item) | PropertyDelegates_ShouldMirrorMockedIDirectoryInfo, EnumerationAndArrayMethods_ShouldDelegateToWrappedIDirectoryInfo, LifecycleAndAccessControlMethods_ShouldDelegateToWrappedIDirectoryInfo | Moq
  - EVIDENCE: RESULT PropertyDelegates_ShouldMirrorMockedIDirectoryInfo = Passed; RESULT EnumerationAndArrayMethods_ShouldDelegateToWrappedIDirectoryInfo = Passed; RESULT LifecycleAndAccessControlMethods_ShouldDelegateToWrappedIDirectoryInfo = Passed; P2-T8 DIW TOKEN using Moq; = 1, .Create( = 4, .Refresh() = 2, .SetAccessControl( = 2

Acceptance for the forms:

1. exactly fifteen FORM lines - met.
2. every form is one of Moq, owned read-only fixture, non-existent-path outcome, no-op-no-throw - met.
3. every named test reads Passed in test-run-final.md - met.
4. Create, Refresh and SetAccessControl each appear in a no-op-no-throw row - met (PDA row 2 and PFA row 9).

AC4: MET (census lower bounds held in P2-T8, including .MemberCount 2; P2-T7 MEASUREMENT 3 FILE lines NOT-LOWER=True for all three files; P2-T5 fifteen tests Passed).
