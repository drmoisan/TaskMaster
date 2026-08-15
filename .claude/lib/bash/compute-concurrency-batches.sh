#!/usr/bin/env bash
# compute-concurrency-batches.sh: destination-portable command-line entry point
# for the parallel surface's concurrency batching. It exists so a workspace that
# received the Claude customization payload can cap fan-out with nothing but
# bash -- no Python, no Poetry, no repository checkout.
#
# Usage:
#   bash .claude/lib/bash/compute-concurrency-batches.sh \
#       --keys "<k1> <k2> ..." --max-concurrency <n>
#
# Item keys are decimal integers matching `-?(0|[1-9][0-9]*)`; a token with a
# leading zero is rejected fail-closed with a lexical error, because the Python
# authority would read such a token differently and a silent disagreement is
# worse than a refusal.
#
# Output contract:
#   stdout  compact JSON array of arrays, identical to Python
#           json.dumps(..., separators=(",", ":"))
#   stderr  on invalid input, the exact message the Python reference
#           implementation raises
#   exit 0  success
#   exit 1  invalid input (max_concurrency below 1)
#   exit 2  usage error or a token outside the accepted integer lexis
set -euo pipefail

# Resolve this script's own directory so the library sources regardless of cwd.
CB_SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
# shellcheck source=.claude/lib/bash/parallel-cohorts.sh
# shellcheck disable=SC1091
source "$CB_SCRIPT_DIR/parallel-cohorts.sh"

pc_enforce_c_locale

cb_usage() {
	# Print the entry point's usage text.
	cat <<'EOF'
Usage: compute-concurrency-batches.sh --keys "<k1> <k2> ..." --max-concurrency <n>

Chunks one cohort's item keys into concurrency-capped batches in ascending key
order. Every batch holds exactly <n> keys except a possibly smaller final batch.

Options:
  --keys              Space-separated cohort item keys (required; may be empty).
  --max-concurrency   The fan-out cap (required).

Prints a compact JSON array of arrays on stdout. When the cap is below 1, prints
the reference implementation's exact message on stderr and exits 1.
EOF
}

cb_require_integer() {
	# Validate one token against the accepted decimal-integer lexis.
	#
	# Args: $1 = the token, $2 = a label naming where the token came from.
	# Exits 2 with a lexical error when the token is outside the lexis.
	local token="$1" label="$2"
	if [[ ! $token =~ ^-?(0|[1-9][0-9]*)$ ]]; then
		printf 'compute-concurrency-batches.sh: %s must be a decimal integer matching -?(0|[1-9][0-9]*); found: %s\n' \
			"$label" "$token" >&2
		exit 2
	fi
}

cb_main() {
	# Parse arguments, compute the batches, and print the result.
	local keys="" cap="" keys_seen=0 token
	while (($# > 0)); do
		case "$1" in
		--keys)
			(($# >= 2)) || {
				cb_usage >&2
				return 2
			}
			keys="$2"
			keys_seen=1
			shift 2
			;;
		--max-concurrency)
			(($# >= 2)) || {
				cb_usage >&2
				return 2
			}
			cap="$2"
			shift 2
			;;
		--help | -h)
			cb_usage
			return 0
			;;
		*)
			cb_usage >&2
			return 2
			;;
		esac
	done
	if ((keys_seen == 0)) || [[ -z $cap ]]; then
		cb_usage >&2
		return 2
	fi

	cb_require_integer "$cap" "max_concurrency"
	pcoh_split_words "$keys"
	local -a key_tokens=("${PCOH_WORDS[@]}")
	for token in "${key_tokens[@]}"; do
		cb_require_integer "$token" "item key"
	done

	if ! pcoh_compute_concurrency_batches "$keys" "$cap"; then
		printf '%s\n' "$PCOH_ERROR" >&2
		return 1
	fi
	printf '%s\n' "$PCOH_RESULT"
	return 0
}

# Guard so the file can be sourced without executing main. main's return code
# is captured and re-exited explicitly as the final statement.
if [[ ${BASH_SOURCE[0]} == "${0}" ]]; then
	cb_rc=0
	cb_main "$@" || cb_rc=$?
	exit "$cb_rc"
fi
