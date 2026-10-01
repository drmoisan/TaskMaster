# Research: SortEmail.TrySaveAttachmentAsync test creates a directory at the repository root (Issue #945)

- Date: 2026-09-30
- Work mode: minor-audit (no spec.md or user-story.md by design)
- Worktree branch: bug/sort-email-attachment-test-creates-directory-945
- Method: every claim below was re-read in this worktree. `[V]` = verified by reading the cited lines with a tool in this session; `[P]` = predicted, not executed (no build or test run was performed; this is research only).
- Sibling: #940 research (same defect class) supplied section 5 row and section 6 route facts; script paths were re-checked here (section 7).

## 1. Current state

### 1.1 Defective test `[V]`

`UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` lines 236-249:

- L237 `public async Task TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile()`.
- L240 `CreateAttachmentMock("saved.txt", OlAttachmentType.olByValue)` (a `MockBehavior.Loose` `Mock<Attachment>`, helper at 346-366).
- L241 `var destinationPath = Path.Combine(GetRepositoryRoot().FullName, "saved.txt");`
- L244 `bool saved = await attachment.Object.TrySaveAttachmentAsync(destinationPath);`
- L247-248 asserts `saved` is true and `SaveAsFile(destinationPath)` was called once.

The directory passed to `Directory.CreateDirectory` is the repository root, which already exists, so the call is a no-op today. The defect is that a real creation API is reached; a rooted literal path without a seam would create a directory on disk.

### 1.2 Production method `[V]`

`UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` (`#nullable enable` at L1; `public static class SortEmail` at L23):

- L888 `[ExcludeFromCodeCoverage]`; L889-892 `internal static async Task<bool> TrySaveAttachmentAsync(this Attachment attachment, string filePathSave)`.
- Branch A, try body (L894-899): L896 `System.IO.Directory.CreateDirectory(Path.GetDirectoryName(filePathSave));` then L897 `await Task.Run(() => attachment.SaveAsFile(filePathSave));` then L898 `return true;`.
- Branch B, `catch (System.UnauthorizedAccessException e)` (L900-954):
  - L902 `Debug.WriteLine`.
  - L907-912: if static `_removeReadOnly == Empty`, L911 `_removeReadOnly = YesNoToAll.ShowDialog(message)` (a WinForms dialog: the UI hazard).
  - L914-937 (Yes / YesToAll): L919 `new DirectoryInfo(...)`, L922 `di.Attributes &= ~ReadOnly` (a real attribute write), inner catch returns false (L924-928), `finally` resets `_removeReadOnly` to Empty when it is `Yes` (L929-935), then the retry at L936 `return await TrySaveAttachmentAsync(attachment, filePathSave);` (the only self-recursion).
  - L938-949 (No / NoToAll): logs, resets on `No`, `return false`.
  - L950-953: any other value, `throw;`.
- Branch C, `catch (System.Exception) { throw; }` (L955-958): any non-UnauthorizedAccess exception (including one thrown by the directory-creation step) propagates unchanged.
- Static mutable state `[V]`: `private static YesNoToAllResponse _attachmentsOverwrite / _attachmentsAltName / _picturesOverwrite / _removeReadOnly` at L625-628; `Cleanup_Files` resets them (assignment at L560).
- The dialog is reached only when the try body throws `UnauthorizedAccessException` and `_removeReadOnly` is `Empty`. No test may drive that: with a seam that throws only non-`UnauthorizedAccessException` types, Branch B is unreachable.

### 1.3 Existing file-system seams `[V]`

