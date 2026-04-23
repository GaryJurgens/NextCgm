<template>
  <div class="auth-page">
    <div class="auth-card">
      <h2>Create an Account</h2>
      <form @submit.prevent="handleRegister">
        <div class="form-group">
          <label>First Name</label>
          <input type="text" v-model="firstName" class="form-control" placeholder="First Name" required />
        </div>
        <div class="form-group">
          <label>Last Name</label>
          <input type="text" v-model="lastName" class="form-control" placeholder="Last Name" required />
        </div>
        <div class="form-group">
          <label>Email</label>
          <input type="email" v-model="email" class="form-control" placeholder="Email Address" required />
        </div>
        <div class="form-group">
          <label>Password</label>
          <input type="password" v-model="password" class="form-control" placeholder="Create a Password" required />
        </div>

        <div class="form-group">
          <label>Country</label>
          <select v-model="selectedCountry" @change="onCountryChange" class="form-control" required>
            <option value="" disabled>Select Country</option>
            <option v-for="country in countries" :key="country.countryListID" :value="country.countryListID">
              {{ country.name }}
            </option>
          </select>
        </div>

        <div class="form-group" v-if="states.length > 0">
          <label>State/Province</label>
          <select v-model="selectedState" class="form-control" required>
            <option value="" disabled>Select State/Province</option>
            <option v-for="state in states" :key="state.provinceStateListID" :value="state.provinceStateListID">
              {{ state.name }}
            </option>
          </select>
        </div>

        <div class="form-group" v-if="timeZones.length > 0">
          <label>Time Zone</label>
          <select v-model="selectedTimeZone" class="form-control" required>
            <option value="" disabled>Select Time Zone</option>
            <option v-for="tz in timeZones" :key="tz.timeZoneDataID" :value="tz.timeZoneDataID">
              {{ tz.zoneName }} ({{ tz.gmtOffsetName }})
            </option>
          </select>
        </div>

        <button type="submit" class="btn-block" :disabled="loading">
          {{ loading ? 'Registering...' : 'Register' }}
        </button>
        <p v-if="error" class="error-message">{{ error }}</p>
      </form>
      <div class="auth-links">
        <p>Already have an account? <router-link to="/login">Login here</router-link></p>
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
const firstName = ref('');
const lastName = ref('');
const email = ref('');
const password = ref('');
const loading = ref(false);
const error = ref('');

const countries = ref([]);
const states = ref([]);
const timeZones = ref([]);

const selectedCountry = ref('');
const selectedState = ref('');
const selectedTimeZone = ref('');

onMounted(async () => {
  try {
    const response = await api.getCountries();
    if (response.data.success) {
      countries.value = response.data.payload;
    }
  } catch (err) {
    console.error('Failed to load countries', err);
  }
});

const onCountryChange = async () => {
  selectedState.value = '';
  selectedTimeZone.value = '';
  states.value = [];
  timeZones.value = [];

  if (!selectedCountry.value) return;

  try {
    const [statesRes, tzRes] = await Promise.all([
      api.getStates(selectedCountry.value),
      api.getTimeZones(selectedCountry.value)
    ]);

    if (statesRes.data.success) {
      states.value = statesRes.data.payload;
    }
    if (tzRes.data.success) {
      timeZones.value = tzRes.data.payload;
    }
  } catch (err) {
    console.error('Failed to load states or timezones', err);
  }
};

const handleRegister = async () => {
  loading.value = true;
  error.value = '';
  try {
    const response = await api.register({
      Payload: {
        FirstName: firstName.value,
        LastName: lastName.value,
        EmailUsername: email.value,
        PasswordHash: password.value,
        CountryListID: selectedCountry.value,
        ProvinceStateListID: selectedState.value || "00000000-0000-0000-0000-000000000000",
        TimeZoneID: selectedTimeZone.value || "00000000-0000-0000-0000-000000000000"
      }
    });
    if (response.data.success) {
      authStore.setEmailForOtp(email.value);
      router.push('/verify-otp');
    } else {
      error.value = response.data.message || 'Registration failed';
    }
  } catch (err) {
    error.value = err.response?.data?.message || err.message || 'An error occurred';
  } finally {
    loading.value = false;
  }
};
</script>
