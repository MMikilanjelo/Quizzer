import axios, { isAxiosError, AxiosRequestConfig } from 'axios';
import { Result, ok, err } from '@/src/shared/error/Result';
import { AppError } from '@/src/shared/error/AppError';

const axiosInstance = axios.create({
  baseURL: 'https://your-api.com/api',
  timeout: 10000,
  headers: { 'Content-Type': 'application/json' },
});

const handleApiError = (e: unknown): AppError => {
  if (isAxiosError(e)) {
    if (!e.response) {
      return { code: 'NETWORK_ERROR', message: 'Unable to connect to the server.' };
    }

    const status = e.response.status;

    if (status >= 500) {
      return { code: 'SERVER_ERROR', message: 'The server encountered an error.' };
    }
  }

  return { code: 'UNKNOWN_ERROR', message: 'An unexpected error occurred.' };
};

export const apiClient = {
  get: async <T>(url: string, config?: AxiosRequestConfig): Promise<Result<T>> => {
    try {
      const response = await axiosInstance.get<T>(url, config);
      return ok(response.data);
    } catch (e) {
      return err(handleApiError(e));
    }
  },

  post: async <T, D = unknown>(
    url: string,
    data?: D,
    config?: AxiosRequestConfig
  ): Promise<Result<T>> => {
    try {
      const response = await axiosInstance.post<T>(url, data, config);
      return ok(response.data);
    } catch (e) {
      return err(handleApiError(e));
    }
  },
};
