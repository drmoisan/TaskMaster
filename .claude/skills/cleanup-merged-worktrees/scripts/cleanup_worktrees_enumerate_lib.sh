#!/usr/bin/env bash
# cleanup_worktrees_enumerate_lib.sh: sourceable enumeration/protection function
# group for the cleanup-worktrees tool, split out of cleanup_worktrees_lib.sh to keep
# every file within the 500-line cap. Provides the git-binary test seam
# (cleanup_wt_git), branch/worktree enumeration (enumerate_branches,
# parse_worktree_list), path normalization (normalize_wt_path), the
# current-worktree/branch and base-branch protection set (compute_protected), the
# main-freshness warning (check_main_freshness), the shared absolute-path predicate
# (cleanup_wt_is_absolute_path), and the worktree-tracking scan roots
# (cleanup_wt_split_roots, cleanup_wt_derive_scan_roots, cleanup_wt_scan_roots). The
# classification ladder, consolidation, deletion, and the CLI live in sibling files
# (cleanup_worktrees_lib.sh, cleanup_worktrees_actions_lib.sh, cleanup-worktrees.sh).
#
# Sourcing contract: defines functions and the single constant CLEANUP_WT_BASE_BRANCH
# and runs no other work at source time, so wrapper and bats suites source it safely.
# It MUST be sourced before cleanup_worktrees_lib.sh, whose classification functions
# call cleanup_wt_git, parse_worktree_list, compute_protected, and normalize_wt_path
# defined here. The filesystem-scan helper cleanup_worktrees_scan_helper.sh runs as a
# separate process and sources this library directly for cleanup_wt_is_absolute_path.
#
# All git commands go through cleanup_wt_git so tests can stub the git binary via
# CLEANUP_WT_GIT_BIN. Git exit-code capture rule: the `git worktree list --porcelain`
# output consumed by parse_worktree_list is captured in the PARENT shell
# (`out=$(cmd) || rc=$?`, then iterated over `<<<"$out"`) so a non-zero exit is
# observed here rather than lost inside a process substitution. On a hard failure
# parse_worktree_list emits no records and returns git's non-zero exit code, and
# compute_protected propagates that failure rather than degrading to an empty
# (weakened) protected set. An empty porcelain output with exit 0 remains a valid
# empty list. enumerate_branches captures `git for-each-ref` in the parent shell
# (`out=$(...) || rc=$?`) BEFORE sorting, so a for-each-ref hard failure is observed
# even when the caller lacks pipefail; it returns git's exit code with no stdout rather
# than an empty branch list. compute_protected captures `git rev-parse --abbrev-ref
# HEAD` and `git rev-parse --show-toplevel`; a hard failure of either is fatal (git's
# exit code, no weakened fallback), and the base-branch record is emitted only after
# both guards pass. The detached-HEAD case (rev-parse prints HEAD) is unaffected.

cleanup_wt_git() {
	# Resolve the git binary honoring the CLEANUP_WT_GIT_BIN override seam, then
	# execute it with the caller's arguments.
	#
	# When CLEANUP_WT_GIT_BIN is set to a non-empty value it must point to an
	# existing executable; an empty or nonexistent value is treated as missing and
	# falls back to `command -v git`. This mirrors the SHELL_QC_<TOOL>_BIN seam in
	# scripts/bash/shell_qc_lib.sh so the bats suites can stub git deterministically.
	#
	# Args: the git subcommand and its arguments.
	# Returns git's exit code, or 127 when no git binary can be resolved.
	local override=${CLEANUP_WT_GIT_BIN:-}
	local git_bin=""
	if [[ -n $override && -x $override ]]; then
		git_bin=$override
	else
		git_bin=$(command -v git 2>/dev/null) || git_bin=""
	fi
	if [[ -z $git_bin ]]; then
		printf 'cleanup-worktrees: no git binary resolved (CLEANUP_WT_GIT_BIN=%s)\n' "$override" >&2
		return 127
	fi
	"$git_bin" "$@"
}

