<template>
  <div class="dashboard-layout">
    <!-- Top Navigation Bar -->
    <header class="top-navbar">
      <div class="navbar-container">
        <div class="navbar-brand">
          <span class="logo-text">NextCGM</span>
        </div>
        <div class="navbar-actions">
          <router-link to="/dashboard" class="btn btn-sm btn-outline-primary mr-3">Dashboard</router-link>
          <router-link to="/settings" class="btn btn-sm btn-outline-primary mr-3">Settings</router-link>
          <button @click="logout" class="btn btn-sm btn-danger-outline ml-3">Logout</button>
        </div>
      </div>
    </header>

    <!-- Main Content -->
    <main class="dashboard-container">
      <div class="text-center mb-8">
        <h2 class="section-title text-3xl font-extrabold text-gray-900 sm:text-4xl">
          Subscription Plans
        </h2>
        <p class="mt-4 text-xl text-gray-600">
          Choose the right plan for your needs.
        </p>
      </div>

      <div v-if="loadingPlans" class="empty-state">
        <span class="spinner" style="border-top-color: var(--primary-color);"></span>
        <p>Loading plans...</p>
      </div>

      <div v-else class="plans-grid">
        <div v-for="plan in plans" :key="plan.id" class="plan-card">
          <!-- Top Part: Name, Description & Features HTML -->
          <div class="plan-header">
            <h3 class="plan-badge">
              {{ plan.name }}
            </h3>
            <p class="plan-desc">
              {{ plan.description || 'Access to premium features.' }}
            </p>
            <div class="plan-features" v-if="plan.featuresHtml" v-html="plan.featuresHtml"></div>
          </div>
          
          <!-- Bottom Part: Cost & Subscribe Button -->
          <div class="plan-footer">
            <div class="plan-price">
              {{ plan.currencySymbol }} {{ plan.price }}
              <span class="plan-interval">/mo</span>
            </div>
            <button @click="initiatePayment(plan)" :disabled="loading" class="btn btn-primary w-full">
              <span v-if="loading" class="spinner"></span>
              {{ loading ? 'Processing...' : 'Subscribe Now' }}
            </button>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import api from '../services/api';
import { getUserId, getUserEmail } from '../utils/auth';
import { useAuthStore } from '../stores/auth';

const router = useRouter();
const authStore = useAuthStore();
const plans = ref([]);
const loadingPlans = ref(true);
const loading = ref(false);

const logout = () => {
  authStore.logout();
  router.push('/login');
};

onMounted(async () => {
  try {
    const userId = getUserId();
    const response = await api.getPlans({ userId: userId });
    if (response.data && response.data.success) {
      plans.value = response.data.plans;
    }
  } catch (error) {
    console.error('Error fetching plans:', error);
  } finally {
    loadingPlans.value = false;
  }
});

const initiatePayment = async (plan) => {
  try {
    loading.value = true;
    
    const userId = getUserId();
    const userEmail = getUserEmail();

    const payload = {
      userEntityID: userId,
      amount: plan.price * 100, // convert to smallest currency unit (e.g. cents)
      planId: plan.id, // Backend expects SubscriptionPlanID (Guid)
      email: userEmail 
    };
    console.log("InitiatePayment payload: ", payload);

    const response = await api.initiatePayment(payload);

    if (response.data.success && response.data.authorizationUrl) {
      // Redirect to Paystack payment page
      window.location.href = response.data.authorizationUrl;
    } else {
      alert('Failed to initiate payment: ' + (response.data.message || 'Unknown error'));
    }
  } catch (error) {
    console.error('Payment initiation error', error);
    alert('An error occurred while initiating payment.');
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
/* Layout */
.dashboard-layout {
  min-height: 100vh;
  background-color: var(--bg-main);
}

/* Top Navbar */
.top-navbar {
  background-color: #ffffff;
  border-bottom: 1px solid var(--border-color);
  box-shadow: var(--shadow-sm);
  position: sticky;
  top: 0;
  z-index: 10;
}

.navbar-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0.75rem 2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.navbar-brand {
  display: flex;
  align-items: center;
}

.logo-text {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--primary-color);
  letter-spacing: -0.025em;
}

.navbar-actions {
  display: flex;
  align-items: center;
}

.ml-3 { margin-left: 1rem; }
.mr-3 { margin-right: 1rem; }
.mb-8 { margin-bottom: 2rem; }
.text-center { text-align: center; }

/* Main Container */
.dashboard-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}

/* Section Title */
.section-title {
  font-size: 1.5rem;
  font-weight: 600;
  color: #111827;
  margin-bottom: 0.5rem;
}

/* Buttons */
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
  transition: all 0.2s ease;
  text-decoration: none;
}

.w-full {
  width: 100%;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-sm {
  padding: 0.375rem 0.75rem;
  font-size: 0.75rem;
}

.btn-primary {
  background-color: var(--primary-color);
  color: white;
  box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
}

.btn-primary:hover:not(:disabled) {
  background-color: var(--primary-hover);
}

.btn-outline-primary {
  background-color: transparent;
  border-color: var(--primary-color);
  color: var(--primary-color);
}

.btn-outline-primary:hover:not(:disabled) {
  background-color: #eff6ff;
}

.btn-danger-outline {
  background-color: transparent;
  border-color: #fca5a5;
  color: var(--danger-color);
}

.btn-danger-outline:hover:not(:disabled) {
  background-color: #fef2f2;
  border-color: var(--danger-color);
}

/* Spinner */
.spinner {
  display: inline-block;
  width: 1rem;
  height: 1rem;
  margin-right: 0.5rem;
  border: 2px solid rgba(255,255,255,0.3);
  border-radius: 50%;
  border-top-color: white;
  animation: spin 1s ease-in-out infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Empty State */
.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  background-color: white;
  border-radius: 0.75rem;
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-sm);
}

/* Plans Grid */
.plans-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 2rem;
  margin-top: 2rem;
}

.plan-card {
  background: white;
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  border: 1px solid var(--border-color);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.plan-card:hover {
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
}

.plan-header {
  padding: 2rem;
  border-bottom: 1px solid var(--border-color);
}

.plan-badge {
  display: inline-block;
  padding: 0.25rem 0.75rem;
  background-color: #eff6ff;
  color: var(--primary-color);
  border-radius: 9999px;
  font-size: 0.875rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin: 0 0 1rem 0;
}

.plan-price {
  font-size: 3rem;
  font-weight: 800;
  color: #111827;
  display: flex;
  align-items: baseline;
}

.plan-interval {
  font-size: 1.25rem;
  font-weight: 500;
  color: var(--text-muted);
  margin-left: 0.25rem;
}

.plan-desc {
  margin-top: 0.5rem;
  color: var(--text-muted);
  font-size: 1rem;
}

.plan-features {
  margin-top: 1.5rem;
  color: var(--text-main);
  font-size: 0.875rem;
  line-height: 1.5;
}

/* Base styles for the injected HTML */
.plan-features :deep(ul) {
  list-style-type: disc;
  padding-left: 1.5rem;
  margin-top: 0.5rem;
}

.plan-features :deep(li) {
  margin-bottom: 0.5rem;
}

.plan-footer {
  padding: 2rem;
  background-color: #f9fafb;
  flex-grow: 1;
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
}
</style>