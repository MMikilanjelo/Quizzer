import {create} from 'zustand';

interface LoadingState {
    isVisible: boolean;
    message?: string;
    showLoader: (message?: string) => void;
    hideLoader: () => void;
}

export const useLoadingStore = create<LoadingState>((set) => ({
    isVisible: false,
    message: undefined,
    showLoader: (message) => set({isVisible: true, message}),
    hideLoader: () => set({isVisible: false, message: undefined}),
}));
