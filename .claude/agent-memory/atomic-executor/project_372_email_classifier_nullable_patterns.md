---
name: 372-email-classifier-nullable-patterns
description: Recurring annotation patterns and gotchas found remediating the UtilitiesCS EmailIntelligence classifier cluster (#372) under per-file #nullable enable
metadata:
  type: project
---

Wave-1 child #372 (`utilitiescs-nullable-email-classifier`) remediated 36 files under `UtilitiesCS/EmailIntelligence/{Bayesian,ClassifierGroups,Flags}`. Measured CS86xx set was **30 emitting files / 188 diagnostics** (research's ~30 static estimate, NOT the epic's ~18); both `Flags/` and `Performance/` were confirmed in scope. Reusable patterns for sibling children:

- **Engine properties set post-construction** (via a builder / `InitAsync` / property-setter that the compiler doesn't track as ctor-init) → `= null!` (or `= default!` for unconstrained `T`). Applies to `Globals`, `ClassifierGroup`, `EngineName`, `TypedItem`, `AsyncAction`, `CgUtilities`, delegate fields, `Combined`, lazy caches. Keeps `IsActivated => X is not null` and hot-path derefs working with no runtime change. Property-setter assignment (`Parent = x`) does NOT satisfy CS8618 for the backing field — hence `= null!` on the field.
- **Factory/Init returns** that already `return null`/`return default` → `Task<T?>` / `T?`; this cascades to callers (`CreateEngineAsync`, `ValidateJson`, destructuring sites) which then also go nullable.
- **Prediction<T>.Class became `T?` in Batch A** → cascades everywhere `.Class` feeds a non-null `string` (Triage/Actionable/Category); fix with `.Class!` (classify results are populated in practice).
- **#363 ThrowIfNull no-narrowing**: after a bare `X.ThrowIfNull()`, the next deref needs a justified `X!` with a `// why` comment (delegates set in InitAsync alongside ClassifierGroup). Never convert to `if (x is null) throw`.
- **`x as Corpus`/`as MailItem`/MemberwiseClone** → wrap `(x as T)!` (Clone/cast always yields T).
- **net481 `string.IsNullOrEmpty(x)` does NOT narrow** — after the check, x still needs `!`.
- **DTO auto-property `= null!` adds an executable line** (BayesianMetricTypes went 97%→93% because a few measurement DTOs aren't instantiated in tests). Not an AC4 regression (no previously-covered line lost coverage; overall coverage rose) but prefer nullable `?` where consumers don't deref-as-non-null to avoid new uncovered lines.
- **`await Deserialize()!` gotcha**: `x = await Deserialize(...)!` binds `!` to the Task (result stays T?). Must write `x = (await Deserialize(...))!` to null-forgive the awaited result.
- **FlagDetails.List setter handles null** → `SetXList` assignments use `.List = value!`. Nullable events: `event Handler? Name;`.
- Interfaces `IFolderPredictor`/`IFlagTranslator` were NOT forced (no CS8766/8767) — left EXCLUDE.

See [[project_nullable_remediation_annotation_patterns]], [[project_nullable_epic_pragma_gate_and_analyzer_restore]], [[project_analyzer_version_skew_fresh_worktree]].
