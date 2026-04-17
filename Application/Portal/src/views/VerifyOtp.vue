<template>
  <div class="auth-container">
    <h2>Verify OTP</h2>
    <p>Please enter the OTP sent to your email.</p>
    <form @submit.prevent="handleVerify">
      <div class="form-group">
        <label>OTP Code</label>
        <input type="text" v-model="otp" required />
      </div>
      <button type="submit" :disabled="loading">Verify</button>
      <p v-if="error" class="error">{{ error }}</p>
    </form>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import api from '../services/api';

const router = useRouter();
const otp = ref('');
const email = ref('');
const loading = ref(false);
const error = ref('');

onMounted(() => {
  email.value = localStorage.getItem('emailForOtp') || '';
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
      localStorage.setItem('token', response.data.token);
      localStorage.setItem('user', JSON.stringify(response.data.payload));
      localStorage.removeItem('emailForOtp');
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
  background-color: #17a2b8;
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
