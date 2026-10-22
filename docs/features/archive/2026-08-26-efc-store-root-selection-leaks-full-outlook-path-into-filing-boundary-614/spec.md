# efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary (Spec)

- **Issue:** #614
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-26T20-10
- **Status:** Implemented - all 26 acceptance criteria verified and checked off
- **Version:** 1.1 (acceptance criteria checked off after implementation)
- **Work Mode:** `full-bug`

> **Authoritative acceptance-criteria source.** Work mode for this issue is `full-bug`. Per
> `.claude/skills/acceptance-criteria-tracking/SKILL.md`, `spec.md` is the sole AC source for
> `full-bug` work. No `user-story.md` exists for this feature and none is to be created; any
> checkbox list outside the `## Acceptance Criteria` section of this file is informational and is
> not tracked for check-off.

> **Evidence basis.** Every factual claim in the Root Cause Analysis and Proposed Fix sections
> traces to `research/2026-08-26T10-30-store-root-path-leak-defect-census-research.md`, which
> verified each hop with `file:line` evidence against this worktree (post-PR-#611). Line numbers
> cited here are from that artifact; the runtime stack captured in `issue.md` came from a release
> build and its `FolderConverter.cs` line numbers differ by a few lines. Where the research records
> an open question or a precision caveat, this spec carries that uncertainty forward rather than
> resolving it by assertion.

> **Redaction.** All mailbox addresses, user-profile paths, host names, and organization names in
> this document are placeholders (`\\mailbox@example.com`, `C:\Users\<user>\OneDrive - <Org>`),
> per the host-identifier leakage constraint tracked in open issue #602.

## Context
Filing an email from the Email Filer Controller (EFC) throws `ArgumentException` from
`FolderConverter.ToFsFolderpath` because a full Outlook hierarchy path (the mailbox store root,
e.g. `\\mailbox@example.com`) reaches `EmailFilerConfig.DestinationOlStem`, which by contract must
be an archive-relative stem. The reported "illegal character" (the `.` in the mailbox address) is a
downstream symptom of a path-representation contract that is enforced at no boundary; the true
defect set spans breadcrumb hierarchy selection, archive-root resolution, special-folder
resolution, and the Outlook-path-to-filesystem-path converter itself.

Environment:
- OS/version: Windows 11 Pro 10.0.26200; Outlook desktop with a Microsoft 365 mailbox whose store
  display name is the account email address.
- Python version: Not applicable; the affected implementation and tests are C# (.NET Framework
  4.8.1 VSTO add-in).
- Command/flags used: Not applicable; reached interactively through the EFC folder-list breadcrumb
  surface and the OK button.
- Data source or fixture: Archive root `\\mailbox@example.com\Archive`; OneDrive commercial root
  `C:\Users\<user>\OneDrive - <Org>`.

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Filing fails outright for the affected selection path, and adjacent defects in the same chain can
silently produce a wrong destination rather than an exception.


## Repro & Evidence
Steps to Reproduce:
1. Open the QuickFiler EFC surface against a mailbox whose store root path is
   `\\mailbox@example.com` and whose archive root is `\\mailbox@example.com\Archive`.
2. Bind folder rows so that a breadcrumb row renders its ancestor chain
   (`IFolderHierarchyProvider.GetAncestorChainAsync`, which is requested with
   `FolderTreeRequest.AllStores` and therefore walks all the way up to the store root).
3. Activate an ancestor segment at or above the archive root. In the observed case this was the
   store-root segment itself.
4. Press OK to execute the move.

Expected:
Either the segment at or above the archive root is not selectable as a filing destination, or the
selection is clamped or rejected before it reaches the filing boundary. Every value that flows into
`EfcDataModel.MoveToFolderAsync` and `EmailFilerConfig.DestinationOlStem` remains an
archive-relative stem (for example `Clients\North`), and `FolderConverter.ToFsFolderpath` validates
only the segments it derives, never the caller-supplied filesystem ancestor root, which
legitimately contains `.`, spaces and `-`.

Actual:
`ArgumentException` is thrown and the move fails:

```
System.ArgumentException: fsPathExDividers has a value of
Users<user>OneDrive - <Org>mailbox@example.com which contains illegal characters .
Parameter name: fsPath
   at UtilitiesCS.FolderConverter.ToFsFolderpath(String olBranchPath, String olAncestorPath, String fsAncestorEquivalent, Boolean ask) in UtilitiesCS\OutlookObjects\Folder\FolderConverter.cs:line 169
   at UtilitiesCS.EmailIntelligence.EmailParsingSorting.EmailFilerConfig.ResolvePaths(Folder currentFolder) in UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailFilerConfig.cs:line 188
   at UtilitiesCS.EmailIntelligence.EmailParsingSorting.EmailFiler.ResolvePaths(Folder currentFolder) in UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailFiler.cs:line 378
   at UtilitiesCS.EmailIntelligence.EmailParsingSorting.EmailFiler.SortAsync(...) in UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailFiler.cs:line 133
   at QuickFiler.Controllers.EfcDataModel.MoveToFolderAsync(...) in QuickFiler\Controllers\EfcDataModel.cs:line 293
   at QuickFiler.EfcHomeController.ExecuteMovesCoreAsync() in QuickFiler\Controllers\EfcHomeController.ExecuteMoves.cs:line 75
   at QuickFiler.EfcHomeController.ExecuteMovesAsync() in QuickFiler\Controllers\EfcHomeController.ExecuteMoves.cs:line 40
   at QuickFiler.Controllers.EfcFormController.ActionOkAsync() in QuickFiler\Controllers\EfcFormController.cs:line 716
   at QuickFiler.Controllers.EfcFormController.ButtonOK_Click(...) in QuickFiler\Controllers\EfcFormController.cs:line 441
```

Algebraic reconstruction of the reported message. This is the evidence that the stem, not the
character class, is the defect. `ToFsFolderpath` strips the first three characters (`C:\`) and then
removes every `\`, so the reported value implies:

```
fsPath == "C:\Users\<user>\OneDrive - <Org>" + "\" + "\\mailbox@example.com"
```

Working backwards through `EmailFilerConfig.ResolvePaths`:

```
DestinationOlPath = OlAncestor + "\" + DestinationOlStem
fsPath            = DestinationOlPath.Replace(OlAncestor, FsAncestorEquivalent)
```

With `OlAncestor = \\mailbox@example.com\Archive` and
`FsAncestorEquivalent = C:\Users\<user>\OneDrive - <Org>`, the only stem that reproduces the
reported string exactly is `DestinationOlStem == "\\mailbox@example.com"`, that is, the mailbox
store root carried through as a full Outlook path. No stem containing a real destination folder
name reproduces the message, because the message ends immediately after the mailbox address.

**Precision caveat carried from the research (§1.3).** Because the validator strips every backslash
before checking, the message alone cannot distinguish the stem `\\mailbox@example.com` from a
hypothetical `mailbox@example.com` or `\mailbox@example.com`; any stem whose backslash-stripped
form is `mailbox@example.com` reproduces the identical message. The producer analysis (research
§1.1: the store-root segment is selectable and returned verbatim) plus the reported user action
(store-root ancestor activation) identifies `\\mailbox@example.com` as the actual value. This is a
strong inference from two independent lines of evidence, not a direct observation of the field
value.

Logs / Screenshots:
- [x] Attached minimal logs or screenshot
- Snippet: the exception and stack shown above, captured at the throw site.


## Scope & Non-Goals

### In scope

Every confirmed defect on the path-representation chain that contributes either to this crash or to
a silently-wrong filing destination on the same chain. Concretely, defects **D1 through D9** as
confirmed in the research census (research §2), namely:

1. **D1** — `BreadcrumbBridgeRouter.ToArchiveRelativePath` verbatim pass-through
   (`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:504-526`, verbatim return at `:525`).
2. **D2** — activatable ancestor and child segments at, above, or outside the archive root
   (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs:151-172`;
   `BreadcrumbBridgeRouter.cs:426, :440`).
3. **D3** — the presented-row leak: `SelectRow` uses `row.FilingTarget` verbatim
   (`BreadcrumbBridgeRouter.cs:484-487`), and the `ToHierarchyPath` mirror hazard
   (`BreadcrumbBridgeRouter.cs:140-163`, prefixing at `:162`).
4. **D4** — unvalidated concatenation in both `EmailFilerConfig.ResolvePaths` overloads
   (`UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs:187-188` and `:203-204`),
   plus the same-class neighbours `GetStem` (`:232-235`) and `IsDeleteRelevant` (`:171`).
5. **D5** — all seven confirmed `FolderConverter` defects (5a–5g) plus the unused `ask` parameter
   (`UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs`).
6. **D6** — `AppOlObjects.ArchiveRootPath` is an unverified string combine scoped to the default
   store (`TaskMaster/AppGlobals/AppOlObjects.cs:201-210, :237-248, :253-256`).
7. **D7 (fallback-chain half only)** — the `OneDrive` special-folder fallback to `AppData` and then
   to `SpecialFolders.First().Value` (`TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs:206-235`),
   which is on-chain because `EfcDataModel.MoveToFolderAsync` consumes `SpecialFolders["OneDrive"]`
   (`QuickFiler/Controllers/EfcDataModel.cs:276-289`).
8. **D8** — `EfcDataModel.MoveToFolderAsync(MAPIFolder, olAncestor, ...)` unanchored replace plus
   single-backslash strip (`QuickFiler/Controllers/EfcDataModel.cs:335-360`, `:344-348`).
9. **D9** — `ActionOkAsync` guard asymmetry, accepting `""` and any non-`"===="` string
   (`QuickFiler/Controllers/EfcFormController.cs:706`), while the stricter `IsValidSelection`
   (`:1038-1050`) is not used on the OK path.

Also in scope: the shared `ArchiveStemContract` validator introduced to enforce the contract, the
regression and unit tests enumerated in `## Test Strategy`, and the evidence artifacts required by
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.

**Budget note for the planner.** The research (§7) states that the D4 boundary guard makes D6 and
the D7 fallback non-fatal for #614, and that the planner may defer them to follow-up issues if
budget requires. This spec keeps them in scope because both produce a silently-wrong filing
destination on the same chain. If the planner defers either one, that deferral must be recorded as
a scope reduction with a promoted follow-up issue, not as a silent omission; the corresponding
acceptance criteria (AC14, AC15) must then be struck by an explicit, dated correction-log entry in
this file rather than left unchecked.