enumerate_branches() {
	# Enumerate local branches via plumbing, one "name sha" pair per line.
	#
	# Uses `git for-each-ref` rather than `git branch` so loose refs and packed-refs
	# are read uniformly and the output carries no decoration markers (`*`, `+`) or
	# column padding. Output is LC_ALL=C sorted for deterministic ordering and
	# deterministic cross-branch cherry-pick order.
	#
	# The for-each-ref result is captured in the parent shell (`out=$(...) || rc=$?`) so
	# a non-zero for-each-ref exit is observed here even when the caller lacks pipefail
	# (the bats harness); piping straight into sort would attribute only sort's exit to
	# rc and lose the for-each-ref failure. On a hard failure it prints a diagnostic to
	# stderr, emits no stdout, and returns git's non-zero exit code. Returns git's exit
	# code from for-each-ref (0 on success).
	local rc=0 out
	out=$(cleanup_wt_git for-each-ref \
		--format='%(refname:short) %(objectname)' refs/heads/) || rc=$?
	if ((rc != 0)); then
		printf 'cleanup-worktrees: git for-each-ref refs/heads/ failed (rc=%s)\n' "$rc" >&2
		return "$rc"
	fi
	# printf-pipe (not a herestring) so an empty ref list emits nothing rather than one
	# empty line.
	printf '%s' "$out" | LC_ALL=C sort
}

parse_worktree_list() {
	# Parse `git worktree list --porcelain` stanza-wise into pipe-delimited records.
	#
	# Emits one record per worktree: `path|head|branch-or-DETACHED|flags`. A new
	# stanza begins at each `worktree ` line; a blank line closes the current stanza.
	# The branch field carries the short branch name (refs/heads/ stripped) or the
	# literal DETACHED when the stanza had a `detached` line. flags is a
	# comma-separated set drawn from main,detached,bare,locked,prunable; the first
	# stanza always carries the `main` flag (the main worktree is never a candidate).
	#
	# The porcelain listing is captured in the parent shell (`out=$(...) || rc=$?`) so
	# a non-zero git exit is observed here rather than being lost inside a process
	# substitution. Returns 0 on success (including an empty porcelain output with exit
	# 0, which is a valid empty list); on a hard git failure it prints a diagnostic to
	# stderr, emits no records, and returns git's non-zero exit code.
	local rc=0 line out
	local path="" head="" branch="" detached=0 bare=0 locked=0 prunable=0
	local first=1 have=0
	emit_record() {
		# Flush the accumulated stanza as one record; no-op when none accumulated.
		((have == 0)) && return 0
		local -a flag_parts=()
		((first == 1)) && flag_parts+=("main")
		((detached == 1)) && flag_parts+=("detached")
		((bare == 1)) && flag_parts+=("bare")
		((locked == 1)) && flag_parts+=("locked")
		((prunable == 1)) && flag_parts+=("prunable")
		local flags=""
		local IFS=,
		flags="${flag_parts[*]}"
		local branch_field="DETACHED"
		[[ -n $branch ]] && branch_field=$branch
		printf '%s|%s|%s|%s\n' "$path" "$head" "$branch_field" "$flags"
		first=0
	}
	# Guarded parent-shell capture: a non-zero git exit is observed here (an unguarded
	# `out=$(...)` would abort under the wrapper's set -euo pipefail). On a hard
	# failure, return before emitting any record so the caller cannot mistake a git
	# failure for an empty worktree list.
	out=$(cleanup_wt_git worktree list --porcelain) || rc=$?
	if ((rc != 0)); then
		printf 'cleanup-worktrees: git worktree list --porcelain failed (rc=%s)\n' "$rc" >&2
		return "$rc"
	fi
	while IFS= read -r line || [[ -n $line ]]; do
		case "$line" in
		"worktree "*)
			emit_record
			path=${line#worktree }
			head="" branch="" detached=0 bare=0 locked=0 prunable=0
			have=1
			;;
		"HEAD "*) head=${line#HEAD } ;;
		"branch "*) branch=${line#branch refs/heads/} ;;
		detached) detached=1 ;;
		bare) bare=1 ;;
		"locked"*) locked=1 ;;
		"prunable"*) prunable=1 ;;
		"") ;; # stanza separator; the next `worktree ` flushes the record
		esac
	done <<<"$out"
	emit_record
	return "$rc"
}

