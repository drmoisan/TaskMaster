# 2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed (Plan)

- **Issue:** #637
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-31T10-22
- **Status:** Ready for Codex preflight
- **Version:** 0.8
- **Work Mode:** full-bug (from `issue.md`); `spec.md` is the sole acceptance-criteria source (AC1-AC30).

## Conventions (read before executing any task)

**FEATURE_DIR** — the feature folder for this issue is
`docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`.
Every evidence path in this plan is written relative to FEATURE_DIR
(for example `evidence/baseline/p0-t12-csharpier-check.md` means
`FEATURE_DIR/evidence/baseline/p0-t12-csharpier-check.md`). Commands that require a literal pathspec
or a literal search operand spell the folder path in full rather than using the name FEATURE_DIR,
because a command carrying a placeholder cannot be executed verbatim. The sites that spell it in full
are the git pathspecs described under "Git pathspec scoping", the token scan in P7-T10, and the two
host-identity redaction scans in P6-T5 and P7-T11.

**Working directory** — every command below runs with the current directory set to the worktree root,
the directory containing `TaskMaster.sln` and `global.json`. All repository-relative paths in commands
resolve against that root. The worktree is chosen when the plan is executed and is deliberately not
named here: no command in this plan needs an absolute path, and writing one into a committed artifact
would embed a host account name. The placeholder convention this feature already uses for a worktree
path in prose is the one at `research/research.2026-08-29T12-30.md:6`,
`<repo-root>/.claude/worktrees/<worktree-id>`.

