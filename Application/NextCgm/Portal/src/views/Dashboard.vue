<template>
  <div class="dashboard-layout">
    <!-- Top Navigation Bar -->
    <header class="top-navbar">
      <div class="navbar-container">
        <div class="navbar-brand">
          <span class="logo-text">NextCGM</span>
        </div>
        <div class="navbar-actions">
          <router-link to="/payment" class="btn btn-sm btn-outline-primary mr-3">Plans</router-link>
          <router-link to="/settings" class="btn btn-sm btn-outline-primary mr-3">Settings</router-link>
          <span class="welcome-text">Welcome, {{ user?.firstName }}</span>
          <button @click="logout" class="btn btn-sm btn-danger-outline ml-3">Logout</button>
        </div>
      </div>
    </header>

    <!-- Main Content -->
    <main class="dashboard-container">
      <div class="user-info-card">
        <div class="info-row">
          <span class="info-label">Nightscout API Key:</span>
          <code class="api-key-code">{{ user?.apiKeyForNightScout }}</code>
        </div>
        <div class="card-actions">
          <button @click="createContainer" :disabled="creating" class="btn btn-primary">
            <span v-if="creating" class="spinner"></span>
            {{ creating ? 'Creating...' : 'Create Instance' }}
          </button>
          <p v-if="createError" class="error-text mt-2">{{ createError }}</p>
        </div>
      </div>

      <div class="instances-section">
        <h3 class="section-title">Your Instances</h3>
        
        <div v-if="containers.length === 0" class="empty-state">
          <div class="empty-icon">📦</div>
          <p>You don't have any instances yet.</p>
          <button @click="createContainer" :disabled="creating" class="btn btn-outline-primary mt-3">
            Create Your First Instance
          </button>
        </div>
        
        <div v-else class="table-responsive">
          <table class="instances-table">
            <thead>
              <tr>
                <th>
                  Web UI URL
                  <div class="th-subtitle">(this is your address to the nightscout portal)</div>
                </th>
                <th>Status</th>
                <th>Image</th>
                <th>
                  Nightscout API URL
                  <div class="th-subtitle">(this is the URL you Place in the XDrip nightsout Upload)</div>
                </th>
                <th class="text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="container in containers" :key="container.id || container.dockerContainersID || container.instanceID">
                <td>
                  <a v-if="container.friendlyContainerURL" :href="'https://' + container.friendlyContainerURL" target="_blank" class="instance-link">
                    {{ container.friendlyContainerURL }}
                  </a>
                  <span v-else class="text-muted">Pending...</span>
                </td>
                <td>
                  <span :class="['status-badge', getStatusClass(container.dockerStatus)]">
                    {{ container.dockerStatus || 'Unknown' }}
                  </span>
                </td>
                <td>
                  <span class="image-name">{{ container.imageNameInUse || container.baseImageContaierName || 'N/A' }}</span>
                </td>
                <td>
                  <div class="api-url-container" v-if="container.friendlyContainerURL && user?.apiKeyForNightScout">
                    <code class="api-url">https://{{ user.apiKeyForNightScout }}@{{ container.friendlyContainerURL }}/api/v1/</code>
                    <button @click="copyToClipboard(`https://${user.apiKeyForNightScout}@${container.friendlyContainerURL}/api/v1/`)" class="btn-icon" title="Copy API URL">
                      📋
                    </button>
                  </div>
                  <span v-else class="text-muted">Not available</span>
                </td>
                <td class="text-right">
                  <button @click="removeContainer(container)" :disabled="container.isDeleting" class="btn btn-sm btn-danger-outline">
                    {{ container.isDeleting ? 'Removing...' : 'Remove' }}
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import api from '../services/api';
import { useAuthStore } from '../stores/auth';

const router = useRouter();
const authStore = useAuthStore();
const user = computed(() => authStore.user);
const containers = ref([]);
const creating = ref(false);
const createError = ref('');

const fetchContainers = async () => {
  try {
    const response = await api.getContainers();
    if (response.data.success && response.data.payload) {
      containers.value = response.data.payload;
    }
  } catch (err) {
    console.error('Failed to fetch containers:', err);
  }
};

onMounted(() => {
  if (authStore.isAuthenticated && authStore.user) {
    if (authStore.user.dockerContainers) {
      containers.value = authStore.user.dockerContainers;
    }
    // Fetch latest containers
    fetchContainers();
  } else {
    router.push('/login');
  }
});

const logout = () => {
  authStore.logout();
  router.push('/login');
};

const createContainer = async () => {
  creating.value = true;
  createError.value = '';
  try {
    const response = await api.createContainer({});
    if (response.data.success) {
      if (response.data.payload) {
        containers.value.push(response.data.payload);
      }
      await fetchContainers();
    } else {
      createError.value = response.data.message || 'Failed to create instance';
    }
  } catch (err) {
    createError.value = err.response?.data?.message || err.message || 'An error occurred';
  } finally {
    creating.value = false;
  }
};

