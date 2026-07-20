# utilitiescs-nullable-email-classifier — Spec

- **Issue:** #372
- **Parent (optional):** Epic `utilitiescs-nullable-remediation` (Wave 1)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-18T22-06
- **Status:** Draft
- **Version:** 0.1
- **Integration branch:** `epic/utilitiescs-nullable-remediation-integration`
- **Upstream contract dependency:** #363 (`utilitiescs-nullable-extensions`, Wave 0)
- **Complexity band:** C3

## Overview

What need or gap does this idea address?

The CI nullable gate, repaired by PR #361 to use `msbuild /t:Rebuild` so it performs a
genuine recompile rather than a silently-skipped incremental build, cannot be enforced
against new code until the pre-existing nullable-reference-type debt (CS86xx diagnostics)
is remediated under a per-file `#nullable enable` opt-in architecture. This feature is the
Wave-1 child that remediates the `UtilitiesCS/EmailIntelligence/` classifier cluster:
`Bayesian`, `ClassifierGroups`, and `Flags`.

These modules are classified T1 (Critical) per `.claude/rules/quality-tiers.md`: they are
classifier engines (SpamBayes, Triage) whose behavior bugs can cause silent model drift or
misclassification. This work is null-annotation and null-safety remediation only. It makes
no change to classifier scoring, model logic, corpus/probability math, or any observable
behavior. It consumes the nullability annotation contracts published by the Wave-0
Extensions child (issue #363), whose shared extension methods (for example `ThrowIfNull`,
`IEnumerableExtensions`, `DictionaryExtensions`, `NullExtensions`, `StringExtensions`) are
called throughout these classifier files. Existing golden, property, and characterization
tests remain unchanged.

## Behavior

What should the feature do at a high level?

Each remediated file receives a per-file `#nullable enable` pragma and is brought to zero
CS86xx diagnostics under that pragma with `TreatWarningsAsErrors`. Remediation applies
nullable annotations (`?`), generic constraints (`where T : notnull`), unconstrained `T?`
returns and `out` parameters, null-flow corrections, and null-forgiving operators (`!`)
only where justified. Existing null guards already present in the files remain as-is.

The work is annotation and null-safety only. There are no behavior changes, no refactors,
no API redesign, and no feature work. No classifier scoring path, model logic, or
corpus/probability math is altered. Public method signatures remain behavior-compatible:
an existing caller that compiles today continues to compile and behaves identically. The
annotation choices reflect the true null behavior of each member so that the resulting
signatures are safe contracts, and so that annotations on any interface co-annotated with
its implementer stay consistent.

## Inputs / Outputs

- Inputs (CLI flags, files, env vars): none. This is a library-internal source change with
  no runtime inputs.
- Outputs (artifacts, logs, telemetry): none added. No logging or telemetry is introduced.
- Config keys and defaults: none.
- Versioning or backward-compatibility constraints: public member signatures of the
  remediated classifier types remain behavior-compatible. The observable change is limited
  to nullability annotations, which are additive contract metadata rather than a source- or
  binary-breaking behavior change.

## API / CLI Surface

List commands, flags, request/response shapes, and examples.

There is no CLI surface and no new API. This is a library-internal change. The relevant
"API surface" is the set of nullability annotations applied to the public and protected
members of the remediated classifier types in
`UtilitiesCS/EmailIntelligence/Bayesian`, `.../ClassifierGroups`, and `.../Flags`.

- Example invocations with expected outputs (concise): not applicable; no command or CLI
  flag is added. No `/p:Nullable=enable` global flag is introduced into any verification
  command (see Toolchain Note).
