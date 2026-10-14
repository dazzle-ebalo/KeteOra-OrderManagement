// Customers page: customer list with New Zealand delivery addresses and a search box

let allCustomers = [];

function render() {
  const term = document.getElementById('search').value.trim().toLowerCase();
  const body = document.getElementById('customers');
  body.replaceChildren();

  const shown = allCustomers.filter(c =>
    term === '' ||
    c.name.toLowerCase().includes(term) ||
    c.email.toLowerCase().includes(term) ||
    c.address.toLowerCase().includes(term));

  for (const c of shown) {
    const row = el('tr');
    row.append(
      el('td', c.name, 'strong'),
      el('td', c.email, 'muted'),
      el('td', c.phone, 'muted'),
      el('td', c.address),
      el('td', c.region, 'muted'),
      el('td', c.orders, 'num'),
      el('td', money(c.spent), 'num'));
    body.appendChild(row);
  }
  if (shown.length === 0) emptyRow(body, 7, 'No customers match.');
}

api('/api/customers').then(customers => {
  allCustomers = customers;
  render();
});

document.getElementById('search').addEventListener('input', render);
