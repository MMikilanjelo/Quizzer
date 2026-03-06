import {AppError} from "@/src/shared/error/AppError";

export type Ok<T> = {
    type: "ok";
    value: T;
};

export type Err<E> = {
    type: "err";
    error: E;
};

export type Result<T, E = AppError> = Ok<T> | Err<E>;

export const ok = <T>(value: T): Ok<T> => ({
    type: "ok",
    value,
});

export const err = <E>(error: E): Err<E> => ({
    type: "err",
    error,
});
