## SATURN COMPILATION SCRIPT ##
# By Andrew Maney, 2024
# https://github.com/MEMESCOEP/Saturn


## IMPORTS ##
from rich.table import Table as RTable
from rich import print as RPrint
from rich.panel import Panel
from datetime import datetime
import subprocess
import traceback
import shutil
import time
import sys
import os


## VARIABLES ##
EnableConsoleInBuild = False
SkipCleanup = False
NoSplash = False
UseUPX = False
BuildStartTime = datetime.now()
CurrentPlatform = "THIS_PLATFORM"
#AppIconPath = "./src/Images/Icon.png"
OutputPath = "./bin/"
UPXPath = r""
PlatformOptions = [
    # Windows
    "win-x86",
    "win-x64",
    "win-arm64",

    # Linux
    "linux-musl-x64",
    "linux-x64",
    "linux-bionic-arm64",
    "linux-musl-arm64",
    "linux-arm64",
    "linux-arm",

    # MacOS
    "osx-x64",
    "osx-arm64",

    # FreeBSD
    "freebsd-arm64",
    "freebsd-x64",
    "freebsd",

    # Mobile (Android & iOS)
    "android-arm64",
    "android-arm",
    "android",
    "ios-arm64",
    "ios-arm",
    "ios",
]
DotnetOptions = [
    "dotnet",
    "publish",
    "Saturn.sln",
    "-p:PublishSingleFile=true",
    "--self-contained",
    "false",
]

## FUNCTIONS ##
def BuildFailure(ReturnCode):
    print("\n\n")
    RPrint(Panel.fit("[red]🛑 BUILD FAILED! 🛑[/red]", style="bold"))
    RPrint(f"[red]Build failed: {ReturnCode}[/red]")

    if str(ReturnCode).isnumeric():
        exit(ReturnCode)

    else:
        exit(-1)

def StartProcess(Process):
    Child = subprocess.Popen(Process)
    Child.wait()
    ReturnCode = Child.poll()

    if ReturnCode != 0:
        BuildFailure(ReturnCode)

def ShowDetails():
    DetailsTable = RTable(title="Saturn", style="magenta")

    DetailsTable.add_column("Property", style="cyan", no_wrap=True)
    DetailsTable.add_column("Description", style="yellow")

    DetailsTable.add_row("Author", "memescoep")
    DetailsTable.add_row("Github", "https://github.com/MEMESCOEP/Saturn")
    DetailsTable.add_row("Dotnet", "8.0 & later")
    DetailsTable.add_row("License", "MIT License (https://opensource.org/license/mit/)")

    RPrint(DetailsTable)

def ShowHelp():
    ShowDetails()
    print("\n\n\n")

    CommandTable = RTable(title="Compiler", style="magenta")
    ExtrasTable = RTable(title="\n\n\nExtras", style="magenta")
    OSTable = RTable(title="\n\n\nSupported Operating Systems", style="magenta")

    CommandTable.add_column("Command", style="cyan", no_wrap=True)
    CommandTable.add_column("Description", style="yellow")
    ExtrasTable.add_column("Notes", style="cyan", no_wrap=True)
    OSTable.add_column("OS", style="cyan", no_wrap=True)
    OSTable.add_column("Version", style="yellow")

    CommandTable.add_row("--help", "Shows help")
    CommandTable.add_row("--platform", "Forces compilation for the specified platform. Available options are:\n  " + "\n  ".join(PlatformOptions) + "\n")
    CommandTable.add_row("--verbose", "Enables verbosity when compiling")
    CommandTable.add_row("--upx <upx dir>", "Uses UPX to compress the binary (useful for decreasing its size) [italic bold red]!! BROKEN FOR NOW !![/italic bold red]")
    CommandTable.add_row("--skip-cleanup", "Skips clean up steps after compiling (useful for debugging)")
    ExtrasTable.add_row("Some antiviruses might falsely flag the binary.")
    ExtrasTable.add_row("Only Dotnet 8.0 and later are currently supported. Please use one of these versions.")
    OSTable.add_row("Microsoft Windows", "Windows 10 and later")
    OSTable.add_row("GNU/Linux", "Distros with X11/Wayland support")
    OSTable.add_row("Apple MacOS", "Untested")
    OSTable.add_row("FreeBSD", "Untested")

    RPrint(CommandTable)
    RPrint(OSTable)
    RPrint(ExtrasTable)


