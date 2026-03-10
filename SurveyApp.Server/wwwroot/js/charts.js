window.renderChart = (canvasId, labels, data, chartType) => {

    const ctx = document.getElementById(canvasId);

    if (!ctx)
        return;

    new Chart(ctx, {
        type: chartType,
        data: {
            labels: labels,
            datasets: [{
                label: 'Ответы',
                data: data,
                backgroundColor: [
                    '#4e73df',
                    '#1cc88a',
                    '#36b9cc',
                    '#f6c23e',
                    '#e74a3b',
                    '#858796',
                    '#5a5c69'
                ],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: chartType === 'pie'
                }
            },
            scales: chartType === 'bar' ? {
                y: {
                    beginAtZero: true
                }
            } : {}
        }
    });
};