const removeContainer = async (container) => {
  if (!confirm(`Are you sure you want to remove this instance?`)) {
    return;
  }
  
  container.isDeleting = true;
  try {
    const containerId = container.id || container.dockerContainersID || container.instanceID;
    const response = await api.deleteContainer({ dockerContainersID: containerId });
    if (response.data.success) {
      containers.value = containers.value.filter(c => 
        (c.id || c.dockerContainersID || c.instanceID) !== containerId
      );
    } else {
      alert(response.data.message || 'Failed to remove instance');
    }
  } catch (err) {
    alert(err.response?.data?.message || err.message || 'An error occurred while removing instance');
  } finally {
    container.isDeleting = false;
  }
};

const copyToClipboard = async (text) => {
  try {
    await navigator.clipboard.writeText(text);
    // Could add a toast notification here
  } catch (err) {
    console.error('Failed to copy text: ', err);
  }
};

const getStatusClass = (status) => {
  if (!status) return 'status-unknown';
  const s = status.toLowerCase();
  if (s.includes('run') || s.includes('up')) return 'status-running';
  if (s.includes('stop') || s.includes('exit')) return 'status-stopped';
  if (s.includes('fail') || s.includes('error')) return 'status-error';
  return 'status-pending';
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

.welcome-text {
  font-size: 0.875rem;
  color: var(--text-main);
  font-weight: 500;
}

.ml-3 {
  margin-left: 1rem;
}

.mr-3 {
  margin-right: 1rem;
}

/* Main Container */
.dashboard-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}

/* User Info Card */
.user-info-card {
  background: white;
  padding: 1.5rem;
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  border: 1px solid var(--border-color);
  margin-bottom: 2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 1rem;
}

.info-row {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.info-label {
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-muted);
  font-weight: 600;
}

.info-value {
  font-size: 1rem;
  color: var(--text-main);
  font-weight: 500;
}

.api-key-code {
  background-color: #f3f4f6;
  padding: 0.25rem 0.5rem;
  border-radius: 0.375rem;
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
  font-size: 0.875rem;
  color: #111827;
  border: 1px solid #e5e7eb;
}

.card-actions {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
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

.btn-icon {
  background: transparent;
  border: none;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 0.25rem;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background-color 0.2s;
}

.btn-icon:hover {
  background-color: #e5e7eb;
}

.error-text {
  color: var(--danger-color);
  margin: 0;
  font-size: 0.875rem;
}

/* Section Title */
.section-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #111827;
  margin-bottom: 1.25rem;
}

/* Empty State */
.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  background-color: white;
  border-radius: 0.75rem;
  border: 1px dashed #d1d5db;
}

.empty-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
  opacity: 0.5;
}

.empty-state p {
  color: var(--text-muted);
  margin-bottom: 1.5rem;
}

/* Table Styles */
.table-responsive {
  overflow-x: auto;
  background: white;
  border-radius: 0.75rem;
  box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
  border: 1px solid var(--border-color);
}

.instances-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
  white-space: nowrap;
}

.instances-table th {
  background-color: #f9fafb;
  padding: 1rem 1.5rem;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-muted);
  border-bottom: 1px solid var(--border-color);
}

.th-subtitle {
  font-size: 0.65rem;
  font-weight: normal;
  text-transform: none;
  letter-spacing: normal;
  color: #9ca3af;
  margin-top: 0.25rem;
}

.instances-table td {
  padding: 1.25rem 1.5rem;
  vertical-align: middle;
  border-bottom: 1px solid var(--border-color);
  font-size: 0.875rem;
}

.instances-table tr:last-child td {
  border-bottom: none;
}

.instances-table tr:hover {
  background-color: #f9fafb;
}

/* Table Content Styles */
.instance-link {
  color: var(--primary-color);
  text-decoration: none;
  font-weight: 500;
}

.instance-link:hover {
  text-decoration: underline;
}

.image-name {
  color: #4b5563;
  background-color: #f3f4f6;
  padding: 0.25rem 0.5rem;
  border-radius: 0.25rem;
  font-family: monospace;
  font-size: 0.75rem;
}

.api-url-container {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.api-url {
  background-color: #f3f4f6;
  padding: 0.375rem 0.75rem;
  border-radius: 0.375rem;
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
  font-size: 0.75rem;
  color: #374151;
  border: 1px solid #e5e7eb;
  max-width: 300px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* Badges */
.status-badge {
  display: inline-flex;
  align-items: center;
  padding: 0.25rem 0.625rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
  text-transform: capitalize;
}

.status-running {
  background-color: #dcfce7;
  color: #166534;
}

.status-stopped {
  background-color: #f3f4f6;
  color: #4b5563;
}

.status-error {
  background-color: #fee2e2;
  color: #991b1b;
}

.status-pending {
  background-color: #fef3c7;
  color: #92400e;
}

.status-unknown {
  background-color: #e5e7eb;
  color: #374151;
}

/* Utilities */
.text-right {
  text-align: right;
}

.text-muted {
  color: var(--text-muted);
}

.mt-3 {
  margin-top: 1rem;
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
