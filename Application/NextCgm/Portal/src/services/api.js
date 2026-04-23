import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'https://api.nextcgm.co.za/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default {
  login(data) {
    return api.post('/Users/Login', data);
  },
  register(data) {
    return api.post('/Users/CreateUser', data);
  },
  verifyOtp(data) {
    return api.post('/Users/VerifyOtp', data);
  },
  getContainers() {
    return api.get('/Containers/GetAllContainersByUser');
  },
  createContainer(data) {
    return api.post('/Containers/CreateContainer', data);
  },
  deleteContainer(data) {
    return api.post('/Containers/DeleteContainer', data);
  },
  getCountries() {
    return api.get('/Locations/Countries');
  },
  getStates(countryId) {
    return api.get(`/Locations/States/${countryId}`);
  },
  getTimeZones(countryId) {
    return api.get(`/Locations/TimeZones/${countryId}`);
  },
  initiatePayment(data) {
    return api.post('/Payment/Initiate', data);
  },
  verifyPayment(data) {
    return api.post('/Payment/Verify', data);
  },
  getPlans() {
    return api.get('/Billing/GetPlans');
  },
  getUserSubscriptions(data) {
    return api.post('/Billing/GetUserSubscriptions', data);
  },
  cancelSubscription(data) {
    return api.post('/Billing/CancelSubscription', data);
  }
};
