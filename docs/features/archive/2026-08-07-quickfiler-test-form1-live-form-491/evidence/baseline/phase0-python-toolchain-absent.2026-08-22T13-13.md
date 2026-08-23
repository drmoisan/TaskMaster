Timestamp: 2026-08-22T13-13
Command: git ls-files scripts/dev_tools; git ls-files pyproject.toml
EXIT_CODE: 0
Output Summary: Both commands produced empty output. No `scripts/dev_tools/` tree and no `pyproject.toml` are tracked in this repository. Any skill step naming a `poetry run python -m scripts.dev_tools.*` invocation is therefore unrunnable by absence in this repository; it is recorded as unrunnable and is not fabricated or silently skipped. No Python coverage tooling exists here; C# coverage is measured exclusively through `scripts\vscode\Invoke-MSTestWithCoverage.ps1`.
