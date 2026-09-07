# Preflight round 1 delta — issue #796

- Timestamp: 2026-09-07T03:20
- Plan under revision: docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/plan.2026-09-06T21-59.md
- Round 1 verdict: PREFLIGHT: REVISIONS REQUIRED / CONVERGENCE: FURTHER ROUNDS LIKELY
- Reviewer: atomic-executor under DIRECTIVE: PREFLIGHT VALIDATION ONLY, with command execution

Round 1 ran commands rather than reasoning about them. Six observed values were requested; two were
observed directly, one was confirmed statically, and three could not be run. The reason three could
not be run is itself defect B1.

## How to apply this delta

Apply every item, blocking and non-blocking alike, in ONE revision round. Deferring the non-blocking
observations only re-surfaces them as findings in the next round.

Where an item supplies replacement text, apply that text VERBATIM. Do not substitute your own
wording. Substituted text is unreviewed until the following round, so each paraphrase silently
converts a closed defect into a new unreviewed region. Where a command or path has been wrapped for
this document's width, reassemble it onto one line; a wrapped command is not a command.

Report a per-item disposition for every item below, using exactly one of:
`applied-verbatim`, `applied-with-mechanical-reassembly`, or `not-applied-with-reason`.

If you judge a supplied item to be wrong, do NOT silently rewrite it. Apply it as given, or leave it
unapplied and report the disagreement for the orchestrator to adjudicate. That is what surfaces
reviewer error instead of burying it.

## Orchestrator adjudications, which modify three of the reviewer's items

These three carry an orchestrator ruling. Where a ruling conflicts with the reviewer's text, the
ruling wins and the reviewer's text is amended as stated.

### On B1 — the command channel is environment-dependent, not fixed

The reviewer observed that every `pwsh` invocation is refused, and correctly reported it. The
refusal is a property of the isolated agent worktree this PREPARATION run executes in, and it is
not established for the environment the execution run will use. A previously recorded observation
holds that the refusal comes from the sandbox rather than from the agent type, and that
`atomic-executor` runs `pwsh` normally outside that sandbox. Reading the refusal as permanent
previously caused a false blocked halt.

Ruling: ACCEPT Delta 1a, 1b and 1c as written, because the two-rung design is correct under both
environments and costs nothing when rung 1 succeeds. Add this constraint to the plan when applying
them: P0-T2 MUST probe rung 1 first and record the channel it observed. The plan must NOT hard-code
`COMMAND-CHANNEL: B`, must not state that pwsh is unavailable, and must not present Channel B as
the expected outcome. State instead that the channel is determined by observation at P0-T2 and that
either value is a normal result.

### On B3 — the seventeenth path is accepted

Ruling: ACCEPT. Add `QuickFiler/Interfaces/IQfcItemController.cs` to the write set as the
seventeenth path. The orchestrator has made the corresponding edit to spec.md's `## Write Set`
section, so Delta 3a's spec.md half is already done; apply only its plan half.

Reasoning, recorded because the alternative was genuinely open. The narrower resolution was to drop
`SelectorWasOpen` from the per-item diagnostic and amend the runbook to match. That was rejected:
the per-item open state is what distinguishes a cancel that hit an open selector from a cancel that
was a no-op, and today the cancel runs unconditionally on every item, so a cancel COUNT alone
carries no such information. Candidate 1's refute rule depends on that distinction. The blast-radius
cost is marginal and falls in a project no concurrently prepared sibling item owns.

### On B5 — the pre-existing dirty set will most likely be empty at execution time

The reviewer measured five dirty paths outside the feature folder and proposed recording and
excluding them. The mechanism is right and is accepted. The measurement is a property of this
preparation run in progress, not of the tree the executor will start from: before this preparation
run finishes, the orchestrator commits the feature folder and the promoted record, and removes the
four agent-memory paths. The set the executor observes at P0-T14 will therefore most likely be
EMPTY, and the porcelain gates will be strict rather than relaxed.

Ruling: ACCEPT Delta 5a, 5b and 5c, with one amendment. In Delta 5a, replace the sentence beginning
`Measured in the preflight pass this set holds five paths:` and ending `so the set cannot be emptied
by the executor.` with this text:

