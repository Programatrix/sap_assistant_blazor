🧭 Guía definitiva: Despliegue exitoso del frontend Blazor Server con Docker (Opción 2)
Objetivo: Desplegar el frontend de Blazor Server (SAPAssistant) en un servidor Ubuntu remoto, usando Docker, compilando dentro del contenedor.

🖥️ 1. Estructura local del proyecto
Tu estructura en tu PC es así:

python
Copiar
Editar
E:\Programacion\C#\sap_assistant_blazor\
│
├── Dockerfile          👈 Está en esta carpeta (muy importante)
├── SAPAssistant\
│   ├── SAPAssistant.csproj
│   ├── Program.cs
│   └── ... (todo el código fuente del frontend)
📤 2. Subir los archivos al servidor remoto
Desde tu PC (PowerShell o CMD):

bash
Copiar
Editar
cd "E:\Programacion\C#\sap_assistant_blazor"

scp -r SAPAssistant Dockerfile root@91.99.139.55:/root/frontend
✅ Esto copia:

La carpeta completa del proyecto con el código fuente: SAPAssistant/

El Dockerfile necesario para construir el contenedor

🐧 3. Acceder al servidor por SSH
bash
Copiar
Editar
ssh root@91.99.139.55
cd /root/frontend
🐳 4. Construir la imagen Docker con compilación incluida
Asegúrate de estar en /root/frontend y ejecuta:

bash
Copiar
Editar
docker rm -f asistente-frontend  # Por si ya existía un contenedor anterior
docker rmi asistente-frontend    # Elimina la imagen vieja, si existía (opcional)
docker build --no-cache -t asistente-frontend .
✅ Esto usa tu Dockerfile para compilar y publicar automáticamente el proyecto Blazor dentro del contenedor.

🚀 5. Ejecutar el contenedor
bash
Copiar
Editar
docker run -d -p 5000:80 --name asistente-frontend asistente-frontend
Esto levanta el contenedor y expone la aplicación en el puerto 5000.

🌐 6. Acceder a la aplicación desde el navegador
Ir a:
http://91.99.139.55:5000

✅ Verás la versión actual del frontend. Ya no carga SAPAssistant.styles.css, y todo coincide con lo que ves en desarrollo local.