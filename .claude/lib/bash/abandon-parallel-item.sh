#!/usr/bin/env bash
# abandon-parallel-item.sh: bundled destination-runtime port of
# scripts/dev_tools/parallel_mutation_abandon_cli.py (issue #763). It executes the
# abandon disposition of /parallel-remove -- close the item's pull request, then
# remove its worktree -- with nothing but bash, gh, and git on PATH: no Python, no
# Poetry, no repository checkout.
#
# Usage:
#   bash .claude/lib/bash/abandon-parallel-item.sh --item <key> --disposition abandon --confirm-abandon --pr <pr-number> --worktree <worktree-path>
#
# The PreToolUse hook .claude/hooks/enforce-parallel-abandon-gate.ps1 matches the
# command text of that invocation: a command carrying the disposition token
# without the confirmation marker is denied before it runs. The option tokens are
# declared once each below, and tests/scripts/dev_tools/test_parallel_abandon_token_seam.py
# binds them to the Python reference and to the hook.
#
# Exit codes:
#   0  success; both side effects ran and nothing was written to stdout
#   1  a side effect failed; the first failure is reported and nothing after it runs
#   2  refusal (a disposition other than abandon, or no confirmation marker) or a
#      usage error; both are reported before any side effect
#
# Declared divergences from the Python reference, all within "usage errors exit 2":
#   - argparse usage text is not reproduced; only exit code 2 is matched, with one
#     stderr line prefixed "PARALLEL_ABANDON_ERROR: usage error:".
#   - option abbreviations are rejected (argparse accepts a unique prefix).
#   - --item and --pr accept only canonical decimal integers (0 or [1-9][0-9]*).
#   - -h and --help are unknown options.
#   - an option value beginning with -- is treated as a missing value.
# Repeated options keep the last value and the joined --name=value form is
# accepted, as argparse does.
set -euo pipefail

# The option tokens and messages the abandon surface is built from. The first
# three lines are read by the seam test and by tests/shell/parallel_abandon.bats.
readonly ABANDON_DISPOSITION_OPTION='--disposition'
readonly ABANDON_DISPOSITION_VALUE='abandon'
readonly ABANDON_CONFIRM_OPTION='--confirm-abandon'
readonly ABANDON_ITEM_OPTION='--item'
readonly ABANDON_PR_OPTION='--pr'
readonly ABANDON_WORKTREE_OPTION='--worktree'
readonly ABANDON_VALID_DISPOSITIONS='detach abandon'
readonly ABANDON_ERROR_PREFIX='PARALLEL_ABANDON_ERROR:'

abandon_usage_error() {
	# Report a usage error on stderr and exit 2 before any side effect.
	#
	# Args: $1 = the detail naming the offending argument.
	printf '%s usage error: %s\n' "$ABANDON_ERROR_PREFIX" "$1" >&2
	exit 2
}

abandon_require_integer() {
	# Require a canonical decimal integer, as the reference's type=int requires
	# an integer; leading zeros, signs, and blanks are usage errors here.
	#
	# Args: $1 = the value, $2 = the option it was given for.
	if [[ ! $1 =~ ^(0|[1-9][0-9]*)$ ]]; then
		abandon_usage_error "$2 requires a canonical decimal integer; got '$1'"
	fi
}

abandon_require_disposition() {
	# Require a member of the disposition vocabulary, as the reference's
	# argparse choices do. Membership is not the abandon refusal: a valid
	# disposition other than abandon is refused later with its own message.
	#
	# Args: $1 = the disposition value.
	local candidate
	# Scan the space-separated vocabulary for an exact match.
	for candidate in $ABANDON_VALID_DISPOSITIONS; do
		if [ "$candidate" = "$1" ]; then
			return 0
		fi
	done
	abandon_usage_error "$ABANDON_DISPOSITION_OPTION must be one of: $ABANDON_VALID_DISPOSITIONS; got '$1'"
}

