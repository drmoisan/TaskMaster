#!/usr/bin/env bash
# parallel-mutation.sh: sourceable bash port of the three removal functions of
# the parallel mutation engine, so a workspace that received the Claude
# customization payload can decide a removal, recolor the unstarted subgraph,
# and build the removal's mutations[] record without an interpreter.
#
# Parity reference (the repository authority; this file reproduces its
# observable outputs and rejection messages byte for byte):
#   decide_removal      scripts/dev_tools/parallel_mutation_protocol.py
#   recolor_unstarted   scripts/dev_tools/parallel_mutation_protocol.py
#   build_remove_entry  scripts/dev_tools/_parallel_mutation_entries.py
#   MutationEntry       scripts/dev_tools/_parallel_mutation_models.py
#   rejection messages  scripts/dev_tools/_parallel_mutation_errors.py
#
# Declared divergences (also listed in both parity lanes):
#   1. A `--removal-disposition` value outside `detach abandon` is a usage error
#      (exit 2) raised by the command line before the library runs. A direct
#      library call and the Python `UnknownEnumMemberError` both reject it with
#      the same message, which lists `detach, abandon`
#      (spec D5; aligned under #890).
#   2. `at` is a caller-supplied string instead of a `datetime`.
#   3. Option abbreviations are rejected.
#
# Contract. Every public function is pure: no file I/O, no network access, no
# clock read, and no external utility beyond the ones parallel-cohorts.sh
# already uses. Each sets PM_RESULT to one compact JSON object and returns 0 on
# success, or sets PM_ERROR to the exact reference message and returns 1 on a
# rule rejection. Integer arguments are expected in canonical decimal form; the
# entry point remove-parallel-item.sh validates them before calling here.
#
# shellcheck disable=SC2034
# SC2034 is disabled file-wide because PM_RESULT and PM_ERROR are written here
# and read by the entry point remove-parallel-item.sh and by the bats suites,
# which shellcheck analyses separately.

# Resolve this file's own directory so its dependencies source regardless of
# the caller's working directory.
PM_LIB_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
# shellcheck source=.claude/lib/bash/parallel-cohorts.sh
# shellcheck disable=SC1091
source "$PM_LIB_DIR/parallel-cohorts.sh"

# The compact JSON object produced by the most recent success.
PM_RESULT=""

# The rejection message set by the most recent non-zero return.
PM_ERROR=""

# Literal prefix shared by every engine rejection message.
PM_REJECTION_PREFIX="Parallel mutation rejected:"

# Item states whose items have not started (vertices of the recolored subgraph).
PM_UNSTARTED_STATES="proposed admitted prepared scheduled"

# The single item state that pins an item against recoloring.
PM_PINNED_STATE="in_flight"

# The two accepted dispositions of an in-flight removal, in canonical order.
PM_VALID_DISPOSITIONS="detach abandon"

pm_fail() {
	# Record a rejection message and return 1.
	#
	# Args: $1 = the complete, literal rejection message.
	PM_ERROR="$1"
	return 1
}

