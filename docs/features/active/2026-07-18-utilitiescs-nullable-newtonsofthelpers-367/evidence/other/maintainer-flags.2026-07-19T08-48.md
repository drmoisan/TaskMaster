# Maintainer Flags — utilitiescs-nullable-newtonsofthelpers (#367)

This artifact records cross-cutting flags surfaced during execution. It is appended to by later per-batch flag tasks (P3-T3, P6-T4, P7-T1).

## P0-T6 — Cross-cutting flags

- Timestamp: 2026-07-19T08-48

### (a) Pragma-only verification-command deviation (deliberate, documented, NOT resolved here)

The nullable / type-check verification step for this child uses the pragma-only build and MUST NOT add `/p:Nullable=enable`:

`msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`

Rationale: adding `/p:Nullable=enable` turns nullable ON project-wide and surfaces the entire epic's ~2131 pre-existing CS86xx diagnostics across ~234 files as false failures unrelated to issue #367. Enforcement for this child is per-file `#nullable enable` pragma only. This is a deliberate, documented per-child deviation from the stock `.claude/rules/csharp.md` / `CLAUDE.md` type-check command. It is NOT resolved by editing `.claude/rules/*`, and `UtilitiesCS.csproj` keeps no `<Nullable>` element.

### (b) Rules-vs-convention conflict (FLAGGED for the epic capstone child, NOT resolved here)

`.claude/rules/csharp.md` (Toolchain item 3) documents the type-check command as `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` — forcing nullable globally. This conflicts with the per-file `#nullable enable` opt-in convention used by this epic. The conflict is flagged for the epic capstone child and is NOT resolved in this feature; no `.claude/rules/*` file is edited.

### (c) Pre-existing analyzer-package version mismatch in committed csprojs (environment bootstrap, NOT a feature change)

Every first-party `.csproj` `<Analyzer Include>` item references analyzer versions (Meziantou.Analyzer 3.0.101, SonarAnalyzer.CSharp 10.27.0.140913, Microsoft.CodeAnalysis.BannedApiAnalyzers 3.3.4) that differ from the versions in `packages.config` (3.0.123 / 10.29.0.143774 / 5.6.0). `nuget restore` restores only packages.config versions, so the stale `<Analyzer Include>` paths do not resolve and the build fails CS0006 until the referenced versions are also present. This inconsistency is PRE-EXISTING on both this branch HEAD and `origin/main`. It was worked around by installing the referenced analyzer versions into the gitignored `packages/` folder (environment bootstrap; no tracked-file edit). Flagged for maintainer awareness; not fixed here (fixing would require editing every `.csproj`, which is out of scope).

### (d) Pre-existing warnings that TreatWarningsAsErrors promotes to build errors (NOT nullable, NOT in scope)

Under `/p:TreatWarningsAsErrors=true` (no `/p:Nullable=enable`), the solution build fails on PRE-EXISTING non-nullable warnings that are unrelated to `NewtonsoftHelpers/`:
- vendored `SVGControl/SvgImageSelector.cs`: 2x `CS0649` (field never assigned) — blocks the whole solution because `UtilitiesCS` depends on `SVGControl`;
- `UtilitiesCS` production: 14x `CS0618` (obsolete `System.Linq.AsyncEnumerable` overloads) + 1x `CS0168` (unused variable).

Consequently the exact solution-level plan command cannot, by itself, compile `NewtonsoftHelpers/` to exercise the nullable state. The genuine per-file CS86xx verification (documented in `evidence/baseline/nullable-build-baseline.2026-07-19T08-48.md`) compiles `UtilitiesCS.csproj` under TreatWarningsAsErrors with ONLY these pre-existing non-nullable codes exempted (`/p:WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168`), leaving `CS86xx` fatal; this gate is GREEN at baseline. Flagged for maintainer awareness; these pre-existing warnings are not remediated by this feature.

## P3-T3 — NLogTraceWriter.cs GLOBAL namespace flag

- Timestamp: 2026-07-19T08-48

`UtilitiesCS/NewtonsoftHelpers/NLogTraceWriter.cs` declares its class `NLogTraceWriter` in the GLOBAL namespace (there is no `namespace` block; the class sits directly at file scope). This is a PRE-EXISTING structural oddity, not a nullable issue. The file was annotated in place (`#nullable enable` at the top, `Exception? ex`, `Action<string, Exception?>? GetLogFunction`, behavior-preserving `!` on `GetCurrentMethod()!.DeclaringType!`) with the namespace left unchanged. Moving the class into a namespace would be an out-of-scope reference/behavior change (it would alter the type's fully-qualified name and could break references). Flagged, not "fixed."

## P6-T4 — Three wrapper files exceed the 500-line limit (PRE-EXISTING, flagged not fixed)

- Timestamp: 2026-07-19T08-48

The three dictionary-wrapper files exceed the repo 500-line limit as a PRE-EXISTING condition (same handling as `PrettyPrint.cs` in sibling #364). Annotation-only work adds a `#nullable enable` line plus per-line annotations/comments and cannot bring these under 500 without a refactor, which is out of scope. The files are NOT split.

- `UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs` — was ~645 lines pre-feature, 649 after annotation.
- `UtilitiesCS/NewtonsoftHelpers/WrapperPeopleScoDictionaryNew.cs` — was ~607 lines pre-feature, 615 after annotation.
- `UtilitiesCS/NewtonsoftHelpers/WrapperScDictionary.cs` — was ~520 lines pre-feature, 524 after annotation.

The small line growth is from the added `#nullable enable` pragma and the `// why` comments documenting the deliberate `= null!`/`!` decisions; all three were already over 500 before this feature. Flagged for the maintainer; not remediated here.

## P7-T1 — Duplicate PeopleScoConverter: which copy is live

- Timestamp: 2026-07-19T08-48

Two files named `PeopleScoConverter.cs` exist:
- IN SCOPE (LIVE): `UtilitiesCS/NewtonsoftHelpers/PeopleScoConverter.cs` — declares `public class PeopleScoConverter : JsonConverter<PeopleScoDictionaryNew>` under namespace `ToDoModel.Data_Model.People` (active code).
- OUT OF SCOPE (DEAD): `ToDoModel/Data Model/People/PeopleScoConverter.cs` — its `namespace` (line 9) AND class declaration (line 11) are BOTH commented out; the file declares no live type.

Registration evidence: every `new PeopleScoConverter()` call site — production `UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs:127`, plus `ToDoModel.Test/.../PeopleScoDictionaryNewTests.cs:267` and `UtilitiesCS.Test/NewtonsoftHelpers/PeopleScoConverter_Tests.cs` — resolves to the single live type, which is the in-scope `NewtonsoftHelpers/` copy (the only compiled `PeopleScoConverter` in namespace `ToDoModel.Data_Model.People`). The ToDoModel copy contributes nothing (fully commented out).

Decision: ONLY the in-scope `UtilitiesCS/NewtonsoftHelpers/PeopleScoConverter.cs` is annotated by this feature (P7-T4). The out-of-scope commented-out `ToDoModel/` copy is left unchanged.
