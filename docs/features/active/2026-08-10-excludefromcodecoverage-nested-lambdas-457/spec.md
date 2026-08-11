# 2026-08-10-excludefromcodecoverage-nested-lambdas-457 (Spec)

- **Issue:** #457
- **Parent (optional):** epic `build-ci-coverage-gate-fidelity` (wave 1)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-10T14-30
- **Status:** Approved for planning
- **Version:** 1.0

> **Work mode `full-bug`.** Per `.claude/skills/acceptance-criteria-tracking/SKILL.md`, this file is the
> sole authoritative acceptance-criteria source for this feature. A `user-story.md` exists in this folder
> because the epic preparation deliverables list names it; it carries no acceptance criteria and must not
> be treated as an AC source.

## Context

- **Summary.** A method-level `[ExcludeFromCodeCoverage]` attribute does not suppress instrumentation of
  lambdas declared inside the attributed member. The C# compiler hoists those lambdas into a separate
  compiler-generated closure type (`<>c`, `<>c__DisplayClass<N>_<M>`) whose members do not inherit the
  attribute. The collector therefore omits the attributed member's own `<method>` element but still emits
  the lambda bodies under the closure type, leaving their source lines in the coverage denominator.
- **Impact mechanism.** When a member is exempt precisely because it cannot execute in a unit-test host
  (the repository's "thin exempt production forwarder" seam pattern), its nested lambda bodies are
  permanently uncovered and permanently counted. The result is an invisible, unreachable per-file coverage
  ceiling. This is a silent measurement defect: nothing fails, nothing crashes, and the figure is simply
  wrong in a direction that cannot be closed by writing tests.
- **Observed environments.** Windows 11 Pro 10.0.26200; .NET Framework 4.8.1 targets; coverage produced by
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (`dotnet-coverage collect --output-format cobertura`) with
  instrumentation settings from `coverage.config`; post-processing by `ConvertTo-KoverageCoberturaXml` in
  `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`.
- **Who is affected, and how often.** Every file that mixes `[ExcludeFromCodeCoverage]` members with
  testable members and declares lambdas inside the exempt members. The repository currently carries 263
  `[ExcludeFromCodeCoverage]` occurrences across 110 `.cs` files. `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`
  measures 90.7% line coverage and cannot exceed approximately 91.5% ((258 - 22) / 258) regardless of test
  effort. Epic #136 requires every testable file to reach the repository line-coverage floor and several of
  its children plan to adopt this same seam pattern; each would inherit an unannounced ceiling.
- **Severity.** Medium. No runtime defect; a measurement-fidelity defect that invalidates gate evidence.
- **First observed.** Recorded in issue #457 (potential entry dated 2026-08-07). The defect is present in
  committed coverage artifacts, so it predates that date.

## Repro & Evidence

**Steps to reproduce**

1. Take a member carrying `[ExcludeFromCodeCoverage]` that declares one or more lambdas in its body.
2. Run `scripts/vscode/Invoke-MSTestWithCoverage.ps1`.
3. Inspect the Cobertura report for that file's `<line>` entries.
4. Observe that the lambda bodies' source lines are present with `hits="0"` under a
   `<class name="…&lt;&gt;c__DisplayClass…">` sibling element, while the attributed member's own lines are
   correctly absent.

**Expected vs actual.** Expected: a lambda declared inside a member carrying `[ExcludeFromCodeCoverage]`
leaves the coverage denominator exactly as the attributed member's own lines do. Actual: the lambda bodies
are emitted under the compiler-generated closure type and counted in the denominator.

**Determinism.** Always reproducible. The behavior is a deterministic property of Roslyn lambda lowering
plus the collector's attribute-exclusion semantics; it is not timing- or data-dependent.

**Evidence already recorded (from committed artifacts, no live run performed during research).**

The primary evidence artifact is the raw (pre-post-processing, absolute `filename` values, closure classes
still present as siblings) report at
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/coverage-baseline.cobertura.xml`.
It contains 873 occurrences of the escaped token `&lt;&gt;c`.

Representative emitted shape, abridged only by truncating the absolute path prefix:

```xml
<class line-rate="0" branch-rate="1" complexity="1"
       name="QuickFiler.Viewers.BreadcrumbPopupUiOperations.&lt;&gt;c__DisplayClass41_0"
       filename="…\QuickFiler\Viewers\BreadcrumbPopupUiOperations.cs">
  <methods>
    <method line-rate="0" branch-rate="1" complexity="1" name="&lt;BeginProductionNavigation&gt;b__0" signature="()">
      <lines><line number="406" hits="0" branch="False" /></lines>
    </method>
    <method line-rate="0" branch-rate="1" complexity="1" name="&lt;BeginProductionNavigation&gt;b__1" signature="()">
      <lines><line number="409" hits="0" branch="False" /></lines>
    </method>
  </methods>
  <lines>
    <line number="406" hits="0" branch="False" />
    <line number="409" hits="0" branch="False" />
  </lines>
</class>
```

Measured defect inventory for `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` (line numbers below are
Cobertura `<line number>` data values from the artifact, not code locators):

| Closure class | Declaring member (attributed) | Cobertura line numbers | Hits |
| --- | --- | --- | --- |
| `<>c__DisplayClass41_0` | `BeginProductionNavigation` | 406, 409 | 0 |
| `<>c__DisplayClass42_0` | `DisposeProductionSurface` | 415, 416 | 1 |
| `<>c__DisplayClass46_0` | `BindProductionNavigation` | 471, 472, 474, 480-484, 490 | 0 |
| `<>c__DisplayClass46_1` | `BindProductionNavigation` | 473, 475-480, 485-489 | 0 |

Distinct uncovered lines across `41_0`, `46_0` and `46_1` total **22**, which reproduces the issue's
`(258 - 22) / 258` ceiling exactly.

A second, independent reproduction: `TaskVisualization/FlagTasks.cs` emits
`<class name="TaskVisualization.FlagTasks.&lt;&gt;c">` carrying `&lt;InitializeToDoList&gt;b__13_0`, and no
`<class name="TaskVisualization.FlagTasks">` element exists for that file at all, because every member of
the type is attributed.

## Scope & Non-Goals

**In scope**

- Excluding compiler-generated closure-type lines whose declaring member carries `[ExcludeFromCodeCoverage]`
  from the coverage denominator, implemented as a post-processing filter in the PowerShell coverage pipeline.
- Regression coverage proving both required directions: a lambda inside an exempt member is excluded, and a
  lambda inside a non-exempt member is still counted.
- A re-captured repository coverage baseline measured against the post-#441 arithmetic, recorded numerically
  under `<FEATURE>/evidence/baseline/` and `<FEATURE>/evidence/qa-gates/`.
- Recording the documented residuals (below) as named, scoped known limitations with follow-up issue handoffs.

**Out of scope / non-goals**

- **Re-tuning any coverage threshold.** Threshold reconciliation is owned by issue #494, which runs after
  this feature (epic wave 2). A corrected figure that would fail an existing threshold is recorded in
  evidence and handed to #494. This feature must not lower, raise, or otherwise adjust a threshold to
  accommodate a number that moved.
- **Editing `CLAUDE.md` or anything under `.claude/rules/`.** Those edits belong to sibling features #512
  and #494.
- **Any C# source change.** No `[ExcludeFromCodeCoverage]` attribute is added, moved, or removed by this
  feature.
- **Any change to `coverage.config` or `TaskMaster.runsettings`.** See Root Cause Analysis for why the
  settings surface cannot deliver the fix.
- **Local-function exclusion** (`<Member>g__Local|N_M`) and **exempt-async-member lambda exclusion**. Both
  are documented residuals handed to follow-up issues; see Risks & Mitigations.

**Explicitly excluded systems.** The `/p:Nullable=enable` type-check command documented in `CLAUDE.md` is a
known defect (issue #522) producing roughly 200-414 spurious errors against a clean `main`. It is not a
blocking gate for this feature. This feature's toolchain is the PowerShell toolchain
(`run_poshqc_format` → `run_poshqc_analyze` → `run_poshqc_test`) per `.claude/rules/powershell.md`.

## Root Cause Analysis

**Confirmed root cause.** Two independent facts compose into the defect:

1. **Roslyn lowering.** A lambda declared in a member body is hoisted into a compiler-generated closure type
   nested in the declaring type (`<>c` for non-capturing cached lambdas, `<>c__DisplayClass<N>_<M>` for
   capturing lambdas). `[ExcludeFromCodeCoverage]` applied to the declaring member is not propagated by the
   compiler to the synthesized closure type or its members.
2. **Collector attribute exclusion is member-scoped.** Neither `coverage.config` nor `TaskMaster.runsettings`
   declares an `<Attributes>` block (both carry only `<ModulePaths><Exclude>`), so the collector's documented
   defaults apply, including `^System\.Diagnostics\.CodeAnalysis\.ExcludeFromCodeCoverageAttribute$`. The
   collector therefore suppresses the attributed member and nothing else. The closure type is a distinct CLR
   type carrying no such attribute, so it is instrumented normally.

**Supporting signals.**

- The collector genuinely honors member-level `[ExcludeFromCodeCoverage]`: an attributed member emits **no
  `<method>` element at all**, not a zero-hit one. In `BreadcrumbPopupUiOperations.cs`, the attributed members
  `ShowOwnedPopup`, `CreateProductionControl`, `BeginProductionInitialization`, `ReadProductionCore`,
  `BeginProductionNavigation`, `DisposeProductionSurface` and `BindProductionNavigation` each return **no
  match** when searched for as `name="<member>"` in the raw artifact.
- The closure type survives with the **same `filename`** as its declaring type and with `<method name>`
  values that embed the declaring member's name (`&lt;Member&gt;b__N`).

**This is the mechanism that makes the fix possible.** Absence of a plain `<method>` element is the report's
own record that the member was excluded at instrumentation time, and the declaring member's identity is
recoverable from the synthesized method name. Both inputs the fix needs are therefore present in the
Cobertura XML alone, with no assembly metadata read and no C# source parsing.

**Affected components.**

- `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` — `ConvertTo-KoverageCoberturaXml` (the
  post-processing pipeline), `Merge-CoberturaClassesByFilename`, `Get-CoberturaCoverageSummary`.
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — `Invoke-MSTestWithCoverageMain` dot-sources the helpers
  module and drives collection; no change is required there.

## Proposed Fix

### Design summary (what changes where)

Adopt **Candidate 1c**: a post-processing filter in the PowerShell pipeline that removes compiler-generated
closure-type coverage whose declaring member is **absent from the instrumented method set** of the same
declaring type and filename. The filter is a pure XML-to-XML transform over a document the pipeline already
parses.

The rule, stated precisely:

- A `<class>` whose name contains the `.<>c` marker is a **closure class**.
- For each `<method>` on a closure class, derive the **declaring member token** from the synthesized name
  (`^<(?<m>[^<>]+)>b__`, `^<(?<m>[^<>]+)>g__`, the last `<(?<m>[^<>]+)>d__\d+` segment of a class name, and
  the inner token of `<<(?<m>[^<>]+)>b__\d+>d`).
- Build a **presence set** per `(declaringType, filename)` pair. A member is present if either
  (1) a `<method name="X">` appears on a class whose name contains no `.<`, where `X` does not begin with
  `<`; or (2) a class named `Type.<Member>d__<N>` exists (async/iterator state machine).
- Drop closure methods whose declaring member is **not** in the presence set. Retain methods whose declaring
  member could not be derived (fail-safe: an unrecognized name shape yields no derived member and the method
  is kept).

#### The async correction (load-bearing)

An `async` or iterator member also emits **no plain `<method>` element**, because its whole body moves to a
state machine class `Type.<Member>d__<N>` whose only method is `MoveNext`. Verified in the raw artifact:
`QuickFiler.Viewers.BreadcrumbPopupUiOperations.<CreateAndInstallSurfaceAsync>d__33`, `…<IgnoreFailureAsync>d__35`,
`…<ObserveExternalAsync>d__34`, `…<RetryAsync>d__36`, `TaskMaster.AppOlObjects.<LoadAsync>d__110`,
`…<LoadStoresAsync>d__117`.

**Therefore rule (2) of the presence set — admitting `d__` state-machine class names — is mandatory, not
optional.** Without it, a naive "declaring member has no `<method>` element implies exempt" rule wrongly
deletes lambdas inside non-exempt async members and the second required direction fails. The live
counter-example verified in the artifact is `BreadcrumbPopupUiOperations.<>c__DisplayClass33_1` and
`…<>c__DisplayClass33_2`, which are **covered** (`line-rate="1"`) lambdas declared inside the **non-exempt
async** member `CreateAndInstallSurfaceAsync`.

#### Justification of the selected surface against the alternatives

| Candidate | Verdict | Reason |
| --- | --- | --- |
| **1a — blanket exclusion of every `<>c` / `<>c__DisplayClass` class** | **Disqualified** | Deletes lambdas declared inside non-exempt members, failing the required second direction. Direct counter-example from the raw artifact: `BreadcrumbPopupUiOperations.<>c__DisplayClass25_0` (`<BeginNavigationAsync>b__0`, `line-rate="1"`) belongs to the non-exempt member `BeginNavigationAsync`, which is present in the report as `<method name="BeginNavigationAsync">`. |
| **1b — key on `CompilerGeneratedAttribute` recorded in the Cobertura output** | **Disqualified, not implementable** | Cobertura carries no attribute metadata. The only attributes on `<class>` are `line-rate`, `branch-rate`, `complexity`, `name`, `filename`. Recovering attribute data would require a companion assembly-metadata read (Mono.Cecil or `System.Reflection.Metadata`), which is .NET work rather than PowerShell work, and would bind the post-processor to build outputs that may not exist when a report is consumed. |
| **1c — key on the synthesized method name; infer exemption from the declaring member's absence** | **Selected** | The only variant that satisfies both directions from the Cobertura XML alone. |
| **1c-source — read the `.cs` file and look for `[ExcludeFromCodeCoverage]` above the enclosing member** | **Rejected sub-variant** | Requires C# attribute-list, comment and expression-body parsing in PowerShell; makes the post-processor fail when a report is processed away from its source tree; forces a committed `.cs` fixture into the test tree. Variant 1c needs none of this. |
| **2 — instrumentation-time exclusion via `coverage.config` / dotnet-coverage settings** | **Disqualified** | The only settings-level lever that reaches closure types is `<Attribute>^System\.Runtime\.CompilerServices\.CompilerGeneratedAttribute$</Attribute>`, and Microsoft documents the consequence: excluding `CompilerGeneratedAttribute` excludes all code using `async`, `await`, `yield return` and auto-implemented properties. That removes every lambda, async state machine and iterator repository-wide, including those in non-exempt members — failing direction 2. The `<Functions><Exclude>` lever is correct in both directions in principle but requires a hand-maintained enumeration of every attributed member (263 occurrences across 110 files today) duplicated in a config file with no compiler or test keeping it in sync, and it cannot be regression-tested under Pester without executing the full C# suite, so it cannot satisfy the "deterministic Pester regression tests, no temporary files" criterion. Disqualified on direction 2, on maintenance cost, and on testability. |
| **3 — type-level `[ExcludeFromCodeCoverage]` as a source convention** | **Disqualified by construction** | Moving the attribute from member to type does suppress the nested closure types, but it removes the entire type from the denominator. For `BreadcrumbPopupUiOperations` that drops roughly 234 covered lines along with the 22 uncovered ones, deleting exactly the testable seam the pattern exists to create, and it makes lambdas in non-exempt members of the same type disappear from the denominator — failing direction 2. Blast radius: 263 attribute occurrences across 110 files; any file mixing exempt and testable members (the common case, and the point of the seam pattern) cannot adopt it. |

Positive justification for 1c, summarized: it is correct in both directions; it is deterministic (no clock,
no filesystem, no process, no network — identical input yields identical output); it is testable under Pester
with inline here-string fixtures and no temporary files; its blast radius is one new production PowerShell
file plus one dot-source line and one call site in an existing one, with zero C# files, zero configuration
files and zero threshold changes; and the rule is derived from each report at run time, so nothing must be
kept in sync by hand.

### Boundaries and invariants to preserve

- **Hard ordering constraint.** `Remove-CoberturaExemptClosureCoverage` MUST run **before**
  `Merge-CoberturaClassesByFilename`, inside `ConvertTo-KoverageCoberturaXml`, immediately after the
  `//class[@filename]` path-normalization loop. `Merge-CoberturaClassesByFilename` groups `<class>` elements
  by `filename`, selects as primary the first member whose `name` does not match `<`, unions the group's
  class-level `<lines>`, and keeps only the primary's `<methods>`. A closure type always shares its declaring
  type's `filename`, so the merge always collapses it and the surviving node no longer carries the
  `<Member>b__…` method names the filter depends on. **This is a constraint, not a preference: running the
  filter after the merge does not degrade the result, it makes the result unobtainable, because the
  information the filter reads no longer exists in the document.**
- The resulting order inside `ConvertTo-KoverageCoberturaXml` is: remove non-allowlisted `<package>` →
  normalize `//class[@filename]` → **`Remove-CoberturaExemptClosureCoverage` (new)** →
  `Merge-CoberturaClassesByFilename` → inject `<sources>` → `Get-CoberturaCoverageSummary` and write the
  document-level rate attributes.
- `Get-CoberturaCoverageSummary` is invoked twice downstream — once per merged class inside the merge, once
  for the document total — and both consume the already-filtered tree, so neither call site changes.
- **Fail-safe direction.** Every failure mode of the presence-set key is in the under-exclusion direction: a
  file measures no better than it truly is. Over-exclusion (deleting coverage that should count) is not an
  acceptable failure mode and no rule may be added that permits it.
- **No behavior change for non-closure classes.** The filter must not mutate a `<class>` whose name contains
  no `.<>c` marker.
- The filter must be idempotent: applying it twice to the same document yields no further change.

### Dependencies or blocked work

This feature depends on issue #441 (epic wave 0, feature folder `cobertura-coverage-arithmetic-441`). See
Assumptions, Constraints, Dependencies.

### Implementation strategy (what changes, not sequencing)

All locators below are **function/symbol anchors**. Absolute line numbers are deliberately not used, because
#441 will shift every line in `Invoke-MSTestWithCoverage.Helpers.ps1`.

#### Files/modules to change

**New:** `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`

Rationale for a new file rather than growing the existing helpers module: `Invoke-MSTestWithCoverage.Helpers.ps1`
is 357 lines today, #441 will add to it, and the repository ceiling in `.claude/rules/general-code-change.md`
and `.claude/rules/powershell.md` is 500 lines. Roughly 110 further lines would leave insufficient headroom.
Two production PowerShell files is within the direct-mode change budget in `.claude/rules/powershell.md`.

**Modified:** `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` — exactly two edits:

1. a dot-source line near the top:
   `. (Join-Path $PSScriptRoot 'Invoke-MSTestWithCoverage.ClosureFilter.ps1')` (`$PSScriptRoot` resolves to
   the containing script's own directory even when the file is itself dot-sourced, which is how
   `Invoke-MSTestWithCoverageMain` loads it);
2. a single `Remove-CoberturaExemptClosureCoverage -XmlDocument $xml` call inside
   `ConvertTo-KoverageCoberturaXml` at the position fixed by the ordering constraint above.

No change to `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, `coverage.config`, `TaskMaster.runsettings`,
`scripts/vscode/TaskMaster.cli.runsettings`, or any C# file.

#### Functions/classes/CLI commands impacted

New functions in `Invoke-MSTestWithCoverage.ClosureFilter.ps1`, all advanced functions with `[CmdletBinding()]`
per `.claude/rules/powershell.md`:

- `Get-CoberturaClosureDeclaringMemberName` — pure. Given a synthesized class name or method name, returns the
  declaring member token or `$null`.
- `Test-CoberturaClosureClassName` — pure. `$true` when the class name contains the `.<>c` marker (covering
  `<>c`, `<>c__DisplayClass<N>_<M>`, generic suffixes, and nested `<>c….<<Member>b__K>d`). Deliberately
  `$false` for `Type.<Member>d__<N>` state machines.
- `Get-CoberturaDeclaringTypeName` — pure. Class name truncated at the first `.<`.
- `Get-CoberturaInstrumentedMemberName` — builds the presence set for one `<package>`, keyed by
  `"$declaringType|$filename"`, admitting members from exactly the two sources listed in the design summary.
  `<Member>g__Local|N_M` methods are deliberately **not** admitted, so they cannot mask an otherwise-absent
  declaring member.
- `Remove-CoberturaExemptClosureCoverage -XmlDocument [xml]` — the orchestrating function.

Existing functions reused, not modified: `Get-CoberturaLineConditionCoverageParts` and
`Get-CoberturaCoverageSummary` (for rate recomputation on a scratch document, exactly as
`Merge-CoberturaClassesByFilename` already does).

Existing function modified: `ConvertTo-KoverageCoberturaXml` (one added call).

#### Data flow and validation changes

For each `<package>`: build the presence set; for each closure class, derive each `<method>`'s declaring
member (falling back to a class-name-derived token when the method name yields none, for example `MoveNext`
on a nested async-lambda state machine); drop methods whose declaring member is absent from the presence set
for that `(declaringType, filename)` key; keep methods whose declaring member could not be derived. Then:

- if no method was dropped, leave the class untouched;
- otherwise rebuild `./lines` as the de-duplicated union of the retained methods' `./lines/line` (max `hits`,
  richest `condition-coverage`) and recompute `line-rate` / `branch-rate`;
- if zero methods are retained, remove the `<class>` element from its `<classes>` parent.

Removing a closure class removes its lines from **both** the numerator and the denominator; see the baseline
caveat in Assumptions.

#### Error handling and logging updates

`throw` on a malformed document only where the existing pipeline already does (a missing `<packages>` node is
already handled by `ConvertTo-KoverageCoberturaXml` and `Get-CoberturaCoverageSummary`). The filter itself
must not throw on an unrecognized name shape: it returns `$null` for the derived member and retains the
method, per the fail-safe invariant. No new console output; no `Write-Host`.

#### Rollback/feature-flag considerations

Not applicable. The change is confined to a post-processing transform of a generated artifact. Rollback is
reverting the two-file change; no persisted state, no schema, no migration.

### Technical specifications (interfaces/contracts)

- **Inputs/outputs.** `Remove-CoberturaExemptClosureCoverage` accepts a mandatory `[xml]$XmlDocument` and
  mutates it in place, matching the existing signature and mutation convention of
  `Merge-CoberturaClassesByFilename`. It returns nothing. The helper functions accept and return strings or
  hashtable-shaped presence sets and perform no I/O.
- **Required configuration keys and defaults.** None. No new configuration key, no new command-line switch,
  no new environment variable. The filter is unconditional.
- **Backward compatibility.** The output remains valid Cobertura. The document-level `line-rate`,
  `branch-rate`, `lines-covered`, `lines-valid`, `branches-covered` and `branches-valid` attributes continue
  to be written by the existing summary step. Consumers that recompute rates from `<lines>` and consumers
  that read the `line-rate` attribute both see a consistent document, because per-class rates are recomputed
  whenever a class is modified. Any file whose every class is removed disappears from the report entirely;
  that is the correct semantic for a wholly exempt file (`TaskVisualization/FlagTasks.cs` is the verified
  example) but is a visible change for any downstream consumer that enumerates files.
- **Performance constraints.** The filter adds one additional pass over the already-parsed DOM before the
  merge pass. The committed sample report carries on the order of 10^5 `<line>` elements; a single additional
  linear pass is not expected to be material against the cost of the C# test run that produces it. No
  explicit latency budget is set; the plan should record observed wall-clock post-processing time before and
  after in evidence.

## Assumptions, Constraints, Dependencies

**Dependency on issue #441 (explicit, blocking).**

Issue #441 is epic wave 0 (feature folder `cobertura-coverage-arithmetic-441`) and executes before this
feature. It replaces the `.//lines/line` descendant axis with the child axis `./lines/line` in
`Get-CoberturaCoverageSummary` and `Merge-CoberturaClassesByFilename`, and corrects the blended
union/primary-methods denominator in `Merge-CoberturaClassesByFilename`. After it lands, each source line
appears exactly once in the denominator, and per-file `line-rate` equals the rate computed from the merged
class-level `<lines>` set alone (distinct line numbers, max hits per number).

**This spec is written against that post-#441 contract**, not against the current double-counted behavior.
Consequences:

- Every locator in this spec is a function/symbol anchor, never an absolute line number, because #441 will
  have shifted them.
- The insertion point fixed by the ordering constraint remains valid regardless of #441's exact
  implementation, because it is upstream of both functions #441 modifies.
- If #441 has not landed when implementation begins, the filter still functions, but any numeric baseline
  captured before #441 reflects the pre-#441 double count and will not match post-#441 figures. **Sequencing
  must be honored before any baseline is captured.**

**Stated assumption, not verified at authoring time:** #441's own prepared plan was not available on the epic
integration branch when this spec was written (`docs/features/active/*441*` returned no match in this
worktree). The post-#441 contract above is taken as supplied by the epic charter and the issue transcript. If
#441's landed behavior differs, the ordering constraint still holds but every recorded number must be
recaptured.

**Other assumptions**

- **Collector default attribute excludes are in force.** Neither `coverage.config` nor `TaskMaster.runsettings`
  declares an `<Attributes>` block, so the collector's four documented defaults apply, including
  `^System\.Diagnostics\.CodeAnalysis\.ExcludeFromCodeCoverageAttribute$`. If an `<Attributes>` block is ever
  added to either file, the four defaults must be re-listed or they are lost, and this feature's exemption
  signal would be invalidated.
- **One `<class>` element per (CLR type, source file) pair.** Inferred from four separate
  `<class name="TaskMaster.AppOlObjects" …>` elements with four different partial-class filenames in the raw
  artifact. It matches every case examined but was not confirmed against collector source. If a future
  collector emitted one class element per type spanning multiple partial files, the filename component of the
  presence-set key would produce under-exclusion, not over-exclusion.
- **Roslyn name shapes.** `<>c`, `<>c__DisplayClass<N>_<M>`, `d__<N>` and `g__` are Roslyn implementation
  details — stable in practice across every C# compiler in use here, but not contractual. The filter fails
  safe on an unrecognized shape.
- **No live coverage run was performed during research.** All collector-behavior claims are read from
  committed artifacts.

**Measured baseline caveat (constraint on every numeric expectation)**

Research confirmed the issue's 22-line figure reproduces exactly (22 distinct uncovered lines across
`<>c__DisplayClass41_0`, `46_0` and `46_1`). However, `<>c__DisplayClass42_0` contributes **2 covered lines**
from the exempt member `DisposeProductionSurface`. A correct fix removes those from **both the numerator and
the denominator**, so the corrected rate is **not** `covered / (valid - 22)`.

