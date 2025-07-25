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

window.drawChart = (canvasId, labels, data, chartType = 'bar', dotNetHelper = null) => {
    const ctx = document.getElementById(canvasId).getContext('2d');

    if (window.resultCharts && window.resultCharts[canvasId]) {
        window.resultCharts[canvasId].destroy();
    }

    window.resultCharts = window.resultCharts || {};

    window.resultCharts[canvasId] = new Chart(ctx, {
        type: chartType,
        data: {
            labels: labels,
            datasets: [{
                label: 'Valores',
                data: data,
                backgroundColor: [
                    'rgba(54, 162, 235, 0.5)',
                    'rgba(255, 99, 132, 0.5)',
                    'rgba(255, 206, 86, 0.5)',
                    'rgba(75, 192, 192, 0.5)',
                    'rgba(153, 102, 255, 0.5)',
                    'rgba(255, 159, 64, 0.5)'
                ],
                borderColor: [
                    'rgba(54, 162, 235, 1)',
                    'rgba(255, 99, 132, 1)',
                    'rgba(255, 206, 86, 1)',
                    'rgba(75, 192, 192, 1)',
                    'rgba(153, 102, 255, 1)',
                    'rgba(255, 159, 64, 1)'
                ],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: chartType !== 'bar' && chartType !== 'line'
                }
            },
            onClick: (event, elements) => {
                if (elements.length > 0 && dotNetHelper) {
                    const index = elements[0].index;
                    const clickedLabel = labels[index];
                    dotNetHelper.invokeMethodAsync("OnChartLabelClicked", clickedLabel);
                }
            }
        }
    });
};

window.destroyChart = (canvasId) => {
    if (window.resultCharts && window.resultCharts[canvasId]) {
        window.resultCharts[canvasId].destroy();
        delete window.resultCharts[canvasId];
    }
    if (window.miniCharts && window.miniCharts[canvasId]) {
        window.miniCharts[canvasId].destroy();
        delete window.miniCharts[canvasId];
    }
};
