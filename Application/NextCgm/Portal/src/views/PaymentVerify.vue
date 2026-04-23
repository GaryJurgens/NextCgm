<template>
  <div class="dashboard-layout">
    <header class="top-navbar">
      <div class="navbar-container">
        <div class="navbar-brand">
          <span class="logo-text">NextCGM</span>
        </div>
      </div>
    </header>

    <main class="dashboard-container" style="display: flex; justify-content: center; align-items: center; min-height: 60vh;">
      <div class="text-center p-8 bg-white rounded-lg shadow-sm border border-gray-200">
        
        <div v-if="verifying">
          <span class="spinner" style="border-top-color: var(--primary-color); width: 2rem; height: 2rem;"></span>
          <h2 class="mt-4 text-xl font-semibold">Verifying Payment...</h2>
          <p class="mt-2 text-gray-500">Please wait while we confirm your transaction with Paystack.</p>
        </div>

        <div v-else-if="success">
          <div style="font-size: 3rem; color: #166534;">✅</div>
          <h2 class="mt-4 text-2xl font-bold text-gray-900">Payment Successful!</h2>
          <p class="mt-2 text-gray-600">Your subscription has been activated successfully.</p>
          <button @click="goToDashboard" class="btn btn-primary mt-6">Go to Dashboard</button>
        </div>

        <div v-else>
          <div style="font-size: 3rem; color: #991b1b;">❌</div>
          <h2 class="mt-4 text-2xl font-bold text-gray-900">Payment Failed</h2>
          <p class="mt-2 text-gray-600">{{ errorMessage || 'We could not verify your transaction.' }}</p>
          <button @click="goToPayment" class="btn btn-danger-outline mt-6">Try Again</button>
        </div>

      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import api from '../services/api';

const route = useRoute();
const router = useRouter();

const verifying = ref(true);
const success = ref(false);
const errorMessage = ref('');

onMounted(async () => {
  const reference = route.query.reference || route.query.trxref || route.query.transaction_id;

  if (!reference) {
    verifying.value = false;
    success.value = false;
    errorMessage.value = 'No transaction reference found.';
    return;
  }

  try {
    const response = await api.verifyPayment({ reference });
    
    if (response.data && response.data.success) {
      success.value = true;
    } else {
      success.value = false;
      errorMessage.value = response.data.message || 'Transaction verification failed.';
    }
  } catch (error) {
    console.error('Verification error:', error);
    success.value = false;
    errorMessage.value = 'An error occurred while communicating with the server.';
  } finally {
    verifying.value = false;
  }
});

const goToDashboard = () => {
  router.push('/dashboard');
};

const goToPayment = () => {
  router.push('/payment');
};
</script>

<style scoped>
.dashboard-layout {
  min-height: 100vh;
  background-color: var(--bg-main);
}
.top-navbar {
  background-color: #ffffff;
  border-bottom: 1px solid var(--border-color);
  box-shadow: var(--shadow-sm);
}
.navbar-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0.75rem 2rem;
}
.logo-text {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--primary-color);
}
.dashboard-container {
  max-width: 1200px;
  margin: 0 auto;
}
.bg-white { background-color: #ffffff; }
.rounded-lg { border-radius: var(--radius-lg); }
.shadow-sm { box-shadow: var(--shadow-sm); }
.border { border: 1px solid var(--border-color); }
.p-8 { padding: 2rem; }
.text-center { text-align: center; }
.mt-4 { margin-top: 1rem; }
.mt-2 { margin-top: 0.5rem; }
.mt-6 { margin-top: 1.5rem; }
.text-gray-500 { color: #6b7280; }
.text-gray-600 { color: #4b5563; }
.text-gray-900 { color: #111827; }
.font-semibold { font-weight: 600; }
.font-bold { font-weight: 700; }
.text-xl { font-size: 1.25rem; }
.text-2xl { font-size: 1.5rem; }
.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0.625rem 1.25rem;
  font-size: 0.875rem;
  font-weight: 500;
  border-radius: 0.5rem;
  border: 1px solid transparent;
  cursor: pointer;
}
.btn-primary {
  background-color: var(--primary-color);
  color: white;
}
.btn-primary:hover {
  background-color: var(--primary-hover);
}
.btn-danger-outline {
  background-color: transparent;
  border-color: #fca5a5;
  color: var(--danger-color);
}
.btn-danger-outline:hover {
  background-color: #fef2f2;
}
.spinner {
  display: inline-block;
  border: 3px solid rgba(0,0,0,0.1);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}
@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>