# Bug Specification: Folder Settings never persist; User Email shows "Error Loading" (Issue #797)

- Issue: #797
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/797
- Branch: bug/folder-settings-never-persist-797
- Base commit: c431dc3297e864041d829e8d79b348960b8d8019
- Work Mode: full-bug
- Date: 2026-09-06
- Requirements source: issue.md in this feature folder (AC1-AC8, settled with the maintainer on 2026-09-06)
- Research source: research/research-folder-settings-persistence.md in this feature folder

> **Authoritative acceptance-criteria source.** Work Mode is full-bug, so this file is the single
> authoritative acceptance-criteria source per the acceptance-criteria-tracking skill. No
> user-story.md is produced for this work item, and no second checkbox list exists anywhere in this
> feature folder other than the issue.md copy from which the criteria below were transcribed verbatim.

> **Formatting is deliberate — do not "fix" it.** A downstream scheduler harvests backtick-delimited
> path tokens from this document to derive the change footprint for a parallel run against three
> concurrent sibling work items. Every file this change creates or modifies is therefore backticked
> exactly once, inside the `## Write Set` section, and nowhere else. All other file and line
> citations in this document are written as plain prose without backticks, on purpose. Adding
> backticks to a citation elsewhere would inject a false write claim and needlessly serialize the
> run; removing a backtick inside the Write Set would drop a real file from the footprint.
>
> One unavoidable exception: the `## Acceptance Criteria` block below is reproduced verbatim from
> issue.md, and the maintainer-authored text contains inline code spans. Those spans are quoted
> runtime values and C# member names (a runtime file name under the user's local AppData directory,
> a percent-prefixed environment path, a generic method signature, and an escaped backslash pair).
> None of them is a repository-relative path and none is a write claim. They are preserved because
> verbatim reproduction of the criteria takes precedence.

---

## Summary

Values chosen in Settings -> Folder Settings (Archive Root Outlook, Archive Root File System, Junk
Potential, Junk Email) survive only the current Outlook session and are lost on restart. The settings
file StoresWrapper.json has never been created on the reporting machine, and the save path silently
does nothing when no file path is configured, so the defect emits no diagnostic signal. In the same
dialog, User Email renders the generic placeholder "Error Loading" because the Exchange SMTP lookup
throws a COM exception that is caught, converted to null, and never retried.

Two independent root causes produce the reported symptoms. They are kept separately traceable through
this document: root cause 1 is a bootstrap gap between the loader and the serializer; root cause 2 is
an unretried COM failure in the SMTP lookup. AC1 through AC5 derive from root cause 1; AC6 derives
from root cause 2; AC7 and AC8 are adjacent defects in the same rendering method.

Severity is High. The dialog cannot persist any per-store setting on a machine where the file has
never been created, which is every fresh install, and the archive root and junk folder settings it
manages feed the filing and junk-mail workflows.

---

## Root Cause Analysis

### Root cause 1 — bootstrap gap between the loader and the serializer

When the settings file is absent the deserializer returns null and the caller builds a fresh wrapper
that never adopts the loader's disk configuration, so the wrapper carries an empty file path and the
serializer's guard silently returns without writing and without logging.

Verified trace at the base commit (line numbers re-derived by the research against this worktree):

1. TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs lines 39-44 resolves the loader from
   IntelRes.Config by the key "StoresWrapper" and calls the non-typed deserialize forwarder.
2. The live path binds SmartSerializable&lt;T&gt;.Deserialize&lt;U&gt;(loader) in
   UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs lines 214-234, not the
   SmartSerializableBase overload cited in issue.md. The defect shape is identical in both classes,
   but the edit target and regression surface must be read off SmartSerializable.cs.
3. DeserializeJson at SmartSerializable.cs lines 388-394 returns null because DiskExists is false.
4. The loader-configuration copy at SmartSerializable.cs lines 222-225 is guarded by
   `if (instance is not null)`, so on the null path the loader's disk configuration is discarded.
5. AppOlObjects.StoreLoading.cs line 64 calls BuildFreshStoresWrapper, which constructs a wrapper
   whose Config.Disk.FilePath is the FilePathHelper default empty string
   (UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs line 71).
6. StoreWrapperController.SaveChanges line 356 calls Model.Serialize(). The guard at
   SmartSerializable.cs line 444 is `Config.Disk.FilePath != ""`, so with an empty path the method
   returns without writing and without logging.
7. Because the file is never written, every subsequent Outlook start takes the same null path.
   In-session persistence works only because the values live in the in-memory store wrapper.

