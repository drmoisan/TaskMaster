#!/usr/bin/env bash
# parallel-cohorts.sh: sourceable bash port of
# scripts/dev_tools/parallel_cohort_computation.py. Partitions an item-key set
# into execution cohorts by deterministic greedy graph coloring in Welsh-Powell
# order, and chunks one cohort into concurrency-capped batches.
#
# Determinism is the objective, not optimality. Every ordering decision is made
# by an explicit numeric sort under LC_ALL=C: the Welsh-Powell visit order comes
# from `sort -k1,1nr -k2,2n` over `<degree> <item_key>` lines, which is the
# composite key (-degree, item_key) ascending; cohort membership and batch
# contents come from `sort -n`. Associative arrays are used for adjacency
# membership, degree counting, and index assignment only, and their iteration
# order never reaches output.
#
# Error messages reproduce the Python module's four literal failure messages
# byte for byte, including the tuple repr `(a, b)` with its comma and space.
# The caller reads PCOH_ERROR after a non-zero return.
#
# The Python module remains the repository authority.
#
# shellcheck disable=SC2034
# SC2034 is disabled file-wide because PCOH_ERROR and PCOH_RESULT are
# written here and read by the entry points compute-cohorts.sh and
# compute-concurrency-batches.sh, which shellcheck analyses separately.

# Resolve this file's own directory so its dependencies source regardless of
# the caller's working directory.
PCOH_LIB_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
# shellcheck source=.claude/lib/bash/parallel-common.sh
# shellcheck disable=SC1091
source "$PCOH_LIB_DIR/parallel-common.sh"

# Adjacency membership flags keyed by `a,b`; never iterated into output.
declare -gA PCOH_ADJACENT=()

# Distinct-neighbor degree per item key; never iterated into output.
declare -gA PCOH_DEGREE=()

# Space-separated neighbor list per item key; never iterated into output.
declare -gA PCOH_NEIGHBORS=()

# Assigned cohort index per item key; never iterated into output.
declare -gA PCOH_INDEX_OF=()

# The failure message set by the most recent non-zero return.
PCOH_ERROR=""

# The compact JSON array-of-arrays produced by the most recent success.
PCOH_RESULT=""

# Scratch array populated by pcoh_split_words.
PCOH_WORDS=()

pcoh_split_words() {
	# Populate PCOH_WORDS by splitting a space-separated string.
	#
	# Args: $1 = the string to split. An empty or whitespace-only string yields
	# an empty array, which is the empty-graph and empty-cohort case. `read -ra`
	# is used rather than an unquoted expansion so a token is never subjected to
	# pathname expansion.
	PCOH_WORDS=()
	read -ra PCOH_WORDS <<<"${1-}"
}

pcoh_fail() {
	# Record a failure message and return 1.
	#
	# Args: $1 = the complete, literal failure message.
	PCOH_ERROR="$1"
	return 1
}

pcoh_validate_item_keys() {
	# Reject duplicate item keys, walking the supplied order.
	#
	# Duplicates break the uniqueness assumption behind the total-order sort
	# key (-degree, item_key), so they are rejected rather than deduplicated.
	#
	# Args: $1 = space-separated item keys.
	# Returns 0 when every key is distinct, 1 with PCOH_ERROR otherwise.
	local key seen=""
	pcoh_split_words "$1"
	local -a key_items=("${PCOH_WORDS[@]}")
	# Walk in the supplied order so the first repeat encountered is the key
	# reported, which keeps the message stable for a given input.
	for key in "${key_items[@]}"; do
		if pc_contains_word "$seen" "$key"; then
			pcoh_fail "Duplicate item key $key in item_keys; item keys must be unique because cohort ordering relies on key uniqueness."
			return 1
		fi
		seen="$seen $key"
	done
	return 0
}

pcoh_record_edge() {
	# Record one validated edge on both endpoints, ignoring repeats.
	#
	# Storing a membership flag per ordered pair normalizes the edge list by
	# construction: an edge supplied as (a, b), as (b, a), or supplied
	# repeatedly collapses to the same single neighbor entry on both sides.
	#
	# Args: $1 = first endpoint, $2 = second endpoint.
	local first="$1" second="$2"
	[[ -z ${PCOH_ADJACENT["$first,$second"]-} ]] || return 0
	PCOH_ADJACENT["$first,$second"]=1
	PCOH_ADJACENT["$second,$first"]=1
	PCOH_NEIGHBORS["$first"]="${PCOH_NEIGHBORS["$first"]} $second"
	PCOH_NEIGHBORS["$second"]="${PCOH_NEIGHBORS["$second"]} $first"
	PCOH_DEGREE["$first"]=$((PCOH_DEGREE["$first"] + 1))
	PCOH_DEGREE["$second"]=$((PCOH_DEGREE["$second"] + 1))
}

