#!/usr/bin/env bash
# parallel-manifest-validate.sh: sourceable bash port of
# scripts/dev_tools/parallel_manifest_contract.py. Validates a parallel-run
# manifest document against invariants M1 through M7 and exposes the two
# default-resolving accessors that every consumer uses instead of reading
# `mode` and `max_concurrency` directly.
#
# Section order is load-bearing and mirrors the Python module exactly: the M1
# frontmatter checks short-circuit with a single error; then run identity
# (M2 parallel, M3 mode, M4 max_concurrency, M5 created_at) in schema field
# order; then the M7 prohibited-key scan, deep results in document order
# followed by the top-level results; then the M6 items collection.
#
# Every error string begins with the literal prefix `Parallel manifest` and
# ends with a period. The Python module remains the repository authority.
#
# Out-of-subset input. When the YAML scanner refuses a construct it does not
# model, this module returns exit status 2 with the refusal detail in
# PM_SUBSET_DETAIL and emits no validation errors. Refusing to answer is
# deliberate: a guessed parse could disagree with the Python authority
# silently, whereas an explicit refusal is visible to the caller.
#
# shellcheck disable=SC2034
# SC2034 is disabled file-wide because PM_SUBSET_DETAIL is written here and
# read by the entry point validate-parallel-manifest.sh, which shellcheck
# analyses as a separate file and therefore cannot see the use.

# Resolve this file's own directory so its dependencies source regardless of
# the caller's working directory.
PM_LIB_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
# shellcheck source=.claude/lib/bash/parallel-items-validate.sh
# shellcheck disable=SC1091
source "$PM_LIB_DIR/parallel-items-validate.sh"

# Literal context prefix for every error this module and its helpers emit.
PM_CONTEXT="Parallel manifest"

# Documented default run mode when the manifest omits `mode` (invariant M3).
PM_DEFAULT_MODE="closed"

# Documented default fan-out when the manifest omits `max_concurrency` (M4).
PM_DEFAULT_MAX_CONCURRENCY=4

# Inclusive bounds on a present `max_concurrency` (invariant M4, A7).
PM_MIN_CONCURRENCY=1
PM_MAX_CONCURRENCY=8

# Keys the manifest rejects at any nesting level (invariant M7).
PM_DEEP_PROHIBITED_KEYS="depends_on"

# Keys the manifest rejects at the document root only (invariant M7).
PM_TOP_LEVEL_PROHIBITED_KEYS="integration_branch"

# Detail recorded when the parser refuses an out-of-subset construct.
PM_SUBSET_DETAIL=""

pm_parse_manifest() {
	# Parse manifest text into the shared node table (invariant M1).
	#
	# Args: $1 = raw manifest document text.
	# Returns 0 when the frontmatter parsed into a mapping. Returns 1 after
	# appending the single M1 error to PC_ERRORS for a missing, unterminated,
	# unparseable, or non-mapping block. Returns 2 without appending anything
	# when the document uses a construct outside the supported subset.
	local text="$1"
	if ! yp_extract_frontmatter_body "$text" "$PM_CONTEXT"; then
		pc_error_add "$YP_FENCE_ERROR"
		return 1
	fi
	if yp_parse_body; then
		return 0
	fi
	# Routing table for the three parser failure modes: a sequence or empty
	# root is M1's non-mapping case, a malformed line is M1's YAML-error case,
	# and an unsupported construct makes the module refuse to answer.
	case "$YP_STATUS" in
	not_a_mapping)
		pc_error_add "$PM_CONTEXT frontmatter must be a mapping."
		return 1
		;;
	yaml_error)
		pc_error_add "$PM_CONTEXT frontmatter is not valid YAML: $YP_DETAIL."
		return 1
		;;
	*)
		PM_SUBSET_DETAIL="$YP_DETAIL"
		return 2
		;;
	esac
}

