# Research: SortEmail latent logic defects L1 to L4 (Issue #959)

- **Issue:** #959 (work mode `full-bug`, `issue.md` line 12)
- **Feature folder:** `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/`
- **Branch:** `bug/sort-email-latent-logic-defects-959`
- **Timestamp:** 2026-10-02T05-15
- **Evidence basis:** every citation was read in this worktree with Read, Grep and Glob. No command was run. Line numbers are those of this branch, after the #956 partial split.
- **Tag legend:** `[V]` verified by reading the cited file; `[I]` inference from verified facts; `[P]` prediction to be confirmed at execution; `[V-web]` web source (none succeeded in this session, see section 4.6).
- **Out of scope:** issue #966 (SortEmail residuals, follow-ups F1 to F3 of the #956 research). Nothing below proposes work on the overwrite or alternate-name prompt seams, the dead `SaveAttachmentsOld`/`IsPicture`, or the exclusions on already-tested helpers, except where a scope boundary must be stated.

> **SCOPE SUPERSEDED (parent parallel-orchestrator notice, 2026-10-02, maintainer directive).** The out-of-scope statement above, section 8, and proposed AC 8 are superseded. Issue #966 is consolidated into this item (see `issue.md`, "Consolidated scope"), and the pull request closes both #959 and #966. In scope in addition to L1 to L4: #966 F1, F2, F3, CR-1, the try-save `Debug.WriteLine` to project logger change plus removal of the ineffective outer rethrow, and removal of unused `using` directives in the SortEmail partials. The section 8 observations in the same files or with the same root cause are also in scope: the missing `try`/`finally` around `Cleanup_Files` in `EfcDataModel.MoveToFolderAsync` (sticky prompt state, same root cause as L3); the private duplicate of L4 in `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs` with the same three defects; `SanitizeArray`'s exclusion (F3); and the CSV header shape, which must be decided (intended or defect) and fixed if a defect. Binding rule for every agent and subagent: any defect found in the same files or with the same root cause is fixed in this item, never listed as a follow-up; only completely unrelated defects may be reported for filing. This research does not yet cover the added items; the spec and plan authors must research them before specifying them.

File aliases used below (all under `UtilitiesCS/EmailIntelligence/EmailParsingSorting/`):

| Alias | File | Lines | `#nullable enable` |
|---|---|---|---|
| A | `SortEmail.AttachmentSaving.cs` | 343 | line 1 `[V]` |
| T | `SortEmail.TrySaveAttachment.cs` | 173 | line 1 `[V]` |
| U | `SortEmail.UndoAndMoveLog.cs` | 196 | line 1 `[V]` |
| S | `SortEmail.cs` (retained partial; `logger` at 25 to 27) | 277 | line 1 `[V]` |
| TST1 | `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` | 458 | none |
| TST2 | `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs` | 376 | none |

Line counts are the last numbered line of each Read.

---

## 1. Current state shared by all four defects `[V]`

### 1.1 The enum and the prompt session

`UtilitiesCS/Dialogs/YesNoToAll.cs` lines 14 to 21:

```csharp
public enum YesNoToAllResponse
{
    Empty = 0,
    Yes = 1,
    No = 2,
    YesToAll = 4,
    NoToAll = 8,
}
```

No `[Flags]` attribute is present (line 13 is blank, line 14 is the declaration). `YesNoToAll.ShowDialog` (65 to 109) returns exactly one of the five values; Cancel maps to `Empty` (47 to 50, 99 to 104).

`UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs` (70 lines, `internal sealed`): `Response` (33), `Ask` (40 to 48, prompts only when `Empty`), `ReleaseSingleAnswer` (54 to 60, clears `Yes`/`No`, keeps `YesToAll`/`NoToAll`), `Reset` (65 to 68).

### 1.2 Static prompt state and `Cleanup_Files` (A 25 to 36)

```csharp
25        private static YesNoToAllResponse _responseSaveFile = YesNoToAllResponse.Empty;
26        private static YesNoToAllResponse _attachmentsOverwrite = YesNoToAllResponse.Empty;
27        private static YesNoToAllResponse _attachmentsAltName = YesNoToAllResponse.Empty;
28        private static YesNoToAllResponse _picturesOverwrite = YesNoToAllResponse.Empty;
29
30        public static void Cleanup_Files()
31        {
32            _responseSaveFile = YesNoToAllResponse.Empty;
33            _attachmentsOverwrite = YesNoToAllResponse.Empty;
34            _picturesOverwrite = YesNoToAllResponse.Empty;
35            RemoveReadOnlyPrompt.Reset();
36        }
```

`RemoveReadOnlyPrompt` is the `private static readonly YesNoToAllPromptSession` at T 28 to 30.

### 1.3 Test infrastructure available

