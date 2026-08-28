#!/usr/bin/env bash
# shell-qc.sh: native bash quality-control wrapper for repository shell scripts.
# Subcommands: check | format | test [--coverage] | --help. No Python, no Poetry.
# The heavy lifting lives in scripts/bash/shell_qc_lib.sh so the functions are unit
# testable from bats by sourcing the library directly.
set -euo pipefail

# Resolve this script's own directory so the library sources regardless of cwd.
SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
# The library path is resolved at runtime from SCRIPT_DIR, so shellcheck cannot load it
# as a static input without -x; SC1091 is the expected, benign result and is suppressed.
# shellcheck source=scripts/bash/shell_qc_lib.sh
# shellcheck disable=SC1091
source "$SCRIPT_DIR/shell_qc_lib.sh"

usage() {
	# Print the wrapper usage/help text.
	cat <<'EOF'
Usage: shell-qc.sh <command> [options]

Commands:
  check              Run shfmt -d (diff) and shellcheck over discovered scripts.
  format             Rewrite discovered scripts in place with shfmt -w.
  test [--coverage]  Run bats over tests/shell and tests/bash. With --coverage,
                     run under kcov, emit a Cobertura cov.xml, and print a
                     "Bash coverage (lines): NN.N%" summary.
  --help, -h, help   Print this help and exit 0.

Discovery searches tools/ and scripts/; a file qualifies by a .sh suffix or a
bash/sh shebang (including env forms); the dirs .venv .git node_modules dist
build are excluded.

Environment overrides:
  SHELL_QC_<TOOL>_BIN    Path to shfmt/shellcheck/bats/kcov; an empty or
                         nonexistent value is treated as missing.
  SHELL_QC_KCOV_OUT_DIR  Coverage output directory (default artifacts/pester/kcov).
EOF
}

main() {
	# Dispatch on the first argument. Unknown or missing subcommands, extra
	# positionals on check/format, and unknown test flags print usage to stderr and
	# exit 2 (argparse usage-error parity). Subcommand return codes are captured so
	# an intermediate failure is never masked before the final exit.
	local exit_code=0
	local command=${1:-}
	case "$command" in
	check)
		if (($# > 1)); then
			usage >&2
			return 2
		fi
		run_check || exit_code=$?
		;;
	format)
		if (($# > 1)); then
			usage >&2
			return 2
		fi
		run_format || exit_code=$?
		;;
	test)
		shift
		local coverage=0
		# Only --coverage is accepted; any other token is a usage error.
		while (($# > 0)); do
			case "$1" in
			--coverage)
				coverage=1
				;;
			*)
				usage >&2
				return 2
				;;
			esac
			shift
		done
		if ((coverage == 1)); then
			run_test_coverage || exit_code=$?
		else
			run_test || exit_code=$?
		fi
		;;
	--help | -h | help)
		usage
		return 0
		;;
	*)
		usage >&2
		return 2
		;;
	esac
	return "$exit_code"
}

# Guard so the file can be sourced without executing main. main's return code is
# captured and re-exited explicitly as the final statement, so intermediate
# `|| rc=$?` captures inside the library cannot mask a real failure.
if [[ ${BASH_SOURCE[0]} == "${0}" ]]; then
	rc=0
	main "$@" || rc=$?
	exit "$rc"
fi
