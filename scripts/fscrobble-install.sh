#!/usr/bin/env bash
# ---
# Author: Ali Baghernejad
# Copyright (c) FScrobble All rights reserved.
# Under MIT  Licenced. Check out the Licence file in the Root of the Repo.
# ---

set -euo pipefail

REPO="alibaghernejad/FScrobble"
BIN_NAME="FScrobble.Shell"
INSTALL_ROOT="$HOME/.local"
INSTALL_DIR="$INSTALL_ROOT/lib/fscrobble"
BIN_LINK="$INSTALL_ROOT/bin/fscrobble"
SERVICE_DIR="$HOME/.config/systemd/user"
SERVICE_TEMPLATE_NAME="fscrobble.service"
SERVICE_FILE="$SERVICE_DIR/$SERVICE_TEMPLATE_NAME"
SERVICE_TEMPLATE_URL="https://raw.githubusercontent.com/alibaghernejad/FScrobble/main/systemd/$SERVICE_TEMPLATE_NAME"
TMP_DIR="$(mktemp -d)"
ARCHIVE_NAME="fscrobble.tar.gz"

echo "Fetching latest release info..."
API_JSON=$(wget -qO- "https://api.github.com/repos/$REPO/releases/latest")
DOWNLOAD_URL=$(echo "$API_JSON" | grep "browser_download_url" | grep "linux-x64.*tar.gz" | head -n1 | cut -d '"' -f 4)

if [ -z "$DOWNLOAD_URL" ]; then
    echo "❌ Could not find a Linux-x64 tarball in the latest release."
    exit 1
fi

echo "Found release: $DOWNLOAD_URL"

echo "Downloading release archive..."
wget -qO "$TMP_DIR/$ARCHIVE_NAME" "$DOWNLOAD_URL"

echo "Extracting to $INSTALL_DIR..."
mkdir -p "$INSTALL_DIR"
tar -xzf "$TMP_DIR/$ARCHIVE_NAME" -C "$INSTALL_DIR"

MAIN_BIN="$(find "$INSTALL_DIR" -type f -executable -name "$BIN_NAME" | head -n1)"
if [ -z "$MAIN_BIN" ]; then
    echo "❌ Could not find the main executable after extraction."
    exit 1
fi

echo "Linking $MAIN_BIN to $BIN_LINK..."
mkdir -p "$INSTALL_ROOT/bin"
ln -sf "$MAIN_BIN" "$BIN_LINK"

echo "Downloading systemd user service template..."
mkdir -p "$SERVICE_DIR"
wget -qO "$TMP_DIR/$SERVICE_TEMPLATE_NAME" "$SERVICE_TEMPLATE_URL"

echo "Installing user systemd unit at $SERVICE_FILE..."
sed \
  -e "s|{{ExecStart}}|$BIN_LINK|g" \
  -e "s|{{WorkingDirectory}}|$INSTALL_DIR|g" \
  "$TMP_DIR/$SERVICE_TEMPLATE_NAME" > "$SERVICE_FILE"

echo "Reloading user systemd daemon..."
systemctl --user daemon-reexec || true
systemctl --user daemon-reload

echo "Enabling and starting FScrobble user service..."
systemctl --user enable fscrobble
systemctl --user start fscrobble

echo "✅ FScrobble installed and running!"
echo "Check with: systemctl --user status fscrobble"
echo "Make sure ~/.local/bin is in your PATH:"
echo "   export PATH=\"\$HOME/.local/bin:\$PATH\""
