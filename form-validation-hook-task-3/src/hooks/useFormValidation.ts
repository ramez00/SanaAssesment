import { useState, useCallback, useMemo } from 'react';
import type { ValidationRule } from './validators';

/**
 * Configuration object defining validation rules for form fields.
 * Each key is a field name, and the value is an array of validation rules.
 */
export type ValidationConfig<T extends string> = {
  [K in T]: ValidationRule[];
};

/**
 * Represents the state of a single form field.
 */
export interface FieldState {
  /** Current value of the field */
  value: string;
  /** Error message for this field (null if valid) */
  error: string | null;
  /** Whether the field has been touched/modified by the user */
  touched: boolean;
}

/**
 * State object containing all field states.
 */
export type FormFields<T extends string> = {
  [K in T]: FieldState;
};

/**
 * Return type of the useFormValidation hook.
 */
export interface UseFormValidationReturn<T extends string> {
  /** Current state of all form fields (values, errors, touched status) */
  fields: FormFields<T>;
  
  /** Whether all fields are valid */
  isValid: boolean;
  
  /** Whether any field has been touched */
  isDirty: boolean;
  
  /** Object containing just the values of all fields */
  values: Record<T, string>;
  
  /** Object containing just the errors of all fields */
  errors: Record<T, string | null>;
  
  /**
   * Update the value of a specific field.
   * @param fieldName - Name of the field to update
   * @param value - New value for the field
   */
  setValue: (fieldName: T, value: string) => void;
  
  /**
   * Set a field as touched.
   * @param fieldName - Name of the field to mark as touched
   */
  setTouched: (fieldName: T) => void;
  
  /**
   * Validate a single field.
   * @param fieldName - Name of the field to validate
   * @returns Error message or null if valid
   */
  validateField: (fieldName: T) => string | null;
  
  /**
   * Validate all fields in the form.
   * @returns true if all fields are valid, false otherwise
   */
  validateForm: () => boolean;
  
  /**
   * Reset the form to initial state.
   */
  resetForm: () => void;
  
  /**
   * Reset a specific field to initial state.
   * @param fieldName - Name of the field to reset
   */
  resetField: (fieldName: T) => void;
  
  /**
   * Get props to spread on an input element.
   * Provides value, onChange, and onBlur handlers.
   * @param fieldName - Name of the field
   */
  getFieldProps: (fieldName: T) => {
    value: string;
    onChange: (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => void;
    onBlur: () => void;
  };
  
  /**
   * Set multiple field values at once.
   * @param values - Object with field names as keys and new values
   */
  setValues: (values: Partial<Record<T, string>>) => void;
}

/**
 * Options for the useFormValidation hook.
 */
export interface UseFormValidationOptions<T extends string> {
  /** Initial values for form fields */
  initialValues?: Partial<Record<T, string>>;
  
  /** Whether to validate fields on change (default: true) */
  validateOnChange?: boolean;
  
