/* eslint-disable no-undef */
/* eslint-disable quotes */
/* eslint-disable no-unused-vars */
/* eslint-disable indent */

let yearlyProfitsChart;

async function fetchYearlyProfitsData(startDate, endDate) {
    const response = await fetch(`/Financials/GetYearlyProfitsData?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}`);
    const data = await response.json();
    return data;
}

function toPascalCase(obj) {
    const newObj = {};
    for (let key in obj) {
        const newKey = key.charAt(0).toUpperCase() + key.slice(1);
        newObj[newKey] = obj[key];
    }
    return newObj;
}  

function exportYearlyProfitsToCSV(currentYearData, lastYearData) {
    const CSV_SEPARATOR = ',';
    const LINE_SEPARATOR = '\n';

    // Header  
    let csvContent = 'Date,Selected Year Profits,Selected Year Items Sold,Selected Year ShipStation Sales,Previous Year Profits,Previous Year Items Sold,Previous Year ShipStation Sales' + LINE_SEPARATOR;

    // Combine data and sort by date  
    const combinedData = currentYearData.concat(lastYearData).sort((a, b) => new Date(a.Date) - new Date(b.Date));

    // Get unique dates without the year  
    const uniqueDatesSet = new Set(combinedData.map(item => {
        const date = new Date(item.Date);
        return (date.getMonth() + 1) + '/' + date.getDate();
    }));
    const uniqueDates = Array.from(uniqueDatesSet).sort((a, b) => new Date(`2000/${a}`) - new Date(`2000/${b}`));

    // Iterate through the unique dates and create CSV rows  
    uniqueDates.forEach(dateWithoutYear => {
        const currentYearRow = currentYearData.find(data => {
            const date = new Date(data.Date);
            return (date.getMonth() + 1) + '/' + date.getDate() === dateWithoutYear;
        });
        const lastYearRow = lastYearData.find(data => {
            const date = new Date(data.Date);
            return (date.getMonth() + 1) + '/' + date.getDate() === dateWithoutYear;
        });

        // CSV Row  
        csvContent += [
            dateWithoutYear,
            currentYearRow ? currentYearRow.Profits : '',
            currentYearRow ? currentYearRow.ItemsSold : '',
            currentYearRow ? currentYearRow.ShipStationSales : '',
            lastYearRow ? lastYearRow.Profits : '',
            lastYearRow ? lastYearRow.ItemsSold : '',
            lastYearRow ? lastYearRow.ShipStationSales : ''
        ].join(CSV_SEPARATOR) + LINE_SEPARATOR;
    });
    return csvContent;
}

function downloadCSVFile(content, fileName) {
    const blob = new Blob([content], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);

    link.setAttribute('href', url);
    link.setAttribute('download', fileName);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}  

function updateChartAndExportCSV(currentYearProfitsData, lastYearProfitsData, startDate, endDate) {
    const currentYearData = currentYearProfitsData.filter(item => new Date(item.Date) >= startDate && new Date(item.Date) <= endDate);

    const selectedYear = startDate.getFullYear();

    // Filter last year data first and then adjust the dates
    const lastYearDataFiltered = lastYearProfitsData.filter(item => new Date(item.Date) >= new Date(startDate.getFullYear() - 1, startDate.getMonth(), startDate.getDate()) && new Date(item.Date) <= new Date(endDate.getFullYear() - 1, endDate.getMonth(), endDate.getDate()));
    const lastYearData = adjustDatesToSelectedRange(lastYearDataFiltered, selectedYear);

    const chartContainer = document.querySelector('#yearlyProfitsChart');
    console.log('Current Year Data:', JSON.stringify(currentYearData));
    console.log('Last Year Data:', JSON.stringify(lastYearData));

    renderYearlyProfitsChart(chartContainer, currentYearData, lastYearData, startDate, endDate);
    updateYearlyProfitsTotals(currentYearProfitsData, lastYearProfitsData);
} 