Runtime confirmation (read-only; the log lives outside the repository). The pair of lines below
recurs once per Outlook start at 17:29:59, 19:09:20 and 19:26:35 on 2026-09-06:

```text
[VSTA_Main] WARN  TaskMaster.AppOlObjects - StoresWrapper config deserialized to null; rebuilding from live stores.
[VSTA_Main] ERROR UtilitiesCS.OutlookObjects.Store.StoreWrapper - Error retrieving PrimarySmtpAddress from secondary inbox. The operation failed.
```

Two properties of the loader are established by the research and are load-bearing for the fix. First,
the loader already in scope at AppOlObjects.StoreLoading.cs line 39 already carries the correct,
resource-derived path; nothing new must be constructed. Second, the sibling three-argument overload
(SmartSerializable.cs lines 257-310) copies the loader configuration unconditionally, which is why
RecentFolders, which uses that overload, has a file on disk while StoresWrapper does not.

Downstream consequences of root cause 1 that carry their own acceptance criteria:

- The empty-path guard is silent (AC2). It also compares only against the empty string, so a null
  FilePath passes the guard and reaches the write path; FilePathHelper can assign a null
  \_filePath in its property-changed handler, so the null case is genuinely reachable.
- The write is deferred by a three-second single-shot timer (AC4). The timer callback runs on a
  ThreadPool background thread, which is not joined at process exit, so a pending write is lost with
  no log entry when Outlook tears down the AppDomain.
- Junk folder selections are persisted twice, by a per-store JSON path and by .NET user settings, and
  the two mechanisms resolve relative paths against different roots and have different scope (AC5).
  Because of root cause 1 the JSON side currently writes nothing at all while the settings side
  writes successfully, so the two stores of truth are already divergent on every affected machine.

Evidence-strength calibration on AC5: the research verified that the reflection target
ApplyJunkFolderSelections does exist with an exactly matching signature at
TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs lines 36-45, so the reflection lookup succeeds in
production and the warn-and-return branch is reached only by test doubles. The issue text's
"silently returns with only a warning when the method is not found" describes a real but
production-unreachable branch. The substantive AC5 defect is the double persistence itself plus the
untyped, rename-fragile binding.

### Root cause 2 — unretried COM failure in the Exchange SMTP lookup

The Exchange SMTP lookup throws a COMException that is caught, converted to null, and rendered as a
generic placeholder, and the lookup is never retried.

Verified trace:

1. UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs line 83 assigns UserEmailAddress from
   GetSmtpAddressFromStore, once per Init.
2. GetSmtpAddressFromStore (StoreWrapper.cs lines 179-217) walks RootFolder -> Session -> CurrentUser
   (line 184) -> AddressEntry (line 190) -> GetExchangeUser (line 196) -> PrimarySmtpAddress
   (line 202). A COMException anywhere in that chain is caught by a single outer catch at line 209,
   logged, and converted to `return null` at line 215. The observed failure threw at line 184,
   matching the log's reported line number.
3. UserEmailAddress carries JsonIgnore (StoreWrapper.cs lines 173-174), so a successful lookup is not
   cached across restarts.
4. StoreWrapperController.cs line 296 renders the null as the literal "Error Loading". That literal
   occurs at exactly three sites, all in StoreWrapperController.cs (lines 294, 295 and 296), and
   nowhere else in any C# file in the repository; no existing test asserts it.

The Outlook-side cause of the COM failure is not determinable from the log, and this specification
does not claim to identify it. The fix addresses the absence of a fallback and the absence of a
retry, not the underlying Outlook condition.

Correction to one attribution in issue.md, recorded because it changes scope. issue.md states that
the same session's ThreadMonitor captured the UI thread inside the Exchange primary-SMTP getter, and
infers that a second caller of this chain blocks on it. The research verified the captured stacks and
found they belong to RecipientStatic.GetRecipientAddress in the QuickFiler recipient-resolution path,
not to StoreWrapper.GetSmtpAddressFromStore. The blocking hazard for the Exchange primary-SMTP getter
is real and repeatedly evidenced across multiple timestamps in the same log, but it is attributable
to a different, out-of-scope caller. That caller is not fixed under this issue.

### Adjacent defects in the same rendering method

- AC7: the leading double-backslash store prefix on the Inbox and Root Folder labels is Outlook's
  native MAPIFolder.FolderPath read directly at StoreWrapperController.cs lines 294-295. There is no
  transformation between the COM read and the label. This is cosmetic, not a fault.
