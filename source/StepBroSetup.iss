; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "StepBro"
#define MyAppVersion "0.1.31"
#define MyAppPublisher "SchmutStein"
#define MyAppURL "http://www.schmutstein.com/"
#define MyAppExeName "StepBro.Workbench.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{4059DAF1-2A04-4820-A4A0-913E6577DAD2}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=..\LICENSE
; Remove the following line to run in administrative install mode (install for all users.)
PrivilegesRequired=lowest
OutputDir=Installer
OutputBaseFilename=stepbro.setup.{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Dirs]
Name: "{app}\modules"
Name: "{app}\examples"
Name: "{app}\scripts"

; NOTE: Don't use "Flags: ignoreversion" on any shared system files
[Files]
Source: "bin\stepbro.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\StepBro.Workbench.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\StepBro.Workbench.WPF.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\StepBro.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Antlr4.Runtime.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\CommandLine.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\FastColoredTextBox.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\WeifenLuo.WinFormsUI.Docking.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\WeifenLuo.WinFormsUI.Docking.ThemeVS2015.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\System.AppContext.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\System.Buffers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\System.Console.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\System.Diagnostics.DiagnosticSource.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\System.Globalization.Calendars.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\System.IO.Compression.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\System.IO.Compression.ZipFile.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\System.IO.FileSystem.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\System.IO.FileSystem.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\System.Memory.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\System.Net.Http.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\System.Net.Sockets.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\System.Runtime.InteropServices.RuntimeInformation.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\System.Security.Cryptography.Algorithms.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\System.Security.Cryptography.Encoding.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\System.Security.Cryptography.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\System.Security.Cryptography.X509Certificates.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\System.Threading.Tasks.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\ActiproSoftware.DataGrid.Contrib.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\ActiproSoftware.Docking.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\ActiproSoftware.Editors.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\ActiproSoftware.Grids.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\ActiproSoftware.Shared.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\ActiproSoftware.SyntaxEditor.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\ActiproSoftware.Text.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\modules\PCANBasic.dll"; DestDir: "{app}\modules"; Flags: ignoreversion
Source: "bin\modules\StepBro.CAN.dll"; DestDir: "{app}\modules"; Flags: ignoreversion
Source: "bin\modules\StepBro.Process.dll"; DestDir: "{app}\modules"; Flags: ignoreversion
Source: "bin\modules\StepBro.Streams.dll"; DestDir: "{app}\modules"; Flags: ignoreversion
Source: "bin\modules\Stepbro.TestInterface.dll"; DestDir: "{app}\modules"; Flags: ignoreversion
Source: "..\examples\scripts\Demo Conditional Statements.sbs"; DestDir: "{app}\examples\scripts"
Source: "..\examples\scripts\Demo Procedure.sbs"; DestDir: "{app}\examples\scripts"
Source: "..\examples\scripts\Demo Workbench.sbs"; DestDir: "{app}\examples\scripts"
Source: "..\scripts\*.*"; DestDir: "{app}\scripts"
