REM Post build script to create a distributable package
REM Visual Studio Usage: 
REM if $(ConfigurationName) == Release call $(SolutionDir)scripts/pack.bat $(ProjectDir) $(ProjectPath)


@echo off
SETLOCAL EnableDelayedExpansion

REM Assign command line arguments to variable
SET "PROJECT_PATH=%~1"
SET "RUNYAK=%~2"

REM Go to root directory
cd ..

REM Navigate to the scripts directory
cd scripts/

REM Check if the virtual environment folder exists
if not exist "venv\" (
    REM Create the virtual environment
    python -m venv venv
    REM Install dependencies from requirements.txt
    venv\Scripts\pip install -r requirements.txt
)

REM Activate the virtual environment
call .\venv\Scripts\activate

python package.py %PROJECT_PATH% %RUNYAK%

:END
ENDLOCAL