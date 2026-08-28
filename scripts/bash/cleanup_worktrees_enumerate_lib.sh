#!/usr/bin/env bash
# cleanup_worktrees_enumerate_lib.sh: sourceable enumeration/protection function
# group for the cleanup-worktrees tool, split out of cleanup_worktrees_lib.sh to keep
# every file within the 500-line cap. Provides the git-binary test seam
# (cleanup_wt_git), branch/worktree enumeration (enumerate_branches,
# parse_worktree_list), path normalization (normalize_wt_path), the
# current-worktree/branch protection set (compute_protected), and the main-freshness
# warning (check_main_freshness). The classification ladder, consolidation, deletion,
# and the CLI live in sibling files (cleanup_worktrees_lib.sh,
# cleanup_worktrees_actions_lib.sh, cleanup-worktrees.sh).
#
# Sourcing contract: this library defines functions only; it never runs work at
# source time, so the wrapper and the bats suites can source it without side effects.
# It MUST be sourced before cleanup_worktrees_lib.sh, whose classification functions
# call cleanup_wt_git, parse_worktree_list, compute_protected, and normalize_wt_path
# defined here.
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
# HEAD` and `git rev-parse --show-toplevel`; a hard failure of either is fatal (returns
# git's exit code), never a weakened protection fallback. The detached-HEAD case
# (rev-parse succeeds printing HEAD) is unaffected.

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
