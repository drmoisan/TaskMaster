#!/usr/bin/env bash
# parallel-mutation-ops.sh: sourceable bash port of the add, close, and requeue
# functions of the parallel mutation engine (issue #890), so a workspace that
# received the Claude customization payload can decide an admission, gate a run
# close, and build the add, close, and requeue mutations[] records without an
# interpreter. It sources parallel-mutation.sh, which carries the removal
# functions and the shared message builders.
#
# Parity reference (the repository authority; this file reproduces its
# observable outputs and rejection messages byte for byte):
#   decide_admission     scripts/dev_tools/parallel_mutation_protocol.py
#   decide_close         scripts/dev_tools/parallel_mutation_protocol.py
#   build_add_entry      scripts/dev_tools/_parallel_mutation_entries.py
#   build_close_entry    scripts/dev_tools/_parallel_mutation_entries.py
#   build_requeue_entry  scripts/dev_tools/_parallel_mutation_entries.py
#   ItemRecord, MutationEntry  scripts/dev_tools/_parallel_mutation_models.py
#   rejection messages   scripts/dev_tools/_parallel_mutation_errors.py
#
# Declared divergences (also listed in both parity lanes):
#   1. A duplicate key in `close --items` is a usage error (exit 2) raised by
#      the command line; the Python mapping would keep the last record.
#   2. `at` is a caller-supplied string instead of a `datetime`.
#   3. Option abbreviations are rejected.
#   4. `admit` has no rejection branch, in either implementation.
#
# Contract. Every public function is pure: no file I/O, no network access, no
# clock read, and no external utility beyond `sort`. Each sets PM_RESULT to one
# compact JSON object and returns 0 on success, or sets PM_ERROR to the exact
# reference message and returns 1 on a rule rejection. Integer arguments are
# expected in canonical decimal form; the entry point mutate-parallel-run.sh
# validates them before calling here. Entry validation order follows
# MutationEntry: the stamped recolor_generation first, then item_key.
#
# shellcheck disable=SC2034
# SC2034 is disabled file-wide because PM_RESULT and PM_ERROR are written here
# and read by the entry point mutate-parallel-run.sh and by the bats suites,
# which shellcheck analyses separately.

# Resolve this file's own directory so its dependencies source regardless of
# the caller's working directory.
PMOPS_LIB_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
# shellcheck source=.claude/lib/bash/parallel-mutation.sh
# shellcheck disable=SC1091
source "$PMOPS_LIB_DIR/parallel-mutation.sh"

pm_decide_admission() {
	# Decide whether a candidate joins the current cohort or is deferred.
	#
	# Args: $1 = candidate key, $2 = space-separated `a:b` conflict edges over
	# all items, $3 = space-separated in-flight keys, $4 = space-separated
	# current-cohort member keys. The candidate is deferred when any edge joins
	# it to a key in the union of $3 and $4, in either endpoint position.
	# Returns 0 with PM_RESULT holding the decision; admission never rejects.
	local candidate="$1" edges="$2" edge first second outcome="admit_current_cohort"
	local recompute=false
	PM_RESULT=""
	PM_ERROR=""
	pcoh_split_words "$3 $4"
	local blocking=" ${PCOH_WORDS[*]} "
	pcoh_split_words "$edges"
	local -a edge_list=("${PCOH_WORDS[@]}")
	# Scan every edge for one joining the candidate to a blocking key; the
	# first such edge settles the decision.
	for edge in "${edge_list[@]}"; do
		first="${edge%%:*}"
		second="${edge#*:}"
		if { [[ $first == "$candidate" ]] && [[ $blocking == *" $second "* ]]; } ||
			{ [[ $second == "$candidate" ]] && [[ $blocking == *" $first "* ]]; }; then
			outcome="defer_and_recolor"
			recompute=true
			break
		fi
	done
	PM_RESULT="{\"candidate\":$candidate,\"outcome\":\"$outcome\",\"triggers_recompute\":$recompute}"
	return 0
}

pm_build_add_entry() {
	# Build the single mutations[] record for an accepted add.
	#
	# Args: $1 = item key, $2 = `true` for a deferred add (stamps g + 1),
	# otherwise `false` (stamps g), $3 = the generation before the add, $4 =
	# the `at` timestamp string. Returns 0 with PM_RESULT holding the record in
	# F3 field order, 1 with PM_ERROR holding the reference contract message.
	local item_key="$1" deferred="$2" generation="$3" at="$4" stamped
	PM_RESULT=""
	PM_ERROR=""
	stamped="$generation"
	if [[ $deferred == true ]]; then
		stamped=$((generation + 1))
	fi
	if ((stamped < 0)); then
		pm_fail "$(pm_entry_contract_message recolor_generation "$stamped" add)"
		return 1
	fi
	if ((item_key <= 0)); then
		pm_fail "$(pm_entry_contract_message item_key "$item_key" add)"
		return 1
	fi
	PM_RESULT="{\"op\":\"add\",\"item_key\":$item_key,\"at\":$(pm_json_string "$at"),\"prior_state\":null,\"new_state\":\"scheduled\",\"disposition\":null,\"recolor_generation\":$stamped}"
	return 0
}

