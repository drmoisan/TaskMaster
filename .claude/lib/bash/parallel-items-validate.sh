#!/usr/bin/env bash
# parallel-items-validate.sh: sourceable bash port of the shared work-item
# validators in scripts/dev_tools/_parallel_state_common.py -- validate_items,
# validate_item_record, _validate_merge_status, and validate_blast_radius_block.
# It reads the node table populated by parallel-yaml-emit.sh and appends errors
# to the PC_ERRORS accumulator owned by parallel-common.sh.
#
# Message order is part of the contract, not an implementation detail. Within
# one item the order is issue_num, feature_folder, state, kind (when required),
# merge_status, then the blast-radius block in field order. Across items the
# order is positional, and every duplicate issue_num is reported after the
# whole per-entry pass, in ascending key order, so the sequence is reproducible
# regardless of the order the entries appeared in.
#
# The Python module remains the repository authority; this file must reproduce
# its output byte for byte for every fixture in tests/fixtures/parallel_manifest_bash.

# Resolve this file's own directory so its dependencies source regardless of
# the caller's working directory.
PI_LIB_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
# shellcheck source=.claude/lib/bash/parallel-common.sh
# shellcheck disable=SC1091
source "$PI_LIB_DIR/parallel-common.sh"
# shellcheck source=.claude/lib/bash/parallel-yaml-emit.sh
# shellcheck disable=SC1091
source "$PI_LIB_DIR/parallel-yaml-emit.sh"

pi_repr_at() {
	# Echo the Python repr of the value stored at a node path.
	#
	# Args: $1 = node path. An unknown path renders as None, matching the
	# Python code's mapping.get default.
	local path="$1" type value
	type=$(yp_type_of "$path")
	value=$(yp_value_of "$path")
	pc_repr "$type" "$value"
}

pi_is_string_list() {
	# Return 0 when a node is a sequence whose every entry is a non-empty string.
	#
	# An empty sequence satisfies the predicate: every blast-radius collection
	# other than paths is legitimately empty. Mirrors is_string_list.
	#
	# Args: $1 = node path.
	local path="$1"
	[[ $(yp_type_of "$path") == seq ]] || return 1
	local count index entry_type entry_value
	count=$(yp_count_of "$path")
	# A single blank entry fails the whole list, because it would silently
	# widen a radius and under-report contention.
	for ((index = 0; index < count; index++)); do
		entry_type=$(yp_type_of "${path}[${index}]")
		entry_value=$(yp_value_of "${path}[${index}]")
		pc_is_non_empty_string "$entry_type" "$entry_value" || return 1
	done
	return 0
}

pi_validate_blast_radius_block() {
	# Validate one blast_radius block against spec invariant 9.
	#
	# Args: $1 = node path of the block, $2 = item-scoped context prefix.
	local path="$1" context="$2"
	if [[ $(yp_type_of "$path") != map ]]; then
		pc_error_add "$context blast_radius must be an object."
		return 0
	fi
	local field
	local -a radius_fields=()
	read -ra radius_fields <<<"$PC_BLAST_RADIUS_LIST_FIELDS"
	# Report every malformed collection field rather than stopping at the
	# first, so one validation pass tells the author everything to fix.
	for field in "${radius_fields[@]}"; do
		if ! pi_is_string_list "${path}.${field}"; then
			pc_error_add "$context blast_radius.$field must be a list of non-empty strings."
		fi
	done

	local source_type source_value
	source_type=$(yp_type_of "${path}.source")
	source_value=$(yp_value_of "${path}.source")
	if [[ $source_type != str ]] || ! pc_enum_members_contains "$PC_VALID_SOURCES" "$source_value"; then
		pc_error_add "$(pc_enum_error "$context" "blast_radius.source" "$PC_VALID_SOURCES" "$(pi_repr_at "${path}.source")")"
	fi

	local computed_type computed_value
	computed_type=$(yp_type_of "${path}.computed_at")
	computed_value=$(yp_value_of "${path}.computed_at")
	if ! pc_is_non_empty_string "$computed_type" "$computed_value"; then
		pc_error_add "$context blast_radius.computed_at must be a non-empty string."
	fi
}

pi_validate_merge_status() {
	# Validate merge_status membership and its agreement with item state.
	#
	# Absence is the backward-compatible case: an item with no merge_status is
	# treated as not_started and yields no error, so the check is presence
	# gated rather than requirement gated.
	#
	# Args: $1 = item node path, $2 = item-scoped context, $3 = state repr,
	# $4 = raw state value.
	local path="$1" context="$2" state_repr="$3" state_value="$4"
	yp_has "${path}.merge_status" || return 0
	local status_type status_value
	status_type=$(yp_type_of "${path}.merge_status")
	status_value=$(yp_value_of "${path}.merge_status")
	# An out-of-enum value short-circuits the consistency rule, because
	# agreement with state is meaningless for a value that is not a status.
	if [[ $status_type != str ]] || ! pc_enum_members_contains "$PC_VALID_MERGE_STATUS" "$status_value"; then
		pc_error_add "$(pc_enum_error "$context" "merge_status" "$PC_VALID_MERGE_STATUS" "$(pi_repr_at "${path}.merge_status")")"
		return 0
	fi

	# Invariant 8 pins the two terminal families to their item states; every
	# other status places no constraint on state.
	if pc_contains_word "$PC_MERGED_MERGE_STATUSES" "$status_value" && [[ $state_value != merged ]]; then
		pc_error_add "$context merge_status $(pc_repr_string "$status_value") requires state 'merged'; found: $state_repr."
		return 0
	fi
	if pc_contains_word "$PC_BLOCKED_MERGE_STATUSES" "$status_value" && [[ $state_value != blocked ]]; then
		pc_error_add "$context merge_status $(pc_repr_string "$status_value") requires state 'blocked'; found: $state_repr."
	fi
}

