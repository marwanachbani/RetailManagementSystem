; RetailManagementSystem.iss
; Sprint 0 skeleton — fill in {#MyAppVersion} etc. via /D defines or update directly
; once Sprint releases start producing real builds.

#define MyAppName "Retail Management System"
#define MyAppVersion "0.1.0"
#define MyAppPublisher "Your Company"
#define MyAppExeName "RMS.WPF.exe"

[Setup]
AppId={{8C2F2C3E-4B7B-4B0E-9C9B-RMSAPP0001}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\RetailManagementSystem
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=..\..\artifacts
OutputBaseFilename=RMS-Setup-{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
; Sprint 1+: point this at the real publish output, e.g.
; ..\..\src\Desktop\RMS.WPF\bin\Release\net10.0-windows\publish\*
Source: "..\..\src\Desktop\RMS.WPF\bin\Release\net10.0-windows\publish\*"; \
  DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; \
  Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ProgramDataDir, LogsDir, BackupsDir: string;
begin
  if CurStep = ssPostInstall then
  begin
    { ProgramData\RetailManagementSystem and its subfolders are also created
      defensively here in case the app's own EnsureProgramDataFolders() has
      not yet run (e.g. installed for a different Windows user). }
    ProgramDataDir := ExpandConstant('{commonappdata}\RetailManagementSystem');
    LogsDir := ProgramDataDir + '\logs';
    BackupsDir := ProgramDataDir + '\backups';

    if not DirExists(ProgramDataDir) then
      CreateDir(ProgramDataDir);
    if not DirExists(LogsDir) then
      CreateDir(LogsDir);
    if not DirExists(BackupsDir) then
      CreateDir(BackupsDir);
  end;
end;
