# Phase 0 — Instructions Read (Issue #418)

Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`
Tasks covered: `[P0-T2]`, `[P0-T3]`, `[P0-T4]`

Timestamp: 2026-08-04T14-55

---

## [P0-T2] Policy Reads

Policy Order: `CLAUDE.md` -> `.claude/rules/general-code-change.md` -> `.claude/rules/general-unit-test.md` -> `.claude/rules/csharp.md`

Each file was read in full, in that exact order, with the `Read` tool.

### Files read

| # | Path | Lines | Read in full |
| --- | --- | --- | --- |
| 1 | `CLAUDE.md` | 442 | yes |
| 2 | `.claude/rules/general-code-change.md` | 81 | yes |
| 3 | `.claude/rules/general-unit-test.md` | 106 | yes |
| 4 | `.claude/rules/csharp.md` | 97 | yes |

### Constraints carried into Phase 1 and Phase 2

- C# toolchain order is format (`csharpier`) -> lint (analyzer build) -> type-check
  (nullable build) -> test (`vstest` with coverage). Any step that fails or rewrites files
  restarts the loop from step 1.
- Do not use `dotnet format`; `csharpier` only, and formatter output wins over hand
  formatting.
- 500-line ceiling applies to every production file, test file, and reusable script.
  Markdown documentation is exempt.
- Tests: MSTest framework, Moq for mocking, FluentAssertions for assertions,
  Arrange-Act-Assert structure.
- Determinism: no temporary files (`UT4`, zero approved exceptions), no network, no
  external processes, no `Thread.Sleep` / `Task.Delay`.
- Coverage: repository-wide line coverage `>= 85%` and branch coverage `>= 75%` per
  `.claude/rules/general-unit-test.md`; `.claude/rules/csharp.md` and `CLAUDE.md` state a
  `>= 80%` repository-wide line floor. The stricter `>= 85%` line floor governs.
  New or changed modules, classes, and methods must reach `>= 90%`. Coverage regression on
  changed lines is a blocking finding.
- Error handling: fail fast and explicitly; a bare `catch { }` that silently swallows is a
  policy violation. Broad `catch (Exception)` is permitted only at a defined boundary and
  only when context is added — this is the basis for the plan's single-catch-site design.
- DI seams: prefer the smallest seam. Interface seam first, then an injectable
  `Func<>`/`Action<>` delegate seam, then an adapter seam. The plan's
  `Func<byte[], SvgDocument>` parse seam is the second option and is consistent with policy.
- Analyzer severity-first invariant: new analyzer rule severities are configured at
  `suggestion` in `.editorconfig` before any `<Analyzer Include>` wiring, because the
  type-check step promotes `warning` severities to errors.
- Prohibited: broad refactors across unrelated projects, weakening assertions to make tests
  pass, and reporting success without running the required toolchain.

---

## [P0-T3] Requirements Source and Fail-Closed Check

Timestamp: 2026-08-04T14-57

AC source: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md`, the explicit `## Acceptance Criteria` section (heading found at `issue.md:70`)

AC count: 11

Work Mode: minor-audit (marker `- Work Mode: minor-audit` found at `issue.md:12`)

Fail-closed check:

- `spec.md: absent`
- `user-story.md: absent`

### Verification detail

| Check | Command | Result |
| --- | --- | --- |
| `## Acceptance Criteria` heading present | `grep -n '^## Acceptance Criteria$' issue.md` | `70:## Acceptance Criteria` |
| Work-mode marker present | `grep -n '^- Work Mode: minor-audit$' issue.md` | `12:- Work Mode: minor-audit` |
| AC items in that section, unchecked | `awk '/^## Acceptance Criteria$/{f=1;next} /^## /{f=0} f' issue.md \| grep -c '^- \[ \] \*\*AC-'` | `11` |
| AC items in that section, checked | `awk '/^## Acceptance Criteria$/{f=1;next} /^## /{f=0} f' issue.md \| grep -c '^- \[x\] \*\*AC-'` | `0` |
| `spec.md` present | `ls spec.md` | `No such file or directory` |
| `user-story.md` present | `ls user-story.md` | `No such file or directory` |
| Recursive presence check | `find . -iname 'spec.md' -o -iname 'user-story.md'` | no matches |

