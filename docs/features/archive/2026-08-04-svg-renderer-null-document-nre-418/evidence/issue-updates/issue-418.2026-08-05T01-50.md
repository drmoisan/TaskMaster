# Issue #418 Update Mirror — Remediation Cycle 1 Evidence-Note Amendments

Timestamp: 2026-08-05T02-12 (UTC)

- Task: `[P2-T10]`
- Issue: #418 — https://github.com/drmoisan/TaskMaster/issues/418
- Target file amended: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md`
- Evidence series: `2026-08-05T01-50`

PostedAs: not posted

POSTING BLOCKED — reason: this cycle's `[P2-T10]` scope is **append-only evidence-note amendments to the
local feature `issue.md`**, and the plan's Scope Lock authorizes no GitHub write. No instruction in the
remediation plan or the execution directive authorizes posting to the GitHub issue, and the amendments are
citation and figure updates for the reaudit rather than a status change a reader of the GitHub issue needs.
Posting the corresponding update to GitHub is the orchestrator's / PR-authoring step, not this executor's.
Recorded here so the omission is auditable rather than silent.

## Constraints observed

- `git diff --numstat` for `issue.md`: **6 insertions, 0 deletions** — additions only.
- Amendments were appended under **AC-2, AC-5, and AC-8 only**.
- **No AC text was rewritten.** No line beginning `- [ ]` or `- [x]` changed state, verified by
  `git diff -U0 | grep -cE "^[-+].*- \[[ x]\]"` returning **0**.
- **AC-1 through AC-10 remain `[x]` and AC-11 remains `- [ ]`.** AC-11 is R-1, the human WinForms-designer
  load runbook, which is excluded from this plan; no automated evidence substitutes for the human capture
  at `evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md`.

## Exact text appended, verbatim

The three paragraphs below are the exact inserted lines, reproduced byte-for-byte from
`git diff -- issue.md`.

### Appended under AC-2


  Evidence-note amendment 2026-08-05 (remediation cycle 1, task `[P2-T10]`). Two updates to the citations above; **the criterion's text and its `[x]` state are unchanged, and its substantive requirement is unchanged and still met.** (1) **Relocation.** R-6 moved the entire `AssemblyResolve` region out of `SVGControl/SvgRenderer.cs` into the new file `SVGControl/SvgAssemblyResolver.cs` (`internal static class SvgAssemblyResolver`), and moved `PublicKeyTokensEqual` to `SVGControl/SvgAssemblyProbe.cs`. The two resolver catch sites cited above as "lines 99, 131" now live in `SVGControl/SvgAssemblyResolver.cs`; the parse-path boundary cited as "line 435" remains in `SVGControl/SvgRenderer.cs`. The move is behavior-preserving — `SvgRenderer`'s static constructor is retained and calls `SvgAssemblyResolver.Install()`, so the handler still installs exactly once per AppDomain. Evidence: `evidence/other/resolver-extraction.2026-08-05T01-50.md`. (2) **Catch-site inventory gains one entry.** R-3 added a containment `catch (Exception ex)` to the outer `try` in `SvgAssemblyResolver.ResolveByNameAndKey`, so that `try` now has exactly one catch and one finally, and `Path.Combine`, `self.Location`, and `self.CodeBase` can no longer raise out of the handler. Like the two catches already present, it uses **`Trace.TraceWarning` and not `log4net`**, for the documented re-entrancy reason (a `log4net` call inside an `AssemblyResolve` handler can itself trigger a re-entrant assembly load). The inventory is therefore three resolver catches plus the one parse-path boundary, all four declaring `Exception ex` and all four logging rather than discarding; zero bare `catch` blocks remain. A known residual is recorded: the pre-guard region (`new AssemblyName(args.Name)` and `loaded.GetName()`) stays outside the new catch, with the reason given in the remediation plan's Design Decision 11. Evidence: `evidence/other/resolver-containment.2026-08-05T01-50.md` and `evidence/qa-gates/analyzer-build.2026-08-05T01-50.md` (`EXIT_CODE: 0`, 0 errors, 0 new diagnostics).

### Appended under AC-5


  Evidence-note amendment 2026-08-05 (remediation cycle 1, task `[P2-T10]`). **The coverage figures cited above are superseded by `evidence/qa-gates/coverage-delta.2026-08-05T01-50.md`.** The criterion's text and its `[x]` state are unchanged, and every gate it names still passes. Post-remediation figures, measured with the same per-`<line>`-descendant counting method so the comparison is like-for-like: repository-wide line **85.4097%** (93539/109518, PASS vs `>= 85%`) and branch **78.7220%** (21584/27418, PASS vs `>= 75%`), both improved. `SVGControl.SvgRenderer` class **332/414 = 80.1932%**, up from 424/588 = 72.109%; the denominator fell because `ResolveByNameAndKey` and `PublicKeyTokensEqual` moved out under R-6 and the static constructor shortened, **not** because any line lost coverage — the delta is reconciled member by member in the cited artifact. `SVGControl.SvgAssemblyProbe` holds **100% line and 100% branch** (102/102, 92/92) on a 50%-larger denominator. Two members named above as gaps are now closed: `PublicKeyTokensEqual` moved to `SvgAssemblyProbe` and rose from **0/15 = 0%** to **15/15 = 100%** line-rate with 18/18 = 100% branch-rate (eight new tests, task `[P1-T15]`), and the three-argument byte-array constructor rose from **13/17 = 76.471%** to **17/17 = 100%** (one new test, task `[P1-T14]`). The only genuinely new member this cycle adds, `SvgAssemblyResolver.Install()`, measures **6/6 = 100%** line-rate, above the `>= 90%` gate. The ratified exception is re-recorded as `COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgAssemblyResolver.ResolveByNameAndKey` — it travels with the relocated member, which is still `private static` and still invoked only by the CLR on a failed assembly bind; `SVGControl.SvgAssemblyResolver` is a relocation, not a new module, so the `>= 90%` new-module threshold does not attach to it. The `>= 85%` modified-file floor on `SVGControl/SvgRenderer.cs` is **not** targeted this cycle per R-4's explicit scope boundary; the residual is owned by `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md`. Supporting run: `evidence/qa-gates/test-coverage.2026-08-05T01-50.md` (9 assemblies, 6150/6150 passed, 0 failed).

### Appended under AC-8


  Evidence-note amendment 2026-08-05 (remediation cycle 1, task `[P2-T10]`). Three updates to the citations above; **the criterion's text and its `[x]` state are unchanged, and its substantive requirement is unchanged and still met.** (1) **Relocation.** R-6 moved `ResolveByNameAndKey` — cited above as "`SvgRenderer.cs` lines 47-143, unmoved" — into the new file `SVGControl/SvgAssemblyResolver.cs`, and moved `PublicKeyTokensEqual` — cited above at line 126 — into `SVGControl/SvgAssemblyProbe.cs`, where it is now `internal static bool PublicKeyTokensEqual(byte[]? a, byte[]? b)`. Both file-and-line citations above therefore resolve to the new locations. The move is behavior-preserving: `SvgRenderer`'s static constructor is retained with the body `SvgAssemblyResolver.Install();`, so touching `SvgRenderer` still installs the handler exactly once per AppDomain, which is the observable behavior this criterion depends on. Strategy order, the `_resolving.Add`/`Remove` re-entrance guard around strategies 2 and 3, the `PublicKeyTokensEqual` gate on every returned assembly, the empty-`Location` skip, and the terminal `return null;` are all preserved verbatim. Evidence: `evidence/other/resolver-extraction.2026-08-05T01-50.md`. (2) **Test count.** The note above says "the nine `SvgAssemblyProbeDirectoryTests`". After task `[P1-T12]` (+1, the invalid-path-character `baseDirectory` case) and task `[P1-T15]` (+8, the `PublicKeyTokensEqual` cases) there are **eighteen**; the figure "nine" above describes the pre-remediation state and must not be read as newly stale. All eighteen pass. Evidence: `evidence/regression-testing/remediation-tests.2026-08-05T01-50.md` (9 assemblies, 6150/6150 passed, 0 failed). (3) **Containment strengthened.** The code-review CR-2 caveat noted under this criterion is now addressed: `GetProbeDirectories` applies the `Path.GetInvalidPathChars()` filter to the `baseDirectory` candidate so all three candidates are validated identically, and a containment `catch (Exception ex)` was added to the outer `try` so `self.Location`, `self.CodeBase`, and `Path.Combine` can no longer throw out of the `AssemblyResolve` handler. The public-key-token match this criterion requires be preserved is now verified by measurement rather than by inspection alone: `PublicKeyTokensEqual` measures 100% line and 100% branch coverage. Evidence: `evidence/other/resolver-containment.2026-08-05T01-50.md`.
