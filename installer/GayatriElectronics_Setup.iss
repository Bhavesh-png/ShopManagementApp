; =============================================================
;  Gayatri Electronics & Hardware - Shop Management Application
;  Inno Setup Installer Script
;  Generated for .NET 8 WinForms Self-Contained Application
; =============================================================

#define AppName "Gayatri Electronics"
#define AppFullName "Gayatri Electronics & Hardware"
#define AppVersion "1.0.0"
#define AppPublisher "Gayatri Electronics"
#define AppExeName "GayatriElectronics.exe"
#define AppDescription "Shop Management Desktop Application"
#define SourceDir "..\ShopManagementApp.UI\bin\Release\net8.0-windows\win-x64\publish"

[Setup]
; Basic App Info
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#AppFullName}
AppVersion={#AppVersion}
AppVerName={#AppFullName} v{#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL=https://gayatrielectronics.local
AppSupportURL=https://gayatrielectronics.local/support
AppCopyright=Copyright (C) 2024 {#AppPublisher}

; Install Directory
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=no

; Output
OutputDir=Output
OutputBaseFilename=ShopManager_Setup_v{#AppVersion}
SetupIconFile=..\ShopManagementApp.UI\Assets\AppIcon.ico

; Compression
Compression=lzma2/ultra64
SolidCompression=yes
LZMAUseSeparateProcess=yes
LZMANumBlockThreads=4

; Installer Appearance
WizardStyle=modern
WizardResizable=no
ShowLanguageDialog=no
DisableWelcomePage=no

; Privileges
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

; Uninstall
UninstallDisplayIcon={app}\{#AppExeName}
UninstallDisplayName={#AppFullName}
CreateUninstallRegKey=yes

; Misc
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon";   Description: "Create a &Desktop shortcut";    GroupDescription: "Additional icons:"
Name: "startmenuicon"; Description: "Create a &Start Menu shortcut"; GroupDescription: "Additional icons:"

[Files]
; Main Executable
Source: "{#SourceDir}\{#AppExeName}"; DestDir: "{app}"; Flags: ignoreversion

; Assets folder (icons etc.)
Source: "..\ShopManagementApp.UI\Assets\*"; DestDir: "{app}\Assets"; Flags: ignoreversion recursesubdirs createallsubdirs

; NOTE: .pdb files are debug symbols - excluded from release installer

[Icons]
; Desktop Shortcut
Name: "{autodesktop}\{#AppFullName}"; Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\Assets\AppIcon.ico"; Tasks: desktopicon; Comment: "{#AppDescription}"

; Start Menu Shortcuts
Name: "{group}\{#AppFullName}";    Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\Assets\AppIcon.ico"; Tasks: startmenuicon; Comment: "{#AppDescription}"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"; Tasks: startmenuicon

[Run]
; Launch app after install
Filename: "{app}\{#AppExeName}"; Description: "Launch {#AppFullName} now"; Flags: nowait postinstall skipifsilent unchecked

[UninstallDelete]
; Clean up any leftover files/folders after uninstall
Type: filesandordirs; Name: "{app}"

; [Registry] section removed — Inno Setup manages Add/Remove Programs automatically via AppId.

[Messages]
WelcomeLabel1=Welcome to [name] Setup
WelcomeLabel2=This will install [name/ver] on your computer.%n%nA complete Shop Management System for Gayatri Electronics & Hardware, including Inventory, Billing, Repairs, and Customer management.%n%nClick Next to continue.
FinishedLabel=Setup has successfully installed [name] on your computer.%n%nThe application is ready to use.
