#!/usr/bin/env bash
# cleanup_worktrees_scan_helper.sh: the bundled real implementation behind the
# cleanup-worktrees filesystem-scan seam (CLEANUP_WT_SCAN_BIN, resolved by
# cleanup_wt_scan_bin in cleanup_worktrees_report_records_lib.sh). It performs the
# read-only filesystem observations the report-record functions need and emits them as
# one pipe-delimited tuple per candidate directory. It never writes, moves, or deletes
# anything.
#
# Usage:
#   cleanup_worktrees_scan_helper.sh scan-dirs <root-dir> [<root-dir> ...]
#
# For each <root-dir> that exists as a directory, and for each immediate subdirectory
# of that root, one record is written to stdout:
#
#   <path>|<has_gitfile>|<gitdir_target_exists>|<size>
#
#   path                 the subdirectory path as constructed from the given root.
#   has_gitfile          1 when <path>/.git exists as a regular file (the worktree
#                        pointer-file shape), else 0.
#   gitdir_target_exists NA when has_gitfile is 0 (there is no pointer to resolve);
#                        otherwise 1 when the `gitdir: <target>` line's target exists
#                        (resolved relative to <path> when the target is not absolute),
#                        else 0. A target is absolute when it begins with `/` or with a
#                        drive letter followed by `/` or `\` (for example `C:/repo`).
#   size                 best-effort `du -sh` size of <path>, or the literal `unknown`
#                        when du fails or prints nothing. Size is advisory only, so a
#                        du failure never fails the scan.
#
# A root that does not exist is skipped silently: an absent worktree-tracking folder is
# a normal state, not an error. Records are emitted in the shell's glob order per root;
# callers that need a total order sort the combined output themselves.
#
# Pointer-filename test seam: the pointer file's name is read from
# CLEANUP_WT_SCAN_GITFILE_NAME and defaults to `.git`, which is the only name a real
# worktree ever uses. The seam exists because git refuses to index any path component
# named `.git`, so a checked-in fixture tree cannot carry a real `.git` pointer file and
# the repository's test policy prohibits creating one at test time. The bats fixtures
# under tests/fixtures/cleanup_worktrees/scan_roots/ therefore name the pointer file
# `dotgit` and set this variable, exactly as the git-binary seam CLEANUP_WT_GIT_BIN and
# the SHELL_QC_<TOOL>_BIN seams substitute a checked-in stand-in for a real dependency.
# An empty value is treated as missing and falls back to `.git`.
set -euo pipefail
# shellcheck source=.claude/skills/cleanup-merged-worktrees/scripts/cleanup_worktrees_enumerate_lib.sh
# shellcheck disable=SC1091
source "$(dirname -- "${BASH_SOURCE[0]}")/cleanup_worktrees_enumerate_lib.sh"

scan_helper_dir_size() {
	# Echo the best-effort human-readable size of a directory, or `unknown`.
	#
	# The du/cut pipeline is captured with `|| size=""` so a du failure under
	# `set -euo pipefail` degrades to `unknown` rather than aborting the scan.
	#
	# Args: $1 = directory path. Always returns 0.
	local dir=${1:-}
	local size=""
	size=$(du -sh "$dir" 2>/dev/null | cut -f1) || size=""
	if [[ -z $size ]]; then
		size="unknown"
	fi
	printf '%s\n' "$size"
}

scan_helper_gitfile_name() {
	# Echo the worktree pointer file's name, honoring the CLEANUP_WT_SCAN_GITFILE_NAME
	# test seam. An empty or unset value falls back to the real name `.git`.
	#
	# Args: none. Always returns 0 and echoes exactly one name.
	local override=${CLEANUP_WT_SCAN_GITFILE_NAME:-}
	if [[ -n $override ]]; then
		printf '%s\n' "$override"
		return 0
	fi
	printf '%s\n' ".git"
}

scan_helper_target_present() {
	# Return 0 when <path> exists, else non-zero. This is the helper's single
	# filesystem existence check, kept in its own function as a test seam: bats tests
	# redefine this function after sourcing the helper so that a drive-letter target
	# can be reported present on a runner that has no such drive (issue #706).
	#
	# Args: $1 = path.
	[[ -e ${1:-} ]]
}

scan_helper_gitdir_target_exists() {
	# Echo 1 when the worktree pointer file in <dir> names an existing gitdir target,
	# else 0. A missing, unreadable, or malformed pointer file yields 0 (the pointer
	# does not resolve), never a crash.
	#
	# Args: $1 = directory path holding the pointer file, $2 = pointer file name.
	# Always returns 0.
	local dir=${1:-}
	local gitfile=${2:-.git}
	local line="" target=""
	line=$(grep -m1 '^gitdir:' "$dir/$gitfile" 2>/dev/null) || line=""
	target=${line#gitdir:}
	# Strip a trailing CR (a pointer file written on Windows) and leading whitespace.
	target=${target%$'\r'}
	target=${target#"${target%%[![:space:]]*}"}
	if [[ -z $target ]]; then
		printf '0\n'
		return 0
	fi
	if ! cleanup_wt_is_absolute_path "$target"; then
		target="$dir/$target"
	fi
	if scan_helper_target_present "$target"; then
		printf '1\n'
	else
		printf '0\n'
	fi
}

scan_helper_scan_dirs() {
	# Emit one `<path>|<has_gitfile>|<gitdir_target_exists>|<size>` record per immediate
	# subdirectory of each given root. Roots that are not directories are skipped.
	#
	# Args: the root directories. Always returns 0.
	local root entry path has_gitfile target_exists size gitfile
	gitfile=$(scan_helper_gitfile_name)
	for root in "$@"; do
		[[ -d $root ]] || continue
		for entry in "$root"/*/; do
			[[ -d $entry ]] || continue
			path=${entry%/}
			has_gitfile=0
			target_exists="NA"
			if [[ -f "$path/$gitfile" ]]; then
				has_gitfile=1
				target_exists=$(scan_helper_gitdir_target_exists "$path" "$gitfile")
			fi
			size=$(scan_helper_dir_size "$path")
			printf '%s|%s|%s|%s\n' "$path" "$has_gitfile" "$target_exists" "$size"
		done
	done
}

scan_helper_usage() {
	# Print the helper usage text.
	cat <<'EOF'
Usage: cleanup_worktrees_scan_helper.sh scan-dirs <root-dir> [<root-dir> ...]

Emits one read-only record per immediate subdirectory of each root:
  <path>|<has_gitfile:0|1>|<gitdir_target_exists:0|1|NA>|<size-or-unknown>
EOF
}

scan_helper_main() {
	# Dispatch on the subcommand. Only `scan-dirs` is supported; anything else prints
	# usage to stderr and exits 2 (usage-error parity with cleanup-worktrees.sh).
	local command=${1:-}
	case "$command" in
	scan-dirs)
		shift
		scan_helper_scan_dirs "$@"
		;;
	*)
		scan_helper_usage >&2
		return 2
		;;
	esac
	return 0
}

# Guard so the file can be sourced without executing the scan.
if [[ ${BASH_SOURCE[0]} == "${0}" ]]; then
	rc=0
	scan_helper_main "$@" || rc=$?
	exit "$rc"
fi
