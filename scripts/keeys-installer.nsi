# Define installer name and output
OutFile "KeeysInstaller.exe"

# Define installation directory
InstallDir "$PROGRAMFILES\Keeys"

# Define default section
Section "Install"

  # Create installation directory
  SetOutPath "$INSTDIR"

  # Copy all files to installation directory
  File /r "${GITHUB_WORKSPACE}/output/windows/*"

  # Add installation directory to PATH
  WriteEnvStr "PATH" "$PATH;$INSTDIR"

SectionEnd

# Define uninstaller
Section "Uninstall"

  # Remove files
  Delete "$INSTDIR\*.*"

  # Remove installation directory
  RMDir "$INSTDIR"

  # Remove PATH entry
  ReadEnvStr $0 "PATH"
  StrReplace $0 $0 "$INSTDIR;" ""
  WriteEnvStr "PATH" $0

SectionEnd
