import { jwtDecode } from 'jwt-decode';

const TOKEN_KEY = 'token';
const USER_ID_KEY = 'userId'; // Can be changed to 'doctorId' based on the type of user

// Save token and user-related information in localStorage
export function saveAuthData(token, userId) {
  localStorage.setItem(TOKEN_KEY, token);
  localStorage.setItem(USER_ID_KEY, userId);
}

// Get the token from localStorage
export function getToken() {
  return localStorage.getItem(TOKEN_KEY);
}

// Get the user ID (either doctorId or userId)
export function getUserId() {
  return localStorage.getItem(USER_ID_KEY);
}

// Get the expiration time of the token
export function getTokenExpiration(token) {
    const decodedToken = jwtDecode(token); // Update here
    if (decodedToken.exp) {
      return decodedToken.exp * 1000; // Convert to milliseconds
    }
    return null;
  }
  

// Check if the token is expired
export function isTokenExpired(token) {
  const expirationTime = getTokenExpiration(token);
  if (!expirationTime) return true; // If there's no expiration, consider the token expired
  return Date.now() > expirationTime;
}

// Remove token and user-related data from localStorage and log the user out
export function logout() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_ID_KEY);
  window.location.href = '/login'; // Redirect to login page
}

// Check token expiration and set up auto-logout
export function setupAutoLogout() {
  const token = getToken();
  if (token && !isTokenExpired(token)) {
    const expirationTime = getTokenExpiration(token);
    const timeout = expirationTime - Date.now();
    setTimeout(() => {
      logout(); // Log the user out when the token expires
    }, timeout);
  } else {
    logout(); // Log the user out if the token is expired on page load
  }
}
