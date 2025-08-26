let es;

export function connectSSE(requestId, baseUrl, dotNetRef) {
    const url = `${baseUrl}/progress/sse/${requestId}`;
    es = new EventSource(url);

    es.onopen = () => {
        dotNetRef.invokeMethodAsync('OnReconnectStateChanged', false);
    };

    es.onmessage = e => {
        if (!e.data || e.data === ":hb") return;
        try {
            const parsed = JSON.parse(e.data);
            dotNetRef.invokeMethodAsync('OnProgressEvent', JSON.stringify(parsed));
        } catch {
            // Ignora errores de parseo
        }
    };

    es.onerror = () => {
        dotNetRef.invokeMethodAsync('OnReconnectStateChanged', true);
        try { es.close(); } catch { }
    };
}

export function closeSSE() {
    try { es?.close(); } catch { }
}

