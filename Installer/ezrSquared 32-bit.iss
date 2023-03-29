; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#include "environment.iss"

#define MyAppName "ezr�"
#define MyAppVersion "prerelease-1.5.1.1.0"
#define MyAppPublisher "Uralstech"
#define MyAppURL "https://uralstech.github.io/ezrSquared/"
#define MyAppExeName "ezrSquared.exe"
#define MyAppAssocName MyAppName + " Source Code"
#define MyAppAssocExt ".ezr2"
#define MyAppAssocKey StringChange(MyAppAssocName, " ", "") + MyAppAssocExt

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{2A507B20-D8C0-4E66-891F-13CFC590FCFA}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
ChangesAssociations=yes
ChangesEnvironment=yes
MinVersion=6.1.7600
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=D:\Code\csharp\ezrSquared\LICENSE.txt
InfoAfterFile=D:\Code\csharp\ezrSquared\Changelog.txt
; Remove the following line to run in administrative install mode (install for all users.)
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
OutputDir=D:\Code\csharp\ezrSquared\Installer\bin
OutputBaseFilename=ezrSquared Installer (Windows 32-bit)
SetupIconFile=D:\Code\csharp\ezrSquared\Graphics\Icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[LangOptions]
DialogFontSize=10

[CustomMessages]
WebDocumentation=%1 Online Documentation
LocalDocumentation=%1 Offline Documentation

[Types]
Name: "full"; Description: "Full installation"
Name: "compact"; Description: "Compact installation"
Name: "custom"; Description: "Custom installation"; Flags: iscustom

[Components]
Name: "main"; Description: "ezr� Interpreter"; Types: full compact custom; Flags: fixed
Name: "libs"; Description: "Standard Libraries"; Types: full compact
Name: "docs"; Description: "Documentation"; Types: full

[Tasks]
Name: "addtopath"; Description: "Add ezr� to PATH environment variable"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce

[Dirs]
Name: "{app}\Libraries"

[Files]
Source: "D:\Code\csharp\ezrSquared\LICENSE.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Code\csharp\ezrSquared\bin\Release\net7.0\win-x86\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Code\csharp\ezrSquared\bin\Release\net7.0\win-x86\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "D:\Code\csharp\ezrSquared\Offline Site\ezrSquared Offline\*"; DestDir: "{app}\Documentation"; Flags: ignoreversion recursesubdirs createallsubdirs; Components: docs
Source: "D:\Code\csharp\ezrSquared\Libraries\io\bin\Release\net7.0\win-x86\io.dll"; DestDir: "{app}\Libraries"; Flags: ignoreversion; Components: libs
Source: "D:\Code\csharp\ezrSquared\Libraries\std\bin\Release\net7.0\win-x86\std.dll"; DestDir: "{app}\Libraries"; Flags: ignoreversion; Components: libs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Registry]
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocExt}\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppAssocKey}"; ValueData: ""; Flags: uninsdeletevalue
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}"; ValueType: string; ValueName: ""; ValueData: "{#MyAppAssocName}"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""
Root: HKA; Subkey: "Software\Classes\Applications\{#MyAppExeName}\SupportedTypes"; ValueType: string; ValueName: ".myp"; ValueData: ""

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
begin
    if (CurStep = ssPostInstall) and WizardIsTaskSelected('addtopath')
     then EnvAddPath(ExpandConstant('{app}'));
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
    if CurUninstallStep = usPostUninstall
    then EnvRemovePath(ExpandConstant('{app}'));
end;

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:WebDocumentation,{#MyAppName}}"; Filename: "{#MyAppURL}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{group}\{cm:LocalDocumentation,{#MyAppName}}"; Filename: "{app}\Documentation\START_HERE.html"; Components: docs
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