- Contracts and validation rules:
  - Public and protected member signatures remain behavior-compatible; only nullability
    annotations change (for example, an optional `SegmentStopWatch sw = null` parameter
    becomes `SegmentStopWatch? sw = null`; a `default`-returning `Task<T>` becomes
    `Task<T?>`; a null-returning `GetWordInfo` becomes `WordInfo?`).
  - Annotation choices reflect the member's actual null behavior. Because these are T1
    classifier engines, an incorrect annotation on a scoring or corpus path could propagate
    a false null-state assumption; annotate to the true behavior and do not alter math.
  - Where a classifier file calls a #363-annotated extension method, honor that method's
    published nullability contract rather than re-deriving it. In particular,
    `NullExtensions.ThrowIfNull<T>` is `where T : notnull` and returns the non-null value
    with **no** `[NotNull]` post-condition attribute, so a bare `x.ThrowIfNull();` statement
    does **not** narrow the variable's null-state under `#nullable enable`. Reach zero CS86xx
    by capturing the return value, adding a justified `!` with a `// why` comment, or
    annotating an invariant-guaranteed member as non-null. Do not rewrite these into new
    `if (x is null) throw` guards, and do not add a `[NotNull]` polyfill.
  - Nullable post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`,
    `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`,
    `[MemberNotNull]`) are not available or polyfilled on this target and must not be used or
    added (see Constraints & Risks).

## Data & State

Data flow, storage, or state changes introduced by this feature.

- Data transformations and invariants: none changed. This is annotation-only; no runtime
  data flow, transform, scoring computation, or invariant is altered.
- Caching or persistence details: none. Serialization helpers
  (`BayesianSerializationHelper`, `DoNotSerializeContractResolver`, `CorpusInherit`
  JSON paths) keep their existing behavior; only nullable annotations on their signatures
  change.
- Migration or backfill requirements (if any): none. In particular, no project-level
  `<Nullable>` element is introduced into `UtilitiesCS.csproj`; the project has no
  `<Nullable>` element today and must keep none. Enforcement is per-file pragma only, and
  non-remediated files remain null-oblivious and must not be cross-blocked.

## Constraints & Risks

List notable constraints (performance, compatibility, scope) or risks.

- Target framework net481, C# 12 (`LangVersion` 12.0). Nullable syntax is available: `?`,
  `!`, unconstrained `T?`, `where T : notnull`, and `is null` / `is not null` flow analysis.
- Nullable post-condition attributes from `System.Diagnostics.CodeAnalysis` (`[NotNullWhen]`,
  `[MaybeNullWhen]`, `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`,
  `[DoesNotReturn]`, `[MemberNotNull]`) are NOT available on this target and are NOT
  polyfilled in the repository. They must not be used or added. Zero CS86xx is reachable
  without them (proven by the already-enabled `NullExtensions.cs` from #363), using plain
  `?`, `where T : notnull`, unconstrained `T?`, guard clauses, and justified `!`. Adding
  such a polyfill would be new production surface and is out of scope.
- No `init` accessor, positional `record`, or `record struct` may be introduced. net481
  lacks `IsExternalInit`, so those constructs fail CS0518. `FolderHierarchyNode` is an
  existing `sealed record` with get-only auto-properties set in a constructor (no `init`
  accessor) and compiles today; when remediating it, keep the get-only + constructor shape
  and do not add an `init` accessor or convert it to a positional record.
- These are T1 classifier engines. An incorrect annotation on a scoring or corpus path could
  propagate a false null-state assumption. Annotate to the true null behavior; do not alter
  arithmetic, comparisons, constants, clamps, ordering, or control flow. See the DO-NOT-ALTER
  scoring guard list in the Implementation Strategy.
- Prefer annotation plus justified `!` over new runtime guard statements. New
  `if (x is null) throw` statements are executable lines that would require new test coverage
  (AC4 pressure) and could constitute a behavior change (AC3). Existing guards stay as-is.
- The `NullExtensions.ThrowIfNull` no-narrowing property (§0 of research) is the most
  repetitive remediation pattern in this cluster and the most likely place a well-meaning
  "add a guard" edit would violate AC3/AC4. Bare `ThrowIfNull()` statements do not narrow;
  the executor must annotate or add a justified `!` at each dereference rather than convert
  the call into a throwing guard.
- Several in-scope files exceed the general 500-line limit (`BayesianClassifierShared.cs`
  ~1008, `BayesianClassifierGroup.cs` ~515, `CategoryClassifierGroup.cs` ~523,
  `FlagParser.cs` ~633, and, if in scope, `BayesianPerformanceMeasurement.cs` ~1537). This
  is pre-existing. The annotation-only rule forbids splitting them here; flag for a future
  refactor issue and do not split.
- `OlFolder/OlFolderClassifierGroup.cs` is Outlook-interop-bound. Annotation-only work still
  applies; watch for COM-returned reference types that the SDK surfaces as non-nullable but
  can be null at runtime, and use a justified `!` where the COM contract guarantees non-null.
  No behavior change.
- Scope-boundary ambiguity: the `Performance/` subfolder (measurement/benchmark tooling) and
  the `Flags/` subfolder are the two most likely drivers of the estimate gap described below;
  their inclusion is confirmed at Phase 0 before batching (see Implementation Strategy).

## Implementation Strategy

- Implementation scope (what changes, not sequencing): add a `#nullable enable` pragma to
  each in-scope `.cs` file that emits CS86xx under the pragma and bring each to zero CS86xx
  under `TreatWarningsAsErrors`; leave already-clean or excluded files untouched. No project
  or solution file changes.
