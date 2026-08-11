#!/usr/bin/env bash
# parallel-common.sh: sourceable bash port of the shared predicates, enum
# vocabularies, and error-string builders that back the parallel-surface
# validators. This is the bash counterpart of
# scripts/dev_tools/_parallel_state_common.py; the Python module remains the
# repository authority and this file must reproduce its output byte for byte.
#
# Scope. Pure string and arithmetic helpers only: nothing here reads a file,
# starts a process, reads the clock, or mutates global state other than the
# PC_ERRORS accumulator its own reset/add functions own. The enum tuples are
# consumed from .claude/rules/parallel-orchestration.md and are never extended
# here; member order is load-bearing because pc_enum_error renders it.
#
# Locale. Every entry point that sources this file calls pc_enforce_c_locale
# before doing any work, so sorting and character classification are byte
# ordered and independent of the destination workspace's environment.
#
# shellcheck disable=SC2034
# SC2034 is disabled file-wide because every constant below is consumed by a
# sibling file of this library -- parallel-items-validate.sh,
# parallel-manifest-validate.sh, and parallel-cohorts.sh -- which shellcheck
# analyses separately and therefore cannot see the use.

# Item lifecycle states, in canonical order (schema S4).
PC_VALID_ITEM_STATES="proposed, admitted, prepared, scheduled, in_flight, merged, withdrawn, blocked"

# Per-item merge lifecycle, in canonical order (schema S4).
PC_VALID_MERGE_STATUS="not_started, worktree_created, pr_open, ci_green, merged, worktree_removed, blocked_drift, blocked_ci_loop_limit"

# Blast-radius confidence sources, in canonical order (schema S4).
PC_VALID_SOURCES="derived, declared, observed"

# Work-item kinds carried by the manifest (schema S4).
PC_VALID_KINDS="feature, bug"

# Run modes; closed is the documented default (schema S4).
PC_VALID_MODES="closed, open"

# Merge-status values meaning the item reached a terminal merged outcome.
PC_MERGED_MERGE_STATUSES="merged worktree_removed"

# Merge-status values meaning the item is blocked.
PC_BLOCKED_MERGE_STATUSES="blocked_drift blocked_ci_loop_limit"

# The four blast_radius collection fields, in serialization order.
PC_BLAST_RADIUS_LIST_FIELDS="paths modules shared_surfaces contracts"

# Path label for the document root in prohibited-key error strings.
PC_ROOT_PATH="<root>"

# Accumulated validation errors, in emission order.
PC_ERRORS=()

pc_enforce_c_locale() {
	# Pin the byte-ordered C locale for the remainder of the process.
	#
	# Determinism countermeasure R9: every numeric sort, glob expansion, and
	# character class in this library must behave identically regardless of the
	# destination workspace's locale settings.
	export LC_ALL=C
}

pc_errors_reset() {
	# Discard any accumulated errors so a fresh validation starts empty.
	PC_ERRORS=()
}

pc_error_add() {
	# Append one error string to the accumulator.
	#
	# Args: $1 = the complete error string, already fully rendered.
	PC_ERRORS+=("$1")
}

pc_errors_count() {
	# Echo the number of accumulated errors.
	printf '%s' "${#PC_ERRORS[@]}"
}

