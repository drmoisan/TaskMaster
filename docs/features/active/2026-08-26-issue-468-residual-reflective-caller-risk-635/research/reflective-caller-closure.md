# Research — settling the issue #468 residual reflective-caller risk (issue #635)

- Date: 2026-08-29
- Branch: `bug/issue-468-residual-reflective-caller-risk-635`
- Feature folder: `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635`
- Work mode: `full-bug`
- Repository root is referred to throughout as `<repo-root>`.

---

## 0. Tooling constraint on this research session (read first)

**No shell tool was available.** The `Bash` tool returned
`Error: No such tool available: Bash. Bash is disabled for this session, in subagents as well as here.`
on every attempt. Consequently:

- No `git` command was executed. Every statement in this document that would normally be proved by
  `git log`, `git show`, `git ls-files`, or `git grep` is instead proved by content search over the
  **working tree at `HEAD`** using the ripgrep-backed `Grep` tool and by filesystem enumeration using
  `Glob`, or is explicitly marked **UNMEASURED**.
- The `Grep` tool honours `.gitignore`. `<repo-root>/.gitignore` excludes `[Bb]in/`, `[Oo]bj/`,
  `**/[Pp]ackages/*`, `[Tt]est[Rr]esult*/`, `artifacts/`, `coverage/*`, `.claude/state/` and
  `.claude/settings.local.json`. The searched set is therefore a close approximation of the tracked
  set, but it is **not identical**: it also includes any untracked-and-unignored file, and it cannot
  distinguish "tracked" from "present on disk".
- R5 asked that each candidate command be executed and its exit code recorded. That was not possible.
  Section 5 gives the command forms with their documented exit-code semantics and marks each one
  **NOT EXECUTED — must be run and recorded by the executor**. This is the single largest gap in this
  research and the plan must not treat the section-5 outputs as pre-verified.

Where a measurement below is stated with a number, that number was produced by a tool call in this
session against the working tree on 2026-08-29 and is reproducible with the tools named.

---

## 1. R1 — the twelve identifiers

### 1.1 The list, verbatim

Extracted from `docs/features/active/qfc-collection-controller-defects-468/evidence/other/p1-t1-reflective-caller-search.2026-08-26T08-25.md`
lines 25-28 (the search-(a) command) and cross-checked against the per-identifier baseline table in
`docs/features/active/qfc-collection-controller-defects-468/evidence/baseline/p0-t15-source-facts.2026-08-26T08-25.md`
lines 102-116.

| # | Identifier as searched by AC-16 | Removed member's exact name | Declaration line at baseline | Accessibility |
|---|---|---|---|---|
| 1 | `WireUpKeyboardHandler` | `WireUpKeyboardHandler` | `:1254` | `public void` |
| 2 | `AnyOpenDropDownsAsync` | `AnyOpenDropDownsAsync` | `:1324` | `internal async Task<bool>` |
| 3 | `LoadGroups_02cAsync` | `LoadGroups_02cAsync` | `:587` | `public async Task` |
| 4 | `LoadGroups_02bAsync` | `LoadGroups_02bAsync` | `:635` | `public async Task` |
| 5 | `LoadGroup_03bAsync` | `LoadGroup_03bAsync` | `:654` | `private async Task<QfcItemGroup>` |
| 6 | `LoadConversationsAndFoldersAsync` | `LoadConversationsAndFoldersAsync` | `:761` | `public async Task` |
| 7 | `LoadItemGroup` (**bare stem**) | `LoadItemGroup` | `:776` | `internal async Task` |
| 8 | `LoadSequentialAsync` | `LoadSequentialAsync` | `:827` | `public async Task` |
| 9 | `LoadGroupSequential` | `LoadGroupSequential` | `:842` | `public async Task` |
| 10 | `CacheTlpForMove` | `CacheTlpForMove` | `:865` | `internal void` |
| 11 | `SwapTlp` | `SwapTlp` | `:870` | `internal void` |
| 12 | `CaptureTlpTemplate` | `CaptureTlpTemplate` | `:1991` | `internal void` |

All line numbers are into `QuickFiler/Controllers/QfcCollectionController.cs` **at the #468 base
commit**, per the P0-T15 table. The file is now 1,140 lines shorter and those numbers no longer
resolve.

### 1.2 Identifier 7 — stem versus member name

`docs/features/active/qfc-collection-controller-defects-468/evidence/other/p1-t1-reflective-caller-search.2026-08-26T08-25.md:39-41`
records that search (a) used the bare stem `LoadItemGroup`, deliberately broader than the removed
member. Reporting them separately as required:

- **Removed member's exact name:** `LoadItemGroup` — the whole identifier. The narrowing device the
  #468 plan used was not a different name but the **parenthesised form `LoadItemGroup(`**, which
  distinguishes the dead member from the live `LoadItemGroupsAndViewers_02`. Source:
  `docs/features/active/qfc-collection-controller-defects-468/evidence/baseline/p0-t15-source-facts.2026-08-26T08-25.md:148-151`.
- **Stem as searched:** `LoadItemGroup` without the parenthesis, which additionally matches the
  **live, preserved** member `LoadItemGroupsAndViewers_02`.

Both forms are measured separately in section 2.

### 1.3 A thirteenth removed member the AC-16 sweep did not include

The #468 removal was **twelve methods plus one private field**. The field is `_templateTlp`:

- `docs/features/active/qfc-collection-controller-defects-468/spec.md:334` — "Twelve members plus the
  field `_templateTlp` (`:70`)".
- The removal commit subject is
  `fix(468): remove unreachable load paths and the dead _templateTlp field`, commit `63eebd47`
  (`docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/p1-t9-commit.2026-08-26T08-45.md:23`).

`_templateTlp` was included in the AC-16 **corroborating scoped sweep** (line 171 of the AC-16
artifact) but **not** in search (a), the build-input-file-type search. That is a real gap, and it is
the gap that matters most, because the only name-based mechanism that actually exists in this
repository is field reflection (section 4). **The #635 sweep must search thirteen identifiers, not
twelve.** This is recorded as a discrepancy between the AC-16 list and what was actually removed.

### 1.4 Discrepancy check against the current tree

The removal is confirmed present. A content search for all thirteen identifiers restricted to the
`QuickFiler` production tree returns exactly two lines, both belonging to the **live** member:

```
QuickFiler/Controllers/QfcCollectionController.cs:344:            LoadItemGroupsAndViewers_02(listMailItems, template);
QuickFiler/Controllers/QfcCollectionController.cs:669:        public void LoadItemGroupsAndViewers_02(IList<MailItem> items, RowStyle template)
```

