import {Result} from "@/src/shared/error/Result";
import {RegisterFormData} from "@/src/features/auth/schemas/register-schema";
import {apiClient} from "@/src/shared/api/api";

export interface RegisterApiPayload {
    email: string;
    password: string;
}

export const authService = {
    async register(formData: RegisterFormData): Promise<Result<void>> {

        const payload: RegisterApiPayload = {
            email: formData.email,
            password: formData.password,
        };

        return apiClient.post<void, RegisterApiPayload>('/register', payload);
    },
}


