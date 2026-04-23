export const getUser = () => {
  try {
    const userData = localStorage.getItem('user');
    return userData ? JSON.parse(userData) : null;
  } catch (error) {
    console.error('Error parsing user data from localStorage', error);
    return null;
  }
};

export const getUserId = () => {
  const user = getUser();
  return user?.userEntityID || user?.userEntityId || user?.id || null;
};

export const getUserEmail = () => {
  const user = getUser();
  return user?.emailUsername || localStorage.getItem('userEmail') || 'test@example.com';
};