normalize_wt_path() {
	# Normalize a worktree path for comparison across Windows/WSL and git output.
	#
	# Converts backslashes to forward slashes, lowercases (case-insensitive match on
	# Windows), and strips a single trailing slash. Echoes the normalized value;
	# echoes nothing for an empty input.
	#
	# Args: $1 = path.
	local p=${1:-}
	[[ -z $p ]] && return 0
	p=${p//\\//}
	p=${p,,}
	p=${p%/}
	printf '%s\n' "$p"
}

# The ladder's fixed comparison base. compute_protected protects it by name in every
# checkout topology and delete_candidate refuses it. It is deliberately not read from
# the environment, so no override can unprotect the base branch (issue #594).
CLEANUP_WT_BASE_BRANCH="main"

compute_protected() {
	# Compute the protected branch and protected worktree paths (PROTECTED_CURRENT).
	#
	# Dual exclusion, both checks required:
	#   1. Current branch via `git rev-parse --abbrev-ref HEAD`. A `HEAD` result means
	#      detached: no branch name to protect (only the worktree path is protected).
	#   2. Current worktree path via `git rev-parse --show-toplevel`, compared after
	#      slash/case normalization against each porcelain worktree path.
	# The main worktree (first porcelain stanza) is always protected regardless of the
	# above. Emits `protected-branch|<name>` (omitted when detached) and one
	# `protected-path|<normalized-path>` line per protected worktree.
	#
	# The base branch CLEANUP_WT_BASE_BRANCH is always protected by name, whichever
	# worktree (if any) has it checked out. Its `protected-branch|` record is emitted
	# only after both rev-parse guards pass, so the fail-closed contract below is
	# unchanged, and it is omitted when the current branch already equals the base, so
	# no duplicate record is emitted.
	#
	# Returns 0 on success. A parse_worktree_list hard failure propagates as its
	# non-zero return; the caller must treat that as fatal, not as an empty (weakened)
	# protected set. A hard failure of `git rev-parse --abbrev-ref HEAD` or
	# `git rev-parse --show-toplevel` is likewise fatal (return git's exit code), never a
	# weakened protection fallback; the detached-HEAD case (rev-parse succeeds printing
	# HEAD) is unaffected and keeps its branch-name omission.
	local rc=0 current_branch current_top norm_cur cbrc=0 ctrc=0
	current_branch=$(cleanup_wt_git rev-parse --abbrev-ref HEAD) || cbrc=$?
	if ((cbrc != 0)); then
		printf 'cleanup-worktrees: git rev-parse --abbrev-ref HEAD failed (rc=%s)\n' "$cbrc" >&2
		return "$cbrc"
	fi
	current_top=$(cleanup_wt_git rev-parse --show-toplevel) || ctrc=$?
	if ((ctrc != 0)); then
		printf 'cleanup-worktrees: git rev-parse --show-toplevel failed (rc=%s)\n' "$ctrc" >&2
		return "$ctrc"
	fi
	norm_cur=$(normalize_wt_path "$current_top")
	if [[ -n $current_branch && $current_branch != HEAD ]]; then
		printf 'protected-branch|%s\n' "$current_branch"
	fi
	# Base-branch protection is unconditional and emitted only after both rev-parse
	# guards pass; it is skipped when the current branch already equals the base.
	if [[ $current_branch != "$CLEANUP_WT_BASE_BRANCH" ]]; then
		printf 'protected-branch|%s\n' "$CLEANUP_WT_BASE_BRANCH"
	fi
	local first=1 record path norm pout prc=0
	# Guarded parent-shell capture: a parse_worktree_list hard failure must abort here,
	# not degrade to an empty protected set. Compare the current toplevel against each
	# porcelain worktree path; the first stanza (main worktree) is always protected.
	pout=$(parse_worktree_list) || prc=$?
	if ((prc != 0)); then
		return "$prc"
	fi
	while IFS= read -r record; do
		[[ -z $record ]] && continue
		path=${record%%|*}
		norm=$(normalize_wt_path "$path")
		if ((first == 1)); then
			printf 'protected-path|%s\n' "$norm"
		elif [[ -n $norm_cur && $norm == "$norm_cur" ]]; then
			printf 'protected-path|%s\n' "$norm"
		fi
		first=0
	done <<<"$pout"
	return "$rc"
}

check_main_freshness() {
	# Warn when local `main` diverges from `origin/main`; never block classification.
	#
	# Compares `git rev-parse main` with `git rev-parse origin/main`. On mismatch it
	# emits the report line `WARN|main-divergence|<local-sha>|<origin-sha>`. A stale
	# local `main` can only produce a false "not merged" (the safe direction), so this
	# is advisory only and always returns 0. If either rev-parse fails (e.g. no
	# origin/main configured), the check is skipped without error.
	local local_sha origin_sha
	local_sha=$(cleanup_wt_git rev-parse main 2>/dev/null) || return 0
	origin_sha=$(cleanup_wt_git rev-parse origin/main 2>/dev/null) || return 0
	if [[ -n $local_sha && -n $origin_sha && $local_sha != "$origin_sha" ]]; then
		printf 'WARN|main-divergence|%s|%s\n' "$local_sha" "$origin_sha"
	fi
	return 0
}

cleanup_wt_is_absolute_path() {
	# Return 0 when <path> is absolute, else 1. Pure: no filesystem access.
	#
	# Absolute forms are a leading `/` (POSIX paths, MSYS `/c/...` paths, and `//server`
	# UNC paths) and a drive letter followed by `/` or `\` (`C:/...`, `c:/...`,
	# `C:\...`), which is the form Git for Windows writes into a worktree's `.git`
	# pointer file (issue #706). A drive letter with no separator (`C:rel`) is
	# drive-relative and is not absolute; an empty path is not absolute. This is the one
	# absolute-path predicate of the skill: the preserve library and the filesystem-scan
	# helper both call it.
	#
	# Args: $1 = path. Returns 0 (absolute) or 1 (not absolute).
	local path=${1:-}
	[[ $path == /* || $path == [A-Za-z]:[/\\]* ]]
}

cleanup_wt_split_roots() {
	# Split a CLEANUP_WT_ORPHAN_ROOTS value into absolute roots, one per line.
	#
	# Entries are separated by `;`, by a newline, or by `:`. A `:` is part of the path
	# instead when the text before it in the current entry is exactly one ASCII letter
	# and the character after it is `/` or `\` (a drive letter, as in `C:/x`). Empty
	# entries are dropped. An entry that is not absolute (cleanup_wt_is_absolute_path) is
	# dropped with a one-line stderr diagnostic naming it, because a relative root would
	# be resolved against the process's current working directory.
	#
	# The value is walked one character at a time with parameter expansion and never
	# word-split, so no pathname expansion occurs and a `*` is emitted literally.
	#
	# Args: $1 = the override value. Always returns 0.
	local value=${1:-}
	local -a entries=()
	local entry="" ch next i
	for ((i = 0; i < ${#value}; i++)); do
		ch=${value:i:1}
		if [[ $ch == ';' || $ch == $'\n' ]]; then
			entries+=("$entry")
			entry=""
		elif [[ $ch == ':' ]]; then
			next=${value:i+1:1}
			if [[ ${#entry} -eq 1 && $entry == [A-Za-z] && $next == [/\\] ]]; then
				entry+=$ch
			else
				entries+=("$entry")
				entry=""
			fi
		else
			entry+=$ch
		fi
	done
	entries+=("$entry")
	for entry in "${entries[@]}"; do
		[[ -z $entry ]] && continue
		if ! cleanup_wt_is_absolute_path "$entry"; then
			printf 'cleanup-worktrees: CLEANUP_WT_ORPHAN_ROOTS entry is not absolute, dropped: %s\n' "$entry" >&2
			continue
		fi
		printf '%s\n' "$entry"
	done
	return 0
}

cleanup_wt_derive_scan_roots() {
	# Echo the registration-derived scan roots, one per line.
	#
	# Input is parse_worktree_list output; its first record is the main worktree. The
	# candidate for every later record is the parent directory of its path (backslashes
	# converted to `/`). A candidate is kept only when its normalize_wt_path value N:
	#   - is not the main worktree's normalized path and is not one of its ancestors
	#     (scanning an ancestor would list the main worktree's siblings), and
	#   - is not equal to, and not inside, any registered worktree, main included
	#     (its subdirectories are that worktree's own content, not worktrees).
	# Kept candidates are emitted in LC_ALL=C order of N, once per N, using the first
	# spelling seen in record order.
	#
	# Args: $1 = parse_worktree_list output. Always returns 0.
	local records=${1:-}
	local -a paths=() norms=()
	local record path
	while IFS= read -r record; do
		[[ -z $record ]] && continue
		path=${record%%|*}
		paths+=("$path")
		norms+=("$(normalize_wt_path "$path")")
	done <<<"$records"
	((${#paths[@]} < 2)) && return 0
	local main_norm=${norms[0]}
	local -A seen=()
	local -a kept=()
	local i p parent n w inside
	for ((i = 1; i < ${#paths[@]}; i++)); do
		p=${paths[i]//\\//}
		parent=${p%/*}
		[[ -z $parent || $parent == "$p" ]] && continue
		n=$(normalize_wt_path "$parent")
		[[ -z $n || -n ${seen[$n]:-} ]] && continue
		[[ $n == "$main_norm" || $main_norm == "$n"/* ]] && continue
		inside=0
		for w in "${norms[@]}"; do
			[[ -z $w ]] && continue
			if [[ $n == "$w" || $n == "$w"/* ]]; then
				inside=1
				break
			fi
		done
		((inside == 1)) && continue
		seen[$n]=1
		kept+=("$n|$parent")
	done
	((${#kept[@]} == 0)) && return 0
	local line
	while IFS= read -r line; do
		printf '%s\n' "${line#*|}"
	done < <(printf '%s\n' "${kept[@]}" | LC_ALL=C sort -t '|' -k1,1)
	return 0
}

cleanup_wt_scan_roots() {
	# Echo the worktree-tracking roots to scan, one per line.
	#
	# Configured roots come first: the CLEANUP_WT_ORPHAN_ROOTS entries
	# (cleanup_wt_split_roots) when that variable is set, otherwise the default pair
	# derived from the main worktree path (the first `git worktree list --porcelain`
	# stanza, the same derivation consolidation_worktree_path uses in
	# cleanup_worktrees_actions_lib.sh): `<main>/.claude/worktrees` then `<main>-wt`.
	# Registration-derived roots (cleanup_wt_derive_scan_roots) are always appended.
	# The combined list is deduplicated by normalize_wt_path, keeping the first spelling.
	#
	# The listing is read once (`out=$(...) || rc=$?`). On a parse_worktree_list hard
	# failure nothing is derived from it: with no override no root is emitted at all,
	# rather than a bare relative path resolved against the current working directory,
	# and with an override exactly the override roots are emitted.
	# cleanup_wt_scan_records returns 0 with no record for an empty root list, so the
	# advisory records degrade to silence rather than to a misleading scan.
	# Always returns 0.
	local out rc=0 first_record main_wt="" configured="" derived=""
	out=$(parse_worktree_list) || rc=$?
	if ((rc == 0)); then
		first_record=${out%%$'\n'*}
		main_wt=${first_record%%|*}
		derived=$(cleanup_wt_derive_scan_roots "$out")
	fi
	if [[ -n ${CLEANUP_WT_ORPHAN_ROOTS:-} ]]; then
		configured=$(cleanup_wt_split_roots "$CLEANUP_WT_ORPHAN_ROOTS")
	elif [[ -n $main_wt ]]; then
		configured="${main_wt}/.claude/worktrees"$'\n'"${main_wt}-wt"
	fi
	local -A seen=()
	local root n
	while IFS= read -r root; do
		[[ -z $root ]] && continue
		n=$(normalize_wt_path "$root")
		# A root of `/` normalizes to empty, which is not a valid array key.
		[[ -z $n ]] && n=/
		[[ -n ${seen[$n]:-} ]] && continue
		seen[$n]=1
		printf '%s\n' "$root"
	done <<<"${configured}"$'\n'"${derived}"
	return 0
}