  /** Whether to validate fields on blur (default: true) */
  validateOnBlur?: boolean;
}

/**
 * Custom React Hook for form validation.
 * 
 * @param validationConfig - Configuration object defining validation rules for each field
 * @param options - Optional configuration options
 * @returns Object containing form state and helper functions
 * 
 * @example
 * ```tsx
 * const { fields, isValid, validateForm, getFieldProps } = useFormValidation({
 *   name: [required(), minLength(3)],
 *   email: [required(), emailFormat()],
 * });
 * 
 * // In your JSX:
 * <input {...getFieldProps('name')} />
 * {fields.name.error && <span>{fields.name.error}</span>}
 * ```
 */
export function useFormValidation<T extends string>(
  validationConfig: ValidationConfig<T>,
  options: UseFormValidationOptions<T> = {}
): UseFormValidationReturn<T> {
  const {
    initialValues = {},
    validateOnChange = true,
    validateOnBlur = true,
  } = options;

  // Get field names from the validation config
  const fieldNames = Object.keys(validationConfig) as T[];

  // Initialize form state
  const createInitialState = useCallback((): FormFields<T> => {
    const state = {} as FormFields<T>;
    for (const fieldName of fieldNames) {
      state[fieldName] = {
        value: (initialValues as Record<T, string>)[fieldName] || '',
        error: null,
        touched: false,
      };
    }
    return state;
  }, [fieldNames, initialValues]);

  const [formState, setFormState] = useState<FormFields<T>>(createInitialState);

  /**
   * Validate a single field against its validation rules.
   */
  const validateField = useCallback(
    (fieldName: T): string | null => {
      const rules = validationConfig[fieldName];
      const value = formState[fieldName].value;

      for (const rule of rules) {
        const error = rule(value);
        if (error) {
          return error;
        }
      }
      return null;
    },
    [validationConfig, formState]
  );

  /**
   * Update the value of a specific field.
   */
  const setValue = useCallback(
    (fieldName: T, value: string) => {
      setFormState((prev) => {
        const newState = { ...prev };
        newState[fieldName] = {
          ...prev[fieldName],
          value,
          touched: true,
        };

        // Validate on change if enabled
        if (validateOnChange) {
          const rules = validationConfig[fieldName];
          let error: string | null = null;
          for (const rule of rules) {
            error = rule(value);
            if (error) break;
          }
          newState[fieldName].error = error;
        }

        return newState;
      });
    },
    [validationConfig, validateOnChange]
  );

  /**
   * Mark a field as touched and validate it.
   */
  const setTouched = useCallback(
    (fieldName: T) => {
      setFormState((prev) => {
        const newState = { ...prev };
        newState[fieldName] = {
          ...prev[fieldName],
          touched: true,
        };

        // Validate on blur if enabled
        if (validateOnBlur) {
          const rules = validationConfig[fieldName];
          const value = prev[fieldName].value;
          let error: string | null = null;
          for (const rule of rules) {
            error = rule(value);
            if (error) break;
          }
          newState[fieldName].error = error;
        }

        return newState;
      });
    },
    [validationConfig, validateOnBlur]
  );

  /**
   * Validate all fields and return whether the form is valid.
   */
  const validateForm = useCallback((): boolean => {
    let isValid = true;
    const newState = { ...formState };

    for (const fieldName of fieldNames) {
      const rules = validationConfig[fieldName];
      const value = formState[fieldName].value;
      let error: string | null = null;

      for (const rule of rules) {
        error = rule(value);
        if (error) {
          isValid = false;
          break;
        }
      }

      newState[fieldName] = {
        ...formState[fieldName],
        error,
        touched: true,
      };
    }

    setFormState(newState);
    return isValid;
  }, [formState, fieldNames, validationConfig]);

  /**
   * Reset the entire form to initial state.
   */
  const resetForm = useCallback(() => {
    setFormState(createInitialState());
  }, [createInitialState]);

  /**
   * Reset a specific field to initial state.
   */
  const resetField = useCallback(
    (fieldName: T) => {
      setFormState((prev) => ({
        ...prev,
        [fieldName]: {
          value: (initialValues as Record<T, string>)[fieldName] || '',
          error: null,
          touched: false,
        },
      }));
    },
    [initialValues]
  );

  /**
   * Get props to spread on an input element.
   */
  const getFieldProps = useCallback(
    (fieldName: T) => ({
      value: formState[fieldName].value,
      onChange: (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        setValue(fieldName, e.target.value);
      },
      onBlur: () => {
        setTouched(fieldName);
      },
    }),
    [formState, setValue, setTouched]
  );

  /**
   * Set multiple field values at once.
   */
  const setValues = useCallback(
    (values: Partial<Record<T, string>>) => {
      setFormState((prev) => {
        const newState = { ...prev };
        for (const [fieldName, value] of Object.entries(values) as [T, string][]) {
          if (fieldName in prev) {
            newState[fieldName] = {
              ...prev[fieldName],
              value,
              touched: true,
            };

            if (validateOnChange) {
              const rules = validationConfig[fieldName];
              let error: string | null = null;
              for (const rule of rules) {
                error = rule(value);
                if (error) break;
              }
              newState[fieldName].error = error;
            }
          }
        }
        return newState;
      });
    },
    [validationConfig, validateOnChange]
  );

  // Computed values
  const values = useMemo(() => {
    const result = {} as Record<T, string>;
    for (const fieldName of fieldNames) {
      result[fieldName] = formState[fieldName].value;
    }
    return result;
  }, [formState, fieldNames]);

  const errors = useMemo(() => {
    const result = {} as Record<T, string | null>;
    for (const fieldName of fieldNames) {
      result[fieldName] = formState[fieldName].error;
    }
    return result;
  }, [formState, fieldNames]);

  const isValid = useMemo(() => {
    for (const fieldName of fieldNames) {
      const rules = validationConfig[fieldName];
      const value = formState[fieldName].value;
      for (const rule of rules) {
        if (rule(value)) {
          return false;
        }
      }
    }
    return true;
  }, [formState, fieldNames, validationConfig]);

  const isDirty = useMemo(() => {
    for (const fieldName of fieldNames) {
      if (formState[fieldName].touched) {
        return true;
      }
    }
    return false;
  }, [formState, fieldNames]);

  return {
    fields: formState,
    isValid,
    isDirty,
    values,
    errors,
    setValue,
    setTouched,
    validateField,
    validateForm,
    resetForm,
    resetField,
    getFieldProps,
    setValues,
  };
}

export default useFormValidation;
