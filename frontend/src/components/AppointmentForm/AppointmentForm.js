import React, { useState, useEffect } from 'react';
import './AppointmentForm.css'; // Assuming you have styles
import { API_BASE_URL } from '../../config'; // Import global API URL

const AppointmentForm = () => {
  const [formData, setFormData] = useState({
    name: '',
    age: '',
    medicalHistory: '',
    treatmentSchedule: '',
    medications: '',
    contact: '',
    userID: localStorage.getItem("userId"),
    DoctorID: '',
    DoctorName: '',
  });

  const [appointments, setAppointments] = useState([]);
  const [doctors, setDoctors] = useState([]); // Will hold doctor data fetched from the API
  const [selectedDoctor, setSelectedDoctor] = useState('');

  // Fetch doctors from API
  useEffect(() => {
    const fetchDoctors = async () => {
      try {
        const response = await fetch(`https://localhost:5001/api/doctor/all`, {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
          },
        });
        const data = await response.json();
        setDoctors(data); // Assuming API response directly returns the list of doctors
      } catch (error) {
        console.error('Error fetching doctors:', error);
      }
    };

    fetchDoctors();
  }, []);

  // Fetch appointments from the API
  useEffect(() => {
    const fetchAppointments = async () => {
      try {
        // Retrieve userId from local storage
        const userId = localStorage.getItem('userId');
        if (!userId) {
          console.error('User ID not found in local storage');
          return;
        }

        // Fetch appointments for the specific user
        const response = await fetch(`${API_BASE_URL}/get-appointments/${userId}`, {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
          },
        });

        if (!response.ok) {
          throw new Error('Failed to fetch appointments');
        }

        const data = await response.json();
        setAppointments(data.data); // Assuming API response has the "data" key
      } catch (error) {
        console.error('Error fetching appointments:', error);
      }
    };

    fetchAppointments();
  }, []);

  // Handle form input changes
  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
  };

  // Handle doctor selection
  const handleDoctorChange = (e) => {
    const selectedDoctorId = e.target.value;
    const selectedDoctorName = doctors.find(doc => doc.id == selectedDoctorId)?.fullName || '';

    setSelectedDoctor(selectedDoctorId);
    setFormData({
      ...formData,
      DoctorID: selectedDoctorId,
      DoctorName: selectedDoctorName // Set the doctor's name here
    });
  };

  // Handle form submission
  const handleSubmit = async (e) => {
    e.preventDefault();

    // Retrieve the token from localStorage
    const token = localStorage.getItem('token');

    // If no token is found, handle the error (e.g., redirect to login or show an error message)
    if (!token) {
      alert('You are not authorized. Please log in.');
      return;
    }

    try {
      const response = await fetch(`${API_BASE_URL}/submit-appointment`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`, // Include the token in the Authorization header
        },
        body: JSON.stringify(formData), // Assuming `formData` is the data you're submitting
      });

      if (response.ok) {
        const data = await response.json();
        alert('Appointment saved successfully!');
        window.location.reload();
      } else {
        const errorData = await response.json();
        alert('Failed to save appointment: ' + errorData.message);
      }
    } catch (error) {
      console.error('Error:', error);
      alert('An error occurred while submitting the form.');
    }
  };


  return (
    <div>
      <header className="header">
        <div className="container">
          <div className="logo">Health Haven Hospital</div>
          <nav className="nav">
            <ul>
              <li><a href="/home">Home</a></li>
              <li><a href="#doctors">Doctors</a></li>
              <li><a href="#appointment">Appointment</a></li>
              <li><a href="#about">About</a></li>
              <li>
                <a
                  href="/login"
                  onClick={() => {
                    // Clear localStorage on logout
                    localStorage.clear();
                  }}
                >
                  Logout
                </a>
              </li>
            </ul>
          </nav>
        </div>
      </header>

      <div className="appointment-page" style={{ backgroundImage: 'url("/path-to-background-image.jpg")', backgroundSize: 'cover', backgroundPosition: 'center' }}>
        {/* Doctor Selection */}
        <div className="doctor-selection-container">
          <h2>Select a Doctor</h2>
          <select value={selectedDoctor} onChange={handleDoctorChange}>
            <option value="">Choose a doctor</option>
            {doctors.map((doctor) => (
              <option key={doctor.id} value={doctor.id}>
                {doctor.fullName} ({doctor.specialty})
              </option>
            ))}
          </select>
        </div>

        {selectedDoctor && (
          <div className="appointment-form-container">
            <h2>Schedule an Appointment with {doctors.find(doc => doc.id == selectedDoctor)?.fullName}</h2>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Doctor Name (Readonly)</label>
                <input
                  type="text"
                  name="doctorName"
                  value={doctors.find(doc => doc.id == selectedDoctor)?.fullName || ''}
                  readOnly
                />
              </div>
              <div className="form-group">
                <label>Patient Name</label>
                <input
                  type="text"
                  name="name"
                  value={formData.name}
                  onChange={handleChange}
                  required
                />
              </div>
              <div className="form-group">
                <label>Age</label>
                <input
                  type="number"
                  name="age"
                  value={formData.age}
                  onChange={handleChange}
                  required
                />
              </div>
              <div className="form-group">
                <label>Medical History</label>
                <textarea
                  name="medicalHistory"
                  value={formData.medicalHistory}
                  onChange={handleChange}
                ></textarea>
              </div>
              <div className="form-group">
                <label>Treatment Schedule</label>
                <input
                  type="text"
                  name="treatmentSchedule"
                  value={formData.treatmentSchedule}
                  onChange={handleChange}
                />
              </div>
              <div className="form-group">
                <label>Medications</label>
                <textarea
                  name="medications"
                  value={formData.medications}
                  onChange={handleChange}
                ></textarea>
              </div>
              <div className="form-group">
                <label>Contact Info</label>
                <input
                  type="text"
                  name="contact"
                  value={formData.contact}
                  onChange={handleChange}
                  required
                />
              </div>
              <button type="submit" className="submit-btn">Submit</button>
            </form>
          </div>
        )}

        {/* Display fetched appointments */}
        <div className="appointments-list">
          <h2>Upcoming Appointments</h2>
          {appointments.length > 0 ? (
            <ul>
              {appointments.map((appointment) => (
                <li key={appointment.id}>
                  <div className="appointment-info">
                    <span className="patient-name">{appointment.name}</span>
                    <span className="appointment-status">Upcoming</span>
                  </div>
                  <div className="appointment-details">
                    <p><strong>Age:</strong> {appointment.age}</p>
                    <p><strong>Medical History:</strong> {appointment.medicalHistory}</p>
                    <p><strong>Treatment Schedule:</strong> {appointment.treatmentSchedule}</p>
                    <p><strong>Medications:</strong> {appointment.medications}</p>
                    <p><strong>Contact:</strong> {appointment.contact}</p>
                  </div>
                </li>
              ))}
            </ul>
          ) : (
            <p>No appointments available</p>
          )}
        </div>
      </div>
    </div>
  );
};

export default AppointmentForm;
