#!/usr/bin/env bash
# mutate-parallel-run.sh: destination-portable command-line entry point for the
# decision and audit-record steps of the parallel-add, parallel-close, and
# parallel-orchestrate skills (issue #890). It exists so a workspace that
# received the Claude customization payload can run those steps with nothing
# but bash -- no interpreter, no repository checkout.
#
# Usage:
#   bash .claude/lib/bash/mutate-parallel-run.sh admit --candidate <key> \
#       [--edges "<a>:<b> ..."] --in-flight "<k> ..." \
#       --current-cohort-members "<k> ..."
#   bash .claude/lib/bash/mutate-parallel-run.sh recolor --unstarted "<k> ..." \
#       [--edges "<a>:<b> ..."] --pinned "<k> ..." --generation <g> \
#       --current-cohort <c> --highest-pinned-cohort <h>
#   bash .claude/lib/bash/mutate-parallel-run.sh add-entry --item <key> \
#       --deferred true|false --generation <g> [--at <timestamp>]
#   bash .claude/lib/bash/mutate-parallel-run.sh close --items "<k>:<state> ..."
#   bash .claude/lib/bash/mutate-parallel-run.sh close-entry --generation <g> \
#       [--at <timestamp>]
#   bash .claude/lib/bash/mutate-parallel-run.sh requeue-entry --item <key> \
#       --generation <g> [--at <timestamp>]
#
# `--edges` is optional; `--in-flight`, `--current-cohort-members`,
# `--unstarted`, `--pinned`, and `--items` accept an empty string. Integer
# tokens match `-?(0|[1-9][0-9]*)`. An `--items` token is an integer, a colon,
# and a `[A-Za-z_]+` state; a duplicate key is a usage error. `--at` defaults to
# the current UTC time as yyyy-MM-ddTHH-mm; the clock is read only when `--at`
# is omitted.
#
# Exit-code contract (identical to remove-parallel-item.sh):
#   exit 0  success; one compact JSON object on stdout
#   exit 1  rule rejection; the reference implementation's exact message as
#           one stderr line
#   exit 2  usage error; one stderr line beginning
#           "PARALLEL_MUTATION_ERROR: usage error:"
# Argument validation runs first, then rule rejection, then output.
#
# Parity reference: scripts/dev_tools/parallel_mutation_protocol.py
# (decide_admission, recolor_unstarted, decide_close) and
# scripts/dev_tools/_parallel_mutation_entries.py (build_add_entry,
# build_close_entry, build_requeue_entry).
#
# Declared divergences (also listed in both parity lanes):
#   1. A duplicate key in `close --items` is a usage error (exit 2); the Python
#      mapping would silently keep the last record for that key.
#   2. `at` is a caller-supplied string instead of a `datetime`.
#   3. Option abbreviations are rejected.
#   4. `admit` has no rejection branch, in either implementation.
set -euo pipefail

# Resolve this script's own directory so the libraries source regardless of cwd.
MPR_SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
# shellcheck source=.claude/lib/bash/parallel-mutation-ops.sh
# shellcheck disable=SC1091
source "$MPR_SCRIPT_DIR/parallel-mutation-ops.sh"
# shellcheck source=.claude/lib/bash/parallel-mutation-cli.sh
# shellcheck disable=SC1091
source "$MPR_SCRIPT_DIR/parallel-mutation-cli.sh"

pc_enforce_c_locale

mpr_admit() {
	# Run the admit subcommand.
	local candidate edges in_flight members rc=0
	pmcli_parse_options "candidate edges in-flight current-cohort-members" "$@"
	pmcli_require candidate in-flight current-cohort-members
	pmcli_integer "${PMCLI_OPTS[candidate]}" "--candidate"
	candidate="$PMCLI_VALUE"
	pmcli_edge_list "${PMCLI_OPTS[edges]-}"
	edges="$PMCLI_VALUE"
	pmcli_integer_list "${PMCLI_OPTS["in-flight"]}" "--in-flight"
	in_flight="$PMCLI_VALUE"
	pmcli_integer_list "${PMCLI_OPTS["current-cohort-members"]}" "--current-cohort-members"
	members="$PMCLI_VALUE"
	pm_decide_admission "$candidate" "$edges" "$in_flight" "$members" || rc=$?
	pmcli_emit "$rc"
}

