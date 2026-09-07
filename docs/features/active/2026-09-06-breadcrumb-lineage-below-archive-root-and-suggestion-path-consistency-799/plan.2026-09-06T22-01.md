# 2026-09-06-breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency (Plan)

- **Issue:** #799
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-06T22-01
- **Status:** Ready for preflight — revision round 2 (orchestrator-adjudicated round 1 delta applied in place)
- **Version:** 1.2
- **Work Mode:** full-bug (resolved from `issue.md` line 12 and `spec.md` line 9)
- **Language in scope:** C# only (UtilitiesCS, QuickFiler, UtilitiesCS.Test, QuickFiler.Test; four legacy non-SDK projects with explicit Compile Include items and no globbing)
- **Authoritative AC source:** `spec.md`, section "Acceptance Criteria", lines 834-841, AC1 through AC8. There is no user-story.md in this feature folder and none is required.

**Fail-closed evidence rule:** Every baseline, regression, and QA artifact named by a task must exist with all required fields before that task may be checked off. A missing or field-incomplete artifact makes the outcome BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Each evidence-producing task names its exact artifact path. Work is not complete without the artifact.

---

## Plan-wide rules

**R1 — Evidence location (non-overridable).** Every evidence artifact is written under the canonical scheme
`<FEATURE>/evidence/<kind>/` with `<kind>` in `baseline`, `regression-testing`, `qa-gates`, `issue-updates`, `other`, where
`<FEATURE>` abbreviates the repository-relative feature folder
docs/features/active/2026-09-06-breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency-799.
No caller supplied a non-canonical evidence path, so no `EVIDENCE_LOCATION_OVERRIDE_REJECTED` record is required by this plan.
The Cobertura document written to artifacts\csharp\coverage.xml is a tool output document, not an evidence artifact:
.claude/hooks/enforce-evidence-locations.ps1 names artifacts/csharp/ as an explicitly permitted path at its line 26 and does not
list it among the forbidden prefixes at lines 64-77.

**R2 — Evidence artifact schema.** Every command-bearing task writes an artifact containing, at minimum, the literal field
lines `Timestamp:` (format `yyyy-MM-ddTHH-mm`), `Command:`, `EXIT_CODE:`, and `Output Summary:`. A task whose command is
expected to exit non-zero additionally writes `ExpectedExitCode: 1`.

**R3 — Evidence filename length and path hygiene.** This feature folder name is 84 characters, so every evidence FILENAME is
kept short (the form `p0-t2-base.md`). No artifact may contain an absolute host path or a host account name: replace a
repository root with `<repo-root>`, a user-profile segment with `<user>`, and a machine name with `<host>`. This applies to
tool stdout, MSBuild logs, stack traces, Cobertura `filename` values, and TRX content alike. TRX files carry `runUser` and
`computerName` attributes in mixed casing; never paste raw TRX content into an artifact, and record only parsed counter
values and fully qualified test names. The one deliberate exception is the vswhere-resolved vstest.console.exe path that
[P0-T7] is required to record, because pinning that path is the task's whole purpose.

**R4 — Token-assertion case rule.** Every token-presence or token-absence assertion in this plan is case-sensitive.
Use `Select-String -CaseSensitive -SimpleMatch` or `git grep` without `-i`. PowerShell `-match` and a bare `Select-String`
are case-insensitive and must not be used for these gates.

**R5 — Named tests before phrase searches.** Where an acceptance condition can be carried by a named MSTest method, the
condition is stated as that method passing. Phrase searches are used only where no test can express the condition, and
every such literal is quoted verbatim in this document outside its command span.

**R6 — Base reference.** [P0-T2] records `BASE-SHA` (the commit at plan start) into `<FEATURE>/evidence/baseline/p0-t2-base.md`.
Every later `git diff` in this plan uses that recorded value as its ref operand. No SHA is pinned as a literal expectation
in this document.

**R7 — Scope pathspec.** The spec's Write Set is the change footprint, but this plan is additionally required to write
evidence artifacts under `<FEATURE>/evidence/` and to check off AC boxes in `spec.md`. Every scope-boundary gate in this plan
is therefore evaluated over the source pathspec `'*.cs' '*.csproj'` only, carries a `git add --intent-to-add` companion so
newly created files are visible to an anchored diff, and carries a `git status --porcelain --untracked-files=all` companion
because neither mechanism alone is correct in both states: an anchored diff cannot see an untracked file, and porcelain
status goes empty once the change is committed.

**R8 — 500-line ceiling, `.cs` only, with three disclosed pre-existing violations.**
.claude/rules/general-code-change.md caps production code, test code and reusable script files at 500 lines. It does not
reach project files: `.csharpierignore` lines 9-14 record that project files are owned by Visual Studio and are not C#
source. Every ceiling assertion in this plan is therefore scoped to `.cs` paths, and every project-file count is recorded as
an exempt observation rather than asserted.

Three files this plan touches are ALREADY over the ceiling before any change, measured in this worktree in this pass:
`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` 1003 lines and `QuickFiler/Controllers/EfcFormController.cs` 1320
lines are in the Write Set; UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs is 1066 lines and is NOT touched
by this plan. These are disclosed as pre-existing, not repaired here, and are gated by a per-file budget rather than by the
ceiling — see D11.

**R9 — Hard ordering constraint at the ceiling.** `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is EXACTLY 500
lines today, at the ceiling and not near it. Its provider construction at lines 147-149 must gain one argument. The
compensating relocation of the breadcrumb pipeline helper into the new partial
`QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs` is therefore sequenced BEFORE the argument is added, by
[P2-T3] preceding [P2-T14]. Any other order puts the file at 501 lines in an intermediate state. The count is re-measured
after the final CSharpier pass by [P3-T10], because the formatter can change line counts.

**R10 — MSBuild command forms.** The two gate builds use exactly the CLAUDE.md commands, with `/t:Rebuild` and without
`/p:Nullable=enable`. Iterative builds inside Phases 1 and 2 use `/t:Build` with no `/p:` gate switches; those builds exist
to produce test assemblies, not to run gates, and every source edit changes a timestamp so `CoreCompile` is not skipped.
A project-file build, if ever needed, must use `/p:Platform=AnyCPU`; the quoted `"/p:Platform=Any CPU"` form is a
solution-level alias only.

**R11 — Shell-variable re-binding (non-optional).** No variable survives between tasks: every command block runs in its own
shell. A block that uses `$vstest` must be preceded, in that same block, by the two resolution lines that [P0-T7] pins. A
block that uses `$BaseSha` must be preceded, in that same block, by a binding that resolves to the 40-hexadecimal value
[P0-T2] recorded as `BASE-SHA`, with no placeholder token left in the command. An unbound `$BaseSha` degrades an anchored
`git diff --name-only` into the ref-less form, which compares the worktree against the index and passes vacuously once the
change is committed, so the binding is load-bearing rather than cosmetic. A block that runs a `dotnet` command must first
re-bind `DOTNET_ROOT` and `PATH` to the repository-local SDK, because this worktree has no host SDK that satisfies
`global.json`. The three preambles are:

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
$BaseSha = (Select-String -Path 'docs\features\active\2026-09-06-breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency-799\evidence\baseline\p0-t2-base.md' -CaseSensitive -Pattern '^BASE-SHA: ([0-9a-f]{40})$').Matches[0].Groups[1].Value
```

The `$BaseSha` binding reads the recorded value out of the [P0-T2] artifact rather than carrying a hand-typed literal. That
substitutes the recorded value exactly, leaves no placeholder in the plan text, and fails loudly if [P0-T2] never ran or
recorded a malformed value. An executor that prefers to paste the recorded 40-hexadecimal value directly after [P0-T2] has
run satisfies this rule equally. Each affected task's `Output Summary:` records the resolved vstest path reduced per R3 and
the `$BaseSha` value it bound.

**R11b — Execution-environment clause.** Every command block in this plan is a PowerShell block and
requires a session in which `pwsh` may be invoked. A worktree-isolated agent session refuses every
Bash invocation of `pwsh`, in both the `-Command` and the `-File` form. The executor records, in
`<FEATURE>/evidence/baseline/p0-t3-sdk.md`, the derived line `EXEC-ENVIRONMENT: pwsh-permitted` once
it has confirmed a PowerShell block runs. An executor that cannot obtain such a session reports
BLOCKED at that task and stops; it must not substitute an unrecorded command shape for a documented
one, because a substituted shape is unreviewed and its success-case output is unobserved.

**R12 — Path-notation rule for this document.** Backticks in this plan are reserved for (a) the concrete repository paths
this change creates or modifies, taken from the `spec.md` Write Set and written with forward slashes; (b) bare filenames
carrying no directory separator, which a path extractor cannot classify as repository paths; and (c) code identifiers.
Every other file reference — comparisons, precedents, citations, sibling-owned files and out-of-scope files — is written as
bare prose with no backticks, including its File.cs:123 line citation. Inside fenced command blocks, Windows backslash path
forms are used wherever a forward-slash form is not required by git, so those spans are inert to the extractor. This is a
scheduling requirement, not a style preference: Get-BlastRadius harvests backtick-delimited path tokens from both the plan
and the spec and treats every accepted token as a write claim, with no notion of polarity, so a backticked path inside a
sentence saying the change will not touch it would still serialize this item against a concurrent sibling.

**R13 — Suite selection excludes the environmentally-hanging shell-icon classes.** Four UtilitiesCS.Test classes that call
SHGetFileInfo stall vstest on this machine. Every multi-assembly run in this plan carries the same four
`FullyQualifiedName!~` clauses that the most recent completed C# bug plan used, and the exclusion is recorded in each
artifact so the reduced denominator is visible on both sides of every comparison.

---

## Decisions Record

**D1 — The AC1/AC2 trim is placed in the hierarchy provider, and that keeps this item's diff off all six sibling-owned
files.** The QuickFiler drop-down reaches the ancestor chain through the UtilitiesCS breadcrumb bridge router's
SetSuggestionsAsync, and the Efc list through the QuickFiler router's FetchChainAsync at
QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs:50-81; both call the provider's GetAncestorChainAsync, so one
change satisfies AC1 on both surfaces. The six files a concurrent sibling owns are, in bare prose:
UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs (489 lines),
UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.SearchPresentation.cs,
UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs,
UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Highlight.cs,
QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs, and QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs.
[P3-T11] asserts the final anchored diff contains no hunk in any of them.

**D2 — The archive root reaches the provider as an OPTIONAL SECOND CONSTRUCTOR PARAMETER of delegate type returning a
string, never as an eagerly read value.** IOlObjects.ArchiveRootPath throws InvalidOperationException when the root is
unresolvable, and QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs:242 declares
BindBreadcrumbRowsAsync_WhenArchiveRootThrows_ReportsOnceAndDoesNotThrow precisely because of that throw. Reading the root
eagerly at construction would create a new throw site inside EfcFormController.ConfigureBreadcrumbControl (which this plan
edits at line 1053) and inside the relocated breadcrumb pipeline helper, neither of which is inside a try. Optional with a
null default keeps every existing provider construction compiling unchanged: 13 constructions in
UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs (lines 76, 98, 118, 139, 157, 175, 193, 218,
240, 269, 299, 316, 338) and 6 in UtilitiesCS.Test/OutlookObjects/Folder/FolderHierarchyProviderAdapterTests.cs (lines 98,
122, 142, 170, 185, 202), including the single-null-argument construction at OutlookFolderHierarchyProviderTests.cs:316,
which binds unambiguously to the first parameter because the type declares exactly one constructor.

**D3 — The accessor is stored in an internal get-only auto-property, not a private readonly field.** [P1-T4] adds the
parameter before [P2-T5] reads it. A `private readonly` field assigned and never read raises CS0414, which
`/p:TreatWarningsAsErrors=true` promotes to an error. An internal get-only auto-property has a compiler-generated backing
field read by its getter and raises no such warning, so the Phase 1 intermediate state is warning-clean. `internal` is
sufficient because only the declaring type reads it, and UtilitiesCS grants InternalsVisibleTo("UtilitiesCS.Test") at
UtilitiesCS/Properties/AssemblyInfo.cs:19.

**D4 — The AC7 absence classification is published through a NEW SMALL PUBLIC INTERFACE declared in the provider's own
file, not by adding a member to IFolderHierarchyProvider.** UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs
declares exactly three members at lines 31-63. net48 has no default interface members, so a fourth member would break every
implementer, and — decisively — every breadcrumb router test constructs `new Mock<IFolderHierarchyProvider>(MockBehavior.Strict)`,
so a strict mock would throw the first time production called the new member. Declaring
`IFolderLabelAbsenceReport` in `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` and having the Efc
router obtain it with an `as` cast in its existing constructor body means a strict mock simply is not an
`IFolderLabelAbsenceReport`, the field is null, and AC7 suppression is inert in every existing router test. Production is
unaffected: research and this pass both confirm no adapter wraps the provider — the two production constructions at
QuickFiler/Controllers/EfcFormController.cs:1053 and `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`:147 hand the
concrete provider straight to the router. The interface is public because QuickFiler consumes it; it lives in the
provider's file because it is a diagnostics view of exactly that type, and no new UtilitiesCS production file is in the
Write Set.

**D5 — ESCALATION BRANCH TAKEN: AC7 row suppression is delivered on the Efc surface only.** `spec.md` decision D-B requires
the planner to escalate rather than edit a sibling-owned file if the QuickFiler presented row set proves to be composed only
inside the sibling-owned bridge router. Re-derived in this pass: it is. The QuickFiler drop-down's row set is built as the
local `built` list inside FolderBreadcrumbBridgeRouter.SetSuggestionsAsync at
UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs:42-86, and swapped into the model under the shared lock at
:88-96. `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` only hands the predictor's row model to the viewer at
its lines 212 and 221; at that moment no provider resolution has been attempted, so the zero-candidate classification does
not yet exist there and could be produced only by adding a second, synchronous resolution pass on the UI thread inside
AssignFolderComboBox — which is both a duplicate of the router's work and a change to the very ordering the sibling owns.
The documented fallback is therefore taken: [P2-T12] delivers suppression on the Efc surface only, the QuickFiler surface
keeps today's fallback rendering, and [P3-T21] records the deviation under Rollout and Follow-up in `spec.md`. The AC7
logging half is delivered on BOTH surfaces, because it lives in the provider that both surfaces route through.

**D6 — The AC7 gate is two distinct per-instance structures, not one.** The log gate must never reset, or "once per label
per session" is violated; the suppression signal must reset, or a label that becomes resolvable after a snapshot refresh
stays suppressed forever. [P2-T6] therefore adds two `ConcurrentDictionary<string, byte>` fields with
`StringComparer.OrdinalIgnoreCase`: a reported-labels set that only ever gains entries and gates the `logger.Error`
emission through `TryAdd`, and an absent-labels set that gains an entry when the candidate count is zero and loses it the
moment the same path resolves. A bare `HashSet` is prohibited: ResolveLeafKeyAsync is async and awaits
AcquireSnapshotAsync, so its continuations are not guaranteed to run on one thread. A static set is prohibited by
`spec.md` decision D-B and by .claude/rules/general-unit-test.md, because it is process-wide mutable state shared across
viewers and across test methods in one assembly. Removal on success must be applied in ResolveLeafKeyAsync, not only in the
suffix pass, because the exact-path match at
`UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs`:74-77 returns before the suffix pass is reached.

**D7 — AC6 is additive: the projected score is ADDED alongside the raw score, never substituted for it.** The join key is
built by BreadcrumbRowBuilder.BuildProbabilityIndex at UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:210-229,
which assigns through the indexer at :224 rather than calling `Add`, so duplicate keys are tolerated and the last write
wins. A plain substitution would fix the stem-presented case but break the rooted-presented case, which
QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs:118-166
(Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively) exercises with `fullTarget` = the rooted path and a
score carrying the same rooted path: after a substitution the score key would be the stem while the presented text stayed
rooted, and the percentage would vanish. That test does not assert the percentage, so the regression would ship silently.
[P2-T11] therefore emits both entries and no existing key is ever removed. `BreadcrumbRowBuilder.cs` is NOT modified.

**D8 — The two #439 Efc router test files require NO HUNK, and that is a re-derived finding rather than an omission.**
`spec.md` lists `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` and
`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.Activation.cs` as MODIFY/retarget. Re-derived in this pass:
every test in both files constructs `new Mock<IFolderHierarchyProvider>(MockBehavior.Strict)` and supplies the ancestor
chain directly through `ReturnsAsync` (base file lines 30, 125, 178, 261, 311, 385 — six strict provider mocks, one per test in that file; Activation partial lines 21, 70, 118, 186).
The AC1/AC2 trim lives inside the provider's GetAncestorChainAsync, below that mock boundary, so it cannot reach either
file. What they pin is the router's faithful rendering of whatever chain the provider returns plus the #614 selection
boundaries, both of which this change preserves and both of which are the AC3 invariant the spec carries forward.
Editing them would also be actively harmful: the shared `Chain` helper at base-file lines 434-448 emits a leading
`\Archive` segment that three tests depend on for their own assertions, and
Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection asserts at line 426 that activating segment index 0 yields
`\Archive`, so removing that segment from the fixture would break a #614 boundary test that has nothing to do with this
change while pinning nothing new. [P3-T11] asserts the two files carry no hunk, and [P3-T21] records the disposition.
The retarget that IS load-bearing is the one that drives the real provider — see D9.

