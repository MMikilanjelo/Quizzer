import React, { useState } from 'react';
import { useRouter } from 'expo-router';
import { useTranslation } from 'react-i18next';
import { Box, Text, Button, TextInput, Screen } from '@/src/components';

export const LoginScreen = () => {
  const router = useRouter();
  const { t } = useTranslation();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  return (
    <Screen>
      <Box flex={1} paddingHorizontal="l" paddingTop="xl" gap="m">
        <Box>
          <Text variant="title2">{t('login.title')}</Text>
        </Box>

        <Box gap="m">
          <TextInput
            placeholder={t('login.emailPlaceholder')}
            value={email}
            onChangeText={setEmail}
            keyboardType="email-address"
            autoCapitalize="none"
            preset="email"
          />
          <TextInput
            placeholder={t('login.passwordPlaceholder')}
            value={password}
            onChangeText={setPassword}
            preset="password"
          />
        </Box>

        <Box alignItems="flex-end">
          <Text variant="regularNoneMedium" color="primaryText">
            {t('login.forgotPassword')}
          </Text>
        </Box>

        <Button
          title={t('login.loginButton')}
          variant="primary"
          onPress={() => {
            router.replace('/(tabs)/index');
          }}
        />

        <Box flex={1} justifyContent="flex-start" alignItems="center">
          <Text variant="regularNormalRegular">
            {t('login.noAccount')}
            <Text
              color="primaryText"
              variant="regularNormalRegular"
              onPress={() => router.push('/register')}
            >
              {t('login.registerNow')}
            </Text>
          </Text>
        </Box>
      </Box>
    </Screen>
  );
};
