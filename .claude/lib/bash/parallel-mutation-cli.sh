#!/usr/bin/env bash
# parallel-mutation-cli.sh: sourceable command-line helpers shared by the two
# parallel mutation entry points, remove-parallel-item.sh (issue #791) and
# mutate-parallel-run.sh (issue #890). It holds option parsing, required-option
# checks, integer, key-list, and edge-list validation, the `--at` timestamp, and
# the result printer, so both entry points apply one grammar and one exit-code
# contract.
#
# Grammar. Every option takes exactly one value. An unknown name, an
# abbreviation, a `--name=value` form, a repeated option, or a missing value is
# a usage error; a value beginning with `--` counts as missing. Integer tokens
# match `-?(0|[1-9][0-9]*)`. `--at` matches `[0-9A-Za-z:.+-]+` and defaults to
# the current UTC minute as yyyy-MM-ddTHH-mm.
#
# Exit-code contract carried by these helpers:
#   exit 2  usage error; one stderr line beginning
#           "PARALLEL_MUTATION_ERROR: usage error:" (pmcli_usage_error)
#   exit 1  rule rejection; PM_ERROR printed as one stderr line (pmcli_emit)
#   exit 0  success; PM_RESULT printed as one stdout line (pmcli_emit)
#
# Dependencies. pmcli_emit reads PM_RESULT and PM_ERROR, which
# parallel-mutation.sh owns; an entry point sources that library before calling
# it. The only external command is `date`, run solely when `--at` is omitted.
#
# shellcheck disable=SC2034
# SC2034 is disabled file-wide because PMCLI_OPTS and PMCLI_VALUE are written
# here and read by the two entry points, which shellcheck analyses separately.

# Resolve this file's own directory so its dependencies source regardless of
# the caller's working directory.
PMCLI_LIB_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
# shellcheck source=.claude/lib/bash/parallel-cohorts.sh
# shellcheck disable=SC1091
source "$PMCLI_LIB_DIR/parallel-cohorts.sh"

# Parsed option values, keyed by option name without the leading dashes.
declare -gA PMCLI_OPTS=()

# The canonical value stored by the most recent validator.
PMCLI_VALUE=""

pmcli_usage_error() {
	# Print one usage-error line on stderr and exit 2.
	#
	# Args: $1 = the specific reason.
	printf 'PARALLEL_MUTATION_ERROR: usage error: %s\n' "$1" >&2
	exit 2
}

pmcli_parse_options() {
	# Parse `--name value` pairs into PMCLI_OPTS against an allowed-name list.
	#
	# Args: $1 = space-separated allowed option names, then the arguments.
	local allowed="$1" name
	shift
	PMCLI_OPTS=()
	while (($# > 0)); do
		name="${1#--}"
		if [[ $1 != --* ]] || ! pc_contains_word "$allowed" "$name"; then
			pmcli_usage_error "unknown option: $1"
		fi
		if [[ -n ${PMCLI_OPTS["$name"]+set} ]]; then
			pmcli_usage_error "option given more than once: $1"
		fi
		if (($# < 2)) || [[ $2 == --* ]]; then
			pmcli_usage_error "option $1 requires a value"
		fi
		PMCLI_OPTS["$name"]="$2"
		shift 2
	done
}

pmcli_require() {
	# Exit with a usage error unless every named option was supplied.
	#
	# Args: the option names that are required.
	local name
	for name in "$@"; do
		[[ -n ${PMCLI_OPTS["$name"]+set} ]] || pmcli_usage_error "missing required option: --$name"
	done
}

pmcli_integer() {
	# Validate one integer token and store it in canonical form in PMCLI_VALUE.
	#
	# Args: $1 = the token, $2 = a label naming where it came from. The
	# validators store their result in a global instead of echoing it, because
	# a usage error raised inside a command substitution would exit only the
	# subshell.
	if [[ ! $1 =~ ^-?(0|[1-9][0-9]*)$ ]]; then
		pmcli_usage_error "$2 must be a decimal integer matching -?(0|[1-9][0-9]*); found: $1"
	fi
	PMCLI_VALUE=$(($1))
}

pmcli_integer_list() {
	# Validate a whitespace-separated key list; store it space separated.
	#
	# Args: $1 = the list, $2 = a label naming the option.
	local token canonical=""
	pcoh_split_words "$1"
	local -a tokens=("${PCOH_WORDS[@]}")
	for token in "${tokens[@]}"; do
		pmcli_integer "$token" "$2 key"
		canonical="$canonical $PMCLI_VALUE"
	done
	PMCLI_VALUE="${canonical# }"
}

pmcli_edge_list() {
	# Validate a whitespace-separated `a:b` edge list; store it canonical.
	#
	# Args: $1 = the edge list.
	local token first canonical=""
	pcoh_split_words "$1"
	local -a tokens=("${PCOH_WORDS[@]}")
	for token in "${tokens[@]}"; do
		if [[ $token != *:* || $token == *:*:* ]]; then
			pmcli_usage_error "edge must be <a>:<b>; found: $token"
		fi
		pmcli_integer "${token%%:*}" "edge endpoint"
		first="$PMCLI_VALUE"
		pmcli_integer "${token#*:}" "edge endpoint"
		canonical="$canonical $first:$PMCLI_VALUE"
	done
	PMCLI_VALUE="${canonical# }"
}

pmcli_timestamp() {
	# Store the validated `--at` value, or the current UTC minute when omitted.
	if [[ -n ${PMCLI_OPTS[at]+set} ]]; then
		PMCLI_VALUE="${PMCLI_OPTS[at]}"
		[[ $PMCLI_VALUE =~ ^[0-9A-Za-z:.+-]+$ ]] ||
			pmcli_usage_error "--at must match [0-9A-Za-z:.+-]+; found: $PMCLI_VALUE"
	else
		# The only clock read of either entry point, taken only when --at is
		# omitted.
		PMCLI_VALUE=$(date -u +%Y-%m-%dT%H-%M)
	fi
}

pmcli_emit() {
	# Print the library outcome and return the matching exit code.
	#
	# Args: $1 = the library function's return code.
	if (($1 != 0)); then
		printf '%s\n' "$PM_ERROR" >&2
		return 1
	fi
	printf '%s\n' "$PM_RESULT"
	return 0
}
