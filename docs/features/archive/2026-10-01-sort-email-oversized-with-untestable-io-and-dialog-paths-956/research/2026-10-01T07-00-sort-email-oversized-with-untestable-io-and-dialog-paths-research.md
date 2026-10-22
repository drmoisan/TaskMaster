# Research: SortEmail.cs oversized, with untestable I/O and dialog paths (Issue #956)

- **Issue:** #956 (work mode `full-bug`, `issue.md` line 12)
- **Feature folder:** `docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/`
- **Branch:** `bug/sort-email-oversized-with-untestable-io-and-dialog-paths-956`, cut from `origin/main` `9b3eea58447c264eae6f95a4bfee3bfcec7fb17f` (SHA supplied by the delegation prompt; not re-observed, because no shell channel was available in this research session)
- **Timestamp:** 2026-10-01T07-00
- **Evidence basis:** every citation below was read in this worktree with the Read, Grep and Glob tools. Abbreviations: `SRC` = `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs`; `TST` = `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs`.
- **Tag legend:** `[V]` verified by reading the cited file; `[I]` inference from verified facts; `[P]` prediction to be confirmed at execution.

---

## 1. Structure inventory of SortEmail.cs (Q1)

### 1.1 File-level facts `[V]`

- Line count: **1,454** (last content line 1454 is the namespace closing brace; the issue records the same figure at `issue.md` line 16 and the #945 review at `code-review.2026-09-30T13-00.md` line 33).
- Line 1: `#nullable enable` — the whole file participates in nullable analysis.
- Usings, lines 2 to 19: `System`, `System.Collections.Generic`, `System.Diagnostics`, `System.Diagnostics.CodeAnalysis`, `System.IO`, `System.Linq`, `System.Text.RegularExpressions`, `System.Threading.Tasks`, `System.Windows.Forms`, `Deedle`, `Microsoft.Office.Interop.Outlook`, `SDILReader`, `UtilitiesCS`, `UtilitiesCS.EmailIntelligence`, `UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder`, `UtilitiesCS.OutlookExtensions`, `UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable`, alias `Outlook = Microsoft.Office.Interop.Outlook`.
- Namespace `UtilitiesCS` (line 21). Declaration line 23: `public static class SortEmail` — **not** `partial` today.
- Regions: `#region Public Methods` 25 to 620; `#region Private Static Variables` 622 to 632; `#region Helper Methods` 634 to 1338; members 1341 to 1452 are outside any region. Method-internal regions `tocollapse` (1136 to 1146) and `Hide` (1152 to 1199) are inside `SaveAttachmentsOld`.

### 1.2 Member inventory `[V]`

Ranges include the attribute line and leading comment or XML documentation. "Excl" = carries `[ExcludeFromCodeCoverage]`.

| # | Lines | Visibility | Member | Excl | Responsibility group |
|---|---|---|---|---|---|
| 1 | 27-29 | private static readonly | `logger` (log4net) | - | shared |
| 2 | 31-40 | public | `InitializeSortToExisting(...)` (throws `NotImplementedException`) | no | entry stub |
| 3 | 42-74 | public | `SortAsync(bool, string, bool, bool, bool, IApplicationGlobals)` (Explorer selection; `MessageBox.Show` at 60) | yes | MailItem sort family |
| 4 | 76-110 | public | `SortAsync(IList<MailItem>, bool, string, bool, bool, bool, IApplicationGlobals)` | yes | MailItem sort family |
| 5 | 112-179 | public | `SortAsync(IList<MailItemHelper>, ..., string olAncestor, string fsAncestorEquivalent)` | yes | MailItemHelper sort family |
| 6 | 181-208 | public | `UpdatePredictiveEngineAsync(...)` | yes | MailItemHelper sort family |
| 7 | 210-301 | public | `ProcessMailItemAsync(...)` (`File.Delete` at 245) | yes | MailItemHelper sort family |
| 8 | 303-453 | public | `SortAsync(IList<MailItem>, ..., string olAncestor, string fsAncestorEquivalent)` (`File.Delete` at 369) | yes | MailItem sort family |
| 9 | 455-553 | public | `Sort(IList<MailItem>, ...)` (synchronous; `File.Delete` at 515) | yes | MailItem sort family |
| 10 | 555-561 | public | `Cleanup_Files()` (resets four of the five prompt fields) | no | prompt-response state |
| 11 | 563-618 | public | `UndoAsync(SloStack<IMovedMailInfo>, IApplicationGlobals)` (`MessageBox.Show` at 582, 605, 615) | yes | undo / move log |
| 12 | 624-628 | private static | `_responseSaveFile`, `_attachmentsOverwrite`, `_attachmentsAltName`, `_picturesOverwrite`, `_removeReadOnly` | - | prompt-response state |
| 13 | 630 | private const | `MAX_PATH = 256` (used only at 1195, 1197, 1238) | - | legacy attachment routine |
| 14 | 636-659 | internal | `GetAttachmentsInfo(...)` | yes | attachment saving |
| 15 | 661-699 | internal | `GetAttachmentsInfoAsync(...)` | yes | attachment saving |
| 16 | 701-760 | public ext | `SaveAttachment(this AttachmentHelper)` (`File.Exists` 704; dialogs 710, 733) | yes | attachment saving |
| 17 | 762-823 | public ext | `SaveAttachmentAsync(this AttachmentHelper)` (`File.Exists` 767; dialogs 773, 796) | yes | attachment saving |
| 18 | 825-837 | public ext | `SaveAttachmentAsync(this AttachmentHelper, string destinationPath)` | yes | attachment saving |
| 19 | 839-886 | internal | `SaveCaseAsync(...)` (dialog 853) | yes | attachment saving |
| 20 | 888-904 | internal ext | `TrySaveAttachmentAsync(this Attachment, string)` (wrapper, lambda at 902) | yes | attachment saving (try-save) |
| 21 | 906-984 | internal ext | `TrySaveAttachmentAsync(this Attachment, string, Action<string> createDirectory)` (core; dialog 936) | yes | attachment saving (try-save) |
| 22 | 986-1005 | internal | `SaveCase(...)` | yes | attachment saving |
| 23 | 1007-1016 | internal ext | `IsPicture(this Attachment)` (no callers) | yes | attachment saving |
| 24 | 1018-1057 | private | `ResolvePaths(IList<MailItem>, ...)` (commented legacy signature 1018-1023) | yes | MailItem sort family |
| 25 | 1059-1103 | private | `ResolvePaths(Folder, ..., out Folder?)` | yes | MailItemHelper sort family |
| 26 | 1105-1114 | internal | `SaveMessageAsMsgAsync(MailItem, string)` | yes | message saving |
| 27 | 1116-1123 | internal | `SaveMessageAsMSG(MailItem, string)` | yes | message saving |
| 28 | 1125-1336 | internal | `SaveAttachmentsOld(...)` (no callers; dialogs 1263, 1280; `InputBox.ShowDialog` 1297; `MessageBox.Show` 1305) | yes | legacy attachment routine |
| 29 | 1341-1351 | private | `PushToUndoStack(...)` | yes | undo / move log |
| 30 | 1353-1366 | private | `CaptureMoveDetails(...)` | yes | undo / move log |
| 31 | 1368-1384 | private | `SanitizeArrayLineTSV(ref string[])` (tested by reflection, TST 346) | yes | undo / move log |
| 32 | 1386-1396 | internal | `StripTabsCrLf(string)` | no | undo / move log |
| 33 | 1398-1429 | public | `WriteCSV_StartNewFileIfDoesNotExist(string, string)` (`File.Exists` 1406; `FileIO2.WriteTextFile` 1425) | yes | undo / move log |
| 34 | 1431-1452 | private | `SanitizeArray(string[,]?, ref string[]?)` (tested by reflection, TST 363) | yes | undo / move log |

No nested types exist in the class.

---

## 2. Proposed partial-file split (Q2)

### 2.1 Recommendation

Convert line 23 to `public static partial class SortEmail` and split into six files of the same class, each a verbatim move of whole members. This keeps every public/internal/private signature, the type identity used by `typeof(SortEmail).GetMethod(..., BindingFlags.NonPublic | BindingFlags.Static)` (TST 346, 363), and all extension-method resolution unchanged. In-repo precedent for a `static partial` split with `Type.Responsibility.cs` file names: `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.*.cs` (`public static partial class OlTableExtensions`, registered at `UtilitiesCS.csproj` lines 1072 to 1076), and the instance-class precedent `EmailDataMiner.*.cs` in the same folder (`UtilitiesCS.csproj` lines 679 to 681, 865).

Per-file header (about 24 lines): `#nullable enable` on line 1, then the full using block of SRC lines 2 to 19 copied verbatim, the namespace, and `public static partial class SortEmail`. Two rules are load-bearing:

1. **Every new partial must start with `#nullable enable`.** The moved members use nullable annotations (`object?` 36, `string?` 144, `Folder?` 145, `MailItem?` 271, `string[,]?` 1405, and others); without the directive in a file, the compiler reports CS8632 for each annotation, which `/p:TreatWarningsAsErrors=true` turns into a build error `[I]` (CLAUDE.md C#1.3: nullable is per-file opt-in).
2. **Copy the whole using block.** Trimming per file risks a compile failure for no benefit; no `IDE0005` entry exists in `.editorconfig` and no project sets `GenerateDocumentationFile` (Grep over `.editorconfig`, `UtilitiesCS/UtilitiesCS.csproj`, `Directory.Build.props`: zero matches) `[V]`, so unused usings do not fail the analyzer gate `[I]`.

Drop the three class-level `#region`/`#endregion` pairs (25/620, 622/632, 634/1338): their members land in different files and a region cannot span files. The method-internal regions in `SaveAttachmentsOld` and the `#pragma warning disable/restore CS0618` pairs (234/239, 360/362, 675/688) move with their methods unchanged.

### 2.2 File map

All paths are under `UtilitiesCS/EmailIntelligence/EmailParsingSorting/`. Estimates are header plus moved lines plus separating blank lines.

| File | Members moved (SRC line ranges) | Est. lines |
|---|---|---|
| `SortEmail.cs` (retained, edited) | header 1-24 (line 23 becomes `partial`); `logger` 27-29; `InitializeSortToExisting` 31-40; `SortAsync` (MailItemHelper) 112-179; `UpdatePredictiveEngineAsync` 181-208; `ProcessMailItemAsync` 210-301; `ResolvePaths(Folder, ...)` 1059-1103 | ~280 |
| `SortEmail.MailItemSort.cs` (new) | `SortAsync` (Explorer) 42-74; `SortAsync` (IList<MailItem>, 7 args) 76-110; `SortAsync` (IList<MailItem>, 9 args) 303-453; `Sort` 455-553; `ResolvePaths(IList<MailItem>, ...)` 1018-1057 | ~390 |
| `SortEmail.AttachmentSaving.cs` (new) | prompt-state fields 624-628; `Cleanup_Files` 555-561; `GetAttachmentsInfo` 636-659; `GetAttachmentsInfoAsync` 661-699; `SaveAttachment` 701-760; `SaveAttachmentAsync` 762-823; `SaveAttachmentAsync(dest)` 825-837; `SaveCaseAsync` 839-886; `SaveCase` 986-1005; `IsPicture` 1007-1016; `SaveMessageAsMsgAsync` 1105-1114; `SaveMessageAsMSG` 1116-1123 | ~345 |
| `SortEmail.TrySaveAttachment.cs` (new) | `TrySaveAttachmentAsync` 888-904 and 906-984, plus the seam design of section 4 (new core overload, read-only adapter, production prompt session field) | ~170 |
| `SortEmail.LegacyAttachmentSaving.cs` (new) | `MAX_PATH` 630; `SaveAttachmentsOld` 1125-1336 | ~245 |
| `SortEmail.UndoAndMoveLog.cs` (new) | `UndoAsync` 563-618; `PushToUndoStack` 1341-1351; `CaptureMoveDetails` 1353-1366; `SanitizeArrayLineTSV` 1368-1384; `StripTabsCrLf` 1386-1396; `WriteCSV_StartNewFileIfDoesNotExist` 1398-1429; `SanitizeArray` 1431-1452 | ~195 |

Arithmetic check `[I]`: 1,454 source lines + 5 duplicated headers (~26 each, ~130) + ~45 lines of seam growth is about 1,630; the six estimates sum to about 1,625. The largest file (`SortEmail.MailItemSort.cs`) retains about 110 lines of headroom. The plan should gate each file at `< 500` lines after CSharpier.

`Cleanup_Files` is placed with the prompt fields it resets. `_responseSaveFile` is read only by `SaveAttachmentsOld` (1253, 1278, 1285, 1289) and `Cleanup_Files` (557); a `private static` field in one partial is visible to all partials, so co-locating it with `Cleanup_Files` is a cohesion choice, not a compile requirement.

Static field initialization order across partial declarations is unspecified by the C# specification `[I]`; none of the moved initializers depends on another (all are enum constants, a `const`, and the log4net logger), so the split cannot change initialization behavior.

### 2.3 Rejected alternatives (brief)

- **Extract separate static types** (for example `AttachmentSaver`): changes call sites (`EmailFiler.cs` 445 calls the `SaveAttachmentAsync` extension; tests call `SortEmail.*` and reflect on `typeof(SortEmail)`), and changes extension-method resolution. Rejected: the issue requires no behavior change and the partial split satisfies the 500-line rule without moving the public surface.
- **Fewer, larger files** (for example legacy sort plus legacy attachments in one file): about 630 lines. Rejected on the limit.

---

## 3. Project files (Q3) `[V]`

- `UtilitiesCS/UtilitiesCS.csproj` is a legacy, non-SDK project: `<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">` (line 2), `TargetFrameworkVersion v4.8.1` (16), `LangVersion 12.0` (10), and explicit `<Compile Include=...>` items. The SortEmail entry is line 817: `    <Compile Include="EmailIntelligence\EmailParsingSorting\SortEmail.cs" />`.
- New entries take the identical form (four-space indent, backslash separators, self-closing, no `DependentUpon`, matching `EmailDataMiner.*` at 679 to 681). Insert directly after line 817:

```xml
    <Compile Include="EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs" />
    <Compile Include="EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs" />
    <Compile Include="EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs" />
    <Compile Include="EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs" />
    <Compile Include="EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs" />
```

  plus, for the session type of section 4, after line 574 (`<Compile Include="Dialogs\YesNoToAll.cs" />`): `    <Compile Include="Dialogs\YesNoToAllPromptSession.cs" />`.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` is also legacy non-SDK (line 2, `ToolsVersion="15.0"`, `TargetFrameworkVersion v4.8.1` line 17, `LangVersion Latest` line 18). `SortEmail_Tests.cs` entry: line 98, `    <Compile Include="EmailIntelligence\SortEmail_Tests.cs" />`. Dialog tests: lines 441 to 442. New entries: `    <Compile Include="EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs" />` after line 98, and `    <Compile Include="Dialogs\YesNoToAllPromptSession_Tests.cs" />` after line 442.
- `UtilitiesCS/Properties/AssemblyInfo.cs` line 19 grants `InternalsVisibleTo("UtilitiesCS.Test")` (line 18 grants `DynamicProxyGenAssembly2`), so internal overloads and an internal session type are reachable from tests and Moq.
- `.csharpierignore` keeps `.csproj` out of CSharpier (#945 plan fact 6); csproj edits are hand-written. `.editorconfig` sets `end_of_line = crlf` for `.cs` (#945 plan line 59); new `.cs` files should be CRLF and CSharpier normalizes divergence.

---

## 4. Prompt seam design for TrySaveAttachmentAsync (Q4, Q5)

### 4.1 YesNoToAll `[V]`

`UtilitiesCS/Dialogs/YesNoToAll.cs` (`#nullable enable` at 10, namespace `UtilitiesCS`):

- `public enum YesNoToAllResponse { Empty = 0, Yes = 1, No = 2, YesToAll = 4, NoToAll = 8 }` (14 to 21).
- `public static class YesNoToAll` (23); single dialog method `public static YesNoToAllResponse ShowDialog(string message)` (65 to 109). It resets an `AsyncLocal` response, shows a modal `MyBox.ShowDialog` with five buttons, and returns the clicked value. The **Cancel** button maps to `RespondCancel`, which yields `YesNoToAllResponse.Empty` (47 to 50, 99 to 104).
- The method group converts to `Func<string, YesNoToAllResponse>` directly.

Call sites in SRC (`YesNoToAll.ShowDialog(`): 710 (`SaveAttachment`, pictures), 733 (`SaveAttachment`, attachments), 773 (`SaveAttachmentAsync`, pictures), 796 (`SaveAttachmentAsync`, attachments), 853 (`SaveCaseAsync`, alt name), 856 (inside a comment, not a call), **936 (`TrySaveAttachmentAsync` core, read-only prompt)**, 1263 and 1280 (`SaveAttachmentsOld`). Eight live calls; only 936 is in scope for the seam.

### 4.2 The #945 seam as it stands `[V]`

- SRC 888 to 904: `[ExcludeFromCodeCoverage] internal static Task<bool> TrySaveAttachmentAsync(this Attachment attachment, string filePathSave)` — non-async, single statement forwarding `path => System.IO.Directory.CreateDirectory(path)` (902).
- SRC 906 to 984: `[ExcludeFromCodeCoverage] internal static async Task<bool> TrySaveAttachmentAsync(this Attachment attachment, string filePathSave, Action<string> createDirectory)`; `createDirectory(Path.GetDirectoryName(filePathSave))` at 921; the recursive retry passes the delegate through at 961.
- Production callers (unchanged by #945): 819 (`SaveAttachmentAsync`), 864 and 879 (`SaveCaseAsync`), all via the two-argument overload.
- Tests, TST 236 to 289: a rooted in-memory literal `C:\Sortemail945Sandbox\attachments` (238); `TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile` (245 to 266) records `mkdir:` and `save:` events in a `List<string>` and asserts their order; `TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave` (272 to 289) passes a throwing delegate and asserts `Times.Never` on `SaveAsFile`. Neither test reaches the `UnauthorizedAccessException` branch, so neither reads `_removeReadOnly`.
- Threading pattern: the seam is a per-call parameter; nothing static is settable. The #945 review recorded the follow-up "extract the read-only ShowDialog prompt ... remove `[ExcludeFromCodeCoverage]` ... add tests for the `UnauthorizedAccessException` branches" (`code-review.2026-09-30T13-00.md` line 60).

### 4.3 Static prompt-response state and its hazard (Q5) `[V]`

Fields (SRC 624 to 628), their readers and writers:

| Field | Read/written by | Reset by `Cleanup_Files` (555-561)? |
|---|---|---|
| `_responseSaveFile` | `SaveAttachmentsOld` 1253, 1278, 1285, 1289 | yes (557) |
| `_attachmentsOverwrite` | `SaveAttachment` 731-750; `SaveAttachmentAsync` 794-812; `SaveAttachmentsOld` 1253-1274 | yes (558) |
| `_attachmentsAltName` | `SaveCaseAsync` 851-873 | **no** |
| `_picturesOverwrite` | `SaveAttachment` 708-726; `SaveAttachmentAsync` 771-789 | yes (559) |
| `_removeReadOnly` | `TrySaveAttachmentAsync` core 932-971 | yes (560) |

Production caller of `Cleanup_Files`: `QuickFiler/Controllers/EfcDataModel.cs` line 309, after each filer invocation, so "ToAll" answers are sticky for one filing operation.

How the `UnauthorizedAccessException` handler (925 to 979) uses `_removeReadOnly`:

1. 932 to 937: if `_removeReadOnly == Empty`, build the message `"The folder {dir} is read-only. Do you want to remove the readonly attribute?"` and assign `_removeReadOnly = YesNoToAll.ShowDialog(message)`; otherwise reuse the sticky value without prompting.
2. 939 to 962 (`Yes` or `YesToAll`): construct `new DirectoryInfo(Path.GetDirectoryName(filePathSave))` (944, outside the inner `try`), then inside the inner `try` clear the read-only attribute with `di.Attributes &= ~FileAttributes.ReadOnly` (947). On any exception: `Debug.WriteLine`, `return false` (949 to 953). The `finally` (954 to 960) resets the field to `Empty` only when it is `Yes`. If the attribute write succeeded, retry recursively (961).
3. 963 to 974 (`No` or `NoToAll`): `Debug.WriteLine`, reset to `Empty` only when `No`, `return false`.
4. 975 to 978 (any other value, in practice `Empty` from Cancel): `throw;` rethrows the `UnauthorizedAccessException`.
5. 980 to 983: `catch (System.Exception) { throw; }` rethrows anything else.

Hazard for parallel tests `[I]`: `scripts/vscode/TaskMaster.cli.runsettings` sets `Workers 0` and `Scope ClassLevel` (lines 5 to 6), so test classes in `UtilitiesCS.Test` run concurrently within one process and share the static field.

- A test that answers `YesToAll` or `NoToAll` leaves `_removeReadOnly` sticky; any other test reaching the handler afterwards skips its prompt and takes the sticky branch, so its outcome depends on execution order.
- Even a `Yes` test exposes the value between the prompt (936) and the `finally` reset (958), which spans an `await` boundary in the retry.
- The existing `Cleanup_Files_DoesNotThrow` (TST 174 to 180) writes `_removeReadOnly = Empty` (560) at an arbitrary moment relative to any concurrently running test class, which can turn a sticky-state test into a prompting one mid-run.
- A test cannot restore the field deterministically without `[DoNotParallelize]` or serial execution. Both are excluded by repository practice: parallel runs must stay parallel, and serialization masks an isolation violation (agent-memory `feedback_tests_must_run_parallel_serial_masks_isolation_violation`).

### 4.4 Candidate designs

| Design | Description | Verdict |
|---|---|---|
| A. Per-call prompt delegate + per-call state holder object | Core overload takes `Func<string, YesNoToAllResponse>` (inside a session object, see below) and the sticky state travels in that object; production passes one `static readonly` session instance so stickiness and `Cleanup_Files` behave exactly as today; tests construct a fresh session per test. | **Recommended** |
| B. Per-call delegate, keep the static field | Prompt seam only; `_removeReadOnly` stays static. | Rejected: tests still race on the static field (section 4.3). |
| C. `ref YesNoToAllResponse` state parameter | Pass the state by reference. | Rejected: `async` methods cannot have `ref` parameters (CS1988). |
| D. `AsyncLocal<YesNoToAllResponse>` for `_removeReadOnly` | Mirrors `YesNoToAll` (52 to 57). | Rejected: a value assigned inside an awaited async method does not flow back to the caller, so a `YesToAll` answer would no longer stick across the consecutive per-attachment calls made by `ProcessMailItemAsync`/`EmailFiler`, which changes production behavior. |
| E. `StrongBox<YesNoToAllResponse>` plus a separate `Func` | Two coupled primitives per call. | Rejected: two parameters that must always travel together, with a compiler-services type in a domain signature. Design A bundles them into one cohesive type. |
| F. Settable static `Func` seam | Tests replace a static delegate. | Rejected by the issue (`issue.md` line 56) and by the parallel-run rule. |
| G. Interface seam (`IAttachmentSaveEnvironment`) | `.claude/rules/csharp.md` lists an interface as the preferred seam. | Rejected for this item: the #945 overload pair is a delegate seam already pinned by two tests, and the change budget favors extending it consistently. Recorded so the deviation from interface-first is explicit. |

### 4.5 Recommended design (A), concretely

**New type** `UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs` (namespace `UtilitiesCS`, `#nullable enable`, `internal sealed class`, about 50 lines with XML docs). Name uniqueness was verified by Grep (no existing match).

- `internal YesNoToAllPromptSession(Func<string, YesNoToAllResponse> showDialog)`, which throws `ArgumentNullException` on null.
- `internal YesNoToAllResponse Response { get; private set; }` (initially `Empty`).
- `internal YesNoToAllResponse Ask(string message)`: if `Response == Empty`, set `Response = showDialog(message)`; return `Response`.
- `internal void ReleaseSingleAnswer()`: if `Response` is `Yes` or `No`, set it to `Empty`. This reproduces both reset sites (958 resets only `Yes` in a branch where only `Yes`/`YesToAll` are possible; 971 resets only `No` in a branch where only `No`/`NoToAll` are possible).
- `internal void Reset()`: set `Response = Empty`.

**`SortEmail.TrySaveAttachment.cs` overloads:**

1. Two-argument overload (unchanged signature, keeps `[ExcludeFromCodeCoverage]`, keeps the #945 lambda): forwards to overload 2 with `path => System.IO.Directory.CreateDirectory(path)`. Recorded justification: its only behavior is wiring the real directory-creation default, and calling it from a test creates a real directory, which UT4 prohibits (the same reason as the #945 review line 37).
2. Three-argument overload `(this Attachment, string, Action<string> createDirectory)`, the existing tested signature, kept for TST 257 and 281: becomes a one-statement forward to overload 3 with `ClearReadOnlyAttributeOnDisk` and the production session `RemoveReadOnlyPrompt`. **`[ExcludeFromCodeCoverage]` is removed**; the two existing tests cover it.
3. New core `internal static async Task<bool> TrySaveAttachmentAsync(this Attachment attachment, string filePathSave, Action<string> createDirectory, Action<string> clearReadOnly, YesNoToAllPromptSession removeReadOnlyPrompt)`: the current body of 906 to 984 with `_removeReadOnly` replaced by `removeReadOnlyPrompt`, `YesNoToAll.ShowDialog(message)` replaced by `removeReadOnlyPrompt.Ask(message)`, the `DirectoryInfo` attribute write replaced by `clearReadOnly(directory)` inside the existing inner `try`, and the retry passing all three seams. **No `[ExcludeFromCodeCoverage]`.**
4. `private static readonly YesNoToAllPromptSession RemoveReadOnlyPrompt = new(YesNoToAll.ShowDialog);`, the production sticky state. It replaces the `_removeReadOnly` field at 628, and `Cleanup_Files` calls `RemoveReadOnlyPrompt.Reset()` in place of line 560. It is not a seam: nothing can replace it, and tests never pass it.
5. `[ExcludeFromCodeCoverage] private static void ClearReadOnlyAttributeOnDisk(string directoryPath)`, which contains the two original statements (`var di = new DirectoryInfo(directoryPath); di.Attributes &= ~FileAttributes.ReadOnly;`). Recorded justification: a pure file-system adapter whose execution requires the real file system (UT4). It is passed as a method group, so no lambda closure is introduced.

Net `[ExcludeFromCodeCoverage]` count in SortEmail after the change `[P]`: 28 - 1 (overload 2) + 1 (adapter) = **28**. The excluded executable mass falls from the whole try-save body (about 60 lines) to the 2-line adapter plus the 1-statement wrapper.

**Behavior-equivalence notes `[I]`:**

- The message string is now built before `Ask` even when a sticky answer exists. Building it has no side effect, so the result is unchanged.
- Original line 944 constructs `DirectoryInfo` outside the inner `try`, so an `ArgumentNullException` or `ArgumentException` there propagates, whereas inside `clearReadOnly` it would be caught and return `false`. This case cannot be reached in production: `createDirectory(Path.GetDirectoryName(filePathSave))` at 921 runs first with the same argument, and the real `Directory.CreateDirectory` throws on a null or invalid path before `SaveAsFile` is attempted, so the `UnauthorizedAccessException` handler never sees such a path. To keep a literal match anyway, compute `var directory = Path.GetDirectoryName(filePathSave);` before the inner `try` (as 935 and 944 already do) and call `clearReadOnly(directory)` inside it.
- Thread-safety is unchanged: an unsynchronized static shared object replaces an unsynchronized static field.
- Null guards: the #945 review (line 35) accepted no guard on `createDirectory` for an internal method. For consistency, the core may stay unguarded. The session constructor guards its delegate because it is a new type that must meet the >= 90% bar with tested negative paths.

---

## 5. UnauthorizedAccessException branches and required tests (Q6)

### 5.1 Branch enumeration `[V]`

Within the core (SRC 919 to 983), and nothing else it calls, since `createDirectory`, `SaveAsFile` and the prompt are seams or a mock:

| ID | Path | Lines |
|---|---|---|
| B0 | success | 921-923 |
| B1 | handler entry, state `Empty`, prompt shown | 925-937 |
| B2 | handler entry, sticky state, no prompt | 932 false arm |
| B3 | `Yes`/`YesToAll`, attribute cleared, retry | 939-947, 961 |
| B4 | `Yes`/`YesToAll`, attribute clear throws, `return false` | 949-953 |
| B5 | `finally` resets when `Yes` | 956-959 (true arm) |
| B6 | `finally` keeps `YesToAll` | 956 (false arm) |
| B7 | `No`/`NoToAll`, `return false`; reset when `No` | 963-973 (969 both arms) |
| B8 | `Empty` after prompt (Cancel), `throw;` | 975-978 |
| B9 | non-`UnauthorizedAccessException`, rethrow | 980-983 |
| B10 | `createDirectory` throws (outside the handler; covered by the existing IOException test) | 921 |

### 5.2 Required tests

New file: `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs`, class `SortEmail_TrySaveAttachment_Tests`, namespace `UtilitiesCS.Test.EmailIntelligence`. All tests call the five-argument core with a recording `createDirectory`, a recording `clearReadOnly`, a fresh `YesNoToAllPromptSession` whose delegate records messages and returns scripted answers, and a `MockBehavior.Loose` `Mock<Attachment>` using `SetupSequence(x => x.SaveAsFile(path)).Throws(new UnauthorizedAccessException()).Pass()`. That void-sequence shape has in-repo precedent at `UtilitiesCS.Test/OutlookObjects/Item/OutlookItemFlaggableTests.cs` 203 to 206; Moq is 4.21.0 (`UtilitiesCS.Test/packages.config` line 65). Use a rooted in-memory literal path in the #945 style, distinct per class (for example `C:\Sortemail956Sandbox\attachments\saved.txt`). No test touches the disk.

| Test | Scenario | Asserts | Branches |
|---|---|---|---|
| T1 | save succeeds first time | returns true; prompt never called; `clearReadOnly` never called; `Response == Empty` | B0 |
| T2 | first save throws UAE; prompt returns `Yes`; retry succeeds | true; prompt called once with message containing the directory and `is read-only`; `clearReadOnly` called once with the directory; `SaveAsFile` twice; `Response == Empty` | B1, B3, B5 |
| T3 | as T2 with `YesToAll` | true; `Response == YesToAll` (sticky) | B1, B3, B6 |
| T4 | sticky `YesToAll` reuse: call twice on one session, each first save throws UAE | prompt called exactly once across both calls; both return true | B2, B3 |
| T5 | prompt returns `No` | false; `clearReadOnly` never called; `SaveAsFile` once; `Response == Empty` | B1, B7 (reset arm) |
| T6 | prompt returns `NoToAll`, then a second call that throws UAE | both false; prompt called once; `Response == NoToAll` | B1, B2, B7 (sticky arm) |
| T7 | prompt returns `Empty` (Cancel) | `ThrowAsync<UnauthorizedAccessException>`; `clearReadOnly` never called; `Response == Empty` | B1, B8 |
| T8 | `Yes`; `clearReadOnly` throws (for example `IOException`) | false; `SaveAsFile` once (no retry); `Response == Empty` | B4, B5 |
| T9 | `YesToAll`; `clearReadOnly` throws | false; `Response == YesToAll` | B4, B6 |
| T10 | `SaveAsFile` throws a non-UAE (for example `IOException` or `COMException`) | exception propagates; prompt never called | B9 |
| T11 | `Yes`; retry throws UAE again; second prompt returns `No` | false; prompt called twice; `clearReadOnly` once; `SaveAsFile` twice | B3, B5, B1, B7 (bounded retry) |

Do **not** write a `YesToAll`-plus-retry-still-fails test: it loops without bound (latent defect L2, section 9).

New file `UtilitiesCS.Test/Dialogs/YesNoToAllPromptSession_Tests.cs` (namespace `UtilitiesCS.Test.Dialogs`, matching `MyBox_Tests.cs` and others): S1 constructor null throws `ArgumentNullException`; S2 `Ask` with `Empty` state invokes the delegate and stores its answer; S3 `Ask` with a non-`Empty` state returns it without invoking; S4 `ReleaseSingleAnswer` clears `Yes` and `No`; S5 `ReleaseSingleAnswer` keeps `YesToAll` and `NoToAll` (a `[DataRow]` set over the four values is acceptable); S6 `Reset` clears a sticky value; S7 `Ask` when the delegate returns `Empty` leaves `Response == Empty` and asks again on the next call.

### 5.3 Read-only removal seam (file-system calls inside the handler) `[V]`

The only file-system calls inside the handler are `new DirectoryInfo(...)` (944) and the `Attributes` setter write (947). No `File.SetAttributes` or `FileInfo.IsReadOnly` exists in SRC (Grep over `File\.(...|SetAttributes|GetAttributes)` and `new FileInfo`: no match). Both move behind `Action<string> clearReadOnly`; the production default is the excluded adapter `ClearReadOnlyAttributeOnDisk` (section 4.5 item 5). `Path.GetDirectoryName` (935, 944) is a pure string operation and needs no seam.

---

## 6. Remaining direct file-system calls (Q7) `[V]`

Family: every `System.IO.File`, `System.IO.Directory`, `DirectoryInfo` or `FileInfo` call, plus the file-writing helper `FileIO2.WriteTextFile`, in SRC.

| Line | Call | Enclosing method | Excl | COM-bound / reachability | Recommendation |
|---|---|---|---|---|---|
| 245 | `File.Delete` (in `Task.Run`) | `ProcessMailItemAsync` | yes | takes `MailItemHelper`, `Folder`; calls `MailItem.Move`; reachable only from the MailItemHelper `SortAsync` (112), which has no compiled external caller | Record justification J1 |
| 369 | `File.Delete` | `SortAsync` (MailItem, 9 args) | yes | `MailItem` COM chain; no compiled external caller | J1 |
| 515 | `File.Delete` | `Sort` | yes | `MailItem` COM chain; no compiled external caller | J1 |
| 704 | `File.Exists` | `SaveAttachment` | yes | wraps COM `Attachment`; reachable only from `Sort` (507) | J2 |
| 767 | `File.Exists` | `SaveAttachmentAsync(this AttachmentHelper)` | yes | **live**: `EmailFiler.SaveAttachmentAsync` (`EmailFiler.cs` 443-446) to SRC 825-837 to here | J3 |
| 902 | `System.IO.Directory.CreateDirectory` | `TrySaveAttachmentAsync` (2 args) | yes (kept) | default of the #945 seam | already seamed; wrapper exclusion justified (section 4.5) |
| 944/947 | `new DirectoryInfo`, `Attributes` write | `TrySaveAttachmentAsync` core | removed | live (via 767 path) | **Seam** (`clearReadOnly`, section 4.5) |
| 1212, 1214, 1216, 1218, 1227 | `File.Exists` / `File.Delete` | `SaveAttachmentsOld` | yes | zero callers (Grep: definition 1126 only) | J4 |
| 1406 | `File.Exists` | `WriteCSV_StartNewFileIfDoesNotExist` | yes | live caller `TaskMaster/AppGlobals/AppOlObjects.cs` 301 | J5 |
| 1425 | `FileIO2.WriteTextFile` | same | yes | same | J5 |

Indirect writes through COM interfaces (`Attachment.SaveAsFile` 757, 922, 997, 1000, 1331; `MailItem.SaveAs` 1113, 1122) are already mockable through Moq, as TST 245-266 and 291-339 do, and need no seam.

Justification texts (for `spec.md`):

- **J1.** The enclosing method is a pre-existing `[ExcludeFromCodeCoverage]` Outlook-interop orchestration path (`MailItem.Move`, `Folder`, `Explorer.Selection`) with no injectable COM seam. The file delete is one statement inside it, and seaming it would not make the method testable. Note: CLAUDE.md UT2 exemption (c) names `TaskVisualization`, `QuickFiler`, `TaskMaster`, `ToDoModel` and `Tags`, **not `UtilitiesCS`**, so this is recorded as a pre-existing, unchanged exclusion and not as a new claim under (c). The maintainer may need to ratify the wording.
- **J2.** As J1. `SaveAttachment` is reachable only from the synchronous `Sort`.
- **J3.** Live path. The method also holds two `YesNoToAll.ShowDialog` sites and the static `_picturesOverwrite`/`_attachmentsOverwrite` state, so a `File.Exists` seam alone does not make it testable. Widening the prompt-session design to those sites is follow-up work F1 (section 9), kept out of this item to bound scope.
- **J4.** Dead code with zero callers. Recommend a follow-up to delete it, rather than adding a seam to unreachable code.
- **J5.** Called once at add-in start to seed the moved-mails CSV. The `File.Exists` branch is effectively unreachable in production (latent defect L4, section 9). A seam would only cover a defective branch, so record the justification and promote L4.

---

## 7. ExcludeFromCodeCoverage inventory (Q8) `[V]`

28 attributes, on lines 42, 76, 112, 181, 210, 303, 455, 564, 636, 661, 701, 762, 825, 839, 893, 912, 986, 1007, 1024, 1059, 1105, 1116, 1125, 1341, 1353, 1368, 1398, 1431. They map one-to-one to members 3-9, 11, 14-31 (except 32 `StripTabsCrLf`), 33 and 34 of section 1.2. Non-excluded members: `InitializeSortToExisting`, `Cleanup_Files`, `StripTabsCrLf`.

Removed by this fix: **912** (the current core, which becomes the covered three-argument forward); the new five-argument core is added without it. Kept: **893** (two-argument wrapper, justified). Added: one, on the `ClearReadOnlyAttributeOnDisk` adapter (justified). All others are unchanged and move verbatim with their members.

Observation, out of scope: `SanitizeArrayLineTSV` (1368) and `SanitizeArray` (1431) are excluded yet already tested by reflection (TST 341-382), and `SaveMessageAsMsgAsync`/`SaveMessageAsMSG` (1105, 1116) are excluded yet directly tested (TST 291-339). Removing those four attributes is a coverage-only follow-up (F3).

Coverage mechanics `[V]`: `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` exists, which post-filters closures of excluded members (issue #457). The kept two-argument wrapper's lambda is therefore filtered. The new core's `Task.Run` lambda and async state machine become measured once the attribute is removed, and T1-T11 cover them.

---

## 8. Existing tests (Q9) `[V]`

`TST` is 457 lines, `[TestClass] public class SortEmail_Tests` (32-33), 15 test methods:

| Line | Test |
|---|---|
| 42 | `InitializeSortToExisting_AlwaysThrows_NotImplementedException` |
| 55 | `InitializeSortToExisting_WithExplicitArgs_StillThrows_NotImplementedException` |
| 80 | `SortAsync_MailHelpers_WhenNull_ThrowsArgumentNullException` |
| 105 | `SortAsync_MailHelpers_WhenEmpty_ThrowsArgumentNullException` |
| 141 | `StripTabsCrLf_WithControlCharacters_ReturnsCleanedSingleSpacedString` |
| 158 | `StripTabsCrLf_WithPlainText_ReturnsOriginalString` |
| 175 | `Cleanup_Files_DoesNotThrow` |
| 183 | `GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments` |
| 210 | `GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments` |
| 246 | `TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile` |
| 273 | `TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave` |
| 292 | `SaveMessageAsMsgAsync_WhenSubjectNeedsSanitizing_UsesMsgSavePath` |
| 317 | `SaveMessageAsMSG_WhenSubjectNeedsSanitizing_UsesMsgSavePath` |
| 342 | `SanitizeArrayLineTSV_WhenArrayContainsNullsAndWhitespaceControlCharacters_ReturnsSanitizedLine` |
| 360 | `SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows` |

Eleven new tests at about 25 lines each (about 275 lines) would push TST to about 730 lines, so they go in the new file `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs` (estimate 330-380 lines including a small private `CreateAttachmentMock` helper; the TST helper at 386-406 is `private static` and cannot be shared). If the estimate exceeds about 450 lines after CSharpier, split T1-T6 and T7-T11 into two classes (`...SaveAttachment_Prompt_Tests` and `...SaveAttachment_Failure_Tests`). TST itself stays unchanged: existing tests are part of the spec, and all 15 must remain green after the split.

The session tests file `UtilitiesCS.Test/Dialogs/YesNoToAllPromptSession_Tests.cs` is estimated at 120-160 lines.

---

## 9. Callers outside SortEmail.cs (Q11) `[V]`

Compiled callers (Grep for `SortEmail` over `*.cs`, and for extension-call shapes):

| File:line | Member | Affected by split? |
|---|---|---|
| `TaskMaster/Ribbon/RibbonController.cs` 230 | `UtilitiesCS.SortEmail.UndoAsync(...)` | no |
| `TaskMaster/AppGlobals/AppOlObjects.cs` 301 | `SortEmail.WriteCSV_StartNewFileIfDoesNotExist(...)` | no |
| `QuickFiler/Controllers/EfcDataModel.cs` 309 | `SortEmail.Cleanup_Files()` | no (body changes one statement; signature unchanged) |
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs` 445 | extension `attachment.SaveAttachmentAsync(Config.SaveFsPath!)` | no |
| `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` (45, 59, 84, 109, 147, 164, 178, 193, 221, 257, 281, 310, 335; reflection 346, 363) | various | no |

Not compiled or not calls: `QuickFiler/Legacy/QfcController.cs` 781, 792, 1658 (Grep over `QuickFiler/QuickFiler.csproj` finds no `QfcController.cs` or `Legacy` entry; line 781 calls a nonexistent `SortEmail.Run`); commented lines in `QfcItemController.MailActions.cs` (156, 170, 188), `QfcFormController.EventHandlers.cs` 338, and `ToDoModel.Test/Email Utilities/*` (35, 59, 85, 114, 136, 39); doc comments in `SloStack.cs` 14, `IFolderPredictor.cs` 8, `EmailFiler.cs` 20; the ribbon callback name `SortEmail_Click` (`RibbonViewer.cs` 167, `RibbonCommandBoundaryTests.cs` 192, `RibbonExplorer.xml`). No `using static UtilitiesCS.SortEmail` exists.

A partial-class split preserves the type, namespace, member names, signatures and accessibility, so **no call site changes** `[I]`. The only signature-level additions are the new five-argument internal overload and the new internal type.

### Latent defects observed (report-only; promote, do not fix in #956)

- **L1** `SaveCase` (986-1005): `case (YesNoToAllResponse.NoToAll | YesNoToAllResponse.No)` is the constant 10 and `(Yes | YesToAll)` is 5. The enum values are 1, 2, 4 and 8, so neither case can match, and the synchronous `SaveAttachment` never saves an existing-file attachment. Low impact: `Sort` has no compiled caller.
- **L2** `TrySaveAttachmentAsync`: with a sticky `YesToAll`, if the attribute clear succeeds but `SaveAsFile` keeps throwing `UnauthorizedAccessException` (for example an ACL denial rather than a read-only attribute), the retry at 961 recurses without bound. This path is live via `EmailFiler`.
- **L3** `Cleanup_Files` never resets `_attachmentsAltName` (626), so a `YesToAll`/`NoToAll` alternate-name answer persists for the add-in lifetime.
- **L4** `WriteCSV_StartNewFileIfDoesNotExist` (1398-1429): `Path.Combine(strFileName, strFileLocation)` has its arguments reversed. The condition is inverted relative to the method name. If the branch ran, `SanitizeArray` would dereference the null `strOutput` (1404, 1442).

Follow-ups for promotion: **F1** apply `YesNoToAllPromptSession` to the overwrite and alternate-name prompts in `SaveAttachmentAsync`/`SaveCaseAsync` (J3); **F2** delete the dead `SaveAttachmentsOld` and `IsPicture`; **F3** remove the four exclusions on already-tested members (section 7); L1 to L4.

---

## 10. Toolchain facts for the plan (Q10) `[V]`

- CLAUDE.md order: (1) `dotnet tool run csharpier format .`, verified with `dotnet tool run csharpier check .`, after `dotnet tool restore`; (2) `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; (3) `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` (no `/p:Nullable=enable`, no `/t:Build`); (4) the `test: MSTest with Coverage (Koverage)` task or `scripts/vscode/Invoke-MSTestWithCoverage.ps1`.
- `Invoke-MSTestWithCoverage.ps1`: script parameters `SearchRoot`, `Configuration`, `CoverageOutput` (default `coverage\coverage.cobertura.xml`) and `NoExecute` (lines 1-13). The inner function fixes `ResultsDirectory = 'coverage\test-results'` and `LogFileName = 'mstest-coverage-run.trx'` (297-298). The inner vstest filter is hard-coded to `/TestCaseFilter:TestCategory!=LiveOutlook` (91). Assembly discovery is `*.Test.dll` under `bin\<Configuration>`, excluding `obj`, `ref` and any path whose part relative to the search root contains `.claude\` (348-353). A scoped `SearchRoot` skips threshold assertions (286-291). The entry guard `if ($MyInvocation.InvocationName -ne '.')` (459) makes it safe to dot-source.
- Discrepancy `[I]`: the #945 plan (fact 5, line 72) states that the runner cannot discover assemblies from a `.claude/worktrees/...` worktree. Lines 323-353 make the `.claude\` exclusion relative to `$resolvedSearchRoot`, which is derived from `$ScriptRoot\..\..`, the worktree root when the worktree's own script copy is run. On that reading, a run from the worktree discovers its own assemblies. This was not exercised at runtime here. The planner may keep #945's DIRECT route for parity and determinism, or add a one-line `-NoExecute` probe that prints the discovered count.
- #945 reference template (`docs/features/active/2026-09-30-sort-email-attachment-test-creates-directory-at-repository-root-945/plan.2026-09-30T07-20.md`):
  - Command channel: `pwsh -NoProfile -Command '<payload>'` with `Set-Location -LiteralPath "WORKTREE"` first (line 51). Tools resolved through vswhere (54). `/nodeReuse:false` plus a file logger (55).
  - `CMD-BUILD` (118-135): project build of `UtilitiesCS.Test.csproj`.
  - `CMD-REBUILD` (139-155): solution rebuild gate with `GATEARGS`.
  - `CMD-VSTEST` (157-186): `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_Tests" "/ResultsDirectory:coverage\test-results\945\<task>" "/Logger:trx;LogFileName=<task>.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"`, asserting the TRX `total` because a zero-match filter exits 0 (56).
  - `CMD-COVERAGE-DIRECT` (218-246): `dotnet-coverage collect ... --settings coverage\effective-coverage-945.config -- vstest.console.exe <assemblies> ... "/TestCaseFilter:TestCategory!=LiveOutlook<EXCLUSION>"`.
  - `CMD-COVERAGE-POST` (250-274): `ConvertTo-KoverageCoberturaXml`, floor asserts, `Get-CoberturaFirstPartyCoverageReport`, JaCoCo projection plus reconciliation.
  - `CMD-PACKAGE-COMPARE` (278+): UtilitiesCS package LINE/BRANCH band of 0.10 pp and a per-file uncovered-line delta for `SortEmail.cs` (D-7, line 86).
  - Stall-probe exclusion of four local-hang classes (D-5, line 84; fact 8, line 75). Known flaky test #780 (75).
- Adaptations for #956 `[I]`:
  - Scoped filter covering old and new SortEmail classes: `FullyQualifiedName~EmailIntelligence.SortEmail_`. It matches `SortEmail_Tests` and `SortEmail_TrySaveAttachment_Tests`; Grep shows no other `SortEmail_*` class. Expected total: 15 + 11 = **26** `[P]`.
  - Session filter: `FullyQualifiedName~Dialogs.YesNoToAllPromptSession_Tests` (7 tests, or more with `DataRow` expansion) `[P]`.
  - The #945 per-file Level-1 delta (`SORTEMAIL-UNCOVERED-DELTA`) keys on the filename ending `SortEmail.cs`. After the split, the comparison must aggregate over every filename matching `EmailParsingSorting\SortEmail*.cs` in the final document against `SortEmail.cs` in the baseline, or the delta becomes meaningless. Its expected direction is **downward**: the core becomes measured and covered.
  - A fail-before for the regression tests is a compile-red run: the five-argument overload and `YesNoToAllPromptSession` do not exist until the fix. This is the same shape as #945 D-3 (line 82).

---

## 11. Requirements mapping (for spec.md)

Proposed acceptance criteria for `spec.md`. Numbers come from section 12.

1. Every file `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail*.cs` is under 500 lines. `SortEmail` is `public static partial class`, and no public or internal member signature that existed before is removed or changed.
2. The five new partial files and `Dialogs/YesNoToAllPromptSession.cs` are registered in `UtilitiesCS.csproj`; the two new test files are registered in `UtilitiesCS.Test.csproj`.
3. `TrySaveAttachmentAsync` has a five-argument core taking `createDirectory`, `clearReadOnly` and a `YesNoToAllPromptSession`. The core and the three-argument overload carry no `[ExcludeFromCodeCoverage]`. The two-argument wrapper and the read-only adapter keep or carry it, with an in-code justification comment.
4. No `YesNoToAll.ShowDialog` call remains in the try-save code path. The production prompt is supplied once as `new YesNoToAllPromptSession(YesNoToAll.ShowDialog)`, and `Cleanup_Files` resets it.
5. Tests T1-T11 and S1-S7 exist and pass under the parallel CLI runsettings. No test creates a file or directory (sandbox `Test-Path` before and after, per #945).
6. The 15 existing `SortEmail_Tests` pass unchanged.
7. `spec.md` records each section 6 call site with either "seam" or one of J1-J5.
8. The full toolchain passes. Coverage for the changed lines is not reduced, and the new type is at least 90% covered.

---

## 12. Numeric Derivation Evidence

**N1: SortEmail.cs line count = 1,454**
- Complete Family: physical lines of SRC at the worktree head.
- Exhaustive Search Scope: the whole file.
- Inclusion Rules: every line, including blank lines.
- Exclusion Rules: the display-only empty line after the terminal newline.
- Primary Search Strategy or Query Expression: full Read of SRC; last numbered content line.
- Primary Member Set: lines 1 to 1454.
- Primary Count: 1454.
- Cross-check Search Strategy or Query Expression: the independent records `issue.md` line 16 and #945 `code-review.2026-09-30T13-00.md` line 33 ("Grep line count 1454 at head").
- Cross-check Member Set: lines 1 to 1454.
- Cross-check Count: 1454.
- Member-set Comparison: identical.

**N2: `[ExcludeFromCodeCoverage]` attributes in SRC = 28**
- Complete Family: every application of `ExcludeFromCodeCoverage` in any syntactic form.
- Exhaustive Search Scope: SRC.
- Inclusion Rules: any token `ExcludeFromCodeCoverage`.
- Exclusion Rules: none.
- Primary Search Strategy or Query Expression: Grep content `\[ExcludeFromCodeCoverage\]`.
- Primary Member Set: {42, 76, 112, 181, 210, 303, 455, 564, 636, 661, 701, 762, 825, 839, 893, 912, 986, 1007, 1024, 1059, 1105, 1116, 1125, 1341, 1353, 1368, 1398, 1431}.
- Primary Count: 28.
- Cross-check Search Strategy or Query Expression: Grep count mode on the bare token `ExcludeFromCodeCoverage`, which also catches `Excl(Justification=...)` or combined attribute lists; plus member enumeration from the full read (section 1.2).
- Cross-check Member Set: section 1.2 members marked "yes" (same 28 attribute lines).
- Cross-check Count: 28.
- Member-set Comparison: identical; no non-bracketed form exists.

**N3: live `YesNoToAll.ShowDialog` calls in SRC = 8 (try-save scope: 1)**
- Complete Family: invocations of `YesNoToAll.ShowDialog`.
- Exhaustive Search Scope: SRC.
- Inclusion Rules: executable invocations.
- Exclusion Rules: commented-out text; other dialog types.
- Primary Search Strategy or Query Expression: Grep `YesNoToAll\.ShowDialog\(`, giving {710, 733, 773, 796, 853, 856, 936, 1263, 1280}, minus comment line 856.
- Primary Member Set: {710, 733, 773, 796, 853, 936, 1263, 1280}.
- Primary Count: 8.
- Cross-check Search Strategy or Query Expression: Grep `ShowDialog`, giving 10 lines, minus `InputBox.ShowDialog` 1297 and comment 856.
- Cross-check Member Set: {710, 733, 773, 796, 853, 936, 1263, 1280}.
- Cross-check Count: 8.
- Member-set Comparison: identical.

**N4: test methods in TST = 15**
- Complete Family: `[TestMethod]` members of `SortEmail_Tests`.
- Exhaustive Search Scope: TST.
- Inclusion Rules: `[TestMethod]`-attributed methods.
- Exclusion Rules: helpers.
- Primary Search Strategy or Query Expression: Grep `\[TestMethod\]`.
- Primary Member Set: attribute lines {41, 54, 79, 104, 140, 157, 174, 182, 209, 245, 272, 291, 316, 341, 359}.
- Primary Count: 15.
- Cross-check Search Strategy or Query Expression: method declarations enumerated from the full read (section 8).
- Cross-check Member Set: declaration lines {42, 55, 80, 105, 141, 158, 175, 183, 210, 246, 273, 292, 317, 342, 360}, each the line after an attribute.
- Cross-check Count: 15.
- Member-set Comparison: one-to-one (each declaration is attribute line + 1).

**N5: direct file-system call sites in SRC = 14**
- Complete Family: calls to `System.IO.File.*`, `System.IO.Directory.*`, `new DirectoryInfo`, `new FileInfo`, and the file writer `FileIO2.WriteTextFile`.
- Exhaustive Search Scope: SRC.
- Inclusion Rules: executable call sites.
- Exclusion Rules: pure `Path.*` string operations; COM `SaveAsFile`/`SaveAs` (mockable).
- Primary Search Strategy or Query Expression: Grep `\bFile\.(Delete|Exists|Copy|Move|Open|Read|Write|Create|SetAttributes|GetAttributes)|\bDirectory\.\w+|new DirectoryInfo|new FileInfo|FileIO2\.`.
- Primary Member Set: {245, 369, 515, 704, 767, 902, 944, 1212, 1214, 1216, 1218, 1227, 1406, 1425}.
- Primary Count: 14.
- Cross-check Search Strategy or Query Expression: the broader Grep `File\.|Directory\.|DirectoryInfo|FileInfo|Path\.`, filtered by hand to I/O calls (dropping `Path.*`, `MAX_PATH`, `FolderPath`, `ToFsFolderpath`), plus the `FileIO2.` hit from the member read of 1398-1429.
- Cross-check Member Set: {245, 369, 515, 704, 767, 902, 944, 1212, 1214, 1216, 1218, 1227, 1406} ∪ {1425}.
- Cross-check Count: 14.
- Member-set Comparison: identical. The `Attributes` write at 947 acts on the object from 944 and is treated as part of site 944.

**N6: compiled external call sites of SortEmail members = 4 (production)**
- Complete Family: call sites outside SRC in compiled production projects.
- Exhaustive Search Scope: all `*.cs` in the worktree, filtered by csproj membership and comment status.
- Inclusion Rules: executable calls in compiled files.
- Exclusion Rules: comments; files absent from their csproj; tests (listed separately).
- Primary Search Strategy or Query Expression: Grep `SortEmail` over `*.cs`/`*.csproj`.
- Primary Member Set: {RibbonController.cs:230, AppOlObjects.cs:301, EfcDataModel.cs:309}, with QfcController.cs excluded as not compiled.
- Primary Count: 3 qualified call sites.
- Cross-check Search Strategy or Query Expression: Grep for unqualified extension-call shapes `\.(SaveAttachmentAsync|SaveAttachment|TrySaveAttachmentAsync|IsPicture|...)\(`, giving EmailFiler.cs:445 plus the same three qualified sites via `UndoAsync`, `WriteCSV_...` and `Cleanup_Files`.
- Cross-check Member Set: {RibbonController.cs:230, AppOlObjects.cs:301, EfcDataModel.cs:309, EmailFiler.cs:445}.
- Cross-check Count: 4.
- Member-set Comparison: the union is 4. The primary query cannot see extension calls by design, and the cross-check finds the three primary sites plus EmailFiler. The family count of 4 is asserted from the union, and every member is confirmed by both a qualified-name or extension-shape match and the file read.

Post-change predictions (26 scoped tests, 28 attributes after the change, under-500 files) are `[P]` and must be measured by the plan, not asserted from this section.

---

## Automation Feasibility

No human interaction is expected. Every change is a source or project-file edit plus deterministic unit tests that use Moq-backed COM interfaces and in-memory delegates. No Outlook process, no modal dialog (the prompt is a test delegate), and no file-system access occur in tests. The toolchain (CSharpier, two msbuild rebuilds, vstest under `dotnet-coverage`) runs unattended through the #945 command shapes. Two items need a maintainer decision but not interaction during execution: (a) the J1/J2 justification wording, because UT2 exemption (c) does not list `UtilitiesCS`; (b) whether keeping `[ExcludeFromCodeCoverage]` on the two-argument wrapper satisfies "remove the exclusion from `TrySaveAttachmentAsync`" (this research recommends it: the wrapper's only behavior is real directory creation). Both can be recorded in `spec.md` as decisions without blocking automation.
