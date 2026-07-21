# Research: utilitiescs-nullable-ci-capstone (Issue #376)

- Date: 2026-07-19
- Scope: Wave-2 capstone of epic `utilitiescs-nullable-remediation` (issue #376). Research only;
  no production file, workflow YAML, csproj, or `.claude/rules/*` file was modified.
- Worktree examined: `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-ac3a98310ffd36d6d`,
  branch `feature/utilitiescs-nullable-ci-capstone` (based on
  `origin/epic/utilitiescs-nullable-remediation-integration`, tip `dd17719a`).

## 0. Load-bearing precondition finding: children are PREPARED, not yet EXECUTED

Before the six lettered concerns, one cross-cutting fact changes how (b) and (d) must be read.
Grepping the worktree for the per-file opt-in pragma shows the actual remediation has **not**
been executed on this branch yet:

- `#nullable enable` (or `#nullable` at all) appears in only **25 files** under `UtilitiesCS/`
  (27 occurrences; `MailItemHelper.Html.cs` and `MeetingItemHelper.cs` each have 2) and **3 files**
  under `SVGControl/` (`ValueStringBuilder.cs`, `RelativePath.cs`, `PathInternal.cs` — the
  svgcontrol spec's own "already-clean verify-only" files, not newly remediated ones).
- Every child feature folder (`docs/features/active/utilitiescs-nullable-*/`) has a `spec.md` and
  a `plan.<timestamp>.md` with all atomic-task checkboxes unchecked (`- [ ]`), and **no**
  `evidence/`, `code-review*`, or execution-artifact directory exists under any of the twelve
  child folders (confirmed via glob: zero matches for
  `docs/features/active/utilitiescs-nullable-*/evidence/**` and
  `docs/features/active/utilitiescs-nullable-*/code-review*`).
- This means "all twelve remediation children are already fanned in" (as stated in the task
  framing) refers to their **preparation artifacts** (spec/user-story/plan, dependency graph,
  wave assignment) being merged into the integration branch — not to the underlying `.cs` file
  remediation itself. The ~234-file / ~2131-diagnostic remediation is planned but not yet
  performed anywhere in this worktree.

Consequence for this research: findings for (b) and (d) below are grounded in the pragma set that
**currently exists** (25 + 3 files, all of them pre-existing organic opt-ins predating the epic,
not epic output). The capstone's atomic plan must re-run the same greps at execution time, because
by the time the capstone actually executes (after wave-0/wave-1 children land), the opted-in file
count will be much larger. Nothing in (a) or (c) depends on the remediation being complete — the
gate-step edit and the rules-conflict flag are correct regardless of how many files are opted in.

## (a) CI gate finalization — exact minimal edit

### Current state (verified)

`.github/workflows/ci.yml`, job `Format, build, analyze, and test` (`runs-on: windows-latest`),
step **"Build with nullable warnings treated as errors"**, lines 103–115:

```yaml
      - name: Build with nullable warnings treated as errors
        shell: pwsh
        run: |
          # Use /t:Rebuild (not /t:Build) so this step always performs a genuine full
          # recompile under /p:Nullable=enable. The preceding "Build with analyzers"
          # step already compiled everything under the projects' own Nullable settings;
          # MSBuild's incremental up-to-date check does not invalidate on a changed
          # -p:Nullable command-line property alone, so a plain /t:Build here would
          # silently skip recompilation and never actually enforce this gate.
          & msbuild $env:SOLUTION_PATH /t:Rebuild /m /p:Configuration=Debug `
              "/p:Platform=Any CPU" `
              /p:Nullable=enable /p:TreatWarningsAsErrors=true
          if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
```

`UtilitiesCS/UtilitiesCS.csproj` and `SVGControl/SVGControl.csproj` were grepped for `Nullable`:
**zero matches in both** — confirmed no project-level `<Nullable>` element exists in either
project today. `UtilitiesCS.csproj` sets `<LangVersion>12.0</LangVersion>` and
`SVGControl.csproj` sets `<LangVersion>latest</LangVersion>`, both targeting
`TargetFrameworkVersion v4.8.1` — LangVersion ≥ 8 is what makes the per-file `#nullable enable`
pragma meaningful even though the project-level nullable context defaults to "disable" (oblivious)
in the absence of the `<Nullable>` element. This is standard Roslyn/MSBuild semantics: nullable
annotation/warning context is resolved per-file, and a `#nullable enable` (or
`#nullable enable annotations|warnings`) pragma establishes an enabled context for that file
regardless of the project-level default, while files without the pragma stay oblivious and never
report CS86xx.

### Proposed minimal edit

Drop `/p:Nullable=enable` from the `msbuild` invocation; keep `/t:Rebuild` and
`/p:TreatWarningsAsErrors=true` unchanged. Proposed after-text:

```yaml
      - name: Build with nullable warnings treated as errors
        shell: pwsh
        run: |
          # Use /t:Rebuild (not /t:Build) so this step always performs a genuine full
          # recompile. Enforcement now relies entirely on each file's own #nullable
          # enable pragma (the repo's per-file opt-in convention; UtilitiesCS.csproj and
          # SVGControl.csproj carry no project-level <Nullable> element) plus
          # /p:TreatWarningsAsErrors=true. MSBuild's incremental up-to-date check does
          # not invalidate on this command-line property change alone, so a plain
          # /t:Build would silently skip recompilation and never enforce this gate.
          & msbuild $env:SOLUTION_PATH /t:Rebuild /m /p:Configuration=Debug `
              "/p:Platform=Any CPU" `
              /p:TreatWarningsAsErrors=true
          if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
```

Only the one command line changes (`/p:Nullable=enable` removed) and the explanatory comment is
updated to describe the pragma-driven rationale; the `/t:Rebuild` justification (MSBuild
incremental up-to-date check does not invalidate on a changed `-p:` property) remains **fully
valid** after the edit — it was never specific to the `Nullable` property, it is a general
statement about MSBuild's up-to-date check, and it still applies verbatim to
`TreatWarningsAsErrors`. No other line in the step (exit-code handling, `shell: pwsh`, step name)
needs to change for AC1.

### Confirmation of the two enforcement claims (AC1)

1. **Opted-in files stay enforced.** Under Roslyn/MSBuild semantics, `#nullable enable` sets the
   file's nullable annotation and warning context independent of the project default. CS86xx
   diagnostics are ordinary compiler warnings once the file's context is enabled; `/t:Rebuild`
   guarantees the file is actually recompiled, and `/p:TreatWarningsAsErrors=true` promotes any
   warning (including CS86xx) emitted for that compilation to a build error, exactly as it does
   today for `EnableNETAnalyzers`/`EnforceCodeStyleInBuild` diagnostics in the preceding step. This
   requires no global flag — it already happens today with `/p:Nullable=enable` present, because
   the global flag is a strict superset (it also forces nullable on for oblivious files); removing
   it removes only the superset behavior, not the per-file behavior.
2. **Non-opted-in files stay silent.** A file with no `#nullable` pragma and no project-level
   `<Nullable>` element compiles under the "oblivious" nullable context. In an oblivious context
   the compiler does not evaluate nullable-flow rules and cannot emit CS86xx-series diagnostics
   for that file, so `/p:TreatWarningsAsErrors=true` has nothing in the CS86xx range to promote.
   This was independently confirmed by every already-prepared child's `spec.md`/`plan.md`
   (residuals #375, svgcontrol #368, dialogs-misc #374) which all specify the identical
   verification command `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true`
   (no `/p:Nullable=enable`) as their own per-child gate, and all state the same rationale
   in near-identical wording (e.g. residuals spec.md line 336: "Under `TreatWarningsAsErrors`, any
   CS86xx in a pragma-enabled file becomes an error while non-opted files stay silent."; svgcontrol
   spec.md line 263: "... becomes a build error while the un-opted-in Designer/generated files and
   any not-yet-remediated hand-authored files elsewhere in the solution stay silent."). The twelve
   children's plans were authored against this exact gate-step contract, so the capstone edit
   is not a new design — it is finalizing the same gate all twelve siblings already assumed and
   independently verified as their acceptance mechanism.

## (b) Genuine-enforcement verification — method and candidate files

This is a method/candidate-file specification for atomic-execution time, not a live run.

### Candidate opted-in file

`UtilitiesCS/OutlookObjects/Folder/PercentageFormatter.cs` — file-scope `#nullable enable` on
line 1 (not namespace-scoped), a small static formatter class (35 lines), single public method
`FormatPercent(double? probability)`. Low blast radius (no other file depends on internal state;
it is a pure static helper), easy to introduce and revert a one-line defect, e.g. inside
`FormatPercent`:

```csharp
string? maybeNull = null;
int len = maybeNull.Length;   // CS8602: dereference of a possibly null reference
```

Under the proposed gate (`/t:Rebuild /p:TreatWarningsAsErrors=true`, no global `/p:Nullable`),
this file's own pragma makes CS8602 fire and `TreatWarningsAsErrors` promotes it to a build error
→ **gate FAILS**. Revert by deleting the two inserted lines; `PercentageFormatter.cs` returns to
its current, already-verified-clean state.

Alternative/backup candidate: `UtilitiesCS/Extensions/NullExtensions.cs` (namespace-scoped
`#nullable enable` on line 12) — also small and low-risk, but it is a generic extension method
consumed elsewhere in the (still-unremediated) codebase, so `PercentageFormatter.cs` is the safer
first choice because it currently has zero consumers found in a scan of its class name outside
its own file.

### Candidate non-opted-in file

`UtilitiesCS/Dialogs/ActionButton.cs` — confirmed to have **no** `#nullable` pragma anywhere
(absent from the 25-file opted-in grep result). It already contains a structurally uninitialized
non-nullable reference field pattern that would be a genuine CS8618 candidate if nullable were
enabled: `private string _name;` (line 94) is never assigned in the constructor
`ActionButton(Button button, DialogResult dialogResult, Action action)` (lines 17–23). Two
equally valid deliberate-defect options for this file:

1. Add a local null-literal assignment to a non-nullable local, e.g. inside any method:
   `string local = null; Console.WriteLine(local.Length);` — under nullable-oblivious context this
   compiles with **no diagnostic at all** (CS8600/CS8602 only exist in an enabled context), so the
   gate **PASSES** (no cross-block).
2. Simplest introduce-and-observe option: temporarily comment out the `_name = name;` line already
   present in one constructor and confirm the existing (already-uninitialized-in-one-ctor) pattern
   still builds clean — this requires no new code at all, only relies on the field already being
   uninitialized on one path, but is less illustrative for a reviewer than option 1's explicit
   null-literal assignment.

Option 1 is recommended: it is a two-line, obviously reversible insertion whose intent (a null
literal assigned to and dereferenced through a non-nullable `string`) is self-evidently a nullable
defect class, independent of any pre-existing field-initialization nuance in the file.

### Method (fail-before / pass-after evidence capture)

1. Confirm the pre-defect baseline is clean: run the proposed gate command once with no defect
   present and capture `EXIT_CODE: 0` plus a short `Output Summary:` to an evidence artifact
   (per `evidence-and-timestamp-conventions`, under
   `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/other/`).
2. Insert the opted-in-file defect only; re-run the gate; capture `EXIT_CODE: <non-zero>` and the
   `CS8602` line from the MSBuild error output as fail-before evidence.
3. Revert the opted-in-file defect (confirm `git diff` for that file is empty); insert the
   non-opted-in-file defect only; re-run the gate; capture `EXIT_CODE: 0` as pass-after
   (non-cross-block) evidence.
4. Revert the non-opted-in-file defect (confirm `git diff` is empty repo-wide); run the gate one
   final time to capture the restored-clean state as closing evidence.
5. Each of the four runs is a separate evidence artifact; none of the four leaves a working-tree
   diff at completion (`git status` clean at hand-off), satisfying the issue's Constraints & Risks
   requirement that the deliberately-introduced defect "must not leave a failing gate or a real
   defect on the branch."

Because the currently-opted-in file set (28 files total) will grow substantially as the wave-0/
wave-1 children execute before the capstone runs, the atomic plan must re-grep for
`#nullable enable` immediately before this verification step and re-select a still-opted-in file
and a still-non-opted-in file at that time — the two candidates named here are correct as of this
research pass but are not guaranteed to still be representative (e.g. `ActionButton.cs` is itself
in-scope for the already-prepared `dialogs-misc` child's Batch, so it may already be opted in by
execution time; a residual non-Dialogs, non-yet-scheduled file should be reconfirmed as
non-opted-in then).

## (c) Rules-vs-convention conflict — exact conflicting lines, flag-only

`.claude/rules/csharp.md` (not edited; quoted verbatim for the flag):

- Toolchain section, item 3 (line 16):
  > 3. **Type Checking — Nullable Analysis**: Enable nullable reference types and fail on warnings.
  > Command: `msbuild <solution>.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"
  > /p:Nullable=enable /p:TreatWarningsAsErrors=true`

- "Severity-first ordering invariant" section (lines 81–83):
  > All new analyzer rule severities are configured in `.editorconfig` at `severity = suggestion`
  > (never `warning`/`error`) BEFORE any `<Analyzer Include>` item is wired into a project. This is
  > required because the type-check toolchain step runs `msbuild ... /p:Nullable=enable
  > /p:TreatWarningsAsErrors=true`, which promotes any `warning`-severity analyzer diagnostic to a
  > build error. Keeping new analyzer diagnostics at `suggestion` (message level) prevents the
  > analyzer adoption from breaking the protected nullable gate.

Both citations document the toolchain's type-check step as forcing `/p:Nullable=enable` globally
(also present in the identically-worded root `CLAUDE.md` "C# Toolchain" section and the
`csharp-code-change-policy`/`C# Unit Test Policy` sections of `CLAUDE.md`). This conflicts with
the per-file opt-in convention the epic adopts (and that this capstone's AC1 finalizes in the
actual CI workflow). Policy prohibits editing any `.claude/rules/*` file, so this is a flag, not a
resolution.

### Flagging precedent (coverage-threshold conflict)

Searched the repo for a prior instance of a policy-conflict being surfaced to the maintainer
rather than resolved by editing rules. Found:
`docs/features/archive/2026-07-06-appevents-loadasync-inbox-gating-243/runbooks/coverage-threshold-exception.runbook.md`
— a dedicated runbook artifact recording a coverage-threshold exception decision, referenced from
that feature's `remediation-plan.2026-07-06T12-29.md`, rather than any edit to the coverage-floor
rule text itself. The `.claude/rules/general-unit-test.md` "COM/VSTO/WinForms coverage exemption"
section similarly documents that such exemptions require ratification "by the project maintainer"
and are "tracked" in a feature branch, not resolved by silently changing the numeric floor.

Applying the same pattern to this capstone: the `spec.md` (AC4) records the conflict as an
explicit maintainer-decision item quoting the two lines above, states that no `.claude/rules/*`
file is edited, and — consistent with the runbook precedent — recommends the maintainer either (i)
accept the workflow's per-file-pragma gate as the authoritative implementation and treat
`csharp.md`'s wording as documentation debt to be corrected in a future, maintainer-approved rules
edit, or (ii) explicitly ratify an exception analogous to the coverage-threshold runbook. This
capstone does not choose between (i) and (ii); it only presents both to the maintainer.

## (d) Optional project-level flip — feasibility assessment

### Trade-offs

- **What a project-level `<Nullable>enable</Nullable>` flip would add over per-file pragmas:**
  applies the enabled nullable context to every file compiled by the project by default, including
  any file a future contributor adds without remembering to add the pragma; removes the
  possibility of a file silently regressing to oblivious status by having its pragma accidentally
  deleted; is the more conventional/idiomatic .NET configuration (per-file pragma is normally a
  migration technique, not a steady-state end state).
- **Risk:** it would immediately surface CS86xx debt in *every* file that is not yet opted in at
  flip time, including generated/Designer files and any file legitimately excluded (Interfaces,
  Designer files) unless those are separately suppressed (e.g., via
  `<Nullable>disable</Nullable>` overrides in a `.editorconfig`/per-file `#nullable disable`, or
  `NoWarn`/exclusion glob at the project level — none of which currently exist in either csproj).
  A flip performed before every file is genuinely clean would re-create exactly the
  "silently-masked, then suddenly-blocking" failure mode PR #361 was written to fix, just moved
  from the CI-flag layer to the project-config layer.
- **How it would be gated:** per the epic Non-Goals and issue AC5, only as a separate,
  maintainer-approved step, executed after every remaining in-scope file is opted in and clean,
  with its own dedicated verification pass (full solution `/t:Rebuild` with the flip in place,
  no per-file pragma reliance needed at that point since the pragmas become redundant once the
  project defaults to enabled).

### Current feasibility (evidence-based)

**Not feasible today.** As established in Section 0, only 25 of `UtilitiesCS/`'s ~485 `.cs` files
(per the `Glob UtilitiesCS/**/*.cs` count) and 3 of `SVGControl/`'s `.cs` files currently carry
`#nullable enable`. Even after all twelve children execute, exclusions remain by design:

- `UtilitiesCS/Interfaces/**` (~62 files) — epic-wide exclusion, left with no pragma (CS8618
  cannot fire in interface-only files, so this is inert for the flip's actual risk surface, but a
  project-level flip does not skip these files; they compile under the enabled context regardless
  of pragma, and being interface-only they are expected to stay warning-free).
- `UtilitiesCS/Properties/Resources.Designer.cs` and `Settings.Designer.cs` — epic-wide exclusion,
  left null-oblivious deliberately; a project-level flip would force these into an enabled context
  too. Generated Designer files are a known source of CS86xx noise (implicit non-nullable fields
  assigned by designer-generated `InitializeComponent()` patterns), so this is the single largest
  concrete risk the flip introduces beyond what per-file pragmas ever exposed.
- Six `OlFolderTools` Designer-generated files (residuals #375 spec, Maintainer Decision item 3) —
  same Designer-file risk as above, at smaller scale.
- `PeopleScoDictionaryNewBackup.cs` — dead, uncompiled duplicate; irrelevant to the flip since it
  is outside the csproj's `<Compile Include>` set regardless of `<Nullable>`.
- Any file still pending a maintainer decision to remediate vs. exclude vs. delete (`MSDemoConv.cs`,
  `To Depricate/*`, `MailResolution_ToRemove` — see Section (e)) is, by definition, not yet in a
  known-clean state, so the flip cannot safely happen until those decisions resolve one way or the
  other.

**Conclusion:** AC5 should record the flip as evaluated-but-not-executed, with the concrete
blocking condition being "Designer-generated files (Resources/Settings + 6 OlFolderTools) and the
Interfaces tree would enter an enabled nullable context under a project-level flip without ever
having been individually verified clean," which is a materially different (and currently
unverified) risk surface from the per-file pragma approach's exhaustively-tested surface.

## (e) Epic-wide maintainer-decision consolidation (source-cited inventory)

| Item | Source (child + file) | Decision needed |
|---|---|---|
| `UtilitiesCS/Interfaces/**` (~62 `.cs`) | Epic manifest `epic.md` "Epic-wide exclusions" (lines 230–235); independently confirmed by `dialogs-misc` spec.md "Ownership Gaps" table (`Interfaces/**` row, ~62 files, "CS8618 cannot fire") | Formal epic-wide exclusion from all children (extends existing `Interfaces/IHelperClasses/` precedent) — recorded, not a live blocker. |
| `UtilitiesCS/Properties/Resources.Designer.cs` + `Settings.Designer.cs` (2 `.cs`) | Epic manifest `epic.md` lines 236–238; `dialogs-misc` spec.md "Ownership Gaps" table | Leave null-oblivious (no pragma); `AssemblyInfo.cs` already in `dialogs-misc` scope as verify-only. |
| `PeopleScoDictionaryNewBackup.cs` | `utilitiescs-nullable-residuals` spec.md, Maintainer Decisions item 1 (lines 224–232); epic manifest "residuals (#375) execution-time findings" (lines 249–253) | Dead, uncompiled duplicate (CS0101 conflict with live `PeopleScoDictionaryNew.cs`); not in the csproj `<Compile Include>` set. Exclude from opt-in set or delete the file — maintainer choice. |
| 6 `OlFolderTools` Designer-generated files | `utilitiescs-nullable-residuals` spec.md lines 160–164, 210; epic manifest lines 249–253 | Left null-oblivious (no pragma), consistent with the epic-wide Designer-file exclusion; generated halves of WinForms partial classes. |
| Three pre-existing >500-line files in the residual set: `OutlookObjects/AppointmentItem/MeetingItemHelper.cs` (847 lines), `OutlookObjects/Recipient/RecipientStatic.cs` (773 lines), `OutlookObjects/Fields/UserDefinedFields.cs` (722 lines) | `utilitiescs-nullable-residuals` spec.md, Maintainer Decisions item 6 (lines 258–268); epic manifest "residuals (#375) execution-time findings" (lines 259–262) | Flagged, not split — same precedent as Wave-0 `threading` (#369) applying to `TimeOutTask.cs` (975 lines). Splitting is a refactor, out of scope for annotation-only remediation. |
| `Examples/MSDemoConv.cs` | `utilitiescs-nullable-residuals` spec.md, Maintainer Decisions item 2 (lines 233–238); originally surfaced by `dialogs-misc` spec.md "Ownership Gaps" table (`Examples/MSDemoConv.cs` row) | Default: remediate annotation-only; alternatives (exclude via `[ExcludeFromCodeCoverage]`/pragma omission, or delete) surfaced for maintainer decision — demo/sample code, not production surface. |
| `To Depricate/FileIO2.cs` and `To Depricate/StringManipulation.cs` | `utilitiescs-nullable-residuals` spec.md, Maintainer Decisions item 3 (lines 239–244); originally surfaced by `dialogs-misc` spec.md "Ownership Gaps" table (`To Depricate/*` row) | Real production helpers explicitly named for future deprecation. Annotation-only is feasible but may be wasted effort; maintainer chooses remediate vs. exclude vs. schedule deletion. Flagged; not deleted within `residuals`. |
| `OutlookObjects/MailResolution.cs` class `MailResolution_ToRemove` | `utilitiescs-nullable-residuals` spec.md, Maintainer Decisions item 4 (lines 245–248) | `_ToRemove` suffix signals a deletion candidate. Default: remediate in place (annotation-only is trivial); flag as a deletion candidate; do not delete within `residuals`. |
| `SvgImageSelector.ImagePath` dead-setter / `_relativeImagePath!` judgment call | `utilitiescs-nullable-svgcontrol` spec.md lines 58, 145–153 | The `set` accessor body is entirely commented out (functional no-op), so `_relativeImagePath` is never assigned on any live path, yet the `get` fallback dereferences it. Default: null-forgiving `_relativeImagePath!` with an in-code comment; described as "the single highest-consequence judgment call in the cluster" requiring explicit maintainer acceptance. |
| `SVGControl/RelativePath.cs` (1678 lines) | `utilitiescs-nullable-svgcontrol` spec.md lines 126, 161 | Already exceeds the repo's 500-line limit; is one of 3 already-clean "verify-only" files in this cluster (not newly remediated, but flagged as a pre-existing oversized file consistent with the same no-split precedent as the residuals files above). |
| `dialogs-misc` → `helperclasses` (#364) `depends_on` edge | `utilitiescs-nullable-dialogs-misc` spec.md lines 164–170, 259–260 (Constraints & Risks item 4) | Grep-unconfirmed by source (zero `HelperClasses/` type references under `Dialogs/`). Retained (harmless — both Wave-0 upstreams are prepared) and flagged, not dropped. |
| `residuals` → `reusabletypes` (#366) undeclared dependency edge | `utilitiescs-nullable-residuals` spec.md, Maintainer Decisions item 5 (lines 249–257); epic manifest "residuals (#375) execution-time findings" (lines 254–258) | Six in-scope files consume `#366` types (`TreeNode<T>`, `SmartSerializableLoader`, `ScoDictionaryNew<,>`) not declared in `depends_on`. Harmless for ordering (Wave 0 precedes Wave 1); flagged for epic-planner to add the edge or confirm annotated null-neutrality. |
| Rules-vs-convention conflict (`.claude/rules/csharp.md` forcing global `/p:Nullable=enable`) | Epic manifest lines 148–153; independently re-stated (not resolved) in `residuals` spec.md lines 340–344 and `svgcontrol` spec.md lines 270–277 | See Section (c) above — this capstone's own AC4/AC6 responsibility to consolidate and surface, not any other child's. |

All twelve rows above are cross-referenced from at least the epic manifest or one child `spec.md`;
none were newly invented for this research pass. The capstone's `spec.md` "Maintainer Decision
Summary" (AC6) should reproduce this table (or an equivalent single consolidated list) rather than
requiring the maintainer to read every child spec individually.

## (f) CI-workflow authoring rules — applicability

- **`.claude/rules/ci-workflows.md`** governs steps whose `run:` block *intentionally invokes a
  failing nested command* (e.g., a negative-path self-validation). The proposed (a) edit is a
  narrow, single-line removal (`/p:Nullable=enable`) from an already-compliant step; it does not
  add a deliberately-failing nested command, and it does not change the step's existing exit-code
  handling. The step already ends with the compliant pattern
  `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }` (line 115), which is preserved verbatim by the
  proposed edit. **This rule's specific "deliberately-failing nested command" trigger does not
  apply to AC1's edit.** It does apply, however, to the (b) genuine-enforcement verification's
  fail-before step if that verification is ever wired into a workflow `run:` block (rather than
  run locally as evidence capture) — any such wiring must reset `$LASTEXITCODE` or use an explicit
  `exit 0`/`exit 1` on the success path, per the rule.
- **`.claude/rules/benchmark-baselines.md`** governs performance-baseline JSON provenance
  (`HostEnvironmentInfo.ProcessorName`, sibling `baseline.provenance.json`). The capstone's edit
  touches no `scripts/benchmarks/**` path and introduces no baseline artifact; this rule does not
  apply to (a)–(e) of this capstone.
- **`modified-workflow-needs-green-run` policy rule** (`.claude/skills/feature-review-workflow/
  SKILL.md` lines 68–75): the proposed edit is a diff under `.github/workflows/**`
  (`ci.yml`), which is exactly the trigger path this rule matches. It requires "evidence of a green
  workflow run against the branch head... in the remediation inputs," accepting either a PR-context
  run or a `workflow_dispatch` run whose head SHA matches the branch head. **This is an
  execution/merge-time concern**, not something research or planning can satisfy in advance — the
  spec/plan should record that atomic execution (and later epic-orchestrator's fan-in to
  `main`) must capture a green CI run against the capstone branch head before the change can merge,
  consistent with how PR #361 itself (the gate-repair predecessor) was subject to the same rule.

## Summary of file/line citations used

- `.github/workflows/ci.yml` lines 39–44 (job name/runner), 103–115 (gate step, current text).
- `UtilitiesCS/UtilitiesCS.csproj` — grep for `Nullable`: no matches; `LangVersion` = `12.0`,
  `TargetFrameworkVersion` = `v4.8.1`.
- `SVGControl/SVGControl.csproj` — grep for `Nullable`: no matches; `LangVersion` = `latest`,
  `TargetFrameworkVersion` = `v4.8.1`.
- `.claude/rules/csharp.md` lines 16, 81–83 (quoted verbatim above).
- `.claude/rules/ci-workflows.md` (full file, reproduced in project CLAUDE.md context).
- `.claude/rules/benchmark-baselines.md` (full file, reproduced in project CLAUDE.md context).
- `.claude/skills/feature-review-workflow/SKILL.md` lines 68–75.
- `docs/features/epics/utilitiescs-nullable-remediation/epic.md` lines 139–277 (capstone design,
  residual-scope decision, epic-wide exclusions, residuals execution-time findings).
- `docs/features/active/utilitiescs-nullable-ci-capstone/issue.md` (AC1–AC7, full file).
- `docs/features/active/utilitiescs-nullable-residuals/spec.md` lines 1–40, 160–268, 330–344.
- `docs/features/active/utilitiescs-nullable-residuals/plan.2026-07-18T23-13.md` lines 1–29.
- `docs/features/active/utilitiescs-nullable-dialogs-misc/spec.md` lines 46, 160–190, 255–284.
- `docs/features/active/utilitiescs-nullable-svgcontrol/spec.md` lines 28, 45, 58, 126, 145–196,
  260–277.
- `docs/features/archive/2026-07-06-appevents-loadasync-inbox-gating-243/runbooks/
  coverage-threshold-exception.runbook.md` (flagging precedent).
- `UtilitiesCS/Dialogs/ActionButton.cs` lines 1–183 (candidate non-opted-in file).
- `UtilitiesCS/OutlookObjects/Folder/PercentageFormatter.cs` (full file, candidate opted-in file).
- `UtilitiesCS/Extensions/NullExtensions.cs` lines 1–30 (backup candidate opted-in file).

## Rejected alternatives

- **Keeping `/p:Nullable=enable` and instead excluding not-yet-opted files via `NoWarn` or
  per-project `<Compile Remove>` gymnastics** was considered and rejected: it would require
  maintaining an ever-changing exclude list in the csproj (itself a production-file edit this
  research-only pass and the epic's Non-Goals both avoid defaulting to), whereas simply dropping
  the global flag achieves the same effect for free using existing MSBuild/Roslyn per-file context
  resolution.
- **Editing `.claude/rules/csharp.md` to match the new gate** was considered and rejected per
  explicit policy prohibition (no `.claude/rules/*` edits); handled instead via the Section (c)
  flag.
