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
# shellcheck source=.claude/skills/cleanup-merged-worktrees/scripts/cleanup_worktrees_enumerate_lib.sh
# shellcheck disable=SC1091
source "$SCRIPT_DIR/cleanup_worktrees_enumerate_lib.sh"
# The report-record library depends on the enumeration library above and is called BY
# run_report/run_apply in the two libraries sourced below, so it is sourced between them.
# shellcheck source=.claude/skills/cleanup-merged-worktrees/scripts/cleanup_worktrees_report_records_lib.sh
# shellcheck disable=SC1091
source "$SCRIPT_DIR/cleanup_worktrees_report_records_lib.sh"
# shellcheck source=.claude/skills/cleanup-merged-worktrees/scripts/cleanup_worktrees_lib.sh
# shellcheck disable=SC1091
source "$SCRIPT_DIR/cleanup_worktrees_lib.sh"
# shellcheck source=.claude/skills/cleanup-merged-worktrees/scripts/cleanup_worktrees_actions_lib.sh
# shellcheck disable=SC1091
source "$SCRIPT_DIR/cleanup_worktrees_actions_lib.sh"
# shellcheck source=.claude/skills/cleanup-merged-worktrees/scripts/cleanup_worktrees_detached_lib.sh
# shellcheck disable=SC1091
source "$SCRIPT_DIR/cleanup_worktrees_detached_lib.sh"
# Neither of the three libraries below requires being sourced last, and neither claims to
# be. Each library's source-time work is limited to function definitions plus constant and
# environment-default variable assignments; none of them calls a function while being
# sourced, and no function name is defined by more than one library, so no later source can
# shadow an earlier definition. Bash resolves a function name when the call runs, not when
# the caller is defined, so the cross-library calls resolve whatever the order: the
# disposable-dirt classifier is called BY run_report and BY delete_candidate in the
# libraries above, and the preserve library calls cleanup_wt_git from the enumeration
# library and consolidation_worktree_path from the actions library. The order here is
# therefore grouping for readability only.
# shellcheck source=.claude/skills/cleanup-merged-worktrees/scripts/cleanup_worktrees_dirt_lib.sh
# shellcheck disable=SC1091
source "$SCRIPT_DIR/cleanup_worktrees_dirt_lib.sh"
# The line-ending and index group is placed immediately before the preserve library, whose
# read-only planning phase calls the functions it defines.
# shellcheck source=.claude/skills/cleanup-merged-worktrees/scripts/cleanup_worktrees_preserve_eol_lib.sh
# shellcheck disable=SC1091
source "$SCRIPT_DIR/cleanup_worktrees_preserve_eol_lib.sh"
# shellcheck source=.claude/skills/cleanup-merged-worktrees/scripts/cleanup_worktrees_preserve_lib.sh
# shellcheck disable=SC1091
source "$SCRIPT_DIR/cleanup_worktrees_preserve_lib.sh"

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
  preserve | --preserve
                       Stage the manifest's preserved_files[] findings onto the
                       consolidation branch. Takes no operand; the manifest path comes
                       from CLEANUP_WT_MANIFEST_PATH. Copies each named source file
                       verbatim, carries its MEMORY.md index line across using the
                       destination file's own re-derived line-ending convention, and
                       refuses the whole pass when the incoming bytes carry a
                       host-identifying token. Exit 0 clean, 1 something was skipped or
                       an index was created, 3 host-token hard stop, 127 a required tool
                       could not be resolved.
  --help | -h | help   Print this help and exit 0.

Flags:
  --clear-disposable   Apply mode only, opt-in, and destructive. When a worktree removal
                       is blocked because the worktree is dirty, and every per-file
                       verdict for that worktree is disposable, the working tree is
                       cleared with reset --hard and clean -fd and the SAME unforced
                       removal is retried once after a fresh in-process re-verification.
                       Refuses the clear for the whole worktree when any per-file verdict
                       is UNIQUE, including the fail-closed UNIQUE assigned when a
                       classification read errors. Ignored files are never cleared. This
                       flag may be given before or after the apply argument; supplying it
                       without apply mode is a usage error and exits 2.

Report lines (pipe-delimited, LC_ALL=C ordered): BRANCH|<name>|<state>;
COMMIT|<branch>|<sha>|<state>|<paths>|<author>|<date>;
WORKTREE|<path>|<branch>|<flags> for a branch-backed worktree registration;
WORKTREE|<path>|DETACHED|<state>|<flags> for a detached-HEAD worktree registration,
which is classified on its own HEAD SHA and whose fifth field preserves the porcelain
locked and prunable markers; WARN|main-divergence|<local>|<origin>;
DIRTY|<path>|<status>; ACTION|<verb>|<target>|<result> (apply mode);
DIRTFILE|<worktree>|<verdict>|<detail>|<xy>|<path>, one per dirty entry in porcelain order;
DIRTSUM|<worktree>|<aggregate>|<detail>, exactly one per dirty worktree. The verdicts are
DISPOSABLE_BUILD_ARTIFACT, DISPOSABLE_SESSION_ARTIFACT, CONTENT_ON_MAIN, CONTENT_IN_HISTORY,
STAGED_TREE_IS_COMMIT, and UNIQUE; the aggregate is ALL_DISPOSABLE or HAS_UNIQUE. Both are
read-only records and neither unlocks a destructive action on its own.

PRESERVE|<worktree-path>|<source-path>|<verdict> (preserve mode), a manifest
preserved_files[] finding staged onto the consolidation branch, whose per-file outcome is
reported by the companion ACTION|preserve-stage|... record.

