import React, { useState } from 'react';
import { useController, Control, Path, FieldValues } from 'react-hook-form';
import { FadeIn, FadeOut, LinearTransition } from 'react-native-reanimated';
import { TextInput, TextInputProps, InputPreset } from './TextInput'; // 1. Import InputPreset
import { AnimatedBox } from '../Box';
import { Text } from '../Text';

interface ControlledInputProps<T extends FieldValues> extends Omit<
  TextInputProps,
  'value' | 'onChangeText' | 'error'
> {
  name: Path<T>;
  control: Control<T>;
  caption?: string;
  title?: string;
  preset?: InputPreset;
}

export const ControlledInput = <T extends FieldValues>({
  name,
  control,
  caption,
  title,
  preset = 'text',
  ...rest
}: ControlledInputProps<T>) => {
  const [isFocused, setIsFocused] = useState(false);

  const {
    field: { onChange, onBlur, value },
    fieldState: { error },
  } = useController({ name, control });

  const activeError = isFocused ? null : error?.message;

  const showBottomLabel = !!(activeError || caption);

  const handleFocus = (e: any) => {
    setIsFocused(true);
    rest.onFocus?.(e);
  };

  const handleBlur = (e: any) => {
    setIsFocused(false);
    onBlur();
    rest.onBlur?.(e);
  };

  return (
    <AnimatedBox
      width="100%"
      gap="sm"
      layout={LinearTransition.springify().damping(26).stiffness(400).mass(0.8)}
    >
      {title ? (
        <Text variant="regularNormalRegular" color="mainText">
          {title}
        </Text>
      ) : null}

      <TextInput
        value={value}
        onChangeText={onChange}
        onFocus={handleFocus}
        onBlur={handleBlur}
        error={!!activeError}
        preset={preset}
        {...rest}
      />

      {showBottomLabel ? (
        <AnimatedBox
          key={`error-label-${name}`}
          entering={FadeIn.duration(200)}
          exiting={FadeOut.duration(150)}
        >
          <Text variant="smallNormalRegular" color={activeError ? 'textError' : 'captionText'}>
            {activeError || caption}
          </Text>
        </AnimatedBox>
      ) : null}
    </AnimatedBox>
  );
};