- New classes/functions/commands to add or update: none. No new types, methods, commands, or
  files are added; only nullability annotations on existing members change.

### Remediation-set determination (behavioral, measured at Phase 0)

The remediation set is defined behaviorally as "every in-scope `.cs` file that emits CS86xx
under the per-file pragma." The definitive set is **measured**, not enumerated in advance, at
Phase 0 by a genuine recompile:

```
msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true
```

(with the narrower project-scoped form
`msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild ... /p:TreatWarningsAsErrors=true` used
per batch). Do NOT pass `/p:Nullable=enable` globally; the global flag forces nullable
solution-wide and surfaces the full pre-existing debt, drowning this child's signal.

Research statically estimates roughly 30–33 candidate files after excluding dead `Obsolete/`
code (6 files), Performance viewers and their Designer-generated partners (4 files),
interface-only files (2 files), and the empty `Bayesian/SpamBayes.cs` stub. The epic's
planning estimate is ~18. The gap is expected and is reconciled at Phase 0 for two reasons:

1. task-researcher cannot compile, so its classification is a static upper bound. The ~18
   figure most plausibly reflects files that will **actually emit CS86xx** after the pragma;
   several candidate files are small and already null-guarded (`PerParentClassifier`,
   `FolderHierarchyNode`, `Prediction`, `ConditionalItemEngine`, `SpamBayes.Conditions`,
   `SpamBayes.Actions`) and may need only a pragma line with zero or near-zero code change,
   or may prove already null-clean.
2. Scope-boundary ambiguity. The `Flags/` subfolder (6 files, including the 633-line
   `FlagParser.cs`) and the `Performance/` subfolder (measurement/benchmark tooling,
   including the 1537-line `BayesianPerformanceMeasurement.cs`) are the two most likely
   subfolder boundaries the estimate may have deferred. Their inclusion is confirmed at
   Phase 0 before batching.

The measured CS86xx set is authoritative; the ~18 figure is treated as a target for files
requiring code edits, and the measured set is reported against it.

### Leaf-first batch grouping (contract-core before consumers)

Ordering respects partial-class co-remediation, shared/base types before consumers, and
subfolder grouping. The definitive task-by-task sequencing belongs to the atomic plan, not
this spec.

- **Batch A — pure data/contract leaves (no in-cluster dependents):** `Prediction.cs`,
  `FolderHierarchyNode.cs`, `LcppnFolderPredictorConfig.cs`,
  `DoNotSerializeContractResolver.cs`, `BayesianClassifierExtensions.cs`. `Prediction<T>` is
  consumed by predictors, so annotate it first.
- **Batch B — corpus/count core (consumed by all scoring):** `Corpus.cs`, `CorpusInherit.cs`.
  `Corpus` is the token-frequency substrate referenced by the scoring engines.
- **Batch C — scoring engine core (highest-risk for the guard list):**
  `BayesianClassifierShared.cs`, then `BayesianClassifierGroup.cs`, then
  `PerParentClassifier.cs`, `FolderHierarchyTree.cs`. Annotate the shared engine before its
  aggregators.
- **Batch D — engine base + generic engines (base before derived):** `TristateEngine.cs`,
  `ConditionalItemEngine.cs`, `MulticlassEngine.cs`, `ManagerAsyncLazy.cs`,
  `ClassifierGroupUtilities.cs`. Annotate the abstract bases first so derived overrides
  inherit consistent nullability and avoid CS8765/CS8767 override-mismatch.
- **Batch E — derived engines + predictors (consume Batches C/D):** the SpamBayes partial set
  (4 files, together), the Triage partial set (2 files, together),
  `ActionableClassifierGroup.cs`, `CategoryClassifierGroup.cs`, `LcppnFolderPredictor.cs`,
  `OlFolder/LcppnFolderPredictorStore.cs`, `OlFolder/OlFolderClassifierGroup.cs`,
  `SpamInitTimingProbe.cs`. Co-annotate `IFolderPredictor.cs` here if implementer nullability
  forces CS8767/CS8766.
