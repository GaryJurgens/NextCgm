import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5132/api',
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
  }
};
