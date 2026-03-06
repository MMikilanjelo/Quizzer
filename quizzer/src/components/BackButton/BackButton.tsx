import React from 'react';
import { Pressable } from 'react-native';
import { useRouter } from 'expo-router';
import { theme } from '@/src/shared/theme/theme';
import { AnimatedBox } from '../Box'; // Using your AnimatedBox
import { ChevronLeft } from 'lucide-react-native'; // Better icon
import { FadeIn, FadeOut } from 'react-native-reanimated';

export const BackButton = () => {
  const router = useRouter();

  return (
    <Pressable onPress={() => router.back()} hitSlop={{ top: 20, bottom: 20, left: 20, right: 20 }}>
      <AnimatedBox
        entering={FadeIn.duration(200)}
        exiting={FadeOut.duration(150)}
        width={32}
        height={32}
        alignItems="flex-start"
        justifyContent="center"
      >
        <ChevronLeft size={24} color={theme.colors.mainText} strokeWidth={1.5} />
      </AnimatedBox>
    </Pressable>
  );
};