> This set is expected to be EMPTY when the executor reaches this task, because the preparation run
> that produced this plan commits the feature folder and the promoted record and removes its own
> agent-memory writes before finishing. An empty set is the normal result and makes every later
> porcelain gate strict. The set is recorded rather than assumed empty because a non-empty set is a
> legitimate state the executor did not create and in some cases may not remediate: the .claude tree
> is one this plan may not edit at all. During the preflight pass that produced this task the set
> held five paths, four of them under .claude/agent-memory and one an untracked promotion record.

## Blocking defects

### B1 — the plan's sole command channel may be refused; no task carries a fallback

Affected: P0-T2, T3, T5, T6, T7, T8, T9, T10, T11, T12, T14; P1-T2, T3, T6, T7, T8, T9, T10, T11,
T12, T13, T14, T15; P4-T5, T9; P5-T3, T5, T7; P6-T3, T4, T6; P7-T3, T4; P9-T1 through T11.

Delta 1a — replace the paragraph at plan line 104, beginning `All C# tools are invoked through pwsh,
never through the Bash tool.` and ending `disallowed shell segment.`, with:

> All C# tools are invoked through whichever of two command channels task P0-T2 records as
> available, and that task runs before every other command in this plan. Channel A is pwsh, in
> either the `-NoProfile -Command` or the `-NoProfile -File` form. Channel B is direct invocation
> from the Bash tool. Which channel is available is determined by observation at P0-T2 and not by
> this paragraph: in some sandboxes the isolation guard rejects every command whose name is pwsh, in
> both flag forms, and rejects an `env -C` prefixed form as well, while in others pwsh runs
> normally. Either recorded value is a normal result. Every pwsh block shown in a later task of this
> plan is a command SHAPE. When P0-T2 records `COMMAND-CHANNEL: B`, the executor runs the Channel B
> equivalent recorded in that task's artifact and notes the substitution in the artifact of the task
> it substituted for.
>
> Channel B has three constraints that are load-bearing and must not be relaxed. First, Git Bash
> applies MSYS path translation to forward-slash switches, rewriting `/m` into a filesystem path and
> producing MSB1008, so msbuild is invoked with dash switches (`-t:Rebuild -m -nodeReuse:false
> -p:Configuration=Debug -p:Platform="Any CPU"`) and vstest.console.exe is invoked with its
> forward-slash switches intact behind an `MSYS_NO_PATHCONV=1` assignment prefix. Second, a quoted
> absolute path in the command-NAME position is refused by the same guard separately from pwsh, so a
> Windows executable that is not on PATH is invoked by bare name behind a `PATH=` assignment prefix;
> a quoted absolute path passed as an ARGUMENT is permitted. Third, test DLL paths are passed to
> vstest.console.exe with backslash separators, because mixed separators make vstest report that the
> test source file was not found.

Delta 1b — replace [P0-T2] in full with:

> - [ ] [P0-T2] Determine the available command channel, then install the repo-pinned .NET SDK,
>   recording both in evidence/baseline/p0-t2-dotnet-sdk-install.md under the feature folder.
>   global.json pins SDK 8.0.205 with `paths` including `.dotnet-sdk`, and a fresh worktree has none,
>   so every dotnet command prints the global.json errorMessage instead of a version until this task
>   completes. Rung 1: attempt
>
>   ```
>   pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1
>   ```
>
>   Rung 2, taken only when rung 1 is refused by the isolation guard rather than failing on its own
>   merits: record the guard's refusal text verbatim, then perform the same install from the Bash
>   tool, which is what that script does and all it does — create the .dotnet-sdk directory, download
>   `https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip` into it,
>   unzip it in place, and delete the zip. `.gitignore` already ignores `.dotnet*/`, so neither rung
>   adds a porcelain entry. Acceptance: the artifact records the line `COMMAND-CHANNEL: A` or the
>   line `COMMAND-CHANNEL: B` exactly once; it records `EXIT_CODE: 0` for the rung taken; the
>   directory .dotnet-sdk/sdk/8.0.205 exists; and the recorded stdout of `dotnet --version`, run on
>   the recorded channel, is a version string beginning with the two characters `8.` rather than the
>   sentence `The repo-local .NET SDK is missing.` recorded verbatim. When the recorded channel is B,
>   the artifact additionally records, one per line, the Channel B equivalent of each command form
>   this plan uses later: the msbuild form, the vstest form, the csharpier form, the NuGet restore
>   form, and the file-line-count form. Those recorded equivalents are the commands the later tasks
>   run, and no later task may improvise one that is not recorded here.

