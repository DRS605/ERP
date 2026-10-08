@echo off
title ALXOR Core - Datos de ejemplo
echo Esto rellena el ERP con una empresa, clientes, articulos y facturas de ejemplo,
echo para ver el panel y los informes con contenido. (Solo para probar.)
echo.
echo IMPORTANTE: arranca antes ALXOR Core con ArrancarTienda.bat y dejalo abierto.
echo.
pause
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\datos-demo.ps1" -BaseUrl http://localhost:8080
echo.
echo Hecho. Entra con:  demo@alxorcore.es  /  Demo1234!
pause