### Out of scope / non-goals

- The three **off-chain** research findings (research §3), which are being promoted to their own
  issues: the orphaned duplicate `UtilitiesCS/EmailIntelligence/FolderConverter.cs` dead file with
  always-false guards; `FolderPredictor.CreateFolder`'s non-short-circuit `|` before
  `parentBranchPath[0]` (`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:691`); and the
  `MatchBestSpecialFolder` substring-matching half of D7
  (`TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs:87`), which the research reclassified as
  latent hardening because a repository search found no production caller.
- **Issue #499** (`breadcrumb-router-stale-selectedfolderpath-after-rebind`). It touches the same
  `SelectedFolderPath` field, so this fix must state its interaction explicitly and must neither
  absorb nor regress it. See `### Boundaries and invariants to preserve`.
- **Issue #609 rework.** PR #611 modified only
  `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` and its test file
  (`ProjectSuggestionPath`, `FolderPredictor.cs:845-858`). That fix and its projection semantics
  ("out-of-root full paths are byte-for-byte unchanged") are not reopened here. `FolderPredictor.cs`
  is not a file this change edits.
- **Pre-existing test flakes #594, #592, #586, #584.** These are not caused by, and are not fixed
  by, this change. They must not be used to block or to justify this change.

### Explicitly excluded systems, integrations, or datasets

- No live Outlook process, no COM automation, and no Microsoft Graph call in any test.
- No filesystem access and no temporary files in any test (CLAUDE.md § UT4: zero approved
  exceptions).
- No new external NuGet dependency.
- No change to `BindRowsAsync`'s selection-clearing semantics
  (`BreadcrumbBridgeRouter.cs:59, :136`) — that is #499's decision surface.
- No new wrapper value type threaded through public APIs (rejected; see
  `### Rejected alternatives`).
- No change to serialized/persisted configuration schema for `EmailFilerConfig`.


## Root Cause Analysis

The root cause is that **the path-representation contract distinguishing a full Outlook hierarchy
path from an archive-relative stem is documented in prose and enforced at no boundary.** Every
defect below is an instance of that single missing invariant, either producing a non-relative value
(producers D1, D2, D3, D8), failing to reject one (consumers D4, D9), mis-handling it once it
arrives (D5), or supplying an ancestor root against which no anchored comparison can succeed
(D6, D7).

### Rejection of the "remove `.` from `IllegalFolderCharacters`" hypothesis

The Copilot hypothesis — remove `.` from `IllegalFolderCharacters` — is **rejected as a root cause**
and must not be presented as the fix for #614. The character class is independently wrong (see D5b
below), but removing `.` would make the observed call *succeed* and file mail to a destination
derived from `C:\Users\<user>\OneDrive - <Org>\\\mailbox@example.com`, converting a hard crash into
silent misfiling. The defective character class is a separate, lesser defect that this change also
corrects, on its own merits and behind the stem guard, not as the remedy for the reported failure
(research §2 "Refutation record").

### Confirmed defect set (supersedes the hypothesis list in `issue.md`)

Verdict vocabulary from the research: **CONFIRMED** (code exhibits the defect), **PARTIAL** (part
confirmed, part corrected). No candidate was refuted outright.

#### D1 — `ToArchiveRelativePath` silent verbatim pass-through — CONFIRMED (primary producer)

`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:504-526`. When the input is not at or under
`_archiveRootPath`, the method returns the input verbatim at `:525` — no rejection, no clamp, no
diagnostic. `SelectHierarchyPath` assigns that value to `SelectedFolderPath` (`:494-502`). For the
store-root segment `\\mailbox@example.com`, the stored value is the full store-root path.

Two adjacent behaviours in the same method matter downstream: an input exactly equal to the archive
root returns `string.Empty` (`:512-515`), which `ActionOkAsync` accepts (D9); and an empty root path
(`root.Length == 0`, `:507-510`) also returns the input verbatim, which is the no-archive-root
binding mode used by the public `BindRowsAsync` overload (`:75-82`).

#### D2 — segments at/above the archive root and cross-store segments are selectable — CONFIRMED

- `BreadcrumbRow.ActivateSegment` imposes no archive-root floor; its only rejections are kind, index
  range, missing key, and re-activation — `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs:151-172`.
- The ancestor chain walks `ParentKey` to the store root and returns root-first —
  `UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotQueries.cs:96-99, 109-135`. The store root is
  itself a snapshot node — `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyReader.cs:97`.
  The store-root segment therefore renders as segment 0 of every chain.
- The provider always queries all stores with a stale snapshot allowed —
  `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs:73-79` — and
  `ResolveLeafKeyAsync` first-matches by path across every store's nodes (`:64-68`), so cross-store
  resolution is possible.
- The router forwards any activated segment's `FullPath` — `BreadcrumbBridgeRouter.cs:426` and
  `:440`.

**Correction to the hypothesis in `issue.md`.** The leaf segment itself is *not* activatable
(`segmentIndex >= _segments.Count - 1` is rejected at `BreadcrumbRow.cs:156`). The exposure is
therefore narrower than "any segment": it is **ancestor segments (including the store root) and
expanded children**.

#### D3 — presented-target leak and `ToHierarchyPath` mirror hazard — CONFIRMED

`BreadcrumbBridgeRouter.cs:140-163`: a presented target not at or under the root is prefixed —
`return root + "\\" + presentedTarget.TrimStart('\\', '/');` (`:162`) — fabricating a nonexistent
hierarchy path such as `\\mailbox@example.com\Archive\other@example.org\Foo`.

**Correction to the hypothesis in `issue.md`.** `ToHierarchyPath`'s present-day consequence is a
silent failed lookup, not a crash: `ResolveLeafKeyAsync` returns null, `FetchChainAsync` returns
null (`:450-459`), and the row falls back to single-segment rendering. The **crash-class** leak for
presented rows flows instead through `SelectRow`, which uses `row.FilingTarget` verbatim
(`:484-487`); row-selecting an out-of-root suggestion leaks the full path into `SelectedFolderPath`
with no segment activation at all. This is a second producer of the D1 class and is the higher
severity half of D3.

#### D4 — `EmailFilerConfig` concatenates with no stem validation — CONFIRMED

`UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs`: `ResolvePaths(Folder)` at
`:187-188`, `ResolvePaths()` at `:203-204`. Neither validates that `DestinationOlStem` is relative;
`$"{OlAncestor}\\{DestinationOlStem}"` accepts a `\\`-rooted stem silently. Same-class neighbours in
the same file: `GetStem` uses an unanchored replace-all `folderPath.Replace(olAncestor, "")`
(`:232-235`), and `IsDeleteRelevant` uses substring `Contains(OlAncestor)` (`:171`).

**What #609 / PR #611 did and did not do.** The final #609 remediation modified only
`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` and its test file; the remediation plan
explicitly prohibited touching `BreadcrumbBridgeRouter.cs`, `EmailFilerConfig.cs`,
`EfcDataModel.cs`, and `EfcFormController.cs`. The fix is `ProjectSuggestionPath`
(`FolderPredictor.cs:845-858`), applied to suggestion text (`AddSuggestions`, `:804-808`) and
suggestion rows/scores (`AddSuggestionRows`, `:832-843`); it strips the prefix **only when** the
suggestion starts with `ArchiveRootPath + "\"`, and its own acceptance criteria require that
out-of-root full paths be byte-for-byte unchanged. #609 therefore covers exactly the
"stem already carries the archive root prefix" case for persisted suggestions and deliberately does
**not** cover "stem is an unrelated absolute path" — a store root, a cross-store path, or any
out-of-root full path, which is #614. The existing
`Issue609_ResolvePaths_PrefixesAtMailboxArchiveRootExactlyOnce` test
(`UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs:255-272`) asserts only single-prefix
construction for a relative stem; it adds no stem validation.

#### D5 — `FolderConverter.ToFsFolderpath` and neighbours — ALL SEVEN CONFIRMED

File: `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs`.

- **D5a — validates the caller-supplied filesystem ancestor.** `:157-159` validates
  `fsPath.Substring(3)`, which contains the whole `fsAncestorEquivalent` minus the drive prefix. Any
  legitimate OneDrive root containing `.` fails **every** filing, regardless of the stem. Only
  derived segments should be validated.
- **D5b — wrong character class.** `:39-42`: `@"[\/:*?""<>|]."` — as a character array this is
  `[ \ / : * ? " < > | ] .`. It wrongly bans `.`, `[`, `]` (all legal in Windows names) and omits the
  `Path.GetInvalidFileNameChars()` control characters and the real per-segment rules (trailing dot
  or space, reserved device names). `SanitizeFilename` (`:133-139`) already uses the correct
  `Path.GetInvalidFileNameChars()` class, so the file disagrees with itself.
