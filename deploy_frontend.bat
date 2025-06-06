@echo off
setlocal

echo 🔄 Limpiando carpeta de publicación anterior...
rd /s /q publish

echo 📦 Publicando proyecto Blazor Server...
dotnet publish SAPAssistant\SAPAssistant.csproj -c Release -o publish

echo 🚚 Subiendo archivos al servidor...
scp -r SAPAssistant Dockerfile root@91.99.139.55:/root/frontend

echo ✅ Listo. Ahora ejecuta el script en el servidor para construir el contenedor.
pause
