window.drawMiniChart = (canvasId, chartData) => {
    const ctx = document.getElementById(canvasId).getContext('2d');

    if (window.miniCharts && window.miniCharts[canvasId]) {
        window.miniCharts[canvasId].destroy();
    }

    window.miniCharts = window.miniCharts || {};

    window.miniCharts[canvasId] = new Chart(ctx, {
        type: 'line',
        data: {
            labels: chartData.map((_, i) => i + 1),
            datasets: [{
                data: chartData,
                borderColor: '#4CAF50',
                borderWidth: 2,
                fill: false,
                tension: 0.4,
                pointRadius: 0
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: { display: false },
                y: { display: false }
            },
            plugins: {
                legend: { display: false },
                tooltip: { enabled: false }
            }
        }
    });
};
