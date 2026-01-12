import RegistrationForm from './components/RegistrationForm';
import './App.css';

function App() {
  return (
    <div className="app">
      <div className="background-decoration">
        <div className="orb orb-1"></div>
        <div className="orb orb-2"></div>
        <div className="orb orb-3"></div>
      </div>
      <main className="main-content">
        <RegistrationForm />
      </main>
      <footer className="footer">
        <p>
          Built with <span className="heart">♥</span> using{' '}
          <code>useFormValidation</code> hook
        </p>
      </footer>
    </div>
  );
}

export default App;
