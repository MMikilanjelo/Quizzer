import React from 'react';
import {ActivityIndicator, StyleSheet} from 'react-native';
import {useLoadingStore} from "@/src/shared/stores/ui/useLoadingStore";
import {Box} from '../Box';
import {theme} from '@/src/shared/theme/theme';

export const LoadingOverlay = () => {
    const isVisible = useLoadingStore((state) => state.isVisible);

    if (!isVisible) return null;

    return (
        <Box
            style={StyleSheet.absoluteFill}
            backgroundColor="tint"
            justifyContent="center"
            alignItems="center"
            zIndex={9999}
        >
            <Box
                padding="xl"
                backgroundColor="mainBackground"
                borderRadius="l"
                shadowColor="activityIndicatorShadow"
                shadowOffset={{width: 0, height: 4}}
                shadowOpacity={0.15}
                shadowRadius={12}
                elevation={8}
                alignItems="center"
                justifyContent="center"
            >
                <ActivityIndicator
                    size="large"
                    color={theme.colors.activityIndicatorBg}
                />
            </Box>
        </Box>
    );
};
