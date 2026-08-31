#!/usr/bin/env bash
# parallel-lane-assertion.sh: sourceable bash port of
# scripts/dev_tools/parallel_lane_assertion.py. Answers one question for a
# parallel-run planner: did the hand-authored lane grouping in the manifest's
# optional `expected_conflict_components` key (invariant M8) survive
# blast-radius derivation? The module derives the connected components of the
# DERIVED conflict graph and reports every disagreement with the asserted
# grouping, in four classes. The Python module remains the repository authority
# and this file must reproduce its output byte for byte.
#
# Responsibilities and boundaries. This module is a DIAGNOSTIC. It never
# overrides a derived conflict edge, never feeds the cohort computation, never
# influences scheduling, and writes no artifact and no checkpoint field. Every
# finding is advisory: the entry point reports and exits 0 whether or not
# findings were produced, so a disagreement can never block a planning run. The
# asserted grouping is an ASSERTION, not a declaration.
#
# Scope. Every function here is pure in the sense the Python reference declares
# for itself at scripts/dev_tools/parallel_lane_assertion.py:34-38: it reads the
# node table pm_parse_manifest already populated plus its own arguments, writes
# only this module's own PLA_ globals, reads no file and no clock, and starts no
# process other than `sort`. The single I/O boundary is the entry point
# .claude/lib/bash/report-lane-assertion.sh, which calls pc_enforce_c_locale
# before any work, so every sort and character class below is byte ordered.
#
# shellcheck disable=SC2034
# SC2034 is disabled file-wide because the class-token constants and the result
# globals declared here are written in this file and read by the entry point
# .claude/lib/bash/report-lane-assertion.sh, which shellcheck analyses as a
# separate file and therefore cannot see the use.

# Resolve this file's own directory so its dependency sources regardless of the
# caller's working directory.
PLA_LIB_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
# shellcheck source=.claude/lib/bash/parallel-manifest-validate.sh
# shellcheck disable=SC1091
source "$PLA_LIB_DIR/parallel-manifest-validate.sh"

# The four report classes. Each is a stable token so a consumer can group
# findings without parsing the human-readable detail text.
PLA_EXPECTED_TOGETHER_DERIVED_APART="expected_together_derived_apart"
PLA_EXPECTED_APART_DERIVED_TOGETHER="expected_apart_derived_together"
PLA_MEMBER_NAMES_NO_ITEM="member_names_no_item"
PLA_ITEM_COVERED_BY_NO_COMPONENT="item_covered_by_no_component"

# The single informational class, held as a space-separated set so
# pc_contains_word decides membership. The other three classes indicate a real
# disagreement with the derived graph; this one only reports that the assertion
# was silent about an item.
PLA_INFORMATIONAL_KINDS="$PLA_ITEM_COVERED_BY_NO_COMPONENT"

# Endpoint separator on the command line, matching the `--edges "<a>:<b> ..."`
# convention of the cohort-computation entry points.
PLA_EDGE_SEPARATOR=":"

# Parsed conflict edges in input order, one `<first> <second>` pair per element.
# Populated by pla_parse_edges and read by pla_derive_components.
PLA_EDGES=()

pla_is_lexical_integer() {
	# Return 0 when a token is a decimal integer in the strict accepted lexis.
	#
	# Args: $1 = the raw token. The lexis is the one
	# .claude/lib/bash/compute-cohorts.sh:59 already applies, so the entry points
	# agree on what an endpoint is. It is deliberately stricter than Python's
	# int(), which also accepts a leading `+`, a leading zero, an underscore
	# separator, and a non-ASCII decimal digit; those forms are a declared
	# divergence class, excluded rather than reproduced, because reproducing them
	# would need a second lexis no other entry point shares.
	[[ $1 =~ ^-?(0|[1-9][0-9]*)$ ]]
}

pla_parse_edges() {
	# Populate PLA_EDGES from a whitespace-separated `<a>:<b>` edge list.
	#
	# Args: $1 = the raw --edges value; empty or whitespace-only yields no
	# edges. A token that is not two accepted integers separated by a colon is
	# dropped rather than aborting the diagnostic, which would deny the operator
	# the findings that the remaining edges still support. Input order is
	# preserved because it is observable through the derived-component order.
	local text="${1-}" token first second
	PLA_EDGES=()
	local -a tokens=()
	# read -ra rather than an unquoted expansion, so a token carrying a glob
	# character is never subjected to pathname expansion.
	read -ra tokens <<<"$text"
	for token in "${tokens[@]}"; do
		# Partition on the FIRST colon, matching str.partition: a token with two
		# colons yields a second endpoint that still carries one, which the lexis
		# then rejects.
		[[ $token == *"$PLA_EDGE_SEPARATOR"* ]] || continue
		first="${token%%"$PLA_EDGE_SEPARATOR"*}"
		second="${token#*"$PLA_EDGE_SEPARATOR"}"
		pla_is_lexical_integer "$first" || continue
		pla_is_lexical_integer "$second" || continue
		PLA_EDGES+=("$first $second")
	done
}

