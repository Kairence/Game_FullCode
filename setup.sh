#!/bin/bash
set -e

echo "=========================================="
echo " Code Quality Tools Setup (macOS/Linux)"
echo "=========================================="
echo ""

# 1. Check dotnet is available
echo "[1/4] Checking .NET SDK..."
if ! command -v dotnet &> /dev/null; then
    # Try Homebrew dotnet@8 path
    if [ -f "/opt/homebrew/opt/dotnet@8/bin/dotnet" ]; then
        export PATH="/opt/homebrew/opt/dotnet@8/bin:$PATH"
        export DOTNET_ROOT="/opt/homebrew/opt/dotnet@8/libexec"
        echo "  -> Found dotnet@8 via Homebrew"
    elif [ -f "/usr/local/opt/dotnet@8/bin/dotnet" ]; then
        export PATH="/usr/local/opt/dotnet@8/bin:$PATH"
        export DOTNET_ROOT="/usr/local/opt/dotnet@8/libexec"
        echo "  -> Found dotnet@8 via Homebrew (Intel Mac)"
    else
        echo "[!] .NET SDK not found. Install with:"
        echo "    brew install dotnet@8"
        exit 1
    fi
fi
echo "  .NET SDK: $(dotnet --version)"
echo ""

# 2. Restore dotnet local tools (CSharpier, Husky)
echo "[2/4] Restoring dotnet tools..."
dotnet tool restore
echo ""

# 3. Install Husky git hooks
echo "[3/4] Installing Husky git hooks..."
dotnet husky install
echo ""

# 4. Verify
echo "[4/4] Verifying installation..."
dotnet tool run dotnet-csharpier --version
echo ""

echo "=========================================="
echo " Setup Complete!"
echo "=========================================="
echo ""
echo "Available commands:"
echo "  dotnet csharpier .          - Format all C# files"
echo "  dotnet csharpier --check .  - Check formatting (no changes)"
echo "  dotnet format               - Apply .editorconfig rules"
echo "  dotnet format --verify-no-changes  - Check style (no changes)"
echo ""
echo "TIP: Add to your ~/.zshrc for persistent PATH:"
echo "  export PATH=\"/opt/homebrew/opt/dotnet@8/bin:\$PATH\""
echo "  export DOTNET_ROOT=\"/opt/homebrew/opt/dotnet@8/libexec\""
