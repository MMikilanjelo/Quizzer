import * as z from 'zod';
import i18n from '@/src/shared/localization/i18n';
import { TFunction } from 'i18next';

export const createRegisterSchema = (t: TFunction) =>
  z
    .object({
      username: z.string().min(3, { message: i18n.t('validation.usernameMin') }),
      email: z.email({ message: i18n.t('validation.emailInvalid') }),
      password: z.string().min(8, { message: i18n.t('validation.passwordMin') }),
      confirmPassword: z.string(),
    })
    .refine((data) => data.password === data.confirmPassword, {
      message: i18n.t('validation.passwordsDontMatch'),
      path: ['confirmPassword'],
    });

export type RegisterFormData = z.infer<ReturnType<typeof createRegisterSchema>>;