**Base commit** — the diff anchor for this plan is the literal commit
`0eda184ca0009bc79ac9b7146897270c17c095fa`. Every `git diff` in this plan supplies it explicitly. No
task pins a HEAD SHA: no acceptance condition in this plan asserts that `HEAD` equals a stated
value, and P0-T8 records `git rev-parse HEAD` as an observation rather than as a gate. This anchor
is the third this plan has carried, and both supersessions have the same cause. The plan was first
authored against `ecdb1c84`. `origin/main` then advanced to `fa2ddefa` (pull request #700,
issue #638) and that work was merged into this branch, which left `ecdb1c84` an ancestor of `HEAD`
but no longer a clean pre-change baseline, because files under `QuickFiler` and `QuickFiler.Test`
that this plan does not own then differed between it and `HEAD`; the anchor moved to `b9476588`.
`origin/main` has since advanced to `69aa28dd` (pull request #702, issue #644), and that work was
merged into this branch at `0eda184ca0009bc79ac9b7146897270c17c095fa`. That merge put `b9476588`
in exactly the position `ecdb1c84` had been in: the issue #644 navigation key-ledger work the merge
brought in lies between `b9476588` and `HEAD`, so a diff anchored there reports files this plan does
not own, and the gates that enumerate or scope over a whole tree — P0-T8's empty-diff baseline
proof, P6-T6's exact ten-path enumeration, P8-T30's `QuickFiler QuickFiler.Test` diff, and P5-T5's
tree-wide `QuickFiler.Test` diff — would be unsatisfiable as written. Anchoring at the merge commit
puts all of that work behind the anchor.
`0eda184ca0009bc79ac9b7146897270c17c095fa` is the post-merge, pre-change baseline, and P0-T8 proves
that property by a check that can fail rather than by an ancestry check that cannot. The proof is
not vacuous merely because this anchor is the branch tip at the moment this plan was amended.
P0-T8's diff fails as soon as any file under `QuickFiler` or `QuickFiler.Test` differs between the
anchor and `HEAD` in the checkout the executor actually runs in, and its porcelain companion fails
on any staged, unstaged or untracked change in those same trees. The anchor stops being the tip
at P6-T6, after which every later anchored diff carries this plan's own changes.

**Git pathspec scoping** — `.claude/` is a tracked directory in this repository and carries unrelated
in-flight modifications, and `docs/features/parallel/` and `artifacts/` are owned by other processes.
Every `git status --porcelain` and `git diff` gate in this plan is therefore scoped with an explicit
pathspec naming only first-party source, test and feature-document trees. The feature-document
component of every such pathspec is this feature's own folder and never the parent directory
`docs/features/active`. That narrowing is load-bearing rather than cosmetic, and it is required
prospectively rather than by any present-tense observation of the tree. Whether a given sibling
folder under `docs/features/active` is tracked, untracked, or modified is a worktree-local property
that does not travel with the branch: this repository carries several concurrent worktrees on
different branches, and the checkout in which this plan was authored is not necessarily the one in
which it is executed. This plan therefore records no claim about the present tracking state of any
sibling folder and does not depend on one. The executor also runs later than this planning pass, and a
concurrent run in this checkout can leave an untracked or modified sibling folder under
`docs/features/active` at any point between planning and execution. A `git add`
over the parent directory would then stage and commit another feature's folder onto this branch, and a
`git status --porcelain` over the parent directory would report that folder and make every emptiness
gate that consumes it unsatisfiable. This plan does not assume that the tree it observed at planning
time is the tree the executor will meet, so every gate is scoped to paths this plan owns. The default
pathspec is therefore
`-- QuickFiler QuickFiler.Test docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`;
P6-T6 narrows it to `-- QuickFiler QuickFiler.Test` for its post-commit cleanliness check, and P8-T30
widens it to the nine production and test trees that task audits. No pathspec in this plan names
`.claude/`, `docs/features/parallel/`, `artifacts/`, or the bare `docs/features/active`.
An unscoped gate is unsatisfiable here and must not be substituted.

**Evidence artifact schema** — every command-step artifact records, as separate lines:
`Timestamp:` (ISO-8601 `yyyy-MM-ddTHH-mm`), `Command:` (the exact command), `EXIT_CODE:`, and
`Output Summary:`. A task whose command is expected to exit non-zero additionally records
`ExpectedExitCode:` with that integer. Baseline and final-QC test artifacts additionally record the
numeric coverage headline values named in their task text. Canonical evidence kinds used by this plan
are `evidence/baseline/`, `evidence/regression-testing/`, `evidence/qa-gates/`, and `evidence/other/`.
`evidence/coverage/` is not a canonical kind and is not used. Nothing under `artifacts/` is used for
evidence.

**Nullable opt-in token discipline.** No evidence artifact under `evidence/` may spell the literal
token that P7-T10 scans for. Where a task requires recording that the solution-wide nullable opt-in
property is absent from a `Command:` line, the artifact records that fact on a line beginning with
the key `NULLABLE_OPT_IN_PROPERTY:` — the short form the tasks below name is
`NULLABLE_OPT_IN_PROPERTY: absent`, and the long form is
`NULLABLE_OPT_IN_PROPERTY: absent from the recorded Command line`; either satisfies the rule — and
quotes the `Command:` line verbatim, which carries the proof without reproducing the token. The task
text below is the binding form where the two differ. Exactly one artifact cannot satisfy
this rule: `evidence/qa-gates/p7-t10-toolchain-audit.md` must record its own scan command, and that
command's pattern is the token itself. P7-T10's scan is restricted to this feature's own folder, and
within that folder it excludes that single file by an explicit `--glob` exclusion stated in P7-T10;
no other evidence artifact of this feature is excluded from it. Evidence artifacts belonging to other
feature folders lie outside the scan's directory operand entirely, and this plan neither reads nor
changes them.

**Evidence transcript redaction (host-identity discipline).** Every artifact this plan writes under
`evidence/` is committed, and several of them record captured stdout verbatim. MSBuild prints absolute
project paths, and vstest names its TRX file after the account and the machine, so a verbatim
transcript carries the host account name and the machine name into a committed file. This repository
has already had to promote host-identity leaks in committed artifacts to a tracked issue, and no hook
catches this class: `enforce-evidence-locations.ps1` checks where an artifact is written, not what it
contains. Therefore, before any evidence artifact is written, every occurrence of the absolute
worktree path inside the captured text is replaced with the literal `<worktree-root>`, and every
remaining absolute path that begins with the Windows per-user profile root is replaced with
`<user-profile>`. A third replacement is required and is not covered by the first two: vstest composes
its results file name from the account name and the machine name, so a transcript line naming that
file still carries host identity after its directory prefix has been redacted. Every occurrence of a
vstest results file name is therefore replaced with the placeholder `<trx-file>`, which deliberately
carries no extension, so that the redaction can be gated by searching for the extension itself. The rule applies to every recording task in this plan — P0-T12, P0-T13, P0-T14,
P2-T8, P2-T9, P3-T4, P4-T3, P5-T6, P6-T4, P7-T1, P7-T2, P7-T3 and P7-T4 — and to the TRX-derived
content P2-T13 quotes. Two sweep tasks verify it, P6-T5 before the Phase 6 commit and P7-T11 before
the Phase 7 commit. No third sweep is scheduled for Phase 8: every Phase 8 artifact records
repository-relative paths, search results and check-off statements, and none captures a toolchain
transcript. The per-write rule above nonetheless binds every Phase 8 artifact as well.
Each sweep task runs two searches over this feature's evidence tree and requires zero matches from
both: one for the fixed string `C:\Users`, and one for the fixed string `.trx`. That two-component
path prefix is sufficient for the first search and is deliberately the whole pattern: on Windows the
account segment always follows it immediately, so a
zero-match result over the prefix proves the account segment is absent too, and the account name is
therefore never spelled anywhere in this plan or in any artifact it produces. The second search covers
the results-file case the first cannot reach, because that file name carries the account and the
machine with no preceding profile path. Each sweep task excludes
its own artifact from its own scan by an explicit `--glob` exclusion, for the same reason P7-T10
excludes its own artifact: the artifact must record its own `Command:` line, and that command's
pattern is the string being searched for. P7-T11 additionally excludes P6-T5's artifact, because that
earlier artifact carries the same `Command:` line and is inside P7-T11's scan scope. The restriction
to the `**/evidence/**` subtree is
load-bearing for the same reason it is in P7-T10: this plan file and `spec.md` both sit at the root of
the feature folder, and this plan file necessarily spells the search string in the two task texts that
run the gate, so a scan of the folder without the evidence-subtree glob could never return 0.

**PowerShell invocation form** — every MSBuild and vstest command is issued through
`pwsh -NoProfile -Command '...'` with outer single quotes and inner double quotes. A bare `/m` passed
to a POSIX shell layer is rewritten to a path and MSBuild fails with MSB1008. Every acceptance
condition expressed as a PowerShell expression is likewise issued through
`pwsh -NoProfile -Command '...'` with outer single quotes and inner double quotes. The
`(Get-Content -LiteralPath ...).Count` sites are P1-T7, P2-T1, P2-T2, P2-T4, P2-T5, P2-T7, P4-T1,
P5-T5, P6-T1, P6-T3 and P7-T9; the `Test-Path` sites are P0-T9, P0-T11 and P1-T7. Only `git` and `rg`
invocations are issued directly.

**Search invocation form** — every `rg` invocation in this plan is issued with its pattern in single
quotes. `-F` is used only where the pattern is a fixed string whose regex metacharacters — a literal
backslash, or a parenthesis — must match those same characters in the target text, and in that case
a backslash is written once. This plan has exactly six such sites — P2-T3, P2-T6, P5-T1, P5-T3, P6-T5
and P7-T11 — and each spells `-F` in its own task text. Every other `rg` pattern in this plan is a regular expression whose
backslashes are regex escape sequences; those patterns are issued in single quotes without `-F`,
because `-F` would match the escape sequences as literal text and return zero matches for text that
is present. A
pattern that a task below renders in double quotes is issued with that same pattern text enclosed in
single quotes instead; single quotes preserve every backslash exactly, so no pattern below changes
meaning under this rule. The rule is load-bearing because a POSIX shell collapses a doubled backslash
inside double quotes, which turns a written `\\B` into the regex assertion `\B` and returns zero
matches for a literal that is present in the file.

**Toolchain order** — format, then analyzers, then nullable, then tests. Restart from the format step
whenever a step fails or changes a file. `/t:Rebuild` is mandatory: a warm `/t:Build` exits 0 with
`CoreCompile` skipped on every project, so the analyzer and nullable gates become vacuous.
`/p:Nullable=enable` must never be added: no project carries a `<Nullable>` element and there is no
`Directory.Build.props`, so the property conscripts every unannotated file.
`QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` carries `#nullable enable` at line 1;
`QuickFiler/Controllers/EfcDataModel.cs` does not.

**Test invocation, verified against `.github/workflows/_mstest-coverage.yml`.** That workflow's test
step (lines 70-86) discovers assemblies under `$env:GITHUB_WORKSPACE` filtered by `\bin\Debug\` and
not `\obj\` and not `\ref\`, and invokes vstest with `/EnableCodeCoverage /InIsolation /Logger:trx`
and `/TestCaseFilter:"TestCategory!=LiveOutlook"`. I verified all three required properties against
that file:

1. `TestCategory!=LiveOutlook` — present at `_mstest-coverage.yml:83`. Exactly one test method in the
   repository carries that category: `TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs:72`.
2. `/InIsolation` — present at `_mstest-coverage.yml:83`.
3. Workspace-root scoping of assembly discovery — present at `_mstest-coverage.yml:70`.

Full-suite runs in this plan use the repository wrapper
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`. I read that script and verified its parameters and
its behavior rather than assuming them:

- Parameters are `-SearchRoot`, `-Configuration`, `-CoverageOutput`, `-NoExecute`
  (`Invoke-MSTestWithCoverage.ps1:1-13`, `:248-259`).
- It passes `/InIsolation` and `/TestCaseFilter:TestCategory!=LiveOutlook` on the inner vstest call
  (`:76`).
- It roots assembly discovery at `$repoRoot`, computed as the script directory plus `..\..` (`:271`),
  which for this worktree is the worktree root. That is the local analogue of CI's
  `$env:GITHUB_WORKSPACE` scoping and is why a `\.claude\` exclusion filter must **not** be added
  here: the worktree root path itself contains `\.claude\`, so such a filter would exclude every
  assembly and the run would discover nothing.
- `-SearchRoot .` is passed explicitly on every invocation in this plan.

Scoped verification runs in this plan call `vstest.console.exe` directly on **one explicitly named
assembly path**, `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, resolved against the worktree root.
That is not a full-assembly discovery search, so no discovery filter can apply to it; naming the single
path is strictly stronger than any filter because it cannot resolve into another worktree. Every such
run still carries `/InIsolation` and conjoins `TestCategory!=LiveOutlook` into its
`/TestCaseFilter`, so its population is comparable to the baseline population restricted to that
assembly.

**Coverage observables — observed, not inferred.** I read
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` before writing any assertion over its output.
Findings that this plan depends on:

- The wrapper prints **no** coverage percentage on a successful run. Its success-case stdout literals
  are `Post-processing coverage XML for Koverage compatibility...` and `Done. Coverage artifact: `
  (`Invoke-MSTestWithCoverage.ps1:338`, `:344`). Numeric coverage is therefore read from the Cobertura
  document, not from stdout.
- `ConvertTo-KoverageCoberturaXml` sets `line-rate`, `branch-rate`, `lines-covered`, `lines-valid`,
  `branches-covered` and `branches-valid` on the `/coverage` element (`Helpers.ps1:442-447`; line 441
  is the `Get-CoberturaCoverageSummary` call that produces the values). Those six
  attributes are the numeric headline this plan asserts over.
- `Assert-CoberturaLineCoverageThreshold` (`Helpers.ps1:459-491`) throws when the repository line rate
  is below 80 percent, and it is called at `Invoke-MSTestWithCoverage.ps1:341`, **before** the
  post-processed document is written back at `:343`. A sub-threshold run therefore leaves the on-disk
  XML as the raw, unfiltered dotnet-coverage output. The headline reader task below re-applies
  `ConvertTo-KoverageCoberturaXml` in memory, which is idempotent on an already-processed document
  (path rewriting no longer matches a prefix, package filtering and class merging are no-ops, and the
  `sources` node already exists), so it yields the same six numbers in both cases.
- The enforced repository floor in the runner is **80 percent line coverage** (`Helpers.ps1:487-489`:
  the percentage is assigned at `:486`, the enforcing comparison `if ($percentage -lt 80)` is at
  `:487`, and the `80%` message literal is at `:489`).

**Coverage-floor authority resolution.** `AGENTS.md` is the Codex repository-policy authority. Its
General Unit Test Policy requires repository-wide line coverage to remain at or above **80 percent**,
new modules, classes and methods to target at least **90 percent** coverage, and changed lines not to
lose coverage. `.agents/skills/csharp/SKILL.md` repeats those same three requirements. The repository
runner enforces the 80-percent repository floor at
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487`, so the binding repository-wide policy and
the executed gate use the same figure. The change-scoped gates remain independently blocking: no
changed line may lose coverage, and every Cobertura sequence point in the new helper must be covered,
with both decision outcomes demonstrated as P7-T7 specifies. P7-T8 states and applies this Codex
authority resolution.

**Formatting observables.** `dotnet tool run csharpier format .` rewrites files and still exits 0, so
its exit code alone proves nothing. The discriminating observation used by this plan is therefore a
before-and-after `git status --porcelain` comparison over the scoped pathspec, taken in the same task
as the write-mode run. I did **not** observe CSharpier 1.2.6's success-case summary wording in this
session, so no acceptance condition in this plan asserts over that wording; every csharpier task
records its stdout verbatim into its artifact for audit and gates only on the exit code and on the
tree observation. `.csharpierignore` excludes `**/evidence/**`, `*.cobertura.xml`, `*.trx`,
`*.csproj`, `*.props` and `*.targets`, so evidence artifacts, coverage documents and the test project
file are outside the formatter's scope.

**Anchored-diff form.** Before P6-T6 commits, nothing this plan changes is in `HEAD`, so a two-dot
`BASE..HEAD` diff reports nothing for it. Every pre-commit diff gate in this plan therefore uses the
index form `git diff 0eda184ca0009bc79ac9b7146897270c17c095fa --cached -- <paths>` and is preceded in
the same task by a `git add` over the same paths. Every post-commit diff gate uses
`git diff 0eda184ca0009bc79ac9b7146897270c17c095fa..HEAD -- <paths>`. Both forms are anchored to an
explicit ref; the bare unanchored `git diff` is never used.

**Name-listing diffs carry a companion.** A `git diff --name-only` or `--name-status` enumerates
tracked changes only and never reports an untracked path, so on its own it cannot fail on a file this
plan creates and leaves uncommitted. Every name-listing diff in this plan therefore carries a
`git add` span or a `git status --porcelain` span in the same task, and the task text states what the
executor must observe in that companion output. The two mechanisms are complementary and each alone
is wrong in one state: the anchored diff is blind to untracked files, and porcelain status goes empty
once the change is committed. This plan contains exactly three name-listing diff sites, and all three
carry a companion. P0-T8 runs `git status --porcelain -- QuickFiler QuickFiler.Test` alongside its
baseline diff and asserts that span is empty. P6-T6 runs
`git add QuickFiler QuickFiler.Test docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`
before its diff and asserts that `git status --porcelain -- QuickFiler QuickFiler.Test` produces no
output after the commit. P8-T30 runs a porcelain span over the nine trees it audits, ahead of its two diffs, and
asserts that span is empty; the reconciliation of the two spans at that point in the ordering is
stated in P8-T30 itself.

**Checkbox-counting declaration (operator constraint 3).** This plan ships **no** tool, script, or
reusable helper that counts checkboxes, criteria, or list items in a generated document. The only
counting of checkboxes it performs is three inline, section-scoped counts, all of them confined to
the `## Acceptance Criteria` section of `spec.md`: P0-T6's baseline count of the acceptance criteria
in that section, and the two independently constructed verifications in P8-T31. Section scoping is
mandatory in all three because `spec.md` genuinely contains five checkboxes outside that section — the Impact/Severity
block at `spec.md:54-57` (four) and the Logs/Screenshots line at `spec.md:86` (one) — so a whole-file
count over-reports by exactly five. Because no reusable tool is introduced, the fixture-test
obligation attached to such a tool does not arise; if a future revision introduces one, that
obligation attaches and must be satisfied before the tool is used.

**Execution checkpoint boundaries.** This plan contains 107 atomic tasks. After completing and
checking off task 35 (`[P2-T10]`), task 70 (`[P7-T8]`), task 105 (`[P8-T31]`), and the final partial
interval at task 107 (`[P8-T33]`), the atomic executor must stop mutation and return the exact
`PROGRESS_COMMIT_REQUIRED` signal stated at that boundary. The orchestrator must then stage only the
completed interval's in-scope paths, collect canonical commit context through the repository
automation adapter, resolve and persist the routed `commit-steward` receipt, delegate the commit
message to that exact profile, create the commit, and record the task interval and resulting SHA in
`artifacts/orchestration/orchestrator-state.json` before execution resumes. No executor may mutate the
worktree while a boundary commit is being prepared.

The existing HEAD-materialization points after `[P6-T6]`, `[P7-T2]`, and `[P7-T12]` remain necessary
because later gates use anchored `BASE..HEAD` diffs. They use the same orchestrator-controlled commit
protocol and do not replace the mandatory task-count boundaries. No atomic task invokes `git commit`
or selects its own commit message, and these boundaries add or renumber no tasks.

## Scope

In scope, exactly four changes:

- **A.** `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` — bind the discarded `out _` of
  `ArchiveStemContract.TryMakeArchiveRelative` at line 99, commit the stem when non-empty, and treat an
  empty stem as a deterministic non-selection with a value-free diagnostic. The change stays nested
  inside the existing `ArchiveStemContract.IsFullOutlookPath(selection)` arm.
- **B.** One new pure `internal static` helper, `EfcDataModel.ToFilingStemOrVerbatim`, declared in a
  new partial-class file `QuickFiler/Controllers/EfcDataModel.FilingStem.cs` and called from the
  `DestinationOlStem` assignment at `QuickFiler/Controllers/EfcDataModel.cs:337` in the `string`
  overload of `MoveToFolderAsync`. Change B therefore also makes two supporting edits: the class
  declaration at `EfcDataModel.cs:21` gains the `partial` keyword, and
  `QuickFiler/QuickFiler.csproj` gains one `<Compile Include>` item for the new file.
- **C.** `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` — one assertion, one test
  method name, and one two-line arrange comment, recorded as a deliberate spec correction.
- **D.** Three stale "deferred to issue #637" records.

**Why change B is split into a second file.** `spec.md:414-416` authorizes this: "if it does not, the
helper moves to its own file rather than the 500-line limit being exceeded." That condition is met.
`QuickFiler/Controllers/EfcDataModel.cs` is 485 lines on the merged tree, so its headroom to the
500-line limit in the Agent Code Change Policy section of `AGENTS.md` is 15 lines. The final helper needs roughly
24 to 26 lines: five to eight lines of XML documentation that must state the contract, describe the
second parameter, express the gate without naming `IsFullOutlookPath`, and express totality without
using the character sequence `throw`; plus a body of about twelve lines, because the call
`ArchiveStemContract.TryMakeArchiveRelative(candidatePath, archiveAncestor, out string stem)` at
eight-space indentation exceeds CSharpier's default print width of 100 columns once wrapped in an
`if (` at twelve-space indentation, so that call occupies more than one line. Even the
behavior-preserving seam alone, had it been placed in `EfcDataModel.cs`, would have taken that file to
roughly 498 lines, leaving no room for the Phase 4 body. Placing the helper in
`EfcDataModel.cs` therefore cannot satisfy the 500-line limit, and the split is taken. The helper is a
partial-class member of the same type, so the fixed identifier `EfcDataModel.ToFilingStemOrVerbatim`
that AC11 and the "Fixed identifiers" section name is preserved exactly. In-place single-line
substitutions are the only edits made to `EfcDataModel.cs` itself, so that file's line count is
unchanged by this plan and no line number in it shifts.

Out of scope and owned by issue #695: the unhandled keyboard entry points to `ActionOkAsync`, the
half-completed button-path teardown, and the verbatim `DestinationOlStem` assignments in
`EfcDataModel.OpenOlFolderAsync` at `EfcDataModel.cs:364` and `OpenFsFolderAsync` at `:388`. No task in
this plan touches any of those. The `Globals.Ol.ArchiveRootPath` benign-degrade item is no longer
pending in `EfcDataModel`: issue #638 delivered it there, adding `TryGetArchiveRoot` at
`EfcDataModel.cs:271-297` with the guarded read at `:284` and the
`UserDiagnosticAction(ArchiveRootUnavailableMessage)` degrade at `:358` and `:382`. This plan
preserves that work unchanged and adds nothing to it; `spec.md:164-172` still describes the whole
benign-degrade item as pending, and that staleness is recorded by P8-T32 rather than corrected in
`spec.md`.

## Fixed identifiers (the executor does not choose these)

- New helper: `EfcDataModel.ToFilingStemOrVerbatim(string candidatePath, string archiveAncestor)`,
  `internal static string`.
- New production file: `QuickFiler/Controllers/EfcDataModel.FilingStem.cs`, namespace
  `QuickFiler.Controllers`, whose only type declaration is written verbatim as
  `    internal partial class EfcDataModel` and whose only member is `ToFilingStemOrVerbatim` with its
  XML documentation. Its `using` set is exactly `using UtilitiesCS.OutlookObjects.Folder;`, which is
  the namespace of `ArchiveStemContract` (`UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs:4`,
  type at `:18`) and is the only namespace the helper body needs.
- Amended declaration line in `QuickFiler/Controllers/EfcDataModel.cs:21`: `    internal class EfcDataModel`
  becomes `    internal partial class EfcDataModel`. This is a one-token, single-line substitution; the
  file's line count and every line number in it are unchanged by it.
- New project item in `QuickFiler/QuickFiler.csproj`, written verbatim as
  `    <Compile Include="Controllers\EfcDataModel.FilingStem.cs" />`.
- New test file: `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue637Tests.cs`, class
  `BreadcrumbBridgeRouterIssue637Tests`, with exactly these ten test methods:
  `RowSelected_ArchiveRootExactFilingTarget_IsNotSelected`,
  `RowSelected_ArchiveRootExactFilingTarget_PreservesAPriorValidSelection`,
  `RowSelected_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem`,
  `RowSelected_RootedTargetUnderArchiveRoot_CaseInsensitiveAndTrailingSeparatorRoot_CommitsTheStem`,
  `RowSelected_RelativeFilingTarget_CommitsTheValueVerbatim`,
  `RowSelected_TrashPseudoRow_CommitsTheSentinelVerbatim`,
  `RowSelected_OutOfRootRootedTarget_IsStillRejected`,
  `RowSelected_SeparatorBoundaryNearMissTarget_IsStillRejected`,
  `RowSelected_RootedTargetWithNoBoundArchiveRoot_PassesThroughVerbatim`,
  `SelectFirstRow_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem`.
- New sibling test class `EfcDataModelIssue637Tests`, whose declaration line is written verbatim as
  `    public class EfcDataModelIssue637Tests`, matching the form of the existing declaration at
  `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs:16`. It is added to that same existing
  file, with exactly these eight test methods:
  `ToFilingStemOrVerbatim_RootedUnderAncestor_ReturnsTheStem`,
  `ToFilingStemOrVerbatim_RootedUnderCaseDifferingAncestor_ReturnsTheStem`,
  `ToFilingStemOrVerbatim_RelativeStem_ReturnsTheInputVerbatim`,
  `ToFilingStemOrVerbatim_TrashSentinel_ReturnsTheInputVerbatim`,
  `ToFilingStemOrVerbatim_ArchiveRootExact_ReturnsTheInputVerbatimAndDoesNotThrow`,
  `ToFilingStemOrVerbatim_OutOfRootRootedInput_ReturnsTheInputVerbatimAndDoesNotThrow`,
  `ToFilingStemOrVerbatim_NullEmptyWhitespaceOrSeparatorOnlyAncestor_ReturnsTheInputVerbatim`,
  `ToFilingStemOrVerbatim_NullOrEmptyCandidate_ReturnsTheInputVerbatim`.
- Renamed test method (change C):
  `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch` becomes
  `Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively`.
- New archive-root-exact diagnostic string (change A), value-free and containing no `@`:
  `Breadcrumb row rejected: target is the archive root itself.`
- Preserved out-of-root diagnostic string, unchanged:
  `Breadcrumb row rejected: target is outside the archive root.`
- Change-D replacement texts, fixed here:
  1. `QuickFiler/Controllers/EfcSelectionGuard.cs:30` becomes
     `        /// normalization in BreadcrumbBridgeRouter.SelectRow is implemented by issue #637.`
  2. `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:146` becomes
     `            // RC-1 inversion: rooted values are never filing stems here; the producer normalizes.`
  3. `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:152` becomes
     `                    "a rooted value is never a filing stem at this surface and the producer now normalizes before this predicate is reached"`

## Tree observations recorded while authoring this plan

These were re-derived against the working tree and one disagrees with `spec.md`. They are recorded so
no downstream artifact inherits a wrong figure.

1. `QuickFiler/Controllers/EfcDataModel.cs` is **485** lines on the merged tree — 423 at the
   `ecdb1c84` planning base, and 424 in the spec's implementation table at `spec.md:401`.
   Headroom to the 500-line limit is **15**. This is what forces the change-B file split described
   under "Scope". AC25's binding clause ("at or under 500 lines") is unaffected, and AC25's
   parenthetical already reads 485 in `spec.md`.
2. The composition test `Issue614_GuardAcceptedSelection_DoesNotThrowAtFilingBoundary` spans
   `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:167-214`; AC23 and the spec cite `:167-213`.
   The closing brace is at 214. No behavioral consequence.
3. The `#499` clear-on-rebind block spans `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:143-147`;
   the spec cites `:143-146`. The write at `:145` and the read at `:143` that AC24 names are exact.
4. Research section 11's claim that no `EfcDataModelTests.cs` exists is wrong; the file exists at 409
   lines. `spec.md` already records this correction and the spec wins.
5. The `MoveToFolder` family census moved when issue #638 merged. On the merged tree the family-stem
   search returns **23** lines across **6** files, and the syntax-anchored search returns **10** lines
   across **5** files: 3 declarations and 7 call sites, leaving 13 residual non-member textual
   references. Research section 6 says 16 lines across 6 files and `spec.md` says 16 across 5; both
   describe the pre-merge tree. AC16 already carries the measured counts and citations; P1-T2 and
   P8-T16 carry the measured figures. The one file the stem search reaches that the syntax-anchored
   search does not is `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs`, whose single match at
   `:55` is a comment naming `MoveToFolderAsync` and not a member reference.
   `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`, added by issue #638, appears in both
   searches and contributes the call site at `:314`.
6. This worktree has no `.dotnet-sdk` directory and no `packages` directory, so the repo-local SDK and
   the NuGet package restore must both be bootstrapped before any toolchain command runs.
7. Issue #638 landed on this branch before execution begins. It changed
   `QuickFiler/Controllers/EfcDataModel.cs` (+68/-3), added one `<Compile Include>` line at
   `QuickFiler.Test/QuickFiler.Test.csproj:116`, and added the 389-line test file
   `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` with 11 `[TestMethod]` members and a
   `private sealed class TestableEfcDataModel : EfcDataModel` at `:377`. That subclass derives from
   `EfcDataModel`, and adding `partial` to a class declaration does not change its accessibility, its
   base list, or its members, so the Phase 2 `partial` edit leaves it compiling unchanged. This plan
   interacts with that file in exactly two ways: it must not break
   `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce` at `:172`, whose
   `olObjects.VerifyGet(value => value.ArchiveRootPath, Times.Once());` at `:185` pins a single
   archive-root read per call; and the file contributes one call site at `:314` to the `MoveToFolder`
   census. It contains no `DestinationOlStem`, no `ToFilingStem` and no `SelectedFolderPath`, so
   changes A, C and D do not touch it.
8. `QuickFiler/QuickFiler.csproj` is a non-SDK-style project with **130** explicit `<Compile Include>`
   items. The `ItemGroup` carrying the `Controllers\` entries opens at `:287`, and
   `<Compile Include="Controllers\EfcDataModel.cs" />` is at `:289`. A production file absent from
   this project does not compile into the assembly.
9. Issue #644 (pull request #702) landed on this branch in the same merge that produced the current
   base anchor, so all of that work is behind the anchor and appears in no diff this plan takes. It
   added `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs` and registered
   it at `QuickFiler.Test/QuickFiler.Test.csproj:133`, which is below every line this plan cites in
   that file — `:57`, `:64`, `:114` and `:116` — so no cited line number in it shifted. The
   citations most exposed to that merge were re-derived against the merged tree and are unchanged:
   `QuickFiler/Controllers/EfcDataModel.cs` is still 485 lines with its declaration at `:21` and the
   `DestinationOlStem` assignment at `:337`; `QuickFiler/QuickFiler.csproj` still carries 130
   `<Compile Include>` items with the `ItemGroup` at `:287` and `Controllers\EfcDataModel.cs` at
   `:289`; and `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` is still 209 lines with
   the discarded `out _` at `:99`. The `MoveToFolder` and `SelectedFolderPath` censuses in
   observation 5 and in P1-T2 and P1-T3 were re-measured on the merged tree and are also unchanged.

### Phase 0 — Context, policy reads, and baseline capture

- [x] [P0-T1] Read `AGENTS.md` in full at the worktree root before any other task in this phase.
      Acceptance: the standing-instructions entry `AGENTS.md — standing instructions` is recorded
      first in the `Policy Order:` field of the artifact written by P0-T5.
- [x] [P0-T2] Re-read the `Agent Code Change Policy` section of `AGENTS.md`. Acceptance: the entry
      `AGENTS.md — Agent Code Change Policy` is recorded second in P0-T5's `Policy Order:`, and the
      artifact records the 500-line file-size limit and four-step toolchain-loop requirement.
- [x] [P0-T3] Re-read the `General Unit Test Policy` section of `AGENTS.md`. Acceptance: the entry
      `AGENTS.md — General Unit Test Policy` is recorded third in P0-T5's `Policy Order:`, and the
      artifact records the repository-wide 80-percent line floor, the at-least-90-percent target for
      new modules, classes and methods, and the no-regression requirement for changed lines.
- [x] [P0-T4] Read `.agents/skills/csharp/SKILL.md` in full. Acceptance: that path is recorded fourth
      in P0-T5's `Policy Order:`, and the artifact records the exact Codex C# format, analyzer,
      nullable and coverage-enabled test sequence together with MSTest, Moq and FluentAssertions.
- [x] [P0-T5] Write `evidence/baseline/phase0-instructions-read.md` containing `Timestamp:`,
      `Policy Order:`, an explicit ordered list of the four policy entries from P0-T1 through P0-T4,
      and `Distinct Files Read:`. Acceptance: the file exists; `Policy Order:` contains exactly those
      four ordered entries; and `Distinct Files Read:` contains exactly `AGENTS.md` and
      `.agents/skills/csharp/SKILL.md`.
- [x] [P0-T6] Read `spec.md` in full and write `evidence/baseline/p0-t6-spec-read.md` recording the
      count of acceptance criteria found inside the `## Acceptance Criteria` section only. Acceptance:
      the recorded count is exactly 30, and the artifact also records that the five checkboxes at
      `spec.md:54`, `:55`, `:56`, `:57` and `:86` lie outside that section and are excluded.
- [x] [P0-T7] Read `research/research.2026-08-29T12-30.md` in full and write
      `evidence/baseline/p0-t7-research-read.md` listing the two numbered corrections `spec.md`
      records under "Corrections to the research file", the second of which bundles two distinct
      file-count facts, and stating that `spec.md` governs where they conflict.
      Acceptance: the artifact names the `EfcDataModelTests.cs` existence correction, the
      `MoveToFolder` five-file correction, and the `SelectedFolderPath` three-production-file
      correction.
- [x] [P0-T8] Record the current worktree branch and prove the base commit is a clean pre-change baseline. Run
      `git rev-parse --abbrev-ref HEAD`, `git rev-parse HEAD`,
      `git diff --name-only 0eda184ca0009bc79ac9b7146897270c17c095fa..HEAD -- QuickFiler QuickFiler.Test`,
      and, as the porcelain companion to that name-listing diff,
      `git status --porcelain -- QuickFiler QuickFiler.Test`,
      and write `evidence/baseline/p0-t8-git-base.md` recording all four outputs verbatim. The porcelain
      span is required because a name-listing diff compares two commits and therefore reports neither an
      untracked path nor a modified-but-unstaged tracked file; either state falsifies the clean-baseline
      conclusion this task exists to establish, and both would otherwise surface for the first time at
      P6-T6's exact ten-path enumeration. `git rev-parse HEAD` is recorded as an observation and is
      never compared against a stated value. The anchor is the commit at which `origin/main` was
      merged into this branch, and it was the branch tip when this plan was amended, so on an
      untouched checkout the name-listing diff is empty for that reason; it becomes non-empty the
      moment the executor's checkout carries any further commit under those two trees, and the
      porcelain companion beside it reports any staged, unstaged or untracked change in the same
      trees. Together those are exactly the states this task exists to detect.
      Acceptance: the `git diff --name-only` invocation produces **no output at all**; the
      `git status --porcelain` invocation produces **no output at all**; and the
      recorded branch name is
      `agent-af95f0a8159ff28fa-wt-2026-08-31T08-39`. An ancestry check is
      deliberately not used here. `git merge-base --is-ancestor` exits 0 for any ancestor, including
      an ancestor that predates work this plan does not own, so it cannot fail in the state this task
      exists to detect; the empty-diff form fails as soon as any file under `QuickFiler` or
      `QuickFiler.Test` differs between the anchor and `HEAD`, which is the property every later
      "exactly N paths", "no hunk in range" and "added line numbers" gate depends on. If the branch
      name differs, if the diff produces any output, or if the porcelain span produces any output,
      record `BASE MISMATCH` in the artifact
      together with the offending output, stop, and report to the orchestrator; do not proceed to
      P0-T9.
- [x] [P0-T9] Bootstrap the repo-local .NET SDK with
      `pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1` and write
      `evidence/baseline/p0-t9-sdk-bootstrap.md`. Acceptance: `EXIT_CODE: 0`, and after the run the
      path `.dotnet-sdk/dotnet.exe` exists (record the result of `Test-Path .dotnet-sdk/dotnet.exe` as
      `True` in `Output Summary:`). `global.json` pins SDK `8.0.205` with `paths` `[".dotnet-sdk", "$host$"]`,
      so this step is a prerequisite of every `dotnet` invocation below. If the exit code is non-zero
      or the path does not exist, record the captured output under a section headed
      `BOOTSTRAP_FAILED:`, stop, and report to the orchestrator; do not proceed to the next task and
      do not attempt a repair, because no toolchain command in this plan can run without the
      repo-local SDK.
- [x] [P0-T10] Restore the pinned CSharpier tool with
      `pwsh -NoProfile -Command 'dotnet tool restore; "EXIT_CODE=$LASTEXITCODE"'` and write
      `evidence/baseline/p0-t10-dotnet-tool-restore.md`. The manifest is `dotnet-tools.json` at the
      worktree root and pins `csharpier` `1.2.6`. Acceptance: `EXIT_CODE: 0`, and the captured stdout
      is recorded verbatim in the artifact. No assertion is placed on a version banner, because
      CSharpier 1.2.6 requires a subcommand and the bare-option form is not a form I have observed
      running here; the operative proof that the restore succeeded is that P0-T12's
      `dotnet tool run csharpier check .` produces a CSharpier result rather than a tool-resolution
      error, which P0-T12 records. If the exit code is non-zero, record the captured output under a
      section headed `BOOTSTRAP_FAILED:`, stop, and report to the orchestrator; do not proceed to the
      next task and do not attempt a repair.
- [x] [P0-T11] Restore NuGet packages with
      `pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1` and write
      `evidence/baseline/p0-t11-nuget-restore.md`. This script resolves MSBuild through vswhere and
      runs `/t:Restore /p:RestorePackagesConfig=true`; it does not rewrite any `.csproj` HintPath.
      Acceptance: `EXIT_CODE: 0`, and after the run the directory `packages` exists (record
      `Test-Path packages` as `True`). If the exit code is non-zero or the directory does not exist,
      record the captured output under a section headed `BOOTSTRAP_FAILED:`, stop, and report to the
      orchestrator; do not proceed to the next task and do not attempt a repair, because an
      unrestored package graph produces CS0006 reference errors that are indistinguishable from real
      analyzer findings.
- [x] [P0-T12] Capture the baseline format state **read-only** with
      `pwsh -NoProfile -Command 'dotnet tool run csharpier check .; "EXIT_CODE=$LASTEXITCODE"'` and
      write `evidence/baseline/p0-t12-csharpier-check.md`. The write-mode `format` command must not be
      run in Phase 0: repairing pre-existing drift before the baseline would either waive it silently
      or make a later zero-diff gate unsatisfiable. Acceptance: the artifact records `EXIT_CODE:`, the
      captured stdout verbatim, and a section headed `BASELINE_FORMAT_DRIFT:` listing every file path
      the captured output names as needing formatting (the list is empty when `EXIT_CODE: 0`, and a
      non-zero exit code is the signal that the list is non-empty). Later zero-diff formatting gates
      exclude exactly the paths in that list and nothing else. The artifact also records whether the
      invocation produced a CSharpier result at all, which is the proof that P0-T10's tool restore
      succeeded.
      If the `BASELINE_FORMAT_DRIFT` list names any path outside `QuickFiler` and `QuickFiler.Test`,
      record `OUT_OF_SCOPE_FORMAT_DRIFT:` with that sub-list, stop, and report to the orchestrator
      before proceeding to P0-T13. P7-T1's repo-wide write-mode format run would repair those paths,
      P7-T2 stages only `QuickFiler QuickFiler.Test` so the repair would never be committed, and
      P8-T30's porcelain span over the nine audited trees would then be non-empty and that task
      unsatisfiable. Repairing pre-existing drift in trees this plan does not own is outside this
      plan's scope and requires an explicit orchestrator decision.
- [x] [P0-T13] Complete the baseline analyzer build after the recorded fresh-worktree analyzer
      bootstrap recovery. Preserve the existing first-attempt evidence file
      `evidence/baseline/p0-t13-msbuild-analyzers.md` unchanged: its `EXIT_CODE: 1` and
      `BASELINE_BUILD_RED:` section record only `CS0006` diagnostics for the absent
      `Meziantou.Analyzer.3.0.156` and `Roslynator.Analyzers.4.16.0` analyzer DLLs. Only when that
      exact evidence condition holds, run `nuget install Meziantou.Analyzer -Version 3.0.156
      -OutputDirectory packages` and `nuget install Roslynator.Analyzers -Version 4.16.0
      -OutputDirectory packages`; both commands must exit `0`. This is a fresh-worktree bootstrap of
      ignored `packages` contents only: do not edit any `.csproj`, `packages.config`, workflow, or
      NuGet-policy file, and do not integrate `origin/main`. Record the two commands and their exit
      codes in `evidence/baseline/p0-t13-analyzer-backfill.md`. Before the rebuild retry, enumerate
      every `<Analyzer Include>` reference in every `*.csproj` whose path contains either of those
      two exact package versions, resolve each path from its project directory, and record every
      resolved analyzer DLL path and `Test-Path` result in that same artifact; every result must be
      `True`. Then run `git status --porcelain -- '*.csproj' '*/packages.config' 'packages'`; it must
      exit `0` with empty output, which is the proof that the bootstrap changed no tracked project or
      package-policy surface. Immediately after those checks, retry the following existing baseline
      analyzer command verbatim and write the retry result to
      `evidence/baseline/p0-t13-msbuild-analyzers.retry.md`:
      `pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuildExe = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuildExe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; "EXIT_CODE=$LASTEXITCODE"'`
      Acceptance: the first-attempt evidence remains unchanged; both NuGet commands and the status
      command exit `0`; every referenced analyzer DLL is present; the status output is empty; and
      the retry artifact records `EXIT_CODE: 0`, the MSBuild final status line, the `Warning(s)` and
      `Error(s)` counts as printed, and `(Rebuild target(s))` at least once. The retry artifact's
      `Command:` line must contain `/t:Rebuild` and `EnableNETAnalyzers=true` and
      `EnforceCodeStyleInBuild=true`, and must not contain the solution-wide nullable opt-in property
      — record this as `NULLABLE_OPT_IN_PROPERTY: absent`; do not spell the token in the artifact. If
      the evidence condition does not hold, either NuGet command fails, a referenced analyzer DLL is
      absent, the status output is non-empty, or the retry exits non-zero, record the applicable
      output under `BOOTSTRAP_FAILED:` or `BASELINE_BUILD_RED:` in the new recovery artifact, stop,
      and report to the orchestrator; do not proceed to P0-T14 or attempt another repair.
- [x] [P0-T14] Capture the baseline nullable build. Run
      `pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuildExe = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuildExe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true; "EXIT_CODE=$LASTEXITCODE"'`
      and write `evidence/baseline/p0-t14-msbuild-nullable.md`. Acceptance: `EXIT_CODE: 0`; the
      captured output contains `(Rebuild target(s))`; and the recorded `Command:` line contains
      `/t:Rebuild` and does not contain the solution-wide nullable opt-in property — record this as
      `NULLABLE_OPT_IN_PROPERTY: absent`; do not spell the token in the artifact. If the exit code is
      non-zero, record the full diagnostic list under a section headed `BASELINE_BUILD_RED:`, stop,
      and report to the orchestrator; do not proceed to the next task and do not attempt a repair,
      because a pre-existing red baseline is outside this plan's scope.
- [x] [P0-T15] Capture the baseline full test run with coverage. Preserve the existing failed
      first-attempt evidence file `evidence/baseline/p0-t15-mstest-coverage.md` unchanged. Permit at
      most two recovery retries, and only for the documented #592 60,000ms QuickFiler
      pump/dispatcher-timeout cascade. Before each retry, verify that no `dotnet-coverage`, `vstest`,
      or `testhost` process targeting this worktree remains; wait until the 17 P0-T13/P0-T14 MSBuild
      nodes have exited; and write a separate immutable retry artifact
      `evidence/baseline/p0-t15-retry-<attempt>.md` that records `Timestamp:`, the unchanged command,
      `EXIT_CODE:`, #592 qualification evidence, pre-retry process counts, machine-load observation,
      and `Output Summary:`. Execute this unchanged command in one persistent terminal session and
      poll that same process ID to terminal completion; do not use `Start-Process` or another launcher:
      `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\p0-t15-baseline.cobertura.xml`.
      Do not alter runsettings, worker count, timeout, filter, wrapper, or the canonical nine-assembly
      list. `coverage/*` is gitignored (`.gitignore:144`), so the Cobertura document does not dirty the
      tree. Acceptance: a retry exits `0`; reports the nine canonical assemblies and total, passed, and
      failed test counts; creates `coverage/p0-t15-baseline.cobertura.xml`; and records all six numeric
      `/coverage` attributes in its `Output Summary:` with derived line and branch percentages. The
      successful retry artifact includes a `BASELINE_FAILURE_SET:` section naming every failing test's
      fully qualified name (empty when the run passes). As a read-only P0-T15 substep, run the P0-T16
      coverage-headline command against `coverage\p0-t15-baseline.cobertura.xml`, record those values
      in the successful retry artifact, verify P0-T15, and check P0-T15 off before starting P0-T16.
      P0-T16 does not modify P0-T15 evidence. On a non-#592 failure, or after two cleared-load #592
      retry attempts without the required output, stop and report the baseline blocker; do not perform
      further retries or any repair.
- [x] [P0-T16] Read the baseline numeric coverage headline. Run
      `pwsh -NoProfile -Command '. ".\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1"; $raw = Get-Content -LiteralPath ".\coverage\p0-t15-baseline.cobertura.xml" -Raw -Encoding UTF8; [xml]$d = ConvertTo-KoverageCoberturaXml -XmlContent $raw -RepoRoot (Get-Location).Path; $c = $d.SelectSingleNode("/coverage"); foreach ($a in @("line-rate","branch-rate","lines-covered","lines-valid","branches-covered","branches-valid")) { $a + "=" + $c.GetAttribute($a) }'`
      and write `evidence/baseline/p0-t16-coverage-headline.md`. Acceptance: `EXIT_CODE: 0`, and
      `Output Summary:` records all six numeric values, plus the derived baseline line-coverage
      percentage computed as `line-rate` multiplied by 100 and the derived branch percentage computed
      as `branch-rate` multiplied by 100. Confirm that all six attributes and both derived
      percentages equal the values already recorded in the successful
      `evidence/baseline/p0-t15-retry-<attempt>.md` artifact; P0-T16 does not modify that artifact or
      any prior checklist state. These are the baseline figures the Phase 7 delta task compares against.
- [x] [P0-T17] Record the baseline uncovered-line sets for the two production files this plan changes.
      Run
      `pwsh -NoProfile -Command '. ".\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1"; $raw = Get-Content -LiteralPath ".\coverage\p0-t15-baseline.cobertura.xml" -Raw -Encoding UTF8; [xml]$d = ConvertTo-KoverageCoberturaXml -XmlContent $raw -RepoRoot (Get-Location).Path; foreach ($f in @("QuickFiler\Controllers\BreadcrumbBridgeRouter.Selection.cs","QuickFiler\Controllers\EfcDataModel.cs")) { $u = @(); foreach ($c in $d.SelectNodes("//class")) { if ($c.GetAttribute("filename") -eq $f) { foreach ($l in $c.SelectNodes("./lines/line")) { if ([int]$l.GetAttribute("hits") -eq 0) { $u += [int]$l.GetAttribute("number") } } } }; $f + " uncovered=" + (($u | Sort-Object -Unique) -join ",") } '`
      and write `evidence/baseline/p0-t17-baseline-uncovered-lines.md`. Acceptance: `EXIT_CODE: 0`, and
      the artifact records one `uncovered=` line for each of the two file paths, even when the set is
      empty. The third production file this plan touches,
      `QuickFiler\Controllers\EfcDataModel.FilingStem.cs`, is deliberately absent from this command:
      it does not exist on the baseline tree, so it has no baseline coverage row and no baseline
      uncovered set. The artifact records that absence in a line reading
      `QuickFiler\Controllers\EfcDataModel.FilingStem.cs baseline=absent`, which is the baseline
      P7-T7 compares its post-change measurement of that file against.

### Phase 1 — Pre-change census re-derivation, two independent searches per number

Every number in this phase is a number that `spec.md` already carries inside an approved acceptance
criterion. Each task verifies it by a search over the full symbol family and cross-checks it with a
second, independently constructed search. No number in this phase is verified by a single-pass grep.

- [x] [P1-T1] Re-derive the selection family census (AC9: 2 declarations, 7 call sites).
      Search 1, path-anchored: `rg -n "Select(Row|HierarchyPath)\s*\(" --glob "*.cs" QuickFiler/Controllers/BreadcrumbBridgeRouter.cs QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs`.
      Search 2, syntax-anchored and independently constructed on unqualified-invocation form rather
      than on file paths: `rg -n "^\s+(private void )?Select(Row|HierarchyPath)\(" --glob "*.cs" .`
      run over the whole repository. Search 2 excludes the unrelated Family-B `SelectRow(int index)`
      surface on `BreadcrumbStateModel`, `BreadcrumbSelectionSession`, `FolderBreadcrumbBridgeRouter`
      and `BreadcrumbBridgeCoordinator`, because every Family-B site carries a receiver or a
      non-`private void` modifier. A naive `rg "SelectRow"` returns roughly ten times too many lines
      and must not be used. Write `evidence/baseline/p1-t1-selection-family.md`. Acceptance: both
      searches return exactly 9 lines; the two line sets are identical; the artifact lists them and
      classifies each as declaration or call; declarations total 2
      (`BreadcrumbBridgeRouter.Selection.cs:83` and `:109`) and call sites total 7
      (`BreadcrumbBridgeRouter.cs:201`, `:286`, `BreadcrumbBridgeRouter.Arrows.cs:138`, `:153`, `:161`,
      `BreadcrumbBridgeRouter.Selection.cs:33`, `:47`).
- [x] [P1-T2] Re-derive the `MoveToFolder` family census (AC16: 3 declarations, 7 call sites).
      Search 1, family-stem: `rg -n "MoveToFolder" --glob "*.cs" .` — the bare stem catches any
      non-`Async` sibling or partially renamed overload that an `Async`-suffixed pattern would miss.
      Search 2, independently constructed on invocation and declaration syntax:
      `rg -n "MoveToFolderAsync\s*\(" --glob "*.cs" .` — this excludes the `MoveToFolderAsyncAction`
      delegate property, its null test and its invocation, which are textual references rather than
      family members. Write `evidence/baseline/p1-t2-movetofolder-family.md`. Acceptance: Search 1
      returns 23 lines across 6 files; Search 2 returns 10 lines across 5 files; the artifact classifies
      Search 2's 10 lines as exactly 3 declarations (`EfcDataModel.cs:303`, `EfcDataModel.cs:398`,
      `EfcHomeController.ExecuteMoves.cs:89`) and 7 call sites
      (`EfcHomeController.ExecuteMoves.cs:78`, `:98`, `EfcDataModel.cs:408`, `EfcFormController.cs:537`,
      `:844`, `EfcHomeControllerExecuteMovesTests.cs:87`,
      `EfcDataModelArchiveRootTests.cs:314`); and the artifact records that Search 1 minus
      Search 2 leaves exactly 13 non-member textual references, closing the 23-line accounting. The
      artifact also records that the stem-search file count is 6 and the syntax-anchored file count is
      5, that the one stem-search file the syntax-anchored search does not reach is
      `EfcHomeControllerTests.cs`, whose single match at `:55` is a comment, and that
      `EfcDataModelArchiveRootTests.cs` appears in both searches, contributing the `:314` call site,
      and that the 16-line figure in research section 6 and in `spec.md` describes the tree before
      issue #638 merged.
- [x] [P1-T3] Re-derive the `SelectedFolderPath` surface (AC24: 9 lines across 3 production files, 2
      writes, 3 reads). Search 1: `rg -c "SelectedFolderPath" --glob "*.cs" .`, recording the per-file
      counts. Search 2, independently constructed by scoping to the production project directories up
      front rather than by subtracting the test projects from Search 1's table:
      `rg -n "SelectedFolderPath" --glob "*.cs" QuickFiler/ UtilitiesCS/ TaskMaster/ ToDoModel/ Tags/ TaskVisualization/`.
      Write
      `evidence/baseline/p1-t3-selectedfolderpath-surface.md`. Acceptance: Search 1 returns 74 lines
      across 9 files; Search 2 returns exactly 9 lines across exactly 3 files
      (`BreadcrumbBridgeRouter.cs`, `BreadcrumbBridgeRouter.Selection.cs`, `EfcFormController.cs`); the
      artifact classifies those 9 lines as 1 declaration (`BreadcrumbBridgeRouter.cs:59`), 1 doc
      reference (`:61`), 2 writes (`:145` and `BreadcrumbBridgeRouter.Selection.cs:134`), 3 reads
      (`:143`, `BreadcrumbBridgeRouter.Selection.cs:138`, `EfcFormController.cs:321`) and 2 event-only
      lines (`:62`, `:146`); and it records that the production split is 3 production files and 6 test
      files, not the 2-and-7 stated in research section 7.
- [x] [P1-T4] Re-derive the stale deferral record census (AC22: 3 records).
      Search 1, on the deferral phrase: `rg -n "deferred to issue #637" --glob "*.cs" .`.
      Search 2, independently constructed on the issue reference alone so it cannot miss a differently
      worded deferral: `rg -n "#637" --glob "*.cs" .`. Write
      `evidence/baseline/p1-t4-deferral-records.md`. Acceptance: Search 1 returns exactly 3 lines
      (`QuickFiler/Controllers/EfcSelectionGuard.cs:30`,
      `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:146`, and `:152`); Search 2 returns a
      superset whose every additional line is enumerated in the artifact and individually classified as
      not a deferral claim; and the artifact quotes the current text of all three Search 1 lines
      verbatim.
- [x] [P1-T5] Re-derive the existing `ToArchiveRelativeStem` test count (AC15: 8 tests).
      Search 1, on the method-name convention:
      `rg -n "public void ToArchiveRelativeStem_" QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`.
      Search 2, independently constructed on the call to the member under test rather than on test
      naming: `rg -n "EfcDataModel\.ToArchiveRelativeStem\(" QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`.
      Write `evidence/baseline/p1-t5-toarchiverelativestem-tests.md`. Acceptance: both searches return
      exactly 8 lines; the artifact records the declaration line numbers 21, 34, 48, 62, 72, 87, 100,
      111 and records that `ToArchiveRelativeStem_ArchiveRootItself_Throws` is the method at line 62.
- [x] [P1-T6] Re-derive the no-bound-root pass-through test pair (AC4: 2 tests).
      Search 1, by name:
      `rg -n "Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection|SegmentActivate_WithNoBoundArchiveRoot_PreservesThePassThroughMode" --glob "*.cs" .`.
      Search 2, independently constructed on the binding mechanism that produces an empty bound root —
      a separator-only fourth argument or the three-argument public overload:
      `rg -n "BindRowsAsync\(" --glob "*.cs" QuickFiler.Test/Controllers/` with each hit classified by
      whether it supplies an archive root. Write `evidence/baseline/p1-t6-passthrough-tests.md`.
      Acceptance: Search 1 returns exactly 2 declaration lines
      (`BreadcrumbBridgeRouterIssue439Tests.cs:619` and `BreadcrumbBridgeRouterIssue614Tests.cs:188`);
      the artifact records that the first binds `@"\"` at `:645` and asserts `Be(@"\Archive")` at
      `:665`, and the second uses the three-argument overload at `:213` and asserts `Be(@"\Archive")` at
      `:221`; and Search 2's classification identifies the same two tests as the only pass-through
      cases and no others.
- [x] [P1-T7] Re-derive the file line counts AC25 depends on. Construction 1:
      `pwsh -NoProfile -Command 'foreach ($p in @("QuickFiler\Controllers\EfcDataModel.cs","QuickFiler\Controllers\BreadcrumbBridgeRouter.Selection.cs","QuickFiler\Controllers\EfcSelectionGuard.cs","QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue439Tests.cs","QuickFiler.Test\Controllers\EfcDataModelIssue614Tests.cs","QuickFiler.Test\Controllers\EfcSelectionGuardTests.cs")) { $p + "=" + (Get-Content -LiteralPath $p).Count }'`.
      Construction 2, independently constructed with a line-oriented search rather than a file read:
      `rg -c "^" --glob "*.cs" QuickFiler/Controllers/ QuickFiler.Test/Controllers/` filtered to the
      same six paths. `Measure-Object -Line` must not be substituted for `(Get-Content).Count`; it
      reports a different figure for a file without a trailing newline. In the same task run
      `pwsh -NoProfile -Command 'Test-Path "QuickFiler\Controllers\EfcDataModel.FilingStem.cs"'` to
      establish the seventh path's baseline. That path is checked with `Test-Path` rather than added
      to the two line-count constructions because this plan creates it in Phase 2 and
      `Get-Content -LiteralPath` on an absent path throws, which would make this task unsatisfiable.
      Write `evidence/baseline/p1-t7-file-line-counts.md`. Acceptance: both constructions agree on all
      six paths; `EfcDataModel.cs` is 485; `BreadcrumbBridgeRouter.Selection.cs` is 209;
      `EfcSelectionGuard.cs` is 79; `BreadcrumbBridgeRouterIssue439Tests.cs` is 694;
      `EfcDataModelIssue614Tests.cs` is 123; `EfcSelectionGuardTests.cs` is 296; the `Test-Path` result
      for `QuickFiler\Controllers\EfcDataModel.FilingStem.cs` is `False` and is recorded as the
      baseline for that path; and the artifact records that `spec.md`'s implementation table at
      `spec.md:401` states 424 for `EfcDataModel.cs` while AC25 at `spec.md:977` already states 485,
      that the `ecdb1c84` planning base had 423, and that the merged
      tree value 485 governs, leaving 15 lines of headroom to the 500-line limit.
- [x] [P1-T8] Re-derive the single pinning assertion (AC20: exactly 1 existing assertion changes).
      Construction 1, on the assertion form:
      `rg -n "SelectedFolderPath\.Should\(\)\.Be\(" --glob "*.cs" QuickFiler.Test/` with every hit
      classified by whether the selected row's filing target is a full Outlook path at or under a
      non-empty bound root. Construction 2, independently constructed on the trigger side rather than
      the assertion side: `rg -n "rowSelected|SelectFirstRow" --glob "*.cs" QuickFiler.Test/` with
      every hit classified by its bound root and its presented filing target. Write
      `evidence/baseline/p1-t8-pinning-assertion.md`. Acceptance: both constructions identify exactly
      one assertion that must change — `BreadcrumbBridgeRouterIssue439Tests.cs:165`,
      `router.SelectedFolderPath.Should().Be(fullTarget);` inside the method declared at `:119` — and
      the artifact records that no test anywhere binds a presented row whose filing target equals the
      bound archive root, so zero tests depend on the archive-root-exact case being a selection.

### Phase 2 — Regression tests that fail before the fix

The change-B tests name a member that does not exist yet, and a test file referencing a missing member
makes the whole `QuickFiler.Test` assembly fail to compile, which would prevent every other test in
this phase from running at all. P2-T1 through P2-T4 therefore land a behavior-preserving seam first:
the helper is declared and called, but returns its input verbatim, which is byte-for-byte the behavior
of the current assignment at `EfcDataModel.cs:337`. The red in this phase is a genuine runtime red, not
a compile failure.

The seam is delivered as a partial-class member in a new file, for the 500-line reason stated under
"Scope". Three tasks are therefore prerequisites of the seam and are ordered ahead of it here rather
than left to Phase 4: the `partial` keyword on the existing declaration (P2-T1), the new file itself
(P2-T2), and the project registration that makes the new file compile (P2-T3). Only then does P2-T4
redirect the assignment. Reversing any of those orders produces a build that does not compile.

- [x] [P2-T1] Make the existing declaration partial. In `QuickFiler/Controllers/EfcDataModel.cs`,
      replace line 21, `    internal class EfcDataModel`, with
      `    internal partial class EfcDataModel`. This is a one-token, single-line substitution: the
      file stays at 485 lines and no line number in it shifts, which is what lets every line citation
      in Phase 4 and Phase 8 remain valid after the edit. Adding `partial` changes no accessibility, no
      base list and no member, so
      `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs:377`, which declares
      `private sealed class TestableEfcDataModel : EfcDataModel`, continues to compile unchanged.
      Acceptance:
      `rg -n "internal partial class EfcDataModel" QuickFiler/Controllers/EfcDataModel.cs` returns
      exactly 1 line and it is line 21;
      `rg -n "^    internal class EfcDataModel$" QuickFiler/Controllers/EfcDataModel.cs` returns 0
      lines; and
      `pwsh -NoProfile -Command '(Get-Content -LiteralPath "QuickFiler\Controllers\EfcDataModel.cs").Count'`
      reports exactly 485.
- [x] [P2-T2] Create the new partial-class file
      `QuickFiler/Controllers/EfcDataModel.FilingStem.cs`. It contains exactly one `using` directive,
      `using UtilitiesCS.OutlookObjects.Folder;`, the namespace `QuickFiler.Controllers`, the type
      declaration written verbatim as `    internal partial class EfcDataModel`, and one member: the
      seam form of `internal static string ToFilingStemOrVerbatim(string candidatePath, string archiveAncestor)`
      whose body is exactly
      `_ = ArchiveStemContract.TryMakeArchiveRelative(candidatePath, archiveAncestor, out _);` followed
      by `return candidatePath;`, carrying an XML documentation comment stating that this is the #637
      seam and that the normalization lands in P4-T1. The explicit discard on the
      `TryMakeArchiveRelative` call is required so that both parameters are used and no
      unused-parameter diagnostic can be promoted to an error by `/p:TreatWarningsAsErrors=true`. The
      seam's XML documentation must not contain the token `MoveToFolder`; refer to its caller as
      "the `string` filing overload" instead, because P8-T16 asserts the family stem search still
      returns exactly 23 lines. For the same reason the seam's XML documentation must not contain
      either of the two literals whose exact count of 1 is asserted below —
      `internal static string ToFilingStemOrVerbatim` and `DestinationOlStem = ToFilingStemOrVerbatim`
      — so it must not reproduce the declaration signature or the assignment statement; naming the
      method by its bare identifier is permitted and is classified rather than counted by P4-T2. Three
      further tokens are barred from this file, in its documentation and in its body alike, because
      P4-T1's acceptance asserts each of their pre-edit counts in this file: `IsFullOutlookPath`, which
      P4-T1 requires to be 0 before its edit and 1 after; the character sequence `throw` in any form,
      including `throws`, which P4-T1 requires to be absent both before and after; and `Globals` in any
      form, which P4-T1 requires to be absent. The seam body given above uses none of the three. The
      two literals this task creates are quoted verbatim here because both are absent from the tracked
      tree until this task writes them:
      `internal static string ToFilingStemOrVerbatim` and `internal partial class EfcDataModel`.
      Acceptance: the file exists;
      `rg -n "internal static string ToFilingStemOrVerbatim" QuickFiler/Controllers/EfcDataModel.FilingStem.cs`
      returns exactly 1 line;
      `rg -n "internal partial class EfcDataModel" QuickFiler/Controllers/EfcDataModel.FilingStem.cs`
      returns exactly 1 line; and
      `pwsh -NoProfile -Command '(Get-Content -LiteralPath "QuickFiler\Controllers\EfcDataModel.FilingStem.cs").Count'`
      is at most 500.
- [x] [P2-T3] Register the new production file in the non-SDK project. Insert
      `    <Compile Include="Controllers\EfcDataModel.FilingStem.cs" />` into
      `QuickFiler/QuickFiler.csproj` immediately after the existing line 289,
      `    <Compile Include="Controllers\EfcDataModel.cs" />`, which sits inside the `ItemGroup` that
      opens at line 287. That project carries 130 explicit `<Compile Include>` items and no wildcard
      glob, so a production file absent from it compiles into nothing and the seam would not exist at
      run time. The literal this task creates is `Controllers\EfcDataModel.FilingStem.cs`, quoted here
      verbatim because it is absent from the tracked tree until this task inserts it. The acceptance
      search below is the fixed-string, single-quoted form required by the "Search invocation form"
      convention: the backslash is written once and `-F` disables regex interpretation, so no shell
      layer and no regex engine can consume it. Acceptance:
      `rg -F -n 'Controllers\EfcDataModel.FilingStem.cs' QuickFiler/QuickFiler.csproj`
      returns exactly 1 line; that line is line 290; and it is inside the same `ItemGroup` that begins
      at line 287.
- [x] [P2-T4] Redirect the `DestinationOlStem` assignment to the seam. In
      `QuickFiler/Controllers/EfcDataModel.cs`, change line 337 from
      `                DestinationOlStem = folderpath,` to
      `                DestinationOlStem = ToFilingStemOrVerbatim(folderpath, olAncestor),`.
      The second argument is the local produced by `if (!TryGetArchiveRoot(out var olAncestor))` at
      line 327, and it is the same local the initializer already assigns to `OlAncestor` at line 339.
      Naming `Globals.Ol.ArchiveRootPath` at line 337 is **prohibited**. Issue #638 removed exactly
      that unguarded read from exactly this method and left the property read once, inside the
      `try` of `TryGetArchiveRoot` at line 284. Writing the property here would reintroduce the
      unguarded read and would read it twice in one call, failing the merged regression test
      `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce` at
      `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs:172`, whose
      `olObjects.VerifyGet(value => value.ArchiveRootPath, Times.Once());` at `:185` pins the single
      read. P2-T10 gates that constraint at a scoped run rather than leaving it documented, because
      every other scoped run in Phases 2 through 6 is filtered to a class that would not observe it.
      This is a single-line substitution, so `EfcDataModel.cs` stays at 485 lines and no line number
      in it shifts. Acceptance:
      `rg -n "DestinationOlStem = ToFilingStemOrVerbatim" QuickFiler/Controllers/EfcDataModel.cs`
      returns exactly 1 line and it is line 337;
      `rg -n "Globals.Ol.ArchiveRootPath" QuickFiler/Controllers/EfcDataModel.cs` returns exactly 1
      line and it is line 284; and
      `pwsh -NoProfile -Command '(Get-Content -LiteralPath "QuickFiler\Controllers\EfcDataModel.cs").Count'`
      reports exactly 485.
- [x] [P2-T5] Create `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue637Tests.cs` containing
      the class `BreadcrumbBridgeRouterIssue637Tests` with exactly the ten test methods named in the
      "Fixed identifiers" section. Use fixture Shape 2 and do not invent a new fixture shape: a
      `[TestInitialize]` `Setup` and `[TestCleanup]` `Cleanup` modelled on
      `BreadcrumbBridgeRouterIssue614Tests.cs:38-58`, the log4net `MemoryAppender` attach and detach
      helpers modelled on `:338-356`, the `Key` and `Segment` helpers modelled on `:328-336`, the
      `Inbound` and `RowSelected` JSON helpers modelled on `:264-267` and `:288-291`, the
      `RenderedMessages` and `AssertRejectionDiagnosticWithoutIdentifiers` helpers modelled on
      `:304-326`, and a `BindRows` helper modelled on `BindChain` at `:236-262` that accepts an archive
      root plus one or more presented row texts and sets up `ResolveLeafKeyAsync` and
      `GetAncestorChainAsync` for every presented row that `BreadcrumbRowBuilder.Classify` treats as a
      suggestion. To produce an empty bound root, pass the separator-only value `@"\"`, which
      `BindRowsAsync` trims to empty at `BreadcrumbBridgeRouter.cs:107-109`. Framework is MSTest with
      Moq and FluentAssertions; no temporary file, no wall-clock wait, no Outlook process. No text in
      this file may contain the token `MoveToFolder`, because P8-T16 asserts that a repository-wide
      `*.cs` search for that stem still returns exactly 23 lines. Acceptance:
      the file exists; `rg -c "\[TestMethod\]" QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue637Tests.cs`
      returns 10; each of the ten fixed method names is found exactly once by
      `rg -n "public void " QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue637Tests.cs`; and
      `(Get-Content -LiteralPath "QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue637Tests.cs").Count`
      is at most 500.
- [x] [P2-T6] Register the new test file in the non-SDK project. Insert
      `    <Compile Include="Controllers\BreadcrumbBridgeRouterIssue637Tests.cs" />` into
      `QuickFiler.Test/QuickFiler.Test.csproj` immediately after the existing line 64,
      `    <Compile Include="Controllers\BreadcrumbBridgeRouterIssue439Tests.cs" />`. A file absent
      from this project compiles into nothing and its tests silently never run. The literal this task
      creates is `Controllers\BreadcrumbBridgeRouterIssue637Tests.cs`, quoted here verbatim because it
      is absent from the tracked tree until this task inserts it. The acceptance search below is the
      fixed-string, single-quoted form required by the "Search invocation form" convention: the
      backslash is written once and `-F` disables regex interpretation, so no shell layer and no
      regex engine can consume it. Acceptance:
      `rg -F -n 'Controllers\BreadcrumbBridgeRouterIssue637Tests.cs' QuickFiler.Test/QuickFiler.Test.csproj`
      returns exactly 1 line, and that line is inside the same `ItemGroup` that begins at line 57.
- [x] [P2-T7] Add the change-B helper tests. In the existing file
      `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`, add a new sibling `[TestClass]`
      `EfcDataModelIssue637Tests` containing exactly the eight test methods named in the "Fixed
      identifiers" section, reaching `EfcDataModel.ToFilingStemOrVerbatim` through the existing
      `InternalsVisibleTo("QuickFiler.Test")` at `QuickFiler/Properties/AssemblyInfo.cs:5`. The eight
      existing `ToArchiveRelativeStem` tests in the file are not modified. The file is already
      registered in `QuickFiler.Test/QuickFiler.Test.csproj` — at `:114` on the pre-change tree, and
      at `:115` once P2-T6 has inserted its line above it — so no new `Compile Include` is
      required. The declaration line this task creates is
      `    public class EfcDataModelIssue637Tests`, matching the form of the existing declaration at
      `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs:16`. The acceptance search below
      therefore asserts over the literal `class EfcDataModelIssue637Tests`, which is quoted here
      verbatim because it is absent from the tracked tree until this task creates it. No text this
      task adds may contain the token `MoveToFolder`, because P8-T16 asserts that a repository-wide
      `*.cs` search for that stem still returns exactly 23 lines. Acceptance:
      `rg -n "class EfcDataModelIssue637Tests" QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`
      returns exactly 1 line; each of the eight fixed method names is found exactly once in that file;
      `git add QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs` followed in the same task by
      `git diff 0eda184ca0009bc79ac9b7146897270c17c095fa --cached -- QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`
      shows zero removed content lines, meaning zero lines beginning with a single `-`; and
      `(Get-Content -LiteralPath "QuickFiler.Test\Controllers\EfcDataModelIssue614Tests.cs").Count` is
      at most 500.
- [x] [P2-T8] Run the analyzer build and write `evidence/regression-testing/p2-t8-msbuild-analyzers.md`
      using the P0-T13 command verbatim. Acceptance: `EXIT_CODE: 0`; the output contains
      `(Rebuild target(s))`; and the `Error(s)` count is 0. A non-zero exit here means the seam, the
      partial-class split, the project registrations, or the new test files do not compile and must be
      repaired before P2-T10 runs.
- [x] [P2-T9] Run the nullable build and write `evidence/regression-testing/p2-t9-msbuild-nullable.md`
      using the P0-T14 command verbatim. Acceptance: `EXIT_CODE: 0`; the output contains
      `(Rebuild target(s))`; the recorded `Command:` line does not contain the solution-wide nullable
      opt-in property — record this as `NULLABLE_OPT_IN_PROPERTY: absent`; do not spell the token in
      the artifact. This gate is where an unused-parameter or nullable diagnostic introduced by the
      seam would surface as an error.
- [x] [P2-T10] Prove the seam did not regress issue #638's single-read guarantee. Use the scoped
      vstest command stated in full in P2-T11 below, with the filter
      `"/TestCaseFilter:FullyQualifiedName~EfcDataModelArchiveRootTests&TestCategory!=LiveOutlook"`
      and the results directory `coverage\testresults\p2-t10`, and write
      `evidence/regression-testing/p2-t10-issue638-preserved.md`. This task exists because P2-T4
      rewrites the one line issue #638 changed in the `string` overload, and every other scoped run in
      Phases 2 through 6 is filtered to a class that cannot observe the result; without this task the
      regression would surface for the first time at the full-suite run in P7-T5. Acceptance: the
      output does not contain `No test matches the given testcase filter`; the run reports 11 tests for
      that class, which is the number of `[TestMethod]` members the file carries; the artifact names
      `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce` individually as **passing**,
      together with the statement that its `Times.Once()` assertion is what proves P2-T4 passed the
      existing `olAncestor` local rather than reading `Globals.Ol.ArchiveRootPath` a second time; and
      the failing set is a subset of the `BASELINE_FAILURE_SET` recorded in
      `evidence/baseline/p0-t15-mstest-coverage.md`, with every still-failing baseline member named. If
      that baseline set is empty, `EXIT_CODE: 0` with 11 passed and 0 failed is required. If
      `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce` is itself in the baseline failing
      set, record `BASELINE PROTECTS NOTHING`, stop, and report to the orchestrator: the invariant this
      task exists to protect would already be red before this plan ran, and no result here would
      distinguish a regression from that pre-existing state.

**Progress-commit boundary after task 35.** Stop after checking off `[P2-T10]` and return
`PROGRESS_COMMIT_REQUIRED: P0-T1..P2-T10`. Do not begin `[P2-T11]` until the orchestrator has used
canonical commit context and the routed commit-steward profile, then recorded the completed interval's
commit SHA in the canonical checkpoint.

- [x] [P2-T11] [expect-fail] Run the new router regression tests before the fix. Run
      `pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; $asm = Join-Path (Get-Location).Path "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll"; & $vstest $asm /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeRouterIssue637Tests&TestCategory!=LiveOutlook" /Logger:trx "/ResultsDirectory:coverage\testresults\p2-t11"; "EXIT_CODE=$LASTEXITCODE"'`
      and write `evidence/regression-testing/p2-t11-router-tests-red.md` with `ExpectedExitCode: 1`.
      Acceptance: the output does not contain `No test matches the given testcase filter`; the run
      reports 10 tests total; exactly these 5 fail, named individually in the artifact:
      `RowSelected_ArchiveRootExactFilingTarget_IsNotSelected`,
      `RowSelected_ArchiveRootExactFilingTarget_PreservesAPriorValidSelection`,
      `RowSelected_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem`,
      `RowSelected_RootedTargetUnderArchiveRoot_CaseInsensitiveAndTrailingSeparatorRoot_CommitsTheStem`,
      `SelectFirstRow_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem`; and exactly these 5
      pass: `RowSelected_RelativeFilingTarget_CommitsTheValueVerbatim`,
      `RowSelected_TrashPseudoRow_CommitsTheSentinelVerbatim`,
      `RowSelected_OutOfRootRootedTarget_IsStillRejected`,
      `RowSelected_SeparatorBoundaryNearMissTarget_IsStillRejected`,
      `RowSelected_RootedTargetWithNoBoundArchiveRoot_PassesThroughVerbatim`. A different partition is a
      defect in the tests, not evidence of the bug, and must be repaired before Phase 3.
- [x] [P2-T12] [expect-fail] Run the new helper tests before the fix. Run the P2-T11 command with the
      filter substring changed to `FullyQualifiedName~EfcDataModelIssue637Tests` and the results
      directory changed to `coverage\testresults\p2-t12`, and write
      `evidence/regression-testing/p2-t12-helper-tests-red.md` with `ExpectedExitCode: 1`. Acceptance:
      the output does not contain `No test matches the given testcase filter`; the run reports 8 tests
      total; exactly these 2 fail: `ToFilingStemOrVerbatim_RootedUnderAncestor_ReturnsTheStem` and
      `ToFilingStemOrVerbatim_RootedUnderCaseDifferingAncestor_ReturnsTheStem`; and the other 6 pass,
      because the seam already returns the input verbatim for every non-normalizable case.
- [x] [P2-T13] Prove the new test file actually executes rather than silently compiling into nothing.
      From the TRX produced by P2-T11 at `coverage\testresults\p2-t11`, extract every `UnitTestResult`
      whose `testName` begins with one of the ten fixed method names, and write
      `evidence/regression-testing/p2-t13-compile-include-observed.md`. Quote only the `testName` and
      `outcome` attribute values of each result. Do not quote the results file's name, and do not quote
      any absolute path: vstest composes that file name from the account and the machine, so quoting it
      would put host identity into a committed artifact, and the "Evidence transcript redaction"
      convention requires it to be written as `<trx-file>` if it must be referred to at all.
      Acceptance: the artifact records
      exactly 10 such results; it quotes the `Compile Include` line added by P2-T6 verbatim; and it
      records that removing that line would make this count 0, which is the observable AC26 requires.

### Phase 3 — Change A, producer normalization in `SelectRow`

- [x] [P3-T1] Apply change A in `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`. Replace
      the guard currently at lines 94-106 so that the `_boundRoot.Length != 0` and
      `ArchiveStemContract.IsFullOutlookPath(selection)` conjunction opens a block; inside that block,
      a failed `ArchiveStemContract.TryMakeArchiveRelative(selection, _boundRoot, out string stem)`
      logs the unchanged message `Breadcrumb row rejected: target is outside the archive root.` and
      returns; a succeeded call with `stem.Length == 0` logs the new message
      `Breadcrumb row rejected: target is the archive root itself.` and returns; otherwise `selection`
      is reassigned to `stem`. Control then falls through to the unchanged
      `CommitSelection(row, selection);`. `selection` is a non-nullable `string` and `stem` is
      definitely assigned by `ArchiveStemContract.cs:112` on every exit path, so no nullable temporary
      is introduced and `CommitSelection`'s non-nullable `string` parameter is satisfied. Replace the
      stale comment at lines 94-95, which asserts the superseded pass-verbatim behavior. The
      replacement comment must not contain the token `IsFullOutlookPath`; refer to the arm as the
      full-path gate instead, because this task's acceptance asserts an exact count of 1 for that
      token in this file. For the same reason the replacement comment must not contain the token
      `out string stem`, whose exact count of 2 this task also asserts, nor the token `out _`, whose
      count this task asserts is 0, nor either of the two diagnostic message literals whose exact
      count of 1 apiece P3-T2 asserts over this same file: describe the two rejection outcomes in
      prose without quoting their message text. Acceptance:
      `rg -n "out string stem" QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` returns 2
      lines (the new one in `SelectRow` and the existing one at `:120` in `SelectHierarchyPath`);
      `rg -n "out _" QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` returns 0 lines; and
      `rg -c "IsFullOutlookPath" QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` returns 1.
- [x] [P3-T2] Verify the nesting and the preserved diagnostics required by AC3 and AC6. Acceptance:
      `rg -n "Breadcrumb row rejected: target is outside the archive root." QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`
      returns exactly 1 line;
      `rg -n "Breadcrumb row rejected: target is the archive root itself." QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`
      returns exactly 1 line; neither message contains the character `@`; and the artifact
      `evidence/regression-testing/p3-t2-nesting.md` quotes the whole edited `SelectRow` body and
      records that `_boundRoot.Length != 0` is still the first conjunct, so the no-bound-root
      pass-through mode is untouched.
- [x] [P3-T3] Verify AC8: `SelectHierarchyPath` and `CommitSelection` are unmodified. Run
      `git add QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` then
      `git diff 0eda184ca0009bc79ac9b7146897270c17c095fa --cached -U0 -- QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`
      and write `evidence/regression-testing/p3-t3-selectionfile-diff.md`. Acceptance: every hunk
      header in the diff addresses a line range that lies entirely within the original lines 83 to 107;
      no hunk touches the original line range 109 to 139; and the artifact lists the hunk headers
      verbatim.
- [x] [P3-T4] Run the analyzer build and the nullable build using the P0-T13 and P0-T14 commands
      verbatim, and write `evidence/regression-testing/p3-t4-builds.md` recording both. Acceptance:
      both record `EXIT_CODE: 0`; both outputs contain `(Rebuild target(s))`; and neither recorded
      `Command:` line contains the solution-wide nullable opt-in property — record this as
      `NULLABLE_OPT_IN_PROPERTY: absent`; do not spell the token in the artifact.
- [x] [P3-T5] Run the router regression suite green. Use the P2-T11 command with the results directory
      changed to `coverage\testresults\p3-t5`, and write
      `evidence/regression-testing/p3-t5-router-tests-green.md`. Acceptance: `EXIT_CODE: 0`; 10 tests
      total; 10 passed; 0 failed; 0 skipped; and the five tests that failed in P2-T11 are named
      individually in the artifact as now passing.
- [x] [P3-T6] Run the unmodified router test classes to prove no collateral regression. Use the P2-T11
      command with the filter
      `"/TestCaseFilter:(FullyQualifiedName~BreadcrumbBridgeRouterIssue614Tests|FullyQualifiedName~BreadcrumbBridgeRouterTests|FullyQualifiedName~BreadcrumbBridgeRouterQueueTests)&TestCategory!=LiveOutlook"`
      and the results directory `coverage\testresults\p3-t6`, and write
      `evidence/regression-testing/p3-t6-router-siblings.md`. Acceptance: `EXIT_CODE: 0`; 0 failed; and
      the artifact records that `RowSelected_OutOfRootFilingTarget_DoesNotStoreAFullOutlookPath` and
      `SegmentActivate_WithNoBoundArchiveRoot_PreservesThePassThroughMode` both passed. This filter
      deliberately excludes `BreadcrumbBridgeRouterIssue439Tests`, whose pinning assertion is expected
      to be red between P3-T1 and P5-T1; that class is run green in P5-T6.

### Phase 4 — Change B, normalization in the `string` overload of `MoveToFolderAsync`

- [x] [P4-T1] Replace the seam body in `QuickFiler/Controllers/EfcDataModel.FilingStem.cs` with the
      real normalization. `ToFilingStemOrVerbatim` returns `candidatePath` unchanged when
      `ArchiveStemContract.IsFullOutlookPath(candidatePath)` is false; otherwise it calls
      `ArchiveStemContract.TryMakeArchiveRelative(candidatePath, archiveAncestor, out string stem)` and
      returns `stem` when that call succeeds and `stem.Length != 0`, and returns `candidatePath`
      unchanged in every other case. The method is total, never throws, performs no I/O, writes no log,
      and touches no static mutable state. It deliberately does not adopt
      `ToArchiveRelativeStem`'s throw on the archive-root-exact input; the rationale is recorded in
      `spec.md` under "Error handling and logging updates". Update the XML documentation to state the
      final contract and remove the seam wording added by P2-T2. The helper's XML documentation must
      not contain the token `MoveToFolder`; refer to its caller as "the `string` filing overload"
      instead, because P8-T16 asserts the family stem search still returns exactly 23 lines. Two
      further tokens are barred from that documentation for the same reason — each is a token an
      acceptance condition asserts an exact count for over this same file, and the natural wording of
      the contract would otherwise add an occurrence. First, `IsFullOutlookPath`: AC12 phrases the
      contract as "The helper is gated on `ArchiveStemContract.IsFullOutlookPath`", but this task
      asserts an exact count of 1 for that token in this file, so the documentation states the gate as
      "returns its input unchanged unless the input is a full Outlook path" without naming the
      predicate. Second, the character sequence `throw` in any form, including `throws`: this task
      asserts that the token is absent from this file both before and after the edit, so the totality
      claim is worded as "returns a value for every input and propagates no exception". The token
      `Globals.Ol.ArchiveRootPath` is barred as well and for an additional reason: the helper takes its
      archive ancestor as a parameter and must never name the global, because doing so would reopen the
      unguarded-read defect issue #638 closed. The documentation therefore describes the second
      parameter as the archive ancestor supplied by the caller. Record the run in
      `evidence/regression-testing/p4-t1-helper-implemented.md`, capturing the output of
      `rg -n "throw" QuickFiler/Controllers/EfcDataModel.FilingStem.cs` taken immediately before and
      immediately after the edit, each recorded with `ExpectedExitCode: 1` because ripgrep exits 1 on
      zero matches. Acceptance: both `rg` invocations report zero matches and exit non-zero, so the
      helper introduces no throw site and none was present in the seam;
      `rg -n "IsFullOutlookPath" QuickFiler/Controllers/EfcDataModel.FilingStem.cs` returns exactly 1
      line and it is inside the helper, where before this task it returned 0 lines;
      `rg -n "Globals" QuickFiler/Controllers/EfcDataModel.FilingStem.cs` returns 0 lines, recorded
      with `ExpectedExitCode: 1`;
      `pwsh -NoProfile -Command '(Get-Content -LiteralPath "QuickFiler\Controllers\EfcDataModel.FilingStem.cs").Count'`
      is at most 500; and
      `pwsh -NoProfile -Command '(Get-Content -LiteralPath "QuickFiler\Controllers\EfcDataModel.cs").Count'`
      reports exactly 485, because this task changes no line of that file.
- [x] [P4-T2] Record the helper's line range and verify its purity, and write
      `evidence/regression-testing/p4-t2-helper-shape.md`. Acceptance: the artifact records the first
      and last line numbers of the `ToFilingStemOrVerbatim` declaration body **in
      `QuickFiler/Controllers/EfcDataModel.FilingStem.cs`**, and records separately that
      `QuickFiler/Controllers/EfcDataModel.cs` contains no part of the helper, so a reader cannot
      mistake one file's line numbers for the other's; it records that the body
      contains no `await`, no `Globals`, no `logger`, and no `throw`; it records that the only call
      sites of the helper are the single assignment at `EfcDataModel.cs:337` and the eight tests in
      `EfcDataModelIssue637Tests`, verified by `rg -n "ToFilingStemOrVerbatim" --glob "*.cs" .`; and it
      enumerates every line `rg -n "ToFilingStemOrVerbatim" --glob "*.cs" QuickFiler/` returns and
      classifies each as the single declaration, the single call, or an XML-documentation reference,
      with exactly one declaration and exactly one call. A second call site anywhere in `QuickFiler/`
      fails this task.
- [x] [P4-T3] Run the analyzer build and the nullable build using the P0-T13 and P0-T14 commands
      verbatim, and write `evidence/regression-testing/p4-t3-builds.md`. Acceptance: both record
      `EXIT_CODE: 0`; both outputs contain `(Rebuild target(s))`; and neither recorded `Command:` line
      contains the solution-wide nullable opt-in property — record this as
      `NULLABLE_OPT_IN_PROPERTY: absent`; do not spell the token in the artifact.
- [x] [P4-T4] Run the helper test class green. Use the P2-T12 command with the results directory
      changed to `coverage\testresults\p4-t4`, and write
      `evidence/regression-testing/p4-t4-helper-tests-green.md`. Acceptance: `EXIT_CODE: 0`; 8 tests
      total; 8 passed; 0 failed; and the two tests that failed in P2-T12 are named individually as now
      passing.
- [x] [P4-T5] Prove the eight existing `ToArchiveRelativeStem` tests are unchanged and still pass. Use
      the P2-T11 command with the filter
      `"/TestCaseFilter:FullyQualifiedName~EfcDataModelIssue614Tests&TestCategory!=LiveOutlook"` and the
      results directory `coverage\testresults\p4-t5`, and write
      `evidence/regression-testing/p4-t5-toarchiverelativestem-unchanged.md`. Acceptance:
      `EXIT_CODE: 0`; the run reports 8 tests for that class; 8 passed including
      `ToArchiveRelativeStem_ArchiveRootItself_Throws`; and, after
      `git add QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`,
      `git diff 0eda184ca0009bc79ac9b7146897270c17c095fa --cached -- QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`
      shows zero removed content lines.
- [x] [P4-T6] Verify AC17: the non-goals are untouched. Run
      `git add QuickFiler/Controllers/EfcDataModel.cs` then
      `git diff 0eda184ca0009bc79ac9b7146897270c17c095fa --cached -U0 -- QuickFiler/Controllers/EfcDataModel.cs`
      and write `evidence/regression-testing/p4-t6-nongoals-untouched.md`, quoting every hunk header
      verbatim. This plan makes exactly two edits to this file, both single-line substitutions, so the
      file's line count and every line number in it are unchanged and the diff is exactly two hunks.
      Acceptance, in five parts.
      First, the diff contains exactly two hunk headers: one addressing old line 21 only — the
      `partial` keyword added by P2-T1, accepted in either the `-21 +21` or the `-21,1 +21,1` spelling
      git may emit under `-U0` — and one addressing old line 337 only, the `DestinationOlStem`
      assignment redirected by P2-T4. Any third hunk, and any hunk whose old-side range spans more
      than one line, fails this task.
      Second, no hunk header addresses any line inside the protected range 271 to 297. That range is
      `TryGetArchiveRoot`, introduced by issue #638 with its declaration at 280; it is that issue's
      code and this plan must not touch it.
      Third, no hunk header addresses any line inside 349 to 396 (`OpenOlFolderAsync` at 349-372 and
      `OpenFsFolderAsync` at 374-396) or inside 398 to 448 (the `MAPIFolder` overload at 398-419 and
      `ToArchiveRelativeStem` at 421-448).
      Fourth, `rg -n "Globals.Ol.ArchiveRootPath" QuickFiler/Controllers/EfcDataModel.cs` returns
      **exactly 1** line, and it is line 284. The artifact quotes it and classifies it as the single
      guarded read inside `TryGetArchiveRoot`. There is no second read to classify: issue #638
      replaced the three former `OlAncestor` initializer reads with the `out var olAncestor` local
      produced by `TryGetArchiveRoot`, and P2-T4 is required to pass that same local rather than the
      global.
      Fifth, the artifact records the inverse assertion that actually protects the non-goal: the single
      read at line 284 remains inside the `try` block at 282-286, whose `catch (InvalidOperationException ex)`
      is at 287, exactly as issue #638 wrote it, and no read is added, removed, or moved into or out of
      that block. The artifact quotes lines 280 through 297 verbatim to evidence this.

### Phase 5 — Change C, the recorded spec correction to the issue #439 assertion

- [x] [P5-T1] In `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`, replace line 165
      `            router.SelectedFolderPath.Should().Be(fullTarget);` with
      `            router.SelectedFolderPath.Should().Be(@"Clients\North");`. The expected value is
      derived from the fixture in the same method: `archiveRoot` is `@"\Archive"` at `:123` and
      `fullTarget` is `@"\aRcHiVe\Clients\North"` at `:124`, so `TryMakeArchiveRelative` matches through
      the `OrdinalIgnoreCase` `StartsWith` at `ArchiveStemContract.cs:131`, the boundary character at
      index 8 is a backslash per `:137-141`, and the stem is
      `fullTarget.Substring(8).TrimStart('\\','/')`, which is `Clients\North`. Acceptance: the
      fixed-string search
      `rg -F -n 'router.SelectedFolderPath.Should().Be(@"Clients\North");' QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`
      returns exactly 1 line and it is line 165, and the fixed-string search
      `rg -F -n 'router.SelectedFolderPath.Should().Be(fullTarget);' QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`
      returns 0 lines.
- [x] [P5-T2] Rename the enclosing method at line 119 from
      `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch` to
      `Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively`, on one line so the file
      line count is unchanged. Acceptance:
      `rg -n "Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch" --glob "*.cs" .`
      returns 0 lines, and
      `rg -n "Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively" --glob "*.cs" .`
      returns exactly 1 line.
- [x] [P5-T3] Narrow the arrange comment at lines 121-122 to the provider claim it still supports,
      keeping it exactly two lines so the file line count is unchanged. The replacement text is:
      `            // Arrange: the presented target is rooted with casing different from the configured`
      and
      `            // root, so the provider must receive the original full path unchanged (#439).`
      The asserted token below is the replacement's own distinguishing suffix rather than the phrase
      the two versions share. Line 122 already reads
      `            // root, so the provider must receive the original full path unchanged.` before this
      task runs, so a search for that shared phrase returns 1 line before any edit and cannot fail. The
      literal `unchanged (#439).` is absent from the tracked tree until this task writes it and is
      quoted here verbatim for that reason; it is asserted with `-F` because the parentheses and the
      period are regex metacharacters that must match those same characters. Acceptance:
      `rg -F -n 'unchanged (#439).' QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`
      returns exactly 1 line and it is line 122, and
      `rg -n "already rooted with casing different" QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`
      returns 0 lines, recorded with `ExpectedExitCode: 1`.
- [x] [P5-T4] Verify AC19: the companion provider assertion and `ToHierarchyPath` are preserved. Write
      `evidence/regression-testing/p5-t4-provider-assertion-preserved.md`. Acceptance: lines 161 to 164
      of `BreadcrumbBridgeRouterIssue439Tests.cs` are byte-identical to their pre-change text, quoted
      in the artifact; and, after `git add QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`,
      `git diff 0eda184ca0009bc79ac9b7146897270c17c095fa --cached -U0 -- QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`
      produces no output at all, since this plan changes no line of that file.
- [x] [P5-T5] Verify the file did not grow and that exactly one assertion changed. Run
      `pwsh -NoProfile -Command '(Get-Content -LiteralPath "QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue439Tests.cs").Count'`,
      then `git add QuickFiler.Test` followed by
      `git diff 0eda184ca0009bc79ac9b7146897270c17c095fa --cached -- QuickFiler.Test`, and write
      `evidence/regression-testing/p5-t5-single-assertion-change.md`. Acceptance: the line count is
      exactly 694; among the diff's removed content lines, exactly one matches `.Should()`, and it is
      `            router.SelectedFolderPath.Should().Be(fullTarget);`; and the artifact records the
      change as a deliberate spec correction, stating that the issue #439 criterion that a rooted target
      survives selection is superseded by issue #614's archive-relative-stem invariant, which #614
      enforced on the `SelectHierarchyPath` half and at the filing boundary but not on the `SelectRow`
      half, and that this is explicitly not a weakened test; and the artifact additionally records the
      P5-T2 rename (both the removed and the added method name) and the two replacement comment lines
      from P5-T3, quoted verbatim, so that all three clauses of AC18 are evidenced in one artifact.
- [x] [P5-T6] Run the issue #439 test class green. First re-run the P0-T13 analyzer build command
      verbatim so that the Phase 5 test edits are compiled into
      `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`; record its `EXIT_CODE:` and its
      `(Rebuild target(s))` line in the same artifact. Without this rebuild the scoped run would
      execute the assembly P4-T3 produced, which still carries the old method name and the old
      assertion, and its acceptance would be unsatisfiable. Then use the P2-T11 command with the filter
      `"/TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeRouterIssue439Tests&TestCategory!=LiveOutlook"`
      and the results directory `coverage\testresults\p5-t6`, and write
      `evidence/regression-testing/p5-t6-issue439-green.md`. Acceptance: `EXIT_CODE: 0`; 0 failed; and
      the artifact records that both `Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively`
      and `Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection` passed.

### Phase 6 — Change D, stale-comment cleanup

- [x] [P6-T1] Replace `QuickFiler/Controllers/EfcSelectionGuard.cs:30` with the fixed replacement text
      given in "Fixed identifiers", item 1. The surrounding claim that the guard still rejects rooted
      values stays as written; only the deferral wording changes. Acceptance:
      `rg -n "is implemented by issue #637" QuickFiler/Controllers/EfcSelectionGuard.cs` returns
      exactly 1 line, and `(Get-Content -LiteralPath "QuickFiler\Controllers\EfcSelectionGuard.cs").Count`
      is exactly 79.
- [x] [P6-T2] Replace `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:146` with the fixed
      replacement text given in "Fixed identifiers", item 2. Acceptance:
      `rg -n "the producer normalizes" QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` returns
      exactly 1 line, and it is line 146.
- [x] [P6-T3] Replace the `because` string at `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:152`
      with the fixed replacement text given in "Fixed identifiers", item 3, on one line so the file
      line count is unchanged. Acceptance:
      `rg -n "the producer now normalizes before this predicate is reached" QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs`
      returns exactly 1 line, and
      `(Get-Content -LiteralPath "QuickFiler.Test\Controllers\EfcSelectionGuardTests.cs").Count` is
      exactly 296.
- [x] [P6-T4] Verify the deferral is gone and the guard's behavior is unchanged. First re-run the
      P0-T13 analyzer build command verbatim so that the P6-T2 and P6-T3 test edits are compiled into
      `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`; record its `EXIT_CODE:` and its
      `(Rebuild target(s))` line in the same artifact. Without this rebuild the scoped run would
      execute the assembly P4-T3 produced, which predates those edits, so the run would not be
      evidence about the edited file that AC23 requires. Then run
      `rg -c "deferred to issue #637" --glob "*.cs" .`, then run the P2-T11 command with the filter
      `"/TestCaseFilter:FullyQualifiedName~EfcSelectionGuardTests&TestCategory!=LiveOutlook"` and the
      results directory `coverage\testresults\p6-t4`, and write
      `evidence/regression-testing/p6-t4-deferral-cleared.md`. The `*.cs` glob is load-bearing: the
      phrase remains present in `spec.md`, in the research file, and in this plan, all of which are
      Markdown and are correctly excluded. Acceptance: the `rg` invocation reports 0 matches and exits
      non-zero, recorded with `ExpectedExitCode: 1` for that step; the scoped test run records
      `EXIT_CODE: 0` with 0 failed; and the artifact records that
      `Issue614_GuardAcceptedSelection_DoesNotThrowAtFilingBoundary` passed and that neither
      `IsValidFilingSelection` nor `IsValidCreationSelection` had any executable line changed, verified
      by a `git diff 0eda184ca0009bc79ac9b7146897270c17c095fa --cached -- QuickFiler/Controllers/EfcSelectionGuard.cs`
      run in the same task after `git add QuickFiler/Controllers/EfcSelectionGuard.cs`, whose only
      changed line is line 30.
- [x] [P6-T5] Redact host identity from every evidence artifact written so far, then prove it. Apply
      the "Evidence transcript redaction" convention to every file already written under
      `docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637/evidence/`:
      replace every occurrence of the absolute worktree path with the literal `<worktree-root>`, and
      every remaining absolute path beginning with the Windows per-user profile root with
      `<user-profile>`, and every vstest results file name with `<trx-file>`. Then run two searches.
      Search 1:
      `rg -F -n 'C:\Users' docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637 --glob '**/evidence/**' --glob '!**/p6-t5-evidence-redaction.md'`.
      Search 2:
      `rg -F -n '.trx' docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637 --glob '**/evidence/**' --glob '!**/p6-t5-evidence-redaction.md'`.
      Write `evidence/other/p6-t5-evidence-redaction.md` with `ExpectedExitCode: 1`. Acceptance:
      both searches report 0 matches and exit non-zero; the artifact lists every evidence file it
      rewrote and, for each, the number of replacements made; the artifact records that a zero-match
      result over the two-component prefix `C:\Users` proves the account segment is absent as well,
      because on Windows that segment always follows the prefix immediately; and it records that
      Search 2 covers the vstest results file name, which carries the account and the machine with no
      preceding profile path and so is invisible to Search 1. The `-F` flag is required in both
      because the backslash and the period are regex metacharacters that must match themselves. The
      `--glob` exclusion of
      this task's own artifact is required for the same reason P7-T10 excludes its own: this artifact
      records its own two `Command:` lines, and those commands' patterns are the strings being searched
      for. No other evidence file of this feature is excluded.
- [x] [P6-T6] Prepare the changes-A-through-D HEAD-materialization boundary. Write
      `evidence/other/p6-t6-commit.md`, check off this task, and return
      `PROGRESS_COMMIT_REQUIRED: P2-T11..P6-T6` without invoking `git commit`. The orchestrator must
      stage `QuickFiler`, `QuickFiler.Test`, and this feature folder; collect canonical commit context;
      obtain the message from the routed commit-steward profile; create the commit; and record its SHA
      before resuming Phase 7. This intermediate commit is required because every Phase 7 and Phase 8
      gate is anchored to `0eda184ca0009bc79ac9b7146897270c17c095fa..HEAD`, and an anchored diff
      reports nothing for changes that are not yet committed. Acceptance after the orchestrator resumes
      execution: the checkpoint records the interval and a non-empty commit SHA;
      `git status --porcelain -- QuickFiler QuickFiler.Test` produces no output; and
      `git diff --name-only 0eda184ca0009bc79ac9b7146897270c17c095fa..HEAD -- QuickFiler QuickFiler.Test`
      lists exactly these ten paths and no others:
      `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`,
      `QuickFiler/Controllers/EfcDataModel.cs`,
      `QuickFiler/Controllers/EfcDataModel.FilingStem.cs`,
      `QuickFiler/Controllers/EfcSelectionGuard.cs`,
      `QuickFiler/QuickFiler.csproj`,
      `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`,
      `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue637Tests.cs`,
      `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`,
      `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs`,
      `QuickFiler.Test/QuickFiler.Test.csproj`.
      The enumeration is ten rather than the eight this plan named before the change-B file split: the
      split adds `QuickFiler/Controllers/EfcDataModel.FilingStem.cs` and the
      `QuickFiler/QuickFiler.csproj` registration that makes it compile.

### Phase 7 — Final QC toolchain loop and coverage delta

Run the four steps in order. If any step fails or changes a file, return to P7-T1 and run the phase
again from the start.

- [x] [P7-T1] Format. Record
      `git status --porcelain -- QuickFiler QuickFiler.Test docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`
      before the run, then run
      `pwsh -NoProfile -Command 'dotnet tool run csharpier format .; "EXIT_CODE=$LASTEXITCODE"'`, then
      record the same `git status --porcelain` invocation after the run, and write
      `evidence/qa-gates/p7-t1-csharpier-format.md`. The exit code alone proves nothing here because
      CSharpier exits 0 whether or not it rewrote a file, and its summary line was not observed in
      this planning session, so no acceptance condition here reads it. Acceptance: `EXIT_CODE: 0`; both porcelain
      outputs are recorded verbatim; and any path that differs between them is either a path this plan
      changed or a path listed in the `BASELINE_FORMAT_DRIFT` section of
      `evidence/baseline/p0-t12-csharpier-check.md`. A path that is in neither set means the repo-wide
      format pass touched unrelated source and must be reported to the orchestrator before proceeding.
      In the same task, run
      `git status --porcelain -- UtilitiesCS UtilitiesCS.Test TaskMaster TaskMaster.Test ToDoModel Tags TaskVisualization`
      after the format run and record its output verbatim. That span must produce no output. It is
      required because this task's other two porcelain spans are scoped to `QuickFiler`,
      `QuickFiler.Test` and this feature's folder, so a repo-wide format rewrite in any of these seven
      trees is invisible to them and would first surface at P8-T30, after the commit that could have
      carried it. Any line here means the repo-wide format pass touched a tree this plan does not own;
      stop and report to the orchestrator.
- [x] [P7-T2] Verify the format. Run
      `pwsh -NoProfile -Command 'dotnet tool run csharpier check .; "EXIT_CODE=$LASTEXITCODE"'` and
      write `evidence/qa-gates/p7-t2-csharpier-check.md`. Acceptance: `EXIT_CODE: 0`, and the captured
      stdout is recorded verbatim. The exit code is the gate here rather than any summary wording,
      because `check` is read-only and returns non-zero exactly when some file would be reformatted;
      the write-mode discrimination that a read-only command cannot supply is provided by P7-T1's
      before-and-after porcelain pair. Record the boundary-ready state in
      `evidence/qa-gates/p7-t2-csharpier-check.md`, check off this task, and return
      `PROGRESS_COMMIT_REQUIRED: P7-T1..P7-T2` without invoking `git commit`. The orchestrator must
      stage the in-scope paths, collect canonical commit context, obtain the message from the routed
      commit-steward profile, create the commit, and record its SHA before P7-T3 begins. This ensures
      every subsequent `0eda184ca0009bc79ac9b7146897270c17c095fa..HEAD` diff describes the same file
      contents the P7-T5 build measures. If CSharpier changed no source, the evidence and plan check-off
      still make the boundary non-empty; the artifact records that the source already matched `HEAD`.
- [ ] [P7-T3] Analyzers. Run the P0-T13 command verbatim and write
      `evidence/qa-gates/p7-t3-msbuild-analyzers.md`. Acceptance: `EXIT_CODE: 0`; the output contains
      `(Rebuild target(s))`; the `Error(s)` count is 0; and the recorded `Command:` line contains
      `/t:Rebuild` and `EnableNETAnalyzers=true` and `EnforceCodeStyleInBuild=true` and contains
      neither `/t:Build` nor the solution-wide nullable opt-in property — record this as
      `NULLABLE_OPT_IN_PROPERTY: absent`; do not spell the token in the artifact.
- [ ] [P7-T4] Nullable. Run the P0-T14 command verbatim and write
      `evidence/qa-gates/p7-t4-msbuild-nullable.md`. Acceptance: `EXIT_CODE: 0`; the output contains
      `(Rebuild target(s))`; and the recorded `Command:` line contains `/t:Rebuild` and
      `TreatWarningsAsErrors=true` and contains neither `/t:Build` nor the solution-wide nullable
      opt-in property — record this as `NULLABLE_OPT_IN_PROPERTY: absent`; do not spell the token in
      the artifact.
- [ ] [P7-T5] Full test run with coverage. Run
      `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\p7-t5-postchange.cobertura.xml`
      and write `evidence/qa-gates/p7-t5-mstest-coverage.md`. The exit-code condition is stated
      against the recorded baseline rather than as an unconditional zero, because the wrapper throws
      whenever the inner vstest run reports any failure and a repository-wide zero-failure demand would
      be unsatisfiable if the baseline itself carried failures. Acceptance: the number of discovered
      test assemblies matches the number recorded in `evidence/baseline/p0-t15-mstest-coverage.md`; the
      post-change failing set is a subset of the `BASELINE_FAILURE_SET` recorded there; no test that
      passed in the baseline is failing now; the artifact names every baseline failure that is still
      failing; and when `BASELINE_FAILURE_SET` is empty, 0 failed is required. The exit code is judged
      separately, because the wrapper throws on two independent conditions: a non-zero inner vstest
      exit (`Invoke-MSTestWithCoverage.ps1:235-237`) and a repository line rate below 80 percent
      (`Invoke-MSTestWithCoverage.Helpers.ps1:487-489`, called at `:341`). When the captured output
      contains the literal `is below the required 80% threshold.`, the artifact records
      `COVERAGE_FLOOR_THROW: yes` together with the printed percentage, records `ExpectedExitCode: 1`,
      and P7-T8's `BASELINE BELOW FLOOR` branch governs; the on-disk
      `coverage\p7-t5-postchange.cobertura.xml` is then the raw dotnet-coverage output, because the
      threshold assertion at `:341` precedes the post-processed write-back at `:343`, and P7-T6 and
      P7-T7 re-apply `ConvertTo-KoverageCoberturaXml` in memory and are unaffected. When that literal
      is absent, the artifact records `COVERAGE_FLOOR_THROW: no` and `EXIT_CODE: 0` is required
      whenever `BASELINE_FAILURE_SET` is empty. `Output Summary:` additionally carries the six
      numeric `/coverage` attribute values and the derived line and branch percentages. As a
      read-only P7-T5 substep, run the P7-T6 coverage-headline command against
      `coverage\p7-t5-postchange.cobertura.xml`, record those values in this P7-T5 `Output Summary:`,
      verify P7-T5, and check P7-T5 off before starting P7-T6. P7-T6 does not modify P7-T5.
- [ ] [P7-T6] Read the post-change numeric coverage headline. Run the P0-T16 command with the input
      path changed to `.\coverage\p7-t5-postchange.cobertura.xml` and write
      `evidence/qa-gates/p7-t6-coverage-headline.md`. Acceptance: `EXIT_CODE: 0`, and `Output Summary:`
      records all six numeric attribute values plus the derived line-coverage percentage and branch
      percentage. Confirm that all six attributes and both derived percentages equal the values
      already recorded in `evidence/qa-gates/p7-t5-mstest-coverage.md`; P7-T6 does not modify that
      artifact or any prior checklist state.
- [ ] [P7-T7] Verify changed-line coverage. Run the P0-T17 command with the input path changed to
      `.\coverage\p7-t5-postchange.cobertura.xml` and with its file list extended to the three
      production files this plan touches — `QuickFiler\Controllers\BreadcrumbBridgeRouter.Selection.cs`,
      `QuickFiler\Controllers\EfcDataModel.cs` and
      `QuickFiler\Controllers\EfcDataModel.FilingStem.cs` — and in the same task run
      `git diff -U0 0eda184ca0009bc79ac9b7146897270c17c095fa..HEAD -- QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs QuickFiler/Controllers/EfcDataModel.cs QuickFiler/Controllers/EfcDataModel.FilingStem.cs`
      to enumerate the added line numbers from the hunk headers. The base anchor
      `0eda184ca0009bc79ac9b7146897270c17c095fa` is the post-merge, pre-change baseline P0-T8 proved
      clean, so the added-line set this diff produces contains only lines this plan added and none
      of issue #638's or issue #644's. Re-derive the
      `ToFilingStemOrVerbatim` line range against the post-format working tree in this same task,
      recording the declaration line and the closing-brace line, and record both that range and the
      range `evidence/regression-testing/p4-t2-helper-shape.md` recorded, stating whether they differ.
      Every coverage assertion in this task is evaluated against the re-derived range; the P4-T2 range
      is recorded for audit only. The range is re-derived against
      `QuickFiler/Controllers/EfcDataModel.FilingStem.cs`, which is the file that declares the helper;
      `QuickFiler/Controllers/EfcDataModel.cs` declares no part of it. This re-derivation is required
      because P4-T2 measured the range in
      Phase 4, P7-T1 then ran the write-mode formatter over `EfcDataModel.FilingStem.cs` — the first
      format pass over the hand-written helper body, since Phases 2 through 6 contain no format step —
      and P7-T5 measured the tree after it, so a formatter change to the helper's extent would make the
      P4-T2 range identify the wrong lines. Write
      `evidence/qa-gates/p7-t7-changed-line-coverage.md`. Acceptance: the artifact lists, per file, the
      set of added line numbers and the set of line numbers with zero hits; the intersection of those
      two sets is empty for all three files; and, for
      `QuickFiler/Controllers/EfcDataModel.FilingStem.cs`,
      every line number inside the re-derived `ToFilingStemOrVerbatim` range that carries a `<line>`
      node in the post-change Cobertura document has non-zero hits, and the artifact separately
      enumerates every line number inside that range that carries no `<line>` node, classifying each
      as XML documentation, the method signature, a blank line, or a brace. That split is required
      because Cobertura emits a `<line>` node only for a sequence point, so a documentation or
      signature line has no `hits` attribute to read and an assertion over it could never be
      satisfied. The artifact also records that at least one line inside the range carries a `<line>`
      node, which is the observation that keeps this clause from passing vacuously on a range with no
      coverage rows at all. Together these are the new-code coverage requirement for the new helper
      stated in AC29; and, for the re-derived
      `ToFilingStemOrVerbatim` range, the artifact records the line nodes carrying `branch="True"`
      together with their `condition-coverage` values, and states that the `IsFullOutlookPath`
      conditional shows both branches taken — that is, a `condition-coverage` value of the form
      `100% (2/2)` on that line — which is AC29's "both sides of its gate exercised" clause. The
      capitalized `True` and the `condition-coverage` spelling are the forms the runner actually
      emits, observed in
      `docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/qa-gates/coverage-final.cobertura.xml`,
      where `branch="True"` occurs 11674 times and `branch="true"` occurs zero times; a lowercase
      match must not be substituted. If no line node inside the re-derived range carries
      `branch="True"` at all — the case where the compiler emits no separate branch point for the
      guard — the artifact records that observation verbatim and satisfies AC29's clause instead by
      naming the two fixed witness tests recorded green in
      `evidence/regression-testing/p4-t4-helper-tests-green.md`:
      `ToFilingStemOrVerbatim_RootedUnderAncestor_ReturnsTheStem` for the true side and
      `ToFilingStemOrVerbatim_RelativeStem_ReturnsTheInputVerbatim` for the false side. The executor
      does not choose between these two forms of proof: the branch-node form applies whenever a
      `branch="True"` node exists in the re-derived range, and the witness-test form applies only when
      none does. The `HEAD` this diff is anchored against already carries the formatting result,
      because P7-T2 committed it before P7-T5 measured the tree.
- [ ] [P7-T8] Report the coverage delta. Write `evidence/qa-gates/p7-t8-coverage-delta.md` containing
      three labelled numeric sections: baseline coverage, copied from
      `evidence/baseline/p0-t16-coverage-headline.md`; post-change coverage, copied from
      `evidence/qa-gates/p7-t6-coverage-headline.md`; and changed and new-code coverage, copied from
      `evidence/qa-gates/p7-t7-changed-line-coverage.md`. The artifact also states the coverage-floor
      authority resolution this plan applies, in these terms: the General Unit Test Policy in
      `AGENTS.md` and `.agents/skills/csharp/SKILL.md` both require repository-wide line coverage at or
      above 80 percent, new modules, classes and methods to target at least 90 percent coverage, and no
      coverage regression on changed lines. The binding repository-wide figure is also the one the
      runner enforces, at
      `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487`, so the gate this task applies and the
      gate the tooling applies are the same number. Acceptance: all three sections carry numeric
      values and none carries a placeholder; the post-change line-coverage percentage is at or above
      **80**, and that clause is blocking; the artifact carries the authority statement above with both
      Codex policy citations; the new helper meets the at-least-90-percent target through P7-T7's
      stronger requirement that every emitted Cobertura sequence point has non-zero hits; and the
      changed-line section records an empty uncovered intersection. One exception applies to
      the blocking clause and to nothing else: if the baseline figure recorded in
      `evidence/baseline/p0-t16-coverage-headline.md` is itself already below 80, the artifact records
      `BASELINE BELOW FLOOR`, reports that pre-existing condition to the orchestrator, and the binding
      requirement becomes that the post-change figure is at or above the recorded baseline figure. The
      change-scoped gates — no changed line loses coverage, and every line of the new helper is covered
      — remain blocking in every case, including under that exception.

**Progress-commit boundary after task 70.** Stop after checking off `[P7-T8]` and return
`PROGRESS_COMMIT_REQUIRED: P2-T11..P7-T8`. Do not begin `[P7-T9]` until the orchestrator has used
canonical commit context and the routed commit-steward profile, then recorded the boundary SHA and all
intermediate HEAD-materialization SHAs within this task interval in the canonical checkpoint.

- [ ] [P7-T9] File-size audit, run after the formatter rather than before it, because CSharpier can
      change a file's line count. Run
      `pwsh -NoProfile -Command 'foreach ($p in @("QuickFiler\Controllers\EfcDataModel.cs","QuickFiler\Controllers\EfcDataModel.FilingStem.cs","QuickFiler\Controllers\BreadcrumbBridgeRouter.Selection.cs","QuickFiler\Controllers\EfcSelectionGuard.cs","QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue439Tests.cs","QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue637Tests.cs","QuickFiler.Test\Controllers\EfcDataModelIssue614Tests.cs","QuickFiler.Test\Controllers\EfcSelectionGuardTests.cs")) { $p + "=" + (Get-Content -LiteralPath $p).Count }'`
      and write `evidence/qa-gates/p7-t9-file-sizes.md`. Acceptance: `EfcDataModel.cs` is at most 500,
      with the exact value recorded, and the artifact states whether it is still 485, which is the
      value the plan expects because this plan makes only single-line substitutions in it;
      `EfcDataModel.FilingStem.cs` is at most 500, with the exact value recorded;
      `BreadcrumbBridgeRouterIssue637Tests.cs` is at most 500; `EfcDataModelIssue614Tests.cs` is at most
      500; `BreadcrumbBridgeRouterIssue439Tests.cs` is at most 694 and therefore has not grown, with
      the exact value recorded; `EfcSelectionGuard.cs` is at most 79, with the exact value recorded;
      `EfcSelectionGuardTests.cs` is at most 296, with the exact value recorded; and
      `BreadcrumbBridgeRouter.Selection.cs` is at most 500. The upper bounds replace exact
      equalities because this task runs after a write-mode formatter that can reduce a line count,
      and AC25 requires only that these files not grow.
- [ ] [P7-T10] Toolchain non-vacuity audit. Write `evidence/qa-gates/p7-t10-toolchain-audit.md`
      enumerating the four final-QC command steps in order with their recorded `Command:` lines quoted
      verbatim. Acceptance: the format step is `dotnet tool run csharpier format .` invoked through
      `dotnet tool run` and not through a globally installed binary; both MSBuild `Command:` lines
      contain `/t:Rebuild` and neither contains `/t:Build`; no evidence artifact of this feature
      spells the solution-wide nullable opt-in token, verified by
      `rg -n 'Nullable=enable' docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637 --glob '**/evidence/**/*.md' --glob '!**/p7-t10-toolchain-audit.md'`
      returning 0 matches and exiting non-zero, recorded with `ExpectedExitCode: 1` for that step;
      and both MSBuild transcripts contain `(Rebuild target(s))`. Three scope restrictions on that
      command are load-bearing. The directory operand must be this feature's folder rather than
      `docs/features/active`, because many evidence Markdown files under other feature folders in that
      tree contain the token and this plan cannot change them, so the parent-directory form can never
      return 0 and the gate could never pass. No count of those files is stated here deliberately: the
      count changes as other feature work lands, and this justification does not depend on its value.
      The restriction to the evidence subtree is required
      because `spec.md` and this plan both discuss the token in prose and both live at the root of
      this feature's folder, so a scan of the folder without the `**/evidence/**/*.md` glob would
      never return 0 either. The exclusion of `p7-t10-toolchain-audit.md` is required because this
      artifact must record its own scan under `Command:`, and that command's pattern is the token
      itself, so without the exclusion the gate would be defeated by the plan's own recording
      instruction. Every other evidence artifact of this feature is in scope, and the "Nullable opt-in
      token discipline" convention keeps each of them free of the token by recording
      `NULLABLE_OPT_IN_PROPERTY: absent` instead. This artifact
      additionally records the two deliberate substitutions against AC28's literal command list, each
      with its justification: `msbuild` is invoked through the vswhere-resolved absolute path rather
      than as a bare `msbuild` PATH entry, with the switch list character-for-character identical to
      AC28's, and the artifact records the output of
      `pwsh -NoProfile -Command 'if (Get-Command msbuild -ErrorAction SilentlyContinue) { "ON_PATH" } else { "NOT_ON_PATH" }'`
      stating which of the two outcomes was observed and what it means. When the probe
      records `NOT_ON_PATH`, the vswhere resolution is necessary and the artifact records it as such.
      When the probe records `ON_PATH`, the artifact additionally records the output of the command
      that prints the resolved source of the `msbuild` command —
      `pwsh -NoProfile -Command '(Get-Command msbuild).Source'` — and states that the vswhere
      resolution is used for determinism rather than necessity, because the PATH entry is not pinned
      by this repository and can resolve to a different MSBuild, and records whether the two resolved
      paths name the same binary; and the
      test step is `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, whose inner
      vstest call at `Invoke-MSTestWithCoverage.ps1:76` carries `/InIsolation` and
      `/TestCaseFilter:TestCategory!=LiveOutlook` and whose coverage is collected by
      `dotnet-coverage --output-format cobertura` rather than by `/EnableCodeCoverage`, which is the
      repository's standard runner and the local analogue of
      `.github/workflows/_mstest-coverage.yml:83`. The substitutions are recorded, not resolved.
- [ ] [P7-T11] Redact host identity from the Phase 7 evidence artifacts, then prove it. Apply the
      "Evidence transcript redaction" convention to every file written under
      `docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637/evidence/`
      since P6-T5 ran: replace every occurrence of the absolute worktree path with the literal
      `<worktree-root>`, and every remaining absolute path beginning with the Windows per-user profile
      root with `<user-profile>`, and every vstest results file name with `<trx-file>`. The Phase 7
      artifacts are the ones most exposed to this, because
      P7-T3 and P7-T4 record MSBuild transcripts and MSBuild prints an absolute project path for every
      project it builds. Then run two searches. Search 1:
      `rg -F -n 'C:\Users' docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637 --glob '**/evidence/**' --glob '!**/p7-t11-evidence-redaction.md' --glob '!**/p6-t5-evidence-redaction.md'`.
      Search 2:
      `rg -F -n '.trx' docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637 --glob '**/evidence/**' --glob '!**/p7-t11-evidence-redaction.md' --glob '!**/p6-t5-evidence-redaction.md'`.
      Write `evidence/qa-gates/p7-t11-evidence-redaction.md` with `ExpectedExitCode: 1`.
      Acceptance: both searches report 0 matches and exit non-zero; the artifact lists every evidence
      file it rewrote and, for each, the number of replacements made; and the artifact records that the
      scan covers the whole feature evidence tree, so it re-verifies the Phase 0 through Phase 6
      artifacts P6-T5 cleared as well as the Phase 7 artifacts. The `-F` flag is required in both
      because the backslash and the period are regex metacharacters that must match themselves.
      Exactly two evidence artifacts are
      excluded, and both for the same reason P7-T10 excludes its own: each records its own `Command:`
      lines, and those commands' patterns are the strings being searched for. They are this task's
      artifact and `evidence/other/p6-t5-evidence-redaction.md`, which P6-T5 wrote earlier in the same
      feature tree. Omitting the second exclusion would make this gate unsatisfiable. No other evidence
      file of this feature is excluded.
- [ ] [P7-T12] Prepare the QA-evidence HEAD-materialization boundary. Write
      `evidence/other/p7-t12-commit.md`, check off this task, and return
      `PROGRESS_COMMIT_REQUIRED: P7-T9..P7-T12` without invoking `git commit`. The orchestrator must
      stage `QuickFiler`, `QuickFiler.Test`, and this feature folder; collect canonical commit context;
      obtain the message from the routed commit-steward profile; create the commit; and record its SHA
      before Phase 8 begins. Acceptance after execution resumes: the checkpoint records the interval
      and a non-empty commit SHA; `git status --porcelain -- QuickFiler QuickFiler.Test` produces no
      output; and
      `git status --porcelain -- docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`
      lists at most this task's own evidence
      artifact and this plan file, with every other feature-folder path already in `HEAD`. Record
      both outputs verbatim. The boundary commit carries the remaining Phase 7 evidence artifacts; the
      formatting result itself is already in `HEAD` through the P7-T2 boundary.

### Phase 8 — Acceptance-criteria reconciliation

Each task below verifies one acceptance criterion against evidence already on disk and then changes
that criterion's `- [ ]` to `- [x]` in the `## Acceptance Criteria` section of `spec.md`. No criterion
is checked off before its cited evidence exists. Exactly one criterion is checked off per task.

- [ ] [P8-T1] AC1: cite `evidence/regression-testing/p3-t5-router-tests-green.md` showing
      `RowSelected_ArchiveRootExactFilingTarget_IsNotSelected` and
      `RowSelected_ArchiveRootExactFilingTarget_PreservesAPriorValidSelection` passing, and
      `evidence/regression-testing/p2-t11-router-tests-red.md` showing both failing before the fix.
      Acceptance: both artifacts exist and name both tests; AC1 is checked off.
- [ ] [P8-T2] AC2: cite `evidence/regression-testing/p3-t5-router-tests-green.md` showing
      `RowSelected_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem` and
      `RowSelected_RootedTargetUnderArchiveRoot_CaseInsensitiveAndTrailingSeparatorRoot_CommitsTheStem`
      passing. Acceptance: the artifact names both tests as passing; AC2 is checked off.
- [ ] [P8-T3] AC3: cite `evidence/regression-testing/p3-t2-nesting.md` for the nesting inside the
      `IsFullOutlookPath` arm, and `evidence/regression-testing/p3-t5-router-tests-green.md` for
      `RowSelected_RelativeFilingTarget_CommitsTheValueVerbatim` passing. Acceptance: both artifacts
      exist and the nesting artifact quotes the edited body; AC3 is checked off.
- [ ] [P8-T4] AC4: cite `evidence/baseline/p1-t6-passthrough-tests.md` for the two existing tests,
      `evidence/regression-testing/p3-t6-router-siblings.md` and
      `evidence/regression-testing/p5-t6-issue439-green.md` for both passing unmodified, and
      `evidence/regression-testing/p3-t5-router-tests-green.md` for the new
      `RowSelected_RootedTargetWithNoBoundArchiveRoot_PassesThroughVerbatim`. Acceptance: all three
      artifacts exist and name the tests; AC4 is checked off.
- [ ] [P8-T5] AC5: cite `evidence/regression-testing/p3-t5-router-tests-green.md` for
      `RowSelected_TrashPseudoRow_CommitsTheSentinelVerbatim`, and
      `evidence/regression-testing/p4-t4-helper-tests-green.md` for
      `ToFilingStemOrVerbatim_TrashSentinel_ReturnsTheInputVerbatim`. Acceptance: both artifacts exist
      and name the tests; the check-off record additionally states that AC5's citation of
      `EfcDataModel.cs:272` for the `folderpath != "Trash to Delete"` comparison is stale — the
      comparison is at line **316** on the merged tree — that the discrepancy is recorded in full by
      P8-T32, and that AC5's binding clause, which is behavioral rather than positional, is
      unaffected; AC5 is checked off.
- [ ] [P8-T6] AC6: cite `evidence/regression-testing/p3-t2-nesting.md` for the preserved message
      literal, `evidence/regression-testing/p3-t5-router-tests-green.md` for
      `RowSelected_OutOfRootRootedTarget_IsStillRejected` and
      `RowSelected_SeparatorBoundaryNearMissTarget_IsStillRejected`, and
      `evidence/regression-testing/p3-t6-router-siblings.md` for
      `RowSelected_OutOfRootFilingTarget_DoesNotStoreAFullOutlookPath`. Acceptance: all three artifacts
      exist and name the tests; AC6 is checked off.
- [ ] [P8-T7] AC7: cite `evidence/regression-testing/p3-t2-nesting.md` recording that the new message
      contains no `@`, and `evidence/regression-testing/p3-t5-router-tests-green.md` for
      `RowSelected_ArchiveRootExactFilingTarget_IsNotSelected`, which asserts through
      `AssertRejectionDiagnosticWithoutIdentifiers`. Acceptance: both artifacts exist; AC7 is checked
      off.
- [ ] [P8-T8] AC8: cite `evidence/regression-testing/p3-t3-selectionfile-diff.md` showing no hunk in
      the original line range 109 to 139. Acceptance: the artifact exists and lists the hunk headers;
      AC8 is checked off.
- [ ] [P8-T9] AC9: re-run both P1-T1 searches against the post-change tree and write
      `evidence/qa-gates/p8-t9-selection-family-post.md`. Acceptance: both searches still return
      exactly 9 lines with the same classification of 2 declarations and 7 call sites, and no Family-B
      member appears in either result; AC9 is checked off.
- [ ] [P8-T10] AC10: cite `evidence/regression-testing/p3-t5-router-tests-green.md` for
      `SelectFirstRow_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem`, which reaches
      `SelectRow` through the public `SelectFirstRow` at `BreadcrumbBridgeRouter.cs:196-203` rather
      than through the `rowSelected` inbound message. Acceptance: the artifact names that test as
      passing; AC10 is checked off.
- [ ] [P8-T11] AC11: cite `evidence/regression-testing/p4-t2-helper-shape.md` for the single
      `internal static` declaration in `QuickFiler/Controllers/EfcDataModel.FilingStem.cs`, the single
      assignment call site at `QuickFiler/Controllers/EfcDataModel.cs:337`, and the purity record, and
      `evidence/regression-testing/p4-t4-helper-tests-green.md` for the eight tests that invoke the
      helper directly without constructing an `EmailFiler`. Also cite
      `evidence/regression-testing/p2-t13-compile-include-observed.md` and
      `evidence/other/p6-t6-commit.md` for the two supporting edits the file split requires: the
      `partial` keyword on `EfcDataModel.cs:21` and the `<Compile Include>` registration in
      `QuickFiler/QuickFiler.csproj`. Acceptance: all four artifacts exist; the check-off record states
      that AC11's text already names the new declaring file and the assignment at line 337,
      re-verified against the merged tree in P8-T32 list entry A3, that the helper remains the member
      `EfcDataModel.ToFilingStemOrVerbatim` because the new file is a partial of the same type, and
      that the split is the remedy `spec.md:414-416` authorizes for the 15-line headroom; AC11 is
      checked off.
- [ ] [P8-T12] AC12: cite `evidence/regression-testing/p4-t4-helper-tests-green.md` for
      `ToFilingStemOrVerbatim_RelativeStem_ReturnsTheInputVerbatim` and
      `ToFilingStemOrVerbatim_TrashSentinel_ReturnsTheInputVerbatim`. Acceptance: the artifact names
      both tests as passing; AC12 is checked off.
- [ ] [P8-T13] AC13: cite `evidence/regression-testing/p4-t4-helper-tests-green.md` for
      `ToFilingStemOrVerbatim_RootedUnderAncestor_ReturnsTheStem` and
      `ToFilingStemOrVerbatim_RootedUnderCaseDifferingAncestor_ReturnsTheStem`, together with
      `evidence/regression-testing/p2-t12-helper-tests-red.md` showing both failing before the fix.
      Acceptance: both artifacts exist and name both tests; AC13 is checked off.
- [ ] [P8-T14] AC14: cite `evidence/regression-testing/p4-t4-helper-tests-green.md` for
      `ToFilingStemOrVerbatim_ArchiveRootExact_ReturnsTheInputVerbatimAndDoesNotThrow`,
      `ToFilingStemOrVerbatim_OutOfRootRootedInput_ReturnsTheInputVerbatimAndDoesNotThrow`,
      `ToFilingStemOrVerbatim_NullEmptyWhitespaceOrSeparatorOnlyAncestor_ReturnsTheInputVerbatim` and
      `ToFilingStemOrVerbatim_NullOrEmptyCandidate_ReturnsTheInputVerbatim`, and
      `evidence/regression-testing/p4-t2-helper-shape.md` for the record that the body contains no
      `throw`. Acceptance: both artifacts exist and name all four tests; AC14 is checked off.
- [ ] [P8-T15] AC15: cite `evidence/baseline/p1-t5-toarchiverelativestem-tests.md` for the count of 8
      and the declaration line numbers, and
      `evidence/regression-testing/p4-t5-toarchiverelativestem-unchanged.md` for the zero-removed-line
      diff and the 8 passing results including `ToArchiveRelativeStem_ArchiveRootItself_Throws`, and
      `evidence/regression-testing/p4-t6-nongoals-untouched.md` for the unmodified `MAPIFolder`
      overload at `EfcDataModel.cs:398-419` and its call to `ToArchiveRelativeStem` at line **407**.
      Acceptance: all three artifacts exist; the check-off record states that AC15 already carries
      `:421-448`, `:398-419` and `:407`, re-verified against the merged tree in P8-T32 list
      entries A5, A6 and A8; AC15 is checked off.
- [ ] [P8-T16] AC16: re-run both P1-T2 searches against the post-change tree and write
      `evidence/qa-gates/p8-t16-movetofolder-family-post.md`. Acceptance: the syntax-anchored search
      still returns exactly **10** lines across **5** files, classified as 3 declarations and 7 call
      sites; the stem search still returns **23** lines across **6** files; no new overload and no
      signature change appears; the check-off record states that AC16 already carries those measured
      figures, re-verified in P8-T32 list entry A9, and that the 16-line figure retained at
      `spec.md:313` describes the pre-#638 tree and is recorded in P8-T32 list entry B6; AC16 is
      checked off.
- [ ] [P8-T17] AC17: cite `evidence/regression-testing/p4-t6-nongoals-untouched.md`. Acceptance: the
      artifact shows no hunk in the ranges 349 to 396 (`OpenOlFolderAsync` and `OpenFsFolderAsync`) or
      398 to 448 (the `MAPIFolder` overload and `ToArchiveRelativeStem`), and no hunk in the protected
      range 271 to 297; it records that `Globals.Ol.ArchiveRootPath` occurs exactly once in the file,
      at line 284; and it records that the guarded read at 284 and the
      `UserDiagnosticAction(ArchiveRootUnavailableMessage)` degrade at 358 and 382, all introduced by
      issue #638, are preserved unchanged. The check-off record states that AC17's current wording,
      already present in
      `spec.md`, is the one this plan can satisfy, and records the reason: its original clause
      required that no `Globals.Ol.ArchiveRootPath` read
      gains a try/catch or a degrade, and issue #638 had already given the file's single read both, so
      that clause was false on the merged tree before this plan ran and no action this plan authorizes
      could make it true. AC17's current clause requires instead that #638's guarded read and degrade
      are preserved unchanged, which is the property this plan can and does deliver; AC17 is checked
      off.
- [ ] [P8-T18] AC18: cite `evidence/regression-testing/p5-t5-single-assertion-change.md`, which
      records all three clauses. Acceptance: the artifact records the corrected assertion at
      line 165, the renamed method, and the narrowed two-line comment; AC18 is checked off.
- [ ] [P8-T19] AC19: cite `evidence/regression-testing/p5-t4-provider-assertion-preserved.md`.
      Acceptance: the artifact quotes lines 161 to 164 byte-identically and shows no diff hunk over
      `ToHierarchyPath` at the original `BreadcrumbBridgeRouter.cs:152-167`; AC19 is checked off.
- [ ] [P8-T20] AC20: cite `evidence/baseline/p1-t8-pinning-assertion.md` for the pre-change derivation
      by two independent constructions, and
      `evidence/regression-testing/p5-t5-single-assertion-change.md` for the post-change diff showing
      exactly one removed `.Should()` line across the whole `QuickFiler.Test` tree. Acceptance: both
      artifacts exist and agree that the count is 1; AC20 is checked off.
- [ ] [P8-T21] AC21: write `evidence/other/p8-t21-spec-correction-record.md` carrying the change
      description text for this correction, copied from the record P5-T5 wrote into
      `evidence/regression-testing/p5-t5-single-assertion-change.md`. This artifact is the designated
      source text for the pull-request change description, so the statement is owned by this plan
      rather than deferred to a step outside it. Acceptance: the artifact states that the issue #439
      criterion that a rooted target survives selection is superseded by issue #614's
      archive-relative-stem invariant, which #614 enforced on the `SelectHierarchyPath` half and at the
      filing boundary but not on the `SelectRow` half, and that the change is a deliberate spec
      correction and explicitly not a weakened test; AC21 is checked off.
- [ ] [P8-T22] AC22: cite `evidence/baseline/p1-t4-deferral-records.md` for the pre-change count of 3
      and `evidence/regression-testing/p6-t4-deferral-cleared.md` for the post-change count of 0 over
      `*.cs`. Acceptance: both artifacts exist and the post-change count is 0; AC22 is checked off.
- [ ] [P8-T23] AC23: cite `evidence/regression-testing/p6-t4-deferral-cleared.md` for the scoped
      `EfcSelectionGuardTests` run with 0 failures including
      `Issue614_GuardAcceptedSelection_DoesNotThrowAtFilingBoundary`, and for the single-line diff over
      `EfcSelectionGuard.cs`. Acceptance: the artifact exists and records both; AC23 is checked off.
- [ ] [P8-T24] AC24: re-run both P1-T3 searches against the post-change tree and write
      `evidence/qa-gates/p8-t24-selectedfolderpath-post.md`. Acceptance: the production surface is
      still 9 lines across 3 files with 2 writes and 3 reads, no new write site appears, no new public
      API member appears, and `rg -n "public string\? SelectedFolderPath \{ get; private set; \}" QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`
      returns exactly 1 line; AC24 is checked off.
- [ ] [P8-T25] AC25: cite `evidence/qa-gates/p7-t9-file-sizes.md` and
      `evidence/baseline/p1-t7-file-line-counts.md`. Acceptance: the artifact shows every
      listed file at or under 500 lines, including the new
      `QuickFiler/Controllers/EfcDataModel.FilingStem.cs`; `BreadcrumbBridgeRouterIssue439Tests.cs` at
      or under 694 and therefore not grown; and the check-off record states that `EfcDataModel.cs` was
      **485** lines before the change, not the 424 the spec's implementation table at `spec.md:401`
      still states, that AC25's parenthetical already reads 485 in `spec.md`, re-verified in
      P8-T32 list entry A1, that the
      resulting headroom of 15 lines is what forced the change-B file split, and that AC25 already
      names the new file; AC25 is checked off. The bound is stated as "at or under" rather
      than "exactly" for the same reason it is in P7-T9: the figure is read after a write-mode
      formatter that can reduce a line count, and AC25 requires only non-growth.
- [ ] [P8-T26] AC26: cite `evidence/regression-testing/p2-t13-compile-include-observed.md` for the
      `Compile Include` line and the 10 observed test results, and
      `evidence/regression-testing/p3-t5-router-tests-green.md` for the same 10 tests executing after
      the fix. Acceptance: both artifacts exist and both record 10 executed tests; AC26 is checked off.
- [ ] [P8-T27] AC27: cite `evidence/qa-gates/p7-t4-msbuild-nullable.md` for the clean nullable build,
      and verify in the same task that
      `rg -n "^#nullable enable" QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` returns
      line 1 and that `evidence/regression-testing/p3-t2-nesting.md` records that `stem` is a
      non-nullable `string` passed to `CommitSelection` without a nullable temporary. Acceptance: all
      three checks hold; AC27 is checked off.
- [ ] [P8-T28] AC28: cite `evidence/qa-gates/p7-t10-toolchain-audit.md`. Acceptance: the artifact
      quotes all four final-QC commands verbatim in order, shows `/t:Rebuild` on both MSBuild lines,
      records `NULLABLE_OPT_IN_PROPERTY: absent` for every one of the four quoted final-QC `Command:`
      lines — do not spell the token in this task's own record — and shows every step recording
      `EXIT_CODE: 0` in the final pass; and the artifact records the two deliberate substitutions
      against AC28's literal command list, each with its justification: `msbuild` is invoked through
      the vswhere-resolved absolute path rather than as a bare `msbuild` PATH entry, with the switch
      list character-for-character identical to AC28's and with the recorded `ON_PATH` or
      `NOT_ON_PATH` observation P7-T10 captured together with the statement of what that outcome
      means: on `NOT_ON_PATH` the artifact must record the vswhere resolution as necessary, and on
      `ON_PATH` it must record the resolved source of the `msbuild` command, state that the vswhere
      resolution is used for determinism rather than necessity because the PATH entry is not pinned
      by this repository, and record whether the two resolved paths name the same binary; and the
      test step is
      `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, whose inner vstest call at
      `Invoke-MSTestWithCoverage.ps1:76` carries `/InIsolation` and
      `/TestCaseFilter:TestCategory!=LiveOutlook` and whose coverage is collected by
      `dotnet-coverage --output-format cobertura` rather than by `/EnableCodeCoverage`, which is the
      repository's standard runner and the local analogue of
      `.github/workflows/_mstest-coverage.yml:83`. The substitutions are recorded, not resolved.
      AC28 is checked off.
- [ ] [P8-T29] AC29: cite `evidence/baseline/p0-t16-coverage-headline.md`,
      `evidence/qa-gates/p7-t6-coverage-headline.md`,
      `evidence/qa-gates/p7-t7-changed-line-coverage.md` and
      `evidence/qa-gates/p7-t8-coverage-delta.md`. Acceptance: the baseline capture is under
      `evidence/baseline/` and the post-change capture under `evidence/qa-gates/`, with no artifact
      written to `evidence/coverage/` or to any path under `artifacts/`; the changed-line uncovered
      intersection is empty;
      every line of the new helper that carries a Cobertura `<line>` node has non-zero hits, with at
      least one such node present, and P7-T7's enumeration of the range's node-free lines is cited,
      judged against the
      `ToFilingStemOrVerbatim` range P7-T7 re-derived against the post-format working tree rather than
      against the pre-format range `evidence/regression-testing/p4-t2-helper-shape.md` recorded, with
      P7-T7's record of whether the two ranges differ cited here; the artifact
      `evidence/qa-gates/p7-t8-coverage-delta.md` records either a post-change line-coverage
      percentage at or above 80 — the binding repository-wide floor in `AGENTS.md` and
      `.agents/skills/csharp/SKILL.md` under the authority resolution P7-T8 states — or an explicit
      `BASELINE BELOW FLOOR` finding with the
      post-change figure at or above the recorded baseline; the changed-line intersection is empty for
      all three production files, including the new
      `QuickFiler/Controllers/EfcDataModel.FilingStem.cs`; the `IsFullOutlookPath` conditional in the
      new helper
      shows both branches taken, per the `condition-coverage` values P7-T7 recorded, or — when P7-T7
      records that the helper's range carries no `branch="True"` node — per the two witness tests
      P7-T7 names; and AC29 is checked off.
- [ ] [P8-T30] AC30: verify no behavior outside changes A through D was altered. Run three commands in
      this task, in this order. First the porcelain companion,
      `git status --porcelain -- QuickFiler QuickFiler.Test UtilitiesCS UtilitiesCS.Test TaskMaster TaskMaster.Test ToDoModel Tags TaskVisualization`.
      Second
      `git diff --name-only 0eda184ca0009bc79ac9b7146897270c17c095fa..HEAD -- QuickFiler QuickFiler.Test`.
      Third
      `git diff --name-only 0eda184ca0009bc79ac9b7146897270c17c095fa..HEAD -- UtilitiesCS UtilitiesCS.Test TaskMaster TaskMaster.Test ToDoModel Tags TaskVisualization`.
      Write `evidence/qa-gates/p8-t30-scope-boundary.md` recording all three outputs verbatim. The
      porcelain companion is required because a name-listing diff enumerates tracked changes only and
      never reports an untracked path, so the two diffs alone cannot fail on a file this plan created
      and left uncommitted. At the point this task runs the division of labour between the two
      mechanisms is fixed and is stated here: P6-T6 committed changes A through D, P7-T2 committed the
      formatting result, and P7-T12 committed the Phase 7 evidence, so both anchored diffs carry the
      enumeration assertion, and the porcelain
      span is expected to be empty because every path it covers is already in `HEAD`. That emptiness
      is itself the assertion and not a null result — an untracked or unstaged file anywhere in those
      nine trees, whether a new test file, a stray source file, or an evidence artifact written
      outside the feature folder, appears in the porcelain output and in neither diff, so any line in
      that output fails this task and must be reported to the orchestrator. The porcelain pathspec
      omits `docs/features/active` deliberately: Phase 8 writes evidence artifacts and edits `spec.md`
      under that path and P8-T33 commits them afterwards, so a porcelain span covering it would be
      non-empty for reasons this gate is not measuring. Acceptance: the porcelain invocation produces
      no output; the second command lists the ten paths enumerated in P6-T6, plus — only when the
      `BASELINE_FORMAT_DRIFT` section of `evidence/baseline/p0-t12-csharpier-check.md` is non-empty —
      the paths in that section that lie under `QuickFiler` or `QuickFiler.Test`, each of which the
      artifact must show as a formatting-only change committed by P7-T2, and no others; when
      `BASELINE_FORMAT_DRIFT` is empty the list is exactly the ten paths; the third command
      produces no output, which is the evidence that `UtilitiesCS`, `TaskMaster`,
      `ToDoModel`, `Tags`, `TaskVisualization`, `UtilitiesCS.Test` and `TaskMaster.Test` contain no
      changed file; and AC30 is checked off.
- [ ] [P8-T31] Verify all thirty criteria are checked off, using two independently constructed
      section-scoped counts. Construction 1, range-scoped: extract the lines of `spec.md` between the
      line matching `^## Acceptance Criteria$` and the line matching `^## Risks & Mitigations$`, and
      count within that slice the lines matching `^- \[x\] AC` and the lines matching `^- \[ \] AC`.
      Construction 2, token-scoped and independent of the range extraction: count over the whole file
      the lines matching `^- \[x\] AC` and the lines matching `^- \[ \] AC`. Construction 2 is
      section-discriminating by token rather than by range, because the five checkboxes outside the
      section at `spec.md:54`, `:55`, `:56`, `:57` and `:86` are not followed by `AC`. Write
      `evidence/qa-gates/p8-t31-ac-reconciliation.md`. Acceptance: both constructions report 30 checked
      and 0 unchecked; both agree; and the artifact records that an unscoped count of every `- [x]` and
      `- [ ]` line in `spec.md` would over-report by exactly 5, naming those five line numbers.

**Progress-commit boundary after task 105.** Stop after checking off `[P8-T31]` and return
`PROGRESS_COMMIT_REQUIRED: P7-T9..P8-T31`. Do not begin `[P8-T32]` until the orchestrator has used
canonical commit context and the routed commit-steward profile, then recorded the boundary SHA and the
P7-T12 intermediate SHA within this task interval in the canonical checkpoint.

- [ ] [P8-T32] Record the spec-versus-tree reconciliation in
      `evidence/other/p8-t32-spec-tree-discrepancies.md`. `spec.md` was authored against the tree
      before issue #638 merged, and a prior revision of `spec.md` already applied the acceptance-
      criteria corrections that shift required. This plan performs no `spec.md` text edit: its only
      write to that file is the `- [ ]` to `- [x]` flip in P8-T1 through P8-T30. This task therefore
      records two lists — criteria whose citations were already corrected before execution, verified
      as still matching the tree, and citations that remain stale and are deliberately left uncorrected.
      Acceptance: the artifact carries both lists below, and for every entry records the figure
      `spec.md` carries, the figure measured on the merged tree, whether the two agree, and whether any
      acceptance criterion's binding clause is affected.

      **List A — already corrected in `spec.md`; re-verified against the merged tree, all agree.**
      Each entry is recorded as a verification, not as a correction this plan makes.
      A1. AC25 (`spec.md:976-983`) states `EfcDataModel.cs` at **485** lines with **15** lines of
          headroom; the tree is 485. This is what forced the change-B file split.
      A2. AC16 (`spec.md:926`) cites the `string` overload declaration at `EfcDataModel.cs:303`;
          the tree is 303.
      A3. AC11 (`spec.md:900-906`) names `QuickFiler/Controllers/EfcDataModel.FilingStem.cs` at
          `spec.md:900` and cites the `DestinationOlStem` assignment at `EfcDataModel.cs:337` at
          `spec.md:903-904`; the tree is 337.
      A4. AC17 (`spec.md:933`) cites `OpenOlFolderAsync` at `:349-372` and `OpenFsFolderAsync` at
          `:374-396`; the tree is 349-372 and 374-396.
      A5. AC15 and AC16 (`spec.md:918-920`, `:926`) cite the `MAPIFolder` overload at `:398-419` with
          its declaration at `:398`; the tree is 398-419 and 398.
      A6. AC15 (`spec.md:920`) cites the `ToArchiveRelativeStem` call inside that overload at `:407`;
          the tree is 407.
      A7. AC16 (`spec.md:928`) cites the `MoveToFolderAsync` delegation call at `:408`; the tree is
          408 and the call spans 408-414.
      A8. AC15 (`spec.md:918`) cites `ToArchiveRelativeStem` at `:421-448` with its declaration at
          `:434`; the tree is 421-448 and 434.
      A9. AC16 (`spec.md:925-932`) states 3 declarations and 7 call sites, a family-stem search of
          **23** lines across **6** files, and a syntax-anchored search of **10** lines across **5**
          files; P1-T2 and P8-T16 measure exactly those figures.
      A10. AC17 (`spec.md:933-937`) is already worded to require that issue #638's guarded read at
          `EfcDataModel.cs:284` and its `UserDiagnosticAction(ArchiveRootUnavailableMessage)` degrade
          at `:358` and `:382` are preserved unchanged. The clause a prior `spec.md` revision replaced
          — that no `Globals.Ol.ArchiveRootPath` read gains a try/catch or a degrade — was false on
          the merged tree before this plan ran, because #638 had already given the file's single read
          both, so no action this plan authorizes could have made it true. The artifact records this
          as the reason the current wording is the one that can be satisfied.

      **List B — still stale in `spec.md`; deliberately not corrected, because each remains
      satisfiable as written or lies outside the acceptance criteria.**
      B1. AC23 (`spec.md:967`) and the prose at `spec.md:783` cite
          `Issue614_GuardAcceptedSelection_DoesNotThrowAtFilingBoundary` at `:167-213`; the span is
          `:167-214`. Binding clause unaffected.
      B2. `spec.md:376` cites the `#499` clear-on-rebind block at `BreadcrumbBridgeRouter.cs:143-146`;
          the block spans `:143-147`. The write at `:145` and the read at `:143` that AC24 names are
          exact, so AC24's binding clause is unaffected.
      B3. AC5 (`spec.md:874`) and the prose at `spec.md:127`, `:360`, `:635` and `:1021` cite the
          `folderpath != "Trash to Delete"` comparison at `EfcDataModel.cs:272`; it is at **316**.
          AC5's binding clause is behavioral and is unaffected.
      B4. The implementation table at `spec.md:401`, the headroom sentence at `spec.md:414-416`, the
          constraint sentence at `spec.md:582` and the census line at `spec.md:710` all retain the
          **424**-line figure and its derived **76**-line headroom; the tree is 485 with 15. No
          acceptance criterion depends on these four sites, and the file split they authorize is taken
          on the measured figure, so they are recorded rather than corrected.
      B5. The prose at `spec.md:119-120`, `:122`, `:284`, `:445`, `:469` and `:514` retains the
          pre-#638 citations `:259-265` for the `string` overload declaration and `:287` for the
          `DestinationOlStem` assignment; the tree gives `:303-309` for that declaration and `:337`
          for that assignment. Recorded, not corrected.
      B6. `spec.md:313` states the `MoveToFolder` family as 16 lines across 5 files; the merged tree
          gives 23 stem lines across 6 files. This sentence sits in "Corrections to the research file"
          and describes the pre-#638 tree. Recorded, not corrected.
      B7. `spec.md:164-172` describes the whole `Globals.Ol.ArchiveRootPath` benign-degrade item as an
          open non-goal owned by issue #695 and cites the two verbatim `DestinationOlStem` assignments
          as `:308` and `:326`. The `EfcDataModel` half of that item shipped in issue #638 and is no
          longer pending; the two assignments are at **364** and **388** and do remain verbatim, so
          that half of the statement still holds. Prose outside the acceptance criteria; no binding
          clause depends on it.

      The acceptance-criteria count in `spec.md` is unchanged at 30: this plan adds, removes and
      splits no criterion, and edits no criterion's text.
- [ ] [P8-T33] Finalise the last partial interval without invoking `git commit`. Write
      `evidence/other/p8-t33-final-commit.md`, check off this task, verify that
      `git status --porcelain -- QuickFiler QuickFiler.Test` produces no output, record
      `git status --porcelain -- docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`
      verbatim, and return `PROGRESS_COMMIT_REQUIRED: P8-T32..P8-T33`. Before review, the orchestrator
      must stage this feature folder, collect canonical commit context, obtain the message from the
      routed commit-steward profile, create the commit, record its SHA for the final partial interval,
      and verify both status spans are empty. Before that boundary commit, the feature-folder status
      may list only this task's evidence artifact and this plan file, with every other feature-folder
      path already in `HEAD`. The pathspec scoping is required because `.claude/` is tracked and
      carries unrelated in-flight modifications that this plan must not commit, and because sibling
      feature folders under `docs/features/active` are owned by other work. This task runs long after
      planning, and the tracking state of a sibling folder under that parent directory is
      worktree-local and unobserved by this plan, so a concurrent run in this
      checkout can leave an untracked or modified sibling folder under that parent directory
      before this task executes; a `git add` over the parent directory would then commit another feature's
      work onto this branch, and a `git status --porcelain` span over the parent directory would report
      that folder and make this gate unsatisfiable. Both spans are therefore scoped so that this gate
      cannot depend on state this plan does not own. The feature-folder
      span is stated separately because this task necessarily writes its own artifact after the
      commit and checks off its own box in this plan file, both of which live under this feature's
      folder.
