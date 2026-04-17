<template>
  <div class="dashboard-container">
    <header>
      <h2>Welcome, {{ user?.FirstName }} ({{ user?.UserSubDomain }})</h2>
      <button @click="logout" class="logout-btn">Logout</button>
    </header>

    <div class="actions">
      <button @click="createContainer" :disabled="creating" class="create-btn">
        {{ creating ? 'Creating...' : 'Create New Container' }}
      </button>
      <p v-if="createError" class="error">{{ createError }}</p>
    </div>

    <div class="containers-section">
      <h3>Your Containers</h3>
      <div v-if="containers.length === 0" class="no-containers">
        <p>You don't have any containers yet.</p>
      </div>
      <div v-else class="container-grid">
        <div v-for="container in containers" :key="container.id || container.DockerContainersID || container.InstanceID" class="container-card">
          <h4>{{ container.FriendlyContainerURL || 'Unnamed Container' }}</h4>
          <p><strong>Status:</strong> {{ container.DockerStatus }}</p>
          <p><strong>Image:</strong> {{ container.ImageNameInUse || container.BaseImageContaierName }}</p>
          <p v-if="container.FriendlyContainerURL">
            <strong>URL:</strong> <a :href="'https://' + container.FriendlyContainerURL" target="_blank">{{ container.FriendlyContainerURL }}</a>
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import api from '../services/api';

const router = useRouter();
const user = ref(null);
const containers = ref([]);
const creating = ref(false);
const createError = ref('');

onMounted(() => {
  const userData = localStorage.getItem('user');
  if (userData) {
    user.value = JSON.parse(userData);
    // The user payload might already contain containers
    if (user.value.DockerContainers) {
      containers.value = user.value.DockerContainers;
    }
  } else {
    router.push('/login');
  }
});

const logout = () => {
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  router.push('/login');
};

const createContainer = async () => {
  creating.value = true;
  createError.value = '';
  try {
    const response = await api.createContainer({});
    if (response.data.success) {
      // Add the new container to the list
      if (response.data.payload) {
        containers.value.push(response.data.payload);
      }
      // Optionally refresh user data if needed
    } else {
      createError.value = response.data.message || 'Failed to create container';
    }
  } catch (err) {
    createError.value = err.response?.data?.message || err.message || 'An error occurred';
  } finally {
    creating.value = false;
  }
};
</script>

<style scoped>
.dashboard-container {
  padding: 20px;
}
header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
  padding-bottom: 10px;
  border-bottom: 1px solid #ddd;
}
.logout-btn {
  padding: 8px 16px;
  background-color: #dc3545;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}
.actions {
  margin-bottom: 30px;
}
.create-btn {
  padding: 10px 20px;
  background-color: #28a745;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 16px;
}
.create-btn:disabled {
  background-color: #ccc;
}
.error {
  color: red;
  margin-top: 10px;
}
.container-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}
.container-card {
  background: white;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}
.container-card h4 {
  margin-top: 0;
  color: #007bff;
}
.no-containers {
  color: #666;
  font-style: italic;
}
</style>