pcoh_build_adjacency() {
	# Build symmetric adjacency, normalizing edge direction and duplicates.
	#
	# Args: $1 = space-separated item keys, $2 = space-separated `a:b` edges.
	# Populates PCOH_DEGREE and PCOH_NEIGHBORS. Returns 0 on success, 1 with
	# PCOH_ERROR when an edge is a self-loop or names an undeclared endpoint.
	local keys="$1" key edge first second
	PCOH_DEGREE=()
	PCOH_NEIGHBORS=()
	PCOH_ADJACENT=()
	pcoh_split_words "$keys"
	local -a key_list=("${PCOH_WORDS[@]}")
	# Seed every declared key so an isolated vertex survives into the coloring
	# step; inferring vertices from the edge list alone would drop it.
	for key in "${key_list[@]}"; do
		PCOH_DEGREE["$key"]=0
		PCOH_NEIGHBORS["$key"]=""
	done

	pcoh_split_words "${2-}"
	local -a edge_list=("${PCOH_WORDS[@]}")
	# Validate then record each conflict. Validation order is part of the
	# contract: the self-loop check precedes the endpoint-membership check, and
	# the first endpoint is checked before the second.
	for edge in "${edge_list[@]}"; do
		first="${edge%%:*}"
		second="${edge#*:}"
		if [[ $first == "$second" ]]; then
			pcoh_fail "Self-loop edge on item key $first; the conflict relation is defined over distinct items, so an item cannot conflict with itself."
			return 1
		fi
		if ! pc_contains_word "$keys" "$first"; then
			pcoh_fail "Conflict edge ($first, $second) names item key $first, which is not a member of item_keys; every edge endpoint must be a declared item key."
			return 1
		fi
		if ! pc_contains_word "$keys" "$second"; then
			pcoh_fail "Conflict edge ($first, $second) names item key $second, which is not a member of item_keys; every edge endpoint must be a declared item key."
			return 1
		fi
		pcoh_record_edge "$first" "$second"
	done
	return 0
}

pcoh_welsh_powell_order() {
	# Echo the item keys in Welsh-Powell visit order, one per line.
	#
	# The composite key (-degree, item_key) is a total order because item keys
	# are unique. That is the single load-bearing determinism guard: the visit
	# order depends on the graph alone, never on the caller's input order. The
	# ordering is delegated to sort so no bash-side comparison logic can drift
	# from the Python sort key.
	#
	# Args: $1 = space-separated item keys.
	local key
	pcoh_split_words "$1"
	local -a key_items=("${PCOH_WORDS[@]}")
	for key in "${key_items[@]}"; do
		printf '%s %s\n' "${PCOH_DEGREE["$key"]}" "$key"
	done | LC_ALL=C sort -k1,1nr -k2,2n | cut -d' ' -f2
}

pcoh_lowest_free_index() {
	# Echo the smallest cohort index not held by any assigned neighbor.
	#
	# Args: $1 = the item key being placed.
	local key="$1" neighbor taken="" candidate=0
	pcoh_split_words "${PCOH_NEIGHBORS["$key"]}"
	local -a neighbors=("${PCOH_WORDS[@]}")
	# Collecting neighbor indices into a membership string is order
	# insensitive, so reading the neighbor list here cannot affect the outcome.
	for neighbor in "${neighbors[@]}"; do
		if [[ -n ${PCOH_INDEX_OF["$neighbor"]-} ]]; then
			taken="$taken ${PCOH_INDEX_OF["$neighbor"]}"
		fi
	done
	# Scan upward from zero for the first free index. An isolated vertex and
	# the first-visited vertex both land in cohort 0.
	while pc_contains_word "$taken" "$candidate"; do
		candidate=$((candidate + 1))
	done
	printf '%s' "$candidate"
}

pcoh_assign_cohort_indices() {
	# Assign each vertex the lowest cohort index free among its neighbors.
	#
	# Args: $1 = space-separated item keys. Populates PCOH_INDEX_OF. A vertex
	# never takes an index already held by a neighbor, so each index class is
	# an independent set of the conflict graph.
	local ordered key
	PCOH_INDEX_OF=()
	ordered=$(pcoh_welsh_powell_order "$1")
	[[ -n $ordered ]] || return 0
	# Visit vertices in Welsh-Powell order; each takes the smallest index its
	# already-assigned neighbors have not taken.
	while IFS= read -r key; do
		[[ -n $key ]] || continue
		PCOH_INDEX_OF["$key"]=$(pcoh_lowest_free_index "$key")
	done <<<"$ordered"
	return 0
}

