import React, {useEffect} from 'react';
import {Pressable} from 'react-native';
import Animated, {
    useAnimatedStyle,
    useSharedValue,
    withSpring,
    interpolateColor
} from 'react-native-reanimated';
import {Box} from '../Box';
import {Text} from '../Text';
import {theme} from "@/src/shared/theme/theme";

interface RadioButtonProps {
    label?: string;
    value: boolean;
    onValueChange: (v: boolean) => void;
    isDisabled?: boolean;
}

export const RadioButton = ({label, value, onValueChange, isDisabled}: RadioButtonProps) => {
    const active = useSharedValue(value ? 1 : 0);

    useEffect(() => {
        active.value = withSpring(value ? 1 : 0, {
            stiffness: 250,
            damping: 20,
            mass: 0.5,
        });
    }, [value, active]);

    const outerCircleStyle = useAnimatedStyle(() => {
        const bgColor = interpolateColor(
            active.value,
            [0, 1],
            [theme.colors.transparent, theme.colors.radioBorderActive]
        );

        const borderColor = interpolateColor(
            active.value,
            [0, 1],
            [theme.colors.radioBorder, theme.colors.radioBorderActive]
        );

        return {
            backgroundColor: bgColor,
            borderColor: borderColor,
        };
    });

    const innerDotStyle = useAnimatedStyle(() => ({
        transform: [{scale: active.value}],
        opacity: active.value,
    }));

    return (
        <Pressable
            onPress={() => !isDisabled && onValueChange(!value)}
            disabled={isDisabled}
            style={{opacity: isDisabled ? 0.5 : 1}}
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
                            borderRadius: theme.borderRadii.pill,
                            borderWidth: theme.borderWidths.s,
                            alignItems: "center",
                            justifyContent: "center",
                        },
                        outerCircleStyle
                    ]}
                >
                    <Animated.View
                        style={[
                            {
                                width: 10,
                                height: 10,
                                borderRadius: theme.borderRadii.pill,
                                backgroundColor: theme.colors.radioCircleActive,
                            },
                            innerDotStyle
                        ]}
                    />
                </Animated.View>
            </Box>
        </Pressable>
    );
};
