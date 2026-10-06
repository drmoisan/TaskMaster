# Research: #966 consolidation into #959 (F1, F2, F3, CR-1, try-save logging, usings, and the three same-root-cause items)

- **Issue:** #959 (work mode `full-bug`), consolidated scope per `issue.md` lines 22 to 35 (maintainer directive, 2026-10-02). The pull request closes #959 and #966.
- **Feature folder:** `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/`
- **Branch:** `bug/sort-email-latent-logic-defects-959`
- **Timestamp:** 2026-10-02T05-50
- **Relationship to the first research file:** `research/2026-10-02T05-15-sort-email-latent-logic-defects-research.md` remains authoritative for L1 to L4 except where section 0.2 below states a design change. This file does not repeat its content; it cites it as "R1 section N".
- **Evidence basis:** every citation was read in this worktree with Read, Grep and Glob. No command was run. Line numbers are those of this branch.
- **Tag legend:** `[V]` verified by reading the cited file; `[I]` inference from verified facts; `[P]` prediction to be confirmed at execution.
- **Scope rule applied (binding, `issue.md` line 35):** every defect found in the same files or with the same root cause is specified for fixing here. One such defect not listed by the orchestrator was found and is included: section 1.6 (`SaveAttachmentAsync(helper, destinationPath)` re-roots only one of the two save paths). No completely unrelated defect was found.

File aliases (paths relative to the worktree root; the SortEmail partials are under `UtilitiesCS/EmailIntelligence/EmailParsingSorting/`):

| Alias | File | Read last line | `#nullable enable` |
|---|---|---|---|
| A | `SortEmail.AttachmentSaving.cs` | 343 | line 1 `[V]` |
| T | `SortEmail.TrySaveAttachment.cs` | 173 | line 1 `[V]` |
| U | `SortEmail.UndoAndMoveLog.cs` | 196 | line 1 `[V]` |
| S | `SortEmail.cs` | 277 | line 1 `[V]` |
| M | `SortEmail.MailItemSort.cs` | 388 | line 1 `[V]` |
| L | `SortEmail.LegacyAttachmentSaving.cs` | 240 | line 1 `[V]` |
| E | `QuickFiler/Controllers/EfcDataModel.cs` | 465 | none (line 1 is `using System;`) `[V]` |
| TD | `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs` | 403 | none `[V]` |
| TST1 | `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` | 458 | none |
| TST2 | `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs` | 376 | none |
| ART | `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` | 399 | none |
| SPEC956 | `docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md` | n/a | n/a |
| CR956 | same folder, `code-review.2026-10-01T22-24.md` | 91 | n/a |

"Read last line" is the number of the last line the Read tool printed; each file ends with a newline, so the content-line count used by CR956 line 13 is one less (for example A: 342). Both conventions are under the 500-line limit for every file in this document; predictions below use the Read convention.

---

## 0. Summary of decisions and their effect on L1 to L4

### 0.1 Per-item recommendation

