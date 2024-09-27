import React from 'react';
import { Route, Routes, Navigate } from 'react-router-dom';
import Login from './components/Login';
import Register from './components/Register';
import Home from './components/Home';
import Navbar from './components/Navbar/Navbar';  
import AppointmentForm from './components/AppointmentForm/AppointmentForm'; 
import DoctorRegistration from './components/Doctor/DoctorRegistration';
import DoctorDashboard from './components/Doctor/DoctorDashboard'

// Create a ProtectedRoute component
const ProtectedRoute = ({ children }) => {
  const userId = localStorage.getItem('userId'); 

  if (!userId) {
    // If no userId, redirect to login
    return <Navigate to="/login" replace />;
  }

  return children; 
};

function App() {
  return (
    <>
      
      <Routes>
        <Route path="/" element={<Navigate replace to="/login" />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/appointment" element={<AppointmentForm />} />
        <Route path="/doctor-register" element={<DoctorRegistration />} />
        <Route path="/doctorDashboard" element={<DoctorDashboard />} />


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
