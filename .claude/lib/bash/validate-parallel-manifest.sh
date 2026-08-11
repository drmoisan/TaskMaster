#!/usr/bin/env bash
# validate-parallel-manifest.sh: destination-portable command-line entry point
# for parallel-run manifest validation and the two default-resolving accessors.
# It exists so a workspace that received the Claude customization payload can
# validate a manifest with nothing but bash -- no Python, no Poetry, no yq, and
# no repository checkout.
#
# Usage:
#   bash .claude/lib/bash/validate-parallel-manifest.sh <manifest-path>
#   bash .claude/lib/bash/validate-parallel-manifest.sh --print-mode <manifest-path>
#   bash .claude/lib/bash/validate-parallel-manifest.sh --print-max-concurrency <manifest-path>
#
# Output contract:
#   stdout  validation errors one per line (empty for a valid manifest), or the
#           resolved accessor value under --print-mode / --print-max-concurrency
#   exit 0  the manifest is valid, or the accessor resolved
#   exit 1  the manifest is invalid
#   exit 2  usage error, unreadable manifest, or a YAML construct outside the
#           supported subset
#
# The accessors resolve the documented defaults -- `closed` and `4` -- when the
# manifest omits the key or carries a malformed value, matching
# manifest_mode and manifest_max_concurrency in the Python authority.
set -euo pipefail

# Resolve this script's own directory so the library sources regardless of cwd.
VM_SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
# shellcheck source=.claude/lib/bash/parallel-manifest-validate.sh
# shellcheck disable=SC1091
source "$VM_SCRIPT_DIR/parallel-manifest-validate.sh"

pc_enforce_c_locale

vm_usage() {
	# Print the entry point's usage text.
	cat <<'EOF'
Usage: validate-parallel-manifest.sh [--print-mode | --print-max-concurrency] <manifest-path>

Validates a parallel-run manifest against invariants M1 through M7 and prints
one error per line on stdout; a valid manifest prints nothing and exits 0.

Options:
  --print-mode              Print the resolved run mode (default: closed).
  --print-max-concurrency   Print the resolved fan-out cap (default: 4).
EOF
}

vm_require_readable() {
	# Return 0 when the manifest path names a readable file.
	#
	# Args: $1 = manifest path. A missing manifest is an operator error rather
	# than a validation verdict, so the caller exits 2 instead of reporting it
	# as a manifest defect.
	local path="$1"
	if [[ ! -f $path || ! -r $path ]]; then
		printf 'validate-parallel-manifest.sh: manifest not found or not readable: %s\n' "$path" >&2
		return 1
	fi
	return 0
}

vm_report_subset_refusal() {
	# Print the out-of-subset refusal and exit 2.
	#
	# Refusing to answer is deliberate: a guessed parse could disagree with the
	# Python authority silently, whereas an explicit refusal is visible.
	printf 'validate-parallel-manifest.sh: manifest uses a YAML construct outside the supported subset: %s\n' \
		"$PM_SUBSET_DETAIL" >&2
	exit 2
}

vm_main() {
	# Dispatch on the optional accessor flag and run the requested operation.
	local mode="validate" path="" status=0 text
	# Routing table: the two accessor flags each consume the following path
	# argument; anything else is treated as the manifest path itself.
	case "${1-}" in
	--print-mode)
		mode="print-mode"
		path="${2-}"
		;;
	--print-max-concurrency)
		mode="print-max-concurrency"
		path="${2-}"
		;;
	--help | -h)
		vm_usage
		return 0
		;;
	-*)
		vm_usage >&2
		return 2
		;;
	*)
		path="${1-}"
		;;
	esac
	[[ -n $path ]] || {
		vm_usage >&2
		return 2
	}

	vm_require_readable "$path" || return 2
	text=$(cat -- "$path")
	pm_validate_text "$text" || status=$?
	if ((status == 2)); then
		vm_report_subset_refusal
	fi

	# The accessors read the parsed node table, which pm_validate_text has
	# already populated, so a malformed-but-parseable manifest still resolves.
	if [[ $mode == "print-mode" ]]; then
		pm_manifest_mode
		printf '\n'
		return 0
	fi
	if [[ $mode == "print-max-concurrency" ]]; then
		pm_manifest_max_concurrency
		printf '\n'
		return 0
	fi

	pc_errors_print
	(($(pc_errors_count) == 0)) || return 1
	return 0
}

# Guard so the file can be sourced without executing main. main's return code
# is captured and re-exited explicitly as the final statement.
if [[ ${BASH_SOURCE[0]} == "${0}" ]]; then
	vm_rc=0
	vm_main "$@" || vm_rc=$?
	exit "$vm_rc"
fi
