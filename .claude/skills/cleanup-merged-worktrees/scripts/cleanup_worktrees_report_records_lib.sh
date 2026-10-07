#!/usr/bin/env bash
# cleanup_worktrees_report_records_lib.sh: sourceable report-record function group for
# the cleanup-worktrees tool. It adds the report-mode visibility records that the
# classification ladder cannot see on its own: filesystem directories that are no
# longer registered worktrees, remote-tracking refs whose remote is gone, worktrees
# whose gitdir pointer no longer resolves, and the ancestor relationship between two
# branches that both resolve NOT_MERGED. The enumeration/protection seam
# (cleanup_wt_git, enumerate_branches, parse_worktree_list, normalize_wt_path,
# compute_protected, check_main_freshness, cleanup_wt_scan_roots) lives in
# cleanup_worktrees_enumerate_lib.sh and the classification ladder (classify_branch)
# lives in cleanup_worktrees_lib.sh; both MUST be sourced before this file. Keeping
# these records in a sibling file keeps every file within the 500-line cap.
#
# Sourcing contract: this library defines functions only; it never runs work at source
# time, so the wrapper and the bats suites can source it without side effects.
#
# Report line contract added by this library (pipe-delimited, LC_ALL=C ordered, one
# record per line; all four are advisory/read-only and never unlock a destructive
# action):
#   ORPHAN_DIR|<path>|<size>                 a directory under a worktree-tracking root
#                                            with no `.git` file and no registration.
#                                            <size> is best-effort and may be the
#                                            literal `unknown`.
#   STALE_REF|<refname>                      a refs/remotes/<name>/* ref whose <name> is
#                                            not a configured remote. <refname> is the
#                                            full ref form (refs/remotes/...).
#   CHILD_OF|<branch>|<ancestor>             advisory, informational: <branch> is a git
#                                            ancestor of <ancestor>, and BOTH resolved
#                                            exactly NOT_MERGED through their own
#                                            unchanged ladders. Emitted alongside, never
#                                            instead of, the branch's own BRANCH| line.
#                                            It names a containment relationship only; no
#                                            ladder rung is skipped for either branch and
#                                            neither verdict is derived from the other.
#   WARN|registration-lost|<path>            a worktree directory whose `.git` gitdir
#                                            pointer names a target that does not exist.
#
# Filesystem-scan seam: every filesystem read goes through cleanup_wt_scan_bin so the
# bats suites can stub the scan deterministically via CLEANUP_WT_SCAN_BIN, mirroring
# the CLEANUP_WT_GIT_BIN seam in cleanup_worktrees_enumerate_lib.sh. Unlike that seam,
# the fallback target is the bundled real implementation
# .claude/skills/cleanup-merged-worktrees/scripts/cleanup_worktrees_scan_helper.sh rather than a PATH lookup, because no
# single standard system binary emits the combined
# `path|has_gitfile|gitdir_target_exists|size` tuple these records need.

cleanup_wt_scan_bin() {
	# Resolve the filesystem-scan helper honoring the CLEANUP_WT_SCAN_BIN override seam
	# and echo its path; the caller invokes it.
	#
	# When CLEANUP_WT_SCAN_BIN is set to a non-empty value it must point to an existing
	# executable; an empty or nonexistent value is treated as missing and falls back to
	# the bundled cleanup_worktrees_scan_helper.sh alongside this library.
	#
	# Args: none. Always returns 0 and echoes exactly one path.
	local override=${CLEANUP_WT_SCAN_BIN:-}
	if [[ -n $override && -x $override ]]; then
		printf '%s\n' "$override"
		return 0
	fi
	local here
	here=$(dirname "${BASH_SOURCE[0]}")
	printf '%s\n' "$here/cleanup_worktrees_scan_helper.sh"
}