mpr_recolor() {
	# Run the recolor subcommand; same options as remove-parallel-item.sh.
	local unstarted edges pinned generation current highest rc=0
	pmcli_parse_options "unstarted edges pinned generation current-cohort highest-pinned-cohort" "$@"
	pmcli_require unstarted pinned generation current-cohort highest-pinned-cohort
	pmcli_integer_list "${PMCLI_OPTS[unstarted]}" "--unstarted"
	unstarted="$PMCLI_VALUE"
	pmcli_edge_list "${PMCLI_OPTS[edges]-}"
	edges="$PMCLI_VALUE"
	pmcli_integer_list "${PMCLI_OPTS[pinned]}" "--pinned"
	pinned="$PMCLI_VALUE"
	pmcli_integer "${PMCLI_OPTS[generation]}" "--generation"
	generation="$PMCLI_VALUE"
	pmcli_integer "${PMCLI_OPTS["current-cohort"]}" "--current-cohort"
	current="$PMCLI_VALUE"
	pmcli_integer "${PMCLI_OPTS["highest-pinned-cohort"]}" "--highest-pinned-cohort"
	highest="$PMCLI_VALUE"
	pm_recolor_unstarted "$unstarted" "$edges" "$pinned" "$generation" "$current" "$highest" || rc=$?
	pmcli_emit "$rc"
}

mpr_add_entry() {
	# Run the add-entry subcommand.
	local item deferred generation at rc=0
	pmcli_parse_options "item deferred generation at" "$@"
	pmcli_require item deferred generation
	pmcli_integer "${PMCLI_OPTS[item]}" "--item"
	item="$PMCLI_VALUE"
	deferred="${PMCLI_OPTS[deferred]}"
	[[ $deferred == true || $deferred == false ]] ||
		pmcli_usage_error "--deferred must be true or false; found: $deferred"
	pmcli_integer "${PMCLI_OPTS[generation]}" "--generation"
	generation="$PMCLI_VALUE"
	pmcli_timestamp
	at="$PMCLI_VALUE"
	pm_build_add_entry "$item" "$deferred" "$generation" "$at" || rc=$?
	pmcli_emit "$rc"
}

mpr_close() {
	# Run the close subcommand over every `<key>:<state>` token.
	local token key state canonical="" seen=" " rc=0
	pmcli_parse_options "items" "$@"
	pmcli_require items
	pcoh_split_words "${PMCLI_OPTS[items]}"
	local -a tokens=("${PCOH_WORDS[@]}")
	for token in "${tokens[@]}"; do
		[[ $token =~ ^(-?(0|[1-9][0-9]*)):([A-Za-z_]+)$ ]] ||
			pmcli_usage_error "--items token must be <key>:<state>; found: $token"
		state="${BASH_REMATCH[3]}"
		key=$((BASH_REMATCH[1]))
		# A duplicate key would let one record silently replace another, so it
		# is rejected before the library runs (declared divergence 1).
		[[ $seen != *" $key "* ]] || pmcli_usage_error "--items key given more than once: $key"
		seen="$seen$key "
		canonical="$canonical $key:$state"
	done
	pm_decide_close "${canonical# }" || rc=$?
	pmcli_emit "$rc"
}

mpr_close_entry() {
	# Run the close-entry subcommand.
	local generation at rc=0
	pmcli_parse_options "generation at" "$@"
	pmcli_require generation
	pmcli_integer "${PMCLI_OPTS[generation]}" "--generation"
	generation="$PMCLI_VALUE"
	pmcli_timestamp
	at="$PMCLI_VALUE"
	pm_build_close_entry "$generation" "$at" || rc=$?
	pmcli_emit "$rc"
}

mpr_requeue_entry() {
	# Run the requeue-entry subcommand.
	local item generation at rc=0
	pmcli_parse_options "item generation at" "$@"
	pmcli_require item generation
	pmcli_integer "${PMCLI_OPTS[item]}" "--item"
	item="$PMCLI_VALUE"
	pmcli_integer "${PMCLI_OPTS[generation]}" "--generation"
	generation="$PMCLI_VALUE"
	pmcli_timestamp
	at="$PMCLI_VALUE"
	pm_build_requeue_entry "$item" "$generation" "$at" || rc=$?
	pmcli_emit "$rc"
}

mpr_main() {
	# Dispatch on the subcommand.
	(($# > 0)) ||
		pmcli_usage_error "missing subcommand; expected one of admit, recolor, add-entry, close, close-entry, requeue-entry"
	local subcommand="$1"
	shift
	case "$subcommand" in
	admit) mpr_admit "$@" ;;
	recolor) mpr_recolor "$@" ;;
	add-entry) mpr_add_entry "$@" ;;
	close) mpr_close "$@" ;;
	close-entry) mpr_close_entry "$@" ;;
	requeue-entry) mpr_requeue_entry "$@" ;;
	*) pmcli_usage_error "unknown subcommand: $subcommand" ;;
	esac
}

# Guard so the file can be sourced without executing main. main's return code
# is captured and re-exited explicitly as the final statement.
if [[ ${BASH_SOURCE[0]} == "${0}" ]]; then
	mpr_rc=0
	mpr_main "$@" || mpr_rc=$?
	exit "$mpr_rc"
fi
