---
name: project-736-efc-archiveroot-boundary-sink-plan-seams
description: "#736 planning seams: EfcFormControllerTests.cs at 485 lines makes the spec Write Set infeasible; research undercounted catch clauses by one; KbdExecuteAsync_ filter collides with QFC; Outlook interop makes Action/Exception/Application ambiguous; an instance property initializer cannot reach this; plus round-1 preflight defects I already had memories for"
metadata:
  type: project
---

Seams found while authoring the #736 atomic plan (EFC archive-root COM guard plus boundary-sink
reporting). Each was invisible from the spec and the research, both of which had been reviewed.

**1. A near-ceiling TEST file can make a spec's Write Set jointly unsatisfiable with its own Test
Strategy.** `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` is **485** lines. The 500-line
ceiling in `.claude/rules/general-code-change.md` covers test code explicitly, so there are 15 lines
of headroom — less than one MSTest method. #736's spec mandated nine new tests in that file while its
`## Write Set` listed no Part2 file and no `QuickFiler.Test/QuickFiler.Test.csproj` entry, and its
AC11 pins the diff to the Write Set. Sibling-partial precedent already exists in the same project
(`BreadcrumbPopupBoundaryCoverageTests.Part2.cs`,
`BreadcrumbDropDownSearchIntegrationTests.Part2.cs`), but `EfcFormControllerTests` is declared
`public class`, not `partial`, so the amendment is three things: the `partial` keyword, the new
`.Part2.cs`, and the legacy csproj `<Compile Include>` entry.

**Why:** the natural move is to size only the PRODUCTION files a plan touches. The test file is where
the line budget actually runs out, and a spec author counting only production files will not see it.

**How to apply:** before accepting any Write Set, measure every TEST file in it and subtract from 500.
If the remainder is smaller than the plan's own test additions, report the Write Set as a blocking
spec defect and name the amendment — do not silently place tests elsewhere. Related:
[[project-489-partn-reroute-amendment-seams]] (verify the parent is `partial` first).

**2. The #736 research undercounted `catch (` in EfcFormController.cs.** §1.3 states 9 and enumerates
{151, 481, 498, 516, 578, 593, 973, 1016, 1020}, omitting **1163** — which the same section's own
cross-check names when it derives the reporter invocation at 1165 from the handler at 1163. The true
count is **10**. Verified separately: the token `TryReportBoundaryFault` occurs 7 times (1 declaration
at 138 + 6 invocations), and `.Dispose()` occurs 2 times (796, 881). Re-derive every count a plan
gates on; a research artifact's own cross-check can contradict its stated total.

**3. `FullyQualifiedName~KbdExecuteAsync_` collects QFC tests too.**
`QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs` already declares
`KbdExecuteAsync_WhenDeactivateKbdTrue_...` and `KbdExecuteAsync_WhenDeactivateKbdFalse_...`, so a
bare method-prefix filter inflates every expected total/passed/failed triple by two. Prefix every
`FullyQualifiedName~` operand with its owning class name (`EfcFormControllerTests.KbdExecuteAsync_`).

**4. `EfcFormController.cs` imports both `System` and `Microsoft.Office.Interop.Outlook`,** so
`Action`, `Exception`, and `Application` are all ambiguous in that file — which is why the existing
declaration at `:128` spells `System.Action<string, System.Exception>`. Any new member must
fully qualify, including `System.Windows.Forms.Application.OpenForms`, or the file will not compile.
`AppOlObjects.cs` solves the same collision differently, with `using Exception = System.Exception;`.

**5. An AC that says "the DEFAULT sink must surface to the user" forces a static seam.** A C#
instance property initializer cannot reference `this`, so a default assigned at
`internal System.Action<string, System.Exception> BoundaryErrorSink { get; set; } = ...` can only
call static members. Making the surfacing observable in a test therefore needs a static injectable
seam, and a plain shared static races under the ClassLevel parallelization in
`scripts/vscode/TaskMaster.cli.runsettings`. The in-repo answer is `AsyncLocal<T>` per-flow storage:
`MyBox.DialogInvoker` (`UtilitiesCS/Dialogs/MyBox.cs:26-45`) carries a comment recording exactly this
reasoning. Reuse that shape rather than inventing one.

