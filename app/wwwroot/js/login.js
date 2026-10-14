// Sign-in form: sends the credentials as JSON; the server answers with an HttpOnly session cookie

document.getElementById('login-form').addEventListener('submit', async event => {
  event.preventDefault();
  const message = document.getElementById('message');
  const password = document.getElementById('password');
  message.textContent = '';

  const response = await fetch('/api/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username: document.getElementById('username').value, password: password.value })
  });

  if (response.ok) {
    window.location.href = '/';
    return;
  }

  password.value = '';
  message.textContent =
    response.status === 429 ? 'Too many attempts. Wait a minute and try again.' :
    response.status === 503 ? "The admin account isn't set up on this server yet." :
    'Incorrect username or password.';
});
