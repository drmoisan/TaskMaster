# Research: unused `using Microsoft.Graph.*` directives and the stale CLAUDE.md `Directory.Build.props` clause (folded into #973)

- **Issue:** #973 (folded scope by maintainer direction, 2026-10-03)
- **Branch:** bug/remaining-stale-binding-redirect-pairs-973
- **Author:** task-researcher
- **Evidence tags:** `[V]` verified by tool in this session; `[I]` inference from verified facts; `[U]` unverified.

## Timestamp derivation

The Bash tool was disabled in this session, so `git var GIT_COMMITTER_IDENT` and `git log -1` could not be run. The most recent clock reading accessible was the worktree HEAD reflog (`.git/worktrees/<worktree-id>/logs/HEAD`, last line, and the branch reflog `.git/logs/refs/heads/bug/remaining-stale-binding-redirect-pairs-973`, last line): epoch `1791002501` with offset `-0400`, the commit "docs(973): record preflight clearance for plan revision 1.2". `1791002501 - 1767225600 (2026-01-01T00:00:00Z) = 23776901 s = 275 days + 04:41:41`, so the instant is 2026-10-03T04:41:41Z = 2026-10-03T00:41:41 local (-04:00), giving the filename stamp `2026-10-03T00-41` `[V]`. This matches the stamp of the artifact committed in that commit (`evidence/other/preflight-clearance.2026-10-03T00-41.md`). The stamp is a lower bound on the authoring time of this file: no later clock reading was available.

## 0. Summary