**D9 — The complete retargeting surface is two tests, both re-derived in this pass.**
(1) GetAncestorChainAsync_HappyPath_ReturnsRootToLeafSegments at
`UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs`:72-91 constructs the real provider and
asserts the full store-rooted chain `\Root`, `\Root\Clients`, `\Root\Clients\Acme` at lines 84-87. It would stay green
untouched only because it constructs the provider WITHOUT a root accessor, which is not how production constructs it;
leaving it that way would pin only the disabled configuration. [P1-T13] rewrites it against a provider configured with a
root accessor, as production configures it. The no-accessor companion case is NOT added to that file, because it has only
21 lines of headroom against the 500-line ceiling and a same-shape companion would consume all of it; the off switch is
pinned instead by GetAncestorChainAsync_WithoutRootAccessor_ReturnsTheUntrimmedChain in the new file
`UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTrimTests.cs` that [P1-T8] creates.
(2) ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection at
`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`:212-243 asserts at lines 219-226 that an EMPTY
archive root strips exactly one leading separator. That is the behaviour AC4 eliminates, so [P1-T14] rewrites the
assertion to the identity projection rather than preserving it.
A sweep of every other test that reads the affected members found no third obligation, and the reason is recorded per
family so a reviewer does not read a non-edit as an oversight: the recents fixtures in
UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs:250-265 and :166-189 and in
UtilitiesCS.Test/OutlookObjects/Folder/FolderRowTests.cs:29-70, :73-101 and :104 all use the RELATIVE recents value
`Recent\One` (and `Recent\Two`) against the root `\\ArchiveRoot`, so the AC5 projection is the identity for them and they
stay green; the two Issue609 suggestion-projection tests at FolderPredictorTests.cs:191-247 already encode exactly the
semantics ToDisplayStem reproduces (in-root full path to stem, relative unchanged, out-of-root full path unchanged,
case-variant to stem) and stay green; the GetOlSubpath assertions at FolderPredictorTests.cs:577-591 and at
ToDoModel.Test, directory Email Utilities, file FolderHandlerTests_Written.cs lines 35-67 both pass a root that is a strict
proper prefix terminated by a separator, which TryMakeArchiveRelative reproduces exactly, and the include-children-false
branch is not converted at all; and every other breadcrumb router test in QuickFiler.Test and UtilitiesCS.Test mocks
IFolderHierarchyProvider, so neither the trim nor the AC7 suppression reaches it (D4, D8).

**D10 — Persisted-corpus sites and the uncompiled sort utility are deliberately LEFT, per `spec.md` decision D-A sites 5,
6 and 7.** The folder minimal wrapper's relative-path loader at
UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs:56-86 and the folder wrapper source file's loader are unchanged,
because their full-path fallback is depended upon by the persisted relative-path restore branch and by the classifier
corpus, and the wrapper source file is already over the 500-line ceiling. The ToDoModel email-utilities sort file is
unchanged because it is not a Compile item in its project and has no live caller, so converting it would be a no-op unless
the file were first added to the build. Both space-containing paths are named in words only, and this item's footprint
therefore contains no space-containing path. Neither is in the Write Set and neither carries backticks anywhere in this
plan; the absence of backticks on those references is deliberate and must not be "corrected".

**D11 — File-size budgets, not a blanket ceiling gate.** Two Write Set files are already over the ceiling and one is
exactly at it, so a blanket "at or below 500" assertion would be unsatisfiable on the first and vacuous on the third.
[P3-T10] therefore asserts a per-file budget:
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` at or below 500 (hard, and reachable only because R9's relocation
runs first);
`QuickFiler/Controllers/EfcFormController.cs` at or below 1322, that is baseline 1320 plus at most two lines, because the
single added constructor argument is formatted by CSharpier as one additional line and the collapsed single-line call is
about 126 columns including indent, well past the print width — this file legitimately GROWS and a no-growth assertion on
it would be unsatisfiable;
`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` at or below 1003, that is no growth, which carries at least eight
lines of slack by derivation: the ProjectSuggestionPath body at lines 848-861 (14 lines) collapses to at most 5, the
include-children branch of GetOlSubpath at lines 955-965 (11 lines) collapses to at most 5, and the two recents projections
add at most 2 lines each;
every other `.cs` file this plan creates or edits at or below 500.

**D12 — `dotnet-coverage`, not `/EnableCodeCoverage`.** `vstest.console.exe /EnableCodeCoverage` writes a binary `.coverage`
file and the two collectors conflict, so every coverage run in this plan uses
`dotnet-coverage collect --output-format cobertura -- <vstest> ...`, exactly the form the most recent completed C# bug plan
used.

**D13 — Comparability, not a repository-wide rate.** The repository-wide Cobertura `line-rate` attribute is not a stable
gate on this harness. The coverage comparison in [P3-T8] is made on four first-party counters produced by ONE pinned
aggregation applied identically to both documents, with `lines-valid` comparability stated as an explicit precondition and
the derived percentages used instead when the denominators differ. The aggregation prints its own success-case output line,
whose exact form is `LINES_COVERED=<n> LINES_VALID=<n> BRANCHES_COVERED=<n> BRANCHES_VALID=<n> PACKAGES_MATCHED=<n>`, so
the values the acceptance conditions read are values the block definitively prints. `PACKAGES_MATCHED` exists so a
package-name mismatch surfaces as a loud zero instead of a silent zero-counter run.
The aggregation deliberately counts every `line` element under a matched package, which selects the class-level and the method-level elements alike and therefore over-counts the denominator relative to a de-duplicated per-line count. That is sound for the comparison it exists to make, because the identical method is applied to both documents, but the resulting percentage is a comparability index and NOT the repository line-coverage rate the 80 percent policy floor is defined over. [P0-T12] therefore records `BASELINE_FLOOR:` explicitly qualified as measured against the comparability index, and no task in this plan gates on it.

**D14 — Assembly discovery excludes worktree copies by construction.** The nine first-party test assemblies are named
explicitly on every run command. A path that is never enumerated cannot be loaded, so no worktree under a `.claude` segment
can enter a run.

---

## Write Set and disposition (restated from `spec.md`, lines 652-702)

| Path | Disposition |
|---|---|
| `UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs` | CREATE — the shared display projection (AC4, AC5, AC6) |
| `UtilitiesCS/OutlookObjects/Folder/ArchiveChainProjection.cs` | CREATE — the chain trim (AC1, AC2) |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` | MODIFY — 141 lines; trim, optional root accessor, AC2 error, AC7 gate and absence report |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | MODIFY — 1003 lines, pre-existing over-ceiling; AC4 delegation, AC5 recents, GetOlSubpath true branch |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | MODIFY — 312 lines; AC4 delegation only (D5) |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | MODIFY — 304 lines; AC6 score projection, AC7 Efc suppression |
| `QuickFiler/Controllers/EfcFormController.cs` | MODIFY — 1320 lines, pre-existing over-ceiling; one argument at line 1053 |
| `QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs` | CREATE — receives the relocated pipeline helper (R9) |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | MODIFY — exactly 500 lines; relocation out, then one argument |
| `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs` | CREATE |
| `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveChainProjectionTests.cs` | CREATE |
| `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTrimTests.cs` | CREATE |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorRecentsProjectionTests.cs` | CREATE |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs` | CREATE |
| `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` | MODIFY — 479 lines, budget +4 (ceiling 483, see [P1-T13]); retarget (D9) |
| `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs` | MODIFY — 354 lines; retarget (D9) |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` | NO HUNK — re-derived finding (D8) |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.Activation.cs` | NO HUNK — re-derived finding (D8) |
| `UtilitiesCS/UtilitiesCS.csproj` | MODIFY — two new production Compile Include entries |
| `QuickFiler/QuickFiler.csproj` | MODIFY — one new partial Compile Include entry |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | MODIFY — four new test Compile Include entries |
| `QuickFiler.Test/QuickFiler.Test.csproj` | MODIFY — one new test Compile Include entry |

Out of scope and absent from the Write Set, named in bare prose: anything under the dot-claude, dot-codex or dot-agents
trees; the two published JSON files under the config directory; every GitHub workflow file; the solution file; the
repository-root build property files; the six sibling-owned files named in D1; UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs;
UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs; UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotQueries.cs;
UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs; UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs;
the folder wrapper source file; and the ToDoModel email-utilities sort file.

---

## Citation table (re-derived against the current tree in this pass)

