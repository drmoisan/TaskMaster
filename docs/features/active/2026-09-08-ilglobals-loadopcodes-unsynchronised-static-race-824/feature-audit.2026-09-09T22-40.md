# Feature Audit — Issue #824 (ILGlobals.LoadOpCodes unsynchronised static race)

- **Issue:** #824
- **Work Mode:** `full-bug` (marker at `issue.md:12`)
- **AC source (sole, authoritative):** `docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/spec.md`
- **Base branch:** `epic/review-residuals-2026-09-08-integration`, base commit `553f874a287261af0dd42e4f9270d31ac475308a`
- **Reviewer:** feature-review agent
- **Timestamp:** 2026-09-09T22-40
- **Disposition:** **PASS — 12 of 12 acceptance criteria verified. 0 blocking findings.**

---

## AC Source Resolution

`issue.md:12` records `- Work Mode: full-bug`. Per the `acceptance-criteria-tracking` skill,
`full-bug` resolves to `spec.md` **only**. **No `user-story.md` exists and none should exist**; its
absence is correct for this work mode and is not recorded as a gap.

The three unchecked boxes at `spec.md:30-33` (`Blocker`, `High`, `Low`) are severity selectors in the
Impact / Severity block, with `Medium` selected at `:32`. They are not acceptance criteria and are
excluded from every count below.

Criteria counted by `Grep` over `spec.md`: `- [x] **AC` = **12**, `- [ ] **AC` = **0**. All twelve
were already checked off by the executor. No criterion was added, reworded, renumbered, or reordered
by this review, and none needed to be newly checked off.

---

## Verification Basis

The Bash tool was banned for this review, so no `git` command was executed. Each verdict below states
its basis:

- **Re-derived** — this reviewer measured it directly from the working tree, a Cobertura document, or
  a toolchain log.
- **Evidence-attested** — read from a committed evidence artifact; not independently re-derivable
  without git.

Four criteria (AC5, AC8, AC9, AC10) contain a `git diff --stat` emptiness clause that cannot be
re-executed under this restriction. Each is corroborated by an independent, anchor-free secondary
observation, listed per criterion. All four are graded **PASS** on that combined basis rather than
UNVERIFIED, because the corroborating observations are sufficient to discriminate the failure case.

---

## Per-Criterion Verdicts

| AC | Subject | Verdict |
|---|---|---|
| AC1 | Publication mechanism (`readonly` fields + static constructor) | **PASS** |
| AC2 | Primary behavioural gate — `LoadOpCodes()` does not republish | **PASS** |
| AC3 | Secondary structural gate — both fields are `InitOnly` | **PASS** |
| AC4 | Supporting test — exhaustive opcode-table population | **PASS** |
| AC5 | `LoadOpCodes()` retained as a forced-initialization call | **PASS** |
| AC6 | Corrected nullable annotations and comment | **PASS** |
| AC7 | Four `LoadOpCodes_*` tests no longer mis-attribute published state | **PASS** |
| AC8 | No `[DoNotParallelize]` on either test class | **PASS** |
| AC9 | No synchronisation primitive; `MethodBodyReader.cs` unchanged | **PASS** |
| AC10 | Build-file discipline | **PASS** |
| AC11 | Clean full C# toolchain pass in repository-standard order | **PASS** |
| AC12 | Coverage captured and not regressed on changed lines | **PASS** |

**12 PASS / 0 PARTIAL / 0 FAIL / 0 UNVERIFIED.**

**No criterion was checked off without adequate support.** Every one of the twelve was independently
examined against the delivered code or against a named artifact, and each is supported.

---

## Detailed Evaluation

### AC1 — Publication mechanism — **PASS**

**Re-derived.** Reading `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` directly:

