import React, { useEffect, useState } from 'react';
import { Box, Card, CardContent, Typography, Avatar, Grid, IconButton } from '@mui/material';
import EmailIcon from '@mui/icons-material/Email';
import PhoneIcon from '@mui/icons-material/Phone';
import WorkIcon from '@mui/icons-material/Work';
import BadgeIcon from '@mui/icons-material/Badge';
import ExperienceIcon from '@mui/icons-material/WorkspacePremium';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';

const DoctorDashboard = () => {
  const [doctorDetails, setDoctorDetails] = useState(null);

  useEffect(() => {
    // Fetch doctor details from localStorage or an API if needed
    const storedDoctorId = localStorage.getItem('doctorId');
    if (storedDoctorId) {
      // Simulate fetching doctor data (You can replace this with an API call if needed)
      const fakeDoctorData = {
        id: storedDoctorId,
        fullName: 'Dr. John Doe',
        email: 'john.doe@example.com',
        phoneNumber: '1234567890',
        specialty: 'Cardiology',
        licenseNumber: 'LIC12345',
        experienceYears: 10,
        avatarUrl: 'https://i.pravatar.cc/150?img=12', // Sample avatar image
      };
      setDoctorDetails(fakeDoctorData);
    }
  }, []);

  if (!doctorDetails) {
    return <Typography>Loading doctor details...</Typography>;
  }

  return (
    <Box sx={{ padding: 4, backgroundColor: '#f5f5f5', minHeight: '100vh' }}>
      <Typography variant="h4" gutterBottom>
        Welcome to Doctor Dashboard
      </Typography>
      
      {/* Doctor Profile Card */}
      <Card sx={{ maxWidth: 800, margin: 'auto', borderRadius: 3 }}>
        <CardContent>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={4} display="flex" justifyContent="center">
              <Avatar
                alt={doctorDetails.fullName}
                src={doctorDetails.avatarUrl}
                sx={{ width: 150, height: 150 }}
              />
            </Grid>

            <Grid item xs={12} sm={8}>
              <Typography variant="h5" component="div" gutterBottom>
                {doctorDetails.fullName}
              </Typography>

              <Grid container spacing={1} alignItems="center">
                <Grid item>
                  <IconButton>
                    <EmailIcon />
                  </IconButton>
                </Grid>
                <Grid item>
                  <Typography>{doctorDetails.email}</Typography>
                </Grid>
              </Grid>

              <Grid container spacing={1} alignItems="center">
                <Grid item>
                  <IconButton>
                    <PhoneIcon />
                  </IconButton>
                </Grid>
                <Grid item>
                  <Typography>{doctorDetails.phoneNumber}</Typography>
                </Grid>
              </Grid>

              <Grid container spacing={1} alignItems="center">
                <Grid item>
                  <IconButton>
                    <WorkIcon />
                  </IconButton>
                </Grid>
                <Grid item>
                  <Typography>Specialty: {doctorDetails.specialty}</Typography>
                </Grid>
              </Grid>

              <Grid container spacing={1} alignItems="center">
                <Grid item>
                  <IconButton>
                    <BadgeIcon />
                  </IconButton>
                </Grid>
                <Grid item>
                  <Typography>License: {doctorDetails.licenseNumber}</Typography>
                </Grid>
              </Grid>

              <Grid container spacing={1} alignItems="center">
                <Grid item>
                  <IconButton>
                    <ExperienceIcon />
                  </IconButton>
                </Grid>
                <Grid item>
                  <Typography>Experience: {doctorDetails.experienceYears} years</Typography>
                </Grid>
              </Grid>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Additional sections could be added here like recent appointments, schedule, etc. */}
      <Box sx={{ marginTop: 4 }}>
        <Typography variant="h5" gutterBottom>
          Quick Actions
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6} md={4}>
            <Card
              sx={{ textAlign: 'center', padding: 2, borderRadius: 3, backgroundColor: '#1976d2', color: 'white' }}
            >
              <AccountCircleIcon sx={{ fontSize: 40 }} />
              <Typography>View Profile</Typography>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={4}>
            <Card
              sx={{ textAlign: 'center', padding: 2, borderRadius: 3, backgroundColor: '#43a047', color: 'white' }}
            >
              <WorkIcon sx={{ fontSize: 40 }} />
              <Typography>Manage Appointments</Typography>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={4}>
            <Card
              sx={{ textAlign: 'center', padding: 2, borderRadius: 3, backgroundColor: '#f57c00', color: 'white' }}
            >
              <ExperienceIcon sx={{ fontSize: 40 }} />
              <Typography>View Experience</Typography>
            </Card>
          </Grid>
        </Grid>
      </Box>
    </Box>
  );
};

export default DoctorDashboard;
