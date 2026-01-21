/**
 * Dashboard Charts - İnteraktif Grafik Yönetimi
 * Modern Chart.js implementasyonu
 */

// Global chart instances
let chartInstances = {};

// Renk paleti
const colors = {
    primary: '#3B82F6',
    success: '#10B981',
    warning: '#F59E0B',
    danger: '#EF4444',
    info: '#06B6D4',
    purple: '#8B5CF6',
    pink: '#EC4899',
    orange: '#F97316',
    lime: '#84CC16',
    indigo: '#6366F1',

    // Status colors
    pending: '#FCD34D',
    shipped: '#60A5FA',
    delivered: '#34D399',
    cancelled: '#F87171',
    returned: '#A78BFA'
};

// Para formatı
function formatCurrency(value) {
    return new Intl.NumberFormat('tr-TR', {
        style: 'currency',
        currency: 'TRY',
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    }).format(value);
}

// Sayı formatı
function formatNumber(value) {
    return new Intl.NumberFormat('tr-TR').format(value);
}

// API'den veri çek
async function fetchChartData(endpoint, companyId = null) {
    try {
        const startDate = document.getElementById('startDate')?.value;
        const endDate = document.getElementById('endDate')?.value;

        let url = `/Home/${endpoint}?`;
        if (startDate) url += `startDate=${startDate}&`;
        if (endDate) url += `endDate=${endDate}&`;
        if (companyId) url += `companyId=${companyId}&`;

        const response = await fetch(url);
        if (!response.ok) throw new Error('API error');
        return await response.json();
    } catch (error) {
        console.error(`Error fetching ${endpoint}:`, error);
        return null;
    }
}

// Tüm grafikleri yükle
async function loadAllCharts() {
    showLoading();

    const companyId = document.getElementById('companyFilter')?.value || null;

    try {
        // Tüm verileri paralel olarak çek
        const [
            salesTrendData,
            categorySalesData,
            customerSegmentData,
            orderStatusData,
            geoData,
            avgCartData,
            topProductsData
        ] = await Promise.all([
            fetchChartData('GetSalesTrend', companyId),
            fetchChartData('GetCategoryStock', companyId),
            fetchChartData('GetCustomerSegmentation', companyId),
            fetchChartData('GetOrderStatusDistribution', companyId),
            fetchChartData('GetGeographicDistribution', companyId),
            fetchChartData('GetAverageCartTrend', companyId),
            fetchChartData('GetTopProducts', companyId)
        ]);

        // Grafikleri oluştur
        if (salesTrendData) createSalesTrendChart(salesTrendData);
        if (categorySalesData) createCategorySalesChart(categorySalesData);
        if (customerSegmentData) createCustomerSegmentChart(customerSegmentData);
        if (orderStatusData) createOrderStatusChart(orderStatusData);
        if (geoData) createGeographicHeatmap(geoData);
        if (avgCartData) createAverageCartChart(avgCartData);
        if (topProductsData) createTopProductsChart(topProductsData);

    } catch (error) {
        console.error('Error loading charts:', error);
    } finally {
        hideLoading();
    }
}

// 1. Satış Trendi Grafiği (Line Chart)
function createSalesTrendChart(data) {
    const ctx = document.getElementById('salesTrendChart');
    if (!ctx) return;

    // Mevcut grafiği yok et
    if (chartInstances.salesTrend) {
        chartInstances.salesTrend.destroy();
    }

    chartInstances.salesTrend = new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.map(d => d.date),
            datasets: [{
                label: 'Satış Tutarı',
                data: data.map(d => d.revenue),
                borderColor: colors.primary,
                backgroundColor: createGradient(ctx, colors.primary),
                fill: true,
                tension: 0.4,
                pointRadius: 2,
                pointHoverRadius: 8,
                pointBackgroundColor: colors.primary,
                pointBorderColor: '#fff',
                pointBorderWidth: 2
            }, {
                label: 'Sipariş Sayısı',
                data: data.map(d => d.orders),
                borderColor: colors.success,
                backgroundColor: 'transparent',
                borderDash: [5, 5],
                tension: 0.4,
                pointRadius: 0,
                yAxisID: 'y1'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                intersect: false,
                mode: 'index'
            },
            plugins: {
                legend: {
                    display: true,
                    position: 'top',
                    labels: {
                        usePointStyle: true,
                        padding: 20
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0,0,0,0.8)',
                    titleFont: { size: 14, weight: 'bold' },
                    bodyFont: { size: 13 },
                    padding: 12,
                    cornerRadius: 8,
                    callbacks: {
                        label: function (context) {
                            if (context.datasetIndex === 0) {
                                return 'Satış: ' + formatCurrency(context.parsed.y);
                            }
                            return 'Sipariş: ' + context.parsed.y + ' adet';
                        }
                    }
                }
            },
            scales: {
                x: {
                    grid: { display: false },
                    ticks: { maxRotation: 45 }
                },
                y: {
                    position: 'left',
                    grid: { color: 'rgba(0,0,0,0.05)' },
                    ticks: {
                        callback: value => formatCurrency(value)
                    }
                },
                y1: {
                    position: 'right',
                    grid: { display: false },
                    ticks: {
                        callback: value => value + ' adet'
                    }
                }
            },
            onClick: (event, elements) => {
                if (elements.length > 0) {
                    const index = elements[0].index;
                    const date = data[index].fullDate;
                    showDayDetail(date, data[index]);
                }
            }
        }
    });
}

