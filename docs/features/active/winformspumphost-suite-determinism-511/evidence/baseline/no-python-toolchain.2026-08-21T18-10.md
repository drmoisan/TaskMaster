# Baseline — Python Toolchain Absence Finding

Timestamp: 2026-08-22T09-49

Command:

```
ls -d scripts/dev_tools
ls -l pyproject.toml
ls scripts/
ls -l poetry.lock
git ls-files "*.py"
ls .claude/lib/
```

All run from the worktree root
`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad37a256a0fb60243`.

EXIT_CODE: 0

Output Summary:

## Both required negative existence checks

1. **`scripts/dev_tools/` does not exist.**

   ```
   ls: cannot access 'scripts/dev_tools': No such file or directory
   ```

   The `scripts/` directory exists but contains only PowerShell:

   ```
   dev-tools/                     (contains only run-actionlint.ps1)
   temp-extract-coverage.ps1
   vscode/
   ```

   Note the hyphen: the repository has `scripts/dev-tools/`, not the underscored
   `scripts/dev_tools/` that Python module paths of the form `scripts.dev_tools.*` would require. The
   hyphenated directory holds one PowerShell script and no Python.

2. **No `pyproject.toml` exists at the worktree root.**

   ```
   ls: cannot access 'pyproject.toml': No such file or directory
   ```

   `poetry.lock` is likewise absent:

   ```
   ls: cannot access 'poetry.lock': No such file or directory
   ```

   There is therefore no Poetry manifest and no `poetry run` environment to resolve.

Supporting evidence: `git ls-files "*.py"` returns only two tracked Python files, both inside an
archived feature folder and neither part of any toolchain:

```
docs/features/archive/2026-07-18-stale-app-config-binding-redirects-354/scripts/fix_binding_redirects.py
docs/features/archive/2026-07-18-stale-app-config-binding-redirects-354/tests/scripts/test_fix_binding_redirects.py
```

## Finding

**There is no Python toolchain in this repository.** Any skill step, rule citation, or process
instruction naming a Python dev-tools module — for example a command of the form
`poetry run python -m scripts.dev_tools.<module>` — is **unrunnable by absence** in this repository.
Such a step is reported as unrunnable-by-absence. It is never fabricated and never silently skipped.

This applies to every Python validator named in the `.claude/rules/` files read in Phase 0, including
`scripts/dev_tools/validate_orchestrator_state.py`,
`scripts/dev_tools/validate_orchestration_artifacts.py`,
`scripts/dev_tools/plan_gate_discrimination.py`, and the parallel-surface validators. Those rule files
describe enforcement mechanisms that exist in a different repository snapshot; in this checkout the
Python modules they name are not present. The rule files remain the **policy** this fix is measured
against — they are cited, not edited, and this child edits nothing under `.claude/`.

The PowerShell equivalents that do exist live under `.claude/lib/`:

```
bash/
blast-radius/
codex-routing/
discovery-validation/
mermaid/
model-routing/
orchestrator-state/
```

Consistent with this finding, the plan itself contains no Python command anywhere (Binding
Constraint 8), and no step in Phase 0 or Phase 1 required one. No Python step was skipped, because
none was scheduled.

## Acceptance conditions

1. **Artifact exists with all four fields** (`Timestamp:`, `Command:`, `EXIT_CODE:`,
   `Output Summary:`) — met.
2. **Records both negative existence checks** — met: `scripts/dev_tools/` absent and root
   `pyproject.toml` absent, each with the verbatim command output.
