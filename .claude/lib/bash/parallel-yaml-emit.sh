#!/usr/bin/env bash
# parallel-yaml-emit.sh: structural half of the restricted block-YAML parser.
# Walks the indentation of the frontmatter body lines produced by
# parallel-yaml-scan.sh and populates that module's node table in document
# order. Sourcing this file also sources the scanning half, so a consumer needs
# only this one source line.
#
# Algorithm. A stack of open containers is kept, each carrying the indent
# column its children sit at, its rendered path, whether it is a mapping or a
# sequence, and how many children it has taken. A `key:` line with no inline
# value opens a pending node whose kind is not yet known: the next content line
# decides it, becoming a mapping or sequence when it is indented deeper, and
# leaving the pending node a null scalar when it is not. That one-line lookahead
# is the whole of the parser's ambiguity handling.
#
# Failure modes are the two the scanning half defines. A malformed line sets
# YP_STATUS=yaml_error and is rendered by the caller through the M1 message; a
# construct that is valid YAML but outside the subset sets
# YP_STATUS=out_of_subset and makes the caller refuse to answer rather than
# risk disagreeing with the Python authority.
#
# shellcheck disable=SC2034
# SC2034 is disabled file-wide because the node-table and status globals this
# module writes -- YP_TYPE, YP_COUNT, YP_STATUS -- are declared in
# parallel-yaml-scan.sh and read by parallel-items-validate.sh and
# parallel-manifest-validate.sh, which shellcheck analyses as separate files.

# Resolve this file's own directory so the scanning half sources regardless of
# the caller's working directory.
YP_EMIT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
# shellcheck source=.claude/lib/bash/parallel-yaml-scan.sh
# shellcheck disable=SC1091
source "$YP_EMIT_DIR/parallel-yaml-scan.sh"

# Path label for the document root in key-scoped errors.
YP_ROOT_LABEL="<root>"

# Open-container stack, parallel arrays indexed by depth.
YP_STACK_INDENT=()
YP_STACK_PATH=()
YP_STACK_KIND=()
YP_STACK_COUNT=()

# The pending node opened by a `key:` or bare `-` line, if any.
YP_PENDING_PATH=""
YP_PENDING_INDENT=-1
YP_PENDING_ACTIVE=0

yp_stack_reset() {
	# Clear the container stack and the pending-node slot.
	YP_STACK_INDENT=()
	YP_STACK_PATH=()
	YP_STACK_KIND=()
	YP_STACK_COUNT=()
	YP_PENDING_PATH=""
	YP_PENDING_INDENT=-1
	YP_PENDING_ACTIVE=0
}

yp_stack_push() {
	# Push one open container onto the stack.
	#
	# Args: $1 = indent column of the container's children, $2 = rendered path,
	# $3 = kind (map or seq).
	YP_STACK_INDENT+=("$1")
	YP_STACK_PATH+=("$2")
	YP_STACK_KIND+=("$3")
	YP_STACK_COUNT+=(0)
}

yp_stack_depth() {
	# Echo the number of open containers.
	printf '%s' "${#YP_STACK_INDENT[@]}"
}