// 2. Kategori Stok Grafiği (Pie/Doughnut Chart)
function createCategorySalesChart(data) {
    const ctx = document.getElementById('categorySalesChart');
    if (!ctx) return;

    if (chartInstances.categorySales) {
        chartInstances.categorySales.destroy();
    }

    chartInstances.categorySales = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: data.map(d => d.name),
            datasets: [{
                data: data.map(d => d.stock || d.sales), // Fallback if data format differs
                backgroundColor: data.map(d => d.color),
                borderWidth: 0,
                hoverOffset: 10
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '60%',
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: 'rgba(0,0,0,0.8)',
                    padding: 12,
                    cornerRadius: 8,
                    callbacks: {
                        label: function (context) {
                            const item = data[context.dataIndex];
                            return [
                                `Stok: ${formatNumber(item.stock || item.sales)} adet`,
                                `Oran: %${item.percentage.toFixed(1)}`
                            ];
                        }
                    }
                }
            },
            onClick: (event, elements) => {
                if (elements.length > 0) {
                    const index = elements[0].index;
                    showCategoryDetail(data[index]);
                }
            }
        }
    });

    // Legend'ı oluştur
    createCategoryLegend(data);
}

function createCategoryLegend(data) {
    const container = document.getElementById('categoryLegend');
    if (!container) return;

    container.innerHTML = data.map(item => `
        <div class="category-legend-item" onclick="highlightCategory('${item.name}')">
            <span class="category-color" style="background-color: ${item.color}"></span>
            <span class="category-name">${item.name}</span>
            <span class="category-value">${formatNumber(item.stock || item.sales)}</span>
            <span class="category-percent">%${item.percentage.toFixed(1)}</span>
        </div>
    `).join('');
}

// 3. Müşteri Segmentasyonu (Bar Chart)
function createCustomerSegmentChart(data) {
    const ctx = document.getElementById('customerSegmentChart');
    if (!ctx) return;

    if (chartInstances.customerSegment) {
        chartInstances.customerSegment.destroy();
    }

    chartInstances.customerSegment = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: ['Müşteri Sayısı', 'Gelir (₺)'],
            datasets: [{
                label: 'Yeni Müşteri',
                data: [data.newCustomers, data.newRevenue],
                backgroundColor: colors.primary,
                borderRadius: 8,
                barPercentage: 0.6
            }, {
                label: 'Tekrar Müşteri',
                data: [data.returningCustomers, data.returningRevenue],
                backgroundColor: colors.success,
                borderRadius: 8,
                barPercentage: 0.6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    position: 'top',
                    labels: { usePointStyle: true }
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            if (context.dataIndex === 1) {
                                return context.dataset.label + ': ' + formatCurrency(context.parsed.y);
                            }
                            return context.dataset.label + ': ' + formatNumber(context.parsed.y);
                        }
                    }
                }
            },
            scales: {
                x: { grid: { display: false } },
                y: {
                    grid: { color: 'rgba(0,0,0,0.05)' },
                    ticks: {
                        callback: function (value) {
                            if (value >= 1000) {
                                return (value / 1000) + 'K';
                            }
                            return value;
                        }
                    }
                }
            }
        }
    });

    // Summary kartlarını güncelle
    document.getElementById('newCustomerCount').textContent = formatNumber(data.newCustomers);
    document.getElementById('returningCustomerCount').textContent = formatNumber(data.returningCustomers);
    document.getElementById('newCustomerRevenue').textContent = formatCurrency(data.newRevenue);
    document.getElementById('returningCustomerRevenue').textContent = formatCurrency(data.returningRevenue);
}

