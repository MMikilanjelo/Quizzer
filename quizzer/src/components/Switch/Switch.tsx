import React, {useEffect} from 'react';
import {Pressable, StyleSheet} from 'react-native';
import Animated, {
    useAnimatedStyle,
    useSharedValue,
    withSpring,
    interpolateColor
} from 'react-native-reanimated';
import {Box} from '../Box';
import {Text} from '../Text';
import {theme} from '@/src/shared/theme/theme';

interface SwitchProps {
    label?: string;
    value: boolean;
    onValueChange: (v: boolean) => void;
    isDisabled?: boolean;
}

export const Switch = ({label, value, onValueChange, isDisabled}: SwitchProps) => {
    const offset = useSharedValue(value ? 1 : 0);

    useEffect(() => {
        offset.value = withSpring(value ? 1 : 0, {
            damping: 20,
            stiffness: 250,
            mass: 0.5,
            overshootClamping: true
        });
    }, [value, offset]);

    const trackStyle = useAnimatedStyle(() => ({
        backgroundColor: interpolateColor(
            offset.value,
            [0, 1],
            [theme.colors.switchBg, theme.colors.switchToggledBg]
        ),
    }));

    const thumbStyle = useAnimatedStyle(() => ({
        transform: [{translateX: offset.value * 16}],
    }));

    return (
        <Box flexDirection="row" alignItems="center" justifyContent="space-between" paddingVertical="s" width="100%">
            {label && <Text variant="regularNormalRegular" color="mainText">{label}</Text>}
            <Pressable
                onPress={() => !isDisabled && onValueChange(!value)}
                disabled={isDisabled}
                style={({pressed}) => ({
                    opacity: isDisabled ? 0.5 : 1,
                    transform: [{scale: pressed && !isDisabled ? 0.96 : 1}]
                })}
            >
                <Animated.View style={[styles.switchTrack, trackStyle]}>
                    <Animated.View style={[styles.switchThumb, thumbStyle]}/>
                </Animated.View>
            </Pressable>
        </Box>
    );
};

const styles = StyleSheet.create({
    switchTrack: {
        width: 44,
        height: 28,
        borderRadius: theme.borderRadii.xxl,
        padding: 4,
    },
    switchThumb: {
        width: 20,
        height: 20,
        borderRadius: 10,
        backgroundColor: theme.colors.switchKnobBg,
    },
});

