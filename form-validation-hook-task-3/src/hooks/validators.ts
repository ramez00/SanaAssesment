/**
 * Validation Functions for Form Validation Hook
 * 
 * Each validator is a higher-order function that returns a ValidationRule.
 * A ValidationRule takes a value and returns either null (valid) or an error message string.
 */

/**
 * Represents a validation rule function.
 * Returns null if valid, or an error message string if invalid.
 */
export type ValidationRule = (value: string) => string | null;

/**
 * Creates a required field validator.
 * @param message - Custom error message (optional)
 * @returns ValidationRule that checks if the value is not empty
 */
export const required = (message?: string): ValidationRule => {
  return (value: string): string | null => {
    const trimmedValue = value.trim();
    if (trimmedValue === '') {
      return message || 'This field is required';
    }
    return null;
  };
};

/**
 * Creates a minimum length validator.
 * @param length - Minimum number of characters required
 * @param message - Custom error message (optional)
 * @returns ValidationRule that checks if the value meets the minimum length
 */
export const minLength = (length: number, message?: string): ValidationRule => {
  return (value: string): string | null => {
    if (value.length < length) {
      return message || `Must be at least ${length} characters`;
    }
    return null;
  };
};

/**
 * Creates a maximum length validator.
 * @param length - Maximum number of characters allowed
 * @param message - Custom error message (optional)
 * @returns ValidationRule that checks if the value doesn't exceed the maximum length
 */
export const maxLength = (length: number, message?: string): ValidationRule => {
  return (value: string): string | null => {
    if (value.length > length) {
      return message || `Must be no more than ${length} characters`;
    }
    return null;
  };
};

/**
 * Creates an email format validator.
 * @param message - Custom error message (optional)
 * @returns ValidationRule that checks if the value is a valid email format
 */
export const emailFormat = (message?: string): ValidationRule => {
  return (value: string): string | null => {
    // Skip validation if empty (use required() for that)
    if (value.trim() === '') {
      return null;
    }
    
    // RFC 5322 compliant email regex (simplified version)
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    
    if (!emailRegex.test(value)) {
      return message || 'Please enter a valid email address';
    }
    return null;
  };
};

/**
 * Creates a pattern validator using a custom regex.
 * @param pattern - Regular expression pattern to match
 * @param message - Error message to show when pattern doesn't match
 * @returns ValidationRule that checks if the value matches the pattern
 */
export const pattern = (regex: RegExp, message: string): ValidationRule => {
  return (value: string): string | null => {
    if (value.trim() === '') {
      return null;
    }
    
    if (!regex.test(value)) {
      return message;
    }
    return null;
  };
};

/**
 * Creates a validator that checks if value matches another field.
 * Useful for password confirmation fields.
 * @param getCompareValue - Function that returns the value to compare against
 * @param message - Custom error message (optional)
 * @returns ValidationRule that checks if values match
 */
export const matches = (
  getCompareValue: () => string,
  message?: string
): ValidationRule => {
  return (value: string): string | null => {
    if (value !== getCompareValue()) {
      return message || 'Values do not match';
    }
    return null;
  };
};