- `:122` — `public static readonly OpCode[] multiByteOpCodes;` — `readonly`, no initializer.
- `:130` — `public static readonly OpCode[] singleByteOpCodes;` — `readonly`, no initializer.
- `:139-169` — explicit static constructor `static ILGlobals()`. It declares
  `OpCode[] singleTable = new OpCode[0x100];` and `OpCode[] multiTable = new OpCode[0x100];` at
  `:141-142`, runs the reflection loop over `typeof(OpCodes).GetFields()` at `:143-166` writing only
  the locals (`:155`, `:163`), and assigns each field exactly once at `:167-168`, **after** the loop's
  closing brace at `:166`.

The repository-wide sweep required by the criterion was re-executed: `Grep` for
`(singleByteOpCodes|multiByteOpCodes)\s*=` over `*.cs` returns exactly **two** matches, both at
`ILGlobals.cs:167-168`, both inside the static constructor. No match anywhere else.

The delivered implementation matches the five-step trace in the Proposed Fix section, including the
requirement that the field assignment **follows** the fill rather than preceding it — which is the
whole substance of the fix.

Corroborating artifact: `evidence/qa-gates/ac1-assignment-sweep.2026-09-09T15-46.md`, re-validated
post-format at `evidence/qa-gates/static-gates-revalidated.2026-09-09T16-15.md`.

### AC2 — Primary behavioural gate — **PASS**

**Re-derived (test) + evidence-attested (red/green pair).**

`LoadOpCodes_DoesNotRepublishPublishedTables` exists at `ILGlobals_Tests.cs:42-69`. It captures both
fields into locals (`:46-47`), Acts with `ILGlobals.LoadOpCodes();` (`:50`), and asserts
reference identity via FluentAssertions `BeSameAs` against each captured local (`:53-60`, `:61-68`),
each with an explanatory reason string.

Fail-before: `evidence/regression-testing/ac2-fail-before.2026-09-09T15-25.md` —
`ExpectedExitCode: 1`, `EXIT_CODE: 1`, `TOTAL=13 / PASSED=12 / FAILED=1`, with a complete
thirteen-row per-test outcome table and the failure site named from the captured stack trace as
`ReferenceTypeAssertions.BeSameAs` raised from the named test. This is a real red run, not an absence
from a pass list: the total is a discovery-count control (12 baseline + 1 added by P1-T1), and the
failing assertion is the reference-identity check the criterion states rather than an incidental
error.

Pass-after: `evidence/regression-testing/ac2-ac3-pass-after.2026-09-09T15-38.md` —
`TOTAL=15 / PASSED=15 / FAILED=0`, with the population held constant at 15 across the P1-T6 and
P2-T5 runs.

Order-independence holds as the criterion states: on the unfixed tree either the captured local is
`null` and the post-call value is a fresh array, or the captured local is a prior array and the call
allocates a different one. There is no third ordering.

**Re-derived: the test contains no sleep, no retry, no timing tolerance, no repeated-run loop, and no
additional thread.** A `Grep` over the whole `SDILReader` test directory for the banned-API set
returned zero matches.

### AC3 — Secondary structural gate — **PASS**

**Re-derived (tests) + evidence-attested (red/green pair).**

`SingleByteOpCodes_FieldIsInitOnly` (`:76-94`) and `MultiByteOpCodes_FieldIsInitOnly` (`:100-118`)
both resolve their field via
`typeof(ILGlobals).GetField(nameof(ILGlobals.<field>), BindingFlags.Public | BindingFlags.Static)`,
assert the returned `FieldInfo` is not null, then assert `IsInitOnly` is true with an explanatory
reason.

The criterion's prohibition is honoured: **neither test asserts anything about
`TypeAttributes.BeforeFieldInit`.** Confirmed by full read of the file — the token does not appear.

Fail-before: `evidence/regression-testing/ac3-fail-before.2026-09-09T15-30.md` —
`ExpectedExitCode: 1`, `EXIT_CODE: 1`, `TOTAL=15 / PASSED=12 / FAILED=3`, with both named tests
listed **Failed** and the third failure correctly identified as the still-red AC2 gate rather than
treated as a defect. Discovery-count control: 12 baseline + 1 + 2.