scan_stale_refs() {
	# Emit STALE_REF|<refname> for every remote-tracking ref whose remote is gone.
	#
	# A ref under refs/remotes/<name>/ belongs to the configured remote <name>. When
	# <name> is absent from `git remote`'s output the ref is a leftover: the remote was
	# removed (or was never configured in this checkout) but its tracking refs were not
	# pruned. The record is advisory and read-only; nothing here deletes a ref.
	#
	# The refname is emitted in full ref form (refs/remotes/<name>/<branch>) rather than
	# the :short form, so the record names the ref exactly as `git for-each-ref` and
	# `git update-ref` spell it. Output is LC_ALL=C sorted.
	#
	# Both git reads are captured in the PARENT shell (`out=$(...) || rc=$?`) so a
	# non-zero exit is observed here rather than lost inside a process substitution. On a
	# hard failure of either read this emits no STALE_REF line and returns git's non-zero
	# exit code, mirroring parse_worktree_list's hard-failure contract: a git failure must
	# never read as "no stale refs".
	local rc=0 refs_out remotes_out ref name remote
	refs_out=$(cleanup_wt_git for-each-ref --format='%(refname)' refs/remotes/) || rc=$?
	if ((rc != 0)); then
		printf 'cleanup-worktrees: git for-each-ref refs/remotes/ failed (rc=%s)\n' "$rc" >&2
		return "$rc"
	fi
	remotes_out=$(cleanup_wt_git remote) || rc=$?
	if ((rc != 0)); then
		printf 'cleanup-worktrees: git remote failed (rc=%s)\n' "$rc" >&2
		return "$rc"
	fi
	local -A configured=()
	while IFS= read -r remote; do
		[[ -z $remote ]] && continue
		configured[$remote]=1
	done <<<"$remotes_out"
	local -a stale=()
	while IFS= read -r ref; do
		[[ -z $ref ]] && continue
		[[ $ref == refs/remotes/* ]] || continue
		name=${ref#refs/remotes/}
		name=${name%%/*}
		[[ -z $name ]] && continue
		[[ -n ${configured[$name]:-} ]] && continue
		stale+=("STALE_REF|$ref")
	done <<<"$refs_out"
	if ((${#stale[@]} > 0)); then
		printf '%s\n' "${stale[@]}" | LC_ALL=C sort
	fi
	return 0
}

cleanup_wt_scan_records() {
	# Echo the raw `<path>|<has_gitfile>|<gitdir_target_exists>|<size>` records for every
	# candidate directory under the resolved scan roots, one per line. The roots come
	# from cleanup_wt_scan_roots in cleanup_worktrees_enumerate_lib.sh.
	#
	# Every filesystem read goes through the CLEANUP_WT_SCAN_BIN seam, so the bats suites
	# replay canned records instead of touching the real filesystem. The scan output is
	# captured in the PARENT shell (`out=$(...) || rc=$?`) so a non-zero exit is observed
	# here; on a hard failure this emits no record and returns the scan's exit code
	# rather than letting a failed scan read as "nothing found".
	local -a roots=()
	local root
	while IFS= read -r root; do
		[[ -z $root ]] && continue
		roots+=("$root")
	done < <(cleanup_wt_scan_roots)
	if ((${#roots[@]} == 0)); then
		return 0
	fi
	local bin out rc=0
	bin=$(cleanup_wt_scan_bin)
	# Every script under scripts/bash/ is tracked mode 100644 and is invoked through the
	# interpreter rather than by its exec bit, so the bundled fallback helper is run with
	# `bash` when it is not executable. An override supplied through CLEANUP_WT_SCAN_BIN
	# is always executable (cleanup_wt_scan_bin only accepts it when it is) and is run
	# directly, so an override may be any executable, not only a bash script.
	if [[ -x $bin ]]; then
		out=$("$bin" scan-dirs "${roots[@]}") || rc=$?
	else
		out=$(bash "$bin" scan-dirs "${roots[@]}") || rc=$?
	fi
	if ((rc != 0)); then
		printf 'cleanup-worktrees: filesystem scan failed (rc=%s)\n' "$rc" >&2
		return "$rc"
	fi
	if [[ -n $out ]]; then
		printf '%s\n' "$out"
	fi
	return 0
}

scan_orphan_dirs() {
	# Emit ORPHAN_DIR|<path>|<size> for every scanned directory that is no longer a
	# worktree: it carries no `.git` pointer file and no `git worktree list` entry names
	# it. These are the residue of a `git worktree remove` that partially ran or failed.
	# The record is advisory and read-only; nothing here deletes a directory.
	#
	# Registered paths are compared after normalize_wt_path normalization, so a
	# backslash/case/trailing-slash difference between the porcelain listing and the
	# filesystem scan cannot make a live worktree look orphaned. Output is LC_ALL=C
	# sorted. A record whose size field is the literal `unknown` is emitted verbatim
	# rather than dropped: size is best-effort and its absence is not a reason to hide a
	# real orphan.
	#
	# Args: $1 = OPTIONAL pre-scanned records, in cleanup_wt_scan_records' output shape.
	# When an argument is supplied it is used verbatim and no scan is performed, so a
	# caller that already scanned (run_report_scans) does not scan a second time. The test
	# is on the ARGUMENT COUNT, not on the value, so "records supplied and empty" is
	# distinguishable from "no records supplied": an empty supplied value means the scan
	# found nothing and this function emits nothing, while no argument at all means the
	# function scans for itself. Direct callers may therefore keep calling it with no
	# argument.
	#
	# Returns the scan's exit code on a hard scan failure (no record emitted), else 0.
	local rc=0 recs wlout wlrc=0 record path has_gitfile size norm
	if (($# > 0)); then
		recs="$1"
	else
		recs=$(cleanup_wt_scan_records) || rc=$?
		if ((rc != 0)); then
			return "$rc"
		fi
	fi
	if [[ -z $recs ]]; then
		return 0
	fi
	local -A registered=()
	wlout=$(parse_worktree_list) || wlrc=$?
	if ((wlrc == 0)); then
		while IFS= read -r record; do
			[[ -z $record ]] && continue
			path=${record%%|*}
			norm=$(normalize_wt_path "$path")
			[[ -n $norm ]] && registered[$norm]=1
		done <<<"$wlout"
	fi
	local -a orphans=()
	while IFS= read -r record; do
		[[ -z $record ]] && continue
		IFS='|' read -r path has_gitfile _ size <<<"$record"
		[[ -z $path ]] && continue
		[[ $has_gitfile == "0" ]] || continue
		norm=$(normalize_wt_path "$path")
		[[ -n ${registered[$norm]:-} ]] && continue
		[[ -z $size ]] && size="unknown"
		orphans+=("ORPHAN_DIR|$path|$size")
	done <<<"$recs"
	if ((${#orphans[@]} > 0)); then
		printf '%s\n' "${orphans[@]}" | LC_ALL=C sort
	fi
	return 0
}

scan_registration_loss() {
	# Emit WARN|registration-lost|<path> for every scanned directory that still carries a
	# `.git` pointer file whose gitdir target no longer exists.
	#
	# This is the half-removed shape: the worktree still looks like a worktree from the
	# outside, but the administrative `.git/worktrees/<name>` entry it points at is gone,
	# so git commands run inside it fail in confusing ways. The record is advisory and
	# read-only; nothing here deletes a directory or prunes a registration.
	#
	# It consumes the same scan output and the same root resolution as scan_orphan_dirs,
	# and the mechanism that makes that true is the caller: run_report_scans performs one
	# cleanup_wt_scan_records call and passes the SAME captured records to both functions,
	# so the two records are always derived from one consistent view of the filesystem. A
	# direct call with no argument scans for itself instead.
	# Output is LC_ALL=C sorted. A record whose shape is unexpected (an empty path or a
	# gitdir_target_exists field that is neither 0 nor 1, such as the NA a pointer-less
	# directory carries) is skipped silently rather than reported or raised, mirroring
	# check_main_freshness's never-blocking contract: this warning must never be the
	# reason a report fails.
	#
	# Args: $1 = OPTIONAL pre-scanned records, in cleanup_wt_scan_records' output shape.
	# The test is on the ARGUMENT COUNT, not on the value, so "records supplied and empty"
	# is distinguishable from "no records supplied".
	#
	# Returns the scan's exit code on a hard scan failure (no record emitted), else 0.
	local rc=0 recs record path has_gitfile target_exists
	if (($# > 0)); then
		recs="$1"
	else
		recs=$(cleanup_wt_scan_records) || rc=$?
		if ((rc != 0)); then
			return "$rc"
		fi
	fi
	if [[ -z $recs ]]; then
		return 0
	fi
	local -a lost=()
	while IFS= read -r record; do
		[[ -z $record ]] && continue
		IFS='|' read -r path has_gitfile target_exists _ <<<"$record"
		[[ -z $path ]] && continue
		[[ $has_gitfile == "1" ]] || continue
		[[ $target_exists == "0" ]] || continue
		lost+=("WARN|registration-lost|$path")
	done <<<"$recs"
	if ((${#lost[@]} > 0)); then
		printf '%s\n' "${lost[@]}" | LC_ALL=C sort
	fi
	return 0
}

run_report_scans() {
	# The sole report-mode entry point for the three advisory scans. It exists so that
	# exactly ONE filesystem scan — and therefore exactly one `du` pass — occurs per
	# report: scan_orphan_dirs and scan_registration_loss each scan for themselves when
	# called directly, so a caller that invoked both would scan the tree twice and could
	# derive its two records from two different views of it.
	#
	# Emission order is the documented one: STALE_REF, then ORPHAN_DIR, then
	# WARN|registration-lost. scan_stale_refs consumes no scan records, so it runs in both
	# branches below.
	#
	# Guarded parent-shell capture: a non-zero cleanup_wt_scan_records exit is observed
	# here, emits no scan-derived record, and is returned, mirroring the hard-failure
	# contract the two record functions already have.
	#
	# Args: none. Returns the maximum non-zero return code observed, else 0.
	local rc=0 srrc=0 recs scanrc=0 orc=0 lrc=0
	recs=$(cleanup_wt_scan_records) || scanrc=$?
	if ((scanrc != 0)); then
		scan_stale_refs || srrc=$?
		if ((srrc > scanrc)); then
			return "$srrc"
		fi
		return "$scanrc"
	fi
	scan_stale_refs || srrc=$?
	if ((srrc > rc)); then
		rc=$srrc
	fi
	scan_orphan_dirs "$recs" || orc=$?
	if ((orc > rc)); then
		rc=$orc
	fi
	scan_registration_loss "$recs" || lrc=$?
	if ((lrc > rc)); then
		rc=$lrc
	fi
	return "$rc"
}

classify_all_branches() {
	# Shared classification driver: classify every enumerated branch once through the
	# unchanged per-branch ladder, emit exactly the BRANCH|/COMMIT| lines that ladder
	# produced, add the advisory CHILD_OF| record where one applies, and return the
	# maximum per-branch return code observed.
	#
	# NO LADDER RUNG IS EVER SKIPPED, and no branch's verdict is ever derived from another
	# branch's verdict. Ancestry determines a branch's state in neither direction:
	#
	#   - At rung 2, a branch merged into main by a merge commit is an ancestor of main
	#     and therefore of every branch cut from a main that already contains it, while
	#     resolving MERGED_CLEAN itself.
	#   - At rung 3, a branch whose net diff against main is empty resolves
	#     MERGED_CONTENT_NEUTRAL while still being an ancestor of an unmerged branch.
	#   - At rung 5, classify_residual_commit decides each residual commit by comparing
	#     the BRANCH TIP's blob against main's. Two branches in an ancestor relationship
	#     have different tips, so the same residual commit can resolve CONTENT_ON_MAIN for
	#     one and UNIQUE for the other. That rung is the last before the verdict and
	#     cannot be derived from any ancestor's result under any topology.
	#
	# There is no sound cut point, so there is no cut. CHILD_OF is consequently an
	# informational record about a containment relationship, not a licence to skip work,
	# and the outcome-preservation invariant holds by construction: the BRANCH| line this
	# function emits is the byte-identical line classify_branch produced.
	#
	# Two-phase contract:
	#
	#   1. Classification. Every enumerated branch is classified by one classify_branch
	#      call. Its captured output is recorded verbatim and its state is read from the
	#      first BRANCH| line's third field. rc rises to the maximum return observed.
	#
	#   2. Pairwise ancestry, restricted to the NOT_MERGED set. For every ordered pair
	#      (X, Y) with X != Y where BOTH X and Y recorded exactly NOT_MERGED, probe
	#      `git merge-base --is-ancestor <X> <Y>` with the exit code captured via
	#      `|| mrc=$?`. Exit 0 means X is an ancestor of Y and appends CHILD_OF|X|Y to X's
	#      recorded lines, for the LC_ALL=C-first such Y so the record is deterministic
	#      when X has several NOT_MERGED ancestor-targets. Exit 1 means "not an ancestor".
	#      Any exit above 1 is a hard git failure: it emits no CHILD_OF record for that
	#      pair, leaves X's already-correct BRANCH| line exactly as classify_branch
	#      produced it — overwriting it would itself break the invariant above — and
	#      raises rc to at least 2 so the failure surfaces in the exit status instead of
	#      degrading silently to "not an ancestor". A hard failure of the ladder's OWN
	#      rung-2 probe is a separate, unaffected case that classify_branch still maps to
	#      BRANCH|<name>|ANCESTRY_ERROR.
	#
	# Restricting the probe to the NOT_MERGED set bounds its cost at k*(k-1) probes for k
	# such branches rather than n*(n-1) for n branches. The base branch `main` never
	# enters the probe set: compute_protected protects it by name (CLEANUP_WT_BASE_BRANCH)
	# in every checkout topology, so classify_branch resolves it PROTECTED_CURRENT at rung 1.
	#
	# Output order is enumerate_branches' original LC_ALL=C order, regardless of the
	# order in which the phases above resolved each branch, so the report is
	# deterministic. An enumerate_branches hard failure aborts before any line is emitted
	# and returns git's exit code.
	local rc=0 crc ebout ebrc=0 cbout
	ebout=$(enumerate_branches) || ebrc=$?
	if ((ebrc != 0)); then
		return "$ebrc"
	fi
	local -a order=()
	local name
	while read -r name _; do
		[[ -z $name ]] && continue
		order+=("$name")
	done <<<"$ebout"
	if ((${#order[@]} == 0)); then
		return 0
	fi
	local -A branch_state=() branch_out=()
	local x y mrc
	# Phase 1: the unchanged full ladder, once for every branch. No rung is skipped and
	# no verdict is substituted, so each recorded line is the ladder's own.
	for x in "${order[@]}"; do
		crc=0
		cbout=$(classify_branch "$x") || crc=$?
		branch_out[$x]=$cbout
		branch_state[$x]=$(printf '%s\n' "$cbout" | awk -F'|' '/^BRANCH\|/{print $3; exit}')
		if ((crc > rc)); then
			rc=$crc
		fi
	done
	# Phase 2: pairwise ancestry, over the NOT_MERGED set only. A branch that resolved
	# anything else is neither a subject nor a target, so `main`, PROTECTED_CURRENT by the
	# unconditional base-branch protection in compute_protected, is excluded by its verdict.
	local -a not_merged=()
	for x in "${order[@]}"; do
		if [[ ${branch_state[$x]:-} == "NOT_MERGED" ]]; then
			not_merged+=("$x")
		fi
	done
	if ((${#not_merged[@]} > 1)); then
		local sorted hit
		local -a probe=()
		# LC_ALL=C order makes the recorded ancestor-target deterministic when a branch
		# has more than one NOT_MERGED ancestor-target.
		sorted=$(printf '%s\n' "${not_merged[@]}" | LC_ALL=C sort)
		while IFS= read -r name; do
			if [[ -n $name ]]; then
				probe+=("$name")
			fi
		done <<<"$sorted"
		for x in "${probe[@]}"; do
			hit=""
			for y in "${probe[@]}"; do
				if [[ $x == "$y" ]]; then
					continue
				fi
				mrc=0
				cleanup_wt_git merge-base --is-ancestor "$x" "$y" >/dev/null 2>&1 || mrc=$?
				if ((mrc == 0)); then
					hit=$y
					break
				fi
				# A hard probe failure emits no record for this pair and never rewrites
				# the branch's own verdict; it surfaces in the return code instead.
				if ((mrc > 1)) && ((rc < 2)); then
					rc=2
				fi
			done
			if [[ -n $hit ]]; then
				branch_out[$x]="${branch_out[$x]:-}"$'\n'"CHILD_OF|$x|$hit"
			fi
		done
	fi
	for x in "${order[@]}"; do
		if [[ -n ${branch_out[$x]:-} ]]; then
			printf '%s\n' "${branch_out[$x]}"
		fi
	done
	return "$rc"
}
