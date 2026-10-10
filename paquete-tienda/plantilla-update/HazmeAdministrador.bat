@echo off
title Core Evolution - Hacerme administrador
echo Esto te marca como administrador de la plataforma en esta instalacion
echo (para ver la seccion "Instalaciones"). Pedira Administrador.
echo.
pause
powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process powershell -Verb RunAs -ArgumentList '-NoProfile','-ExecutionPolicy','Bypass','-File','\"%~dp0scripts\HazmeAdministrador.ps1\"'"
