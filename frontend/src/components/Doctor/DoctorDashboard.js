import React, { useEffect, useState } from 'react';
import { Box, Card, CardContent, Typography, Avatar, Grid, IconButton, TextField, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, Button, Modal } from '@mui/material';
import EmailIcon from '@mui/icons-material/Email';
import PhoneIcon from '@mui/icons-material/Phone';
import WorkIcon from '@mui/icons-material/Work';
import BadgeIcon from '@mui/icons-material/Badge';
import ExperienceIcon from '@mui/icons-material/WorkspacePremium';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';
import LocalHospitalIcon from '@mui/icons-material/LocalHospital';
import axios from 'axios';
import CloseIcon from '@mui/icons-material/Close';
import LogoutIcon from '@mui/icons-material/Logout';
import { useNavigate } from 'react-router-dom';
import DOMPurify from 'dompurify';

const API_BASE_URL = 'https://localhost:5001/api'; // Update to your base API URL

const DoctorDashboard = () => {
  const [doctorDetails, setDoctorDetails] = useState(null);
  const [patients, setPatients] = useState([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [filteredPatients, setFilteredPatients] = useState([]);
  const [showPatientTable, setShowPatientTable] = useState(false);
  const [appointments, setAppointments] = useState([]);
  const [open, setOpen] = useState(false); // Modal state
  const navigate = useNavigate();

  // Fetch doctor details from API using doctorId from localStorage
  useEffect(() => {
    const storedDoctorId = localStorage.getItem('userId');
    if (storedDoctorId) {
      axios
        .get(`${API_BASE_URL}/doctor/${storedDoctorId}`)
        .then(response => {
          setDoctorDetails(response.data); // Assuming API returns doctor data
        })
        .catch(error => {
          console.error('Error fetching doctor data:', error);
        });
    }
  }, []);

  // Fetch Patients
  useEffect(() => {
    // Get the token from localStorage
    const token = localStorage.getItem('token');
    const storedDoctorId = localStorage.getItem('userId');

    // If no token is found, handle the error (you may want to redirect to login or show an error message)
    if (!token) {
      console.error('No token found, please log in.');
      return;
    }

    // Make the request with the Authorization header
    // axios.get(`${API_BASE_URL}/auth/get-users`, {
      axios.get(`${API_BASE_URL}/auth/get-appointments/users-bydoctor/${storedDoctorId}`, {
      headers: {
        Authorization: `Bearer ${token}`, // Include the token in the Authorization header
      },
    })
      .then(response => {
        setPatients(response.data.data); // Assuming the response contains { data: [...] }
        setFilteredPatients(response.data.data);
      })
      .catch(error => {
        console.error('Error fetching patient data:', error);
      });
  }, []);



  // Fetch Appointments by User ID
  const fetchAppointments = (userId) => {
    axios.get(`${API_BASE_URL}/auth/get-appointments/${userId}`)
      .then(response => {
        setAppointments(response.data.data); // Assuming API response contains appointment data array
        setOpen(true); // Open the modal after fetching the data
      })
      .catch(error => {
        console.error('Error fetching appointment data:', error);
      });
  };

  // Handle search input change
  const handleSearch = (e) => {
    const searchValue = e.target.value.toLowerCase();
    setSearchTerm(searchValue);

    const filtered = patients.filter(patient =>
      patient.username.toLowerCase().includes(searchValue) ||
      String(patient.id).includes(searchValue)
    );
    setFilteredPatients(filtered);
  };

  // Handle showing patient table
  const handlePatientCardClick = () => {
    setShowPatientTable(true);
  };

  // Handle Close Modal
  const handleClose = () => setOpen(false);

  const handleLogout = () => {
    localStorage.clear(); // Clear all localStorage
    navigate('/login'); // Redirect to login page (change to the route you want)
  };

  if (!doctorDetails) {
    return <Typography>Loading doctor details...</Typography>;
  }

  // Fetch Appointments by Doctor ID when Manage Appointments is clicked
  const handleManageAppointmentsClick = () => {
    const storedDoctorId = localStorage.getItem('userId');

    axios.get(`${API_BASE_URL}/auth/get-appointments/bydoctor/${storedDoctorId}`)
      .then(response => {
        setAppointments(response.data.data); // Store the appointments in the state
        setOpen(true); // Open modal to show appointments
      })
      .catch(error => {
        console.error('Error fetching appointments:', error);
      });
  };


  return (
    <Box sx={{ padding: 4, backgroundColor: '#f5f5f5', minHeight: '100vh' }}>
      <Grid container justifyContent="space-between" alignItems="center" sx={{ mb: 4 }}>
        <Typography variant="h4">Welcome to Doctor Dashboard</Typography>

        {/* Logout Button */}
        <Button
          variant="contained"
          color="secondary"
          startIcon={<LogoutIcon />}
          onClick={handleLogout}
        >
          Logout
        </Button>
      </Grid>

      {/* Doctor Profile Card */}
      <Card sx={{ maxWidth: 800, margin: 'auto', borderRadius: 3 }}>
        <CardContent>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={4} display="flex" justifyContent="center">
              <Avatar
                alt={doctorDetails.fullName}
                src={doctorDetails.avatarUrl || 'https://i.pravatar.cc/150?img=12'} // Default avatar if not available
                sx={{ width: 150, height: 150 }}
              />
            </Grid>

            <Grid item xs={12} sm={8}>
              <Typography variant="h5" component="div" gutterBottom>
                {DOMPurify.sanitize(doctorDetails.fullName)}
              </Typography>

              {/* Doctor Details */}
              <Grid container spacing={1} alignItems="center">
                <Grid item><IconButton><EmailIcon /></IconButton></Grid>
                <Grid item><Typography>{doctorDetails.email}</Typography></Grid>
              </Grid>

              <Grid container spacing={1} alignItems="center">
                <Grid item><IconButton><PhoneIcon /></IconButton></Grid>
                <Grid item><Typography>{doctorDetails.phoneNumber}</Typography></Grid>
              </Grid>

              <Grid container spacing={1} alignItems="center">
                <Grid item><IconButton><WorkIcon /></IconButton></Grid>
                <Grid item><Typography>Specialty: {doctorDetails.specialty}</Typography></Grid>
              </Grid>

              <Grid container spacing={1} alignItems="center">
                <Grid item><IconButton><BadgeIcon /></IconButton></Grid>
                <Grid item><Typography>License: {doctorDetails.licenseNumber}</Typography></Grid>
              </Grid>

              <Grid container spacing={1} alignItems="center">
                <Grid item><IconButton><ExperienceIcon /></IconButton></Grid>
                <Grid item><Typography>Experience: {doctorDetails.experienceYears} years</Typography></Grid>
              </Grid>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Quick Actions */}
      <Box sx={{ marginTop: 4 }}>
        <Typography variant="h5" gutterBottom>
          Quick Actions
        </Typography>
        <Grid container spacing={2}>
          {/* <Grid item xs={12} sm={6} md={4}>
            <Card
              sx={{ textAlign: 'center', padding: 2, borderRadius: 3, backgroundColor: '#1976d2', color: 'white' }}
            >
              <AccountCircleIcon sx={{ fontSize: 40 }} />
              <Typography>View Profile</Typography>
            </Card>
          </Grid> */}
          <Grid item xs={12} sm={6} md={4}>
            <Card
              sx={{ textAlign: 'center', padding: 2, borderRadius: 3, backgroundColor: '#f57c00', color: 'white', cursor: 'pointer' }}
              onClick={handleManageAppointmentsClick} // Add this click handler
            >
              <WorkIcon sx={{ fontSize: 40 }} />
              <Typography>Manage Appointments</Typography>
            </Card>
          </Grid>

          {/* <Grid item xs={12} sm={6} md={4}>
            <Card
              sx={{ textAlign: 'center', padding: 2, borderRadius: 3, backgroundColor: '#f57c00', color: 'white' }}
            >
              <ExperienceIcon sx={{ fontSize: 40 }} />
              <Typography>View Experience</Typography>
            </Card>
          </Grid> */}
          <Grid item xs={12} sm={6} md={4}>
            <Card
              sx={{ textAlign: 'center', padding: 2, borderRadius: 3, backgroundColor: '#4caf50', color: 'white', cursor: 'pointer' }}
              onClick={handlePatientCardClick}
            >
              <LocalHospitalIcon sx={{ fontSize: 40 }} />
              <Typography>Patient</Typography>
            </Card>
          </Grid>
        </Grid>
      </Box>

      {/* Patient List Table - Only show when the card is clicked */}
      {showPatientTable && (
        <Box sx={{ marginTop: 4 }}>
          <Typography variant="h5" gutterBottom>
            Patient List
          </Typography>

          {/* Search Bar */}
          <TextField
            fullWidth
            label="Search by ID or Username"
            value={searchTerm}
            onChange={handleSearch}
            sx={{ marginBottom: 3 }}
          />

          {/* Patient Table */}
          <TableContainer component={Paper}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>ID</TableCell>
                  <TableCell>Username</TableCell>
                  <TableCell>Email</TableCell>
                  <TableCell>Action</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredPatients.map((patient) => (
                  <TableRow key={patient.userId}>
                    <TableCell>{patient.userId}</TableCell>
                    <TableCell>{patient.username}</TableCell>
                    <TableCell>{patient.email}</TableCell>
                    <TableCell>
                      <Button
                        variant="contained"
                        color="primary"
                        onClick={() => fetchAppointments(patient.userId)}
                      >
                        View
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </Box>
      )}

      {/* Modal for Viewing Appointment Details */}
      <Modal
        open={open}
        onClose={handleClose}
        aria-labelledby="appointment-modal-title"
        aria-describedby="appointment-modal-description"
      >
        <Box
          sx={{
            position: 'absolute',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            width: '80%',
            maxHeight: '80vh', // Limit the height of the modal
            bgcolor: 'background.paper',
            borderRadius: 2, // Rounded corners
            boxShadow: 24,
            p: 4,
            overflowY: 'auto' // Scrollable content
          }}
        >
          <Typography
            id="appointment-modal-title"
            variant="h5"
            component="h2"
            sx={{
              fontWeight: 'bold',
              marginBottom: 3,
              textAlign: 'center',
              color: '#1976d2' // Modern blue color
            }}
          >
            Appointments
          </Typography>

          {/* Scrollable Container for Appointments */}
          <Box
            sx={{
              maxHeight: '60vh', // Limit height to make it scrollable
              overflowY: 'auto', // Enable vertical scrolling
              paddingRight: 2 // Avoid content being hidden behind scrollbar
            }}
          >
            {appointments.length > 0 ? (
              appointments.map(appointment => (
                <Box
                  key={appointment.id}
                  sx={{
                    marginBottom: 3,
                    p: 3,
                    borderRadius: 2, // Rounded corners for each appointment
                    backgroundColor: '#fff',
                    boxShadow: '0px 4px 8px rgba(0, 0, 0, 0.1)', // Slight shadow for card-like feel
                    transition: 'transform 0.3s ease-in-out',
                    '&:hover': {
                      transform: 'scale(1.02)', // Scale on hover for a modern effect
                    }
                  }}
                >
                  <Typography variant="h6" gutterBottom sx={{ color: '#333' }}>
                    {appointment.name}
                  </Typography>
                  <Typography variant="body2" gutterBottom>
                    <strong>Patient Name:</strong> {appointment.userDetails.username}
                  </Typography>
                  <Typography variant="body2" gutterBottom>
                    <strong>Patient Email:</strong> {appointment.userDetails.email}
                  </Typography>
                  <Typography variant="body2" gutterBottom>
                    <strong>Age:</strong> {appointment.age}
                  </Typography>
                  <Typography variant="body2" gutterBottom>
                    <strong>Medical History:</strong> {appointment.medicalHistory}
                  </Typography>
                  <Typography variant="body2" gutterBottom>
                    <strong>Treatment Schedule:</strong> {appointment.treatmentSchedule}
                  </Typography>
                  <Typography variant="body2" gutterBottom>
                    <strong>Medications:</strong> {appointment.medications}
                  </Typography>
                  <Typography variant="body2" gutterBottom>
                    <strong>Contact:</strong> {appointment.contact}
                  </Typography>
                  <Typography variant="body2" gutterBottom>
                    <strong>Doctor:</strong> {appointment.doctorDetails.fullName}
                  </Typography>
                </Box>
              ))
            ) : (
              <Typography variant="body2" color="textSecondary">
                No appointments found.
              </Typography>
            )}
          </Box>

          {/* Close Button */}
          <Box sx={{ textAlign: 'center', marginTop: 2 }}>
            <Button
              variant="contained"
              color="primary"
              onClick={handleClose}
              sx={{
                borderRadius: '20px',
                padding: '8px 16px',
                fontWeight: 'bold',
              }}
            >
              <CloseIcon sx={{ marginRight: 1 }} />
              Close
            </Button>
          </Box>
        </Box>
      </Modal>

    </Box>
  );
};

export default DoctorDashboard;
