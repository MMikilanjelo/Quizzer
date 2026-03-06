import { Stack, Redirect } from 'expo-router';
import { theme } from '@/src/shared/theme/theme';
import { BackButton } from '@/src/components/BackButton';

export default function AuthLayout() {
  return <Redirect href="/(tabs)" />;
  return (
    <Stack
      screenOptions={{
        headerTitleAlign: 'center',
        headerShadowVisible: false,
        headerBackVisible: false,
        animation: 'slide_from_right',
        headerStyle: {
          backgroundColor: theme.colors.mainBackground,
        },
        headerLeft: ({ canGoBack }) => (canGoBack ? <BackButton /> : null),
        headerTitleStyle: {
          fontFamily: theme.textVariants.largeNoneRegular.fontFamily,
          fontSize: 18,
          color: theme.colors.mainText,
        },
        contentStyle: {
          backgroundColor: theme.colors.mainBackground,
        },
      }}
    >
      <Stack.Screen
        name="login"
        options={{
          headerTitle: 'Log in',
          headerLeft: () => null,
        }}
      />
      <Stack.Screen name="register" options={{ headerTitle: 'Register' }} />
    </Stack>
  );
}