- `InternalsVisibleTo("UtilitiesCS.Test")` and `("DynamicProxyGenAssembly2")`: `UtilitiesCS/Properties/AssemblyInfo.cs` 18 to 19. Internal members and the internal session type are reachable from tests and from Moq.
- `UtilitiesCS.Test/packages.config` line 65: Moq 4.21.0. MSTest and FluentAssertions are referenced (#956 research, plan fact 7).
- Parallelism: `scripts/vscode/TaskMaster.cli.runsettings` lines 5 to 6: `Workers 0`, `Scope ClassLevel`. Test classes run concurrently in one process.
- TST2 (376 lines, 11 tests at 33, 57, 84, 110, 140, 165, 193, 217, 245, 273, 297) has a private helper `SaveAsync` (323 to 331) calling the five-argument core, and a private nested recorder `Seams` (338 to 373): `CreatedDirectories`, `ClearedDirectories`, `PromptMessages`, `ClearException`, a fresh `YesNoToAllPromptSession` per instance, and a scripted answer queue. Constants: `SandboxDirectory`, `SandboxFilePath`, `ExpectedPrompt` (24 to 27).
- TST1 (458 lines, 15 tests) has `CreateAttachmentMock` (386 to 406, `private static`, not shareable), reflection on private static methods of `SortEmail` (346, 363), and `Cleanup_Files_DoesNotThrow` (174 to 180), which calls `SortEmail.Cleanup_Files()` concurrently with any other class.
- Reflection on a private static field: precedent `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` 52 to 54 (`typeof(IdleAsyncQueue).GetField("_subscribeGuard", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, ...)`).
- `Mock<Attachment>` (the `Microsoft.Office.Interop.Outlook.Attachment` interface) with `SaveAsFile` scripted by `SetupSequence(...).Throws(...).Pass()`: TST2 63 to 66 and others. `Callback(...).Throws(...)` chain on a void setup: `QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs` 314 to 315.
- Delegate seam for file existence: `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.Serialization.cs` 28 to 34 (wrapper passes `path => File.Exists(path)`, `path => File.ReadAllText(path)`) and 37 to 43 (`internal static T? DeserializeFromFolder<T>(..., Func<string, bool> fileExists, Func<string, string> readAllText)`).
- Test project registration: `UtilitiesCS.Test/UtilitiesCS.Test.csproj` is a legacy non-SDK project with explicit `<Compile Include>` items; line 98 `EmailIntelligence\SortEmail_Tests.cs`, line 99 `EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs`, line 444 `Dialogs\YesNoToAllPromptSession_Tests.cs`. **Every new test file needs its own entry** or it silently does not compile. Production registration (`UtilitiesCS/UtilitiesCS.csproj` 818 to 823) already lists all six SortEmail partials; no production project-file change is needed for L1 to L4.
- Interop namespace hazard (from the #956 plan, PD-9, confirmed by TST1 45, 58, 178 writing `System.Action`): a test file that imports `Microsoft.Office.Interop.Outlook` must write `System.Action` and `System.Exception`, because the interop namespace declares types named `Action` and `Exception`.

### 1.4 Compiled production entry points into these partials `[V]`

| Site | Member reached | Compiled? |
|---|---|---|
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs` 443 to 446 (`SaveAttachmentAsync(AttachmentHelper)` returns `attachment.SaveAttachmentAsync(Config.SaveFsPath!)`) | A 227 to 239, then A 164 to 225, then T two-argument overload (A 221) or `SaveCaseAsync` (A 179, 203) | yes |
| `QuickFiler/Controllers/EfcDataModel.cs` 309 | `Cleanup_Files` (A 30) | yes |
| `TaskMaster/AppGlobals/AppOlObjects.cs` 301 to 304 | `WriteCSV_StartNewFileIfDoesNotExist(_globals.FS.Filenames.MovedMails, myDocuments)` (U 140) | yes |
| `SortEmail.MailItemSort.cs` 299 (`attachment.SaveAttachment()`), S 165 | `SaveAttachment` (A 104), which calls `SaveCase` | compiled, but `Sort`/`SortAsync` have no compiled external caller (#956 research section 9; `QuickFiler/Legacy/QfcController.cs` is not in `QuickFiler.csproj`) |

---

## 2. L1: `SaveCase` switch labels combine enum values (A 290 to 309)

### 2.1 Current code `[V]`

```csharp
290        [ExcludeFromCodeCoverage]
291        internal static void SaveCase(
292            YesNoToAllResponse response,
293            Attachment attachment,
294            string filePathSave,
295            string filePathSaveAlt
296        )
297        {
298            switch (response)
299            {
300                case (YesNoToAllResponse.NoToAll | YesNoToAllResponse.No):
301                    attachment.SaveAsFile(filePathSaveAlt);
302                    break;
303                case (YesNoToAllResponse.Yes | YesNoToAllResponse.YesToAll):
304                    attachment.SaveAsFile(filePathSave);
305                    break;
306                default:
307                    break;
308            }
309        }
```

`NoToAll | No` is the constant `(YesNoToAllResponse)10` and `Yes | YesToAll` is `(YesNoToAllResponse)5`. The enum has no `[Flags]` attribute and every value that can reach the parameter is one of 0, 1, 2, 4, 8 (section 1.1), so neither label matches and every call takes `default` `[I]`. The compiler accepts the labels because a bitwise-or of enum constants is itself a constant of the enum type.

### 2.2 Intended behavior and values that reach it `[V]`

The asynchronous twin `SaveCaseAsync` (A 241 to 288) uses the correct pattern-with-guard form at 251 to 252 (`case YesNoToAllResponse r when (r == NoToAll || r == No)`) and 279 to 280 (`Yes || YesToAll`). The synchronous intent, read from its own labels and from the twin, is: `No`/`NoToAll` saves to `filePathSaveAlt`; `Yes`/`YesToAll` saves to `filePathSave`; `Empty` does nothing. (The synchronous version does not show the alternate-name prompt; this is pre-existing and not changed.)

Callers: A 116 to 121 and 139 to 144 inside `SaveAttachment` (A 104), which passes `_picturesOverwrite` or `_attachmentsOverwrite` after the overwrite prompt; both are values from `YesNoToAll.ShowDialog`, so 0, 1, 2, 4 or 8. `SaveAttachment` is reached only from `Sort` (`SortEmail.MailItemSort.cs` 299), which has no compiled caller, so L1 is latent in production `[I]`.

### 2.3 Minimal fix

Replace the two combined labels with stacked single-value labels:

```csharp
case YesNoToAllResponse.NoToAll:
case YesNoToAllResponse.No:
    attachment.SaveAsFile(filePathSaveAlt);
    break;
case YesNoToAllResponse.Yes:
case YesNoToAllResponse.YesToAll:
    attachment.SaveAsFile(filePathSave);
    break;
```

Rejected alternatives: `HasFlag` (the enum is not a flags enum and no caller ever passes a combination; `HasFlag` would also make `Empty` (0) match every case); mirroring the `when` guards of `SaveCaseAsync` (correct but longer than needed).

Remove `[ExcludeFromCodeCoverage]` at A 290: the method becomes directly tested and contains no file-system or dialog call (`Attachment.SaveAsFile` is a mockable COM interface member, as TST1 253 and TST2 39 show). This follows the #956 precedent of removing the exclusion from a member once it is covered (spec AC4).

### 2.4 Observability and test

`SaveCase` is `internal static`, reachable from `UtilitiesCS.Test` through `InternalsVisibleTo`. A test passes a `Mock<Attachment>(MockBehavior.Loose)` and two rooted literal paths (no file is created because `SaveAsFile` is a mock), then verifies `SaveAsFile(alt)` or `SaveAsFile(main)` with `Times.Once` and the other with `Times.Never`.

| Test | Rows | Pre-fix | Post-fix |
|---|---|---|---|
| `SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath` | `[DataRow(No)]`, `[DataRow(NoToAll)]` | RED (`Times.Once` sees 0 calls) | GREEN |
| `SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath` | `[DataRow(Yes)]`, `[DataRow(YesToAll)]` | RED | GREEN |
| `SaveCase_WhenAnswerIsEmpty_DoesNotSave` | one | GREEN (control) | GREEN |

Red-first is possible with no production change. Location: new file `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` (shared with L3; see section 6.2 for why TST1 is not extended).

---

## 3. L2 (High): unbounded retry with a sticky `YesToAll` (T 87 to 160)

### 3.1 Current code `[V]`

```csharp
 87        internal static async Task<bool> TrySaveAttachmentAsync(
 88            this Attachment attachment,
 89            string filePathSave,
 90            Action<string> createDirectory,
 91            Action<string> clearReadOnly,
 92            YesNoToAllPromptSession removeReadOnlyPrompt
 93        )
 94        {
 95            try
 96            {
 97                createDirectory(Path.GetDirectoryName(filePathSave));
 98                await Task.Run(() => attachment.SaveAsFile(filePathSave));
 99                return true;
100            }
101            catch (System.UnauthorizedAccessException e)
102            {
103                Debug.WriteLine(e.Message);
...
108                if (removeReadOnlyPrompt.Response == YesNoToAllResponse.Empty)
109                {
...
112                    removeReadOnlyPrompt.Ask(message);
113                }
114
115                if (
116                    (removeReadOnlyPrompt.Response == YesNoToAllResponse.Yes)
117                    || (removeReadOnlyPrompt.Response == YesNoToAllResponse.YesToAll)
118                )
119                {
120                    var directory = Path.GetDirectoryName(filePathSave);
121                    try
122                    {
123                        clearReadOnly(directory);
124                    }
125                    catch (System.Exception inner)
126                    {
127                        Debug.WriteLine(inner.Message);
128                        return false;
129                    }
130                    finally
131                    {
132                        removeReadOnlyPrompt.ReleaseSingleAnswer();
133                    }
134                    return await TrySaveAttachmentAsync(
135                        attachment,
136                        filePathSave,
137                        createDirectory,
138                        clearReadOnly,
139                        removeReadOnlyPrompt
140                    );
141                }
142                else if (
143                    (removeReadOnlyPrompt.Response == YesNoToAllResponse.No)
144                    || (removeReadOnlyPrompt.Response == YesNoToAllResponse.NoToAll)
145                )
146                {
147                    Debug.WriteLine($"The file {filePathSave} was not saved.");
148                    removeReadOnlyPrompt.ReleaseSingleAnswer();
149                    return false;
150                }
151                else
152                {
153                    throw;
154                }
155            }
156            catch (System.Exception)
157            {
158                throw;
159            }
160        }
```

The two-argument overload (T 40 to 51, `[ExcludeFromCodeCoverage]`) forwards to the three-argument overload (T 60 to 73) with real `Directory.CreateDirectory`; the three-argument overload forwards to the core with `ClearReadOnlyAttributeOnDisk` (T 165 to 170, `[ExcludeFromCodeCoverage]`) and the production `RemoveReadOnlyPrompt` (T 28 to 30).

### 3.2 Exact loop trace `[V]`

The retry is the recursive call at 134 to 140, which passes the same five arguments. With `SaveAsFile` throwing `UnauthorizedAccessException` on every call and `clearReadOnly` succeeding:

1. Call 1: 97 `createDirectory`; 98 throws; 108 `Response == Empty`, so 112 `Ask` returns `YesToAll`; 115 to 118 true; 123 `clearReadOnly` succeeds; 132 `ReleaseSingleAnswer` keeps `YesToAll`; 134 recursion.
2. Call 2: 97; 98 throws; 108 false (`YesToAll`), no prompt; 115 to 118 true; 123 succeeds; 132 keeps `YesToAll`; 134 recursion.
3. Call n: identical to call 2.

There is no attempt counter, no state that changes between iterations, and no condition that becomes false. The only exits are: `SaveAsFile` succeeds (99), `clearReadOnly` throws (128), or a non-`UnauthorizedAccessException` escapes (156 to 158). Because every level awaits `Task.Run`, the recursion does not grow the thread stack; it is an asynchronous loop that keeps the thread pool busy and allocates one awaiting state machine per iteration that is never released while the loop runs `[I]`.

With a single `Yes`, 132 releases the answer, so call 2 prompts again at 112 (this is TST2 test T11, 297 to 317, bounded by the user). With `No`/`NoToAll`, 149 returns `false`. With `Empty` (Cancel), 153 rethrows (T7, 193 to 211). The unbounded case is exactly: `YesToAll` held, `clearReadOnly` succeeds, `SaveAsFile` keeps throwing `UnauthorizedAccessException`.

Plausible production trigger `[I]`: `SaveCaseAsync` A 281 calls the two-argument overload with `filePathSave` when the file already exists and the overwrite answer is `Yes`/`YesToAll`. If that existing file is itself read-only (or otherwise denied) and the interop surfaces the denial as `UnauthorizedAccessException`, clearing the directory's read-only attribute succeeds without changing the file, and the loop runs. Not reproduced at runtime; the issue records the same (`issue.md` line 43).

### 3.3 Every caller and how each handles each outcome `[V]`

| Caller | `true` | `false` | exception |
|---|---|---|---|
| A 221 to 223 (`SaveAttachmentAsync`, file does not exist) | `await` discards the bool | discarded; the method returns normally with the attachment unsaved | propagates |
| A 266 (`SaveCaseAsync`, alternate name) | discarded | discarded | propagates |
| A 281 (`SaveCaseAsync`, overwrite) | discarded | discarded | propagates |
| T 46 to 50 (two-argument wrapper) and T 66 to 72 (three-argument forward) | returned | returned | propagates |
| T 134 (recursion) | returned | returned | propagates |
| TST1 257, 281 (three-argument) | asserted | not exercised | asserted (`IOException`) |
| TST2 325 (five-argument, all 11 tests) | asserted | asserted | asserted (T7 `UnauthorizedAccessException`, T10 `IOException`) |

Upstream of A 221/266/281, the production chain is: `EmailFiler.SaveAttachmentAsync` 443 to 446 (returns the `Task<bool>` typed as `Task`, bool unreadable) -> `SaveAttachmentsPicturesAsync` 279 to 282 (`await SaveAttachmentAsync(x)` inside `ForEachAsync`, discards) -> `ProcessMailHelperAsync` 164 (`await`, then the mail is moved at 169 to 170) -> `SortAsync` 146 to 149 (`await` per mail helper, no `catch`) -> `EfcDataModel.MoveToFolderAsync` 308 (`await InvokeFilerAsync`, no `try`/`finally`; `Cleanup_Files` at 309 runs only on normal completion) -> `EfcFormController.Actions.cs` 135 to 144, `EfcFormController.EventHandlers.cs` 184 to 191, `EfcHomeController.ExecuteMoves.cs` 98 to 104 (each awaits without a `catch` in the read ranges). The terminal sink of a propagated exception beyond those three sites was not traced in this session `[I]`.

Consequences for the "surface the error" decision:

- **`false` does not surface.** Every production caller discards the bool. A `false` return would silently leave the attachment unsaved and `ProcessMailHelperAsync` 169 would still move the mail. That is the behavior the issue's Expected section rejects, and it is the "silently ignore errors" case of CLAUDE.md General Code Change Policy section 3.
- **Rethrow surfaces.** The same `UnauthorizedAccessException` already propagates on the Cancel path (153) and every other exception propagates at 158; the existing XML contract (T 83 to 86) names rethrow as the cancelled-prompt outcome. A rethrow stops the filing operation before the mail is moved without its attachment (fail fast), which is the existing behavior for T7 and T10.
- Pre-existing residual, out of scope: because `EfcDataModel` 308 to 309 has no `try`/`finally`, any exception skips `Cleanup_Files`, so sticky answers persist to the next operation. This is true today for T7 and T10 paths and is recorded for promotion, not fixed here.

### 3.4 Recommended minimal bounded fix

Rule: a clear-and-retry may happen at most once per call while the held answer cannot be asked again. After the attribute has been cleared once in this call, a second denial with a sticky `YesToAll` cannot be changed by clearing again, so the error is surfaced by rethrowing the original exception.

Shape (keeps the five-argument signature that 11 tests and the three-argument forward pin):

```csharp
internal static Task<bool> TrySaveAttachmentAsync(
    this Attachment attachment, string filePathSave, Action<string> createDirectory,
    Action<string> clearReadOnly, YesNoToAllPromptSession removeReadOnlyPrompt)
{
    return TrySaveAttachmentCoreAsync(attachment, filePathSave, createDirectory,
        clearReadOnly, removeReadOnlyPrompt, isRetryAfterClear: false);
}

private static async Task<bool> TrySaveAttachmentCoreAsync(
    Attachment attachment, string filePathSave, Action<string> createDirectory,
    Action<string> clearReadOnly, YesNoToAllPromptSession removeReadOnlyPrompt,
    bool isRetryAfterClear)
{
    try { /* lines 97 to 99 unchanged */ }
    catch (System.UnauthorizedAccessException e)
    {
        Debug.WriteLine(e.Message);

        // The attribute was already cleared once in this call and a "YesToAll" answer is
        // never asked again, so another clear-and-retry cannot change the outcome (#959 L2).
        if (isRetryAfterClear && removeReadOnlyPrompt.Response == YesNoToAllResponse.YesToAll)
        {
            logger.Error($"The file {filePathSave} is still denied after the read-only attribute was cleared.", e);
            throw;
        }

        /* lines 108 to 154 unchanged, except the recursion at 134 to 140 becomes
           return await TrySaveAttachmentCoreAsync(..., isRetryAfterClear: true); */
    }
    catch (System.Exception) { throw; }
}
```

Why this condition and not `isRetryAfterClear` alone: after a single `Yes`, line 132 releases the answer, so a retry that is denied again must re-prompt (T11 asserts two prompts). On a retry the held answer is either `Empty` (was `Yes`) or `YesToAll` (sticky); `No`/`NoToAll` cannot be held on entry to a retry because a retry follows only `Yes`/`YesToAll`. Checking `== YesToAll` states the causal rule directly; `!= Empty` is equivalent in reachable states.

Why `throw;` and `logger.Error`: section 3.3. `logger` is the log4net field at S 25 to 27, visible to every partial; using it follows the project logging pattern (CLAUDE.md C#4.2) where the handler today uses only `Debug.WriteLine`.

Behavior preserved: T1 to T11 outcomes are unchanged `[I]` (T3 and T4 retry once and succeed; T11 retries after `Yes` with `Response == Empty`, so the guard is false and the second prompt happens; T8 and T9 return `false` from the inner catch before any retry). The `createDirectory` call stays outside the handler (pinned by TST1 273 to 289). The outer `catch (System.Exception) { throw; }` stays.

Rejected alternatives:

| Alternative | Why rejected |
|---|---|
| Return `false` after the second denial | Not surfaced by any production caller (3.3); the mail would be moved without its attachment. |
| Fixed retry count (for example three) | Arbitrary. After one successful clear, repeating the clear cannot change the result; a count hides the causal rule and multiplies the attribute writes. |
| Rewrite the recursion as a `while` loop with an attempt counter | Larger restructure of a method whose branch structure is pinned by 11 tests and by the #956 AC6/AC15 brace analysis; not the minimal change. |
| Add a sixth optional parameter to the internal five-argument overload | Works, but a private core keeps the pinned signature byte-identical and keeps the flag out of the internal surface. |
| `isRetryAfterClear` alone (no answer check) | Breaks T11 (second prompt after a single `Yes` would no longer happen). |

### 3.5 Test: deterministic detection of the unbounded loop without timing

Persistent failure without the file system: `SaveAsFile` is a mocked COM interface member; script it to throw on every call with a plain `Setup`, not `SetupSequence`:

```csharp
var denied = new UnauthorizedAccessException("denied");
attachment.Setup(x => x.SaveAsFile(SandboxFilePath)).Throws(denied);
```

Bounding the loop without `Task.Delay`, `Thread.Sleep` or a timeout: the `createDirectory` seam runs once per attempt (T 97) and is outside the `UnauthorizedAccessException` handler, so a counting fake that throws a sentinel after N calls terminates the pre-fix loop deterministically. Extend the existing `Seams` recorder in TST2 (338 to 373) with a tripwire:

```csharp
public int CreateDirectoryLimit { get; set; } = int.MaxValue;

public void CreateDirectory(string path)
{
    if (CreatedDirectories.Count >= CreateDirectoryLimit)
    {
        throw new InvalidOperationException("retry bound exceeded");
    }
    CreatedDirectories.Add(path);
}
```

Pre-fix, the sentinel thrown at T 97 inside a recursive call propagates through `return await` at 134 (an exception raised inside a `catch` block is not caught by the sibling `catch (System.Exception)` of the same `try`) and reaches the test as `InvalidOperationException`; the assertion `ThrowAsync<UnauthorizedAccessException>` fails: RED. Post-fix, the second attempt rethrows `denied` before a third `createDirectory` call: GREEN.

Proposed test T12 in TST2:

```
TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear
Arrange: Seams(YesToAll) { CreateDirectoryLimit = 3 }; SaveAsFile always throws `denied`.
Act:     Func<Task> act = () => SaveAsync(attachment, seams);
Assert:  (await act.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Should().BeSameAs(denied);
         seams.CreatedDirectories.Should().Equal(SandboxDirectory, SandboxDirectory);   // two attempts
         seams.ClearedDirectories.Should().Equal(SandboxDirectory);                      // one clear
         seams.PromptMessages.Should().Equal(ExpectedPrompt);                            // one prompt
         seams.Session.Response.Should().Be(YesNoToAllResponse.YesToAll);                // sticky answer kept for Cleanup_Files
         attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Exactly(2));
```

`throw;` preserves the exception instance and `await` rethrows the same instance from `Task.Run`, so `BeSameAs` holds `[I]`. T3, T4 and T11 remain as the controls that the bound does not fire on a successful retry or on a re-prompted single `Yes`.

Size: TST2 is 376 lines; one test plus the tripwire property is about 45 lines, predicted about 425 after CSharpier `[P]`, under 500. If a second test is wanted (for example asserting that the limit tripwire itself fails loudly), keep the file under 500 or split per the #956 size rule.

### 3.6 Moq note `[I]`

Whether an exhausted `SetupSequence` repeats its last action or falls back to the default (a no-op for a void member) was not verified: two WebFetch attempts on the Moq changelog returned HTTP 404. The design above does not depend on it, because the persistent failure uses `Setup(...).Throws(...)`, which applies to every call. The existing `SetupSequence(...).Throws(...).Pass()` tests (TST2 63 to 66 and others) are unaffected.

---

## 4. L3: `Cleanup_Files` never resets `_attachmentsAltName` (A 27, 30 to 36)

### 4.1 What `Cleanup_Files` resets and what it misses `[V]`

Reset today: `_responseSaveFile` (32), `_attachmentsOverwrite` (33), `_picturesOverwrite` (34), and the read-only session through `RemoveReadOnlyPrompt.Reset()` (35). Missed: `_attachmentsAltName` (27).

### 4.2 What `_attachmentsAltName` is used for `[V]`

Nine occurrences, all in A: the field (27), and inside `SaveCaseAsync` 253, 255, 262, 263, 271, 272, 275 (258 is a comment). Flow (A 251 to 277): when the overwrite answer is `No`/`NoToAll`, the alternate-name prompt is shown only while the field is `Empty` (253 to 259); `Yes`/`YesToAll` saves to `filePathSaveAlt` (261 to 267); `Yes`/`No` are released (270 to 276) and `YesToAll`/`NoToAll` persist. Because `Cleanup_Files` (called after each filing operation at `EfcDataModel.cs` 309) does not reset it, a `YesToAll` or `NoToAll` alternate-name answer persists for the add-in lifetime, and later filing operations never see the alternate-name prompt. This path is live: `EmailFiler.cs` 445 -> A 227 to 239 -> A 164 to 225 -> A 203 to 208 -> `SaveCaseAsync`.

### 4.3 Minimal fix

Add one statement to `Cleanup_Files`, placed with its siblings:

```csharp
_attachmentsAltName = YesNoToAllResponse.Empty;
```

This does not conflict with a later per-call session for the alternate-name prompt (follow-up F1, #966): that change would replace the field and the reset together.

### 4.4 Observability

The field is `private static`; its only reader is `SaveCaseAsync`, which shows a modal dialog (255) and calls the two-argument `TrySaveAttachmentAsync` (266, 281) that creates a real directory. Behavioral observation through a second save is therefore not possible without the F1 seams, which are out of scope. Options:

| Option | Verdict |
|---|---|
| Reflection on the private static field (`typeof(SortEmail).GetField("_attachmentsAltName", BindingFlags.NonPublic | BindingFlags.Static)`) | **Recommended.** In-repo precedent for private static fields: `IdleAsyncQueue_Tests.cs` 52 to 54; for private members of `SortEmail` by name: TST1 346, 363. No production change. |
| An `internal static` accessor added for tests | Rejected: adds production surface whose only consumer is a test. |
| Convert the field to a `YesNoToAllPromptSession` and test `Reset` | Rejected here: this is F1 (#966). |

Proposed test (new file `SortEmail_AttachmentSaving_Tests.cs`):

```
Cleanup_Files_ResetsEveryPromptAnswerField
[DataRow("_responseSaveFile")] [DataRow("_attachmentsOverwrite")]
[DataRow("_attachmentsAltName")] [DataRow("_picturesOverwrite")]
Arrange: field = typeof(SortEmail).GetField(name, NonPublic | Static); field.Should().NotBeNull();
         field.SetValue(null, YesNoToAllResponse.YesToAll);
Act:     SortEmail.Cleanup_Files();
Assert:  field.GetValue(null).Should().Be(YesNoToAllResponse.Empty);
```

Pre-fix: the `_attachmentsAltName` row is RED (the value read back is `YesToAll`); the other three rows are GREEN controls. Post-fix: all four GREEN.

Parallel-safety argument (must be recorded in the change description as a UT5 call-out, because the test writes static state of `SortEmail`): the only writers of these four fields are `Cleanup_Files` (A 30 to 36) and the dialog-driven members `SaveAttachment`, `SaveAttachmentAsync`, `SaveCaseAsync` and `SaveAttachmentsOld`, none of which any test executes (they need a modal dialog or the real file system). The only concurrent writer in a test run is `Cleanup_Files_DoesNotThrow` (TST1 174 to 180), which writes `Empty`, the value the assertion expects. Pre-fix it cannot write `_attachmentsAltName` at all, so it cannot cause a false pass; post-fix a concurrent reset can only make the assertion true earlier. No test writes a non-`Empty` value except this one, for its own row, immediately before its own `Cleanup_Files` call. The test is therefore order-independent in both states and needs no `[DoNotParallelize]`.

---

## 5. L4: `WriteCSV_StartNewFileIfDoesNotExist` (U 139 to 170)

### 5.1 Current code `[V]`

```csharp
139        [ExcludeFromCodeCoverage]
140        public static void WriteCSV_StartNewFileIfDoesNotExist(
141            string strFileName,
142            string strFileLocation
143        )
144        {
145            string[]? strOutput = null;
146            string[,]? strAryOutput;
147            if (File.Exists(Path.Combine(strFileName, strFileLocation)))
148            {
149                strAryOutput = new string[14, 2];
150
151                strAryOutput[1, 1] = "Triage";
...
163                strAryOutput[13, 1] = "FlaggedAsTask";
164
165                SanitizeArray(strAryOutput, ref strOutput);
166                FileIO2.WriteTextFile(strFileName, strOutput!, folderpath: strFileLocation);
167            }
168            strOutput = null;
169            strAryOutput = null;
170        }
```

`SanitizeArray` (U 172 to 193, `[ExcludeFromCodeCoverage]` at 172) writes `strOutput![j] = ...` at 183 for `j` in 0 to 13.

### 5.2 Three defects in the method `[V]`

- **L4a, reversed `Path.Combine`.** `FileIO2.WriteTextFile(string filename, string[] strOutput, string folderpath)` (`UtilitiesCS/To Depricate/FileIO2.cs` 36 to 48) builds `Path.Combine(folderpath, filename)` at 40, and line 166 passes `strFileName` as the file name and `strFileLocation` as the folder. The existence check at 147 combines them in the opposite order. The sole caller passes a file name and a rooted folder (`AppOlObjects.cs` 301 to 304: `_globals.FS.Filenames.MovedMails`, `myDocuments`); `Path.Combine(fileName, rootedFolder)` returns the rooted folder itself, so `File.Exists` tests a directory path and returns false `[I]`.
- **L4b, inverted condition.** The name says "start new file if does not exist"; the body writes when `File.Exists` is true.
- **L4c, null output array.** `strOutput` is `null` at 145 and is never allocated; `SanitizeArray` assigns `strOutput![j]`, so the branch, once reachable, throws `NullReferenceException` on its first row `[I]`. The #956 research recorded this (section 9, L4). It is not listed in `issue.md` line 20, but fixing L4a and L4b alone would turn a guaranteed no-op into a crash in `AppOlObjects.LoadEmailMoveWriter` at add-in start when the moved-mails file is absent. L4c must be fixed in the same change.

Combined effect today: the branch is never entered, the method is a no-op, and the moved-mails log is never seeded with a header. The caller then appends TSV lines through `FileIO2.WriteTextFileAsync(_globals.FS.Filenames.MovedMails, items, myDocuments)` (`AppOlObjects.cs` 314 to 318), the same file.

### 5.3 Intended behavior

When `Path.Combine(strFileLocation, strFileName)` does not exist, write the header rows through `FileIO2.WriteTextFile(strFileName, lines, folderpath: strFileLocation)`; when it exists, do nothing. The as-coded header is a 14-element array whose element 0 is empty and whose elements 1 to 13 are the column names, each written as its own line (`SanitizeArray` joins the columns of each row; every row has at most one non-empty value). This research does not redesign the content: the issue's Expected statement is "creates the file at the correct path, and only when it is absent" (`issue.md` line 39). The one-name-per-line shape is recorded as an observation for the maintainer (section 8).

Callers: `AppOlObjects.cs` 301 (compiled, once per `EmailMoveWriter` load). `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs` 223 calls its own private copy at 285 to 315, which carries the same three defects; it is a different project and member and is out of scope (section 8).

### 5.4 Minimal fix

```csharp
if (!File.Exists(Path.Combine(strFileLocation, strFileName)))     // L4a, L4b
{
    strAryOutput = new string[14, 2];
    ...
    strOutput = new string[strAryOutput.GetLength(0)];              // L4c
    SanitizeArray(strAryOutput, ref strOutput);
    FileIO2.WriteTextFile(strFileName, strOutput, folderpath: strFileLocation);
}
```

With `strOutput` assigned before use, the `!` at 166 can be dropped; nullable flow analysis (file is `#nullable enable`) sees a non-null value in the branch `[I]`. Lines 168 to 169 (dead null assignments) can stay.

### 5.5 Seams: none exist; the minimal seam and whether it widens scope

Grep over the three partials shows no `Func<string, bool>` or write delegate in `SortEmail`; the only seams are the three in `TrySaveAttachmentAsync` (T 60 to 93), none of which reaches U 147 or U 166. The `createDirectory` delegate from #956 is not usable here: it is a parameter of a different method.

Minimal seam, following the `EmailDataMiner.Serialization.cs` 28 to 43 and T 40 to 73 pattern (delegate seam for a single call path, `.claude/rules/csharp.md` "DI Seams" item 2):

```csharp
// Excluded from coverage: the only behavior of this wrapper is wiring the real file-system
// defaults; calling it from a test would read and write the real disk (UT4).
[ExcludeFromCodeCoverage]
public static void WriteCSV_StartNewFileIfDoesNotExist(string strFileName, string strFileLocation)
{
    WriteCSV_StartNewFileIfDoesNotExist(strFileName, strFileLocation, File.Exists, FileIO2.WriteTextFile);
}

internal static void WriteCSV_StartNewFileIfDoesNotExist(
    string strFileName,
    string strFileLocation,
    Func<string, bool> fileExists,
    Action<string, string[], string> writeTextFile)
{
    /* body of 145 to 169 with the fixes of 5.4, calling fileExists(...) and
       writeTextFile(strFileName, strOutput, strFileLocation) */
}
```

Method-group conversions: `File.Exists` has a single `(string)` overload and `FileIO2.WriteTextFile(string, string[], string)` matches `Action<string, string[], string>`, so no lambda is needed in the excluded wrapper. This matters for coverage mechanics: a lambda inside an excluded member can leak into the measured denominator (#956 plan PD-7 and the closure-filter notes), while a method group has no closure.

Scope assessment: the public two-argument signature and the `AppOlObjects.cs` caller are unchanged; the addition is one `internal` overload plus moving the exclusion to the wrapper (the same shape #945 and #956 used for `TrySaveAttachmentAsync`). The issue's Proposed Fix (`issue.md` line 65) requires a failing regression test before each fix, and without a seam the only possible test would touch the real file system, which UT4 prohibits with no approved exception. The seam is therefore a precondition of the mandated bugfix workflow rather than a widening; this is a planner/orchestrator decision to record in `spec.md`. `SanitizeArray`'s exclusion at U 172 is left as is (F3, #966), even though the new tests exercise it.

### 5.6 Tests (new file `UtilitiesCS.Test/EmailIntelligence/SortEmail_UndoAndMoveLog_Tests.cs`)

Recording fakes: `fileExists` records the queried path and returns a scripted bool; `writeTextFile` records `(fileName, lines, folder)`. Rooted literal inputs, for example `C:\Sortemail959Sandbox\logs` and `MovedMails.txt`; nothing touches the disk because both delegates only record.

| Test | Scenario | Asserts | Pre-fix (seam landed, logic unfixed) | Post-fix |
|---|---|---|---|---|
| L4-T1 | `fileExists` returns false | queried path `== Path.Combine(folder, fileName)`; `writeTextFile` called once with `(fileName, folder)`; `lines.Length == 14`; `lines[1] == "Triage"`; `lines[13] == "FlaggedAsTask"` | RED: queried path is `Path.Combine(fileName, folder)`; no write | GREEN |
| L4-T2 | `fileExists` returns true | `writeTextFile` never called | RED: branch entered, `NullReferenceException` from `SanitizeArray` (L4c) | GREEN |

Red-first sequencing: the four-argument overload does not exist yet, so a test written against it is compile-red against the current tree (the #956 AC12 shape). For a behavioral red, land the seam overload first as a pure forward of the current body (behavior-preserving), observe L4-T1 and L4-T2 fail as described, then apply the 5.4 fixes. Both forms satisfy the bugfix workflow; the behavioral form is more informative and is recommended.

`Path.Combine` in the assertion is a pure string operation and is already used the same way in TST1 251 and 277.

---

## 6. Cross-cutting facts for the plan

### 6.1 Files expected to change, with line counts and attributes `[V]`

| File | Lines now | `#nullable enable` | Exclusions involved | Predicted after `[P]` |
|---|---|---|---|---|
| A `SortEmail.AttachmentSaving.cs` | 343 | yes | A 290 on `SaveCase`: **remove** | about 346 |
| T `SortEmail.TrySaveAttachment.cs` | 173 | yes | T 40 (two-argument wrapper) and T 165 (adapter): keep; the new private core carries none | about 215 |
| U `SortEmail.UndoAndMoveLog.cs` | 196 | yes | U 139: **move** to the two-argument wrapper with a justification comment; the seamed core carries none; U 172 (`SanitizeArray`) unchanged | about 235 |
| TST2 `SortEmail_TrySaveAttachment_Tests.cs` | 376 | no | n/a | about 425 |
| new `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` | 0 | no | n/a | about 150 to 200 |
| new `UtilitiesCS.Test/EmailIntelligence/SortEmail_UndoAndMoveLog_Tests.cs` | 0 | no | n/a | about 120 to 160 |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | n/a | n/a | n/a | +2 `<Compile Include>` lines after line 99, four-space indent, backslash separators, self-closing |

No production project-file change. No caller change (`EmailFiler.cs`, `EfcDataModel.cs`, `AppOlObjects.cs` unchanged). `YesNoToAll.cs` and `YesNoToAllPromptSession.cs` unchanged. All three production files are nullable-enabled, so new code must be null-clean under `/p:TreatWarningsAsErrors=true` (CLAUDE.md C#1.3); the delegate parameters are non-nullable and the private core's `bool` parameter adds no nullable state.

### 6.2 Why new test files rather than extending TST1

TST1 is 458 lines. The L1 and L3 tests are about 100 lines together, which would exceed the 500-line limit. The #956 constraint that TST1 stay byte-identical was specific to that item; it is not binding here, but the size limit is. The new files mirror the partial they test, following the `SortEmail_TrySaveAttachment_Tests` naming. L2's test extends TST2 because it reuses the private `Seams` recorder and the `SaveAsync` helper, which a new file could not share.

### 6.3 Red-first summary

| Defect | Red mechanism | Needs a production pre-step? |
|---|---|---|
| L1 | `Times.Once` on the mocked `SaveAsFile` observes 0 calls | no |
| L2 | tripwire sentinel `InvalidOperationException` replaces the expected `UnauthorizedAccessException` | no |
| L3 | reflection reads `YesToAll` back after `Cleanup_Files` | no |
| L4 | queried path reversed and no write (T1); `NullReferenceException` (T2) | yes: the seam overload (behavior-preserving) must exist first, or the red is a compile failure |

### 6.4 Policy constraints met by the designs

MSTest, Moq, FluentAssertions only; no temporary files or real file-system writes (all paths are recorded by delegates or passed to a mocked `SaveAsFile`); no `[DoNotParallelize]`; no `Thread.Sleep`/`Task.Delay`/timeouts (the L2 bound is a call-count tripwire); class-level parallel run (section 1.3). The one deliberate deviation is L3's write to static state through reflection, with the order-independence argument of section 4.4, to be stated in the change description (UT5).

### 6.5 Coverage mechanics to carry into the plan

- The #956 AC15 exemptions (the two unreachable braces after `throw;` in the `UnauthorizedAccessException` handler, `SortEmail.TrySaveAttachment.cs`) are content-identified; the L2 change adds a second `throw;` inside that handler and moves the body into a private core, so the #959 coverage comparison needs its own baseline and its own exemption analysis rather than reusing #956's figures.
- New or changed members must reach at least 90 percent line coverage (CLAUDE.md UT2): the private core, the fixed `SaveCase`, the seamed `WriteCSV_StartNewFileIfDoesNotExist` core, and `Cleanup_Files`.
- Prefer method groups over lambdas in the two excluded wrappers (sections 5.5 and T 46 to 50, where the existing `path => System.IO.Directory.CreateDirectory(path)` lambda is already the subject of the #956 AC15 exemption (1)).

---

## 7. Behavior semantics after the fixes

| Member | Input state | Outcome |
|---|---|---|
| `SaveCase` | `No`/`NoToAll` | one `SaveAsFile(filePathSaveAlt)` |
| `SaveCase` | `Yes`/`YesToAll` | one `SaveAsFile(filePathSave)` |
| `SaveCase` | `Empty` | no call |
| five-argument `TrySaveAttachmentAsync` | save succeeds (first or retry) | `true` |
| same | denied; answer `No`/`NoToAll` | `false`; single answer released |
| same | denied; `Yes`/`YesToAll`; clear throws | `false`; single answer released |
| same | denied; `Yes`/`YesToAll`; clear ok; retry succeeds | `true` (T2, T3, T4 unchanged) |
| same | denied; `Yes`; clear ok; retry denied | re-prompt (T11 unchanged) |
| same | denied; `YesToAll`; clear ok; retry denied | **new:** `logger.Error`, rethrow the original `UnauthorizedAccessException`; exactly one clear, two save attempts, one prompt; `YesToAll` stays held until `Cleanup_Files` |
| same | denied; `Empty` (Cancel) | rethrow (T7 unchanged) |
| same | non-access exception | propagates (T10 unchanged) |
| `Cleanup_Files` | any | all four enum fields `Empty`; `RemoveReadOnlyPrompt.Response == Empty` |
| `WriteCSV_StartNewFileIfDoesNotExist` (seamed) | `fileExists(Combine(location, name))` false | one `writeTextFile(name, 14 lines, location)` |
| same | `fileExists` true | no write |

---

## 8. Observations outside this issue's scope (record, do not fix)

- `EfcDataModel.MoveToFolderAsync` 308 to 309 has no `try`/`finally`, so any exception from the filer skips `Cleanup_Files` and sticky answers persist into the next operation (pre-existing for the Cancel and non-access paths; the L2 rethrow takes the same route).
- The header written by `WriteCSV_StartNewFileIfDoesNotExist` is one column name per line (14 lines, the first empty), in both this copy and the `ToDoModel` copy; a single tab-separated header line may have been intended.
- `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs` 285 to 315 is a private duplicate of L4 with the same three defects.
- `SanitizeArray` (U 172) remains `[ExcludeFromCodeCoverage]` while exercised by the L4 tests (F3, #966).
- Follow-ups F1 to F3 of the #956 research are tracked under #966 and were not researched here.

---

## 9. Requirements mapping: proposed acceptance criteria for `spec.md`

1. **L1.** `SaveCase` (A) has four single-value `case` labels (`NoToAll`, `No` -> `filePathSaveAlt`; `Yes`, `YesToAll` -> `filePathSave`) and no `|` in any label; its `[ExcludeFromCodeCoverage]` is removed; the three L1 tests of section 2.4 exist in `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` and pass; the two positive tests were observed RED before the fix.
2. **L2.** The internal five-argument `TrySaveAttachmentAsync` keeps its signature and forwards to a private core carrying an `isRetryAfterClear` flag; when the flag is true and the session holds `YesToAll`, the handler logs through `logger.Error` and rethrows the original exception before any further clear; the recursion passes `true`; no `[ExcludeFromCodeCoverage]` on the core; T12 of section 3.5 exists in TST2 and passes, observed RED (sentinel `InvalidOperationException`) before the fix; the 11 existing TST2 tests and the two TST1 try-save tests pass unchanged.
3. **L3.** `Cleanup_Files` assigns `YesNoToAllResponse.Empty` to all four static enum fields; the four-row `DataRow` test of section 4.4 exists and passes; the `_attachmentsAltName` row was observed RED before the fix; the UT5 call-out for the reflective static write is recorded.
4. **L4.** `WriteCSV_StartNewFileIfDoesNotExist` has an `internal` four-argument overload taking `Func<string, bool> fileExists` and `Action<string, string[], string> writeTextFile`; the public two-argument overload forwards `File.Exists` and `FileIO2.WriteTextFile` as method groups and carries `[ExcludeFromCodeCoverage]` with a justification comment; the core tests `!fileExists(Path.Combine(strFileLocation, strFileName))`, allocates `strOutput` to `strAryOutput.GetLength(0)` elements, and writes once; L4-T1 and L4-T2 exist in `UtilitiesCS.Test/EmailIntelligence/SortEmail_UndoAndMoveLog_Tests.cs` and pass, observed RED after the seam and before the logic fix; `AppOlObjects.cs` is unchanged.
5. **Registration and size.** `UtilitiesCS.Test/UtilitiesCS.Test.csproj` gains exactly two `<Compile Include>` entries for the two new test files; every changed or new `.cs` file is under 500 lines after CSharpier.
6. **Test policy.** New tests use MSTest, Moq and FluentAssertions only; none creates a file or directory, uses `[DoNotParallelize]`, `Thread.Sleep`, `Task.Delay` or a timeout; they pass under `Workers 0` / `ClassLevel`.
7. **Toolchain and coverage.** Full CLAUDE.md toolchain passes in one pass; changed lines do not lose coverage; new or changed members (`SaveCase`, the private try-save core, the seamed CSV core, `Cleanup_Files`) each reach at least 90 percent line coverage; evidence committed as Markdown projections only.
8. **Scope.** No change to the overwrite or alternate-name prompt calls, `SaveAttachmentsOld`, `IsPicture`, `SanitizeArray`'s exclusion, any caller, or any file outside the section 6.1 list.

---

## 10. Testing implications (strategy, no code)

- Scoped runs: `FullyQualifiedName~EmailIntelligence.SortEmail_` now matches four classes (`SortEmail_Tests`, `SortEmail_TrySaveAttachment_Tests`, `SortEmail_AttachmentSaving_Tests`, `SortEmail_UndoAndMoveLog_Tests`); predicted total 15 + 12 + (5 rows + 4 rows) + 2 = 38 `[P]`, to be measured, with the TRX total asserted non-zero because a zero-match filter exits 0 (#956 plan convention).
- Negative controls worth recording: revert each fix one at a time and observe only its tests fail (L1: the two positive tests; L2: T12 with the sentinel; L3: the `_attachmentsAltName` row; L4: both tests).
- Sandbox check: `C:\Sortemail959Sandbox` must not exist before or after the run (the #945/#956 `Test-Path` convention); the paths are only recorded by delegates.
- Coverage: new baseline and post-change projections for #959; per-member figures for the four members in AC7; the TrySave file's unreachable-brace exemptions must be re-derived from content after the handler changes.

---

## 11. Numeric Derivation Evidence

**N1: existing `[TestMethod]` members in TST2 = 11**
- Complete Family: test methods of `SortEmail_TrySaveAttachment_Tests`.
- Exhaustive Search Scope: TST2, whole file.
- Inclusion Rules: `[TestMethod]`-attributed methods.
- Exclusion Rules: helpers and the nested `Seams` type.
- Primary Search Strategy or Query Expression: Grep `\[TestMethod\]` over TST2.
- Primary Member Set: attribute lines {33, 57, 84, 110, 140, 165, 193, 217, 245, 273, 297}.
- Primary Count: 11.
- Cross-check Search Strategy or Query Expression: full Read of TST2; the XML summaries numbered `T1.` to `T11.` at lines 30, 54, 81, 107, 137, 162, 190, 214, 242, 270, 294.
- Cross-check Member Set: {T1 ... T11}, each immediately preceding one attribute line of the primary set.
- Cross-check Count: 11.
- Member-set Comparison: one-to-one.

**N2: production call sites of the two-argument `TrySaveAttachmentAsync` = 3**
- Complete Family: executable invocations of any `TrySaveAttachmentAsync` overload in production code, then restricted to the two-argument form.
- Exhaustive Search Scope: all `*.cs` in the worktree.
- Inclusion Rules: executable call expressions in compiled production files.
- Exclusion Rules: declarations (T 41, 60, 87), internal forwards and recursion inside T (46, 66, 134), tests.
- Primary Search Strategy or Query Expression: Grep `TrySaveAttachmentAsync\(` over `*.cs`, then filter by argument count from the surrounding lines.
- Primary Member Set: {A 221, A 266, A 281}.
- Primary Count: 3.
- Cross-check Search Strategy or Query Expression: Grep `\.TrySaveAttachmentAsync\(` (extension-call shape only), which cannot match declarations or the non-extension forwards.
- Cross-check Member Set: production {A 221, A 266, A 281}; tests {TST2 325 (five-argument), TST1 257, TST1 281 (three-argument)} excluded by rule.
- Cross-check Count: 3.
- Member-set Comparison: identical.

**N3: static `YesNoToAllResponse` fields in `SortEmail` = 4; reset by `Cleanup_Files` today = 3**
- Complete Family: `private static YesNoToAllResponse` fields declared in any `SortEmail` partial; assignments to them inside `Cleanup_Files`.
- Exhaustive Search Scope: `UtilitiesCS/**/*.cs` for the fields; A 30 to 36 for the resets.
- Inclusion Rules: field declarations of that type; assignments of `Empty` inside `Cleanup_Files`.
- Exclusion Rules: the `YesNoToAllPromptSession` field (different type; reset through `Reset()`); local variables of the type (`SaveAttachmentsOld` line 47 `response`).
- Primary Search Strategy or Query Expression: Grep `private static YesNoToAllResponse _` over `UtilitiesCS`.
- Primary Member Set: {A 25 `_responseSaveFile`, A 26 `_attachmentsOverwrite`, A 27 `_attachmentsAltName`, A 28 `_picturesOverwrite`}; resets read from A 32 to 34: {`_responseSaveFile`, `_attachmentsOverwrite`, `_picturesOverwrite`}.
- Primary Count: 4 fields; 3 reset.
- Cross-check Search Strategy or Query Expression: the #956 research section 4.3 table (five fields at merge-base, of which `_removeReadOnly` was replaced by the session in #956), plus Grep `_attachmentsAltName` (9 lines, all in A; none in `Cleanup_Files`).
- Cross-check Member Set: fields {`_responseSaveFile`, `_attachmentsOverwrite`, `_attachmentsAltName`, `_picturesOverwrite`}; reset {`_responseSaveFile`, `_attachmentsOverwrite`, `_picturesOverwrite`}; missing {`_attachmentsAltName`}.
- Cross-check Count: 4; 3.
- Member-set Comparison: identical.

**N4: compiled production call sites of `SortEmail.Cleanup_Files` = 1**
- Complete Family: invocations of `SortEmail.Cleanup_Files`.
- Exhaustive Search Scope: all `*.cs` in the worktree.
- Inclusion Rules: executable calls in files that are compiled into a project.
- Exclusion Rules: comments; `QuickFiler/Legacy/QfcController.cs` (not in `QuickFiler.csproj`, per the #956 research section 9 and plan fact 8); tests; the unrelated `ToDoModel` `Cleanup_Files` declaration (`SortItemsToExistingFolder.cs` 391).
- Primary Search Strategy or Query Expression: Grep `Cleanup_Files\(\)`.
- Primary Member Set: {`EfcDataModel.cs` 309}; excluded hits: QfcController 792, comments at `QfcItemController.MailActions.cs` 156 and 188 and `SortItemsToExistingFolderTests_Unfinished.cs` 136, TST1 178, declarations A 30 and ToDoModel 391.
- Primary Count: 1.
- Cross-check Search Strategy or Query Expression: Grep `SortEmail\.Cleanup_Files` (qualified-name shape).
- Cross-check Member Set: {`EfcDataModel.cs` 309}; same exclusions.
- Cross-check Count: 1.
- Member-set Comparison: identical.

**N5: compiled production call sites of `SortEmail.WriteCSV_StartNewFileIfDoesNotExist` = 1**
- Complete Family: invocations of the `SortEmail` member (not the `ToDoModel` private duplicate).
- Exhaustive Search Scope: all `*.cs` in the worktree.
- Inclusion Rules: executable calls resolving to `UtilitiesCS.SortEmail`.
- Exclusion Rules: the declaration (U 140); `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs` 223 (calls the private copy declared at 285 in the same class).
- Primary Search Strategy or Query Expression: Grep `WriteCSV_StartNewFileIfDoesNotExist` (bare name).
- Primary Member Set: {`AppOlObjects.cs` 301}; excluded: U 140, ToDoModel 223, ToDoModel 285.
- Primary Count: 1.
- Cross-check Search Strategy or Query Expression: Grep `SortEmail\.WriteCSV` (qualified shape).
- Cross-check Member Set: {`AppOlObjects.cs` 301}.
- Cross-check Count: 1.
- Member-set Comparison: identical.

**N6: `[ExcludeFromCodeCoverage]` attributes on members touched by L1 to L4 = 4**
- Complete Family: `ExcludeFromCodeCoverage` applications in A, T and U.
- Exhaustive Search Scope: the three files.
- Inclusion Rules: attribute lines whose member is changed by this item (`SaveCase`, the two-argument try-save wrapper, the read-only adapter, `WriteCSV_StartNewFileIfDoesNotExist`).
- Exclusion Rules: attributes on members not changed (A 38, 63, 103, 164, 227, 241, 311, 322, 333; U 26, 82, 94, 109, 172).
- Primary Search Strategy or Query Expression: Grep `ExcludeFromCodeCoverage` with glob `SortEmail.{AttachmentSaving,TrySaveAttachment,UndoAndMoveLog}.cs` (18 lines).
- Primary Member Set: {A 290, T 40, T 165, U 139}.
- Primary Count: 4.
- Cross-check Search Strategy or Query Expression: full Reads of A, T and U, locating the attribute line immediately above each touched member's declaration (A 291, T 41, T 166, U 140).
- Cross-check Member Set: {A 290, T 40, T 165, U 139}.
- Cross-check Count: 4.
- Member-set Comparison: identical. Disposition: A 290 removed; T 40 and T 165 kept; U 139 stays on the two-argument wrapper.

Post-change predictions (file line counts in 6.1, the scoped test total in section 10) are `[P]` and must be measured by the plan.

---

## Automation Feasibility

No human interaction is expected. All four fixes are source edits in three nullable-enabled partials plus two new test files and two project-file lines; every test uses Moq-backed COM interfaces, recording delegates, reflection on a private static field, or a scripted prompt session, with no Outlook process, dialog or disk access. The L2 regression test is bounded by a call-count tripwire rather than time, so it is deterministic under the parallel runsettings. Two decisions for the orchestrator or maintainer, neither blocking execution: (a) accept the additive `internal` four-argument seam for L4 as the precondition of a red-first test (section 5.5); (b) accept the reflective static write in the L3 test with the order-independence argument of section 4.4 as the recorded UT5 exception.
