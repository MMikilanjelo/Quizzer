import { useEffect, useState } from 'react';
import { ThemeProvider } from '@shopify/restyle';
import { Stack, SplashScreen } from 'expo-router';
import { theme } from '@/src/shared/theme/theme';
import { GestureHandlerRootView } from 'react-native-gesture-handler';
import { BottomSheetModalProvider } from '@gorhom/bottom-sheet';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import {
  useFonts,
  Inter_400Regular,
  Inter_500Medium,
  Inter_600SemiBold,
  Inter_700Bold,
} from '@expo-google-fonts/inter';
import { GlobalModalRenderer, LoadingOverlay } from '@/src/components';
import { initI18n } from '@/src/shared/localization/i18n';
import { PortalProvider, PortalHost } from '@gorhom/portal';

SplashScreen.preventAutoHideAsync();

export default function RootLayout() {
  const [isI18nLoaded, setIsI18nLoaded] = useState(false);

  const [fontsLoaded, fontError] = useFonts({
    Inter_400Regular,
    Inter_500Medium,
    Inter_600SemiBold,
    Inter_700Bold,
  });

  useEffect(() => {
    initI18n()
      .then(() => {
        setIsI18nLoaded(true);
      })
      .catch((error) => {
        console.error('Failed to initialize i18n:', error);
        setIsI18nLoaded(true);
      });
  }, []);

  useEffect(() => {
    if ((fontsLoaded || fontError) && isI18nLoaded) {
      SplashScreen.hideAsync();
    }
  }, [fontsLoaded, fontError, isI18nLoaded]);

  if ((!fontsLoaded && !fontError) || !isI18nLoaded) {
    return null;
  }

  return (
    <ThemeProvider theme={theme}>
      <PortalProvider>
        <GestureHandlerRootView style={{ flex: 1 }}>
          <BottomSheetModalProvider>
            <SafeAreaProvider>
              <Stack screenOptions={{ headerShown: false }}>
                <Stack.Screen name="(auth)" />
                <Stack.Screen name="(tabs)" />
              </Stack>
              <LoadingOverlay />
              <GlobalModalRenderer />
              <PortalHost name="root" />
              <PortalHost name="modals" />
            </SafeAreaProvider>
          </BottomSheetModalProvider>
        </GestureHandlerRootView>
      </PortalProvider>
    </ThemeProvider>
  );
}
