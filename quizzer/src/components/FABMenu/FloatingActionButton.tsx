import React from 'react';
import {
  useAnimatedStyle,
  withDelay,
  withSpring,
  withTiming,
  SharedValue,
} from 'react-native-reanimated';
import { Theme } from '@/src/shared/theme/theme';
import { Button } from '../Button';
import { Text } from '../Text/Text';
import { Box, AnimatedBox } from '../Box';
import { ParseKeys } from 'i18next';
import { useTranslation } from 'react-i18next';

const OFFSET = 65;
const SPRING_CONFIG = { damping: 18, stiffness: 120, mass: 1 };

export interface FABAction {
  id: string;
  labelKey: ParseKeys;
  icon: React.ReactNode;
  onPress: () => void;
}

interface FABProps {
  isExpanded: SharedValue<boolean>;
  index: number;
  action: FABAction;
  theme: Theme;
}

export const FloatingActionButton = ({ isExpanded, index, action, theme }: FABProps) => {
  const { t } = useTranslation();
  const containerAnimatedStyle = useAnimatedStyle(() => {
    const isVisible = isExpanded.value;
    const delay = isVisible ? index * 100 : 0;
    const moveValue = isVisible ? -OFFSET * index : 0;

    return {
      opacity: withDelay(delay, withTiming(isVisible ? 1 : 0, { duration: 200 })),
      transform: [
        { translateY: withSpring(moveValue, SPRING_CONFIG) },
        { scale: withDelay(delay, withSpring(isVisible ? 1 : 0, SPRING_CONFIG)) },
      ],
    };
  });

  const textAnimatedStyle = useAnimatedStyle(() => ({
    opacity: withTiming(isExpanded.value ? 1 : 0, { duration: 200 }),
    transform: [{ translateX: withTiming(isExpanded.value ? 0 : 10) }],
  }));

  return (
    <AnimatedBox
      style={containerAnimatedStyle}
      position="absolute"
      bottom={0}
      right={0}
      flexDirection="row"
      alignItems="center"
      justifyContent="flex-end"
      width={250}
      pointerEvents="box-none"
    >
      <AnimatedBox style={textAnimatedStyle}>
        <Text variant="smallNoneBold" color="mainText" marginRight="m">
          {t(action.labelKey)}
        </Text>
      </AnimatedBox>

      <Box style={theme.shadows.medium}>
        <Button
          shape="circle"
          icon={action.icon}
          onPress={action.onPress}
          variant="secondary"
          size="large"
        />
      </Box>
    </AnimatedBox>
  );
};