abandon_side_effect_failed() {
	# Report a failed side effect and exit 1, so nothing after it runs.
	#
	# Args: $1 = the exit code (-1 when the executable is not on PATH), then the
	# command vector, which is rendered joined by single spaces.
	local code="$1"
	shift
	printf '%s abandon side effect failed with exit code %s: %s\n' "$ABANDON_ERROR_PREFIX" "$code" "$*" >&2
	exit 1
}

abandon_run_side_effect() {
	# Run one side-effect command resolved through PATH. A non-zero exit is
	# captured rather than aborting under set -e, so it can be reported.
	#
	# Args: the command vector, executable name first.
	local executable status=0
	if ! executable="$(command -v -- "$1")"; then
		abandon_side_effect_failed -1 "$@"
	fi
	"$executable" "${@:2}" || status=$?
	if [ "$status" -ne 0 ]; then
		abandon_side_effect_failed "$status" "$@"
	fi
}

item=''
disposition=''
pr=''
worktree=''
confirmed=0
have_item=0
have_disposition=0
have_pr=0
have_worktree=0

# Parse the argument vector. The bare confirmation flag sets the marker; each of
# the four valued options takes its value joined (--name=value) or as the next
# argument; anything else -- an unknown option, an abbreviation, a value on the
# confirmation flag, or a positional argument -- is a usage error.
while [ "$#" -gt 0 ]; do
	argument="$1"
	shift
	if [ "$argument" = "$ABANDON_CONFIRM_OPTION" ]; then
		confirmed=1
		continue
	fi
	name="${argument%%=*}"
	case "$name" in
	"$ABANDON_ITEM_OPTION" | "$ABANDON_DISPOSITION_OPTION" | "$ABANDON_PR_OPTION" | "$ABANDON_WORKTREE_OPTION") ;;
	*) abandon_usage_error "unrecognized argument: $argument" ;;
	esac
	# Take the joined value when present, otherwise the next argument; an absent
	# value is represented by the option prefix so one check rejects both forms.
	if [ "$name" != "$argument" ]; then
		value="${argument#*=}"
	elif [ "$#" -gt 0 ]; then
		value="$1"
		shift
	else
		value='--'
	fi
	if [[ $value == --* ]]; then
		abandon_usage_error "$name requires a value"
	fi
	# Store the value on its option; a repeated option keeps the last value.
	case "$name" in
	"$ABANDON_ITEM_OPTION") item="$value" have_item=1 ;;
	"$ABANDON_DISPOSITION_OPTION") disposition="$value" have_disposition=1 ;;
	"$ABANDON_PR_OPTION") pr="$value" have_pr=1 ;;
	*) worktree="$value" have_worktree=1 ;;
	esac
done

# Every valued option is required, as in the reference parser.
if [ "$have_item" -eq 0 ] || [ "$have_disposition" -eq 0 ] || [ "$have_pr" -eq 0 ] || [ "$have_worktree" -eq 0 ]; then
	abandon_usage_error "the options $ABANDON_ITEM_OPTION, $ABANDON_DISPOSITION_OPTION, $ABANDON_PR_OPTION, and $ABANDON_WORKTREE_OPTION are required"
fi
abandon_require_integer "$item" "$ABANDON_ITEM_OPTION"
abandon_require_integer "$pr" "$ABANDON_PR_OPTION"
abandon_require_disposition "$disposition"

# The two refusals come before any side effect: this entry point exists only to
# execute the confirmed abandon path, so any other request does nothing.
if [ "$disposition" != "$ABANDON_DISPOSITION_VALUE" ]; then
	printf "%s this CLI executes the '%s' disposition only; got '%s'.\n" "$ABANDON_ERROR_PREFIX" "$ABANDON_DISPOSITION_VALUE" "$disposition" >&2
	exit 2
fi
if [ "$confirmed" -ne 1 ]; then
	printf '%s refusing to abandon item %s without the explicit %s confirmation marker; no side effect was performed.\n' "$ABANDON_ERROR_PREFIX" "$item" "$ABANDON_CONFIRM_OPTION" >&2
	exit 2
fi

# Close the pull request before removing the worktree: removing the worktree
# first would leave an open pull request whose checkout is gone.
abandon_run_side_effect gh pr close "$pr"
abandon_run_side_effect git worktree remove "$worktree"
exit 0
