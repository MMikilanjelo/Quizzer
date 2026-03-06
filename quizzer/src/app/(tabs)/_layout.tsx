import React from 'react';
import { View } from 'react-native';
import { Tabs } from 'expo-router';
import { Feather } from '@expo/vector-icons';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { FABMenu, FABAction } from '@/src/components';
import { Camera, Share2 } from 'lucide-react-native';
import { theme } from '@/src/shared/theme/theme';

const TabsLayout = () => {
  const insets = useSafeAreaInsets();

  const tabBarHeight = 50 + insets.bottom;

  const menuActions: FABAction[] = [
    {
      id: 'camera-action',
      labelKey: 'common.dismiss',
      icon: <Camera size={20} />,
      onPress: () => {},
    },
    {
      id: 'share-action',
      labelKey: 'common.dismiss',
      icon: <Share2 size={20} />,
      onPress: () => {},
    },
  ];

  return (
    <View style={{ flex: 1 }}>
      <Tabs
        screenOptions={{
          tabBarActiveTintColor: theme.colors.tabsActive,
          tabBarInactiveTintColor: theme.colors.tabsInactive,
          tabBarStyle: {
            backgroundColor: theme.colors.tabBarBg,
            height: tabBarHeight,
            paddingTop: 8,
          },
          headerShown: false,
        }}
      >
        <Tabs.Screen
          name="index"
          options={{
            title: '',
            tabBarIcon: ({ color, size }) => <Feather name="home" size={size} color={color} />,
          }}
        />
        <Tabs.Screen
          name="friends"
          options={{
            title: '',
            tabBarIcon: ({ color, size }) => <Feather name="search" size={size} color={color} />,
          }}
        />
        <Tabs.Screen
          name="leaderboard"
          options={{
            title: '',
            tabBarIcon: ({ color, size }) => <Feather name="bell" size={size} color={color} />,
          }}
        />
        <Tabs.Screen
          name="quizzes"
          options={{
            title: '',
            tabBarIcon: ({ color, size }) => <Feather name="mail" size={size} color={color} />,
          }}
        />
      </Tabs>

      <FABMenu actions={menuActions} bottomOffset={tabBarHeight + 20} rightOffset={20} />
    </View>
  );
};

export default TabsLayout;
