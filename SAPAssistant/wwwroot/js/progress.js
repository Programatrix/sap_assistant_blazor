export function connectSSE(requestId, baseUrl, dotNetRef) {
  let es;
  let retry = 1000;

  function start() {
    const url = `${baseUrl}/progress/sse/${requestId}`;
    es = new EventSource(url);
    es.onopen = () => dotNetRef.invokeMethodAsync('OnReconnectStateChange', false);
    es.onmessage = e => dotNetRef.invokeMethodAsync('OnProgressEvent', e.data);
    es.onerror = () => {
      dotNetRef.invokeMethodAsync('OnReconnectStateChange', true);
      es.close();
      setTimeout(() => {
        retry = Math.min(retry * 2, 10000);
        start();
      }, retry);
    };
  }

  start();
  return es;
}

export function closeSSE(es) {
  if (es) {
    es.close();
  }
}
