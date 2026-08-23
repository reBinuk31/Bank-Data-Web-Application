    // Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// site.js

// Load the default or error or logout view dynamically
function loadView(status) {
    let apiUrl = '/api/login/defaultview';
    if (status === 'error') apiUrl = '/api/login/error';
    else if (status === 'logout') apiUrl = '/api/login/logout';

    console.log('Loading view for status:', status);

    fetch(apiUrl)
        .then(response => response.text())
        .then(data => {
            document.getElementById('main').innerHTML = data;
            if (status === 'logout') {
                const logoutButton = document.getElementById('LogoutButton');
                if (logoutButton) logoutButton.style.display = "none";
            }
        })
        .catch(error => console.error('Error loading view:', error));
}

// Called when user clicks "Login"
async function performAuth() {
    const name = document.getElementById('SName').value;
    const password = document.getElementById('SPass').value;

    const data = { Username: name, Password: password };

    try {
        const response = await fetch("/api/Login/auth", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            throw new Error("Login request failed: " + response.statusText);
        }

        const result = await response.json();

        if (result.login) {
            // ✅ Redirect to dashboard
            window.location.href = result.redirect;
        } else {
            alert("Invalid username or password.");
        }
    } catch (error) {
        console.error("Login error:", error);
        alert("Server error. Please try again later.");
    }
}

document.addEventListener('DOMContentLoaded', () => {
    loadView(); // Loads default login view when page loads
});