**6. `MyBoxViewer` is public but its `TextMessage` control is `internal` to UtilitiesCS**
(`MyBoxViewer.Designer.cs:175`), so QuickFiler cannot set the message on it and cannot reuse
`MyBoxModeless` either (that class is `internal static`). A non-blocking notification raised from
QuickFiler has to build its own `System.Windows.Forms.Form`. Guard it with
`System.Windows.Forms.Application.OpenForms.Count == 0` so the headless test host returns
immediately — the pre-existing `BoundaryErrorSink_DefaultDelegate_InvokesWithoutThrowing` invokes the
default delegate directly and would otherwise construct a window on an MSTest thread.

**7. `SortEmail.Cleanup_Files()` becomes newly reachable when a filer seam short-circuits.** In
`EfcDataModel.MoveToFolderAsync` it sits after the filer call, so it never ran in the finding-6 test
before. It is safe headlessly — `UtilitiesCS.Test` already pins `Cleanup_Files_DoesNotThrow` — but a
plan that adds an overridable filer seam must check what the seam newly exposes downstream.

**8. Round-1 preflight returned 11 defects, and THREE of them were already in my own memory index —
I did not consult it when authoring Phase 0.** The three: (a) `.dotnet-sdk` is absent in an agent
worktree so `dotnet tool restore` fails with global.json's `The repo-local .NET SDK is missing.`
([[agent-worktrees-need-sdk-and-nuget-bootstrap]] step 1, and its step 4 on the `dotnet-coverage`
global tool); (b) a `\.claude\`-absence assertion on discovered assembly paths is unsatisfiable
inside a worktree ([[worktree-root-breaks-dotclaude-exclusion]]); (c) `.claude/agent-memory/**`
residue from sibling agents in the same run defeats a "porcelain names nothing outside the feature
folder" clause ([[agent-memory-is-tracked-scope-git-gates]]). **Read the memory index against the
Phase 0 draft before handing off, not after the executor returns it.**

**9. A `git status --porcelain` clause asserted EMPTY is unsatisfiable in any task that writes its own
evidence artifact after the commit it observes.** P7-T3/P7-T4 each ran after a PRIOR task had written
an uncommitted artifact. The satisfiable form is a NEGATIVE clause: every porcelain line names a path
under the feature folder or `.claude/agent-memory/`, and NO line names a path under the code trees —
that negative is what actually proves every code change reached the commit the anchored diff reads.
A terminal clean-tree proof needs the double-amend shape: commit, write artifact, amend, capture a
genuinely empty span, append it, amend once more with nothing written afterwards.

**10. A three-occurrence source literal read as one.** `OlAncestor = olAncestor,` and
`new EmailFiler(` each occur THREE times in `EfcDataModel.cs` (339/366/390 and 343/370/394), once per
public entry point — `MoveToFolderAsync`(5-param, declared :303), `OpenOlFolderAsync` (:349),
`OpenFsFolderAsync` (:374). Five acceptance clauses said "exactly one". Note `MoveToFolderAsync` is an
OVERLOAD PAIR: the `MAPIFolder`-first overload at :398 contains neither literal. Count every
occurrence AND name its enclosing member before writing a single-occurrence gate.

**11. `MyBoxModeless` cannot be reused from QuickFiler — reconfirmed against a reviewer proposal to
adopt it.** Point 6 above already recorded this; a round-1 reviewer nonetheless proposed adopting its
injectable 5-arg overload as the default notifier. Two independent blockers:
`internal static class MyBoxModeless` (MyBoxModeless.cs:21) and
`UtilitiesCS/Properties/AssemblyInfo.cs:18-20` grants `InternalsVisibleTo` to exactly
DynamicProxyGenAssembly2, UtilitiesCS.Test, ToDoModel.Test — not QuickFiler; and its only entry
points are the 4-arg/5-arg `ShowStoreLockupNotification`, which take a store identity plus three
button actions and hardcode the caption, with no single-string general-notice overload. Also
re-verified: `EmailFiler.SortAsync(IList<MailItemHelper>)` at :128 is NOT virtual, but the
parameterless `SortAsync()` at :137 IS — cite the right overload.

**12. Round 2 returned three BLOCKING defects, all of them introduced by MY OWN round-1 replacement
text.** Round 1's nine other fixes held; the two clauses I rewrote broke in new ways.

- The two path clauses I added to the coverage tasks asserted over runner output that does not exist,
  and compared a forward-slash `git rev-parse --show-toplevel` value against backslash `FullName`
  values. Both recorded in [[worktree-root-breaks-dotclaude-exclusion]].
- **A coverage-floor threshold derived from a HARDCODED count of unreachable lines breaks when one
  "line" is a method BODY.** D2 said "with two uncovered lines fixed, the strict quotient reaches
  90.00% at a denominator of 20" — but one of the two items was "the production body of
  `InvokeFilerAsync`", and a method body carries a sequence point on the opening brace, every
  statement, and the closing brace. So `U != 2`, the `N >= 20` threshold is wrong for the real `U`,
  and the companion clause demanding a single "post-change line number" for that item is
  unsatisfiable. The fix is to parameterize: define `U` as the enumerated set's size, derive the
  floor as `N >= 10U` (since `(N-U)/N >= 0.90` iff `N >= 10U`), and make the escape branch
  "denominator below `10U`". Also state that no expected percentage is pinned, because it depends
  on `U`.

**How to apply:** whenever an unreachable-line exclusion set names a MEMBER or a BODY rather than a
single line, the arithmetic must be symbolic. Count the items, do not count the lines, and never bake
a numeric denominator threshold into the decisions record.

**13. Two AC check-off tasks omitted the artifact for one CONJUNCT of a multi-conjunct AC.** #736's
AC5 has four conjuncts; its fourth (null-sink and throwing-sink branches covered) is delivered two
phases earlier than the other three, so the check-off task cited only the Phase-4 artifacts. AC2's
XML-documentation conjunct was delivered by a pure source-edit task that writes NO artifact at all —
so its citation cannot be an artifact path and must instead be the delivered text, quoted verbatim,
with an acceptance clause that the quoted sentence appears in the named file. **Decompose every AC
into conjuncts and map each conjunct to a task before writing the check-off task's citation list.**

**14. Fixing an exception-type assertion in ONE test case leaves its SIBLING cases exposed to the
same defect.** Round 2 flagged that P1-T5's retry case, described only as "asserts both read
delegates were invoked", could be implemented to pass pre-fix and falsify the pinned 6/2/4 split. My
own sibling sweep then found the redaction case had the identical hole: it asserted only that the
thrown MESSAGE carried no path, and the pre-fix `COMException`'s message carries no path either, so
it too would pass before the fix. Every red-before test case in an `[expect-fail]` split must pin the
EXCEPTION TYPE, not just a property of whatever escaped.

**15. Round 3: a single shared `try` makes a "both delegates invoked twice" assertion unsatisfiable.**
The fix task required exactly ONE `catch (` in the file, so a `COMException` from the FIRST read
short-circuits the second: the second delegate is invoked ZERO times, not twice. The retry test as
written would have stayed red forever against the pinned 6/6/0 green. When a seam wraps N operations
in one `try`, derive the per-delegate invocation counts from the short-circuit, not from the call
count. Argument-evaluation order was the load-bearing detail that made "composed first" provable:
`RequireResolvedArchiveRoot(composedArchiveRootPath, resolvedArchiveFolderPath, log)` declares
composed first and C# evaluates arguments left to right, so the order holds whether the core assigns
locals or inlines the two delegate invocations.

**16. Round 3 found ONE check-off task citing artifacts from artifact-less tasks; my own sweep found
THREE more.** Point 13 above recorded the shape (cite delivered text, not a path). The round-3
reviewer flagged P7-T5 only. Sweeping every check-off task against the set of tasks that actually
write an artifact found the same defect in P7-T6 (cited P1-T8), P7-T7 (P2-T9), P7-T9 (P4-T5), and
P7-T11 (P5-T1/T3/T4) — all pure source edits. **Build the artifact-writing task set once, then diff
every citation list against it**, rather than fixing the one instance a reviewer names. The paired
fix is threading: each artifact-less task names the next artifact-writing task as its recorder, AND
that consumer's acceptance gains a clause requiring the observations, or the requirement is stranded.

**17. A "quote it verbatim" gate is wrap-fragile when the sentence exceeds the file's wrap width.**
The XML-doc sentence is 120 chars; with an 8-space indent and `/// ` it is 132 columns against a
~96-column surrounding block, so an author matching the neighbours would split it and a line-oriented
search would return zero matches. Two fixes together: pin the exact sentence in the plan's own
literals block (so the executor cannot author the wording it is later judged against), and require it
unwrapped on one `///` line. CSharpier does not reflow comment content, so the line survives format.

**18. "Character-for-character identical" is a stronger claim than a copied command usually supports.**
The plan's discovery filter used `'\\bin\\Debug\\'` where the script uses `"\\bin\\$Configuration\\"`.
Same resulting match string — PowerShell does not treat a backslash as an escape inside double quotes
— but different literal text. Say "semantically identical, and here is why", and cite the script's
line range.

**19. Round 4: the plan wrote its own host-token rule into D10 and then never enforced it.** The one
task that asserted it checked artifact **names**, so it passed while ~17 `.trx` and 4 `.min.log`
artifacts carried the account and machine name into the delivery commit. Two acceptance clauses
actively *required* recording a token (a `Get-Command dotnet-coverage` path resolves under
`$env:USERPROFILE`; `git rev-parse --show-toplevel` prints a path containing the account name).
**A rule stated in a decisions record is not enforced until some task's acceptance measures it, and a
name-shaped gate never measures a content-shaped rule.** Details in
[[trx-carries-host-tokens-in-two-casings]].

**20. Round 4's own delta text carried a false citation, and applying it verbatim would have
introduced the defect it was written to close.** The reviewer's DEF-D fix said to write "`/t:Rebuild`
substituted for CI's `/t:Build`" into the **nullable** baseline task. Re-derived:
`.github/workflows/_build-nullable.yml:57` already uses `/t:Rebuild`; it is
`.github/workflows/_build-analyzers.yml:50` that uses `/t:Build`. The reviewer also mislabelled the
nullable task as "the analyzer-baseline task". **Verify a reviewer's factual premise before
transcribing its replacement text**, exactly as the contract requires for one's own citations.

**21. Round 5: an evidence artifact the plan REQUIRES to be committed carried a `.gitignore`-matched
extension, and every gate on it checked only on-disk EXISTENCE.** The four msbuild non-vacuity logs
were named `*.min.log`; `.gitignore:84` is the bare pattern `*.log` with no negation (the only `!`
lines, 137/145/193/219, name `.axoCover/settings.json`, `coverage/.gitkeep`, `**/[Pp]ackages/build/`,
`?*.[Cc]ache/`), and zero `.log` files exist in the worktree, so there was no precedent either. An
ignored file sits on disk exactly like a compliant one, so "the log exists at the evidence path"
passed while AC13's *retained as evidence* conjunct could never be satisfied. Renaming to `.log.txt`
was the only compliant repair — `.gitignore` was outside the ratified Write Set, so adding a negation
would have breached the scope-containment AC. **Two rules:** (a) for any artifact an AC requires to be
COMMITTED, pair the existence clause with a trackedness clause; (b) `git ls-files -- <path>` prints
nothing for a newly written UNTRACKED file whether or not it is ignored, so the clause is unsatisfiable
without a preceding `git add -N <path>` — and it must be `-N`, never `-f`, because an un-forced add of
an ignored path exits non-zero and that non-zero exit is the discriminator
([[untracked-file-and-linecount-gate-seams]]). Sibling sweep found the prose in the host-token
sanitisation task still naming the artifacts by the old `.min.log` extension.

Related: [[project-464-efc-controller-plan-seams]] (budgeted ceiling, not a shrink, on this same
controller file), [[declaration-only-seam-task-for-fail-before]], [[trx-needs-resultsdirectory]],
[[agent-worktrees-need-sdk-and-nuget-bootstrap]], [[worktree-root-breaks-dotclaude-exclusion]],
[[agent-memory-is-tracked-scope-git-gates]].
