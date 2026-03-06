import { create } from 'zustand';
import React from 'react';
import { Result } from '@/src/shared/error/Result';

export type ModalConfig =
  | { type: 'SERVER_ERROR'; onDismiss?: () => void }
  | { type: 'NETWORK_ERROR'; onRetry: () => Promise<Result<unknown>>; onDismiss?: () => void }
  | { type: 'UNKNOWN_ERROR' }
  | { type: 'INFO'; title: string; message: string; onConfirm?: () => void }
  | { type: 'CUSTOM'; render: (popModal: () => void) => React.ReactNode };

interface QueuedModal {
  id: string;
  config: ModalConfig;
}

interface ModalState {
  queue: QueuedModal[];
  pushModal: (config: ModalConfig) => void;
  popModal: () => void;
  clearAll: () => void;
}

export const useModalStore = create<ModalState>((set, get) => ({
  queue: [],

  pushModal: (config) =>
    set((state) => {
      const alreadyExists = state.queue.some((m) => m.config.type === config.type);

      if (alreadyExists) return state;

      return {
        queue: [
          ...state.queue,
          {
            id: `${config.type}-${Date.now()}`,
            config,
          },
        ],
      };
    }),

  popModal: () =>
    set((state) => {
      if (state.queue.length === 0) return state;
      return { queue: state.queue.slice(1) };
    }),

  clearAll: () => set({ queue: [] }),
}));
