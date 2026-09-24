# Kairence UO (ServUO) Development Rules

## 1. Gump HTML Formatting
When generating HTML strings for Gumps (e.g., using `AddHtml`), **DO NOT use single quotes (`'`) for attribute values** like colors. The custom ServUO Gump HTML parser often fails to parse single quotes and will default to black text or break the formatting.
- **BAD (Breaks Parser):** `<BASEFONT COLOR='#FFFF00'>`
- **GOOD (Parses Correctly):** `<BASEFONT COLOR=#FFFF00>`
- **GOOD (Parses Correctly):** `<BASEFONT COLOR="#FFFF00">`

## 2. Backup File Extensions
When creating backups of C# script files (`.cs`) within the `Scripts` directory, **NEVER leave the extension as `.cs`** (e.g., `filename_backup.cs`).
The ServUO compiler automatically compiles every `.cs` file in the Scripts folder on startup. Having two files with the same class definitions will cause a `Duplicate Type` fatal crash.
- **BAD:** `RespawnCore_Backup.cs`
- **GOOD:** `RespawnCore_Backup.bak` or `RespawnCore_Backup.txt`

## 3. Code Deletion Policy
**NEVER delete existing code/features without explicit user confirmation.**
If a task requires removing existing logic, functions, or significant blocks of code, you must STOP and ask the user for permission before proceeding with the deletion.

## 4. Compilation & Syntax Check Policy
**NEVER run `dotnet build`** to check for syntax or compilation errors.
Running `dotnet build` overwrites the user's Release mode core assemblies with Debug ones and generates hundreds of lines of MSBuild warnings, which wastes context tokens.
Instead, **always run `.\ServUO.exe`** (in the background, briefly) to check the internal Roslyn script compilation log. It is much faster, preserves core files, and consumes significantly fewer tokens.
If you MUST modify the server core files (not the Scripts folder) and need to recompile the core, run the `Compile.WIN - Release.bat` script in the root directory instead of using dotnet directly.

## 5. DokuWiki Output Formatting
When generating text for the user to copy-paste into their DokuWiki, always format it as simply and concisely as possible.
- Do NOT include verbose explanations, internal design logic, or developer notes (e.g., "Gap 50", "200 달성해도 100% 성공 불가") in the final wiki text.
- Provide only the direct facts, stats, and item names that players need to see.
- Use standard DokuWiki syntax (e.g., `====== H1 ======`, `===== H2 =====`, `==== H3 ====`, `  *` for lists).

## 6. Native Tools Policy (Avoid rrun_command)
When reading or editing files, **DO NOT use terminal commands or Python scripts** via 
run_command (e.g., cat, grep, python patch.py) unless absolutely necessary.

run_command triggers IDE security prompts and wastes tokens.
Instead, **ALWAYS use native agentic tools**: view_file, grep_search, find_by_name, list_dir for reading, and 
eplace_file_content / write_to_file for editing. Reserve 
run_command exclusively for compiling, building, or running the game server.

## 7. Strict Script Execution Policy
When the user instructs you to run a specific script (e.g., a .bat or .sh file) or follow a manual procedure, **DO NOT add your own arbitrary commands or interventions** (like git commit --amend, git push --force, or overriding inputs).
- Execute **exactly** what the user requested.
- If a script requires input or interaction, do not bypass it with dummy data; either follow the user's explicit instructions for the input, or ask the user how to proceed.
- Over-correction and unrequested automated commands often lead to fatal synchronization errors or disrupt the user's local workspace.
