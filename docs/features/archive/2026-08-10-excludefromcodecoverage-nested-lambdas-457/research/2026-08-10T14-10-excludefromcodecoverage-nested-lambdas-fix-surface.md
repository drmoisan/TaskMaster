# Research: `[ExcludeFromCodeCoverage]` does not suppress nested lambdas — fix surface

Timestamp: 2026-08-10T14-10
Issue: #457
Feature folder: `docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457`
Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-af6843b0a129fc575`

---

## 1. Assumptions

1. **Post-#441 arithmetic contract (stated, not verified).** Issue #441 has not landed on this branch;
   `docs/features/active/*441*` does not exist here (verified by glob — no matches). This analysis is written
   against the contract supplied by the orchestrator: after #441, `Get-CoberturaCoverageSummary` and
   `Merge-CoberturaClassesByFilename` use the child axis `./lines/line` rather than the descendant axis
   `.//lines/line`, each source line appears exactly once in the denominator, and per-file `line-rate`
   equals the rate computed from the merged class-level `<lines>` set alone (distinct line numbers, max hits
   per number). All locators below are function/symbol anchors, never absolute line numbers.
2. **Evidence corpus.** No C# coverage run was executed in this session. All Cobertura evidence is read from
   committed artifacts. Two classes of artifact exist in the repo and must not be confused:
   - **Post-processed** artifacts (relative `filename` values, closure classes already merged away). Example:
     `docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/evidence/qa-gates/coverage-final.cobertura.xml`.
   - **Raw** artifacts (absolute `filename` values, closure classes still present as sibling `<class>` elements).
     Example, used as the primary evidence source here:
     `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/coverage-baseline.cobertura.xml`
     (873 occurrences of the escaped token `&lt;&gt;c`; absolute `filename` attributes).
3. **Collector behavior is stable.** `dotnet-coverage` emits one `<class>` element per (CLR type, source file)
   pair. This is inferred from the four separate `<class name="TaskMaster.AppOlObjects" …>` elements with four
   different partial-class filenames in the raw artifact. It matches every case examined but was not confirmed
   against collector source.
4. **Default attribute excludes are in force.** Neither `coverage.config` nor `TaskMaster.runsettings` declares
   an `<Attributes>` block (both carry only `<ModulePaths><Exclude>`), so the collector's documented defaults
   apply, including `^System\.Diagnostics\.CodeAnalysis\.ExcludeFromCodeCoverageAttribute$`.
5. **Scope guard.** The fix must not re-tune any coverage threshold (owned by #494) and must not edit
   `CLAUDE.md` or `.claude/rules/**`.

---

## 2. Current state — verified facts

### 2.1 Pipeline shape

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` (`Invoke-MSTestWithCoverageMain`) collects via
`Invoke-DotnetCoverageCollection` → `Get-DotnetCoverageArgumentList` (`dotnet-coverage collect --output-format cobertura
--settings <derived coverage.config> -- vstest.console.exe … /Settings:scripts/vscode/TaskMaster.cli.runsettings`),
then post-processes with `ConvertTo-KoverageCoberturaXml` from
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`.

`ConvertTo-KoverageCoberturaXml` executes, in order:

1. remove `<package>` elements not in `Get-KoverageProjectAllowlist`;
2. rewrite every `//class[@filename]` via `ConvertTo-KoverageRelativePath`;
3. `Merge-CoberturaClassesByFilename`;
4. inject `<sources><source>.</source></sources>` when absent;
5. `Get-CoberturaCoverageSummary` and write the document-level rate attributes.

`scripts/vscode/TaskMaster.cli.runsettings` carries MSTest parallelization only and no data collector; the
effective instrumentation settings come solely from `coverage.config` (a `<Configuration><CodeCoverage>`
document with a single `<ModulePaths><Exclude>` block). `TaskMaster.runsettings` at the repo root is the
Visual Studio auto-detected file and carries the same single `<ModulePaths><Exclude>` block nested under
`DataCollectionRunSettings/DataCollectors/DataCollector/Configuration/CodeCoverage`.

### 2.2 The collector DOES honor member-level `[ExcludeFromCodeCoverage]`

An attributed member is not merely reported as uncovered; its `<method>` element is absent from the report
entirely. In `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`, the members `ShowOwnedPopup`,
`CreateProductionControl`, `BeginProductionInitialization`, `ReadProductionCore`, `BeginProductionNavigation`,
`DisposeProductionSurface` and `BindProductionNavigation` all carry `[ExcludeFromCodeCoverage]`. A search of the
raw artifact for `name="(BeginProductionNavigation|DisposeProductionSurface|BindProductionNavigation|
CreateProductionControl|ReadProductionCore|BeginProductionInitialization|ShowOwnedPopup)"` returns **no matches**.

This is the load-bearing observation for the recommended fix: *absence of a plain `<method>` element is the
report's own record that the member was excluded at instrumentation time.*

### 2.3 Verified Cobertura representation of compiler-generated closure types

Closure types appear as **separate `<class>` sibling elements inside the same `<classes>` container**, with the
**same `filename`** as the declaring member's source file, and with **`<method name>` values that embed the
declaring member's name**.

Concrete XML from the raw artifact (`…/424/evidence/baseline/coverage-baseline.cobertura.xml`), abridged only by
truncating the absolute path prefix:

```xml
<class line-rate="0" branch-rate="1" complexity="1"
       name="QuickFiler.Viewers.BreadcrumbPopupUiOperations.&lt;&gt;c__DisplayClass41_0"
       filename="…\QuickFiler\Viewers\BreadcrumbPopupUiOperations.cs">
  <methods>
    <method line-rate="0" branch-rate="1" complexity="1" name="&lt;BeginProductionNavigation&gt;b__0" signature="()">
      <lines>
        <line number="406" hits="0" branch="False" />
      </lines>
    </method>
    <method line-rate="0" branch-rate="1" complexity="1" name="&lt;BeginProductionNavigation&gt;b__1" signature="()">
      <lines>
        <line number="409" hits="0" branch="False" />
      </lines>
    </method>
  </methods>
  <lines>
    <line number="406" hits="0" branch="False" />
    <line number="409" hits="0" branch="False" />
  </lines>
</class>
```

A second, non-capturing example from `TaskVisualization/FlagTasks.cs` (post-processed artifact, hence already
merged, which is why its `<lines>` set is larger than its single `<method>`):

```xml
<class line-rate="0" branch-rate="0" complexity="3"
       name="TaskVisualization.FlagTasks.&lt;&gt;c" filename="TaskVisualization\FlagTasks.cs">
  <methods>
    <method line-rate="0" branch-rate="1" complexity="1" name="&lt;InitializeToDoList&gt;b__13_0" signature="(object)">
      <lines><line number="114" hits="0" branch="False" /></lines>
    </method>
  </methods>
  <lines>
    <line number="114" hits="0" branch="False" />
    <line number="136" hits="0" branch="False" />
    … lines 137-143 …
    <line number="159" hits="0" branch="False" />
  </lines>
</class>
```

`FlagTasks.cs` line 114 is `?.Select(x => new OutlookItem(x))` inside `[ExcludeFromCodeCoverage] InitializeToDoList`;
lines 136-143 are the block lambda later in the same member; line 159 is
`toDoSelection.ForEach(x => x.WriteFlagsBatch(flagsToSet));` inside `[ExcludeFromCodeCoverage] PopulateUdf`. No
`<class name="TaskVisualization.FlagTasks" …>` element exists for that file at all, because every member of the
type is attributed. The defect is therefore reproduced in committed evidence.

#### 2.3.1 Answers to the specific representational questions

| Question | Verified answer |
|---|---|
| `<class name=…>` value shape | `<Namespace>.<DeclaringType>.<>c`, `<Namespace>.<DeclaringType>.<>c__DisplayClass<N>_<M>`, optionally with a generic suffix (`…<>c__DisplayClass24_0&lt;T&gt;`), and nested forms (`…<>c__DisplayClass13_0.&lt;&lt;NormalizeFactory&gt;b__0&gt;d`). `<` and `>` are XML-escaped as `&lt;`/`&gt;`. |
| `<class filename=…>` value | Identical to the declaring member's source file. Raw output uses absolute paths; post-processing rewrites to repo-relative. |
| `<method name=…>` carries the synthesized name | Yes. `&lt;Member&gt;b__<N>_<M>` for cached non-capturing lambdas on `<>c`, `&lt;Member&gt;b__<K>` for lambdas on a `<>c__DisplayClass`, `&lt;Member&gt;g__<Local>|<N>_<M>` for local functions, `MoveNext` for state machines. |
| Separate `<class>` element, or methods inside the declaring type's `<class>`? | **Separate `<class>` element**, sibling to the declaring type's element inside the same `<classes>`, in the same `<package>`. The one exception is local functions, which are emitted as `<method>` elements *inside the declaring type's own `<class>`*. |
| Class `<lines>` vs union of `<method>` lines | In raw output the class `<lines>` set equals the de-duplicated union of its methods' lines. Verified on `<>c__DisplayClass41_0`, `42_0`, `46_0`, `46_1`. |

#### 2.3.2 Async and iterator members are a distinct shape and a trap

Async/iterator members do **not** produce a `<method>` element in the declaring type's `<class>`; the whole body
moves to a state machine `<class name="Type.&lt;Member&gt;d__<N>">` whose only method is `MoveNext`. Verified:
`QuickFiler.Viewers.BreadcrumbPopupUiOperations.&lt;CreateAndInstallSurfaceAsync&gt;d__33`,
`…&lt;IgnoreFailureAsync&gt;d__35`, `…&lt;ObserveExternalAsync&gt;d__34`, `…&lt;RetryAsync&gt;d__36`,
`TaskMaster.AppOlObjects.&lt;LoadAsync&gt;d__110`, `…&lt;LoadStoresAsync&gt;d__117` — and, correspondingly, a
search for `name="(BeginNavigationAsync|CreateAndInstallSurfaceAsync|IgnoreFailureAsync|ObserveExternalAsync|
RetryAsync|NormalizeFactory)"` returns matches only for the two **non-async** members `NormalizeFactory` and
`BeginNavigationAsync`.

This matters because `BreadcrumbPopupUiOperations.<>c__DisplayClass33_1` and `…33_2` are covered lambdas
(`line-rate="1"`) declared inside the **non-exempt async** member `CreateAndInstallSurfaceAsync`. A naive
"declaring member has no `<method>` element ⇒ exempt" rule would wrongly delete them, violating direction 2.
The presence test must accept a `<Member>d__N` state-machine class as proof the member exists.

### 2.4 Measured defect inventory for `BreadcrumbPopupUiOperations.cs`

From the raw artifact, the closure classes whose declaring member is attributed:

| Closure class | Declaring member (attributed) | Lines | Hits |
|---|---|---|---|
| `<>c__DisplayClass41_0` | `BeginProductionNavigation` | 406, 409 | 0 |
| `<>c__DisplayClass42_0` | `DisposeProductionSurface` | 415, 416 | 1 |
| `<>c__DisplayClass46_0` | `BindProductionNavigation` | 471, 472, 474, 480-484, 490 | 0 |
| `<>c__DisplayClass46_1` | `BindProductionNavigation` | 473, 475-480, 485-489 | 0 |

Distinct uncovered lines across 41_0 / 46_0 / 46_1 = **22**, which reproduces the issue's `(258 - 22) / 258`
ceiling exactly. Note the additional two **covered** lines in `42_0`: a correct fix removes them from *both*
numerator and denominator, so the corrected rate is not simply `covered / (valid - 22)`. Any numeric expectation
recorded in evidence must be measured, not derived.

### 2.5 Repository conventions confirmed

- Pester tests mirror production layout: `tests/scripts/vscode/` contains `Invoke-MSTestWithCoverage.Helpers.Tests.ps1`,
  `Invoke-MSTest.RunSettings.Tests.ps1`, `Invoke-VSBuild.Tests.ps1`, `Install-RepoDotNetSdk.Tests.ps1`. Convention
  confirmed for production path `scripts/vscode/`.
- The existing helper test dot-sources the production script in `BeforeAll` and uses **inline here-string XML
  fixtures** — no temp files. It already contains two fixtures with `&lt;&gt;c` closure classes, so the fixture
  style needed here is established precedent.
- `.claude/rules/powershell.md`: PowerShell 7+, advanced functions with `[CmdletBinding()]`, approved verbs,
  files under 500 lines, change budget of 2 production files per direct-mode change, wrapper-function seams for
  executables, Pester v5 via PoshQC MCP (`run_poshqc_format` → `run_poshqc_analyze` → `run_poshqc_test`).
- `Invoke-MSTestWithCoverage.Helpers.ps1` is currently 357 lines. #441 will add to it.

---

## 3. Candidate evaluation

### Candidate 1 — Post-processing exclusion in `Invoke-MSTestWithCoverage*.ps1`

Three sub-variants, evaluated separately.

**1a. Exclude every class whose name matches `<>c` / `<>c__DisplayClass`.**
**Disqualified.** It deletes lambdas declared inside non-exempt members. Direct counter-example from the raw
artifact: `BreadcrumbPopupUiOperations.<>c__DisplayClass25_0` (`<BeginNavigationAsync>b__0`, lines 165-169+,
`line-rate="1"`) belongs to the non-exempt member `BeginNavigationAsync`, which is present in the report as
`<method name="BeginNavigationAsync" …>`. Fails the required second direction.

**1b. Key on `CompilerGeneratedAttribute` recorded in the Cobertura output.**
**Disqualified — not implementable.** Cobertura carries no attribute metadata. The only attributes on `<class>`
are `line-rate`, `branch-rate`, `complexity`, `name`, `filename`. Recovering attribute data would require a
companion assembly-metadata read (Mono.Cecil or `System.Reflection.Metadata`), which is a .NET dependency, not
PowerShell/configuration work, and would bind the post-processor to build outputs that may not exist when a
report is consumed.

**1c. Key on the synthesized method name, and infer exemption from the declaring member's absence.**
**Viable, and the only variant that satisfies both directions from the Cobertura XML alone.**

The crux question posed by the orchestrator — *is the declaring-member linkage recoverable from the Cobertura XML
alone?* — resolves to **yes, twice over**:

- *Which member declared the lambda* is recoverable from the mangled name: `<method name="&lt;Member&gt;b__…">`,
  and for nested async-lambda state machines from the class name `…&lt;&lt;Member&gt;b__0&gt;d`.
- *Whether that member was attributed* is recoverable from the report's own instrumentation record: because the
  collector's default `<Attributes><Exclude>` list contains
  `^System\.Diagnostics\.CodeAnalysis\.ExcludeFromCodeCoverageAttribute$`, an attributed member emits **no**
  `<method>` element (§2.2). Absence is the exemption signal.

The absence signal needs exactly one correction, established in §2.3.2: a member whose body was moved into an
async/iterator state machine also has no plain `<method>` element, so the presence set must additionally admit
`<Member>d__N` class names for the same declaring type and filename.

A source-reading sub-variant (open the `.cs` file and check for `[ExcludeFromCodeCoverage]` above the member
whose line range contains the lambda) was considered and rejected: it requires C# attribute-list/comment/
expression-body parsing in PowerShell, it makes the post-processor fail when a report is processed away from
its source tree, and it would force a committed `.cs` fixture into the test tree. Variant 1c needs neither.

**Verdict: viable in both directions.**

### Candidate 2 — Instrumentation-time exclusion via `coverage.config`

Schema, from the Microsoft documentation. The elements permitted directly inside `<CodeCoverage>` are:
`SymbolSearchPaths`, `ModulePaths` (with `Include`/`Exclude`/`IncludeDirectories`), `Functions`, `Attributes`,
`Sources`, `CompanyNames`, `PublicKeyTokens`, plus the scalar switches `UseVerifiableInstrumentation`,
`AllowLowIntegrityProcesses`, `CollectFromChildProcesses`, `CollectAspDotNet`,
`EnableStaticNativeInstrumentation`, `EnableDynamicNativeInstrumentation`,
`EnableStaticNativeInstrumentationRestore`, and (dotnet-coverage) `EnableStaticManagedInstrumentation` /
`EnableDynamicManagedInstrumentation`.

Sources:
- `https://learn.microsoft.com/en-us/visualstudio/test/customizing-code-coverage-analysis`
- `https://learn.microsoft.com/en-us/dotnet/core/additional-tools/dotnet-coverage`

**Schema sharing.** The dotnet-coverage page states explicitly for `--settings`: *"The format is the same as the
data collector configuration inside a runsettings file."* The difference is only the wrapper: dotnet-coverage
uses `<Configuration><CodeCoverage>…` as the document root, while `.runsettings` nests the identical
`<CodeCoverage>` element under `RunSettings/DataCollectionRunSettings/DataCollectors/DataCollector/Configuration`.
`scripts/vscode/TaskMaster.cli.runsettings` carries no data collector at all, so nothing added there would take
effect on the CLI path; the repo-root `TaskMaster.runsettings` does carry one and is the Visual Studio path.

**Is `ExcludeFromCodeCoverageAttribute` honored?** Yes — it is one of the four documented defaults:

```xml
<Attributes>
  <Exclude>
    <Attribute>^System\.Diagnostics\.DebuggerHiddenAttribute$</Attribute>
    <Attribute>^System\.Diagnostics\.DebuggerNonUserCodeAttribute$</Attribute>
    <Attribute>^System\.CodeDom\.Compiler\.GeneratedCodeAttribute$</Attribute>
    <Attribute>^System\.Diagnostics\.CodeAnalysis\.ExcludeFromCodeCoverageAttribute$</Attribute>
  </Exclude>
</Attributes>
```

**Does any such exclude propagate to compiler-generated closure types? No.** That is precisely the defect: §2.4
shows attributed members suppressed while their closure types survive.

The only settings-level lever that would reach closure types is
`<Attribute>^System\.Runtime\.CompilerServices\.CompilerGeneratedAttribute$</Attribute>`, and Microsoft documents
the consequence directly: *"If you exclude the CompilerGeneratedAttribute attribute, code that uses language
features such as `async`, `await`, `yield return`, and auto-implemented properties is excluded from code coverage
analysis. To exclude truly generated code, only exclude the GeneratedCodeAttribute attribute."* That removes
**every** lambda, async state machine and iterator repo-wide, including those in non-exempt members.
**Disqualified on direction 2.**

The `<Functions><Exclude>` lever could in principle name each exempt member's mangled lambda methods
(`<Function>.*\.&lt;BeginProductionNavigation&gt;b__.*</Function>`), which is correct in both directions, but it
requires a hand-maintained enumeration of every attributed member in the repo — 263 `[ExcludeFromCodeCoverage]`
occurrences across 110 `.cs` files today — duplicated in a config file with no compiler or test to keep it in
sync. It also cannot be regression-tested under Pester without executing the full C# suite, so it cannot satisfy
the "deterministic Pester regression tests, no temp files" acceptance criterion.
**Disqualified on maintenance cost and testability.**

Two further notes for the plan, whichever candidate wins: (a) neither settings file currently declares
`<Attributes>`, so if one is ever added the four defaults must be re-listed or they are lost; (b) any settings
change would have to be mirrored in both `coverage.config` and `TaskMaster.runsettings` to keep the CLI and
Visual Studio paths consistent.

**Verdict: disqualified.**

### Candidate 3 — Type-level `[ExcludeFromCodeCoverage]` as a source convention

Moving the attribute from member to type does suppress the closure types (they are nested inside the attributed
type), but it removes the entire type from the denominator. For `BreadcrumbPopupUiOperations` that would drop
roughly 234 covered lines along with the 22 uncovered ones — it deletes exactly the testable seam the pattern
exists to create, and it makes lambdas in non-exempt members of the same type disappear from the denominator.
**Disqualified on direction 2 by construction.**

Blast radius, for the record: 263 attribute occurrences across 110 files; any file with a mix of exempt and
testable members (the overwhelmingly common case, and the whole point of the seam pattern) cannot adopt it. The
seam pattern does not survive.

**Verdict: disqualified.**

### Candidate 4 (found during research) — Hybrid: post-processing filter plus a documented residual

Not a separate mechanism; it is candidate 1c with two behaviors made explicit rather than left implicit
(§6.2 and §6.3). Folded into the recommendation.

---

## 4. Recommendation

**Adopt Candidate 1c: a post-processing filter in the PowerShell pipeline that removes compiler-generated
closure-type coverage whose declaring member is absent from the instrumented method set, run before
`Merge-CoberturaClassesByFilename`.**

Justification against the alternatives:

- **Correctness in both directions.** It is the only candidate that keeps lambdas in non-exempt members. §2.3.1
  and §2.3.2 establish that both required inputs — declaring member identity and declaring member exemption —
  are recoverable from the Cobertura XML alone.
- **Determinism.** The filter is a pure XML-to-XML transform over a document the pipeline already parses. No
  clock, no filesystem, no process, no network. Identical input yields identical output.
- **Testability under Pester without temp files.** Inline here-string Cobertura fixtures, exactly matching the
  established style of `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`. Candidate 2 cannot be
  unit-tested at all without a full C# collection run; candidate 3 has no PowerShell surface to test.
- **Blast radius.** One new production PowerShell file plus one dot-source line in an existing one; zero C#
  files, zero configuration files, zero threshold changes. Candidate 3 would touch up to 110 C# files.
- **Maintenance cost.** The rule is derived from the report each run; nothing must be kept in sync by hand.
  Candidate 2's `<Functions>` variant would require a 263-entry hand-maintained list.

---

## 5. Implementation sketch

### 5.1 Pipeline ordering constraint (hard requirement)

`Merge-CoberturaClassesByFilename` groups `<class>` elements by `filename` and collapses the group into one
node, choosing as primary the first member whose `name` does not match `<` and unioning the `<lines>` of the
whole group. A closure type and its declaring type always share a `filename` (§2.3.1), therefore **they are
always merged**, and after the merge the closure lines are indistinguishable from declaring-type lines: the
surviving node keeps only the primary's `<methods>`, so the `<Member>b__…` linkage is destroyed.

> **The closure filter MUST run before `Merge-CoberturaClassesByFilename`.** Running it after the merge is not
> merely suboptimal; the information it depends on no longer exists in the document.

Insertion point inside `ConvertTo-KoverageCoberturaXml`: after the `//class[@filename]` path-normalization loop
and immediately before the `Merge-CoberturaClassesByFilename -XmlDocument $xml` call. `Get-CoberturaCoverageSummary`
is invoked twice downstream — once per merged class inside the merge, once for the document total at the end —
and both consume the already-filtered tree, so no change to either call site is required.

Resulting order in `ConvertTo-KoverageCoberturaXml`:

```
remove non-allowlisted <package>
  → normalize //class[@filename]
  → Remove-CoberturaExemptClosureCoverage        (NEW)
  → Merge-CoberturaClassesByFilename
  → inject <sources>
  → Get-CoberturaCoverageSummary + document attributes
```

### 5.2 New production file and function anchors

Put the new logic in a new file rather than growing `Invoke-MSTestWithCoverage.Helpers.ps1`: that file is 357
lines today and #441 will add to it; the 500-line ceiling in `.claude/rules/general-code-change.md` and
`.claude/rules/powershell.md` leaves too little headroom for roughly 110 further lines.

**New:** `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`

- `Get-CoberturaClosureDeclaringMemberName` — pure. Given a synthesized class name or method name, returns the
  declaring member token or `$null`. Recognizes `^<(?<m>[^<>]+)>b__`, `^<(?<m>[^<>]+)>g__`, and, for class names,
  the last `<(?<m>[^<>]+)>d__\d+` segment and the inner token of `<<(?<m>[^<>]+)>b__\d+>d`.
- `Test-CoberturaClosureClassName` — pure. `$true` when the class name contains the `.<>c` marker (covers `<>c`,
  `<>c__DisplayClass<N>_<M>`, generic suffixes, and nested `<>c…​.<<Member>b__K>d`). Deliberately **false** for
  `Type.<Member>d__N` state machines — see §6.2.
- `Get-CoberturaDeclaringTypeName` — pure. Class name truncated at the first `.<`.
- `Get-CoberturaInstrumentedMemberName` — builds the presence set for one `<package>`, keyed by
  `"$declaringType|$filename"`. Members are admitted from exactly two sources:
  1. `<method name="X">` on any class whose name contains no `.<`, where `X` does not begin with `<`;
  2. the `<Member>` token of any class named `Type.<Member>d__<N>` (async/iterator state machine).
  `<Member>g__Local|N_M` methods are deliberately **not** admitted — see §6.3.
- `Remove-CoberturaExemptClosureCoverage -XmlDocument [xml]` — the orchestrating function. For each `<package>`:
  build the presence set; for each closure class, derive each `<method>`'s declaring member (falling back to a
  class-name-derived token when the method name yields none, e.g. `MoveNext` on a nested async-lambda state
  machine); drop methods whose declaring member is not in the presence set for that `(declaringType, filename)`
  key; keep methods whose declaring member could not be derived (conservative). Then:
  - if no method was dropped, leave the class untouched;
  - otherwise rebuild `./lines` as the de-duplicated union of the retained methods' `./lines/line` (max `hits`,
    richest `condition-coverage`) and recompute `line-rate` / `branch-rate` by reusing
    `Get-CoberturaLineConditionCoverageParts` and `Get-CoberturaCoverageSummary` on a scratch document, exactly
    as `Merge-CoberturaClassesByFilename` already does;
  - if zero methods are retained, remove the `<class>` element from its `<classes>` parent.

**Modified:** `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`

- add `. (Join-Path $PSScriptRoot 'Invoke-MSTestWithCoverage.ClosureFilter.ps1')` near the top (`$PSScriptRoot`
  resolves to the containing script's own directory even when the file is dot-sourced);
- add the single `Remove-CoberturaExemptClosureCoverage -XmlDocument $xml` call inside
  `ConvertTo-KoverageCoberturaXml` at the position fixed in §5.1.

This is two production PowerShell files, within the direct-mode change budget in `.claude/rules/powershell.md`.

### 5.3 Worked trace against real data

| Closure class | Derived member | Presence-set lookup | Outcome |
|---|---|---|---|
| `BreadcrumbPopupUiOperations.<>c__DisplayClass41_0` | `BeginProductionNavigation` | absent (no plain method, no `d__` class) | class removed — 2 uncovered lines leave the denominator |
| `…<>c__DisplayClass46_0` / `46_1` | `BindProductionNavigation` | absent | classes removed — 20 further distinct uncovered lines leave the denominator |
| `…<>c__DisplayClass42_0` | `DisposeProductionSurface` | absent | class removed — 2 **covered** lines leave both numerator and denominator |
| `…<>c__DisplayClass25_0` | `BeginNavigationAsync` | present (plain `<method name="BeginNavigationAsync">`) | retained |
| `…<>c__DisplayClass33_1` / `33_2` | `CreateAndInstallSurfaceAsync` | present via `…<CreateAndInstallSurfaceAsync>d__33` | retained — this is the case that forces rule 2 of the presence set |
| `…<>c__DisplayClass8_0` | `.ctor` | present (`<method name=".ctor">`) | retained — correct; the lambda on line 58 is in a non-attributed constructor |
| `AppOlObjects.<>c__DisplayClass121_0` | `ResolveInboxForStore` | absent; no `TaskMaster.AppOlObjects` class exists for `AppOlObjects.StoreRehook.cs` | class removed |
| `FlagTasks.<>c` (and its sibling display classes) | `InitializeToDoList`, `PopulateUdf` | absent; no `TaskVisualization.FlagTasks` class exists for that file | classes removed; the file disappears from the report entirely, which is the correct semantic for a wholly-exempt file |
| `AppOlObjects.<LoadAsync>d__110` | n/a — not a closure class | not evaluated | untouched |

---

## 6. Known limitations of the recommendation

### 6.1 Overload-name collisions cause under-exclusion, never over-exclusion

The presence set is keyed by member *name*, not signature. If one overload of `Foo` is attributed and another is
not, the non-attributed overload keeps `Foo` in the presence set and the attributed overload's lambdas are
retained. Similarly, two types in the same file sharing a member name are separated by the declaring-type key,
but a partial type spanning files is separated by the filename key. Every failure mode of the key is in the
under-exclusion direction, so the metric errs conservatively (a file measures no better than it truly is).

### 6.2 Lambda bodies inside `[ExcludeFromCodeCoverage]` **async** members remain counted

If an attributed member is `async` or an iterator, its state machine class `Type.<Member>d__N` is the only trace
of the member. The filter cannot distinguish "attributed async member" from "non-attributed async member" — and
because a `d__` class is admitted into the presence set (rule 2, required by §2.3.2), lambdas declared inside an
attributed async member are **retained**. This is a deliberate, documented residual, chosen because the
alternative would delete covered lambdas in non-exempt async members.

Whether the collector emits a `d__` class at all for an attributed async member could not be determined from the
committed artifacts; if it does not, those lambdas are excluded and the residual is narrower than described. This
should be measured during implementation with a targeted probe and recorded in evidence.

**Recommended follow-up:** promote this residual to its own issue rather than widening #457.

### 6.3 Local functions inside attributed members remain counted

A local function is emitted as `<method name="&lt;Member&gt;g__Local|N_M">` inside the **declaring type's own**
`<class>` element, not inside a closure type, and does not inherit the member's attribute. The recommended filter
scopes to closure classes only, so these are untouched. `g__` methods are also excluded from the presence set, so
they do not mask an otherwise-absent declaring member.

Extending the filter to strip `g__` methods from the declaring type's class when the declaring member is absent
is a natural symmetric extension, but it means mutating non-closure classes, which broadens blast radius beyond
the issue's stated acceptance criteria ("a lambda declared inside a member…"). **Recommendation: leave out of
#457 scope and record as a follow-up issue.**

### 6.4 Interaction with #441

If #441 has not landed when this work begins, the filter still functions, but the recorded numeric baselines
will reflect the pre-#441 double count and will not match post-#441 figures. Sequencing (#441 wave 0 before #457
wave 1) must be honored before any baseline is captured.

---

## 7. Test design

Location: `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`, mirroring the production path
`scripts/vscode/`, per the repository convention confirmed in §2.5. Additionally extend
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` with the end-to-end ordering assertions
through `ConvertTo-KoverageCoberturaXml`.

**Fixture strategy: inline here-string Cobertura documents only.** No temp files, no on-disk fixtures, no
committed `.cs` sources. This matches the existing helper test file, which already carries two closure-class
here-string fixtures. Each fixture is a minimal `<coverage><packages><package><classes>` document with two or
three `<class>` elements sharing one `filename`. Class names use the escaped forms `&lt;&gt;c__DisplayClass41_0`
and method names the escaped `&lt;Member&gt;b__0`, exactly as emitted.

### Required regression cases — both directions

| # | Direction | Fixture | Assertion |
|---|---|---|---|
| 1 | **Exclude** | declaring class with `<method name="Visible">`; sibling `<>c__DisplayClass41_0` with `<method name="&lt;Exempt&gt;b__0">` on lines 406, 409 | after `ConvertTo-KoverageCoberturaXml`, no `//line[@number='406']` or `[@number='409']` survives, and document `lines-valid` counts only `Visible`'s lines |
| 2 | **Keep** | same shape, but the closure method is `<method name="&lt;Visible&gt;b__0">` | lines 406, 409 survive and are in `lines-valid` |
| 3 | **Keep (async guard)** | closure `<>c__DisplayClass33_1` with `<Async&gt;b__0`, no plain `<method name="Async">`, plus sibling class `Ns.T.&lt;Async&gt;d__33` with `MoveNext` | closure lines survive — this is the case that fails if the presence set omits `d__` classes |
| 4 | **Mixed closure** | one `<>c` class carrying both `<Exempt&gt;b__0_0` and `<Visible&gt;b__1_0` | only the exempt method's lines are dropped; the class survives; its `<lines>` equals the union of retained methods' lines; `line-rate` is recomputed |
| 5 | **Whole-class removal** | closure class whose every method resolves to an absent member, and no declaring-type class for that filename | the `<class>` element is removed from `<classes>` entirely; the filename disappears from the report |
| 6 | **Ordering** | fixture 1 driven through `ConvertTo-KoverageCoberturaXml` (which merges) | the single merged class for the filename contains none of the exempt closure lines — proves the filter ran pre-merge |
| 7 | **State machine untouched** | class `Ns.T.&lt;Foo&gt;d__1` with `MoveNext`, no plain `<method name="Foo">` | class retained unchanged — pins the documented §6.2 behavior so it cannot regress silently |
| 8 | **Covered closure lines** | exempt-member closure whose lines have `hits="1"` (the `DisposeProductionSurface` shape) | lines leave **both** `lines-covered` and `lines-valid`; the document rate is recomputed consistently |
| 9 | **Unit purity** | direct calls to `Get-CoberturaClosureDeclaringMemberName` with `&lt;M&gt;b__0`, `&lt;M&gt;b__1_2`, `&lt;M&gt;g__L|3_0`, `Ns.T.&lt;M&gt;d__4`, `Ns.T.&lt;&gt;c__DisplayClass5_0.&lt;&lt;M&gt;b__0&gt;d`, `MoveNext`, `.ctor` | each returns the expected token or `$null` |
| 10 | **Idempotence** | run the filter twice on the same document | second pass makes no further change |

Toolchain per `.claude/rules/powershell.md`: `run_poshqc_format` → `run_poshqc_analyze` → `run_poshqc_test`,
restarting from step 1 on any failure or file change, with exit codes recorded in evidence.

---

## 8. Open risks and items not verified locally

1. **No live coverage run.** Every claim about collector output comes from committed artifacts. The behavior of
   `dotnet-coverage` for an attributed **async** member (does it emit the `d__` class?) is unverified and drives
   §6.2. Verify with a targeted probe during implementation.
2. **#441 not present.** The post-441 arithmetic contract is taken as given (Assumption 1). If #441's landed
   behavior differs, §5.1's insertion point still holds (it is upstream of both affected functions) but any
   recorded numbers must be recaptured.
3. **Collector version drift.** The `<>c` / `<>c__DisplayClass` / `d__` / `g__` name shapes are Roslyn
   implementation details, stable in practice across every C# compiler in use but not contractual. The filter
   fails safe: an unrecognized shape yields no derived member and the class is retained.
4. **Per-(type, file) class emission** (Assumption 3) is inferred, not confirmed against collector source. If a
   future collector emitted one class element per type spanning multiple partial files, the filename component of
   the presence-set key would produce under-exclusion, not over-exclusion.
5. **Downstream consumers.** Whether Koverage or any CI step reads the per-`<class>` `line-rate` attribute rather
   than recomputing was not established; the sketch recomputes it defensively.
6. **Baseline movement.** Removing covered closure lines (§2.4, `42_0`) changes the numerator as well as the
   denominator. The corrected repository figure must be measured. Per the issue's scope, any figure that would
   fail an existing threshold is recorded in evidence and handed to #494; no threshold is touched here.
7. **File-size headroom.** `Invoke-MSTestWithCoverage.Helpers.ps1` is 357 lines before #441. The new-file
   recommendation in §5.2 exists to keep both files under the 500-line ceiling; confirm the actual post-#441 size
   before deciding otherwise.
