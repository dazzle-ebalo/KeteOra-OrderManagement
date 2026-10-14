// Products page: stock summary and the product list

function stockBadge(stock) {
  const cell = el('td');
  if (stock === 0) cell.appendChild(el('span', 'Out of stock', 'badge stock-out'));
  else if (stock < 5) cell.appendChild(el('span', 'Low stock', 'badge stock-low'));
  else cell.appendChild(el('span', 'In stock', 'badge stock-in'));
  return cell;
}

api('/api/products').then(products => {
  document.getElementById('sum-products').textContent = products.length;
  document.getElementById('sum-stock').textContent = products.reduce((n, p) => n + p.stock, 0);
  document.getElementById('sum-low').textContent = products.filter(p => p.stock > 0 && p.stock < 5).length;
  document.getElementById('sum-out').textContent = products.filter(p => p.stock === 0).length;

  const body = document.getElementById('products');
  body.replaceChildren();
  for (const p of products) {
    const row = el('tr');
    row.append(
      el('td', p.sku, 'muted'),
      el('td', p.name, 'strong'),
      el('td', p.category, 'muted'),
      el('td', money(p.price), 'num'),
      el('td', p.stock, 'num'),
      stockBadge(p.stock),
      el('td', p.sold, 'num'));
    body.appendChild(row);
  }
  if (products.length === 0) emptyRow(body, 7, 'No products yet.');
});
