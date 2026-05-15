EdgeDrillDiag.exe reverse notes

Source executable:
C:\Users\Пользователь\Downloads\Telegram Desktop\EdgeDrillDiag.exe

SHA256:
938ACA0F25EC739DCA104C60ACE32F3D32FB93A46E0392D32739D76C5E0E63F3

Detected format:
PyInstaller executable, Python 3.12.

Main extracted bytecode:
C:\Users\Пользователь\Downloads\telegram_album_bot\EdgeDrillDiag_decompile\EdgeDrillDiag.exe_extracted\edge_drill_diag.pyc

Best local decompilation artifacts:
C:\Users\Пользователь\Downloads\telegram_album_bot\EdgeDrillDiag_decompile\decompiled\edge_drill_diag_pycdc.py
  Partial pycdc module output. Imports and STYLESHEET are recovered, class bodies are incomplete because pycdc hit unsupported Python 3.12 MAKE_CELL opcode.

C:\Users\Пользователь\Downloads\telegram_album_bot\EdgeDrillDiag_decompile\decompiled\edge_drill_diag_byteripper.py
  Rough byteripper output. It contains class and method structure, but it is not valid Python and has incorrect reconstructed expressions.

C:\Users\Пользователь\Downloads\telegram_album_bot\EdgeDrillDiag_decompile\codeobjects_pycdc\
  pycdc output for each extracted code object/method. Many smaller methods decompiled better here than the full module.

Exact analysis artifacts:
C:\Users\Пользователь\Downloads\telegram_album_bot\EdgeDrillDiag_decompile\analysis\edge_drill_diag_codeobjects.txt
  Function/class/code-object index with names, line numbers, arguments, locals, globals.

C:\Users\Пользователь\Downloads\telegram_album_bot\EdgeDrillDiag_decompile\analysis\edge_drill_diag_disassembly.txt
  Full recursive Python 3.12 bytecode disassembly.

C:\Users\Пользователь\Downloads\telegram_album_bot\EdgeDrillDiag_decompile\analysis\edge_drill_diag_strings.txt
  Extracted string constants.

Notes:
Classic decompyle3/uncompyle6 do not support Python 3.12 bytecode for this file.
PyLingual was installed and patched for xdis tuple compatibility, but the main module did not complete after 10 minutes and was stopped.
No attempt was made to run the original EdgeDrillDiag.exe.
