# Remediation Inputs — stale-app-config-binding-redirects (Issue #354)

- Generated: 2026-07-18T14-45
- Source artifacts: `policy-audit.2026-07-18T14-45.md`, `code-review.2026-07-18T14-45.md`, `feature-audit.2026-07-18T14-45.md`
- Feature folder: `docs/features/active/2026-07-18-stale-app-config-binding-redirects-354`

## Remediation-Required Findings

### 1. (Blocking) Python coverage artifact absent for `fix_binding_redirects.py`

- **Finding:** The branch adds a new, permanently-committed Python file (`docs/features/active/2026-07-18-stale-app-config-binding-redirects-354/scripts/fix_binding_redirects.py`, 77 lines) with zero unit tests and no `artifacts/python/lcov.info` coverage artifact. Per the mandatory Coverage Verification procedure, any language with changed files must have a coverage artifact; its absence is a FAIL, not an N/A, since the file is committed to the repository (not a session-scoped scratchpad throwaway).
- **Evidence:** `policy-audit.2026-07-18T14-45.md` §1.2.1 (Python row), §3.1, §5; `code-review.2026-07-18T14-45.md` findings table rows 1 and 3.
- **Recommended remediation:** Either (a) add a `pytest` suite for `fix_binding_redirects.py` covering at least: a stale redirect being corrected, an already-correct redirect left unchanged (idempotency), and a project missing `app.config`/`.csproj` being skipped — then generate `artifacts/python/lcov.info` and confirm >= 90% new-code coverage; or (b) if the script is intended purely as a one-off, already-executed audit tool with no future reuse, move it out of the permanently-tracked `scripts/` path (e.g., document it as a historical, non-reusable artifact in the plan/evidence only, not as a durable committed script) and record that decision explicitly. Given the `durable-script-copy-into-feature-folder` convention this repo already follows (committing proven-correct feature scripts into `<FEATURE>/scripts/` for durability), option (a) is the more consistent choice.

### 2. (Blocking) Python code-quality gaps in `fix_binding_redirects.py`

- **Finding:** The script has zero type hints on any function, zero docstrings (module or function level), and zero loop/branch intent comments, all of which are mandatory under `.claude/rules/python.md` and `.claude/rules/self-explanatory-code-commenting.md`. `black`/`ruff`/`pyright` all report clean under this repo's default tool configuration, but that does not substitute for these explicit, separately-stated policy requirements.
- **Evidence:** `policy-audit.2026-07-18T14-45.md` §3.1; `code-review.2026-07-18T14-45.md` findings table rows 1 and 2.
- **Recommended remediation:** Add full parameter/return type hints to `project_list()` and the nested helpers; add a module docstring and function docstrings (Google-style, per the commenting policy); add intent comments above both `for` loops. Consider hoisting `repl`/`_ver_tuple` out of the loop body per the code-review's Low-severity design finding (non-blocking, but convenient to address in the same pass).

## Non-Blocking / Follow-Up Items (not remediation-required for this issue, but documented)

### 3. `SVGControl/app.config` residual stale redirect (out of this issue's defined scope)

- **Finding:** `SVGControl/app.config`'s `System.Runtime.CompilerServices.Unsafe` bindingRedirect caps at `6.0.2.0` while `SVGControl.csproj` references `6.0.3.0`. Not named in `issue.md`'s Suspected-Cause inventory; excluded by name in the fix script; conventionally treated as vendored/exempt for other build gates in this repo.
- **Recommended follow-up:** Open a separate, small issue to either correct this redirect or formally document why `SVGControl`/`SVGControl.Test` are exempt from AC1-style binding-redirect audits (not just analyzer/nullable gates). Does not block issue #354.

### 4. `issue.md` baseline-narrative discrepancy

- **Finding:** `issue.md` describes 8 of 21 tests failing at baseline due to a `Microsoft.Bcl.TimeProvider` mismatch; the executor's own baseline evidence shows 0 failures in the actual working-tree state at capture time (that specific package's redirect already matched). The executor documented this transparently in the evidence file itself, but `issue.md` is not updated to note the discrepancy.
- **Recommended follow-up:** Optional — add a short addendum to `issue.md` or a dedicated evidence cross-reference so future readers are not misled about the reproducibility of the originally-reported repro steps. Low priority; does not affect the validity of the fix or its verification.

## Handoff

Per `remediation-handoff-atomic-planner`, items 1 and 2 above (both scoped to `docs/features/active/2026-07-18-stale-app-config-binding-redirects-354/scripts/fix_binding_redirects.py`) are the remediation-required findings that should be handed to an atomic-planner/atomic-executor remediation cycle. Items 3 and 4 are non-blocking follow-ups suitable for separate, lower-priority issues and are not required to close out this review cycle for issue #354.