// 4. Sipariş Durumları (Stacked Bar Chart)
function createOrderStatusChart(data) {
    const ctx = document.getElementById('orderStatusChart');
    if (!ctx) return;

    if (chartInstances.orderStatus) {
        chartInstances.orderStatus.destroy();
    }

    chartInstances.orderStatus = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: data.map(d => d.date),
            datasets: [{
                label: 'Beklemede',
                data: data.map(d => d.pending),
                backgroundColor: colors.pending,
                borderRadius: 4,
                stack: 'stack0'
            }, {
                label: 'Kargoda',
                data: data.map(d => d.shipped),
                backgroundColor: colors.shipped,
                borderRadius: 4,
                stack: 'stack0'
            }, {
                label: 'Teslim Edildi',
                data: data.map(d => d.delivered),
                backgroundColor: colors.delivered,
                borderRadius: 4,
                stack: 'stack0'
            }, {
                label: 'İptal',
                data: data.map(d => d.cancelled),
                backgroundColor: colors.cancelled,
                borderRadius: 4,
                stack: 'stack0'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    mode: 'index',
                    intersect: false,
                    callbacks: {
                        footer: function (tooltipItems) {
                            const total = tooltipItems.reduce((sum, item) => sum + item.parsed.y, 0);
                            return 'Toplam: ' + total + ' sipariş';
                        }
                    }
                }
            },
            scales: {
                x: {
                    stacked: true,
                    grid: { display: false }
                },
                y: {
                    stacked: true,
                    grid: { color: 'rgba(0,0,0,0.05)' }
                }
            }
        }
    });

    // Toplam sayıları hesapla ve göster
    const totals = {
        pending: data.reduce((sum, d) => sum + d.pending, 0),
        shipped: data.reduce((sum, d) => sum + d.shipped, 0),
        delivered: data.reduce((sum, d) => sum + d.delivered, 0),
        cancelled: data.reduce((sum, d) => sum + d.cancelled, 0)
    };

    document.getElementById('totalPending').textContent = formatNumber(totals.pending);
    document.getElementById('totalShipped').textContent = formatNumber(totals.shipped);
    document.getElementById('totalDelivered').textContent = formatNumber(totals.delivered);
    document.getElementById('totalCancelled').textContent = formatNumber(totals.cancelled);

    // İptal oranı yüksekse kırmızı vurgula
    const cancelRate = totals.cancelled / (totals.pending + totals.shipped + totals.delivered + totals.cancelled) * 100;
    if (cancelRate > 10) {
        document.getElementById('totalCancelled').classList.add('pulse-danger');
    }
}

// 5. Coğrafi Dağılım (Heatmap)
function createGeographicHeatmap(data) {
    const container = document.getElementById('cityGrid');
    if (!container) return;

    container.innerHTML = data.map(item => `
        <div class="col-md-4 col-sm-6">
            <div class="city-card intensity-${item.intensity}" onclick="showCityDetail('${item.city}', '${item.state}')">
                <div class="d-flex justify-content-between align-items-start">
                    <div>
                        <div class="fw-semibold">${item.city}</div>
                        <small class="text-muted">${item.state}</small>
                    </div>
                    <span class="badge ${getIntensityBadge(item.intensity)}">${item.orderCount}</span>
                </div>
                <div class="mt-2">
                    <div class="small text-muted">Gelir</div>
                    <div class="fw-bold">${formatCurrency(item.revenue)}</div>
                </div>
                <div class="progress mt-2" style="height: 4px;">
                    <div class="progress-bar ${getIntensityProgressBar(item.intensity)}" 
                         style="width: ${item.percentage}%"></div>
                </div>
            </div>
        </div>
    `).join('');
}

function getIntensityBadge(intensity) {
    const badges = {
        critical: 'bg-danger',
        high: 'bg-warning text-dark',
        medium: 'bg-info',
        low: 'bg-success'
    };
    return badges[intensity] || 'bg-secondary';
}

function getIntensityProgressBar(intensity) {
    const bars = {
        critical: 'bg-danger',
        high: 'bg-warning',
        medium: 'bg-info',
        low: 'bg-success'
    };
    return bars[intensity] || 'bg-secondary';
}

// 6. Ortalama Sepet Tutarı (Line Chart)
function createAverageCartChart(data) {
    const ctx = document.getElementById('avgCartChart');
    if (!ctx) return;

    if (chartInstances.avgCart) {
        chartInstances.avgCart.destroy();
    }

    chartInstances.avgCart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.map(d => d.date),
            datasets: [{
                label: 'Ortalama Sepet',
                data: data.map(d => d.value),
                borderColor: colors.purple,
                backgroundColor: createGradient(ctx, colors.purple),
                fill: true,
                tension: 0.4,
                pointRadius: 0,
                pointHoverRadius: 6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: ctx => 'Ort. Sepet: ' + formatCurrency(ctx.parsed.y)
                    }
                }
            },
            scales: {
                x: {
                    display: false,
                    grid: { display: false }
                },
                y: {
                    grid: { color: 'rgba(0,0,0,0.05)' },
                    ticks: {
                        callback: value => formatCurrency(value)
                    }
                }
            }
        }
    });

    // Güncel ortalamayı göster
    if (data.length > 0) {
        const current = data[data.length - 1].value;
        const previous = data.length > 1 ? data[data.length - 2].value : current;
        const change = previous > 0 ? ((current - previous) / previous * 100) : 0;

        document.getElementById('currentAvgCart').textContent = formatCurrency(current);

        const changeEl = document.getElementById('avgCartChange');
        if (change >= 0) {
            changeEl.className = 'badge bg-success-subtle text-success';
            changeEl.innerHTML = `<i class="fa-solid fa-arrow-up me-1"></i>${change.toFixed(1)}%`;
        } else {
            changeEl.className = 'badge bg-danger-subtle text-danger';
            changeEl.innerHTML = `<i class="fa-solid fa-arrow-down me-1"></i>${Math.abs(change).toFixed(1)}%`;
        }
    }
}

