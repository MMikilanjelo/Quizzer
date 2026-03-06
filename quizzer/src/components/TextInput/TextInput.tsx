import React, { useState, useRef } from 'react';
import {
  TextInput as NativeTextInput,
  TextInputProps as NativeProps,
  Pressable,
  Animated,
} from 'react-native';
import { Eye, EyeOff } from 'lucide-react-native';
import { useTheme } from '@shopify/restyle';
import { Theme } from '@/src/shared/theme/theme';
import { Box } from '../Box';

export type InputPreset = 'text' | 'email' | 'password' | 'username';

const PRESET_CONFIGS: Record<InputPreset, Partial<NativeProps>> = {
  text: {
    autoCapitalize: 'sentences',
  },
  email: {
    keyboardType: 'email-address',
    autoCapitalize: 'none',
    autoCorrect: false,
    autoComplete: 'email',
  },
  username: {
    keyboardType: 'default',
    autoCapitalize: 'none',
    autoCorrect: false,
    autoComplete: 'username',
  },
  password: {
    keyboardType: 'default',
    autoCapitalize: 'none',
    autoCorrect: false,
  },
};

export interface TextInputProps extends NativeProps {
  error?: boolean;
  preset?: InputPreset;
  rightIcon?: React.ReactNode;
}

export const TextInput = ({
  error,
  preset = 'text',
  rightIcon,
  editable = true,
  onFocus,
  onBlur,
  ...rest
}: TextInputProps) => {
  const theme = useTheme<Theme>();

  const [isFocused, setIsFocused] = useState(false);

  const [isVisible, setIsVisible] = useState(false);

  const scaleAnim = useRef(new Animated.Value(1)).current;

  const isPasswordPreset = preset === 'password';

  const toggleVisibility = () => {
    setIsVisible(!isVisible);
    Animated.sequence([
      Animated.timing(scaleAnim, { toValue: 0.7, duration: 100, useNativeDriver: true }),
      Animated.spring(scaleAnim, { toValue: 1, friction: 4, tension: 50, useNativeDriver: true }),
    ]).start();
  };

  const getBorderColor = () => {
    if (error) return theme.colors.textError;
    if (isFocused) return theme.colors.inputBorderFocused;
    return theme.colors.inputBorderDefault;
  };

  const presetProps = PRESET_CONFIGS[preset];

  return (
    <Box
      height={48}
      px="m"
      borderRadius="m"
      flexDirection="row"
      alignItems="center"
      style={{
        backgroundColor: editable
          ? theme.colors.inputBackgroundDefault
          : theme.colors.inputBackgroundDisabled,
      }}
    >
      <Box
        position="absolute"
        top={0}
        bottom={0}
        left={0}
        right={0}
        borderRadius="m"
        borderWidth={isFocused || error ? 2 : 1}
        pointerEvents="none"
        style={{ borderColor: getBorderColor() }}
      />

      <NativeTextInput
        editable={editable}
        onFocus={(e) => {
          setIsFocused(true);
          onFocus?.(e);
        }}
        onBlur={(e) => {
          setIsFocused(false);
          onBlur?.(e);
        }}
        placeholderTextColor={theme.colors.captionText}
        secureTextEntry={isPasswordPreset && !isVisible}
        style={[
          {
            flex: 1,
            fontSize: 16,
            color: theme.colors.mainText,
          },
          rest.style,
        ]}
        {...presetProps}
        {...rest}
      />

      {rightIcon && <Box>{rightIcon}</Box>}

      {isPasswordPreset && !rightIcon && (
        <Pressable
          onPress={toggleVisibility}
          hitSlop={12}
          style={{
            width: 24,
            height: 24,
            justifyContent: 'center',
            alignItems: 'center',
          }}
        >
          <Animated.View style={{ transform: [{ scale: scaleAnim }] }}>
            {isVisible ? (
              <Eye size={24} color={theme.colors.inputTextPlaceholder} strokeWidth={2} />
            ) : (
              <EyeOff size={24} color={theme.colors.inputTextPlaceholder} strokeWidth={2} />
            )}
          </Animated.View>
        </Pressable>
      )}
    </Box>
  );
};