pm_validate_identity() {
	# Validate run identity fields against invariants M2 through M5.
	#
	# The mode and max_concurrency checks are presence gated because both keys
	# are optional: absence is the documented authoring shape that the
	# accessors resolve, so it must contribute no error.
	local slug_type slug_value
	slug_type=$(yp_type_of "parallel")
	slug_value=$(yp_value_of "parallel")
	if ! pc_is_non_empty_string "$slug_type" "$slug_value"; then
		pc_error_add "$PM_CONTEXT parallel must be a non-empty string."
	fi

	if yp_has "mode"; then
		local mode_type mode_value
		mode_type=$(yp_type_of "mode")
		mode_value=$(yp_value_of "mode")
		if [[ $mode_type != str ]] || ! pc_enum_members_contains "$PC_VALID_MODES" "$mode_value"; then
			pc_error_add "$(pc_enum_error "$PM_CONTEXT" "mode" "$PC_VALID_MODES" "$(pi_repr_at "mode")")"
		fi
	fi

	if yp_has "max_concurrency"; then
		local cap_type cap_value
		cap_type=$(yp_type_of "max_concurrency")
		cap_value=$(yp_value_of "max_concurrency")
		if ! pc_in_bounded_range "$cap_type" "$cap_value" "$PM_MIN_CONCURRENCY" "$PM_MAX_CONCURRENCY"; then
			pc_error_add "$PM_CONTEXT max_concurrency must be an integer from $PM_MIN_CONCURRENCY through $PM_MAX_CONCURRENCY; found: $(pi_repr_at "max_concurrency")."
		fi
	fi

	local created_type created_value
	created_type=$(yp_type_of "created_at")
	created_value=$(yp_value_of "created_at")
	if ! pc_is_non_empty_string "$created_type" "$created_value"; then
		pc_error_add "$PM_CONTEXT created_at must be a non-empty string."
	fi
}

pm_validate_text() {
	# Validate a manifest document against invariants M1 to M7.
	#
	# Args: $1 = raw manifest document text, authored with LF, CRLF, or CR
	# line endings.
	# Returns 0 after populating PC_ERRORS with the complete error list, which
	# is empty for a valid manifest. Returns 2 without populating PC_ERRORS
	# when the document uses a construct outside the supported subset.
	local text="$1" parse_status=0
	pc_errors_reset
	PM_SUBSET_DETAIL=""
	pm_parse_manifest "$text" || parse_status=$?
	# An out-of-subset refusal propagates so the caller can distinguish it from
	# a validation verdict; an M1 failure returns the single-element error list
	# already accumulated, because no field check is meaningful without a
	# mapping to read fields from.
	if ((parse_status == 2)); then
		return 2
	fi
	if ((parse_status != 0)); then
		return 0
	fi

	pm_validate_identity
	pi_scan_prohibited_keys "$PM_CONTEXT" "$PM_DEEP_PROHIBITED_KEYS" "$PM_TOP_LEVEL_PROHIBITED_KEYS"
	# The manifest carries `kind` on every item, unlike the orchestrator
	# checkpoint, so the shared item validator is asked to require it (S1).
	pi_validate_items "items" "$PM_CONTEXT" 1
	return 0
}

pm_manifest_mode() {
	# Echo the run mode declared by the parsed manifest (invariant M3).
	#
	# A present but non-string value resolves to the default so this accessor
	# always returns a mode; the malformed value is reported separately by
	# pm_validate_text.
	if [[ $(yp_type_of "mode") == str ]]; then
		yp_value_of "mode"
	else
		printf '%s' "$PM_DEFAULT_MODE"
	fi
}

pm_manifest_max_concurrency() {
	# Echo the fan-out cap declared by the parsed manifest (invariant M4).
	#
	# A present but non-integer value resolves to the default so this accessor
	# always returns an integer; the malformed value is reported separately by
	# pm_validate_text. Booleans are classified as `bool` by the scanner and
	# therefore never satisfy the integer test.
	if [[ $(yp_type_of "max_concurrency") == int ]]; then
		yp_value_of "max_concurrency"
	else
		printf '%s' "$PM_DEFAULT_MAX_CONCURRENCY"
	fi
}