pi_validate_item_record() {
	# Validate one work-item record against spec invariants 5 through 9.
	#
	# Args: $1 = item node path, $2 = item-scoped context prefix, $3 = 1 when
	# kind is required (the manifest and planner surfaces carry it).
	local path="$1" context="$2" require_kind="$3"
	if [[ $(yp_type_of "$path") != map ]]; then
		pc_error_add "$context must be an object."
		return 0
	fi

	local issue_type issue_value
	issue_type=$(yp_type_of "${path}.issue_num")
	issue_value=$(yp_value_of "${path}.issue_num")
	if ! pc_is_positive_integer "$issue_type" "$issue_value"; then
		pc_error_add "$context issue_num must be a positive integer; found: $(pi_repr_at "${path}.issue_num")."
	fi

	local folder_type folder_value
	folder_type=$(yp_type_of "${path}.feature_folder")
	folder_value=$(yp_value_of "${path}.feature_folder")
	if ! pc_is_non_empty_string "$folder_type" "$folder_value"; then
		pc_error_add "$context feature_folder must be a non-empty string."
	fi

	local state_type state_value state_repr
	state_type=$(yp_type_of "${path}.state")
	state_value=$(yp_value_of "${path}.state")
	state_repr=$(pi_repr_at "${path}.state")
	if [[ $state_type != str ]] || ! pc_enum_members_contains "$PC_VALID_ITEM_STATES" "$state_value"; then
		pc_error_add "$(pc_enum_error "$context" "state" "$PC_VALID_ITEM_STATES" "$state_repr")"
	fi

	if ((require_kind == 1)); then
		local kind_type kind_value
		kind_type=$(yp_type_of "${path}.kind")
		kind_value=$(yp_value_of "${path}.kind")
		if [[ $kind_type != str ]] || ! pc_enum_members_contains "$PC_VALID_KINDS" "$kind_value"; then
			pc_error_add "$(pc_enum_error "$context" "kind" "$PC_VALID_KINDS" "$(pi_repr_at "${path}.kind")")"
		fi
	fi

	pi_validate_merge_status "$path" "$context" "$state_repr" "$state_value"
	pi_validate_blast_radius_block "${path}.blast_radius" "$context"
}

pi_validate_items() {
	# Validate the items collection, including issue_num uniqueness.
	#
	# Args: $1 = node path of the items collection, $2 = surface prefix,
	# $3 = 1 when kind is required on every entry.
	local path="$1" context="$2" require_kind="$3"
	if [[ $(yp_type_of "$path") != seq ]]; then
		pc_error_add "$context items must be a list."
		return 0
	fi

	local count index entry_path item_ctx issue_type issue_value
	local seen=""
	local -a duplicates=()
	count=$(yp_count_of "$path")
	# Validate each entry in place, and in the same pass accumulate the primary
	# keys so uniqueness is decided without a second traversal.
	for ((index = 0; index < count; index++)); do
		entry_path="${path}[${index}]"
		item_ctx=$(pc_item_context "$context" "$index")
		pi_validate_item_record "$entry_path" "$item_ctx" "$require_kind"
		[[ $(yp_type_of "$entry_path") == map ]] || continue
		issue_type=$(yp_type_of "${entry_path}.issue_num")
		issue_value=$(yp_value_of "${entry_path}.issue_num")
		[[ $issue_type == int ]] || continue
		if pc_contains_word "$seen" "$issue_value"; then
			duplicates+=("$issue_value")
		fi
		seen="$seen $issue_value"
	done

	((${#duplicates[@]} > 0)) || return 0
	# Report duplicates in ascending numeric order, de-duplicated, so a key
	# repeated three times still yields exactly one message.
	local ordered duplicate
	ordered=$(printf '%s\n' "${duplicates[@]}" | sort -n -u)
	while IFS= read -r duplicate; do
		[[ -n $duplicate ]] || continue
		pc_error_add "$context has duplicate items[].issue_num: $duplicate."
	done <<<"$ordered"
}

pi_scan_prohibited_keys() {
	# Reject prohibited keys per manifest invariant M7.
	#
	# Args: $1 = surface prefix, $2 = space-separated keys rejected at any
	# nesting level, $3 = space-separated keys rejected at the document root
	# only. Deep results come first, in document order, then the top-level
	# results in the order the third argument lists them.
	local context="$1" deep_keys="$2" top_keys="$3"
	local index path key total=${#YP_ORDER[@]}
	# Walk the node table in document order so the message sequence reproduces
	# Python's depth-first, insertion-ordered traversal exactly.
	for ((index = 0; index < total; index++)); do
		path="${YP_ORDER[index]}"
		key="${YP_KEY[$path]-}"
		[[ -n $key ]] || continue
		if pc_contains_word "$deep_keys" "$key"; then
			pc_error_add "$context carries prohibited key '$key' at ${YP_PARENT[$path]}."
		fi
	done

	# The shallow pass exists because manifest M7 bans integration_branch at
	# the top level only, where a nested occurrence is legitimate child data.
	local -a top_key_list=()
	read -ra top_key_list <<<"$top_keys"
	for key in "${top_key_list[@]}"; do
		if yp_has "$key"; then
			pc_error_add "$context carries prohibited key '$key' at $PC_ROOT_PATH."
		fi
	done
}
