import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Email domain validator
 * @param allowedDomain - Required email domain (e.g., 'company.com')
 * @returns ValidatorFn
 */
export function emailDomainValidator(allowedDomain: string): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null;
    }

    const email = control.value as string;
    const domain = email.split('@')[1];

    if (domain && domain.toLowerCase() === allowedDomain.toLowerCase()) {
      return null;
    }

    return { emailDomain: { requiredDomain: allowedDomain, actualDomain: domain } };
  };
}

/**
 * Strong email format validator
 * Validates email format with stricter rules than default
 */
export function strongEmailValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null;
    }

    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    
    if (emailRegex.test(control.value)) {
      return null;
    }

    return { strongEmail: true };
  };
}

/**
 * Multiple email domains validator
 * @param allowedDomains - Array of allowed domains
 */
export function emailDomainsValidator(allowedDomains: string[]): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null;
    }

    const email = control.value as string;
    const domain = email.split('@')[1];

    if (domain && allowedDomains.some(d => d.toLowerCase() === domain.toLowerCase())) {
      return null;
    }

    return { emailDomains: { allowedDomains, actualDomain: domain } };
  };
}
