import * as signalR from 'https://cdn.jsdelivr.net/npm/@microsoft/signalr@7.0.5/dist/esm/signalr.js';

let connection = null;

export async function connectToProgressHub(hubUrl, requestId, dotNetRef) {
    if (connection) await disconnect();

    connection = new signalR.HubConnectionBuilder()
        .withUrl(`${hubUrl}?requestId=${requestId}`)
        .withAutomaticReconnect()
        .build();

    connection.on("ProgressUpdate", (update) => {
        dotNetRef.invokeMethodAsync("OnProgress", update);
    });

    connection.onreconnecting(() => {
        dotNetRef.invokeMethodAsync("OnReconnectStateChanged", true);
    });

    connection.onreconnected(() => {
        dotNetRef.invokeMethodAsync("OnReconnectStateChanged", false);
    });

    connection.onclose(() => {
        dotNetRef.invokeMethodAsync("OnReconnectStateChanged", false);
    });

    try {
        await connection.start();
        await connection.invoke("Subscribe", requestId);
        console.log("✅ SignalR conectado para la solicitud:", requestId);
    } catch (err) {
        console.warn("❌ Error conectando a SignalR", err);
    }
}

export async function disconnect() {
    if (connection) {
        try {
            await connection.stop();
        } catch (err) {
            console.warn("❌ Error al desconectar SignalR", err);
        } finally {
            connection = null;
        }
    }
}
