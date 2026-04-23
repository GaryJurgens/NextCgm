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
          <router-link to="/payment" class="btn btn-sm btn-outline-primary mr-3">Plans</router-link>
          <button @click="logout" class="btn btn-sm btn-danger-outline ml-3">Logout</button>
        </div>
      </div>
    </header>

    <!-- Main Content -->
    <main class="dashboard-container">
      <div class="settings-card">
        <div class="settings-header">
          <h3 class="section-title">
            Account Settings
          </h3>
          <p class="settings-desc">
            Manage your subscriptions and billing details.
          </p>
        </div>

        <div class="settings-body">
          <dl class="settings-list">
            <div class="settings-row">
              <dt class="settings-label">
                Subscription Status
              </dt>
              <dd class="settings-value">
                <div v-if="loading" class="flex items-center">
                  <span class="spinner" style="border-top-color: var(--primary-color);"></span>
                  <span>Checking...</span>
                </div>
                
                <span v-else-if="!hasActiveSubscription && !hasFailedSubscription" class="status-error">
                  No active subscription
                </span>
                
                <div v-else-if="hasFailedSubscription" class="failed-alert">
                  <span class="status-error mb-2" style="display: inline-block;">Payment Failed</span>
                  <span class="failed-desc">
                    We could not process your latest payment. You have until <strong>{{ gracePeriodEndDate }}</strong> to update your payment details or you will lose access.
                  </span>
                  <button @click="updatePaymentDetails" class="btn btn-primary mt-3">
                    Update Payment Details
                  </button>
                </div>

                <div v-else class="active-status-container">
                  <span class="status-running">Active (Next charge: {{ nextChargeDate }})</span>
                </div>

                <button 
                  v-if="hasActiveSubscription || hasFailedSubscription"
                  @click="cancelSubscription" 
                  :disabled="canceling"
                  class="btn btn-danger-outline ml-auto mt-2 sm:mt-0">
                  <span v-if="canceling" class="spinner" style="border-top-color: var(--danger-color);"></span>
                  {{ canceling ? 'Canceling...' : 'Cancel Subscription' }}
                </button>
              </dd>
            </div>
          </dl>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import api from '../services/api';
import { getUserId } from '../utils/auth';
import { useAuthStore } from '../stores/auth';

const router = useRouter();
const authStore = useAuthStore();
const loading = ref(true);
const canceling = ref(false);
const hasActiveSubscription = ref(false);
const hasFailedSubscription = ref(false);
const nextChargeDate = ref('');
const gracePeriodEndDate = ref('');
const subscriptionId = ref(null);

const logout = () => {
  authStore.logout();
  router.push('/login');
};

const loadSubscriptions = async () => {
  try {
    loading.value = true;
    
    const userId = getUserId();

    const response = await api.getUserSubscriptions({
      userEntityID: userId
    });

    if (response.data && response.data.success && response.data.payload && response.data.payload.length > 0) {
      const activeSub = response.data.payload.find(s => s.status === 'active');
      const failedSub = response.data.payload.find(s => s.status === 'past_due');

      if (failedSub) {
        hasFailedSubscription.value = true;
        subscriptionId.value = failedSub.billingSubscriptionEntityID;
        gracePeriodEndDate.value = new Date(failedSub.gracePeriodEndDate).toLocaleDateString();
      } else if (activeSub) {
        hasActiveSubscription.value = true;
        subscriptionId.value = activeSub.billingSubscriptionEntityID;
        nextChargeDate.value = new Date(activeSub.currentPeriodEnd).toLocaleDateString();
      }
    } else {
      hasActiveSubscription.value = false;
    }
  } catch (error) {
    console.error('Failed to load subscriptions', error);
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  loadSubscriptions();
});

const updatePaymentDetails = () => {
  router.push('/payment');
};

const cancelSubscription = async () => {
  if (!confirm('Are you sure you want to cancel your subscription?')) return;
  
  try {
    canceling.value = true;
    const response = await api.cancelSubscription({
      subscriptionId: subscriptionId.value
    });
    
    if (response.data && response.data.success) {
      alert('Subscription cancelled successfully.');
      hasActiveSubscription.value = false;
      hasFailedSubscription.value = false;
    } else {
      alert('Failed to cancel: ' + (response.data.message || 'Unknown error'));
    }
  } catch (error) {
    console.error('Failed to cancel subscription', error);
    alert('An error occurred');
  } finally {
    canceling.value = false;
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
.ml-auto { margin-left: auto; }
.mt-2 { margin-top: 0.5rem; }
.mt-3 { margin-top: 1rem; }
.mb-2 { margin-bottom: 0.5rem; }
.flex { display: flex; }
.items-center { align-items: center; }

/* Main Container */
.dashboard-container {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem;
}

/* Settings Card */
.settings-card {
  background: white;
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  border: 1px solid var(--border-color);
  overflow: hidden;
}

.settings-header {
  padding: 1.5rem 1.5rem;
  border-bottom: 1px solid var(--border-color);
}

.section-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #111827;
  margin: 0 0 0.25rem 0;
}

.settings-desc {
  font-size: 0.875rem;
  color: var(--text-muted);
  margin: 0;
}

.settings-body {
  padding: 0;
}

.settings-list {
  margin: 0;
}

.settings-row {
  display: flex;
  flex-direction: column;
  padding: 1.5rem;
  border-bottom: 1px solid var(--border-color);
}

@media (min-width: 640px) {
  .settings-row {
    flex-direction: row;
    align-items: flex-start;
  }
  .sm\:mt-0 {
    margin-top: 0;
  }
}

.settings-row:last-child {
  border-bottom: none;
}

.settings-label {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--text-muted);
  width: 100%;
  max-width: 200px;
  margin-bottom: 1rem;
}

@media (min-width: 640px) {
  .settings-label {
    margin-bottom: 0;
  }
}

.settings-value {
  flex-grow: 1;
  font-size: 0.875rem;
  color: var(--text-main);
  margin: 0;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 1rem;
}

@media (min-width: 640px) {
  .settings-value {
    flex-direction: row;
    align-items: center;
  }
}

/* Status Colors & Badges */
.status-running {
  background-color: #dcfce7;
  color: #166534;
  padding: 0.25rem 0.625rem;
  border-radius: 9999px;
  font-weight: 500;
  display: inline-block;
}

.status-error {
  background-color: #fee2e2;
  color: #991b1b;
  padding: 0.25rem 0.625rem;
  border-radius: 9999px;
  font-weight: 500;
  display: inline-block;
}

.failed-alert {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
}

.failed-desc {
  color: var(--text-muted);
  font-size: 0.875rem;
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
</style>