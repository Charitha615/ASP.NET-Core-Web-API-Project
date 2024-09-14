import React from 'react';
import { Route, Routes, Navigate } from 'react-router-dom';
import Login from './components/Login';
import Register from './components/Register';
import Home from './components/Home';
import Navbar from './components/Navbar/Navbar';  // Import Navbar
import AppointmentForm from './components/AppointmentForm/AppointmentForm'; // Import the new component

// Create a ProtectedRoute component
const ProtectedRoute = ({ children }) => {
  const userId = localStorage.getItem('userId'); // Check if userId exists in localStorage

  if (!userId) {
    // If no userId, redirect to login
    return <Navigate to="/login" replace />;
  }

  return children; // If userId exists, render the protected component
};

function App() {
  return (
    <>
      {/* Add Navbar here */}
      <Routes>
        <Route path="/" element={<Navigate replace to="/login" />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/appointment" element={<AppointmentForm />} />

        {/* Protect the /home route */}
        <Route
          path="/home"
          element={
            <ProtectedRoute>
              <Home />
            </ProtectedRoute>
          }
        />
      </Routes>
    </>
  );
}

export default App;
