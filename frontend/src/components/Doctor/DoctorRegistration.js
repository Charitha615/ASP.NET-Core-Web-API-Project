import React, { useState } from "react";
import axios from "axios";
import "./DoctorRegistration.css"; 
import Swal from "sweetalert2";
import { useNavigate } from "react-router-dom";

const DoctorRegistration = () => {
    const navigate = useNavigate(); 
  const [formData, setFormData] = useState({
    name: "",
    email: "",
    phone: "",
    specialty: "",
    licenseNumber: "",
    experience: "",
    password: "",
    confirmPassword: "",
  });

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    // Perform password confirmation check
    if (formData.password !== formData.confirmPassword) {
      alert("Passwords do not match!");
      return;
    }

    try {
      const response = await axios.post("https://localhost:5001/api/doctor/register", {
        fullName: formData.name,
        email: formData.email,
        phoneNumber: formData.phone,
        specialty: formData.specialty,
        licenseNumber: formData.licenseNumber,
        experienceYears: formData.experience,
        password: formData.password, // Assuming backend uses this or for extending auth functionality
      });

      console.log(response.data);
      Swal.fire({
        icon: "success",
        title: "Doctor Registered",
        text: "You have registered successfully!",
        timer: 2000,
        showConfirmButton: false,
      });
    //   alert("Doctor registered successfully!");
    setTimeout(() => {
        navigate("/login");
      }, 2000);
    } catch (error) {
      console.error("There was an error registering the doctor!", error);
      alert("Failed to register doctor. Please try again.");
    }
  };

  return (
    <div className="registration-container">
      <div className="registration-form">
        <h2>Doctor Registration</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="name">Full Name</label>
            <input
              type="text"
              id="name"
              name="name"
              value={formData.name}
              onChange={handleInputChange}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input
              type="email"
              id="email"
              name="email"
              value={formData.email}
              onChange={handleInputChange}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="phone">Phone</label>
            <input
              type="number"
              id="phone"
              name="phone"
              value={formData.phone}
              onChange={handleInputChange}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="specialty">Specialty</label>
            <input
              type="text"
              id="specialty"
              name="specialty"
              value={formData.specialty}
              onChange={handleInputChange}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="licenseNumber">License Number</label>
            <input
              type="text"
              id="licenseNumber"
              name="licenseNumber"
              value={formData.licenseNumber}
              onChange={handleInputChange}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="experience">Years of Experience</label>
            <input
              type="number"
              id="experience"
              name="experience"
              value={formData.experience}
              onChange={handleInputChange}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              type="password"
              id="password"
              name="password"
              value={formData.password}
              onChange={handleInputChange}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="confirmPassword">Confirm Password</label>
            <input
              type="password"
              id="confirmPassword"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleInputChange}
              required
            />
          </div>

          <button type="submit" className="submit-btn">
            Register
          </button>
        </form>
       
      </div>
    </div>
  );
};

export default DoctorRegistration;
