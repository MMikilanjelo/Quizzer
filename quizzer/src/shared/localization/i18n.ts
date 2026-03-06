import {createInstance} from 'i18next';
import {initReactI18next} from 'react-i18next';
import * as Localization from 'expo-localization';
import en from './languages/en.json';

const resources = {en: {translation: en}};

const i18n = createInstance();

export const initI18n = async () => {
    return await i18n
        .use(initReactI18next)
        .init({
            resources,
            lng: Localization.getLocales()[0].languageCode ?? 'en',
            fallbackLng: 'en',
            interpolation: {
                escapeValue: false,
            },
        });
};

export default i18n;