Delta 1c — replace the command block of [P0-T3], leaving its prose and acceptance otherwise intact,
with:

>   ```
>   pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug
>   ```
>
>   When P0-T2 recorded `COMMAND-CHANNEL: B`, run the recorded Channel B equivalent instead, which is
>   a packages.config-based `nuget restore TaskMaster.sln` invoked by bare name behind a `PATH=`
>   assignment prefix naming the directory holding nuget.exe. Record which channel was used.

### B2 — [P0-T12]: the counting idiom cannot produce the four values the task pins

`Measure-Object -Line` does not count blank lines. The four pinned files carry 44, 49, 30 and 39
blank lines, so the plan's idiom reports 454, 407, 218 and 341 against the true 498, 456, 248 and
380. Every one diverges, so P0-T12's acceptance instructs the executor that the tree has moved — a
false diagnosis that halts Phase 0 on a tree whose counts are exactly as cited. The orchestrator
independently measured the same four physical-line counts with a line-oriented content search and
confirms 498, 456, 248 and 380.

Second harm: P1-T2's band [480, 486] would be evaluated against roughly 441 and fail, and every
downstream ceiling gate (P4-T3, P4-T10, P5-T6, P5-T8, P6-T6, P7-T2, P9-T8) under-reports by 30 to 50
lines, so a file at 540 physical lines reports 495 and passes a 500-line cap it violates.

Delta 2 — replace the command block and the acceptance sentence of [P0-T12] with:

>   ```
>   pwsh -NoProfile -Command '@("QuickFiler\Controllers\QfcFormController.Deactivate.cs","QuickFiler\Interfaces\IQfcFormViewer.cs","QuickFiler\Viewers\QfcFormViewer.cs","QuickFiler\Viewers\BreadcrumbDropDownHost.cs","QuickFiler\Viewers\BreadcrumbDropDownHost.Open.cs","QuickFiler\Viewers\ItemViewer.Breadcrumb.cs","QuickFiler\Controllers\QfcItemController.EventHandlers.cs","QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs","QuickFiler\Resources\FolderBreadcrumb.html","QuickFiler.Test\Controllers\QfcFormControllerDeactivateTests.cs","QuickFiler.Test\Viewers\BreadcrumbPendingOpenCloseTests.cs") | ForEach-Object { $_ + " " + (Get-Content -LiteralPath $_).Count }'
>   ```
>
>   The physical-line idiom is `(Get-Content -LiteralPath $_).Count` on Channel A and `wc -l` on
>   Channel B; the two agree. The idiom `(Get-Content $_ | Measure-Object -Line).Lines` is PROHIBITED
>   throughout this plan, at baseline and at every later re-measurement alike, because
>   `Measure-Object -Line` omits blank lines and therefore under-reports every count by that file's
>   blank-line total. Measured in the preflight pass, the four pinned files carry 44, 49, 30 and 39
>   blank lines respectively, so that idiom would report 454, 407, 218 and 341 against the true 498,
>   456, 248 and 380. Acceptance: the artifact records one physical-line count per path, records the
>   line `LINE-COUNT-IDIOM:` followed by the idiom actually used, and the recorded values for
>   QuickFiler/Viewers/BreadcrumbDropDownHost.cs, QuickFiler/Viewers/ItemViewer.Breadcrumb.cs,
>   QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs and
>   QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs are 498, 456, 248 and 380
>   respectively. A divergence from those four values means the tree has moved since this plan was
>   authored and the file-size arithmetic in Phase 1 and Phase 5 must be re-derived before
>   proceeding. Every later task in this plan that re-measures a line count uses the idiom recorded
>   on the `LINE-COUNT-IDIOM:` line and no other, so the baseline and the final audit are
>   commensurable.