function formatCurrency(value) {
    return value.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

function updateYearlyProfitsTotals(currentYearProfitsData, lastYearProfitsData) {
    const totalCurrentYearProfits = currentYearProfitsData.reduce((sum, item) => sum + item.Profits, 0);
    const totalLastYearProfits = lastYearProfitsData.reduce((sum, item) => sum + item.Profits, 0);
    const totalsElement = document.querySelector("#yearlyProfitsTotals");
    totalsElement.innerHTML = `Total Selected: $${formatCurrency(totalCurrentYearProfits)} | Total Previous: $${formatCurrency(totalLastYearProfits)}`;
} 
function adjustDatesToSelectedRange(data, selectedYear) {
    return data.map(item => {
        const itemDate = new Date(item.Date);
        itemDate.setFullYear(selectedYear);
        return { ...item, Date: itemDate };
    });
}
function renderYearlyProfitsChart(chartContainer, currentYearProfitsData, lastYearProfitsData, startDate = new Date(), endDate = new Date()) {  
    const baseYear = new Date().getFullYear();
    const selectedYear = new Date(currentYearProfitsData[0]?.Date || new Date().getFullYear()).getFullYear();
    const previousYear = selectedYear - 1;

    // Combine the data and assign a unified base year, specifically for getting both years to overlap.  
    const combinedData = currentYearProfitsData.concat(lastYearProfitsData).map((item, index, array) => ({
        x: new Date(baseYear, new Date(item.Date).getMonth(), new Date(item.Date).getDate()).getTime(),
        y: item.Profits,
        itemsSold: item.ItemsSold,
        originalDate: item.Date,
        seriesIndex: index < currentYearProfitsData.length ? 0 : 1,
        dataIndex: index,
    })).sort((a, b) => a.x - b.x);

    const minDate = Math.min(...combinedData.map(item => item.x));
    const maxDate = Math.max(...combinedData.map(item => item.x));  

    const options = {
        chart: {
            stacked: false,
            type: 'area',
            height: 350,
            zoom: {
                enabled: true,
                autoscaleYAxis: true
            },
            animations: {
                enabled: true
            }
        },
        plotOptions: { 
            line: {
                curve: "smooth",

            }
        },
        stroke: {
            curve: 'smooth'
        },
        grid: {
            row: {
                colors: ["transparent", "transparent"],
                opacity: 0.2
            },
            borderColor: "#f1f3fa",
        },
        series: [
            {
                name: 'Selected Year',
                data: combinedData.filter(item => item.seriesIndex === 0),
                itemsSold: currentYearProfitsData.map(item => item.ItemsSold),
                shipStationSales: currentYearProfitsData.map(item => item.ShipStationSales),
            },
            {
                name: 'Previous Year',
                data: combinedData.filter(item => item.seriesIndex === 1),
                itemsSold: lastYearProfitsData.map(item => item.ItemsSold),
                shipStationSales: lastYearProfitsData.map(item => item.ShipStationSales),
            },
        ],  
        dataLabels: {
            enabled: false
        },
        legend: {
            position: 'top',
        },

        annotations: {
            position: 'back',
            yaxis: [
                {
                    y: 0,
                    borderColor: 'transparent',
                    label: {
                        text: '',
                        offsetX: 0,
                        offsetY: -10,
                        style: {
                            color: '#333',
                            background: 'transparent',
                        },
                    },
                },
            ],
        },  
        markers: {
            size: 0,
        },
        xaxis: {
            labels: {
                datetimeFormatter: {
                    month: "MMM 'yy",
                    day: "dd MMM"
                }
            },
            type: "datetime",
            categories: [],
            min: minDate,
            max: maxDate  
        },
        yaxis: {
            title: {
                text: 'Profits',
            },
        labels: {  
            formatter: function (value) {  
                return '$' + formatCurrency(value);
            },  
        },  
        },
        fill: {
            gradient: {
                enabled: true,
                shadeIntensity: 1,
                inverseColors: false,
                opacityFrom: 0.5,
                opacityTo: 0.1,
                stops: [0, 70, 80, 100]
            },
        },
        colors: ['#008FFB', '#00E396'],
        tooltip: {
            shared: true,
            intersect: false,
            x: {
                format: 'MM/dd'
            },
            y: {
                formatter: function (value, { seriesIndex, dataPointIndex, w }) {
                    const itemsSold = w.config.series[seriesIndex].itemsSold[dataPointIndex] || 0;
                    const shipStationSales = w.config.series[seriesIndex].shipStationSales[dataPointIndex] || 0;
                    return 'Profits: $' + formatCurrency(value) + ' | Items Sold: ' + itemsSold + ' | ShipStation Sales: $' + formatCurrency(shipStationSales);
                },  
            },
        },
    };

    updateYearlyProfitsTotals(currentYearProfitsData, lastYearProfitsData); 

    if (yearlyProfitsChart) {
        yearlyProfitsChart.destroy();
    }

    yearlyProfitsChart = new ApexCharts(chartContainer, options);
    yearlyProfitsChart.render();
}  