- **(A) All six `using Microsoft.Graph.*` directives in the five UtilitiesCS files are REMOVABLE.** No type name and no extension-method call in any of the five files binds through any of the six namespaces (section 1). Removal cannot silently re-bind anything; the only possible failure mode is a compile error, which both msbuild gates would surface (section 2). All five files are hand-written, carry `#nullable enable` on line 1, are compiled through explicit `Compile Include` items in `UtilitiesCS/UtilitiesCS.csproj`, and are CRLF (section 3).
- **The Microsoft.Graph package reference stays.** After the removals no `.cs` file in the repository names a `Microsoft.Graph` namespace, but the assembly References, packages.config entries and app.config redirects remain, and `UtilitiesCS.Test` reads `packages/Microsoft.Graph.*/lib/netstandard2.0/Microsoft.Graph.xml` as a large-file fixture (section 4). Not proposed for removal.
- **(B) CLAUDE.md line 211 is the only occurrence in CLAUDE.md** of the stale "there is no `Directory.Build.props`" clause. `Directory.Build.props` (sets only `RxUseUnsupportedPackagesConfig`, #730) and `Directory.Build.targets` (VSTO signing only) exist; no `.csproj`, `.props` or `.targets` anywhere contains `<Nullable`. A replacement sentence that keeps the conclusion is in section 5. Many other feature-folder documents repeat the stale clause; they are historical records of other items and this research recommends not editing them (section 5.4).
- **Toolchain:** CSharpier owns the five `.cs` files; IDE0005 and CS8019 are invisible to every gate in this repository (no `GenerateDocumentationFile`, `.editorconfig` severity ceiling), which is why the directives survived; coverage denominators do not move because `using` directives have no sequence points (section 6).
- **Spec/plan conflict:** spec.md Scope ("No C# source file changes"), AC16 ("no file with the .cs extension"), Rollout line 341, and plan P0-T15, P4-T4, P4-T10, P4-T12, P5-T16, D2 and section 5 "Explicitly not written: any `.cs` file" all contradict fold-in (A) and must be amended by the planner before execution (section 8).

## 1. Per-directive binding analysis (research question 1)

### 1.1 Method

For each file: every simple type name used in a type position and every extension-method call was enumerated by reading the file end to end, then checked against the public top-level types of the directive's namespace as documented in the shipped XML documentation of the referenced assembly. The referenced assembly is `Microsoft.Graph 6.7.0.0` with HintPath `..\packages\Microsoft.Graph.6.7.0\lib\netstandard2.0\Microsoft.Graph.dll` (`UtilitiesCS/UtilitiesCS.csproj:131-132`) `[V]`; the documentation read was `<primary-checkout>/packages/Microsoft.Graph.6.7.0/lib/netstandard2.0/Microsoft.Graph.xml` `[V]`. `Microsoft.Graph.Core 4.0.1` (`UtilitiesCS.csproj:135-136`) contributes zero types to `Microsoft.Graph.Models`, `Microsoft.Graph.Drives`, `Microsoft.Graph.Communications` or `Microsoft.Graph.Security` (Grep `"T:Microsoft\.Graph\.(Models|Drives|Communications|Security)\.` over `Microsoft.Graph.Core.xml` net462: 0 matches) `[V]`, so `Microsoft.Graph.dll` is the sole provider of all six namespaces.

A `using` directive makes two things visible: the namespace's top-level types (by simple name and arity) and the extension methods declared in its static classes. Nested types are never imported by a `using`. Extension methods live only in static classes; the XML documentation does not mark staticness, so two heuristics were applied: (i) type names ending in `Extension(s)`, `Helper` or `Utilities` in the namespace, and (ii) a direct search for any method in the namespace whose name matches an extension method the files call. The compile gates are the definitive proof (section 7).

### 1.2 `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs:7` -- `using Microsoft.Graph.Models.TermStore;` -- REMOVABLE

- Top-level documented types in the namespace (Grep `"T:Microsoft\.Graph\.Models\.TermStore\.[A-Za-z0-9_]+"`, declaration block at XML lines 116718-117096): `Group`, `GroupCollectionResponse`, `LocalizedDescription`, `LocalizedLabel`, `LocalizedName`, `Relation`, `RelationCollectionResponse`, `Set`, `SetCollectionResponse`, `Store`, `StoreCollectionResponse`, `Term`, `TermCollectionResponse` `[V]`. `RelationType` and `TermGroupScope` have no `T:` or `F:` entry (Grep: 0 matches) `[V]`; whether they exist undocumented is immaterial because neither name appears in the file.
- Simple type names used in the file: `Stopwatch` (35, 37, 64, 73, 82, 122, 128, 134, 222, 228, 234, 240), `COMException` (252, 270), `List<AddressEntry>` (177), `AddressEntry` (145, 177, 218), `Application` (141), `FolderMinimalWrapper` (296, 300, 302), `FilePathHelper` (298), `CurrentStoreContext` (62), `StoreWrapperInitClock` (91), `StoreWrapperInitProbe` (92), attributes `JsonProperty`/`JsonIgnore` (161-183), qualified `log4net.ILog`, `System.Exception`, `System.Threading.Thread`, and alias-qualified `Outlook.Store`, `Outlook.Folder`, `Outlook.OlExchangeStoreType`, `Outlook.OlDefaultFolders` `[V]`.
- Extension calls: `.Cast<AddressEntry>()`, `.ToList()` (145-146, System.Linq) `[V]`. `CurrentStoreContext.Begin` (62) and `StoreWrapperInitClock.Add` (91) are static calls on repository types.
- Collision check: the namespace exports `Store`, and `using Microsoft.Office.Interop.Outlook;` (line 8) exports `Store` too; the file never uses the simple name `Store` (every use is `Outlook.Store`), which is why no CS0104 exists today; `Application` (141) exists in `Microsoft.Graph.Models` but not in `Microsoft.Graph.Models.TermStore`, so it binds uniquely to Outlook interop `[V]`.
- Result: none of the 13 names is used; no static class exists in the namespace by the name heuristics; no namespace method matches `Cast`, `ToList`, `Begin`, `Add` or `EmitLine` (Grep: 0) `[V]`. **REMOVABLE.**

### 1.3 `UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs:10` -- `using Microsoft.Graph.Models;` -- REMOVABLE (the maintainer's "may genuinely use" caveat does not hold)

- Simple type names used in the file: `Explorer` (55), `View` (62), `MailItem` (209-210, 251), `List<string>` (45), `HashSet<string>` (213), `TreeNode<string>` (123), `DASLFilterParser` (72), `Regex` (78, 91, 94, 123, 186-188), `MAPIFields` (183, static access), `MailItemHelper` (217, 256), `Task` (38, 192, 240), `CancellationToken` (192, 240), `string`, `char`, qualified `EmailIntelligence.Triage` (26, 31), `log4net.ILog`, `System.Exception` `[V]`.
- Extension calls: `Select`, `Last`, `ToArray` (47), `IsNullOrEmpty` (83, 87), `Cast<object>`, `Where`, `Cast<MailItem>`, `ToAsyncEnumerable` (207-211, 248-252), `WithCancellation` (215, 254), `First` (143, 160), `DeleteUdf` (264) `[V]`.
- Namespace check: Grep `"T:Microsoft\.Graph\.Models\.(Explorer|View|MailItem|TreeNode|DASLFilterParser|MAPIFields|MailItemHelper|Selection|Triage|Regex|HashSet|CancellationToken|Task)"` over the XML: **0 matches** `[V]`. `Microsoft.Graph.Models.List` exists (XML 55821) but is non-generic; `List<string>` requires arity 1, so the Graph type is never a candidate and no ambiguity exists (C# looks types up by name and arity) `[V][I]`. `Microsoft.Graph.Models.Application`, `.Folder`, `.Message`, `.Attachment`, `.Recipient`, `.Group`, `.OutlookItem`, `.KeyValuePair` exist but none of those simple names appears in this file `[V]`.
- Extension-method check: the only `*Extensions` static class in `Microsoft.Graph.Models` is `DateTimeTimeZoneExtensions` with `ToDateTime(DateTimeTimeZone)`, `ToDateTimeOffset(DateTimeTimeZone)` and five `ToDateTimeTimeZone(...)` overloads on `DateTime`/`DateTimeOffset` (XML 437-488) `[V]`; the file calls none. Grep for any `Microsoft.Graph.Models.*` method named `Select|Last|ToArray|IsNullOrEmpty|Cast|Where|ToAsyncEnumerable|WithCancellation|DeleteUdf|First|ToList|...`: **0 matches** `[V]`.
- Result: **REMOVABLE.** The maintainer's caveat that this file "may genuinely use Microsoft.Graph.Models" is not borne out: every Outlook-sounding name in it (`Explorer`, `View`, `MailItem`) is an Outlook interop type and has no `Microsoft.Graph.Models` counterpart.

### 1.4 `UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs:11-12` -- two request-builder namespaces -- both REMOVABLE

- Line 11 `Microsoft.Graph.Communications.OnlineMeetings.GetAllRecordingsmeetingOrganizerUserIdMeetingOrganizerUserIdWithStartDateTimeWithEndDateTime` exports exactly three top-level types: `...GetResponse`, `...RequestBuilder`, `...Response` (XML 176932-177047) `[V]`.
- Line 12 `Microsoft.Graph.Drives.Item.Items.Item.GetActivitiesByInterval` exports exactly three: `GetActivitiesByIntervalGetResponse`, `GetActivitiesByIntervalRequestBuilder`, `GetActivitiesByIntervalResponse` (XML 280919-281025) `[V]`.
- The file uses none of those six names: Grep `GetAllRecordings|GetActivitiesByInterval|LanguageDetails|SearchWithQ|TermStore|Graph\.Models` over `UtilitiesCS/**/*.cs` matches only the six `using` lines themselves `[V]`. Request-builder and response classes are instance classes (they are constructed with request adapters), so they host no extension methods `[I]`.
- Note: the file uses the simple name `OutlookItem` (488, 511, 513) and `Microsoft.Graph.Models.OutlookItem` exists (XML 69432); that is irrelevant here because this file does not import `Microsoft.Graph.Models`, and it is a reason never to "widen" any of these directives to `Microsoft.Graph.Models` `[V]`.
- Result: **both REMOVABLE.**

### 1.5 `UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs:18` -- `using Microsoft.Graph.Security.AttackSimulation.Trainings.Item.LanguageDetails;` -- REMOVABLE

- The namespace exports one top-level type, `LanguageDetailsRequestBuilder` (XML 819507-819572) `[V]`. The file never names it (Grep above) `[V]`. The file's `KeyValuePair<string, SmartSerializableLoader>` (86, 105) would collide only with `Microsoft.Graph.Models.KeyValuePair` (non-generic, different arity, and not imported here) `[V]`. **REMOVABLE.**

### 1.6 `UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs:6` -- `using Microsoft.Graph.Drives.Item.Items.Item.SearchWithQ;` -- REMOVABLE

- The namespace exports `SearchWithQRequestBuilder`, `SearchWithQGetResponse`, `SearchWithQResponse` (XML 280642, 282235-282342) `[V]`. The file never names any of them `[V]`. All Outlook types in the file are alias-qualified (`Outlook.MAPIFolder`, `Outlook.Folder`, `Outlook.NameSpace`, `Outlook.Store`) `[V]`. **REMOVABLE.**

### 1.7 Classification table

| # | File:line | Directive | Namespace top-level types | Binding identifiers in file | Class |
|---|---|---|---|---|---|
| 1 | StoreWrapper.cs:7 | Microsoft.Graph.Models.TermStore | 13 (Group, Set, Store, Term, Relation, Localized*, *CollectionResponse) | none | REMOVABLE |
| 2 | Triage_OlLogic.cs:10 | Microsoft.Graph.Models | thousands; 0 match the file's identifiers | none | REMOVABLE |
| 3 | CategoryClassifierGroup.cs:11 | ...GetAllRecordings...WithEndDateTime | 3 | none | REMOVABLE |
| 4 | CategoryClassifierGroup.cs:12 | ...Drives.Item.Items.Item.GetActivitiesByInterval | 3 | none | REMOVABLE |
| 5 | ManagerAsyncLazy.cs:18 | ...Security.AttackSimulation.Trainings.Item.LanguageDetails | 1 | none | REMOVABLE |
| 6 | FolderMinimalWrapper.cs:6 | ...Drives.Item.Items.Item.SearchWithQ | 3 | none | REMOVABLE |

No directive is USED, so no narrower `using` or alias is needed and nothing stays.

## 2. Ambiguity and overload-resolution effects of removal (research question 2)

- **No ambiguity can exist today.** A simple name that resolves to types in two imported namespaces of the same scope is CS0104, an error; the branch builds with 0 errors (plan P0-T16 baseline contract; the #825 gate logs recorded 0 warnings) `[V]`. All six directives sit in the same compilation-unit scope as the other `using` lines of their file, so there is no inner/outer scope interplay `[V]`.
- **Removal can only shrink candidate sets.** Type lookup consults enclosing-namespace members before `using`-imported types, so an imported Graph type never shadows a repository type; removing an import therefore either changes nothing (nothing bound through it) or produces CS0246/CS0103/CS0234 (it was the only binding). For extension methods, the candidate set at each scope is the union over the scope's `using` directives; removing a directive that contributed no applicable candidate (section 1) leaves overload resolution identical `[I]`.
- **Silent re-binding is impossible here**: it would require a name that today binds to a Graph type and after removal binds to a different type found elsewhere; section 1 shows no identifier binds to a Graph type at all `[V][I]`.

## 3. Compilation, generation and nullable status (research question 3)

| File | `Compile Include` (UtilitiesCS.csproj) | Generated? | `#nullable enable` | Lines / CR count |
|---|---|---|---|---|
| EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.cs | :610 | No (hand-written, issue comments) | line 1 | 539 / 539 |
| EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogic.cs | :616 | No | line 1 | 270 / 270 |
| EmailIntelligence\ClassifierGroups\ManagerAsyncLazy.cs | :704 | No | line 1 | 356 / 356 |
| OutlookObjects\Folder\FolderMinimalWrapper.cs | :738 | No | line 1 | 188 / 188 |
| OutlookObjects\Store\StoreWrapper.cs | :758 | No | line 1 | 306 / 306 |

All `[V]` (csproj Grep; Read of line 1; Grep `^` and `\r$` counts). Equal line and CR counts mean each file is CRLF with a terminated last line; the Edit tool must preserve that (plan convention D11 applies the same gate to configs). Because every file already carries `#nullable enable`, all five already participate in the `TreatWarningsAsErrors` gate; deleting a `using` line adds no nullable-flow effect.

Pre-existing observation, not introduced by this change: `CategoryClassifierGroup.cs` is 539 lines, above the 500-line file limit in CLAUDE.md General Code Change Policy section 4. The removal lowers it to 537; it remains above the limit. Splitting the file is outside a hygiene fold-in (Bugfix Workflow: "If you uncover deeper design problems, open a new issue instead of widening scope"). Record it as pre-existing in the evidence so the reviewer does not attribute it to this item.

## 4. Does UtilitiesCS still need the Microsoft.Graph package after the removals? (research question 4)

Recorded, not proposed for action:

- After the six deletions, **no `.cs` file in the repository names a `Microsoft.Graph` namespace**: Grep `Microsoft\.Graph` over `**/*.cs` today returns the six `using` lines plus four lines in `UtilitiesCS.Test/Extensions/AsyncSerialization_Tests.cs` (431, 446, 458, 471) that are a comment, a directory-name pattern, a file name and an error message, not namespace uses `[V]`.
- Remaining dependents of the package: `UtilitiesCS/UtilitiesCS.csproj:131-137` and `UtilitiesCS.Test/UtilitiesCS.Test.csproj:678-684` (References to Microsoft.Graph 6.7.0.0 and Microsoft.Graph.Core 4.0.1.0); `UtilitiesCS/packages.config:34-35` and `UtilitiesCS.Test/packages.config:37-38`; binding redirects for `Microsoft.Graph` at `UtilitiesCS/app.config:259` and `UtilitiesCS.Test/app.config:314` and for `Microsoft.Graph.Core` in 15 app.config files `[V]`.
- `UtilitiesCS.Test/Extensions/AsyncSerialization_Tests.cs:435-473` (`GetLargeTextFixture`) walks up from the test assembly directory to a `packages` folder and opens `Microsoft.Graph.*/lib/netstandard2.0/Microsoft.Graph.xml` as a read-only large-file fixture; this is a file-system dependency on the restored package folder, independent of any assembly reference `[V]`.
- `Microsoft.Graph.Core 4.0.1` is also the declared consumer that pins several of the #973 IdentityModel redirect versions (research 1 section 5.2, row "Microsoft.Graph.Core 4.0.1 (:35)") `[V]`.

Conclusion: the compile-time use of `Microsoft.Graph.dll` by UtilitiesCS becomes zero, but the package stays installed; removing it would be a separate dependency item touching csproj, packages.config, redirects and a test fixture.

## 5. CLAUDE.md bullet, the root build files, and other documents (research question 5)

### 5.1 Exact current text

`CLAUDE.md:211` (inside "C#1. Tooling & Baseline for C#", item 3 "Type Checking", under "Two properties of it are load-bearing and must not be 'restored'"):

```
     - **Do not add `/p:Nullable=enable`.** No project in this repository carries a `<Nullable>` element and there is no `Directory.Build.props`, so the property is a solution-wide opt-in that conscripts every file which has never adopted the pragma. Forcing it produced 195 errors in `UtilitiesCS.csproj` on 2026-08-10 against zero errors without it, and CI omits it deliberately. Removing it loses no enforcement over any file that has opted in.
```

Grep `Directory\.Build\.props|Nullable=enable` over CLAUDE.md returns only line 211 `[V]`, so the fix is a single-line edit.

### 5.2 Facts about the root build files

- `Directory.Build.props` exists at the repository root, 18 lines; its only property is `<RxUseUnsupportedPackagesConfig>true</RxUseUnsupportedPackagesConfig>` (`Directory.Build.props:15-17`), with a comment citing issue #730 (`:2-14`) `[V]`.
- `Directory.Build.targets` exists at the repository root, 30 lines; it sets `SignManifests`/`SignAssembly` false for the TaskMaster project when `CI == true` (`:10-13`) and a `SetTaskMasterManifestCert` target for developer builds (`:19-29`) `[V]`.
- Neither file contains `Nullable`, `NoWarn`, `TreatWarningsAsErrors` or `WarningsNotAsErrors` `[V]`.
- No `<Nullable` element exists in any `*.csproj`, `*.props`, `*.targets`, `*.vbproj` or `*.fsproj` under the worktree (Grep `<Nullable`, 0 matches; the stricter `<Nullable>` also 0) `[V]`. No csproj references either Directory.Build file explicitly (research 1 line 26); none needs to, because MSBuild auto-imports them `[V][I]`.

### 5.3 Proposed replacement (conclusion unchanged)

Replace the first sentence of line 211 only:

```
     - **Do not add `/p:Nullable=enable`.** No project in this repository carries a `<Nullable>` element, and neither root build file sets one (`Directory.Build.props` sets only `RxUseUnsupportedPackagesConfig`, issue #730; `Directory.Build.targets` only toggles VSTO signing for the TaskMaster project), so the property is a solution-wide opt-in that conscripts every file which has never adopted the pragma. Forcing it produced 195 errors in `UtilitiesCS.csproj` on 2026-08-10 against zero errors without it, and CI omits it deliberately. Removing it loses no enforcement over any file that has opted in.
```

The second and third sentences are unchanged. The bullet's conclusion (do not add the property; it is a solution-wide opt-in) is preserved; the premise is corrected from "no Directory.Build.props exists" to "no build file, root or project, sets `<Nullable>`".

### 5.4 Other documents repeating the stale claim

Search: Grep `(is|has|have|with|exists|carries) no \`?Directory\.Build\.props\`?|no \`?Directory\.Build\.props\`? exists|Directory\.Build\.props (does not|doesn't) exist` over `docs/**` excluding `**/evidence/**`, plus the broader `Directory\.Build\.props` Grep over `**/*.md` (250-line cap reached; the narrower pattern completed) `[V]`.

- **Owned guidance documents outside `.claude/`:** CLAUDE.md:211 is the only one. No `README`, `docs/*.md` guidance file or `.claude/rules/csharp.md` carries the claim (the latter is push-down owned and out of scope in any case) `[V]`.
- **Feature-folder documents under `docs/features/active/` (non-evidence) that assert the absence** `[V]`: 476 `spec.md:428` and `research/2026-08-24T00-45-...md:778`; 446 `plan.2026-08-24T09-37.md:104`; 498 `spec.md:869` and `research/2026-08-24T09-50-...md:1078`; 511 `remediation-plan.2026-08-23T20-57.md:49`, `plan.2026-08-21T18-10.md:84`, `issue.md:125`; 469 `plan.2026-08-29T12-22.md:68`; 648 `plan.2026-08-31T20-07.md:190`; 656 `spec.md:522`, `research/2026-08-31T20-15-...md:663`, `plan.2026-08-31T20-10.md:80`; 663 `spec.md:468`, `plan.2026-08-31T20-16.md:161`; 633 `plan.2026-08-31T19-35.md:48`; 637 `research/research.2026-08-29T12-30.md:725`; 678 `remediation-plan.2026-09-01T23-44.md:71`, `plan.2026-08-31T21-12.md:70`; 670 `spec.md:270`, `research/initializewebviewasync-fault-observation.2026-08-31T20-30.md:449`; 784-787-788-809 `plan.2026-09-07T20-14.md:58`; 801-805-812 `research/2026-09-07T23-45-...md:814`; 823 `plan.2026-09-08T23-50.md:20`, 821 `plan.2026-09-08T23-50.md:161`, 816 `plan.2026-09-12T13-23.md:179`, 782 `plan.2026-09-05T15-47.md:608` (these four reported as long lines; content not displayed, match presumed on the pattern) `[V]`. Archived folders (512, 449, 445, 394) also carry it but are archive by definition.
- **Documents that already state the correct fact** `[V]`: 826 `spec.md:825-827` and its research; 797 `spec.md:290`; 751 evidence; 872 research :247; 895 research :60; this feature's research 2 (`2026-10-02T22-53-...md:51`). Note that research 2's remark that research 1 section 2.1 is stale over-reads research 1 line 26, which says only that no csproj contains a `Directory.Build` reference (true; the import is automatic).

Recommendation: do not edit the other feature folders. Each is the frozen record of a different item's reasoning at its own time (several pre-date #730, which added the file on 2026-09-02); rewriting them would widen this item's write set across dozens of folders for no behavioural benefit, and the orchestrator memory note (`merge-invalidates-counts-and-universals-not-just-citations.md:49`) records a prior ruling that this cross-folder claim is not to be fixed on an item branch. CLAUDE.md is the one owned, loaded-every-session document, and it is the one the maintainer directed.

## 6. Toolchain impact (research question 6)

- **CSharpier.** The five `.cs` files are in scope of `dotnet tool run csharpier check .` (`.csharpierignore` excludes only `**/evidence/**`, coverage/trx documents, `*.csproj`, `*.props`, `*.targets`, `**/packages.config`, `**/app.config`) `[V]`. Each file's `using` block is already in System-first alphabetical order with the `Outlook =` alias last (StoreWrapper, FolderMinimalWrapper), so deleting one line from the middle preserves the ordering the formatter accepted at baseline `[V][I]`. The checked-file count does not change (no file added or removed), so plan P4-T4's "checked count equals P0-T15" clause still holds `[I]`.
- **MSBuild gates.** A removed-but-used directive would fail `CoreCompile` with CS0246 (type), CS0103 (name), CS0234 (namespace member) or CS1061 (extension method) in both the analyzer gate and the `TreatWarningsAsErrors` gate; these are errors independent of warning promotion `[I]`. Section 1 predicts zero such diagnostics; section 7 makes the prediction falsifiable.
- **IDE0005 / CS8019 state.** `.editorconfig` has no `IDE0005` entry and sets `dotnet_analyzer_diagnostic.severity = suggestion` (line 27) with `MSTEST0032` the only rule at `warning` (line 29) `[V]`; no `.csproj`, `.props` or `.targets` sets `GenerateDocumentationFile` (Grep count 0) `[V]`, and IDE0005 is reported in command-line builds only when that property is set. CS8019 is a hidden-severity compiler diagnostic. Consequence: the six directives are invisible to every gate today, which is how they survived, and the gates will not confirm their removal either; the Grep in section 7 is the only direct check.
- **Coverage.** `using` directives emit no IL sequence points, so the first-party `lines` denominator and every numerator are unchanged by the deletions; per-file line numbers in the Cobertura document shift up by one (two for CategoryClassifierGroup) but counts do not `[I]`. Spec AC14 branch A (equal denominators, 0.10-point tolerance) therefore still applies; the baseline must be captured before the `.cs` edits, as plan P0-T19 already requires for the config edits.
- **CLAUDE.md edit.** Markdown; no toolchain gate. A PreToolUse hook may guard CLAUDE.md; per the binding HOOKS rule a block is recorded and reported, not routed around.
- **Change-budget note.** Fold-in (A) touches five production `.cs` files (one or two deleted lines each, zero added). The `csharp-change-budget-router` skill counts production files; the orchestrator should record the routing consequence rather than have an executor discover it.

## 7. Verification approach that can fail (research question 7)

Each check is stated with its pre-edit (red) and post-edit (green) value so the executor observes it failing before the edit.

1. **Directive absence, per file.** Grep `^using Microsoft\.Graph` with `-n` over each of the five files: before, 1 match (2 in CategoryClassifierGroup.cs); after, 0. Repository-wide: Grep `^using Microsoft\.Graph[.;]` over `**/*.cs`: before 6 matches in 5 files; after 0 `[V]` (before values measured in this session).
2. **Diff shape, per file.** `git -C <execution-worktree-root> diff --numstat <PLAN-START-HEAD> -- <file>` must read `0 1` for StoreWrapper.cs, Triage_OlLogic.cs, ManagerAsyncLazy.cs, FolderMinimalWrapper.cs and `0 2` for CategoryClassifierGroup.cs; `git diff -- <file>` must show only the deleted `using` line(s). Line and CR counts after: 305/305, 269/269, 537/537, 355/355, 187/187 (CRLF preserved; any inequality is a line-ending regression).
3. **Compile proof.** Both CLAUDE.md Rebuild gates (plan CMD-REBUILD) exit 0 with `ERRORS: 0`, `SKIP_CORECOMPILE_LINES: 0` (a skipped compile cannot prove anything) and, added for this fold-in, `CS0246|CS0103|CS0104|CS0234|CS1061` line count 0 in the captured file log. Positive control: the log shows `CoreCompile` executing for `UtilitiesCS.csproj` (the project that owns the five files).
4. **Format gate.** `dotnet tool run csharpier check .` exit 0 with the same `Checked N files` count as the baseline.
5. **CLAUDE.md.** Grep `there is no \`Directory\.Build\.props\`` over CLAUDE.md: before 1 (line 211), after 0; Grep `neither root build file sets one` : before 0, after 1; the "195 errors" and "CI omits it deliberately" sentences remain on the same line (Grep `195 errors in \`UtilitiesCS.csproj\` on 2026-08-10` count 1 before and after); `git diff --numstat -- CLAUDE.md` reads `1 1`.
6. **No collateral.** `git diff --name-only <PLAN-START-HEAD> -- '*.cs'` lists exactly the five paths and nothing else; `git status --porcelain --untracked-files=all -- '*.cs'` empty after commit.

## 8. Spec and plan amendments required before execution

The folded scope contradicts the current spec and plan at these points (all `[V]` by Read/Grep in this session):

- spec.md:68 "No C# source file (any file with the .cs extension) changes"; spec.md:75 and :341 list the `using` removal as a follow-up; AC16 (spec.md:313) "The diff of this change contains no file with the .cs extension"; Write Set (spec.md:215-242) lacks the five `.cs` files and CLAUDE.md; Constraints (spec.md:252) "the C# toolchain is mandatory because csproj files change" (now also because `.cs` files change).
- plan.2026-10-02T22-16.md: section 5 line 86 "Explicitly not written: any `.cs` file"; D2 (line 28) "This change edits no .cs file"; P0-T15 (line 354) and P4-T4 (line 419) STOP texts "this item edits no `.cs` file" / "no `.cs` file may be edited"; P4-T10 (line 425) `CHANGED-PRODUCTION-LINES: 0` justified by an empty `git diff -- '*.cs'` (the figure stays 0 in the coverage sense, but the justification must become "five files, deletions of `using` lines only, no executable line"); P4-T12 (line 427) `.cs` must print nothing; P5-T16 (line 448); CMD-FOOTPRINT's Write Set union; D8 commit allocation (which of commits A/B/C carries the `.cs` and CLAUDE.md edits; a fourth commit or inclusion in commit C are both workable; CLAUDE.md and the five `.cs` paths are outside the #539 staging exemption, so the orchestrator checkpoint admits them as it does commits A and B).
- Proposed additional acceptance criteria for the planner (numeric claims backed by section 9): AC19 (six directives removed; section 7 checks 1-2), AC20 (compile and format proof; checks 3-4), AC21 (CLAUDE.md sentence; check 5), AC22 (no other `.cs` or document in the diff; check 6). The Bugfix Workflow's "failing test first" has no unit-test subject for either fold-in; the red-before observations are the Greps of section 7 (6 before / 0 after; 1 before / 0 after), which the executor records before editing.

## 9. Numeric Derivation Evidence

### Count A: directives to remove = 6, in 5 files

- **Complete Family:** every `using` directive in a compiled `.cs` file under the worktree whose namespace is `Microsoft.Graph` or a sub-namespace of it.
- **Exhaustive Search Scope:** all `*.cs` files under the worktree root (production and test; every project), excluding nothing.
- **Inclusion Rules:** a line beginning with `using Microsoft.Graph` followed by `.` or `;` (plain namespace directives; `using static` and alias forms would also start with `using` and name `Microsoft.Graph`, and are caught by the primary search).
- **Exclusion Rules:** comments, string literals and identifiers that merely contain the text `Microsoft.Graph` (the four lines in `UtilitiesCS.Test/Extensions/AsyncSerialization_Tests.cs:431,446,458,471`).
- **Primary Search Strategy or Query Expression:** Grep `Microsoft\.Graph`, glob `**/*.cs`, content mode, then classify each hit by reading it.
- **Primary Member Set:** StoreWrapper.cs:7; Triage_OlLogic.cs:10; CategoryClassifierGroup.cs:11; CategoryClassifierGroup.cs:12; ManagerAsyncLazy.cs:18; FolderMinimalWrapper.cs:6 (10 raw hits, 4 excluded by the exclusion rule).
- **Primary Count:** 6 directives, 5 files.
- **Cross-check Search Strategy or Query Expression:** Grep `^using Microsoft\.Graph[.;]`, type `cs`, content mode (anchored form, different pattern and file selector).
- **Cross-check Member Set:** StoreWrapper.cs:7; Triage_OlLogic.cs:10; FolderMinimalWrapper.cs:6; ManagerAsyncLazy.cs:18; CategoryClassifierGroup.cs:11; CategoryClassifierGroup.cs:12.
- **Cross-check Count:** 6 directives, 5 files.
- **Member-set Comparison:** identical after normalising order; the maintainer's list in the delegation prompt names the same six lines. The count is asserted.

### Count B: occurrences of the stale clause in CLAUDE.md = 1

- **Complete Family:** every line of CLAUDE.md asserting that `Directory.Build.props` does not exist.
- **Exhaustive Search Scope:** the whole of `CLAUDE.md`.
- **Inclusion Rules:** any mention of `Directory.Build.props`.
- **Exclusion Rules:** none.
- **Primary Search Strategy or Query Expression:** Grep `Directory\.Build\.props|Nullable=enable` over CLAUDE.md.
- **Primary Member Set:** CLAUDE.md:211.
- **Primary Count:** 1.
- **Cross-check Search Strategy or Query Expression:** Grep `(is|has|have|with|exists|carries) no \`?Directory\.Build\.props\`?|no \`?Directory\.Build\.props\`? exists|Directory\.Build\.props (does not|doesn't) exist` over `**/*.md` (the result set restricted to CLAUDE.md).
- **Cross-check Member Set:** CLAUDE.md:211.
- **Cross-check Count:** 1.
- **Member-set Comparison:** identical. The count is asserted.

### Count C: `<Nullable` elements in project and build files = 0

- **Complete Family:** every `<Nullable` element in any MSBuild project or build file under the worktree.
- **Exhaustive Search Scope:** glob `**/*.{csproj,props,targets,vbproj,fsproj}`.
- **Inclusion Rules:** the text `<Nullable` (covers `<Nullable>`, `<Nullable Condition=...>`).
- **Exclusion Rules:** none.
- **Primary Search Strategy or Query Expression:** Grep `<Nullable`, that glob, count mode.
- **Primary Member Set:** empty.
- **Primary Count:** 0.
- **Cross-check Search Strategy or Query Expression:** Grep `<Nullable>|Directory\.Build` over `**/*.{csproj,props,targets}` (the earlier, stricter pattern run first in this session).
- **Cross-check Member Set:** empty.
- **Cross-check Count:** 0.
- **Member-set Comparison:** both empty. The assertion "no project and neither root build file sets `<Nullable>`" is supported; the two root files were additionally read in full.

## 10. Candidate approaches and recommendation

- **(1) Delete the six directives outright; reword CLAUDE.md:211 in place.** Simplest; matches the maintainer's direction ("remove only after confirming", confirmed in section 1); verifiable by the Greps and the compile gates. **Recommended.**
- **(2) Replace each directive with a narrower alias or `using static`.** Only applicable to a USED directive; none is USED. Rejected.
- **(3) Promote IDE0005 so the build catches unused usings in future.** Requires `GenerateDocumentationFile` in 18 csproj plus a severity change under a protected ceiling (memory `console-out-rs0030-826`); far outside this item. Rejected; may be filed separately.
- **(4) Also rewrite the other feature-folder documents that repeat the stale clause.** Rejected for the reasons in section 5.4.

## 11. Testing implications

No new unit test is warranted: fold-in (A) changes no executable line and fold-in (B) is documentation. The fail-able checks are the section 7 Greps (observed red before the edit) and the existing C# gates (compile proof). The MSTest coverage run remains required by the spec for the csproj changes and doubles as the regression run for (A); the equal-denominator comparison in AC14 is the quantitative proof that no executable line was touched.

## Automation Feasibility

Fully automatable. Every step is an Edit-tool deletion or single-line replacement, a Grep count, a `git -C` diff/numstat, or one of the already-planned toolchain commands; no manual observation is needed. Constraint carried from this session: with the Bash tool disabled no `git` or `pwsh` command could be run here, so the executor must be launched non-isolated with `pwsh` available (plan C5), and the HOOKS rule applies to the CLAUDE.md edit.

## Rejected alternatives (brief)

Narrower usings/aliases (no USED directive); IDE0005 promotion (out of scope); cross-folder document sweep (frozen records, prior ruling against).