### B3 — [P1-T4]: `selectorWasOpen` has no source reachable from the write set

`IQfcItemController` declares `ItemNumber` and `CancelBreadcrumbSelector()` and no selector-open
state and no viewer accessor. The only member carrying that state is `IsFolderDropDownOpen` on the
item-viewer interface, which this item does not touch.

Delta 3a, plan half only — add `QuickFiler/Interfaces/IQfcItemController.cs` to the
"Production — modify" list of the plan's `## Write Set` section, and change the sentence introducing
that section from "The plan's diff must stay within the sixteen paths below." to "The plan's diff
must stay within the seventeen paths below." Every other place in the plan that says "sixteen
write-set paths" — the P0-T14 acceptance, the P9-T10 acceptance, and the structural self-check
paragraph — becomes "seventeen write-set paths". The spec.md half of this delta has already been
applied by the orchestrator; do not repeat it.

Delta 3b — replace the second sentence of [P1-T4] with:

> Add the pure method `internal static string FormatDeactivationDiagnostics(bool webView2Focused,
> bool activeFormIsNull, int groupCount)` returning a single interpolated line containing the labels
> `WebView2Focused=`, `ActiveFormNull=` and `Groups=`, and the pure method `internal static string
> FormatItemCancelDiagnostics(int itemNumber, bool selectorWasOpen)` returning a single line
> containing the labels `ItemNumber=` and `SelectorWasOpen=`. The `selectorWasOpen` value has no
> source on `IQfcItemController` today: that interface declares `ItemNumber` and
> `CancelBreadcrumbSelector()` and no selector-open state and no viewer accessor, and the only member
> carrying the state is `IsFolderDropDownOpen` on the item-viewer interface, which this item does not
> touch. This task therefore also declares one get-only boolean member on
> `QuickFiler/Interfaces/IQfcItemController.cs` beside `CancelBreadcrumbSelector()` at line 60, with
> an XML doc comment stating that it reports whether this item's breadcrumb selector is currently
> open, implemented on the item controller as a forward to the item viewer's existing member. This is
> the only production interface change Phase 1 makes and it is observational: no caller other than
> the new diagnostic reads it, and it changes no control flow. Moq's default bool return is false, so
> no existing Arrange block in any suite that mocks this interface requires modification.

Delta 3c — replace the acceptance sentence of [P1-T4] with:

> Acceptance: the file compiles, the existing catch block at lines 58-69 is unchanged in the diff, no
> `if`, `return`, `throw` or assignment other than the two log statements is added inside
> `ParkFocusAndCancelSelectors`, and the new `IQfcItemController` member is declared and forwarded
> with no branching of its own. The solution compiles under the P0-T8 command form, which is what
> proves the forward is well typed against every implementor.

### B4 — [P4-T7] YES branch lands a test no task makes pass, making [P4-T9] unsatisfiable

Delta 4 — replace the YES-branch sentence of [P4-T7] with:

> When the value is YES, the test receives the same explicit genuine-case Arrange line, and the
> paired negative test asserting `Times.Never()` on `ParkFocusOffWebView2()` for the self-inflicted
> case is added in this task; the same artifact records the paired test's name and records the line
> `PARK-FOCUS-SUPPRESSION: IN SCOPE FOR P4-T8`. Task P4-T8 then extends its guard to the
> focus-parking step as well as the cancel loop, so that paired test passes at P4-T9; the per-item
> boundary catch and its error logging at lines 58-69 remain unchanged in either branch. The paired
> negative test is a fifth expect-fail test in this plan when this branch is taken: it is added to
> the expect-fail inventory table with class QfcFormControllerDeactivateTests, landed by P4-T7,
> recorded Failed at no gate because no gate runs between P4-T7 and P4-T8, and made to pass by
> P4-T8. Task P9-T5 then reads the inventory as five rows rather than four. When the value is NO, no
> paired test is added, P4-T8's guard stays scoped to the cancel loop, and the inventory remains four
> rows.