- No `IFileSystem` (or `IDirectory`) type exists anywhere in the repository (`\bIFileSystem\b` count 0 across `*.cs`). The issue's phrase "existing IFileSystem-style seams" does not match a directory-creation interface. What exists: `IDirectoryInfo`, `IFileInfo`, `IFileSystemInfo` (`UtilitiesCS/Interfaces/IHelperClasses/`), `DirectoryInfoWrapper`, `FileSystemInfoWrapper`, `PhysicalDirectoryInfoAdapter`, `PhysicalFileInfoAdapter` (`UtilitiesCS/HelperClasses/FileSystem/`). These wrap an existing `DirectoryInfo`; none exposes a create-by-path operation usable before the directory exists (`IDirectoryInfo.Create()` requires constructing a `DirectoryInfo` for the path first, which is not a path-string seam).
- Closest in-repo precedent for the exact call `[V]`: `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.Serialization.cs` L145-174: an instance `internal virtual` wrapper passes `path => Directory.CreateDirectory(path)` (L155) into an `internal static` overload taking `Action<string> createDirectory` (L164) which calls `createDirectory(disk.FolderPath)` (L168). The lambda form is required because `Directory.CreateDirectory` returns `DirectoryInfo`, so a method group does not convert to `Action<string>`.
- Other delegate-seam precedent: `OneDriveDownloader` `Func<string, Stream>` (L106-133), `FileIO2` `Func<string, TextWriter>` (L88-100), `AssemblyBindingFallback` `Func<string,bool> _fileExists` (L188).
- `SortEmail.cs` itself contains no `Func<>`/`Action<>` seam and no `IFileSystem` use `[V]` (grep for `Func<|Action<|IFileSystem|IDirectory` returned no seam member).

## 2. Callers `[V]`

`TrySaveAttachmentAsync` (grep over all `*.cs`, then cross-checked by a files-with-matches search over `*.cs`, `*.vb`, `*.ps1`, `*.md`):

| Location | Kind |
|---|---|
| `SortEmail.cs` L819 (`SaveAttachmentAsync(this AttachmentHelper)`, `else` branch when `File.Exists` is false) | production |
| `SortEmail.cs` L864 (`SaveCaseAsync`, alternate-name save) | production |
| `SortEmail.cs` L879 (`SaveCaseAsync`, YesToAll/Yes overwrite save) | production |
| `SortEmail.cs` L936 (self-recursive retry inside the method) | production |
| `SortEmail_Tests.cs` L244 | test (the defective test; the only test caller) |

Other matches from the wider pattern set: `EmailFiler.cs` L281/L443-445 calls `SaveAttachmentAsync` (the `AttachmentHelper` extension), which reaches `TrySaveAttachmentAsync` transitively; the only test-side reference is the override at `EmailFiler_TestSupport.cs` L266 (`protected internal override Task SaveAttachmentAsync(AttachmentHelper)`), which replaces the method and does not call `SortEmail`. No test calls `SaveAttachment`, `SaveAttachmentAsync`, `SaveCase`, or `SaveCaseAsync` on `SortEmail` directly. Conclusion: the defective test is the only test in the repository that reaches `Directory.CreateDirectory` through `SortEmail`. The remaining `Directory.CreateDirectory` sites under `UtilitiesCS/EmailIntelligence` are `EmailDataMiner.Serialization.cs` L155 (already a seam default) and L185 (`[ExcludeFromCodeCoverage]`, separate class), and `ClassifierGroupUtilities.cs` L171 (out of scope).

## 3. The other four `GetRepositoryRoot()` uses `[V]`

Test file L196, L223, L255, L280 (helper defined L393-415; walks up from the test assembly to `TaskMaster.sln`, a read-only `File.Exists` probe):

| Line | Test | Production path | Real FS write or create? |
|---|---|---|---|
| 196 | `GetAttachmentsInfo_...FiltersOutDocumentsAndOleAttachments` | `SortEmail.GetAttachmentsInfo` L637-659 builds `AttachmentHelper` objects; `Init` (AttachmentHelper.cs L61-124) uses `AdjustForMaxPath` (L216-234, `Path.Combine` only), `PrependDatePrefix`, `GetNameSuffix` (string formatting). Filter reads `AttachmentInfo.IsImage` (`FileExtension` list lookup, AttachmentSerializable.cs L229). The lazy `AttachmentData` (L109-117, file reads at L151/L177-178) is not evaluated. | No |
| 223 | `GetAttachmentsInfoAsync_...` | L662-699, same `AttachmentHelper.CreateAsync` -> `Init` path | No |
| 255 | `SaveMessageAsMsgAsync_...` | L1081-1089 `AdjustForMaxPath` (string) then strict-mock `mailItem.SaveAs` | No |
| 280 | `SaveMessageAsMSG_...` | L1092-1098 same | No |

