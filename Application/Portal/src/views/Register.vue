<template>
  <div class="auth-container">
    <h2>Register</h2>
    <form @submit.prevent="handleRegister">
      <div class="form-group">
        <label>First Name</label>
        <input type="text" v-model="firstName" required />
      </div>
      <div class="form-group">
        <label>Last Name</label>
        <input type="text" v-model="lastName" required />
      </div>
      <div class="form-group">
        <label>Email</label>
        <input type="email" v-model="email" required />
      </div>
      <div class="form-group">
        <label>Password</label>
        <input type="password" v-model="password" required />
      </div>

      <div class="form-group">
        <label>Country</label>
        <select v-model="selectedCountry" @change="onCountryChange" required>
          <option value="" disabled>Select Country</option>
          <option v-for="country in countries" :key="country.countryListID" :value="country.countryListID">
            {{ country.name }}
          </option>
        </select>
      </div>

      <div class="form-group" v-if="states.length > 0">
        <label>State/Province</label>
        <select v-model="selectedState" required>
          <option value="" disabled>Select State/Province</option>
          <option v-for="state in states" :key="state.provinceStateListID" :value="state.provinceStateListID">
            {{ state.name }}
          </option>
        </select>
      </div>

      <div class="form-group" v-if="timeZones.length > 0">
        <label>Time Zone</label>
        <select v-model="selectedTimeZone" required>
          <option value="" disabled>Select Time Zone</option>
          <option v-for="tz in timeZones" :key="tz.timeZoneDataID" :value="tz.timeZoneDataID">
            {{ tz.zoneName }} ({{ tz.gmtOffsetName }})
          </option>
        </select>
      </div>

      <button type="submit" :disabled="loading">Register</button>
      <p v-if="error" class="error">{{ error }}</p>
    </form>
    <p>Already have an account? <router-link to="/login">Login</router-link></p>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import api from '../services/api';

const router = useRouter();
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
      localStorage.setItem('emailForOtp', email.value);
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
.form-group input, .form-group select {
  width: 100%;
  padding: 8px;
  box-sizing: border-box;
}
button {
  width: 100%;
  padding: 10px;
  background-color: #28a745;
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