Pass-after: same artifact as AC2, `PASSED=15 / FAILED=0`.

### AC4 — Supporting test — **PASS**

**Re-derived.** `OpCodeTables_ContainEveryOpCodeDeclaredOnOpCodes` exists at
`ILGlobals_Tests.cs:126-188`. It enumerates every `public static` field of type `OpCode` on
`typeof(OpCodes)` and, per opcode, asserts `singleByteOpCodes[value]` equals it when `value < 0x100`,
otherwise asserts `(value & 0xff00) == 0xfe00` and `multiByteOpCodes[value & 0xff]` equals it. It
adds a non-vacuity guard, `assertedCount.Should().BeGreaterThan(0, ...)`, that the criterion does not
require and that is a genuine improvement.

**The qualification is honoured everywhere, which was checked specifically.** The criterion requires
that this test "must not be reported or relied upon as evidence that the race is fixed." Every
location that mentions AC4 was examined:

- the test's own XML doc comment at `ILGlobals_Tests.cs:120-125` — "Supporting test, not a gate for
  issue #824. It passes on the unfixed tree as well...";
- `evidence/regression-testing/ilglobals-tests-after-rework.2026-09-09T15-44.md:57-60`;
- `evidence/regression-testing/ac2-ac3-pass-after.2026-09-09T15-38.md:65-66`;
- `evidence/other/ac-checkoff-notes.2026-09-09T16-19.md:45-55` — "AC4 is checked off because the test
  exists, is correctly implemented, and passes — which is what the criterion asks — not because its
  passing demonstrates anything about the race."

**No artifact presents AC4 as evidence that the race is fixed. No blocking finding on this axis.**
Carrying the qualification in the source file, not only in the audit trail, is the stronger placement,
since the audit trail is archived at feature close.

### AC5 — `LoadOpCodes()` retained — **PASS**

**Re-derived (production side) + corroborated (unchanged-file side).**

`ILGlobals.cs:177-180` declares `public static void LoadOpCodes()` with the single-statement body
`RuntimeHelpers.RunClassConstructor(typeof(ILGlobals).TypeHandle);`. The required
`using System.Runtime.CompilerServices;` directive is present at `:6`. The method is neither removed
nor left with an empty body.

`MethodBodyReader_Tests.cs` unchanged: the `git diff --stat` clause could not be re-executed. Three
anchor-free corroborations, all re-derived by this reviewer:

1. line count is **489**, exactly the recorded baseline, so the file retains its 11 lines of headroom
   against the 500-line cap;
2. `ILGlobals.LoadOpCodes()` is still present at `:364`, and compiles against the now-`readonly`
   fields because the call site is a read of a method, not a write to a field;
3. the file is absent from the caller-measured footprint against the true base `553f874a`.

Evidence: `evidence/qa-gates/ac5-loadopcodes-retained.2026-09-09T15-48.md` records both the
merge-base-anchored and `HEAD`-anchored diffs as empty and agreeing, plus a porcelain listing with no
entry naming the path.

**Non-blocking observation (does not affect this verdict):** the method's doc comment at `:172-173`
says "Retained for the **five** existing call sites". A repo-wide `Grep` for `LoadOpCodes` over
`*.cs` at head returns exactly **two** invocations. The count was correct pre-change; this feature's
own AC7 rework removed three of the four. AC5 constrains the declaration and body, not the comment
text, so the criterion is met. Recorded as finding N-1 in `code-review.2026-09-09T22-40.md`.

### AC6 — Corrected nullable annotations and comment — **PASS**

**Re-derived.**

- `Grep` for `null!` over `ILGlobals.cs`: **0 matches**. Both `= null!` suppressions are deleted.
- `Grep` for `populated by LoadOpCodes` over the file: **0 matches**. The stale three-line comment is
  gone.
