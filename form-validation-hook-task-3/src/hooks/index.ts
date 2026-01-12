// Export the main hook
export { useFormValidation, default } from './useFormValidation';

// Export all types
export type {
  ValidationConfig,
  FieldState,
  FormFields,
  UseFormValidationReturn,
  UseFormValidationOptions,
} from './useFormValidation';

// Export validation functions and types
export {
  required,
  minLength,
  maxLength,
  emailFormat,
  pattern,
  matches,
} from './validators';

export type { ValidationRule } from './validators';
