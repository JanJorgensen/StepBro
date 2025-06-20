; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "StepBro"
#define MyAppVersion "1.4.0"
#define MyAppPublisher "SchmutStein"
#define MyAppURL "http://www.schmutstein.com/"
#define MyAppExeName "StepBro.Workbench.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{4BB543B9-5D25-42B4-A044-74CA77F2EBE1}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
ChangesEnvironment=yes
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=..\LICENSE
; The following lines have been outcommented to force the user to install in administrative mode
; This is so we can add the path to the StepBro.exe file to the %PATH% variable
; PrivilegesRequired=lowest
; PrivilegesRequiredOverridesAllowed=dialog
OutputDir=Installer
OutputBaseFilename=stepbro.setup.{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
UsePreviousAppDir=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\bin\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\bin\{#MyAppExeName}"; Tasks: desktopicon

;[Run]
;Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Dirs]
Name: "{app}\bin"
Name: "{app}\bin\modules"
Name: "{app}\examples"
Name: "{app}\scripts"
Name: "{app}\scripts\smoketest"

; NOTE: Don't use "Flags: ignoreversion" on any shared system files
[Files]
Source: "bin\*"; DestDir: "{app}\bin"; Flags: ignoreversion recursesubdirs
Source: "StepBro.SimpleWorkbench\StepBro.Workbench.ico"; DestDir: "{app}\bin"; Flags: ignoreversion recursesubdirs
Source: "StepBro Station Properties.cfg"; DestDir: "{%USERPROFILE}\Documents"; Flags: onlyifdoesntexist

[Registry]
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\Session Manager\Environment"; \
    ValueType: expandsz; ValueName: "Path"; ValueData: "{olddata};{app}\bin"; \
    Check: NeedsAddPath(ExpandConstant('{app}\bin'))

Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\Session Manager\Environment"; \
    ValueType:string; ValueName: "STEPBRO_STATION_PROPERTIES"; \
    ValueData: "{%USERPROFILE}\Documents\StepBro Station Properties.cfg"; Flags: preservestringtype; \
    Check: NeedsAddEnvironmentVariable('STEPBRO_STATION_PROPERTIES');

; Add 'My PDF Editor' menu item to the Shell menu for PDF files:
Root: "HKCR"; Subkey: "SystemFileAssociations\.sbs\shell\Open with StepBro Workbench"; ValueType: none; ValueName: ""; ValueData: ""; Flags: uninsdeletekey
; Specify icon for the menu item:
Root: "HKCR"; Subkey: "SystemFileAssociations\.sbs\shell\Open with StepBro Workbench"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\bin\StepBro.Workbench.ico"; Flags: uninsdeletekey
; Define command for the menu item:
Root: "HKCR"; Subkey: "SystemFileAssociations\.sbs\shell\Open with StepBro Workbench\command"; ValueType: string; ValueName: ""; ValueData: """{app}\bin\{#MyAppExeName}""  ""%1"""; Flags: uninsdeletekey
    
[Code]
function NeedsAddPath(Param: string): boolean;
var
  OrigPath: string;
begin
  if not RegQueryStringValue(HKEY_LOCAL_MACHINE,
    'SYSTEM\CurrentControlSet\Control\Session Manager\Environment',
    'Path', OrigPath)
  then begin
    Result := True;
    exit;
  end;
  { look for the path with leading and trailing semicolon }
  { Pos() returns 0 if not found }
  Result := Pos(';' + Param + ';', ';' + OrigPath + ';') = 0;
end;

function NeedsAddEnvironmentVariable(Param: string): boolean;
begin
  Result := GetEnv(Param) = '';
end;