- The replacement documentation at `:115-121` and `:124-129` states all three required properties: the
  tables are published once by the static constructor, are never reassigned thereafter, and
  `readonly` prevents reassignment of the reference but not element mutation, so callers must treat
  the contents as read-only.
- `#nullable enable` remains at `:1`.
- **Zero CS86xx diagnostics.** Re-derived from `coverage/msbuild-nullable-final.log`: `Grep` for
  `: error [A-Z]+[0-9]+:|: warning [A-Z]+[0-9]+:` returns **0**, under
  `/p:TreatWarningsAsErrors=true`, which promotes CS86xx to errors. In particular no CS8618 is raised
  on either non-nullable `readonly` field.

This discharges the risk `spec.md:581-583` and `:318-320` explicitly record as an unverified research
claim: definite assignment in the static constructor does satisfy the compiler's null-state analysis,
and no `null!` suppression was needed.

### AC7 — Four `LoadOpCodes_*` tests reworked — **PASS**

**Re-derived.**

- `Grep` for `ILGlobals\.LoadOpCodes\(\)` over `ILGlobals_Tests.cs` returns exactly **one**
  invocation, at `:50`, inside the body of `LoadOpCodes_DoesNotRepublishPublishedTables` (declared
  `:43`, body bounded `:44-69`). It is the AC2 Act, as required.
- The four old names —
  `LoadOpCodes_Initializes_SingleByteOpCodes`, `LoadOpCodes_Initializes_MultiByteOpCodes`,
  `LoadOpCodes_PopulatesKnownSingleByteOpCodes`, `LoadOpCodes_PopulatesKnownOpCode_Ret` — do not
  appear anywhere in the file. Confirmed by full read.
- `SingleByteOpCodes_IsPublishedWithFullLength` (`:17`) and
  `MultiByteOpCodes_IsPublishedWithFullLength` (`:29`) exist and assert non-null plus `Length == 0x100`
  (`:20-21`, `:32-33`), preserving the assertions previously at `:18-19` and `:29-30`. Neither calls
  `LoadOpCodes()`; both carry an XML comment explaining that reading the field triggers publication,
  which is the property under test.
- The two spot checks are deleted, their coverage subsumed by the strictly stronger AC4 test.

This is the substantive half of the criterion: the old names taught the false mental model —
"`LoadOpCodes()` is what populates the tables" — that produced the defect.

### AC8 — No `[DoNotParallelize]` — **PASS**

**Re-derived.** `Grep` for `\[DoNotParallelize\]` over the entire
`UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/` directory — a strictly wider search than the
criterion's two named files — returns **zero matches**. Neither class gained the attribute as a
stabilisation measure.

`UtilitiesCS.Test/Properties/AssemblyInfo.cs` read directly: `:18-21` carries

```csharp
[assembly: Parallelize(
    Workers = 0,
    Scope = Microsoft.VisualStudio.TestTools.UnitTesting.ExecutionScope.ClassLevel
)]
```

unweakened, unnarrowed, and unremoved. The `git diff --stat` emptiness clause could not be
re-executed; the file's contents at the required lines are the direct corroboration, and the path is
absent from the caller-measured footprint.

### AC9 — No synchronisation primitive; `MethodBodyReader.cs` unchanged — **PASS**

**Re-derived (primitive sweep) + strongly corroborated (unchanged file).**

`Grep` for `lock\s*\(|volatile|Lazy<` over `ILGlobals.cs` returns **zero matches**, confirming that
rejected candidate 2 and the `volatile` variant were not partially adopted. Also re-derived: zero
`ExcludeFromCodeCoverage` attributes in the file.

`MethodBodyReader.cs` unchanged: the `git diff --stat` clause could not be re-executed. Three
anchor-free corroborations, all re-derived:

1. line count is **299**, exactly the recorded baseline;
2. the Cobertura class element for `SDILReader.MethodBodyReader` is **byte-identical** across the two
   documents — `line-rate="0.9732620320855615" branch-rate="0.8947368421052632" complexity="38"` at
   `coverage/baseline.cobertura.xml:44420` and `coverage/post-change.cobertura.xml:44426`. Any edit to
   the file's executable lines would have perturbed at least one of these three values. The 6-line
   positional shift between the two documents is fully accounted for by `ILGlobals` growing above it;
3. the path is absent from the caller-measured footprint against `553f874a`.

The reads at `:105` and `:110` therefore keep their exact current expression form, no lock is
introduced on the instruction-decode path, and no consumer-side `catch` in that file is broadened.

### AC10 — Build-file discipline — **PASS**

**Evidence-attested, with the inherited change named rather than summarised.**

The delivered outcome is the criterion's **preferred** branch: no change at all to
`UtilitiesCS.Test/UtilitiesCS.Test.csproj`, achieved by placing all four new tests in
`ILGlobals_Tests.cs`, already registered at `UtilitiesCS.Test/UtilitiesCS.Test.csproj:264` with 367
lines of headroom at baseline. The one-added-line fallback was not exercised.
`UtilitiesCS.csproj` is likewise unchanged: the fix edits an already-registered production file in
place and adds no new production file.