pm_json_string() {
	# Echo a JSON string literal for a raw value.
	#
	# Args: $1 = the raw value. Escapes the backslash first so a later escape's
	# own backslash is not doubled, then the quote and the three control
	# characters a caller-supplied token could carry.
	local body="$1"
	body=${body//\\/\\\\}
	body=${body//\"/\\\"}
	body=${body//$'\n'/\\n}
	body=${body//$'\r'/\\r}
	body=${body//$'\t'/\\t}
	printf '"%s"' "$body"
}

pm_json_nullable_string() {
	# Echo a JSON string literal, or null for an empty value.
	#
	# Args: $1 = the raw value; empty means absent (Python None).
	if [[ -z ${1-} ]]; then
		printf 'null'
	else
		pm_json_string "$1"
	fi
}

pm_unknown_item_message() {
	# Echo the UnknownItemError message for one item key.
	#
	# Args: $1 = the offending integer key, rendered as Python repr renders it.
	printf '%s item key %s does not resolve to a tracked items[].issue_num.' \
		"$PM_REJECTION_PREFIX" "$1"
}

pm_entry_contract_message() {
	# Echo the MutationEntryContractError message for one op.
	#
	# Args: $1 = field name, $2 = the already-rendered repr of the value, $3 =
	# the op name, optional and defaulting to `remove` so every removal call
	# site passes two arguments unchanged.
	printf "Parallel mutation entry rejected: %s %s violates the F3 mutations[] contract for op '%s'." \
		"$1" "$2" "${3:-remove}"
}

pm_decide_removal() {
	# Decide the outcome of removing one item, per the FR2 behavior table.
	#
	# Args: $1 = item key, $2 = the item's state, or empty when the key resolves
	# to no tracked item, $3 = disposition, or empty when none was supplied.
	# Returns 0 with PM_RESULT holding the decision, 1 with PM_ERROR holding the
	# reference rejection message.
	local item_key="$1" state="${2-}" disposition="${3-}" prior
	PM_RESULT=""
	PM_ERROR=""
	# An absent record and a non-positive key both name nothing the run tracks;
	# the reference rejects the latter while constructing the item record.
	if [[ -z $state ]] || ((item_key <= 0)); then
		pm_fail "$(pm_unknown_item_message "$item_key")"
		return 1
	fi
	if ! pc_enum_members_contains "$PC_VALID_ITEM_STATES" "$state"; then
		pm_fail "$PM_REJECTION_PREFIX item $item_key state $(pc_repr str "$state") is not one of $PC_VALID_ITEM_STATES."
		return 1
	fi

	# Routing table, one branch per FR2 row. An unstarted item is a vertex of
	# the recolored subgraph, so its removal recomputes and records no
	# disposition even when one was supplied.
	if pc_contains_word "$PM_UNSTARTED_STATES" "$state"; then
		PM_RESULT="{\"item_key\":$item_key,\"prior_state\":$(pm_json_string "$state"),\"new_state\":\"withdrawn\",\"disposition\":null,\"triggers_recompute\":true}"
		return 0
	fi
	if [[ $state == "$PM_PINNED_STATE" ]]; then
		if [[ -z $disposition ]]; then
			pm_fail "$PM_REJECTION_PREFIX removal of in-flight item $item_key requires an explicit disposition, one of detach, abandon; no default is inferred."
			return 1
		fi
		if ! pc_contains_word "$PM_VALID_DISPOSITIONS" "$disposition"; then
			pm_fail "$PM_REJECTION_PREFIX item $item_key disposition $(pc_repr str "$disposition") is not one of ${PM_VALID_DISPOSITIONS// /, }."
			return 1
		fi
		prior="$PM_PINNED_STATE"
		PM_RESULT="{\"item_key\":$item_key,\"prior_state\":\"$prior\",\"new_state\":\"withdrawn\",\"disposition\":\"$disposition\",\"triggers_recompute\":false}"
		return 0
	fi
	if [[ $state == merged ]]; then
		pm_fail "$PM_REJECTION_PREFIX item $item_key is already merged into main and cannot be removed."
		return 1
	fi
	# Only withdrawn and blocked remain; neither is a live removal target.
	pm_fail "$(pm_unknown_item_message "$item_key")"
	return 1
}

pm_smallest_overlap() {
	# Echo the smallest key present in both key lists, or nothing.
	#
	# Args: $1 = space-separated unstarted keys, $2 = space-separated pinned keys.
	local key smallest=""
	pcoh_split_words "$1"
	local -a unstarted_keys=("${PCOH_WORDS[@]}")
	pcoh_split_words "$2"
	local pinned=" ${PCOH_WORDS[*]} "
	for key in "${unstarted_keys[@]}"; do
		[[ $pinned == *" $key "* ]] || continue
		if [[ -z $smallest ]] || ((key < smallest)); then
			smallest="$key"
		fi
	done
	printf '%s' "$smallest"
}

pm_render_assignments() {
	# Echo the cohort_assignments object for a cohort list and an offset.
	#
	# Args: $1 = compact JSON array of arrays from pcoh_compute_cohorts, $2 = the
	# uniform offset. List position is the local color index; keys are emitted
	# in ascending numeric order, matching the reference rendering.
	local cohorts="$1" offset="$2" group key index=0 lines="" ordered rendered=""
	# Strip the outer brackets and the first and last inner bracket, then turn
	# each `],[` boundary into a space so every group is one comma-joined word.
	cohorts=${cohorts#[}
	cohorts=${cohorts%]}
	cohorts=${cohorts#[}
	cohorts=${cohorts%]}
	pcoh_split_words "${cohorts//'],['/ }"
	local -a groups=("${PCOH_WORDS[@]}")
	for group in "${groups[@]}"; do
		pcoh_split_words "${group//,/ }"
		for key in "${PCOH_WORDS[@]}"; do
			lines="$lines$key $((offset + index))"$'\n'
		done
		index=$((index + 1))
	done
	if [[ -n $lines ]]; then
		ordered=$(printf '%s' "$lines" | LC_ALL=C sort -k1,1n)
		while IFS=' ' read -r key index; do
			[[ -n $key ]] || continue
			[[ -z $rendered ]] || rendered="$rendered,"
			rendered="$rendered\"$key\":$index"
		done <<<"$ordered"
	fi
	printf '{%s}' "$rendered"
}

pm_recolor_unstarted() {
	# Recolor the unstarted subgraph, leaving every pinned item untouched.
	#
	# Args: $1 = space-separated unstarted keys, $2 = space-separated `a:b`
	# conflict edges over all items, $3 = space-separated pinned keys, $4 = the
	# generation before this recolor, $5 = current_cohort, $6 =
	# highest_pinned_cohort. Returns 0 with PM_RESULT holding the assignments
	# and the next generation, 1 with PM_ERROR holding the reference message.
	local unstarted="$1" edges="$2" pinned="$3" generation="$4"
	local current_cohort="$5" highest_pinned="$6"
	local overlap edge first second crosses=0 induced="" offset
	PM_RESULT=""
	PM_ERROR=""

	# Check order is part of the contract: overlap, then the negative base,
	# then the pinned barrier, then the induced coloring.
	overlap=$(pm_smallest_overlap "$unstarted" "$pinned")
	if [[ -n $overlap ]]; then
		pm_fail "$(pm_unknown_item_message "$overlap")"
		return 1
	fi
	if ((current_cohort < 0)); then
		pm_fail "current_cohort must be >= 0 per F3 invariant 12; received $current_cohort."
		return 1
	fi

	pcoh_split_words "$unstarted"
	local unstarted_set=" ${PCOH_WORDS[*]} "
	pcoh_split_words "$pinned"
	local pinned_set=" ${PCOH_WORDS[*]} "
	pcoh_split_words "$edges"
	local -a edge_list=("${PCOH_WORDS[@]}")
	# Decide the pinned barrier before the induced restriction discards the
	# unstarted-to-pinned edges, then keep only edges with both ends unstarted.
	for edge in "${edge_list[@]}"; do
		first="${edge%%:*}"
		second="${edge#*:}"
		if [[ $unstarted_set == *" $first "* && $pinned_set == *" $second "* ]] ||
			[[ $unstarted_set == *" $second "* && $pinned_set == *" $first "* ]]; then
			crosses=1
		fi
		if [[ $unstarted_set == *" $first "* && $unstarted_set == *" $second "* ]]; then
			induced="$induced $edge"
		fi
	done

	if ! pcoh_compute_cohorts "$unstarted" "$induced"; then
		pm_fail "$PCOH_ERROR"
		return 1
	fi

	# A uniform shift keeps the local-to-absolute map injective.
	if ((crosses == 1)); then
		offset=$((highest_pinned + 1))
	else
		offset="$current_cohort"
	fi
	PM_RESULT="{\"cohort_assignments\":$(pm_render_assignments "$PCOH_RESULT" "$offset"),\"generation\":$((generation + 1))}"
	return 0
}

pm_build_remove_entry() {
	# Build the single mutations[] record for an accepted removal.
	#
	# Args: $1 = item key, $2 = prior state, $3 = disposition, or empty for
	# none, $4 = `true` when the removal recomputes, otherwise `false`, $5 = the
	# generation before the removal, $6 = the `at` timestamp string. Returns 0
	# with PM_RESULT holding the record in F3 field order, 1 with PM_ERROR
	# holding the reference contract message.
	local item_key="$1" prior_state="$2" disposition="${3-}" recompute="$4"
	local generation="$5" at="$6" stamped
	PM_RESULT=""
	PM_ERROR=""
	# The stamped generation is computed first; the record validation then
	# runs in field order, so the first violation names the earliest field.
	stamped="$generation"
	if [[ $recompute == true ]]; then
		stamped=$((generation + 1))
	fi
	if ((stamped < 0)); then
		pm_fail "$(pm_entry_contract_message recolor_generation "$stamped")"
		return 1
	fi
	if ((item_key <= 0)); then
		pm_fail "$(pm_entry_contract_message item_key "$item_key")"
		return 1
	fi
	if ! pc_enum_members_contains "$PC_VALID_ITEM_STATES" "$prior_state"; then
		pm_fail "$PM_REJECTION_PREFIX item $item_key state $(pc_repr str "$prior_state") is not one of $PC_VALID_ITEM_STATES."
		return 1
	fi
	# An in-flight removal must record a valid disposition; every other
	# removal must record none.
	if [[ $prior_state == "$PM_PINNED_STATE" ]]; then
		if [[ -z $disposition ]]; then
			pm_fail "$(pm_entry_contract_message disposition None)"
			return 1
		fi
		if ! pc_contains_word "$PM_VALID_DISPOSITIONS" "$disposition"; then
			pm_fail "$(pm_entry_contract_message disposition "$(pc_repr str "$disposition")")"
			return 1
		fi
	elif [[ -n $disposition ]]; then
		pm_fail "$(pm_entry_contract_message disposition "$(pc_repr str "$disposition")")"
		return 1
	fi
	PM_RESULT="{\"op\":\"remove\",\"item_key\":$item_key,\"at\":$(pm_json_string "$at"),\"prior_state\":$(pm_json_string "$prior_state"),\"new_state\":\"withdrawn\",\"disposition\":$(pm_json_nullable_string "$disposition"),\"recolor_generation\":$stamped}"
	return 0
}
