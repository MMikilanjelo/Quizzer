import React, { useEffect } from 'react';
import { Pressable } from 'react-native';
import { MaterialIcons } from '@expo/vector-icons';
import Animated, {
  useAnimatedStyle,
  useSharedValue,
  withSpring,
  interpolateColor,
} from 'react-native-reanimated';
import { Box } from '../Box';
import { Text } from '../Text';
import { theme } from '@/src/shared/theme/theme';

interface CheckboxProps {
  label?: string;
  value: boolean;
  onValueChange: (v: boolean) => void;
  isDisabled?: boolean;
}

export const Checkbox = ({ label, value, onValueChange, isDisabled }: CheckboxProps) => {
  const active = useSharedValue(value ? 1 : 0);

  useEffect(() => {
    active.value = withSpring(value ? 1 : 0, {
      stiffness: 250,
      damping: 20,
      mass: 0.5,
    });
  }, [value, active]);

  const boxAnimatedStyle = useAnimatedStyle(() => {
    const bgColor = interpolateColor(
      active.value,
      [0, 1],
      [theme.colors.transparent, theme.colors.checkboxBgActive] //
    );

    const borderColor = interpolateColor(
      active.value,
      [0, 1],
      [theme.colors.checkboxBorder, theme.colors.checkboxBgActive] //
    );

    return {
      backgroundColor: bgColor,
      borderColor: borderColor,
    };
  });

  const checkIconStyle = useAnimatedStyle(() => ({
    transform: [{ scale: active.value }],
    opacity: active.value,
  }));

  return (
    <Pressable
      onPress={() => !isDisabled && onValueChange(!value)}
      disabled={isDisabled}
      style={{ opacity: isDisabled ? 0.5 : 1 }}
    >
      <Box
        flexDirection="row"
        alignItems="center"
        justifyContent="space-between"
        paddingVertical="s"
        width="100%"
      >
        {label && (
          <Text variant="regularNormalRegular" color="mainText">
            {label}
          </Text>
        )}

        <Animated.View
          style={[
            {
              width: 24,
              height: 24,
              borderRadius: theme.borderRadii.xs,
              borderWidth: theme.borderWidths.s,
              alignItems: 'center',
              justifyContent: 'center',
            },
            boxAnimatedStyle,
          ]}
        >
          <Animated.View style={checkIconStyle}>
            <MaterialIcons name="check" size={18} color={theme.colors.checkboxCheck} />
          </Animated.View>
        </Animated.View>
      </Box>
    </Pressable>
  );
};