- **D5c — `Substring(3)` assumes `X:\`.** `:158`. A UNC or relative `fsAncestorEquivalent` is
  silently mangled, or throws `ArgumentOutOfRangeException` for strings shorter than 3. When the
  replace in D5d fails to match on case, `fsPath` is still the Outlook path and `Substring(3)` chops
  `\\m` off the mailbox name instead of a drive prefix.
- **D5d — unanchored, case-sensitive replace-all.** `:155`
  `olBranchPath.Replace(olAncestorPath, fsAncestorEquivalent)`. A repeated ancestor substring is
  replaced at every occurrence; a case-differing ancestor is not replaced at all. It should be a
  prefix-anchored `OrdinalIgnoreCase` strip.
- **D5e — exception message leaks host identifiers.** `:161-167` embeds `fsPathExDividers` — the
  user-profile path plus mailbox address — into the `ArgumentException` message. This is the runtime
  counterpart of open issue #602.
- **D5f — "Remove illegal characters" always yields the empty string.** `:110-113`:
  `illegalFolderName.Replace(illegalFolderName, "")` replaces the whole string unconditionally.
  Intended semantics were per-character removal. The existing test **codifies the bug**:
  `UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs:329` asserts
  `result["Remove illegal characters"]()...Should().BeEmpty()`. Treating that test as spec would
  freeze the defect; the assertion must be updated with the fix.
- **D5g — `ResolveOlRoot` uses `Contains`.** `:226-242` (`:228`, `:232`). A substring match instead
  of a prefix test: `\\mailbox@example.com\Archive2\...` matches the `ArchiveRootPath` branch, and a
  mid-string occurrence anywhere qualifies. It should be `StartsWith` on a separator-terminated
  prefix.
- **Additional, same file** — `ToFsFolderpath` declares `bool ask = true` (`:145`) and never reads
  it. The signature implies an interactive fallback that does not exist. Folded into the D5 cleanup.
  This also means the named regression test cannot be perturbed by a dialog.

#### D6 — `AppOlObjects.ArchiveRootPath` unverified and default-store-scoped — CONFIRMED

`TaskMaster/AppGlobals/AppOlObjects.cs`: `Root` is `App.Session.DefaultStore.GetRootFolder()`
(`:201-210`, assignment at `:207`); `ArchiveRootPath = Path.Combine(Root.FolderPath, "Archive")`
(`:237-248`, `:244`) — a string combine, never checked for existence. The actual folder is resolved
separately and later by `LoadArchiveRoot` (`:253-256`). It is never scoped to the store owning the
current folder. With a folder from another store: `EfcDataModel.MoveToFolderAsync(MAPIFolder, ...)`'s
`Replace(olAncestor, "")` no-ops (the full path becomes the stem — the same crash class via the
create-folder path, D8), `EmailFilerConfig.IsDeleteRelevant`'s `Contains` fails, and
`FolderConverter.ResolveOlRoot` throws "not a branch of any known root". This is the
configuration-level defect that makes D1's verbatim pass-through inevitable for cross-store and
store-root inputs.

#### D7 — `AppFileSystemFolderPaths` fallback and matching — PARTIAL

`TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs`.

- **Fallback chain — CONFIRMED and on-chain.** `LoadFolders` resolves the `OneDrive` key from the
  `OneDriveCommercial` → `OneDrive` → `OneDrivePersonal` environment variables (`:206-226`), then
  falls back to `AppData` (`:229-232`) and finally to `SpecialFolders.First().Value`, an arbitrary
  dictionary entry (`:235`). A wrong `FsAncestorEquivalent` is accepted silently; filing then writes
  `.msg` artifacts under `AppData` or an arbitrary folder instead of failing fast. It is on-chain
  because `EfcDataModel.MoveToFolderAsync` consumes `SpecialFolders["OneDrive"]`
  (`QuickFiler/Controllers/EfcDataModel.cs:276-289`).
- **`MatchBestSpecialFolder` substring matching — CONFIRMED but OFF-CHAIN.** The pure helper
  (`:77-91`, `Contains` at `:87`) matches by substring rather than path prefix, and existing tests
  codify substring semantics
  (`TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs:96-115`).
  A repository search found **no production caller** — only the interface member, the instance
  delegator, and test doubles. It is therefore **not** part of the #614 defect set and is out of
  scope here; it is being promoted separately as latent hardening.

#### D8 — `EfcDataModel.MoveToFolderAsync(MAPIFolder, olAncestor, ...)` — CONFIRMED

`QuickFiler/Controllers/EfcDataModel.cs:335-360`:

```csharp
var folderpath = folder.FolderPath.Replace(olAncestor, "");                 // :344 unanchored replace-all
if (folderpath.StartsWith(@"\")) { folderpath = folderpath.Substring(1); }  // :345-348 strips ONE '\'
```

When `folder` is not under `olAncestor` (cross-store, store root, or a D6 mismatch), the replace
no-ops and `Substring(1)` leaves `\mailbox@example.com\...` — a separator-leading absolute stem —
which flows into the string overload as `DestinationOlStem`. Live callers:
`EfcFormController.ButtonCreate_Click` (`:500-507`) and `CreateFolderAsync` (`:778-787`), both
passing `_globals.Ol.ArchiveRootPath`. Same defect class as D1; a second entry point into D4.

#### D9 — `ActionOkAsync` guard asymmetry — CONFIRMED (found during the research; on-chain)

`QuickFiler/Controllers/EfcFormController.cs:706` accepts any non-null string that does not start
with `"===="`, including `string.Empty` — which is exactly what `ToArchiveRelativePath` returns for
a selection of the archive root itself (`BreadcrumbBridgeRouter.cs:512-515`). An empty stem produces
`DestinationOlPath = OlAncestor + "\"` (a trailing separator) and a failed folder resolution at
runtime. The stricter `IsValidSelection` (`EfcFormController.cs:1038-1050`, which rejects
`Length < 3`) is used by the create-folder paths but **not** by OK. The two guards should share one
predicate at the same boundary as the D4 stem validation.

### Verified consumer chain (for traceability)

`EfcFormController.BindRowsAsync` with `_globals.Ol.ArchiveRootPath`
(`EfcFormController.cs:891`) → `BreadcrumbBridgeRouter.FetchChainAsync` (`:443-474`) →
`ActivateSegment`/`ActivateChild` (`:410-427`, `:429-441`) → `SelectHierarchyPath` (`:494-502`) →
`EfcFormController.SelectedFolder` (`:287-293`) → `ActionOkAsync` (`:700-710`) →
`EfcHomeController.ExecuteMovesCoreAsync` (`QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:66`,
forwarded at `:75-81` / `:94-101`) → `EfcDataModel.MoveToFolderAsync(string, ...)`
(`EfcDataModel.cs:258-296`, assignments at `:286`, `:288`, `:289`; sort at `:292-293`) →
`EmailFiler.SortAsync` → `ResolvePaths(Folder)`
(`UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:133`, `:377-378`) →
`EmailFilerConfig.ResolvePaths(Folder)` (`EmailFilerConfig.cs:183-197`) →
`FolderConverter.ToFsFolderpath` throw site (`FolderConverter.cs:155-167`).


## Proposed Fix

### Design summary (what changes where):

Introduce one small, host-neutral, pure static class in `UtilitiesCS` —
`UtilitiesCS.OutlookObjects.Folder.ArchiveStemContract` — that expresses the path-representation
contract as executable code, and enforce it at three points:

1. **Producer (router).** `BreadcrumbBridgeRouter.SelectHierarchyPath` uses
   `ArchiveStemContract.TryMakeArchiveRelative`. On `false` it logs a diagnostic and returns
   **without changing the selection**. Rejection is chosen over clamping-to-root because clamping
   would silently select the archive root, which is itself not a valid filing target (D9). This
   closes D1 and D2, and the same call closes the `SelectRow` / `FilingTarget` half of D3.
2. **Consumer (filing boundary).** Both `EmailFilerConfig.ResolvePaths` overloads call
   `ArchiveStemContract.RequireArchiveRelativeStem(DestinationOlStem, nameof(DestinationOlStem))`
   before concatenation. This is the invariant: it protects every producer, including #499's stale
   value and D8's create-folder path, and it carries the named regression test (D4).
3. **Secondary producer.** `EfcDataModel.MoveToFolderAsync(MAPIFolder, ...)` derives its stem via an
   extracted pure helper backed by `TryMakeArchiveRelative` instead of the inline
   `Replace` + `Substring(1)` (D8).

`FolderConverter` (D5a–e, D5g, and the dead `ask` parameter) is corrected as independent hardening
in the same change: validate only derived per-segment names against the real Windows rules, anchor
the prefix strip with `OrdinalIgnoreCase`, drop the `Substring(3)` assumption, redact the exception
message, and replace `ResolveOlRoot`'s `Contains` with a separator-terminated prefix test. D5f's
`BuildAlternativesDictionary` option is corrected to remove only illegal characters, and the test
that codifies the defect (`FolderConverterTests.cs:329`) is updated.

`AppOlObjects.ArchiveRootPath` (D6) and the `OneDrive` fallback (D7 fallback half) are changed to
fail explicitly, with a deterministic diagnostic, rather than to return an unverified or arbitrary
value silently.

`EfcFormController.ActionOkAsync` and `IsValidSelection` are unified on a single predicate at the
OK boundary (D9).

### Boundaries and invariants to preserve:

- **The stem contract is the invariant.** After this change, no value reaching
  `EmailFilerConfig.DestinationOlStem` may be a full Outlook path. The router fix is user
  experience; the boundary guard is the invariant. Neither substitutes for the other.
- **Pure logic stays separate from I/O and COM.** `ArchiveStemContract` performs no filesystem
  access, no COM call, no logging, and no environment read. It is string-in / string-out and is
  fully unit-testable without Outlook (CLAUDE.md § General Code Change Policy 1.4, § 6.2).
- **#499 must not be absorbed or regressed.** Two concrete constraints, taken from research §6:
  (a) the #614 router change confines its writes to the selection actions
  (`SelectHierarchyPath`, `SelectRow`, `ToArchiveRelativePath`) and must **not** change
  `BindRowsAsync`'s clearing semantics (`BreadcrumbBridgeRouter.cs:59, :136`) — clearing or
  restoring on rebind is #499's decision, deferred there because it is an observable contract
  change; (b) on rejecting an out-of-root activation, prefer "leave `SelectedFolderPath` unchanged
  and log or post a diagnostic" over "set it to null", because nulling on rejection would partially
  implement #499's clear-on-invalidation semantics as a side effect and pre-empt its open design
  question (whether to raise `SelectedFolderPathChanged(null)`). The #614 filing-boundary guard
  independently protects the #499 scenario: even a stale full-path value can no longer reach
  `ToFsFolderpath`. A stale *relative* stem — the common #499 case — is unaffected either way.
- **#609 semantics preserved.** `FolderPredictor.ProjectSuggestionPath` continues to pass
  out-of-root full paths through byte-for-byte. `FolderPredictor.cs` is not edited by this change.
- **Public API stability.** No signature on `EmailFilerConfig`, `EfcDataModel`, `EfcHomeController`,
  or `IOlObjects` changes shape, apart from the removal of the never-read `ask` parameter on
  `ToFsFolderpath` (see `#### Backward-compatibility expectations`).
- **Existing must-stay-green behaviour.** Direct row selection, ancestor activation, child
  activation, banner and trash pseudo-rows, and case-insensitive matching of mailbox roots
  containing `@` retain their current behaviour
  (`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`).
- **Leaf segments remain non-activatable.** The existing floor at `BreadcrumbRow.cs:156` is not
  relaxed.

### Dependencies or blocked work:

- **No blocking dependency.** All three enforcement points and all the seams they require exist in
  the current worktree (research §4).
- **Coordinated, not blocking:** open issue #499 (shared field, constraints above); open issue #602
  (host-identifier redaction — D5e is its runtime counterpart, and this change makes the #614 path
  compliant without closing #602 generally); issue #615 (analyzer version skew, already promoted).
- **Closed and not reopened:** issue #609 / PR #611.
- **Test-infrastructure dependency:** `QuickFiler.Test` reaches the internal
  `BindRowsAsync(rows, scores, archiveRootPath, ct)` overload through
  `[InternalsVisibleTo("QuickFiler.Test")]` (`QuickFiler/Properties/AssemblyInfo.cs:5`). If
  `ArchiveStemContract` or the extracted `EfcDataModel` helper is declared `internal`, the
  corresponding `InternalsVisibleTo` must already exist or be added; `ArchiveStemContract` is
  specified below as `public` precisely to avoid a cross-assembly visibility problem, because
  `QuickFiler` consumes it from `UtilitiesCS`.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

**New:**
- `UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs` — the pure contract validator. Must be
  added to `UtilitiesCS/UtilitiesCS.csproj` as an explicit `<Compile Include>` item: this project
  uses explicit includes, evidenced by `UtilitiesCS/UtilitiesCS.csproj:1054`, which includes only
  the `OutlookObjects` copy of `FolderConverter.cs`. Omitting the include produces a file that
  compiles nowhere.

**Modified (production):**
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs` — D4 (`:187-188`,
  `:203-204`) and the same-class neighbours `GetStem` (`:232-235`), `IsDeleteRelevant` (`:171`).
- `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` — D5a (`:157-159`), D5b (`:39-42`),
  D5c (`:158`), D5d (`:155`), D5e (`:161-167`), D5f (`:110-113`), D5g (`:226-242`), dead `ask`
  parameter (`:145`).
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` — D1 (`:494-502`, `:504-526`), D3
  (`:484-487`, `:140-163`).
- `QuickFiler/Controllers/EfcDataModel.cs` — D8 (`:335-360`, specifically `:344-348`).
- `QuickFiler/Controllers/EfcFormController.cs` — D9 (`:706`, `:1038-1050`).
- `TaskMaster/AppGlobals/AppOlObjects.cs` — D6 (`:237-248`, `:253-256`).
- `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` — D7 fallback half (`:206-235`) only. The
  `MatchBestSpecialFolder` helper at `:77-91` is **not** edited.

**Modified (test):**
- `UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs` — the named regression test.
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs` — new D5 cases, plus the
  mandatory update of the assertion at `:329` that codifies D5f.
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` — producer-side companion tests.
- `QuickFiler.Test/Controllers/EfcDataModelTests.cs` — D8 pure-helper cases.
- New `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemContractTests.cs`.
- `QuickFiler.Test` / `TaskMaster.Test` additions for D9 and D6/D7 as the seams allow.

**Not edited (asserted by AC25):** `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`,
`UtilitiesCS/EmailIntelligence/FolderConverter.cs` (the uncompiled duplicate),
`TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs:77-91`,
`BreadcrumbBridgeRouter.BindRowsAsync` clearing semantics.

#### Functions/classes/CLI commands impacted:

- `ArchiveStemContract.IsFullOutlookPath`, `.RequireArchiveRelativeStem`, `.TryMakeArchiveRelative`
  (new).
- `BreadcrumbBridgeRouter.SelectHierarchyPath`, `.ToArchiveRelativePath`, `.SelectRow`,
  `.ToHierarchyPath`.
- `EmailFilerConfig.ResolvePaths()`, `.ResolvePaths(Folder)`, `.GetStem`, `.IsDeleteRelevant`.
- `FolderConverter.ToFsFolderpath`, `.IllegalFolderCharacters`, `.BuildAlternativesDictionary`,
  `.ResolveOlRoot`.
- `EfcDataModel.MoveToFolderAsync(MAPIFolder, string, ...)` and a new extracted pure helper.
- `EfcFormController.ActionOkAsync`, `.IsValidSelection`.
- `AppOlObjects.ArchiveRootPath`.
- `AppFileSystemFolderPaths.LoadFolders`.
- No CLI command exists or is added; the surface is a VSTO add-in.

#### Data flow and validation changes:

Before: `segment.FullPath` → `ToArchiveRelativePath` (verbatim on failure) → `SelectedFolderPath` →
`SelectedFolder` → `ActionOkAsync` (null / `"===="` guard only) → `DestinationOlStem` →
`OlAncestor + "\" + stem` → `ToFsFolderpath` → validated whole path → `ArgumentException` embedding
host identifiers.

After: `segment.FullPath` → `TryMakeArchiveRelative(fullPath, archiveRoot, out stem)`. On `false`,
the selection is unchanged and a diagnostic is emitted; nothing propagates. On `true`, the
archive-relative `stem` is stored. At the filing boundary,
`RequireArchiveRelativeStem(DestinationOlStem, nameof(DestinationOlStem))` runs **before**
concatenation and throws a redacted, deterministic `ArgumentException` naming the parameter and the
violated rule if the invariant is broken. `ToFsFolderpath` then performs a prefix-anchored,
`OrdinalIgnoreCase` strip and validates **only the derived segments**, never
`fsAncestorEquivalent`.

Validation additions, by boundary:
- Router selection: reject non-archive-relative, cross-store, and at-or-above-root activations.
- OK button: one shared predicate rejecting null, empty, `"===="`-prefixed, and non-relative
  selections (D9).
- Filing boundary: `RequireArchiveRelativeStem` on both overloads (D4).
- `EfcDataModel` stem derivation: `TryMakeArchiveRelative`, explicit failure instead of a mangled
  stem (D8).
- Converter: per-segment Windows name validation on derived segments only (D5a, D5b).
- Configuration resolution: explicit failure for an unresolvable archive root (D6) or `OneDrive`
  special folder (D7 fallback half).

#### Error handling and logging updates:

- **Fail fast and explicitly** (CLAUDE.md § 3.1, § C#4.1). `RequireArchiveRelativeStem` throws
  `ArgumentException` — it does not clamp, coerce, or return a default.
- **Redaction is mandatory.** No new or modified exception message, log line, or diagnostic may
  embed a mailbox address, user-profile path, host name, or organization name. Messages name the
  **parameter** and the **violated rule** (for example: the parameter must be an archive-relative
  stem and must not begin with a path separator). This directly corrects D5e and is the #614-scoped
  contribution to open issue #602.
- **Router rejection is a diagnostic, not an exception.** A user activating an out-of-root segment
  is a normal interaction, not a programming error; the router logs through the project logging
  pattern used by `BreadcrumbBridgeRouter` today and leaves the selection unchanged. It must not
  throw into a UI event handler.
- **No ad-hoc console output** in production code (CLAUDE.md § C#4.2).
- **No broad `catch (Exception)`** is added. Existing boundary catches are not widened.

#### Rollback/feature-flag considerations (if applicable):

- **No feature flag.** The change restores a documented invariant; a flag would preserve a code path
  that files mail to a wrong destination, and would double the test matrix for no operational
  benefit.
- **Rollback unit** is the single PR. Revert is clean because the change adds one new file, edits
  seven production files, and introduces no schema, migration, or persisted-state change.
- **Partial rollback** is possible along the enforcement-point boundary: the `FolderConverter` (D5)
  hardening and the `AppGlobals` (D6, D7) hardening can each be reverted independently of the
  `ArchiveStemContract` guard, because they share no state. The D4 boundary guard is the load-bearing
  change and must not be reverted while the router change is retained (that combination would remove
  the invariant while leaving only the user-experience half).

### Technical specifications (interfaces/contracts):

**New type.** `public static class ArchiveStemContract`, namespace
`UtilitiesCS.OutlookObjects.Folder`, file
`UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs`. It must be `public` because `QuickFiler`
(`BreadcrumbBridgeRouter`, `EfcDataModel`, `EfcFormController`) consumes it across the assembly
boundary. It performs no I/O, holds no state, takes no dependency on
`Microsoft.Office.Interop.Outlook`, and stays well under the 500-line file limit.

```csharp
// Target framework is .NET Framework 4.8.1: no `init` accessors, no `record`,
// no `record struct` (no IsExternalInit polyfill in this solution).
public static class ArchiveStemContract
{
    /// <summary>True when <paramref name="value"/> is a full Outlook hierarchy path
    /// rather than an archive-relative stem.</summary>
    public static bool IsFullOutlookPath(string value);

    /// <summary>Throws <see cref="System.ArgumentException"/> when
    /// <paramref name="value"/> is not an archive-relative stem. The message names
    /// <paramref name="paramName"/> and the violated rule and never embeds
    /// <paramref name="value"/>.</summary>
    public static void RequireArchiveRelativeStem(string value, string paramName);

    /// <summary>Prefix-anchored, OrdinalIgnoreCase, separator-aware strip of
    /// <paramref name="archiveRoot"/> from <paramref name="fullPath"/>. Returns false —
    /// it does not pass the input through — when <paramref name="fullPath"/> is not at or
    /// under <paramref name="archiveRoot"/>.</summary>
    public static bool TryMakeArchiveRelative(string fullPath, string archiveRoot, out string stem);
}
```

**Definition of "archive-relative stem".** A stem is the portion of an Outlook folder path *below*
the archive root, expressed with no leading separator — for example `Clients\North`. It is the value
`EmailFilerConfig.DestinationOlStem` is contractually required to hold, and it is what
`$"{OlAncestor}\\{DestinationOlStem}"` composes correctly.

**What is rejected by `RequireArchiveRelativeStem`:**
- `null`.
- The empty string and whitespace-only values. The empty case is not hypothetical: it is exactly
  what `ToArchiveRelativePath` returns for a selection of the archive root itself
  (`BreadcrumbBridgeRouter.cs:512-515`), and it currently passes the OK guard (D9).
- Any value for which `IsFullOutlookPath` returns true.

**Definition of `IsFullOutlookPath`** (as established by research §7): true when the value is
`\\`-rooted or separator-leading. After the D8 fix stops producing single-`\` remnants,
`value.StartsWith("\\")` covers both forms; the planner should nonetheless treat a single leading
`\` or `/` as non-relative, because D8's current output is a single-separator-leading path and
defence in depth at the consumer costs nothing.

**Open question carried forward, not resolved by assertion.** The research does not establish
whether a drive-rooted value (`C:\...`) or a value containing `:` can reach this boundary, and no
evidence was gathered on that case. The planner must decide explicitly whether `IsFullOutlookPath`
also rejects drive-rooted input, and must record the decision and its rationale. This spec does not
assert an answer.

**Separator awareness in `TryMakeArchiveRelative`.** The comparison must be prefix-anchored and
`OrdinalIgnoreCase`, and the match must terminate on a separator boundary, so that
`\\mailbox@example.com\Archive2\Foo` is **not** treated as being under
`\\mailbox@example.com\Archive`. This is the same class of defect as D5g and D4's `IsDeleteRelevant`
and must not be reintroduced in the new helper. On a successful match the returned `stem` carries no
leading separator. A `fullPath` exactly equal to `archiveRoot` yields `stem == string.Empty`, which
`RequireArchiveRelativeStem` then rejects at the filing boundary and which the router treats as a
non-selection.

**Reuse obligation.** `ToArchiveRelativePath`, the extracted `EfcDataModel` stem helper, and
`FolderConverter`'s prefix strip must all be backed by the single `TryMakeArchiveRelative`
implementation. Three independent re-implementations of prefix-anchored stripping is what produced
this defect set; the fix must not add a fourth (CLAUDE.md § 1.2 Reusability).

#### Inputs/outputs and formats:

- All inputs and outputs are `System.String` Outlook hierarchy paths and stems. No new serialized
  format, DTO, or wire format is introduced.
- Outlook hierarchy path form: `\\<store display name>\<folder>\<folder>`; the store display name
  in the reported environment is the account email address and legitimately contains `@` and `.`.
- Filesystem ancestor form: a rooted Windows path that legitimately contains `.`, spaces, and `-`
  (for example `C:\Users\<user>\OneDrive - <Org>`). After D5a, the converter must never validate
  this caller-supplied value.
- Stem form: separator-delimited, no leading separator, non-empty.

#### Required configuration keys and defaults:

- **No new configuration key, application setting, or environment variable is introduced.**
- Existing keys whose *resolution behaviour* changes: the `OneDrive` entry of
  `AppFileSystemFolderPaths.SpecialFolders` (D7 fallback half) and `AppOlObjects.ArchiveRootPath`
  (D6). Both change from silent fallback to explicit failure with a diagnostic. Their key names,
  types, and successful-resolution values are unchanged.
- Environment variables read today and still read: `OneDriveCommercial`, `OneDrive`,
  `OneDrivePersonal` (`AppFileSystemFolderPaths.cs:206-226`). No new variable is read.

#### Backward-compatibility expectations:

- **Persisted data:** no change. No stored `EmailFilerConfig` value, settings file, or serialized
  suggestion changes shape or meaning.
- **Behavioural changes visible to the user** (research §7):
  - Activating a store-root, cross-store, or at-or-above-archive-root segment no longer changes the
    filing selection; a diagnostic is emitted instead of a silent pass-through.
  - Pressing OK with an empty or non-relative selection fails fast with a clear, redacted message
    instead of the leaked `ArgumentException`.
  - Folder names containing `.`, `[`, or `]` become filable once D5b is corrected; today they
    crash. This is a **widening** of accepted input, so it cannot break a previously working case.
  - The "Remove illegal characters" dialog option begins performing per-character removal instead of
    returning the empty string (D5f).
- **API surface:** the only source-breaking change is the removal of the never-read `bool ask = true`
  parameter from `FolderConverter.ToFsFolderpath` (`FolderConverter.cs:145`). It is an optional
  parameter with a default, so in-repo call sites that omit it are unaffected; any call site that
  passes it explicitly must be updated in the same change. If the planner judges that removal
  exceeds the fix budget, marking it `[Obsolete]` and ignoring it is an acceptable alternative that
  preserves compatibility, provided the dead-parameter status is documented in code.
- **Test-visible change:** `FolderConverterTests.cs:329` currently asserts the D5f defect and **must**
  be updated. This is a deliberate, documented spec correction, not a regression.

#### Performance constraints (latency/throughput/memory):

- All added work is `O(n)` in path length over strings of a few hundred characters, performed once
  per selection action and once per filed item. No measurable latency change is expected, and no
  performance benchmark gate is imposed.
- `ArchiveStemContract` must not allocate per-call regular expressions. Any regular expression used
  by the per-segment validation (D5b) must be a `static readonly` compiled instance or be replaced
  by a character-set test, so the converter does not regress the per-item filing path.
- No new I/O, no new COM round-trip, and no additional Outlook folder-tree query is introduced on
  the filing path. In particular, the D6 archive-root validation must not add a per-item COM call;
  validate at resolution time, not per filed message.
- Memory: no new caches, no new long-lived collections.

### Rejected alternatives

- **Delete `.` from `IllegalFolderCharacters`.** Rejected: symptom level. It converts the crash into
  silent misfiling to a path derived from the mailbox store root (research §2 "Refutation record").
- **A wrapper value type (`ArchiveRelativeStem` struct) threaded through the public APIs.** Rejected
  for #614: it would ripple through `EmailFilerConfig`, `EfcDataModel`, `EfcHomeController`,
  `FolderPredictor`, and serialized configuration, breaking public surface for the same protection
  that two validation call sites provide. It is contrary to Simplicity-first (CLAUDE.md § 1.1), and
  .NET Framework 4.8.1 lacks `init` accessors and records, which makes the ergonomics worse.
  Revisit only if a third path representation appears.
- **Fix only the router (producer).** Rejected: it leaves D8's producer, #499's stale value, and any
  persisted out-of-root suggestion able to reach the unguarded concatenation. The boundary guard is
  the invariant; the router fix is the user experience.


## Assumptions, Constraints, Dependencies

### Assumptions (environment, data, access)

- The reported environment has a single relevant mailbox whose store display name is the account
  email address, an archive root of `\\mailbox@example.com\Archive`, and a OneDrive commercial root
  of `C:\Users\<user>\OneDrive - <Org>`.
- The user action that produced the report was activation of the store-root ancestor segment. This
  is an inference from the producer analysis plus the reported steps, and it is corroborated by the
  algebraic reconstruction; it was not directly observed. The precision caveat in
  `## Repro & Evidence` applies.
- Outlook interop interfaces (`Folder`, `MAPIFolder`, `Store`) are mockable with Moq in this
  repository, as demonstrated by the existing tests cited in `## Test Strategy`.
- Line numbers cited throughout are those of this worktree at the time of the research
  (2026-08-26T10-30, post-PR-#611). If the planner or executor finds a cited line no longer matches,
  the correct response is to re-locate the construct by name and record the drift — not to assume
  the defect is absent.

### Constraints (budget, performance, compatibility)

- **.NET Framework 4.8.1.** `init` accessors, `record`, and `record struct` are **not available**;
  there is no `IsExternalInit` polyfill in this solution. Use plain classes, plain `readonly struct`
  where a value type is warranted, and ordinary properties or constructor assignment.
- **Test framework is fixed:** MSTest (`Microsoft.VisualStudio.TestTools.UnitTesting`), Moq for
  mocking, FluentAssertions for assertions. xUnit and NUnit must not be introduced
  (CLAUDE.md § CUT1, § CUT2).
- **No temporary files in tests.** CLAUDE.md § UT4 states the prohibition with **zero currently
  approved exceptions**. No test in this change may create, read, or delete a file on disk.
- **Determinism.** No `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `Random.Shared`, or any
  wall-clock wait in test code (`.claude/rules/general-unit-test.md`, Determinism Infrastructure).
- **No file over 500 lines** for production code, test code, or reusable scripts (CLAUDE.md § 4.1).
  `FolderConverter.cs` and `EfcFormController.cs` are being edited; the executor must confirm the
  post-change line count of every edited file.
- **Fail fast and explicitly**; no silent fallback, no broad catch without re-raise
  (CLAUDE.md § 3.1).
- **Pure logic separate from I/O and from COM/Outlook interop** (CLAUDE.md § 1.4, § 6.2).
- **No new external dependency** (CLAUDE.md § 6.3, § C#7).
- **Coverage.** CLAUDE.md § UT2 governs: repository-wide line coverage `>= 80%` measured over the
  testable denominator (COM/VSTO/WinForms/Outlook-Interop exemptions apply), and `>= 90%` for new
  modules, classes, and methods. Note a pre-existing divergence: `.claude/rules/general-unit-test.md`
  states `>= 85%` line and `>= 75%` branch. Per the Policy Compliance Order in CLAUDE.md, CLAUDE.md
  is authority 1 and governs this change; reconciling the two documents is out of scope for #614.
- No baseline coverage evidence exists for this feature yet
  (`<FEATURE>/evidence/baseline/` is empty at the time of writing), so the repository-wide figure is
  specified below as a record-and-report obligation rather than as a blocking numeric gate.

### External dependencies (services, libraries, releases)

- None added. The change uses only the BCL (`System.String`, `System.IO.Path`,
  `System.Text.RegularExpressions` if the planner chooses that form) and existing project
  references.
- Issue #615 (analyzer version skew) is tracked separately and may affect analyzer output during the
  toolchain pass; it is not a functional dependency of this fix.


## Data / API / Config Impact

### User-facing or API changes

- Store-root, cross-store, and at-or-above-archive-root breadcrumb segment activation no longer
  changes the filing selection; the user sees a diagnostic and the previous selection is retained.
- OK with an empty or non-relative selection fails fast with a clear, redacted message.
- Folder names containing `.`, `[`, or `]` become filable (a widening of accepted input).
- The "Remove illegal characters" alternative-folder option performs per-character removal instead
  of returning the empty string.
- `FolderConverter.ToFsFolderpath`'s optional `bool ask` parameter is removed (or marked
  `[Obsolete]`; see `#### Backward-compatibility expectations`). No other public signature changes.

### Data or migration considerations

- **None.** No persisted schema, settings file, serialized suggestion, or stored configuration value
  changes shape or meaning. No migration, no backfill, no data repair step.
- Persisted suggestions that contain out-of-root full paths continue to exist and continue to be
  passed through unchanged by `FolderPredictor.ProjectSuggestionPath` (#609 semantics). They are now
  rejected at the filing boundary instead of crashing the converter. That is the intended
  behavioural change and requires no data cleanup.

### Logging/telemetry updates (if any)

- New: a router-side diagnostic when a segment activation or row selection is rejected as
  non-archive-relative. It must identify the rejection reason and must not include the rejected path
  value or any host identifier.
- Changed: `FolderConverter.ToFsFolderpath`'s `ArgumentException` message no longer embeds
  `fsPathExDividers` (D5e). New: a redacted, deterministic contract message from
  `RequireArchiveRelativeStem` naming the parameter and the violated rule.
- Changed: explicit diagnostics on unresolvable archive root (D6) and unresolvable `OneDrive`
  special folder (D7 fallback half), replacing silent fallback.
- No new telemetry pipeline, event name, or metric is introduced.

### Compatibility notes (CLI flags, config schemas, versioning)

- No CLI exists for this surface; no flags change.
- No configuration schema or version number changes.
- No `InternalsVisibleTo` removal. If a new one is required for a helper declared `internal`, it is
  additive only.


## Test Strategy

All tests use MSTest, Moq, and FluentAssertions, in Arrange–Act–Assert form, with a descriptive name
or comment stating the scenario and expected outcome. No test may touch Outlook, the network, the
filesystem, or the wall clock, and no test may create a temporary file.

### The named primary regression test

- **Project / file:** `UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs`
- **Class:** `EmailFilerConfig_Tests`
- **Test name:**
  `Issue614_ResolvePaths_WithStoreRootStem_RejectsNonRelativeStemWithoutLeakingIdentifiers`
- **Seam:** the pure configuration seam already used by the #609 test
  (`EmailFilerConfig_Tests.cs:255-272`) — `Globals = null`, parameterless `ResolvePaths()`. No
  Outlook, no filesystem, no globals.
- **Exact inputs:**
  - `OlAncestor = @"\\mailbox@example.com\Archive"`
  - `DestinationOlStem = @"\\mailbox@example.com"`
  - `FsAncestorEquivalent = @"C:\Users\<user>\OneDrive - <Org>"`

  The filesystem ancestor must be a realistic value containing no character from the current illegal
  set other than the `.` contributed by the mailbox domain, so that the `.` is the sole trigger and
  the test reproduces the field report. Placeholder tokens are used here per the redaction
  constraint; the executor must substitute non-identifying literal values of the same shape (a
  fabricated user name and organization name), never a real one.
- **Pre-fix behaviour (established algebraically in research §1.3):** `ResolvePaths()` reaches
  `FolderConverter.ToFsFolderpath`, which throws `ArgumentException` whose message contains the
  concatenated, backslash-stripped filesystem ancestor followed by the mailbox address. The dead
  `ask` parameter guarantees no dialog path can fire, so the failure is deterministic.
- **Assertion shape (must fail pre-fix, pass post-fix):** two assertions, both of which fail on the
  pre-fix tree:
  1. The act throws the **contract** exception — a deterministic type and message naming
     `DestinationOlStem` as not archive-relative. Pre-fix the thrown exception is the
     `FolderConverter` one, so this fails.
  2. The exception message contains **neither** the mailbox address **nor** the filesystem ancestor.
     Pre-fix the message contains both, so this fails.
- **Non-vacuity requirement:** the executor must run this test against the pre-fix tree and capture
  the failure output as evidence before applying any production change (AC17).

### Producer-side companion test

- **Project / file:** `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs`, following the
  existing mock-wiring pattern (`:33-123`: `Mock<IFolderHierarchyProvider>`,
  `Mock<IBreadcrumbWebHost>`, `SetupProviderChain`, the `Inbound` helper) and the archive-root
  binding pattern of `BreadcrumbBridgeRouterIssue439Tests.cs:20-116`.
- **Scenario:** bind with `archiveRootPath = @"\\mailbox@example.com\Archive"` and a provider chain
  whose segment 0 is `\\mailbox@example.com`; send `segmentActivate` for index 0; assert
  `SelectedFolderPath` is **not** the store-root full path. Pre-fix it is that path, verbatim (D1).
- The internal `BindRowsAsync(rows, scores, archiveRootPath, ct)` overload is reachable via
  `[InternalsVisibleTo("QuickFiler.Test")]` (`QuickFiler/Properties/AssemblyInfo.cs:5`).

### Unit tests for the fixed behaviour and boundaries

| Defect | Project / class | Scenarios |
| --- | --- | --- |
| Contract type | `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemContractTests.cs` (new) | `IsFullOutlookPath` for `\\`-rooted, single-separator-leading, and relative values; `RequireArchiveRelativeStem` for null, empty, whitespace, `\\`-rooted, and valid relative; `TryMakeArchiveRelative` for under-root, exact-root, out-of-root, cross-store, case-differing root, `Archive2`-style separator-boundary near-miss, and repeated-ancestor-substring inputs |
| D1, D2 | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` | Store-root segment activation and cross-store segment activation leave the selection unchanged; child activation under the root still works; a valid ancestor activation under the root still works |
| D3 | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` | Row selection of an out-of-root suggestion (the `FilingTarget` pass-through at `:484-487`) does not leak a full path into `SelectedFolderPath` |
| D4 | `UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs` | The named regression test above (pure seam); the same rejection through the `ResolvePaths(Folder)` overload using `Mock<IApplicationGlobals>` / `Mock<IOlObjects>` / `Mock<Folder>` (pattern at `:97-109`); single-prefix construction for a valid relative stem remains correct |
| D5a, D5b | `UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs` | Dotted and bracketed filesystem roots now succeed; per-segment illegal characters, trailing dot, trailing space, and reserved device names in **derived** segments fail |
| D5c | same | A UNC filesystem ancestor and a short (`< 3` character) ancestor no longer mangle or throw `ArgumentOutOfRangeException` |
| D5d | same | Repeated ancestor substring is stripped once, at the prefix only; a case-differing ancestor still matches |
| D5e | same | The thrown message contains no host identifier — asserted by checking the message does not contain the mailbox address or the filesystem ancestor |
| D5f | same, **updating the assertion at `:329`** | "Remove illegal characters" removes only the illegal characters and returns a non-empty result for a name that contains at least one legal character |
| D5g | same | `ResolveOlRoot` selects by separator-terminated prefix: an `Archive2`-style sibling does not match the `Archive` branch |
| D6 | `TaskMaster.Test/AppGlobals/AppOlObjectsTests.cs` and consumer-side `Mock<IOlObjects>` tests | An unresolvable or cross-store archive root produces an explicit failure with a diagnostic rather than a silent no-op. The property itself is COM-bound (`DefaultStore`), so validation is tested at the consumers or in a pure helper, never against live Outlook |
| D7 (fallback half) | `TaskMaster.Test/AppGlobals/` | With no `OneDrive*` environment variable resolvable, resolution fails explicitly instead of returning `AppData` or `SpecialFolders.First().Value`. If `LoadFolders` is hardened, introduce an injectable `Func<string, string> getEnvironmentVariable` delegate seam (the repository's DI-seam preference) rather than mutating process environment state in a test |
| D8 | `QuickFiler.Test/Controllers/EfcDataModelTests.cs` | The extracted pure helper (recommended shape: `internal static string ToArchiveRelativeStem(string folderPath, string olAncestor)`) for under-root, store-root, cross-store, and case-differing-ancestor inputs. `Mock<MAPIFolder>` / `Mock<Folder>` supply `FolderPath` |
| D9 | `QuickFiler.Test/Controllers/` | The OK-path predicate rejects `null`, `string.Empty`, a `"===="`-prefixed sentinel, and a non-relative selection, and accepts a valid relative stem — matching `IsValidSelection`'s strictness |

### Edge cases and negative scenarios

Explicitly required: empty stem (D9 / archive-root-exact selection); `\\`-rooted stem (the reported
case); single-separator-leading stem (D8's current output); cross-store path; case-differing archive
root; `Archive2`-style separator-boundary near-miss; repeated ancestor substring; UNC and
short filesystem ancestor; filesystem ancestor containing `.`, `[`, `]`, spaces, and `-`; derived
segment with a trailing dot, a trailing space, or a reserved device name; mailbox root containing
`@`; and the empty-archive-root binding mode (`BreadcrumbBridgeRouter.cs:507-510`).

### Error handling and logging verification

- Every new or changed exception message is asserted to name the offending parameter and the
  violated rule.
- Every new or changed exception message is asserted **not** to contain a mailbox address, a
  user-profile path, a host name, or an organization name.
- The router rejection path is asserted to leave `SelectedFolderPath` unchanged and to **not** set it
  to `null` (the #499 boundary).

### Must-stay-green set (no behaviour change permitted)

- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` — direct row selection,
  ancestor activation, child activation, banner and trash pseudo-rows, case-insensitive root match,
  mailbox roots containing `@`.
- `UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs` →
  `Issue609_ResolvePaths_PrefixesAtMailboxArchiveRootExactlyOnce` (`:255-272`).
- `Issue609_FolderPredictor*` projection tests in
  `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs`.
- The remaining existing `EmailFilerConfig_Tests` and `FolderConverterTests` suites, with the single
  documented exception of `FolderConverterTests.cs:329`, which codifies D5f and must be updated.
- Pre-existing flakes #594, #592, #586, and #584 are outside this set. If one of them fails during
  verification, record it as a known pre-existing flake with its issue number; do not attempt to fix
  it under #614 and do not use it to declare this change blocked.

### Coverage impact and targets for changed lines/modules

- `>= 90%` line coverage on `ArchiveStemContract` and on every new or changed method, per
  CLAUDE.md § UT2 for new modules. The pure helpers make this inexpensive.
- No reduction in coverage for the lines changed by this work.
- Repository-wide coverage over the testable denominator (`>= 80%`, CLAUDE.md § UT2) is recorded and
  reported against the merge-base baseline. Because no baseline evidence exists for this feature at
  the time of writing, the blocking obligation is "capture the baseline, capture the post-change
  figure, and demonstrate no regression"; the absolute repository-wide number is a
  record-and-report obligation, not an independent blocking gate for this change.
- Coverage artifacts are written to `<FEATURE>/evidence/coverage/` and baselines to
  `<FEATURE>/evidence/baseline/`, per
  `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Writing to `artifacts/coverage/`,
  `artifacts/baselines/`, or `artifacts/qa/` is a policy violation.

### Toolchain commands to run (format → lint → type-check → test)

Run in this exact order; restart from step 1 if any step fails or modifies a file
(CLAUDE.md § C#1, § CUT3):

1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`; always via
   `dotnet tool run`, never a global install; run `dotnet tool restore` once per worktree first)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

Two properties of steps 2 and 3 are load-bearing and must not be "restored": `/t:Rebuild` is
required because MSBuild's up-to-date check does not invalidate on a command-line `/p:` change, so a
warm `/t:Build` returns exit 0 with `CoreCompile` skipped and the gate cannot fail; and
`/p:Nullable=enable` must **not** be added, because nullable is per-file opt-in in this solution and
forcing it solution-wide reports errors that are red on `main` regardless of any change. Both points
are stated in CLAUDE.md § C#1.2 and § C#1.3. Non-vacuity of the build steps must be demonstrated —
a build that skips `CoreCompile` on every project proves nothing.

### Manual validation steps (required)

Manual validation is required because the reported failure is reached through live Outlook, and no
automated test may touch a live Outlook process:

1. File a message to a normal archive subfolder and confirm success (unchanged behaviour).
2. Activate the store-root ancestor segment and confirm the selection does not change and a
   diagnostic is emitted.
3. Press OK with the archive root itself selected and confirm the fast, redacted failure rather than
   the leaked `ArgumentException`.
4. File to a folder whose name contains `.` and confirm it now succeeds.
5. Confirm no message, dialog, or log line produced in steps 2–4 contains a real mailbox address,
   user-profile path, host name, or organization name.

Manual validation results are recorded in `<FEATURE>/evidence/qa/` with an ISO-8601 timestamp.


## Acceptance Criteria

Each criterion below is checkable by an executor or reviewer against named evidence. This section is
the sole authoritative AC source for this `full-bug` issue.

- [x] **AC1 (contract type).** A new pure static class `ArchiveStemContract` exists at
      `UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs`, is registered in
      `UtilitiesCS/UtilitiesCS.csproj` as an explicit `<Compile Include>` item, exposes
      `IsFullOutlookPath(string)`, `RequireArchiveRelativeStem(string, string)`, and
      `TryMakeArchiveRelative(string, string, out string)`, performs no filesystem, network, COM, or
      environment access, uses no `init` accessor, `record`, or `record struct`, and its file is
      under 500 lines. The decision on whether `IsFullOutlookPath` also rejects drive-rooted input is
      recorded explicitly with its rationale.
- [x] **AC2 (D1).** `BreadcrumbBridgeRouter.SelectHierarchyPath`
      (`BreadcrumbBridgeRouter.cs:494-502`) no longer stores a value produced by the verbatim
      pass-through at `:525`. A unit test asserts that activating a segment not at or under the
      archive root leaves `SelectedFolderPath` unchanged and emits a diagnostic.
- [x] **AC3 (D2).** Unit tests assert that store-root segment activation, cross-store segment
      activation, and at-or-above-archive-root ancestor activation each leave `SelectedFolderPath`
      unchanged, while a valid under-root ancestor activation and a valid child activation continue
      to set it correctly. Leaf segments remain non-activatable (`BreadcrumbRow.cs:156` unchanged).
- [x] **AC4 (D3).** `SelectRow`'s `row.FilingTarget` pass-through (`BreadcrumbBridgeRouter.cs:484-487`)
      is guarded by the same contract, and a unit test asserts that selecting an out-of-root
      suggestion row does not place a full Outlook path into `SelectedFolderPath`. `ToHierarchyPath`
      (`:140-163`) no longer fabricates an out-of-root hierarchy path by prefixing at `:162`.
- [x] **AC5 (D4).** Both `EmailFilerConfig.ResolvePaths` overloads
      (`EmailFilerConfig.cs:187-188` and `:203-204`) call
      `RequireArchiveRelativeStem(DestinationOlStem, nameof(DestinationOlStem))` **before**
      concatenation. `GetStem` (`:232-235`) and `IsDeleteRelevant` (`:171`) use prefix-anchored,
      `OrdinalIgnoreCase`, separator-terminated comparisons instead of unanchored
      `Replace` / `Contains`.
- [x] **AC6 (D5a).** `ToFsFolderpath` validates only the segments it derives and never the
      caller-supplied `fsAncestorEquivalent`. A test proves that a filesystem ancestor of the shape
      `C:\Users\<user>\OneDrive - <Org>` (containing `.`, a space, and `-`) succeeds for a valid
      relative stem.
- [x] **AC7 (D5b).** `IllegalFolderCharacters` (`FolderConverter.cs:39-42`) is replaced by per-segment
      Windows name validation covering `Path.GetInvalidFileNameChars()`, trailing dot, trailing
      space, and reserved device names, and no longer bans `.`, `[`, or `]`. Tests cover each of
      those four rules positively and negatively.
- [x] **AC8 (D5c).** The `fsPath.Substring(3)` drive-prefix assumption (`FolderConverter.cs:158`) is
      removed. Tests prove a UNC ancestor and an ancestor shorter than three characters neither
      throw `ArgumentOutOfRangeException` nor silently mangle the path.
- [x] **AC9 (D5d).** The ancestor strip (`FolderConverter.cs:155`) is prefix-anchored, separator-aware,
      and `OrdinalIgnoreCase`. Tests prove a repeated ancestor substring is stripped only at the
      prefix and a case-differing ancestor still matches.
- [x] **AC10 (D5e).** The `ArgumentException` raised at `FolderConverter.cs:161-167` no longer embeds
      `fsPathExDividers`. A test asserts the message contains neither the mailbox address nor the
      filesystem ancestor.
- [x] **AC11 (D5f).** `BuildAlternativesDictionary`'s "Remove illegal characters" option
      (`FolderConverter.cs:110-113`) removes only the illegal characters, and the assertion at
      `UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs:329` — which currently codifies
      the defect by asserting an empty result — is updated to assert the corrected semantics.
- [x] **AC12 (D5g and dead parameter).** `ResolveOlRoot` (`FolderConverter.cs:226-242`) selects the
      root by separator-terminated prefix rather than `Contains`, proven by an `Archive2`-style
      near-miss test. The never-read `bool ask = true` parameter (`:145`) is removed, or marked
      `[Obsolete]` with its dead status documented in code; whichever is chosen is stated explicitly
      in the change description.
- [x] **AC13 (D6).** `AppOlObjects.ArchiveRootPath` (`AppOlObjects.cs:237-248`) no longer returns an
      unverified, default-store-scoped string combine silently; an unresolvable or cross-store
      archive root produces an explicit, redacted diagnostic. The validation adds no per-filed-item
      COM round-trip. Verified through the mockable `IOlObjects` seam, not against live Outlook.
- [x] **AC14 (D7 fallback half).** `AppFileSystemFolderPaths.LoadFolders` (`:206-235`) no longer falls
      back for the `OneDrive` key to `AppData` or to `SpecialFolders.First().Value` silently; an
      unresolvable `OneDrive` root fails explicitly with a redacted diagnostic. Any environment
      access introduced for testability uses an injectable delegate seam; no test mutates process
      environment state. `MatchBestSpecialFolder` (`:77-91`) is **not** modified.
- [x] **AC15 (D8).** `EfcDataModel.MoveToFolderAsync(MAPIFolder, olAncestor, ...)` (`:344-348`) derives
      its stem via `TryMakeArchiveRelative` through an extracted pure helper instead of
      `Replace` + `Substring(1)`. Unit tests cover under-root, store-root, cross-store, and
      case-differing-ancestor inputs. Both live callers
      (`EfcFormController.cs:500-507` and `:778-787`) continue to work.
- [x] **AC16 (D9).** `EfcFormController.ActionOkAsync` and the `IsValidSelection` property delegate to
      two scope-specific predicates in one shared guard type (`EfcSelectionGuard`): the filing predicate
      `IsValidFilingSelection` rejects `null`, `string.Empty`, whitespace, a `"===="`-prefixed sentinel,
      and any full Outlook path, and carries no minimum-length rule; the creation predicate
      `IsValidCreationSelection` additionally enforces the three-character minimum. Tests prove each
      rejection and that a valid relative stem is accepted.
- [x] **AC17 (primary regression test, fails before and passes after).**
      `UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs` contains
      `Issue614_ResolvePaths_WithStoreRootStem_RejectsNonRelativeStemWithoutLeakingIdentifiers` with
      inputs `OlAncestor = @"\\mailbox@example.com\Archive"`,
      `DestinationOlStem = @"\\mailbox@example.com"`, and a filesystem ancestor of the
      `C:\Users\<user>\OneDrive - <Org>` shape, using the pure configuration seam (`Globals = null`,
      parameterless `ResolvePaths()`). Captured evidence shows it **failing** on the pre-fix tree and
      **passing** after the fix. It asserts both that the contract exception is thrown and that the
      message leaks no host identifier.
- [x] **AC18 (producer-side companion test).**
      `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` contains a test that binds with
      `archiveRootPath = @"\\mailbox@example.com\Archive"` and a provider chain whose segment 0 is
      `\\mailbox@example.com`, sends `segmentActivate` for index 0, and asserts `SelectedFolderPath`
      is not the store-root full path. Evidence shows it failing pre-fix.
- [x] **AC19 (no regression of the #609 / #439 scenarios).** All of the following remain green with
      unchanged behaviour: `BreadcrumbBridgeRouterIssue439Tests` (direct row selection, ancestor
      activation, child activation, banner and trash pseudo-rows, mailbox roots containing `@`,
      case-insensitive root match); `Issue609_ResolvePaths_PrefixesAtMailboxArchiveRootExactlyOnce`
      (`EmailFilerConfig_Tests.cs:255-272`); and the `Issue609_FolderPredictor*` projection tests in
      `FolderPredictorTests.cs`. `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` is not
      modified by this change.
- [x] **AC20 (explicit, non-absorbing interaction with open issue #499).** The change does not modify
      `BindRowsAsync`'s selection-clearing semantics (`BreadcrumbBridgeRouter.cs:59, :136`), and on
      rejecting an out-of-root activation it leaves `SelectedFolderPath` unchanged rather than
      setting it to `null`. A test asserts the non-null, unchanged behaviour. The change description
      states in one paragraph how #614 interacts with #499 and that #499 remains open and
      unregressed.
- [x] **AC21 (redaction).** No production message, log line, diagnostic, test literal, evidence
      artifact, or document added or modified by this change contains a real mailbox address,
      user-profile path, host name, or organization name. Placeholders of the
      `\\mailbox@example.com` and `C:\Users\<user>\OneDrive - <Org>` shape, or fabricated
      equivalents, are used throughout (open issue #602).
- [x] **AC22 (test-policy compliance).** Every new or modified test uses MSTest with Moq and
      FluentAssertions in Arrange–Act–Assert form, is independent, isolated, and deterministic,
      creates no temporary file, and contains no `Thread.Sleep`, `Task.Delay`, `DateTime.Now`,
      `Random.Shared`, or wall-clock wait. Test files live in the mirrored `*.Test` project trees, not
      alongside production source.
- [x] **AC23 (coverage).** `ArchiveStemContract` and every new or changed method reach `>= 90%` line
      coverage (CLAUDE.md § UT2). No changed line loses coverage relative to the merge-base baseline.
      A merge-base baseline artifact and a post-change coverage artifact are both captured, and the
      repository-wide testable-denominator figure is recorded with an explicit statement that this
      change does not lower it. All artifacts are written under `<FEATURE>/evidence/baseline/` and
      `<FEATURE>/evidence/coverage/`; none is written to `artifacts/baselines/`,
      `artifacts/coverage/`, or `artifacts/qa/`.
- [ ] **AC24 (full four-step toolchain).** A single clean pass of all four steps, in order, is
      recorded with the exact commands and their exit codes: `dotnet tool run csharpier check .`;
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`;
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`;
      and `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`. `/p:Nullable=enable` is not
      added and `/t:Build` is not substituted for `/t:Rebuild`. Non-vacuity is demonstrated for both
      MSBuild steps (the build did not skip `CoreCompile` on every project). Evidence is written to
      `<FEATURE>/evidence/qa/`.
- [x] **AC25 (scope isolation and file-size limit).** No file outside the in-scope list is modified.
      Specifically unmodified: `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`,
      `UtilitiesCS/EmailIntelligence/FolderConverter.cs` (the uncompiled duplicate), and
      `AppFileSystemFolderPaths.MatchBestSpecialFolder` (`:77-91`). No production, test, or script
      file exceeds 500 lines after the change; the executor records the post-change line count of
      every edited file.
- [x] **AC26 (manual validation).** The five manual validation steps in `## Test Strategy` are
      executed against a live Outlook profile and their results — including the redaction check —
      are recorded in a timestamped artifact under `<FEATURE>/evidence/qa/`. Any step that cannot be
      executed is recorded as not executed, with the reason; it is not silently omitted.


## Risks & Mitigations

### Technical or operational risks

1. **Blast radius.** The change touches seven production files across three assemblies
   (`UtilitiesCS`, `QuickFiler`, `TaskMaster`). A regression in `FolderConverter` or
   `EmailFilerConfig` affects every filing operation, not only the reported path.
2. **Behaviour change is user-visible.** Segment activations that previously "worked" (silently
   selecting an out-of-root path) now do nothing but emit a diagnostic. A user who had learned to
   rely on the old behaviour will perceive this as a new restriction rather than as a fix.
3. **Widening the accepted character set (D5b) is a real semantic change.** Folder names containing
   `.`, `[`, or `]` will now be converted to filesystem paths where they previously threw. If any
   downstream consumer assumed the old, narrower guarantee, it will now receive input it has never
   seen.
4. **Updating `FolderConverterTests.cs:329` removes an existing assertion.** A reviewer who treats
   existing tests as spec (CLAUDE.md § 7.3) will correctly flag this. Without an explicit record,
   the change looks like a test weakened to make a build pass.
5. **Accidental absorption of #499.** The natural implementation of "reject an invalid selection" is
   to null the field; that would silently implement half of #499's design and pre-empt its open
   question.
6. **Scope creep into D6 and D7.** Both live in `TaskMaster/AppGlobals`, are COM-adjacent, and have
   weaker seams than the rest of the change. Hardening them can expand well beyond the fix budget.
7. **Verification depends on live Outlook for the manual steps.** No automated test can reproduce
   the end-to-end user path, so a defect that only manifests through real COM behaviour could pass
   the automated gate.
8. **Pre-existing test flakes (#594, #592, #586, #584) can mask or mimic a regression** during the
   test step.
9. **Line-number drift.** The cited line numbers come from a 2026-08-26 snapshot of this worktree. A
   concurrent merge could move them.
10. **Analyzer version skew (#615)** could produce diagnostics during step 2 that are unrelated to
    this change.

### Mitigations and rollbacks

1. The must-stay-green set in `## Test Strategy` is the blast-radius control, and AC19 makes it
   blocking. The full four-step toolchain (AC24) runs over the whole solution, not only the changed
   projects.
2. The rejection path emits a diagnostic rather than failing silently, so the new behaviour is
   observable to the user at the moment it occurs. The manual validation steps (AC26) confirm the
   diagnostic is present and readable.
3. Widening is strictly safer than narrowing here: no previously succeeding input begins to fail.
   Per-segment validation is tested positively and negatively (AC7), and the change is confined to
   segments the converter derives (AC6), never to caller-supplied roots.
4. AC11 requires the `:329` update to be explicit and the research records why the current assertion
   codifies a defect. The change description must quote the old assertion and state why it was
   wrong.
5. AC20 makes "leave `SelectedFolderPath` unchanged, do not null it" a blocking, test-asserted
   criterion, and forbids touching `BindRowsAsync`'s clearing semantics.
6. D6 and D7 are scoped narrowly to "fail explicitly instead of falling back silently" — no
   redesign of store resolution. The `MatchBestSpecialFolder` half is excluded outright (AC14). The
   budget note in `## Scope & Non-Goals` gives the planner a documented, auditable path to defer
   them rather than an undocumented one.
7. The manual validation steps are blocking (AC26), and the D4 boundary guard means that even if a
   producer defect survives, the failure is a fast, redacted, diagnosable exception rather than a
   wrong filing destination.
8. Known flakes are identified by issue number before the test step and recorded as pre-existing if
   they occur; they are not fixed under #614 and are not accepted as a reason to declare the change
   blocked.
9. If a cited line no longer matches, re-locate the construct by name and record the drift in the
   change description rather than assuming the defect is absent.
10. #615 is already promoted and tracked. Analyzer findings unrelated to the changed files are
    recorded against #615 rather than absorbed here.
11. **Rollback:** revert the single PR. No migration, no persisted-state change, no feature flag to
    unwind. Partial rollback is available along the enforcement-point boundary described in
    `#### Rollback/feature-flag considerations`, with the constraint that the D4 boundary guard must
    not be reverted while the router change is retained.


## Rollout & Follow-up

### Release/rollout steps

1. Deliver as a single PR against `main` from
   `bug/efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614`.
2. The PR body is authored with the `pr-author` skill and its SHA-256 provenance receipt before
   `gh pr create` or `gh pr edit --body-file`, per CLAUDE.md.
3. The PR must state: the confirmed defect set D1–D9 and which are addressed; the explicit rejection
   of the "remove `.` from `IllegalFolderCharacters`" hypothesis and why; the `FolderConverterTests.cs:329`
   assertion change and its justification; the `ToFsFolderpath` `ask` parameter decision; the
   interaction with #499; and any deferral of D6 or D7 with its follow-up issue number.
4. No staged rollout, no feature flag, no configuration change is required at deployment. The
   add-in ships as a unit.
5. Evidence artifacts (baseline, coverage, QA gates, manual validation) are committed under
   `<FEATURE>/evidence/<kind>/` with ISO-8601 timestamps. The worktree must be clean before the
   change is considered complete.

### Post-fix monitoring or clean-up tasks

- After release, confirm through the manual steps that the store-root activation path produces the
  diagnostic and not the `ArgumentException`, and that normal filing to archive subfolders is
  unaffected.
- Watch for user reports of "the breadcrumb no longer lets me select X". Any such report should be
  triaged against the archive-root floor: if a legitimate destination is being rejected, the archive
  root resolution (D6) is the first thing to check, not the contract.
- **The three off-chain research findings are being promoted to their own issues** and are not
  fixed here:
  1. The orphaned duplicate `UtilitiesCS/EmailIntelligence/FolderConverter.cs` dead file with
     always-false guards (`:30`, `:40`), not compiled per `UtilitiesCS/UtilitiesCS.csproj:1054`;
     recommended action is deletion. Severity Low.
  2. `FolderPredictor.CreateFolder`'s non-short-circuit `|` before `parentBranchPath[0]`
     (`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:691`), which throws
     `IndexOutOfRangeException` on an empty `parentBranchPath` regardless of the left operand. Not
     reachable from the OK path. Severity Low/Medium.
  3. `AppFileSystemFolderPaths.MatchBestSpecialFolder` substring matching
     (`TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs:87`), with no production caller today, but
     whose codified-substring tests
     (`AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs:96-115`) would freeze the wrong
     semantics for any future caller. Severity Low.
- If the planner defers D6 or D7 under the budget note in `## Scope & Non-Goals`, promote each
  deferral to its own issue and record the issue number here.
- Open issue #602 (host-identifier redaction) is advanced but not closed by this change: only the
  #614 path is made compliant.
- Open issue #499 remains open. Its design question — whether `BindRowsAsync` should clear or
  restore `SelectedFolderPath` on rebind, and whether to raise `SelectedFolderPathChanged(null)` —
  is untouched by this change and should be decided there.

### Links

- Issue: #614 — https://github.com/drmoisan/TaskMaster/issues/614
- Related open: #499 (stale `SelectedFolderPath` after rebind); #602 (host-identifier leakage);
  #615 (analyzer version skew — already promoted).
- Related closed: #609 and PR #611 (EFC full-path destination resolution regression; scope limited
  to `FolderPredictor.ProjectSuggestionPath`).
- Pre-existing flakes, not in scope: #594, #592, #586, #584.
- Research artifact:
  `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/research/2026-08-26T10-30-store-root-path-leak-defect-census-research.md`
- Issue record:
  `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/issue.md`
- Policy: `CLAUDE.md` (§ General Code Change Policy, § C# Code Change Policy, § General Unit Test
  Policy, § C# Unit Test Policy); `.claude/skills/acceptance-criteria-tracking/SKILL.md`;
  `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.
