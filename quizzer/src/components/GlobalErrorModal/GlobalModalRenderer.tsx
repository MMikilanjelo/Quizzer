import React from 'react';
import { match } from 'ts-pattern';
import { useTranslation } from 'react-i18next';
import { MaterialIcons } from '@expo/vector-icons';
import { Modal } from '../Modal';
import { Box } from '../Box';
import { Button } from '../Button';
import { useModalStore } from '@/src/shared/stores/ui/useModalStore';
import { StatusIcon } from '@/src/components/StatusIcon';
import { OperationState } from '@/src/shared/types/operationState';
import { Portal } from '@gorhom/portal';

export const GlobalModalRenderer = () => {
  const { queue, popModal } = useModalStore();
  const { t } = useTranslation();

  const activeModal = queue[0];

  const [opState, setOpState] = React.useState<OperationState>('idle');

  React.useEffect(() => {
    setOpState('idle');
  }, [activeModal?.id]);

  if (!activeModal) return null;

  const config = activeModal.config;

  const isProcessing = opState === 'processing';

  const handleDismiss = (onDismiss?: () => void) => {
    if (isProcessing || opState === 'success') return;
    onDismiss?.();
    popModal();
  };

  return (
    <Portal hostName="modals">
      {match(config)
        .with({ type: 'NETWORK_ERROR' }, (c) => {
          const handleRetry = async () => {
            if (opState === 'processing' || opState === 'success') return;

            setOpState('processing');

            const result = await c.onRetry();

            await match(result)
              .with({ type: 'ok' }, async () => {
                setOpState('success');

                await new Promise((resolve) => setTimeout(resolve, 400));

                handleDismiss();
              })
              .with({ type: 'err' }, async () => {
                setOpState('error');
              })
              .exhaustive();
          };
          return (
            <Modal
              key={activeModal.id}
              isVisible
              title={t('apiErrors.titles.network')}
              subtitle={t('apiErrors.NETWORK_ERROR')}
              onDismiss={() => handleDismiss(c.onDismiss)}
              icon={<StatusIcon state={opState} />}
              footer={
                <Box gap="s">
                  <Button
                    title={t('common.retry')}
                    onPress={handleRetry}
                    isLoading={isProcessing}
                    variant="primary"
                  />
                  <Button
                    title={t('common.dismiss')}
                    onPress={() => handleDismiss(c.onDismiss)}
                    variant="ghost"
                  />
                </Box>
              }
            />
          );
        })
        .with({ type: 'SERVER_ERROR' }, (c) => (
          <Modal
            key={activeModal.id}
            isVisible
            title={t('apiErrors.titles.error')}
            subtitle={t('apiErrors.INTERNAL_ERROR')}
            onDismiss={() => handleDismiss(c.onDismiss)}
            icon={<MaterialIcons name="cloud-off" size={64} color="red" />}
            footer={
              <Button
                title={t('common.dismiss')}
                onPress={() => handleDismiss(c.onDismiss)}
                variant="ghost"
              />
            }
          />
        ))
        .with({ type: 'UNKNOWN_ERROR' }, () => (
          <Modal
            key={activeModal.id}
            isVisible
            title={t('apiErrors.titles.error')}
            subtitle={t('apiErrors.UNKNOWN')}
            onDismiss={popModal}
            icon={<MaterialIcons name="error-outline" size={64} color="red" />}
            footer={<Button title={t('common.dismiss')} onPress={popModal} variant="ghost" />}
          />
        ))
        .with({ type: 'INFO' }, (c) => (
          <Modal
            key={activeModal.id}
            isVisible
            title={c.title}
            subtitle={c.message}
            onDismiss={() => handleDismiss(c.onConfirm)}
            icon={<MaterialIcons name="info-outline" size={64} color="blue" />}
            footer={
              <Button
                title={t('common.ok')}
                onPress={() => handleDismiss(c.onConfirm)}
                variant="primary"
              />
            }
          />
        ))
        .with({ type: 'CUSTOM' }, (c) => (
          <React.Fragment key={activeModal.id}>{c.render(popModal)}</React.Fragment>
        ))
        .exhaustive()}
    </Portal>
  );
};
