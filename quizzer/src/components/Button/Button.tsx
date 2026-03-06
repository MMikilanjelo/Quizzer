import React, { useRef } from 'react';
import { Pressable, ActivityIndicator, Animated } from 'react-native';
import { useTheme, BoxProps } from '@shopify/restyle';
import { Theme } from '@/src/shared/theme/theme';
import { Box } from '../Box/Box';
import { Text } from '../Text/Text';

type ButtonVariant = 'primary' | 'secondary' | 'outline' | 'ghost';
type TextVariant = Exclude<keyof Theme['textVariants'], 'defaults'>;
type ButtonSize = 'large' | 'small';

interface BaseButtonProps extends Omit<BoxProps<Theme>, 'shape'> {
  onPress: () => void;
  variant?: ButtonVariant;
  size?: ButtonSize;
  isLoading?: boolean;
  isDisabled?: boolean;
}

interface PillButtonProps extends BaseButtonProps {
  shape?: 'pill';
  title: string;
  leftIcon?: React.ReactNode;
  rightIcon?: React.ReactNode;
  icon?: never;
}

interface CircleButtonProps extends BaseButtonProps {
  shape: 'circle';
  icon: React.ReactNode;
  title?: never;
  leftIcon?: never;
  rightIcon?: never;
}
export type ButtonProps = PillButtonProps | CircleButtonProps;

const VARIANT_CONFIG: Record<
  ButtonVariant,
  {
    bg: keyof Theme['colors'];
    bgPressed: keyof Theme['colors'];
    text: keyof Theme['colors'];
    textPressed: keyof Theme['colors'];
    border: keyof Theme['colors'];
  }
> = {
  primary: {
    bg: 'buttonPrimaryBg',
    bgPressed: 'buttonPrimaryBgPressed',
    text: 'buttonPrimaryText',
    textPressed: 'buttonPrimaryTextPressed',
    border: 'transparent',
  },
  secondary: {
    bg: 'buttonSecondaryBg',
    bgPressed: 'buttonSecondaryBgPressed',
    text: 'buttonSecondaryText',
    textPressed: 'buttonSecondaryTextPressed',
    border: 'transparent',
  },
  outline: {
    bg: 'transparent',
    bgPressed: 'transparent',
    text: 'buttonOutlineText',
    textPressed: 'buttonOutlineTextPressed',
    border: 'buttonOutlineBorder',
  },
  ghost: {
    bg: 'transparent',
    bgPressed: 'buttonGhostBgPressed',
    text: 'buttonOutlineText',
    textPressed: 'buttonOutlineTextPressed',
    border: 'transparent',
  },
};

const SIZE_CONFIG = {
  large: {
    height: 48,
    textVariant: 'regularNoneMedium' as TextVariant,
    padding: 'l' as const,
  },
  small: {
    height: 32,
    textVariant: 'smallNoneMedium' as TextVariant,
    padding: 'm' as const,
  },
};

export const Button = (props: ButtonProps) => {
  const {
    onPress,
    variant = 'primary',
    size = 'large',
    shape = 'pill',
    isLoading = false,
    isDisabled = false,
    ...rest
  } = props;

  const theme = useTheme<Theme>();
  const scaleAnim = useRef(new Animated.Value(1)).current;

  const animate = (toValue: number) => {
    if (!isDisabled && !isLoading) {
      Animated.spring(scaleAnim, { toValue, useNativeDriver: true }).start();
    }
  };

  const config = VARIANT_CONFIG[variant];
  const sizeConfig = SIZE_CONFIG[size];

  const isCircle = shape === 'circle';
  const buttonWidth = isCircle ? sizeConfig.height : undefined;
  const paddingHorizontal = isCircle ? 'none' : sizeConfig.padding;
  const borderWidth = variant === 'outline' ? 2 : 0;

  return (
    <Pressable
      onPress={onPress}
      onPressIn={() => animate(0.95)}
      onPressOut={() => animate(1)}
      disabled={isDisabled || isLoading}
    >
      {({ pressed }) => {
        const bgKey = isDisabled ? 'buttonDisabledBg' : pressed ? config.bgPressed : config.bg;
        const textKey = isDisabled
          ? 'buttonDisabledText'
          : pressed
            ? config.textPressed
            : config.text;
        const borderKey = isDisabled ? 'transparent' : config.border;

        return (
          <Animated.View style={{ transform: [{ scale: scaleAnim }] }}>
            <Box
              {...rest}
              height={sizeConfig.height}
              width={buttonWidth}
              backgroundColor={bgKey}
              paddingHorizontal={paddingHorizontal}
              borderRadius="pill"
              borderWidth={borderWidth}
              borderColor={borderKey}
              flexDirection="row"
              alignItems="center"
              justifyContent="center"
            >
              {isLoading ? (
                <ActivityIndicator color={theme.colors[textKey]} size="small" />
              ) : shape === 'circle' ? (
                <Box>{props.icon}</Box>
              ) : (
                <>
                  {props.leftIcon && <Box>{props.leftIcon}</Box>}
                  <Text variant={sizeConfig.textVariant} color={textKey}>
                    {props.title}
                  </Text>
                  {props.rightIcon && <Box>{props.rightIcon}</Box>}
                </>
              )}
            </Box>
          </Animated.View>
        );
      }}
    </Pressable>
  );
};