pc_errors_print() {
	# Print every accumulated error, one per line, in emission order.
	#
	# Prints nothing for an empty accumulator, which is the valid-artifact case.
	((${#PC_ERRORS[@]} > 0)) || return 0
	local entry
	# Emit in accumulation order: message sequence is part of the parity
	# contract, so the accumulator is never sorted or de-duplicated.
	for entry in "${PC_ERRORS[@]}"; do
		printf '%s\n' "$entry"
	done
}

pc_repr_string() {
	# Echo the Python repr of a string, including Python's quote selection.
	#
	# Python prefers single quotes and switches to double quotes only when the
	# string contains a single quote and no double quote. Backslashes are
	# escaped first so a later escape's own backslash is not doubled, then the
	# three printable control escapes, then the active quote character.
	#
	# Args: $1 = the raw string value.
	local value="$1"
	local quote="'"
	# Quote selection: only a string carrying an apostrophe but no double quote
	# switches Python to double quotes.
	if [[ $value == *"'"* && $value != *'"'* ]]; then
		quote='"'
	fi
	local body=${value//\\/\\\\}
	body=${body//$'\n'/\\n}
	body=${body//$'\r'/\\r}
	body=${body//$'\t'/\\t}
	# Escape whichever quote is active. Under double quoting the escape is a
	# no-op by the selection rule above; under single quoting it fires only for
	# a string that carries both quote characters.
	if [[ $quote == "'" ]]; then
		body=${body//\'/\\\'}
	else
		body=${body//\"/\\\"}
	fi
	printf '%s%s%s' "$quote" "$body" "$quote"
}

pc_repr() {
	# Echo the Python repr of a lexically typed value.
	#
	# Args: $1 = type tag (absent, null, bool, int, float, str), $2 = raw value.
	# An absent key reads as None, matching Python's mapping.get default.
	local type="$1"
	local value="${2-}"
	# Routing table: the five scalar types the manifest subset can carry, plus
	# the absent-key case that Python renders as None.
	case "$type" in
	absent | null)
		printf 'None'
		;;
	bool)
		if [[ $value == true ]]; then
			printf 'True'
		else
			printf 'False'
		fi
		;;
	int | float)
		printf '%s' "$value"
		;;
	str)
		pc_repr_string "$value"
		;;
	*)
		# A container reaching repr would be a caller defect; render the tag so
		# the failure is visible rather than silently producing a bare value.
		printf '<%s>' "$type"
		;;
	esac
}

pc_is_non_empty_string() {
	# Return 0 when a typed value is a string carrying a non-space character.
	#
	# Args: $1 = type tag, $2 = raw value. Mirrors is_non_empty_string.
	[[ $1 == str ]] || return 1
	local stripped="${2-}"
	# A left strip is sufficient: a value that is entirely whitespace becomes
	# empty, which is exactly what Python's two-sided strip would report.
	stripped=${stripped#"${stripped%%[![:space:]]*}"}
	[[ -n $stripped ]]
}

pc_is_integer() {
	# Return 0 when a typed value is a genuine integer rather than a boolean.
	#
	# Args: $1 = type tag. The parser classifies booleans as `bool`, never as
	# `int`, so the boolean exclusion that Python needs is structural here.
	[[ $1 == int ]]
}

pc_is_positive_integer() {
	# Return 0 when a typed value is an integer greater than zero.
	#
	# Args: $1 = type tag, $2 = raw value.
	[[ $1 == int ]] || return 1
	local number="$2"
	((number > 0))
}

pc_is_non_negative_integer() {
	# Return 0 when a typed value is an integer of zero or more.
	#
	# Args: $1 = type tag, $2 = raw value.
	[[ $1 == int ]] || return 1
	local number="$2"
	((number >= 0))
}

pc_in_bounded_range() {
	# Return 0 when a typed value is an integer inside an inclusive range.
	#
	# Args: $1 = type tag, $2 = raw value, $3 = minimum, $4 = maximum.
	[[ $1 == int ]] || return 1
	local number="$2" minimum="$3" maximum="$4"
	((number >= minimum && number <= maximum))
}

pc_enum_error() {
	# Echo the standard out-of-enum error string for a field.
	#
	# Args: $1 = context prefix, $2 = dotted field name, $3 = comma-and-space
	# joined member list in canonical order, $4 = the already-rendered repr of
	# the offending value. Mirrors enum_error so the wording cannot drift.
	printf '%s %s must be one of %s; found: %s.' "$1" "$2" "$3" "$4"
}

pc_item_context() {
	# Echo the context prefix for one items[] entry.
	#
	# Args: $1 = surface prefix, $2 = zero-based entry index. The positional
	# index is used rather than issue_num because it exists for every entry,
	# including one whose issue_num is missing.
	printf '%s items[%s]' "$1" "$2"
}

pc_contains_word() {
	# Return 0 when a space-separated list contains an exact word.
	#
	# Args: $1 = space-separated haystack, $2 = needle. Used for the enum
	# membership tests whose member lists are stored space separated.
	local haystack=" $1 "
	[[ $haystack == *" $2 "* ]]
}

pc_enum_members_contains() {
	# Return 0 when a comma-and-space joined enum list contains an exact member.
	#
	# Args: $1 = the rendered member list, $2 = candidate member.
	local haystack="${1//, / }"
	pc_contains_word "$haystack" "$2"
}
