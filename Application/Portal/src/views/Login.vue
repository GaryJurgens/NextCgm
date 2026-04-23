<template>
  <div class="auth-container">
    <h2>Login</h2>
    <form @submit.prevent="handleLogin">
      <div class="form-group">
        <label>Email</label>
        <input type="email" v-model="email" required />
      </div>
      <div class="form-group">
        <label>Password</label>
        <input type="password" v-model="password" required />
      </div>
      <button type="submit" :disabled="loading">Login</button>
      <p v-if="error" class="error">{{ error }}</p>
    </form>
    <p>Don't have an account? <router-link to="/register">Register</router-link></p>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import api from '../services/api';

const router = useRouter();
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
      localStorage.setItem('emailForOtp', email.value);
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

<style scoped>
.auth-container {
  max-width: 400px;
  margin: 50px auto;
  padding: 20px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}
.form-group {
  margin-bottom: 15px;
}
.form-group label {
  display: block;
  margin-bottom: 5px;
}
.form-group input {
  width: 100%;
  padding: 8px;
  box-sizing: border-box;
}
button {
  width: 100%;
  padding: 10px;
  background-color: #007bff;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}
button:disabled {
  background-color: #ccc;
}
.error {
  color: red;
  margin-top: 10px;
}
</style>