- **Batch F — Flags subfolder (confirm scope at Phase 0):** `FlagDetails.cs`,
  `FlagClassNoItem.cs`, `FlagConsolidator.cs`, `FlagTranslator.cs`, `FlagParser.cs`;
  co-annotate `IFlagTranslator.cs` if forced.
- **Batch G — Performance tooling (confirm scope at Phase 0):** `BayesianMetricTypes.cs`,
  `BayesianSerializationHelper.cs`, `BayesianPerformanceMeasurement.cs`. Deferred/last because
  it is measurement tooling, not scoring, and `BayesianPerformanceMeasurement.cs` is a heavy
  (1537-line) surface.

Cross-file ordering constraints: `Prediction` → predictors; `Corpus`/`CorpusInherit` →
scoring engines; `BayesianClassifierShared` → `BayesianClassifierGroup`/`PerParentClassifier`;
`TristateEngine`/`MulticlassEngine` (base) → `SpamBayes`/`Triage`/`Actionable`/`Category`
(derived); `IFolderPredictor`/`IFlagTranslator` co-batch with their implementers.

### Partial-class co-remediation

Partial-class groups must be remediated together in one batch because members are shared
across files:

- **SpamBayes partial set** (`public partial class SpamBayes : TristateEngine, ...`):
  `ClassifierGroups/SpamBayes/SpamBayes.cs`, `SpamBayes.Actions.cs`, `SpamBayes.Classify.cs`,
  `SpamBayes.Conditions.cs` — four files, together. Its abstract base `TristateEngine.cs` is
  annotated before or with this group. Note: the 10-line `Bayesian/SpamBayes.cs` is a
  different, unrelated `internal class SpamBayes {}` stub in the `...Bayesian` namespace and
  is excluded.
- **Triage partial set** (`public partial class Triage`): `ClassifierGroups/Triage/Triage.cs`
  and `Triage_OlLogic.cs` — two files, together.
- **Bayesian partials:** none in scope. `BayesianClassifierShared`, `BayesianClassifierGroup`,
  `Corpus`, `CorpusInherit`, `PerParentClassifier`, and `Prediction` are each single-file
  non-partial types.

### DO-NOT-ALTER scoring guard list

Annotate around these regions without changing any arithmetic, comparison, constant, clamp,
ordering, or control flow. Reaching zero CS86xx must not introduce a new
`if (x is null) throw` on any scoring path, must not reorder operations, and must not change a
`Math.Max`/`Math.Min`/division/log/exp expression. Prefer annotation plus a justified `!`
(with a `// why` comment) or `where T : notnull`.

- **`BayesianClassifierShared.cs` (core engine):** `UpdateProbability` (Paul Graham
  probability incl. `Knobs.MinScore`/`MaxScore` clamps and the `nm == 0` case),
  `UpdateProbabilitySb` overloads (Robinson Bayesian adjustment), `CombineProbabilities`
  (chi/Graham product combine; keep the existing `probabilities is null` throw and
  `Count == 0` early returns), `GetInterestingList`, `GetMatchProbability`,
  `GetProbabilityDrivers`, `MergeProb`/`GetNotMatchIncidence`, `Chi2SpamProb` overloads
  (chi-squared with `frexp` underflow handling, `1e-200` thresholds, `Math.Log(2)` scaling),
  `chi2Q`, `GetClues`, `GetWordDistance`, and the `KnobList` constants (`MinScore=0.011`,
  `MaxScore=0.99`, `UnknownWordProb=0.5`, `UnknownWordStrength=0.45`, `MaxDiscriminators=150`,
  etc.). `Train`/`TrainMultiTag`/`UnTrain`/`UnTrainMultiTag` count-update paths
  (`Interlocked`, `AddOrUpdate`, `UpdateOrRemove(..., out int)`) are DO-NOT-ALTER. Note the
  legitimate nullable returns: `GetWordInfo` may return null (annotate `WordInfo?`, keep the
  existing `if (record is null)` branch in `GetWordDistance`); the `Chi2SpamProb` non-evidence
  path returns `(prob, null)` (annotate the return list element as nullable rather than
  changing the return). `_parent` is nullable-by-construction; annotate its null-state without
  adding a hot-path runtime guard.
- **`PerParentClassifier.cs` (hierarchical-shrinkage Naive Bayes):** `ScoreChildren`,
  `ChildLogScore` (shrinkage blend `λ·P_leaf + (1-λ)·P_parent`, softmax `Normalize`),
  `LaplaceProbability` (add-one smoothing, `LaplaceAlpha = 1.0`), `Normalize` (numerically
  stable softmax incl. the `sum <= 0` uniform fallback). Keep existing guards
  (`ValidateInvariants`, `RequireChildSegment`, `tokens is null` throws) as-is; annotate the
  existing `group = null` default param as `BayesianClassifierGroup? group = null`.
