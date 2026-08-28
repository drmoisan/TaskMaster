#!/usr/bin/env bash
# cleanup-worktrees.sh: thin CLI wrapper for the cleanup-merged-worktrees tool. It
# classifies local branches/worktrees against main and, in apply mode only, deletes
# delete-eligible candidates and drives consolidation. All logic lives in the two
# sourceable libraries so the functions are unit-testable from bats by sourcing them
# directly; this wrapper only resolves paths, prints usage, and dispatches.
set -euo pipefail

# Resolve this script's own directory so the libraries source regardless of cwd.
SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
# The library paths are resolved at runtime from SCRIPT_DIR, so shellcheck cannot load
# them as static inputs without -x; SC1091 is the expected, benign result. The
# enumeration/protection library is sourced first because cleanup_worktrees_lib.sh's
# classification functions call cleanup_wt_git, parse_worktree_list, compute_protected,
# and normalize_wt_path defined there.
# shellcheck source=scripts/bash/cleanup_worktrees_enumerate_lib.sh
# shellcheck disable=SC1091
source "$SCRIPT_DIR/cleanup_worktrees_enumerate_lib.sh"
# shellcheck source=scripts/bash/cleanup_worktrees_lib.sh
# shellcheck disable=SC1091
source "$SCRIPT_DIR/cleanup_worktrees_lib.sh"
# shellcheck source=scripts/bash/cleanup_worktrees_actions_lib.sh
# shellcheck disable=SC1091
source "$SCRIPT_DIR/cleanup_worktrees_actions_lib.sh"

usage() {
	# Print the wrapper usage/help text.
	cat <<'EOF'
Usage: cleanup-worktrees.sh [command]

Commands:
  (no args) | report   Dry-run report (default). Classifies every local branch and
                       worktree against main and prints a deterministic, machine-
                       parseable report. Performs NO mutation of any kind.
  --apply | apply      Apply mode. Deletes delete-eligible candidates (MERGED_CLEAN,
                       MERGED_CONTENT_NEUTRAL, MERGED_EQUIVALENT) after same-process
                       re-verification, removing worktrees without force and deleting
                       branches with -D. Never acts on NOT_MERGED,
                       HAS_UNIQUE_RESIDUALS, or PROTECTED_CURRENT candidates.
  --help | -h | help   Print this help and exit 0.

Report lines (pipe-delimited, LC_ALL=C ordered): BRANCH|<name>|<state>;
COMMIT|<branch>|<sha>|<state>|<paths>|<author>|<date>;
WORKTREE|<path>|<branch-or-DETACHED>|<flags>; WARN|main-divergence|<local>|<origin>;
DIRTY|<path>|<status>; ACTION|<verb>|<target>|<result> (apply mode).

Environment overrides:
  CLEANUP_WT_GIT_BIN            Path to the git binary; an empty or nonexistent value
                               is treated as missing (falls back to PATH git). This is
                               the test-stub seam.
  CLEANUP_WT_STUB_SCENARIO     Scenario directory consumed by the checked-in git stub
                               (tests only).
  CLEANUP_WT_CONSOLIDATION_PATH Override the derived consolidation worktree path
                               (<main-worktree-path>-wt/documentationandmemories).
EOF
}

main() {
	# Dispatch on the first argument. No args or `report` runs the dry-run report;
	# `--apply`/`apply` runs apply mode; `--help`/`-h`/`help` prints usage and exits 0;
	# anything else prints usage to stderr and exits 2 (usage-error parity with
	# shell-qc.sh). Subcommand return codes are captured so an intermediate failure is
	# never masked before the final exit.
	local exit_code=0
	local command=${1:-}
	case "$command" in
	"" | report)
		run_report || exit_code=$?
		;;
	--apply | apply)
		run_apply || exit_code=$?
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
# `|| rc=$?` captures inside the libraries cannot mask a real failure.
if [[ ${BASH_SOURCE[0]} == "${0}" ]]; then
	rc=0
	main "$@" || rc=$?
	exit "$rc"
fi
