# Feature Audit — utilitiescs-nullable-svgcontrol (Issue #368)

- Feature branch: `feature/utilitiescs-nullable-svgcontrol-368`
- Base: `origin/epic/utilitiescs-nullable-remediation-integration` @ `6d4da8bb4d881dc26c421440464ce5575e3fb15f`
- Head: `c194362d612497f1fd5a6ee36aec7f52c4b949d4`
- Work mode: `full-feature` — AC sources: `spec.md` and `user-story.md` (both list identical AC1–AC6;
  `issue.md` also lists them, already checked off by the executor and independently re-verified
  here per the reviewer check-off protocol)
- Timestamp: 2026-07-19T11-15

## Summary

All 6 acceptance criteria (AC1–AC6) are independently verified **PASS**. The plan's 47/47 tasks
are checked off and each task's claimed evidence artifact was inspected; the build, format, and
test claims were independently reproduced rather than merely read. The two flagged
maintainer-judgment deviations (making `ISvgResource` nullable; annotating additional members not
named in the plan's literal task text) are evaluated as legitimate, necessary consequences of the
stated per-file zero-CS86xx architecture, not scope creep. This audit does not resolve the
separately-tracked coverage-artifact gate (see `policy-audit`), which is a procedural/systemic
finding independent of AC satisfaction.

## Scope and Baseline

- Baseline: `origin/epic/utilitiescs-nullable-remediation-integration` @
  `6d4da8bb4d881dc26c421440464ce5575e3fb15f` (recomputed independently via `git merge-base HEAD
  origin/epic/utilitiescs-nullable-remediation-integration`; matches the caller-supplied base).
- In-scope files per `spec.md`: 20 total `.cs` files under `SVGControl/` — 12 hand-authored
  remediation targets, 3 already-`#nullable enable` verify-only files, 5 Designer/generated files
  not opted in. Independently re-enumerated via `find SVGControl -maxdepth 2 -iname "*.cs"`: exactly
  20 files found, matching the spec's inventory by name in every group.
- Diff actually touches: the 12 hand-authored files (confirmed via `git diff --stat`), plus one
  unrelated PowerShell tooling fix (`scripts/vscode/Invoke-MSTestWithCoverage.ps1`) and feature-doc/
  evidence artifacts. Zero diff on the 3 verify-only files and zero diff on the 5
  Designer/generated files (independently re-confirmed via `git diff --stat` against those exact
  8 paths).

## Acceptance Criteria Inventory

| ID | Criterion | Source |
|---|---|---|
| AC1 | Every hand-authored `.cs` file in `SVGControl/` that emits CS86xx carries `#nullable enable` and compiles with zero nullable diagnostics under the per-file pragma with `TreatWarningsAsErrors`. | `spec.md`, `user-story.md`, `issue.md` |
| AC2 | No project-level `<Nullable>` element is introduced into `SVGControl.csproj`, and no `<Nullable>` element is introduced at the solution level. | `spec.md`, `user-story.md`, `issue.md` |
| AC3 | No behavior change; existing tests still pass. | `spec.md`, `user-story.md`, `issue.md` |
| AC4 | No coverage regression on changed lines. | `spec.md`, `user-story.md`, `issue.md` |
| AC5 | Public signatures of the remediated control, parser, and converter types remain behavior-compatible; nullability annotations reflect actual null behavior. | `spec.md`, `user-story.md`, `issue.md` |
| AC6 | WinForms `*.Designer.cs` and generated `Properties/Resources.Designer.cs` files remain consistent with the pragma build; any edit to them is mechanical and behavior-preserving. | `spec.md`, `user-story.md`, `issue.md` |

## Acceptance Criteria Evaluation

### AC1 — Zero CS86xx under per-file pragma

**PASS (independently re-verified).** Ran `msbuild TaskMaster.sln /t:Rebuild
/p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` directly in this
review (not merely read from evidence): build log shows 6 total errors, and
`grep -c "CS8[0-9][0-9][0-9]"` on the full log returns **0**. The 6 errors are exactly the 2
pre-existing `CS0649` diagnostics in `SvgImageSelector.cs` and 4 pre-existing `CS0006` diagnostics
in `VBFunctions.csproj` (neither touched by this feature's diff, confirmed via `git diff --stat`).
This matches `evidence/qa-gates/final-nullable-pragma-gate.md` exactly. All 12 hand-authored files
carry the pragma (confirmed via `git diff`); the 3 verify-only files remain untouched and already
carried it.

### AC2 — No project/solution-level `<Nullable>`

**PASS (independently re-verified).** `grep -n "Nullable" SVGControl/SVGControl.csproj` and
`grep -n "Nullable" TaskMaster.sln` both return 0 matches, re-run directly in this review.

### AC3 — No behavior change; existing tests still pass

**PASS (independently re-verified).** Rebuilt `SVGControl.Test.csproj` and ran
`vstest.console.exe` directly against `SVGControl.Test.dll` in this review: "Total tests: 37,
Passed: 37, Failed: 0" — matching `evidence/qa-gates/final-tests-coverage.md` exactly. Every
nullable-annotation edit reviewed line-by-line (Section "Deviation Analysis" below and the
`code-review` findings table) is additive metadata reflecting already-observed null behavior; no
new guard/throw statement, no new fallback value, and no renamed/removed member was found in the
diff. The one behaviorally-sensitive judgment call (`SvgImageSelector.ImagePath`) was resolved
conservatively (null-forgiving `!`, not a new fallback), preserving the exact pre-existing return
value on every code path, documented in `evidence/other/imagepath-judgment-call-decision.md` and
independently re-read against the applied code.

### AC4 — No coverage regression on changed lines

**PASS, with a documented, pre-existing, numerically-vacuous condition for 11 of 12 files.**
`RelativePath.cs` (the only file in scope with a genuine automated-test baseline) is verify-only
and untouched; its coverage is byte-identical before/after (line-rate 56.75%, branch-rate 54.35%,
independently spot-checked against the raw XML headline attributes in
`evidence/qa-gates/final-coverage.cobertura.xml` and `evidence/baseline/baseline-coverage.cobertura.xml`).
No line in any of the 12 remediated files that had non-zero coverage before this feature lost
coverage after it — because none of the 12 had any covered lines before (confirmed: `SVGControl.Test`
exercises only `RelativePath.cs`). The package-level `SVGControl` line-rate movement (26.65% ->
26.64%) is a strict non-regression: `lines-covered` is unchanged at 870 in both baselines, and the
entire delta is attributable to 2 new instrumentable-but-never-covered lines added by the pragma
edits. This satisfies the literal "no regression on changed lines" AC text. It does not by itself
satisfy the separate, repo-wide coverage-artifact/threshold gate tracked in `policy-audit` (Section
1.2.1), which is recorded there as a FAIL for procedural/systemic reasons unrelated to AC4's
narrower regression test.

### AC5 — Public signature behavior-compatibility

**PASS (independently re-verified).** Reviewed the full diff of all 12 files against
`evidence/qa-gates/final-signature-compat.md`'s per-file table; every public-signature change
(e.g., `ButtonSVG.ObjectToByteArray(object? obj)`, `SvgRenderer.Render(): Bitmap?`,
`SvgImageSelector.ResourceName: ISvgResource?`) is an additive nullability annotation on a
parameter, return type, property, field, or event that reflects behavior the implementation
already exhibited (documented guard clauses, `null`-returning paths, or `[ThreadStatic]`
lazy-initialization semantics that predate this feature). No parameter was added or removed, no
method renamed, no access modifier changed, no overload added or removed.

### AC6 — Designer/generated files remain consistent

**PASS (independently re-verified).** `git diff --stat` and `git status --short` against the exact
5 named paths (`ButtonSVG.Designer.cs`, `PictureBoxSVG.Designer.cs`, `ToggleSwitch.Designer.cs`,
`Properties/Resources.Designer.cs`, `Properties/AssemblyInfo.cs`) both return no output, re-run
directly in this review — confirming zero edits to any of them, consistent with the plan's
research finding that none required a change to keep the pragma build clean.

## Deviation Analysis (Judgment Calls Beyond the Plan's Literal Text)

### Deviation 1 — `ISvgResource.Name`/`.Data` made nullable (not just `SvgResource`'s implementation)

**Judged legitimate, not scope creep.** `SvgResource`'s parameterless constructor never assigns
`Name`/`Data`; annotating only the concrete class's properties as nullable while leaving the
interface's declarations non-nullable produces a genuine `CS8766` compiler diagnostic (an
implementation returning a "more nullable" type than its interface contract declares). Reaching
"zero CS86xx" — the literal text of AC1 — is impossible without also annotating the interface.
The change is confined to `SVGControl/ISvgResource.cs` itself (both the interface and its one
implementing class in the same file); `SVGControl/` has no `ProjectReference` from any other
epic-cluster project (confirmed by `spec.md`'s own research and unchanged by this review), so this
annotation does not propagate as a cross-module contract obligation to any other Wave-0/Wave-1
epic child. This is the correct, minimal fix for the actual compiler error, not an expansion of
scope.

### Deviation 2 — Additional members annotated beyond the plan's literal task text

Members: `SvgRenderer.PropertyChanged`, `SvgRenderer._resolving`,
`SvgRenderer.ResolveByNameAndKey`, `SvgImageSelector.PropertyChanged`, `SvgImageSelector.Render()`,
`SVGFileNameEditor._ofd`.

**Judged legitimate, not scope creep.** Each is documented per-batch
(`evidence/qa-gates/batch-a-nullable-gate.md`, `batch-c-nullable-gate.md`, `batch-d-nullable-gate.md`,
independently re-read in this review) with a specific rationale tied to the member's actual,
pre-existing null-state behavior:
- `PropertyChanged` events (both classes) already used the null-conditional `?.Invoke` pattern
  before this feature; annotating the event as nullable is a direct reflection of that
  pre-existing pattern, not a new behavior.
- `_resolving` is `[ThreadStatic]`, meaning it is genuinely `null` on every thread until its
  pre-existing `??=` lazy-initializer runs; this is accurately modeled as nullable, not
  newly introduced.
- `ResolveByNameAndKey` already had two `return null;` paths before this feature (confirmed via
  `git diff` context lines showing the `return null;` statements are unchanged, pre-existing code).
- `Render()` (on `SvgImageSelector`) is a direct passthrough of the already-nullable
  `SvgRenderer.Render()`.
- `_ofd` is only ever assigned inside `InitializeDialog`, which the `EditValue` method's existing
  `if (_ofd != null)` guard already protects against; the field was genuinely nullable in every
  code path before this feature touched it.

None of these six introduces a new guard clause, a new fallback value, or a renamed/removed
member; all are additive annotations reaching the literal "zero CS86xx" bar that AC1 requires.
Per the acceptance-criteria-tracking reviewer protocol, a deviation that is a necessary and
correctly-documented consequence of satisfying the stated AC text is not treated as an
unauthorized scope expansion.

### `SvgImageSelector.ImagePath` judgment call (Phase 3)

**Judged correct and conservatively resolved.** Independently re-read
`evidence/other/imagepath-judgment-call-decision.md` against the applied code at
`SVGControl/SvgImageSelector.cs`'s `ImagePath` getter. The chosen resolution
(`return _relativeImagePath!;` with an in-code comment, rather than a `?? "(none)"` fallback)
preserves the exact pre-existing return value on every reachable code path, including the
currently-unreachable-in-practice `else` branch (the setter is entirely commented out today). A
`?? "(none)"` fallback would have silently reused an existing sentinel value for a distinct
null-state condition — a genuine, CI-undetectable behavior change that AC3 does not permit. This
is the single most consequential decision in the cluster and was treated as such (a dedicated
decision document, not a routine batch-gate line item), consistent with the spec's explicit
instruction.

## Acceptance Criteria Check-off

All 6 acceptance criteria are independently verified PASS in this review. `issue.md` already had
all 6 items checked off (`[x]`) by the executor; this review confirms each check-off is warranted
and leaves them checked (per the reviewer check-off protocol, no further edit to `issue.md` was
required since all criteria are genuinely satisfied). No AC item is downgraded to unchecked.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/issue.md` (`## Acceptance
  Criteria`), cross-verified against identical AC text in `spec.md` and `user-story.md`
- Total AC items: 6
- Checked off (delivered): 6
- Remaining (unchecked): 0
- Items remaining: none

## Verdict

**Ready to merge into the epic integration branch from an acceptance-criteria and
code-correctness standpoint.** All AC1–AC6 are independently verified PASS. Merge readiness is
separately gated by the procedural coverage-artifact finding recorded in `policy-audit` and
`remediation-inputs` (absent canonical `artifacts/csharp/coverage.xml` and
`artifacts/pester/powershell-coverage.xml`), which is a systemic, pre-existing environment gap and
not a defect in this feature's `SVGControl/` changes.