- AC8: StoreWrapperController.cs line 169 assigns Current from List.Find, which returns null when no
  store matches. PopulateWithCurrent then dereferences Current without a null-conditional operator at
  lines 288-291, while the very next block at lines 294-296 uses the null-conditional form — an
  inconsistency inside a single method. A null Current therefore throws NullReferenceException before
  any placeholder can render. GetRelativeFsPath at lines 459-460 has the same unguarded dereference
  and is called from line 298.

---

## Scope and Non-Goals

### In scope

The footprint stays inside: the serializer under the reusable type classes tree; the Outlook store
wrapper and its controller; the store-loading and junk-folder partials in the TaskMaster application
globals; one new interface file in the already-established UtilitiesCS interfaces folder; and the
corresponding tests and their non-SDK-style project compile entries.

One additional in-scope cleanup, not covered by any acceptance criterion: the single-ampersand
operator at StoreWrapperController.cs line 464 is changed to the short-circuit form. The research
verified that both operands are pure and null-tolerant, so this is behaviourally inert. It is a
readability and consistency fix only, and the change description must not claim it repairs a fault.
It lands in a file already in the Write Set because GetRelativeFsPath moves to the new display
partial.

### Explicit scope constraints

- Nothing under the dot-claude, dot-codex or dot-agents trees is edited.
- Neither published JSON file under the config directory is edited.
- No GitHub workflow file is edited.

### Non-Goals

> The file names in this section are deliberately written without backticks. They are out-of-scope
> paths, and backticking them would register them as write claims with the footprint extractor.

1. **Pre-existing 500-line cap violation in the serializer is not resolved.**
   UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs is already 613 lines
   against the 500-line cap in the general code change policy, before any change here. This change
   adds a small number of lines to it and does not split it. Rationale: splitting a shared
   reusable-type-classes file during a parallel run would create merge contention with concurrently
   running sibling work items, and the split is a separable concern that should be raised on its own
   rather than folded into a bug fix. Stated plainly: this change does not resolve that pre-existing
   violation, and it does not introduce a new violation class.
2. **The shared deserialize overload is not changed.** Its null return on the file-absent path is a
   load-bearing fail-soft contract, documented in-code and relied upon by a second production caller
   in TaskMaster/AppGlobals/AppAutoFileObjects.FolderPredictorLoad.cs. See D1.
3. **SmartSerializableBase.cs is not edited at all.** It is 545 lines, already over the cap, and the
   live StoresWrapper path does not enter it.
4. **The IOlObjects interface is not extended.** See D2.
5. **No VSTO add-in lifecycle file is modified.** See D3.
6. **No resource file is modified.** See D7.
7. **The QuickFiler recipient-resolution blocking hazard is not fixed** under this issue. It is a
   different caller of the same Outlook getter and is out of scope.
8. **A genuinely non-blocking Outlook COM property read is not delivered.** The research verified
   that no existing seam in UtilitiesCS provides one: the repository's only timeout primitive
   dispatches work to ThreadPool (MTA) threads, and an Outlook interop object is STA-apartment-bound,
   so the call marshals back to the STA and the STA still blocks while the caller merely abandons the
   wait. AC6 is therefore satisfied with a synchronous retry on the UI thread, attempted at most once
   per dialog open and only when the address is null. A non-blocking read is recorded here as a
   potential follow-up work item, not folded in.
9. **The dead-branch observation at StoreWrapperController.cs line 466 is not addressed.** The
   research recorded, as a secondary observation, that the path-name producer never returns an empty
   name, making that branch effectively unreachable. That is a separate finding outside AC1-AC8.
10. **AC3 is not automated.** It is manual verification by Outlook restart; see the Verification
    section.

---

## Write Set

Every file this change creates or modifies appears below as a concrete repository-relative path
inside backticks. This is the only section of this document containing backticked paths, except for
the verbatim acceptance-criteria block noted in the header blockquote.

### Production — modify

- `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs`
- `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs`
- `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs`
- `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs`
- `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`

### Production — create

- `UtilitiesCS/Interfaces/IGlobals/IJunkFolderSelectionSink.cs`
- `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs`

### Tests — modify

- `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.ButtonAndPopulate.cs`
- `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperControllerTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperTests.cs`
- `TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs`

### Tests — create

- `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableSerializeGuardTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs`

### Project compile-entry carriers — modify

Every project in this solution is non-SDK-style, verified by the research: each project file opens
with a ToolsVersion attribute and the 2003 MSBuild namespace and closes with an import of the C#
targets, and no project file carries an Sdk attribute. A newly created C# file is therefore not
picked up by a wildcard and must be registered by a hand-added compile entry.

- `UtilitiesCS/UtilitiesCS.csproj`
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
- `TaskMaster.Test/TaskMaster.Test.csproj`