| Item | Recommendation |
|---|---|
| 1 F1 | Replace the three enum fields with three `private static readonly YesNoToAllPromptSession` instances; route both overwrite prompts and the alternate-name prompt through `Ask` / `ReleaseSingleAnswer`; delete `_responseSaveFile`; seam `SaveAttachmentAsync` and `SaveCaseAsync` with `Func<string, bool> fileExists` and `Func<Attachment, string, Task<bool>> trySave` plus the sessions; seam the synchronous `SaveAttachment` with `fileExists` and the two overwrite sessions; `Cleanup_Files` resets through a call-time `AllPromptSessions` property. Also fix the in-file re-rooting defect (1.6). Do not delete the synchronous chain (1.7). |
| 2 F2 | Delete `SaveAttachmentsOld` and `IsPicture` (zero callers). L becomes empty (its only other member, `MAX_PATH`, is used only inside `SaveAttachmentsOld`): delete the file and remove `UtilitiesCS/UtilitiesCS.csproj` line 820. |
| 3 F3 | Of 28 `[ExcludeFromCodeCoverage]` attributes, remove 7 (members with existing tests or seamed here), delete 3 with their members, keep 18 (wrappers and Outlook-interop orchestration). Table in section 3. |
| 4 CR-1 | Replace SPEC956 line 155 and the trailing clause of line 99 with the texts in section 4; add one dated supersession note. |
| 5 Try-save | `Debug.WriteLine` at T 103, 127, 147 become `logger.Warn`, `logger.Error`, `logger.Warn`; delete T 156 to 159. No test asserts logging. The #956 AC15 exemption (3) is re-described; L2 adds one predicted exemption. |
| 6 Usings | Per-file final blocks in section 6. `Deedle`, `SDILReader`, the `Outlook =` alias and the self-referential `using UtilitiesCS;` are unused in all five surviving files. IDE0005 is not reported in this build, so the compiler is the only gate. |
| 7 EfcDataModel | `try`/`finally` around `InvokeFilerAsync` with a `protected internal virtual void ResetFilerPromptState()` seam (the #736 `InvokeFilerAsync` pattern); new QuickFiler.Test file with a red-first test. |
| 8 ToDoModel | TD is not compiled (no `<Compile Include>` in `ToDoModel.csproj`) and `MASTER_SortEmailsToExistingFolder` has zero callers: delete the file. Fixing in place or delegating to `SortEmail` would edit code the compiler never checks. |
| 9 CSV header | Defect. The file is `999999EmailMoves.tsv`; records are one 13-field tab-separated line; the as-coded header is 14 lines (one empty, then one name per line). Write one tab-separated line of the 13 names in `Details` index order; delete `SanitizeArray` and its TST1 test. |

### 0.2 Design changes to R1 (the first research file)

| R1 design | Change | Reason |
|---|---|---|
| L1 (R1 section 2) | Unchanged fix. The three L1 tests move to a new file `SortEmail_SaveCase_Tests.cs` together with the `SaveCaseAsync` tests, instead of `SortEmail_AttachmentSaving_Tests.cs`. | 500-line limit once the F1 tests exist (section 10). |
| L2 (R1 section 3.4) | The private core keeps a single `catch (System.UnauthorizedAccessException e)`; the outer `catch (System.Exception) { throw; }` is removed (item 5). `Debug.WriteLine` calls become logger calls. R1 section 3.4's sentence "The outer `catch (System.Exception) { throw; }` stays" is superseded. | Item 5. T10 (TST2 273) pins propagation of non-access exceptions. |
| L3 (R1 section 4) | The one-line fix and the four-row reflection test are **phase 1** and stay red-first against the current code. In the F1 phase the enum fields cease to exist; the reflection test is replaced by the structural test of section 1.5. The UT5 call-out for a reflective static write (R1 4.4) applies only to phase 1 and disappears with the replacement. | F1 replaces the fields. |
| L4 (R1 section 5) | The seam overload stands. The body no longer builds a `string[14, 2]`: it writes one tab-separated header line (item 9). L4c (null `strOutput`) disappears by construction. `SanitizeArray` loses its last caller and is deleted with its TST1 test. L4-T1 asserts a single 13-column line; L4-T2's red mechanism is "a write is recorded although the file exists" instead of `NullReferenceException`. | Item 9. |
| R1 section 6.1 write set | Superseded by section 10 of this file. | Consolidation. |
| R1 section 9 AC 8 (scope) | Superseded (already noted in R1's banner). | Consolidation. |

---

## 1. Item 1: F1, prompt sessions for the overwrite and alternate-name prompts

### 1.1 Current state `[V]`

- Static state (A 25 to 28): `_responseSaveFile`, `_attachmentsOverwrite`, `_attachmentsAltName`, `_picturesOverwrite`, all `private static YesNoToAllResponse`. `Cleanup_Files` (A 30 to 36) resets three of them and calls `RemoveReadOnlyPrompt.Reset()`.
- Dialog call expressions in the partials (derivation N4): A 112, 135 (synchronous `SaveAttachment`), A 175, 198 (`SaveAttachmentAsync`), A 255 (`SaveCaseAsync`), L 165, 182 (`SaveAttachmentsOld`). T 29 is a method-group reference inside `new(YesNoToAll.ShowDialog)`, not a call.
- File-system calls in A: `File.Exists` at 106 (`SaveAttachment`) and 169 (`SaveAttachmentAsync`).
- Two-argument `TrySaveAttachmentAsync` call sites: A 221, 266, 281 (R1 derivation N2).
- `_responseSaveFile` readers and writers (derivation N1): declaration A 25; reset A 32; read L 155 (`(int)_attachmentsOverwrite + (int)_responseSaveFile == 0`), read L 180, write L 187, read L 191. All L sites are inside `SaveAttachmentsOld`, which F2 deletes. After F2 the field has a declaration and a reset and no reader: delete it.
- `YesNoToAllPromptSession` (`UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs`, 70 lines): `Ask` prompts only while `Response == Empty` and returns the held answer (40 to 48); `ReleaseSingleAnswer` clears `Yes`/`No` and keeps the `ToAll` answers (54 to 60); `Reset` clears everything (65 to 68); `Response` has a private setter (33). The constructor takes `Func<string, YesNoToAllResponse>` (24).
- Live production entry: `EmailFiler.cs` 443 to 446 calls `attachment.SaveAttachmentAsync(Config.SaveFsPath!)` (A 227 to 239), which sets `FolderPathSave` (A 235) and forwards to `SaveAttachmentAsync(this AttachmentHelper)` (A 164 to 225).

### 1.2 Equivalence of the field pattern to the session API `[I]`

Each prompt site today has the shape: `if (field == Empty) field = ShowDialog(message); use(field); if (field == Yes || field == No) field = Empty;` (A 173 to 192, 196 to 215, 253 to 276). `session.Ask(message)` performs the first statement and returns the held value; `session.ReleaseSingleAnswer()` performs the last. A cancelled dialog returns `Empty` (`YesNoToAll.cs` 47 to 50, 65 to 108), leaves the field `Empty` today and leaves `Response` `Empty` with `Ask` (`YesNoToAllPromptSession.cs` 44). The mapping is therefore behavior-preserving at every site, which is the same argument CR956 line 47 recorded for the read-only prompt.

### 1.3 Recommended design

Fields and cleanup (A, replacing lines 25 to 36):

```csharp
// Production answer state of the three attachment prompts. Tests pass their own sessions to the
// seamed overloads below, so no test reads or writes these instances. Cleanup_Files resets them.
private static readonly YesNoToAllPromptSession AttachmentsOverwritePrompt = new(YesNoToAll.ShowDialog);
private static readonly YesNoToAllPromptSession PicturesOverwritePrompt = new(YesNoToAll.ShowDialog);
private static readonly YesNoToAllPromptSession AttachmentsAltNamePrompt = new(YesNoToAll.ShowDialog);

// A property, not an array field: a static field initializer in this partial could run before the
// RemoveReadOnlyPrompt initializer in SortEmail.TrySaveAttachment.cs and capture null, because the
// initialization order across partial files is unspecified (#956 research 2.2).
private static YesNoToAllPromptSession[] AllPromptSessions =>
    new[] { AttachmentsOverwritePrompt, PicturesOverwritePrompt, AttachmentsAltNamePrompt, RemoveReadOnlyPrompt };

public static void Cleanup_Files()
{
    foreach (var prompt in AllPromptSessions)
    {
        prompt.Reset();
    }
}
```

Naming follows the #956 precedent `RemoveReadOnlyPrompt` (T 28) rather than the `_camelCase` suggestion-level naming rule, so the four production sessions read alike.

Asynchronous path (A 164 to 225 and 241 to 288 become):

```csharp
// Excluded from coverage: wiring only. Calling it from a test would read the real file system
// (File.Exists), show the production dialogs and create a real directory (UT4).
[ExcludeFromCodeCoverage]
public static Task SaveAttachmentAsync(this AttachmentHelper attachmentHelper)
{
    return SaveAttachmentAsync(
        attachmentHelper,
        File.Exists,
        PicturesOverwritePrompt,
        AttachmentsOverwritePrompt,
        AttachmentsAltNamePrompt,
        TrySaveAttachmentAsync
    );
}

internal static async Task SaveAttachmentAsync(
    AttachmentHelper attachmentHelper,
    Func<string, bool> fileExists,
    YesNoToAllPromptSession picturesOverwritePrompt,
    YesNoToAllPromptSession attachmentsOverwritePrompt,
    YesNoToAllPromptSession altNamePrompt,
    Func<Attachment, string, Task<bool>> trySave
)
{
    if (!fileExists(attachmentHelper.FilePathSave))
    {
        await trySave(attachmentHelper.Attachment, attachmentHelper.FilePathSave);
        return;
    }

    var overwritePrompt = attachmentHelper.AttachmentInfo.IsImage
        ? picturesOverwritePrompt
        : attachmentsOverwritePrompt;
    var answer = overwritePrompt.Ask(
        $"The file {attachmentHelper.FilePathSave} already exists. Overwrite?"
    );
    await SaveCaseAsync(answer, attachmentHelper.Attachment, attachmentHelper.FilePathSave,
        attachmentHelper.FilePathSaveAlt, altNamePrompt, trySave);
    overwritePrompt.ReleaseSingleAnswer();
}

internal static async Task SaveCaseAsync(
    YesNoToAllResponse response,
    Attachment attachment,
    string filePathSave,
    string filePathSaveAlt,
    YesNoToAllPromptSession altNamePrompt,
    Func<Attachment, string, Task<bool>> trySave
)
{
    switch (response)
    {
        case YesNoToAllResponse r when (r == YesNoToAllResponse.NoToAll || r == YesNoToAllResponse.No):
            var altAnswer = altNamePrompt.Ask(
                $"The file {filePathSave} already exists. Save with an alternate name?"
            );
            if (altAnswer == YesNoToAllResponse.Yes || altAnswer == YesNoToAllResponse.YesToAll)
            {
                await trySave(attachment, filePathSaveAlt);
            }
            altNamePrompt.ReleaseSingleAnswer();
            break;
        case YesNoToAllResponse r when (r == YesNoToAllResponse.YesToAll || r == YesNoToAllResponse.Yes):
            await trySave(attachment, filePathSave);
            break;
        default:
            break;
    }
}
```

Notes on the shape:

- The two duplicated branches of A 171 to 216 collapse into one path that selects the session; the selection stays in the measured core (a selection in the excluded wrapper would hide a branch).
- `TrySaveAttachmentAsync` is passed as a method group. Among the three overloads only the two-argument one (T 41) is convertible to `Func<Attachment, string, Task<bool>>`, so the conversion is unambiguous `[I]`, and a method group has no closure to leak into the measured denominator (#956 plan PD-7, R1 5.5).
- The `bool` returned by `trySave` is still discarded, as every production caller discards it today (R1 3.3). Surfacing is by rethrow (L2). No change here.
- The prompt message texts are unchanged.
- `default: await Task.CompletedTask;` (A 285) is dropped; the method still contains awaits, so CS1998 does not arise `[I]`.
- The four-argument `SaveCaseAsync` has no caller outside A 179 and 203 (Grep `SaveCaseAsync\(`: A 179, 203, 242 only), so it is replaced, not kept.

Synchronous path (A 103 to 162 becomes):

```csharp
// Excluded from coverage: wiring only (real File.Exists and the production sessions).
[ExcludeFromCodeCoverage]
public static void SaveAttachment(this AttachmentHelper attachmentHelper)
{
    SaveAttachment(attachmentHelper, File.Exists, PicturesOverwritePrompt, AttachmentsOverwritePrompt);
}

internal static void SaveAttachment(
    AttachmentHelper attachmentHelper,
    Func<string, bool> fileExists,
    YesNoToAllPromptSession picturesOverwritePrompt,
    YesNoToAllPromptSession attachmentsOverwritePrompt
)
{
    if (!fileExists(attachmentHelper.FilePathSave))
    {
        attachmentHelper.Attachment.SaveAsFile(attachmentHelper.FilePathSave);
        return;
    }

    var overwritePrompt = attachmentHelper.AttachmentInfo.IsImage
        ? picturesOverwritePrompt
        : attachmentsOverwritePrompt;
    var answer = overwritePrompt.Ask($"The file {attachmentHelper.FilePathSave} already exists. Overwrite?");
    SaveCase(answer, attachmentHelper.Attachment, attachmentHelper.FilePathSave, attachmentHelper.FilePathSaveAlt);
    overwritePrompt.ReleaseSingleAnswer();
}
```

The synchronous path does not show the alternate-name prompt today (A 116 to 121 call `SaveCase` directly); that is preserved. Seaming it costs one overload and lets the L1 fix be exercised from its real caller (section 1.8, sync tests). The minimal alternative (route through the sessions without a `fileExists` seam and keep the exclusion) is acceptable if the planner prefers fewer tests; the recommendation is to seam it because the fixture is shared with the asynchronous tests.

Smallest seam set: `{fileExists, trySave}` plus the three sessions for the live path; `{fileExists}` plus two sessions for the synchronous path. No new type, no interface (CR956 CR-2 recorded the delegate-plus-sealed-class choice as deliberate; a second consumer of the session does not justify an interface, because every test still constructs the concrete type with a scripted delegate, exactly as TST2 338 to 373 does).

Rejected alternatives: a holder type bundling the three sessions (adds a type and its own tests for three properties and a reset); a settable static seam (excluded by #956 AC5 and the parallel-run rule); choosing the overwrite session in the excluded wrapper (hides a branch); an `internal static` accessor for the sessions (production surface whose only consumer is a test).

### 1.4 Callers affected `[V]`

| Member | Callers | Effect |
|---|---|---|
| `SaveAttachmentAsync(this AttachmentHelper)` | A 238 (destination overload), M 153 | signature unchanged |
| `SaveAttachmentAsync(this AttachmentHelper, string)` | `EmailFiler.cs` 445, S 165 | signature unchanged; body changes per 1.6 |
| `SaveAttachment(this AttachmentHelper)` | M 299 | signature unchanged |
| `SaveCaseAsync` (4 args) | A 179, 203 | replaced by the 6-argument core |
| `SaveCase` | A 116, 139 (through the new sync core) | unchanged (L1 fix applies) |
| `Cleanup_Files` | E 309 | signature unchanged; body iterates `AllPromptSessions` |
| `_responseSaveFile` | A 25, 32, L 155, 180, 187, 191 | deleted with F2 |

No caller outside the SortEmail partials changes.

### 1.5 L3 under F1: how the regression test stays red-first now and meaningful later

Phase order (section 11): L3 is fixed before F1.

1. **Phase L3 (current code).** R1 section 4.4's test `Cleanup_Files_ResetsEveryPromptAnswerField` with four `DataRow`s on the enum field names is RED on `_attachmentsAltName` and GREEN after the one-line fix. Its RED run is recorded under `evidence/regression-testing/`.
2. **Phase F1.** The enum fields no longer exist, so that test is replaced (same file) by a structural test that reads static state only:

```
Cleanup_Files_ResetsEveryPromptSession
Arrange: sessionFields = typeof(SortEmail).GetFields(NonPublic | Static)
                             .Where(f => f.FieldType == typeof(YesNoToAllPromptSession)).ToList();
         resetTargets  = (YesNoToAllPromptSession[]) typeof(SortEmail)
                             .GetProperty("AllPromptSessions", NonPublic | Static)!.GetValue(null)!;
Assert:  sessionFields.Should().HaveCount(4);                  // every prompt holder is a session this test knows
         foreach field: resetTargets.Should().Contain(s => ReferenceEquals(s, field.GetValue(null)));
         resetTargets.Should().HaveCount(4);                   // no duplicate, no stray entry
```

   Why structural: a session's `Response` has a private setter and `Ask` on a production instance shows the real dialog, so no test can hold a non-`Empty` answer in a production session and observe `Cleanup_Files` clear it. What the invariant needs is that every static session is reset; `Cleanup_Files` is a `foreach` over `AllPromptSessions` (covered by TST1 175 `Cleanup_Files_DoesNotThrow`), and `Reset` itself is pinned by `YesNoToAllPromptSession_Tests` S6 (SPEC956 AC9). A new static session added without being listed fails the `HaveCount(4)`/`Contain` pair.
   Negative control for the evidence tree (fail-before exception for a refactor step): remove one element from `AllPromptSessions`, observe the test fail, restore (the #956 negative-control pattern, CR956 line 66).
3. The reflective write and its UT5 call-out (R1 4.4) apply to phase L3 only; the structural test only reads.

The behavioral half of L3 is also covered after F1: the `SaveCaseAsync` tests of 1.8 show with a test-owned session that `Yes` is released and `YesToAll`/`NoToAll` are kept across calls, which is the stickiness that `Cleanup_Files` exists to end.

### 1.6 Same-file defect found: the destination overload re-roots only one of two save paths `[V]`

`SaveAttachmentAsync(this AttachmentHelper attachmentHelper, string destinationPath)` (A 227 to 239) assigns `attachmentHelper.FolderPathSave = destinationPath` (A 235) and forwards. `FolderPathSave` writes `FilePathHelperSave.FolderPath` (`AttachmentHelper.cs` 199 to 203), whose setter recomputes `FilePath = Path.Combine(folder, fileName)` (`FilePathHelper.cs` 83 to 91, 360 to 367). `FilePathSaveAlt` is backed by a different `FilePathHelper` (`FilePathHelperSaveAlt`, `AttachmentHelper.cs` 174 to 189) that the overload never touches.

On the live path the helpers are constructed with the mail's Outlook folder **name**, not a file-system path: `MailItemHelper.Properties.cs` 255 and `MailItemHelper.cs` 159 call `new AttachmentHelper(x, SentDate, FolderName[, EmailPrefixToStrip])`; `EmailFiler.cs` 440 feeds those helpers to 445. After A 235, `FilePathSave` is `<SaveFsPath>\<name>` but `FilePathSaveAlt` is still `<FolderName>\<name>_<suffix><ext>`, a relative path. When the overwrite answer is `No` and the alternate-name answer is `Yes`, `SaveCaseAsync` (A 266) passes that relative path to `TrySaveAttachmentAsync`, which calls `Directory.CreateDirectory` on `Path.GetDirectoryName` of it (T 49, 97) and saves relative to the process working directory `[I]`. Same file, same member family as F1: fixed here under the scope rule.

Fix: extract the re-rooting into a tested helper and set both folders. `FilePathHelperSaveAlt` is `internal` (`AttachmentHelper.cs` 175), so no change to `AttachmentHelper` is required:

```csharp
[ExcludeFromCodeCoverage]   // wiring: forwards to the excluded File.Exists wrapper
public static Task SaveAttachmentAsync(this AttachmentHelper attachmentHelper, string destinationPath)
{
    RedirectSaveFolder(attachmentHelper, destinationPath);
    return SaveAttachmentAsync(attachmentHelper);
}

/// <summary>Re-roots both the primary and the alternate save path to <paramref name="destinationPath"/>,
/// keeping their file names. The helpers are built with the mail's folder name, so both paths must move.</summary>
internal static void RedirectSaveFolder(AttachmentHelper attachmentHelper, string destinationPath)
{
    attachmentHelper.FolderPathSave = destinationPath;
    attachmentHelper.FilePathHelperSaveAlt.FolderPath = destinationPath;
}
```

Test (file `SortEmail_AttachmentSaving_Tests.cs`): `RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths`: construct the helper with folder `C:\Sortemail959Sandbox\origin`, redirect to `C:\Sortemail959Sandbox\destination`, assert `Path.GetDirectoryName(helper.FilePathSave)` and `Path.GetDirectoryName(helper.FilePathSaveAlt)` both equal the destination and both file names are unchanged. Red-first in two steps, like L4: land `RedirectSaveFolder` containing only the existing statement (behavior-preserving extraction), observe the alternate-path assertion RED, add the second statement, GREEN. No file system is touched: `FilePathHelper`'s setters are string operations (`FilePathHelper.cs` 348 to 396); only `Exists()` and `GetLastWriteTimeUtc()` read the disk and neither is called.

### 1.7 Is the synchronous chain dead, and does F2-style deletion apply? `[V]`

Evidence: `Sort` (M 248) is the only caller of `SaveAttachment` (M 299; M 155 and 301 are comments). Callers of `Sort`: Grep `(^|[^.\w])Sort\(\s*$` over all `*.cs` returns only the declaration M 248; Grep `SortEmail\.Sort\(` returns only the commented ToDoModel.Test line 114 (derivation N6). `QuickFiler/Legacy/QfcController.cs` is not in `QuickFiler.csproj` (Grep `Legacy\\QfcController\.cs` over `QuickFiler*/*.csproj`: 0). The chain `Sort` -> `SaveAttachment` -> `SaveCase` has no compiled caller.

Decision: do **not** delete it in this item. (a) `Sort` and `SaveAttachment` are `public` members of `UtilitiesCS`; F2 names two `internal` members with a definition-only proof, and the #966 text scopes F2 to exactly those two. (b) The issue's Expected Behavior for L1 ("Switch cases match the intended flag combinations", `issue.md` line 51) asks for a fix, not a removal. (c) Dead code is not a defect, so the scope rule does not pull its removal in. The chain is routed through the sessions and seamed (1.3), which costs one overload and removes two exclusions. If the maintainer later wants the synchronous chain retired, it is a separate decision affecting `Sort`, `SaveAttachment`, `SaveCase`, `GetAttachmentsInfo`, `SaveMessageAsMSG` and `ResolvePaths(IList<MailItem>, ...)`.

### 1.8 Tests for F1 (MSTest, Moq, FluentAssertions; no disk, dialog, static write, sleep or retry)

Fixture facts `[V]`: `new AttachmentHelper(attachment, sentOn, saveFolderPath)` (`AttachmentHelper.cs` 34 to 37) computes `FilePathSave` and `FilePathSaveAlt` from strings only (61 to 124); TST1 183 to 207 constructs helpers from `CreateAttachmentMock` (TST1 386 to 406) with `IsImage` true for `photo.jpg` and false for `report.pdf`. `FilePathSaveAlt` carries a `DateTime.Now` suffix computed by production code (`AttachmentHelper.cs` 327 to 330); tests read the value back from the helper and never predict it. A scripted session is `new YesNoToAllPromptSession(message => { messages.Add(message); return answers.Dequeue(); })`, the TST2 `Seams` shape; an unscripted prompt throws from the empty queue, which is how "this session was not asked" is proven. A recording `trySave` is `(attachment, path) => { saves.Add(path); return Task.FromResult(true); }`.

New file `UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs` (L1 plus `SaveCaseAsync`):

| Test | Scenario and assertions | Fail-before |
|---|---|---|
| `SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath` (2 rows) | R1 2.4 | RED (L1) |
| `SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath` (2 rows) | R1 2.4 | RED (L1) |
| `SaveCase_WhenAnswerIsEmpty_DoesNotSave` | R1 2.4 | GREEN control |
| `SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt` (2 rows) | one `trySave(filePathSave)`; alt session never asked | compile-red (overload absent) |
| `SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePathAndReleasesAnswer` | prompt text equals `The file <path> already exists. Save with an alternate name?`; one `trySave(filePathSaveAlt)`; `Response == Empty` | compile-red |
| `SaveCaseAsync_WhenAnswerIsNoToAllAndAltNameAnswerIsYesToAll_KeepsAnswerAcrossCalls` | two calls; one prompt; two alternate saves; `Response == YesToAll` | compile-red |
| `SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsNoToAll_DoesNotSaveAndKeepsAnswer` | two calls; one prompt; no save; `Response == NoToAll` | compile-red |
| `SaveCaseAsync_WhenAnswerIsNoAndAltNamePromptIsCancelled_DoesNotSaveAndStaysAskable` | answer `Empty`; no save; `Response == Empty`; second call prompts again | compile-red |
| `SaveCaseAsync_WhenAnswerIsEmpty_DoesNothing` | no prompt, no save | compile-red |

New file `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` (asynchronous and synchronous cores, re-rooting, cleanup):

| Test | Scenario and assertions | Fail-before |
|---|---|---|
| `SaveAttachmentAsync_WhenFileDoesNotExist_SavesWithoutPrompting` | `fileExists` false, queried with `FilePathSave`; one `trySave(FilePathSave)`; no session asked | compile-red |
| `SaveAttachmentAsync_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly` | `photo.jpg`; pictures session scripted `Yes`; attachments session unscripted; `trySave(FilePathSave)` once | compile-red |
| `SaveAttachmentAsync_WhenFileExistsAndAttachmentIsDocument_AsksAttachmentsPromptOnly` | `report.pdf` mirror | compile-red |
| `SaveAttachmentAsync_WhenOverwriteAnswerIsYes_ReleasesAnswerAfterSave` | `Response == Empty` after the call | compile-red |
| `SaveAttachmentAsync_WhenOverwriteAnswerIsYesToAll_KeepsAnswerAndDoesNotAskAgain` | two helpers, one prompt, two saves, `Response == YesToAll` | compile-red |
| `SaveAttachmentAsync_WhenOverwriteAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePath` | `trySave(FilePathSaveAlt)` once; both single answers released | compile-red |
| `SaveAttachment_WhenFileDoesNotExist_SavesDirectly` | `SaveAsFile(FilePathSave)` once (mock verify) | compile-red |
| `SaveAttachment_WhenFileExistsAndAnswerIsYes_OverwritesAndReleasesAnswer` | `SaveAsFile(FilePathSave)` once; `Response == Empty` | compile-red |
| `SaveAttachment_WhenFileExistsAndAnswerIsNoToAll_SavesToAlternatePathAndKeepsAnswer` | `SaveAsFile(FilePathSaveAlt)` once; `Response == NoToAll` | compile-red; also RED before the L1 fix |
| `RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths` | section 1.6 | behavior-RED after step 1 of 1.6 |
| `Cleanup_Files_ResetsEveryPromptSession` | section 1.5 | replaces the phase-L3 test; negative control recorded |

Both files need `<Compile Include>` entries in `UtilitiesCS.Test/UtilitiesCS.Test.csproj` after line 99 (R1 1.3). Paths are rooted literals under `C:\Sortemail959Sandbox\`, a folder name distinct from the #945 and #956 sandboxes, probed absent before and after the run as in #956.

---

## 2. Item 2: F2, delete `SaveAttachmentsOld` and `IsPicture`

Evidence `[V]` (derivation N2): Grep `SaveAttachmentsOld|IsPicture\b` over every `*.cs` in the worktree returns exactly two lines, both declarations: L 28 and A 312. Because the grep is textual it also covers comments, string literals (reflection by name such as `GetMethod("IsPicture")`) and `nameof(...)` in every test project. Zero callers.

Consequences:

- Delete A 311 to 320 (`IsPicture`). `Path` is then unreferenced in A (section 6).
- Delete L 27 to 238 (`SaveAttachmentsOld`). The file's only other member is `private const int MAX_PATH = 256;` (L 25), whose only uses are L 97, 99 and 140, inside the deleted method (derivation N3). L would contain only usings and an empty partial declaration: delete `SortEmail.LegacyAttachmentSaving.cs` and remove `UtilitiesCS/UtilitiesCS.csproj` line 820 (`<Compile Include="EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs" />`). The `[ExcludeFromCodeCoverage]` at L 27 goes with it. `InputBox.ShowDialog` (L 199) and the two `YesNoToAll.ShowDialog` calls at L 165 and 182 disappear.
- Delete `_responseSaveFile` (A 25, A 32), per section 1.1.
- SPEC956 AC1 (six partial files) is a checked, historical criterion of a merged item and is not edited for this; the dated note of section 4.3 covers it.

Test: none (deletion of code with zero callers; fail-before exception: no test can execute it). The build is the verification: a surviving reference would fail with CS0103/CS1061.

---

## 3. Item 3: F3, `[ExcludeFromCodeCoverage]` dispositions

28 attributes in the six partials `[V]` (derivation N5). "Tested by" names an existing TST1 test or a test specified in this file.

| # | File:line | Member | Tested by | Disposition |
|---|---|---|---|---|
| 1 | A 38 | `GetAttachmentsInfo` | TST1 183 `GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments` | **Remove.** Pure LINQ over a mocked `MailItem`. Add a second row (`saveAttachments: true, savePictures: true`) so both `if` bodies are covered toward the 90 percent target for changed members `[P]`. |
| 2 | A 63 | `GetAttachmentsInfoAsync` | TST1 210 | **Remove.** Same; add the mirror row. The `SelectAwait` lambda compiles into a closure class that a method-level exclusion does not cover, so it is measured already (#956 plan PD-7). |
| 3 | A 103 | `SaveAttachment` wrapper | none possible (real `File.Exists`, production sessions) | **Keep** on the one-statement wrapper; the new core carries none. |
| 4 | A 164 | `SaveAttachmentAsync` wrapper | same | **Keep** on the wrapper; the core carries none. |
| 5 | A 227 | `SaveAttachmentAsync(helper, destinationPath)` | forwards to #4 | **Keep**; `RedirectSaveFolder` (1.6) is extracted and tested. |
| 6 | A 241 | `SaveCaseAsync` | section 1.8 tests | **Remove** (seamed). |
| 7 | A 290 | `SaveCase` | L1 tests | **Remove** (R1 2.3). |
| 8 | A 311 | `IsPicture` | n/a | **Deleted** with the member (F2). |
| 9 | A 322 | `SaveMessageAsMsgAsync` | TST1 292 `SaveMessageAsMsgAsync_WhenSubjectNeedsSanitizing_UsesMsgSavePath` | **Remove.** `MailItem.SaveAs` is a mocked COM member; the rest is string logic. |
| 10 | A 333 | `SaveMessageAsMSG` | TST1 317 | **Remove.** |
| 11 | T 40 | two-argument `TrySaveAttachmentAsync` | none possible (real `Directory.CreateDirectory`) | **Keep** (SPEC956 Risk (b)). |
| 12 | T 165 | `ClearReadOnlyAttributeOnDisk` | none possible | **Keep**. |
| 13 | U 26 | `UndoAsync` | none (`MessageBox`, `MailItem.Move`) | **Keep**. |
| 14 | U 82 | `PushToUndoStack` | none (`MovedMailInfo` constructor reads `Parent`, `StoreID`, `EntryID` of live items, `MovedMailInfo.cs` 16 to 24) | **Keep**. |
| 15 | U 94 | `CaptureMoveDetails` | none (`Details` reads `UserProperties`, `PropertyAccessor`, recipients) | **Keep**. |
| 16 | U 109 | `SanitizeArrayLineTSV` | TST1 341 `SanitizeArrayLineTSV_WhenArrayContainsNullsAndWhitespaceControlCharacters_ReturnsSanitizedLine` | **Remove.** |
| 17 | U 139 | `WriteCSV_StartNewFileIfDoesNotExist` | L4 tests on the seamed core | **Keep on the two-argument wrapper only** (R1 5.5); the core carries none. |
| 18 | U 172 | `SanitizeArray` | TST1 359 | **Deleted** with the member (item 9); the TST1 test is deleted with it. If the maintainer rules the header intended, this becomes **Remove** instead. |
| 19 | S 40 | `SortAsync(IList<MailItemHelper>, ...)` | TST1 80, 105 cover only the null/empty guard | **Keep.** The guard is 3 of about 50 lines; the remainder is Outlook orchestration (J1). Removing the exclusion would add about 45 uncovered lines for a member that cannot be made testable here. |
| 20 | S 109 | `UpdatePredictiveEngineAsync` | none | **Keep** (globals serialization). |
| 21 | S 138 | `ProcessMailItemAsync` | none | **Keep** (J1). |
| 22 | S 231 | `ResolvePaths(Folder, ...)` | none | **Keep**. |
| 23 to 27 | M 25, 59, 95, 247, 353 | three `SortAsync`, `Sort`, `ResolvePaths(IList<MailItem>, ...)` | none (TST1 names only the S overload) | **Keep** (J1). |
| 28 | L 27 | `SaveAttachmentsOld` | n/a | **Deleted** with the file (F2). |

Totals: remove 7 (#1, 2, 6, 7, 9, 10, 16); delete 3 (#8, 18, 28); keep 18. After the change the six-file family holds 18 attributes `[P]`.

Removal criterion applied, as the orchestrator asked: a member loses its exclusion only when it is fully exercisable without the file system, a dialog or an Outlook process. #1, 2, 9, 10 and 16 already are; #6 and #7 become so through the seams and L1.

---

## 4. Item 4: CR-1, the #956 spec wording about the `DirectoryInfo` boundary

### 4.1 Where `new DirectoryInfo` runs now `[V]`

T 120 to 133: `var directory = Path.GetDirectoryName(filePathSave);` precedes the inner `try`; `clearReadOnly(directory)` runs inside it; the production delegate is `ClearReadOnlyAttributeOnDisk` (T 165 to 170), whose first statement is `var di = new DirectoryInfo(directoryPath);`. So the construction runs inside the inner `try` and a constructor exception is caught by `catch (System.Exception inner)` (T 125 to 129) and returns `false`. CR956 line 19 records that at merge-base line 944 the construction preceded the `try`.

### 4.2 Exact current and corrected texts

SPEC956 line 155 (bullet under "Boundaries and invariants to preserve"), current:

```
- The `DirectoryInfo` construction's exception boundary (path computed before the inner `try`).
```

Corrected:

```
- The read-only clear's exception boundary: `Path.GetDirectoryName` is computed before the inner `try`, and the `DirectoryInfo` construction and the attribute write both run inside it through `ClearReadOnlyAttributeOnDisk` (D2 items 3 and 5). At merge-base the construction ran before the `try`; a `DirectoryInfo` constructor failure is therefore now caught and returns false. This is not reachable in production because `Directory.CreateDirectory` and `Path.GetDirectoryName` already validated the same path in the first `try` (code review CR-1, 2026-10-01).
```

SPEC956 line 99, D2 item 3, current trailing clause:

```
`var directory = Path.GetDirectoryName(filePathSave);` is computed **before** the inner `try`, as merge-base line 944 does, so the exception boundary is unchanged.
```

Corrected:

```
`var directory = Path.GetDirectoryName(filePathSave);` is computed **before** the inner `try`, as merge-base line 944 does; the `DirectoryInfo` construction, which merge-base line 944 performed before the `try`, now runs inside it as the first statement of the adapter (see "Boundaries and invariants to preserve").
```

CR956 itself is a review record and is not edited. SPEC956 AC6 and AC13 are checked criteria of a merged item; they are left as written.

### 4.3 Adjacent note (recommended, one paragraph)

Because this item removes the outer `catch (System.Exception) { throw; }` (item 5) and bounds the retry (L2), SPEC956's paragraph "**Must not be widened.**" (line 149) and AC6's "the outer rethrow ... unchanged, ... no retry bound is added" describe constraints that held for #956 only. Add after line 149:

```
> Superseded 2026-10-02 by #959 (closes #966): the outer rethrow is removed, `Debug.WriteLine` is replaced by the class logger, and a one-clear retry bound is added. The constraints in this paragraph and in AC6 applied to #956 only.
```

---

## 5. Item 5: try-save path, logger instead of `Debug.WriteLine`, outer rethrow removed

### 5.1 Facts `[V]`

- `logger` is `private static readonly log4net.ILog logger` at S 25 to 27, visible to every partial; used at S 211 to 214 (`logger.Error($"...", e)`), S 273 (`logger.Error(e)`), M 182.
- `Debug.WriteLine` sites in the partials: T 103 (`e.Message` on `UnauthorizedAccessException`), T 127 (`inner.Message` when the clear throws), T 147 (file not saved after `No`/`NoToAll`), U 177 (inside `SanitizeArray`, deleted by item 9). No other partial uses `Debug`.
- Outer `catch (System.Exception) { throw; }`: T 156 to 159. CR956 CR-3 (line 21) records it as a no-op rethrow and names T10 (TST2 273, `IOException` propagates) as the pin.

### 5.2 Change

| Line | Current | Replacement | Level rationale |
|---|---|---|---|
| T 103 | `Debug.WriteLine(e.Message);` | `logger.Warn($"Saving {filePathSave} was denied; the read-only prompt decides whether to retry.", e);` | Recoverable: a prompt and a retry follow. |
| T 127 | `Debug.WriteLine(inner.Message);` | `logger.Error($"The read-only attribute of {directory} could not be cleared; {filePathSave} was not saved.", inner);` | The save is abandoned and every production caller discards the `false` (R1 3.3), so this is the only record of the loss. |
| T 147 | `Debug.WriteLine($"The file {filePathSave} was not saved.");` | `logger.Warn($"The file {filePathSave} was not saved because the read-only change was declined.");` | User decision, no data loss beyond the declined file. |
| T 156 to 159 | `catch (System.Exception) { throw; }` | deleted | No effect on propagation: an exception not matched by the first `catch` propagates unchanged without it. |
| L2 (R1 3.4) | new | `logger.Error($"The file {filePathSave} is still denied after the read-only attribute was cleared.", e); throw;` | As R1 3.4. |

Message texts are proposals; the plan may adjust wording. The `(string, Exception)` overload is the pattern at S 211.

### 5.3 Observability and tests

- Fail-before: this is a behavior-preserving refactor with no observable defect; the pins are T7 (TST2 193, Cancel rethrows the same `UnauthorizedAccessException`), T10 (TST2 273, `IOException` propagates) and TST1 273 (`IOException` from `createDirectory` propagates). Record a fail-before exception rather than a red test.
- Can tests observe log4net? Yes: `MemoryAppender` precedents exist (`UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableSerializeGuardTests.cs` 205 to 239 attaches to the root logger; `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperControllerTests.cs` 225 to 250 attaches to one type's logger). Must they? No: logging is not part of the method's contract, log4net without configuration makes the calls no-ops, and a root-logger appender under `Workers 0` / `Scope ClassLevel` (`scripts/vscode/TaskMaster.cli.runsettings` 5 to 6) receives events from concurrently running classes. Recommendation: assert no log output. If the plan wants one, attach the appender to `LogManager.GetLogger(typeof(SortEmail))` only, filter by logger name and level, and detach in `finally`, as `StoreWrapperControllerTests` does.

### 5.4 Effect on the #956 coverage exemptions and on L2

SPEC956 AC15 names three content-identified exemptions in T: (1) the wrapper lambda `System.IO.Directory.CreateDirectory(path)` (T 49, unchanged); (2) the closing brace after `throw;` in the final `else` of the `UnauthorizedAccessException` handler (unchanged); (3) the closing brace of that `catch` block, described as "precedes `catch (System.Exception)`". After this item (3) is still unreachable (every branch of the handler returns or throws before it) but its description must become "the closing brace of the `catch (System.UnauthorizedAccessException e)` block, which is the last statement-level brace before the method's closing brace". The removed T 156 to 159 were covered lines (T10 reaches B9, CR956 line 65), so their removal lowers the measured denominator without creating an uncovered line. L2 adds `if (isRetryAfterClear && removeReadOnlyPrompt.Response == YesNoToAllResponse.YesToAll) { logger.Error(...); throw; }`; its closing brace follows `throw;` and is predicted to appear as a fourth unreachable brace `[P]`. R1 6.5 already requires the #959 comparison to re-derive the exemptions from content; this section gives the expected list: (1), (2), re-described (3), and the predicted (4).

L2 design (R1 3.4): the private core has one `catch`; `Debug.WriteLine(e.Message)` in the sketch becomes the T 103 replacement above. Nothing else in R1 3.4 or 3.5 changes; T12 is unaffected.

---

## 6. Item 6: using directives per file

### 6.1 Facts `[V]`

- Every partial carries the same 18 directives at lines 2 to 19 (derivation N7). CR956 CR-4 (line 22) recorded the replication.
- Namespaces of the identifiers the partials use: `SliceRow` (`Extensions/ArrayExtensions.cs` 9), `IsNullOrEmpty(this string?)` (`Extensions/StringExtensions.cs` 9), `MailItemHelper` (`OutlookObjects/MailItem/MailItemHelper.cs` 23), `FolderPredictor` (15), `FolderConverter` incl. `ToFsFolderpath` (14), `IMovedMailInfo` (5), `MovedMailInfo` (10), `IApplicationGlobals` (5), `InputBox` (7), `YesNoToAll` and `YesNoToAllResponse` (`Dialogs/YesNoToAll.cs` 12), `YesNoToAllPromptSession` (13), `FileIO2` (`To Depricate/FileIO2.cs` 10), `EmailDetails.Details` (`OutlookObjects/MailItem/EmailDetails.cs` 15): all `namespace UtilitiesCS`. `SetUdf` (both overloads, `OutlookObjects/Fields/UserDefinedFields.cs` 285 and 313): `namespace UtilitiesCS.OutlookExtensions` (line 17). `AttachmentHelper`: `UtilitiesCS.EmailIntelligence` (`AttachmentHelper.cs` 16). `OlFolderClassifierGroup`: `UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder` (22). `SloStack<T>`: `UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable` (9). `ToAsyncEnumerable`, `SelectAwait`, `ForEachAsync` and `Where` on `IAsyncEnumerable<T>`: `System.Linq` (System.Linq.Async 7.0.1 and System.Interactive.Async 7.0.1, `UtilitiesCS/packages.config` 88 and 97; `UtilitiesCS.csproj` 358 to 359 and 398 to 399). `ExcludeFromCodeCoverage`: `System.Diagnostics.CodeAnalysis`.
- `Deedle`, `SDILReader` and the alias `Outlook =`: Grep `Outlook\.|Deedle|SDILReader|Frame<|Series<` over the six partials matches only the `using` lines themselves (12 lines: `Deedle` and `SDILReader` in each file). No identifier from either namespace and no `Outlook.`-qualified name is used.
- `using UtilitiesCS;` inside `namespace UtilitiesCS` is redundant in every file.
- Interop collisions: `Microsoft.Office.Interop.Outlook` declares `Exception`, `Action` (non-generic) and `Application`. Every partial writes `System.Exception` (for example T 101 to 125, S 209, M 180) and T writes `Action<string>` (generic, arity 1, which the non-generic interop `Action` does not collide with). `using System;` must stay wherever `Func<>`, `Action<>`, `ArgumentNullException` or `NotImplementedException` are named, which is every surviving file. Removing `using System;` from a file that still wrote an unqualified `Exception` would silently bind it to the interop interface (and fail with CS0155 in a `catch`); no such file remains.
- IDE0005 severity: `.editorconfig` contains no `IDE0005` entry (Grep `IDE0005|CS8019` over `.editorconfig`, `*.csproj`, `*.props`, `*.targets`: 0); the global default `dotnet_analyzer_diagnostic.severity = suggestion` (`.editorconfig` 27) applies. IDE0005 is only reported in a command-line build when `GenerateDocumentationFile` is set, and no project sets it (Grep `GenerateDocumentationFile|DocumentationFile` over `*.csproj`: 0). The analyzer rebuild therefore cannot flag an unused directive; it also cannot flag a missing one other than by the compile errors CS0246/CS0103/CS1061, which the TreatWarningsAsErrors rebuild would surface. Verification is by reading and by a successful rebuild.
- CSharpier orders directives `System*` first, then alphabetical, aliases last `[I]` from the present block order; write the blocks in that order so the format step changes nothing.

### 6.2 Final using blocks (after every change in this file)

**A `SortEmail.AttachmentSaving.cs`** (8):

```csharp
using System;                                   // Func<string, bool>, Func<Attachment, string, Task<bool>>
using System.Collections.Generic;               // IEnumerable<AttachmentHelper>, IAsyncEnumerable<AttachmentHelper>
using System.Diagnostics.CodeAnalysis;          // ExcludeFromCodeCoverage
using System.IO;                                // File (method group File.Exists)
using System.Linq;                              // Cast, Where, Select, ToAsyncEnumerable, SelectAwait
using System.Threading.Tasks;                   // Task
using Microsoft.Office.Interop.Outlook;         // Attachment, MailItem, OlAttachmentType, OlSaveAsType
using UtilitiesCS.EmailIntelligence;            // AttachmentHelper
```

Removed: `System.Diagnostics` (no `Debug`), `System.Text.RegularExpressions`, `System.Windows.Forms`, `Deedle`, `SDILReader`, `UtilitiesCS`, `UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder`, `UtilitiesCS.OutlookExtensions`, `UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable`, `Outlook =`. `Path` leaves A with `IsPicture`.

**T `SortEmail.TrySaveAttachment.cs`** (5):

```csharp
using System;                                   // Action<string>
using System.Diagnostics.CodeAnalysis;          // ExcludeFromCodeCoverage
using System.IO;                                // Path, DirectoryInfo (System.IO.Directory and System.IO.FileAttributes are written qualified)
using System.Threading.Tasks;                   // Task, Task<bool>
using Microsoft.Office.Interop.Outlook;         // Attachment
```

Removed: `System.Collections.Generic`, `System.Diagnostics` (after item 5), `System.Linq`, `System.Text.RegularExpressions`, `System.Windows.Forms`, `Deedle`, `SDILReader`, `UtilitiesCS`, `UtilitiesCS.EmailIntelligence`, `...ClassifierGroups.OlFolder`, `UtilitiesCS.OutlookExtensions`, `...Observable`, `Outlook =`. `YesNoToAll`, `YesNoToAllResponse`, `YesNoToAllPromptSession` and `logger` need no directive (same namespace or same class).

**U `SortEmail.UndoAndMoveLog.cs`** (10):

```csharp
using System;                                   // Func<string, bool>, Action<string, string[], string>
using System.Diagnostics.CodeAnalysis;          // ExcludeFromCodeCoverage
using System.IO;                                // File (method group), Path.Combine
using System.Linq;                              // Skip, ToArray, Select
using System.Text.RegularExpressions;           // Regex
using System.Threading.Tasks;                   // Task
using System.Windows.Forms;                     // DialogResult, MessageBox, MessageBoxButtons
using Microsoft.Office.Interop.Outlook;         // MailItem
using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;   // OlFolderClassifierGroup
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;   // SloStack<IMovedMailInfo>
```

Removed: `System.Collections.Generic` (no generic collection type is named), `System.Diagnostics` (the only `Debug` leaves with `SanitizeArray`; if the maintainer keeps `SanitizeArray`, keep this directive), `Deedle`, `SDILReader`, `UtilitiesCS`, `UtilitiesCS.EmailIntelligence`, `UtilitiesCS.OutlookExtensions`, `Outlook =`. `Details`, `MailItemHelper`, `IMovedMailInfo`, `MovedMailInfo`, `IApplicationGlobals`, `FileIO2`, `StripTabsCrLf` are in `UtilitiesCS`.

**S `SortEmail.cs`** (9):

```csharp
using System;                                   // NotImplementedException, ArgumentNullException
using System.Collections.Generic;               // IList<MailItemHelper>, List<Task>
using System.Diagnostics.CodeAnalysis;          // ExcludeFromCodeCoverage
using System.IO;                                // File.Delete
using System.Linq;                              // FirstOrDefault, ToAsyncEnumerable, ForEachAsync, Where (IAsyncEnumerable)
using System.Threading.Tasks;                   // Task
using Microsoft.Office.Interop.Outlook;         // Folder, MailItem
using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;   // OlFolderClassifierGroup
using UtilitiesCS.OutlookExtensions;            // SetUdf
```

Removed: `System.Diagnostics`, `System.Text.RegularExpressions`, `System.Windows.Forms`, `Deedle`, `SDILReader`, `UtilitiesCS`, `UtilitiesCS.EmailIntelligence` (S never names `AttachmentHelper`; `x.SaveAttachmentAsync(saveFsPath)` at 165 is an extension declared in `SortEmail` itself and `x` is inferred), `...Observable`, `Outlook =`. `log4net.ILog` and `System.Reflection.MethodBase` are written qualified (S 25 to 27). `IsNullOrEmpty()` (S 170) and `ToFsFolderpath` (S 249, 259) are `UtilitiesCS`.

**M `SortEmail.MailItemSort.cs`** (9):

```csharp
using System;                                   // ArgumentNullException
using System.Collections.Generic;               // IList<MailItem>, List<Task>
using System.Diagnostics.CodeAnalysis;          // ExcludeFromCodeCoverage
using System.IO;                                // File.Delete
using System.Linq;                              // Cast, Where, Select, ToList, FirstOrDefault, ForEachAsync, Where (IAsyncEnumerable)
using System.Threading.Tasks;                   // Task
using System.Windows.Forms;                     // MessageBox
using Microsoft.Office.Interop.Outlook;         // MailItem, Folder
using UtilitiesCS.OutlookExtensions;            // SetUdf
```

Removed: `System.Diagnostics`, `System.Text.RegularExpressions`, `Deedle`, `SDILReader`, `UtilitiesCS`, `UtilitiesCS.EmailIntelligence` (M never names `AttachmentHelper`), `...ClassifierGroups.OlFolder`, `...Observable`, `Outlook =`. `FolderConverter`, `FolderPredictor`, `IApplicationGlobals`, `IsNullOrEmpty`, `ToFsFolderpath` are `UtilitiesCS`.

**L**: deleted (item 2).

The identifier-to-namespace mapping above is the derivation `[I]`; the rebuild is the verification. The counts (8, 5, 10, 9, 9) are consequences of the table and are `[P]` until the rebuild passes.

---

## 7. Item 7: `EfcDataModel.MoveToFolderAsync` calls `Cleanup_Files` without `try`/`finally`

### 7.1 Facts `[V]`

- E 268 to 311: the five-parameter `MoveToFolderAsync(string folderpath, bool saveAttachments, bool saveEmail, bool savePictures, bool moveConversation)`; guards return `false` at 278, 289, 294; `var result = await InvokeFilerAsync(config, mailHelpers);` (308); `SortEmail.Cleanup_Files();` (309); `return result;` (310). `InvokeFilerAsync` is `protected internal virtual` (320 to 326), the #736 seam.
- E is `internal partial class EfcDataModel` (21) with a `log4net` logger (23 to 25), 465 lines, no `#nullable enable`.
- Callers of the five-parameter overload (derivation N8): production E 387 (inside the `MAPIFolder` overload, 377 to 398, which shows a `MessageBox` on `false`), `EfcHomeController.ExecuteMoves.cs` 98, `EfcFormController.Actions.cs` 136, `EfcFormController.EventHandlers.cs` 184; test ART 316 (`MoveAsync` helper used by the seven `MoveToFolderAsync_*` tests at ART 48, 71, 175, 196, 226, 251 and by 184). Signatures do not change, so no caller changes.
- Existing tests reaching line 309: ART 175 `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce` (its `TestableEfcDataModel`, ART 379 to 397, overrides `InvokeFilerAsync` to `Task.FromResult(true)`, so the body continues to 309 and the real `Cleanup_Files` runs in QuickFiler.Test today). Fixture: `CreateOlObjects` (330), `CreateGlobals` (340), `SpecialFoldersWithOneDrive` (355), `SpecialFoldersWithoutOneDrive` (363). Other EfcDataModel test files (`EfcDataModelTests.cs`, `EfcDataModelIssue614Tests.cs`, `EfcDataModelIssue792CarryTests.cs`, `QuickFiler.Test.csproj` 125 to 128) do not call `MoveToFolderAsync` (Grep `MoveToFolderAsync` lists no line in them).
- Observability from QuickFiler.Test: `UtilitiesCS/Properties/AssemblyInfo.cs` 18 to 20 grants `InternalsVisibleTo` to `DynamicProxyGenAssembly2`, `UtilitiesCS.Test` and `ToDoModel.Test` only. QuickFiler.Test cannot name `YesNoToAllPromptSession` or any internal member of `SortEmail`. Reflection could read the private static sessions but cannot put a non-`Empty` answer into one (private setter; `Ask` shows the dialog). A seam in `EfcDataModel` is therefore the only dialog-free observation point.

### 7.2 Change (E 308 to 310, plus one member after 326)

```csharp
bool result;
try
{
    result = await InvokeFilerAsync(config, mailHelpers);
}
finally
{
    // Sticky "to all" prompt answers must not survive into the next filing operation when the
    // filer throws (#959; same root cause as the missing _attachmentsAltName reset).
    ResetFilerPromptState();
}
return result;

/// <summary>
/// Releases the sticky prompt answers held by <see cref="SortEmail"/> after a filing operation.
/// Virtual for the same reason as <see cref="InvokeFilerAsync"/>: a test override records the
/// call, because the answers live in internal state that QuickFiler.Test cannot observe.
/// </summary>
protected internal virtual void ResetFilerPromptState()
{
    SortEmail.Cleanup_Files();
}
```

`result` is definitely assigned after the `try`/`finally` because the `try` block's only exit without assignment is an exception `[I]`. `Cleanup_Files` only resets sessions and cannot throw, so the `finally` cannot mask the original exception. Predicted E length about 482 `[P]`.

### 7.3 Tests: new file `QuickFiler.Test/Controllers/EfcDataModelFilerCleanupTests.cs`

Nested probe `FilerCleanupProbe : EfcDataModel` built exactly like ART 379 to 397 (base constructor with a null mail item, `ConversationResolver` with a parameterless `MailItemHelper`), plus `public Func<Task<bool>> Filer { get; set; }` and `public int ResetCalls { get; private set; }`, overriding `InvokeFilerAsync` to `Filer()` and `ResetFilerPromptState` to `ResetCalls++`. Fixture helpers copied from ART 330 to 366 (about 35 lines); promoting them to a shared `internal static` fixture class is the reusable alternative, at the cost of editing ART.

| Test | Arrange | Assert | Fail-before |
|---|---|---|---|
| `MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates` | `Filer = () => Task.FromException<bool>(new InvalidOperationException("filer failed"))` | `await act.Should().ThrowAsync<InvalidOperationException>()`; `probe.ResetCalls.Should().Be(1)` | Step 1 (seam extracted, no `finally`): RED, `ResetCalls == 0`. Step 2: GREEN. |
| `MoveToFolderAsync_WhenFilerSucceeds_ResetsPromptStateOnce` | `Filer = () => Task.FromResult(true)` | `moved == true`; `ResetCalls == 1` | GREEN control |
| `MoveToFolderAsync_WhenAGuardReturnsFalse_DoesNotResetPromptState` | `SpecialFoldersWithoutOneDrive()` | `moved == false`; `ResetCalls == 0` | GREEN control; pins that cleanup belongs to the filer invocation, as today |

Register the file in `QuickFiler.Test/QuickFiler.Test.csproj` after line 127. The ART tests keep exercising the production `ResetFilerPromptState` body, so the one-line seam stays covered.

---

## 8. Item 8: the ToDoModel duplicate of the L4 helper

### 8.1 Facts `[V]`

- TD 285 to 315 is a private copy of `WriteCSV_StartNewFileIfDoesNotExist` with L4a (reversed `Path.Combine`, 292), L4b (inverted condition, 292) and L4c (`strOutput` null, 290 and 310); TD 317 to 337 is a copy of `SanitizeArray`; the caller is TD 223 inside `CaptureMoveDetails` (210 to 231), itself called from `MASTER_SortEmailsToExistingFolder` (TD 197).
- TD is **not compiled**: `ToDoModel/ToDoModel.csproj` has no `<Compile Include>` naming it (Grep `SortItemsToExistingFolder` over all `*.csproj`: only `ToDoModel.Test.csproj` 75 to 76, which name the two test files); its only `Email Utilities` item is `CaptureEmailAddressesModule.cs` (line 145); no wildcard `Compile` item exists (Grep `Compile Include="(\*\*|.*\*\.cs|Email Utilities)`: one hit, line 145). Derivation N9.
- `MASTER_SortEmailsToExistingFolder` has zero callers (Grep over `*.cs`: declaration TD 28 only). The two ToDoModel.Test files mention the type only in comments (`SortItemsToExistingFolderTests.cs` 62; the `_Unfinished` file mentions `SortEmail`, not the type, at 35 to 136) and compile without it.
- Further same-root-cause defects in the copy, all unreachable because the file is not compiled: `StripTabsCrLf` uses `[\t\n\r]*` (TD 275), which matches the empty string at every position, so `Regex.Replace` inserts a space between every character (`"abc"` becomes `"a b c"` after the collapse and trim) `[I]`, where `SortEmail.StripTabsCrLf` uses `+` (U 129) and is tested (TST1 141, 158); `CaptureMoveDetails` appends `strOutput` whose element 0 is never assigned (TD 59, 229 to 230), writing an empty line before every record.
- `ToDoModel.csproj` references `UtilitiesCS` (line 158), so delegating to the public `SortEmail.WriteCSV_StartNewFileIfDoesNotExist` would be legal if the file were compiled.

### 8.2 Decision

Delete `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs`. Fixing the copy in place or re-pointing TD 223 to `SortEmail` would edit code that no project compiles and no test can run; the F2 precedent (dead code with zero callers is deleted rather than seamed, SPEC956 J4) applies. No `.csproj` edit is needed because no project lists the file. The two disabled ToDoModel.Test files are left unchanged (they do not reference the deleted type in code). Test: none (fail-before exception: uncompiled code). If the maintainer wants the file retained, the alternative is to replace TD 285 to 337 with a call to `UtilitiesCS.SortEmail.WriteCSV_StartNewFileIfDoesNotExist` at TD 223 and fix TD 275; that alternative is recorded, not recommended.

---

## 9. Item 9: the CSV header shape

### 9.1 Evidence `[V]`

- File: `_globals.FS.Filenames.MovedMails` (`AppOlObjects.cs` 301 to 304) resolves to `Settings.Default.FileName_MovedEmailsBackup` (`TaskMaster/AppGlobals/AppStagingFilenames.cs` 98 to 110), default `999999EmailMoves.tsv` (`TaskMaster/Properties/Settings.settings` 107 to 109; `TaskMaster/app.config` 343 to 345). A tab-separated-values file.
- Record writer (live): `EmailFiler.cs` 191 to 199 and 453 to 461: `helper.Details().Skip(1).ToArray()` joined with `"\t"` after `s ?? ""` and `StripTabsCrLf` (211 to 217), enqueued to `EmailMoveWriter`, flushed by `FileIO2.WriteTextFileAsync` (`AppOlObjects.cs` 314 to 318), one string per line. The excluded SortEmail path does the same (U 103 to 106, 110 to 125). `EmailMoveWriter.Enqueue` has exactly these two call sites (derivation N10).
- Record layout: `Details` returns `string[14]` with index 0 unused and indices 1 to 13 assigned (`EmailDetails.cs` 38 to 66 for `MailItem`, 77 to 123 for `MailItemHelper`; `_numberOfFields = 13` at 23): 1 Triage, 2 folder path, 3 SentOn, 4 sender address, 5 To, 6 CC, 7 Subject, 8 Body, 9 sender domain, 10 ConversationID, 11 EntryID, 12 attachment names, 13 action taken. `Skip(1)` drops index 0, so a record is exactly 13 tab-separated fields (empties kept by `s ?? ""`).
- Header names in U 151 to 163, at `[i, 1]` for i = 1 to 13: Triage, FolderName, Sent_On, From, To, CC, Subject, Body, fromDomain, Conversation_ID, EntryID, Attachments, FlaggedAsTask. They match the 13 record fields one-to-one in order (derivation N11).
- As-coded output: `strAryOutput` is `string[14, 2]` with only column 1 of rows 1 to 13 set; `SanitizeArray` (U 173 to 193) joins each row's non-empty cells, so the 14 output lines are `""` followed by one name per line.
- Readers: none in the repository. Grep `MovedMails|EmailMoves` over `*.py, *.ps1, *.ipynb, *.R, *.psm1, *.json, *.sql`: 0. Over `*.cs` (Grep `MovedMails\b`): only the filename property, the two writers above, the `SloStack<IMovedMailInfo>` undo stack (a different object) and tests.

### 9.2 Decision: defect

A 14-line header (one empty line, then one column name per line) in a file whose every record is one 13-field line would be read by any TSV consumer as fourteen malformed records. The names and their order show that a single header row was intended; the two-dimensional array is a transliteration artifact (the VBA-era `strAryOutput(1 To 13, 1 To 1)` shape, TD 290 to 315 carries the same). Corrected header: one line, the 13 names in `Details` order, tab-separated.

### 9.3 Fix (combined with L4; R1 5.4 and 5.5 amended)

```csharp
private static readonly string[] MovedMailsHeader =
{
    "Triage", "FolderName", "Sent_On", "From", "To", "CC", "Subject", "Body",
    "fromDomain", "Conversation_ID", "EntryID", "Attachments", "FlaggedAsTask",
};

// Excluded from coverage: wiring the real file-system defaults only (UT4).
[ExcludeFromCodeCoverage]
public static void WriteCSV_StartNewFileIfDoesNotExist(string strFileName, string strFileLocation)
{
    WriteCSV_StartNewFileIfDoesNotExist(strFileName, strFileLocation, File.Exists, FileIO2.WriteTextFile);
}

/// <summary>Seeds the moved-mails log with its single tab-separated header line when the file
/// does not exist; the columns are those of <c>EmailDetails.Details</c> after its unused index 0.</summary>
internal static void WriteCSV_StartNewFileIfDoesNotExist(
    string strFileName,
    string strFileLocation,
    Func<string, bool> fileExists,
    Action<string, string[], string> writeTextFile
)
{
    if (fileExists(Path.Combine(strFileLocation, strFileName)))
    {
        return;
    }

    writeTextFile(strFileName, new[] { string.Join("\t", MovedMailsHeader) }, strFileLocation);
}
```

`SanitizeArray` (U 172 to 193) loses its only caller (U 165) and is deleted; `SliceRow` keeps its other caller (`OlTableExtensions.TableAccess.cs` 459). TST1 359 to 382 (`SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows`, reflection on the deleted member) is deleted; TST1 341 to 357 (`SanitizeArrayLineTSV`) stays. `StripTabsCrLf` is not applied to the header: the literals contain no control characters.

L4 test changes (file `SortEmail_UndoAndMoveLog_Tests.cs`, R1 5.6):

| Test | Assertions after this item | Pre-fix (seam landed as a pure forward of the current body, logic unfixed) |
|---|---|---|
| L4-T1 `WriteCSV_WhenFileDoesNotExist_WritesSingleTabSeparatedHeader` | queried path `== Path.Combine(folder, fileName)`; `writeTextFile` once with `(fileName, folder)`; `lines.Should().ContainSingle()`; `lines[0].Should().Be("Triage\tFolderName\tSent_On\tFrom\tTo\tCC\tSubject\tBody\tfromDomain\tConversation_ID\tEntryID\tAttachments\tFlaggedAsTask")`; `lines[0].Split('\t').Should().HaveCount(13)` | RED: queried path reversed, no write |
| L4-T2 `WriteCSV_WhenFileExists_DoesNotWrite` | `writeTextFile` never called | RED: the inverted condition enters the branch and the recorder sees one write (with the current body the write is preceded by the L4c `NullReferenceException`, which is also a failure) |

Alternative if the maintainer rules the header intended: keep the two-dimensional array, fix L4c, remove `SanitizeArray`'s exclusion (F3 #18 becomes Remove), and keep R1 5.6's 14-line assertions.

---

## 10. Complete write set

Line counts: "now" is the Read last line (content lines are one fewer); "after" is `[P]`. Limit 500.

| # | Path (worktree-relative) | Action | Project | Now | After `[P]` | `#nullable enable` |
|---|---|---|---|---|---|---|
| 1 | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs` | modify (F1, 1.6, F2 `IsPicture` and `_responseSaveFile`, L1, L3, F3, usings) | UtilitiesCS | 343 | about 330 | yes |
| 2 | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs` | modify (L2, item 5, usings) | UtilitiesCS | 173 | about 205 | yes |
| 3 | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs` | modify (L4, item 9, F3 #16, usings) | UtilitiesCS | 196 | about 165 | yes |
| 4 | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` | modify (usings only) | UtilitiesCS | 277 | about 268 | yes |
| 5 | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs` | modify (usings only) | UtilitiesCS | 388 | about 379 | yes |
| 6 | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs` | **delete** (F2) | UtilitiesCS | 240 | 0 | yes |
| 7 | `UtilitiesCS/UtilitiesCS.csproj` | remove line 820 | UtilitiesCS | n/a | n/a | n/a |
| 8 | `QuickFiler/Controllers/EfcDataModel.cs` | modify (item 7) | **QuickFiler** | 465 | about 482 | **no** |
| 9 | `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs` | **delete** (item 8; not compiled) | **ToDoModel** | 403 | 0 | **no** |
| 10 | `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` | modify: delete 359 to 382 (`SanitizeArray` test); add one `DataRow` each to the two `GetAttachmentsInfo*` tests (F3 #1, #2) | UtilitiesCS.Test | 458 | about 440 | no |
| 11 | `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs` | modify (L2 T12 and tripwire, R1 3.5) | UtilitiesCS.Test | 376 | about 425 | no |
| 12 | `UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs` | **create** (section 1.8, first table) | UtilitiesCS.Test | 0 | about 250 | no |
| 13 | `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` | **create** (section 1.8, second table) | UtilitiesCS.Test | 0 | about 340 | no |
| 14 | `UtilitiesCS.Test/EmailIntelligence/SortEmail_UndoAndMoveLog_Tests.cs` | **create** (L4, section 9.3) | UtilitiesCS.Test | 0 | about 130 | no |
| 15 | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | add three `<Compile Include>` lines after line 99 | UtilitiesCS.Test | n/a | n/a | n/a |
| 16 | `QuickFiler.Test/Controllers/EfcDataModelFilerCleanupTests.cs` | **create** (section 7.3) | **QuickFiler.Test** | 0 | about 160 | no |
| 17 | `QuickFiler.Test/QuickFiler.Test.csproj` | add one `<Compile Include>` line after line 127 | **QuickFiler.Test** | n/a | n/a | n/a |
| 18 | `docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md` | modify lines 99 and 155; add the note of 4.3 after line 149 | docs | n/a | n/a | n/a |

Files in other projects: #8 and #16/#17 (QuickFiler, QuickFiler.Test), #9 (ToDoModel; no ToDoModel.Test change). Files deliberately **not** changed: `UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs`, `UtilitiesCS/Dialogs/YesNoToAll.cs`, `UtilitiesCS/OutlookObjects/Attachment/AttachmentHelper.cs` (the `internal FilePathHelperSaveAlt` accessor suffices), `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs`, `TaskMaster/AppGlobals/AppOlObjects.cs`, `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`, both `ToDoModel.Test` files, `ToDoModel/ToDoModel.csproj`, `.editorconfig`.

All three UtilitiesCS production files that gain code are `#nullable enable`; new delegate parameters are non-nullable and the new members introduce no nullable state, so the TreatWarningsAsErrors rebuild adds no CS86xx risk beyond what R1 6.1 recorded. E and TD carry no pragma.

---

## 11. Recommended sequencing (red-first order)

1. **L1** (A): three `SaveCase` tests in #12, RED on two, fix, GREEN.
2. **L3 phase 1** (A): four-row reflection test in #13, RED on `_attachmentsAltName`, one-line fix, GREEN; record the RED run.
3. **L4 and item 9** (U, #14, #10): land the four-argument overload as a pure forward of the current body; L4-T1 and L4-T2 RED; apply 9.3 (single header line, `SanitizeArray` deleted, TST1 test deleted); GREEN.
4. **L2 and item 5** (T, #11): T12 RED (sentinel); private core with the bound, logger calls, outer `catch` removed; GREEN; T7 and T10 as pins.
5. **F1, 1.6, F2** (A, L, #7, #12, #13): seamed cores and sessions; the new tests are compile-red first; `RedirectSaveFolder` two-step RED then GREEN; the phase-2 reflection test is replaced by the structural test with its negative control; L deleted and line 820 removed.
6. **F3 and item 6** (A, T, U, S, M, #10 rows): exclusions per section 3; using blocks per section 6; the rebuild is the verification.
7. **Item 7** (E, #16, #17): seam extracted as a behavior-preserving step; the throwing test RED; `try`/`finally`; GREEN.
8. **Item 8 and CR-1** (#9, #18).
9. Full toolchain in CLAUDE.md order; coverage baseline and post-change projections; exemption re-derivation per 5.4; scoped-run TRX totals asserted non-zero; sandbox `C:\Sortemail959Sandbox` probed absent.

---

## 12. Behavior semantics after all changes (additions to R1 section 7)

| Member | Input state | Outcome |
|---|---|---|
| `SaveAttachmentAsync` core | file absent | one `trySave(FilePathSave)`; no prompt |
| same | file present, image, pictures session `Empty` | pictures session asked once; attachments session untouched |
| same | file present, answer `Yes` | `trySave(FilePathSave)`; answer released |
| same | file present, answer `YesToAll` | `trySave(FilePathSave)`; answer kept; next call not asked |
| same | file present, answer `No`/`NoToAll` | alt-name session asked while `Empty`; `Yes`/`YesToAll` saves to `FilePathSaveAlt`; single answers released |
| `SaveAttachment` core (sync) | as above without the alt-name prompt | `SaveAsFile(FilePathSave)` for `Yes`/`YesToAll`; `SaveAsFile(FilePathSaveAlt)` for `No`/`NoToAll` (L1 fixed); nothing for `Empty` |
| `RedirectSaveFolder` | any helper, destination D | `FilePathSave` and `FilePathSaveAlt` both under D, file names unchanged |
| `Cleanup_Files` | any | all four sessions `Response == Empty` |
| `WriteCSV_StartNewFileIfDoesNotExist` core | `fileExists(Combine(location, name))` false | one write of one 13-column tab-separated header line |
| same | true | no write |
| `EfcDataModel.MoveToFolderAsync` | filer throws | `ResetFilerPromptState` runs once; the exception propagates |
| same | filer returns | `ResetFilerPromptState` runs once; result returned |
| same | a guard returns `false` | no reset (unchanged) |
| five-argument `TrySaveAttachmentAsync` | any | as R1 section 7, with log4net records instead of `Debug` output and no outer catch |

---

## 13. Testing implications (strategy)

- Scoped filter `FullyQualifiedName~EmailIntelligence.SortEmail_` now matches five classes (`SortEmail_Tests`, `SortEmail_TrySaveAttachment_Tests`, `SortEmail_SaveCase_Tests`, `SortEmail_AttachmentSaving_Tests`, `SortEmail_UndoAndMoveLog_Tests`); predicted total 16 (TST1 after the deletion and two added rows) + 12 + 11 rows + 11 + 2 = 52 `[P]`, to be measured with the TRX total asserted non-zero. QuickFiler.Test scope: `FullyQualifiedName~EfcDataModelFilerCleanupTests` (3).
- Negative controls to record: revert each fix alone (L1, L2 sentinel, L3 phase-1 row, L4 both tests, `RedirectSaveFolder` second statement, item 7 `finally`, one element of `AllPromptSessions`).
- Coverage: new or changed members to report at or above 90 percent: `SaveAttachmentAsync` core, `SaveAttachment` core, `SaveCaseAsync`, `SaveCase`, `RedirectSaveFolder`, `Cleanup_Files`, the try-save private core, the CSV core, `EfcDataModel.MoveToFolderAsync` (changed lines) and `ResetFilerPromptState`. Members losing exclusions (section 3) enter the denominator covered by TST1. The deleted L and TD files and `SanitizeArray` leave the denominator (TD was never in it).
- Policy fit: every new test uses MSTest, Moq and FluentAssertions only; the only disk-touching API named anywhere is passed as a method group inside excluded wrappers; no `[DoNotParallelize]`, `Thread.Sleep`, `Task.Delay`, timeout or retry; no test writes static state except the phase-1 L3 test, which is replaced in the same change (its UT5 call-out is recorded for the evidence of that phase only).

---

## 14. Numeric Derivation Evidence

**N1: `_responseSaveFile` occurrences = 6 (1 declaration, 1 reset, 4 inside `SaveAttachmentsOld`)**
- Complete Family: every textual occurrence of the identifier in C# sources.
- Exhaustive Search Scope: all `*.cs` under the worktree.
- Inclusion Rules: any line containing `_responseSaveFile`.
- Exclusion Rules: none.
- Primary Search Strategy or Query Expression: Grep `_responseSaveFile` over `*.cs`.
- Primary Member Set: {A 25, A 32, L 155, L 180, L 187, L 191}.
- Primary Count: 6.
- Cross-check Search Strategy or Query Expression: full Reads of A (25 to 36) and L (155 to 192), locating each use by eye.
- Cross-check Member Set: {A 25 declaration, A 32 reset, L 155 read, L 180 read, L 187 write, L 191 read}.
- Cross-check Count: 6.
- Member-set Comparison: identical.

**N2: callers of `SaveAttachmentsOld` and `IsPicture` = 0 (2 declaration hits)**
- Complete Family: every textual occurrence of either name, including comments, string literals and `nameof`.
- Exhaustive Search Scope: all `*.cs` under the worktree (production and test projects).
- Inclusion Rules: any line matching either identifier.
- Exclusion Rules: the declarations themselves when counting callers.
- Primary Search Strategy or Query Expression: Grep `SaveAttachmentsOld|IsPicture\b` over `*.cs`.
- Primary Member Set: {L 28 declaration, A 312 declaration}; callers {}.
- Primary Count: 2 hits; 0 callers.
- Cross-check Search Strategy or Query Expression: full Reads of L (the file's single method) and A 311 to 320, plus the `MAX_PATH` grep of N3 showing L has no other member.
- Cross-check Member Set: declarations {L 28, A 312}; no call expression in A or L.
- Cross-check Count: 2; 0.
- Member-set Comparison: identical.

**N3: `MAX_PATH` occurrences in the SortEmail partials = 4, all in L**
- Complete Family: textual occurrences of `MAX_PATH` in the six partials.
- Exhaustive Search Scope: `SortEmail*.cs` in `UtilitiesCS/EmailIntelligence/EmailParsingSorting/`.
- Inclusion Rules: any line containing the token.
- Exclusion Rules: none.
- Primary Search Strategy or Query Expression: Grep `MAX_PATH` with glob `SortEmail*.cs`.
- Primary Member Set: {L 25, L 97, L 99, L 140}.
- Primary Count: 4.
- Cross-check Search Strategy or Query Expression: full Read of L and of A, T, U, S, M (no `MAX_PATH` token in the other five; `AttachmentHelper.MAX_PATH` is a different member in another file).
- Cross-check Member Set: {L 25 declaration, L 97, L 99, L 140 uses inside `SaveAttachmentsOld`}.
- Cross-check Count: 4.
- Member-set Comparison: identical.

**N4: `YesNoToAll.ShowDialog` references in the partials = 8 today (7 call expressions, 1 method group); after this item 4 (all method groups)**
- Complete Family: every textual reference to `YesNoToAll.ShowDialog` in the six partials.
- Exhaustive Search Scope: `SortEmail*.cs`.
- Inclusion Rules: executable references.
- Exclusion Rules: the commented line A 258.
- Primary Search Strategy or Query Expression: Grep `YesNoToAll\.ShowDialog` with glob `SortEmail*.cs`.
- Primary Member Set: {T 29 (method group), A 112, A 135, A 175, A 198, A 255, L 165, L 182} (A 258 excluded as a comment).
- Primary Count: 8.
- Cross-check Search Strategy or Query Expression: SPEC956 plan AC-mapping census token `ShowDialog 8` (plan line 1838, "TOKENS-SORTEMAIL ShowDialog 8") and the full Reads of A, T and L.
- Cross-check Member Set: same eight lines.
- Cross-check Count: 8.
- Member-set Comparison: identical. After the change: T 29 plus three initializers in A = 4 method-group references, 0 call expressions `[P]`.

**N5: `[ExcludeFromCodeCoverage]` attributes in the six partials = 28; disposition remove 7, delete 3, keep 18**
- Complete Family: attribute applications in the six partials.
- Exhaustive Search Scope: `SortEmail*.cs`.
- Inclusion Rules: lines containing `ExcludeFromCodeCoverage` other than the `using` lines.
- Exclusion Rules: `using System.Diagnostics.CodeAnalysis;` lines.
- Primary Search Strategy or Query Expression: Grep `ExcludeFromCodeCoverage` with glob `SortEmail*.cs` and `-A 1` to read the member below each.
- Primary Member Set: A {38, 63, 103, 164, 227, 241, 290, 311, 322, 333}; T {40, 165}; U {26, 82, 94, 109, 139, 172}; S {40, 109, 138, 231}; M {25, 59, 95, 247, 353}; L {27}.
- Primary Count: 10 + 2 + 6 + 4 + 5 + 1 = 28.
- Cross-check Search Strategy or Query Expression: CR956 line 34, "28 after (4 + 5 + 10 + 2 + 1 + 6)", an independent count at the #956 review.
- Cross-check Member Set: per-file counts S 4, M 5, A 10, T 2, L 1, U 6.
- Cross-check Count: 28.
- Member-set Comparison: identical per file. Disposition sets (section 3): remove {A 38, 63, 241, 290, 322, 333, U 109} = 7; delete {A 311, U 172, L 27} = 3; keep the remaining 18.

**N6: compiled callers of `Sort` (nine-argument) = 0**
- Complete Family: call expressions of `SortEmail.Sort` in compiled C# sources.
- Exhaustive Search Scope: all `*.cs` under the worktree.
- Inclusion Rules: a call whose argument list follows `Sort(`; because CSharpier breaks a nine-argument call after `(`, the line ends with `Sort(`.
- Exclusion Rules: the declaration M 248; commented lines; files not in any `.csproj`.
- Primary Search Strategy or Query Expression: Grep `(^|[^.\w])Sort\(\s*$` over `*.cs`.
- Primary Member Set: {M 248 declaration}; callers {}.
- Primary Count: 0.
- Cross-check Search Strategy or Query Expression: Grep `SortEmail\.Sort\(` over `*.cs` (qualified shape, any line position).
- Cross-check Member Set: {`ToDoModel.Test/.../SortItemsToExistingFolderTests_Unfinished.cs` 114, commented} excluded by rule; callers {}.
- Cross-check Count: 0.
- Member-set Comparison: identical (empty). Supporting: `\.SaveAttachment\(\)` matches only M 299 executable (M 155, 301 comments), inside `Sort`.

**N7: using directives per partial today = 18**
- Complete Family: `using` lines at the top of each partial.
- Exhaustive Search Scope: lines 2 to 19 of A, T, U, S, M, L.
- Inclusion Rules: lines beginning with `using`.
- Exclusion Rules: none.
- Primary Search Strategy or Query Expression: full Read of each file (lines 2 to 19 are the directives in every file; line 1 is `#nullable enable`, line 20 blank).
- Primary Member Set: `System`, `System.Collections.Generic`, `System.Diagnostics`, `System.Diagnostics.CodeAnalysis`, `System.IO`, `System.Linq`, `System.Text.RegularExpressions`, `System.Threading.Tasks`, `System.Windows.Forms`, `Deedle`, `Microsoft.Office.Interop.Outlook`, `SDILReader`, `UtilitiesCS`, `UtilitiesCS.EmailIntelligence`, `UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder`, `UtilitiesCS.OutlookExtensions`, `UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable`, `Outlook = Microsoft.Office.Interop.Outlook`.
- Primary Count: 18 in each of six files.
- Cross-check Search Strategy or Query Expression: CR956 CR-4 (line 22): "The full 18-directive using block is replicated into every partial".
- Cross-check Member Set: the same block.
- Cross-check Count: 18.
- Member-set Comparison: identical. The post-change counts in section 6.2 are `[P]`.

**N8: production call sites of the five-parameter `EfcDataModel.MoveToFolderAsync` = 4; test call sites = 1**
- Complete Family: call expressions resolving to `EfcDataModel.MoveToFolderAsync(string, bool, bool, bool, bool)`.
- Exhaustive Search Scope: all `*.cs` under the worktree.
- Inclusion Rules: calls on an `EfcDataModel` instance (`_dataModel.`, `dataModel.`, the implicit `this` inside E).
- Exclusion Rules: declarations (E 268, E 377, `EfcHomeController.ExecuteMoves.cs` 89); calls to `EfcHomeController.MoveToFolderAsync` (ExecuteMoves 78, `EfcHomeControllerExecuteMovesTests.cs` 87); the `MoveToFolderAsyncAction` property (ExecuteMoves 21, 97, 105; tests 69, 125).
- Primary Search Strategy or Query Expression: Grep `MoveToFolderAsync|InvokeFilerAsync|Cleanup_Files` over `*.cs`, filtered by the rules.
- Primary Member Set: production {E 387, ExecuteMoves 98, `EfcFormController.Actions.cs` 136, `EfcFormController.EventHandlers.cs` 184}; test {ART 316}.
- Primary Count: 4; 1.
- Cross-check Search Strategy or Query Expression: Grep `MoveToFolderAsync\(\s*$` over `*.cs` (CSharpier-wrapped call shape), filtered by the same rules.
- Cross-check Member Set: production {E 387, ExecuteMoves 98, Actions 136, EventHandlers 184}; test {ART 316}; excluded {E 268, E 377, ExecuteMoves 78, ExecuteMoves 89, `EfcHomeControllerExecuteMovesTests.cs` 87}.
- Cross-check Count: 4; 1.
- Member-set Comparison: identical.

**N9: `<Compile Include>` entries for `SortItemsToExistingFolder.cs` in `ToDoModel.csproj` = 0; `Email Utilities` entries = 1**
- Complete Family: `Compile` items of `ToDoModel/ToDoModel.csproj` naming the file or its folder, plus any wildcard item.
- Exhaustive Search Scope: all `*.csproj` under the worktree for the file name; `ToDoModel.csproj` for the folder and wildcards.
- Inclusion Rules: `Compile Include` attributes.
- Exclusion Rules: `ToDoModel.Test.csproj` entries for the two test files (different files).
- Primary Search Strategy or Query Expression: Grep `SortItemsToExistingFolder` over `*.{cs,csproj}`.
- Primary Member Set: csproj hits {`ToDoModel.Test.csproj` 75, 76 (test files)}; `ToDoModel.csproj` {}.
- Primary Count: 0.
- Cross-check Search Strategy or Query Expression: Grep `Compile Include="(\*\*|.*\*\.cs|Email Utilities)` over `ToDoModel.csproj`.
- Cross-check Member Set: {line 145 `Email Utilities\CaptureEmailAddressesModule.cs`}; no wildcard; no `SortItemsToExistingFolder.cs`.
- Cross-check Count: 1 folder entry; 0 for the file.
- Member-set Comparison: consistent (the file is absent under both strategies).

**N10: `EmailMoveWriter.Enqueue` call sites = 2**
- Complete Family: calls that enqueue a moved-mails record.
- Exhaustive Search Scope: all `*.cs`.
- Inclusion Rules: `EmailMoveWriter.Enqueue(` executable.
- Exclusion Rules: comments (`EmailFiler.cs` 208); property declarations and test stubs.
- Primary Search Strategy or Query Expression: Grep `EmailMoveWriter|\.Details\(` over `*.cs`.
- Primary Member Set: {U 106, `EmailFiler.cs` 460}.
- Primary Count: 2.
- Cross-check Search Strategy or Query Expression: the `.Details(` half of the same Grep, filtered to executable production sites that feed a TSV line: {U 103, `EmailFiler.cs` 455 (via `GetMoveDetails`)}; `EmailDetailsWrapper.cs` 19 and TD 228 excluded (wrapper; uncompiled file).
- Cross-check Member Set: two producer sites, each paired with one `Enqueue`.
- Cross-check Count: 2.
- Member-set Comparison: one-to-one (U 103 to U 106; EmailFiler 455 to 460 through `CaptureMoveDetails` 191 to 199).

**N11: header column names = 13; `Details` record fields after `Skip(1)` = 13**
- Complete Family: header assignments in U; assigned indices of `Details(this MailItem, ...)`.
- Exhaustive Search Scope: U 149 to 163; `EmailDetails.cs` 36 to 68.
- Inclusion Rules: `strAryOutput[i, 1] = "..."` assignments; `strAry[i] = ...` assignments.
- Exclusion Rules: index 0 (never assigned in either).
- Primary Search Strategy or Query Expression: Read U 151 to 163.
- Primary Member Set: indices 1 to 13 with names Triage, FolderName, Sent_On, From, To, CC, Subject, Body, fromDomain, Conversation_ID, EntryID, Attachments, FlaggedAsTask.
- Primary Count: 13.
- Cross-check Search Strategy or Query Expression: Read `EmailDetails.cs` 43 to 65 and the constant `_numberOfFields = 13` at line 23.
- Cross-check Member Set: indices {1, 2, 3, 5, 6, 4, 7, 8, 9, 10, 11, 12, 13} assigned (textual order), i.e. 1 to 13.
- Cross-check Count: 13.
- Member-set Comparison: identical index sets; the header name at index i describes the `Details` field at index i for every i.

Post-change file sizes, test totals and exemption counts in this file are `[P]` and must be measured by the plan.

---

## Automation Feasibility

No human interaction is required. Every change is a source edit in files that are either `#nullable enable` UtilitiesCS partials, the non-nullable `EfcDataModel.cs`, test files, two legacy project files with explicit `<Compile Include>` items, one uncompiled file to delete, and one specification document. Every new test runs against Moq-backed COM interfaces, recording delegates, scripted `YesNoToAllPromptSession` instances or reflection reads, with no Outlook process, dialog, disk access, sleep, retry or serialization attribute. Three maintainer decisions are recorded and none blocks execution: (a) the CSV header is treated as a defect (section 9.2; the alternative is specified); (b) the dead synchronous chain is kept and seamed rather than deleted (section 1.7); (c) the uncompiled ToDoModel file is deleted rather than edited (section 8.2).