- **`Corpus.cs`:** operator `+`, operator `-`, `SubtractAsync`, `SubtractFilter`
  (token-frequency set arithmetic incl. `negTokenWt`/`minCt` thresholds and
  `TryUpdate`/`TryRemove` flow). Annotate `Clone()`'s `as Corpus` result rather than adding a
  throw; annotate `SubtractAsync(..., SegmentStopWatch sw = null)` as `SegmentStopWatch? sw =
  null` (the existing `sw ??= new(...)` already handles it).
- **`Prediction.cs`:** keep the `CompareTo(Prediction<T> other)` `other is null → return 1`
  contract; annotate the parameter as `Prediction<T>?` to match the existing null check and do
  not alter the `_probability.CompareTo` ordering.
- **`TristateEngine.cs`:** keep `GetTristate(double)`'s threshold decision boundaries
  (`> MinimumTrue` → true, `< MaximumFalse` → false, else null) unaltered. Annotate the
  null-by-default delegate fields (`_tokenize`, `_calculateProbability`, `_getTristateAsync`,
  `_callback`, `_threshhold`, ...) as `Func<...>?` / `Action<...>?` / `TristateThreshhold?`.
- **`MulticlassEngine.cs` / `BayesianClassifierGroup.cs`:** annotate `InitAsync`'s and
  `LoadStagingData`'s `default` returns as nullable (`Task<T?>` / `MinedMailInfo[]?`) rather
  than adding a throw; do not alter the `Condition`/`GetOlItemString` message-class/`IPM.Note`
  behavioral filtering or the `ProbabilityThreshold = 0.8` default; treat all
  probability/aggregation math in `Classify`/`RebuildClassifier` as DO-NOT-ALTER.

General temptations to avoid: (a) adding `if (x is null) throw` on a hot path to silence
CS8602 — use annotation plus a justified `!`; (b) changing a null-returning method
(`GetWordInfo`, the `Chi2SpamProb` non-evidence path) into a throwing method; (c) reordering
`Math.Max`/`Math.Min` clamps to "simplify" null flow.

### Interface co-annotation

`IFolderPredictor.cs` and `IFlagTranslator.cs` are interface-only, emit no CS86xx on their
own, and are excluded from standalone remediation. If a remediated implementer
(`BayesianClassifierGroup`, `LcppnFolderPredictor`, `FlagTranslator`) annotates a parameter or
return as nullable, the compiler emits CS8767/CS8766 unless the interface is co-annotated. In
that case the interface is annotated **in the same batch** as its implementer. This is
annotation-only and does not change the interface's behavior.

- Dependency changes (new/removed packages) and rationale: none.
- Logging/telemetry additions and locations: none.
- Rollout plan (feature flags, staged deploys, fallback path): not applicable. Each remediated
  file is independently mergeable because non-opted-in files remain null-oblivious and are not
  cross-blocking under the per-file pragma architecture.

## Definition of Done

- [ ] Acceptance criteria documented and mapped to tests or demos
- [ ] Behavior matches acceptance criteria in all documented environments
- [ ] Tests updated/added (unit/integration as applicable)
- [ ] Edge cases and error handling covered by tests
- [ ] Docs updated (README, docs/features/active/... links)
- [ ] Telemetry/logging added or updated (if applicable)
- [ ] Toolchain pass completed (format → lint → type-check → test)

Acceptance criteria (from `issue.md`, mapped here for traceability):

- [x] AC1: Every in-scope `.cs` file under `UtilitiesCS/EmailIntelligence/Bayesian`,
  `.../ClassifierGroups`, and `.../Flags` that emits CS86xx carries `#nullable enable` and
  compiles with zero nullable diagnostics under the per-file pragma with
  `TreatWarningsAsErrors`.
- [x] AC2: No project-level `<Nullable>` element is introduced into `UtilitiesCS.csproj`.
- [x] AC3: No behavior change; no change to any classifier scoring or model path; existing
  tests (including golden/property tests) still pass unchanged.
- [x] AC4: No coverage regression on changed lines.
- [x] AC5: Public signatures of remediated members remain behavior-compatible; nullability
  annotations reflect actual null behavior and honor the upstream #363 extension contracts.