pcoh_render_cohorts() {
	# Echo the compact JSON array-of-arrays for the current assignment.
	#
	# Args: $1 = space-separated item keys. List position is the cohort index
	# and each inner list holds that cohort's keys sorted ascending.
	local key index highest=-1 ordered
	pcoh_split_words "$1"
	local -a key_items=("${PCOH_WORDS[@]}")
	for key in "${key_items[@]}"; do
		index="${PCOH_INDEX_OF["$key"]}"
		((index > highest)) && highest=$index
	done
	if ((highest < 0)); then
		printf '[]'
		return 0
	fi

	local -a buckets=()
	for ((index = 0; index <= highest; index++)); do
		buckets+=("")
	done
	ordered=$(printf '%s\n' "${key_items[@]}" | LC_ALL=C sort -n)
	# Walk the ascending key order rather than any hash order, so each cohort's
	# keys come out ascending regardless of how the caller ordered its input.
	while IFS= read -r key; do
		[[ -n $key ]] || continue
		index="${PCOH_INDEX_OF["$key"]}"
		if [[ -z ${buckets[index]} ]]; then
			buckets[index]="$key"
		else
			buckets[index]="${buckets[index]},$key"
		fi
	done <<<"$ordered"

	local rendered=""
	for ((index = 0; index <= highest; index++)); do
		[[ -z $rendered ]] || rendered="$rendered,"
		rendered="${rendered}[${buckets[index]}]"
	done
	printf '[%s]' "$rendered"
}

pcoh_compute_cohorts() {
	# Partition item keys into cohorts by deterministic greedy graph coloring.
	#
	# Args: $1 = space-separated item keys, $2 = space-separated `a:b` edges.
	# On success returns 0 with PCOH_RESULT holding the compact JSON output.
	# On malformed input returns 1 with PCOH_ERROR holding the exact Python
	# message. All validation runs before any coloring work.
	local keys="$1" edges="${2-}"
	PCOH_ERROR=""
	PCOH_RESULT=""
	pcoh_validate_item_keys "$keys" || return 1
	pcoh_build_adjacency "$keys" "$edges" || return 1
	pcoh_assign_cohort_indices "$keys"
	PCOH_RESULT=$(pcoh_render_cohorts "$keys")
	return 0
}

pcoh_compute_concurrency_batches() {
	# Chunk one cohort into concurrency-capped batches in ascending key order.
	#
	# The cohort's keys are sorted inside this function rather than trusting
	# the caller's ordering, so determinism does not depend on caller
	# discipline. Every batch is exactly max_concurrency long except a possibly
	# smaller final batch.
	#
	# Args: $1 = space-separated cohort item keys, $2 = the fan-out cap.
	# On success returns 0 with PCOH_RESULT holding the compact JSON output;
	# on a cap below 1 returns 1 with PCOH_ERROR holding the exact message.
	local cap="$2" key sorted
	PCOH_ERROR=""
	PCOH_RESULT=""
	# A cap below 1 would admit no items into any batch, so the cohort could
	# never drain; reject it rather than returning an unusable schedule.
	if ((cap < 1)); then
		pcoh_fail "max_concurrency must be >= 1; received $cap."
		return 1
	fi

	pcoh_split_words "$1"
	local -a supplied=("${PCOH_WORDS[@]}")
	local -a ordered=()
	if ((${#supplied[@]} > 0)); then
		sorted=$(printf '%s\n' "${supplied[@]}" | LC_ALL=C sort -n)
		while IFS= read -r key; do
			[[ -n $key ]] || continue
			ordered+=("$key")
		done <<<"$sorted"
	fi

	local total=${#ordered[@]}
	if ((total == 0)); then
		PCOH_RESULT="[]"
		return 0
	fi

	local start end position rendered="" batch
	# Walk the sorted keys in fixed-size strides so slot filling follows
	# ascending key order and the batch boundaries are reproducible.
	for ((start = 0; start < total; start += cap)); do
		end=$((start + cap))
		((end > total)) && end=$total
		batch=""
		for ((position = start; position < end; position++)); do
			[[ -z $batch ]] || batch="$batch,"
			batch="$batch${ordered[position]}"
		done
		[[ -z $rendered ]] || rendered="$rendered,"
		rendered="${rendered}[$batch]"
	done
	PCOH_RESULT="[$rendered]"
	return 0
}
