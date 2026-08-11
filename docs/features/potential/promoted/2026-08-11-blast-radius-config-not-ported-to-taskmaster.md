# blast-radius-config-not-ported-to-taskmaster (Issue #545)

- Date captured: 2026-08-11
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/blast-radius-config-not-ported-to-taskmaster/ (Issue #545)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #545
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/545
- Last Updated: 2026-08-11
## Summary

`config/blast-radius.json` was pushed down verbatim from the `.claude` governance payload (PR #544) and describes that payload's own directory layout rather than TaskMaster's. As a result the parallel orchestration surface produces a complete conflict graph (zero parallelism) while simultaneously failing to detect real collisions on TaskMaster's build-level shared surfaces.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a — the blast-radius port in TaskMaster is PowerShell (`.claude/lib/blast-radius/*.psm1`)
- Command/flags used: `Get-BlastRadius`, `Test-BlastRadiusConflict` from `.claude/lib/blast-radius/BlastRadius.psm1`
- Data source or fixture: `config/blast-radius.json` at commit `2073f717`

## Steps to Reproduce

1. Check out `main` at `2073f717` (or later).
2. Import `.claude/lib/blast-radius/BlastRadius.psm1`.
3. Call `Get-BlastRadius` for two items in unrelated lanes touching unrelated C# files (for example issue 480 touching a `QuickFiler` source file and issue 287 touching a `ToDoModel` source file).
4. Call `Test-BlastRadiusConflict` on the resulting pair.
5. Separately, call `Get-BlastRadius` for two items that both edit `coverage.config` and `Directory.Build.targets`.

## Expected Behavior

- Step 4: two items editing unrelated C# projects should report `conflict=False`, allowing them to be colored into the same cohort and executed concurrently.
- Step 5: two items editing the same root build files should report those files in their shared-surface sets and report `conflict=True` with a shared-surface reason.

## Actual Behavior

- Step 4 reports `conflict=True reasons=[module_overlap]`. `Get-BlastRadius` always appends the mandatory feature-folder glob `docs/features/active/<name>/**` (`BlastRadius.psm1:171`), and module `docs` maps to `docs/**`. Every item therefore carries module `docs`, every pair overlaps, the conflict graph is a clique, and Welsh-Powell yields one cohort per item. A 59-item run would execute fully serially. Observed pairs:

  ```
  A(480 QuickFiler.cs) vs B(287 ToDoModel.cs) truly independent  conflict=True reasons=[module_overlap]
  A(480) vs E(468) same QuickFiler .csproj                       conflict=True reasons=[module_overlap]
  ```

- Step 5 reports empty shared-surface sets. TaskMaster's real root shared surfaces are absent from `shared_surfaces`, and under the F1a rule a separator-free root token is admitted only as an exact member of that list, so it is dropped silently:

  ```
  C(512) paths: [docs/features/active/2026-08-11-c-512/**]  shared: []
  D(494) paths: [docs/features/active/2026-08-11-d-494/**]  shared: []
  ```

  The real collision is invisible, masked only incidentally by the degenerate `docs` edge. Correcting the module map alone would turn this into a reported false negative.

- Additionally, none of the nine production or nine test C# project directories appear in `modules`, so C# work attributes to no module. `tests/` in TaskMaster contains only `scripts/` (PowerShell), not the C# test projects.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet — the committed config as of `2073f717`:

  ```json
  {
    "version": 1,
    "shared_surfaces": [
      ".claude/settings.json",
      "config/orchestration-routing.json",
      "config/blast-radius.json"
    ],
    "shared_surface_globs": [],
    "modules": {
      "claude-runtime": [".claude/**"],
      "config": ["config/**"],
      "docs": ["docs/**"],
      "tests": ["tests/**"]
    },
    "over_breadth_fraction": 0.25
  }
  ```

## Impact / Severity

- [x] Blocker
- [ ] High
- [ ] Medium
- [ ] Low

Blocker for the parallel orchestration surface specifically: the surface is unusable in TaskMaster until the truth table reflects this repository. The current state is fail-closed on parallelism (serial execution, which is safe but delivers nothing) and fail-open on build-surface collisions (which is not safe once the clique is fixed).

## Suspected Cause / Notes

The governance push-down (`c1c10c3b`, "(chore): push down claude parallel orchestrator") copied `config/blast-radius.json` verbatim from the source payload. The file is repository-shape-specific data, not portable governance, so verbatim push-down is the wrong transport for it.

Files to inspect:

- `config/blast-radius.json`
- `.claude/lib/blast-radius/BlastRadius.psm1` (line 171 appends the mandatory feature-folder glob)
- `.claude/rules/parallel-orchestration.md` (F1a shared-surface admission rule)

## Proposed Fix / Validation Ideas

- [ ] Enumerate the nine production/test `.csproj` directory pairs as `modules` entries so C# work attributes to a real module.
- [ ] Add TaskMaster's real root shared surfaces to `shared_surfaces`: `TaskMaster.sln`, `Directory.Build.targets`, `.editorconfig`, `coverage.config`, plus `.github/workflows/**` via `shared_surface_globs`.
- [ ] Resolve the `docs` module so the mandatory feature-folder glob stops collapsing the conflict graph into a clique. Options: narrow `docs` to the non-feature documentation subtrees, or exclude the per-item feature-folder glob from module attribution.
- [ ] Unit coverage areas: Pester tests over `Get-BlastRadius` module attribution and `Test-BlastRadiusConflict` for (a) two unrelated C# items → no conflict, (b) two items sharing a root build file → conflict with a shared-surface reason, (c) two items in the same `.csproj` → conflict.
- [ ] Integration scenario to retest: seed a two-item generation-0 cohort table and confirm both items color into the same cohort.
- [ ] Manual verification notes: re-run the two probes above and confirm the reported reasons invert.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