// 7. En Çok Satan 5 Ürün (Horizontal Bar Chart)
function createTopProductsChart(data) {
    const ctx = document.getElementById('topProductsChart');
    if (!ctx) return;

    if (chartInstances.topProducts) {
        chartInstances.topProducts.destroy();
    }

    // Gradient renkler
    const gradientColors = [
        colors.primary,
        colors.info,
        colors.success,
        colors.warning,
        colors.purple
    ];

    chartInstances.topProducts = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: data.map(d => truncateText(d.name, 20)),
            datasets: [{
                label: 'Satış Adedi',
                data: data.map(d => d.quantity),
                backgroundColor: gradientColors,
                borderRadius: 8,
                barPercentage: 0.7
            }]
        },
        options: {
            indexAxis: 'y',
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        title: function (context) {
                            return data[context[0].dataIndex].name;
                        },
                        label: function (context) {
                            const item = data[context.dataIndex];
                            return [
                                `Satış: ${formatNumber(item.quantity)} adet`,
                                `Gelir: ${formatCurrency(item.revenue)}`,
                                `Kategori: ${item.category}`
                            ];
                        }
                    }
                }
            },
            scales: {
                x: {
                    grid: { color: 'rgba(0,0,0,0.05)' },
                    ticks: {
                        callback: value => formatNumber(value)
                    }
                },
                y: {
                    grid: { display: false }
                }
            },
            onClick: (event, elements) => {
                if (elements.length > 0) {
                    const index = elements[0].index;
                    showProductDetail(data[index]);
                }
            }
        }
    });
}

// Yardımcı Fonksiyonlar

function createGradient(ctx, color) {
    const gradient = ctx.getContext('2d').createLinearGradient(0, 0, 0, 300);
    gradient.addColorStop(0, color + '40');
    gradient.addColorStop(1, color + '05');
    return gradient;
}

function truncateText(text, maxLength) {
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength - 3) + '...';
}

function showLoading() {
    document.getElementById('loadingOverlay').style.display = 'flex';
}

function hideLoading() {
    document.getElementById('loadingOverlay').style.display = 'none';
}

// Detay Modal/Popup fonksiyonları
function showDayDetail(date, data) {
    console.log('Day detail:', date, data);
    // TODO: Modal ile detay göster
}

function showCategoryDetail(category) {
    console.log('Category detail:', category);
    window.location.href = `/Product?categoryId=${category.categoryId}`;
}

function showCityDetail(city, state) {
    console.log('City detail:', city, state);
    // TODO: Modal ile şehir detayı göster
}

function showProductDetail(product) {
    window.location.href = `/Product/Edit/${product.id}`;
}

function highlightCategory(name) {
    if (chartInstances.categorySales) {
        const chart = chartInstances.categorySales;
        const dataIndex = chart.data.labels.indexOf(name);
        if (dataIndex >= 0) {
            chart.setActiveElements([{ datasetIndex: 0, index: dataIndex }]);
            chart.update();
        }
    }
}

// Grafik İndirme
function downloadChart(chartId, filename) {
    const canvas = document.getElementById(chartId);
    if (!canvas) return;

    const link = document.createElement('a');
    link.download = `${filename}-${new Date().toISOString().split('T')[0]}.png`;
    link.href = canvas.toDataURL('image/png');
    link.click();
}

// Tam Ekran Toggle
function toggleFullscreen(containerId) {
    const container = document.getElementById(containerId);
    if (!container) return;

    container.classList.toggle('chart-fullscreen');

    // Grafiği yeniden boyutlandır
    setTimeout(() => {
        Object.values(chartInstances).forEach(chart => {
            if (chart) chart.resize();
        });
    }, 100);
}

// Escape tuşuyla fullscreen'den çık
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        document.querySelectorAll('.chart-fullscreen').forEach(el => {
            el.classList.remove('chart-fullscreen');
        });
    }
});