This is the one gate the [P0-T15] base-anchor finding materially affects, and
`evidence/qa-gates/ac10-build-file-discipline.2026-09-09T15-53.md` handles it correctly: it records
both anchors, states that they disagree, and **names the four inherited insertions verbatim** rather
than summarising them away. All four are `<Compile Include>` lines registering
`FolderPredictorTests` partial-class files under `OutlookObjects\Folder\` — visibly a sibling child's
owned file set, not #824's. The `HEAD`-anchored diff is empty for both project files, and the
porcelain listing contains no entry naming a project file.

No reordering, reformatting, or whitespace normalisation appears in either diff, because neither
project file was opened for editing at any point in the run.

### AC11 — Clean full C# toolchain pass — **PASS**

**Re-derived** from the on-disk logs for every one of the four steps.

| Step | Requirement | Result | Basis |
|---|---|---|---|
| 1 | `csharpier format .` then `check .` reporting zero files needing formatting | exit 0; `Checked 1622 files in 4670ms.` | `coverage/csharpier-check.log:1` |
| 2 | Analyzer msbuild exiting 0 with zero errors | exit 0; `: error`/`: warning` diagnostics = **0** | `coverage/msbuild-analyzers-final.log` |
| 3 | Nullable msbuild exiting 0 with zero errors | exit 0; `: error`/`: warning` diagnostics = **0**, CS86xx = **0** | `coverage/msbuild-nullable-final.log` |
| 4 | vstest with zero failed tests, AC2/AC3/AC4/AC7 tests present in the run's test list | **7212 / 7212 / 0 failed / 0 skipped**; all six named tests present and `Passed` at `TOTAL=14` | `coverage/coverage-post-change.log:7233-7239`; `evidence/qa-gates/ac11-named-tests.2026-09-09T16-09.md` |

**Non-vacuity, re-derived — this is the clause that makes the msbuild gates capable of failing:**

| Gate | `Skipping target "CoreCompile"` | `^\s*CoreCompile:` |
|---|---|---|
| Analyzers | **0** | **12** |
| Nullable | **0** | **13** |

Zero skips paired with a positive compile count proves both gates actually compiled. `/t:Rebuild` was
used on both and `/p:Nullable=enable` was added to neither, matching the CLAUDE.md and CI forms.

**Loop discipline:** one restart, recorded at
`evidence/qa-gates/qc-loop-final-pass.2026-09-09T16-16.md:29-37`. The first P5-T1 pass reported
`FORMAT_CHANGED_TREE=True` — CSharpier reflowed this feature's own hand-written line wrapping in both
source files — and the loop correctly restarted at step 1 rather than continuing. The second pass
reported `False`, and steps 2 through 5 then ran once each. The final pass completed all steps with
no file modified by the formatter, which is what the criterion requires.

**Documented deviation on the final clause (non-blocking).** The criterion's last sentence requires
that "logs for the final pass" be written under the feature's `evidence/qa-gates/`. Raw logs were
written to the gitignored `coverage/` directory and one compact markdown artifact per command step was
committed under `evidence/qa-gates/`. The reconciliation at
`evidence/other/ac-checkoff-notes.2026-09-09T16-19.md:57-76` gives two verifiable reasons:
`.gitignore:84` is `*.log`, so a raw log under the evidence directory would be untracked and
uncommittable; and an msbuild console log carries absolute host paths. The substitution is preferable
to the literal reading, which would have produced the appearance of compliance with nothing in the
repository. The criterion's purpose — durable evidence in the canonical location — is met. **PASS with
the deviation recorded.**

### AC12 — Coverage captured and not regressed — **PASS**

**Re-derived from both raw Cobertura documents, not from the committed extracts.**

Required documents exist: `evidence/baseline/coverage-classes-baseline.2026-09-09T15-16.md` and
`evidence/qa-gates/coverage-classes-post-change.2026-09-09T16-10.md`, plus the delta analysis at
`evidence/qa-gates/coverage-delta.2026-09-09T16-12.md`. Both are canonical evidence kinds.

Per-class comparison, read directly from the `class` elements of the two Cobertura documents as the
criterion specifies:

| Class | Metric | Baseline | Post-change | Post >= baseline |
|---|---|---|---|---|
| `SDILReader.ILGlobals` | `line-rate` | 0.9459459459459459 | **0.95** | yes |
| `SDILReader.ILGlobals` | `branch-rate` | 0.875 | **0.875** | yes |
| `SDILReader.MethodBodyReader` | `line-rate` | 0.9732620320855615 | **0.9732620320855615** | yes |
| `SDILReader.MethodBodyReader` | `branch-rate` | 0.8947368421052632 | **0.8947368421052632** | yes |

Sources: `coverage/baseline.cobertura.xml:44010` and `:44420`;
`coverage/post-change.cobertura.xml:44010` and `:44426`. All four post-change values meet or exceed
baseline. The comparison is on the per-`class` attributes, the comparable axis; raw and post-processed
root totals are not compared, as the criterion requires.

`SDILReader.ILGlobals` improved because its instrumented line count rose from 37 to 40 while the
uncovered count stayed at 2 (35/37 → 38/40).

**Changed-line non-regression, independently confirmed rather than accepted.** Within the
`SDILReader.ILGlobals` class element the **only** instrumented lines carrying `hits="0"` are **160**
and **161** (`coverage/post-change.cobertura.xml:44049-44050`) — the `{` and the
`throw new Exception("Invalid OpCode.");` of the invalid-opcode guard. Neither is an added line, and
both were uncovered at baseline as lines 142-143 before the edit shifted them. Every instrumented
added line — 141, 142, 155, 163, 167, 168, 169, 178, 179 — carries `hits="1"`. That is exactly
**9 / 9 = 100 %** changed-code coverage, matching the executor's figure. The residual is the same
unreachable path on both sides; the fix neither introduced nor removed an uncovered line.

Document-level rates, re-derived from `coverage/post-change.cobertura.xml:2`: line-rate `0.856241`
(85.6241 %), branch-rate `0.798129` (79.8129 %), both up slightly from baseline. These clear the
`.claude/rules` floors of 85 % line and 75 % branch and the CLAUDE.md 80 % figure. The delta artifact
correctly reports both the 80 and the 85 figures side by side and notes that the repository's own
policy documents disagree, rather than silently resolving the conflict; on this tree the value clears
both, so the conflict is not load-bearing here.

**No threshold, exclusion list, or analyzer severity was lowered, weakened, or deleted.** Re-derived:
none of `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`, `coverage.config`, `.editorconfig`,
or any file under `.claude/rules/` appears in the change footprint; `Grep` confirms zero
`ExcludeFromCodeCoverage` attributes in the changed production file; and no production file was added
to any exclusion list.

**Documented deviation on the "no coverage document is written to any other location" clause
(non-blocking).** The raw Cobertura documents were written to the gitignored `coverage/` directory,
with compact markdown extracts committed to the two canonical evidence directories. The reasons,
recorded at `evidence/other/ac-checkoff-notes.2026-09-09T16-19.md:78-98`, are that `.gitignore:144` is
`coverage/*` and a full-solution Cobertura document is on the order of 10 MB carrying absolute host
paths. The comparison clause is still satisfied literally: the per-`class` values in the extracts were
read directly from the Cobertura documents, which this reviewer confirmed by reading the same
documents and obtaining the same values. **PASS with the deviation recorded.**

---

## Outstanding Items (not acceptance criteria; owed at epic close)

Neither blocks this feature. Both are stated because prose inside a feature folder does not survive
feature-folder archival.

1. **File the follow-up issue for the two latent statics.** `ILGlobals.Cache` (`ILGlobals.cs:113`,
   one read site repo-wide at `ILGlobals_Tests.cs:267`, no production reader) and `ILGlobals.modules`
   (`ILGlobals.cs:131`, zero references repo-wide) are recorded as non-goals at `spec.md:125-135` and
   documented at `evidence/other/follow-up-latent-statics.2026-09-09T16-21.md`, which states filing is
   owned by the epic layer and is not a plan task. It is not yet filed. The scoping decision itself is
   correct and this reviewer agrees with it.
2. **Report the outcome to issue #811** so its AC4 can be re-evaluated against a tree that no longer
   contains this race, per `spec.md:625-626`.

## Plan Checklist State

**Re-derived** by `Grep` over `plan.2026-09-08T23-51.md`: `^- \[x\] \[P` = **68**, `^- \[ \] \[P` =
**1** (line 176, `[P0-T15]`).

[P0-T15] is deliberately unchecked and this is the correct record: its stated acceptance — that every
path in the merge-base-anchored inherited listing satisfies one of the three Owned Write Set classes —
is genuinely not met, because the worktree was fast-forwarded to the epic integration tip after the
plan cleared preflight and 277 of 304 inherited paths belong to already-merged siblings. Marking it
`[x]` would have been false.

The executor's decision not to take the plan's halt branch is assessed in
`code-review.2026-09-09T22-40.md` §4.1 and in `policy-audit.2026-09-09T22-40.md` finding N-2, and is
judged **correct**. **No acceptance criterion is left genuinely unverified by it**: the four criteria
containing an anchored-diff clause (AC5, AC8, AC9, AC10) each carry an independent, anchor-free
corroboration recorded above, and the caller separately re-measured the footprint against the true
base `553f874a` and confirms it equals the Owned Write Set. The root cause — plan Decision D5
anchoring an epic child on `origin/main` rather than on the epic integration branch — belongs upstream
in the planner.

---

### Acceptance Criteria Status

- Source: `docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/spec.md`
- Total AC items: 12
- Checked off (delivered): 12
- Remaining (unchecked): 0
- Items remaining: none

No criterion was newly checked off by this review; all twelve were already `[x]` and all twelve are
independently supported. No criterion text was added, reworded, renumbered, or reordered.

---

## Disposition

**PASS.** 12 of 12 acceptance criteria verified. **0 blocking findings.** No remediation inputs are
produced and no remediation cycle is required. Four non-blocking observations are recorded in
`policy-audit.2026-09-09T22-40.md` §8 and `code-review.2026-09-09T22-40.md` §6.

The feature is ready for epic fan-in, subject to the two owed items above being carried at epic close
rather than lost with the feature folder.