pla_sort_numeric() {
	# Echo a space-separated integer list sorted ascending.
	#
	# Args: $1 = the space-separated list, $2 = `unique` to collapse repeats.
	# Duplicates are retained by default because a finding's member list
	# reproduces the asserted membership, repeats included, whereas the declared
	# item keys are a set. `sort -n` is used rather than a shell comparison so
	# ordering is numeric under the C locale pc_enforce_c_locale pins.
	local input="${1-}" mode="${2-}" out="" line
	local -a words=()
	read -ra words <<<"$input"
	((${#words[@]} > 0)) || return 0
	# Negated test so the guard's own status is 0 on both branches under set -e.
	local -a flags=(-n)
	[[ $mode != unique ]] || flags=(-n -u)
	while IFS= read -r line; do
		[[ -n $line ]] || continue
		out="$out $line"
	done < <(printf '%s\n' "${words[@]}" | sort "${flags[@]}")
	printf '%s' "${out# }"
}

# Asserted lanes read from the manifest, as parallel arrays indexed by the
# position of the entry within the READABLE entries, which is the index the
# Python reference's enumerate() produces and therefore the index that reaches
# the `component[{position}]` label.
PLA_EXPECTED_COUNT=0
PLA_EXPECTED_HAS_NAME=()
PLA_EXPECTED_NAMES=()
PLA_EXPECTED_MEMBERS=()

# Declared item keys, space separated, de-duplicated and ascending.
PLA_ITEM_KEYS=""

pla_read_expected_components() {
	# Populate the PLA_EXPECTED_* arrays from expected_conflict_components.
	#
	# Reads defensively: every shape rule is already enforced by invariant M8,
	# so a malformed entry is skipped rather than reported -- a diagnostic must
	# not fail on input a validator already rejects, and must not emit a second
	# message for a defect M8 reports once.
	local root="expected_conflict_components" total position comp_path
	local members_path member_path member_count index members
	local member_type member_value
	PLA_EXPECTED_COUNT=0
	PLA_EXPECTED_HAS_NAME=()
	PLA_EXPECTED_NAMES=()
	PLA_EXPECTED_MEMBERS=()
	[[ $(yp_type_of "$root") == seq ]] || return 0
	total=$(yp_count_of "$root")
	for ((position = 0; position < total; position++)); do
		comp_path="${root}[${position}]"
		[[ $(yp_type_of "$comp_path") == map ]] || continue
		members_path="${comp_path}.members"
		[[ $(yp_type_of "$members_path") == seq ]] || continue
		# A non-string name reads as absent, so the entry is labelled by
		# position; the name is diagnostic only and never carries identity.
		if [[ $(yp_type_of "${comp_path}.name") == str ]]; then
			PLA_EXPECTED_HAS_NAME+=(1)
			PLA_EXPECTED_NAMES+=("$(yp_value_of "${comp_path}.name")")
		else
			PLA_EXPECTED_HAS_NAME+=(0)
			PLA_EXPECTED_NAMES+=("")
		fi
		members=""
		member_count=$(yp_count_of "$members_path")
		# Manifest order, no de-duplication: the reference keeps the authored
		# tuple as written and sorts only when it renders a finding.
		for ((index = 0; index < member_count; index++)); do
			member_path="${members_path}[${index}]"
			member_type=$(yp_type_of "$member_path")
			member_value=$(yp_value_of "$member_path")
			pc_is_positive_integer "$member_type" "$member_value" || continue
			members="$members $member_value"
		done
		PLA_EXPECTED_MEMBERS+=("${members# }")
		PLA_EXPECTED_COUNT=$((PLA_EXPECTED_COUNT + 1))
	done
}

pla_read_manifest_inputs() {
	# Read the asserted lanes and the declared item keys from the node table.
	#
	# The node table must already be populated by pm_parse_manifest. Populates
	# PLA_EXPECTED_COUNT, the three PLA_EXPECTED_* arrays, and PLA_ITEM_KEYS.
	# Either side may be empty, which is the manifest that asserts nothing and
	# the manifest that declares no item.
	pla_read_expected_components
	# pm_declared_issue_nums already applies the positive-integer test the
	# reference's guard applies, so the two agree on what resolves to an item.
	PLA_ITEM_KEYS=$(pla_sort_numeric "$(pm_declared_issue_nums)" unique)
}

# The derived partition: one element per component, each a space-separated
# ascending member list, ordered by lowest member.
PLA_COMPONENTS=()

pla_derive_components() {
	# Populate PLA_COMPONENTS with the connected components of the conflict
	# graph.
	#
	# Args: $1 = space-separated declared item keys. Reads the parsed edge list
	# from PLA_EDGES. A lane whose items mutually conflict is one connected
	# component; two lanes sharing no edge are two components.
	local keys="$1" root neighbour edge first second current head
	local -a key_tokens=() roots=() neighbours=() queue=()
	local -A adjacency=() seen=()
	PLA_COMPONENTS=()

	# Seed adjacency from every declared key so an isolated vertex survives as
	# its own single-member component.
	read -ra key_tokens <<<"$keys"
	for root in "${key_tokens[@]}"; do
		adjacency["$root"]=""
	done

	# Record each conflict on both endpoints; that symmetry, plus the
	# membership test before the append, makes edge direction and repetition
	# irrelevant. A self-loop and an edge naming an undeclared vertex are
	# skipped rather than reported: malformed-edge reporting belongs to the
	# checkpoint validators, and a diagnostic must degrade gracefully.
	for edge in "${PLA_EDGES[@]}"; do
		first="${edge%% *}"
		second="${edge##* }"
		[[ $first != "$second" ]] || continue
		[[ -n ${adjacency["$first"]+set} ]] || continue
		[[ -n ${adjacency["$second"]+set} ]] || continue
		pc_contains_word "${adjacency["$first"]}" "$second" ||
			adjacency["$first"]="${adjacency["$first"]} $second"
		pc_contains_word "${adjacency["$second"]}" "$first" ||
			adjacency["$second"]="${adjacency["$second"]} $first"
	done

	# Seed a breadth-first walk from each unvisited vertex in ascending key
	# order, making the component sequence a function of the graph alone. A
	# component's first root is necessarily its smallest member -- a smaller one
	# would have been reached from an earlier root and marked seen -- so the
	# components come out ordered by lowest member and need no second sort.
	read -ra roots <<<"$(pla_sort_numeric "$keys" unique)"
	for root in "${roots[@]}"; do
		[[ -z ${seen["$root"]+set} ]] || continue
		seen["$root"]=1
		queue=("$root")
		head=0
		# The visited set is marked at enqueue time, so a vertex reachable by
		# two paths is enqueued once.
		while ((head < ${#queue[@]})); do
			current="${queue[head]}"
			head=$((head + 1))
			read -ra neighbours <<<"${adjacency["$current"]}"
			for neighbour in "${neighbours[@]}"; do
				[[ -z ${seen["$neighbour"]+set} ]] || continue
				seen["$neighbour"]=1
				queue+=("$neighbour")
			done
		done
		PLA_COMPONENTS+=("$(pla_sort_numeric "${queue[*]}" unique)")
	done
}

# Findings in emission order, as parallel arrays. PLA_FINDING_MEMBERS holds the
# ascending item keys the finding concerns, so a consumer can act on a finding
# without re-parsing its detail text.
PLA_FINDING_KINDS=()
PLA_FINDING_DETAILS=()
PLA_FINDING_MEMBERS=()

# Flat key -> component-index lookups on both sides, which turn every membership
# question in the comparison into a constant-time test. Neither is ever
# iterated, so no hash order reaches output.
declare -gA PLA_DERIVED_INDEX=()
declare -gA PLA_EXPECTED_INDEX=()

pla_add_finding() {
	# Append one finding: $1 = class token, $2 = detail, $3 = member list.
	PLA_FINDING_KINDS+=("$1")
	PLA_FINDING_DETAILS+=("$2")
	PLA_FINDING_MEMBERS+=("$3")
}

pla_count_distinct() {
	# Echo the number of distinct words in a space-separated list.
	# The accumulator is named distinctly from the associative `seen` set in
	# pla_derive_components: shellcheck resolves a variable name file-wide, so
	# reusing the name there makes it read this string as that array.
	local input="${1-}" counted="" word count=0
	local -a words=()
	read -ra words <<<"$input"
	for word in "${words[@]}"; do
		if ! pc_contains_word "$counted" "$word"; then
			counted="$counted $word"
			count=$((count + 1))
		fi
	done
	printf '%s' "$count"
}

pla_component_label() {
	# Echo the label for one asserted lane.
	#
	# Args: $1 = the lane's position. A lane carrying a string name renders as
	# that name in single quotes, including when the name is the empty string,
	# which is still a string; a lane with no usable name renders by position
	# instead. The name is never used for identity.
	local position="$1"
	if ((PLA_EXPECTED_HAS_NAME[position] == 1)); then
		printf "'%s'" "${PLA_EXPECTED_NAMES[position]}"
	else
		printf 'component[%s]' "$position"
	fi
}

pla_render_member_list() {
	# Echo a space-separated integer list in the Python list form `[101, 102]`:
	# square brackets, comma-and-space separator, no trailing comma. An empty
	# list renders as `[]`. Args: $1 = the list.
	local input="${1-}" out="" word
	local -a words=()
	read -ra words <<<"$input"
	for word in "${words[@]}"; do
		if [[ -n $out ]]; then
			out="$out, $word"
		else
			out="$word"
		fi
	done
	printf '[%s]' "$out"
}

pla_find_split_lanes() {
	# Append one finding per asserted lane whose members landed apart.
	#
	# Reads PLA_EXPECTED_* and PLA_DERIVED_INDEX. One finding per lane, not per
	# pair: an operator whose lane was split wants one message naming the lane,
	# not a quadratic list of member pairs. Lanes are visited in manifest order,
	# and a member absent from the derived index is left to the unknown-member
	# class rather than counted here.
	local position key resolved landed distinct
	local -a members=()
	for ((position = 0; position < PLA_EXPECTED_COUNT; position++)); do
		resolved=""
		landed=""
		read -ra members <<<"${PLA_EXPECTED_MEMBERS[position]}"
		for key in "${members[@]}"; do
			[[ -n ${PLA_DERIVED_INDEX["$key"]+set} ]] || continue
			resolved="$resolved $key"
			landed="$landed ${PLA_DERIVED_INDEX["$key"]}"
		done
		distinct=$(pla_count_distinct "$landed")
		((distinct > 1)) || continue
		pla_add_finding "$PLA_EXPECTED_TOGETHER_DERIVED_APART" \
			"expected component $(pla_component_label "$position") was derived apart: its members occupy $distinct distinct conflict components" \
			"$(pla_sort_numeric "$resolved")"
	done
}

pla_find_merged_lanes() {
	# Append one finding per derived component spanning two asserted lanes.
	#
	# Reads PLA_COMPONENTS and PLA_EXPECTED_INDEX. A derived component touching
	# two asserted lanes means derivation found contention between lanes asserted
	# to be independent. Components are visited in derived order, and a key
	# covered by no asserted lane is left to the uncovered-item class.
	local index key covered lanes distinct component
	local -a members=()
	for ((index = 0; index < ${#PLA_COMPONENTS[@]}; index++)); do
		component="${PLA_COMPONENTS[index]}"
		covered=""
		lanes=""
		read -ra members <<<"$component"
		for key in "${members[@]}"; do
			[[ -n ${PLA_EXPECTED_INDEX["$key"]+set} ]] || continue
			covered="$covered $key"
			lanes="$lanes ${PLA_EXPECTED_INDEX["$key"]}"
		done
		distinct=$(pla_count_distinct "$lanes")
		((distinct > 1)) || continue
		pla_add_finding "$PLA_EXPECTED_APART_DERIVED_TOGETHER" \
			"derived conflict component $(pla_render_member_list "$component") spans $distinct expected components that were asserted apart" \
			"$(pla_sort_numeric "$covered")"
	done
}

pla_build_indexes() {
	# Populate the two flat key -> component-index lookups.
	#
	# The expected index is built in manifest order, so when one key appears in
	# two asserted lanes the LAST occurrence wins. That is reproduced from the
	# reference deliberately and no error is reported for it: duplicate membership
	# across components is invariant M8's concern, and reporting it twice for one
	# defect would misdescribe an advisory diagnostic.
	local index position key
	local -a members=()
	PLA_DERIVED_INDEX=()
	PLA_EXPECTED_INDEX=()
	for ((index = 0; index < ${#PLA_COMPONENTS[@]}; index++)); do
		read -ra members <<<"${PLA_COMPONENTS[index]}"
		for key in "${members[@]}"; do
			PLA_DERIVED_INDEX["$key"]="$index"
		done
	done
	for ((position = 0; position < PLA_EXPECTED_COUNT; position++)); do
		read -ra members <<<"${PLA_EXPECTED_MEMBERS[position]}"
		for key in "${members[@]}"; do
			PLA_EXPECTED_INDEX["$key"]="$position"
		done
	done
}

pla_compare() {
	# Compare the asserted lane grouping against the derived components.
	#
	# Args: $1 = space-separated declared item keys. Reads PLA_EXPECTED_* and
	# PLA_EDGES; populates PLA_COMPONENTS and the three PLA_FINDING_* arrays.
	# Findings are grouped by class in the fixed order split, merged, unknown
	# member, uncovered item, so a reader sees the grouping disagreements before
	# the authoring errors and the informational class last.
	local keys="$1" key unknown="" uncovered="" index position
	local -a members=() sorted=()
	pla_derive_components "$keys"
	pla_build_indexes
	PLA_FINDING_KINDS=()
	PLA_FINDING_DETAILS=()
	PLA_FINDING_MEMBERS=()

	pla_find_split_lanes
	pla_find_merged_lanes

	# An asserted member naming no manifest item is an authoring error in the
	# assertion itself, so it is reported apart from a grouping disagreement.
	# The candidate list is collected from the authored membership rather than
	# by iterating the index, so no hash order reaches output.
	for ((position = 0; position < PLA_EXPECTED_COUNT; position++)); do
		read -ra members <<<"${PLA_EXPECTED_MEMBERS[position]}"
		for key in "${members[@]}"; do
			[[ -z ${PLA_DERIVED_INDEX["$key"]+set} ]] || continue
			unknown="$unknown $key"
		done
	done
	read -ra sorted <<<"$(pla_sort_numeric "$unknown" unique)"
	for key in "${sorted[@]}"; do
		pla_add_finding "$PLA_MEMBER_NAMES_NO_ITEM" \
			"expected member $key names no manifest item" "$key"
	done

	# Informational only: an item the assertion did not mention, which is
	# legitimate when the operator asserts a subset of the run.
	for ((index = 0; index < ${#PLA_COMPONENTS[@]}; index++)); do
		read -ra members <<<"${PLA_COMPONENTS[index]}"
		for key in "${members[@]}"; do
			[[ -z ${PLA_EXPECTED_INDEX["$key"]+set} ]] || continue
			uncovered="$uncovered $key"
		done
	done
	read -ra sorted <<<"$(pla_sort_numeric "$uncovered" unique)"
	for key in "${sorted[@]}"; do
		pla_add_finding "$PLA_ITEM_COVERED_BY_NO_COMPONENT" \
			"manifest item $key is covered by no expected component" "$key"
	done
}

# The report's closing line, stating that the diagnostic blocks nothing. Held
# as a constant so the entry point and the report share one spelling.
PLA_CLOSING_LINE="Advisory only: this diagnostic never blocks, never modifies a derived edge, never feeds compute_cohorts, and never influences scheduling."

# The rendered report, newline separated and with no trailing newline. The
# single trailing newline is added by the entry point's printf.
PLA_REPORT=""

pla_disagreement_count() {
	# Echo the number of findings outside the informational class.
	local index count=0
	for ((index = 0; index < ${#PLA_FINDING_KINDS[@]}; index++)); do
		if ! pc_contains_word "$PLA_INFORMATIONAL_KINDS" "${PLA_FINDING_KINDS[index]}"; then
			count=$((count + 1))
		fi
	done
	printf '%s' "$count"
}

pla_format_report() {
	# Render the comparison outcome into PLA_REPORT as advisory-only text.
	#
	# A header line naming the derived-component and disagreement counts, one
	# ADVISORY-prefixed line per finding in emission order, and a closing line
	# stating that the diagnostic blocks nothing. The disagreement count covers
	# the first three classes only, because the informational class reports that
	# the assertion was silent rather than that it disagreed.
	local index disagreements
	disagreements=$(pla_disagreement_count)
	PLA_REPORT="Lane assertion: ${#PLA_COMPONENTS[@]} derived conflict component(s); $disagreements disagreement(s)."
	for ((index = 0; index < ${#PLA_FINDING_KINDS[@]}; index++)); do
		PLA_REPORT="$PLA_REPORT"$'\n'"ADVISORY [${PLA_FINDING_KINDS[index]}] ${PLA_FINDING_DETAILS[index]}."
	done
	PLA_REPORT="$PLA_REPORT"$'\n'"$PLA_CLOSING_LINE"
}
