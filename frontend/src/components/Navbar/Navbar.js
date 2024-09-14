// Navbar.js
import React from 'react';
import { Link } from 'react-router-dom';
import './Navbar.css';  // We'll add styles here later

const Navbar = () => {
  return (
    <nav className="navbar">
      <h1>My App</h1>
      <ul className="nav-links">
        <li><Link to="/home">Home</Link></li>
        <li><Link to="/profile">Profile</Link></li>
        <li><Link to="/about">About</Link></li>
        <li><Link to="/contact">Contact</Link></li>
      </ul>
    </nav>
  );
};

export default Navbar;
