# Issue #614 — Store-root selection leaks a full Outlook path into the filing boundary: defect census

- Timestamp: 2026-08-26T10-30
- Branch: `bug/efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614`
- Mode: investigation only; no production or test source file was modified.
- Redaction: all mailbox, user-profile, and organization identifiers are placeholders
  (`\\mailbox@example.com`, `C:\Users\<user>\OneDrive - <Org>`), per issue #602.
- Evidence standard: every claim carries `file:line` against the current worktree. Line numbers
  cited are from this worktree, which is post-PR-#611; the runtime stack in issue.md was captured
  against the release build and its line numbers differ by a few lines in `FolderConverter.cs`.

---

## 1. Verified end-to-end reproduction chain (task 1)

Every hop below was verified by reading the source. The chain holds as hypothesized; no hop
required correction.

### 1.1 Producer: breadcrumb segment activation

1. `EfcFormController` binds breadcrumb rows with the archive root:
   `_router.BindRowsAsync(rows, scores, _globals.Ol.ArchiveRootPath, Token)` —
   `QuickFiler/Controllers/EfcFormController.cs:891`.
2. For each suggestion row the router resolves a hierarchy chain:
   `BreadcrumbBridgeRouter.FetchChainAsync` → `_provider.ResolveLeafKeyAsync` +
   `_provider.GetAncestorChainAsync` — `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:443-474`.
3. The provider always queries **all stores** with a stale snapshot allowed:
   `FolderTreeRequest.AllStores(allowStaleSnapshot: true)` —
   `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs:73-79`.
   `ResolveLeafKeyAsync` first-matches by path across every store's nodes
   (`OutlookFolderHierarchyProvider.cs:66-68`), so cross-store resolution is possible.
4. The ancestor chain walks `ParentKey` **to the store root** and returns root-first:
   `FolderTreeSnapshotQueries.GetAncestorChain` —
   `UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotQueries.cs:95-135` (doc at :96-99 states
   "to the store root"). The snapshot includes a node for each store's root folder:
   `OutlookFolderHierarchyReader.cs:97` (`var root = store.GetRootFolder();`). The store-root
   segment therefore renders as segment 0 of every chain.
5. `ActivateSegment` accepts any validated non-leaf segment; `BreadcrumbRow.ActivateSegment`
   imposes **no archive-root floor** — its only rejections are kind, index range, missing key, and
   re-activation — `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs:151-172`. The router then
   calls `SelectHierarchyPath(row, activeSegment.FullPath)` —
   `BreadcrumbBridgeRouter.cs:410-427` (call at :426); `ActivateChild` likewise at :429-441.
6. `SelectHierarchyPath` assigns `SelectedFolderPath = ToArchiveRelativePath(fullPath)` —
   `BreadcrumbBridgeRouter.cs:494-502`. `ToArchiveRelativePath` returns the input **verbatim**
   when it is not at/under `_archiveRootPath` — `BreadcrumbBridgeRouter.cs:504-526`, verbatim
   return at :525. For the store-root segment `\\mailbox@example.com`, the value returned and
   stored in `SelectedFolderPath` is the full store-root path.

### 1.2 Consumer: OK button to the throw site

7. `EfcFormController.SelectedFolder => _router?.SelectedFolderPath` —
   `QuickFiler/Controllers/EfcFormController.cs:287-293`.
8. `ActionOkAsync` guards only `null` and a `"===="` prefix — `EfcFormController.cs:700-710`
   (guard at :706) — so `\\mailbox@example.com` passes, then
   `EfcHomeController.ExecuteMovesAsync` → `ExecuteMovesCoreAsync` reads
   `_formController.SelectedFolder` — `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:66`
   — and forwards it at :75-81 / :94-101.
9. `EfcDataModel.MoveToFolderAsync(string folderpath, ...)` sets
   `DestinationOlStem = folderpath`, `OlAncestor = Globals.Ol.ArchiveRootPath`,
   `FsAncestorEquivalent = SpecialFolders["OneDrive"]` —
   `QuickFiler/Controllers/EfcDataModel.cs:258-296` (assignments at :286, :288, :289; sort at
   :292-293).
