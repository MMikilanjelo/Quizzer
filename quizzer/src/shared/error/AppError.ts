export type AppErrorCode =
    | 'NETWORK_ERROR'
    | 'SERVER_ERROR'
    | 'UNKNOWN_ERROR';

export interface AppError {
    code: AppErrorCode;
    message: string;
}
