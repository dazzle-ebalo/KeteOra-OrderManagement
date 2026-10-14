// Orders page: summary strip, status tabs, search and the orders table

let allOrders = [];
let currentFilter = 'All';

function dateText(iso) {
  return new Date(iso).toLocaleString('en-NZ', { day: 'numeric', month: 'short', hour: 'numeric', minute: '2-digit' });
}

function badgeCell(text, kind) {
  const cell = el('td');
  cell.appendChild(el('span', text, 'badge ' + kind));
  return cell;
}

function render() {
  const term = document.getElementById('search').value.trim().toLowerCase();
  const body = document.getElementById('orders');
  body.replaceChildren();

  const shown = allOrders.filter(o =>
    (currentFilter === 'All' || o.fulfillment === currentFilter) &&
    (term === '' || ('#' + o.id).includes(term) || o.customer.toLowerCase().includes(term)));

  for (const o of shown) {
    const row = el('tr');
    row.append(
      el('td', '#' + o.id, 'strong'),
      el('td', dateText(o.placedAt), 'muted'),
      el('td', o.customer),
      el('td', o.city, 'muted'),
      el('td', money(o.total), 'num'),
      badgeCell(o.payment, 'pay-' + o.payment.toLowerCase()),
      badgeCell(o.fulfillment, 'ful-' + o.fulfillment.toLowerCase().replace(' ', '-')),
      el('td', o.items + (o.items === 1 ? ' item' : ' items'), 'num muted'),
      el('td', o.delivery, 'muted'));
    body.appendChild(row);
  }
  if (shown.length === 0) emptyRow(body, 9, 'No orders match.');
}

api('/api/orders').then(orders => {
  allOrders = orders;
  document.getElementById('sum-orders').textContent = orders.length;
  document.getElementById('sum-items').textContent = orders.reduce((n, o) => n + o.items, 0);
  document.getElementById('sum-unfulfilled').textContent = orders.filter(o => o.fulfillment === 'Unfulfilled').length;
  document.getElementById('sum-revenue').textContent =
    money(orders.filter(o => o.payment === 'Paid').reduce((n, o) => n + o.total, 0));
  render();
});

for (const tab of document.querySelectorAll('.tab')) {
  tab.addEventListener('click', () => {
    document.querySelectorAll('.tab').forEach(t => t.classList.remove('current'));
    tab.classList.add('current');
    currentFilter = tab.dataset.filter;
    render();
  });
}

document.getElementById('search').addEventListener('input', render);
