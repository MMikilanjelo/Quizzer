import React from 'react';
import Animated, { useAnimatedStyle, useSharedValue, withTiming } from 'react-native-reanimated';
import { useTheme } from '@shopify/restyle';
import { Portal } from '@gorhom/portal';
import { Theme } from '@/src/shared/theme/theme';
import { Button } from '../Button';
import { Box } from '../Box';
import { Plus } from 'lucide-react-native';
import { FloatingActionButton, FABAction } from '../FABMenu';

export interface AnimatedFABMenuProps {
  actions: FABAction[];
  bottomOffset?: number;
  rightOffset?: number;
}

export const FABMenu: React.FC<AnimatedFABMenuProps> = ({
  actions,
  bottomOffset = 30,
  rightOffset = 20,
}) => {
  const theme = useTheme<Theme>();
  const isExpanded = useSharedValue(false);

  const toggleMenu = () => {
    isExpanded.value = !isExpanded.value;
  };

  const plusIconStyle = useAnimatedStyle(() => ({
    transform: [{ rotate: withTiming(isExpanded.value ? '45deg' : '0deg') }],
  }));

  return (
    <Portal hostName="root">
      <Box
        position="absolute"
        bottom={bottomOffset}
        right={rightOffset}
        alignItems="flex-end"
        pointerEvents="box-none"
      >
        <Box position="absolute" bottom={0} right={0} pointerEvents="box-none">
          {actions.map((action, index) => (
            <FloatingActionButton
              key={action.id}
              index={index + 1}
              isExpanded={isExpanded}
              action={action}
              theme={theme}
            />
          ))}
        </Box>

        <Box style={theme.shadows.medium}>
          <Button
            onPress={toggleMenu}
            variant="primary"
            shape="circle"
            size="large"
            icon={
              <Animated.View
                style={[
                  plusIconStyle,
                  {
                    justifyContent: 'center',
                    alignItems: 'center',
                  },
                ]}
              >
                <Plus color={theme.colors.buttonPrimaryText} size={28} strokeWidth={2.5} />
              </Animated.View>
            }
          />
        </Box>
      </Box>
    </Portal>
  );
};