yp_stack_pop() {
	# Discard the innermost open container by truncating every stack array.
	local keep=$((${#YP_STACK_INDENT[@]} - 1))
	((keep >= 0)) || return 0
	YP_STACK_INDENT=("${YP_STACK_INDENT[@]:0:keep}")
	YP_STACK_PATH=("${YP_STACK_PATH[@]:0:keep}")
	YP_STACK_KIND=("${YP_STACK_KIND[@]:0:keep}")
	YP_STACK_COUNT=("${YP_STACK_COUNT[@]:0:keep}")
}

yp_stack_bump() {
	# Increment the innermost container's child count and mirror it into the
	# node table so yp_count_of reports sequence lengths and key counts.
	local last=$((${#YP_STACK_INDENT[@]} - 1))
	((last >= 0)) || return 0
	YP_STACK_COUNT[last]=$((YP_STACK_COUNT[last] + 1))
	local path="${YP_STACK_PATH[last]}"
	if [[ -n $path ]]; then
		YP_COUNT["$path"]=${YP_STACK_COUNT[last]}
	fi
}

yp_render_child_path() {
	# Echo the path of a mapping key inside a parent container.
	#
	# Args: $1 = parent path (empty at the document root), $2 = key name.
	if [[ -z $1 ]]; then
		printf '%s' "$2"
	else
		printf '%s.%s' "$1" "$2"
	fi
}

yp_render_parent_label() {
	# Echo the path label Python uses for the mapping that owns a key.
	#
	# Args: $1 = parent path. The document root renders as `<root>` so
	# prohibited-key errors reproduce the Python wording exactly.
	if [[ -z $1 ]]; then
		printf '%s' "$YP_ROOT_LABEL"
	else
		printf '%s' "$1"
	fi
}

yp_open_pending() {
	# Mark a node as awaiting the next line to decide its kind.
	#
	# Args: $1 = node path, $2 = indent column of the line that opened it.
	YP_PENDING_PATH="$1"
	YP_PENDING_INDENT="$2"
	YP_PENDING_ACTIVE=1
}

yp_resolve_pending() {
	# Decide the kind of a pending node using the next content line.
	#
	# Args: $1 = indent of the next content line, $2 = 1 when that line is a
	# sequence entry. Returns 0 always; the pending slot is cleared.
	local indent="$1" is_seq="$2"
	((YP_PENDING_ACTIVE == 1)) || return 0
	# A deeper line, or a sequence entry at the same column, opens a container;
	# anything else means the key carried no value and reads as null.
	if ((indent > YP_PENDING_INDENT)) || ((indent == YP_PENDING_INDENT && is_seq == 1)); then
		local kind="map"
		((is_seq == 1)) && kind="seq"
		YP_TYPE["$YP_PENDING_PATH"]="$kind"
		YP_COUNT["$YP_PENDING_PATH"]=0
		yp_stack_push "$indent" "$YP_PENDING_PATH" "$kind"
	else
		YP_TYPE["$YP_PENDING_PATH"]="null"
	fi
	YP_PENDING_ACTIVE=0
	YP_PENDING_PATH=""
	YP_PENDING_INDENT=-1
}

yp_emit_map_entry() {
	# Parse one `key: value` or `key:` line into the node table.
	#
	# Args: $1 = owning mapping path (empty at the root), $2 = line content
	# with indentation stripped, $3 = indent column of the line.
	# Returns 0 on success, 1 with YP_STATUS set on a malformed or refused line.
	local parent="$1" content="$2" indent="$3"
	local key rest
	if [[ $content == *": "* ]]; then
		key=${content%%": "*}
		rest=$(yp_trim "${content#*": "}")
	elif [[ $content == *":" ]]; then
		key=${content%":"}
		rest=""
	else
		yp_yaml_error "line is neither a mapping entry nor a sequence entry: $content"
		return 1
	fi
	# Keys in a machine-authored manifest are bare identifiers; a quoted or
	# otherwise exotic key is refused rather than guessed at.
	if [[ ! $key =~ ^[A-Za-z_][A-Za-z0-9_-]*$ ]]; then
		yp_reject "mapping keys must be bare identifiers; found: $key"
		return 1
	fi
	local path parent_label
	path=$(yp_render_child_path "$parent" "$key")
	parent_label=$(yp_render_parent_label "$parent")

	# An inline empty flow collection is a complete container; an empty value
	# defers the decision to the next line; anything else is a scalar.
	if [[ $rest == "[]" ]]; then
		yp_node_add "$path" "seq" "" "$parent_label" "$key"
	elif [[ $rest == "{}" ]]; then
		yp_node_add "$path" "map" "" "$parent_label" "$key"
	elif [[ -z $rest && $content == *":" ]]; then
		yp_node_add "$path" "null" "" "$parent_label" "$key"
		yp_open_pending "$path" "$indent"
	else
		yp_classify_scalar "$rest" || return 1
		yp_node_add "$path" "$YP_SCALAR_TYPE" "$YP_SCALAR_VALUE" "$parent_label" "$key"
	fi
	yp_stack_bump
	return 0
}

yp_emit_seq_entry() {
	# Parse one `- ...` line into the node table.
	#
	# Args: $1 = owning sequence path, $2 = the sequence index to use, $3 = the
	# text after the dash with surrounding whitespace trimmed, $4 = indent
	# column of the dash. Returns 0 on success, 1 with YP_STATUS set otherwise.
	local parent="$1" index="$2" rest="$3" indent="$4"
	local path="${parent}[${index}]"

	# A bare dash defers to the next line; a `key: value` payload opens a
	# compact mapping whose entries sit two columns right of the dash; anything
	# else is a scalar element.
	if [[ -z $rest ]]; then
		yp_node_add "$path" "null" "" "" ""
		yp_open_pending "$path" "$indent"
		return 0
	fi
	if [[ $rest == *": "* || $rest == *":" ]]; then
		yp_node_add "$path" "map" "" "" ""
		yp_stack_push $((indent + 2)) "$path" "map"
		yp_emit_map_entry "$path" "$rest" $((indent + 2)) || return 1
		return 0
	fi
	yp_classify_scalar "$rest" || return 1
	yp_node_add "$path" "$YP_SCALAR_TYPE" "$YP_SCALAR_VALUE" "" ""
	return 0
}

yp_scan_line_guards() {
	# Reject the line-level constructs the subset does not model.
	#
	# Args: $1 = the raw line, $2 = the line with indentation stripped.
	# Returns 0 when the line may be parsed, 1 with YP_STATUS set otherwise.
	local line="$1" content="$2"
	if [[ $line == *$'\t'* ]]; then
		yp_yaml_error "tab characters are not permitted in YAML indentation"
		return 1
	fi
	if [[ $content == *' #'* ]]; then
		yp_reject "trailing comments are outside the subset: $content"
		return 1
	fi
	if [[ $content == '---' || $content == '...' ]]; then
		yp_reject "multi-document streams are outside the subset"
		return 1
	fi
	return 0
}

yp_close_deeper_containers() {
	# Pop every open container whose children sit right of the given column.
	#
	# Args: $1 = the current line's indent column.
	local indent="$1" top
	# Unwinding stops at the first container whose child column is at or left
	# of this line, which is the container the line belongs to.
	while ((${#YP_STACK_INDENT[@]} > 0)); do
		top=$((${#YP_STACK_INDENT[@]} - 1))
		((indent < YP_STACK_INDENT[top])) || break
		yp_stack_pop
	done
}

yp_parse_body() {
	# Parse YP_BODY_LINES into the node table.
	#
	# Returns 0 when YP_STATUS is ok. Sets YP_STATUS to not_a_mapping when the
	# body is empty or its root is a sequence, and to yaml_error or
	# out_of_subset for the failure modes the module header documents.
	yp_reset
	yp_stack_reset
	local root_seen=0
	local total=${#YP_BODY_LINES[@]}
	local index line content indent is_seq rest depth seq_index

	# Walk every body line, resolving the pending node from the previous line
	# before deciding what the current line contributes.
	for ((index = 0; index < total; index++)); do
		line="${YP_BODY_LINES[index]}"
		[[ -n $(yp_trim "$line") ]] || continue
		content=${line#"${line%%[![:space:]]*}"}
		[[ $content == '#'* ]] && continue
		yp_scan_line_guards "$line" "$content" || return 1
		indent=$((${#line} - ${#content}))
		is_seq=0
		[[ $content == "-" || $content == "- "* ]] && is_seq=1

		yp_resolve_pending "$indent" "$is_seq"

		# The first content line fixes the document's root kind; a sequence
		# root is reported as not-a-mapping rather than parsed.
		if ((root_seen == 0)); then
			root_seen=1
			if ((is_seq == 1)); then
				YP_STATUS="not_a_mapping"
				return 1
			fi
			((${#YP_STACK_INDENT[@]} > 0)) || yp_stack_push "$indent" "" "map"
		fi

		yp_close_deeper_containers "$indent"
		if ((${#YP_STACK_INDENT[@]} == 0)); then
			yp_yaml_error "indentation closes the document root: $content"
			return 1
		fi
		depth=$((${#YP_STACK_INDENT[@]} - 1))
		if ((indent != YP_STACK_INDENT[depth])); then
			yp_yaml_error "inconsistent indentation at column $indent: $content"
			return 1
		fi

		# Route the line against the container it belongs to; a mismatch means
		# the document mixes a sequence entry into a mapping or the reverse.
		if ((is_seq == 1)); then
			if [[ ${YP_STACK_KIND[depth]} != seq ]]; then
				yp_yaml_error "sequence entry inside a mapping: $content"
				return 1
			fi
			rest=$(yp_trim "${content#-}")
			seq_index="${YP_STACK_COUNT[depth]}"
			yp_stack_bump
			yp_emit_seq_entry "${YP_STACK_PATH[depth]}" "$seq_index" "$rest" "$indent" || return 1
		else
			if [[ ${YP_STACK_KIND[depth]} != map ]]; then
				yp_yaml_error "mapping entry inside a sequence: $content"
				return 1
			fi
			yp_emit_map_entry "${YP_STACK_PATH[depth]}" "$content" "$indent" || return 1
		fi
	done

	# A pending node left open at end of input carried no value.
	if ((YP_PENDING_ACTIVE == 1)); then
		YP_TYPE["$YP_PENDING_PATH"]="null"
		YP_PENDING_ACTIVE=0
	fi
	if ((root_seen == 0)); then
		YP_STATUS="not_a_mapping"
		return 1
	fi
	YP_STATUS="ok"
	return 0
}
