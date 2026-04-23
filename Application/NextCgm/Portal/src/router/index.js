import { createRouter, createWebHistory } from 'vue-router';
import Login from '../views/Login.vue';
import Register from '../views/Register.vue';
import Dashboard from '../views/Dashboard.vue';
import VerifyOtp from '../views/VerifyOtp.vue';
import Payment from '../views/Payment.vue';
import Settings from '../views/Settings.vue';
import PaymentVerify from '../views/PaymentVerify.vue';
import { useAuthStore } from '../stores/auth';

const routes = [
  { path: '/', redirect: '/login' },
  { path: '/login', component: Login },
  { path: '/register', component: Register },
  { path: '/verify-otp', component: VerifyOtp },
  { path: '/dashboard', component: Dashboard, meta: { requiresAuth: true } },
  { path: '/payment', component: Payment, meta: { requiresAuth: true } },
  { path: '/payment-verify', component: PaymentVerify },
  { path: '/settings', component: Settings, meta: { requiresAuth: true } },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();
  // Ensure state is initialized
  if (!authStore.token && localStorage.getItem('token')) {
    authStore.initializeAuth();
  }
  
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login');
  } else {
    next();
  }
});

export default router;
