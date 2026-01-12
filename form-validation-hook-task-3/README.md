# Custom Form Validation Hook

A reusable React Hook for form validation built with TypeScript.

## Features

- **Type-safe**: Full TypeScript support with generic types
- **Flexible validation rules**: Compose multiple validators per field
- **Real-time validation**: Validate on change and/or blur
- **Easy integration**: Simple API with `getFieldProps` for quick form binding
- **Built-in validators**: `required`, `minLength`, `maxLength`, `emailFormat`, `pattern`, `matches`
- **Form state management**: Track values, errors, touched status, and overall validity

## Installation

```bash
npm install
```

## Running the Demo

```bash
npm run dev
```

## Usage

### Basic Example

```tsx
import { useFormValidation, required, minLength, emailFormat } from './hooks';

type FormFields = 'name' | 'email';

function MyForm() {
  const { fields, isValid, validateForm, getFieldProps } = useFormValidation<FormFields>({
    name: [required(), minLength(3)],
    email: [required(), emailFormat()],
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (validateForm()) {
      console.log('Form is valid!');
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <input {...getFieldProps('name')} />
      {fields.name.error && <span>{fields.name.error}</span>}
      
      <input {...getFieldProps('email')} />
      {fields.email.error && <span>{fields.email.error}</span>}
      
      <button type="submit">Submit</button>
    </form>
  );
}
```

### Available Validators

| Validator | Description | Example |
|-----------|-------------|---------|
| `required(message?)` | Field must not be empty | `required('Name is required')` |
| `minLength(n, message?)` | Minimum character length | `minLength(3, 'Too short')` |
| `maxLength(n, message?)` | Maximum character length | `maxLength(100, 'Too long')` |
| `emailFormat(message?)` | Valid email format | `emailFormat('Invalid email')` |
| `pattern(regex, message)` | Custom regex pattern | `pattern(/^\d+$/, 'Numbers only')` |
| `matches(fn, message?)` | Compare with another value | `matches(() => password, 'Must match')` |

### Hook Return Values

| Property | Type | Description |
|----------|------|-------------|
| `fields` | `FormFields<T>` | Object containing state for each field |
| `isValid` | `boolean` | Whether all fields are currently valid |
| `isDirty` | `boolean` | Whether any field has been touched |
| `values` | `Record<T, string>` | Just the values of all fields |
| `errors` | `Record<T, string \| null>` | Just the errors of all fields |
| `setValue` | `function` | Update a specific field's value |
| `setTouched` | `function` | Mark a field as touched |
| `validateField` | `function` | Validate a single field |
| `validateForm` | `function` | Validate all fields, returns boolean |
| `resetForm` | `function` | Reset form to initial state |
| `resetField` | `function` | Reset a specific field |
| `getFieldProps` | `function` | Get props to spread on input elements |
| `setValues` | `function` | Set multiple field values at once |

### Creating Custom Validators

```tsx
import type { ValidationRule } from './hooks';

// Custom validator factory
const isPhoneNumber = (message?: string): ValidationRule => {
  return (value: string): string | null => {
    if (!value) return null; // Skip if empty (use required() for that)
    
    const phoneRegex = /^\+?[\d\s-]{10,}$/;
    if (!phoneRegex.test(value)) {
      return message || 'Please enter a valid phone number';
    }
    return null;
  };
};

// Usage
const { fields } = useFormValidation({
  phone: [required(), isPhoneNumber()],
});
```

### Options

```tsx
const form = useFormValidation(validationConfig, {
  initialValues: { name: 'John' },  // Pre-fill form fields
  validateOnChange: true,            // Validate as user types (default: true)
  validateOnBlur: true,              // Validate when field loses focus (default: true)
});
```

## Project Structure

```
src/
├── hooks/
│   ├── index.ts              # Exports all hook functionality
│   ├── useFormValidation.ts  # The main custom hook
│   └── validators.ts         # Built-in validation functions
├── components/
│   ├── RegistrationForm.tsx  # Demo component
│   └── RegistrationForm.css  # Demo styles
├── App.tsx                   # Main app component
├── App.css                   # App styles
├── main.tsx                  # Entry point
└── index.css                 # Global styles
```

## License

MIT