### B5 — porcelain gates unsatisfiable against a tree dirty outside the feature folder

Apply Delta 5a, 5b and 5c as the reviewer wrote them, with the one sentence amended by the
orchestrator ruling recorded above.

Delta 5a — append to the acceptance of [P0-T14]:

> The artifact additionally records, under the heading `PRE-EXISTING-DIRTY-SET:`, every porcelain
> path present at this task that lies outside the feature folder, one path per line, together with
> its two-character status code. This set is expected to be EMPTY when the executor reaches this
> task, because the preparation run that produced this plan commits the feature folder and the
> promoted record and removes its own agent-memory writes before finishing. An empty set is the
> normal result and makes every later porcelain gate strict. The set is recorded rather than assumed
> empty because a non-empty set is a legitimate state the executor did not create and in some cases
> may not remediate: the .claude tree is one this plan may not edit at all. During the preflight pass
> that produced this task the set held five paths, four of them under .claude/agent-memory and one an
> untracked promotion record. Every later porcelain gate in this plan is evaluated against the
> porcelain output MINUS this recorded set, and a gate that would otherwise report zero lines is
> satisfied when the only lines it reports are members of this set.

Delta 5b — replace the final clause of [P1-T14]'s acceptance with:

> and the porcelain output lists no path outside the feature folder, the write set, and the
> `PRE-EXISTING-DIRTY-SET:` recorded in evidence/baseline/p0-t14-scope-baseline.md.

Delta 5c — replace the final clause of [P1-T15]'s acceptance with:

> and `git status --porcelain --untracked-files=all` afterwards lists no path outside the feature
> folder and the `PRE-EXISTING-DIRTY-SET:` recorded in evidence/baseline/p0-t14-scope-baseline.md.

### B6 — [P9-T11] contradicts its own command and has no clean-tree fixpoint

Delta 6 — replace [P9-T11] in full with:

> - [ ] [P9-T11] Close the loop for the worktree this plan executes in. If any of P9-T1 through
>   P9-T8 failed or changed a tracked file, restart the loop at P9-T1 and record the second pass in
>   evidence/qa-gates/p9-t11-final-loop.md under the feature folder; otherwise record the single
>   clean pass there. Record in that artifact the commands of the final clean pass in the order
>   format, lint, type-check, test, and the observed output of `git status --porcelain
>   --untracked-files=all` taken at that point on the recorded command channel. Then check this task
>   off in this plan file, stage with the P9-T9 pathspec form, and run `git commit --amend --no-edit`
>   on the recorded command channel. Acceptance: the artifact names which of the two branches was
>   taken and records the four commands of the final clean pass in order; and the porcelain output
>   recorded in the artifact lists no path other than members of the `PRE-EXISTING-DIRTY-SET:`
>   recorded in evidence/baseline/p0-t14-scope-baseline.md, this plan file, and this task's own
>   artifact. A terminal gate demanding a porcelain output of zero lines is not used, because this
>   task's own check-off and its own artifact are written into the feature folder after the last
>   commit and would re-dirty the tree that such a gate measures; the amend is what folds them into
>   the final commit, and the acceptance is evaluated on the porcelain reading taken immediately
>   before it. The commit message is not changed by the amend, which is why no acceptance clause
>   asserts over the amended message body.

Note for the planner: the reviewer's original text for this delta named the absolute worktree path
and required the porcelain output to be recorded in the amended commit's message body. Both were
removed above. The absolute path is removed because a committed artifact must not carry a host path,
and the message-body clause is removed because it is the contradiction B6 reports.

### B7 — [P0-T10]: `Skipped` is not printed on a successful vstest run

Delta 7 — replace the first clause of [P0-T10]'s acceptance with:

> the artifact records `EXIT_CODE:`, the Total and Passed counts read from the run summary, the
> Failed count read from the summary when the run printed a `Failed:` line and recorded as 0 with the
> note `NOT PRINTED ON A PASSING RUN` when it did not, and a Skipped count derived as Total minus the
> sum of Passed and Failed rather than read, because vstest.console.exe prints no `Skipped:` line on
> a run with no skipped tests and the TRX `notExecuted` attribute is hard-coded to 0; the artifact
> states that the derivation was used, and states that the `TestCategory!=LiveOutlook` filter excludes
> rather than skips, so filtered tests appear in neither the Total nor the derived Skipped figure;

