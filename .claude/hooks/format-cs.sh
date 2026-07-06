#!/usr/bin/env bash
# .claude/hooks/format-cs.sh
# Runs `dotnet format` on a single .cs file after Claude edits it.

INPUT=$(cat)                                          # read the event JSON from stdin

# Extract tool_input.file_path without depending on jq.
# Prefer jq if present; otherwise fall back to a portable grep/sed.
if command -v jq >/dev/null 2>&1; then
  FILE=$(printf '%s' "$INPUT" | jq -r '.tool_input.file_path // empty')
else
  FILE=$(printf '%s' "$INPUT" | grep -o '"file_path"[[:space:]]*:[[:space:]]*"[^"]*"' | head -n1 | sed 's/.*"file_path"[[:space:]]*:[[:space:]]*"//; s/"$//')
fi

# Bail quietly if there's no path or the file doesn't exist
[[ -z "$FILE" || ! -f "$FILE" ]] && exit 0

# Only format C# files; ignore everything else
case "$FILE" in
  *.cs) ;;
  *) exit 0 ;;
esac

# `dotnet format --include` matches paths RELATIVE to the workspace, not
# absolute ones. Claude passes file_path as an absolute path, so we must both
# cd into the project dir and strip the prefix, or dotnet formats 0 files.
cd "$CLAUDE_PROJECT_DIR" || exit 0
REL="${FILE#"$CLAUDE_PROJECT_DIR"/}"

dotnet format "$CLAUDE_PROJECT_DIR" --include "$REL" 2>/dev/null

exit 0