10. `EmailFiler.SortAsync` → `ResolvePaths(Folder)` —
    `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:128-135` (call at :133) and
    :377-378.
11. `EmailFilerConfig.ResolvePaths(Folder)` concatenates unconditionally:
    `DestinationOlPath = $"{OlAncestor}\\{DestinationOlStem}"` then
    `SaveFsPath = DestinationOlPath.ToFsFolderpath(OlAncestor, FsAncestorEquivalent!)` —
    `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs:183-197` (:187-188).
12. `FolderConverter.ToFsFolderpath` computes
    `fsPath = olBranchPath.Replace(olAncestorPath, fsAncestorEquivalent)` (:155), then validates
    `fsPath.Substring(3)` with every backslash removed (:157-159) against
    `IllegalFolderCharacters` and throws `ArgumentException` embedding the value (:161-167) —
    `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs`.

### 1.3 Algebraic confirmation of the stem

With `OlAncestor = \\mailbox@example.com\Archive` and
`FsAncestorEquivalent = C:\Users\<user>\OneDrive - <Org>`:

- `DestinationOlStem = \\mailbox@example.com` gives
  `DestinationOlPath = \\mailbox@example.com\Archive\\\mailbox@example.com`. The ancestor
  substring occurs exactly once (the second `\\mailbox@example.com` is not followed by
  `\Archive`), so the replace-all yields
  `fsPath = C:\Users\<user>\OneDrive - <Org>\\\mailbox@example.com`. `Substring(3)` removes
  `C:\`; removing every `\` yields exactly the reported
  `Users<user>OneDrive - <Org>mailbox@example.com`, and the only flagged character is the `.`
  from the mailbox domain. This matches the reported message byte-for-byte.
- **Precision caveat**: because the validator strips every backslash before checking, the message
  alone cannot distinguish the stem `\\mailbox@example.com` from a hypothetical stem
  `mailbox@example.com` or `\mailbox@example.com` — any stem whose backslash-stripped form is
  `mailbox@example.com` reproduces the identical message. The producer analysis in §1.1
  (store-root segment is selectable and returned verbatim) plus the reported user action
  (store-root ancestor activation) identifies `\\mailbox@example.com` as the actual value. No
  stem containing a real destination folder name reproduces the message, because the message ends
  immediately after the mailbox address.

---

## 2. Candidate defect census — confirmed / refuted

Numbering follows the delegation prompt. Verdicts: **CONFIRMED** (code exhibits the defect),
**CONFIRMED-OFF-CHAIN** (real, but not on the crash chain), **PARTIAL** (part confirmed, part
corrected). Nothing in the candidate list was fully refuted; the corrections are noted inline.

### D1 — `ToArchiveRelativePath` silent verbatim pass-through — CONFIRMED (primary producer)

`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:504-526`:

```csharp
return fullPath;   // :525 — input not at/under root: returned verbatim, no log, no rejection
```

Also note :512-515: an input exactly equal to the archive root returns `string.Empty`, which
downstream `ActionOkAsync` accepts (see D9). Empty root path (`root.Length == 0`, :507-510) also
returns the input verbatim, which is the no-archive-root binding mode used by the public
`BindRowsAsync` overload (:75-82).

### D2 — segments at/above the archive root and cross-store segments are selectable — CONFIRMED

- No floor in `BreadcrumbRow.ActivateSegment` — `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs:151-172`.
- Chain reaches the store root — `FolderTreeSnapshotQueries.cs:96-99, 109-135`; store root is a
  snapshot node — `OutlookFolderHierarchyReader.cs:97`.
- All stores, stale allowed — `OutlookFolderHierarchyProvider.cs:73-79`; cross-store first-match
  leaf resolution — `OutlookFolderHierarchyProvider.cs:64-68`.
- Router forwards any activated segment's `FullPath` — `BreadcrumbBridgeRouter.cs:426, :440`.

One correction of emphasis: the leaf segment itself is *not* activatable
(`segmentIndex >= _segments.Count - 1` rejected, `BreadcrumbRow.cs:156`), so the exposure is
ancestor segments (including the store root) and expanded children, exactly as reported.

### D3 — `ToHierarchyPath` mirror hazard — CONFIRMED (lookup-side, lower severity)

`BreadcrumbBridgeRouter.cs:140-163`: a presented target not at/under the root is prefixed:
`return root + "\\" + presentedTarget.TrimStart('\\', '/');` (:162). For an out-of-root absolute
target (which `FolderPredictor.ProjectSuggestionPath` deliberately passes through unchanged,
see D4a) this fabricates a nonexistent hierarchy path such as
`\\mailbox@example.com\Archive\other@example.org\Foo`. Consequence today: `ResolveLeafKeyAsync`
returns null, `FetchChainAsync` returns null (:450-459), and the row falls back to single-segment
rendering — silent wrong lookup, not a crash. Related exposure: `SelectRow` then uses
`row.FilingTarget` **verbatim** (:484-487), so *row-selecting* such an out-of-root suggestion
leaks the full path into `SelectedFolderPath` without any segment activation at all. This is a
second producer of the same class as D1.

### D4 — `EmailFilerConfig` concatenates with no stem validation; #609 scope — CONFIRMED

- `ResolvePaths(Folder)`: `EmailFilerConfig.cs:187-188`. `ResolvePaths()`: :203-204. Neither
  validates that `DestinationOlStem` is relative; `$"{OlAncestor}\\{DestinationOlStem}"` accepts
  a `\\`-rooted stem silently.
- Same-class neighbors in the same file: `GetStem` uses an unanchored, replace-all
  `folderPath.Replace(olAncestor, "")` (:232-235), and `IsDeleteRelevant` uses substring
  `Contains(OlAncestor)` (:171).

**What #609 / PR #611 actually did and did not do** (read from
`docs/features/active/2026-08-25-efc-full-path-destination-resolution-regression-609/issue.md`
and `plan.remediation.2026-08-25T14-18.md`):

- The final #609 remediation modified **only** `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`
  (+ its test file). The plan explicitly prohibited touching `BreadcrumbBridgeRouter.cs`,
  `EmailFilerConfig.cs`, `EfcDataModel.cs`, and `EfcFormController.cs`
  (plan.remediation :17-19).
- The fix is `ProjectSuggestionPath` — `FolderPredictor.cs:845-858` — applied to suggestion text
  (`AddSuggestions`, :804-808) and suggestion rows/scores (`AddSuggestionRows`, :832-843). It
  strips the prefix **only when** the suggestion starts with `ArchiveRootPath + "\"`; the plan's
  own acceptance criteria require that "out-of-root full paths are byte-for-byte unchanged"
  (plan.remediation P2-T1, :41).
