import React from 'react';
import './Home.css'; // Create this for the styling
import { useNavigate } from 'react-router-dom';

const Home = () => {
  const navigate = useNavigate();
  return (
    <div>
      {/* Header with Navbar */}
      <header className="header">
        <div className="container">
          <div className="logo">Health Haven Hospital</div>
          <nav className="nav">
            <ul>
              <li><a href="#services">Services</a></li>
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


      {/* Hero Section */}
      <section className="hero">
        <div className="hero-content">
          <h1>Consult with Experts Online 24/7</h1>
          <p>Get Online support from an expert Doctor 24/7 and lead a healthy life</p>
          <button
            className="cta-button"
            onClick={() => navigate('/appointment')}
            style={{ width: '200px', padding: '15px 30px', fontSize: '16px', marginTop: '30px' }}>
            Make an Appointment
          </button>
        </div>
      </section>


      {/* Services Section */}
      <section id="services" className="services">
        <h2>Why Choose Our Medical</h2>
        <p>Breakthrough in Comprehensive, Flexible Care Delivery Models</p>
        <div className="service-cards">
          <div className="service-card">
            <img src="https://st3.depositphotos.com/1177973/12825/i/450/depositphotos_128254290-stock-photo-cardiologist-holding-red-heart-with.jpg" alt="Cardiology" height={200} />
            <h3>Consult for Cardiology</h3>
            <p>See More Details...</p>
          </div>
          <div className="service-card">
            <img src="https://smf.in/wp-content/uploads/2023/02/neurology-1.png" alt="Neurology" width={100} height={200} />
            <h3>Consult for Neurology</h3>
            <p>See More Details...</p>
          </div>
          <div className="service-card">
            <img src="https://img.freepik.com/premium-vector/human-stomach-illustration-white-background_756535-7661.jpg" alt="Stomach" height={200} />
            <h3>Consult for Stomach</h3>
            <p>See More Details...</p>
          </div>
        </div>
      </section>

      {/* Information Section */}
      <section className="info-section">
        <h2>All-in-One Website Health Solution</h2>
        <p>10 Years Of Experience in Medical Services</p>

        <div className="info-cards">
          <div className="info-card">
            <img src="https://b2b.healthgrades.com/wp-content/uploads/2024/04/Patient-Experience-Recipient-Blog-Hero.webp" alt="Experience" width={100} height={200} />
            <h3>Experienced Doctors</h3>
            <p>We have 10+ years of experience in providing medical services worldwide.</p>
          </div>
          <div className="info-card">
            <img src="https://hospitalnews.com/wp-content/uploads/2014/02/Dollarphotoclub_76406533.jpg" alt="Technology" />
            <h3>Advanced Technology</h3>
            <p>Our hospital uses state-of-the-art technology for all medical procedures.</p>
          </div>
          <div className="info-card">
            <img src="https://bhoomihospitals.com/wp-content/uploads/2024/09/doctor-performing-cpr-patient-highstress-situation-1024x574.jpg" alt="Patient Care" />
            <h3>24/7 Patient Care</h3>
            <p>We offer round-the-clock patient care and emergency services.</p>
          </div>
        </div>
      </section>


      {/* Expert Team Section */}
      <section id="doctors" className="expert-team">
        <h2>Meet Our Expert Team</h2>
        <div className="team-members">
          <div className="member">
            <img src="https://wascal.org/wp-content/uploads/2017/11/doctor1.jpg" alt="Dr. Lee" />
            <h3>Dr. Lee S. Williamson</h3>
            <p>Specialist in Cardiology</p>
          </div>
          <div className="member">
            <img src="https://wascal.org/wp-content/uploads/2017/11/doctor1.jpg" alt="Dr. Greg" />
            <h3>Dr. Greg S. Grinstead</h3>
            <p>Specialist in Neurology</p>
          </div>
          <div className="member">
            <img src="https://wascal.org/wp-content/uploads/2017/11/doctor1.jpg" alt="Dr. Roger" />
            <h3>Dr. Roger K. Jackson</h3>
            <p>Specialist in Orthopedics</p>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="footer">
        <div className="container">
          <div className="footer-info">
            <p><strong>Health Haven Hospital</strong></p>
            <p>Tajgaon UA, Dhaka-1208</p>
            <p>Email: mh.hoyal@gmail.com | Phone: 01820xxxx65</p>
          </div>
          <div className="footer-social">
            <p>Follow us on:</p>
            <a href="#">Facebook</a> | <a href="#">LinkedIn</a> | <a href="#">GitHub</a>
          </div>
        </div>
      </footer>
    </div>
  );
};

export default Home;