The third entry is retained as a write claim to keep the parallel run schedule-safe. The research
records it as conditional: it is required only if the AC1 tests are placed in a new file under the
TaskMaster test project's AppGlobals directory rather than appended to the already-registered
AppOlObjectsCoverageTests.cs. Claiming it unconditionally is the conservative choice; if the
implementation appends to the existing file, this project file may end the change unmodified.

### Requirements document — modify

- `docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/issue.md`

This file is modified for acceptance-criteria check-off only, mirroring the check-offs made in this
specification. No criterion text is altered. This specification itself and the timestamp-named
research and evidence artifacts in this feature folder are excluded from the Write Set by convention.

### Files not written, stated in plain prose

The TaskMaster production project file, TaskMaster.csproj, needs no compile-entry change, because the
two files that receive the AC1 and AC5 production edits — AppOlObjects.StoreLoading.cs and
AppOlObjects.JunkFolders.cs — are already registered in it. The Visual Studio solution file,
TaskMaster.sln at the repository root, is not modified, because every project that receives a new
source file already exists in the solution and only its own project file changes. No repository-root
build property file is modified. Directory.Build.props and Directory.Build.targets both exist at the
repository root as of the base commit; the props file sets a single System.Reactive packages.config
suppression property recorded under issue #730. Neither file participates in this change and neither
is written. No file at the repository root is written by this change.

### Extension note

No file with an extension of resx, config, props or targets is created or modified by this change.
This is consistent with D7: the resource entry that defines the settings file name and its AppData
special folder already carries the correct values, so IntelligenceResources.resx — spelled out, the
UtilitiesCS resource file ending in dot r-e-s-x — is read-only for this work item and is deliberately
absent from the Write Set. The research reached the same conclusion independently, so there is no
disagreement to report.

Three project files are in the Write Set. Spelled out in words in case a downstream extractor drops
the extension, each ends in dot c-s-p-r-o-j: the UtilitiesCS project file, the UtilitiesCS test
project file, and the TaskMaster test project file, at the paths backticked above.

---

## Design and Approach

The decisions below were settled by the orchestrator against verified code and are recorded as the
chosen approach. They are not reopened by this specification.

### D1 — AC1 is fixed at the call site, not in the shared serializer

AC1 is fixed in TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs alone. The fresh-build branch
discards the loader that is already in scope at that call site; the fix applies the loader's
configuration to the freshly built wrapper there, after BuildFreshStoresWrapper returns.

The shared deserialize overload is not changed, for two reasons.

1. Its null return is a load-bearing fail-soft contract. A second production caller, the folder
   predictor load path in TaskMaster/AppGlobals/AppAutoFileObjects.FolderPredictorLoad.cs, documents
   in its own in-code comment that a null return when the dedicated file is absent is intentional and
   that the accessor then falls back to a flat model. Returning a constructed instance would silently
   disable that fallback.
2. The shared reusable-type-classes tree is touched concurrently by sibling work items in this run.

A supporting mechanical fact: on the file-absent path there is no instance to copy onto, because the
JSON reader returns null. "Adopt the loader's disk configuration" is therefore not expressible as a
copy inside that overload without constructing an instance and changing the method's contract.

The seam required already exists and is already exercised: BuildFreshStoresWrapper is protected
internal virtual and is overridden by the TestableAppOlObjects harness in the TaskMaster test
project. LoadStoresAsync already holds the loader in scope.

The branch where the configuration key is not found has no loader and must remain a fresh build with
an empty path. AC2's new error log makes that case visible rather than silent.

Rejected alternative, recorded so it is not retried: switching the call site to the three-argument
askUserOnError overload would fix AC1 with one call-site change, but that overload can raise a modal
dialog during VSTO startup and constructs the wrapper through a parameterless CreateEmpty rather than
through BuildFreshStoresWrapper, leaving Globals null so that the store-filter call would throw.

### D2 — AC5 uses a new dedicated interface, not an extension of IOlObjects

A new interface is created at UtilitiesCS/Interfaces/IGlobals/IJunkFolderSelectionSink.cs with a
single member accepting the junk-certain and junk-potential relative paths. It is implemented by the
existing TaskMaster globals partial TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs, where the
matching method already lives. Explicit interface implementation is preferred so the public surface
of the globals type does not widen; the research verified that an implicit implementation would
require promoting the existing internal method to public.

The controller replaces its reflection lookup with a typed cast to that interface and logs an error,
not a warning, when the cast fails. The System.Reflection using directive in the controller may then
be removed if nothing else in the file uses it.