Both match only the broad stem `LoadItemGroup`. No declaration or call of any of the twelve removed
methods, and no occurrence of `_templateTlp`, survives anywhere in the `QuickFiler` production tree.

**Unverified by this session:** the commit-level proof (`git show 63eebd47 --stat`) that exactly
these thirteen members were removed in one commit. The evidence above is second-hand (the #468
evidence artifacts) plus the current-tree absence. The plan should include a `git show` step.

---

## 2. R2 — the satisfiability constraint, quantified

### 2.1 The AC-16 assertion

`docs/features/active/qfc-collection-controller-defects-468/evidence/other/p1-t1-reflective-caller-search.2026-08-26T08-25.md:219-236`
states that a repository-wide zero-hit condition is unsatisfiable by construction, for two reasons:
(1) `LoadSequentialAsync` names live unrelated members under `TaskMaster/AppGlobals/`; (2)
`docs/features/**` quotes every one of the identifiers. **Both reasons are confirmed against the
current tree, and both are now larger than AC-16 recorded.**

### 2.2 Reason 1 — live, unrelated same-named members

`LoadSequentialAsync` is the only one of the thirteen that names a live member of an unrelated type.
Current declarations, with line numbers verified in this session:

| Declaring file | Line | Declaration |
|---|---|---|
| `TaskMaster/AppGlobals/ApplicationGlobals.cs` | `:144` | `public async Task LoadSequentialAsync()` |
| `TaskMaster/AppGlobals/AppToDoObjects.cs` | `:63` | `public async Task LoadSequentialAsync()` |
| `TaskMaster/AppGlobals/AppAutoFileObjects.cs` | `:84` | `public async Task LoadSequentialAsync()` |

**Drift from AC-16:** the AC-16 artifact (line 227) cites `ApplicationGlobals.cs:139`. The current
line is `:144`, five lines lower. `AppToDoObjects.cs:63` and `AppAutoFileObjects.cs:84` are unchanged.

**Out-of-file `.cs` hit count for `LoadSequentialAsync`:** the complete set of `.cs` lines outside
`QuickFiler/Controllers/QfcCollectionController.cs` is 28 (AC-16 recorded 27):

