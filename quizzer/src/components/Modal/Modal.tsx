import React, { useCallback, useEffect, useRef, useMemo } from 'react';
import { BottomSheetModal, BottomSheetBackdrop, BottomSheetScrollView } from '@gorhom/bottom-sheet';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Box } from '../Box/Box';
import { Text } from '../Text/Text';
import { theme } from '@/src/shared/theme/theme';
import { BottomSheetInputContext } from '@/src/components/Modal/BottomSheetInputContext';

interface ModalProps {
  isVisible: boolean;
  onDismiss?: () => void;
  icon?: React.ReactNode;
  title?: string;
  subtitle?: string;
  children?: React.ReactNode;
  footer?: React.ReactNode;
}

export const Modal = ({
  isVisible,
  onDismiss,
  icon,
  title,
  subtitle,
  children,
  footer,
}: ModalProps) => {
  const bottomSheetModalRef = useRef<BottomSheetModal>(null);
  const insets = useSafeAreaInsets();
  const snapPoints = useMemo(() => ['50%', '75%', '90%'], []);

  const animationConfigs = useMemo(
    () => ({
      damping: 80,
      overshootClamping: true,
      restDisplacementThreshold: 0.1,
      restSpeedThreshold: 0.1,
      stiffness: 500,
    }),
    []
  );

  useEffect(() => {
    if (isVisible) {
      bottomSheetModalRef.current?.present();
    } else {
      bottomSheetModalRef.current?.dismiss();
    }
  }, [isVisible]);

  const renderBackdrop = useCallback(
    (props: any) => (
      <BottomSheetBackdrop
        {...props}
        disappearsOnIndex={-1}
        appearsOnIndex={0}
        opacity={0.5}
        pressBehavior="close"
      />
    ),
    []
  );

  return (
    <BottomSheetModal
      ref={bottomSheetModalRef}
      index={0}
      snapPoints={snapPoints}
      enablePanDownToClose={true}
      onDismiss={onDismiss}
      backdropComponent={renderBackdrop}
      animationConfigs={animationConfigs}
      backgroundStyle={{ backgroundColor: theme.colors.mainBackground }}
      handleIndicatorStyle={{
        backgroundColor: theme.colors.modalHandle,
        width: 36,
        paddingTop: theme.spacing.s,
        height: 5,
      }}
      keyboardBehavior="extend"
      keyboardBlurBehavior="restore"
    >
      <BottomSheetScrollView
        showsVerticalScrollIndicator={false}
        bounces={true}
        contentContainerStyle={{
          flexGrow: 0,
          paddingHorizontal: theme.spacing.l,
          paddingTop: theme.spacing.m,
          paddingBottom: Math.max(insets.bottom, theme.spacing.l),
        }}
      >
        <Box alignItems="center" gap="s" paddingBottom="l">
          {icon && (
            <Box width={64} height={64}>
              {icon}
            </Box>
          )}
          {title && (
            <Text variant="title3" textAlign="center">
              {title}
            </Text>
          )}
          {subtitle && (
            <Text variant="regularNormalRegular" color="subTitleText" textAlign="center">
              {subtitle}
            </Text>
          )}
        </Box>

        {children && (
          <Box width="100%">
            <BottomSheetInputContext.Provider value={true}>
              {children}
            </BottomSheetInputContext.Provider>
          </Box>
        )}

        {footer && (
          <Box width="100%" gap="sm" paddingTop="m">
            {footer}
          </Box>
        )}
      </BottomSheetScrollView>
    </BottomSheetModal>
  );
};