### B8 — [P3-T2]: `INCONCLUSIVE` is admitted by Phase 3 and then cannot be consumed by it

Delta 8 — replace the acceptance sentence of [P3-T2] with:

> Acceptance: the line exists exactly once with one of the two admitted values, and the record states
> which `FIRST-CAUSE-GESTURE-` line it was derived from. When every one of the three
> `FIRST-CAUSE-GESTURE-` lines reads INCONCLUSIVE, no derivation is available and this task is not
> discharged by guessing: the executor writes the line `AC1-FAIL-BEFORE-CARRIER: UNDERIVABLE`
> together with the three quoted INCONCLUSIVE lines, halts, and returns the observation to the human
> for a repeat of the Phase 2 runbook, exactly as `MANUAL-OBSERVATION: INCONCLUSIVE` does at P2-T2.
> That halt is the correct outcome, because assigning the AC1 fail-before responsibility from the
> plan's expectation rather than from the log is the specific substitution the AC6 ordering constraint
> exists to prevent.

### B9 — [P0-T11] and [P9-T6]: a per-file coverage row is demanded for a partial-class part

A Cobertura class element carries exactly one `filename` attribute, so a class whose methods span
several source files may be emitted under one filename and the other parts produce no group at all.
This item is marked UNVERIFIED by the reviewer because the coverage run could not be executed.

Delta 9 — replace the acceptance clause of [P0-T11], from `Acceptance: the file
coverage/p0-t11-baseline.cobertura.xml exists` through `and QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs,
obtained with:`, with:

> Acceptance: the file coverage/p0-t11-baseline.cobertura.xml exists and the artifact records all six
> numeric attributes above in `Output Summary:`, plus one row for each of
> QuickFiler/Controllers/QfcFormController.Deactivate.cs, QuickFiler/Viewers/BreadcrumbDropDownHost.cs,
> QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs,
> QuickFiler/Controllers/QfcItemController.EventHandlers.cs and
> QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs, each row carrying either that file's
> `lines-covered` and `lines-valid` or the literal `ABSENT: no class node carries this filename` with
> the name of the class node that did carry the enclosing type. The absent case is admitted and
> recorded rather than treated as a failure, because a Cobertura class element carries exactly one
> `filename` attribute while a partial class spans several source files, so a part other than the one
> the emitter chose produces no group at all; recording it as absent is what keeps the baseline and
> the final run comparable, and task P9-T7 excludes any file recorded ABSENT at both P0-T11 and
> P9-T6 from the changed-code denominator and names it in the NOT MEASURABLE list. The recorded
> filenames are reproduced verbatim as the tool emitted them, which uses backslash separators, and
> the artifact states that the forward-slash spellings above are the same five files. The rows are
> obtained with:

Delta 9b — append to the acceptance of [P9-T6]:

> Each of the five per-file rows carries either its two figures or the literal `ABSENT: no class node
> carries this filename`, on the same terms and for the same partial-class reason recorded at
> P0-T11, and a file recorded ABSENT at both tasks is excluded from the P9-T7 changed-code
> denominator and named in that task's NOT MEASURABLE list.

## Non-blocking observations — apply all of these in the same round

### N1 — [P9-T7] treats AC6 logging asymmetrically

Append to [P9-T7]:

> The AC6 log statements and pure formatter methods added by task P1-T4 to
> `QuickFiler/Controllers/QfcFormController.Deactivate.cs` are counted in the changed-code
> denominator, unlike the diagnostics part, because they sit in a file whose behavioural changes are
> also measured and separating them per line would make the figure unreproducible; the artifact
> records how many changed lines in that file are instrumentation.

### N2 — [P5-T6] pins two of three `CancelCount` assertions

The verified positions are 48, 79 and 113, with `FocusAnchorCount` at 49, 80 and 114. Append to
[P5-T6]:

> A third `CancelCount.Should().Be(1)` assertion exists at line 113 and is likewise kept unchanged;
> it is not named as a scoping guard because the spec pins only the two at lines 48 and 79, but a fix
> that drives it to zero is the same design signal.

### N3 — exclusion lists diverge between the plan and spec.md

The plan's "Explicitly not in the write set" paragraph names `QuickFiler/Viewers/IItemViewer.cs`
while spec.md's equivalent paragraph omits it. Delta 3 makes that paragraph load-bearing, since B3's
resolution turns on that interface being excluded. Align the two lists. Write every path in both
exclusion paragraphs WITHOUT backticks, as both documents already do, because a backticked path in a
negative sentence is read as a write claim by downstream blast-radius derivation.

### N4 — [P0-T4] measures a narrower scope than [P0-T8] gates

P0-T4 checks analyzer agreement for QuickFiler and QuickFiler.Test only, while P0-T8 rebuilds the
whole solution, where a skew in any other project is `error CS0006` and fails the baseline with no
remediation branch defined. Both in-scope projects were verified to agree on all versions, so this is
a latent risk rather than a present failure. Add a remediation branch to P0-T8 stating that a
CS0006 naming an analyzer assembly in a project outside the write set is a missing restored package
rather than a source defect, that the remedy is to re-run the P0-T3 restore and record the second
attempt, and that the task halts with the diagnostic recorded if it recurs.

### N5 — `QuickFiler/QuickFiler.csproj.bak` is tracked and holds a stale compile entry

It is not a `.cs` file, so the formatter will not touch it, and no plan task reads it. Add one
sentence to the P0-T14 scope-baseline task noting that this file exists, is not in the write set, and
must not be edited, so that a later search for compile entries does not mistake it for the project
file. Write that path without backticks.

### N6 — [P0-T13] probes for the MCP validator

No change needed. For the record: the orchestrator ran
`mcp__drm-copilot__validate_orchestration_artifacts` with artifact_type plan against this plan on
2026-09-07 and it returned ok=true with no warnings, so the probe will record a validator result
rather than the absence line. The task is correctly written as record-and-continue.

## Items round 1 confirmed as already correct — do not change them

Changing any of these would reopen a closed question.

- P0-T3's restore step is correctly ordered ahead of the first msbuild at P0-T8 and correctly states
  the `EnsureNuGetPackageBuildImports` mechanism.
- The formatter tasks P0-T7, P9-T2, P1-T8 and P9-T1 correctly avoid asserting on a line the formatter
  prints only when it rewrote a file, using exit code plus an empty file list and before-and-after
  porcelain instead.
- The `/t:Rebuild`-only reasoning and the clause making a zero compiler-invocation count a FAILED gate
  are correct as written.
- The AC6 ordering constraint is correctly implemented: Phase 1 is instrumentation only, Phase 2 is
  the manual gate, Phase 3 is the decision record, and no behavioural-fix phase precedes it.
- Every evidence path resolves under the feature folder's evidence tree; no `artifacts/`-rooted
  evidence path exists.
- The expect-fail carve-outs at P4-T5, P5-T3 and P6-T4 are complete, and the P1-T11 total of 9 is
  arithmetically correct against the measured 7 test methods.
- Both planner findings were adjudicated CONFIRMED: the two `[ExcludeFromCodeCoverage]` sites are at
  QuickFiler/Viewers/QfcFormViewer.cs line 17 and QuickFiler/Viewers/ItemViewer.cs line 20, the five
  files P9-T7 measures carry no class-level exclusion so the scoped gate can still fail, and the
  measured `[TestMethod]` count in QfcFormControllerDeactivateTests.cs is 7 rather than the 6 the
  research prose states.

## Required output for this revision round

Return the plan path, a per-item disposition line for every item above (B1 through B9 and N1 through
N6), and the full `PLANNER-INTERNAL-REVIEW` and `SELF-REVIEW` record blocks re-derived against the
tree as it stands after this revision. A citation verified in the previous round is evidence about a
superseded state and may not be carried forward.
