// Analytics page: key figures, two bar charts and the top products table

function bars(containerId, rows, label, value, format) {
  const container = document.getElementById(containerId);
  container.replaceChildren();
  const max = Math.max(...rows.map(value), 1);
  for (const row of rows) {
    const line = el('div', undefined, 'bar-row');
    const track = el('div', undefined, 'bar-track');
    const fill = el('div', undefined, 'bar-fill');
    fill.style.width = (value(row) / max * 100) + '%';
    track.appendChild(fill);
    line.append(el('span', label(row)), track, el('span', format(value(row)), 'bar-value'));
    container.appendChild(line);
  }
}

api('/api/analytics').then(data => {
  document.getElementById('sum-orders').textContent = data.orders;
  document.getElementById('sum-revenue').textContent = money(data.paidRevenue);
  document.getElementById('sum-average').textContent = money(data.averageOrderValue);
  document.getElementById('sum-items').textContent = data.itemsSold;

  bars('by-fulfillment', data.byFulfillment, r => r.status, r => r.count, v => v + (v === 1 ? ' order' : ' orders'));
  bars('by-region', data.revenueByRegion, r => r.region, r => r.revenue, money);

  const body = document.getElementById('top-products');
  body.replaceChildren();
  for (const p of data.topProducts) {
    const row = el('tr');
    row.append(el('td', p.product, 'strong'), el('td', p.quantity, 'num'), el('td', money(p.revenue), 'num'));
    body.appendChild(row);
  }
});
