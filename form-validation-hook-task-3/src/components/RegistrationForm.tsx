import React from 'react';
import { useFormValidation, required, minLength, emailFormat, maxLength } from '../hooks';
import './RegistrationForm.css';

/**
 * Define the fields for our registration form.
 * This type ensures type safety throughout the component.
 */
type RegistrationFields = 'username' | 'email' | 'password' | 'confirmPassword' | 'bio';

/**
 * Registration Form Component
 * 
 * Demonstrates the useFormValidation hook with multiple fields
 * and various validation rules.
 */
const RegistrationForm: React.FC = () => {
  // Initialize the form validation hook with our validation rules
  const {
    fields,
    isValid,
    isDirty,
    values,
    validateForm,
    resetForm,
    getFieldProps,
  } = useFormValidation<RegistrationFields>({
    username: [
      required('Username is required'),
      minLength(3, 'Username must be at least 3 characters'),
      maxLength(20, 'Username cannot exceed 20 characters'),
    ],
    email: [
      required('Email is required'),
      emailFormat('Please enter a valid email address'),
    ],
    password: [
      required('Password is required'),
      minLength(8, 'Password must be at least 8 characters'),
    ],
      confirmPassword: [
          required('Please confirm your password'),
          (value: string, fields: any) => {
              return value !== fields.password.value && fields.password.value !== ''
                  ? 'Passwords do not match'
                  : null;
          },
      ],


    bio: [
      maxLength(200, 'Bio cannot exceed 200 characters'),
    ],
           });

  /**
   * Handle form submission
   */
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    // Validate all fields before submitting
    const isFormValid = validateForm();
    
    if (isFormValid) {
      // Form is valid - show success message
      console.log('Form submitted successfully!', values);
      alert(`Registration successful!\n\nUsername: ${values.username}\nEmail: ${values.email}`);
      resetForm();
    } else {
      console.log('Form has validation errors');
    }
  };

  /**
   * Render a form field with label and error message
   */
  const renderField = (
    fieldName: RegistrationFields,
    label: string,
    type: string = 'text',
    placeholder: string = ''
  ) => {
    const field = fields[fieldName];
    const showError = field.touched && field.error;

    return (
      <div className={`form-field ${showError ? 'has-error' : ''} ${field.touched && !field.error ? 'is-valid' : ''}`}>
        <label htmlFor={fieldName}>{label}</label>
        {type === 'textarea' ? (
          <textarea
            id={fieldName}
            {...getFieldProps(fieldName)}
            placeholder={placeholder}
            rows={4}
          />
        ) : (
          <input
            id={fieldName}
            type={type}
            {...getFieldProps(fieldName)}
            placeholder={placeholder}
          />
        )}
        {showError && (
          <span className="error-message" role="alert">
            <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
              <path d="M8 4v4M8 11h.01" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round"/>
              <circle cx="8" cy="8" r="7" stroke="currentColor" strokeWidth="1.5"/>
            </svg>
            {field.error}
          </span>
        )}
        {field.touched && !field.error && (
          <span className="success-indicator">
            <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
              <path d="M3 8.5L6 11.5L13 4.5" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
            </svg>
          </span>
        )}
      </div>
    );
  };

  return (
    <div className="form-container">
      <div className="form-header">
        <h1>Create Account</h1>
        <p>Join us today and start your journey</p>
      </div>
      
      <form onSubmit={handleSubmit} noValidate>
        {renderField('username', 'Username', 'text', 'Enter your username')}
        {renderField('email', 'Email Address', 'email', 'you@example.com')}
        {renderField('password', 'Password', 'password', 'Minimum 8 characters')}
        {renderField('confirmPassword', 'Confirm Password', 'password', 'Re-enter your password')}
        {renderField('bio', 'Bio (optional)', 'textarea', 'Tell us about yourself...')}

        <div className="form-actions">
          <button
            type="submit"
            className="submit-button"
            disabled={!isDirty}
          >
            <span>Create Account</span>
            <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
              <path d="M4 10h12M12 6l4 4-4 4" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
            </svg>
          </button>
          
          <button
            type="button"
            className="reset-button"
            onClick={resetForm}
            disabled={!isDirty}
          >
            Reset Form
          </button>
        </div>

        {/* Form status indicator */}
        <div className="form-status">
          <div className={`status-badge ${isValid ? 'valid' : 'invalid'}`}>
            {isValid ? (
              <>
                <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
                  <path d="M3 8.5L6 11.5L13 4.5" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
                </svg>
                All fields valid
              </>
            ) : (
              <>
                <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
                  <path d="M8 4v4M8 11h.01" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round"/>
                </svg>
                Please fill required fields
              </>
            )}
          </div>
        </div>
      </form>

      {/* Debug panel showing form state */}
      <details className="debug-panel">
        <summary>Form State (Debug)</summary>
        <pre>{JSON.stringify({ values, isValid, isDirty }, null, 2)}</pre>
      </details>
    </div>
  );
};

export default RegistrationForm;