The member is deliberately not added to the existing IOlObjects interface, because that would force
edits to test stubs owned by a concurrently running sibling work item. The research enumerated the
affected stubs: an IOlObjects stub in the UtilitiesCS store controller tests and three
IApplicationGlobals stubs in the QuickFiler test project.

Project reference direction is one-way, from TaskMaster to UtilitiesCS, and is preserved: the
interface is declared in UtilitiesCS and implemented in TaskMaster, so UtilitiesCS gains no reference
to TaskMaster. This is the same shape as the existing store disable and store rehook service
interfaces already in that folder.

AC5 also permits removing the double-persistence path entirely. That is the larger behavioural
change — the settings values are read back by the globals junk-folder accessors — so the typed seam
plus a loud failure is the chosen reading. The divergence itself is mitigated because the JSON
mechanism will begin writing once AC1 lands, and the loud failure removes the silent path.

### D3 — AC4 is a synchronous flush on the explicit Save path

AC4 is satisfied by a synchronous flush on the explicit Save path, not by a new add-in shutdown
handler. The deferred three-second behaviour for all other callers remains unchanged. The VSTO
add-in lifecycle file is deliberately not modified.

Supporting facts from the research: the shutdown handler in the add-in lifecycle file carries the
stock VSTO comment stating that Outlook no longer raises the event, verified present at the base
commit, so a flush placed there would never run; the designer-generated shutdown override must not be
hand-edited and is downstream of the same unraised event.

The thread-safe write method is already public on the serializer, takes the write lock, writes
through the injectable stream-writer seam, and re-arms the single-shot guard in its finally block.
Two facts must be respected by the implementation. First, that method requires a non-null parent
reference; the stores wrapper sets it in both constructors, so the guard is satisfied on both the
deserialized and fresh-built models. Second, AC2's empty-or-null-path error must be evaluated before
any synchronous write, so that the fix does not substitute one silent failure for another. The
implementation therefore introduces a small guarded entry point rather than calling the thread-safe
write method bare from the controller.

Recorded mechanics of the deferred path, which must not regress: the timer is a single-shot
three-second timer produced by an injectable factory; repeat calls inside the window coalesce rather
than reset, so the captured file path is the first caller's; the guard is only re-armed in the write
method's finally block.

### D4 — the controller is split into a display partial

UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs is 478 lines against a 500-line repository
cap, and four acceptance criteria land in it. It is split into a new partial at
UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs carrying the display and rendering
members — PopulateWithCurrent, the exclude-store checkbox binding, GetRelativeFsPath, and the new
double-backslash trim helper. The class declaration gains the partial keyword. The application
globals partials are the in-repo precedent for this layout.

The AC7 trim helper is a small pure private static method on the display partial rather than an
extension of the shared archive-stem contract type, which is a filing-boundary contract with its own
test suite consumed by other work items. The research verified that no existing helper in UtilitiesCS
performs a plain store-prefix trim: the closest behaviours are a navigation-local substring, a
trim-start inside a stem calculation, and root-relative operations that return false rather than
passing the input through.

### D5 — the serializer's pre-existing size violation is accepted, not fixed

UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs is already 613 lines, over
the same 500-line cap, before any change here. This change adds a small amount to it and does not
split it. Rationale: splitting a shared reusable-type-classes file during a parallel run would create
merge contention with concurrently running sibling work items, and the split is a separable concern
that should be raised on its own rather than folded into a bug fix. This change does not resolve that
pre-existing violation. The corresponding Non-Goals entry records the same statement.

### D6 — one existing test expectation is deliberately inverted

An existing test named PopulateWithCurrent_NullCurrent_SetsErrorLoadingText, in
UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.ButtonAndPopulate.cs at lines
123-135, currently asserts that a null current store throws a NullReferenceException. The assertion
codifies the bug while the test name describes the fixed behaviour, so the test contradicts its own
name. AC8 requires that behaviour to change, so that test must be inverted to assert that the
placeholder text renders.

This is called out here explicitly as a deliberate, declared test-expectation change. The General
Code Change Policy treats existing tests as part of the spec, so a reviewer must not mistake this
inversion for a weakened test. The inverted test asserts a stricter outcome than the original: it
requires a specific rendered value rather than merely an exception type.

A second test-double retarget is required and is not an expectation weakening: the reflection-era
doubles in the store controller tests are retargeted to the typed sink. The negative test that
asserts the missing-implementation path does not throw is retargeted, not deleted, so the loud-failure
branch introduced by D2 retains coverage.

### D7 — no resource file change is required

