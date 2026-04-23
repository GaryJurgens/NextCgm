<template>
  <div class="auth-page">
    <div class="auth-card">
      <h2>Welcome Back</h2>
      <form @submit.prevent="handleLogin">
        <div class="form-group">
          <label>Email</label>
          <input type="email" v-model="email" class="form-control" placeholder="Enter your email" required />
        </div>
        <div class="form-group">
          <label>Password</label>
          <input type="password" v-model="password" class="form-control" placeholder="Enter your password" required />
        </div>
        <button type="submit" class="btn-block" :disabled="loading">
          {{ loading ? 'Logging in...' : 'Login' }}
        </button>
        <p v-if="error" class="error-message">{{ error }}</p>
      </form>
      <div class="auth-links">
        <p>Don't have an account? <router-link to="/register">Register here</router-link></p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import api from '../services/api';
import { useAuthStore } from '../stores/auth';

const router = useRouter();
const authStore = useAuthStore();
const email = ref('');
const password = ref('');
const loading = ref(false);
const error = ref('');

const handleLogin = async () => {
  loading.value = true;
  error.value = '';
  try {
    const response = await api.login({
      EmailUsername: email.value,
      Password: password.value
    });
    if (response.data.success) {
      // Login sends OTP, so we redirect to Verify OTP
      authStore.setEmailForOtp(email.value);
      router.push('/verify-otp');
    } else {
      error.value = response.data.message || 'Login failed';
    }
  } catch (err) {
    error.value = err.response?.data?.message || err.message || 'An error occurred';
  } finally {
    loading.value = false;
  }
};
</script>
