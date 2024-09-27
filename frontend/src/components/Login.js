import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { TextField, Button, Box, Typography, Container, MenuItem, Select, InputLabel, FormControl } from '@mui/material';
import { API_BASE_URL } from '../config'; // Import global API URL
import Swal from 'sweetalert'; // Import SweetAlert

const Login = () => {
  const [userType, setUserType] = useState('normal'); // State for selecting user type (normal or doctor)
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [email, setEmail] = useState(''); // For doctor login
  const [licenseNumber, setLicenseNumber] = useState(''); // For doctor login
  const navigate = useNavigate();

  const handleLogin = async (e) => {
    e.preventDefault();

    try {
      let response;
      let data;
      
      if (userType === 'normal') {
        // Normal user login
        response = await fetch(`${API_BASE_URL}/Auth/login`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            username,
            password,
          }),
        });
        data = await response.json();

        if (response.ok) {
          // Store userId in localStorage
          localStorage.setItem('userId', data.userId);
          Swal('Success', 'Login successful', 'success');
          navigate('/home'); // Redirect to home for normal users
        } else {
          Swal('Error', data.message || 'Login failed', 'error');
        }
      } else if (userType === 'doctor') {
        // Doctor login
        response = await fetch(`https://localhost:5001/api/doctor/login`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            Email: email,
            LicenseNumber: licenseNumber,
          }),
        });
        data = await response.json();

        if (response.ok) {
          // Store doctorId in localStorage
          localStorage.setItem('doctorId', data.doctorDetails.id);
          Swal('Success', 'Login successful', 'success');
          navigate('/doctorDashboard'); // Redirect to doctor dashboard for doctors
        } else {
          Swal('Error', data.message || 'Login failed', 'error');
        }
      }
    } catch (error) {
      Swal('Error', 'Something went wrong', 'error');
    }
  };

  return (
    <Container component="main" maxWidth="xs">
      <Box
        sx={{
          marginTop: 8,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
        }}
      >
        <Typography component="h1" variant="h5">
          Login
        </Typography>

        {/* User Type Selection */}
        <FormControl fullWidth sx={{ mt: 2 }}>
          <InputLabel id="userTypeLabel">Login as</InputLabel>
          <Select
            labelId="userTypeLabel"
            id="userType"
            value={userType}
            label="Login as"
            onChange={(e) => setUserType(e.target.value)}
          >
            <MenuItem value="normal">Patient</MenuItem>
            <MenuItem value="doctor">Doctor</MenuItem>
          </Select>
        </FormControl>

        {/* Login Form */}
        <Box component="form" onSubmit={handleLogin} noValidate sx={{ mt: 1 }}>
          {userType === 'normal' ? (
            <>
              <TextField
                margin="normal"
                required
                fullWidth
                id="username"
                label="Username"
                name="username"
                autoComplete="username"
                autoFocus
                value={username}
                onChange={(e) => setUsername(e.target.value)}
              />
              <TextField
                margin="normal"
                required
                fullWidth
                name="password"
                label="Password"
                type="password"
                id="password"
                autoComplete="current-password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />
            </>
          ) : (
            <>
              <TextField
                margin="normal"
                required
                fullWidth
                id="email"
                label="Email"
                name="email"
                autoComplete="email"
                autoFocus
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
              <TextField
                margin="normal"
                required
                fullWidth
                name="licenseNumber"
                label="License Number"
                id="licenseNumber"
                value={licenseNumber}
                onChange={(e) => setLicenseNumber(e.target.value)}
              />
            </>
          )}

          <Button type="submit" fullWidth variant="contained" sx={{ mt: 3, mb: 2 }}>
            Sign In
          </Button>
          <Typography>
            Don't have an account?{' '}
            <Link to="/register" style={{ textDecoration: 'none' }}>
              Register
            </Link>
          </Typography>
        </Box>
      </Box>
    </Container>
  );
};

export default Login;
