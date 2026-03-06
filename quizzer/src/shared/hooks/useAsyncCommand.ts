import {useState} from 'react';
import {match} from 'ts-pattern';
import {useModalStore} from "@/src/shared/stores/ui/useModalStore";
import {Result} from '@/src/shared/error/Result';
import {AppError} from '@/src/shared/error/AppError';

interface CommandOptions<T> {
    onSuccess?: (data: T) => void;
    onError?: (error: AppError) => void;
    silent?: boolean;
}

export const useAsyncCommand = <Args extends any[], ReturnValue>(
    commandFn: (...args: Args) => Promise<Result<ReturnValue, AppError>>,
    options?: CommandOptions<ReturnValue>
) => {
    const [isLoading, setIsLoading] = useState(false);

    const pushModal = useModalStore((state) => state.pushModal);

    const run = async (...args: Args) => {
        return await commandFn(...args);
    };

    const execute = async (...args: Args): Promise<Result<ReturnValue, AppError>> => {
        setIsLoading(true);
        const result = await commandFn(...args);
        setIsLoading(false);

        if (result.type === 'ok') {
            options?.onSuccess?.(result.value);
            return result;
        }

        const error = result.error;

        options?.onError?.(error);

        if (options?.silent) {
            return result;
        }

        match(error.code)
            .with('NETWORK_ERROR', () => {
                pushModal({
                    type: 'NETWORK_ERROR',
                    onRetry: () => run(...args),
                });
            })
            .with('SERVER_ERROR', () => {
                pushModal({type: 'SERVER_ERROR'})
            })
            .with('UNKNOWN_ERROR', () => {
                pushModal({type: 'UNKNOWN_ERROR'})
            })
            .exhaustive();

        return result;
    };

    return {execute, isLoading};
};

