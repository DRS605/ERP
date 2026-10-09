@echo off
title Core Evolution - Actualizar
echo Esto actualiza Core Evolution a la ultima version (pedira Administrador).
echo No borra datos ni vuelve a descargar nada.
echo.
pause
powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process powershell -Verb RunAs -ArgumentList '-NoProfile','-ExecutionPolicy','Bypass','-File','\"%~dp0scripts\actualizar.ps1\"'"