The resource entry that defines the settings file name and its AppData special folder already carries
the correct values: the file name StoresWrapper.json, an AppData special folder that resolves to the
TaskMaster subdirectory of the local application data folder, and a matching local-disk entry. The
defect is that the value is discarded at runtime, not that it is wrong. No resource file is modified.

---

## Acceptance Criteria

The eight criteria below are reproduced verbatim from issue.md, where they were settled with the
maintainer on 2026-09-06. They are the same criteria, not additional ones. They are not renumbered,
reordered, dropped, merged, split or reworded. Check-off must be mirrored in issue.md; this file is
the authoritative source under full-bug work mode.

- [ ] AC1: When `StoresWrapper.json` is absent, the fresh-build path adopts the resource-defined disk configuration so `Config.Disk.FilePath` resolves to `%LocalAppData%\TaskMaster\StoresWrapper.json`, and the first Save creates the file.
- [ ] AC2: `SmartSerializable<T>.Serialize()` logs an error (not a silent return) when invoked with an empty or null `Config.Disk.FilePath`.
- [ ] AC3: A value saved in Folder Settings is present after an Outlook restart (manual verification).
- [ ] AC4: An explicit Save is not lost if Outlook closes within the 3-second deferred-write window (flush on save or on shutdown).
- [ ] AC5: The junk-folder double-persistence path is either removed or made to fail loudly; the reflection lookup is replaced by a typed seam.
- [ ] AC6: User Email shows the SMTP address; on lookup failure it shows a specific message including the reason, falls back to an alternative source (the account SMTP address or the store display name when it is an SMTP address), and the lookup is retried when the dialog opens.
- [ ] AC7: Inbox and Root Folder are displayed without the leading `\\` (cosmetic).
- [ ] AC8: A null `Current` store selection renders the placeholder text instead of throwing.

### Verification detail per criterion

This subsection adds detail only. The checkbox text above is the authoritative wording.

- **AC1 detail.** Satisfied at the store-loading partial per D1. "Adopts the resource-defined disk
  configuration" means the freshly built wrapper's configuration is copied from the loader already
  resolved from the intelligence-resources configuration dictionary, which the research verified
  already carries the correct materialised path. The key-absent branch is out of AC1's scope and
  remains a fresh build with an empty path, made visible by AC2.
- **AC2 detail.** Both the empty-string case and the null case must log at error level. The research
  established that the null case is reachable because the existing guard compares only against the
  empty string and the path helper can assign null. The sibling base-class method already uses a
  null-or-empty check and is the consistent shape to converge on.
- **AC3 detail.** Manual verification by Outlook restart. Automated coverage is not achievable
  without a live Outlook process and is not attempted.
- **AC4 detail.** "Not lost" is demonstrated by the explicit Save path writing without the deferred
  timer firing. The deferred path for all other callers must still require a timer fire; both
  assertions live in the same test file so that the unchanged-behaviour claim is directly evidenced.
- **AC5 detail.** "Fail loudly" is an error-level log on the failed cast, per D2. "Typed seam" is the
  new interface, per D2. Argument order must be pinned by a test, because today the order is enforced
  by nothing except positional agreement between the call site and the method signature.
- **AC6 detail.** Fallback order: the Exchange primary SMTP address; then the address entry's address
  when it contains an at-sign; then the store display name when it contains an at-sign; and finally,
  at the controller, a specific unavailability message carrying the caught exception's message in
  place of the generic placeholder. The first two steps mirror an existing in-repo helper in the
  application globals that already implements exactly this ordering with per-step exception handling.
  The retry site is PopulateWithCurrent, which the research verified is the single method that runs
  both when the dialog opens and on every store re-selection, and which already marshals to the UI
  thread at its top. The retry is attempted at most once per dialog open and only when the address is
  null. The blocking-latency limitation is recorded in Non-Goals item 8 and in Risks.
- **AC7 detail.** A pure trim helper on the display partial, plus the two label assignments. The
  research verified that no existing test pins the untrimmed form, so this adds coverage rather than
  changing an expectation.
- **AC8 detail.** The four unguarded dereferences at the top of PopulateWithCurrent and the one in
  GetRelativeFsPath are guarded so the existing placeholder literals render. Under AC6 the user-email
  literal becomes the new specific unavailability message; the remaining placeholder literals for
  Inbox, Root Folder, the two archive fields and the two junk fields are unchanged. The deliberate
  test inversion is D6.

---

## Verification and Test Strategy

### Test policy

