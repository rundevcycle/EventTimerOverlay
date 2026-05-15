# Copilot Instructions

## Purpose
This file tells Copilot how to behave when modifying or creating files in this repository.

## Workspace
- Targets: `.NET 10`
- IDE: Visual Studio (editors with open files, integrated unit test support, output pane, integrated terminal).

## User preferences
- When returning file changes, show only the relevant changes (hunks) with a few lines of context instead of the entire file.
- When the coding language is unclear, prefer `C#`.
- Keep responses short and impersonal.
- Use `markdown` for documentation and `mermaid` for diagrams.
- Don't guess.  If you are uncertain, check the files ore ask for additional input.
- In XML files, attributes should be nested 4 spaces under its parent, with only 1 attribute per line.
- Try to limit line length to at most 140 characters where possible.


## Coding conventions
- Follow repository `.editorconfig` if present; otherwise follow standard Microsoft C# conventions.
- Enable and respect nullable reference types where the project does.
- Prefer async APIs and include `CancellationToken` in public async methods.
- Use expression-bodied members for short members where appropriate.
- Keep line length reasonable (approx. 100–120 chars).
- Always wrap the bodies of conditional or loop blocks in braces, even if it's a single line.

## Tests
- Prefer unit tests with `NUnit` unless the repo already uses another framework.
- Keep tests fast and deterministic.
- Add or update tests when changing behavior.

## Commits & PRs
- Create small, focused diffs.
- Write imperative, present-tense commit messages (e.g., "Add validation for X").

## Security & secrets
- Never add secrets, credentials, or private keys to the repo.
- Use configuration or secret stores for runtime secrets.
- Ensure `.gitignore` excludes user-specific files (e.g., `.vs/`, `*.user`, `secrets.json`).

## Output & examples
- When showing code examples, include the language name and file path in the code block header.
- Use backticks to refer to files, directories, functions, and classes in prose (e.g., `Program.cs`, `MyService`).

## When modifying files
- Show only the changed hunks with a few lines of context.
- If creating a new file, include the full new file content.
- Keep changes minimal and focused to satisfy the user's request.

## Diagrams
- Use `mermaid` syntax for diagrams.
- If creating a `sequenceDiagram`, escape `#` as `#35;`.
- For non-sequence diagrams, wrap node text in double quotes and escape internal quotes as `&quot;`.

## Agent identity
- This file documents behavior only; do not include personal or irrelevant information.
