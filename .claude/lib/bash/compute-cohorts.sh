#!/usr/bin/env bash
# compute-cohorts.sh: destination-portable command-line entry point for the
# parallel surface's cohort computation. It exists so a workspace that received
# the Claude customization payload can compute cohorts with nothing but bash --
# no Python, no Poetry, no repository checkout.
#
# Usage:
#   bash .claude/lib/bash/compute-cohorts.sh --keys "<k1> <k2> ..." \
#       [--edges "<a>:<b> <a>:<b> ..."]
#
# `--edges` is optional; omitting it, or passing an empty string, means the
# conflict graph has no edges. Item keys and edge endpoints are decimal
# integers matching `-?(0|[1-9][0-9]*)`; a token with a leading zero is
# rejected fail-closed with a lexical error, because the Python authority would
# read such a token differently and a silent disagreement is worse than a
# refusal.
#
# Output contract:
#   stdout  compact JSON array of arrays, identical to Python
#           json.dumps(..., separators=(",", ":"))
#   stderr  on invalid input, the exact message the Python reference
#           implementation raises
#   exit 0  success
#   exit 1  invalid input (duplicate key, self-loop, unknown endpoint)
#   exit 2  usage error or a token outside the accepted integer lexis
set -euo pipefail

# Resolve this script's own directory so the library sources regardless of cwd.
CC_SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
# shellcheck source=.claude/lib/bash/parallel-cohorts.sh
# shellcheck disable=SC1091
source "$CC_SCRIPT_DIR/parallel-cohorts.sh"

pc_enforce_c_locale

cc_usage() {
	# Print the entry point's usage text.
	cat <<'EOF'
Usage: compute-cohorts.sh --keys "<k1> <k2> ..." [--edges "<a>:<b> ..."]

Computes parallel execution cohorts from an undirected conflict graph by
deterministic greedy graph coloring in Welsh-Powell order.

Options:
  --keys    Space-separated item keys (required; may be an empty string).
  --edges   Space-separated conflict edges as <a>:<b> (optional).

Prints a compact JSON array of arrays on stdout. On invalid input, prints the
reference implementation's exact message on stderr and exits 1.
EOF
}

cc_require_integer() {
	# Validate one token against the accepted decimal-integer lexis.
	#
	# Args: $1 = the token, $2 = a label naming where the token came from.
	# Exits 2 with a lexical error when the token is outside the lexis.
	local token="$1" label="$2"
	if [[ ! $token =~ ^-?(0|[1-9][0-9]*)$ ]]; then
		printf 'compute-cohorts.sh: %s must be a decimal integer matching -?(0|[1-9][0-9]*); found: %s\n' \
			"$label" "$token" >&2
		exit 2
	fi
}

cc_validate_tokens() {
	# Validate every key token and every edge endpoint.
	#
	# Args: $1 = space-separated keys, $2 = space-separated `a:b` edges.
	local keys="$1" edges="$2" token
	pcoh_split_words "$keys"
	local -a key_tokens=("${PCOH_WORDS[@]}")
	for token in "${key_tokens[@]}"; do
		cc_require_integer "$token" "item key"
	done

	pcoh_split_words "$edges"
	local -a edge_tokens=("${PCOH_WORDS[@]}")
	# Each edge must be exactly two integer endpoints joined by a single colon;
	# anything else is a malformed edge token rather than a graph error.
	for token in "${edge_tokens[@]}"; do
		if [[ $token != *:* || $token == *:*:* ]]; then
			printf 'compute-cohorts.sh: edge must be <a>:<b>; found: %s\n' "$token" >&2
			exit 2
		fi
		cc_require_integer "${token%%:*}" "edge endpoint"
		cc_require_integer "${token#*:}" "edge endpoint"
	done
}

cc_main() {
	# Parse arguments, compute the cohorts, and print the result.
	local keys="" edges="" keys_seen=0
	while (($# > 0)); do
		case "$1" in
		--keys)
			(($# >= 2)) || {
				cc_usage >&2
				return 2
			}
			keys="$2"
			keys_seen=1
			shift 2
			;;
		--edges)
			(($# >= 2)) || {
				cc_usage >&2
				return 2
			}
			edges="$2"
			shift 2
			;;
		--help | -h)
			cc_usage
			return 0
			;;
		*)
			cc_usage >&2
			return 2
			;;
		esac
	done
	((keys_seen == 1)) || {
		cc_usage >&2
		return 2
	}

	cc_validate_tokens "$keys" "$edges"
	if ! pcoh_compute_cohorts "$keys" "$edges"; then
		printf '%s\n' "$PCOH_ERROR" >&2
		return 1
	fi
	printf '%s\n' "$PCOH_RESULT"
	return 0
}

# Guard so the file can be sourced without executing main. main's return code
# is captured and re-exited explicitly as the final statement.
if [[ ${BASH_SOURCE[0]} == "${0}" ]]; then
	cc_rc=0
	cc_main "$@" || cc_rc=$?
	exit "$cc_rc"
fi