| Repository-relative path | Locator re-derived |
|---|---|
| UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs | 147 lines; three public members at :41, :68, :106; TryMakeArchiveRelative :106-145 with null/whitespace guard :113, trailing-separator trim :118, zero-length root :119-122, equality returning true with an empty stem :124-127, StartsWith prefix test :131, separator-boundary test :137-141, leading-separator trim on the stem :143 |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` | 141 lines; log4net ILog :17-19; `_treeService` :21; single constructor :28-31; GetAncestorChainAsync :34-42; GetImmediateSubfoldersAsync :45-53; ResolveLeafKeyAsync :56-80 with the exact-path early return :74-77; private static ResolveByUniqueSuffix :90-114 with the two-cause emission :108-112; AcquireSnapshotAsync :116-122; MapNodes :124-129; MapNode :131-139 |
| UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs | 65 lines; exactly three members, GetAncestorChainAsync :31-34, GetImmediateSubfoldersAsync :46-49, ResolveLeafKeyAsync :60-63 |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | 1003 lines; FolderRowArray text-parity XML doc :233-242 and getter :243-... ; AddRecents :788-795 with the unprojected AddRange :793; AddSuggestions :807-811 projecting at :810; AddSuggestionRows :835-846 projecting row text and score at :842-844; ProjectSuggestionPath :848-861 with the unconditional prefix concat :855 and the length guard :858; AddRecentRows :866-882 with the unprojected row construction :879; LoopFolders :903-951 with the ArchiveRootPath fallback :911-914 and the two GetOlSubpath calls :918 and :935; GetOlSubpath :953-971 with the include-children true branch :955-965 |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs | 238 lines; empty-chain single-segment fallback :123-131; presented-text probability lookup :133-135; Classify :152-170; MapSegments 1:1 with no trimming :178-208; BuildProbabilityIndex :210-229 keying on score.FolderPath through the INDEXER at :224, so duplicate keys are tolerated; LeafToken :231-236 |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbSegment.cs | 54 lines; four-argument constructor (key, displayName, folderPath, hasChildren) :29-40; FolderPath :49 |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs | 489 lines, sibling-owned; single-parameter constructor :19-23; SetSuggestionsAsync :29-97 composing the local `built` row list at :42-86 and swapping it under the shared lock at :88-96; SetSuggestionFallbacks :100-119; AddPlainRows :156-168; shared `_sync` :15 and `_suggestionGeneration` :16 |
| UtilitiesCS/Properties/AssemblyInfo.cs | InternalsVisibleTo("UtilitiesCS.Test") :19 |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 304 lines; log4net field named `log` :21-23; `_provider` :25; `_boundRoot` :35; five-argument constructor :41-56; public three-argument BindRowsAsync :75-82 forwarding `string.Empty` at :81; internal four-argument BindRowsAsync :92-150 with the bound-root normalization :107-109, the chain loop :110-130, the BuildRows call :132-136 and the AttachSegmentKeys call :137; ToHierarchyPath :152-167 consuming ArchiveStemContract at :157 and :164; AttachSegmentKeys :169-193 indexing presentedRows by row index at :176 |
| QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs | 221 lines; FetchChainAsync :50-81 returning null on a null key at :61-64 and on both catch arms at :68-80 |
| `QuickFiler/Controllers/EfcFormController.cs` | 1320 lines; ConfigureBreadcrumbControl :1047-1067 with the provider construction :1053-1055; BindBreadcrumbRowsAsync :1111-1128 with the raw score read :1115-1117 and the four-argument router call :1118 |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | EXACTLY 500 lines; usings :1-22; `internal partial class QfcItemController` :26; `_breadcrumbViewer` field :28; EnsureBreadcrumbPipeline comment :132-136, ExcludeFromCodeCoverage attribute :137, member :138-163, provider construction :147-149, arrow-event rewiring :153-162 |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | 312 lines; AssignFolderComboBox :191-250 with EnsureBreadcrumbPipeline :206, AddFolderItems :212, SetFolderSuggestions :221 and the projection call :231-234; the duplication rationale comment :223-230; ProjectPredeterminedFolder XML doc :252-271 and body :272-285 |
| `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` | 479 lines, 21 lines of headroom; namespace UtilitiesCS.Test.OutlookObjects.Folder :11; Archive fixture keys :36-70; GetAncestorChainAsync_HappyPath_ReturnsRootToLeafSegments :72-91 with the root-to-leaf assertion :84-87; 13 provider constructions at :76, 98, 118, 139, 157, 175, 193, 218, 240, 269, 299, 316, 338; single-null-argument construction :316; suffix-resolution tests :236-251 and :295-310 |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderHierarchyProviderAdapterTests.cs | 258 lines; 6 provider constructions at :98, 122, 142, 170, 185, 202 |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs | 1066 lines, pre-existing over-ceiling and NOT touched; FolderArray_WhenSuggestionsAndRecentsExist_ReturnsSuggestionsThenRecents :165-189 with the relative recent `Recent\One` :175; Issue609 projection tests :191-247; AddRecents_WhenRecentsExist_AppendsHeaderAndEntries :249-265 with relative recents :255; GetOlSubpath assertions :577-591 |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderRowTests.cs | text-parity test :29-70 with the relative recent :39; FolderRowArray_DoesNotAlterFolderArrayOutput :73-101; FindFolderRows parity :104; recents mock helper :248-255 |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` | 455 lines; namespace QuickFiler.Test.Controllers :12; six strict provider mocks :30, 125, 178, 261, 311, 385; six tests, one per mock; lineage test :20-116 asserting the archive-root index :109 and :113 and the 73 percent cell :114; rooted-target test :118-166 binding a rooted score at :149; boundary test :302-377 building an inline Archive segment :327; slash-only-root test :379-427 asserting segment-0 activation yields `\Archive` :426; shared Chain helper :434-448 emitting the leading Archive segment :444; Segment helper :450-453 |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.Activation.cs` | 253 lines; strict provider mocks :21, 70, 118, 186; four uses of the shared Chain helper :33, 82, 130, 196 |
| `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs` | 354 lines; namespace QuickFiler.Controllers.Tests :10; `public partial class QfcItemController_FolderHandlingTests` :21; ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection :212-243 with the empty-root one-separator assertion :219-226 and the case-insensitive assertion :239-242; the FolderContains-boundary empty-root test doc :245-258 |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue614Tests.cs | 358 lines; namespace QuickFiler.Test.Controllers :16; rooted/relative pair bind :254-259 |
| QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs | BindBreadcrumbRowsAsync_WhenArchiveRootThrows_ReportsOnceAndDoesNotThrow :242 |
| `UtilitiesCS/UtilitiesCS.csproj` | ArchiveStemContract.cs Compile Include :623; BreadcrumbRowBuilder.cs :625; OutlookFolderHierarchyProvider.cs :640; FolderPredictor.cs :808; the EnsureNuGetPackageBuildImports Error target :1293; Analyzer Include block :1301-1310 |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | folder-test Compile Include block :276-307, with ArchiveStemContractTests.cs :281, FolderHierarchyProviderAdapterTests.cs :302, FolderTreeSnapshotQueriesAncestorChainTests.cs :303 and OutlookFolderHierarchyProviderTests.cs :304; EnsureNuGetPackageBuildImports Error :946; Analyzer Include block :934-966 |
| `QuickFiler/QuickFiler.csproj` | BreadcrumbBridgeRouter.cs Compile Include :291; QfcItemController.ViewerSetup.cs :335; QfcItemController.FolderHandling.cs :338; EnsureNuGetPackageBuildImports Error :586; Analyzer Include block :592-601 |
| `QuickFiler.Test/QuickFiler.Test.csproj` | BreadcrumbBridgeRouterIssue439Tests.cs Compile Include :64; QfcItemController.FolderHandlingTests.Part2.cs :182; EnsureNuGetPackageBuildImports Error :501; Analyzer Include block :489-521 |
| `global.json` | SDK 8.0.205 pinned :3 with paths ".dotnet-sdk" and "$host$" :6-9 and the error message naming the repo-local install script :10 |
| `.csharpierignore` | evidence exclusion :4; cobertura :5; coverage :6; trx :8; project-file exclusion rationale :9-14 with `*.csproj` :12 |
| `.gitignore` | test-results bracket class `[Tt]est[Rr]esult*/` :39; `artifacts/` :57; `coverage/*` :144 |
| `coverage.config` | ModulePaths Exclude block :12-22 carrying no Test.dll entry, so the derived config appends one |
| .claude/hooks/enforce-evidence-locations.ps1 | artifacts/csharp/ named as permitted :26; forbidden prefixes :64-77 |
| `spec.md` | Acceptance Criteria AC1 through AC8 at lines 834-841; Write Set at lines 645-725; decision D-A at lines 283-338; decision D-B at lines 340-383; decision D-C at lines 385-407; decision D-D at lines 409-446; Test Strategy at lines 759-830 |

---

### Phase 0 — Baseline capture and toolchain bootstrap

This worktree is NOT bootstrapped. Neither the repository-local SDK tree nor the NuGet packages tree is present, so every
`dotnet` command and every `msbuild` invocation fails until [P0-T3] and [P0-T4] have run. A missing-packages MSBuild failure
must never be recorded as "the analyzer gate is already red at the merge base": each project declares an
EnsureNuGetPackageBuildImports target whose Error fires at BeforeTargets PrepareForBuild
(`UtilitiesCS/UtilitiesCS.csproj`:1293, `QuickFiler/QuickFiler.csproj`:586, `UtilitiesCS.Test/UtilitiesCS.Test.csproj`:946,
`QuickFiler.Test/QuickFiler.Test.csproj`:501), which is a bootstrap failure and nothing else.

- [ ] [P0-T1] Read, in the `policy-compliance-order` sequence, `CLAUDE.md`, .claude/rules/general-code-change.md, .claude/rules/general-unit-test.md, .claude/rules/csharp.md, and .claude/rules/tonality.md, then write `<FEATURE>/evidence/baseline/phase0-instructions-read.md` containing the literal field lines `Timestamp:`, `Policy Order:`, and an explicit list of the five files read with their line counts. Acceptance: the artifact exists and contains all five paths and the three field lines.

- [ ] [P0-T2] Record the branch and base commit into `<FEATURE>/evidence/baseline/p0-t2-base.md`, including the literal field lines `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, and the two derived lines `BASE-BRANCH: <name>` and `BASE-SHA: <40-hex>`. Acceptance: both derived lines are present and `BASE-SHA` is a 40-character lowercase hexadecimal value.

```powershell
git rev-parse --abbrev-ref HEAD
git rev-parse HEAD
git status --porcelain --untracked-files=all
```

- [ ] [P0-T3] Install the repository-local .NET SDK with scripts\vscode\Install-RepoDotNetSdk.ps1 and record `<FEATURE>/evidence/baseline/p0-t3-sdk.md`. `global.json` pins SDK 8.0.205 with the search paths ".dotnet-sdk" then "$host$", and no host SDK on this machine satisfies that pin, so every `dotnet` command fails until this task completes. Record the existence of the .dotnet-sdk directory BEFORE and AFTER the command, so the artifact is truthful whether the tree was absent or already present. Acceptance: after the command, the directory .dotnet-sdk\sdk\8.0.205 exists and `dotnet --version` prints a version beginning `8.0.`; the artifact records both the before/after existence booleans and the printed version.

```powershell
pwsh -NoProfile -File scripts\vscode\Install-RepoDotNetSdk.ps1
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
dotnet --version
Test-Path '.dotnet-sdk\sdk\8.0.205'
```

- [ ] [P0-T4] Restore NuGet packages for the solution and record `<FEATURE>/evidence/baseline/p0-t4-restore.md`. The packages tree is not present in this worktree, so this is a bootstrap step and not a repair. Record the count of packages subdirectories before and after the command, and the resolution status of every Analyzer Include HintPath declared by the four Write Set project files, because an unresolved analyzer path is CS0006, an error, and would fail [P0-T9] and [P0-T10] for a reason unrelated to this change. Analyzer version parity between the project files and packages.config was measured as clean before planning (Meziantou.Analyzer 3.0.203 and Roslynator.Analyzers 5.0.0 on both sides), so this probe is a verification step and no back-fill is planned. Acceptance: the artifact records the restore `EXIT_CODE:`, the before and after subdirectory counts, and one `RESOLVED:` or `UNRESOLVED:` line per analyzer path with zero `UNRESOLVED:` lines.

```powershell
$before = if (Test-Path 'packages') { (Get-ChildItem -Path 'packages' -Directory).Count } else { 0 }
msbuild TaskMaster.sln /t:Restore /m /p:RestorePackagesConfig=true /p:Configuration=Debug "/p:Platform=Any CPU"
$after = (Get-ChildItem -Path 'packages' -Directory).Count
"packages-subdirs before=$before after=$after"
foreach ($proj in @('UtilitiesCS\UtilitiesCS.csproj', 'QuickFiler\QuickFiler.csproj', 'UtilitiesCS.Test\UtilitiesCS.Test.csproj', 'QuickFiler.Test\QuickFiler.Test.csproj')) {
    [xml]$p = Get-Content -LiteralPath $proj
    foreach ($a in $p.SelectNodes('//*[local-name()="Analyzer"]')) {
        $hint = $a.GetAttribute('Include')
        $full = Join-Path (Split-Path -Parent $proj) $hint
        if (Test-Path -LiteralPath $full) { "RESOLVED: $hint" } else { "UNRESOLVED: $hint" }
    }
}
```

- [ ] [P0-T5] Restore the manifest-pinned dotnet tools and record `<FEATURE>/evidence/baseline/p0-t5-tools.md`. Acceptance: the artifact records `EXIT_CODE: 0` and the printed version string contains the substring 1.2.6.

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
dotnet tool restore
dotnet tool run csharpier --version
```

- [ ] [P0-T6] Resolve `dotnet-coverage` and record `<FEATURE>/evidence/baseline/p0-t6-dotnet-coverage.md`. Probe with `Get-Command dotnet-coverage -ErrorAction SilentlyContinue` rather than by running the tool, because an unresolvable command name raises a PowerShell CommandNotFoundException instead of setting a non-zero exit code, so there is no exit code for a branch condition to read. Only when the probe returns nothing, run `dotnet tool install --global dotnet-coverage`, then prepend the user-profile global-tool directory to PATH and re-probe; that prepend is required only on this branch, because a shell that was already running when the tool was installed does not inherit the new directory. On this host the probe branch is the expected one: the completed issue #791 run recorded the tool resolving with no PATH amendment. Acceptance: the artifact records a final `dotnet-coverage --version` invocation with `EXIT_CODE: 0` and the printed version string, states which of the two branches was taken, and records the derived line `DOTNET-COVERAGE-ON-PATH: true`.

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
$probe = Get-Command dotnet-coverage -ErrorAction SilentlyContinue
if ($null -eq $probe) {
    dotnet tool install --global dotnet-coverage
    $env:PATH = "$env:USERPROFILE\.dotnet\tools;$env:PATH"
}
dotnet-coverage --version
```

- [ ] [P0-T7] Resolve vstest.console.exe through vswhere and record the full resolved path into `<FEATURE>/evidence/baseline/p0-t7-vstest.md` as the derived line `VSTEST-PATH: <resolved path>`. This is the one artifact exempted from R3's path reduction, because pinning the resolved path is the task's purpose. Acceptance: `VSTEST-PATH` names an existing file and the artifact records `EXIT_CODE: 0`.

```powershell
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
$vstest
Test-Path $vstest
```

- [ ] [P0-T8] Capture the CSharpier baseline into `<FEATURE>/evidence/baseline/p0-t8-csharpier.md`, recording the verbatim printed line and the derived line `BASELINE-CSHARPIER-CHECKED-FILES: <N>`. The success-case output of this command on a clean tree is the single line of the form `Checked <N> files in <M>ms.` with exit 0; `check` is read-only and returns non-zero on drift, so the exit code is the gate here. If the check reports drift, the artifact must list every drifting path as a disclosed pre-existing set. Acceptance: the artifact records `EXIT_CODE:`, the printed line, and the `BASELINE-CSHARPIER-CHECKED-FILES` numeral.

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
dotnet tool run csharpier check .
```

- [ ] [P0-T9] Capture the analyzer-build baseline into `<FEATURE>/evidence/baseline/p0-t9-analyzers.md` using exactly the CLAUDE.md analyzer command. Acceptance: the artifact records `EXIT_CODE:` and an `Output Summary:` giving the warning and error counts read from the MSBuild summary, and states explicitly that [P0-T4] completed first so a bootstrap failure cannot be misread as an analyzer failure.

```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

- [ ] [P0-T10] Capture the nullable-build baseline into `<FEATURE>/evidence/baseline/p0-t10-nullable.md` using exactly the CLAUDE.md nullable command. `/p:Nullable=enable` must not be added and `/t:Build` must not be substituted. Acceptance: the artifact records `EXIT_CODE:` and the warning and error counts.

```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

- [ ] [P0-T11] Run the UtilitiesCS.Test and QuickFiler.Test assemblies and record their pass/fail counts into `<FEATURE>/evidence/baseline/p0-t11-suites.md` as the derived lines `BASELINE-UT-TOTAL:`, `BASELINE-UT-PASSED:`, `BASELINE-UT-FAILED:`, `BASELINE-QFT-TOTAL:`, `BASELINE-QFT-PASSED:`, `BASELINE-QFT-FAILED:`, read from the TRX `ResultSummary/Counters` element of each run. Do not paste TRX content (R3). Where a results directory holds more than one TRX because a task was re-run, the most recently modified TRX is the one read, and the artifact records which file it was by name reduced per R3. Acceptance: all six derived lines are present and numeric, each `BASELINE-*-FAILED` recorded whatever its value, and the artifact names the four excluded shell-icon classes (R13); the artifact additionally records the two derived lines `EXIT-CODE-UT:` and `EXIT-CODE-QFT:`, one per invocation, together with a single `EXIT_CODE:` field equal to the larger of the two so the artifact satisfies the evidence schema, and each `BASELINE-*-FAILED` value is read from its run's TRX `ResultSummary/Counters` `failed` attribute and NOT from the console, because vstest prints no `Failed:` line at all on a fully passing run.

```powershell
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\799-p0-t11-ut' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\799-p0-t11-qft' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook'
```

- [ ] [P0-T12] Run the full nine-assembly suite under `dotnet-coverage` and record `<FEATURE>/evidence/baseline/p0-t12-coverage.md` with the derived lines `BASELINE-LINES-COVERED:`, `BASELINE-LINES-VALID:`, `BASELINE-BRANCHES-COVERED:`, `BASELINE-BRANCHES-VALID:`, `BASELINE-PACKAGES-MATCHED:`, the two derived percentages, and `BASELINE-TOTAL-TESTS:` / `BASELINE-FAILED-TESTS:`. The four counters come from the pinned aggregation block below, whose success-case output is the single line it prints itself, of the form `LINES_COVERED=<n> LINES_VALID=<n> BRANCHES_COVERED=<n> BRANCHES_VALID=<n> PACKAGES_MATCHED=<n>` (D13). Record `BASELINE_FLOOR: MET` or `BASELINE_FLOOR: NOT MET` against the 80 percent line floor, explicitly qualified as measured against the D13 comparability index rather than against the repository line-coverage rate, and continue either way; a pre-existing repository floor never halts this plan. Where a results directory holds more than one TRX because a task was re-run, the most recently modified TRX is the one read, and the artifact records which file it was by name reduced per R3. Acceptance: the five `BASELINE-` counter lines are present and numeric and `BASELINE-PACKAGES-MATCHED` is greater than zero.

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
New-Item -ItemType Directory -Force -Path 'coverage' | Out-Null
$derived = 'coverage\799-effective-coverage.config'
[xml]$cfg = Get-Content -LiteralPath 'coverage.config'
$excl = $cfg.Configuration.CodeCoverage.ModulePaths.Exclude
$node = $cfg.CreateElement('ModulePath'); $node.InnerText = '.*\.Test\.dll$'
$null = $excl.AppendChild($node); $cfg.Save((Join-Path (Get-Location) $derived))
dotnet-coverage collect --output coverage\799-baseline.cobertura.xml --output-format cobertura --settings coverage\799-effective-coverage.config -- $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll SVGControl.Test\bin\Debug\SVGControl.Test.dll Tags.Test\bin\Debug\Tags.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskTree.Test\bin\Debug\TaskTree.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll ToDoModel.Test\bin\Debug\ToDoModel.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\799-p0-t12' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
$doc = New-Object System.Xml.XmlDocument
$doc.Load((Resolve-Path -LiteralPath 'coverage\799-baseline.cobertura.xml').Path)
$first = @('QuickFiler','SVGControl','Tags','TaskMaster','TaskTree','TaskVisualization','ToDoModel','UtilitiesCS','VBFunctions')
$lc=0;$lv=0;$bc=0;$bv=0;$pm=0
foreach ($pkg in $doc.SelectNodes('//package')) {
    $name = $pkg.GetAttribute('name')
    "PACKAGE: $name"
    if ($first -notcontains $name) { continue }
    $pm++
    foreach ($ln in $pkg.SelectNodes('.//line')) {
        $lv++
        if ([int]$ln.GetAttribute('hits') -gt 0) { $lc++ }
        $cc = $ln.GetAttribute('condition-coverage')
        if ($cc -match '\((\d+)/(\d+)\)') { $bc += [int]$Matches[1]; $bv += [int]$Matches[2] }
    }
}
"LINES_COVERED=$lc LINES_VALID=$lv BRANCHES_COVERED=$bc BRANCHES_VALID=$bv PACKAGES_MATCHED=$pm"
```

- [ ] [P0-T13] Determine, from coverage\799-baseline.cobertura.xml, which Write Set production files are measurable, and write `<FEATURE>/evidence/baseline/p0-t13-measurability.md`. For each of the six EXISTING Write Set production paths, and additionally for UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs, query for a `class` element whose `filename` attribute ends with a directory separator followed by that file's name, and record one line per file of the form `MEASURABLE: <path>` or `UNMEASURABLE: <path>`. The trailing-name match must be separator-anchored, because an unanchored suffix over-selects a sibling whose name merely ends with the same characters. The two files this plan CREATES are recorded separately as `NEW: <path>` and are measured for the first time by [P3-T9]. Acceptance: exactly seven `MEASURABLE:`/`UNMEASURABLE:` lines are present — one for each of the six EXISTING Write Set production paths, plus one for UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs, which this plan does not modify but whose measurability is recorded because [P2-T1], [P2-T2] and [P2-T9] all route through it and a zero-class-element result there would explain an otherwise puzzling [P3-T7] outcome — and the artifact records the class-element counts the determination was made from.

```powershell
$doc = New-Object System.Xml.XmlDocument
$doc.Load((Resolve-Path -LiteralPath 'coverage\799-baseline.cobertura.xml').Path)
$names = @('OutlookFolderHierarchyProvider.cs','FolderPredictor.cs','QfcItemController.FolderHandling.cs','BreadcrumbBridgeRouter.cs','EfcFormController.cs','QfcItemController.ViewerSetup.cs','ArchiveStemContract.cs')
foreach ($n in $names) {
    $hit = 0
    foreach ($c in $doc.SelectNodes('//class')) {
        $f = $c.GetAttribute('filename')
        if ($f.EndsWith('\' + $n) -or $f.EndsWith('/' + $n)) { $hit++ }
    }
    "$n classElements=$hit"
}
```

- [ ] [P0-T14] Record the baseline line count of every file this plan edits or creates into `<FEATURE>/evidence/baseline/p0-t14-sizes.md`, one `<path> = <count>` line per file, covering the six existing Write Set production paths, the two retargeted Write Set test paths, and the two #439 test paths D8 marks NO HUNK, plus a `CEILING: 500 (applies to *.cs only)` line. Record the four project files separately under a `PROJECT-FILE (exempt): <path> = <count>` heading with the R8 reason. Record the three pre-existing over-ceiling files under a `PRE-EXISTING OVER CEILING:` heading with the D11 budgets. Acceptance: every `.cs` path has a numeric count; the four project-file counts are recorded under the exempt heading with their reason; the artifact states that `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is exactly 500 and that `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` has 21 lines of headroom.

- [ ] [P0-T15] Record the pre-change status of the tests this plan retargets or must keep green into `<FEATURE>/evidence/baseline/p0-t15-tests.md`, one line per test of the form `BASELINE-PASS: <FullyQualifiedName>` or `BASELINE-FAIL: <FullyQualifiedName>`, derived from the two TRX documents [P0-T11] wrote. The set is: the two retarget targets named in D9 (GetAncestorChainAsync_HappyPath_ReturnsRootToLeafSegments and ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection); the recents and projection tests named in D9 that must stay green (FolderArray_WhenSuggestionsAndRecentsExist_ReturnsSuggestionsThenRecents, AddRecents_WhenRecentsExist_AppendsHeaderAndEntries, Issue609_FolderPredictor_ProjectsOnlyInRootFullSuggestionPaths, Issue609_FolderPredictor_ProjectsCaseVariantInRootFullSuggestionPath, FolderRowArray_WithSuggestionsAndRecents_MatchesFolderArrayTextAndTagsKinds, GetOlSubpath_WhenAncestorEndsWithSlashOrChildrenExcluded_ReturnsExpectedSegment); and the ten tests of the partial class `BreadcrumbBridgeRouterIssue439Tests`, across both files, which D8 marks NO HUNK (Issue439ArchiveRelativeRowsRenderLineagePreserveFilingTargetAndProbability, Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively, Issue439UnresolvedChainsUseSelectableFallbackForEveryDiagnosableProviderOutcome, Issue439InvalidTypedNavigationDoesNotSelectBannerOrPseudoRows, Issue439ArchiveRootBoundarySelectionAndHostEventRemainDeterministic, Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection, Issue609_DirectRowSelection_UsesFullLookupAndRelativeFilingTarget, Issue609_AncestorActivation_EmitsArchiveRelativeFilingTarget, Issue609_ImmediateChildActivation_EmitsArchiveRelativeFilingTarget and Issue439AncestorActivationQueriesAncestorKeyAndSelectsArchiveRelativeChild). The third of those is the one that pins today's null-chain selectable-fallback rendering, which is exactly the path [P2-T12] modifies, so omitting it would leave the modified path unguarded. This is the set that makes the Phase 2 no-newly-failing comparison meaningful. Acceptance: eighteen `BASELINE-PASS:` or `BASELINE-FAIL:` lines are present, one per named test, each derived from a TRX this plan wrote.

---

### Phase 1 — Declaration seams and failing regression tests

Phase 1 is test-first. Tasks [P1-T1] through [P1-T5] add only type-level declarations and their Compile entries, because the
new tests name types and members that do not exist yet and a missing declaration reddens the whole test assembly at compile
time rather than producing a targeted failure. No production behaviour changes in Phase 1: every seam body either throws
`NotImplementedException` or stores a value nothing reads yet.

- [ ] [P1-T1] Create `UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs` in namespace `UtilitiesCS.OutlookObjects.Folder`, with `#nullable enable`, declaring `public static class ArchiveStemProjection` and the single member `public static string? ToDisplayStem(string? folderPath, string? archiveRoot)` whose body is `throw new NotImplementedException("Issue #799: the display projection body is supplied by [P2-T1].");`. Both parameters and the return carry the `?` annotation because this file opens with `#nullable enable`, because [P1-T6] pins a null-path case, and because two of its three production callers — `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` and `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` — are themselves `#nullable enable` files whose argument expressions are nullable, so an unannotated parameter is CS8604 at those call sites under the [P3-T4] gate. The XML doc must state that this is a LENIENT DISPLAY projection returning the archive-relative stem when the path is strictly under the root and the input unchanged in every other case including an empty or whitespace root, and must state why it is a separate type rather than a member on the strict contract: that contract is a hard boundary that yields an empty string on failure and never passes its input through, and every display site needs the opposite fallback, so a lenient overload there would blur the invariant #614 created it for. Acceptance: the file compiles once [P1-T3] wires it, is at or below 500 lines, and a case-sensitive search of it finds the single-line token `ToDisplayStem`.

- [ ] [P1-T2] Create `UtilitiesCS/OutlookObjects/Folder/ArchiveChainProjection.cs` in namespace `UtilitiesCS.OutlookObjects.Folder`, with `#nullable enable`, declaring `public static class ArchiveChainProjection` and the single member `public static bool TryTrimBelowArchiveRoot(IReadOnlyList<FolderBreadcrumbSegment>? chain, string? archiveRoot, out IReadOnlyList<FolderBreadcrumbSegment> trimmed)` whose body assigns an empty array to `trimmed` and then is `throw new NotImplementedException("Issue #799: the chain trim body is supplied by [P2-T2].");`. The `chain` and `archiveRoot` parameters carry the `?` annotation because [P2-T2] specifies a false return for a null chain and for a null or whitespace root, and this file opens with `#nullable enable`; `trimmed` stays unannotated because it is assigned an empty array on every path before return. The XML doc must state that the archive-root node is the first chain index whose segment FolderPath is the root itself, detected as TryMakeArchiveRelative returning true with an empty stem, that the method returns the remainder of the chain after that index, and that it returns false when no such index exists and also when that index is the last one, because the leaf is then the root and there is nothing to render below it. Acceptance: the file compiles once [P1-T3] wires it, is at or below 500 lines, and a case-sensitive search of it finds the single-line token `TryTrimBelowArchiveRoot`.

- [ ] [P1-T3] Add two one-line self-closing Compile Include entries to `UtilitiesCS/UtilitiesCS.csproj` for the two new production helpers, adjacent to the existing ArchiveStemContract.cs entry at line 623. The project is legacy non-SDK with an insertion-ordered item list and no globbing, so the entries are mandatory and their placement follows the neighbouring folder entries. Acceptance: the project file contains exactly two new Compile Include lines and the two named files compile.

- [ ] [P1-T4] In `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs`, add the declaration-only seams that Phase 1 tests bind against, changing no existing behaviour. Add `public interface IFolderLabelAbsenceReport` with the single member `bool IsAbsentLabel(string folderPath);` and an XML doc recording D4's reason for a separate interface rather than a fourth member on IFolderHierarchyProvider. Declare that the provider implements it, with `public bool IsAbsentLabel(string folderPath) => throw new NotImplementedException("Issue #799: the absence report body is supplied by [P2-T6].");`. Add the optional second constructor parameter `System.Func<string>? archiveRootAccessor = null`, stored in the internal get-only auto-property `internal System.Func<string>? ArchiveRootAccessor { get; }` (D3), leaving the existing null check on the first parameter exactly as it is. Add `internal System.Action<string>? ErrorSink { get; set; }` with an XML doc naming it the injected diagnostic sink that tests observe instead of attaching a log4net appender, so no test mutates the process-global logger repository. Every one of these three declarations carries the `?` annotation because this file opens with `#nullable enable` at line 1 and the [P3-T4] gate runs `/p:TreatWarningsAsErrors=true`: an unannotated `System.Func<string> … = null` is CS8625 and an unannotated never-initialised `System.Action<string>` property is CS8618, and both are promoted to build errors by that gate. Do not change GetAncestorChainAsync, GetImmediateSubfoldersAsync, ResolveLeafKeyAsync or ResolveByUniqueSuffix in this task. Acceptance: the solution compiles; the constructor declares exactly two parameters, the second optional with a null default; and a case-sensitive search of the file finds the single-line tokens `IFolderLabelAbsenceReport`, `ArchiveRootAccessor` and `ErrorSink`; and the analyzer build and the nullable build both exit 0 with the declarations in place, which is the observation that proves no CS8625 or CS8618 was introduced.

- [ ] [P1-T5] Build the solution so the seam declarations are available to the test projects, and record `<FEATURE>/evidence/regression-testing/p1-t5-seam-build.md`. Acceptance: `EXIT_CODE: 0`, which also proves D2's claim that the optional parameter leaves all 19 existing provider constructions compiling unchanged, including the single-null-argument construction at UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs:316.

```powershell
msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

- [ ] [P1-T6] Create `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs` with `[TestClass] public sealed class ArchiveStemProjectionTests` in namespace `UtilitiesCS.Test.OutlookObjects.Folder`, using MSTest and FluentAssertions and no mocks, covering the #614 boundary cases the spec enumerates: a path strictly under the root; a path EQUAL to the root, which must return the input unchanged rather than an empty string; the Archive2 false-prefix case, where a path under a sibling folder named Archive2 tested against a root ending in Archive yields the character 2 at the root's length and is therefore NOT projected; a root supplied with one and with two trailing separators; an empty root and a whitespace-only root, both of which must yield the input unchanged, which is the AC4 one-separator strip elimination; a null path and an empty path; forward-slash separators on both parameters; and a mixed-case root. Acceptance: the file compiles once [P1-T10] wires it, contains at least nine `[TestMethod]` attributes, and is at or below 500 lines.

- [ ] [P1-T7] Create `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveChainProjectionTests.cs` with `[TestClass] public sealed class ArchiveChainProjectionTests` in namespace `UtilitiesCS.Test.OutlookObjects.Folder`, building every chain from `FolderBreadcrumbSegment` literals through the four-argument constructor at UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbSegment.cs:29-40, with no snapshot, no provider and no COM. It covers: a chain that passes through the root, asserting the returned segments are exactly the ones after the root node and that segment identity is preserved by reference; a chain that does not pass through the root, asserting false and an empty output; a chain whose LEAF is the root, asserting false; an empty chain; a single-element chain that is the root; a root supplied with a trailing separator; and the Archive2 false-prefix case at chain level. Acceptance: the file compiles once [P1-T10] wires it, contains at least seven `[TestMethod]` attributes, and is at or below 500 lines.

- [ ] [P1-T8] Create `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTrimTests.cs` with `[TestClass] public sealed class OutlookFolderHierarchyProviderTrimTests` in namespace `UtilitiesCS.Test.OutlookObjects.Folder`, using a `Mock<IOutlookFolderTreeService>` returning a hand-built `FolderTreeSnapshot` in the pattern already used throughout UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs, plus a `Func<string>` root accessor. It contains: `GetAncestorChainAsync_WithRootAccessor_ReturnsSegmentsBelowTheArchiveRoot` (a store-rooted three-level Archive chain; assert the returned segments begin at the first node BELOW the archive root and that neither the store node nor the archive-root node appears); `GetAncestorChainAsync_WithoutRootAccessor_ReturnsTheUntrimmedChain` (the no-accessor construction is the effective off switch); `GetAncestorChainAsync_ChainMissesArchiveRoot_LogsErrorAndReturnsEmpty` (asserting the AC2 error text is delivered exactly once through the injected `ErrorSink` and that the result is an empty segment list, which routes each surface into its existing fallback); `GetAncestorChainAsync_LeafIsTheArchiveRoot_LogsErrorAndReturnsEmpty`; `GetAncestorChainAsync_RootAccessorThrows_DoesNotThrowAndReturnsTheUntrimmedChain` (the accessor is lazy precisely because the archive-root property throws, per D2); `ResolveLeafKeyAsync_SameAbsentLabelTwice_EmitsOneErrorAndReportsAbsence` (resolve an unresolvable label twice against the same provider instance, assert exactly one emission through `ErrorSink`, and assert `IsAbsentLabel` returns true for that path); `ResolveLeafKeyAsync_AmbiguousLabel_EmitsOneErrorAndDoesNotReportAbsence` (a decoy snapshot in which two nodes share the suffix; assert one emission and that `IsAbsentLabel` returns FALSE, which is decision D-B's zero-candidate restriction); and `ResolveLeafKeyAsync_AbsentThenResolvableLabel_ClearsTheAbsenceReport` (D6's reset). Acceptance: the file compiles once [P1-T10] wires it, contains exactly eight `[TestMethod]` attributes, and is at or below 500 lines.

- [ ] [P1-T9] Create `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorRecentsProjectionTests.cs` with `[TestClass] public sealed class FolderPredictorRecentsProjectionTests` in namespace `UtilitiesCS.Test.OutlookObjects.Folder`, following the globals/recents mock construction already used at UtilitiesCS.Test/OutlookObjects/Folder/FolderRowTests.cs:246-258 and UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs:994. It contains `FolderArray_RootedRecentEntry_IsProjectedToTheArchiveRelativeStem`, `FolderRowArray_RootedRecentEntry_IsProjectedToTheArchiveRelativeStem`, `FolderRowArray_AndFolderArray_AgreeOnRecentTextAfterProjection` (the text-parity contract documented at `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`:233-242 and currently unasserted), and `FolderArray_OutOfRootRecentEntry_IsLeftUnchanged`. Each test seeds the recents list with one archive-rooted entry and one already-relative entry, so the projection is observable and the identity case is pinned in the same fixture. Acceptance: the file compiles once [P1-T10] wires it, contains exactly four `[TestMethod]` attributes, and is at or below 500 lines.

- [ ] [P1-T10] Add four one-line self-closing Compile Include entries to `UtilitiesCS.Test/UtilitiesCS.Test.csproj` for the four new test files, appended adjacent to the existing folder-test entries at lines 302-304. The project is legacy non-SDK with an insertion-ordered item list and no globbing. Acceptance: the project file contains exactly four new Compile Include lines and the four named files compile.

- [ ] [P1-T11] Create `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs` with `[TestClass] public sealed class BreadcrumbBridgeRouterScoreJoinTests` in namespace `QuickFiler.Test.Controllers`, modelled on the strict-mock construction already used at QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue614Tests.cs:254-259 (a `Mock<IFolderHierarchyProvider>`, a `Mock<IBreadcrumbWebHost>` capturing NavigateToString output, a real `BreadcrumbMessageCodec`, a real `BreadcrumbHtmlRenderer` and a real `BreadcrumbOutboundQueue`). It contains: `BindRowsAsync_RootedScoreAndRelativeRow_RendersThePercentage` (the AC6 pin: presented row is the archive-relative stem, the score carries the raw rooted path, the bound root is non-empty; assert the rendered document contains the percentage cell for that row); `BindRowsAsync_RootedScoreAndRootedRow_StillRendersThePercentage` (D7's additive requirement: the rooted-presented case must not regress); `BindRowsAsync_EmptyBoundRoot_LeavesTheJoinUnchanged` (the public three-argument overload forwards an empty root, so the projection is the identity and no existing caller changes behaviour); `BindRowsAsync_TrimmedChain_PreservesFilingTargetAndScoreKey` (the AC3 pin, asserted as its own test and not as an incidental consequence: with an ancestor chain that begins below the archive root, the bound row's filing target and the joined score key are both still the archive-relative stem); and `BindRowsAsync_MixedRowSet_RendersLineageOnFolderRowsOnly` (the spec's integration scenario, driven entirely through the router with no WebView2 and no Outlook: a banner row, a suggestion row, a search-result row, the trash pseudo-row and one stale label; assert lineage on both folder row kinds, the existing fallback on the stale label, and no lineage on the banner or trash rows); `BindRowsAsync_ZeroCandidateLabel_SuppressesTheRowAndKeepsSegmentKeysAligned` (the AC7 row half, and the only test in this plan that executes the [P2-T12] branch: the provider mock is extended with `.As<IFolderLabelAbsenceReport>()` so the router's `provider as IFolderLabelAbsenceReport` cast succeeds, `IsAbsentLabel` is set up to return true for exactly one suggestion row's hierarchy path and false for the others, and `FetchChainAsync` is driven to a null chain for that row; assert the rendered document contains no row for the suppressed label, that every surviving row still carries its own segment keys, and that the surviving row count is one lower than the presented row count); and `BindRowsAsync_AmbiguousLabel_IsNotSuppressed` (decision D-B's zero-candidate restriction at the router boundary: same construction, but `IsAbsentLabel` returns false for the null-chain row, and the row must still be rendered with the existing fallback). Extending the mock with `.As<IFolderLabelAbsenceReport>()` is confined to this file and reaches no existing test: no other router test in QuickFiler.Test uses `.As<>()`, so in every one of them the cast still yields null and suppression stays inert exactly as D4 records. Author this file for C# 7.3. QuickFiler.Test.csproj declares no `<LangVersion>` and targets v4.8.1 at its line 18, so it compiles at the 7.3 default while every other project in this plan's scope is at Latest, preview or 12.0. Use classic `using (...) { }` blocks, `!= null` rather than `is not null`, explicitly typed `new` rather than target-typed `new`, `switch` statements rather than switch expressions, and no nullable reference annotation anywhere in the file. Mirroring a construct from an existing UtilitiesCS.Test file into this one is the specific failure mode: it surfaces as CS8370 at the [P1-T15] build, not at edit time. Acceptance: the file compiles once [P1-T12] wires it, contains exactly seven `[TestMethod]` attributes, and is at or below 500 lines.

- [ ] [P1-T12] Add one one-line self-closing Compile Include entry to `QuickFiler.Test/QuickFiler.Test.csproj` for the new test file, appended adjacent to the existing BreadcrumbBridgeRouterIssue439Tests.cs entry at line 64. Acceptance: the project file contains exactly one new Compile Include line and the named file compiles.

- [ ] [P1-T13] Retarget GetAncestorChainAsync_HappyPath_ReturnsRootToLeafSegments in `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` lines 72-91 so the pinned configuration is the one production uses (D9): rename it to `GetAncestorChainAsync_WithRootAccessor_ReturnsSegmentsBelowTheArchiveRoot_HappyPath`, construct the provider with a root accessor returning the fixture's `\Root` path, and replace the root-to-leaf assertion at lines 84-87 with the trimmed expectation `\Root\Clients`, `\Root\Clients\Acme`. Retargeting rather than deleting is required because the deleted behaviour is a specification change and a deleted test pins nothing. No companion case is added HERE. The off switch is pinned instead by `GetAncestorChainAsync_WithoutRootAccessor_ReturnsTheUntrimmedChain` in the new file `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTrimTests.cs` that [P1-T8] creates, which has ample headroom. This placement is a hard constraint rather than a preference: the file edited here is 479 lines against a 500-line ceiling, its existing test body is 20 lines, and a same-shape companion plus its blank separator would consume 21 of the 21 available lines before the one line the added constructor argument itself costs and before CSharpier reflows anything, which would put the file over the ceiling and make the [P3-T10] gate unsatisfiable with no remedy. Acceptance: a case-sensitive search of the file finds zero matches for the single-line token `GetAncestorChainAsync_HappyPath_ReturnsRootToLeafSegments` and exactly one match for `GetAncestorChainAsync_WithRootAccessor_ReturnsSegmentsBelowTheArchiveRoot_HappyPath`; the twelve other provider constructions in the file are unchanged; and the file is at or below 483 lines, that is its [P0-T14] baseline of 479 plus at most four lines.

- [ ] [P1-T14] Retarget the empty-root assertion in `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs` lines 219-226, inside ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection, so it asserts the identity projection rather than the one-separator strip AC4 eliminates: an empty archive root must now leave the input unchanged, and the assertion's `because` text must state that AC4 of issue #799 removed the empty-root strip. Update the surrounding XML doc at lines 206-211 so it no longer describes the removed behaviour, and update the XML doc at lines 245-258 of the following test, whose prose asserts that an empty archive root causes FolderArray entries to be stripped; that following test's own assertions are re-derived against the new behaviour and updated only where they encode the removed strip. The five other boundary assertions in the retargeted test (null root, null path, out-of-root path, empty-remainder guard, case-insensitive prefix) are unchanged, because ToDisplayStem reproduces each of them. Acceptance: a case-sensitive search of the file finds zero matches for the single-line literal `archivePrefix of one separator, which it strips`; the retargeted test still contains exactly six assertions on ProjectPredeterminedFolder; and the file is at or below 500 lines.

- [ ] [P1-T15] Build the solution with the new and retargeted tests in place and record `<FEATURE>/evidence/regression-testing/p1-t15-test-build.md`. Acceptance: `EXIT_CODE: 0`, proving every new test compiles against the Phase 1 seams.

```powershell
msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

- [ ] [P1-T16] [expect-fail] Run the four new and one retargeted UtilitiesCS.Test classes and record `<FEATURE>/evidence/regression-testing/p1-t16-ut-fail.md` with `ExpectedExitCode: 1`. The artifact must enumerate, by fully qualified name, every failing test and state for each whether it fails because a Phase 1 seam throws `NotImplementedException` (tag `SEAM-BLOCKED`) or because the production behaviour is not yet changed (tag `NEW` or `RETARGETED`). Where a results directory holds more than one TRX because a task was re-run, the most recently modified TRX is the one read, and the artifact records which file it was by name reduced per R3. Acceptance: `EXIT_CODE: 1`, and the recorded failure set includes `GetAncestorChainAsync_WithRootAccessor_ReturnsSegmentsBelowTheArchiveRoot` and `FolderArray_RootedRecentEntry_IsProjectedToTheArchiveRelativeStem`.

```powershell
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\799-p1-t16' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:FullyQualifiedName~ArchiveStemProjectionTests|FullyQualifiedName~ArchiveChainProjectionTests|FullyQualifiedName~OutlookFolderHierarchyProviderTrimTests|FullyQualifiedName~FolderPredictorRecentsProjectionTests|FullyQualifiedName~OutlookFolderHierarchyProviderTests'
```

- [ ] [P1-T17] [expect-fail] Run the new QuickFiler.Test score-join class and the retargeted folder-handling class and record `<FEATURE>/evidence/regression-testing/p1-t17-qft-fail.md` with `ExpectedExitCode: 1`, enumerating each failing test by fully qualified name with its failure message reduced per R3. Where a results directory holds more than one TRX because a task was re-run, the most recently modified TRX is the one read, and the artifact records which file it was by name reduced per R3. Acceptance: `EXIT_CODE: 1`, and both `BindRowsAsync_RootedScoreAndRelativeRow_RendersThePercentage` and `ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection` appear in the failure set.

```powershell
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\799-p1-t17' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeRouterScoreJoinTests|FullyQualifiedName~QfcItemController_FolderHandlingTests'
```

- [ ] [P1-T18] Write `<FEATURE>/evidence/regression-testing/p1-t18-red-inventory.md` consolidating the two fail-before artifacts into one list of every test that is red at the end of Phase 1, each tagged `NEW`, `RETARGETED` or `SEAM-BLOCKED`. This is the set Phase 2 must turn green and nothing else. Acceptance: the inventory's entry count equals the sum of the failure counts recorded by [P1-T16] and [P1-T17], and every entry carries exactly one of the three tags.

---

### Phase 2 — Production implementation

- [ ] [P2-T1] Replace the [P1-T1] seam body in `UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs` with the real implementation: guard first with `if (folderPath is null || archiveRoot is null) { return folderPath; }`, then return the stem when `ArchiveStemContract.TryMakeArchiveRelative(folderPath, archiveRoot, out var stem)` returns true AND `stem.Length > 0`, and return `folderPath` unchanged in every other case. The leading null guard is mandatory rather than defensive: `ArchiveStemContract.TryMakeArchiveRelative` declares its two input parameters as non-nullable `string` at UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs:107-108 inside a file that opens with `#nullable enable` at line 1, so passing either [P1-T1] `string?` parameter into it without first narrowing is CS8604, which the [P3-T4] gate promotes to a build error. The guard also delivers the behaviour [P1-T6] pins for the null-path case, so it costs no extra branch. The length condition is what makes "path equals root" non-projectable, because the contract returns true with an empty stem on exact equality at UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs:124-127 and an empty display row is worse than the full path; it reproduces the existing guard at `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`:858 exactly. Acceptance: every `[TestMethod]` in `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs` passes, and a case-sensitive search of the file returns zero matches for the single-line literal `Issue #799: the display projection body is supplied by`.

- [ ] [P2-T2] Replace the [P1-T2] seam body in `UtilitiesCS/OutlookObjects/Folder/ArchiveChainProjection.cs` with the real implementation: scan the chain for the first index whose segment FolderPath satisfies `ArchiveStemContract.TryMakeArchiveRelative(segment.FolderPath, archiveRoot, out var stem)` returning true with `stem.Length == 0`, which is exactly the equality case; assign the remainder after that index to `trimmed` and return true; return false with an empty `trimmed` when no such index exists, when the chain is null or empty, and when that index is the last element. A null `chain` or a null `archiveRoot` returns false through an explicit leading guard, and a whitespace-only `archiveRoot` returns false through the contract's own guard at UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs:113. The null half of that guard is mandatory rather than defensive: the contract declares its two input parameters as non-nullable `string` at ArchiveStemContract.cs:107-108 inside a file that opens with `#nullable enable` at line 1, so passing the [P1-T2] `string? archiveRoot` into it without first narrowing is CS8604, which the [P3-T4] gate promotes to a build error. Acceptance: every `[TestMethod]` in `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveChainProjectionTests.cs` passes, and a case-sensitive search of the file returns zero matches for the single-line literal `Issue #799: the chain trim body is supplied by`.

- [ ] [P2-T3] Relocate the breadcrumb pipeline helper OUT of `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` and INTO the new file `QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs`. This task runs BEFORE [P2-T14] adds the constructor argument; the reverse order puts the viewer-setup file at 501 lines in an intermediate state (R9). Move lines 132-163 verbatim — the two-part comment at 132-136, the `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` attribute at 137, and the member body at 138-163 — together with the blank line that separated it from its neighbour. The new file declares `internal partial class QfcItemController` in namespace `QuickFiler.Controllers` and carries only the using directives the moved code needs, which are the ones for the concrete viewer type and for the arrow-event handler; the provider is referenced by its fully qualified name in the moved code and needs no using. This is a pure relocation: no statement is added, removed or reordered. Acceptance: a case-sensitive search of `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` returns zero matches for the single-line token `EnsureBreadcrumbPipeline` and that file's line count is at least 30 lines lower than the [P0-T14] baseline of 500; a case-sensitive search of `QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs` returns one match for that token and one for the single-line token `ExcludeFromCodeCoverage`.

- [ ] [P2-T4] Add one one-line self-closing Compile Include entry to `QuickFiler/QuickFiler.csproj` for the new item-controller partial, adjacent to the existing QfcItemController.ViewerSetup.cs entry at line 335. Acceptance: the project file contains exactly one new Compile Include line and the solution compiles, which is the only proof that the relocated member is still in the build.

- [ ] [P2-T5] Apply the AC1 and AC2 trim inside GetAncestorChainAsync at `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` lines 34-42, after the snapshot walk and before segment mapping, so row order, banner placement and the trash pseudo-row are untouched. Read the root lazily through the [P1-T4] `ArchiveRootAccessor` property inside a try that treats any exception from the accessor as "no trim configured", which is what keeps the existing archive-root-throws behaviour intact (D2). When the accessor is null or yields a null, empty or whitespace root, return the mapped chain unchanged, which is the effective off switch. Otherwise call `ArchiveChainProjection.TryTrimBelowArchiveRoot`; on true, map and return the trimmed segments; on false, emit one error through the log4net ILog already declared at lines 17-19 AND through the [P1-T4] `ErrorSink`, and return `Array.Empty<FolderBreadcrumbSegment>()`, which routes the Efc surface into the empty-chain single-segment fallback at UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:123-131 and the QuickFiler surface into its existing scored fallback. Do not modify GetImmediateSubfoldersAsync: subfolders are below the leaf and therefore below the archive root by construction. Acceptance: `GetAncestorChainAsync_WithRootAccessor_ReturnsSegmentsBelowTheArchiveRoot`, `GetAncestorChainAsync_WithoutRootAccessor_ReturnsTheUntrimmedChain`, `GetAncestorChainAsync_ChainMissesArchiveRoot_LogsErrorAndReturnsEmpty`, `GetAncestorChainAsync_LeafIsTheArchiveRoot_LogsErrorAndReturnsEmpty` and `GetAncestorChainAsync_RootAccessorThrows_DoesNotThrowAndReturnsTheUntrimmedChain` all pass, and the single retargeted provider test from [P1-T13], `GetAncestorChainAsync_WithRootAccessor_ReturnsSegmentsBelowTheArchiveRoot_HappyPath`, also passes.

- [ ] [P2-T6] Implement the AC7 log gate and absence classification in `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs`. Change ResolveByUniqueSuffix at lines 90-114 from `private static` to a private instance member so it can reach per-instance state, and add the two `ConcurrentDictionary<string, byte>` fields D6 requires, both with `StringComparer.OrdinalIgnoreCase`: a reported-labels set that gates the existing `logger.Error` emission at lines 108-112 through `TryAdd`, so a label already reported by this provider instance emits nothing further, and an absent-labels set that gains the requested path when the candidate count is zero. Keep the message's two causes distinguishable exactly as they are today. Route every emission through the `ErrorSink` as well as through log4net. In ResolveLeafKeyAsync, remove the requested path from the absent-labels set on BOTH success routes — the exact-path match that returns at lines 74-77 and a successful unique-suffix match — so a label that becomes resolvable after a snapshot refresh is no longer reported absent. Replace the [P1-T4] `IsAbsentLabel` seam with a lookup against the absent-labels set. A bare `HashSet` is prohibited here because ResolveLeafKeyAsync awaits AcquireSnapshotAsync and its continuations are not guaranteed to run on one thread; a static set is prohibited because it is process-wide mutable state shared across viewers and across test methods in one assembly. Acceptance: `ResolveLeafKeyAsync_SameAbsentLabelTwice_EmitsOneErrorAndReportsAbsence`, `ResolveLeafKeyAsync_AmbiguousLabel_EmitsOneErrorAndDoesNotReportAbsence` and `ResolveLeafKeyAsync_AbsentThenResolvableLabel_ClearsTheAbsenceReport` all pass; `ResolveLeafKeyAsync_ArchiveRelativeStem_ResolvesToUniqueSuffixMatchNode` in UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs still passes; and a case-sensitive search of the provider file returns zero matches for the single-line literal `Issue #799: the absence report body is supplied by`.

- [ ] [P2-T7] Replace the ProjectSuggestionPath body at `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` lines 848-861 with a delegation to `ArchiveStemProjection.ToDisplayStem`, passing the archive root read defensively from the globals so the existing `_globals is null` identity guard is preserved without a separate early return. This eliminates the empty-root one-separator strip that AC4 names, because the contract returns false for a whitespace-only root at ArchiveStemContract.cs:113. Write the delegation as `ArchiveStemProjection.ToDisplayStem(folderPath, root)!`, with the null-forgiving operator and a one-line comment giving its reason: this file opens with `#nullable enable` at line 1 and ProjectSuggestionPath declares a non-nullable `string` return and a non-nullable `string folderPath` parameter at UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:848, while [P1-T1] declares `ToDisplayStem` with a `string?` return; `ToDisplayStem` returns null only when its own `folderPath` argument is null, which this call site's non-nullable parameter excludes, so the operator is sound and it is required because the unsuppressed form is CS8603 and the [P3-T4] gate promotes it to a build error. Do not change ProjectSuggestionPath's signature: it is called from AddSuggestions at line 810 and AddSuggestionRows at line 842 and widening its return would propagate CS8600 into both. Both call sites are display paths and are unchanged: AddSuggestions at line 810 and AddSuggestionRows at line 842. Acceptance: `Issue609_FolderPredictor_ProjectsOnlyInRootFullSuggestionPaths` and `Issue609_FolderPredictor_ProjectsCaseVariantInRootFullSuggestionPath` in UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs both still pass, and `AddSuggestions_WhenSuggestionsExist_AppendsHeaderAndTopSuggestions` still passes.

- [ ] [P2-T8] Project each recent entry through the same helper at both AC5 sites in `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`: the string append at line 793 inside AddRecents, and the row-model mirror at line 879 inside AddRecentRows. Both must be projected, because the XML doc at lines 233-242 asserts that the string list and the row list are text-identical and the row list is the one the breadcrumb surfaces actually consume, so projecting only one would break a documented contract. Acceptance: all four `[TestMethod]` tests in `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorRecentsProjectionTests.cs` pass, and `AddRecents_WhenRecentsExist_AppendsHeaderAndEntries`, `FolderArray_WhenSuggestionsAndRecentsExist_ReturnsSuggestionsThenRecents` and `FolderRowArray_WithSuggestionsAndRecents_MatchesFolderArrayTextAndTagsKinds` all still pass, because their fixtures use already-relative recents for which the projection is the identity.

- [ ] [P2-T9] Rewrite the include-children TRUE branch of GetOlSubpath at `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` lines 955-965 as a verified prefix removal through `ArchiveStemContract.TryMakeArchiveRelative`, falling back to the input path when the contract returns false, so a path that does not start with the ancestor yields a diagnosable value instead of a garbage substring and a path no longer than the ancestor no longer throws ArgumentOutOfRangeException. The contract's parameter is only NAMED archiveRoot and is root-agnostic, which is what lets it serve this site, where the ancestor is a search root supplied by the caller. Leave the include-children FALSE branch at lines 966-970 exactly as it is: it computes a leaf name, which the contract does not do, and converting it would change a different function. Acceptance: `GetOlSubpath_WhenAncestorEndsWithSlashOrChildrenExcluded_ReturnsExpectedSegment` in UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs still passes, and both GetRelevantOlPathPortion tests in ToDoModel.Test, directory Email Utilities, file FolderHandlerTests_Written.cs still pass.

- [ ] [P2-T10] Replace the ProjectPredeterminedFolder body at `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` lines 272-285 with a one-line delegation to `ArchiveStemProjection.ToDisplayStem`, retaining the member itself so its existing test keeps a target, and rewrite the XML doc at lines 252-271 so the two paragraphs describing the empty-root divergence are replaced by a statement that both members now share one projection and that the empty-root strip was removed by AC4. Also rewrite the duplication-rationale comment at lines 223-230, whose stated reason — that FolderPredictor.ProjectSuggestionPath is private and lives under UtilitiesCS, which the earlier change could not modify — becomes false the moment the shared helper is public. Acceptance: `ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection` passes as retargeted by [P1-T14]; a case-sensitive search of the file returns zero matches for the single-line literal `is private and lives under UtilitiesCS`; and the file is at or below its [P0-T14] baseline of 312 lines.

- [ ] [P2-T11] Project the score paths inside the internal four-argument BindRowsAsync at `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` lines 92-150, immediately before the `_builder.BuildRows` call at lines 132-136, using the bound root the method already normalizes at lines 107-109. Build a new score list that contains every original score UNCHANGED and, additionally, one projected `FolderScore` carrying `ArchiveStemProjection.ToDisplayStem(score.FolderPath, _boundRoot)!` with the same score and probability whenever that projection differs from the original path under an ordinal comparison. The null-forgiving operator carries a one-line comment giving its reason: this file opens with `#nullable enable` at line 1, [P1-T1] declares `ToDisplayStem` with a `string?` return, and `ToDisplayStem` returns null only when its own `folderPath` argument is null, which the projection loop excludes by skipping any score whose `FolderPath` is null before calling; the unsuppressed form is CS8600 or CS8604 at the `FolderScore` construction and the [P3-T4] gate promotes either to a build error. The addition is what makes it safe: substitution would fix the stem-presented case and silently break the rooted-presented case (D7), and the probability index at UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:224 assigns through the indexer, so duplicate keys are tolerated rather than throwing. The public three-argument overload at lines 75-82 forwards an empty root, so the projection is the identity for every caller of that overload and no existing behaviour changes there. Do not modify the row builder and do not change any public signature; re-keying the join was considered and rejected because BuildRows takes only a string list and a score sequence, so there is no correlating identity to key on without changing a public signature and every test that calls it. Acceptance: `BindRowsAsync_RootedScoreAndRelativeRow_RendersThePercentage`, `BindRowsAsync_RootedScoreAndRootedRow_StillRendersThePercentage`, `BindRowsAsync_EmptyBoundRoot_LeavesTheJoinUnchanged` and `BindRowsAsync_TrimmedChain_PreservesFilingTargetAndScoreKey` all pass, and `Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively` and `Issue439ArchiveRelativeRowsRenderLineagePreserveFilingTargetAndProbability` in QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs both still pass.

- [ ] [P2-T12] Apply the AC7 zero-candidate row suppression on the Efc surface only, per the escalation branch D5 records, inside the same internal BindRowsAsync in `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`. Add a private readonly `IFolderLabelAbsenceReport` field assigned in the existing constructor body at lines 48-56 from `provider as IFolderLabelAbsenceReport`, which changes no constructor signature and therefore breaks no existing test; a `Mock<IFolderHierarchyProvider>` is not an `IFolderLabelAbsenceReport`, so the field is null and suppression is inert in every existing router test (D4). In the chain loop at lines 110-130, when a suggestion row's hierarchy path is non-null, its fetched chain is null, and the absence report says that hierarchy path is an absent label, record the presented text in a suppression set; a null chain arising from cancellation or from a provider fault is NOT suppressed, because those rows are not known-absent. Derive the retained presented-row list from the suppression set and pass that SAME list to both `_builder.BuildRows` at lines 132-136 and `AttachSegmentKeys` at line 137, because AttachSegmentKeys indexes the presented rows by row index at line 176 and would mis-align against an unfiltered list. Log the suppressed count at DEBUG through the `log` field declared at lines 21-23. When nothing is suppressed, pass the original list unchanged so the common path allocates nothing. Acceptance: the solution compiles; every `[TestMethod]` in `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs` passes, including `BindRowsAsync_ZeroCandidateLabel_SuppressesTheRowAndKeepsSegmentKeysAligned`, which is the only test that executes this task's branch, and `BindRowsAsync_AmbiguousLabel_IsNotSuppressed`, which pins the zero-candidate restriction at the router boundary; and all ten tests of the partial class `BreadcrumbBridgeRouterIssue439Tests`, across both its files, still pass, which is the observable proof that suppression is inert behind a provider mock that does not implement the absence report.

- [ ] [P2-T13] Add the lazy root accessor argument to the provider construction at `QuickFiler/Controllers/EfcFormController.cs` lines 1053-1055, passing a delegate that reads the archive root from the existing application-globals accessor at call time rather than at construction time. The delegate form is mandatory: the archive-root property throws when the root is unresolvable, this construction is not inside a try, and QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs:242 exists because of that throw (D2). Change nothing else in this file; the raw score read at lines 1115-1117 and the four-argument router call at line 1118 stay exactly as they are, because AC6 is delivered in the router (D7) and this file cannot absorb growth. Acceptance: the solution compiles; `BindBreadcrumbRowsAsync_WhenArchiveRootThrows_ReportsOnceAndDoesNotThrow` still passes; and the file is at or below 1322 lines, that is its [P0-T14] baseline of 1320 plus the at-most-two lines the added argument costs (D11).

- [ ] [P2-T14] Add the same lazy root accessor argument to the provider construction inside the relocated helper in `QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs`. This task must run after [P2-T3] (R9). Change nothing in `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` in this task. Acceptance: the solution compiles; a case-sensitive search of `QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs` finds the single-line token `ArchiveRootPath`; and `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is at or below 500 lines.

- [ ] [P2-T15] Build the solution and record `<FEATURE>/evidence/regression-testing/p2-t15-build.md`. Acceptance: `EXIT_CODE: 0`.

```powershell
msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

- [ ] [P2-T16] Run every test named in the [P1-T18] inventory and record `<FEATURE>/evidence/regression-testing/p2-t16-pass-after.md`. Acceptance: the artifact records the two derived lines `EXIT-CODE-UT:` and `EXIT-CODE-QFT:`, one per invocation, both `0`, together with a single `EXIT_CODE:` field equal to the larger of the two so the artifact satisfies the evidence schema; `FAILED-UT: 0` and `FAILED-QFT: 0`, each read from its run's TRX `ResultSummary/Counters` `failed` attribute and NOT from the console, because vstest prints no `Failed:` line at all on a fully passing run; and, for every test named in the [P1-T18] inventory, a `PASS-AFTER: <FullyQualifiedName>` line derived from the TRX, with the count of those lines equal to the [P1-T18] inventory count. Where a results directory holds more than one TRX because a task was re-run, the most recently modified TRX is the one read, and the artifact records which file it was by name reduced per R3. The two runs' own totals are recorded separately as `P2-T16-TOTAL-PASSED:` and `P2-T16-TOTAL-RUN:` and are NOT asserted against the inventory count, because the filters select whole classes and therefore also run tests that were already green at the end of Phase 1.

```powershell
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\799-p2-t16-ut' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:FullyQualifiedName~ArchiveStemProjectionTests|FullyQualifiedName~ArchiveChainProjectionTests|FullyQualifiedName~OutlookFolderHierarchyProviderTrimTests|FullyQualifiedName~FolderPredictorRecentsProjectionTests|FullyQualifiedName~OutlookFolderHierarchyProviderTests'
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\799-p2-t16-qft' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeRouterScoreJoinTests|FullyQualifiedName~QfcItemController_FolderHandlingTests|FullyQualifiedName~BreadcrumbBridgeRouterIssue439Tests'
```

- [ ] [P2-T17] Run the whole UtilitiesCS.Test and QuickFiler.Test assemblies and record `<FEATURE>/evidence/regression-testing/p2-t17-suites.md` with the derived lines `POST-UT-TOTAL:`, `POST-UT-PASSED:`, `POST-UT-FAILED:`, `POST-QFT-TOTAL:`, `POST-QFT-PASSED:`, `POST-QFT-FAILED:` and a `NEWLY-FAILING:` line listing every test failing here that was not failing in the [P0-T11] baseline. Where a results directory holds more than one TRX because a task was re-run, the most recently modified TRX is the one read, and the artifact records which file it was by name reduced per R3. Acceptance: the artifact records the two derived lines `EXIT-CODE-UT:` and `EXIT-CODE-QFT:`, one per invocation, together with a single `EXIT_CODE:` field equal to the larger of the two so the artifact satisfies the evidence schema; each `POST-*-FAILED` value is read from its run's TRX `ResultSummary/Counters` `failed` attribute and NOT from the console, because vstest prints no `Failed:` line at all on a fully passing run; `NEWLY-FAILING: NONE`; `POST-UT-FAILED` is less than or equal to `BASELINE-UT-FAILED`; and `POST-QFT-FAILED` is less than or equal to `BASELINE-QFT-FAILED`.

```powershell
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\799-p2-t17-ut' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\799-p2-t17-qft' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook'
```

- [ ] [P2-T18] Record the pre-format line count of every file this plan has edited or created into `<FEATURE>/evidence/qa-gates/p2-t18-sizes-interim.md`, one `<path> = <count>` line per file alongside its [P0-T14] baseline count, keeping the four project files under the same `PROJECT-FILE (exempt):` heading [P0-T14] uses (R8). Acceptance: every listed `.cs` count satisfies its D11 budget; the exempt project-file counts are recorded but not asserted against the ceiling; and any `.cs` file within ten lines of its budget is named explicitly with its remaining headroom.

---

### Phase 3 — Final QA loop, coverage, and acceptance-criteria closure

- [ ] [P3-T1] Run the CSharpier formatter over the repository and record `<FEATURE>/evidence/qa-gates/p3-t1-format.md`. `format` rewrites tracked source and still exits 0 after rewriting, so the exit code alone cannot distinguish a clean run from a repairing one; the artifact must therefore record the verbatim printed line of the form `Formatted <N> files in <M>ms.` and, as the distinguishing observation, the `git status --porcelain --untracked-files=all` path set and the `git diff --stat` output anchored to `BASE-SHA`, captured before and after the run, with the two derived lines `PATH_SETS_IDENTICAL:` and `DIFFSTAT_IDENTICAL:`. If the formatter rewrote a file outside this plan's Write Set, that path is recorded and the rewrite is reverted, so the repository-wide pass cannot widen the scope boundary [P3-T11] asserts. Acceptance: `EXIT_CODE: 0` and both derived comparison lines are recorded with their values.

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
$BaseSha = (Select-String -Path 'docs\features\active\2026-09-06-breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency-799\evidence\baseline\p0-t2-base.md' -CaseSensitive -Pattern '^BASE-SHA: ([0-9a-f]{40})$').Matches[0].Groups[1].Value
git add --intent-to-add -- '*.cs' '*.csproj'
$before = @(git status --porcelain --untracked-files=all)
$beforeStat = @(git diff --stat $BaseSha)
dotnet tool run csharpier format .
$after = @(git status --porcelain --untracked-files=all)
$afterStat = @(git diff --stat $BaseSha)
"PATH_SETS_IDENTICAL=$(($null -eq (Compare-Object -ReferenceObject $before -DifferenceObject $after)))"
"DIFFSTAT_IDENTICAL=$(($null -eq (Compare-Object -ReferenceObject $beforeStat -DifferenceObject $afterStat)))"
```

- [ ] [P3-T2] Run the read-only CSharpier check and record `<FEATURE>/evidence/qa-gates/p3-t2-format-check.md` with the verbatim printed line and the derived line `FINAL-CSHARPIER-CHECKED-FILES: <N>`. The success-case output on a clean tree is the single line of the form `Checked <N> files in <M>ms.` with exit 0. Record the delta against `BASELINE-CSHARPIER-CHECKED-FILES` from [P0-T8]; eight new `.cs` files are added by this plan — three production and five test — so a delta of 8 is the expected observation, and any other value must be explained in the artifact. Acceptance: `EXIT_CODE: 0`. The exit code is the gate here, because `check` is read-only and returns non-zero on drift.

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
dotnet tool run csharpier check .
```

- [ ] [P3-T3] Run the analyzer gate and record `<FEATURE>/evidence/qa-gates/p3-t3-analyzers.md`, comparing its warning and error counts against [P0-T9]. Acceptance: `EXIT_CODE: 0` and the error count is 0.

```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

- [ ] [P3-T4] Run the nullable gate and record `<FEATURE>/evidence/qa-gates/p3-t4-nullable.md`, comparing its warning and error counts against [P0-T10]. `/p:Nullable=enable` must not be added and `/t:Build` must not be substituted. Acceptance: `EXIT_CODE: 0` and the error count is 0.

```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

- [ ] [P3-T5] Run the full nine-assembly suite under `dotnet-coverage`, writing the Cobertura document to artifacts\csharp\coverage.xml, and record `<FEATURE>/evidence/qa-gates/p3-t5-tests-coverage.md` with the derived lines `FINAL-LINES-COVERED:`, `FINAL-LINES-VALID:`, `FINAL-BRANCHES-COVERED:`, `FINAL-BRANCHES-VALID:`, `FINAL-PACKAGES-MATCHED:`, the two derived percentages, and `FINAL-TOTAL-TESTS:` / `FINAL-FAILED-TESTS:`. The four counters are aggregated by the same pinned block [P0-T12] used, over the same nine first-party package names, under the same derived coverage configuration and the same test-case filter, so the two sides are produced by one collector, one configuration, one selection and one filter. The artifacts directory is git-ignored at `.gitignore` line 57, so the document is a local tool output rather than committed evidence; the acceptance below is on-disk existence and the recorded counters, not on `git ls-files`. Where a results directory holds more than one TRX because a task was re-run, the most recently modified TRX is the one read, and the artifact records which file it was by name reduced per R3. If the [P3-T6] loop restarts after this task has run, artifacts\csharp\coverage.xml is deleted before [P3-T1] is re-run, because the file is a machine-generated XML document that CSharpier would otherwise take as formatting input. Acceptance: `EXIT_CODE: 0`; `FINAL-FAILED-TESTS:` is read from the run's TRX `ResultSummary/Counters` `failed` attribute rather than from the console, because vstest prints no `Failed:` line on a fully passing run, and is less than or equal to `BASELINE-FAILED-TESTS` from [P0-T12] with a `NEWLY-FAILING:` line naming every test failing here that was not failing there, which must read `NEWLY-FAILING: NONE`; artifacts\csharp\coverage.xml exists; all five `FINAL-` counter lines are numeric; and `FINAL-PACKAGES-MATCHED` equals `BASELINE-PACKAGES-MATCHED` from [P0-T12].

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
New-Item -ItemType Directory -Force -Path 'artifacts\csharp' | Out-Null
dotnet-coverage collect --output artifacts\csharp\coverage.xml --output-format cobertura --settings coverage\799-effective-coverage.config -- $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll SVGControl.Test\bin\Debug\SVGControl.Test.dll Tags.Test\bin\Debug\Tags.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskTree.Test\bin\Debug\TaskTree.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll ToDoModel.Test\bin\Debug\ToDoModel.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\799-p3-t5' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
$doc = New-Object System.Xml.XmlDocument
$doc.Load((Resolve-Path -LiteralPath 'artifacts\csharp\coverage.xml').Path)
$first = @('QuickFiler','SVGControl','Tags','TaskMaster','TaskTree','TaskVisualization','ToDoModel','UtilitiesCS','VBFunctions')
$lc=0;$lv=0;$bc=0;$bv=0;$pm=0
foreach ($pkg in $doc.SelectNodes('//package')) {
    $name = $pkg.GetAttribute('name')
    if ($first -notcontains $name) { continue }
    $pm++
    foreach ($ln in $pkg.SelectNodes('.//line')) {
        $lv++
        if ([int]$ln.GetAttribute('hits') -gt 0) { $lc++ }
        $cc = $ln.GetAttribute('condition-coverage')
        if ($cc -match '\((\d+)/(\d+)\)') { $bc += [int]$Matches[1]; $bv += [int]$Matches[2] }
    }
}
"LINES_COVERED=$lc LINES_VALID=$lv BRANCHES_COVERED=$bc BRANCHES_VALID=$bv PACKAGES_MATCHED=$pm"
```

- [ ] [P3-T6] Record the toolchain loop closure into `<FEATURE>/evidence/qa-gates/p3-t6-loop.md`, listing [P3-T1] through [P3-T5] in order with each artifact path and each recorded exit code, and stating whether any step failed or rewrote a file. Acceptance: the artifact records all five steps as passing in one uninterrupted pass, or, if any step failed or changed files, records the restart and the subsequent clean pass; the checklist box for this task stays unchecked until a clean pass is recorded.

- [ ] [P3-T7] Write `<FEATURE>/evidence/qa-gates/p3-t7-changed-lines.md` comparing coverage on the changed production lines. Restrict the comparison to the paths [P0-T13] reported as `MEASURABLE:`; for each path reported `UNMEASURABLE:`, record `CHANGED-LINE-COVERAGE: NOT MEASURABLE` with the reason the determination gave and name the passing tests that exercise those changed lines as the substitute evidence. For each measurable path, derive the changed line numbers from the anchored `git diff --unified=0` in the command block below and record each changed line's `hits` value from artifacts\csharp\coverage.xml, using a de-duplicated per-line map that merges the class-level line elements with the method-level line elements keyed by line number and resolved by maximum `hits`. Where a diff hunk's added and removed line counts are unequal, no one-to-one baseline mapping exists; record such lines as `baseline=none` and exclude them from the regression count rather than attributing borrowed coverage. A changed line carrying no line element in either branch of the merged map is non-executable — an XML doc comment, a blank line, a using directive, a brace or an interface method declaration — and has no `hits` value; record such lines as `hits=non-executable` and exclude them from both the `hits = 0` count and the regression count. Acceptance: every changed production line in the measurable set is recorded with either a post-change `hits` value or the `hits=non-executable` marker; the count of changed lines with `hits = 0` is stated over executable lines only; and the count of changed lines whose post-change `hits` is lower than their baseline `hits` is `0`.

```powershell
$BaseSha = (Select-String -Path 'docs\features\active\2026-09-06-breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency-799\evidence\baseline\p0-t2-base.md' -CaseSensitive -Pattern '^BASE-SHA: ([0-9a-f]{40})$').Matches[0].Groups[1].Value
git add --intent-to-add -- '*.cs'
git status --porcelain --untracked-files=all -- 'UtilitiesCS/OutlookObjects/Folder' 'QuickFiler/Controllers'
foreach ($p in @('UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs', 'UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs', 'QuickFiler/Controllers/QfcItemController.FolderHandling.cs', 'QuickFiler/Controllers/BreadcrumbBridgeRouter.cs', 'QuickFiler/Controllers/EfcFormController.cs', 'QuickFiler/Controllers/QfcItemController.ViewerSetup.cs')) {
    "=== $p"
    git diff --unified=0 $BaseSha -- $p
}
$doc = New-Object System.Xml.XmlDocument
$doc.Load((Resolve-Path -LiteralPath 'artifacts\csharp\coverage.xml').Path)
foreach ($n in @('OutlookFolderHierarchyProvider.cs','FolderPredictor.cs','QfcItemController.FolderHandling.cs','BreadcrumbBridgeRouter.cs','EfcFormController.cs','QfcItemController.ViewerSetup.cs')) {
    $map = @{}
    foreach ($c in $doc.SelectNodes('//class')) {
        $f = $c.GetAttribute('filename')
        if (-not ($f.EndsWith('\' + $n) -or $f.EndsWith('/' + $n))) { continue }
        foreach ($ln in $c.SelectNodes('.//line')) {
            $num = [int]$ln.GetAttribute('number'); $h = [int]$ln.GetAttribute('hits')
            if (-not $map.ContainsKey($num) -or $map[$num] -lt $h) { $map[$num] = $h }
        }
    }
    "FILE=$n MAPPED_LINES=$($map.Count)"
    foreach ($k in ($map.Keys | Sort-Object)) { "  LINE=$k HITS=$($map[$k])" }
}
```

- [ ] [P3-T8] Write `<FEATURE>/evidence/qa-gates/p3-t8-coverage-delta.md` comparing the five [P0-T12] baseline counters against the five [P3-T5] final counters. Record the comparability precondition first: `FINAL-LINES-VALID` and `BASELINE-LINES-VALID` must be compared and their relation stated, because the denominator grows when new production lines are added and the two sides are only directly comparable when it does not. When the denominators differ, compare the two derived percentages instead and state that the percentage comparison is the one used. Acceptance: the artifact records baseline coverage, post-change coverage, and the changed-line determination from [P3-T7], and states explicitly whether the repository-wide first-party line percentage decreased.

- [ ] [P3-T9] Write `<FEATURE>/evidence/qa-gates/p3-t9-new-type-coverage.md` recording line and branch coverage for the two new production types, read from artifacts\csharp\coverage.xml by selecting the class elements whose `filename` attribute ends with a directory separator followed by `ArchiveStemProjection.cs` or `ArchiveChainProjection.cs`. The repository unit-test policy requires new modules, classes and methods to target at least 90 percent coverage. Acceptance: the block prints one `FILE=` line per new type; `CLASS_ELEMENTS` is greater than zero on both, so a zero-element selection is visible rather than silent; and `LINES_COVERED` divided by `LINES_VALID` is at or above 0.90 for each of the two files.

```powershell
$doc = New-Object System.Xml.XmlDocument
$doc.Load((Resolve-Path -LiteralPath 'artifacts\csharp\coverage.xml').Path)
foreach ($n in @('ArchiveStemProjection.cs','ArchiveChainProjection.cs')) {
    $elems = 0; $map = @{}; $bc = 0; $bv = 0
    foreach ($c in $doc.SelectNodes('//class')) {
        $f = $c.GetAttribute('filename')
        if (-not ($f.EndsWith('\' + $n) -or $f.EndsWith('/' + $n))) { continue }
        $elems++
        foreach ($ln in $c.SelectNodes('.//line')) {
            $num = [int]$ln.GetAttribute('number'); $h = [int]$ln.GetAttribute('hits')
            if (-not $map.ContainsKey($num) -or $map[$num] -lt $h) { $map[$num] = $h }
            $cc = $ln.GetAttribute('condition-coverage')
            if ($cc -match '\((\d+)/(\d+)\)') { $bc += [int]$Matches[1]; $bv += [int]$Matches[2] }
        }
    }
    $lv = $map.Count
    $lc = @($map.Values | Where-Object { $_ -gt 0 }).Count
    "FILE=$n CLASS_ELEMENTS=$elems LINES_COVERED=$lc LINES_VALID=$lv BRANCHES_COVERED=$bc BRANCHES_VALID=$bv"
}
```

- [ ] [P3-T10] Record the post-format line count of every file this plan edited or created into `<FEATURE>/evidence/qa-gates/p3-t10-sizes.md`, one `<path> = <count>` line per file alongside its [P0-T14] baseline, keeping the four project files under the same `PROJECT-FILE (exempt):` heading (R8). This audit runs AFTER the final format because CSharpier can change line counts (R9). Acceptance: `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is at or below 500; `QuickFiler/Controllers/EfcFormController.cs` is at or below 1322; `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` is at or below 1003; every other listed `.cs` file is at or below 500; the exempt project-file counts are recorded but not asserted against the ceiling; and the artifact states the smallest remaining headroom across all listed `.cs` files together with the three disclosed pre-existing over-ceiling files and their budgets (D11).

- [ ] [P3-T11] Write `<FEATURE>/evidence/qa-gates/p3-t11-scope.md` enumerating the changed source set under the R7 pathspec and asserting the scope boundary. The artifact must list the anchored-diff output and the porcelain output side by side, because neither alone is correct in both states: an anchored diff cannot see an untracked path, and porcelain status goes empty once the change is committed. Acceptance: the enumerated set contains only the twenty Write Set paths this plan actually writes — the nine production paths, the five new test paths, the two retargeted test paths, and the four project files — and contains none of the six sibling-owned files named in D1, which are, in bare prose, UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs, UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.SearchPresentation.cs, UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs, UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Highlight.cs, QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs and QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs; and contains none of QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs, QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs, UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs, UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs, UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotQueries.cs, UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs, UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs, QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs or QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.Activation.cs, the last two per the D8 no-hunk finding.

```powershell
$BaseSha = (Select-String -Path 'docs\features\active\2026-09-06-breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency-799\evidence\baseline\p0-t2-base.md' -CaseSensitive -Pattern '^BASE-SHA: ([0-9a-f]{40})$').Matches[0].Groups[1].Value
git add --intent-to-add -- '*.cs' '*.csproj'
git diff --name-only $BaseSha -- '*.cs' '*.csproj'
git status --porcelain --untracked-files=all -- '*.cs' '*.csproj'
```

- [ ] [P3-T12] Write `<FEATURE>/evidence/qa-gates/p3-t12-ac8-verification.md` recording the AC8 finding as verified evidence rather than as a fix, per `spec.md` decision D-C. The artifact must re-run and record the repository-wide negative search below over the six product projects, and must record the five traced transformations and why each leaves a leading underscore unchanged: the verbatim splitter splits on path separators with empty entries removed and inserts nothing; the JSON serializer escapes only the double quote, the backslash and control characters, and the non-indenting format adds no whitespace inside string values; the QuickFiler page assigns segment text through the DOM textContent property, which performs no entity decoding and no transformation; the Efc page encodes ampersand, less-than, greater-than and double-quote only, so underscore and space pass through and no non-breaking space is emitted; and neither stylesheet contains letter-spacing, word-spacing, text-transform, a first-letter pseudo-element or word-break. It must also record that the WinForms mnemonic prefix character is the ampersand and not the underscore, so no combo-box or owner-draw path can be responsible. The conclusion is recorded as a definite finding: the reported space after the leading underscore was a transcription artifact, the renderer is correct, and a renderer change would be a defect. No file is edited by this task. Acceptance: the artifact records `ExpectedExitCode: 1` and `EXIT_CODE: 1`, because `git grep` exits 1 when it matches nothing and zero matches is this task's SUCCESS outcome, so an artifact omitting the expectation would normalise a passing gate to `fail`; the artifact records that the command produced no output lines; it states the conclusion as a definite finding; and it records `FILES-CHANGED-FOR-AC8: 0`. The regex is single-quoted with the inner single quote doubled, because PowerShell does not treat a backslash as an escape inside a double-quoted string and the double-quoted form of this pattern does not parse.

```powershell
git grep -n -E 'Replace\(["'']_|letter-spacing|text-transform|first-letter|word-break|word-spacing' -- "UtilitiesCS/*.cs" "UtilitiesCS/*.html" "UtilitiesCS/*.css" "QuickFiler/*.cs" "QuickFiler/*.html" "QuickFiler/*.css" "ToDoModel/*.cs" "TaskMaster/*.cs" "Tags/*.cs" "TaskVisualization/*.cs"
```

- [ ] [P3-T13] Check off AC1 in `spec.md` line 834 by changing its `- [ ]` to `- [x]`, citing the [P1-T16] fail-before artifact and the [P2-T16] pass-after artifact and naming the passing tests `GetAncestorChainAsync_WithRootAccessor_ReturnsSegmentsBelowTheArchiveRoot` and `GetAncestorChainAsync_WithRootAccessor_ReturnsSegmentsBelowTheArchiveRoot_HappyPath`. Acceptance: exactly one AC checkbox changes in this task and the AC1 line carries `- [x]`.

- [ ] [P3-T14] Check off AC2 in `spec.md` line 835, citing [P2-T16] and naming the passing tests `GetAncestorChainAsync_ChainMissesArchiveRoot_LogsErrorAndReturnsEmpty` and `GetAncestorChainAsync_LeafIsTheArchiveRoot_LogsErrorAndReturnsEmpty`, and recording that AC2's phrase "existing single-segment fallback" is literally true only on the Efc surface, where the row builder's empty-chain branch renders one leaf-only segment, while the QuickFiler surface routes non-suggestion rows through the verbatim splitter and therefore renders a multi-level stem as several segments — existing behaviour that already satisfies AC2's substantive requirement that no row shows a mailbox prefix. Acceptance: exactly one AC checkbox changes in this task and the AC2 line carries `- [x]`.

- [ ] [P3-T15] Check off AC3 in `spec.md` line 836, citing the passing `BindRowsAsync_TrimmedChain_PreservesFilingTargetAndScoreKey` from [P2-T16] and recording that this is an explicit pin rather than an incidental consequence, together with the structural reason: the trim removes only LEADING segments while the filing value is substituted into the LEAF segment. Acceptance: exactly one AC checkbox changes in this task and the AC3 line carries `- [x]`.

- [ ] [P3-T16] Check off AC4 in `spec.md` line 837, citing [P1-T17] and [P2-T16] and the passing retargeted `ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection`, and recording the D-A site disposition: four sites converted ([P2-T7], [P2-T8], [P2-T9], [P2-T10]) and three deliberately left with the reasons D10 records. Acceptance: exactly one AC checkbox changes in this task and the AC4 line carries `- [x]`.

- [ ] [P3-T17] Check off AC5 in `spec.md` line 838, citing the four passing tests in `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorRecentsProjectionTests.cs` from [P2-T16] and recording that both the string append and the row-model mirror are projected, because the row list is the one the breadcrumb surfaces consume and the documented text-parity contract would otherwise break. Acceptance: exactly one AC checkbox changes in this task and the AC5 line carries `- [x]`.

- [ ] [P3-T18] Check off AC6 in `spec.md` line 839, citing [P1-T17] and [P2-T16] and the passing `BindRowsAsync_RootedScoreAndRelativeRow_RendersThePercentage`, and recording the two deviations from the spec's own wording: the projection is applied in the Efc router rather than at the controller call site named by the criterion, because the controller is 1320 lines and cannot absorb growth while the router already normalizes the bound root; and the projected score is ADDED alongside the raw score rather than substituted for it, per D7. Acceptance: exactly one AC checkbox changes in this task and the AC6 line carries `- [x]`.

- [ ] [P3-T19] Check off AC7 in `spec.md` line 840, citing the passing `ResolveLeafKeyAsync_SameAbsentLabelTwice_EmitsOneErrorAndReportsAbsence`, `ResolveLeafKeyAsync_AmbiguousLabel_EmitsOneErrorAndDoesNotReportAbsence` and `ResolveLeafKeyAsync_AbsentThenResolvableLabel_ClearsTheAbsenceReport` from [P2-T16] for the logging half, and the passing `BindRowsAsync_ZeroCandidateLabel_SuppressesTheRowAndKeepsSegmentKeysAligned` and `BindRowsAsync_AmbiguousLabel_IsNotSuppressed` from [P2-T16] for the row-suppression half, which are the only two tests in this plan that execute the [P2-T12] branch, and recording three things: that the filtered branch is taken restricted to the zero-candidate case per decision D-B; that "per session" is realized as "per provider instance" and is enforced by a thread-safe per-instance set rather than a static one; and that the escalation branch D5 records was taken, so row suppression is delivered on the Efc surface only while the logging half is delivered on both surfaces. Acceptance: exactly one AC checkbox changes in this task, the AC7 line carries `- [x]`, and the check-off note names the escalation deviation.

- [ ] [P3-T20] Check off AC8 in `spec.md` line 841, citing `<FEATURE>/evidence/qa-gates/p3-t12-ac8-verification.md` and recording that AC8 is satisfied by the verified finding that the renderer does not alter a leading underscore, that no code change was made, and that a renderer change would have been a defect. Acceptance: exactly one AC checkbox changes in this task, the AC8 line carries `- [x]`, and no file under QuickFiler/Resources appears in the [P3-T11] scope enumeration.

- [ ] [P3-T21] Update the `spec.md` Status line to `Implemented` and add an "Outcome" note under Rollout & Follow-up recording the four deviations this plan makes from the spec's own prose, each with its reason: AC7 row suppression is delivered on the Efc surface only, because the QuickFiler presented row set is composed solely inside the sibling-owned bridge router, which decision D-B forbids this item from editing (D5); the AC6 score projection is additive rather than substitutive, so the rooted-presented-text join cannot regress (D7); the two #439 Efc router test files listed in the Write Set carry no hunk, because every test in both drives a mocked provider below the trim boundary and editing their shared chain fixture would break unrelated #614 boundary tests (D8); and the AC7 absence classification is published through a new small public interface declared in the provider's own file rather than through a fourth member on the shared hierarchy contract, which net48 cannot add without breaking every implementer and every strict mock (D4). Acceptance: the Status line reads `Implemented` and all four deviations are recorded by name.

- [ ] [P3-T22] Update `issue.md` with the outcome and mirror it to `<FEATURE>/evidence/issue-updates/issue-799.<timestamp>.md` per the evidence conventions, including the literal field lines `Timestamp:`, the exact text intended, and `PostedAs:`. The update must state that this is a specification change superseding issue #439's full root-to-leaf lineage, not a regression fix against it, and that #439's filing-target and score-key constraint is preserved and carried forward as AC3. Acceptance: both the local `issue.md` update and the mirror artifact exist and carry the same text.

- [ ] [P3-T23] Write `<FEATURE>/evidence/qa-gates/p3-t23-ac-summary.md` listing AC1 through AC8 with their final checkbox state and the artifact path that justifies each. Acceptance: eight rows are present, each naming at least one existing artifact path, and every row's checkbox state matches the corresponding line in `spec.md`.

---

SELF-REVIEW: RE-DERIVED THIS PASS

Round 1 (initial authoring). Every citation in the Citation table was read directly from this worktree in this pass, and
the sibling lines, tests and fixtures in the same region as each edited citation were re-checked. The sibling sweep produced
seven findings that changed the plan away from what the spec and the research alone would have produced:

- D5, the escalation branch. FolderBreadcrumbBridgeRouter.SetSuggestionsAsync composes the QuickFiler presented row set as
  the local list at lines 42-86 of the sibling-owned file, and QfcItemController.FolderHandling.cs only hands the predictor
  row model to the viewer at lines 212 and 221, before any resolution has happened. Spec decision D-B's escalation
  condition is therefore met and its documented fallback is taken.
- D7, the additive score projection. BreadcrumbRowBuilder.BuildProbabilityIndex assigns through the indexer at line 224,
  so duplicates are safe, and Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively at lines 118-166 binds
  a rooted presented row together with a rooted score. A substituting projection would have silently removed that row's
  percentage without failing any test.
- D8, the two #439 test files need no hunk. Every test in both files uses a strict Moq provider, and the shared Chain
  helper's leading Archive segment at line 444 is load-bearing for Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection,
  which asserts at line 426 that activating segment 0 yields the archive root.
- D9, the retargeting surface is two tests, and the reason each other candidate family stays green was derived rather than
  assumed: the recents fixtures use the relative value Recent\One, the Issue609 tests already encode ToDisplayStem's exact
  semantics, and both GetOlSubpath fixtures pass a separator-terminated proper prefix.
- D4, the separate diagnostics interface. IFolderHierarchyProvider declares exactly three members, has exactly one concrete
  implementer, and is mocked with MockBehavior.Strict in every router test, so a fourth member would have thrown at the
  first production call inside those tests.
- D2, the optional constructor parameter binds unambiguously. The provider declares exactly one constructor, so
  new OutlookFolderHierarchyProvider(null) at OutlookFolderHierarchyProviderTests.cs:316 still binds to the first
  parameter; 19 existing constructions across two test files were counted line by line.
- D11 and R9, the file-size budgets. Line counts were re-measured in this worktree: QfcItemController.ViewerSetup.cs is
  exactly 500, EfcFormController.cs is 1320 and legitimately grows by the added argument so a no-growth gate on it would be
  unsatisfiable, FolderPredictor.cs is 1003 and shrinks by at least eight lines by derivation, and
  OutlookFolderHierarchyProviderTests.cs is 479 with only 21 lines of headroom for its retarget.

Round 2 (revision pass against the orchestrator-adjudicated preflight delta). Every citation that this pass's edits touch
was re-derived directly against this worktree in this pass, together with the sibling lines and tests in the same region:

- QuickFiler.Test/QuickFiler.Test.csproj. No `<LangVersion>` element occurs anywhere in the file; `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` is at line 18. Sibling re-check: UtilitiesCS.Test/UtilitiesCS.Test.csproj carries `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` at line 17 and `<LangVersion>Latest</LangVersion>` at line 18, so the two test projects differ only in the language-version element. This is the premise of the [P1-T11] C# 7.3 authoring constraint.
- QuickFiler.Test, project-wide. Zero files carry a `#nullable` directive. Sibling re-check: the only two `.As<` uses in the whole project are `.As<IDisposable>()` at QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs:301 and at QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs:373, both outside Controllers, so [P1-T11]'s `.As<IFolderLabelAbsenceReport>()` reaches no existing router test and D4's inertness claim survives the new tests.
- QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs. Six `[TestMethod]` attributes at lines 20, 118, 168, 257, 302 and 379, with their methods at 21, 119, 169, 258, 303 and 380, and six strict provider mocks at 30, 125, 178, 261, 311 and 385. The previously omitted pair is Issue439UnresolvedChainsUseSelectableFallbackForEveryDiagnosableProviderOutcome at line 169 with its mock at line 178, and that is the test which pins today's null-chain selectable-fallback rendering — the exact path [P2-T12] modifies.
- QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.Activation.cs. Four `[TestMethod]` attributes at lines 14, 63, 110 and 177, with Issue609_DirectRowSelection_UsesFullLookupAndRelativeFilingTarget at 15, Issue609_AncestorActivation_EmitsArchiveRelativeFilingTarget at 64, Issue609_ImmediateChildActivation_EmitsArchiveRelativeFilingTarget at 111 and Issue439AncestorActivationQueriesAncestorKeyAndSelectsArchiveRelativeChild at 178, and four strict provider mocks at 21, 70, 118 and 186. Ten tests across the two files, not five.
- UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs, UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs and QuickFiler/Controllers/BreadcrumbBridgeRouter.cs each open with `#nullable enable` at line 1. Sibling re-check: QuickFiler/Controllers/QfcItemController.FolderHandling.cs carries no `#nullable` directive, which is why [P2-T10]'s delegation needs no suppression while [P2-T7]'s and [P2-T11]'s do.
- UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs. `TryMakeArchiveRelative` declares `string fullPath` at line 107 and `string archiveRoot` at line 108, both NON-nullable, inside a file that opens with `#nullable enable` at line 1. This is the sibling finding produced by this pass: the `?` annotations that B3a and B3b add to the two new helpers cannot be passed straight through to the contract, so [P2-T1] and [P2-T2] each gained the explicit leading null guard the compiler requires, and [P2-T7] and [P2-T11] each gained a reasoned null-forgiving operator because their host members declare non-nullable `string` returns.
- UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:848. `private string ProjectSuggestionPath(string folderPath)` — non-nullable return and non-nullable parameter, with its two call sites at lines 810 and 842 unchanged, which is why the delegation is suppressed rather than the signature widened.
- QuickFiler/Controllers/QfcItemController.FolderHandling.cs:272. `internal static string ProjectPredeterminedFolder(string folderPath, string archiveRootPath)` with its own null guard at line 274, in a file with no nullable context.
- UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs. 479 lines. GetAncestorChainAsync_HappyPath_ReturnsRootToLeafSegments occupies lines 72-91, a 20-line body, with the root-to-leaf assertion at 84-87 asserting `\Root`, `\Root\Clients` and `\Root\Clients\Acme`, and the provider constructed at 76-78 with a single argument. Thirteen provider constructions at 76, 98, 118, 139, 157, 175, 193, 218, 240, 269, 299, 316 and 338, the single-null-argument one at 316. A same-shape companion case plus its blank separator would consume the file's entire 21-line headroom, which is the derivation behind [P1-T13]'s relocation of the off-switch pin and its 483-line budget.
- Sibling consequences of that relocation, re-checked and corrected in this pass: [P1-T8] already declares GetAncestorChainAsync_WithoutRootAccessor_ReturnsTheUntrimmedChain, so the pin lands on an already-planned test rather than an orphan reference; [P2-T5]'s acceptance said "the two retargeted provider tests from [P1-T13]" and now names the single one; D9's companion-case sentence was corrected; and the Write Set row's budget changed from +21 to +4.
- Record-hygiene correction found by re-reading the bounded record itself in this pass: the round 1 record carried a CITATION line whose path token contained a space (the ToDoModel email-utilities test file). That contradicted this plan's own R12 and D10 convention, which requires both space-containing paths to be named in words only, and it is not a well-formed path token. The line was removed; the fact it carried is retained in D9, which names that file in words and gives its two GetOlSubpath assertion ranges.
- Sibling consequence of the [P1-T11] test-count change, re-checked in this pass: the task's former closing claim that "every provider interaction is Moq-supplied, so the AC7 suppression path is inert in this file by construction" is no longer true of this file and was replaced by the narrower claim the new tests actually support, that no OTHER router test is reached. [P2-T12]'s acceptance and [P3-T19]'s check-off citation were both updated to name the two new tests, and the AC7 row of AC-MAPPING now carries them.

Delta self-check: this document's own prose was checked against the rules it enforces. Every backticked path with a
directory separator is a Write Set path taken from spec.md lines 645-702; every sibling-owned, precedent, comparison and
out-of-scope path is written in bare prose; both space-containing paths are named in words only and neither is in this
item's footprint; no asserted token carries an angle bracket, a dollar-brace, a dollar-paren or a percent sign; every
git diff invocation in this plan carries an explicit ref operand bound inside its own block together with a staging or porcelain companion; the
two write-mode commands (the formatter and the repo-local SDK installer) each record an observation beyond their exit code;
the coverage aggregation prints the exact line its acceptance reads; and no task's acceptance depends on an artifact a
later task writes — [P0-T13] and [P0-T14] precede every consumer, [P1-T18] precedes [P2-T16], [P0-T12] precedes [P3-T8],
and [P3-T5] precedes [P3-T7], [P3-T8] and [P3-T9].

PLANNER-INTERNAL-REVIEW: PASS
CITATION-TO-TREE: PASS
AC-TRACEABILITY: PASS
SCOPE-BOUNDARY: PASS
CITATION: UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs | 147 lines; #nullable enable at line 1; TryMakeArchiveRelative at lines 106-145 declaring NON-nullable string fullPath at line 107 and NON-nullable string archiveRoot at line 108 with out string stem at line 109; whitespace-root guard at line 113; trailing-separator trim at line 118; equality returning true with an empty stem at lines 124-127; StartsWith prefix test at line 131; separator-boundary test at lines 137-141; stem leading-separator trim at line 143
CITATION: UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs | 141 lines; #nullable enable at line 1; log4net ILog at lines 17-19; single constructor at lines 28-31; GetAncestorChainAsync at lines 34-42; ResolveLeafKeyAsync at lines 56-80 with the exact-path early return at lines 74-77; private static ResolveByUniqueSuffix at lines 90-114 with the two-cause emission at lines 108-112
CITATION: UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs | 65 lines; exactly three members at lines 31-34, 46-49 and 60-63
CITATION: UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs | 1003 lines; #nullable enable at line 1; private string ProjectSuggestionPath(string folderPath) declared with a NON-nullable return and a NON-nullable parameter at line 848; text-parity XML doc at lines 233-242; AddRecents at lines 788-795 with the unprojected AddRange at line 793; AddSuggestions at lines 807-811; AddSuggestionRows at lines 835-846; ProjectSuggestionPath at lines 848-861 with the length guard at line 858; AddRecentRows at lines 866-882 with the unprojected row at line 879; GetOlSubpath at lines 953-971 with the include-children true branch at lines 955-965
CITATION: UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs | 238 lines; empty-chain single-segment fallback at lines 123-131; presented-text probability lookup at lines 133-135; MapSegments at lines 178-208; BuildProbabilityIndex at lines 210-229 assigning through the indexer at line 224
CITATION: UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbSegment.cs | 54 lines; four-argument constructor at lines 29-40; FolderPath property at line 49
CITATION: UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs | 489 lines, sibling-owned; SetSuggestionsAsync at lines 29-97 composing the presented row set at lines 42-86 and swapping it under the shared lock at lines 88-96; shared _sync at line 15 and _suggestionGeneration at line 16
CITATION: UtilitiesCS/Properties/AssemblyInfo.cs | InternalsVisibleTo("UtilitiesCS.Test") at line 19
CITATION: QuickFiler/Controllers/BreadcrumbBridgeRouter.cs | 304 lines; #nullable enable at line 1; log field at lines 21-23; _boundRoot at line 35; constructor at lines 41-56; public three-argument BindRowsAsync at lines 75-82 forwarding an empty root at line 81; internal four-argument BindRowsAsync at lines 92-150 with root normalization at lines 107-109, chain loop at lines 110-130, BuildRows call at lines 132-136 and AttachSegmentKeys call at line 137; ToHierarchyPath at lines 152-167; AttachSegmentKeys indexing presented rows at line 176
CITATION: QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs | 221 lines; FetchChainAsync at lines 50-81 returning null on a null key at lines 61-64 and on both catch arms at lines 68-80
CITATION: QuickFiler/Controllers/EfcFormController.cs | 1320 lines; ConfigureBreadcrumbControl at lines 1047-1067 with the provider construction at lines 1053-1055; BindBreadcrumbRowsAsync at lines 1111-1128 with the raw score read at lines 1115-1117 and the four-argument router call at line 1118
CITATION: QuickFiler/Controllers/QfcItemController.ViewerSetup.cs | exactly 500 lines; usings at lines 1-22; internal partial class at line 26; EnsureBreadcrumbPipeline comment at lines 132-136, ExcludeFromCodeCoverage attribute at line 137, member at lines 138-163, provider construction at lines 147-149
CITATION: QuickFiler/Controllers/QfcItemController.FolderHandling.cs | 312 lines; NO #nullable directive anywhere in the file; internal static string ProjectPredeterminedFolder(string folderPath, string archiveRootPath) at line 272 with its own null guard at line 274; AssignFolderComboBox at lines 191-250 with the row-model hand-off at lines 212 and 221 and the projection call at lines 231-234; duplication-rationale comment at lines 223-230; ProjectPredeterminedFolder XML doc at lines 252-271 and body at lines 272-285
CITATION: UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs | 479 lines; namespace at line 11; GetAncestorChainAsync_HappyPath_ReturnsRootToLeafSegments at lines 72-91 with its root-to-leaf assertion at lines 84-87; 13 provider constructions at lines 76, 98, 118, 139, 157, 175, 193, 218, 240, 269, 299, 316 and 338; single-null-argument construction at line 316
CITATION: UtilitiesCS.Test/OutlookObjects/Folder/FolderHierarchyProviderAdapterTests.cs | 258 lines; 6 provider constructions at lines 98, 122, 142, 170, 185 and 202
CITATION: UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs | 1066 lines, pre-existing over-ceiling and untouched; recents test at lines 249-265 with relative recents at line 255; FolderArray recents test at lines 165-189 with the relative recent at line 175; Issue609 projection tests at lines 191-247; GetOlSubpath assertions at lines 577-591
CITATION: UtilitiesCS.Test/OutlookObjects/Folder/FolderRowTests.cs | text-parity test at lines 29-70 with the relative recent at line 39; recents mock helper at lines 246-258
CITATION: QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs | 455 lines; six strict provider mocks at lines 30, 125, 178, 261, 311 and 385, one per test method at lines 21, 119, 169, 258, 303 and 380; lineage test at lines 20-116 with the archive-root index assertions at lines 109 and 113; rooted-target test at lines 118-166 binding a rooted score at line 149; boundary test at lines 302-377; slash-only-root test at lines 379-427 asserting the archive root at line 426; shared Chain helper at lines 434-448 emitting the leading Archive segment at line 444
CITATION: QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.Activation.cs | 253 lines; four [TestMethod] attributes at lines 14, 63, 110 and 177 with Issue609_DirectRowSelection_UsesFullLookupAndRelativeFilingTarget at line 15, Issue609_AncestorActivation_EmitsArchiveRelativeFilingTarget at line 64, Issue609_ImmediateChildActivation_EmitsArchiveRelativeFilingTarget at line 111 and Issue439AncestorActivationQueriesAncestorKeyAndSelectsArchiveRelativeChild at line 178; strict provider mocks at lines 21, 70, 118 and 186; four uses of the shared Chain helper at lines 33, 82, 130 and 196
CITATION: QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs | 354 lines; namespace at line 10; partial class QfcItemController_FolderHandlingTests at line 21; ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection at lines 212-243 with the empty-root one-separator assertion at lines 219-226
CITATION: QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue614Tests.cs | 358 lines; namespace at line 16; rooted/relative pair bind at lines 254-259
CITATION: UtilitiesCS/UtilitiesCS.csproj | ArchiveStemContract.cs Compile Include at line 623; OutlookFolderHierarchyProvider.cs at line 640; FolderPredictor.cs at line 808; EnsureNuGetPackageBuildImports Error at line 1293; Analyzer Include block at lines 1301-1310 naming Meziantou.Analyzer 3.0.203 and Roslynator.Analyzers 5.0.0
CITATION: UtilitiesCS.Test/UtilitiesCS.Test.csproj | TargetFrameworkVersion v4.8.1 at line 17 and LangVersion Latest at line 18; folder-test Compile Include block at lines 276-307 with OutlookFolderHierarchyProviderTests.cs at line 304; EnsureNuGetPackageBuildImports Error at line 946
CITATION: QuickFiler/QuickFiler.csproj | BreadcrumbBridgeRouter.cs Compile Include at line 291; QfcItemController.ViewerSetup.cs at line 335; QfcItemController.FolderHandling.cs at line 338; EnsureNuGetPackageBuildImports Error at line 586
CITATION: QuickFiler.Test/QuickFiler.Test.csproj | TargetFrameworkVersion v4.8.1 at line 18 and NO LangVersion element anywhere in the file; BreadcrumbBridgeRouterIssue439Tests.cs Compile Include at line 64; QfcItemController.FolderHandlingTests.Part2.cs at line 182; EnsureNuGetPackageBuildImports Error at line 501
CITATION: global.json | SDK 8.0.205 at line 3; paths ".dotnet-sdk" and "$host$" at lines 6-9; repo-local install-script error message at line 10
CITATION: .csharpierignore | evidence exclusion at line 4; cobertura at line 5; trx at line 8; project-file exclusion rationale at lines 9-14
CITATION: .gitignore | test-results bracket class at line 39; artifacts/ at line 57; coverage/* at line 144
CITATION: coverage.config | ModulePaths Exclude block at lines 12-22 carrying no Test.dll entry
CITATION: .claude/hooks/enforce-evidence-locations.ps1 | artifacts/csharp/ named as permitted at line 26; forbidden prefixes at lines 64-77
CITATION: scripts/vscode/Install-RepoDotNetSdk.ps1 | present in this worktree; named by the global.json error message as the repo-local SDK installer
CITATION: docs/features/active/2026-09-06-breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency-799/spec.md | Acceptance Criteria AC1 through AC8 at lines 834-841; Write Set at lines 645-725; decision D-A at lines 283-338; decision D-B at lines 340-383 with the escalation rule at lines 366-374; decision D-C at lines 385-407; decision D-D at lines 409-446; Test Strategy at lines 759-830
AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6, AC7, AC8
AC-MAPPING: AC1 | IMPLEMENTATION: P2-T2, P2-T5, P2-T13, P2-T14 | TESTS: P1-T7, P1-T8, P1-T13 | EVIDENCE: <FEATURE>/evidence/regression-testing/p1-t16-ut-fail.md and <FEATURE>/evidence/regression-testing/p2-t16-pass-after.md
AC-MAPPING: AC2 | IMPLEMENTATION: P2-T5 | TESTS: P1-T8 | EVIDENCE: <FEATURE>/evidence/regression-testing/p1-t16-ut-fail.md and <FEATURE>/evidence/regression-testing/p2-t16-pass-after.md
AC-MAPPING: AC3 | IMPLEMENTATION: P2-T5, P2-T11 | TESTS: P1-T11 | EVIDENCE: <FEATURE>/evidence/regression-testing/p2-t16-pass-after.md
AC-MAPPING: AC4 | IMPLEMENTATION: P2-T1, P2-T7, P2-T9, P2-T10 | TESTS: P1-T6, P1-T14 | EVIDENCE: <FEATURE>/evidence/regression-testing/p1-t17-qft-fail.md and <FEATURE>/evidence/regression-testing/p2-t16-pass-after.md
AC-MAPPING: AC5 | IMPLEMENTATION: P2-T8 | TESTS: P1-T9 | EVIDENCE: <FEATURE>/evidence/regression-testing/p1-t16-ut-fail.md and <FEATURE>/evidence/regression-testing/p2-t16-pass-after.md
AC-MAPPING: AC6 | IMPLEMENTATION: P2-T11 | TESTS: P1-T11 | EVIDENCE: <FEATURE>/evidence/regression-testing/p1-t17-qft-fail.md and <FEATURE>/evidence/regression-testing/p2-t16-pass-after.md
AC-MAPPING: AC7 | IMPLEMENTATION: P2-T6, P2-T12 | TESTS: P1-T8 for the logging half and P1-T11 for the row-suppression half, specifically BindRowsAsync_ZeroCandidateLabel_SuppressesTheRowAndKeepsSegmentKeysAligned and BindRowsAsync_AmbiguousLabel_IsNotSuppressed | EVIDENCE: <FEATURE>/evidence/regression-testing/p1-t16-ut-fail.md and <FEATURE>/evidence/regression-testing/p1-t17-qft-fail.md and <FEATURE>/evidence/regression-testing/p2-t16-pass-after.md
AC-MAPPING: AC8 | IMPLEMENTATION: P3-T12 | TESTS: P3-T12 | EVIDENCE: <FEATURE>/evidence/qa-gates/p3-t12-ac8-verification.md
UNRESOLVED-GAPS: NONE
DIRECTIVE: PREFLIGHT VALIDATION ONLY
DIRECTIVE NOTE: the line below is the mechanical handoff signal. It is not a discovered defect in this revision; clearance is outstanding because validation-only preflight has NOT been run by this planner, no atomic-executor delegation tool and no MCP plan validator being present in this planner's tool surface.
PREFLIGHT: REVISIONS REQUIRED
The orchestrator must obtain a genuine `PREFLIGHT: ALL CLEAR` from atomic-executor under `DIRECTIVE: PREFLIGHT VALIDATION ONLY`, and a passing `mcp__drm-copilot__validate_orchestration_artifacts` run with `artifact_type: "plan"`, before execution begins. This plan is not self-approved.
CONVERGENCE: NO FURTHER ROUNDS EXPECTED — every accepted item of the round 1 delta is applied, the two narrowed items are applied in their narrowed form, and the one refuted item is left unapplied as instructed. The three judgment calls the previous round flagged (the D5 escalation to Efc-only row suppression, the D8 no-hunk finding on the two #439 Efc router test files, and the D7 additive score projection) were each re-checked by the reviewer against the tree in round 1 and confirmed, so they are no longer open. The only new material introduced in this pass is the nullable-narrowing consequence of the accepted annotation items, which is closed inside the four tasks that consume the annotated members.