## MAIN CODE ##
try:
    RPrint(Panel.fit("[yellow]🪐 SATURN 🪐[/yellow]", style="bold"))

    # Handle command line arguments
    SkipArgument = False

    for Arg in sys.argv[1:]:
        if SkipArgument:
            SkipArgument = False
            continue

        if "--help" in sys.argv:
            ShowHelp()
            sys.exit(0)

        match Arg:
            case "--verbose":
                print(f"[INFO] >> Verbosity enabled.")
                DotnetOptions.append('--verbosity')
                DotnetOptions.append('detailed')

            case "--upx":
                if len(sys.argv) < (sys.argv.index(Arg) + 2):
                    raise KeyError("The path of the UPX directory needs to be specified")

                if os.path.isdir(sys.argv[sys.argv.index(Arg) + 1]) == False:
                    raise NotADirectoryError("The specified UPX path is not a directory, or it doesn't exist")

                print(f"[INFO] >> The binary will be compressed with UPX.")
                SkipArgument = True
                UPXPath = sys.argv[sys.argv.index(Arg) + 1].replace("'", "").replace("\"", "")
                UseUPX = True

            case "--platform":
                if len(sys.argv) < (sys.argv.index(Arg) + 2):
                    raise KeyError("You must specify a platform when using \"--platform\"")

                EnteredPlatform = sys.argv[sys.argv.index(Arg) + 1].replace("'", "").replace("\"", "")

                # Check if the entered platform is in the platform list
                if any(EnteredPlatform in Platform for Platform in PlatformOptions) == False:
                    raise KeyError(f"The platform \"{EnteredPlatform}\" is not valid. See help for available options.")

                print(f"[INFO] >> The binary will be compiled targeting the \"{EnteredPlatform}\" platform.")

                # Check if the curretn platform must use the app host
                if EnteredPlatform.startswith("android"):
                    print(f"[INFO] >> Compilation targeting the \"{EnteredPlatform}\" platform must use the application host, so this will be configured.")
                    DotnetOptions.append("-p:UseAppHost=true")
                    DotnetOptions.append("-p:PublishSingleFile=false")

                # Check if the target platform is not widely supported
                if EnteredPlatform.startswith("win") == False and EnteredPlatform.startswith("linux") == False and EnteredPlatform.startswith("osx") == False:
                    print(f"[WARN] >> Compilation targeting the \"{EnteredPlatform}\" platform may not be available, so you might need to recompile the Dotnet SDK yourself. See https://github.com/dotnet/runtime/issues/31180 for more information.")

                SkipArgument = True
                CurrentPlatform = EnteredPlatform

            case "--skip-cleanup":
                print(f"[INFO] >> Cleanup steps will be skipped.")
                SkipCleanup = True

            case _:
                ShowHelp()
                raise Exception(f"The argument \"{Arg}\" is unknown.")

    # Compile Saturn
    print(f"[INFO] >> Build started at {BuildStartTime}\n\n")
    RPrint(Panel.fit("[blue]🔧 COMPILING SATURN 🔧[/blue]", style="bold"))

    # Make sure dotnet is present on the system
    if shutil.which("dotnet") == None:
        raise FileNotFoundError("The dotnet executable couldn't be found")

    else:
        DotnetOptions.append("--output")

        # Check if a platform has been set
        if CurrentPlatform == "THIS_PLATFORM":
            DotnetOptions.append(OutputPath)
            DotnetOptions.append("--ucr")

        else:
            DotnetOptions.append(f"{OutputPath}/{CurrentPlatform}")
            DotnetOptions.append("--runtime")
            DotnetOptions.append(CurrentPlatform)

        StartProcess(DotnetOptions)

    # If UPX is enabled, start the UPX executable with the proper args
    if UseUPX == True:
        if CurrentPlatform == "THIS_PLATFORM":
            StartProcess([f"{UPXPath}/upx", "-v", "-9", f"{OutputPath}/Saturn"])

        else:
            StartProcess([f"{UPXPath}/upx", "-v", "-9", f"{OutputPath}/{CurrentPlatform}/Saturn"])

    # Do some cleanup (move the binary, delete temp files & dirs)
    print("\n\n")
    RPrint(Panel.fit("[blue]✨ CLEANING UP ✨[/blue]", style="bold"))

    # Remove temporary files and directories
    if SkipCleanup:
        print(f"[INFO] >> Cleanup steps have been skipped.")

    else:
        print(f"[INFO] >> Waiting for 3 seconds for filesystem to finish updating...")
        time.sleep(3)
        print("[INFO] >> Removing temporary directories and files...")

        if (os.path.exists(f"{OutputPath}Debug")):
            shutil.rmtree(f"{OutputPath}Debug")

        else:
            print(f"[INFO] >> The directory \"{OutputPath}Debug\" doesn't exist.")

        if (os.path.exists(f"{OutputPath}Release")):
            shutil.rmtree(f"{OutputPath}Release")

        else:
            print(f"[INFO] >> The directory \"{OutputPath}Release\" doesn't exist.")

    print("\n\n")

    # The build is complete
    RPrint(Panel.fit("[green]✅ BUILD SUCCEEDED ✅[/green]", style="bold"))
    print(f"[INFO] >> Build finished.\n\tBuild start time: {BuildStartTime}\n\tBuild finish time: {datetime.now()}\n\tTotal build time: {datetime.now() - BuildStartTime}.")

except Exception as EX:
    RPrint(f"[red]{traceback.format_exc()}[/red]")
    BuildFailure(str(EX))
