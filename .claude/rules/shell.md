---
paths:
  - "**/*.sh"
  - "**/*.bats"
  - "scripts/bash/**"
  - "tests/shell/**"
description: Shell (bash) toolchain and coding standards.
---

# Shell (Bash) Code Standards

This rule file summarizes the shell-specific policies for this repository. The shell
quality-control toolchain is native bash and has no Python or Poetry dependency.

## Toolchain

Run the toolchain in this order and restart from step 1 if any step fails or rewrites files:

1. **Formatting — shfmt**: Format all shell scripts with shfmt (write mode). Command:
   `bash scripts/bash/shell-qc.sh format`. The `check` command runs shfmt in diff mode
   (`shfmt -d`) as its first stage.
2. **Linting — shellcheck**: Lint all shell scripts with shellcheck. Command:
   `bash scripts/bash/shell-qc.sh check`. `check` runs `shfmt -d` once over the full file
   list and then `shellcheck` once per file, returning the maximum exit code.
3. **Type checking — not applicable**: Bash has no separate type-check stage. An optional
   syntax check is available via `bash -n`; the VS Code task "Shell QC: 3 bash: type-check"
   provides it. Skip to testing.
4. **Testing — bats**: Run bats tests with `bash scripts/bash/shell-qc.sh test`. Line coverage
   via kcov with `bash scripts/bash/shell-qc.sh test --coverage`.

Do not stop the loop until formatting, linting, and testing complete without errors in a
single pass. Do not substitute the VS Code task wrappers for the native command in automation.

## Native Invocation and Environment

- The toolchain is native bash: the wrapper `scripts/bash/shell-qc.sh` and its library
  `scripts/bash/shell_qc_lib.sh` invoke `shfmt`, `shellcheck`, `bats`, and `kcov` directly.
  No Python interpreter, no Poetry, and no `poetry run` are involved.
- On Windows, run the toolchain under WSL.
- In CI, the toolchain runs on `ubuntu-latest` (`.github/workflows/_shell-coverage.yml` for
  coverage; `.github/workflows/_build-check.yml` runs `--help` as an installability smoke).
- Per-tool path overrides are available for testing via `SHELL_QC_<TOOL>_BIN` (for `shfmt`,
  `shellcheck`, `bats`, `kcov`); an empty or nonexistent value is treated as missing. The
  coverage output directory is `SHELL_QC_KCOV_OUT_DIR` (default `artifacts/pester/kcov`).

## Discovery Contract

- Search roots: `tools/`, `scripts/`, and `.claude/lib/bash/`, relative to the current working
  directory; a missing root is silently skipped. The `.claude/lib/bash/` root carries the
  destination-portable bash library published by push-down, so those scripts are held to the
  same format, lint, test, and coverage standards as `tools/` and `scripts/`.
- A file is a shell script when its suffix (lowercased) is `.sh` or its first line is a
  shebang whose resolved interpreter is `bash` or `sh` (including `env` and `env -S`/`-flag`
  forms; the shebang is lowercased before parsing, so `#!/usr/bin/env BASH` qualifies).
- Excluded directories (pruned at any depth): `.venv`, `.git`, `node_modules`, `dist`,
  `build`.
- The discovered set is de-duplicated and sorted with `LC_ALL=C` for deterministic ordering.
- bats test directories: `tests/shell` and `tests/bash`, whichever exist, in that order.

## Coverage Expectations

- Coverage is measured with kcov, which emits a single merged Cobertura report `cov.xml` under
  `artifacts/pester/kcov` (or `SHELL_QC_KCOV_OUT_DIR`). The run prints
  `Bash coverage (lines): NN.N%`.
- The kcov include pattern covers all three discovery roots — `tools/`, `scripts/`, and
  `.claude/lib/bash/` — so the Claude bash library is measured, not merely discovered. The
  `tests/` tree remains excluded.
- kcov reports **line coverage only**. The uniform line-coverage threshold (>= 85% per
  `.claude/rules/quality-tiers.md`) applies. Branch coverage is not measurable by kcov for
  bash; there is no bash branch-coverage gate.

## CI-vs-Local Version Drift

- CI pins shfmt 3.8.0 (installed as a binary), uses apt-packaged shellcheck and bats, and
  builds kcov v43 from source. Local WSL installs (winget or apt) may drift from these
  versions.
- CI versions are canonical. When local and CI results disagree, defer to CI.

## Coding Standards

- Begin executable scripts with `set -euo pipefail`. Tools that legitimately return non-zero
  (shfmt diff mode, shellcheck, bats, kcov) must be captured with `|| rc=$?` so an intended
  non-zero exit does not abort under `set -e`.
- Keep scripts shellcheck-clean. Suppressions are permitted only when justified inline with a
  `# shellcheck disable=SCxxxx` comment stating the reason.
- Use shfmt default formatting (tab indentation, as in `scripts/bash/coverage_lib.sh`).
- Quote all expansions; resolve tools with `command -v` (honoring the `SHELL_QC_<TOOL>_BIN`
  override seam).
- No production, test, or reusable shell file may exceed 500 lines.
- Tests live in `tests/shell/*.bats` and mirror `scripts/bash/`. Tests must not create
  temporary files; use checked-in fixtures under `tests/fixtures/` and checked-in stub
  binaries under `tests/fixtures/shell_qc/stub-bin/` wired through the `SHELL_QC_<TOOL>_BIN`
  seam.
