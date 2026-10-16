#!/usr/bin/env bash
# remove-parallel-item.sh: destination-portable command-line entry point for
# the parallel-remove skill's decision, recolor, and audit-record steps. It
# exists so a workspace that received the Claude customization payload can run
# those steps with nothing but bash -- no interpreter, no repository checkout.
#
# Usage:
#   bash .claude/lib/bash/remove-parallel-item.sh decide --item <key> \
#       [--state <state>] [--removal-disposition detach|abandon]
#   bash .claude/lib/bash/remove-parallel-item.sh recolor --unstarted "<k> ..." \
#       [--edges "<a>:<b> ..."] --pinned "<k> ..." --generation <g> \
#       --current-cohort <c> --highest-pinned-cohort <h>
#   bash .claude/lib/bash/remove-parallel-item.sh entry --item <key> \
#       --prior-state <state> [--removal-disposition detach|abandon] \
#       --recompute true|false --generation <g> [--at <timestamp>]
#
# `decide` without `--state` means the key resolves to no tracked item.
# `--unstarted`, `--edges`, and `--pinned` accept an empty string. Integer
# tokens match `-?(0|[1-9][0-9]*)`. `--at` defaults to the current UTC time as
# yyyy-MM-ddTHH-mm; the clock is read only when `--at` is omitted.
#
# Exit-code contract:
#   exit 0  success; one compact JSON object on stdout
#   exit 1  rule rejection; the reference implementation's exact message as
#           one stderr line
#   exit 2  usage error; one stderr line beginning
#           "PARALLEL_MUTATION_ERROR: usage error:"
# Argument validation runs first, then rule rejection, then output.
#
# Parity reference: scripts/dev_tools/parallel_mutation_protocol.py
# (decide_removal, recolor_unstarted) and
# scripts/dev_tools/_parallel_mutation_entries.py (build_remove_entry).
#
# Declared divergences (also listed in both parity lanes):
#   1. A `--removal-disposition` value outside `detach abandon` is a usage error
#      (exit 2) raised by the command line before the library runs. A direct
#      library call and the Python `UnknownEnumMemberError` both reject it with
#      the same message, which lists `detach, abandon`
#      (spec D5; aligned under #890).
#   2. `at` is a caller-supplied string instead of a `datetime`.
#   3. Option abbreviations are rejected.
set -euo pipefail

# Resolve this script's own directory so the libraries source regardless of
# cwd. The option-parsing helpers (pmcli_*) are shared with
# mutate-parallel-run.sh through parallel-mutation-cli.sh (issue #890).
RPI_SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
# shellcheck source=.claude/lib/bash/parallel-mutation.sh
# shellcheck disable=SC1091
source "$RPI_SCRIPT_DIR/parallel-mutation.sh"
# shellcheck source=.claude/lib/bash/parallel-mutation-cli.sh
# shellcheck disable=SC1091
source "$RPI_SCRIPT_DIR/parallel-mutation-cli.sh"

pc_enforce_c_locale

rpi_disposition() {
	# Validate the optional disposition; store it, or empty when absent.
	PMCLI_VALUE="${PMCLI_OPTS["removal-disposition"]-}"
	if [[ -n ${PMCLI_OPTS["removal-disposition"]+set} ]] && ! pc_contains_word "detach abandon" "$PMCLI_VALUE"; then
		pmcli_usage_error "--removal-disposition must be one of detach, abandon; found: $PMCLI_VALUE"
	fi
}

rpi_decide() {
	# Run the decide subcommand.
	local item state="" disposition rc=0
	pmcli_parse_options "item state removal-disposition" "$@"
	pmcli_require item
	pmcli_integer "${PMCLI_OPTS[item]}" "--item"
	item="$PMCLI_VALUE"
	if [[ -n ${PMCLI_OPTS[state]+set} ]]; then
		state="${PMCLI_OPTS[state]}"
		pc_enum_members_contains "$PC_VALID_ITEM_STATES" "$state" ||
			pmcli_usage_error "--state must be one of $PC_VALID_ITEM_STATES; found: $state"
	fi
	rpi_disposition
	disposition="$PMCLI_VALUE"
	pm_decide_removal "$item" "$state" "$disposition" || rc=$?
	pmcli_emit "$rc"
}

rpi_recolor() {
	# Run the recolor subcommand.
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

rpi_entry() {
	# Run the entry subcommand.
	local item prior disposition recompute generation at rc=0
	pmcli_parse_options "item prior-state removal-disposition recompute generation at" "$@"
	pmcli_require item prior-state recompute generation
	pmcli_integer "${PMCLI_OPTS[item]}" "--item"
	item="$PMCLI_VALUE"
	prior="${PMCLI_OPTS["prior-state"]}"
	[[ $prior =~ ^[A-Za-z_]+$ ]] || pmcli_usage_error "--prior-state must match [A-Za-z_]+; found: $prior"
	rpi_disposition
	disposition="$PMCLI_VALUE"
	recompute="${PMCLI_OPTS[recompute]}"
	[[ $recompute == true || $recompute == false ]] ||
		pmcli_usage_error "--recompute must be true or false; found: $recompute"
	pmcli_integer "${PMCLI_OPTS[generation]}" "--generation"
	generation="$PMCLI_VALUE"
	# The clock is read only when --at is omitted.
	pmcli_timestamp
	at="$PMCLI_VALUE"
	pm_build_remove_entry "$item" "$prior" "$disposition" "$recompute" "$generation" "$at" || rc=$?
	pmcli_emit "$rc"
}

rpi_main() {
	# Dispatch on the subcommand.
	(($# > 0)) || pmcli_usage_error "missing subcommand; expected one of decide, recolor, entry"
	local subcommand="$1"
	shift
	case "$subcommand" in
	decide) rpi_decide "$@" ;;
	recolor) rpi_recolor "$@" ;;
	entry) rpi_entry "$@" ;;
	*) pmcli_usage_error "unknown subcommand: $subcommand" ;;
	esac
}

# Guard so the file can be sourced without executing main. main's return code
# is captured and re-exited explicitly as the final statement.
if [[ ${BASH_SOURCE[0]} == "${0}" ]]; then
	rpi_rc=0
	rpi_main "$@" || rpi_rc=$?
	exit "$rpi_rc"
fi
