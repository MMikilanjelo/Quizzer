import React from 'react';
import {
    KeyboardAvoidingView,
    ScrollView,
    Platform,
    StyleProp,
    ViewStyle,
    View,
} from 'react-native';
import {useSafeAreaInsets} from 'react-native-safe-area-context';
import {useTheme} from '@shopify/restyle';
import {Theme} from '@/src/shared/theme/theme';
import {Box} from '../Box/Box';

export interface ScreenProps {
    children: React.ReactNode;
    isScrollable?: boolean;
    backgroundColor?: keyof Theme['colors'];
    style?: StyleProp<ViewStyle>;
    contentContainerStyle?: StyleProp<ViewStyle>;
    safeAreaTop?: boolean;
    safeAreaBottom?: boolean;
}

export const Screen = ({
                           children,
                           isScrollable = true,
                           backgroundColor = 'mainBackground',
                           style,
                           contentContainerStyle,
                           safeAreaTop = true,
                           safeAreaBottom = true,
                       }: ScreenProps) => {
    const theme = useTheme<Theme>();

    const insets = useSafeAreaInsets();

    const paddingTop = safeAreaTop ? insets.top : 0;

    const paddingBottom = safeAreaBottom ? Math.max(insets.bottom, 24) : 0;

    const content = isScrollable ? (
        <ScrollView
            contentInsetAdjustmentBehavior="automatic"
            keyboardShouldPersistTaps="handled"
            showsVerticalScrollIndicator={false}
            style={[{flex: 1}, style]}
            contentContainerStyle={[
                {
                    flexGrow: 1,
                    paddingTop,
                    paddingBottom,
                },
                contentContainerStyle,
            ]}
        >
            {children}
        </ScrollView>
    ) : (
        <Box flex={1} style={[{paddingTop, paddingBottom}, style]}>
            {children}
        </Box>
    );

    return (
        <View style={{flex: 1, backgroundColor: theme.colors[backgroundColor]}}>
            <KeyboardAvoidingView
                behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
                style={{flex: 1}}
            >
                {content}
            </KeyboardAvoidingView>
        </View>
    );
};