## Acceptance Criteria Mapping (traceability)

| AC | Verified by | Guard against regression |
|---|---|---|
| AC1 | Per-batch `msbuild /t:Rebuild ... /p:TreatWarningsAsErrors=true` returns zero CS86xx for every pragma-enabled file in the measured set. | Phase 0 records the measured CS86xx set; each batch closes its subset to zero. |
| AC2 | `git diff UtilitiesCS/UtilitiesCS.csproj` shows no `<Nullable>` element added. | Enforcement is per-file pragma only; non-opted files stay null-oblivious. |
| AC3 | Full `UtilitiesCS.Test` EmailIntelligence suite (golden/property/characterization) passes unchanged. | DO-NOT-ALTER guard list; no scoring/corpus math edits; no new `if (x is null) throw` on scoring paths. |
| AC4 | Per-batch changed-line coverage compared against the Phase 0 baseline. | Prefer annotation + justified `!` over new runtime guards to avoid new uncovered executable lines. |
| AC5 | Signature review of remediated members; base/override and interface/implementer nullability kept consistent. | Honor the #363 contracts (e.g., `ThrowIfNull<T> where T : notnull`, no `[NotNull]` narrowing). |

## Seeded Test Conditions (from potential)

- [ ] Existing `UtilitiesCS.Test` EmailIntelligence suite (including golden/property and
  characterization tests, and the subclass test doubles `SubBayesianClassifier`,
  `SubClassifierGroup`, `SubCorpus` that pin protected/virtual scoring seams) continues to
  pass with no behavior change.
- [ ] Changed-line coverage does not regress relative to baseline (prefer annotation and
  justified `!` over new runtime guards to avoid introducing uncovered executable lines).
- [ ] The pragma-driven nullable gate (`msbuild TaskMaster.sln /t:Rebuild
  /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`) produces zero
  CS86xx diagnostics for the remediated files, without passing `/p:Nullable=enable` globally.

## Toolchain Note

Run the repo C# toolchain in CLAUDE.md order (per batch; restart on any change):

1. `csharpier .` (adding a pragma line and `?` annotations reformats; run before each build).
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"
   /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (analyzers / code style).
3. Nullable verification via the per-file pragma gate:
   `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
   /p:TreatWarningsAsErrors=true` (with the project-scoped
   `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild ...` form used per batch). Under
   `TreatWarningsAsErrors`, any CS86xx in a pragma-enabled file becomes an error while
   non-opted files stay silent. `/t:Rebuild` is mandatory (per PR #361) so the compiler
   performs a genuine recompile rather than a silently-skipped incremental build.
4. `vstest.console.exe <UtilitiesCS.Test assembly> /EnableCodeCoverage`.

Do NOT pass `/p:Nullable=enable` globally for this feature's verification. The global flag
forces nullable solution-wide and surfaces the full pre-existing debt (the #363 plan cites a
~2131 whole-repo diagnostic count), drowning this child's signal. That global-flag-versus-
per-file-pragma mismatch is the rules-versus-convention conflict the epic flags for the
maintainer and defers to the Wave-2 CI capstone child; resolving it is out of scope here, and
`.claude/rules/*` must not be edited.

## Upstream contract dependency (#363)

This feature consumes the annotation contracts published by the Wave-0 Extensions child
(issue #363). See `docs/features/active/utilitiescs-nullable-extensions/spec.md` and
`docs/features/active/utilitiescs-nullable-extensions/plan.2026-07-18T21-20.md`. Consumed
methods include `ThrowIfNull`/`ThrowIfNullOrEmpty` (`NullExtensions.cs`),
`IsNullOrEmpty`/`StringJoin`/`ToFormattedText` (`StringExtensions.cs` / `IEnumerableExtensions.cs`),
`ForEach`/`GroupAndCount`/`GroupAndCountAsync`/`ToDictionary` (`IEnumerableExtensions.cs`),
`UpdateOrRemove(..., out TValue?)` (`DictionaryExtensions.cs`), and
`ToLazy`/`ToAsyncLazy`/`DeepCopy`/`SubtractThreadSafe`. Annotation choices in this feature
honor those published contracts rather than re-deriving them. The load-bearing consequence is
that `ThrowIfNull<T>` is `where T : notnull` with no `[NotNull]` attribute, so bare
`x.ThrowIfNull();` statements do not narrow null-state and require annotation or a justified
`!` at the dereference (see the API / CLI Surface contracts above).
