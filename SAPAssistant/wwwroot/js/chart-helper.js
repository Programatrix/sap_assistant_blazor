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

window.drawChart = (canvasId, labels, data) => {
    const ctx = document.getElementById(canvasId).getContext('2d');

    if (window.resultCharts && window.resultCharts[canvasId]) {
        window.resultCharts[canvasId].destroy();
    }

    window.resultCharts = window.resultCharts || {};

    window.resultCharts[canvasId] = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: 'rgba(54, 162, 235, 0.5)',
                borderColor: 'rgba(54, 162, 235, 1)',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false }
            }
        }
    });
};