Advisory, read-only records (report and apply mode; none unlocks a destructive action):
ORPHAN_DIR|<path>|<size> for a scanned directory with no .git pointer file and no
worktree registration, where <size> is best-effort and may be the literal unknown;
STALE_REF|<refname> for a refs/remotes/<name>/* ref whose <name> is not a configured
remote, in full ref form; CHILD_OF|<branch>|<ancestor> emitted alongside a branch's own
BRANCH| line when it is a git ancestor of a branch that resolved exactly NOT_MERGED;
WARN|registration-lost|<path> for a worktree whose .git gitdir pointer no longer
resolves. Deleting any of these is a manual, individually confirmed action.

Branch and detached-worktree states: MERGED_CLEAN, MERGED_CONTENT_NEUTRAL,
MERGED_EQUIVALENT, NOT_MERGED, HAS_UNIQUE_RESIDUALS, PROTECTED_CURRENT, and
ANCESTRY_ERROR. The first three are the delete-eligible allowlist; ANCESTRY_ERROR is a
hard git failure and never unlocks a destructive action.

PROTECTED_CURRENT also covers the base branch main, which is protected by name in every
checkout topology: main is never deleted and a worktree checked out on main is never
removed. As a second guard, apply mode refuses a deletion request for main with the
action result BLOCKED-PROTECTED-BASE.

In apply mode a blocked detached removal sets a non-zero exit status. The blocked results
are BLOCKED-DIRTY, BLOCKED-LOCKED, and BLOCKED-REVERIFY. A checkout holding dirty or
locked detached worktrees therefore exits non-zero from --apply where it previously
exited 0.

Environment overrides:
  CLEANUP_WT_GIT_BIN            Path to the git binary; an empty or nonexistent value
                               is treated as missing (falls back to PATH git). This is
                               the test-stub seam.
  CLEANUP_WT_SCAN_BIN          Path to the filesystem-scan helper; an empty or
                               nonexistent value is treated as missing (falls back to
                               the bundled cleanup_worktrees_scan_helper.sh). This is
                               the filesystem-scan test-stub seam.
  CLEANUP_WT_ORPHAN_ROOTS      Worktree-tracking roots scanned for ORPHAN_DIR and
                               WARN|registration-lost. Entries are separated by a
                               semicolon, a newline, or a colon; a colon that follows
                               a single drive letter and precedes / or \ is part of
                               the path (C:/x). Empty and relative entries are
                               dropped. When set, it replaces the default pair
                               <main>/.claude/worktrees and <main>-wt. The parent of
                               every non-main registered worktree is always added,
                               except a parent that is the main worktree or its
                               ancestor, or is equal to or inside a registered
                               worktree.
  CLEANUP_WT_STUB_SCENARIO     Scenario directory consumed by the checked-in git and
                               scan stubs (tests only).
  CLEANUP_WT_CONSOLIDATION_PATH Override the derived consolidation worktree path
                               (<main-worktree-path>-wt/documentationandmemories).
  CLEANUP_WT_MANIFEST_PATH     Path to the cleanup-worktrees manifest read by preserve
                               mode. Defaults to
                               artifacts/orchestration/cleanup-worktrees-manifest.json.
  CLEANUP_WT_JQ_BIN            Path to the jq binary used to read that manifest; an empty
                               or nonexistent value is treated as missing (falls back to
                               PATH jq). No default. This is the jq test-stub seam.
EOF
}

main() {
	# Dispatch on the first argument. No args or `report` runs the dry-run report;
	# `--apply`/`apply` runs apply mode; `preserve`/`--preserve` stages the manifest's
	# preserved_files[] findings onto the consolidation branch and takes no operand,
	# reading its manifest path from CLEANUP_WT_MANIFEST_PATH; `--help`/`-h`/`help`
	# prints usage and exits 0;
	# anything else prints usage to stderr and exits 2 (usage-error parity with
	# shell-qc.sh). Subcommand return codes are captured so an intermediate failure is
	# never masked before the final exit.
	local exit_code=0
	# --clear-disposable pre-pass, run before the dispatch below so the flag may be given
	# on either side of the mode argument. The flag is stripped from the argument list and
	# the remaining first argument must select apply mode: the flag is opt-in and
	# destructive, so pairing it with report mode (or with no mode at all) is a usage error
	# rather than a silently ignored flag. Every case arm below is unchanged.
	local -a args=()
	local clear_flag=0 arg
	for arg in "$@"; do
		if [[ $arg == "--clear-disposable" ]]; then
			clear_flag=1
			continue
		fi
		args+=("$arg")
	done
	if ((clear_flag == 1)); then
		case "${args[0]:-}" in
		--apply | apply) ;;
		*)
			usage >&2
			return 2
			;;
		esac
		# Read across a source boundary shellcheck does not follow:
		# cleanup_worktrees_dirt_lib.sh gates clear_disposable_dirt on this variable and
		# cleanup_worktrees_actions_lib.sh gates delete_candidate's clear-and-retry block
		# on it, so SC2034 reports it unused here. The suppression below is line-scoped,
		# not file-scoped, so SC2034 stays live for every other variable in this file.
		# The assignment's effect is pinned end to end by the wrapper-driven test pair at
		# the foot of tests/shell/test_cleanup_worktrees_dirt_clear.bats.
		# shellcheck disable=SC2034
		CLEANUP_WT_CLEAR_DISPOSABLE=1
	fi
	if ((${#args[@]} > 0)); then
		set -- "${args[@]}"
	else
		set --
	fi
	local command=${1:-}
	case "$command" in
	"" | report)
		run_report || exit_code=$?
		;;
	--apply | apply)
		run_apply || exit_code=$?
		;;
	preserve | --preserve)
		run_preserve || exit_code=$?
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