- Therefore #609 covers exactly the "stem already carries the archive root prefix" case for
  *persisted suggestions*, and deliberately does **not** cover "stem is an unrelated absolute
  path" (a store root, a cross-store path, or any out-of-root full path). The
  `Issue609_ResolvePaths_PrefixesAtMailboxArchiveRootExactlyOnce` test
  (`UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs:255-272`) asserts only the
  single-prefix construction for a *relative* stem; it adds no stem validation.

### D5 — `FolderConverter.ToFsFolderpath` independent defects — ALL SEVEN CONFIRMED

File: `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs`.

- **5a — validates the caller-supplied filesystem ancestor.** :157-159 validates
  `fsPath.Substring(3)`, which contains the whole `fsAncestorEquivalent` minus the drive prefix.
  Any legitimate OneDrive root containing `.` (e.g., an organization name with a dot) fails
  **every** filing, regardless of the stem. Only derived segments should be validated.
- **5b — wrong character class.** :39-42:
  `@"[\/:*?""<>|]."` — as a char array this is `[ \ / : * ? " < > | ] .`. It wrongly bans `.`,
  `[`, `]` (all legal in Windows names) and omits `Path.GetInvalidFileNameChars()` control
  characters and the real per-segment rules (trailing dot/space, reserved device names).
  `SanitizeFilename` (:133-139) already uses the correct `Path.GetInvalidFileNameChars()` class,
  so the file disagrees with itself.