C# tests use MSTest, with Moq for mocking and FluentAssertions for assertions, per the C# Unit Test
Policy. Tests follow Arrange-Act-Assert with descriptive names. The repository prohibits creating
temporary files in tests, so the serializer tests drive the existing injectable seams the research
identified rather than writing to disk: the read-all-text seam, the disk-exists seam, the
stream-writer seam, the dialog seam, and the timer factory. All five are already exposed to tests by
an established harness in the UtilitiesCS test project, and a manual-fire timer double already
exists. No new production seam is required for AC1, AC3, AC5, AC6, AC7 or AC8.

The one genuine gap is AC2. The research found that the serializer's logger is a private static
log4net logger and is not injectable. The recommended assertion route is the in-memory appender
pattern already used elsewhere in the test suite, in the TaskMaster test project's startup-timing and
app-events helper files. That route adds no production surface to an already over-cap shared file, and
the UtilitiesCS test project already has a direct log4net reference, so the same helper compiles
there. The appender must be detached in a finally block so tests remain independent.

Banned in tests: Thread.Sleep, Task.Delay, real wall-clock waits, and temporary files.

### Criterion-to-evidence map

| AC | Test location | What is asserted | Evidence |
|---|---|---|---|
| AC1 | TaskMaster test project, AppGlobals coverage tests (or a new file there) | Given a loader whose disk path is a fake AppData path and a deserialize stub returning null, the freshly built wrapper's disk path equals the loader's path after the load completes. Negative case: configuration key absent, fresh build, path remains empty. No filesystem access. | vstest results, coverage report |
| AC2 | New serializer guard test file in the UtilitiesCS test project | In-memory log4net appender attached to the serializer's logger; Serialize with an empty path and, separately, with a null path each produce exactly one error-level event; no timer is armed in either case. | vstest results, coverage report |
| AC3 | Manual only | Fresh profile with no settings file; save an archive root; restart Outlook; reopen the dialog and confirm the value is present, the file exists under the local AppData TaskMaster directory, and the log contains no serializer error. | Manual verification note recorded under this feature folder's evidence directory, in the canonical other subdirectory. The evidence conventions define exactly the kinds baseline, regression-testing, qa-gates, issue-updates, other and remediation-baseline; there is no manual kind, and manual verification notes belong under other. |
| AC4 | New serializer guard test file in the UtilitiesCS test project | With a manual-fire timer injected and a memory-stream-backed writer, the explicit-save path writes without the timer firing; the pre-existing deferred path still requires a timer fire. Both in one file, so the unchanged-behaviour claim is evidenced directly. | vstest results, coverage report |
| AC5 | Store controller tests in the UtilitiesCS test project | A double implementing the new sink records both arguments and their order; a second double implementing only the globals interface drives the failed-cast branch and asserts an error-level log and no throw. The existing missing-implementation test is retargeted, not deleted. | vstest results, coverage report |
| AC6 | Store wrapper tests plus the new controller display test partial, both in the UtilitiesCS test project | Table-driven over the mocked Outlook folder chain already built by an existing helper: primary SMTP present; primary SMTP throws and the address entry address contains an at-sign; both fail and the display name contains an at-sign; all fail, producing a specific message containing the exception reason. Controller tests assert the retry runs on a second populate when the address is null and does not run when it is already populated. | vstest results, coverage report |
| AC7 | New controller display test partial | Pure-function cases over the trim helper: leading double backslash present, absent, single backslash, empty, null. Plus one populate test asserting the rendered Inbox and Root Folder label text. | vstest results, coverage report |
| AC8 | Existing button-and-populate test partial (inverted, per D6) plus the new display test partial | A null current store renders the placeholder text and does not throw; the relative-path helper likewise returns a placeholder rather than throwing. | vstest results, coverage report |

### Toolchain

Run in this exact order, and restart from the first step if any step fails or auto-fixes files:

1. CSharpier format:

```text
dotnet tool run csharpier format .
```

verified read-only with:

```text
dotnet tool run csharpier check .
```

2. Analyzer rebuild:

```text
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

3. Nullable rebuild:

```text
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

4. Test with coverage:

```text
vstest.console.exe <test-assembly-paths> /EnableCodeCoverage
```

Two toolchain constraints are mandatory. Always use the Rebuild target, never Build: MSBuild's
up-to-date check does not invalidate on a command-line property change, so a warm Build returns exit
zero with the compile target skipped and the gate cannot fail. Do not add the solution-wide nullable
enable property to step 3: it is deliberately absent from CI, no project in this repository carries a
nullable element, and forcing it conscripts every file that has never adopted the per-file pragma.

### Coverage expectations

Every changed member is reachable through an existing seam, so the new-code coverage target and the
no-regression-on-changed-lines rule are attainable without adding any coverage-exclusion attribute in
this change. No coverage exclusion is introduced.

### Evidence location

