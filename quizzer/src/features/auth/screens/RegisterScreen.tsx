import React from 'react';
import { useRouter } from 'expo-router';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Box, Text, Button, ControlledInput, Screen } from '@/src/components';
import {
  createRegisterSchema,
  RegisterFormData,
} from '@/src/features/auth/schemas/register-schema';
import { authService } from '@/src/shared/api/auth.service';
import { useAsyncCommand } from '@/src/shared/hooks/useAsyncCommand';

export const RegisterScreen = () => {
  const router = useRouter();
  const { t } = useTranslation();
  const schema = createRegisterSchema(t);

  const {
    control,
    handleSubmit,
    formState: { isSubmitting },
  } = useForm<RegisterFormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      username: '',
      email: '',
      password: '',
      confirmPassword: '',
    },
  });

  const { execute: registerUser, isLoading: isRegistering } = useAsyncCommand(
    authService.register,
    {
      onSuccess: () => {
        router.replace('/login');
      },
    }
  );

  const onSubmit = async (data: RegisterFormData) => {
    await registerUser(data);
  };

  const isBusy = isSubmitting || isRegistering;

  return (
    <Screen>
      <Box flex={1} paddingHorizontal="l" paddingTop="xl" gap="m">
        <Text variant="title2">{t('register.title')}</Text>

        <Box gap="s">
          <ControlledInput
            name="username"
            control={control}
            placeholder={t('register.usernamePlaceholder')}
            editable={!isBusy}
            preset="username"
          />

          <ControlledInput
            name="email"
            control={control}
            placeholder={t('register.emailPlaceholder')}
            keyboardType="email-address"
            editable={!isBusy}
            preset="email"
          />

          <ControlledInput
            name="password"
            control={control}
            placeholder={t('register.passwordPlaceholder')}
            editable={!isBusy}
            preset="password"
          />

          <ControlledInput
            name="confirmPassword"
            control={control}
            placeholder={t('register.confirmPasswordPlaceholder')}
            editable={!isBusy}
            preset="password"
          />
        </Box>

        <Button
          title={t('register.registerButton')}
          variant="primary"
          marginTop="l"
          isLoading={isBusy}
          onPress={handleSubmit(onSubmit)}
        />

        <Box
          flex={1}
          justifyContent="flex-start"
          alignItems="center"
          paddingBottom="m"
          marginTop="m"
        >
          <Text variant="regularNormalRegular">
            {t('register.alreadyAccount')}{' '}
            <Text
              color="primaryText"
              variant="regularNormalRegular"
              fontWeight="bold"
              onPress={() => router.push('/login')}
            >
              {t('register.loginNow')}
            </Text>
          </Text>
        </Box>
      </Box>
    </Screen>
  );
};
