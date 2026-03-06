import React from 'react';
import { StyleSheet } from 'react-native';
import { LinearTransition, FadeInUp, FadeOut } from 'react-native-reanimated';
import { WifiOff, AlertTriangle, RefreshCcw, CheckCircle2, LucideIcon } from 'lucide-react-native';
import { AnimatedBox } from '../Box';
import { OperationState } from '@/src/shared/types/operationState';

interface StatusIconProps {
  state: OperationState;
  color?: string;
}

const ICON_CONFIG: Record<OperationState, { Icon: LucideIcon; color: string; stroke: number }> = {
  idle: { Icon: WifiOff, color: '#7047EB', stroke: 1.5 },
  processing: { Icon: RefreshCcw, color: '#7047EB', stroke: 1.5 },
  error: { Icon: AlertTriangle, color: '#FF453A', stroke: 2 },
  success: { Icon: CheckCircle2, color: '#32D74B', stroke: 2 },
};

export const StatusIcon = ({ state, color }: StatusIconProps) => {
  const { Icon, color: configColor, stroke } = ICON_CONFIG[state];

  return (
    <AnimatedBox layout={LinearTransition.springify()} height={64} width={64} alignSelf="center">
      <AnimatedBox
        key={state}
        entering={FadeInUp.springify().damping(12).stiffness(150).mass(0.5)}
        exiting={FadeOut.duration(150)}
        style={StyleSheet.absoluteFill}
        alignItems="center"
        justifyContent="center"
      >
        <Icon size={64} color={color || configColor} strokeWidth={stroke} />
      </AnimatedBox>
    </AnimatedBox>
  );
};