All evidence artifacts produced for this work item — build and QA gate output, regression results,
coverage reports, and the AC3 manual verification note — are written under this feature folder's
evidence directory, in a subdirectory named for the evidence kind, per the
evidence-and-timestamp-conventions skill. No evidence is written to a repository-level artifacts
directory.

---

## Risks and Regression Surface

### Regression surface of the AC1 fix

Because D1 makes no change to the shared deserialize overload, the behavioural regression surface of
that overload is empty. The research nonetheless enumerated it in full so a reviewer can confirm
that no alternative was taken. A set of in-repo forwarders reaches the overload, and the research
identified the terminal production entry points that actually exercise it at runtime as the
store-loading path — the intended fix site, which gains a post-fresh-build configuration copy at the
caller without any change to the overload's own behaviour — and the folder predictor load path, whose
documented fail-soft null contract is preserved exactly. Production entry points that use a different
overload are outside the surface entirely. The forwarder and entry-point tables in the research are
the enumeration of record; this specification does not restate their totals, because the research
supplies formal dual-derivation evidence only for its serializer-member and placeholder-literal
counts.

The research also listed the existing test callers that pin the current behaviour of that overload,
across the serializer, non-typed serializer, linked-list and stack test files in the UtilitiesCS test
project and the two application-globals test files in the TaskMaster test project. All of them must
continue to pass unchanged. Any failure among them indicates the shared overload was modified
contrary to D1.

### Risks

1. **UI-thread latency from the AC6 retry.** Adding a retry at dialog-open time reintroduces a
   synchronous Outlook COM property read on the UI thread, on a chain independently demonstrated
   capable of long blocks. Mitigation: retry at most once per dialog open and only when the address
   is null, which bounds the added latency to the same single lookup the startup path already
   performs. The residual risk is accepted because AC6 as written requires the retry. A non-blocking
   read is recorded as a follow-up, per Non-Goals item 8.
2. **Controller partial split.** Moving the display members to a new partial is a mechanical
   relocation, but the controller already has several existing test files whose fixtures reach the
   relocated members. Mitigation: relocate without behavioural edit
   first, then apply the AC5, AC6, AC7 and AC8 changes, so a failure is attributable to one or the
   other. Both the class declaration and the new partial must carry the partial keyword, and the new
   file needs a hand-added compile entry or it will silently not compile into the assembly.
3. **Missing compile entries.** Both new production files and both new test files require hand-added
   compile entries because every project is non-SDK-style. A missing entry produces a test file that
   silently does not exist rather than a build error, so the analyzer rebuild alone will not catch it.
   Mitigation: confirm the new test method names appear in the vstest run output.
4. **Junk-folder divergence during rollout.** Once AC1 lands, the per-store JSON mechanism begins
   writing for the first time on affected machines, while the .NET user settings already hold values
   written by the second mechanism. The two may disagree on a machine where junk folders were last
   selected under a non-default store, because one resolves relative paths against the selected
   store's root and the other against the default store's root. AC5 as scoped makes the failure loud
   rather than eliminating the second mechanism, so a first-run disagreement is possible. This is a
   known, accepted consequence of the chosen AC5 reading and should be checked during AC3 manual
   verification.
5. **Serializer file size.** The serializer file remains over the 500-line cap after this change. A
   reviewer applying the cap mechanically will flag it. The Non-Goals section and D5 record that this
   is pre-existing and deliberately not resolved here.
6. **Test-double fragility recorded by the research.** Two hazards apply to the test author: mocking
   task-bearing interfaces can throw a type-initialisation exception in this test binary because a
   task-extensions assembly is absent from the test output, a condition already documented in-repo;
   and an existing controller test depends on reference equality in the pairwise comparison helper.
   Neither is introduced by this change, but both can surface as apparent regressions.
7. **AC3 cannot be gated automatically.** Persistence across an Outlook restart is verified manually.
   The automated tests establish that the path is populated and that the write occurs through the
   seam, but they do not prove the file appears on disk in a live VSTO host.

### Unverified or explicitly bounded claims

- The Outlook-side cause of the COM failure in the SMTP chain is not determinable from the available
  log and is not claimed.
- The Outlook account collection is not read anywhere in the repository and the store wrapper holds
  no application reference, so an account-based SMTP fallback is recorded as unavailable rather than
  recommended.
- One alternative address source found on the store wrapper is populated only by a method with no
  caller anywhere in the repository, so it is not a usable fallback without new wiring and is not
  used.
- Whether the TaskMaster test project file ends the change modified depends on where the AC1 tests
  are placed; the Write Set claims it conservatively.
