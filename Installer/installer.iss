; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Work Timer"
#define MyAppVersion "4.2.0"
#define MyAppExeName "WorkTimer4.exe"
#define PublishPath "..\publish"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{AA4E82FC-04B6-4D85-BFE5-65E2D5503BAD}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
VersionInfoVersion={#MyAppVersion}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
; Remove the following line to run in administrative install mode (install for all users.)
PrivilegesRequired=lowest
OutputDir=.\output
OutputBaseFilename=WorkTimer
SetupIconFile=..\WorkTimer4\Assets\clock_blue.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#PublishPath}\projects.json"; DestDir: "{userdocs}\WorkTimer"; Flags: ignoreversion
Source: "{#PublishPath}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\config.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\Microsoft.Toolkit.Mvvm.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\Microsoft.Xaml.Behaviors.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\PropertyTools.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\PropertyTools.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\System.Composition.AttributedModel.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\System.Composition.Convention.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\System.Composition.Hosting.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\System.Composition.Runtime.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\System.Composition.TypedParts.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\WorkTimer4.API.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\WorkTimer4.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\WorkTimer4.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{autostartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