- **5c — `Substring(3)` assumes `X:\`.** :158. A UNC or relative `fsAncestorEquivalent` is
  silently mangled (or, for strings shorter than 3, throws `ArgumentOutOfRangeException`). Also,
  when the `Replace` in 5d fails to match (case mismatch), `fsPath` is still the Outlook path and
  `Substring(3)` chops `\\m` off the mailbox name instead of a drive prefix.
- **5d — unanchored, case-sensitive replace-all.** :155
  `olBranchPath.Replace(olAncestorPath, fsAncestorEquivalent)`. Should be a prefix-anchored,
  `OrdinalIgnoreCase` strip. A repeated ancestor substring is replaced at every occurrence; a
  case-differing ancestor is not replaced at all.
- **5e — exception message leaks host identifiers.** :161-167 embeds `fsPathExDividers` — the
  user-profile path plus mailbox address — into the `ArgumentException` message. Runtime
  counterpart of open issue #602.
- **5f — "Remove illegal characters" always yields empty string.** :110-113:
  `illegalFolderName.Replace(illegalFolderName, "")` replaces the whole string with `""`
  unconditionally. Intended semantics were per-character removal. Note the existing test
  **codifies the bug**: `UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs:329`
  asserts `result["Remove illegal characters"]()...Should().BeEmpty()`. The fix must update this
  assertion (treating the current test as spec would freeze the defect).
- **5g — `ResolveOlRoot` uses `Contains`.** :226-242 (:228, :232). Substring match instead of a
  prefix test: `\\mailbox@example.com\Archive2\...` matches the `ArchiveRootPath` branch, and a
  mid-string occurrence anywhere qualifies. Should be `StartsWith` on a separator-terminated
  prefix.
- **Additional (same file, found while reading): unused `ask` parameter.** `ToFsFolderpath`
  declares `bool ask = true` (:145) and never reads it; the signature implies an interactive
  fallback that does not exist. Fold into the D5 cleanup.

### D6 — `AppOlObjects.ArchiveRootPath` unverified and default-store-scoped — CONFIRMED

`TaskMaster/AppGlobals/AppOlObjects.cs`: `Root` is
`App.Session.DefaultStore.GetRootFolder()` (:201-210, :207);
`ArchiveRootPath = Path.Combine(Root.FolderPath, "Archive")` (:237-248, :244) — a string combine,
never checked for existence (the actual folder is resolved separately and later by
`LoadArchiveRoot`, :253-256). It is never scoped to the store owning the current folder. With a
folder from another store: `EfcDataModel.MoveToFolderAsync(MAPIFolder,...)`'s
`Replace(olAncestor, "")` no-ops (full path becomes the stem — the same crash class via the
create-folder path, D8), `EmailFilerConfig.IsDeleteRelevant`'s `Contains` fails, and
`FolderConverter.ResolveOlRoot` throws "not a branch of any known root". Confirmed as the
configuration-level defect that makes D1's verbatim pass-through inevitable for cross-store and
store-root inputs.

### D7 — `AppFileSystemFolderPaths` arbitrary fallback and `Contains` matching — PARTIAL

`TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs`:

- **Fallback chain confirmed.** `LoadFolders` resolves the `OneDrive` key from
  `OneDriveCommercial` → `OneDrive` → `OneDrivePersonal` env vars (:206-226), then falls back to
  `AppData` (:229-232) and finally to `SpecialFolders.First().Value` — an arbitrary dictionary
  entry (:235). A wrong `FsAncestorEquivalent` is accepted silently; filing then writes `.msg`
  artifacts under AppData or an arbitrary folder instead of failing fast. On-chain in the sense
  that `EfcDataModel.MoveToFolderAsync` consumes `SpecialFolders["OneDrive"]`
  (`EfcDataModel.cs:276-289`).
- **`MatchBestSpecialFolder` `Contains` confirmed but OFF-CHAIN.** The pure helper (:77-91,
  `Contains` at :87) matches by substring, not path prefix, and existing tests codify substring
  semantics (`TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs:96-115`).
  However, a repo search found **no production caller** of `MatchBestSpecialFolder` (only the
  interface member, the instance delegator, and test doubles). It is not on the #614 chain;
  reclassify as latent hardening, not a #614 fix target.

### D8 — `EfcDataModel.MoveToFolderAsync(MAPIFolder, olAncestor, ...)` — CONFIRMED

`QuickFiler/Controllers/EfcDataModel.cs:335-360`:

```csharp
var folderpath = folder.FolderPath.Replace(olAncestor, "");   // :344 unanchored replace-all
if (folderpath.StartsWith(@"\")) { folderpath = folderpath.Substring(1); }  // :345-348 strips ONE '\'
```

When `folder` is not under `olAncestor` (cross-store, store root, or D6 mismatch), the `Replace`
no-ops and `Substring(1)` leaves `\mailbox@example.com\...` — a separator-leading absolute stem —
which flows into the string overload as `DestinationOlStem`. Live callers:
`EfcFormController.ButtonCreate_Click` (:500-507) and `CreateFolderAsync` (:778-787), both with
`_globals.Ol.ArchiveRootPath`. Same defect class as D1, second entry point into D4.

### D9 — `ActionOkAsync` guard asymmetry (found while reading; on-chain)

`EfcFormController.cs:706` accepts any non-null string that does not start with `"===="`,
including `string.Empty` — which is exactly what `ToArchiveRelativePath` returns for a selection
of the archive root itself (`BreadcrumbBridgeRouter.cs:512-515`). An empty stem produces
`DestinationOlPath = OlAncestor + "\"` (trailing separator) and a failed folder resolution at
runtime. The stricter `IsValidSelection` (`EfcFormController.cs:1038-1050`, rejects
`Length < 3`) is used by the create-folder paths but **not** by OK. The two guards should share
one predicate at the same boundary as the D4 stem validation.

### Refutation record

- No candidate was refuted outright. Corrections to the hypotheses as stated: (D2) leaf segments
  are not activatable, only ancestors/children — the exposure is narrower than "any segment";
  (D3) the mirror hazard's present-day consequence is a silent failed lookup plus fallback
  rendering, not a crash, and the crash-class leak via presented rows actually flows through
  `SelectRow`'s `FilingTarget` pass-through rather than through `ToHierarchyPath`; (D7) the
  `MatchBestSpecialFolder` half of the candidate is real but has no production caller, so it is
  not part of the #614 defect set.
- The rejected Copilot hypothesis is re-confirmed as symptom-level: `.` in
  `IllegalFolderCharacters` (D5b) is independently wrong, but removing it would make the observed
  call *succeed* and file mail to a destination derived from
  `C:\Users\<user>\OneDrive - <Org>\\\mailbox@example.com` — converting a crash into silent
  misfiling. It must not be presented as the fix for #614.

---

## 3. Off-chain defects to promote (one line each)

1. **Orphaned duplicate `UtilitiesCS.FolderConverter` dead file with always-false guards** —
   Low — `UtilitiesCS/EmailIntelligence/FolderConverter.cs` declares the same fully-qualified
   static class name as `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` but is not
   compiled (`UtilitiesCS/UtilitiesCS.csproj:1054` includes only the OutlookObjects file); it
   contains a self-comparison `olBranchURI.Scheme != olBranchURI.Scheme` (:30, always false) and
   `relativePath[0].Equals(".")` (char-vs-string, :40, always false), and would be CS0101 if ever
   auto-included (e.g., SDK-style migration). Recommend deletion.
2. **`FolderPredictor.CreateFolder` indexes `parentBranchPath[0]` behind a non-short-circuit
   `|`** — Low/Medium — `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:691`:
   `olAncestor.EndsWith('\\'.ToString()) | parentBranchPath[0] == '\\'` evaluates both operands,
   so an empty `parentBranchPath` throws `IndexOutOfRangeException` regardless of the left
   operand. (Not reachable from the OK path; the async sibling at :752 does not index.)
3. **`AppFileSystemFolderPaths.MatchBestSpecialFolder` substring matching** — Low — `Contains`
   rather than a path-prefix test (`AppFileSystemFolderPaths.cs:87`); no production caller today,
   but the codified-substring tests (`AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs:96-115`)
   would freeze the wrong semantics for any future caller.

---

## 4. Testability seams and existing infrastructure

No live Outlook, no temporary files, no wall-clock waits are needed for any fix below. Existing
patterns to reuse:

| Target | Seam | Existing precedent |
| --- | --- | --- |
| `BreadcrumbBridgeRouter` (D1, D2, D3, D9-adjacent) | Constructor seams `Mock<IFolderHierarchyProvider>` + `Mock<IBreadcrumbWebHost>`; internal `BindRowsAsync(rows, scores, archiveRootPath, ct)` overload reachable via `[InternalsVisibleTo("QuickFiler.Test")]` (`QuickFiler/Properties/AssemblyInfo.cs:5`); inbound actions driven by `ProcessInboundAsync(json)`; selection observed on `SelectedFolderPath` / `SelectedFolderPathChanged` | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs:33-123` (mock wiring, `SetupProviderChain`, `Inbound` helper) and `BreadcrumbBridgeRouterIssue439Tests.cs:20-116` (archive-root binding, `segmentActivate`/`rowSelected` payloads, `SelectedFolderPath` assertions) |
| `EmailFilerConfig` (D4) | Pure configuration seam: property-init `EmailFilerConfig` with `Globals = null` and `ResolvePaths()`; `Mock<IApplicationGlobals>`/`Mock<IOlObjects>`/`Mock<Folder>` for the `ResolvePaths(Folder)` overload (Outlook interop interfaces are Moq-able) | `UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs:199-272`, especially `Issue609_ResolvePaths_PrefixesAtMailboxArchiveRootExactlyOnce` (:255-272) |
| `FolderConverter` (D5) | Pure static extension methods; injectable delegates already exist for every dialog: `AlternativeFolderPrompt`, `AlternativeFolderSelectionDialog`, `AlternativeFolderInputDialog` (`FolderConverter.cs:17-37`) | `UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs` (delegate injection at :170-251, dictionary probing at :316-330, throw-path at :54) — note :329 asserts the D5f bug and must be updated with the fix |
| `EfcDataModel.MoveToFolderAsync(MAPIFolder,...)` (D8) | No pure seam exists today for the inline stem derivation (:344-348). Recommended minimal seam: extract an `internal static string ToArchiveRelativeStem(string folderPath, string olAncestor)` pure helper (mirroring the `FolderPredictor.ProjectSuggestionPath` and `AppFileSystemFolderPaths.MatchBestSpecialFolder` extract-pure-static precedent) and test it directly; `Mock<MAPIFolder>`/`Mock<Folder>` supply `FolderPath` | `QuickFiler.Test/Controllers/EfcDataModelTests.cs` (Moq `MailItem`/`Folder` fixtures, `CreateGlobals()` helper) |
| `AppOlObjects.ArchiveRootPath` (D6) | The property itself is COM-bound (DefaultStore). Consumers already take it through `IOlObjects.ArchiveRootPath`, which is fully mockable — validation belongs at the consumers or in a pure helper, not in a live-Outlook test | `EmailFilerConfig_Tests.cs:97-109` (`Mock<IOlObjects>` returning fixed paths); `TaskMaster.Test/AppGlobals/AppOlObjectsTests.cs` |
| `AppFileSystemFolderPaths` (D7) | Pure static internal `MatchBestSpecialFolder(IReadOnlyDictionary, string)` already extracted (:77-91). The `LoadFolders` fallback chain reads environment variables directly and has no seam; if the planner hardens it, introduce an injectable `Func<string,string> getEnvironmentVariable` delegate seam (repo DI-seam preference #2) | `TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs` |

---

## 5. The one named regression test

- **Project/file**: `UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs`
- **Class**: `EmailFilerConfig_Tests`
- **Proposed name**: `Issue614_ResolvePaths_WithStoreRootStem_RejectsNonRelativeStemWithoutLeakingIdentifiers`
- **Seam**: the pure configuration seam already used by the Issue609 test (:255-272) — no
  Outlook, no filesystem, no globals (`Globals = null`, parameterless `ResolvePaths()`).
- **Exact input**: `OlAncestor = @"\\mailbox@example.com\Archive"`,
  `DestinationOlStem = @"\\mailbox@example.com"`,
  `FsAncestorEquivalent = @"C:\Users\testuser\OneDrive - Contoso"` (a realistic fs ancestor with
  no characters from the current illegal set, so the `.` in the mailbox domain is the sole
  trigger — matching the field report).
- **Pre-fix behavior (verified by §1.3 algebra)**: `ResolvePaths()` reaches
  `FolderConverter.ToFsFolderpath`, which throws `ArgumentException` whose message contains
  `Users testuser OneDrive - Contosomailbox@example.com` — the reported crash, reproduced
  deterministically (the `ask` parameter is dead, so no dialog path can fire).
- **Assertion shape (fails pre-fix, passes post-fix)**: the act throws the *contract* exception
  (deterministic type/message naming `DestinationOlStem` as non-archive-relative), and the
  exception message does **not** contain `mailbox@example.com` or the fs ancestor. Pre-fix the
  thrown exception is the FolderConverter one with the leaked value, so both assertions fail;
  post-fix the D4 boundary guard satisfies them.
- **Producer-side companion** (recommended but secondary): a
  `BreadcrumbBridgeRouterTests`-pattern test binding with
  `archiveRootPath = @"\\mailbox@example.com\Archive"` and a provider chain whose segment 0 is
  `\\mailbox@example.com`, sending `segmentActivate` for index 0, asserting
  `SelectedFolderPath` is not the store-root full path (pre-fix it is, verbatim, per D1).

---

## 6. Interaction with open issue #499 (stale SelectedFolderPath after rebind)

#499 (recorded in
`docs/features/potential/promoted/2026-08-08-breadcrumb-router-stale-selectedfolderpath-after-rebind.md`)
is that `BindRowsAsync` clears `_selectedRowId` (`BreadcrumbBridgeRouter.cs:136`) but not
`SelectedFolderPath` (:59), so a keystroke rebind leaves a stale filing target. Both issues touch
the same field. To avoid regressing or silently absorbing #499:

- The #614 router fix should confine its writes to the **selection actions**
  (`SelectHierarchyPath` / `SelectRow` / `ToArchiveRelativePath`) and must **not** change
  `BindRowsAsync`'s clearing semantics — clearing (or restoring) on rebind is #499's decision,
  explicitly deferred there because it is an observable contract change.
- On rejecting an out-of-root activation, prefer "leave `SelectedFolderPath` unchanged and log /
  post a diagnostic" over "set it to null": nulling on rejection would partially implement
  #499's clear-on-invalidation semantics as a side effect and pre-empt its open design question
  (whether to raise `SelectedFolderPathChanged(null)`).
- The #614 **filing-boundary guard (D4) independently protects the #499 scenario**: even a stale
  full-path value can no longer reach `ToFsFolderpath`. A stale *relative* stem (the common #499
  case) is unaffected either way.
- Regression protection: keep the existing #439/#609 router tests green (direct row selection,
  ancestor activation, child activation, banner and trash pseudo-rows, mailbox roots containing
  `@` — `BreadcrumbBridgeRouterIssue439Tests.cs`, `EmailFilerConfig_Tests.cs:255-272`, and the
  `Issue609_FolderPredictor*` tests in
  `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs`).

---

## 7. Recommended path-representation contract

### Selected approach: shared pure validator + two-point enforcement (no new wrapper type)

Introduce one small host-neutral pure static class in `UtilitiesCS` (e.g.,
`UtilitiesCS.OutlookObjects.Folder.ArchiveStemContract`, well under the 500-line ceiling):

- `bool IsFullOutlookPath(string value)` — true when the value is `\\`-rooted or
  separator-leading (`value.StartsWith("\\")` covers both after the D8 fix stops producing
  single-`\` remnants).
- `void RequireArchiveRelativeStem(string value, string paramName)` — fail-fast
  `ArgumentException` with a redacted message (names the parameter and the violated rule; never
  embeds the value — issue #602).
- `bool TryMakeArchiveRelative(string fullPath, string archiveRoot, out string stem)` — the
  prefix-anchored, `OrdinalIgnoreCase`, separator-aware strip (the correct core of
  `ToArchiveRelativePath` / `ProjectSuggestionPath` / the D8 derivation), returning `false`
  instead of passing the input through when it is not at/under the root.

Enforcement points (minimal-change form):

1. **Producer (router)** — `SelectHierarchyPath` uses `TryMakeArchiveRelative`; on `false` it
   logs and returns without changing the selection (D1/D2). This is the clamp/reject decision
   surfaced to the planner: rejection is recommended over clamping-to-root because clamping would
   silently select the archive root, which is itself not a valid filing target (D9).
2. **Consumer (filing boundary)** — both `EmailFilerConfig.ResolvePaths` overloads call
   `RequireArchiveRelativeStem(DestinationOlStem, nameof(DestinationOlStem))` before
   concatenation (D4). This is the layer that carries the named regression test and protects
   every producer, including #499's stale value and D8's create-folder path.
3. **Secondary producer** — `EfcDataModel.MoveToFolderAsync(MAPIFolder,...)` derives its stem via
   `TryMakeArchiveRelative` (extracted pure helper, D8) instead of `Replace`+`Substring(1)`.

`FolderConverter` (D5a-e,g) is corrected as independent hardening in the same fix: validate only
derived per-segment names with the real Windows rules, anchor the prefix strip, drop
`Substring(3)`, redact the message. `AppOlObjects.ArchiveRootPath` (D6) and the `OneDrive`
fallback (D7) can be scoped to fail explicitly rather than silently, but the D4 guard makes them
non-fatal for #614; the planner may defer them to follow-up issues if budget requires.

### Behavior changes visible to the user vs internal hardening

- **Visible**: store-root / cross-store / above-archive segment activation no longer changes the
  filing selection (with a diagnostic); OK with an empty or non-relative selection fails fast
  with a clear, redacted message instead of the leaked `ArgumentException`; folder names
  containing `.`, `[`, `]` become filable once D5b is fixed (today they crash); the
  "Remove illegal characters" dialog option starts doing what it says (D5f).
- **Internal hardening**: anchored/`OrdinalIgnoreCase` prefix operations (5d, `GetStem`,
  `IsDeleteRelevant`, D8), `Substring(3)` removal (5c), exception redaction (5e),
  `ResolveOlRoot` prefix test (5g), unused `ask` removal.

### Rejected alternatives (brief)

- **Delete `.` from `IllegalFolderCharacters`** — rejected per the delegation prompt: symptom
  level; converts the crash into silent misfiling to a path derived from the mailbox store root.
- **A wrapper value type (`ArchiveRelativeStem` struct) threaded through the public APIs** —
  rejected for #614: it would ripple through `EmailFilerConfig`, `EfcDataModel`,
  `EfcHomeController`, `FolderPredictor`, and serialized configuration, breaking public surface
  for the same protection two validation call sites provide. Contrary to Simplicity-first;
  net48 also lacks `init`/records, making the ergonomics worse. Revisit only if a third
  representation appears.
- **Fix only the router (producer)** — rejected: leaves D8's producer, #499's stale value, and
  any persisted out-of-root suggestion able to reach the unguarded concatenation. The boundary
  guard is the invariant; the router fix is the UX.

---

## 8. Test strategy notes for the planner (no test code here)

- Framework per repo policy: MSTest + Moq + FluentAssertions; AAA structure; no temp files, no
  sleeps; all COM types mocked as in the cited existing tests.
- New-code coverage target >= 90% applies to `ArchiveStemContract` and every touched branch;
  the pure helpers make this cheap.
- Scenario matrix (maps 1:1 to defects): store-root stem rejection (D4, the named regression);
  store-root segment activation and cross-store segment activation leave selection unchanged
  (D1/D2); archive-root-exact activation (empty-stem, D9) rejected at OK; out-of-root suggestion
  row selection (D3-adjacent `FilingTarget` path); D8 pure-helper derivation for under-root,
  store-root, cross-store, and case-differing ancestors; D5 per-segment validation (dotted and
  bracketed fs roots now pass; trailing dot/space and reserved names fail; UNC roots; repeated
  ancestor substring; message contains no host identifiers); D5f dictionary option removes only
  illegal characters (updating `FolderConverterTests.cs:329`).
- Must-stay-green set (no behavior change): `BreadcrumbBridgeRouterIssue439Tests` (direct row,
  ancestor, child, banner, trash, case-insensitive root match),
  `Issue609_ResolvePaths_PrefixesAtMailboxArchiveRootExactlyOnce`, `Issue609_FolderPredictor*`
  projection tests, and the existing `EmailFilerConfig_Tests` / `FolderConverterTests` suites
  except the one assertion at `FolderConverterTests.cs:329` that codifies D5f.