Verdict: none of the four creates or writes on disk (consistent with #940 section 5 row 3). They are out of scope for #945 (the issue is about creation; the parent directs not to widen scope). `GetRepositoryRoot()` therefore must remain, and its `using System.IO`/`AppDomain`-based body (L393-415) stays. The fixed test will no longer call it; the four remaining callers keep it in use, so no dead helper is left behind. Layout dependence on those four sites is a hygiene follow-up, not part of this item.

## 4. Candidate seam designs and recommendation

Constraints: `SortEmail` is a static class with static mutable state; tests run in parallel (`TaskMaster.runsettings` L5-6: `Workers` 0, `Scope` ClassLevel `[V]`); `.claude/rules/csharp.md` "DI Seams" ranks interface seam > injectable delegate seam > adapter seam, "smallest seam that enables reliable unit testing".

- (a) **Internal overload taking `Action<string> createDirectory`; the existing 2-parameter signature delegates to it with `path => System.IO.Directory.CreateDirectory(path)`.** Delegate seam (rung 2), mirrors the in-repo precedent in section 1.3, keeps all 4 production call sites source-compatible, holds no shared state, so it is parallel-safe. The retry recursion at L936 must pass the same delegate through so the retry uses the injected seam.
- (b) **Existing interface seam:** none applies (section 1.3). A new narrow `IDirectoryCreator`/`IFileSystem` interface would be rung 1 but adds a new type and a static-class injection point for one call; it fails "smallest seam" and would still need a static default or a parameter, ending at the same shape as (a) plus an interface.
- (c) **Static settable delegate** (for example `internal static Action<string> CreateDirectory`): rejected. Test assignment mutates process-wide state; ClassLevel parallelism runs other test classes concurrently, and any test that reaches the save path in another class would observe the fake or race the restore; it also violates UT4 (no reliance on mutable global state).
- (d) **Optional parameter `Action<string>? createDirectory = null` on the existing method:** viable (all four production sites are invocations, not method-group conversions), but changes the shape of an existing signature and leaves a nullable default branch; the overload (a) has no such branch and matches the precedent.

**Recommendation: (a).** Sketch (the executor writes final code; do not treat as verbatim):

- Existing `internal static async Task<bool> TrySaveAttachmentAsync(this Attachment attachment, string filePathSave)` becomes a one-statement wrapper returning `TrySaveAttachmentAsync(attachment, filePathSave, path => System.IO.Directory.CreateDirectory(path))`. It keeps `[ExcludeFromCodeCoverage]`.
- New `internal static async Task<bool> TrySaveAttachmentAsync(this Attachment attachment, string filePathSave, Action<string> createDirectory)` holds the current body, with L896 replaced by `createDirectory(Path.GetDirectoryName(filePathSave));` and L936 by `return await TrySaveAttachmentAsync(attachment, filePathSave, createDirectory);`. Nullable-enabled file: the parameter is non-null; add a guard (`ArgumentNullException`) only if consistent with local style (the method has none today).
- The two overloads differ by arity, so there is no ambiguity for the 4 existing invocations; overload resolution with the test's 3-argument call is unambiguous `[P]`.
- Behavior is unchanged for production: identical call, identical exception flow.

### Coverage handling `[V]` rules read; outcome `[P]`

- The attribute stays on the thin wrapper (its only statement is a lambda over a real-FS call and is not unit-testable without touching disk).
- The seamed core contains the WinForms-dialog branch (`YesNoToAll.ShowDialog`, L911), a COM-adjacent/UI branch in the CLAUDE.md UT2 exemption categories (a)/(c). Two options:
  1. **Keep `[ExcludeFromCodeCoverage]` on the core too (recommended for this bug fix).** Coverage delta is zero by construction (no line moves from excluded to included), which satisfies "changed lines must not lose coverage". The new tests then guard behavior, not coverage. This is the smallest change and preserves the maintainer-ratified exemption. Caveat `[P]`: a method-level exclude on an `async` method may leave compiler-generated `d__` state-machine and lambda members in the denominator (see #457 and #455 notes); the executor must compare the UtilitiesCS package rates before and after with the same collector, and the new overload's state machine is a new member that could shift counts by a few lines either way.
  2. Remove the attribute from the core so the happy-path and exception-propagation tests are measured. Then the UnauthorizedAccess branch (about 25 executable lines, unreachable without a UI dialog or preset static) enters the denominator as uncovered lines: a drop of roughly 25 of 43423 lines (the #940 baseline figure, `[E]`, not re-measured), which conflicts with the "new methods target >= 90%" rule for the core. Not recommended unless the orchestrator wants to split the try body into its own fully covered method (a wider change than the defect warrants).
- The orchestrator should record the choice explicitly; this research recommends option 1 and a Phase 0 baseline plus a final same-tool comparison.

### UnauthorizedAccess and dialog branch

Remains unreached by tests. With the seam, the tests throw only non-`UnauthorizedAccessException` types, so Branch B is never entered. Reaching Branch B without a dialog would require presetting the private static `_removeReadOnly` by reflection to `NoToAll` (and restoring it), which mutates global state that `Cleanup_Files_DoesNotThrow` also resets and is not recommended (UT4). The dialog is never reachable from the tests below.

## 5. Rewritten tests

Using a rooted literal never touched on disk, for example directory `@"C:\Sortemail945Sandbox\attachments"` and file `@"C:\Sortemail945Sandbox\attachments\saved.txt"` (built with `Path.Combine` from literal segments, not from any repository lookup). The executor should confirm before and after the run that the literal directory does not exist (`Test-Path` via a single `pwsh -NoProfile -Command` invocation), and that the directory is absent after the tests.

**T-A (rewrite in place; name retained): `TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile`.**

- Arrange: `var events = new List<string>();` mock attachment from `CreateAttachmentMock` (Loose: property getters are set up by the helper; `SaveAsFile` records via `.Setup(x => x.SaveAsFile(destinationPath)).Callback<string>(p => events.Add("save:" + p))`); seam `path => events.Add("mkdir:" + path)`.
- Act: `await attachment.Object.TrySaveAttachmentAsync(destinationPath, createDirectory)`.
- Assert (FluentAssertions): `saved.Should().BeTrue()`; `events.Should().Equal("mkdir:" + expectedDirectory, "save:" + destinationPath)` (exact order and content proves the seam received the directory and ran before `SaveAsFile`); `attachment.Verify(x => x.SaveAsFile(destinationPath), Times.Once)`. Strict-mock alternative acceptable: `Mock<Action<string>>(MockBehavior.Strict)` with `VerifyNoOtherCalls()`; a recording list gives the ordering evidence with fewer moving parts. `SaveAsFile` runs inside `Task.Run` but is awaited, so the list write happens before assertions; a plain `List<string>` is safe because the awaiting test observes it after completion `[P]`.

**T-B (new): `TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave`.**

- Seam throws `new IOException("disk failure")`. Act via `Func<Task>`; Assert `await act.Should().ThrowAsync<IOException>()` and `attachment.Verify(x => x.SaveAsFile(It.IsAny<string>()), Times.Never)`. Reaches Branch C (`catch (System.Exception) { throw; }` at L955-958, which does not swallow) `[V]` by reading; `IOException` is not derived from `UnauthorizedAccessException`, so Branch B and the dialog are not entered `[V]`.
- Do not add a test that throws `UnauthorizedAccessException` (dialog hazard).

The 2-argument wrapper is not directly tested (it would touch the real file system); it remains exempt and is a one-line delegation. UT5 audit: independent, no temp files, no real FS, deterministic.

### Negative control (executor procedure)

- **Required control (side-effect free):** temporarily delete or comment out the `createDirectory(...)` statement in the 3-argument core. Build the test project, run the scoped filter (section 7). Expected `[P]`: T-A fails at the `events.Should().Equal(...)` assertion (no `mkdir:` entry). Record the failure text, revert with `git checkout -- UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs`... only if no other production edits are uncommitted; otherwise revert by re-applying the Edit and prove equality with the saved SHA-256 of the fixed file.
- **Optional stronger control ("bypasses the seam"):** replace the delegate call with `System.IO.Directory.CreateDirectory(Path.GetDirectoryName(filePathSave))`. T-A would then fail on the missing `mkdir:` event but would also create the literal directory on the C: drive; run only with a pre-run `Test-Path` = false, and delete the directory afterwards (via `pwsh`) and confirm absent. Because this control has a real side effect, the required control above is the gate and this one is optional.
- Each control is followed by a confirming passing run after revert.

## 6. Files and line counts `[V]`

| File | Lines | Note |
|---|---|---|
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` | 1429 (closing `}` at L1429) | Already exceeds the 500-line limit in the general policy; pre-existing and not created by this item. The change adds roughly 8-15 lines. Splitting the class is a separate refactor; note for the orchestrator that the limit is already breached and the fix must not be widened to address it. |
| `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` | 417 | Expected to remain below 500 after about +40 lines (new test, event-list plumbing, XML comments) `[P]`. |

`InternalsVisibleTo` `[V]`: `UtilitiesCS/Properties/AssemblyInfo.cs` L19 `[assembly: InternalsVisibleTo("UtilitiesCS.Test")]` (also L18 `DynamicProxyGenAssembly2`, L20 `ToDoModel.Test`, plus `HelperClasses/Tokenizer.cs` L11 and `OlItemSummary.cs` L10 repeat the `UtilitiesCS.Test` grant). The test therefore already calls the `internal` method; the new `internal` overload is equally visible. Both files are explicit `Compile Include` entries (`UtilitiesCS.csproj` L817, `UtilitiesCS.Test.csproj` L98), so no csproj edit is needed unless a new file is added (none is).

## 7. Test-run routes for the executor

Script/paths re-verified in this worktree `[V]`: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (and its `.Helpers`, `.Projection`, `.FirstParty`, `.Threshold`, `.Scope`, `.PackageRate`, `.ClosureFilter` siblings), `scripts/vscode/Invoke-MSTest.ps1`, `TaskMaster.runsettings`, and `scripts/vscode/TaskMaster.cli.runsettings` all exist. Line-number claims about those scripts in #940 section 6.2 were not re-read and are not relied on here.

Test counts (`[TestMethod]` occurrences in `SortEmail_Tests.cs`): before = 14 `[V]`. Cross-check by enumerating names: `InitializeSortToExisting_AlwaysThrows_NotImplementedException`, `..._WithExplicitArgs_StillThrows_NotImplementedException`, `SortAsync_MailHelpers_WhenNull_...`, `SortAsync_MailHelpers_WhenEmpty_...`, `StripTabsCrLf_WithControlCharacters_...`, `StripTabsCrLf_WithPlainText_...`, `Cleanup_Files_DoesNotThrow`, `GetAttachmentsInfo_...`, `GetAttachmentsInfoAsync_...`, `TrySaveAttachmentAsync_WhenSaveSucceeds_...`, `SaveMessageAsMsgAsync_...`, `SaveMessageAsMSG_...`, `SanitizeArrayLineTSV_...`, `SanitizeArray_...` = 14. Both records agree. After = 15 (T-A rewritten in place, T-B added) `[P]`; the class name is unique (`class \w*SortEmail\w*` matches only `SortEmail` and `SortEmail_Tests`), so the filter cannot over-match.

1. **Scoped run** (direct vstest, same shape as #940 section 6.1): build `UtilitiesCS.Test\UtilitiesCS.Test.csproj` with `/t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nodeReuse:false`; then `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:TaskMaster.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_Tests" "/ResultsDirectory:coverage\test-results\945\<task-id>" "/Logger:trx;LogFileName=<task-id>.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"`. Expected `total` 14 before, 15 after; a different total is a failure (vstest exits 0 on a zero-match filter). `/TestCaseFilter:` and `/Tests:` are mutually exclusive. Per-test controls use `FullyQualifiedName~TrySaveAttachmentAsync` (expected total 2 after).
2. **Full coverage route:** the `test: MSTest with Coverage (Koverage)` task or `Invoke-MSTestWithCoverage.ps1` by absolute path from a `pwsh -NoProfile -Command` payload whose first statement is `Set-Location -LiteralPath "<worktree>"` (`pwsh -WorkingDirectory` does not resolve `-File`, per repository memory). If the four locally stalling `UtilitiesCS.Test` classes (`ShellUtilities_Tests`, `ShellUtilitiesStatic_Tests`, `SysImageListHelperTests`, `OSBrowser_Tests`) reproduce the stall, use the #940/#931 DIRECT route (dot-sourced runner helpers, `TestCaseFilter` excluding those four); the DIRECT route command text is in #940 section 6.2 and must be re-verified there before use. Commit only the JaCoCo projection, the one-line first-party summary and the TRX-derived summary (CLAUDE.md "Committed Test Evidence Format"). Take a Phase 0 baseline of the UtilitiesCS package rates before the edit and compare after with the same collector; a few lines of run-to-run variance were recorded in #940. `DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` is a known intermittent failure (#780).
3. Gates around the edit: `dotnet tool run csharpier format .` / `check .`, then the analyzer and nullable msbuild commands exactly as in CLAUDE.md (both `/t:Rebuild`), then tests.

## 8. Edit list

Expected edits (exactly two files):

1. `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` (add the seamed overload; reduce the existing method to a delegating wrapper; thread the delegate through the retry call).
2. `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` (rewrite the defective test; add the exception-propagation test).

No other production file changes: all four production call sites (L819, L864, L879, `EmailFiler.cs` indirectly) keep calling the 2-argument signature, which is retained. No csproj, interface, or new file is needed. The feature-folder documents and evidence under `docs/features/active/2026-09-30-sort-email-attachment-test-creates-directory-at-repository-root-945/` are process artifacts, not source edits.

## 9. Rejected alternatives (brief)

- Static settable delegate: parallel-race hazard (section 4c).
- New `IFileSystem`/`IDirectoryCreator` interface: more surface than a single call needs; no existing interface to reuse.
- Rooted literal without a seam: would create a real directory (worse than the current defect).
- Temp directory or `MethodInfo`/reflection tricks: prohibited by UT4 or fragile.

## 10. Risks and open items

1. `[P]` The method-level `[ExcludeFromCodeCoverage]` on async methods may still emit denominator lines for the compiler-generated state machine and lambdas (section 4); compare package rates before and after.
2. `[P]` `Path.GetDirectoryName(filePathSave)` returns null for a root or empty path; the original code would then throw `ArgumentNullException` from `CreateDirectory` and rethrow via Branch C. The seam version passes null to the delegate; production default behavior is unchanged because the default lambda calls the same API. Not in scope to change.
3. `SortEmail.cs` is over the 500-line limit already (1429). The fix should not add a split; if the reviewers object, a follow-up issue is appropriate.
4. The issue's proposed validation `VerifyNoOtherCalls` applies to a strict `Mock<Action<string>>`; the recording-list form in T-A is an equivalent evidence source that also proves ordering. Either satisfies the issue's "strict mock" idea; the executor should pick one and state it.
