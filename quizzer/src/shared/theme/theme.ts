import { createTheme } from '@shopify/restyle';

const palette = {
  ink: {
    lighter: '#72777A',
    light: '#6C7072',
    base: '#404446',
    dark: '#303437',
    darker: '#202325',
    darkest: '#090A0A',
    darkest70: '#090A0AB3',
  },
  sky: {
    lightest: '#F7F9FA',
    lighter: '#F2F4F5',
    light: '#E3E5E5',
    base: '#CDCFD0',
    dark: '#979C9E',
    white: '#FFFFFF',
  },
  primary: {
    lightest: '#E7E7FF',
    lighter: '#C6C4FF',
    light: '#9990FF',
    base: '#6B4EFF',
    dark: '#5538EE',
  },
  red: {
    lightest: '#FFE5E5',
    lighter: '#FF9898',
    light: '#FF6D6D',
    base: '#FF5247',
    dark: '#D3180C',
  },
  transparent: 'transparent',
};

export const theme = createTheme({
  colors: {
    tabsInactive: palette.sky.dark,
    tabsActive: palette.primary.base,
    tabBarBg: palette.sky.white,
    tint: palette.ink.darkest70,
    mainBackground: palette.sky.white,
    mainText: palette.ink.darkest,
    primaryText: palette.primary.base,
    subTitleText: palette.ink.lighter,
    captionText: palette.ink.lighter,
    transparent: palette.transparent,
    modalHandle: palette.sky.base,

    // Primary Button
    buttonPrimaryBg: palette.primary.base,
    buttonPrimaryBgPressed: palette.primary.dark,
    buttonPrimaryText: palette.sky.white,
    buttonPrimaryTextPressed: palette.sky.white,

    // Secondary Button
    buttonSecondaryBg: palette.primary.lightest,
    buttonSecondaryText: palette.primary.base,
    buttonSecondaryBgPressed: palette.primary.lighter,
    buttonSecondaryTextPressed: palette.primary.dark,

    // Outline Button
    buttonOutlineBorder: palette.primary.base,
    buttonOutlineText: palette.primary.base,
    buttonOutlineTextPressed: palette.primary.dark,

    // Ghost Button
    buttonGhostBgPressed: palette.primary.lightest,
    buttonGhostTextPressed: palette.primary.base,

    // Disabled State
    buttonDisabledBg: palette.sky.light,
    buttonDisabledText: palette.sky.dark,

    // Feedback
    activityIndicatorPrimary: palette.sky.base,
    activityIndicatorSecondary: palette.sky.base,
    textError: palette.red.base,

    // Inputs
    inputTextPlaceholder: palette.ink.lighter,
    inputTextIconDefault: palette.ink.darkest,
    inputTextDefault: palette.ink.darkest,
    inputTextDisabled: palette.sky.base,
    inputBorderDefault: palette.sky.light,
    inputBorderFocused: palette.primary.base,
    inputBorderError: palette.red.base,
    inputBackgroundDefault: palette.sky.white,
    inputBackgroundDisabled: palette.sky.lighter,

    switchBg: palette.sky.light,
    switchToggledBg: palette.primary.base,
    switchKnobBg: palette.sky.white,

    radioBorder: palette.sky.base,
    radioBorderActive: palette.primary.base,
    radioActiveBg: palette.primary.base,
    radioBg: palette.sky.white,
    radioCircleActive: palette.sky.white,

    checkboxBgActive: palette.primary.base,
    checkboxBg: palette.sky.white,
    checkboxBorder: palette.sky.base,
    checkboxBorderActive: palette.primary.base,
    checkboxCheck: palette.sky.white,

    activityIndicatorBg: palette.primary.lightest,
    activityIndicatorShadow: palette.ink.darkest,
  },
  spacing: {
    none: 0,
    xs: 4,
    s: 8,
    sm: 12,
    m: 16,
    l: 24,
    xl: 32,
    auto: 'auto',
  },
  padding: {
    none: 0,
    xs: 4,
    s: 8,
    m: 16,
    l: 24,
    xl: 32,
  },
  borderRadii: {
    none: 0,
    xs: 4,
    s: 8,
    m: 12,
    l: 16,
    xl: 24,
    xxl: 32,
    pill: 999,
  },
  borderWidths: {
    none: 0,
    s: 1,
    m: 2,
    l: 4,
  },
  shadows: {
    none: {
      shadowColor: 'transparent',
      shadowOffset: { width: 0, height: 0 },
      shadowOpacity: 0,
      shadowRadius: 0,
      elevation: 0,
    },
    small: {
      shadowColor: palette.ink.darkest,
      shadowOffset: { width: 0, height: 2 },
      shadowOpacity: 0.1,
      shadowRadius: 3,
      elevation: 3,
    },
    medium: {
      shadowColor: palette.ink.darkest,
      shadowOffset: { width: 0, height: 4 },
      shadowOpacity: 0.2,
      shadowRadius: 5,
      elevation: 6,
    },
    large: {
      shadowColor: palette.ink.darkest,
      shadowOffset: { width: 0, height: 6 },
      shadowOpacity: 0.3,
      shadowRadius: 8,
      elevation: 10,
    },
  },
  textVariants: {
    defaults: {
      fontFamily: 'Inter_Bold',
      color: 'mainText',
      fontSize: 16,
      lineHeight: 24,
    },
    title1: {
      fontFamily: 'Inter_700Bold',
      fontSize: 48,
      lineHeight: 56,
      color: 'mainText',
    },
    title2: {
      fontFamily: 'Inter_700Bold',
      fontSize: 32,
      lineHeight: 36,
      color: 'mainText',
    },
    title3: {
      fontFamily: 'Inter_700Bold',
      fontSize: 24,
      lineHeight: 32,
      color: 'mainText',
    },
    // Large Variants
    largeNoneBold: { fontFamily: 'Inter_700Bold', fontSize: 18, lineHeight: 18 },
    largeNoneMedium: { fontFamily: 'Inter_500Medium', fontSize: 18, lineHeight: 18 },
    largeNoneRegular: { fontFamily: 'Inter_400Regular', fontSize: 18, lineHeight: 18 },
    largeTightBold: { fontFamily: 'Inter_700Bold', fontSize: 18, lineHeight: 20 },
    largeTightMedium: { fontFamily: 'Inter_500Medium', fontSize: 18, lineHeight: 20 },
    largeTightRegular: { fontFamily: 'Inter_400Regular', fontSize: 18, lineHeight: 20 },
    largeNormalBold: { fontFamily: 'Inter_700Bold', fontSize: 18, lineHeight: 24 },
    largeNormalMedium: { fontFamily: 'Inter_500Medium', fontSize: 18, lineHeight: 24 },
    largeNormalRegular: { fontFamily: 'Inter_400Regular', fontSize: 18, lineHeight: 24 },
    // Regular Variants
    regularNoneBold: { fontFamily: 'Inter_700Bold', fontSize: 16, lineHeight: 16 },
    regularNoneMedium: { fontFamily: 'Inter_500Medium', fontSize: 16, lineHeight: 20 },
    regularNoneRegular: { fontFamily: 'Inter_400Regular', fontSize: 16, lineHeight: 20 },
    regularTightBold: { fontFamily: 'Inter_700Bold', fontSize: 16, lineHeight: 20 },
    regularTightMedium: { fontFamily: 'Inter_500Medium', fontSize: 16, lineHeight: 20 },
    regularTightRegular: { fontFamily: 'Inter_400Regular', fontSize: 16, lineHeight: 20 },
    regularNormalBold: { fontFamily: 'Inter_700Bold', fontSize: 16, lineHeight: 24 },
    regularNormalMedium: { fontFamily: 'Inter_500Medium', fontSize: 16, lineHeight: 24 },
    regularNormalRegular: { fontFamily: 'Inter_400Regular', fontSize: 16, lineHeight: 24 },
    // Small Variants
    smallNoneBold: { fontFamily: 'Inter_700Bold', fontSize: 14, lineHeight: 14 },
    smallNoneMedium: { fontFamily: 'Inter_500Medium', fontSize: 14, lineHeight: 14 },
    smallNoneRegular: { fontFamily: 'Inter_400Regular', fontSize: 14, lineHeight: 14 },
    smallTightBold: { fontFamily: 'Inter_700Bold', fontSize: 14, lineHeight: 16 },
    smallTightMedium: { fontFamily: 'Inter_500Medium', fontSize: 14, lineHeight: 16 },
    smallTightRegular: { fontFamily: 'Inter_400Regular', fontSize: 14, lineHeight: 16 },
    smallNormalBold: { fontFamily: 'Inter_700Bold', fontSize: 14, lineHeight: 20 },
    smallNormalMedium: { fontFamily: 'Inter_500Medium', fontSize: 14, lineHeight: 20 },
    smallNormalRegular: { fontFamily: 'Inter_400Regular', fontSize: 14, lineHeight: 20 },
    // Tiny Variants
    tinyNoneBold: { fontFamily: 'Inter_700Bold', fontSize: 12, lineHeight: 12 },
    tinyNoneMedium: { fontFamily: 'Inter_500Medium', fontSize: 12, lineHeight: 12 },
    tinyNoneRegular: { fontFamily: 'Inter_400Regular', fontSize: 12, lineHeight: 12 },
    tinyTightBold: { fontFamily: 'Inter_700Bold', fontSize: 12, lineHeight: 14 },
    tinyTightMedium: { fontFamily: 'Inter_500Medium', fontSize: 12, lineHeight: 14 },
    tinyTightRegular: { fontFamily: 'Inter_400Regular', fontSize: 12, lineHeight: 14 },
    tinyNormalBold: { fontFamily: 'Inter_700Bold', fontSize: 12, lineHeight: 16 },
    tinyNormalMedium: { fontFamily: 'Inter_500Medium', fontSize: 12, lineHeight: 16 },
    tinyNormalRegular: { fontFamily: 'Inter_400Regular', fontSize: 12, lineHeight: 16 },
  },
});

export type Theme = typeof theme;