**Every numeric expectation recorded in evidence must be measured against an actual post-fix run, never
derived arithmetically from the pre-fix figure.** A plan task that predicts a corrected percentage without
measuring it is non-compliant with this spec.

**Constraints**

- PowerShell 7+; advanced functions with `[CmdletBinding()]`; approved verbs; files under 500 lines
  (`.claude/rules/powershell.md`).
- Change budget: at most 2 production PowerShell files in direct mode. This feature uses exactly 2.
- Toolchain: `run_poshqc_format` → `run_poshqc_analyze` → `run_poshqc_test`, restarting from step 1 on any
  failure or file change. Type checking is not applicable to PowerShell.
- No temporary files anywhere, in production code or tests.
- No threshold change (owned by #494); no `CLAUDE.md` or `.claude/rules/**` edit (owned by #512 and #494).

## Data / API / Config Impact

- **User-facing or API changes.** None. No new script parameter, no new switch, no changed exit code, no
  changed console output. The only observable change is the content of the generated Cobertura report.
- **Data / migration considerations.** Coverage reports are regenerated artifacts, not persisted data; there
  is nothing to migrate. Previously committed coverage evidence in other feature folders will not reproduce
  against the corrected pipeline. That is expected and is already flagged as a coordination note in the epic
  charter (twenty-one unmerged branches from epic #136 gate on per-file line rates computed by the
  pre-correction code). This feature does not re-baseline those branches.
- **Logging/telemetry.** No change.
- **Compatibility notes.** Output remains schema-valid Cobertura. Files whose every class is removed
  disappear from the report; downstream consumers that enumerate files must tolerate a shorter file list.
  Whether Koverage or any CI step reads the per-`<class>` `line-rate` attribute rather than recomputing it
  was not established, so the implementation recomputes it defensively.
- **Configuration.** No change to `coverage.config`, `TaskMaster.runsettings`, or
  `scripts/vscode/TaskMaster.cli.runsettings`.

## Test Strategy

**New test file:** `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`.

This mirrors the production path `scripts/vscode/`, which is the confirmed repository convention —
`tests/scripts/vscode/` already contains `Invoke-MSTestWithCoverage.Helpers.Tests.ps1`,
`Invoke-MSTest.RunSettings.Tests.ps1`, `Invoke-VSBuild.Tests.ps1` and `Install-RepoDotNetSdk.Tests.ps1`. The
path is **not** `tests/scripts/powershell/`.

**Extended test file:** `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`, for the
end-to-end assertion that drives a fixture through `ConvertTo-KoverageCoberturaXml` and proves the filter ran
before the merge.

**Fixture strategy: inline here-string Cobertura XML only.** No temporary files, no on-disk fixtures, no
committed `.cs` sources. This matches the existing helper test file, which already dot-sources the production
script in `BeforeAll` and already carries two here-string fixtures containing `&lt;&gt;c` closure classes, so
the style is established precedent rather than a new convention. Each fixture is a minimal
`<coverage><packages><package><classes>` document with two or three `<class>` elements sharing one `filename`.
Class names use the escaped forms (`&lt;&gt;c__DisplayClass41_0`) and method names the escaped
`&lt;Member&gt;b__0`, exactly as emitted by the collector.

**Required regression cases (all ten must be implemented).**

| # | Direction | Fixture | Assertion |
| --- | --- | --- | --- |
| 1 | **Exclude (required direction 1)** | declaring class with `<method name="Visible">`; sibling `<>c__DisplayClass41_0` with `<method name="&lt;Exempt&gt;b__0">` on two lines | after processing, neither closure line survives, and document `lines-valid` counts only `Visible`'s lines |
| 2 | **Keep (required direction 2)** | same shape, but the closure method is `<method name="&lt;Visible&gt;b__0">` | the closure lines survive and remain in `lines-valid` |
| 3 | **Keep — async guard** | closure `<>c__DisplayClass33_1` with `&lt;Async&gt;b__0`, no plain `<method name="Async">`, plus a sibling class `Ns.T.&lt;Async&gt;d__33` with `MoveNext` | the closure lines survive. This is the case that fails if the presence set omits `d__` classes; it is modeled on the verified live counter-example `BreadcrumbPopupUiOperations.<>c__DisplayClass33_1` / `33_2` inside the non-exempt async `CreateAndInstallSurfaceAsync` |
| 4 | **Mixed closure** | one `<>c` class carrying both `&lt;Exempt&gt;b__0_0` and `&lt;Visible&gt;b__1_0` | only the exempt method's lines are dropped; the class survives; its `<lines>` equals the union of the retained methods' lines; `line-rate` is recomputed |
| 5 | **Whole-class removal** | closure class whose every method resolves to an absent member, with no declaring-type class for that filename | the `<class>` element is removed from `<classes>` entirely and the filename disappears from the report |
| 6 | **Pre-merge ordering proof** | fixture 1 driven end-to-end through `ConvertTo-KoverageCoberturaXml` (which merges) | the single merged class for that filename contains none of the exempt closure lines. This test is the ordering constraint's regression guard; it fails if the filter is moved after `Merge-CoberturaClassesByFilename` |
| 7 | **State machine untouched** | class `Ns.T.&lt;Foo&gt;d__1` with `MoveNext` and no plain `<method name="Foo">` | the class is retained unchanged. Pins the documented async residual so it cannot regress silently in either direction |
| 8 | **Covered closure lines** | exempt-member closure whose lines carry `hits="1"` (the `DisposeProductionSurface` shape) | the lines leave **both** `lines-covered` and `lines-valid`, and the document rate is recomputed consistently |
| 9 | **Unit purity** | direct calls to `Get-CoberturaClosureDeclaringMemberName` with `&lt;M&gt;b__0`, `&lt;M&gt;b__1_2`, `&lt;M&gt;g__L\|3_0`, `Ns.T.&lt;M&gt;d__4`, `Ns.T.&lt;&gt;c__DisplayClass5_0.&lt;&lt;M&gt;b__0&gt;d`, `MoveNext`, `.ctor` | each returns the expected token or `$null` |
| 10 | **Idempotence** | run the filter twice over the same document | the second pass produces no further change |

**Edge cases and negative scenarios covered by the above.** Unrecognized name shape (fail-safe retention,
case 9 via `MoveNext` / `.ctor`); a class with no `<methods>` element; a document with no `<packages>` node
(existing `throw` path, already covered by the helpers tests); a closure class whose declaring type has no
class element at all (case 5).

**Error handling and logging verification.** Assert that the filter emits no output stream content and does
not throw on unrecognized name shapes.

**Coverage impact and targets for changed lines/modules.** The new module must meet the repository floor of
line coverage >= 85% and branch coverage >= 75% (`.claude/rules/powershell.md`,
`.claude/rules/quality-tiers.md`). Coverage regression on changed lines is a blocking finding. Both modified
production files must remain under 500 lines.

**Toolchain commands.** `mcp__drm-copilot__run_poshqc_format` → `mcp__drm-copilot__run_poshqc_analyze` →
`mcp__drm-copilot__run_poshqc_test` (Pester v5, repo config
`scripts/powershell/PoshQC/settings/pester.runsettings.psd1`). Restart from step 1 on any failure or file
change. Type checking is not applicable to PowerShell. Exit codes are recorded in
`<FEATURE>/evidence/qa-gates/`.

**Manual validation steps.**

1. Run the full C# coverage pipeline once post-#441 and post-fix, and record the measured repository-wide and
   per-file figures under `<FEATURE>/evidence/baseline/`. Do not derive them.
2. Execute the targeted probe for the open question in Risks & Mitigations: add `[ExcludeFromCodeCoverage]` to
   an `async` member in a scratch (uncommitted) build and inspect whether the collector emits a
   `Type.<Member>d__<N>` class for it. Record the observed answer in evidence and correct the residual
   description if the probe shows no `d__` class is emitted.

## Acceptance Criteria

- [ ] A lambda declared inside a member carrying `[ExcludeFromCodeCoverage]` does not appear in the coverage
      denominator of the post-processed Cobertura report.
- [ ] A lambda declared inside a member that does **not** carry `[ExcludeFromCodeCoverage]` still appears in
      the coverage denominator.
- [ ] The selected fix surface (Candidate 1c, post-processing closure filter) is recorded in this `spec.md`
      with an explicit justification against every candidate alternative evaluated in research: Candidate 2
      (`coverage.config` / dotnet-coverage settings), Candidate 3 (type-level attribute), and the two
      disqualified Candidate-1 variants (blanket `<>c` exclusion; keying on `CompilerGeneratedAttribute`).
- [ ] Deterministic Pester regression tests cover both required directions and create no temporary files, no
      on-disk fixtures, and no committed `.cs` sources; every fixture is an inline here-string Cobertura
      document.
- [ ] A repository coverage baseline is re-captured against the post-#441 arithmetic and recorded numerically
      under `<FEATURE>/evidence/baseline/` and `<FEATURE>/evidence/qa-gates/`.
- [ ] No coverage threshold is changed by this feature; any corrected figure that would fail an existing
      threshold is recorded in evidence and handed to issue #494.
- [ ] Full PowerShell toolchain pass completed in order (`run_poshqc_format` → `run_poshqc_analyze` →
      `run_poshqc_test`) with recorded exit codes in `<FEATURE>/evidence/qa-gates/`.
- [ ] `Remove-CoberturaExemptClosureCoverage` is invoked inside `ConvertTo-KoverageCoberturaXml` after the
      `//class[@filename]` path-normalization loop and **before** `Merge-CoberturaClassesByFilename`, and a
      test drives a fixture end-to-end through `ConvertTo-KoverageCoberturaXml` to prove the pre-merge
      ordering (regression case 6).
- [ ] The presence set admits `Type.<Member>d__<N>` state-machine class names as proof that a declaring member
      exists, and a regression test proves that a covered lambda inside a **non-exempt async** member is
      retained (regression case 3).
- [ ] All ten regression cases enumerated in the Test Strategy section are implemented as individually named,
      passing Pester tests across
      `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` and
      `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`.
- [ ] The filter is a pure XML-to-XML transform: it reads no file, invokes no process, reads no clock, and
      makes no network call, and running it twice over the same document produces no further change.
- [ ] An unrecognized compiler-generated name shape causes the affected class or method to be **retained**,
      not removed; no code path can remove coverage for a member the filter failed to resolve.
- [ ] Production changes are limited to the new file
      `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` and exactly two edits in
      `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (a dot-source line and one call site); both files
      remain under 500 lines; no C# file, no `coverage.config`, no `*.runsettings`, no `CLAUDE.md` and nothing
      under `.claude/rules/` is modified.
- [ ] The corrected per-file figure for `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` is **measured**
      from an actual post-fix, post-#441 run and recorded in evidence, and the record notes that the two
      covered lines contributed by `<>c__DisplayClass42_0` leave both the numerator and the denominator, so
      the corrected rate is not `covered / (valid - 22)`.
- [ ] The three documented residuals are recorded in the feature's evidence and handed off as follow-up issue
      references rather than silently absorbed or widened into this feature: (a) lambda bodies inside
      `[ExcludeFromCodeCoverage]` **async** members remain counted; (b) local functions
      (`<Member>g__Local|N_M`) inside attributed members remain counted; (c) overload-name collisions cause
      under-exclusion, never over-exclusion.
- [ ] The probe for the unverified question — whether the collector emits a `Type.<Member>d__<N>` class for an
      attributed `async` member — is executed and its observed result recorded in
      `<FEATURE>/evidence/baseline/`; the residual description in this spec is corrected if the probe shows no
      `d__` class is emitted.

## Risks & Mitigations

**Documented residuals (deliberate scope choices, not oversights).**

1. **Lambda bodies inside `[ExcludeFromCodeCoverage]` async members remain counted.** If an attributed member
   is `async` or an iterator, its state machine class `Type.<Member>d__<N>` is the only trace of the member,
   and because a `d__` class is admitted into the presence set (mandatory, per the async correction), lambdas
   declared inside an attributed async member are retained. This is deliberate: the alternative would delete
   covered lambdas in non-exempt async members and fail required direction 2.
   *Unverified sub-question:* whether the collector emits a `d__` class **at all** for an attributed async
   member could not be determined from the committed artifacts. If it does not, those lambdas are in fact
   excluded and this residual is narrower than described. **Probe that settles it:** apply
   `[ExcludeFromCodeCoverage]` to an `async` member in a scratch build, run the coverage pipeline, and search
   the raw report for `name="…&lt;Member&gt;d__…"`. Presence confirms the residual as stated; absence narrows
   it. Record the observed result in evidence.
   *Handoff:* promote to its own follow-up issue rather than widening #457.
2. **Local functions inside attributed members remain counted.** A local function is emitted as
   `<method name="&lt;Member&gt;g__Local|N_M">` inside the **declaring type's own** `<class>` element rather
   than inside a closure type, and does not inherit the member's attribute. The filter scopes to closure
   classes only, so these are untouched; `g__` methods are also excluded from the presence set so they cannot
   mask an otherwise-absent declaring member. Extending the filter to strip `g__` methods from a declaring
   type's class is a natural symmetric extension, but it means mutating non-closure classes, which broadens
   blast radius beyond this issue's stated scope ("a lambda declared inside a member…").
   *Handoff:* out of #457 scope; record as a follow-up issue.
3. **Overload-name collisions cause under-exclusion, never over-exclusion.** The presence set is keyed by
   member *name*, not signature. If one overload of `Foo` is attributed and another is not, the non-attributed
   overload keeps `Foo` in the presence set and the attributed overload's lambdas are retained. Two types in
   the same file sharing a member name are separated by the declaring-type key; a partial type spanning files
   is separated by the filename key. Every failure mode is in the conservative direction — a file measures no
   better than it truly is.
   *Handoff:* record as a known limitation with a follow-up issue; do not attempt signature-based keying here.

**Other risks and mitigations.**

| Risk | Mitigation |
| --- | --- |
| Filter placed after `Merge-CoberturaClassesByFilename`, silently producing a no-op | Regression case 6 drives a fixture end-to-end through `ConvertTo-KoverageCoberturaXml` and fails if the merge has already collapsed the closure class. The ordering is also stated as a hard constraint in Proposed Fix. |
| Presence set omits `d__` classes, deleting covered lambdas in non-exempt async members | Regression case 3, modeled on the verified live counter-example, fails immediately. |
| Recorded numbers captured before #441 lands | Sequencing is an explicit dependency; no baseline may be captured until #441 has landed on the integration branch. |
| A corrected figure falls below an existing threshold and creates pressure to lower it | Explicitly out of scope. Record the figure in evidence and hand it to #494. The epic charter states that no child may silently lower a threshold. |
| Roslyn or collector name-shape drift | The filter fails safe: an unrecognized shape yields no derived member and the class is retained. Case 9 pins the recognized shapes. |
| Downstream consumer reads per-class `line-rate` rather than recomputing | The implementation recomputes per-class `line-rate` and `branch-rate` whenever it modifies a class, so both consumption styles agree. |
| Helpers module approaches the 500-line ceiling after #441 | The new logic goes in a separate file. Confirm the actual post-#441 size of `Invoke-MSTestWithCoverage.Helpers.ps1` during planning before revisiting that decision. |
| Committed coverage evidence on unmerged epic #136 branches will not reproduce | Known and recorded in the epic charter as a coordination note. Out of this feature's scope; decide at epic merge time whether #136 lands first or re-baselines afterward. |

## Rollout & Follow-up

**Rollout**

1. Confirm #441 has landed on `epic/build-ci-coverage-gate-fidelity-integration`.
2. Deliver the change on `bug/excludefromcodecoverage-nested-lambdas-457`, targeting the epic integration
   branch.
3. Run the PowerShell toolchain to a clean pass and record exit codes under `<FEATURE>/evidence/qa-gates/`.
4. Run the full C# coverage pipeline once and record the measured repository-wide and per-file figures under
   `<FEATURE>/evidence/baseline/`, including the measured corrected rate for
   `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` and the async-`d__` probe result.
5. Hand any figure that would fail an existing threshold to issue #494 in the evidence record. Change no
   threshold here.

**Post-fix monitoring and clean-up**

- Open follow-up issues for the three documented residuals before this feature closes.
- Feature `coverage-threshold-policy-reconciliation-494` (epic wave 2) consumes this feature's baseline as
  input.
- Epic #136 re-baselining is a separate coordination decision recorded in the epic charter.

**Links**

- Issue: #457
- Epic charter: `docs/features/epics/build-ci-coverage-gate-fidelity/epic.md`
- Research (authoritative for the fix-surface decision):
  `docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/research/2026-08-10T14-10-excludefromcodecoverage-nested-lambdas-fix-surface.md`
- Dependency: issue #441 (feature folder `cobertura-coverage-arithmetic-441`, epic wave 0)
- Downstream: issue #494 (feature folder `coverage-threshold-policy-reconciliation-494`, epic wave 2)
- Related known defect, not a gate here: issue #522 (`/p:Nullable=enable` type-check command)
