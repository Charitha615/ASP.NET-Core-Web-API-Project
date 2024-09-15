// AppointmentForm.js
import React, { useState } from 'react';
import './AppointmentForm.css'; // Create styles for the form
import { API_BASE_URL } from '../../config'; // Import global API URL

const AppointmentForm = () => {
  const [step, setStep] = useState(1);
  const [checkAPINum, setcheckAPINum] = useState(1);
  const [formData, setFormData] = useState({
    name: '',
    age: '',
    medicalHistory: '',
    treatmentSchedule: '',
    medications: '',
    contact: '',
    userID: localStorage.getItem("userId"),
  });

  const checkAPI = () => {
    setcheckAPINum(2); // Just update state here
  };

  // Handle form input changes
  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
  };

  // Proceed to the next step
  const nextStep = () => {
    setStep(step + 1);
  };

  // Go back to the previous step
  const prevStep = () => {
    setStep(step - 1);
  };

  // Handle form submission
  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (checkAPINum === 2) { // Ensure that the API call is made on the correct step
      try {
        const response = await fetch(`${API_BASE_URL}/submit-appointment`, { 
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(formData),
        });
    
        if (response.ok) {
          const data = await response.json();
          alert('Appointment saved successfully!');
        } else {
          const errorData = await response.json();
          alert('Failed to save appointment: ' + errorData.message);
        }
      } catch (error) {
        console.error('Error:', error);
        alert('An error occurred while submitting the form.');
      }
    }
  };

  // Render different form fields based on the current step
  const renderStep = () => {
    switch (step) {
      case 1:
        return (
          <div className="form-step">
            <h2>Patient Information</h2>
            <label>Name</label>
            <input type="text" name="name" value={formData.name} onChange={handleChange} />
            <label>Age</label>
            <input type="number" name="age" value={formData.age} onChange={handleChange} />
            <button type="button" onClick={nextStep}>Next</button>
          </div>
        );
      case 2:
        return (
          <div className="form-step">
            <h2>Medical History</h2>
            <label>Medical History</label>
            <textarea name="medicalHistory" value={formData.medicalHistory} onChange={handleChange}></textarea>
            <label>Treatment Schedule</label>
            <input type="text" name="treatmentSchedule" value={formData.treatmentSchedule} onChange={handleChange} />
            <button type="button" onClick={prevStep}>Back</button>
            <button type="button" onClick={nextStep}>Next</button>
          </div>
        );
      case 3:
        return (
          <div className="form-step">
            <h2>Prescribed Medications</h2>
            <label>Medications</label>
            <textarea name="medications" value={formData.medications} onChange={handleChange}></textarea>
            <label>Contact Info</label>
            <input type="text" name="contact" value={formData.contact} onChange={handleChange} />
            <button type="button" onClick={prevStep}>Back</button>
            <button type="submit" onClick={checkAPI}>Submit</button> {/* This will submit the form */}
          </div>
        );
      default:
        return null;
    }
  };

  return (
    <div className="appointment-form-container">
      <form onSubmit={handleSubmit}>
        {renderStep()}
      </form>
    </div>
  );
};

export default AppointmentForm;