SearchScope: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/` (top level and recursive)
SearchPatterns: `spec.md`, `user-story.md` (case-insensitive)
SearchResult: none

Feature folder contents observed: `evidence/`, `issue.md`, `plan.2026-08-04T14-36.md`,
`research/`, `runbooks/`. No `spec.md` and no `user-story.md`, which matches the plan's
Work-Mode Notes. `MODE_FAIL_CLOSED` is **not** triggered; execution continues.

### Acceptance criteria enumerated (all `- [ ]` at Phase 0 completion)

| AC | Short title | State |
| --- | --- | --- |
| AC-1 | Failing regression test exists first | `- [ ]` |
| AC-2 | No silent exception swallow | `- [ ]` |
| AC-3 | Parse failure degrades visibly instead of throwing NRE | `- [ ]` |
| AC-4 | Fail-fast API exists; null-tolerant call sites keep contract | `- [ ]` |
| AC-5 | Coverage on changed code | `- [ ]` |
| AC-6 | Toolchain passes in a single clean pass | `- [ ]` |
| AC-7 | Underlying failure identified in writing | `- [ ]` |
| AC-8 | `AssemblyResolve` fallback resolves from the assembly's own directory | `- [ ]` |
| AC-9 | `SVGControl.Test` builds and runs | `- [ ]` |
| AC-10 | Incorrect ExCSS redirect in the test config is corrected | `- [ ]` |
| AC-11 | Designer load verified by the documented human step | `- [ ]` |

Phase 0 delivers no acceptance criterion. All eleven remain unchecked.

---

## [P0-T4] Research and Runbook Reads

Timestamp: 2026-08-04T15-02

Both documents were read in full with the `Read` tool.

### Files read (research and runbook)

| # | Path (relative to repository root) | Lines | Read in full |
| --- | --- | --- | --- |
| 5 | `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/research/2026-08-04T15-05-svg-renderer-null-document-research.md` | 607 | yes |
| 6 | `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/runbooks/verify-winforms-designer-load.runbook.md` | 283 | yes |

Paths relative to the feature folder, as named in the plan's Required References:

- `research/2026-08-04T15-05-svg-renderer-null-document-research.md`
- `runbooks/verify-winforms-designer-load.runbook.md`

### Findings carried into Phase 1

From the research artifact:

- Sections 1.1 and 1.2 fix the defect surface: `GetSvgDocument` at
  `SVGControl/SvgRenderer.cs:320-331` swallows every exception and returns `null`; both
  byte-array constructors (lines 126-142) dereference it at lines 129 and 138.
- Section 1.4 establishes the two distinct null-producing paths from
  `SvgDocument.Open<SvgDocument>`: malformed input throws, while element-free input returns
  `null` without throwing. Only the first can carry an `InnerException`. This is the
  asymmetry the plan's `GetSvgDocumentOrThrow` design records.
- Section 2.2 rules out removing the `<style>` element from the default SVG: the ExCSS bind
  occurs when `SvgDocument.Create<T>` is JIT-compiled, not when the `styles.Any()` branch is
  taken. The plan lists that approach as explicitly out of scope.
- Section 4.3 explains why the existing `AssemblyResolve` fallback returns `null` in the
  designer host: `Assembly.Load` binds against the host AppDomain's `ApplicationBase`, which
  is the Visual Studio directory, not the directory containing `SVGControl.dll`. Section 4.4
  gives the ordered candidate-directory remedy that the plan's `[P1-T16]` through `[P1-T18]`
  implement, and the instruction to preserve strategy 1 ordering.
- Section 5.3 scopes the Fizzler redirects out of this change (13 files, provably inert,
  separate issue).
- Section 8.3 is the blocking prerequisite this Phase 0 baseline must record: `SVGControl.Test`
  is absent from `TaskMaster.sln`, its pinned packages are absent from `packages/`, and the
  `EnsureNuGetPackageBuildImports` `<Error>` guard blocks its build. Task `[P0-T10]` captures
  that state as observed rather than as asserted.
- Section 8.5 forbids a test that asserts the `AssemblyResolve` handler is absent, because the
  handler is process-wide and permanently installed. It also requires disposing any `Bitmap`
  produced by a success-path assertion.
- Section 9.2 (H-1/H-2) establishes that the designer-load check is not automatable and must
  be performed by a human, which is the basis for `[P2-T10]` and for leaving AC-11 unchecked.
- Sections 9.3 (U-1, U-2) record two items that remain unverified by design.

From the runbook:

- The cue is explicit: run it after the toolchain is green (AC-6) and before the feature is
  reported done. A pre-fix run produces no usable evidence.
- Three outcomes are defined (Pass, Partial pass, Fail). A `NullReferenceException` reported
  anywhere is a Fail.
- The mandatory evidence path is
  `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md`.
  `artifacts/`-rooted paths are blocked by `.claude/hooks/enforce-evidence-locations.ps1`.
- The runbook is a human action. The executor must not automate it and must leave AC-11
  unchecked.
