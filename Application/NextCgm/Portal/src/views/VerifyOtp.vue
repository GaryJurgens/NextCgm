<template>
  <div class="auth-page">
    <div class="auth-card">
      <h2>Verify OTP</h2>
      <p class="text-center text-muted mb-4">Please enter the OTP sent to your email.</p>
      <form @submit.prevent="handleVerify">
        <div class="form-group">
          <label>OTP Code</label>
          <input type="text" v-model="otp" class="form-control" placeholder="Enter OTP" required />
        </div>
        <button type="submit" class="btn-block" :disabled="loading">
          {{ loading ? 'Verifying...' : 'Verify' }}
        </button>
        <p v-if="error" class="error-message">{{ error }}</p>
      </form>
      <div class="auth-links">
        <p><router-link to="/login">Back to Login</router-link></p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import api from '../services/api';
import { useAuthStore } from '../stores/auth';

const router = useRouter();
const authStore = useAuthStore();
const otp = ref('');
const email = ref('');
const loading = ref(false);
const error = ref('');

onMounted(() => {
  email.value = authStore.emailForOtp || localStorage.getItem('emailForOtp') || '';
  if (!email.value) {
    router.push('/login');
  }
});

const handleVerify = async () => {
  loading.value = true;
  error.value = '';
  try {
    const response = await api.verifyOtp({
      EmailUsername: email.value,
      VerificationCode: otp.value
    });
    if (response.data.success) {
      authStore.setToken(response.data.token);
      authStore.setUser(response.data.payload);
      router.push('/dashboard');
    } else {
      error.value = response.data.message || 'Verification failed';
    }
  } catch (err) {
    error.value = err.response?.data?.message || err.message || 'An error occurred';
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
.text-center {
  text-align: center;
}
.text-muted {
  color: var(--text-muted);
}
.mb-4 {
  margin-bottom: 1.5rem;
}
</style>
