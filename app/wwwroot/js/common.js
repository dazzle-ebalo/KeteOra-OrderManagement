// Shared by every signed-in page: release label, signed-in user, logout and the active menu item

// The release label shown in the sidebar. Change this to show a new version has been deployed.
const RELEASE = 'v1';

// Fetches JSON from the API; if the session has expired, goes back to the login page
async function api(path) {
  const response = await fetch(path, { credentials: 'same-origin' });
  if (response.status === 401) {
    window.location.href = '/login.html';
    throw new Error('Signed out');
  }
  return response.json();
}

// Creates an element; textContent keeps data from being run as HTML
function el(tag, text, className) {
  const node = document.createElement(tag);
  if (text !== undefined) node.textContent = text;
  if (className) node.className = className;
  return node;
}

function money(value) {
  return '$' + Number(value).toLocaleString('en-NZ', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function emptyRow(body, columns, text) {
  const row = el('tr');
  const cell = el('td', text, 'empty');
  cell.colSpan = columns;
  row.appendChild(cell);
  body.appendChild(row);
}

document.addEventListener('DOMContentLoaded', async () => {
  document.getElementById('release').textContent = RELEASE;

  const path = window.location.pathname === '/index.html' ? '/' : window.location.pathname;
  for (const link of document.querySelectorAll('nav a')) {
    if (link.getAttribute('href') === path) link.classList.add('current');
  }

  document.getElementById('logout').addEventListener('click', async () => {
    await fetch('/api/logout', { method: 'POST' });
    window.location.href = '/login.html';
  });

  const me = await api('/api/me');
  document.getElementById('user-name').textContent = me.name;
  document.getElementById('avatar').textContent =
    me.name.split(' ').map(part => part[0]).join('').slice(0, 2).toUpperCase();

  const info = await api('/api/info');
  document.getElementById('server').textContent = info.server;
});
