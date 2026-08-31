#!/usr/bin/env bash
# report-lane-assertion.sh: destination-portable command-line entry point for
# the parallel surface's lane-assertion diagnostic. It exists so a workspace
# that received the Claude customization payload can compare a manifest's
# hand-authored `expected_conflict_components` assertion against the derived
# conflict components with nothing but bash -- no Python, no Poetry, no
# repository checkout.
#
# Usage:
#   bash .claude/lib/bash/report-lane-assertion.sh --manifest <path> \
#       [--edges "<a>:<b> <a>:<b> ..."]
#
# `--edges` is optional; omitting it, or passing an empty string, means the
# derived conflict graph has no edges and every declared item is its own
# component.
#
# Output contract:
#   stdout  the advisory report: a header line, one ADVISORY line per finding,
#           and the closing line, byte identical to the Python authority
#           scripts/dev_tools/parallel_lane_assertion.py
#   stderr  usage text on a usage error
#   exit 0  every non-usage path, including a disagreement, an unreadable
#           manifest, an unparseable manifest, and an out-of-subset manifest
#   exit 2  usage error only
#
# The diagnostic is ADVISORY. A disagreement is never expressible as a non-zero
# exit status, because a planner must not be blocked by a hand-authored
# assertion that derivation contradicts.
set -euo pipefail

# Resolve this script's own directory so the library sources regardless of cwd.
RLA_SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
# shellcheck source=.claude/lib/bash/parallel-lane-assertion.sh
# shellcheck disable=SC1091
source "$RLA_SCRIPT_DIR/parallel-lane-assertion.sh"

pc_enforce_c_locale

rla_usage() {
	# Print the entry point's usage text.
	cat <<'EOF'
Usage: report-lane-assertion.sh --manifest <path> [--edges "<a>:<b> ..."]

Compares a parallel manifest's expected_conflict_components assertion against
the derived conflict components and prints an advisory report on stdout.

Options:
  --manifest  Path to docs/features/parallel/<slug>/parallel.md (required).
  --edges     Derived conflict edges as "<a>:<b> <c>:<d>" (optional).

Advisory only: the report never blocks. Every non-usage path exits 0.
EOF
}

rla_manifest_unreadable_detail() {
	# Echo the reason the manifest could not be read, or nothing when it can.
	#
	# Args: $1 = the manifest path. The Python authority prints str(OSError)
	# here, which names an errno string bash cannot reproduce; that is declared
	# divergence class 4, so only the line's prefix is parity scoped and this
	# detail is deliberately a bash-native phrasing rather than an imitation.
	local path="$1"
	if [[ ! -e $path ]]; then
		printf 'no such file: %s' "$path"
	elif [[ -d $path ]]; then
		printf 'path is a directory: %s' "$path"
	elif [[ ! -r $path ]]; then
		printf 'file is not readable: %s' "$path"
	fi
}

rla_report() {
	# Run the comparison over an already-parsed node table and print the report.
	#
	# The node table must already be populated by pm_parse_manifest. Args:
	# $1 = the raw --edges value.
	pla_read_manifest_inputs
	pla_parse_edges "$1"
	pla_compare "$PLA_ITEM_KEYS"
	pla_format_report
	# One printf with one trailing newline, matching the single print() the
	# Python authority issues, so the two lanes agree byte for byte.
	printf '%s\n' "$PLA_REPORT"
}

rla_main() {
	# Parse arguments, read the manifest, and print the advisory report.
	#
	# Exit 2 is reserved for a usage error -- an unknown flag, a flag missing its
	# value, or an absent --manifest -- and its usage text goes to stderr so it
	# cannot be mistaken for a report. --help is a successful request for the
	# same text, so it goes to stdout and exits 0. Every other path exits 0,
	# because the diagnostic is advisory and a verdict must not be expressible
	# as a non-zero status.
	local manifest="" edges="" manifest_seen=0 text
	while (($# > 0)); do
		case "$1" in
		--manifest)
			(($# >= 2)) || {
				rla_usage >&2
				return 2
			}
			manifest="$2"
			manifest_seen=1
			shift 2
			;;
		--edges)
			(($# >= 2)) || {
				rla_usage >&2
				return 2
			}
			edges="$2"
			shift 2
			;;
		--help | -h)
			rla_usage
			return 0
			;;
		*)
			rla_usage >&2
			return 2
			;;
		esac
	done
	((manifest_seen == 1)) || {
		rla_usage >&2
		return 2
	}

	# An unreadable manifest is reported and exits 0, not 2: the operator asked
	# for a diagnostic and the diagnostic's answer is that it could not look.
	local unreadable
	unreadable=$(rla_manifest_unreadable_detail "$manifest")
	if [[ -n $unreadable ]]; then
		printf 'Lane assertion: manifest unreadable (%s); no comparison made.\n' "$unreadable"
		return 0
	fi

	text=$(cat -- "$manifest")
	pc_errors_reset
	local parse_status=0
	pm_parse_manifest "$text" || parse_status=$?
	if ((parse_status == 2)); then
		# Status 2 is the scanner's refusal to model a construct. It is reported
		# on its own line, distinct from the unparseable line, because a refusal
		# is not a verdict about the manifest: the Python authority would parse
		# this document, and saying so is more useful than a guessed answer.
		printf 'Lane assertion: manifest outside the supported YAML subset (%s); no comparison made.\n' \
			"$PM_SUBSET_DETAIL"
		return 0
	fi
	if ((parse_status == 1)); then
		# The M1 message is reused byte for byte from pm_parse_manifest rather
		# than restated here, so the two lanes cannot drift on its wording.
		printf 'Lane assertion: manifest unparseable (%s).\n' "${PC_ERRORS[0]}"
		return 0
	fi

	rla_report "$edges"
	return 0
}

# Guard so the file can be sourced without executing main. main's return code
# is captured and re-exited explicitly as the final statement.
if [[ ${BASH_SOURCE[0]} == "${0}" ]]; then
	rla_rc=0
	rla_main "$@" || rla_rc=$?
	exit "$rla_rc"
fi
