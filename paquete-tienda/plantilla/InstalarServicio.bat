@echo off
title ALXOR Core - Instalar servicio
echo Esto instalara ALXOR Core como servicio de Windows (siempre arrancado).
echo Pedira permisos de Administrador.
echo.
pause
powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process powershell -Verb RunAs -ArgumentList '-NoProfile','-ExecutionPolicy','Bypass','-File','\"%~dp0scripts\InstalarServicio.ps1\"'"
