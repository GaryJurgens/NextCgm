import { defineStore } from 'pinia';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: null,
    user: null,
    emailForOtp: null,
  }),
  getters: {
    isAuthenticated: (state) => !!state.token,
  },
  actions: {
    setToken(token) {
      this.token = token;
      localStorage.setItem('token', token);
    },
    setUser(user) {
      this.user = user;
      localStorage.setItem('user', JSON.stringify(user));
    },
    setEmailForOtp(email) {
      this.emailForOtp = email;
      localStorage.setItem('emailForOtp', email);
    },
    logout() {
      this.token = null;
      this.user = null;
      this.emailForOtp = null;
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      localStorage.removeItem('emailForOtp');
    },
    initializeAuth() {
      const token = localStorage.getItem('token');
      const user = localStorage.getItem('user');
      const emailForOtp = localStorage.getItem('emailForOtp');
      if (token) this.token = token;
      if (user) this.user = JSON.parse(user);
      if (emailForOtp) this.emailForOtp = emailForOtp;
    }
  },
  persist: true
});