pm_in_flight_list() {
	# Echo a key list as Python renders a list of ints, ascending.
	#
	# Args: $1 = space-separated keys, at least one.
	local ordered key rendered=""
	pcoh_split_words "$1"
	ordered=$(printf '%s\n' "${PCOH_WORDS[@]}" | LC_ALL=C sort -n)
	while IFS= read -r key; do
		[[ -n $key ]] || continue
		[[ -z $rendered ]] || rendered="$rendered, "
		rendered="$rendered$key"
	done <<<"$ordered"
	printf '[%s]' "$rendered"
}

pm_decide_close() {
	# Gate a run close on no item being in flight.
	#
	# Args: $1 = space-separated `<key>:<state>` tokens, one per tracked item.
	# Each token is validated in supplied order as the reference ItemRecord
	# constructor validates it: a non-positive key names nothing the run
	# tracks, and an out-of-enum state is rejected with the item-state members.
	# Returns 0 with PM_RESULT `{"close_permitted":true}`, or 1 with PM_ERROR
	# naming every in-flight key ascending.
	local token key state in_flight=""
	PM_RESULT=""
	PM_ERROR=""
	pcoh_split_words "$1"
	local -a tokens=("${PCOH_WORDS[@]}")
	for token in "${tokens[@]}"; do
		key="${token%%:*}"
		state="${token#*:}"
		if ((key <= 0)); then
			pm_fail "$(pm_unknown_item_message "$key")"
			return 1
		fi
		if ! pc_enum_members_contains "$PC_VALID_ITEM_STATES" "$state"; then
			pm_fail "$PM_REJECTION_PREFIX item $key state $(pc_repr str "$state") is not one of $PC_VALID_ITEM_STATES."
			return 1
		fi
		if [[ $state == "$PM_PINNED_STATE" ]]; then
			in_flight="$in_flight $key"
		fi
	done
	if [[ -n $in_flight ]]; then
		pm_fail "$PM_REJECTION_PREFIX close requires no item in flight; still in flight: $(pm_in_flight_list "$in_flight")."
		return 1
	fi
	PM_RESULT='{"close_permitted":true}'
	return 0
}

pm_build_close_entry() {
	# Build the single run-scoped mutations[] record for an accepted close.
	#
	# Args: $1 = the generation, stamped unchanged, $2 = the `at` timestamp
	# string. Returns 0 with PM_RESULT holding the record, 1 with PM_ERROR when
	# the generation is negative.
	local generation="$1" at="$2"
	PM_RESULT=""
	PM_ERROR=""
	if ((generation < 0)); then
		pm_fail "$(pm_entry_contract_message recolor_generation "$generation" close)"
		return 1
	fi
	PM_RESULT="{\"op\":\"close\",\"item_key\":null,\"at\":$(pm_json_string "$at"),\"prior_state\":null,\"new_state\":null,\"disposition\":null,\"recolor_generation\":$generation}"
	return 0
}

pm_build_requeue_entry() {
	# Build the single mutations[] record for a drift-induced requeue.
	#
	# Args: $1 = item key, $2 = the generation before the requeue (stamps
	# g + 1), $3 = the `at` timestamp string. Returns 0 with PM_RESULT holding
	# the record, 1 with PM_ERROR holding the reference contract message.
	local item_key="$1" generation="$2" at="$3" stamped
	PM_RESULT=""
	PM_ERROR=""
	stamped=$((generation + 1))
	if ((stamped < 0)); then
		pm_fail "$(pm_entry_contract_message recolor_generation "$stamped" requeue)"
		return 1
	fi
	if ((item_key <= 0)); then
		pm_fail "$(pm_entry_contract_message item_key "$item_key" requeue)"
		return 1
	fi
	PM_RESULT="{\"op\":\"requeue\",\"item_key\":$item_key,\"at\":$(pm_json_string "$at"),\"prior_state\":\"$PM_PINNED_STATE\",\"new_state\":\"blocked\",\"disposition\":null,\"recolor_generation\":$stamped}"
	return 0
}