| File | Lines |
|---|---|
| `TaskMaster/ThisAddIn.cs` | `:49` (comment) |
| `TaskMaster/AppGlobals/ApplicationGlobals.cs` | `:84` (call), `:144` (declaration) |
| `TaskMaster/AppGlobals/AppToDoObjects.cs` | `:41` (call), `:63` (declaration) |
| `TaskMaster/AppGlobals/AppAutoFileObjects.cs` | `:60` (call), `:84` (declaration) |
| `TaskMaster/AppGlobals/StartupDiagnosticsProbe.cs` | `:9`, `:95`, `:164` (XML doc) |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperInitClock.cs` | `:16` (XML doc) |
| `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` | `:187`, `:200`, `:226`, `:238`, `:253`, `:274`, `:298`, `:303`, `:334`, `:358` |
| `TaskMaster.Test/AppGlobals/ContinuationProbeSequenceTests.cs` | `:14`, `:27`, `:43`, `:122` |
| `TaskMaster.Test/AppGlobals/TestableApplicationGlobals.cs` | `:11`, `:95` |
| `TaskMaster.Test/AppGlobals/ApplicationGlobalsStartupTimingTests.cs` | `:322` |

**No other identifier of the thirteen has a same-named live member in an unrelated type.** The only
other code collision is intra-file and intra-type: the stem `LoadItemGroup` is a strict prefix of the
live `QfcCollectionController.LoadItemGroupsAndViewers_02` (2 hits, section 1.4). Verified by an
exhaustive `.cs` sweep for all thirteen identifiers, which returns exactly 31 lines across 12 files —
the 28 above, the 2 live-member lines, and one XML doc comment described in section 2.4.

### 2.3 Reason 2 — prose and machine-generated evidence

Repository-wide occurrence counts on 2026-08-29, per identifier (ripgrep matching-line counts over
the non-ignored working tree):

| # | Identifier | Occurrences | Files |
|---|---|---|---|
| 1 | `WireUpKeyboardHandler` | 184 | 47 |
| 2 | `AnyOpenDropDownsAsync` | 84 | 32 |
| 3 | `LoadGroups_02cAsync` | 59 | 30 |
| 4 | `LoadGroups_02bAsync` | 65 | 30 |
| 5 | `LoadGroup_03bAsync` | 148 | 30 |
| 6 | `LoadConversationsAndFoldersAsync` | 86 | 34 |
| 7a | `LoadItemGroup` (stem) | 132 | 36 |
| 7b | `LoadItemGroup(` (parenthesised) | 14 | 5 |
| 8 | `LoadSequentialAsync` | **1331** | **200** |
| 9 | `LoadGroupSequential` | 83 | 30 |
| 10 | `CacheTlpForMove` | 44 | 33 |
| 11 | `SwapTlp` | 43 | 32 |
| 12 | `CaptureTlpTemplate` | 41 | 32 |
| 13 | `_templateTlp` | 27 | 10 |

A single alternation over all thirteen returns **2,259 matching lines across 221 files** (a line
matching several identifiers is counted once, which is why this is less than the column sum).

Breakdown by top-level directory:

| Top-level location | Matching lines | Files |
|---|---|---|
| `docs/` | 2,216 | 205 |
| `.claude/` | 12 | 4 |
| First-party `.cs` source (`QuickFiler/`, `QuickFiler.Test/`, `TaskMaster/`, `TaskMaster.Test/`, `UtilitiesCS/`) | 31 | 12 |
| Every other tracked location (`.github/`, `.agents/`, `.codex/`, `scripts/`, `tests/`, repo root, all non-`.cs` files anywhere outside `docs/` and `.claude/`) | **0** | **0** |

The four `.claude/` files are all agent-memory prose:
`.claude/agent-memory/atomic-planner/project_468_qfc_collection_controller_plan_seams.md` (5),
`.claude/agent-memory/atomic-executor/project_preflight_zerohit_identifier_and_red_test_straddle.md` (3),
`.claude/agent-memory/task-researcher/project_qfc254_residual_after_comexception_fix.md` (2),
`.claude/agent-memory/atomic-planner/project_211_startup_lifetime_heartbeat_seam.md` (2).

Even the **narrow** form `LoadItemGroup(` is non-zero repository-wide: 14 occurrences across 5 files,
all of them prose —
`docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md` (2),
`.../evidence/qa-gates/p1-t3-dead-identifier-sweep.2026-08-26T08-45.md` (3),
`.../evidence/other/p1-t1-reflective-caller-search.2026-08-26T08-25.md` (4),
`.../evidence/baseline/p0-t15-source-facts.2026-08-26T08-25.md` (4),
`.claude/agent-memory/atomic-planner/project_468_qfc_collection_controller_plan_seams.md` (1).

**Conclusion:** an acceptance condition of the form "zero hits repository-wide" is unsatisfiable for
every one of the thirteen identifiers, including the parenthesised narrow form, and including the
identifier with the smallest footprint (`_templateTlp`, 27 hits). The AC-16 judgment was correct and
remains correct. The constraint has grown, not shrunk: `LoadSequentialAsync` alone is now at 1,331
occurrences across 200 files, and `docs/features/**` continues to accrete evidence artifacts that
quote the names.

### 2.4 The one new code-tree hit since AC-16

AC-16 recorded **zero** hits in `QuickFiler.Test`. There is now one:

```
QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs:60:
        /// Issue #444 decision pin. Upstream #468 deleted the dead <c>WireUpKeyboardHandler</c>
```

It is inside a `///` XML documentation comment (verified by reading lines 59-65 of that file). It is
not a string literal, is not compiled into the assembly's metadata as a member name, and cannot be
passed to any reflection API. It is a **category C** hit under the rule below. Its existence is the
concrete demonstration that the AC-16 statement "zero hits anywhere in `QuickFiler.Test`" is a
time-bound measurement, not an invariant, and that the #635 acceptance condition must be robust to
this class of hit rather than assert zero.

### 2.5 Proposed classification rule (mechanical, not judgment-based)

Do **not** write the acceptance condition as a count. Write it as a total classification with one
empty class.

> **AC form.** Every hit produced by the thirteen-identifier sweep is assigned to exactly one of the
> categories A-G below by applying the tests in order, and category **G is empty**.

| Cat | Name | Mechanical test | Currently |
|---|---|---|---|
| **A** | Self-file | Path is `QuickFiler/Controllers/QfcCollectionController.cs`. | 2 lines |
| **B** | Live unrelated same-named member | Path is under `TaskMaster/`, `TaskMaster.Test/`, or `UtilitiesCS/` **and** the matched identifier is `LoadSequentialAsync`; **or** the matched text is a strict superstring of the identifier (stem collision, e.g. `LoadItemGroupsAndViewers_02` for stem `LoadItemGroup`). | 28 lines |
| **C** | Comment or XML doc inside code | Path ends `.cs` **and** the matched line's first non-whitespace token is `//` or `///`, or the line lies inside a `/* … */` block. | 1 line |
| **D** | Authored documentation prose | Path begins `docs/`. | see F |
| **E** | Agent-memory prose | Path begins `.claude/`. | 12 lines |
| **F** | Machine-generated historical evidence | Path begins `docs/` **and** extension is `.trx`, `.xml`, `.cobertura.xml`, or `.txt`, **and** the path contains `/evidence/`. Subset of D, separated because it is generated, not authored, and cannot be edited to remove the name. | D+F = 2,216 lines |
| **G** | **Genuine name-based caller** | Anything not matched by A-F. Concretely: a member-name **string literal** equal to one of the thirteen; **or** a reflection call site whose receiver is `typeof(QfcCollectionController)` or a `QfcCollectionController` instance and whose member-name argument can take one of the thirteen values; **or** an MSBuild / `.resx` / `.config` / `.settings` / `.xaml` token naming one of the thirteen. | **0** |

**Closure argument for variable-valued reflection arguments.** Category G's second clause is not
directly decidable by grep because some reflection call sites pass a `string name` parameter rather
than a literal (section 4.3). It becomes decidable by this closure step, which the plan must state
explicitly as a task:

> For every reflection call site whose receiver is `QfcCollectionController`, the member-name
> argument is either (i) a string literal, enumerated and compared against the thirteen; or (ii) a
> variable. In case (ii), the set of values that variable can take is bounded by the string literals
> present in the source text of the assemblies that call it. Since the thirteen identifiers occur in
> `QuickFiler.Test` source text exactly once, inside a `///` comment (category C), and occur nowhere
> in `QuickFiler` production source text except the two live-member lines (category A), no call site
> can supply one.

**Residual not closed by that argument:** a member name assembled at runtime by string
concatenation or interpolation. No such construction was observed at any of the call sites reviewed
in section 4, but the search did not attempt to prove its absence in general. The plan should record
this as a stated limit of the method rather than claim it away. The practical mitigation is already
in place: the `QfcCollectionController` test-support helpers assert `.Should().NotBeNull(...)` on the
resolved `FieldInfo`/`MethodInfo` (`QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs`
lines 39-41, 52-53, 66-67, 81-82, 96-97, 119-121), so an unresolvable name fails the test loudly.

---

## 3. R3 — repository file-type census and the scope delta

### 3.1 What was measured and what was not

**UNMEASURED (requires `git ls-files`, no shell available):** total tracked file count; tracked `.cs`
count; tracked non-`.cs` count; the full per-extension breakdown with counts. Section 5 supplies the
exact command forms; the executor must run them and record the numbers. These figures are a
**non-vacuity measurement**, not a risk: the widened search's *result* is already known (section 3.3).

**MEASURED:** the extension inventory of the `QuickFiler` and `QuickFiler.Test` trees, exhaustively;
and the existence of extension classes elsewhere in the repository that AC-16's include-list did not
cover.

### 3.2 Non-`.cs` files in the two trees of interest

Exhaustive, by directory enumeration:

| Path | Extension | In AC-16's six? |
|---|---|---|
| `QuickFiler/QuickFiler.csproj` | `.csproj` | yes |
| `QuickFiler/app.config`, `QuickFiler/packages.config` | `.config` | yes |
| 12 `.resx` files under `QuickFiler/Viewers/`, `QuickFiler/Legacy/`, `QuickFiler/Properties/` | `.resx` | yes |
| `QuickFiler/Properties/Settings.settings` | `.settings` | yes |
| `QuickFiler/FodyWeavers.xml` | `.xml` | **no** |
| `QuickFiler/FodyWeavers.xsd` | `.xsd` | **no** |
| `QuickFiler/QuickFiler.csproj.bak` | `.bak` | **no** |
| `QuickFiler/Notes/Item control hierarchy.txt` | `.txt` | **no** |
| `QuickFiler/Notes/notes_interface_hierarchy` | **(no extension)** | **no** |
| `QuickFiler.Test/QuickFiler.Test.csproj` | `.csproj` | yes |
| `QuickFiler.Test/app.config`, `QuickFiler.Test/packages.config` | `.config` | yes |
| `QuickFiler.Test/QuickFiler.Test.csproj.bak` | `.bak` | **no** |

Seven files in the two trees of interest were outside AC-16's include-list. Two of them are the most
interesting for this item, because they are prose kept *inside the production tree* rather than under
`docs/`: `QuickFiler/Notes/Item control hierarchy.txt` and the extensionless
`QuickFiler/Notes/notes_interface_hierarchy`. The latter does mention `IQfcCollectionController`
(line 9) but none of the thirteen identifiers.

Whether `*.csproj.bak` is *tracked* is **UNMEASURED**; `<repo-root>/.gitignore` does not ignore
`*.bak` (only `*.rptproj.bak` at line 252), and the file was searched, so it is at minimum present
and unignored.

### 3.3 Extension classes elsewhere in the repository outside AC-16's include-list

Confirmed present by enumeration, none of them covered by AC-16's six extensions:

- **`.ps1`** — at least 89 files across `.claude/hooks/`, `.codex/hooks/`, `.codex/scripts/`,
  `scripts/dev-tools/`, `scripts/vscode/`, `tests/scripts/vscode/`. AC-16 excluded `.claude/` by
  directory, but `scripts/` and `tests/` were inside its directory scope and were simply not reached
  by its `--include` list.
- **`.yml` / `.yaml`** — e.g. `.github/dependabot.yml`, `.agents/skills/codex-model-routing/agents/openai.yaml`.
- **`.md`** — the whole of `.github/agents/`, `.github/instructions/`, `.github/prompts/`,
  `.github/skills/`, `.agents/skills/`, plus repository-root markdown. AC-16 excluded `docs/` and
  `.claude/`, but `.github/` and `.agents/` markdown was inside its directory scope and outside its
  extension list.
- **`.xml`, `.xsd`, `.bak`, `.txt`, extensionless** — as in section 3.2.
- **`.sln`** and any `.props` / `.targets` / `.runsettings` / `.editorconfig` / `.globalconfig` at
  repository root — presence not individually confirmed in this session for each; the `.sln` is
  referenced throughout `CLAUDE.md` as `TaskMaster.sln`.

### 3.4 The delta, stated plainly

**Delta in files:** UNMEASURED as an exact count. The command that produces it is in section 5.2.
Qualitatively it is **large** — the widened search adds, at minimum, every `.ps1`, `.md`, `.yml`,
`.yaml`, `.xml`, `.xsd`, `.txt`, `.bak`, `.sln` and extensionless tracked file in the repository,
which across `.github/`, `.agents/`, `.codex/`, `scripts/`, `tests/` and the project trees is several
hundred files. AC-16's measured scope was 398 files over six extensions.

**Delta in findings: zero.** This is the substantive result of R3 and it was measured. A
thirteen-identifier alternation over the entire non-ignored working tree returns **no hit in any
non-`.cs` file outside `docs/` and `.claude/`** — not in `.ps1`, not in `.yml`, not in `.md` under
`.github/` or `.agents/`, not in `.xml`, `.xsd`, `.txt`, `.bak`, or the extensionless file. Every one
of the 2,228 non-`.cs` hits is prose or generated evidence under `docs/` (2,216) or `.claude/` (12).

So the widened search is worth performing — it converts an unproven scope assumption into a measured
one — but it is not expected to change the disposition. The plan should say so up front and should
budget for the measurement, not for a remediation.

---

## 4. R4 — reflection entry-point inventory in the `QuickFiler` tree

Scope: `QuickFiler/**` (production) and `QuickFiler.Test/**`. `bin/` and `obj/` are `.gitignore`d and
were therefore never in the searched set — the AC-16 post-filter `grep -v "/bin/\|/obj/"` is
unnecessary under `git grep` and under ripgrep alike, and the plan can drop it.

### 4.1 Production tree — `QuickFiler/**`

| Pattern | Hits in `QuickFiler` production tree |
|---|---|
| `GetMethod(` | **0** |
| `GetMethods(` | **0** |
| `GetMember(` / `GetMembers(` | **0** |
| `GetProperty(` / `GetProperties(` | **0** |
| `GetField(` / `GetFields(` | **0** |
| `GetEvent(` / `GetEvents(` | **0** |
| `InvokeMember(` | **0** |
| `Type.GetType(` | **0** |
| `Activator.CreateInstance` | **0** |
| `Assembly.CreateInstance` / `Assembly.Load` | **0** |
| `Delegate.CreateDelegate` / `CreateDelegate` | **0** |
| `Expression.Call` | **0** |
| `CallByName` | **0** |
| `MethodInfo` / `PropertyInfo` / `FieldInfo` (as declared types) | **0** |
| `FormatterServices` | **1**, and it is an XML doc `<see cref="…"/>` at `QuickFiler/Controllers/QfcDatamodel.cs:123` — not a call |
| `dynamic <identifier>` declarations | **0** (one occurrence of the word in a comment, `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:10`) |
| `using System.Reflection;` | **3** — `QuickFiler/Controllers/EfcHomeController.cs:5`, `QuickFiler/Properties/AssemblyInfo.cs:1`, `QuickFiler/Legacy/QfcController.cs:8` |
| `System.Reflection.MethodBase.GetCurrentMethod().DeclaringType` | ~26, one per class, the log4net logger-declaration idiom |

The three `using System.Reflection;` directives were checked. `EfcHomeController.cs` uses it only for
`MethodBase.GetCurrentMethod().DeclaringType` at `:21`. `AssemblyInfo.cs` uses it for the
`[assembly: Assembly*]` attributes. `MethodBase.GetCurrentMethod()` takes no member-name argument and
cannot resolve a member by name.

**Finding: the `QuickFiler` production assembly performs no name-based member resolution of any
kind.** This reproduces AC-16's search-(b) conclusion for the production tree and extends it from two
patterns to the full list.

### 4.2 Designer, resource, and serialization surfaces

- **`*.Designer.cs`:** present (approximately 20 files under `QuickFiler/Viewers/`,
  `QuickFiler/Legacy/`, `QuickFiler/Properties/`). None contains any of the thirteen identifiers —
  the exhaustive `.cs` sweep in section 2.2 returns 31 lines and none is in a Designer file.
- **`*.resx`:** 12 files in the production tree. None contains any of the thirteen identifiers. The
  only serialization-looking token in them is the standard
  `System.Runtime.Serialization.Formatters.Binary.BinaryFormatter` `resheader` on line 49 of each
  file, which is `.resx` schema boilerplate, not a member reference. No `.resx` names
  `QfcCollectionController` at all.
- **Data binding:** `DataBindings.Add`, `DisplayMember`, `ValueMember`, `DataPropertyName` — **0**
  hits in the entire `QuickFiler` production tree. There is no property-name-string binding surface.
- **Serialization attributes:** `[Serializable]`, `DataContract`, `JsonProperty`, `XmlElement` — **0**
  hits in the entire `QuickFiler` production tree. `QfcCollectionController` carries no serialization
  surface.
- **COM visibility:** `QuickFiler/Properties/AssemblyInfo.cs:22` declares
  `[assembly: ComVisible(false)]`. No type in the assembly is registered for COM, so no `IDispatch`
  late-binding path (VBA `CallByName`, `Application.Run`, Outlook macro) can reach
  `QfcCollectionController` by name. This is a decisive negative for the whole class of host-side
  name-based callers and it is the argument AC-16 did not make.

### 4.3 Test tree — `QuickFiler.Test/**`

| Pattern | Hits | Note |
|---|---|---|
| `GetMethod(` | **69** across 31 files | AC-16 measured **42** on 2026-08-26; the tree has moved |
| `GetField(` | **172** across 65 files | **AC-16 did not search this pattern at all** |
| `GetProperty(` | ~20 | none targets `QfcCollectionController` |
| `GetMember(` | 5, all `ItemViewer` / `IItemViewer` / `WebView2BreadcrumbHost` | |
| `GetMethods(` | 4 (`EfcItemController`, `EfcViewer`, `WebView2BreadcrumbHost`, a dispatcher test) | |
| `GetFields(` | 2 (`BreadcrumbDropDownHost`, `WebView2BreadcrumbHost`) | |
| `GetEvent(` | ~9, all breadcrumb / `IItemViewer` types | |
| `Activator.CreateInstance` | 3 call sites + 1 doc comment | takes a `Type`, not a member name |
| `InvokeMember(` | **0** | |
| `Type.GetType(` | **0** | |
| `Assembly.Load` / `Assembly.CreateInstance` | **0** | |
| `CreateDelegate` | **0** | |
| `Expression.Call` | **0** | |
| `CallByName` | **0** | |
| `dynamic <identifier>` declarations | **0** | |
| `FormatterServices.GetUninitializedObject` | present, incl. `typeof(QfcCollectionController)` | constructs an instance; takes no member name |

**The headline finding of R4.** A name-based mechanism that can reach a `QfcCollectionController`
member **does exist**, and AC-16 did not enumerate it, because AC-16 searched only `GetMethod(` and
`InvokeMember(`. The call sites whose receiver is `QfcCollectionController` are:

| Site | Form | Member-name argument |
|---|---|---|
| `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs:38` | `typeof(QfcCollectionController).GetField(name, NonPublicInstance)` | **variable** `name` |
| `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs:51` | same | **variable** `name` |
| `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs:65` | same | **variable** `name` |
| `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs:80` | `…GetField(name, NonPublicStatic)` | **variable** `name` |
| `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs:95` | same | **variable** `name` |
| `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs:118` | `typeof(QfcCollectionController).GetMethod(name, NonPublicInstance)` then `.Invoke` at `:122` | **variable** `name` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs:34` | `typeof(QfcCollectionController).GetField(name, …)` | **variable** `name`; the only observed call passes `"_kbdHandler"` (`:74`) |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:70` | `.GetField("_itemGroupsToMove", …)` | literal |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:168`, `:497` | `.GetField("_itemGroups", …)` | literal |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:179`, `:263` | `.GetField("_removeGroupByEntryId", …)` | literal |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:382` | `.GetField(name, …)` | **variable** `name` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs:77` | `.GetField("_itemGroups", …)` | literal |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs:44`, `:66`, `:86` | `.GetField(ReentrancyCounterField, NonPublicStatic)` | named constant |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs:44`, `:45` | `controller.GetMethod("ResolveConversationInsertions" \| "ReconcileInsertionCount", AnyStatic)` | literals |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs:296` and `…ConversationTests.cs:331` | `QfcCollectionControllerTestSupport.GetField(controller, "_itemGroupsToMove" \| "_digitRefreshNeeded")` | literals |

Every literal is enumerated above and **none is one of the thirteen**. Every variable site is closed
by the argument in section 2.5: the thirteen identifiers occur in `QuickFiler.Test` source text
exactly once, inside a `///` comment, so no call site can supply one.

Two consequences the plan must carry:

1. **The AC-16 search-(b) per-hit table is stale and its scope was too narrow.** It enumerated 42
   `GetMethod(` hits (now 69) and asserted "the `QuickFiler` production assembly performs no
   reflective method lookup" — true for production, but it did not cover `GetField(` (172 hits) and
   did not observe that `QfcCollectionController.TestSupport.cs` resolves `QfcCollectionController`
   methods by variable name. The #635 enumeration is therefore not redundant work.
2. **The mechanism exists but is inert with respect to the removal.** Field reflection against
   `QfcCollectionController` is used routinely; the removed field `_templateTlp` is exactly the kind
   of member such a call site could name. It does not, and the closure argument shows it cannot.

---

## 5. R5 — executable command forms under the executor's tool allow-list

The `atomic-executor` allow-list permits `Bash(git *)`, `Bash(pwsh *)`, `Bash(poetry run *)`,
`Bash(npx *)`. Bare `grep`, `rg`, `find`, `wc`, `sed`, `awk` as the leading command are not permitted.
All forms below satisfy that constraint.

**Every command in this section is NOT EXECUTED.** No shell was available in this research session
(section 0). The exit-code semantics stated are from documented tool behaviour, not from an observed
run. The executor must run each one and record `Command:`, `EXIT_CODE:`, and verbatim output.

### 5.1 Repository-wide identifier sweep over tracked non-`.cs` files

```
git grep -n -I -F \
  -e WireUpKeyboardHandler -e AnyOpenDropDownsAsync -e LoadGroups_02cAsync \
  -e LoadGroups_02bAsync -e LoadGroup_03bAsync -e LoadConversationsAndFoldersAsync \
  -e LoadItemGroup -e LoadSequentialAsync -e LoadGroupSequential \
  -e CacheTlpForMove -e SwapTlp -e CaptureTlpTemplate -e _templateTlp \
  -- ":(exclude)*.cs"
```

Predicted result: a large hit list, all under `docs/` and `.claude/`. Predicted `EXIT_CODE: 0`.

The classification-critical companion, which subtracts the two prose corpora:

```
git grep -n -I -F \
  -e WireUpKeyboardHandler -e AnyOpenDropDownsAsync -e LoadGroups_02cAsync \
  -e LoadGroups_02bAsync -e LoadGroup_03bAsync -e LoadConversationsAndFoldersAsync \
  -e LoadItemGroup -e LoadSequentialAsync -e LoadGroupSequential \
  -e CacheTlpForMove -e SwapTlp -e CaptureTlpTemplate -e _templateTlp \
  -- ":(exclude)*.cs" ":(exclude)docs/*" ":(exclude).claude/*"
```

Predicted result: no output. Predicted `EXIT_CODE: 1`.

**Exit-code semantics — load-bearing.** `git grep` exits **0** when at least one line matches, **1**
when none matches, and **>1** on error. The second command is the one whose success is a *zero-hit*
condition, so its evidence artifact must declare `ExpectedExitCode: 1` per
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md` lines 113-124. Declaring
`ExpectedExitCode: 0`, or omitting the field, will normalise a correct zero-hit run to `fail`.

`-I` suppresses binary files, which prevents `Binary file … matches` lines from polluting a hit table
that the plan then requires to be enumerated line by line.

Use the `":(exclude)…"` pathspec-magic form rather than the `":!…"` shorthand. Both are valid git
pathspecs; the long form avoids any interaction with `!` in a shell that has history expansion
enabled.

### 5.2 Scope-size measurement (non-vacuity)

```
pwsh -NoProfile -Command "(git ls-files).Count"
pwsh -NoProfile -Command "(git ls-files -- '*.cs').Count"
pwsh -NoProfile -Command "(git ls-files -- ':(exclude)*.cs').Count"
```

Per-extension breakdown of tracked non-`.cs` files, including files with no extension:

```
pwsh -NoProfile -Command "git ls-files -- ':(exclude)*.cs' | ForEach-Object { $e = [System.IO.Path]::GetExtension($_); if ([string]::IsNullOrEmpty($e)) { '(none)' } else { $e.ToLowerInvariant() } } | Group-Object | Sort-Object Count -Descending | ForEach-Object { '{0,6}  {1}' -f $_.Count, $_.Name }"
```

The R3 delta — files a widened all-file-types search adds beyond AC-16's six extensions:

```
pwsh -NoProfile -Command "(git ls-files -- ':(exclude)*.cs' ':(exclude)*.csproj' ':(exclude)*.resx' ':(exclude)*.config' ':(exclude)*.xaml' ':(exclude)*.json' ':(exclude)*.settings').Count"
```

**`$LASTEXITCODE` semantics — load-bearing.** Inside `pwsh -Command`, `$LASTEXITCODE` is set by the
last *native* command that ran. In `(git ls-files …).Count` the last native command is `git`, which
exits 0, so `$LASTEXITCODE` is 0. But the **process** exit code of `pwsh` is determined by the script
block's own completion, not by `$LASTEXITCODE`: unless the command string ends with
`exit $LASTEXITCODE`, a `pwsh -Command` wrapper around a `git grep` that exited 1 **still returns
process exit code 0**. A plan that wraps a zero-hit `git grep` in a pwsh counting pipeline and then
asserts `EXIT_CODE: 1` will be wrong. Two safe patterns:

- Assert the **count**, not the exit code, and let the wrapper exit 0:
  `pwsh -NoProfile -Command "(git grep -n -I -F -e … -- ':(exclude)*.cs' ':(exclude)docs/*' ':(exclude).claude/*' | Measure-Object -Line).Lines"` → expected output `0`, `EXIT_CODE: 0`.
- Or run bare `git grep` and declare `ExpectedExitCode: 1`.

Do not mix the two. Pick one per artifact, because `ExpectedExitCode` is per-file, not per-gate
(`.claude/skills/evidence-and-timestamp-conventions/SKILL.md:122`).

### 5.3 Reflection entry-point enumeration

Full inventory in one command:

```
git grep -n -I -E "GetMethods?\(|GetMembers?\(|GetPropert(y|ies)\(|GetFields?\(|GetEvents?\(|InvokeMember\(|Type\.GetType\(|Activator\.CreateInstance|Assembly\.(Load|CreateInstance)|CreateDelegate|Expression\.Call|CallByName|System\.Reflection|MethodInfo|PropertyInfo|FieldInfo|EventInfo|MemberInfo|FormatterServices|BinaryFormatter|DataBindings\.Add|DisplayMember|ValueMember|DataPropertyName|Serializable|DataContract|JsonProperty|XmlElement" \
  -- "QuickFiler/*" "QuickFiler.Test/*"
```

Per-pattern counts, production tree and test tree separated:

```
pwsh -NoProfile -Command "@('GetMethod(','GetMethods(','GetMember(','GetMembers(','GetProperty(','GetProperties(','GetField(','GetFields(','GetEvent(','GetEvents(','InvokeMember(','Type.GetType(','Activator.CreateInstance','Assembly.CreateInstance','Assembly.Load','CreateDelegate','Expression.Call','CallByName','MethodInfo','PropertyInfo','FieldInfo','FormatterServices','BinaryFormatter','using System.Reflection') | ForEach-Object { $p = $_; $prod = (git grep -n -I -F -e $p -- 'QuickFiler/*' | Measure-Object -Line).Lines; $test = (git grep -n -I -F -e $p -- 'QuickFiler.Test/*' | Measure-Object -Line).Lines; '{0,-28} prod={1,-5} test={2}' -f $p, $prod, $test }"
```

The receiver-scoped closure query that decides category G:

```
git grep -n -I -F -e "typeof(QfcCollectionController)" -- "QuickFiler.Test/*"
```

The narrower zero-hit assertion inside the two trees (this is the assertion that must be **0**):

```
pwsh -NoProfile -Command "(git grep -n -I -F -e WireUpKeyboardHandler -e AnyOpenDropDownsAsync -e LoadGroups_02cAsync -e LoadGroups_02bAsync -e LoadGroup_03bAsync -e LoadConversationsAndFoldersAsync -e 'LoadItemGroup(' -e LoadSequentialAsync -e LoadGroupSequential -e CacheTlpForMove -e SwapTlp -e CaptureTlpTemplate -e _templateTlp -- 'QuickFiler/*' | Measure-Object -Line).Lines"
```

Note this uses the **parenthesised** `LoadItemGroup(` so the live `LoadItemGroupsAndViewers_02` does
not make it unsatisfiable. Expected output `0`. The same command against `'QuickFiler.Test/*'` with
the bare stem returns `1` (the `///` comment at `QfcCollectionControllerNavigationDigitsTests.cs:60`),
so if the plan wants a zero there it must either use the parenthesised form for identifier 7 **and**
exclude comment lines, or — preferably — assert the classification, not the count.

### 5.4 What `git grep` searches, and whether the exclusions weaken the claim

`git grep` without `--no-index`, `--cached`, or a tree-ish argument searches the **tracked files in
the working tree**. It therefore does not search:

- untracked files (including untracked-but-unignored ones);
- ignored files — which in this repository means `bin/`, `obj/`, `packages/`, `TestResult*/`,
  `artifacts/`, `coverage/`, `.claude/state/`, `.claude/settings.local.json`, and the various
  `*.suo` / `*.user` / `*.cache` classes listed in `<repo-root>/.gitignore`.

**Does that weaken the claim? No, for the risk being assessed.** The question is whether a *source
artifact that ships or is checked in* names a removed member. `bin/` and `obj/` contain compiled
output and intermediate copies of the very files being searched; `packages/` contains third-party
NuGet payloads that cannot reference a first-party internal member of an assembly they do not
reference; `TestResult*/`, `artifacts/` and `coverage/` contain generated run output. A hit in any of
them would be a *consequence* of the source, never a cause. Excluding them is correct and should be
stated as a deliberate scoping decision with that reason, per the AC-16 house convention (section 7).

**A supplementary untracked-file search is nevertheless warranted**, cheaply, so that the claim is
"no tracked or untracked-unignored file references a removed member" rather than "no tracked file
does". Command form:

```
pwsh -NoProfile -Command "$f = git ls-files --others --exclude-standard; if ($f) { Select-String -Path $f -SimpleMatch -Pattern 'WireUpKeyboardHandler','AnyOpenDropDownsAsync','LoadGroups_02cAsync','LoadGroups_02bAsync','LoadGroup_03bAsync','LoadConversationsAndFoldersAsync','LoadItemGroup','LoadSequentialAsync','LoadGroupSequential','CacheTlpForMove','SwapTlp','CaptureTlpTemplate','_templateTlp' } else { 'no untracked unignored files' }"
```

`git ls-files --others --exclude-standard` lists exactly the untracked-and-unignored set. If the
working tree is clean this returns nothing and the supplementary search is trivially satisfied — but
recording the empty list is itself the non-vacuity proof for that half of the claim.

---

## 6. R6 — whether any production source file must change

### 6.1 Answer

**The item can be completed without modifying any production source file, and on the current evidence
it should be.** Basis:

- The `QuickFiler` production assembly contains **zero** name-based member-resolution call sites of
  any kind (section 4.1), carries **no** serialization, data-binding, or COM-visible surface
  (section 4.2), and is `[assembly: ComVisible(false)]`.
- The only reflection surface that names `QfcCollectionController` members is in `QuickFiler.Test`,
  and every literal it passes is enumerated and is not one of the thirteen; every variable it passes
  is bounded by source text that contains none of the thirteen (sections 4.3, 2.5).
- No non-`.cs` file anywhere outside `docs/` and `.claude/` contains any of the thirteen identifiers
  (section 2.3).
- The single new code-tree occurrence since AC-16 is an XML doc comment (section 2.4).

The expected output of this item is therefore **evidence artifacts and documents only**: the widened
sweep, the census, the reflection inventory, the classification table, and a recorded decision.

### 6.2 What would have to be true for that to become false

Exactly one thing: category **G** of the section-2.5 classification would have to be non-empty —
a string literal equal to one of the thirteen reaching a reflection API whose receiver is
`QfcCollectionController`, or an MSBuild/`.resx`/`.config`/`.settings`/`.xaml` token naming one. On
the measurements in this document, G is empty. If the executor's `git ls-files`-backed sweep finds a
hit that section 2.5's tests do not place in A-F, that hit is by definition a category-G candidate and
must be read in full before disposition.

### 6.3 Recommended disposition if a genuine caller is found

**Record the finding and escalate it as a separate defect. Do not fix it inside issue #635.**
Justification:

1. **The issue text stops at naming.** `issue.md:60` asks for "a recorded decision either closing the
   risk or naming the specific caller found". "Naming" is the deliverable; repair is not in the
   acceptance ideas, and neither is a regression test for a repair.
2. **The Bugfix Workflow would be violated by an in-place fix.** `CLAUDE.md` requires a failing
   regression test first, then the minimal targeted fix. A genuine name-based caller of a removed
   member manifests as a runtime `NullReferenceException` on a `MethodInfo`/`FieldInfo`, or as a
   silent no-op through a `?.SetValue`. Reproducing either deterministically is a design problem of
   its own and belongs to its own issue with its own plan.
3. **`CLAUDE.md` §"If you uncover deeper design problems, open a new issue instead of widening
   scope."** is explicit and directly on point.
4. **Repository practice requires promotion, not prose.** Out-of-scope defects discovered during a
   feature must be routed through the MCP promotion lifecycle into a real issue; a finding written
   only into a feature folder is lost at merge. The correct artefact is a potential-feature entry
   promoted to an issue, cross-referenced from the #635 decision record.

The single exception worth naming: if the found caller is *in this feature's own evidence corpus* —
that is, a false positive from a `docs/` or `.claude/` file — it is a category D/E/F hit, not
category G, and no escalation applies.

### 6.4 A planning consequence the plan must handle

Work mode is `full-bug`, which normally demands fail-before / pass-after regression evidence. **There
is no defect to reproduce.** No failing test can be written for "a search returns no genuine caller",
and writing one would be a tautology. The plan must therefore include a
**fail-before exception dossier** at
`docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/regression-testing/fail-before-exception.<ts>.md`
carrying `WhyFailingRunImpossible:` and an alternative proof section, per
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md` lines 133-143. The alternative proof is
the non-vacuity measurement itself: a measured, non-empty search scope with a fully classified hit
set. Precedent for the artefact shape exists at
`docs/features/active/qfc-collection-controller-defects-468/evidence/regression-testing/fail-before-exception.2026-08-26T16-24.md`.

---

## 7. R7 — prior-art conventions the plan must require

### 7.1 The AC-16 house style, stated concretely

Reconstructed from
`docs/features/active/qfc-collection-controller-defects-468/evidence/other/p1-t1-reflective-caller-search.2026-08-26T08-25.md`.
Each element is a plan requirement, not a suggestion:

1. **Headline first.** A bolded `## Output Summary` paragraph at the top stating the result and
   whether the task blocks (lines 11-13). A reader who stops after three lines gets the disposition.
2. **Verbatim command in a fenced block**, exactly as run, with no elision (lines 21-29).
3. **Verbatim output in a fenced block**, including `(no output)` when there is none, followed by an
   explicit statement of the exit code and what it means (lines 33-37).
4. **An explicit non-vacuity measurement** in its own subsection, with its own verbatim command and
   output, proving the search scope was non-empty and stating the per-category breakdown that makes
   it credible — AC-16 gave 398 files with a per-extension split and observed that all six declared
   extensions were represented (lines 43-66).
5. **A per-hit enumeration table** with one row per hit, each row carrying the resolved argument and
   an explicit yes/no against the question being asked (lines 95-138). Where an argument is a
   variable rather than a literal, a footnoted closure argument per variable site (lines 149-159).
6. **A distinct "decisive" corroborating search** that settles mechanically what the per-hit table
   settles by reading, so the conclusion does not rest on human review alone (lines 165-215).
7. **A stated reason for anything deliberately not searched**, in its own section, giving the
   argument rather than the assertion — AC-16's `## Why a repository-wide identifier sweep is
   deliberately NOT performed` (lines 219-236) is the model.
8. **An `## Acceptance verification` section** that walks the task's acceptance clauses one by one and
   states `Result: PASS` or otherwise (lines 240-253).

Two amendments this research requires:

- **Element 4 must now also record the delta** against a prior narrower search, not just the absolute
  scope. AC-16 measured 398 files; #635's value is entirely in the difference.
- **Element 5 must be replaced by the classification table of section 2.5** where the hit count is in
  the thousands. A 2,259-row per-hit table is not reviewable. The classification rule preserves the
  rigour of element 5 while remaining mechanical: enumerate the categories exhaustively, count each,
  and enumerate individually only the residue that lands outside A-F.

### 7.2 Repository evidence conventions

From `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`:

- **Location.** Artifacts go under `<FEATURE>/evidence/<kind>/`, where `<kind>` is one of `baseline`,
  `regression-testing`, `qa-gates`, `issue-updates`, `other`, `remediation-baseline` (lines 14-20).
  For this item: the sweep, census, and reflection inventory belong in `evidence/other/`; the
  toolchain gates in `evidence/qa-gates/`; the fail-before exception dossier in
  `evidence/regression-testing/`. Writing to `artifacts/baselines/`, `artifacts/qa/`,
  `artifacts/coverage/`, or `artifacts/evidence/` is a policy violation caught by the
  `enforce-evidence-locations.ps1` PreToolUse hook (lines 22-35).
- **Timestamps.** `yyyy-MM-ddTHH-mm`, e.g. `2026-08-29T09-15` (line 46). Filenames follow
  `<task>-<name>.<timestamp>.md`.
- **Machine-checkable schema.** Each command-step artifact carries `Timestamp:`, `Command:`,
  `EXIT_CODE:`, and — for baseline artifacts, and by house practice here for all of them —
  `Output Summary:` (lines 106-131).
- **`ExpectedExitCode:`** is optional, spelled exactly, integer-valued, first-occurrence-wins, and
  **per-file not per-gate**. A non-integer value makes the whole artifact unparseable and silently
  drops the row from the PR body (lines 113-124). Section 5.2 explains why this field is
  load-bearing for a zero-hit `git grep`, and why a gate needing `ExpectedExitCode: 1` must live in
  its own artifact file.
- **Negative claims must be auditable.** Any statement that something is absent must record
  `SearchScope:`, `SearchPatterns:`, and `SearchResult:` (lines 145-153). Every "zero hits" line in
  this item's artifacts is a negative claim and needs all three.
- **Host-identity hygiene.** No absolute host path, account name, or machine name in any artifact.
  Note that `vstest` names TRX files `<account>_<HOST>_<ts>.trx` by default, so any test artifact must
  control `/ResultsDirectory:` and `LogFileName=` or be renamed before citation. Precedent:
  `docs/features/active/qfc-collection-controller-defects-468/evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.

---

## 8. Summary of required plan content

1. Search **thirteen** identifiers, not twelve — add `_templateTlp` (section 1.3).
2. Express identifier 7 twice: bare stem `LoadItemGroup` for breadth, parenthesised `LoadItemGroup(`
   for any assertion that must reach zero (sections 1.2, 5.3).
3. Write the acceptance condition as the section-2.5 classification with category G empty. Never as
   "zero hits repository-wide" — that is unsatisfiable for all thirteen (section 2.3).
4. Produce the `git ls-files` census and the explicit delta against AC-16's 398-file six-extension
   scope (sections 3.4, 5.2). Record that the delta in *findings* is expected to be zero and say so
   before running it.
5. Enumerate the full reflection surface, including `GetField(` — the pattern AC-16 omitted and the
   only one that actually reaches `QfcCollectionController` (section 4.3).
6. Record the `[assembly: ComVisible(false)]` and no-serialization / no-data-binding findings as the
   affirmative argument that no host-side late-binding path exists (section 4.2).
7. Handle exit codes deliberately: bare `git grep` with zero matches exits 1 and needs
   `ExpectedExitCode: 1`; a `pwsh` counting wrapper exits 0 regardless and must assert the count
   instead (sections 5.1, 5.2).
8. Add a supplementary untracked-file search via `git ls-files --others --exclude-standard`
   (section 5.4).
9. Include a fail-before exception dossier; there is no reproducible defect (section 6.4).
10. Change no production source file. If category G turns out non-empty, name the caller, record the
    decision, and promote a separate issue (section 6.3).

## 9. Open items and unknowns

- **UNMEASURED:** exact tracked file counts and the per-extension breakdown (section 3.1). Commands
  supplied in 5.2.
- **UNMEASURED:** commit-level confirmation that `63eebd47` removed exactly the thirteen members
  (section 1.4). Command: `git show --stat 63eebd47` plus
  `git show 63eebd47 -- QuickFiler/Controllers/QfcCollectionController.cs`.
- **UNVERIFIED:** whether `QuickFiler/QuickFiler.csproj.bak` and
  `QuickFiler.Test/QuickFiler.Test.csproj.bak` are tracked (section 3.2). Command:
  `git ls-files -- '*.bak'`.
- **NOT EXECUTED:** every command in section 5.
- **Stated limit of method:** a member name assembled by runtime string concatenation would defeat
  the section-2.5 closure argument. None was observed; absence was not proved (section 2.5).
