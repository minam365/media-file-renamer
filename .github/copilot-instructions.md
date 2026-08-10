# Copilot Instructions

## General Guidelines
- First general instruction
- Second general instruction

## Code Style
- Use specific formatting rules
- Follow naming conventions

## Project-Specific Rules
- User prefers modifications in-place and to add features by updating existing options/parameters (e.g., adding a boolean flag to FileScanOptions). Ensure changes are applied across the codebase and update all call sites when adding parameters.
- Precompute counts before starting background tasks when performing scans to avoid concurrent disk access. Update call sites when changing options.